using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AillieoUtils.EasyLAN.Sample
{
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
            if (resources == null)
            {
                return null;
            }

            ResourceEntry entry = resources.FirstOrDefault(e => e.key == key);
            if (entry == null)
            {
                return null;
            }

            return entry.value as T;
        }
    }
}
