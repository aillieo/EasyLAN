// -----------------------------------------------------------------------
// <copyright file="ByteArrayExtensions.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils.EasyLAN
{
    using System.Linq;

    internal static class ByteArrayExtensions
    {
        public static string ToStringEx(this byte[] bytes)
        {
            return string.Join("-", bytes.Select(b => $"{b:X2}"));
        }
    }
}
