using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if (UNITY_EDITOR)
using UnityEditor.SceneManagement;
#endif

[ExecuteInEditMode]
[RequireComponent(typeof(ThrustConfig))]
public class ThrustFollowupPickupColumnAdjuster : MonoBehaviour, IValidatable
{
    [SerializeField] private Transform targetColumnTransform;

    private void Start()
    {
        if (!Application.isPlaying)
            return;

#if (UNITY_EDITOR)
        if (PrefabStageUtility.GetCurrentPrefabStage() != null && 
        PrefabStageUtility.GetCurrentPrefabStage() == PrefabStageUtility.GetPrefabStage(transform.root.gameObject))
            return;
#endif

        if (targetColumnTransform == null)
        {
            UnityEngine.Console.LogError($"Target column transform is not assigned! ThrustFollowupPickupColumnAdjuster will not work on '{gameObject.name}'.");
            return;
        }

        AdjustFollowupPickupColumn();
    }

    private void AdjustFollowupPickupColumn()
    {
        ThrustConfig selfThrustConfig = GetComponent<ThrustConfig>();

        selfThrustConfig.shouldSpawnFollowUpPickupsInSpecifiedColumn = true;
        selfThrustConfig.followUpPickupSpawnColumn = (int)targetColumnTransform.position.x;
    }

    public void ValidateAndInitialize()
    {
        if (targetColumnTransform == null)
        {
            UnityEngine.Console.LogError($"Target column transform is not assigned! ThrustFollowupPickupColumnAdjuster will not work on '{gameObject.name}'.", gameObject);
        }
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Application.isPlaying)
            return;

        ValidateAndInitialize();
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
            return;

        ValidateAndInitialize();
    }
#endif
}
