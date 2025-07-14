using UnityEngine;

public class MatchValuesOfBothCamerasLateUpdate : MonoBehaviour
{
    [SerializeField] private CameraData cameraData;

    private void LateUpdate()
    {
        if (!GameManager.DependenciesLoaded)
            return;

        cameraData.SecondaryCamera.fieldOfView = cameraData.TheMainCamera.fieldOfView;
    }
}
