using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace AillieoUtils.EasyLAN.Sample
{
    public class UIRoomListItem : MonoBehaviour
    {
        [SerializeField] private Text text;

        private CancellationTokenSource cancellationTokenSource;

        public void SetGameInfo(NetGameInfo info)
        {
            this.text.text = info.gameName;
        }

        private void JoinGame()
        {

        }

        private void OnDestroy()
        {

        }
    }
}
