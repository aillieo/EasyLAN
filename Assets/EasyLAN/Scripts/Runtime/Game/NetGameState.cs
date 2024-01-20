// -----------------------------------------------------------------------
// <copyright file="NetGameState.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils.EasyLAN
{
    public enum NetGameState : byte
    {
        Uninitiated = 0,
        Listening = 1,
        Ready = 2,
        GamePlaying = 3,
        Destroyed = 4,
    }
}
