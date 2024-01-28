// -----------------------------------------------------------------------
// <copyright file="Logger.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils.EasyLAN
{
    using System;
    using System.Collections.Generic;

    public static class Logger
    {
        public static LogChannel mask;

        [Flags]
        public enum LogChannel
        {
            NetSend,
            NetReceive,
            NetForward,

            Game,
        }

        private static Dictionary<LogChannel, string> channelToTag = new Dictionary<LogChannel, string>()
        {
            { LogChannel.NetSend, "Send" },
             { LogChannel.NetReceive, "Recv" },
             { LogChannel.NetForward, "Fwd" },
             { LogChannel.Game, "Game" },
        };

        public static void Log(LogChannel channel, object message)
        {
            if ((mask & channel) == 0)
            {
                return;
            }

            UnityEngine.Debug.LogFormat("[{0}]{1}", channelToTag[channel], message);
        }

        public static void Warning(LogChannel channel, object message)
        {
            if ((mask & channel) == 0)
            {
                return;
            }

            UnityEngine.Debug.LogWarningFormat("[{0}]{1}", channelToTag[channel], message);

        }

        public static void Error(LogChannel channel, object message)
        {
            if ((mask & channel) == 0)
            {
                return;
            }

            UnityEngine.Debug.LogErrorFormat("[{0}]{1}", channelToTag[channel], message);

        }

        public static void Exception(LogChannel channel, Exception exception)
        {
            if ((mask & channel) == 0)
            {
                return;
            }

            UnityEngine.Debug.LogException(exception);
        }
    }
}
