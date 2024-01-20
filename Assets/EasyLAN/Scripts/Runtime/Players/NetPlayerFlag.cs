// -----------------------------------------------------------------------
// <copyright file="NetPlayerFlag.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils.EasyLAN
{
    using System;

    [Flags]
    public enum NetPlayerFlag
    {
        Unknown = 0,
        Local = 1 << 1,
        Remote = 1 << 2,
        Host = 1 << 3,
    }
}
