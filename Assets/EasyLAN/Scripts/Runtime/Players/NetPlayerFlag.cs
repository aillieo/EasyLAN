using System;

namespace AillieoUtils.EasyLAN
{
    [Flags]
    public enum NetPlayerFlag
    {
        Unknown = 0,
        Local = 1 << 1,
        Remote = 1 << 2,
        Host = 1 << 3,
    }
}
