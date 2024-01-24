// -----------------------------------------------------------------------
// <copyright file="CustomBytes.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils.EasyLAN
{
    using System;

    [Serializable]
    [Proto(5)]
    public struct CustomBytes : IProtocol
    {
        public byte[] bytes;

        public void Deserialize(ByteBuffer readFrom)
        {
            var count = readFrom.ConsumeInt();
            if (count > 0)
            {
                this.bytes = readFrom.Consume(count);
            }
        }

        public void Serialize(ByteBuffer writeTo)
        {
            if (this.bytes != null)
            {
                writeTo.Append(this.bytes.Length);
                writeTo.Append(this.bytes);
            }
            else
            {
                writeTo.Append(0);
            }
        }
    }
}
