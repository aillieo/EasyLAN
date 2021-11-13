using System;

namespace AillieoUtils.EasyLAN
{
    [Flags]
    public enum NetPlayerFlag
    {
        Local = 1 << 1,
        Remote = 1 << 2,
        Host = 1 << 3,
    }
}
