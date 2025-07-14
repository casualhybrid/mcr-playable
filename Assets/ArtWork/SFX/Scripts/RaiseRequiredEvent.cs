using UnityEngine;

/// <summary>
/// This Class is written to raise event on collider or trigger based interaction
/// </summary>
public class RaiseRequiredEvent : MonoBehaviour
{
    [SerializeField] private GameEvent requiredEvent;
    [SerializeField] private InteractionBasedEventTypes eventType;
    [SerializeField] private string triggerTag;

    private void OnCollisionEnter(Collision collision)
    {
        if (eventType == InteractionBasedEventTypes.OnColliderEnter)
        {
            if (collision.gameObject.CompareTag(triggerTag))
            {
                ShootEvent();
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (eventType == InteractionBasedEventTypes.OnColliderStay)
        {
            if (collision.gameObject.CompareTag(triggerTag))
            {
                ShootEvent();
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (eventType == InteractionBasedEventTypes.OnColliderExit)
        {
            if (collision.gameObject.CompareTag(triggerTag))
            {
                ShootEvent();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (eventType == InteractionBasedEventTypes.OnTriggerEnter)
        {
            if (other.gameObject.CompareTag(triggerTag))
            {
                ShootEvent();
            }
        }
    }


    private void OnTriggerStay(Collider other)
    {
        if (eventType == InteractionBasedEventTypes.OnTriggerStay)
        {
            if (other.gameObject.CompareTag(triggerTag))
            {
                ShootEvent();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (eventType == InteractionBasedEventTypes.OnTriggerExit)
        {
            if (other.gameObject.CompareTag(triggerTag))
            {
                ShootEvent();
            }
        }
    }

    /// <summary>
    /// Both OneShot and Looping audios are handled from PlayAudio function
    /// </summary>
    private void ShootEvent()
    {
        if (requiredEvent)
            requiredEvent.RaiseEvent();
        
     //   UnityEngine.Console.Log("Event triggered========================================================" + requiredEvent );
    }

}

/// <summary>
/// Dont change the pattern of these Enum and add new Enum at the end 
/// </summary>
public enum InteractionBasedEventTypes
{
    OnColliderEnter, OnColliderExit, OnTriggerEnter, OnTriggerStay, OnTriggerExit, OnColliderStay, None
};

