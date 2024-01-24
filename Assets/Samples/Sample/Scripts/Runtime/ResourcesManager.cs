// -----------------------------------------------------------------------
// <copyright file="ResourcesManager.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils.EasyLAN.Sample
{
    using System;
    using System.Linq;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public class ResourcesManager : MonoBehaviour
    {
        [Serializable]
        public class ResourceEntry
        {
            public string key;
            public Object value;
        }

        [SerializeField]
        private ResourceEntry[] resources;

        public T Get<T>(string key) where T : Object
        {
            if (this.resources == null)
            {
                return null;
            }

            ResourceEntry entry = this.resources.FirstOrDefault(e => e.key == key);
            if (entry == null)
            {
                return null;
            }

            return entry.value as T;
        }
    }
}
