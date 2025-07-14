using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LayoutGroup))]
public class LayoutRefresher : MonoBehaviour
{
    private LayoutGroup layoutGroup;
    private Coroutine layoutRefreshRoutine;

    private void Awake()
    {
        layoutGroup = GetComponent<LayoutGroup>();
    }

    private void OnEnable()
    {
        ResfreshTheLayout();
    }

    public void ResfreshTheLayout()
    {
        KillExistingRefreshRoutine();

        if(this.gameObject.activeInHierarchy)
        layoutRefreshRoutine = StartCoroutine(RefreshTheLayoutRoutine());
    }

    private IEnumerator RefreshTheLayoutRoutine()
    {
        layoutGroup.enabled = false;
        yield return null;
        layoutGroup.enabled = true;
    }

    private void KillExistingRefreshRoutine()
    {
        if (layoutRefreshRoutine != null)
        {
            StopCoroutine(layoutRefreshRoutine);
            layoutRefreshRoutine = null;
        }
    }
}