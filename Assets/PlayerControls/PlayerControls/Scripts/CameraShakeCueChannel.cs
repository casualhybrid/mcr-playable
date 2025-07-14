using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "CameraShakeVariations", menuName = "ScriptableObjects/Camera/CameraShakeChannel")]
public class CameraShakeCueChannel : ScriptableObject
{
    public UnityAction<CameraShakeConfig> OnShakeTheCamera { get; set; }

    public void RaiseEvent(CameraShakeConfig cameraShakeConfig)
    {
        OnShakeTheCamera(cameraShakeConfig);
    }
}