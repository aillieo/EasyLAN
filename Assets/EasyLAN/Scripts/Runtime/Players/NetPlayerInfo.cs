namespace AillieoUtils.EasyLAN
{
    using System;
    using UnityEngine;

    [Serializable]
    public struct NetPlayerInfo : IInternalObject
    {
        [field:SerializeField]
        public string playerName { get; set; }
    }
}
