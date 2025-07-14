using UnityEngine;

public class NewEnvironmentEnteredTrigger : MonoBehaviour
{
    public Environment enviornment { get; set; }

    [SerializeField] private EnvironmentChannel environmentChannel;
    [SerializeField] private MapProgressionSO mapData;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            this.gameObject.SetActive(false);
            //  UnityEngine.Console.Log("Triggering Player Entered New Env " + enviornment.name);
            environmentChannel.RaisePlayerEnvironmentChangedEvent(enviornment);
          //  mapData.CompletedMapsUpdate(enviornment);
          
        }
    }
}