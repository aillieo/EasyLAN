using System;

namespace AillieoUtils.EasyLAN
{
    internal class ByteBuffer : IDisposable
    {
        private byte[] buffer;
        private int start;
        private int end;

        private int capacity;
        private readonly ByteArrayPool pool = new ByteArrayPool(32);

        public int Length { get; private set; }

        public ByteBuffer(int capacity)
        {
            buffer = pool.Get(capacity);
            this.capacity = buffer.Length;
            start = 0;
            end = 0;
            Length = 0;
        }

        public void Append(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            EnsureCapacity(data.Length);

            if (end + data.Length <= capacity)
            {
                Array.Copy(data, 0, buffer, end, data.Length);
                end += data.Length;
            }
            else
            {
                int remainingSpace = capacity - end;
                Array.Copy(data, 0, buffer, end, remainingSpace);
                Array.Copy(data, remainingSpace, buffer, 0, data.Length - remainingSpace);
                end = data.Length - remainingSpace;
            }

            Length += data.Length;
        }

        public void Prepend(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            EnsureCapacity(data.Length);

            if (start - data.Length >= 0)
            {
                start -= data.Length;
                Array.Copy(data, 0, buffer, start, data.Length);
            }
            else
            {
                int remainingSpace = start;
                start = capacity - (data.Length - remainingSpace);
                Array.Copy(data, 0, buffer, start, remainingSpace);
                Array.Copy(data, remainingSpace, buffer, 0, data.Length - remainingSpace);
            }

            Length += data.Length;
        }

        public void Append(short data)
        {
            EnsureCapacity(2);

            WriteShort(start, data);
            end = (end + 2) % capacity;

            Length += 2;
        }

        public void Prepend(short data)
        {
            EnsureCapacity(2);

            start = (start - 2 + capacity) % capacity;
            WriteShort(start, data);

            Length += 2;
        }

        public void Append(int data)
        {
            EnsureCapacity(4);

            WriteInt(start, data);
            end = (end + 4) % capacity;

            Length += 4;
        }

        public void Prepend(int data)
        {
            EnsureCapacity(4);

            start = (start - 4 + capacity) % capacity;
            WriteInt(start, data);

            Length += 4;
        }


        public void Prepend(char data)
        {
            Prepend((byte)data);
        }

        public void Append(char data)
        {
            Append((byte)data);
        }

        public void Prepend(byte data)
        {
            EnsureCapacity(1);

            start = (start - 1 + capacity) % capacity;
            buffer[start] = data;

            Length++;
        }

        public void Append(byte data)
        {
            EnsureCapacity(1);

            buffer[end] = data;
            end = (end + 1) % capacity;

            Length++;
        }

        public byte[] ToArray()
        {
            byte[] data = new byte[Length];
            if (Length > 0)
            {
                if (start < end)
                {
                    Array.Copy(buffer, start, data, 0, Length);
                }
                else
                {
                    int remainingData = capacity - start;
                    Array.Copy(buffer, start, data, 0, remainingData);
                    Array.Copy(buffer, 0, data, remainingData, Length - remainingData);
                }
            }
            return data;
        }

        public int CopyTo(byte[] byteArray, int index)
        {
            if (byteArray.Length - start < Length)
            {
                throw new ELException();
            }

            if (Length > 0)
            {
                if (start < end)
                {
                    Array.Copy(buffer, start, byteArray, index, Length);
                }
                else
                {
                    int remainingData = capacity - start;
                    Array.Copy(buffer, start, byteArray, index, remainingData);
                    Array.Copy(buffer, 0, byteArray, index + remainingData, Length - remainingData);
                }
            }
            return Length;
        }

        private void EnsureCapacity(int requiredCapacity)
        {
            if (requiredCapacity > capacity - Length)
            {
                int newCapacity = Math.Max(capacity * 2, capacity + requiredCapacity);
                byte[] newBuffer = pool.Get(newCapacity);
                newCapacity = newBuffer.Length;

                if (Length > 0)
                {
                    if (start < end)
                    {
                        Array.Copy(buffer, start, newBuffer, 0, Length);
                    }
                    else
                    {
                        int remainingData = capacity - start;
                        Array.Copy(buffer, start, newBuffer, 0, remainingData);
                        Array.Copy(buffer, 0, newBuffer, remainingData, Length - remainingData);
                    }
                }

                var oldBuffer = buffer;
                buffer = newBuffer;

                if (buffer.Length > 0)
                {
                    pool.Recycle(oldBuffer);
                }

                capacity = newCapacity;
                start = 0;
                end = Length;
            }
        }

