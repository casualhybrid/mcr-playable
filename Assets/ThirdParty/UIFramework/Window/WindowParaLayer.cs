using UnityEngine;
using System.Collections.Generic;

namespace deVoid.UIFramework {
    /// <summary>
    /// This is a "helper" layer so Windows with higher priority can be displayed.
    /// By default, it contains any window tagged as a Popup. It is controlled by the WindowUILayer.
    /// </summary>
    public class WindowParaLayer : MonoBehaviour {
        public static WindowParaLayer instance;

        private void Awake()
        {
            instance = this;
        }
        [SerializeField] 
        private GameObject darkenBgObject = null;


        public List<GameObject> containedScreens = new List<GameObject>();
        
        public void AddScreen(Transform screenRectTransform) {
            screenRectTransform.SetParent(transform, false);
            containedScreens.Add(screenRectTransform.gameObject);
            Debug.LogError($"Screen added: {screenRectTransform.gameObject.name}");
        }

        public void RefreshDarken() {
            for (int i = 0; i < containedScreens.Count; i++) {
                if (containedScreens[i] != null) {
                    if (containedScreens[i].activeSelf) {
                        darkenBgObject.SetActive(true);
                        return;
                    }
                }
            }

            darkenBgObject.SetActive(false);
        }

        public GameObject Get()
        {
            if (containedScreens != null && containedScreens.Count > 0)
                return containedScreens[0];
            else
                return null;
        }
        public void DarkenBG() {
            darkenBgObject.SetActive(true);
            darkenBgObject.transform.SetAsLastSibling();
        }
    }
}
