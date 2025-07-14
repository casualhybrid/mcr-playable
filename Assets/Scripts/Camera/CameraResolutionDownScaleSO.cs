using UnityEngine;

[CreateAssetMenu(fileName = "CameraResolutionDownScale", menuName = "ScriptableObjects/Camera/CameraResolutionDownScale")]
public class CameraResolutionDownScaleSO : ScriptableObject
{
    private float renderScaleValue = 1;
    public float RenderScaleValue => renderScaleValue;
    public bool DisablePostProcessing { get; set; }

    private void ResetScriptableObject()
    {
        renderScaleValue = 1;
        DisablePostProcessing = false;
    }

    public void CalculateRenderScaleValue()
    {
        ResetScriptableObject();

        DeviceGroup deviceGroup = EstimateDevicePerformance.GetDeviceGroup();
        // int resPixels = Screen.currentResolution.height * Screen.currentResolution.width;

        if (deviceGroup == DeviceGroup.LowEnd || deviceGroup == DeviceGroup.MidEnd || EstimateDevicePerformance.isOpenGLTwoDevice
            || EstimateDevicePerformance.isOreoDevice || EstimateDevicePerformance.isRamLowerThanThreeGB || EstimateDevicePerformance.IsPhoneBlackListed)
        {
            UnityEngine.Console.Log("Disabling Post Processing Effects as the device is low or medium end");
            DisablePostProcessing = true;
        }


        //if (deviceGroup == DeviceGroup.LowEnd)
        //{
        //    // If it's greater than 720p
        //    if (resPixels > 921600)
        //    {
        //        renderScaleValue = 921600f / (float)resPixels;
        //    }
        //}

        //// Mid end with 1080p + screen
        //else if (deviceGroup == DeviceGroup.MidEnd && resPixels >= 2073600)
        //{
        //    // Bring it down to 1080p first
        //    renderScaleValue = 2073600f / (float)resPixels;

        //    //20% further reduction. Clamp isn't necessary just for safety
        //    renderScaleValue = Mathf.Clamp(renderScaleValue - 0.2f, 0.6f, 1f);
        //}

        //// Mid end with less than 1080p screen but greater than 720p
        //else if (deviceGroup == DeviceGroup.MidEnd && resPixels < 2073600 && resPixels > 921600)
        //{
        //    // Resduce by 20%
        //    renderScaleValue = Mathf.Clamp(renderScaleValue - 0.2f, 0.6f, 1f);

        //    // If its less than 720p after multiplying with render scale
        //    if (resPixels * renderScaleValue < 921600)
        //    {
        //        renderScaleValue = 921600f / (float)resPixels;
        //    }
        //}

        //// High End With greater 1080p + will not go beyond 1080p
        //else if (deviceGroup == DeviceGroup.HighEnd && resPixels >= 2073600)
        //{
        //    renderScaleValue = 2073600f / (float)resPixels;
        //}

        //renderScaleValue = renderScaleValue > 1 ? 1 : renderScaleValue;

        //// Safety Check
        //if ((renderScaleValue * resPixels) < 921600)
        //{
        //    UnityEngine.Console.LogWarning("Incorrent renderscale value. Resetting to 1");
        //    renderScaleValue = 1;
        //}

        //    UnityEngine.Console.Log("RenderScale Value is " + renderScaleValue);
    }

   

    public void SetRenderScaleValue(float value)
    {
        renderScaleValue = value;
    }
}