        private void WriteShort(int startIndex, short data)
        {
            if (BitConverter.IsLittleEndian)
            {
                buffer[startIndex] = (byte)(data & 0xFF);
                startIndex = (startIndex + 1) % capacity;
                buffer[startIndex] = (byte)((data >> 8) & 0xFF);
            }
            else
            {
                buffer[startIndex] = (byte)((data >> 8) & 0xFF);
                startIndex = (startIndex + 1) % capacity;
                buffer[startIndex] = (byte)(data & 0xFF);
            }
        }

        private void WriteInt(int startIndex, int data)
        {
            if (BitConverter.IsLittleEndian)
            {
                buffer[startIndex] = (byte)(data & 0xFF);
                startIndex = (startIndex + 1) % capacity;
                buffer[startIndex] = (byte)((data >> 8) & 0xFF);
                startIndex = (startIndex + 1) % capacity;
                buffer[startIndex] = (byte)((data >> 16) & 0xFF);
                startIndex = (startIndex + 1) % capacity;
                buffer[startIndex] = (byte)((data >> 24) & 0xFF);
            }
            else
            {
                buffer[startIndex] = (byte)((data >> 24) & 0xFF);
                startIndex = (startIndex + 1) % capacity;
                buffer[startIndex] = (byte)((data >> 16) & 0xFF);
                startIndex = (startIndex + 1) % capacity;
                buffer[startIndex] = (byte)((data >> 8) & 0xFF);
                startIndex = (startIndex + 1) % capacity;
                buffer[startIndex] = (byte)(data & 0xFF);
            }
        }

        public byte[] Consume(int count)
        {
            if (count > Length)
            {
                throw new InvalidOperationException();
            }

            byte[] data = new byte[count];
            if (start + count <= capacity)
            {
                Array.Copy(buffer, start, data, 0, count);
                start += count;
            }
            else
            {
                int remainingData = capacity - start;
                Array.Copy(buffer, start, data, 0, remainingData);
                Array.Copy(buffer, 0, data, remainingData, count - remainingData);
                start = count - remainingData;
            }

            Length -= count;
            return data;
        }

        public byte ConsumeByte()
        {
            if (Length < 1)
            {
                throw new ELException("Not enough bytes");
            }

            byte result = buffer[start];
            start = (start + 1) % buffer.Length;

            Length -= 1;
            return result;
        }

        public int ConsumeInt()
        {
            if (Length < 4)
            {
                throw new ELException("Not enough bytes");
            }

            int result = ReadInt(start);
            start = (start + 4) % buffer.Length;

            Length -= 4;
            return result;
        }

        private int ReadInt(int startIndex)
        {
            if (BitConverter.IsLittleEndian)
            {
                int value = buffer[startIndex];
                startIndex = (startIndex + 1) % capacity;
                value |= buffer[startIndex] << 8;
                startIndex = (startIndex + 1) % capacity;
                value |= buffer[startIndex] << 16;
                startIndex = (startIndex + 1) % capacity;
                value |= buffer[startIndex] << 24;
                return value;
            }
            else
            {
                int value = buffer[startIndex] << 24;
                startIndex = (startIndex + 1) % capacity;
                value |= buffer[startIndex] << 16;
                startIndex = (startIndex + 1) % capacity;
                value |= buffer[startIndex] << 8;
                startIndex = (startIndex + 1) % capacity;
                value |= buffer[startIndex];
                return value;
            }
        }

        public void Clear()
        {
            start = 0;
            end = 0;
            Length = 0;

            if (buffer.Length > 0)
            {
                pool.Recycle(buffer);
            }

            buffer = Array.Empty<byte>();
            capacity = 0;
        }

        public void Dispose()
        {
            Clear();
        }
    }
}
