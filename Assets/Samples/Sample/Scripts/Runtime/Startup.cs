// -----------------------------------------------------------------------
// <copyright file="Startup.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils.EasyLAN.Sample
{
    using UnityEngine;

    public class Startup : MonoBehaviour
    {
        [SerializeField]
        private RectTransform uiRoot;
        [SerializeField]
        private Camera mainCam;
        [SerializeField]
        private ResourcesManager resourcesManager;

        private void Start()
        {
            GameManager.Instance.uiRoot = this.uiRoot;
            GameManager.Instance.mainCam = this.mainCam;
            GameManager.Instance.resourcesManager = this.resourcesManager;
            GameManager.Instance.LoadStartupView();

            Destroy(this.gameObject);
        }
    }
}
