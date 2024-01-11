using System.Threading;
using System.Threading.Tasks;

namespace AillieoUtils.EasyLAN
{
    public static class NetGameMaker
    {
        public static int broadcastPort = 23333;

        public static async Task<NetGameInfo[]> Search(int port, CancellationToken cancellationToken)
        {
            var bytes = NetGameInfo.headBytes;
            return await LANUtils.Search<NetGameInfo>(port, bytes, NetGameInfo.Deserialize, cancellationToken);
        }

        public static NetGameInstance Create(NetGameInfo gameInfo, NetPlayerInfo playerInfo, CancellationToken cancellationToken)
        {
            var game = new NetGameInstance();

            NetPlayer host = NetPlayer.CreateLocal(playerInfo);
            host.flag |= NetPlayerFlag.Host;
            host.id = game.idGenerator.Get();

            game.localPlayer = host;

            game.StartAcceptPlayer(ref gameInfo, cancellationToken);
            game.info = gameInfo;

            byte[] bytes = SerializeUtils.Serialize(gameInfo);
            LANUtils.Listen(broadcastPort, NetGameInfo.ValidateHead, bytes, cancellationToken).Await();

            return game;
        }

        public static async Task<NetGameInstance> Join(NetPlayerInfo info, NetGameInfo gameInfo, CancellationToken cancellationToken)
        {
            var game = new NetGameInstance();
            game.info = gameInfo;

            NetConnection connection = await NetConnection.ConnectAsync(gameInfo.ip, gameInfo.port, cancellationToken);
            var player = await NetPlayer.JoinRemote(connection, info, cancellationToken);

            game.localPlayer = player;

            // 对方是主机 我加入了对方
            game.netHandler.RegisterPlayer(player, connection);

            return game;
        }
    }
}
