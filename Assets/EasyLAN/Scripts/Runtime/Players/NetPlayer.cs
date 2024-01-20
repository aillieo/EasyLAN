// -----------------------------------------------------------------------
// <copyright file="NetPlayer.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils.EasyLAN
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class NetPlayer : IDisposable
    {
        public byte id { get; internal set; }

        public NetPlayerState state { get; internal set; }

        public NetPlayerFlag flag { get; internal set; }

        public NetPlayerInfo info { get; internal set; }

        internal NetConnection connection { get; private set; }

        public NetGameInstance game { get; internal set; }

        internal static NetPlayer CreateLocal(NetPlayerInfo info)
        {
            var netPlayer = new NetPlayer();
            netPlayer.info = info;
            netPlayer.flag |= NetPlayerFlag.Local;

            return netPlayer;
        }

        internal static async Task<NetPlayer> AcceptRemote(NetConnection connection, NetGameInstance game, CancellationToken cancellationToken)
        {
            var netPlayer = new NetPlayer();
            netPlayer.flag |= NetPlayerFlag.Remote;
            netPlayer.state = NetPlayerState.Initialized;

            var info = await connection.ReceiveProto<NetPlayerInfo>(cancellationToken);
            netPlayer.info = info;

            var playerId = game.idGenerator.Get();
            netPlayer.id = playerId;

            var instanceId = new InstanceId() { value = playerId };
            await connection.SendProto(instanceId, cancellationToken);

            netPlayer.state = NetPlayerState.Authenticated;
            netPlayer.connection = connection;

            return netPlayer;
        }

        internal static async Task<NetPlayer> JoinRemote(NetConnection connection, NetPlayerInfo info, CancellationToken cancellationToken)
        {
            var netPlayer = new NetPlayer();
            netPlayer.flag |= NetPlayerFlag.Remote;
            netPlayer.info = info;

            var instanceId = await connection.RequestProto<NetPlayerInfo, InstanceId>(info, cancellationToken);
            netPlayer.id = instanceId.value;

            netPlayer.state = NetPlayerState.Authenticated;
            netPlayer.connection = connection;

            return netPlayer;
        }

        private NetPlayer()
        {
        }

        public bool IsHost()
        {
            return (this.flag & NetPlayerFlag.Host) > 0;
        }

        public bool IsLocal()
        {
            return (this.flag & NetPlayerFlag.Local) > 0;
        }

        public bool IsConnected()
        {
            return this.connection != null && this.connection.IsConnected();
        }

        public void Dispose()
        {
            if (this.connection != null)
            {
                this.connection.Dispose();
                this.connection = null;
            }
        }
    }
}
