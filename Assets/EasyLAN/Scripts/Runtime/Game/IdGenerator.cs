using System;
using System.Threading;

namespace AillieoUtils.EasyLAN
{
    internal class IdGenerator
    {
        private int sid = 0;
        private readonly int mask;

        public IdGenerator()
            : this((int)DateTime.Now.Ticks & 0X7fff)
        {
        }

        public IdGenerator(int mask)
        {
            this.mask = mask;
        }

        public int GetId()
        {
            int id = Interlocked.Increment(ref sid);
            return id ^ mask;
        }
    }
}
