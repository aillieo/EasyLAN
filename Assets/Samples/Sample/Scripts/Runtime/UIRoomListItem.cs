// -----------------------------------------------------------------------
// <copyright file="UIRoomListItem.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils.EasyLAN.Sample
{
    using System.Threading;
    using UnityEngine;
    using UnityEngine.UI;

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
