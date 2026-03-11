using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Unity.Cloud.AssetsEmbedded
{
    static class Utilities
    {
        const int k_MD5_bufferSize = 4096;

        internal static async Task<string> CalculateMD5ChecksumAsync(Stream stream, CancellationToken cancellationToken)
        {
            var position = stream.Position;
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                //In this method, MD5 algorythm is used for calculating checksum of a stream or a file before uploading it.
                //It is not used in a sensitive context.
#pragma warning disable S4790 //Using weak hashing algorithms is security-sensitive
                using (var md5 = MD5.Create())
#pragma warning restore S4790
                {
#if UNITY_WEBGL && !UNITY_EDITOR
                    await CalculateMD5ChecksumInternalAsync(md5, stream, cancellationToken);
#else
                    var result = new TaskCompletionSource<bool>();
                    await Task.Run(async () =>
                    {
                        try
                        {
                            await CalculateMD5ChecksumInternalAsync(md5, stream, cancellationToken);
                        }
                        finally
                        {
                            result.SetResult(true);
                        }
                    }, cancellationToken);
                    await result.Task;
#endif
                    return BitConverter.ToString(md5.Hash).Replace("-", "").ToLowerInvariant();
                }
            }
            catch (Exception e)
            {
                throw new AggregateException(e);
            }
            finally
            {
                stream.Position = position;
            }
        }

        static async Task CalculateMD5ChecksumInternalAsync(MD5 md5, Stream stream, CancellationToken cancellationToken)
        {
            var buffer = new byte[k_MD5_bufferSize];
            int bytesRead;
            do
            {
                cancellationToken.ThrowIfCancellationRequested();
#if UNITY_WEBGL && !UNITY_EDITOR
                bytesRead = await Task.FromResult(stream.Read(buffer, 0, k_MD5_bufferSize));
#else
                bytesRead = await stream.ReadAsync(buffer, 0, k_MD5_bufferSize, cancellationToken);
#endif
                if (bytesRead > 0)
                {
                    md5.TransformBlock(buffer, 0, bytesRead, null, 0);
                }
            } while (bytesRead > 0);

            md5.TransformFinalBlock(buffer, 0, 0);
            await Task.CompletedTask;
        }
    }
}
