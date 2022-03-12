using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AillieoUtils.EasyLAN.Sample
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance
        {
            get
            {
                if (ins == null)
                {
                    ins = new GameObject("[GameManager]").AddComponent<GameManager>();
                }

                return ins;
            }
        }

        private static GameManager ins;

        public Camera mainCam { get; set; }

        public RectTransform uiRoot { get; set; }

        public ResourcesManager resourcesManager { get; set; }

        public NetPlayer netPlayer { get; set; }

        private void Awake()
        {
            if (ins != null && ins != this)
            {
                Destroy(this.gameObject);
            }
        }

        public void LoadStartupView()
        {
            GameObject viewAsset = resourcesManager.Get<GameObject>("UIStartupView");
            Instantiate(viewAsset, uiRoot);
        }
    }

}
