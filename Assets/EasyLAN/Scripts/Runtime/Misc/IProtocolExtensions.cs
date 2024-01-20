// -----------------------------------------------------------------------
// <copyright file="IProtocolExtensions.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils.EasyLAN
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    internal static class IProtocolExtensions
    {
        public static byte[] ToBytes(this IProtocol proto)
        {
            using (var buffer = new ByteBuffer(8))
            {
                proto.Serialize(buffer);
                return buffer.ToArray();
            }
        }
    }
}
