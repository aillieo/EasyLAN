using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AillieoUtils.EasyLAN
{
    internal class ByteArrayPool
    {
        private class Bucket
        {
            public readonly int minimalLengthRaw;
            private readonly byte[][] slots;

            public Bucket(int minimalLengthRaw, int capacity)
            {
                this.minimalLengthRaw = minimalLengthRaw;
                this.slots = new byte[capacity][];
            }

            private readonly ReaderWriterLockSlim lockObject = new ReaderWriterLockSlim();

            public byte[] Get(int minimalLength)
            {
                if (minimalLength == 0)
                {
                    return Array.Empty<byte>();
                }

                while (true)
                {
                    lockObject.EnterUpgradeableReadLock();
                    try
                    {
                        for (int i = 0, len = slots.Length; i < len; i++)
                        {
                            if (slots[i] != null && slots[i].Length >= minimalLength)
                            {
                                lockObject.EnterWriteLock();
                                try
                                {
                                    if (slots[i] != null)
                                    {
                                        var cur = slots[i];
                                        slots[i] = null;
                                        return cur;
                                    }
                                }
                                finally
                                {
                                    lockObject.ExitWriteLock();
                                }
                            }
                        }
                    }
                    finally
                    {
                        lockObject.ExitUpgradeableReadLock();
                    }

                    var length = FindNearestPowerOfTwo(minimalLength);
                    return new byte[length];
                }
            }

            public void Recycle(byte[] array)
            {
                if (array.Length < this.minimalLengthRaw)
                {
                    return;
                }

                lockObject.EnterWriteLock();
                try
                {
                    for (int i = 0, len = slots.Length; i < len; i++)
                    {
                        if (slots[i] == null)
                        {
                            slots[i] = array;
                            return;
                        }
                    }
                }
                finally
                {
                    lockObject.ExitWriteLock();
                }
            }
        }

        internal static readonly ByteArrayPool shared = new ByteArrayPool(64);
        private readonly Bucket[] buckets;

        public ByteArrayPool(int capacity = 32)
        {
            // 1<<3 ... 1<<30
            this.buckets = new Bucket[28];

            for (int i = 0; i < buckets.Length; i++)
            {
                int minimalLengthRaw = 1 << (i + 3);
                int bucketCapacity = Math.Min(capacity, 16);
                buckets[i] = new Bucket(minimalLengthRaw, bucketCapacity);
            }
        }

        public byte[] Get(int minimalLength)
        {
            if (minimalLength == 0)
            {
                return Array.Empty<byte>();
            }

            int index = Log2(minimalLength) - 3;

            if (index < 0 || index >= buckets.Length)
            {
                var length = FindNearestPowerOfTwo(minimalLength);
                return new byte[length];
            }

            return buckets[index].Get(minimalLength);
        }

        public void Recycle(byte[] array)
        {
            if (array.Length == 0)
            {
                return;
            }

            int index = Log2(array.Length) - 3;

            if (index < 0 || index >= buckets.Length)
            {
                return;
            }

            buckets[index].Recycle(array);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Log2(int value)
        {
            int result = 0;
            while (value > 1)
            {
                value >>= 1;
                result++;
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int FindNearestPowerOfTwo(int value)
        {
            // 1<<30
            if (value > 0x40000000)
            {
                return int.MaxValue;
            }

            int po2 = 8;
            while (po2 < value)
            {
                po2 = po2 << 1;
            }

            return po2;
        }
    }
}
