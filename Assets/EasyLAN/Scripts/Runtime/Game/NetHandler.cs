// -----------------------------------------------------------------------
// <copyright file="NetHandler.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils.EasyLAN
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    internal class NetHandler
    {
        private readonly Dictionary<byte, Action<ByteBuffer>> handlers = new Dictionary<byte, Action<ByteBuffer>>();

        internal NetGameInstance game;

        public event Action<byte, byte[]> onData;

        public async Task SendAsync(byte target, ByteBuffer buffer, CancellationToken cancellationToken)
        {
            var localPlayerId = this.game.localPlayer.id;
            var isHost = this.game.localPlayer.IsHost();

            var bytes = buffer.ToArray();

            if (target == localPlayerId)
            {
                this.onData?.Invoke(localPlayerId, bytes);
                return;
            }

            if (isHost)
            {
                // host发给非host的普通玩家
                var player = this.game.GetPlayer(target);
                buffer.Prepend(ChannelFlag.Direct);
                buffer.Prepend(localPlayerId);
                await player.connection.SendAsync(buffer, cancellationToken);
            }
            else
            {
                // 非host的普通玩家发出
                buffer.Prepend(target);
                buffer.Prepend(ChannelFlag.Forward);
                await this.game.localPlayer.connection.SendAsync(buffer, cancellationToken);
            }

            buffer.Clear();
        }

        public async Task BroadcastAsync(ByteBuffer buffer, CancellationToken cancellationToken)
        {
            var localPlayerId = this.game.localPlayer.id;
            var isHost = this.game.localPlayer.IsHost();

            // 先发自己
            var bytes = buffer.ToArray();
            this.onData?.Invoke(localPlayerId, bytes);

            if (isHost)
            {
                // host发给非host的普通玩家
                buffer.Prepend(ChannelFlag.Direct);
                buffer.Prepend(localPlayerId);
                foreach (var player in this.game.GetAllPlayers())
                {
                    if (player.id != localPlayerId)
                    {
                        var copy = buffer.Copy();
                        await player.connection.SendAsync(copy, cancellationToken);
                    }
                }
            }
            else
            {
                buffer.Prepend(ChannelFlag.Broadcast);
                await this.game.localPlayer.connection.SendAsync(buffer, cancellationToken);
            }

            buffer.Clear();
        }

        internal void RegisterPlayer(NetPlayer player, NetConnection netConnection)
        {
            Action<ByteBuffer> handler;
            var playerId = player.id;

            if (this.game.localPlayer.IsHost())
            {
                handler = buffer =>
                {
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

        internal void UnregisterPlayer(byte playerId)
        {
        }

        private void OnData(ByteBuffer buffer)
        {
            var sender = buffer.ConsumeByte();
            this.OnData(sender, buffer);
            buffer.Clear();
        }

        private void OnData(byte sender, ByteBuffer buffer)
        {
            int localPlayerId = this.game.localPlayer.id;
            var isHost = this.game.localPlayer.IsHost();

            var channel = buffer.ConsumeByte();

            switch (channel)
            {
                case ChannelFlag.Direct:
                    UnityEngine.Debug.Log($"[R][Direct {ChannelFlag.Direct}]" + buffer.ToArray().ToStringEx());
                    this.onData?.Invoke(sender, buffer.ToArray());
                    buffer.Clear();
                    break;
                case ChannelFlag.Broadcast:
                    UnityEngine.Debug.Log($"[R][Broadcast {ChannelFlag.Broadcast}]" + buffer.ToArray().ToStringEx());

                    if (!isHost)
                    {
                        throw new ELException();
                    }

                    this.onData?.Invoke(sender, buffer.ToArray());

                    // host发给非host的普通玩家
                    buffer.Prepend(ChannelFlag.Direct);
                    buffer.Prepend(sender);
                    foreach (var player in this.game.GetAllPlayers())
                    {
                        if (player.id != localPlayerId && player.id != sender)
                        {
                            var copy = buffer.Copy();
                            player.connection.SendAsync(copy).Await();
                        }
                    }

                    break;
                case ChannelFlag.Forward:
                    UnityEngine.Debug.Log($"[R][Forward {ChannelFlag.Forward}]" + buffer.ToArray().ToStringEx());

                    var target = buffer.ConsumeByte();
                    if (localPlayerId == target)
                    {
                        var realSender = buffer.ConsumeByte();
                        this.onData?.Invoke(realSender, buffer.ToArray());
                        buffer.Clear();
                    }
                    else
                    {
                        if (!isHost)
                        {
                            throw new ELException();
                        }

                        this.SendAsync(target, buffer, CancellationToken.None).Await();
                    }

                    break;
                default:
                    throw new ELException();
            }
        }
    }
}
