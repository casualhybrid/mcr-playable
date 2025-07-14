using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

public class TutorialHurdles : SerializedMonoBehaviour
{
    public readonly List<GameObject> hurdles = new List<GameObject>();
    [SerializeField] private GameObject[] decals;

    [SerializeField] private TutorialSegmentData tutorialSegmentData;
    [SerializeField] private bool overrideHintOrder;

    [ShowIf("overrideHintOrder")][SerializeField] private List<TutorialHints[]> tutorialHintsInOrder;
    public List<TutorialHints[]> TutorialHintsInOrder => tutorialHintsInOrder;

    public GameObject[] Decals => decals;
    public bool isHintsOrderOverriden => overrideHintOrder;

    private InstantiateSubPrefabsInMultipleFrames instantiateSubPrefabsInMultipleFrames;

    private void Awake()
    {
        instantiateSubPrefabsInMultipleFrames = GetComponentInParent<InstantiateSubPrefabsInMultipleFrames>();
        instantiateSubPrefabsInMultipleFrames.OnInstantiationComplete += Initialize;
    }

    private void OnDestroy()
    {
        instantiateSubPrefabsInMultipleFrames.OnInstantiationComplete -= Initialize;
    }

    private void Initialize()
    {
        RewindToInitialTransform[] rewindableObstacles = GetComponentsInChildren<RewindToInitialTransform>();

        for (int i = 0; i < rewindableObstacles.Length; i++)
        {
            hurdles.Add(rewindableObstacles[i].gameObject);
        }

        tutorialSegmentData.AddRewindableHurdlesToTutorialSegment(int.Parse(this.gameObject.name), this);
    }
}