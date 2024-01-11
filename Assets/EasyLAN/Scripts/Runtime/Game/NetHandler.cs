using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AillieoUtils.EasyLAN
{
    internal class NetHandler
    {
        public NetGameInstance game;
        public event Action<int, byte[]> onData;
        private readonly Dictionary<int, Action<ByteBuffer>> handlers = new Dictionary<int, Action<ByteBuffer>>();

        public async Task SendAsync(int target, ByteBuffer buffer, CancellationToken cancellationToken)
        {
            int localPlayerId = game.localPlayer.id;
            bool isHost = game.localPlayer.IsHost();

            var bytes = buffer.ToArray();

            if (target == localPlayerId)
            {
                this.onData?.Invoke(localPlayerId, bytes);
                return;
            }

            if (isHost)
            {
                // host发给非host的普通玩家
                var player = game.GetPlayer(target);
                buffer.Prepend(localPlayerId);
                buffer.Prepend(ChannelFlag.Direct);
                await player.connection.SendAsync(buffer, cancellationToken);
            }
            else
            {
                // 非host的普通玩家发出
                buffer.Prepend(target);
                buffer.Prepend(ChannelFlag.Forward);
                await game.localPlayer.connection.SendAsync(buffer, cancellationToken);
            }

            buffer.Clear();
        }

        public async Task BroadcastAsync(ByteBuffer buffer, CancellationToken cancellationToken)
        {
            int localPlayerId = game.localPlayer.id;
            bool isHost = game.localPlayer.IsHost();

            // 先发自己
            var bytes = buffer.ToArray();
            this.onData?.Invoke(localPlayerId, bytes);

            if (isHost)
            {
                // host发给非host的普通玩家
                buffer.Prepend(localPlayerId);
                buffer.Prepend(ChannelFlag.Direct);
                foreach (var player in game.GetAllPlayers())
                {
                    if (player.id != localPlayerId)
                    {
                        await player.connection.SendAsync(buffer, cancellationToken);
                    }
                }
            }
            else
            {
                buffer.Prepend(ChannelFlag.Broadcast);
                await game.localPlayer.connection.SendAsync(buffer, cancellationToken);
            }

            buffer.Clear();
        }

        internal void RegisterPlayer(NetPlayer player, NetConnection netConnection)
        {
            Action<ByteBuffer> handler;
            int playerId = player.id;

            if (this.game.localPlayer.IsHost())
            {
                handler = buffer => {
                    this.OnData(playerId, buffer);
                };
            }
            else
            {
                handler = this.OnData;
            }

            netConnection.onData += handler;
            this.handlers.Add(playerId, handler);
        }
        
        internal void UnregisterPlayer(int playerId)
        {

        }

        private void OnData(ByteBuffer buffer)
        {
            var sender = buffer.ConsumeInt();
            this.OnData(sender, buffer);
            buffer.Clear();
        }

        private void OnData(int sender, ByteBuffer buffer)
        {
            var channel = buffer.ConsumeByte();
            switch (channel)
            {
                case ChannelFlag.Direct:
                    this.onData?.Invoke(sender, buffer.ToArray());
                    buffer.Clear();
                    break;
                case ChannelFlag.Broadcast:
                    this.BroadcastAsync(buffer, CancellationToken.None).Await();
                    break;
                case ChannelFlag.Forward:
                    int target = buffer.ConsumeInt();
                    this.SendAsync(target, buffer, CancellationToken.None).Await();
                    break;
                default:
                    throw new ELException();
            }
        }
    }
}
