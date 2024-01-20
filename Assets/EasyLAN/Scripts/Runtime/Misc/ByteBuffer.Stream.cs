// -----------------------------------------------------------------------
// <copyright file="ByteBuffer.Stream.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils.EasyLAN
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public partial class ByteBuffer
    {
        internal async Task<int> ReadFromStreamAsync(Stream stream, int count, CancellationToken cancellationToken)
        {
            this.EnsureCapacity(count);

            if (this.end + count <= this.capacity)
            {
                var bytesRead = await stream.ReadAsync(this.buffer, this.end, count, cancellationToken);
                this.end += bytesRead;
                this.Length += bytesRead;
                return bytesRead;
            }
            else
            {
                var bytesRead = 0;

                var remainingSpace = this.capacity - this.end;
                bytesRead += await stream.ReadAsync(this.buffer, this.end, remainingSpace, cancellationToken);

                if (bytesRead < count)
                {
                    var remainingData = count - bytesRead;
                    var newBytesRead = await stream.ReadAsync(this.buffer, 0, remainingData, cancellationToken);
                    bytesRead += newBytesRead;
                    this.end = newBytesRead;
                }
                else
                {
                    this.end += bytesRead;
                }

                return bytesRead;
            }
        }

        internal async Task WriteToStreamAsync(Stream stream, CancellationToken cancellationToken)
        {
            if (this.end > this.start)
            {
                await stream.WriteAsync(this.buffer, this.start, this.Length, cancellationToken);
                this.Clear();
            }
            else
            {
                var remainingData = this.capacity - this.start;
                await stream.WriteAsync(this.buffer, this.start, remainingData, cancellationToken);

                if (this.end > 0)
                {
                    await stream.WriteAsync(this.buffer, 0, this.end, cancellationToken);
                }

                this.Clear();
            }
        }
    }
}
