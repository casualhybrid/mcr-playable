using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
    public class CameraEffects : MonoBehaviour
    {
        [SerializeField] private Camera primaryCameraForeground;
        [SerializeField] private Camera secondaryCameraBackground;

        public MotionBlur MotionBlur;
        public BloomOptimized BloomOptimized;

        [SerializeField] private bool isBlur = true;
        [SerializeField] private bool isBloom = true;

        [SerializeField] private CameraResolutionDownScaleSO cameraResolutionDownScale;

        [SerializeField] private GameEvent aeroplaneStateHasReachedMaxHeight;
        [SerializeField] private GameEvent playerHasCompletedAeroplaneState;

       // private bool postProcessingDisabled = false;
        private int primaryCameraForegroundCullingMask;
        private int secondaryCameraBackgroundCullingMask;

        private void Awake()
        {
            primaryCameraForegroundCullingMask = primaryCameraForeground.cullingMask;
            secondaryCameraBackgroundCullingMask = secondaryCameraBackground.cullingMask;

            GameBenchmark.OnGameBenchmarkDone += DisablePostProcessingIfAverageFPSLow;
            RemoteConfiguration.RemoteConfigurationDataFetched += RemoteConfiguration_RemoteConfigurationDataFetched;
            aeroplaneStateHasReachedMaxHeight.TheEvent.AddListener(HandleAeroplaneStateHasReachedMaxHeight);
            playerHasCompletedAeroplaneState.TheEvent.AddListener(HandlePlayerHasCompletedAeroplaneState);
        }

        private void DisablePostProcessingIfAverageFPSLow(float averageFPS)
        {
            if (cameraResolutionDownScale.DisablePostProcessing)
                return;

            if(averageFPS < 55)
            {
                UnityEngine.Console.Log("Disabling post processing as average fps are less than 55");
                DisablePostProcessing();
            }
        }

        private void OnDestroy()
        {
            GameBenchmark.OnGameBenchmarkDone -= DisablePostProcessingIfAverageFPSLow;
            RemoteConfiguration.RemoteConfigurationDataFetched -= RemoteConfiguration_RemoteConfigurationDataFetched;
            aeroplaneStateHasReachedMaxHeight.TheEvent.RemoveListener(HandleAeroplaneStateHasReachedMaxHeight);
            playerHasCompletedAeroplaneState.TheEvent.RemoveListener(HandlePlayerHasCompletedAeroplaneState);
        }

        private void RemoteConfiguration_RemoteConfigurationDataFetched()
        {
            DeviceGroup deviceGroup = EstimateDevicePerformance.GetDeviceGroup();

            if (deviceGroup == DeviceGroup.LowEnd || deviceGroup == DeviceGroup.MidEnd || EstimateDevicePerformance.isOpenGLTwoDevice
                  || EstimateDevicePerformance.isOreoDevice || EstimateDevicePerformance.isRamLowerThanThreeGB || EstimateDevicePerformance.IsPhoneBlackListed)
            {
                DisablePostProcessing();
            }
        }

        private void HandleAeroplaneStateHasReachedMaxHeight(GameEvent gameEvent)
        {
            if (cameraResolutionDownScale.DisablePostProcessing)
                return;

            // Show
            secondaryCameraBackground.cullingMask |= 1 << LayerMask.NameToLayer("ObstaclesMesh");
            // Hide
            primaryCameraForeground.cullingMask &= ~(1 << LayerMask.NameToLayer("ObstaclesMesh"));
        }

        private void HandlePlayerHasCompletedAeroplaneState(GameEvent gameEvent)
        {
            if (cameraResolutionDownScale.DisablePostProcessing)
                return;

            // Show
            primaryCameraForeground.cullingMask |= 1 << LayerMask.NameToLayer("ObstaclesMesh");
            // Hide
            secondaryCameraBackground.cullingMask &= ~(1 << LayerMask.NameToLayer("ObstaclesMesh"));
        }

        private IEnumerator Start()
        {
            yield return null;

            if (cameraResolutionDownScale.DisablePostProcessing)
            {
                isBloom = false;
                isBlur = false;
            }

            if(EstimateDevicePerformance.GetDeviceGroup() == DeviceGroup.HighEnd)
            {
                isBlur = false;
            }

            bool bloomInitialized = isBloom && BloomOptimized.Initialize();
            BloomOptimized.enabled = bloomInitialized;

            bool motionBlurInitialized = isBlur && MotionBlur.enabled;
            MotionBlur.enabled = motionBlurInitialized;

            if (!bloomInitialized && !motionBlurInitialized)
            {
                EnableDisableCameras(false);
            }
        }

        private void EnableDisableCameras(bool enableCamera)
        {
            if (enableCamera)
            {
                cameraResolutionDownScale.DisablePostProcessing = false;
                secondaryCameraBackground.gameObject.SetActive(true);

                primaryCameraForeground.clearFlags = CameraClearFlags.Nothing;
                primaryCameraForeground.cullingMask = primaryCameraForegroundCullingMask;
                secondaryCameraBackground.cullingMask = secondaryCameraBackgroundCullingMask;
            }
            else
            {
                cameraResolutionDownScale.DisablePostProcessing = true;
                secondaryCameraBackground.gameObject.SetActive(false);

                primaryCameraForeground.clearFlags = CameraClearFlags.Skybox;
                primaryCameraForeground.cullingMask = -1;
            }
        }

        [Button("Disable PostProcessing")]
        private void DisablePostProcessing()
        {
            BloomOptimized.enabled = false;
            MotionBlur.enabled = false;
            EnableDisableCameras(false);
        }

        [Button("Enable PostProcessing")]
        private void EnablePostProcessing()
        {
            BloomOptimized.enabled = true;
            MotionBlur.enabled = true;
            EnableDisableCameras(true);
        }
    }
}