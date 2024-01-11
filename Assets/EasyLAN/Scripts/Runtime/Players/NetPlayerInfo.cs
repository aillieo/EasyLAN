using System;
using UnityEngine;

namespace AillieoUtils.EasyLAN
{
    [Serializable]
    public struct NetPlayerInfo : IProtocol
    {
        [field: SerializeField]
        public string playerName { get; set; }
    }
}
