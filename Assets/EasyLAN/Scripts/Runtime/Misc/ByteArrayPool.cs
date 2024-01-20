// -----------------------------------------------------------------------
// <copyright file="ByteArrayPool.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils.EasyLAN
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;

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
                    this.lockObject.EnterUpgradeableReadLock();
                    try
                    {
                        for (int i = 0, len = this.slots.Length; i < len; i++)
                        {
                            if (this.slots[i] != null && this.slots[i].Length >= minimalLength)
                            {
                                this.lockObject.EnterWriteLock();
                                try
                                {
                                    if (this.slots[i] != null)
                                    {
                                        var cur = this.slots[i];
                                        this.slots[i] = null;
                                        return cur;
                                    }
                                }
                                finally
                                {
                                    this.lockObject.ExitWriteLock();
                                }
                            }
                        }
                    }
                    finally
                    {
                        this.lockObject.ExitUpgradeableReadLock();
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

                this.lockObject.EnterWriteLock();
                try
                {
                    for (int i = 0, len = this.slots.Length; i < len; i++)
                    {
                        if (this.slots[i] == null)
                        {
                            this.slots[i] = array;
                            return;
                        }
                    }
                }
                finally
                {
                    this.lockObject.ExitWriteLock();
                }
            }
        }

        internal static readonly ByteArrayPool shared = new ByteArrayPool(64);
        private readonly Bucket[] buckets;

        public ByteArrayPool(int capacity = 32)
        {
            // 1<<3 ... 1<<30
            this.buckets = new Bucket[28];

            for (var i = 0; i < this.buckets.Length; i++)
            {
                var minimalLengthRaw = 1 << (i + 3);
                var bucketCapacity = Math.Min(capacity, 16);
                this.buckets[i] = new Bucket(minimalLengthRaw, bucketCapacity);
            }
        }

        public byte[] Get(int minimalLength)
        {
            if (minimalLength == 0)
            {
                return Array.Empty<byte>();
            }

            var index = Log2(minimalLength) - 3;

            if (index < 0 || index >= this.buckets.Length)
            {
                var length = FindNearestPowerOfTwo(minimalLength);
                return new byte[length];
            }

            return this.buckets[index].Get(minimalLength);
        }

        public void Recycle(byte[] array)
        {
            if (array.Length == 0)
            {
                return;
            }

            var index = Log2(array.Length) - 3;

            if (index < 0 || index >= this.buckets.Length)
            {
                return;
            }

            this.buckets[index].Recycle(array);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Log2(int value)
        {
            var result = 0;
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

            var po2 = 8;
            while (po2 < value)
            {
                po2 = po2 << 1;
            }

            return po2;
        }
    }
}
