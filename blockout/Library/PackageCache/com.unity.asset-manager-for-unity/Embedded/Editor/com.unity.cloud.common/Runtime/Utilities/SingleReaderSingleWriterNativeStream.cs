using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;

namespace Unity.Cloud.CommonEmbedded.Runtime
{
    /// <summary>
    /// Stream that writes directly to a Unity <see cref="NativeArray{T}"/>.
    /// </summary>
    /// <remarks>
    /// Supports a single producer and a single consumer. The producer can
    /// write while the consumer reads concurrently. Two producers cannot
    /// write at the same time, nor consumers can read at the same time.
    /// The stream is considered infinite. When all the data is received,
    /// <see cref="CloseWriteChannelIfOpen"/> must be invoked to let the reader
    /// complete properly.
    /// </remarks>
    class SingleReaderSingleWriterNativeStream : Stream
    {
        const int k_ChunkSize = 128 * 1024;

        readonly object m_Lock = new();
        readonly List<NativeArray<byte>> m_Buffers = new();

        long m_WriterIndex;
        long m_ReaderIndex;
        bool m_WriteCompleted;
        bool m_WriteStreamClosed;

        AsyncAutoResetEvent m_Sync = new();

        ~SingleReaderSingleWriterNativeStream()
        {
            Dispose(false);
        }

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            IsDisposed = true;

            if (disposing)
            {
                m_Sync.Dispose();
                m_Sync = null;
            }

            foreach (var b in m_Buffers)
                b.Dispose();

            base.Dispose(disposing);
        }

