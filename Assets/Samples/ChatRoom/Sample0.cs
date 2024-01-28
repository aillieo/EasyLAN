// -----------------------------------------------------------------------
// <copyright file="Sample0.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils.EasyLAN.Sample
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using UnityEngine;

    public class Sample0 : MonoBehaviour
    {
        private readonly ChatModel model = new ChatModel();

        private enum GameState
        {
            NoGame = 1,
            Creating = 2,
            Searching = 3,
            Ready = 4,
            Playing = 5,
        }

        private const int udpPort = 23333;

        private GameState state = GameState.NoGame;

        private CancellationTokenSource cancellationTokenSource;

        private string gameName;
        private string playerName;
        private List<NetGameInfo> foundGames = new List<NetGameInfo>();
        private NetGameInstance netGame0;
        private NetPlayer netPlayer0;

        private string message;

        private void OnEnable()
        {
            this.gameName = nameof(Sample0);
            this.playerName = SampleUtils0.GetRandomName();
        }

        private void OnDisable()
        {
            this.AbortLastRequest();
            this.ExitGame();
        }

        private void OnGUI()
        {
            switch (this.state)
            {
                case GameState.NoGame:
                    this.OnGUI_NoGame();
                    break;
                case GameState.Creating:
                    this.OnGUI_Creating();
                    break;
                case GameState.Searching:
                    this.OnGUI_Searching();
                    break;
                case GameState.Ready:
                    this.OnGUI_Ready();
                    break;
                case GameState.Playing:
                    this.OnGUI_Playing();
                    break;
            }
        }

        private void OnGUI_NoGame()
        {
            this.gameName = GUILayout.TextField(this.gameName);
            this.playerName = GUILayout.TextField(this.playerName);

            if (GUILayout.Button("Create"))
            {
                this.CreateGame();
            }

            if (GUILayout.Button("Search"))
            {
                this.SearchGames();
            }
        }

        private void OnGUI_Creating()
        {
            if (GUILayout.Button("Exit"))
            {
                this.ExitGame();
            }
        }

        private void OnGUI_Searching()
        {
            if (GUILayout.Button("Exit"))
            {
                this.ExitGame();
            }

            if (GUILayout.Button("Search"))
            {
                this.SearchGames();
            }

            foreach (var n in this.foundGames)
            {
                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Join"))
                {
                    this.JoinGame(n);
                }

                GUILayout.Label(n.playerName);
                GUILayout.Label(n.gameName);

                GUILayout.EndHorizontal();
            }
        }

        private void OnGUI_Ready()
        {
            foreach (var p in this.netGame0.GetAllPlayers())
            {
                GUILayout.Label(SampleUtils0.GetPlayerTitle(p.id, p.info.playerName));
            }

            this.message = GUILayout.TextField(this.message);

            if (GUILayout.Button("Send"))
            {
                var msg = $"[{this.netPlayer0.id}]{this.netPlayer0.info.playerName}: {this.message}";
                this.netGame0.Broadcast(SerializeUtils.Serialize(new Messages.String() { value = msg }), this.cancellationTokenSource.Token);
            }

            if (this.netPlayer0.IsHost())
            {
                if (GUILayout.Button("Start"))
                {
                    this.state = GameState.Playing;
                    var task = this.netGame0.Start();
                    task.ContinueWith(t => { });
                }
            }
        }

        private void OnGUI_Playing()
        {
            this.message = GUILayout.TextField(this.message);

            foreach (var p in model.players)
            {
                GUILayout.BeginHorizontal();

                GUILayout.Label(SampleUtils0.GetPlayerTitle(p.Key, p.Value.netInfo.playerName));

                if (GUILayout.Button("Send"))
                {
                    // 单点发送
                    var msg = $"[{this.netPlayer0.id}]{this.netPlayer0.info.playerName}: {this.message}";
                    UnityEngine.Debug.Log($"[GAME] Send message to {p.Key}: {msg}");
                    this.netGame0.Send(p.Key, SerializeUtils.Serialize(new Messages.String() { value = msg }), this.cancellationTokenSource.Token);
                }

                GUILayout.EndHorizontal();
            }

            if (GUILayout.Button("SendAll"))
            {
                // 全体发送
                var msg = $"[{this.netPlayer0.id}]{this.netPlayer0.info.playerName}: {this.message}";
                UnityEngine.Debug.Log($"[GAME] Broadcast message: {msg}");
                this.netGame0.Broadcast(SerializeUtils.Serialize(new Messages.String() { value = msg }), this.cancellationTokenSource.Token);
            }
        }

        private void CreateGame()
        {
            if (this.state != GameState.NoGame)
            {
                return;
            }

            this.AbortLastRequest(true);

            var game = NetGameMaker.Create(
                new NetGameInfo() { gameName = this.gameName },
                new NetPlayerInfo() { playerName = this.playerName },
                this.cancellationTokenSource.Token);

            this.netGame0 = game;
            this.netPlayer0 = game.localPlayer;
            this.netGame0.onPlayerMessage += this.OnMessage;
            this.netGame0.onGameStateChanged += this.OnGameStateChanged;

            this.state = GameState.Ready;
        }

        private async void SearchGames()
        {
            this.state = GameState.Searching;
            this.AbortLastRequest(true);

            NetGameInfo[] games = await NetGameMaker.Search(udpPort, this.cancellationTokenSource.Token);

            Debug.Log($"Search: found {games.Length}");
            foreach (var g in games)
            {
                Debug.Log(g);
            }

            this.foundGames.Clear();
            this.foundGames.AddRange(games);
        }

        private async void JoinGame(NetGameInfo netGameInfo)
        {
            this.AbortLastRequest(true);

            var game = await NetGameMaker.Join(new NetPlayerInfo() { playerName = this.playerName }, netGameInfo, this.cancellationTokenSource.Token);
            this.netGame0 = game;
            this.netPlayer0 = game.localPlayer;
            this.netGame0.onPlayerMessage += this.OnMessage;
            this.netGame0.onGameStateChanged += this.OnGameStateChanged;

            this.state = GameState.Ready;
        }

        private void AbortLastRequest(bool initForNext = false)
        {
            if (this.cancellationTokenSource != null)
            {
                this.cancellationTokenSource.Cancel();
                this.cancellationTokenSource.Dispose();
                this.cancellationTokenSource = null;
            }

            if (initForNext)
            {
                this.cancellationTokenSource = new CancellationTokenSource();
            }
        }

        private void ExitGame()
        {
            this.state = GameState.NoGame;

            this.AbortLastRequest();

            this.foundGames.Clear();

            if (this.netGame0 != null)
            {
                this.netGame0.Dispose();
                this.netGame0 = null;
            }

            if (this.netPlayer0 != null)
            {
                this.netPlayer0.Dispose();
                this.netPlayer0 = null;
            }
        }

        private void OnMessage(byte playerId, byte[] bytes)
        {
            UnityEngine.Debug.Log($"Receive message from {playerId}: byte[{bytes.Length}]");
            if (SerializeUtils.Deserialize(bytes, out object o))
            {
                switch (o)
                {
                    case Messages.String str:
                        UnityEngine.Debug.Log($"[GAME] Receive message from {playerId}: {str.value}");
                        break;
                    case Messages.ReqPlayers rp:
                        if (this.netPlayer0.IsHost())
                        {
                            var msg = new Messages.PlayerList();
                            var players = this.netGame0.GetAllPlayers().ToList();
                            players.Add(this.netPlayer0);

                            msg.players = players
                                .Select(p => new ChatModel.PlayerInfo()
                                {
                                    playerId = p.id,
                                    netInfo = p.info,
                                })
                                .ToArray();
                            var bytesToSend = SerializeUtils.Serialize(msg);
                            this.netGame0.Send(playerId, bytesToSend);
                        }

                        break;

                    case Messages.PlayerList pl:
                        if (!this.netPlayer0.IsHost())
                        {
                            foreach (var p in pl.players)
                            {
                                this.model.players.Add(p.playerId, p);
                            }
                        }

                        break;
                }
            }
            else
            {
                string str = Encoding.UTF8.GetString(bytes);
                UnityEngine.Debug.LogError(str);
            }
        }

        private void OnGameStateChanged(NetGameState netGameState)
        {
            if (netGameState == NetGameState.GamePlaying)
            {
                this.state = GameState.Playing;
                if (this.netGame0.localPlayer.IsHost())
                {
                    foreach (var p in netGame0.GetAllPlayers())
                    {
                        model.players.Add(p.id, new ChatModel.PlayerInfo() { playerId = p.id, netInfo = p.info });
                    }
                }
                else
                {
                    var msg = new Messages.ReqPlayers();
                    var bytes = SerializeUtils.Serialize(msg);
                    this.netGame0.SendToHost(bytes);
                }
            }
        }
    }
}
