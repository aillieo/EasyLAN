// -----------------------------------------------------------------------
// <copyright file="NetGameInstance.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils.EasyLAN
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

    public class NetGameInstance : IDisposable
    {
        internal readonly IdGeneratorReusable idGenerator = new IdGeneratorReusable();
        internal readonly NetHandler netHandler = new NetHandler();

        internal byte hostPlayer;
        private readonly Dictionary<byte, NetPlayer> allPlayers = new Dictionary<byte, NetPlayer>();

        internal NetGameInstance()
        {
            this.netHandler.game = this;
            this.netHandler.onData += this.HandleData;
        }

        public event Action<NetPlayer> onNewPlayer;

        public event Action<NetPlayer> onPlayerLeave;

        public event Action<NetGameState> onGameStateChanged;

        public event Action<byte, byte[]> onPlayerMessage;

        public NetGameState state { get; internal set; } = NetGameState.Uninitiated;

        public NetGameInfo info { get; internal set; }

        public NetPlayer localPlayer { get; internal set; }

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

        public async Task Start()
        {
            if (!this.localPlayer.IsHost())
            {
                throw new InvalidOperationException();
            }

            if (this.state != NetGameState.Ready)
            {
                // throw new InvalidOperationException();
            }

            var buffer = new ByteBuffer(0);
            buffer.Append(Protocols.GetId<SyncNetGameState>());
            var gameState = new SyncNetGameState() { state = NetGameState.GamePlaying };
            gameState.Serialize(buffer);
            await this.netHandler.BroadcastAsync(buffer, CancellationToken.None);

            this.ChangeState(NetGameState.GamePlaying);
        }

        public bool IsListening()
        {
            return this.state == NetGameState.Listening;
        }

        public void StartAcceptPlayer(ref NetGameInfo info, CancellationToken cancellationToken)
        {
            var listener = new TcpListener(IPAddress.Any, 0);
            listener.Start();

            if (string.IsNullOrEmpty(info.ip))
            {
                info.ip = LANUtils.GetLocalIpAddress().ToString();
            }

            if (listener.LocalEndpoint is IPEndPoint iep)
            {
                info.port = iep.Port;
            }

            this.AcceptPlayerAsync(listener, cancellationToken).Await();
        }

        public NetPlayer GetPlayer(byte playerId)
        {
            if (this.allPlayers.TryGetValue(playerId, out NetPlayer player))
            {
                return player;
            }

            return null;
        }

        public void SendToHost(byte[] data)
        {
            this.Send(this.hostPlayer, data, CancellationToken.None);
        }

        public void Send(byte playerId, byte[] data)
        {
            this.Send(playerId, data, CancellationToken.None);
        }

        public void Broadcast(byte[] data)
        {
            this.Broadcast(data, CancellationToken.None);
        }

        public void SendToHost(byte[] data, CancellationToken cancellationToken)
        {
            this.Send(this.hostPlayer, data, cancellationToken);
        }

        public void Send(byte target, byte[] data, CancellationToken cancellationToken)
        {
            var buffer = new ByteBuffer(0);
            buffer.Append(Protocols.GetId<CustomBytes>());
            var proto = new CustomBytes() { bytes = data };
            proto.Serialize(buffer);
            this.netHandler.SendAsync(target, buffer, cancellationToken).Await();
        }

        public void Broadcast(byte[] data, CancellationToken cancellationToken)
        {
            var buffer = new ByteBuffer(0);
            buffer.Append(Protocols.GetId<CustomBytes>());
            var proto = new CustomBytes() { bytes = data };
            proto.Serialize(buffer);
            this.netHandler.BroadcastAsync(buffer, cancellationToken).Await();
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

        internal void ChangeState(NetGameState newState)
        {
            if (this.state == newState)
            {
                return;
            }

            this.state = newState;
            this.onGameStateChanged?.Invoke(newState);
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

        private void HandleData(byte player, IProtocol data)
        {
            UnityEngine.Debug.Log($"[GAME]: {player} {data} ");

            switch (data)
            {
                case SyncNetGameState syncNetGameState:
                    this.ChangeState(syncNetGameState.state);
                    break;
                case CustomBytes customBytes:
                    this.onPlayerMessage(player, customBytes.bytes);
                    break;
            }
        }
    }
}
