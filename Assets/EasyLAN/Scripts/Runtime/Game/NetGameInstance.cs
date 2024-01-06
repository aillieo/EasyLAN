using System;
using System.Collections.Generic;

namespace AillieoUtils.EasyLAN
{
    public class NetGameInstance : IDisposable
    {
        public event Action<NetPlayer> onNewPlayer;
        public event Action<NetPlayer> onPlayerLeave;
        public event Action<NetGameState> onGameStateChanged;

        public NetGameState state { get; internal set; } = NetGameState.Uninitiated;
        public NetGameInfo info { get; private set; }

        private readonly IdGenerator idGenerator = new IdGenerator();
        private readonly Dictionary<int, NetPlayer> allPlayers = new Dictionary<int, NetPlayer>();

        public static NetGameInstance Create(string name, NetPlayer host)
        {
            if ((host.flag & NetPlayerFlag.Local) == 0)
            {
                throw new InvalidOperationException();
            }

            if ((host.flag & NetPlayerFlag.Host) != 0)
            {
                throw new InvalidOperationException();
            }

            if ((host.flag & NetPlayerFlag.Remote) != 0)
            {
                throw new InvalidOperationException();
            }

            var game = new NetGameInstance();
            game.info = new NetGameInfo() { gameName = name };

            host.flag |= NetPlayerFlag.Host;
            host.id = game.idGenerator.Get();

            return game;
        }

        public void Dispose()
        {

        }

        internal void AddPlayer(NetPlayer player, NetPlayerFlag flag)
        {
            player.flag |= flag;
            player.id = this.idGenerator.Get();
            this.allPlayers.Add(player.id, player);
        }

        internal void RemovePlayer(NetPlayer player)
        {

        }

        public IEnumerable<NetPlayer> GetAllPlayers()
        {
            foreach(var pair in this.allPlayers)
            {
                yield return pair.Value;
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
    }
}
