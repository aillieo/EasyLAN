using System;
using System.Threading;
using System.Threading.Tasks;

namespace AillieoUtils.EasyLAN
{
    public class NetPlayer : IDisposable
    {
        public int id { get; internal set; }
        public NetPlayerState state { get; internal set; }
        public NetPlayerFlag flag { get; internal set; }

        public NetPlayerInfo info { get; internal set; }

        internal NetConnection connection { get; private set; }

        public NetGameInstance game { get; internal set; }

        internal static NetPlayer CreateLocal(NetPlayerInfo info)
        {
            NetPlayer netPlayer = new NetPlayer();
            netPlayer.info = info;
            netPlayer.flag |= NetPlayerFlag.Local;

            return netPlayer;
        }

        internal async static Task<NetPlayer> AcceptRemote(NetConnection connection, NetGameInstance game, CancellationToken cancellationToken)
        {
            NetPlayer netPlayer = new NetPlayer();
            netPlayer.flag |= NetPlayerFlag.Remote;
            netPlayer.state = NetPlayerState.Initialized;

            var info = await connection.ReceiveProto<NetPlayerInfo>(cancellationToken);
            netPlayer.info = info;

            var playerId = game.idGenerator.Get();
            netPlayer.id = playerId;

            var InstanceId = new InstanceId() { value = playerId };
            await connection.SendProto(InstanceId, cancellationToken);

            netPlayer.state = NetPlayerState.Authenticated;
            netPlayer.connection = connection;

            return netPlayer;
        }

        internal async static Task<NetPlayer> JoinRemote(NetConnection connection, NetPlayerInfo info, CancellationToken cancellationToken)
        {
            NetPlayer netPlayer = new NetPlayer();
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

        public void Dispose()
        {
            if(this.connection != null)
            {
                this.connection.Dispose();
                this.connection = null;
            }
        }

        public bool IsConnected()
        {
            return this.connection != null && this.connection.IsConnected();
        }
    }
}
