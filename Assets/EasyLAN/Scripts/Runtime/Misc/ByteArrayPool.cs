using System.Collections.Generic;

namespace AillieoUtils.EasyLAN
{
    internal class ByteArrayPool
    {
        private int capacity;

        public ByteArrayPool(int capacity = 32)
        {
            this.capacity = capacity;
        }

        private readonly List<byte[]> pool = new List<byte[]>();

        public byte[] Get(int minimalLength)
        {
            lock (pool)
            {
                foreach (byte[] arr in pool)
                {
                    if (arr.Length >= minimalLength)
                    {
                        pool.Remove(arr);
                        return arr;
                    }
                }
            }

            return new byte[minimalLength];
        }

        public void Recycle(byte[] array)
        {
            if (array.Length == 0)
            {
                return;
            }

            lock (pool)
            {
                if (pool.Count < capacity)
                {
                    pool.Add(array);
                }
            }
        }
    }
}
