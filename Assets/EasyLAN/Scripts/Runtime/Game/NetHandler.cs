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
        internal NetGameInstance game;

        private readonly Dictionary<byte, Action<ByteBuffer>> handlers = new Dictionary<byte, Action<ByteBuffer>>();

        public event Action<byte, IProtocol> onData;

        public async Task SendAsync(byte target, ByteBuffer buffer, CancellationToken cancellationToken)
        {
            var localPlayerId = this.game.localPlayer.id;
            var isHost = this.game.localPlayer.IsHost();

            if (target == localPlayerId)
            {
                this.OnData(localPlayerId, buffer.Copy());
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
                buffer.Prepend(localPlayerId);
                buffer.Prepend(target);
                buffer.Prepend(ChannelFlag.Forward);
                await this.game.localPlayer.connection.SendAsync(buffer, cancellationToken);
            }
        }

        public async Task BroadcastAsync(ByteBuffer buffer, CancellationToken cancellationToken)
        {
            var localPlayerId = this.game.localPlayer.id;
            var isHost = this.game.localPlayer.IsHost();

            // 先发自己
            this.OnData(localPlayerId, buffer.Copy());

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

                buffer.Clear();
            }
            else
            {
                buffer.Prepend(ChannelFlag.Broadcast);
                await this.game.localPlayer.connection.SendAsync(buffer, cancellationToken);
            }
        }

        internal void RegisterPlayer(NetPlayer player, NetConnection netConnection)
        {
            Action<ByteBuffer> handler;
            var playerId = player.id;

            if (this.game.localPlayer.IsHost())
            {
                handler = buffer =>
                {
                    this.OnRawData(playerId, buffer);
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
            this.OnRawData(sender, buffer);
            buffer.Clear();
        }

        private void OnRawData(byte sender, ByteBuffer buffer)
        {
            int localPlayerId = this.game.localPlayer.id;
            var isHost = this.game.localPlayer.IsHost();

            var channel = buffer.ConsumeByte();

            switch (channel)
            {
                case ChannelFlag.Direct:
                    UnityEngine.Debug.Log($"[R][Direct {ChannelFlag.Direct}]" + buffer.ToArray().ToStringEx());
                    this.OnData(sender, buffer);
                    break;
                case ChannelFlag.Broadcast:
                    UnityEngine.Debug.Log($"[R][Broadcast {ChannelFlag.Broadcast}]" + buffer.ToArray().ToStringEx());

                    if (!isHost)
                    {
                        throw new ELException();
                    }

                    this.OnData(sender, buffer.Copy());

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

                    buffer.Clear();

                    break;
                case ChannelFlag.Forward:
                    UnityEngine.Debug.Log($"[R][Forward {ChannelFlag.Forward}]" + buffer.ToArray().ToStringEx());

                    var target = buffer.ConsumeByte();

                    if (localPlayerId == target)
                    {
                        var realSender = buffer.ConsumeByte();
                        this.OnData(realSender, buffer);
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

        private void OnData(byte sender, ByteBuffer buffer)
        {
            var proto = buffer.ConsumeByte();
            var tp = Protocols.GetType(proto);
            var obj = Activator.CreateInstance(tp) as IProtocol;
            obj.Deserialize(buffer);
            this.onData?.Invoke(sender, obj);
        }
    }
}
