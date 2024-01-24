// -----------------------------------------------------------------------
// <copyright file="SampleUtils0.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Linq;
using AillieoUtils.EasyLAN;
using UnityEngine;

public static class SampleUtils0
{
    public static string GetRandomName()
    {
        return string.Concat(Enumerable.Range(0, 5).Select(_ => (char)Random.Range((short)'A', (short)'Z')));
    }

    public static string GetPlayerTitle(NetPlayer player)
    {
        return $"{(player.IsHost() ? "[H]" : string.Empty)}[{player.id}]{player.info.playerName}";
    }
}
