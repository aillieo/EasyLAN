// -----------------------------------------------------------------------
// <copyright file="IdGeneratorReusable.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils.EasyLAN
{
    using System.Threading;

    internal class IdGeneratorReusable
    {
        private int[] bitmap;

        public IdGeneratorReusable()
        {
            this.bitmap = new int[4];
        }

        public byte Get()
        {
            while (true)
            {
                for (var i = 0; i < this.bitmap.Length; i++)
                {
                    var originalValue = Volatile.Read(ref this.bitmap[i]);
                    for (var j = 0; j < 32; j++)
                    {
                        var mask = 1 << j;
                        if ((originalValue & mask) == 0)
                        {
                            var newValue = originalValue | mask;
                            var result = Interlocked.CompareExchange(ref this.bitmap[i], newValue, originalValue);
                            if (result == originalValue)
                            {
                                return (byte)((i * 32) + j);
                            }

                            break;
                        }
                    }
                }
            }
        }

        public void Release(byte id)
        {
            var index = id / 32;
            var bitOffset = id % 32;
            var mask = 1 << bitOffset;

            while (true)
            {
                var originalValue = Volatile.Read(ref this.bitmap[index]);
                var newValue = originalValue & ~mask;
                var result = Interlocked.CompareExchange(ref this.bitmap[index], newValue, originalValue);
                if (result == originalValue)
                {
                    break;
                }
            }
        }
    }
}
