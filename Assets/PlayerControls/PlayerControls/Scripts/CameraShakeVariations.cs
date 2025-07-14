using Cinemachine;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;
using UnityEngine.Events;

public class UnityCameraShakeConfigEvent : UnityEvent<CameraShakeConfig>
{ }

[System.Serializable]
public struct CameraShakeConfig
{
    [SerializeField] private float amplitude;
    [SerializeField] private float frequency;
    [SerializeField] private float shakeTime;
    [SerializeField] private NoiseSettings noiseSettings;

    public float GetAmplitude { get { return amplitude; } set { amplitude = value; } }
    public float GetFrequenct { get { return frequency; } set { frequency = value; } }
    public float GetShakeTime { get { return shakeTime; } set { frequency = value; } }
    public NoiseSettings GetNoiseSettings => noiseSettings;

    [HideInInspector] public bool isValid;
}

[CreateAssetMenu(fileName = "CameraShakeVariations", menuName = "ScriptableObjects/Camera/CameraShakeConfig")]
public class CameraShakeVariations : ScriptableObject
{
    [SerializeField] private bool active = true;
    [SerializeField] private CameraShakeCueChannel cameraShakeCueChannel;
    public CameraShakeConfig CurrentCameraShakeConfig { get; private set; }
    public UnityCameraShakeConfigEvent OnShakeTheCamera { get; set; } = new UnityCameraShakeConfigEvent();

    public UnityEvent OnShakeEnded { get; private set; } = new UnityEvent();

    [System.Serializable]
    private class ShakeCameraDictionary : SerializableDictionaryBase<GameEvent, CameraShakeConfig>
    { }

    [Space]
    [SerializeField] private ShakeCameraDictionary shakeCameraDictionary;

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;

        cameraShakeCueChannel.OnShakeTheCamera += ShakeTheCamereraConfigRecieved;

        foreach (var item in shakeCameraDictionary)
        {
            item.Key.TheEvent.AddListener(ShakeCameraAccordingToEvent);
        }
    }

    private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        RaiseShakeEndedEvent();
        ResetVariable();
    }

    private void ResetVariable()
    {
        CurrentCameraShakeConfig = new CameraShakeConfig();
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;


        cameraShakeCueChannel.OnShakeTheCamera -= ShakeTheCamereraConfigRecieved;

        foreach (var item in shakeCameraDictionary)
        {
            item.Key.TheEvent.RemoveListener(ShakeCameraAccordingToEvent);
        }
    }

    private void ShakeTheCamereraConfigRecieved(CameraShakeConfig config)
    {
      //  UnityEngine.Console.Log($"Camera Shake Requested");

        config.isValid = true;
        CurrentCameraShakeConfig = config;
        OnShakeTheCamera.Invoke(config);
    }

    public void ShakeCameraAccordingToEvent(GameEvent theEvent)
    {
    //    UnityEngine.Console.Log($"Camera Shake Event {theEvent.name}");

        CameraShakeConfig cameraShakeConfig = GetShakeConfigforTheEvent(theEvent);
        cameraShakeConfig.isValid = true;
        CurrentCameraShakeConfig = cameraShakeConfig;
        OnShakeTheCamera.Invoke(cameraShakeConfig);
    }

    public CameraShakeConfig GetShakeConfigforTheEvent(GameEvent theEvent)
    {
        if (!active)
            return new CameraShakeConfig();

        if (shakeCameraDictionary.ContainsKey(theEvent))
        {
            return shakeCameraDictionary[theEvent];
        }
        else
        {
        //    UnityEngine.Console.LogWarning($"Failed getting shake configuration for the event {theEvent.name}");
            return new CameraShakeConfig();
        }
    }

    

    public void RaiseShakeEndedEvent()
    {
        CameraShakeConfig cameraShakeConfig = CurrentCameraShakeConfig;
        cameraShakeConfig.isValid = false;
        CurrentCameraShakeConfig = cameraShakeConfig;
        OnShakeEnded.Invoke();
    }
}