using UnityEngine;

public class CameraShakeCue : MonoBehaviour
{
    [SerializeField] private CameraShakeCueSO cameracue = default;

    [SerializeField] private CameraShakeCueChannel _cameraShakeCueEventChannel = default;

    public void PlayParticleCue(GameEvent theEvent)
    {
        _cameraShakeCueEventChannel.RaiseEvent(cameracue.GetCameraShakeConfig);
    }


    // Temp
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.S))
        {
          var a =  cameracue.GetCameraShakeConfig;
            _cameraShakeCueEventChannel.RaiseEvent(cameracue.GetCameraShakeConfig);
        }
    }
}