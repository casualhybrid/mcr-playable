using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector;
using deVoid.UIFramework;
using System.Reflection;

public class BannerSwitcherFromTopToBottom : MonoBehaviour
{
    public UISettings uiSettings;

#if UNITY_EDITOR

    [Button("SwitchBannerPos")]
    public void SwitchBannerPosition()
    {
        FieldInfo screensToRegField = typeof(UISettings).GetField("screensToRegister", BindingFlags.NonPublic |
                         BindingFlags.Instance);
        List<GameObject> screensToReg =  screensToRegField.GetValue(uiSettings) as List<GameObject>;

        UnityEngine.Console.Log("Length " + screensToReg.Count);

        foreach (var screen in screensToReg)
        {
            UnityEngine.Console.Log("DOING " + screen.name);

            string assetPath = AssetDatabase.GetAssetPath(screen.gameObject);
            GameObject loadedScreen = PrefabUtility.LoadPrefabContents(assetPath);

            PanelTransformModifierBasedOnBannerAds panelTModifier = loadedScreen.GetComponentInChildren<PanelTransformModifierBasedOnBannerAds>(true);

            if (panelTModifier == null)
            {
                PrefabUtility.UnloadPrefabContents(loadedScreen);
                continue;
            }
            

           RectTransform mainPanel = typeof(PanelTransformModifierBasedOnBannerAds).GetField("mainPanel", BindingFlags.NonPublic |
                         BindingFlags.Instance).GetValue(panelTModifier) as RectTransform;

            if(mainPanel == null)
            {
                foreach(Transform child in panelTModifier.transform)
                {
                    if (child.name == "TouchBlocker")
                        continue;

                    mainPanel = child.GetComponent<RectTransform>();
                    break;
                }
            }


            GameObject touchBlockerGameObject = typeof(PanelTransformModifierBasedOnBannerAds).GetField("adContainerGameObject", BindingFlags.NonPublic |
                         BindingFlags.Instance).GetValue(panelTModifier) as GameObject;

            if(touchBlockerGameObject == null)
            {
                PrefabUtility.UnloadPrefabContents(loadedScreen);
                continue;
            }

            RectTransform touchBlocker = touchBlockerGameObject.GetComponent<RectTransform>();

            if (mainPanel != null)
            {
              //  UnityEngine.Console.Log("Anchored Before " + mainPanel.anchoredPosition);
                mainPanel.SetTop(0);
                mainPanel.SetBottom(0);
                mainPanel.anchoredPosition = Vector2.zero;

                mainPanel.SetBottom(170.031f);
            }

            // touch blocker
            touchBlocker.anchorMin = new Vector2(0, 0);
            touchBlocker.anchorMax = new Vector2(1, 0);
            touchBlocker.pivot = new Vector2(0.5f, 0);
            touchBlocker.anchoredPosition = Vector2.zero;

            if(mainPanel != null)
            UnityEngine.Console.Log("Done " + mainPanel?.root.name);

            //PrefabUtility.RecordPrefabInstancePropertyModifications(loadedScreen);
            // PrefabUtility.ApplyPrefabInstance(loadedScreen, InteractionMode.AutomatedAction);

            PrefabUtility.SaveAsPrefabAsset(loadedScreen, assetPath);
            PrefabUtility.UnloadPrefabContents(loadedScreen);
        }
    
    }
#endif
}
