using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnvironmentsEnumSO", menuName = "ScriptableObjects/EnumSO/EnvironmentsEnumSO")]
public class EnvironmentsEnumSO : ScriptableObject
{
    public ScriptableObject CityEnvironment;
    public ScriptableObject ParkEnvironment;
    public ScriptableObject BeachEnvironment;
    public ScriptableObject CoralEnvironment;
    public ScriptableObject MetropolisEnvironment;
    public ScriptableObject GeneralSwitchEnvironment;
    public ScriptableObject IndustrialEnvironment;
}
