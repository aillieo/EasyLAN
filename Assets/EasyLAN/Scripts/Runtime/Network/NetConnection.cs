using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace AillieoUtils.EasyLAN
{
    internal class NetConnection : IDisposable
    {
        private TcpClient tcpClient;
        private StreamReader reader;
        private StreamWriter writer;
        private CancellationToken cancellationToken;

        public event Action<byte[]> onData;
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
            catch (Exception ex)
            {
                Debug.LogError($"Failed to connect to {ip}:{port}. Exception: {ex.Message}");
                rawTcpClient.Dispose();
                return null;
            }

            NetConnection netConnection = new NetConnection();
            netConnection.cancellationToken = cancellationToken;
            netConnection.ConfigTcpClient(rawTcpClient);

            return netConnection;
        }

        public static async Task<NetConnection> AcceptAsync(TcpListener listener, CancellationToken cancellationToken)
        {
            try
            {
                var rawTcpClient = await listener.AcceptTcpClientAsync();
                NetConnection netConnection = new NetConnection();
                netConnection.cancellationToken = cancellationToken;
                netConnection.ConfigTcpClient(rawTcpClient);
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
         
        private void ConfigTcpClient(TcpClient rawTcpClient)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                this.tcpClient = rawTcpClient;
                var stream = tcpClient.GetStream();
                this.reader = new StreamReader(stream);
                this.writer = new StreamWriter(stream);
                this.writer.AutoFlush = true;
                this.StartReadingDataAsync().Await();
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

        public void OnData(string data)
        {
            UnityEngine.Debug.Log("[RECV]: " + data);
            onData?.Invoke(Encoding.UTF8.GetBytes(data));
        }

        public void OnDisconnected(string data)
        {
            UnityEngine.Debug.Log("[CLOSE]: " + data);
            onDisconnected?.Invoke();
        }

        public async Task SendAsync(string data)
        {
            UnityEngine.Debug.Log("[SEND]: " + data);
            await this.writer.WriteLineAsync(data);
        }

        private async Task StartReadingDataAsync()
        {
            while (!this.cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var data = await this.reader.ReadLineAsync();
                    if (data == null)
                    {
                        break;
                    }
                    else
                    {
                        this.OnData(data);
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

            this.writer?.Close();
            this.reader?.Close();
        }
    }
}
