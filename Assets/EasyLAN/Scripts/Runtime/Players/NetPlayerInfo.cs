// -----------------------------------------------------------------------
// <copyright file="NetPlayerInfo.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils.EasyLAN
{
    using System;
    using UnityEngine;

    [Serializable]
    [Proto(2)]
    public struct NetPlayerInfo : IProtocol
    {
        [field: SerializeField]
        public string playerName { get; set; }

        public void Deserialize(ByteBuffer readFrom)
        {
            this.playerName = readFrom.ConsumeString();
        }

        public void Serialize(ByteBuffer writeTo)
        {
            writeTo.Append(this.playerName);
        }
    }
}
