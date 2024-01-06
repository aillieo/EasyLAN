using System;
using System.Text;
using UnityEngine;

namespace AillieoUtils.EasyLAN
{
    [Serializable]
    public struct NetGameInfo : IInternalObject
    {
        public string gameName;
        public string playerName;
        public int version;

        public string ip;
        public int port;

        public static readonly byte[] headBytes = Encoding.UTF8.GetBytes("AU.EL.NGI");

        public override string ToString()
        {
            return $"gameName={gameName},playerName={playerName},version={version},ip={ip},port={port}";
        }

        public static byte[] Serialize(NetGameInfo netGameInfo)
        {
            {
                return SerializeUtils.Serialize(netGameInfo);
            }

            string json = JsonUtility.ToJson(netGameInfo);
            string data = $"{netGameInfo.version}|{json}";
            byte[] bodyBytes = Encoding.UTF8.GetBytes(data);
            byte[] bytes = new byte[headBytes.Length + bodyBytes.Length];
            Array.Copy(headBytes, 0, bytes, 0, headBytes.Length);
            Array.Copy(bodyBytes, 0, bytes, headBytes.Length, bodyBytes.Length);
            return bytes;
        }

        public static bool ValidateHead(byte[] bytes)
        {
            {
                return true;
            }

            if (bytes.Length < headBytes.Length)
            {
                return false;
            }

            for (int i = 0; i < headBytes.Length; ++i)
            {
                if (headBytes[i] != bytes[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static bool Deserialize(byte[] buffer, out NetGameInfo netGameInfo)
        {
            {
                var succeed = SerializeUtils.Deserialize(buffer, out IInternalObject o);
                if (succeed)
                {
                    netGameInfo = (NetGameInfo)o;
                    return true;
                }
                else
                {
                    netGameInfo = default;
                    return false;
                }
            }

            if (!ValidateHead(buffer))
            {
                netGameInfo = default;
                return false;
            }

            try
            {
                string body = Encoding.UTF8.GetString(buffer, headBytes.Length, buffer.Length - headBytes.Length);
                int sep = body.IndexOf('|');
                if (sep < 0)
                {
                    netGameInfo = default;
                    return false;
                }

                string version = body.Substring(0, sep);
                string json = body.Substring(sep + 1);
                netGameInfo = JsonUtility.FromJson<NetGameInfo>(json);
                return true;
            }
            catch(Exception e)
            {
                UnityEngine.Debug.LogException(e);
                netGameInfo = default;
                return false;
            }
        }
    }
}
