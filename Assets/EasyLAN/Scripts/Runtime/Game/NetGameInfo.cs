// -----------------------------------------------------------------------
// <copyright file="NetGameInfo.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils.EasyLAN
{
    using System;
    using System.Text;
    using UnityEngine;

    [Serializable]
    [Proto(1)]
    public struct NetGameInfo : IProtocol
    {
        internal static readonly byte[] headBytes = Encoding.UTF8.GetBytes("AU.EL.NGI");

        public string gameName;
        public string playerName;
        public string version;

        public string ip;
        public int port;

        public override string ToString()
        {
            return $"gameName={this.gameName},playerName={this.playerName},version={this.version},ip={this.ip},port={this.port}";
        }

        internal static bool ParseRawBytes(byte[] rawBytes, out NetGameInfo obj)
        {
            try
            {
                var buffer = new ByteBuffer(rawBytes);
                obj = default;
                obj.Deserialize(buffer);
                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }

            obj = default;
            return true;
        }

        internal static bool ValidateRawBytes(byte[] bytes)
        {
            return true;
        }

        internal static bool ValidateVersion(string local, string remote)
        {
            return true;
        }

        internal static bool ValidateHead(byte[] bytes)
        {
            if (bytes.Length != headBytes.Length)
            {
                return false;
            }

            for (var i = 0; i < headBytes.Length; ++i)
            {
                if (headBytes[i] != bytes[i])
                {
                    return false;
                }
            }

            return true;
        }

        internal byte[] GetRawBytes()
        {
            return this.ToBytes();
        }

        public void Serialize(ByteBuffer writeTo)
        {
            writeTo.Append(headBytes.Length);
            writeTo.Append(headBytes);
            writeTo.Append(this.version);
            var json = JsonUtility.ToJson(this);
            writeTo.Append(json);
        }

        public void Deserialize(ByteBuffer readFrom)
        {
            var headerLen = readFrom.ConsumeInt();
            var header = readFrom.Consume(headerLen);
            if (!ValidateHead(header))
            {
                UnityEngine.Debug.LogError("invalid header");
                return;
            }

            var remoteVersion = readFrom.ConsumeString();
            if (!ValidateVersion(this.version, remoteVersion))
            {
                UnityEngine.Debug.LogError("invalid version");
                return;
            }

            try
            {
                var json = readFrom.ConsumeString();
                this = JsonUtility.FromJson<NetGameInfo>(json);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
        }
    }
}
