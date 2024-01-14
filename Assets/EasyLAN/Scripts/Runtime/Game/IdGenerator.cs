using System.Threading;

namespace AillieoUtils.EasyLAN
{
    using System.Threading;

    internal class IdGenerator
    {
        private int[] bitmap;

        public IdGenerator()
        {
            bitmap = new int[4];
        }

        public byte Get()
        {
            while (true)
            {
                for (int i = 0; i < bitmap.Length; i++)
                {
                    int originalValue = Volatile.Read(ref bitmap[i]);
                    for (int j = 0; j < 32; j++)
                    {
                        int mask = 1 << j;
                        if ((originalValue & mask) == 0)
                        {
                            int newValue = originalValue | mask;
                            int result = Interlocked.CompareExchange(ref bitmap[i], newValue, originalValue);
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
            int index = id / 32;
            int bitOffset = id % 32;
            int mask = 1 << bitOffset;

            while (true)
            {
                int originalValue = Volatile.Read(ref bitmap[index]);
                int newValue = originalValue & ~mask;
                int result = Interlocked.CompareExchange(ref bitmap[index], newValue, originalValue);
                if (result == originalValue)
                {
                    break;
                }
            }
        }
    }
}
