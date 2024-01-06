using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace AillieoUtils.EasyLAN
{
    public static class LANUtils
    {
        public delegate bool Matcher(byte[] rawBytes);
        public delegate bool Parser<T>(byte[] rawBytes, out T obj);

        public static async Task<T[]> Search<T>(int portToSend, byte[] pattern, Parser<T> parser, CancellationToken cancellationToken)
        {
            // todo 返回一个NetWork实例 可以反复使用socket
            using (UdpClient udpClient = new UdpClient())
            {
                udpClient.EnableBroadcast = true;

                // 客户端广播自己 去找服务器
                IPEndPoint broadcast = new IPEndPoint(IPAddress.Broadcast, portToSend);
                await udpClient.SendAsync(pattern, pattern.Length, broadcast);

                if (cancellationToken.IsCancellationRequested)
                {
                    return Array.Empty<T>();
                }

                List<T> result = null;

                while (!cancellationToken.IsCancellationRequested)
                {
                    Task complete = await Task.WhenAny(
                        udpClient.ReceiveAsync(),
                        Task.Delay(1000));

                    if (complete is Task<UdpReceiveResult> receive)
                    {
                        UdpReceiveResult udpResult = await receive;

                        // 客户端收到了服务器的回应
                        if (parser(udpResult.Buffer, out T remote))
                        {
                            if (result == null)
                            {
                                result = new List<T>();
                            }

                            result.Add(remote);
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                if (result != null)
                {
                    return result.ToArray();
                }

                return Array.Empty<T>();
            }
        }

        public static async Task Listen(int portToListen, Matcher matcher, byte[] response, CancellationToken cancellationToken)
        {
            using (UdpClient udpClient = new UdpClient(portToListen))
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // 服务器监听客户端消息
                    while (udpClient.Available > 0 && !cancellationToken.IsCancellationRequested)
                    {
                        IPEndPoint any = new IPEndPoint(IPAddress.Any, portToListen);
                        byte[] receive = udpClient.Receive(ref any);
                        if (matcher(receive))
                        {
                            await udpClient.SendAsync(response, response.Length, any);
                        }
                    }

                    await Task.Delay(100);
                }
            }
        }
    }
}
