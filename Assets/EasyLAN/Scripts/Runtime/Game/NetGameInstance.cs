using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace AillieoUtils.EasyLAN
{
    public class NetGameInstance : IDisposable
    {
        public event Action<NetPlayer> onNewPlayer;
        public event Action<NetPlayer> onPlayerLeave;
        public event Action<NetGameState> onGameStateChanged;
        public event Action<int, byte[]> onPlayerMessage;

        public NetGameState state { get; internal set; } = NetGameState.Uninitiated;
        public NetGameInfo info { get; internal set; }
        public NetPlayer localPlayer { get; internal set; }

        internal readonly IdGenerator idGenerator = new IdGenerator();
        internal readonly NetHandler netHandler = new NetHandler();
        private readonly Dictionary<int, NetPlayer> allPlayers = new Dictionary<int, NetPlayer>();

        internal NetGameInstance()
        {
            netHandler.game = this;

            netHandler.onData += (p, b) => UnityEngine.Debug.LogError($"收到来自 {p} 的消息 { System.Text.Encoding.UTF8.GetString(b) }");
        }

        public void Dispose()
        {
            // TODO
        }

        internal void AddPlayer(NetPlayer player)
        {
            this.allPlayers.Add(player.id, player);
        }

        internal void RemovePlayer(NetPlayer player)
        {
        }

        public IEnumerable<NetPlayer> GetAllPlayers()
        {
            foreach (var pair in this.allPlayers)
            {
                var player = pair.Value;
                if (player != null && player.IsConnected() && player.state == NetPlayerState.Authenticated)
                {
                    yield return player;
                }
            }
        }

        internal void ChangeState(NetGameState newState)
        {
            if(this.state == newState)
            {
                return;
            }

            this.state = newState;
            this.onGameStateChanged?.Invoke(newState);
        }

        public async Task Start()
        {
            if (!localPlayer.IsHost())
            {
                throw new InvalidOperationException();
            }

            this.ChangeState(NetGameState.GamePlaying);
        }

        public bool IsListening()
        {
            return this.state == NetGameState.Listening;
        }

        public void StartAcceptPlayer(ref NetGameInfo info, CancellationToken cancellationToken)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 0);
            listener.Start();

            if (string.IsNullOrEmpty(info.ip))
            {
                info.ip = LANUtils.GetLocalIpAddress().ToString();
            }

            if (listener.LocalEndpoint is IPEndPoint iep)
            {
                info.port = iep.Port;
            }

            AcceptPlayerAsync(listener, cancellationToken).Await();
        }

        private async Task AcceptPlayerAsync(TcpListener listener, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var connection = await NetConnection.AcceptAsync(listener, cancellationToken);
                var player = await NetPlayer.AcceptRemote(connection, this, cancellationToken);
                if (player != null)
                {
                    // 我是主机 我接受了一个远端的连接
                    this.netHandler.RegisterPlayer(player, connection);
                    this.AddPlayer(player);
                }
            }

            listener.Stop();
        }

        public NetPlayer GetPlayer(int playerId)
        {
            if (this.allPlayers.TryGetValue(playerId, out NetPlayer player))
            {
                return player;
            }

            return null;
        }

        public void Send(int playerId, byte[] data)
        {
            Send(playerId, data, CancellationToken.None);
        }

        public void Broadcast(byte[] data)
        {
            Broadcast(data, CancellationToken.None);
        }

        public void Send(int target, byte[] data, CancellationToken cancellationToken)
        {
            ByteBuffer buffer = new ByteBuffer(data.Length);
            buffer.Append(data);
            this.netHandler.SendAsync(target, buffer, cancellationToken).Await();
        }

        public void Broadcast(byte[] data, CancellationToken cancellationToken)
        {
            ByteBuffer buffer = new ByteBuffer(data.Length);
            buffer.Append(data);
            this.netHandler.BroadcastAsync(buffer, cancellationToken).Await();
        }
    }
}
