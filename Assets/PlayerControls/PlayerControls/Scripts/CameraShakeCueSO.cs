using UnityEngine;

[CreateAssetMenu(fileName = "CameraShakeCue", menuName = "ScriptableObjects/Camera/CameraShakeCue")]
public class CameraShakeCueSO : ScriptableObject
{
    [SerializeField] private CameraShakeConfig cameraShakeConfig;

    public CameraShakeConfig GetCameraShakeConfig => cameraShakeConfig;


}