using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace AillieoUtils.EasyLAN
{
    public static class NetGameMaker
    {
        public static async Task Broadcast(string hostName, int udpPort, string ip, int tcpPort, CancellationToken cancellationToken)
        {
            NetGameInfo netGameInfo = new NetGameInfo() { gameName = hostName, ip = ip, port = tcpPort };
            byte[] bytes = SerializeUtils.Serialize(netGameInfo);
            await LANUtils.Listen(udpPort, NetGameInfo.ValidateHead, bytes, cancellationToken);
        }

        public static async Task<NetGameInfo[]> Search(int port, CancellationToken cancellationToken)
        {
            var bytes = NetGameInfo.headBytes;
            return await LANUtils.Search<NetGameInfo>(port, bytes, NetGameInfo.Deserialize, cancellationToken);
        }

        public static async Task AcceptPlayerAsync(NetGameInstance gameInstance, int port, CancellationToken cancellationToken)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();

            while (!cancellationToken.IsCancellationRequested)
            {
                var connection = await NetConnection.AcceptAsync(listener, cancellationToken);
                var player = NetPlayer.CreateLocal(new NetPlayerInfo());
                if(await NetPlayer.CreateRemote(player, connection))
                {
                    gameInstance.AddPlayer(player, NetPlayerFlag.Remote);
                }
            }

            listener.Stop();
        }

        public static async Task<bool> JoinRemote(NetPlayer player, NetGameInfo gameInfo, CancellationToken cancellationToken)
        {
            NetConnection connection = await NetConnection.ConnectAsync(gameInfo.ip, gameInfo.port, cancellationToken);
            await NetPlayer.CreateRemote(player, connection);
            return true;
        }
    }
}
