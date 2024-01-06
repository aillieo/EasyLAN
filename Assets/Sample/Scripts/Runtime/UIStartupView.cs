using AillieoUtils.EasyLAN;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace AillieoUtils.EasyLAN.Sample
{
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
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
            }

            cancellationTokenSource = new CancellationTokenSource();
            string host = inputField.text;
            if (string.IsNullOrWhiteSpace(host))
            {
                host = "host";
            }

            await NetGameMaker.Broadcast(host, 23333, "127.0.0.1", 23334, cancellationTokenSource.Token);
        }

        public async void SearchGames()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
            }

            cancellationTokenSource = new CancellationTokenSource();
            NetGameInfo[] games = await NetGameMaker.Search(23333, cancellationTokenSource.Token);

            Debug.Log($"Search: found {games.Length}");
            foreach (var g in games)
            {
                Debug.Log(g);
            }

            UpdateRoomList(games);
        }

        private void UpdateRoomList(NetGameInfo[] games)
        {
            int gameCount = games.Length;
            while (gameCount > listRoot.childCount)
            {
                GameObject newItem = Instantiate(uiListItem, listRoot);
            }

            for (int i = 0; i < listRoot.childCount; ++i)
            {
                GameObject item = listRoot.GetChild(i).gameObject;
                bool active = i < gameCount;
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
