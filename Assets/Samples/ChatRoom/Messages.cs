// -----------------------------------------------------------------------
// <copyright file="Messages.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils.EasyLAN.Sample
{
    using System;

    public static class Messages
    {
        [Serializable]
        public class String
        {
            public string value;
        }

        [Serializable]
        public class ReqPlayers
        {
        }

        [Serializable]
        public class PlayerList
        {
            public ChatModel.PlayerInfo[] players;
        }
    }
}
