using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace AillieoUtils.EasyLAN
{
    internal class NetConnection : IDisposable
    {
        private TcpClient tcpClient;
        private NetworkStream stream;

        public event Action<ByteBuffer> onData;
        public event Action onDisconnected;

        private NetConnection()
        {
        }

        public bool IsConnected()
        {
            return this.tcpClient != null && this.tcpClient.Connected;
        }

        public static async Task<NetConnection> ConnectAsync(string ip, int port, CancellationToken cancellationToken)
        {
            TcpClient rawTcpClient = new TcpClient();

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

            NetConnection netConnection = new NetConnection();
            netConnection.ConfigTcpClient(rawTcpClient, cancellationToken);

            return netConnection;
        }

        public static async Task<NetConnection> AcceptAsync(TcpListener listener, CancellationToken cancellationToken)
        {
            try
            {
                var rawTcpClient = await listener.AcceptTcpClientAsync();
                NetConnection netConnection = new NetConnection();
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
         
        private void ConfigTcpClient(TcpClient rawTcpClient, CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                this.tcpClient = rawTcpClient;
                this.stream = tcpClient.GetStream();
                this.StartReadingDataAsync(cancellationToken).Await();
            }
        }

        public void Dispose()
        {
            if (tcpClient != null)
            {
                tcpClient.Dispose();
                tcpClient = null;
            }
        }

        private void OnData(byte[] data)
        {
            UnityEngine.Debug.Log("[RECV]: " + data.ToStringEx());
            ByteBuffer buffer = new ByteBuffer(data.Length);
            buffer.Append(data);
            onData?.Invoke(buffer);
        }

        private void OnDisconnected(string data)
        {
            UnityEngine.Debug.Log("[CLOSE]: " + data);
            onDisconnected?.Invoke();
        }

        public async Task SendAsync(ByteBuffer buffer, CancellationToken cancellationToken)
        {
            if (!IsConnected())
            {
                throw new ELException();
            }

            int length = buffer.Length;
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

                    var buffer = new byte[length];
                    var read = await this.stream.ReadBytesAsync(buffer, length, cancellationToken);
                    if (read <= 0)
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

            this.OnDisconnected("");

            this.stream?.Close();
        }
    }
}
