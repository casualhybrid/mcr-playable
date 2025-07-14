using TheKnights.SaveFileSystem;
using Unity.Services.Analytics;
using UnityEngine;
using Sirenix.OdinInspector;


public class CollisionBasedAudioPlayer : MonoBehaviour
{
    [Tooltip("The AudioManager listens to this event, fired by objects in any scene, to play sounds")]
    [SerializeField] private AudioEventChannelSO audioEventChannel = default;
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private TypeOfChannel channelType;
    [SerializeField] private InteractionBasedEventTypes eventType;
    [SerializeField] private string eventStr;
    [SerializeField] private string triggerTag;
    [SerializeField] private bool isUsingParameter;
    [ShowIf("isUsingParameter")] [SerializeField] private string parameterName;

    private void OnCollisionEnter(Collision collision)
    {
        if (eventType == InteractionBasedEventTypes.OnColliderEnter)
        {
            if (collision.gameObject.CompareTag(triggerTag))
            {
                ShootAudioEvent();
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (eventType == InteractionBasedEventTypes.OnColliderStay)
        {
            if (collision.gameObject.CompareTag(triggerTag))
            {
                ShootAudioEvent();
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (eventType == InteractionBasedEventTypes.OnColliderExit)
        {
            if (collision.gameObject.CompareTag(triggerTag))
            {
                ShootAudioEvent();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (eventType == InteractionBasedEventTypes.OnTriggerEnter)
        {
            if (other.gameObject.CompareTag(triggerTag))
            {
                ShootAudioEvent();
            }
        }
    }


    private void OnTriggerStay(Collider other)
    {
        if (eventType == InteractionBasedEventTypes.OnTriggerStay)
        {
            if (other.gameObject.CompareTag(triggerTag))
            {
                ShootAudioEvent();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (eventType == InteractionBasedEventTypes.OnTriggerExit)
        {
            if (other.gameObject.CompareTag(triggerTag))
            {
                ShootAudioEvent();
            }
        }
    }

    /// <summary>
    /// Both OneShot and Looping audios are handled from PlayAudio function
    /// </summary>
    private void ShootAudioEvent()
    {
        if (channelType == TypeOfChannel.OneShot)
        {
            if (isUsingParameter)
            {
                int parameter = saveManager.MainSaveFile.currentlySelectedCharacter + 1;
                audioEventChannel.RaiseEvent(gameObject, eventStr, parameterName, parameter);
            }
            else
            {
                audioEventChannel.RaiseEvent(gameObject, eventStr);
            }
        }
        else
        {
            // This else is for looping sounds...
            // Looping sounds SFX is handled in SFXSettings.cs
            audioEventChannel.RaiseEvent(gameObject, eventStr);
        }
    }

    public void SendEvent(string eventName)
    {
        AnalyticsManager.CustomData(eventName);
    }
}
