// -----------------------------------------------------------------------
// <copyright file="IProtocol.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils.EasyLAN
{
    public interface IProtocol
    {
        void Serialize(ByteBuffer writeTo);

        void Deserialize(ByteBuffer readFrom);
    }
}
