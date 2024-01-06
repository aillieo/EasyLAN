namespace AillieoUtils.EasyLAN
{
    using System;
    using System.Text;
    using System.Threading.Tasks;

    public class NetPlayer : IDisposable
    {
        public int id { get; internal set; }
        public NetPlayerState state { get; internal set; }
        public NetPlayerFlag flag { get; internal set; }

        public NetPlayerInfo info { get; internal set; }

        internal NetConnection connection { get; private set; }

        public event Action<byte[]> onData;
        public event Action onDisconnected;

        public static NetPlayer CreateLocal(NetPlayerInfo info)
        {
            NetPlayer netPlayer = new NetPlayer();
            netPlayer.info = info;
            netPlayer.flag |= NetPlayerFlag.Local;
            return netPlayer;
        }

        internal async static Task<bool> CreateRemote(NetPlayer player, NetConnection connection)
        {
            player.flag |= NetPlayerFlag.Remote;

            player.connection = connection;
            connection.onData += player.onData;
            connection.onDisconnected += player.onDisconnected;
            return true;
        }

        private NetPlayer()
        {
        }

        public async Task SendAsync(string message)
        {
            if (connection != null && connection.IsConnected())
            {
                await connection.SendAsync(message);
            }
            else
            {
                UnityEngine.Debug.LogError("Failed to send...");
            }
        }

        public async Task SendAsync(IInternalObject message)
        {
            byte[] bytes = SerializeUtils.Serialize(message);
            string data = Encoding.UTF8.GetString(bytes);
            await this.SendAsync(data);
        }

        public void Dispose()
        {
            if(this.connection != null)
            {
                this.connection.Dispose();
                this.connection = null;
            }
        }
    }
}
