using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

namespace AillieoUtils.EasyLAN
{
    public class NetPlayer : IDisposable
    {
        public NetPlayerFlag flag { get; private set; }
        private bool disposedValue;

        private TcpClient tcpClient;
        private UdpClient udpClient;

        public static NetPlayer CreateHost()
        {
            NetPlayer player = new NetPlayer();
            player.flag = NetPlayerFlag.Host;
            return player;
        }

        private NetPlayer()
        {

        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // managed
                }

                // unmanaged
                if (tcpClient != null)
                {
                    tcpClient.Dispose();
                    tcpClient = null;
                }

                if (udpClient != null)
                {
                    udpClient.Dispose();
                    udpClient = null;
                }

                disposedValue = true;
            }
        }

        ~NetPlayer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
