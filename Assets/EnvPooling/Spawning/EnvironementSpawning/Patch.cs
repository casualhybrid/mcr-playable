using Sirenix.OdinInspector;
using System;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
using UnityEngine;

public class Patch : MonoBehaviour, IFloatingReset
{
    public bool ShoudNotOffsetOnRest { get; set; }

    public event Action<Patch> PatchHasFinished;

    [SerializeField] private BoxCollider boxCollider;
    [SerializeField] private float distanceBeforeItCanReAppear = -1;
    [ReadOnly] [SerializeField] private int instanceID = 0;
    [SerializeField] private EnvCategory envCategory;

    public int InstanceID => instanceID;
    public BoxCollider BoxCollider => boxCollider;

    public EnvCategory EnvCategory
    { get { return envCategory; } set { envCategory = value; } }

    public float EndingZPoint => boxCollider.bounds.max.z;

    public float DiffBWPivotAndColliderMinBoundPoint;
    public float DiffBWPivotAndColliderMaxBoundPoint;


    //private void Awake()
    //{
    //    CoroutineRunner.Instance.StartCoroutine(WaitForFixedUpdateAndExecute(() =>
    //    {
    //        DiffBWPivotAndColliderMinBoundPoint = boxCollider.bounds.min.z - transform.position.z;
    //        DiffBWPivotAndColliderMaxBoundPoint = boxCollider.bounds.max.z - transform.position.z;
    //    }));
    //}

    //private IEnumerator WaitForFixedUpdateAndExecute(Action action)
    //{
    //    yield return new WaitForFixedUpdate();
    //    action();
    //}

    [Button("Calculate BOunds")]
    public void CalculateBounds()
    {
        DiffBWPivotAndColliderMinBoundPoint = boxCollider.bounds.min.z - transform.position.z;
        DiffBWPivotAndColliderMaxBoundPoint = boxCollider.bounds.max.z - transform.position.z;
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        instanceID = GetInstanceID();

        //if (Application.isPlaying)
        //    return;

        //if (!this.gameObject.activeInHierarchy)
        //    return;

        //if (!PrefabStageUtility.GetCurrentPrefabStage())
        //    return;

        //UnityEngine.Console.Log("Validate");

        //DiffBWPivotAndColliderMinBoundPoint = boxCollider.bounds.min.z - transform.position.z;
        //DiffBWPivotAndColliderMaxBoundPoint = boxCollider.bounds.max.z - transform.position.z;
    }

#endif

    public float DistanceBeforeItCanReAppear => distanceBeforeItCanReAppear;

    public bool isLimitedSpawningByDistance => distanceBeforeItCanReAppear != -1f;

    public float GetLengthOfPatch => boxCollider.size.z;

    public void SendPatchFinishedEvent()
    {
        StartCoroutine(SendPatchFinishedEventAfterDuration());
    }

    public void DestroyThePatch()
    {
        StartCoroutine(DestroyThePatchRoutine());
    }

    private IEnumerator DestroyThePatchRoutine()
    {
        yield return new WaitForSeconds(1);
        Destroy(this.gameObject);
    }

    private IEnumerator SendPatchFinishedEventAfterDuration()
    {
        yield return new WaitForSeconds(1);
        if (PatchHasFinished != null)
            PatchHasFinished(this);
    }

    public void OnBeforeFloatingPointReset()
    {
    }

    public void OnFloatingPointReset(float movedOffset)
    {
    }
}