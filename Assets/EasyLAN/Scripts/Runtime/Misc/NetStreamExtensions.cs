using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace AillieoUtils.EasyLAN
{
    internal static class NetStreamExtensions
    {
        internal static async Task<int> ReadIntAsync(this NetworkStream stream, CancellationToken cancellationToken)
        {
            byte[] intBuffer = new byte[4];
            int bytesRead = await stream.ReadAsync(intBuffer, 0, 4, cancellationToken);
            if (bytesRead != 4)
            {
                return -1;
            }

            int length = BitConverter.ToInt32(intBuffer, 0);
            return length;
        }

        internal static async Task<int> ReadBytesAsync(this NetworkStream stream, byte[] buffer, int length, CancellationToken cancellationToken)
        {
            int totalBytesRead = 0;
            length = Math.Min(length, buffer.Length);

            while (totalBytesRead < length)
            {
                int bytesRead = await stream.ReadAsync(buffer, totalBytesRead, length - totalBytesRead, cancellationToken);

                if (bytesRead == 0)
                {
                    return -1;
                }

                totalBytesRead += bytesRead;
            }

            return length;
        }
    }
}
