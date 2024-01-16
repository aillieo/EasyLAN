using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AillieoUtils.EasyLAN
{
    internal partial class ByteBuffer
    {
        public async Task<int> ReadFromStreamAsync(Stream stream, int count, CancellationToken cancellationToken)
        {
            this.EnsureCapacity(count);

            if (end + count <= capacity)
            {
                var bytesRead = await stream.ReadAsync(this.buffer, this.end, count, cancellationToken);
                end += bytesRead;
                Length += bytesRead;
                return bytesRead;
            }
            else
            {
                int bytesRead = 0;

                int remainingSpace = capacity - end;
                bytesRead += await stream.ReadAsync(this.buffer, this.end, remainingSpace, cancellationToken);

                if (bytesRead < count)
                {
                    int remainingData = count - bytesRead;
                    int newBytesRead = await stream.ReadAsync(this.buffer, 0, remainingData, cancellationToken);
                    bytesRead += newBytesRead;
                    end = newBytesRead;
                }
                else
                {
                    end += bytesRead;
                }

                return bytesRead;
            }
        }

        public async Task WriteToStreamAsync(Stream stream, CancellationToken cancellationToken)
        {
            if (this.end > this.start)
            {
                await stream.WriteAsync(this.buffer, this.start, this.Length, cancellationToken);
                this.Clear();
            }
            else
            {
                int remainingData = capacity - start;
                await stream.WriteAsync(this.buffer, this.start, remainingData, cancellationToken);

                if (end > 0)
                {
                    await stream.WriteAsync(this.buffer, 0, end, cancellationToken);
                }

                this.Clear();
            }
        }
    }
}
