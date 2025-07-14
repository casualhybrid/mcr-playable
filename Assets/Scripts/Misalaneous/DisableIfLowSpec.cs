using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableIfLowSpec : MonoBehaviour
{
    private void OnEnable()
    {
        if(EstimateDevicePerformance.isOreoDevice || EstimateDevicePerformance.isRamLowerThanThreeGB || EstimateDevicePerformance.IsPhoneBlackListed)
        {
            this.gameObject.SetActive(false);
        }
    }
}
