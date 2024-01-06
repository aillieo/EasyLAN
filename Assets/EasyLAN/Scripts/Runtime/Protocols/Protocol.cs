using System;

namespace AillieoUtils.EasyLAN
{
    [Flags]
    public enum Protocol : byte
    {
        Internal = 1 << 1,
        Custom = 1 << 2,
        SyncProp = 1 << 3,
        RPC = 1 << 4,

        Broadcast = 1 << 5,
        Forward = 1 << 6,
    }
}
