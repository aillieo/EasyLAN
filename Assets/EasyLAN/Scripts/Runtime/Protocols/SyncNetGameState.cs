// -----------------------------------------------------------------------
// <copyright file="SyncNetGameState.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils.EasyLAN
{
    using System;

    [Serializable]
    [Proto(4)]
    public struct SyncNetGameState : IProtocol
    {
        public NetGameState state;

        public void Deserialize(ByteBuffer readFrom)
        {
            this.state = (NetGameState)readFrom.ConsumeByte();
        }

        public void Serialize(ByteBuffer writeTo)
        {
            writeTo.Append((byte)this.state);
        }
    }
}
