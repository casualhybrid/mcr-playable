using Cinemachine;
using UnityEngine;

public class DampingXOverrideExtension : CinemachineExtension
{
    [SerializeField] private float damping;
    [SerializeField] private float xCamMaxValue;

    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (stage == CinemachineCore.Stage.Body)
        {
            var rawPosition = state.RawPosition;
            float clampedXPosition = Mathf.Abs(vcam.Follow.position.x) > xCamMaxValue / 100 ? Mathf.Sign(vcam.Follow.position.x) * xCamMaxValue / 100 : vcam.Follow.position.x;
            rawPosition.x = Mathf.MoveTowards(vcam.transform.position.x, clampedXPosition, deltaTime * damping);

          
            state.RawPosition = rawPosition;
        }
    }

 
}