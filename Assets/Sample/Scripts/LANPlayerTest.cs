using AillieoUtils.EasyLAN;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class LANPlayerTest : MonoBehaviour
{
    [SerializeField]
    private GameObject uiLobby;
    [SerializeField]
    private GameObject uiInGame;
    [SerializeField]
    private GameObject uiListItem;
    [SerializeField]
    private Transform listRoot;

    private CancellationTokenSource cancellationTokenSource;

    private void OnEnable()
    {

    }

    private void OnDisable()
    {

    }

    public async void CreateGame()
    {
        uiInGame.SetActive(true);
        uiLobby.SetActive(false);

        if (cancellationTokenSource != null)
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
        }

        cancellationTokenSource = new CancellationTokenSource();
        await NetWork.Listen("host", 23333, cancellationTokenSource.Token);
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
        NetGameInfo[] games = await NetWork.Search(23333, cancellationTokenSource.Token);

        Debug.Log($"ËÑË÷µ½ÓÎÏ· {games.Length}");
        foreach (var g in games)
        {
            Debug.Log(g);
        }

        int gameCount = games.Length;        
        while (gameCount > listRoot.childCount)
        {
            GameObject newItem = Instantiate(uiListItem, listRoot);
        }

        for (int i = 0; i < listRoot.childCount; ++ i)
        {
            GameObject item = listRoot.GetChild(i).gameObject;
            bool active = i < gameCount;
            item.SetActive(active);
            if (active)
            {
                item.transform.Find("Text").gameObject.GetComponent<Text>().text = games[i].gameName;
            }
        }
    }

    public void JoinGame()
    {

    }

    public void LeaveGame()
    {

    }

    public void CancelGame()
    {

    }

    private void OnDestroy()
    {
        
    }
}