        public bool IsDisposed { get; private set; }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public void CloseWriteChannelIfOpen(bool writeCompleted)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(SingleReaderSingleWriterNativeStream));

            if (m_WriteStreamClosed)
                return;

            m_WriteCompleted = writeCompleted;
            m_WriteStreamClosed = true;
            m_Sync.Set();
        }

        public override void Flush()
        {
            // Nothing to do
        }

        public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
            try
            {
                int bytesRead;
                while ((bytesRead = await ReadAsync(new Memory<byte>(buffer), cancellationToken).ConfigureAwaitFalse()) != 0)
                {
                    await destination.WriteAsync(new ReadOnlyMemory<byte>(buffer, 0, bytesRead), cancellationToken).ConfigureAwaitFalse();
                }
            }
            catch (Exception)
            {
                cancellationToken.ThrowIfCancellationRequested();
                throw;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(SingleReaderSingleWriterNativeStream));

            if (!m_WriteStreamClosed && !TaskExtensions.MultithreadingEnabled)
            {
                throw new InvalidOperationException(
                    $"Cannot use {nameof(Read)} method on incomplete streams on platforms " +
                    $"that don't support multithreading since it's going to freeze the app.");
            }

            return ReadAsync(buffer, offset, count, CancellationToken.None).Result;
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return ReadAsync(new Memory<byte>(buffer, offset, count), cancellationToken).AsTask();
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(SingleReaderSingleWriterNativeStream));

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (m_ReaderIndex == m_WriterIndex && !m_WriteStreamClosed)
                    await m_Sync.WaitAsync(cancellationToken).ConfigureAwaitFalse();
                else
                    break;
            }

            return ReadToBuffer(buffer);
        }

        int ReadToBuffer(Memory<byte> buffer)
        {
            if (IsDisposed)
                throw new IOException("Stream disposed during a read operation.");

            if (m_WriteStreamClosed && !m_WriteCompleted)
                throw new IOException("The writer stream closed without writing all data.");

            var readerIndex = m_ReaderIndex;
            var writerIndex = m_WriterIndex;
            var read = 0;
            var remainingToRead = Math.Min(writerIndex - readerIndex, buffer.Length);

            while (remainingToRead > 0)
            {
                var readIndex = (int)(readerIndex / k_ChunkSize);

                var offset = (int)(readerIndex % k_ChunkSize);
                var nativeSpace = k_ChunkSize - offset;
                var toRead = (int)Math.Min(remainingToRead, nativeSpace);

                m_Buffers[readIndex].AsSpan().Slice(offset, toRead).CopyTo(buffer.Span[read..]);

                readerIndex += toRead;
                read += toRead;
                remainingToRead -= toRead;

                if (toRead == nativeSpace)
                {
                    lock (m_Lock)
                    {
                        // Transfer the buffer at the end of the list to minimize managed<->native calls,
                        // don't RemoveAt(0) from the list since it may get costly for bigger streams.
                        // Having a real resizable circular buffer would be more optimal here.
                        m_Buffers.Add(m_Buffers[readIndex]);

                        // Needs to remove the reference else NativeArray throws when Dispose is called twice.
                        m_Buffers[readIndex] = new NativeArray<byte>();
                    }
                }
            }

            m_ReaderIndex = readerIndex;

            return read;
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(SingleReaderSingleWriterNativeStream));

            if (m_WriteStreamClosed)
                throw new InvalidOperationException("Cannot write to stream when the writer stream is closed.");

            if (m_WriteCompleted)
                throw new InvalidOperationException("Cannot write to stream when the writer stream is completed.");

            var writerIndex = m_WriterIndex;
            var written = 0;
            var remainingToWrite = buffer.Length;

            while (remainingToWrite > 0)
            {
                var writeIndex = (int)(writerIndex / k_ChunkSize);

                lock (m_Lock)
                {
                    if (writeIndex == m_Buffers.Count)
                        m_Buffers.Add(new NativeArray<byte>(k_ChunkSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory));
                }

                var offset = (int)(writerIndex % k_ChunkSize);
                var nativeSpace = k_ChunkSize - offset;
                var toWrite = Math.Min(remainingToWrite, nativeSpace);

                buffer.Slice(written, toWrite).CopyTo(m_Buffers[writeIndex].AsSpan()[offset..]);

                writerIndex += toWrite;
                written += toWrite;
                remainingToWrite -= toWrite;
            }

            m_WriterIndex = writerIndex;
            m_Sync.Set();
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return WriteAsync(new ReadOnlyMemory<byte>(buffer, offset, count), cancellationToken).AsTask();
        }

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            Write(buffer.Span);
            return default;
        }

        sealed class AsyncAutoResetEvent : IDisposable
        {
            static readonly Task k_Completed = Task.FromResult(true);

            readonly Queue<(TaskCompletionSource<bool> Source, CancellationTokenRegistration Ctr)> m_PendingSources = new();

            bool m_Signaled;
            bool m_IsDisposed;

            public void Dispose()
            {
                if (m_IsDisposed)
                    return;

                m_IsDisposed = true;

                foreach (var (s, c) in m_PendingSources)
                {
                    s.TrySetCanceled();
                    c.Dispose();
                }
            }

            public Task WaitAsync(CancellationToken token = default)
            {
                if (m_IsDisposed)
                    throw new ObjectDisposedException(nameof(AsyncAutoResetEvent));

                lock (m_PendingSources)
                {
                    token.ThrowIfCancellationRequested();

                    if (m_Signaled)
                    {
                        m_Signaled = false;
                        return k_Completed;
                    }

                    var tcs = new TaskCompletionSource<bool>();
                    var ctr = token.Register(() => tcs.SetCanceled());
                    m_PendingSources.Enqueue((tcs, ctr));
                    return tcs.Task;
                }
            }

            public void Set()
            {
                if (m_IsDisposed)
                    throw new ObjectDisposedException(nameof(AsyncAutoResetEvent));

                while (!TrySignal())
                {
                    // Nothing
                }
            }

            bool TrySignal()
            {
                (TaskCompletionSource<bool> Source, CancellationTokenRegistration Ctr) toRelease;

                lock (m_PendingSources)
                {
                    if (m_PendingSources.Count == 0)
                    {
                        m_Signaled = true;
                        return true;
                    }

                    toRelease = m_PendingSources.Dequeue();
                }

                toRelease.Ctr.Dispose();
                return toRelease.Source.TrySetResult(true);
            }
        }
    }
}
