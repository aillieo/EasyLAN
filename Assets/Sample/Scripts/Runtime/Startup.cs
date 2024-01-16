using UnityEngine;

namespace AillieoUtils.EasyLAN.Sample
{
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
            GameManager.Instance.uiRoot = uiRoot;
            GameManager.Instance.mainCam = mainCam;
            GameManager.Instance.resourcesManager = resourcesManager;
            GameManager.Instance.LoadStartupView();

            Destroy(this.gameObject);
        }
    }
}
