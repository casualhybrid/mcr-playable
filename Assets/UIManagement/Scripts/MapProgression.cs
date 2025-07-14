using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TheKnights.SaveFileSystem;
using UnityEngine;
using UnityEngine.UI;

public class MapProgression : MonoBehaviour
{
    [SerializeField] private bool useSaveFileForCoveredEnvironments = false;
    [SerializeField] private Image fillImage;
    [SerializeField] private Image[] patches;
    [SerializeField] private Sprite currentArea, notCurrentArea;
    [SerializeField] private GamePlaySessionData gamePlaySessionData;
    [SerializeField] private MapProgressionSO mapProgressData;
    [SerializeField] private EnvironmentSwitchingOrder environmentSwitchingOrder;
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private RectTransform fillBarRT;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private float xDistBetweenMap;
    [SerializeField] private float speedXFill = .25f;
    // [SerializeField] private float speedXMultiplierScroll = 1f;

    private readonly HashSet<int> passedUIEnvironmnets = new HashSet<int>();

    private int completedEnvironments => Mathf.Clamp(!useSaveFileForCoveredEnvironments ?
        MapProgressionSO.uniqueEnvironmentsCompletedThisSession : saveManager.MainSaveFile.UniqueEnvironmentsCompleted, 0, environmentSwitchingOrder.GetTotalUniqueEnvironments);

    //    private void OnValidate()
    //    {
    //        if (Application.isPlaying)
    //            return;

    //        if (patches.Length < 2)
    //            return;

    //        xDistBetweenMaps = patches[1].rectTransform.parent.GetComponent<RectTransform>().anchoredPosition.x -
    //              patches[0].rectTransform.parent.GetComponent<RectTransform>().anchoredPosition.x;

    //        UnityEngine.Console.Log("Distance changed to " + xDistBetweenMaps)
    //;    }

    private void OnEnable()
    {
        ResetAnimData();

        int completedEnv = completedEnvironments;

        if (completedEnv == environmentSwitchingOrder.GetTotalUniqueEnvironments)
            completedEnv = completedEnvironments - 1;
     
        float fillForEnvCompleted = ((float)(completedEnv) * xDistBetweenMap) / fillBarRT.rect.width;

        float progressInCurEnvNormalized = mapProgressData.distanceCoveredByPlayerInThisEnv / environmentSwitchingOrder.DistanceCoveredByEnvBeforeSwitch[completedEnv];
        progressInCurEnvNormalized = Mathf.Clamp(progressInCurEnvNormalized, 0, 0.9f);
        float curEnvProgressFill = (progressInCurEnvNormalized * xDistBetweenMap) / fillBarRT.rect.width;

        scrollRect.horizontalNormalizedPosition = 0;
        fillImage.fillAmount = 0;

        float totalFill = fillForEnvCompleted + curEnvProgressFill;
        
        if(1 - totalFill <= 0.007f)
        {
            totalFill = 1f;
        }

        StartCoroutine(ProgressBarAnim(totalFill));

        //UnityEngine.Console.Log($"Fill Real {fillForEnvCompleted} and partial {curEnvProgressFill}");
    }

    private void OnDisable()
    {
        passedUIEnvironmnets.Clear();
    }

    private void ResetAnimData()
    {
        for (int i = 0; i < patches.Length; i++)
        {
            patches[i].sprite = notCurrentArea;
        }
    }

    private IEnumerator ProgressBarAnim(float toFill)
    {
        // UnityEngine.Console.Log($"Request for a fill of {toFill}");

        yield return new WaitForSecondsRealtime(.4f);
        yield return null;

        int completedEnv = completedEnvironments;

        float currentFill = fillImage.fillAmount;

        Vector2 scrollStartPos = scrollRect.content.anchoredPosition;

        Vector2 targetScrollPos = completedEnv > 0 ? scrollRect.transform.InverseTransformPoint(scrollRect.content.position)
            - scrollRect.transform.InverseTransformPoint(patches[completedEnv - 1].transform.position) : scrollStartPos;

        targetScrollPos.y = scrollRect.content.anchoredPosition.y;

        scrollRect.enabled = false;

        while (currentFill < toFill)
        {
            currentFill += Time.unscaledDeltaTime * speedXFill;

            currentFill = Mathf.Clamp(currentFill, 0, toFill);

            fillImage.fillAmount = currentFill;
            scrollRect.content.anchoredPosition = Vector2.Lerp(scrollStartPos, targetScrollPos, (currentFill / toFill)  /*speedXMultiplierScroll*/);
            yield return null;

            int envInThisFillRate = Mathf.FloorToInt(currentFill / (xDistBetweenMap / fillBarRT.rect.width));

            //  UnityEngine.Console.Log(currentFill / (xDistBetweenMap / fillBarRT.rect.width));

            for (int i = 0; i <= envInThisFillRate; i++)
            {
                if (passedUIEnvironmnets.Contains(i))
                    continue;

                passedUIEnvironmnets.Add(i);
                patches[i].sprite = currentArea;
                DOTweenAnimation anim = patches[i].GetComponentInParent<DOTweenAnimation>();
                anim?.DORestart();
            }
        }

        //  scrollRect.content.anchoredPosition = targetScrollPos;
        scrollRect.enabled = true;
    }

    //private float GetFillIrrespectiveOfRectTransforms(float fill)
    //{
    //    float totalEnv = patches.Length;
    //    float completedEnv = mapProgressData.environmentsCompletedThisSession;
    //    float factor = 1f / totalEnv;

    //    //  float _fill = (Mathf.Pow(completedEnv, 2) * xDistBetweenMap * factor) / (fillBarRT.rect.width * fill);
    //    float _fill = ((completedEnv * xDistBetweenMap) + (completedEnv * fillBarRT.rect.width * factor) - (fillBarRT.rect.width * fill)) / fillBarRT.rect.width;

    //    UnityEngine.Console.Log($"Found {_fill} and other {fill}");

    //    return _fill;
    //}
}