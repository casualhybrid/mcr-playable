using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "TutorialSegmentData", menuName = "ScriptableObjects/Tutorial/TutorialSegmentData")]
public class TutorialSegmentData : ScriptableObject
{
    [SerializeField] private List<TutorialSegment> tutorialSegments;

    public int SegmentsCount => tutorialSegments.Count;

    public int curTutorialSegmentIndex { get; set; }

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += ResetDefaultValues;
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= ResetDefaultValues;
    }

    private void ResetDefaultValues(Scene arg0, Scene arg1)
    {
        curTutorialSegmentIndex = 0;
    }

    public void AddCheckPointToTutorialSegment(int segmentIndex, Vector3 checkPointPosition)
    {
        var segment = tutorialSegments[segmentIndex];
        segment.checkPoint = checkPointPosition;

        tutorialSegments[segmentIndex] = segment;
    }

    public void AddRewindableHurdlesToTutorialSegment(int segmentIndex, /*List<GameObject> rewindableHurdles*/ TutorialHurdles tutorialHurdles)
    {
        var segment = tutorialSegments[segmentIndex];
      //  segment.TutorialHurdles.hurdles.Clear();
        segment.TutorialHurdles = tutorialHurdles;

        tutorialSegments[segmentIndex] = segment;
    }

    public TutorialSegment GetTutorialSegmentByIndex(int index)
    {
        return tutorialSegments[index];
    }

    public TutorialSegment GetCurrentTutorialSegment()
    {
        return tutorialSegments[curTutorialSegmentIndex];
    }
}