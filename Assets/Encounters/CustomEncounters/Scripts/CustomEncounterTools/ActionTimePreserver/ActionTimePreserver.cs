using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

#if (UNITY_EDITOR)

using UnityEditor.SceneManagement;

#endif

[ExecuteInEditMode]
public class ActionTimePreserver : MonoBehaviour, IValidatable, ICustomEncounterEntity
{
    public string tagToSubjectify = "Default";
    public float preservationMultiplier = 1f;
    public bool reversePreservation = false;


    [SerializeField] private bool isPartOfCustomEncounter = true;

    [ReadOnly] [SerializeField] private List<ActionTimePreserverSubject> preservedSubjects = new List<ActionTimePreserverSubject>();

    public bool IsPartOfCustomEncounter => isPartOfCustomEncounter;

    public List<ActionTimePreserverSubject> PreservedSubjects
    {
        get
        {
            return preservedSubjects;
        }
    }

    public bool isActionTimePreservationComplete { get; set; } = false;
    private int previousUpdatePreservedSubjectCount;

    [ReadOnly] [SerializeField] private CustomEncounter _customEncounter;

    public CustomEncounter CustomEncounter
    {
        get
        {
            if (_customEncounter == null)
            {
                _customEncounter = GetComponentInParent<CustomEncounter>();

#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
                PrefabUtility.RecordPrefabInstancePropertyModifications(this);
#endif
            }

            return _customEncounter;
        }

        set
        {
            _customEncounter = value;
        }
    }

    private float nonOverridenGameTimeScale // AKA current game speed
    {
        get
        {
            return speedHandler.isOverriden ? SpeedHandler.GameTimeScaleBeforeOverriden : SpeedHandler.GameTimeScale;
        }
    }

    private float normalizedGameSpeedValue
    {
        get
        {
            return (speedHandler.GetForwardSpeedBasedOnSpecificGameTimeScale(nonOverridenGameTimeScale) - speedHandler.GetForwardSpeedBasedOnSpecificGameTimeScale(gameManager.GetMinimumSpeed)) /
            (speedHandler.GetForwardSpeedBasedOnSpecificGameTimeScale(gameManager.GetMaximumSpeed) - speedHandler.GetForwardSpeedBasedOnSpecificGameTimeScale(gameManager.GetMinimumSpeed));
        }
    }

    public float calculatedObstacleSpacingMultiplier
    {
        get
        {
            return Mathf.Lerp(speedHandler.GetForwardSpeedBasedOnSpecificGameTimeScale(gameManager.GetMinimumSpeed) / speedHandler.GetForwardSpeedBasedOnSpecificGameTimeScale(gameManager.GetMinimumSpeed),
            speedHandler.GetForwardSpeedBasedOnSpecificGameTimeScale(gameManager.GetMaximumSpeed) / speedHandler.GetForwardSpeedBasedOnSpecificGameTimeScale(gameManager.GetMinimumSpeed), normalizedGameSpeedValue);
        }
    }

    [Header("References")]
    [SerializeField] private SpeedHandler speedHandler;

    [SerializeField] private GameManager gameManager;

    private void Awake()
    {
        if (_customEncounter == null)
        {
            _customEncounter = GetComponentInParent<CustomEncounter>();
        }

        if (_customEncounter == null)
            return;

        if (!Application.isPlaying)
            return;

        if (!CustomEncounter.IsSkeleton)
            return;

        CustomEncounter.CustomEncounterSkeleton.OnObstaclesSpawnedFromMetaData += CustomEncounterSkeleton_OnObstaclesSpawnedFromMetaData;

    }

    private void OnDisable()
    {
        isActionTimePreservationComplete = false;
    }

