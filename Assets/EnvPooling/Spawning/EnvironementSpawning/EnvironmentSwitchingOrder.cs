using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "EnvironmentSwitchingOrder", menuName = "ScriptableObjects/Environement/EnvironmentSwitchingOrder")]
public class EnvironmentSwitchingOrder : ScriptableObject
{
    [SerializeField] private int noOfUniqueEnvironments;

    [SerializeField] private float[] distanceCoveredByEnvBeforeSwitch;
    [SerializeField] private Environment[] enviornmentToLoopThrough;

    public float[] DistanceCoveredByEnvBeforeSwitch => distanceCoveredByEnvBeforeSwitch;
    public Environment[] EnvironmentToLoopThrough => enviornmentToLoopThrough;

    public int GetTotalUniqueEnvironments => noOfUniqueEnvironments;

    private void OnValidate()
    {
        noOfUniqueEnvironments = enviornmentToLoopThrough.Distinct().Count();
    }
}
