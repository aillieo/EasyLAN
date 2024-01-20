// -----------------------------------------------------------------------
// <copyright file="ByteBuffer.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils.EasyLAN
{
    using System;
    using System.Text;

    public partial class ByteBuffer : IDisposable
    {
        private static readonly bool isLittleEndian;

        private byte[] buffer;
        private int start;
        private int end;

        private int capacity;

        internal int Length { get; private set; }

        static ByteBuffer()
        {
            isLittleEndian = BitConverter.IsLittleEndian;
        }

        public ByteBuffer(int capacity)
        {
            this.buffer = ByteArrayPool.shared.Get(capacity);
            this.capacity = this.buffer.Length;
            this.start = 0;
            this.end = 0;
            this.Length = 0;
        }

        public ByteBuffer(byte[] data)
            : this(data, 0, data.Length)
        {
        }

        public ByteBuffer(byte[] data, int index, int count)
        {
            this.buffer = data;
            this.capacity = this.buffer.Length;
            this.start = index;
            this.end = index + count;
            this.Length = count;
        }

        public ByteBuffer Copy()
        {
            var copy = new ByteBuffer(this.Length);
            if (this.Length > 0)
            {
                if (this.start < this.end)
                {
                    Array.Copy(this.buffer, this.start, copy.buffer, 0, this.Length);
                }
                else
                {
                    var remainingData = this.capacity - this.start;
                    Array.Copy(this.buffer, this.start, copy.buffer, 0, remainingData);
                    Array.Copy(this.buffer, 0, copy.buffer, remainingData, this.Length - remainingData);
                }
            }

            copy.Length = this.Length;
            copy.start = 0;
            copy.end = copy.start + copy.Length;
            copy.capacity = copy.buffer.Length;
            return copy;
        }

        public void Append(byte[] data, int index, int count)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            this.EnsureCapacity(count);

            if (this.end + count <= this.capacity)
            {
                Array.Copy(data, index, this.buffer, this.end, count);
                this.end += count;
            }
            else
            {
                var remainingSpace = this.capacity - this.end;
                Array.Copy(data, index, this.buffer, this.end, remainingSpace);
                Array.Copy(data, index + remainingSpace, this.buffer, 0, count - remainingSpace);
                this.end = count - remainingSpace;
            }

            this.Length += count;
        }

        public void Append(byte[] data)
        {
            this.Append(data, 0, data.Length);
        }

        public void Prepend(byte[] data, int index, int count)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            this.EnsureCapacity(count);

            if (this.start - count >= 0)
            {
                this.start -= count;
                Array.Copy(data, index, this.buffer, this.start, count);
            }
            else
            {
                var remainingSpace = this.start;
                this.start = this.capacity - (count - remainingSpace);
                Array.Copy(data, index, this.buffer, this.start, remainingSpace);
                Array.Copy(data, index + remainingSpace, this.buffer, 0, count - remainingSpace);
            }

            this.Length += count;
        }

        public void Prepend(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            this.Prepend(data, 0, data.Length);
        }

        public void Append(short data)
        {
            this.EnsureCapacity(2);

            this.WriteShort(this.start, data);
            this.end = (this.end + 2) % this.capacity;

            this.Length += 2;
        }

        public void Prepend(short data)
        {
            this.EnsureCapacity(2);

            this.start = (this.start - 2 + this.capacity) % this.capacity;
            this.WriteShort(this.start, data);

            this.Length += 2;
        }

        public void Append(int data)
        {
            this.EnsureCapacity(4);

            this.WriteInt(this.end, data);
            this.end = (this.end + 4) % this.capacity;

            this.Length += 4;
        }

        public void Prepend(int data)
        {
            this.EnsureCapacity(4);

            this.start = (this.start - 4 + this.capacity) % this.capacity;
            this.WriteInt(this.start, data);

            this.Length += 4;
        }

        public void Prepend(char data)
        {
            this.Prepend((byte)data);
        }

        public void Append(char data)
        {
            this.Append((byte)data);
        }

        public void Prepend(byte data)
        {
            this.EnsureCapacity(1);

            this.start = (this.start - 1 + this.capacity) % this.capacity;
            this.buffer[this.start] = data;

            this.Length++;
        }

        public void Append(byte data)
        {
            this.EnsureCapacity(1);

            this.buffer[this.end] = data;
            this.end = (this.end + 1) % this.capacity;

            this.Length++;
        }

        public byte[] ToArray()
        {
            var byteArray = new byte[this.Length];
            if (this.Length > 0)
            {
                if (this.start < this.end)
                {
                    Array.Copy(this.buffer, this.start, byteArray, 0, this.Length);
                }
                else
                {
                    var remainingData = this.capacity - this.start;
                    Array.Copy(this.buffer, this.start, byteArray, 0, remainingData);
                    Array.Copy(this.buffer, 0, byteArray, remainingData, this.Length - remainingData);
                }
            }

            return byteArray;
        }

        private void EnsureCapacity(int requiredCapacity)
        {
            if (requiredCapacity > this.capacity - this.Length)
            {
                var newCapacity = Math.Max(this.capacity * 2, this.capacity + requiredCapacity);
                var newBuffer = ByteArrayPool.shared.Get(newCapacity);
                newCapacity = newBuffer.Length;

                if (this.Length > 0)
                {
                    if (this.start < this.end)
                    {
                        Array.Copy(this.buffer, this.start, newBuffer, 0, this.Length);
                    }
                    else
                    {
                        var remainingData = this.capacity - this.start;
                        Array.Copy(this.buffer, this.start, newBuffer, 0, remainingData);
                        Array.Copy(this.buffer, 0, newBuffer, remainingData, this.Length - remainingData);
                    }
                }

                var oldBuffer = this.buffer;
                this.buffer = newBuffer;

                if (oldBuffer.Length > 0)
                {
                    ByteArrayPool.shared.Recycle(oldBuffer);
                }

                this.capacity = newCapacity;
                this.start = 0;
                this.end = this.Length;
            }
        }

        private void WriteShort(int startIndex, short data)
        {
            if (isLittleEndian)
            {
                this.buffer[startIndex] = (byte)(data & 0xFF);
                startIndex = (startIndex + 1) % this.capacity;
                this.buffer[startIndex] = (byte)((data >> 8) & 0xFF);
            }
            else
            {
                this.buffer[startIndex] = (byte)((data >> 8) & 0xFF);
                startIndex = (startIndex + 1) % this.capacity;
                this.buffer[startIndex] = (byte)(data & 0xFF);
            }
        }

        private void WriteInt(int startIndex, int data)
        {
            if (isLittleEndian)
            {
                this.buffer[startIndex] = (byte)(data & 0xFF);
                startIndex = (startIndex + 1) % this.capacity;
                this.buffer[startIndex] = (byte)((data >> 8) & 0xFF);
                startIndex = (startIndex + 1) % this.capacity;
                this.buffer[startIndex] = (byte)((data >> 16) & 0xFF);
                startIndex = (startIndex + 1) % this.capacity;
                this.buffer[startIndex] = (byte)((data >> 24) & 0xFF);
            }
            else
            {
                this.buffer[startIndex] = (byte)((data >> 24) & 0xFF);
                startIndex = (startIndex + 1) % this.capacity;
                this.buffer[startIndex] = (byte)((data >> 16) & 0xFF);
                startIndex = (startIndex + 1) % this.capacity;
                this.buffer[startIndex] = (byte)((data >> 8) & 0xFF);
                startIndex = (startIndex + 1) % this.capacity;
                this.buffer[startIndex] = (byte)(data & 0xFF);
            }
        }

        public byte[] Consume(int count)
        {
            if (count > this.Length)
            {
                throw new ELException($"this.Length = {this.Length} but count = {count}");
            }

            var result = new byte[count];
            this.Consume(count, result, 0);
            return result;
        }

        public void Consume(int count, byte[] destinationArray, int destinationIndex)
        {
            if (count > this.Length)
            {
                throw new InvalidOperationException();
            }

            if (this.start + count <= this.capacity)
            {
                Array.Copy(this.buffer, this.start, destinationArray, destinationIndex, count);
                this.start += count;
            }
            else
            {
                var remainingData = this.capacity - this.start;
                Array.Copy(this.buffer, this.start, destinationArray, destinationIndex, remainingData);
                Array.Copy(this.buffer, 0, destinationArray, destinationIndex + remainingData, count - remainingData);
                this.start = count - remainingData;
            }

            this.Length -= count;
        }

        public byte ConsumeByte()
        {
            if (this.Length < 1)
            {
                throw new ELException("Not enough bytes");
            }

            var result = this.buffer[this.start];
            this.start = (this.start + 1) % this.buffer.Length;

            this.Length -= 1;
            return result;
        }

        public int ConsumeInt()
        {
            if (this.Length < 4)
            {
                throw new ELException("Not enough bytes");
            }

            var result = this.ReadInt(this.start);
            this.start = (this.start + 4) % this.buffer.Length;

            this.Length -= 4;
            return result;
        }

        private int ReadInt(int startIndex)
        {
            if (isLittleEndian)
            {
                int value = this.buffer[startIndex];
                startIndex = (startIndex + 1) % this.capacity;
                value |= this.buffer[startIndex] << 8;
                startIndex = (startIndex + 1) % this.capacity;
                value |= this.buffer[startIndex] << 16;
                startIndex = (startIndex + 1) % this.capacity;
                value |= this.buffer[startIndex] << 24;
                return value;
            }
            else
            {
                var value = this.buffer[startIndex] << 24;
                startIndex = (startIndex + 1) % this.capacity;
                value |= this.buffer[startIndex] << 16;
                startIndex = (startIndex + 1) % this.capacity;
                value |= this.buffer[startIndex] << 8;
                startIndex = (startIndex + 1) % this.capacity;
                value |= this.buffer[startIndex];
                return value;
            }
        }

        public void Append(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                this.Append((int)0);
                return;
            }

            var byteCount = Encoding.UTF8.GetByteCount(data);
            var byteArray = ByteArrayPool.shared.Get(byteCount);

            try
            {
                Encoding.UTF8.GetBytes(data, 0, data.Length, byteArray, 0);
                this.Append(byteCount);
                this.Append(byteArray, 0, byteCount);
            }
            finally
            {
                ByteArrayPool.shared.Recycle(byteArray);
            }
        }

        public string ConsumeString()
        {
            var byteCount = this.ConsumeInt();
            var byteArray = ByteArrayPool.shared.Get(byteCount);

            try
            {
                this.Consume(byteCount, byteArray, 0);
                return Encoding.UTF8.GetString(byteArray, 0, byteCount);
            }
            finally
            {
                ByteArrayPool.shared.Recycle(byteArray);
            }
        }

        public void Clear()
        {
            this.start = 0;
            this.end = 0;
            this.Length = 0;

            if (this.buffer.Length > 0)
            {
                ByteArrayPool.shared.Recycle(this.buffer);
            }

            this.buffer = Array.Empty<byte>();
            this.capacity = 0;
        }

        public void Dispose()
        {
            this.Clear();
        }
    }
}