    private void CustomEncounterSkeleton_OnObstaclesSpawnedFromMetaData()
    {
        foreach (ActionTimePreserverSubject subject in preservedSubjects)
        {
            if (subject.firstObjectInGroupTransform != null && subject.firstObjectInGroupTransform.name.Contains("Temp_"))
            {
                Destroy(subject.firstObjectInGroupTransform.gameObject);
            }


            if (subject.lastObjectInGroupTransform != null && subject.lastObjectInGroupTransform.name.Contains("Temp_"))
            {
                Destroy(subject.lastObjectInGroupTransform.gameObject);
            }

            foreach (var obstacleEntity in subject.GetComponentsInChildren<Obstacle>())
            {
                var groupMetaData = CustomEncounter.CustomEncounterSkeleton.actionTimeSubjectsChildObjectDestructions[subject].actionTimeSubjectFirstLastGroupMetaData;

                if (obstacleEntity.transform.parent == groupMetaData.firstObjectGroupMeta.ParentT && obstacleEntity.transform.GetSiblingIndex() == groupMetaData.firstObjectGroupMeta.ChildOrder)
                {
                    subject.firstObjectInGroupTransform = obstacleEntity.transform;
                }
                if (obstacleEntity.transform.parent == groupMetaData.lastObjectGroupMeta.ParentT && obstacleEntity.transform.GetSiblingIndex() == groupMetaData.lastObjectGroupMeta.ChildOrder)
                {
                    subject.lastObjectInGroupTransform = obstacleEntity.transform;
                }
            }
        }
    }

    public void InitializeActionTimePreserver()
    {
        if (!Application.isPlaying)
            return;

        if (isActionTimePreservationComplete)
            return;

#if (UNITY_EDITOR)
        if (PrefabStageUtility.GetCurrentPrefabStage() != null &&
        PrefabStageUtility.GetCurrentPrefabStage() == PrefabStageUtility.GetPrefabStage(transform.root.gameObject))
            return;
#endif
        previousUpdatePreservedSubjectCount = 0;
        foreach (ActionTimePreserverSubject preservedSubject in preservedSubjects)
            preservedSubject.relativeObjectTransformForDistanceCalc = null;
        ResetVirtualDeltaPositonsOfSubjects();

        List<ActionTimePreserverSubject> currentActionTimePreserverSubjects = GetComponentsInChildren<ActionTimePreserverSubject>().ToList();
        for (int i = currentActionTimePreserverSubjects.Count - 1; i >= 0; i--)
        {
            if (!currentActionTimePreserverSubjects[i].selfTags.Contains(tagToSubjectify))
            {
                currentActionTimePreserverSubjects.RemoveAt(i);
            }
        }
        int subjectCount = currentActionTimePreserverSubjects.Count;
        if (subjectCount == 0)
        {
            UnityEngine.Console.LogError($"Prefab: {gameObject.transform.root.gameObject.name}. This encounter does not meet the requirements of 'ActionTimePreserver' script attached to it");
        }

        UnchainSubjects();
        ChainPreservedSubjectsTogether();   

        for (int i = 0; i < preservedSubjects.Count; i++)
        {
            if (preservedSubjects[i].relativeObjectTransformForDistanceCalc == null)
                continue;

            preservedSubjects[i].Move(new Vector3(0, 0, preservedSubjects[i].GetDeltaDistanceToPreserveActionTime(preservationMultiplier, calculatedObstacleSpacingMultiplier)));
        }

        isActionTimePreservationComplete = true;
    }

