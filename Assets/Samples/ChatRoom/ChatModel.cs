// -----------------------------------------------------------------------
// <copyright file="ChatModel.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils.EasyLAN.Sample
{
    using System;
    using System.Collections.Generic;

    public class ChatModel
    {
        public struct Message
        {
            public byte player;
            public string content;
        }

        [Serializable]
        public struct PlayerInfo
        {
            public byte playerId;
            public NetPlayerInfo netInfo;
        }

        public readonly Dictionary<byte, PlayerInfo> players = new Dictionary<byte, PlayerInfo>();
        public readonly Queue<Message> messages = new Queue<Message>();
        public NetPlayer owner;
        public NetGameInstance room;

        public void Clear()
        {
            players.Clear();
            messages.Clear();
        }
    }
}
