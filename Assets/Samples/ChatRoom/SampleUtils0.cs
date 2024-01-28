// -----------------------------------------------------------------------
// <copyright file="SampleUtils0.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils.EasyLAN.Sample
{
    using System.Linq;
    using UnityEngine;

    public static class SampleUtils0
    {
        public static string GetRandomName()
        {
            return string.Concat(Enumerable.Range(0, 5).Select(_ => (char)Random.Range((short)'A', (short)'Z')));
        }

        public static string GetPlayerTitle(int playerId, string playerName)
        {
            return $"[{playerId}]{playerName}";
        }
    }
}
