// -----------------------------------------------------------------------
// <copyright file="AcceptPlayer.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils.EasyLAN
{
    using System;

    [Serializable]
    [Proto(3)]
    public struct AcceptPlayer : IProtocol
    {
        public byte host;
        public byte instanceId;

        public void Deserialize(ByteBuffer readFrom)
        {
            this.host = readFrom.ConsumeByte();
            this.instanceId = readFrom.ConsumeByte();
        }

        public void Serialize(ByteBuffer writeTo)
        {
            writeTo.Append(this.host);
            writeTo.Append(this.instanceId);
        }
    }
}
