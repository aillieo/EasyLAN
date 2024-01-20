// -----------------------------------------------------------------------
// <copyright file="LANUtils.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils.EasyLAN
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;

    internal static class LANUtils
    {
        public delegate bool Matcher(byte[] rawBytes);

        public delegate bool Parser<T>(byte[] rawBytes, out T obj);

        public static bool IsNetworkAvailable()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return false;
            }

            return Application.internetReachability != NetworkReachability.NotReachable;
        }

        public static async Task<T[]> Search<T>(int portToSend, byte[] pattern, Parser<T> parser, CancellationToken cancellationToken)
        {
            using (var udpClient = new UdpClient())
            {
                udpClient.EnableBroadcast = true;

                // 客户端广播自己 去找服务器
                var broadcast = new IPEndPoint(IPAddress.Broadcast, portToSend);
                await udpClient.SendAsync(pattern, pattern.Length, broadcast);

                if (cancellationToken.IsCancellationRequested)
                {
                    return Array.Empty<T>();
                }

                List<T> result = null;

                while (!cancellationToken.IsCancellationRequested)
                {
                    var receive = udpClient.ReceiveAsync();
                    try
                    {
                        await receive.SetTimeout(1000);
                    }
                    catch (TimeoutException)
                    {
                        break;
                    }

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

                if (result != null)
                {
                    return result.ToArray();
                }

                return Array.Empty<T>();
            }
        }

        public static async Task Listen(int portToListen, Matcher matcher, byte[] response, CancellationToken cancellationToken)
        {
            using (var udpClient = new UdpClient(portToListen))
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // 服务器监听客户端消息
                    while (udpClient.Available > 0 && !cancellationToken.IsCancellationRequested)
                    {
                        var any = new IPEndPoint(IPAddress.Any, portToListen);
                        var receive = udpClient.Receive(ref any);
                        if (matcher(receive))
                        {
                            await udpClient.SendAsync(response, response.Length, any);
                        }
                    }

                    await Task.Delay(100);
                }
            }
        }

        public static IPAddress GetLocalIpAddress()
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface iface in interfaces)
            {
                if (iface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || iface.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    IPInterfaceProperties ipProperties = iface.GetIPProperties();
                    foreach (UnicastIPAddressInformation ipInfo in ipProperties.UnicastAddresses)
                    {
                        if (ipInfo.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            IPAddress localIpAddress = ipInfo.Address;

                            if (IsPrivateIp(localIpAddress))
                            {
                                return localIpAddress;
                            }
                        }
                    }
                }
            }

            throw new NotSupportedException();
        }

        private static bool IsPrivateIp(IPAddress ipAddress)
        {
            var addressBytes = ipAddress.GetAddressBytes();

            if (addressBytes.Length == 4)
            {
                // 10.0.0.0 - 10.255.255.255
                if (addressBytes[0] == 10)
                {
                    return true;
                }

                // 172.16.0.0 - 172.31.255.255
                if (addressBytes[0] == 172 && addressBytes[1] >= 16 && addressBytes[1] <= 31)
                {
                    return true;
                }

                // 192.168.0.0 - 192.168.255.255
                if (addressBytes[0] == 192 && addressBytes[1] == 168)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
