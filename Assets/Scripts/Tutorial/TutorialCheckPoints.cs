using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class TutorialCheckPoints : MonoBehaviour
{
    [SerializeField] private TutorialSegmentData tutorialSegmentData;
    [SerializeField] private List<Transform> transformCheckPoints;


    public void UpdateTutorialSegmentCheckPointData()
    {
        int i = 0;
        foreach (Transform t in transformCheckPoints)
        {
            Vector3 pos = t.position;
            tutorialSegmentData.AddCheckPointToTutorialSegment(i++, pos);
        }
    }
}