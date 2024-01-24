// -----------------------------------------------------------------------
// <copyright file="UIStartupView.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils.EasyLAN.Sample
{
    using System.Threading;
    using UnityEngine;
    using UnityEngine.UI;

    public class UIStartupView : MonoBehaviour
    {
        [SerializeField] private GameObject uiListItem;
        [SerializeField] private Transform listRoot;
        [SerializeField] private InputField inputField;

        private CancellationTokenSource cancellationTokenSource;

        private void OnEnable()
        {

        }

        private void OnDisable()
        {

        }

        public async void CreateGame()
        {
            if (this.cancellationTokenSource != null)
            {
                this.cancellationTokenSource.Cancel();
                this.cancellationTokenSource.Dispose();
                this.cancellationTokenSource = null;
            }

            this.cancellationTokenSource = new CancellationTokenSource();
            var host = this.inputField.text;
            if (string.IsNullOrWhiteSpace(host))
            {
                host = "host";
            }

            //await NetGameInstance.Broadcast(host, 23333, "127.0.0.1", 23334, cancellationTokenSource.Token);
        }

        public async void SearchGames()
        {
            if (this.cancellationTokenSource != null)
            {
                this.cancellationTokenSource.Cancel();
                this.cancellationTokenSource.Dispose();
                this.cancellationTokenSource = null;
            }

            this.cancellationTokenSource = new CancellationTokenSource();
            NetGameInfo[] games = await NetGameMaker.Search(23333, this.cancellationTokenSource.Token);

            Debug.Log($"Search: found {games.Length}");
            foreach (var g in games)
            {
                Debug.Log(g);
            }

            this.UpdateRoomList(games);
        }

        private void UpdateRoomList(NetGameInfo[] games)
        {
            var gameCount = games.Length;
            while (gameCount > this.listRoot.childCount)
            {
                GameObject newItem = Instantiate(this.uiListItem, this.listRoot);
            }

            for (var i = 0; i < this.listRoot.childCount; ++i)
            {
                GameObject item = this.listRoot.GetChild(i).gameObject;
                var active = i < gameCount;
                item.SetActive(active);
                if (active)
                {
                    UIRoomListItem itemScript = item.GetComponent<UIRoomListItem>();
                    itemScript.SetGameInfo(games[i]);
                }
            }
        }

        private void OnDestroy()
        {

        }
    }
}
