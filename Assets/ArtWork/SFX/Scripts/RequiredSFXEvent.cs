using UnityEngine;

public class RequiredSFXEvent : MonoBehaviour
{
    [SerializeField] private GameEvent enviormentPatchSFXEvent;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (enviormentPatchSFXEvent)
            {
                enviormentPatchSFXEvent.RaiseEvent();
                
            }

            
        }
    }
}
