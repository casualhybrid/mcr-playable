using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;
using UnityEditor;
using FrostyMaxSaveManager;
using TheKnights.SaveFileSystem;
using System.Reflection;


namespace deVoid.UIFramework
{
    /// <summary>
    /// Template for an UI. You can rig the prefab for the UI Frame itself and all the screens that should
    /// be instanced and registered upon instantiating a new UI Frame.
    /// </summary>
    
    [CreateAssetMenu(fileName = "UISettings", menuName = "deVoid UI/UI Settings")]
    public class UISettings : ScriptableObject
    {

        [Tooltip("Prefab for the UI Frame structure itself")]
        [SerializeField] private UIFrame templateUIPrefab = null;
        [Tooltip("Prefabs for all the screens (both Panels and Windows) that are to be instanced and registered when the UI is instantiated")]
        [SerializeField] private List<GameObject> screensToRegister = null;
        [Tooltip("In case a screen prefab is not deactivated, should the system automatically deactivate its GameObject upon instantiation? If false, the screen will be at a visible state upon instantiation.")]
        [SerializeField] private bool deactivateScreenGOs = true;

        private UIScreenEvents uIScreenEvents;
        private UIFrame uiFrame;

        /// <summary>
        /// Creates an instance of the UI Frame Prefab. By default, also instantiates
        /// all the screens listed and registers them. If the deactivateScreenGOs flag is
        /// true, it will deactivate all Screen GameObjects in case they're active.
        /// </summary>
        /// <param name="instanceAndRegisterScreens">Should the screens listed in the Settings file be instanced and registered?</param>
        /// <returns>A new UI Frame</returns>
        public UIFrame CreateUIInstance(Scene scene , UIScreenEvents uIScreenEvents, bool instanceAndRegisterScreens = true) {
            var newUI = Instantiate(templateUIPrefab);
            uiFrame = newUI;

            GameObject newUIGameObject = newUI.gameObject;
            SceneManager.MoveGameObjectToScene(newUIGameObject, scene);
            this.uIScreenEvents = uIScreenEvents;

            if (instanceAndRegisterScreens) {
                foreach (var screen in screensToRegister) {
                    var screenInstance = Instantiate(screen);
                    var screenController = screenInstance.GetComponent<IUIScreenController>();
                    screenController.UIScreenEvents = uIScreenEvents;
                   

                    if (screenController != null) {
                        newUI.RegisterScreen(screen.name, screenController, screenInstance.transform);
                        if (deactivateScreenGOs && screenInstance.activeSelf) {
                            screenInstance.SetActive(false);
                        }
                    }
                    else {
                        UnityEngine.Console.LogError("[UIConfig] Screen doesn't contain a ScreenController! Skipping " + screen.name);
                    }
                }
            }

            return newUI;
        }
        
        public void RegisterThisScreenToTheLayer(string screenID)
        {
            for (int i = 0; i < screensToRegister.Count; i++)
            {
                var screen = screensToRegister[i];
              
                string id = screen.name;

                if (id != screenID)
                {
                  //  UnityEngine.Console.Log($"This isn't the screen {id} and {screenID}");
                    continue;
                }
             

              //  UnityEngine.Console.Log($"Registerting THE SCREEN {screenID} YAY");

                var screenInstance = Instantiate(screen);
                var screenController = screenInstance.GetComponent<IUIScreenController>();
                screenController.UIScreenEvents = uIScreenEvents;

                if (screenController != null)
                {
                    uiFrame.RegisterScreen(screen.name, screenController, screenInstance.transform);
                    if (deactivateScreenGOs && screenInstance.activeSelf)
                    {
                        screenInstance.SetActive(false);
                    }
                }
                else
                {
                    UnityEngine.Console.LogError("[UIConfig] Screen doesn't contain a ScreenController! Skipping " + screen.name);
                }

            }


          
        }

        public bool DoesThisScreenExists(string id)
        {
            foreach (var screen in screensToRegister)
            {
                if (screen.name == id)
                    return true;
            }

            return false;
        }

        //[Button("RemoveAdsPurchasedRef")]
        //public void SetSaveManagerReferences()
        //{
        //    foreach (var screen in screensToRegister)
        //    {
        //        string path = AssetDatabase.GetAssetPath(screen);
        //        GameObject root = PrefabUtility.LoadPrefabContents(path);

        //        PanelTransformModifierBasedOnBannerAds modifier = root.GetComponentInChildren<PanelTransformModifierBasedOnBannerAds>(true);

        //        if (modifier == null)
        //        {
        //            PrefabUtility.UnloadPrefabContents(root);
        //            continue;
        //        }


        //        GameEvent manager = AssetDatabase.LoadAssetAtPath<GameEvent>("Assets/ScriptableObjects/Events/Purchasing/PlayerBoughtAdFree.asset");

        //        FieldInfo field = modifier.GetType().GetField("playerBoughtADFree", BindingFlags.Instance | BindingFlags.NonPublic);
        //        field.SetValue(modifier, manager);

        //        Debug.Log(manager.name);

        //        PrefabUtility.SaveAsPrefabAsset(root, path);
        //        PrefabUtility.UnloadPrefabContents(root);

        //    }
        //}


        [Button("DeActivateAll")]
        public void DeActivateAllScreens()
        {
            foreach (var item in screensToRegister)
            {
                item.SetActive(false);
            }
        }


        private void OnValidate() {
            List<GameObject> objectsToRemove = new List<GameObject>();
            for(int i = 0; i < screensToRegister.Count; i++) {
                var screenCtl = screensToRegister[i].GetComponent<IUIScreenController>();
                if (screenCtl == null) {
                    objectsToRemove.Add(screensToRegister[i]);
                }
            }

            if (objectsToRemove.Count > 0) {
                UnityEngine.Console.LogError("[UISettings] Some GameObjects that were added to the Screen Prefab List didn't have ScreenControllers attached to them! Removing.");
                foreach (var obj in objectsToRemove) {
                    UnityEngine.Console.LogError("[UISettings] Removed " + obj.name + " from " + name + " as it has no Screen Controller attached!");
                    screensToRegister.Remove(obj);
                }
            }

            DeActivateAllScreens();
        }        
    }
}
