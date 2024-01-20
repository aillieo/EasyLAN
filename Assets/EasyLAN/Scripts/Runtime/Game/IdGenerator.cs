// -----------------------------------------------------------------------
// <copyright file="IdGenerator.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils.EasyLAN
{
    using System;
    using System.Threading;

    internal class IdGenerator
    {
        private readonly int mask;
        private int sid = 0;

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
            var id = Interlocked.Increment(ref this.sid);
            return id ^ this.mask;
        }
    }
}
