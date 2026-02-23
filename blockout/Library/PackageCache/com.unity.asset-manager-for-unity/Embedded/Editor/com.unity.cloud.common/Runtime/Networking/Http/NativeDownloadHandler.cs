using System;
using System.Buffers;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;

namespace Unity.Cloud.CommonEmbedded.Runtime
{
    /// <summary>
    /// A download handler that uses <see cref="SingleReaderSingleWriterNativeStream"/>
    /// to write data to a native stream.
    /// </summary>
    class NativeDownloadHandler : DownloadHandlerScript
    {
        public event Action HeadersReceived;
        public event Action ContentReceived;

        SingleReaderSingleWriterNativeStream m_Writer;

        bool m_Disposed;
        byte[] m_Buffer;
        int m_ContentLength;
        int m_BytesReceived;
        bool m_HeadersReceivedInvoked;

        public NativeDownloadHandler(SingleReaderSingleWriterNativeStream writer, int bufferSize = 128 * 1024)
            : this(writer, ArrayPool<byte>.Shared.Rent(bufferSize))
        {
        }

        NativeDownloadHandler(SingleReaderSingleWriterNativeStream writer, byte[] pooledBuffer)
            : base(pooledBuffer)
        {
            m_Writer = writer;
            m_Buffer = pooledBuffer;
        }

        public override void Dispose()
        {
            if (m_Disposed)
                return;

            m_Disposed = true;

            ArrayPool<byte>.Shared.Return(m_Buffer);
            m_Buffer = null;

            CloseWriteChannel(false);
            base.Dispose();
        }

        public bool HasContent { get; private set; }

        protected override float GetProgress()
        {
            var length = GetContentLength();
            return length == 0 ? 0 : Mathf.Clamp01((float)m_BytesReceived / length);
        }

        protected override void ReceiveContentLengthHeader(ulong contentLength)
        {
            m_ContentLength = (int)contentLength;
        }

        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            Assert.IsTrue(data != null, "data should not be null");
            Assert.IsTrue(dataLength != 0, "dataLength should not be 0");

            InvokeHeadersReceived();

            m_BytesReceived += dataLength;

            if (m_Writer.IsDisposed)
                return false;

            m_Writer.Write(data, 0, dataLength);

            HasContent = true;
            return true;
        }

        protected override void CompleteContent()
        {
            InvokeHeadersReceived();
            CloseWriteChannel(true);
        }

        void InvokeHeadersReceived()
        {
            if (!m_HeadersReceivedInvoked)
            {
                HeadersReceived?.Invoke();
                m_HeadersReceivedInvoked = true;
            }
        }

        void CloseWriteChannel(bool allDataWritten)
        {
            if (m_Writer != null)
            {
                if (!m_Writer.IsDisposed)
                    m_Writer.CloseWriteChannelIfOpen(allDataWritten);

                m_Writer = null;

                if (allDataWritten)
                    ContentReceived?.Invoke();
            }
        }

        int GetContentLength() => m_BytesReceived > m_ContentLength ? m_BytesReceived : m_ContentLength;
    }
}
