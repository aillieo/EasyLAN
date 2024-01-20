// -----------------------------------------------------------------------
// <copyright file="NetConnection.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils.EasyLAN
{
    using System;
    using System.IO;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;

    internal class NetConnection : IDisposable
    {
        private TcpClient tcpClient;
        private NetworkStream stream;

        public event Action<ByteBuffer> onData;

        public event Action onDisconnected;

        private NetConnection()
        {
        }

        public static async Task<NetConnection> ConnectAsync(string ip, int port, CancellationToken cancellationToken)
        {
            var rawTcpClient = new TcpClient();

            try
            {
                await rawTcpClient.ConnectAsync(ip, port);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                rawTcpClient.Dispose();
                return null;
            }

            var netConnection = new NetConnection();
            netConnection.ConfigTcpClient(rawTcpClient, cancellationToken);

            return netConnection;
        }



        public static async Task<NetConnection> AcceptAsync(TcpListener listener, CancellationToken cancellationToken)
        {
            try
            {
                var rawTcpClient = await listener.AcceptTcpClientAsync();
                var netConnection = new NetConnection();
                netConnection.ConfigTcpClient(rawTcpClient, cancellationToken);
                return netConnection;
            }
            catch (ObjectDisposedException e) when (e.ObjectName == typeof(Socket).FullName)
            {
                return null;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
            catch (SocketException)
            {
                return null;
            }
        }

        public bool IsConnected()
        {
            return this.tcpClient != null && this.tcpClient.Connected;
        }

        private void ConfigTcpClient(TcpClient rawTcpClient, CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                this.tcpClient = rawTcpClient;
                this.stream = this.tcpClient.GetStream();
                this.StartReadingDataAsync(cancellationToken).Await();
            }
        }

        public void Dispose()
        {
            if (this.tcpClient != null)
            {
                this.tcpClient.Dispose();
                this.tcpClient = null;
            }
        }

        private void OnData(ByteBuffer buffer)
        {
            UnityEngine.Debug.Log("[RECV]: " + buffer.ToArray().ToStringEx());
            this.onData?.Invoke(buffer);
        }

        private void OnDisconnected(string data)
        {
            UnityEngine.Debug.Log("[CLOSE]: " + data);
            this.onDisconnected?.Invoke();
        }

        public async Task SendAsync(ByteBuffer buffer)
        {
            await this.SendAsync(buffer, CancellationToken.None);
        }

        public async Task SendAsync(ByteBuffer buffer, CancellationToken cancellationToken)
        {
            if (!this.IsConnected())
            {
                throw new ELException();
            }

            UnityEngine.Debug.Log("[SEND]: " + buffer.ToArray().ToStringEx());

            var length = buffer.Length;
            buffer.Prepend(length);
            var data = buffer.ToArray();
            buffer.Clear();
            await this.stream.WriteAsync(data, 0, data.Length, cancellationToken);
        }

        private async Task StartReadingDataAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var length = await this.stream.ReadIntAsync(cancellationToken);

                    if (length < 0)
                    {
                        break;
                    }

                    var buffer = await this.stream.ReadBytesAsync(length, cancellationToken);
                    if (buffer == null)
                    {
                        break;
                    }
                    else
                    {
                        this.OnData(buffer);
                    }
                }
                catch (ObjectDisposedException e) when (e.ObjectName == typeof(NetworkStream).FullName)
                {
                    break;
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (IOException)
                {
                    break;
                }
            }

            var info = cancellationToken.IsCancellationRequested.ToString();

            this.OnDisconnected(info);

            this.stream?.Close();
        }
    }
}