    public void ChainPreservedSubjectsTogether()
    {
        foreach (ActionTimePreserverSubject preservedSubject in preservedSubjects)
        {
            ActionTimePreserverSubject preservedSubjectOfRelativeObject = null;
            Transform relativeObjectTransformForDistanceCalc = null;

            if (!reversePreservation)
            {
                foreach (ActionTimePreserverSubject preservedSubjectBeingComparedTo in preservedSubjects)
                {
                   // UnityEngine.Console.Log($"FIrst {preservedSubjectBeingComparedTo.firstObjectInGroupTransform.position.z} other {preservedSubject.lastObjectInGroupTransform.position.z}");
                    if (preservedSubjectBeingComparedTo.firstObjectInGroupTransform.position.z < preservedSubject.lastObjectInGroupTransform.position.z && // Is 'object being compared to' behind this 'object'
                        (relativeObjectTransformForDistanceCalc == null || preservedSubjectBeingComparedTo.firstObjectInGroupTransform.position.z > relativeObjectTransformForDistanceCalc.position.z)) // Is 'object being compared to' ahread of the 'cached relative object'
                    {
                        preservedSubjectOfRelativeObject = preservedSubjectBeingComparedTo;
                        relativeObjectTransformForDistanceCalc = preservedSubjectBeingComparedTo.firstObjectInGroupTransform;
                    }
                }
            }
            else
            {
                foreach (ActionTimePreserverSubject preservedSubjectBeingComparedTo in preservedSubjects)
                {
                    if (preservedSubjectBeingComparedTo.firstObjectInGroupTransform.position.z > preservedSubject.lastObjectInGroupTransform.position.z && // Is 'object being compared to' ahead of this 'object'
                        (relativeObjectTransformForDistanceCalc == null || preservedSubjectBeingComparedTo.firstObjectInGroupTransform.position.z < relativeObjectTransformForDistanceCalc.position.z)) // Is 'object being compared to' behind the 'cached relative object'
                    {
                        preservedSubjectOfRelativeObject = preservedSubjectBeingComparedTo;
                        relativeObjectTransformForDistanceCalc = preservedSubjectBeingComparedTo.firstObjectInGroupTransform;
                    }
                }
            }

            if (preservedSubjectOfRelativeObject != null)
            {
                preservedSubjectOfRelativeObject.OnMove += preservedSubject.Move;
            }

            preservedSubject.relativeObjectTransformForDistanceCalc = relativeObjectTransformForDistanceCalc;
        }
    }

    public void UnchainSubjects()
    {
        foreach (ActionTimePreserverSubject subject in preservedSubjects)
        {
            subject.OnMove = null;
        }
    }

    public void ResetVirtualDeltaPositonsOfSubjects()
    {
        foreach (ActionTimePreserverSubject subject in preservedSubjects)
        {
            subject.ResetVirtualDeltaPosition();
        }
    }

    public void ValidateAndInitialize()
    {
        AssignSubjectsToPreserve();
    }

    private void AssignSubjectsToPreserve()
    {
        List<ActionTimePreserverSubject> currentActionTimePreserverSubjects = GetComponentsInChildren<ActionTimePreserverSubject>().ToList();
        for (int i = currentActionTimePreserverSubjects.Count - 1; i >= 0; i--)
        {
            if (!currentActionTimePreserverSubjects[i].selfTags.Contains(tagToSubjectify))
            {
                currentActionTimePreserverSubjects.RemoveAt(i);
            }
        }
        int subjectCount = currentActionTimePreserverSubjects.Count;

        if (subjectCount == 0)
        {
            UnityEngine.Console.LogError($"Prefab: {gameObject.transform.root.gameObject.name}. Subjects to preserve count is zero. Action time preserver will not work.");
        }

        if (previousUpdatePreservedSubjectCount != subjectCount || !AreAllChildrenAlreadyPreservedSubjectsList())
        {
            previousUpdatePreservedSubjectCount = subjectCount;

            preservedSubjects.Clear();

            preservedSubjects = GetComponentsInChildren<ActionTimePreserverSubject>().ToList();
            for (int i = preservedSubjects.Count - 1; i >= 0; i--)
            {
                if (!preservedSubjects[i].selfTags.Contains(tagToSubjectify))
                {
                    preservedSubjects.RemoveAt(i);
                }
            }

            foreach (ActionTimePreserverSubject subject in preservedSubjects)
            {
                ObjectDestruction[] objectDestuctionComponentsInSubject = subject.GetComponentsInChildren<ObjectDestruction>();
                Transform firstObjectInGroup = null;
                Transform lastObjectInGroup = null;

                foreach (ObjectDestruction objectDestructionComponent in objectDestuctionComponentsInSubject)
                {
                    if (lastObjectInGroup == null ||
                    lastObjectInGroup.transform.position.z > objectDestructionComponent.transform.parent.position.z ||
                    lastObjectInGroup.transform.position.z == objectDestructionComponent.transform.parent.position.z && objectDestructionComponent.transform.parent.position.x == 0)
                    {
                        lastObjectInGroup = objectDestructionComponent.transform.parent;
                    }

                    if (firstObjectInGroup == null ||
                    firstObjectInGroup.transform.position.z < objectDestructionComponent.transform.parent.position.z ||
                    firstObjectInGroup.transform.position.z == objectDestructionComponent.transform.parent.position.z && objectDestructionComponent.transform.parent.position.x == 0)
                    {
                        firstObjectInGroup = objectDestructionComponent.transform.parent;
                    }
                }

                if (firstObjectInGroup == null)
                {
                    firstObjectInGroup = subject.transform;
                }

                if (lastObjectInGroup == null)
                {
                    lastObjectInGroup = subject.transform;
                }

                subject.firstObjectInGroupTransform = firstObjectInGroup;
                subject.lastObjectInGroupTransform = lastObjectInGroup;
            }
        }
    }

