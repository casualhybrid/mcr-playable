using UnityEngine;

public class PauseSwitchEnvGenerationOnEnable : MonoBehaviour
{
    [SerializeField] private EnvironmentChannel environmentChannel;
   // [SerializeField] private bool alwaysOverrideUnpauseBehaviour = false;

    private void OnEnable()
    {
        environmentChannel.RaiseRequestPauseSwitchEnvironmentGenerationEvent();
    }

    private void OnDisable()
    {
        environmentChannel.RaiseRequestUnPauseSwitchEnvironmentGenerationEvent();
    }
}