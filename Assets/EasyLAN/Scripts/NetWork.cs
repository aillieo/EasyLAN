using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace AillieoUtils.EasyLAN
{
    public class NetWork : IDisposable
    {
        private UdpClient udpClient;
        private CancellationToken cancellationToken;
        private bool disposedValue;

        private NetWork()
        {

        }

        public static async Task<NetGameInfo[]> Search(int port, CancellationToken cancellationToken)
        {
            // todo 返回一个NetWork实例 可以反复使用socket
            using (NetWork net = new NetWork())
            {
                net.cancellationToken = cancellationToken;

                net.udpClient = new UdpClient();
                net.udpClient.EnableBroadcast = true;
                net.udpClient.MulticastLoopback = true;

                // 客户端广播自己 去找服务器
                NetGameInfo netGameInfo = new NetGameInfo() { playerName = "client" };
                byte[] bytes = NetGameInfo.Serialize(netGameInfo);
                IPEndPoint broadcast = new IPEndPoint(IPAddress.Broadcast, port);
                await net.udpClient.SendAsync(bytes, bytes.Length, broadcast);

                if (cancellationToken.IsCancellationRequested)
                {
                    return Array.Empty<NetGameInfo>();
                }

                List<NetGameInfo> games = null;

                while (!cancellationToken.IsCancellationRequested)
                {
                    Task complete = await Task.WhenAny(
                        net.udpClient.ReceiveAsync(),
                        Task.Delay(1000));

                    if (complete is Task<UdpReceiveResult> receive)
                    {
                        UdpReceiveResult result = await receive;

                        // 客户端收到了服务器的回应
                        NetGameInfo remote = NetGameInfo.Deserialize(result.Buffer);
                        remote.ip = result.RemoteEndPoint.Address.ToString();
                        remote.port = result.RemoteEndPoint.Port;
                        if (remote != null)
                        {
                            Debug.Log("收到了一个");
                            if (games == null)
                            {
                                games = new List<NetGameInfo>();
                            }

                            games.Add(remote);
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                if (games != null)
                {
                    return games.ToArray();
                }

                return Array.Empty<NetGameInfo>();
            }
        }

        public static async Task Listen(string hostName, int port, CancellationToken cancellationToken)
        {
            // todo 返回一个NetPlayer实例 可以反复使用socket
            using (NetWork net = new NetWork())
            {
                net.udpClient = new UdpClient(port);
                net.udpClient.EnableBroadcast = true;
                net.udpClient.MulticastLoopback = true;

                while (!cancellationToken.IsCancellationRequested)
                {
                    // 服务器监听客户端消息
                    while (net.udpClient.Available > 0 && !cancellationToken.IsCancellationRequested)
                    {
                        IPEndPoint any = new IPEndPoint(IPAddress.Any, port);
                        byte[] receive = net.udpClient.Receive(ref any);
                        NetGameInfo remote = NetGameInfo.Deserialize(receive);
                        remote.ip = any.Address.ToString();
                        remote.port = any.Port;
                        if (remote != null)
                        {
                            // 发现了一个客户端 回包
                            Debug.Log("收到了一个客户端" + remote.ToString());
                            NetGameInfo netGameInfo = new NetGameInfo() { gameName = hostName };
                            byte[] bytes = NetGameInfo.Serialize(netGameInfo);
                            await net.udpClient.SendAsync(bytes, bytes.Length, any);
                        }
                    }

                    await Task.Delay(100);
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {

                Debug.LogError("Dispose");

                if (disposing)
                {
                    // managed
                    cancellationToken = default;
                }

                // unmanaged
                if (udpClient != null)
                {
                    try 
                    {
                        udpClient.Close();
                    }
                    finally
                    {
                        udpClient.Dispose();
                        udpClient = null;
                    }
                }

                disposedValue = true;
            }
        }

        ~NetWork()
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
