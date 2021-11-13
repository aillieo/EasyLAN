using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml.Serialization;

namespace AillieoUtils.EasyLAN
{
    public class NetGameInfo
    {
        public string gameName;
        public string playerName;
        public int version;

        public string ip;
        public int port;

        public override string ToString()
        {
            return $"gameName={gameName},playerName={playerName},version={version},ip={ip},port={port}";
        }

        public static byte[] Serialize(NetGameInfo netGameInfo)
        {
            //using (MemoryStream memStream = new MemoryStream())
            //{
            //    XmlSerializer xmlSerializer = new XmlSerializer(typeof(NetGameInfo));
            //    xmlSerializer.Serialize(memStream, netGameInfo);
            //    return memStream.ToArray();
            //}

            string str = $"{netGameInfo.version}|{netGameInfo.gameName}|{netGameInfo.playerName}";
            return Encoding.UTF8.GetBytes(str);
        }

        public static NetGameInfo Deserialize(byte[] buffer)
        {
            //using (MemoryStream memStream = new MemoryStream(buffer))
            //{
            //    XmlSerializer xmlSerializer = new XmlSerializer(typeof(NetGameInfo));
            //    return xmlSerializer.Deserialize(memStream) as NetGameInfo;
            //}

            string str = Encoding.UTF8.GetString(buffer);
            string[] strs = str.Split('|');
            int.TryParse(strs[0], out int ver);

            return new NetGameInfo()
            {
                version = ver,
                gameName = strs[1],
                playerName = strs[2],
            };
        }
    }
}
