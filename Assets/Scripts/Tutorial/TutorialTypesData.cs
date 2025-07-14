using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public enum TutorialHints
{
    PhoneGestures, ArrowGestures, HandGestures, Decals, Text
}

[System.Serializable]
public struct TutorialProperties
{
    [SerializeField] private bool isSlowDownOnHint;
    [SerializeField] private bool autoPassOnFaliure;
    [SerializeField] private bool onlyShowHintAfterExperimentCompleted;
    [SerializeField] private bool dontDisplayHintsIfExperimenting;
    [SerializeField] private bool showAllHintsAtOnce;
    [SerializeField] private List<TutorialHints[]> tutorialHintsInOrder;

    public readonly bool OnlyShowHintAfterExperimentCompleted => onlyShowHintAfterExperimentCompleted;
    public readonly bool DontDisplayHintsIfExperimenting => dontDisplayHintsIfExperimenting;
    public readonly bool IsSlowDownOnHint => isSlowDownOnHint;
    public readonly bool IsAutoPassOnFaliure => autoPassOnFaliure;

    public readonly bool ShowAllHintsAtOnce => showAllHintsAtOnce;
    public readonly List<TutorialHints[]> TutorialHintsInOrder => tutorialHintsInOrder;
}

public class TutorialInstanceData
{
    [SerializeField] private TutorialSegmentData tutorialSegmentData;
    [SerializeField] private EnviornmentSO tutorialEnvironment;
    [SerializeField] private TutorialProperties tutorialProperties;

    public TutorialSegmentData TutorialSegmentData => tutorialSegmentData;
    public EnviornmentSO TutorialEnvironment => tutorialEnvironment;

    public TutorialProperties TutorialProperties => tutorialProperties;
}

[CreateAssetMenu(fileName = "TutorialTypesData", menuName = "ScriptableObjects/Tutorial/TutorialTypesData")]
public class TutorialTypesData : SerializedScriptableObject
{
    [SerializeField] private Dictionary<int, TutorialInstanceData> tutorialTypesData;

    public TutorialInstanceData GetTutorialInstanceData(int type)
    {
        return tutorialTypesData[type];
    }
}