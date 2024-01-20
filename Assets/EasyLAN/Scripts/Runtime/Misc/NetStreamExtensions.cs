// -----------------------------------------------------------------------
// <copyright file="NetStreamExtensions.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils.EasyLAN
{
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

    internal static class NetStreamExtensions
    {
        internal static async Task<int> ReadAsync(this NetworkStream stream, ByteBuffer buffer, int count, CancellationToken cancellationToken)
        {
            return await buffer.ReadFromStreamAsync(stream, count, cancellationToken);
        }

        internal static async Task WriteAsync(this NetworkStream stream, ByteBuffer buffer, CancellationToken cancellationToken)
        {
            await buffer.WriteToStreamAsync(stream, cancellationToken);
        }

        internal static async Task<int> ReadIntAsync(this NetworkStream stream, CancellationToken cancellationToken)
        {
            var buffer = new ByteBuffer(4);
            var bytesRead = await stream.ReadAsync(buffer, 4, cancellationToken);
            if (bytesRead != 4)
            {
                return -1;
            }

            var lengthValue = buffer.ConsumeInt();
            return lengthValue;
        }

        internal static async Task<ByteBuffer> ReadBytesAsync(this NetworkStream stream, int length, CancellationToken cancellationToken)
        {
            var totalBytesRead = 0;
            var buffer = new ByteBuffer(length);

            while (totalBytesRead < length)
            {
                var bytesRead = await stream.ReadAsync(buffer, length - totalBytesRead, cancellationToken);

                if (bytesRead == 0)
                {
                    return null;
                }

                totalBytesRead += bytesRead;
            }

            return buffer;
        }
    }
}
