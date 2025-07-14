using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentDataUpdater : MonoBehaviour
{
    #region Variables
    [SerializeField] private EnvironmentData environmentData;
    #endregion

    #region Unity Callbacks
    private void Update()
    {
        environmentData.UpdatedistanceBetweenPlayerAndCurrentRampBuilding();
    }
    #endregion
}