    private bool AreAllChildrenAlreadyPreservedSubjectsList()
    {
        List<ActionTimePreserverSubject> currentActionTimePreserverSubjects = GetComponentsInChildren<ActionTimePreserverSubject>().ToList();
        for (int i = currentActionTimePreserverSubjects.Count - 1; i >= 0; i--)
        {
            if (!currentActionTimePreserverSubjects[i].selfTags.Contains(tagToSubjectify))
            {
                currentActionTimePreserverSubjects.RemoveAt(i);
            }
        }

        for (int i = 0; i < currentActionTimePreserverSubjects.Count; i++)
        {
            ObjectDestruction[] objectDestuctionComponentsInSubject = currentActionTimePreserverSubjects[i].GetComponentsInChildren<ObjectDestruction>();
            Transform firstObjectInGroup = null;
            Transform lastObjectInGroup = null;

            foreach (ObjectDestruction objectDestructionComponent in objectDestuctionComponentsInSubject)
            {
                if (lastObjectInGroup == null ||
                lastObjectInGroup.transform.position.z > objectDestructionComponent.transform.parent.position.z ||
                lastObjectInGroup.transform.position.z == objectDestructionComponent.transform.parent.position.z && objectDestructionComponent.transform.parent.position.x == 0)
                {
                    lastObjectInGroup = objectDestructionComponent.transform.parent;
                }

                if (firstObjectInGroup == null ||
                firstObjectInGroup.transform.position.z < objectDestructionComponent.transform.parent.position.z ||
                firstObjectInGroup.transform.position.z == objectDestructionComponent.transform.parent.position.z && objectDestructionComponent.transform.parent.position.x == 0)
                {
                    firstObjectInGroup = objectDestructionComponent.transform.parent;
                }
            }

            if (firstObjectInGroup == null)
            {
                firstObjectInGroup = currentActionTimePreserverSubjects[i].transform;
            }

            if (lastObjectInGroup == null)
            {
                lastObjectInGroup = currentActionTimePreserverSubjects[i].transform;
            }

            if (preservedSubjects[i].firstObjectInGroupTransform != firstObjectInGroup ||
            preservedSubjects[i].lastObjectInGroupTransform != lastObjectInGroup)
            {
                return false;
            }
        }

        return true;
    }

#if UNITY_EDITOR

    private void Update()
    {
        if (Application.isPlaying)
            return;

        if (CustomEncounter == null)
            return;
        
        if (CustomEncounter.IsSkeleton)
            return;

        ValidateAndInitialize();
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
            return;

        if (isPartOfCustomEncounter && (_customEncounter != null && _customEncounter.IsSkeleton))
            return;

        ValidateAndInitialize();
    }

#endif
}