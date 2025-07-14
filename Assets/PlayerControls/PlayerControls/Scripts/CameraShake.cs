using Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class CameraShake : MonoBehaviour
{
    private CinemachineVirtualCamera cam;
    private CinemachineBasicMultiChannelPerlin channelPerlin;
    private static bool isCameraShaking;

    [SerializeField] private GameManager gameManager;
    [SerializeField] private PlayerSharedData playerSharedData;
    [SerializeField] private NoiseSettings carIdleNoiseSetting;

    [SerializeField] private CameraShakeVariations cameraShakeVariations;

    private void Awake()
    {
        cam = GetComponent<CinemachineVirtualCamera>();
     
        channelPerlin = cam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();


        cam.m_Transitions.m_OnCameraLive.AddListener(HandleCameraGoLive);
        cameraShakeVariations.OnShakeTheCamera.AddListener(ShakeCamera);
        cameraShakeVariations.OnShakeEnded.AddListener(ShakeHasEnded);
    }

    private void OnDestroy()
    {
        cameraShakeVariations.OnShakeEnded.RemoveListener(ShakeHasEnded);
        cam.m_Transitions.m_OnCameraLive.RemoveListener(HandleCameraGoLive);
        cameraShakeVariations.OnShakeTheCamera.RemoveListener(ShakeCamera);
    }

    private void HandleCameraGoLive(ICinemachineCamera can, ICinemachineCamera fromCam)
    {
        CameraShakeConfig cameraShakeConfig = cameraShakeVariations.CurrentCameraShakeConfig;

        if (!cameraShakeConfig.isValid)
        {
            ResetShake();
        }
        else
        {
            ShakeCamera(cameraShakeConfig);
        }
    }

    private void ShakeCamera(CameraShakeConfig cameraShakeConfig)
    {
        //   UnityEngine.Console.Log("Shaking Camera");
        
        isCameraShaking = true;
        channelPerlin.m_AmplitudeGain = cameraShakeConfig.GetAmplitude;
        channelPerlin.m_FrequencyGain = cameraShakeConfig.GetFrequenct;
        channelPerlin.m_NoiseProfile = cameraShakeConfig.GetNoiseSettings;
    }

    private void Update()
    {
        if (!isCameraShaking && playerSharedData.IsIdle)
        {
            channelPerlin.m_NoiseProfile = carIdleNoiseSetting;
            
            channelPerlin.m_AmplitudeGain = MyExtensions.RangeMapping(SpeedHandler.GameTimeScale, gameManager.GetMaximumSpeed, gameManager.GetMinimumSpeed, 0.04f, 0.08f);
            channelPerlin.m_FrequencyGain = MyExtensions.RangeMapping(SpeedHandler.GameTimeScale, gameManager.GetMaximumSpeed, gameManager.GetMinimumSpeed, 1.5f, 3f);
        }
    }

    private void ResetShake()
    {
        channelPerlin.m_FrequencyGain = 0;
        channelPerlin.m_AmplitudeGain = 0;
    }

    private void ShakeHasEnded()
    {
        isCameraShaking = false;
        ResetShake();
    }
}