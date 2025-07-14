using UnityEditor;
using UnityEngine;

/// <summary>
///     This Component is connected with traffic, which is 
///         acting like hurdles....
/// </summary>
public class IsPlayerPassingBy : MonoBehaviour
{
    [SerializeField] private PlayerSharedData playerSharedData;
    private GameObject playerObj;
    private EventBasedAudioPlayer audioPlayerObj;
    private bool isPassBy = false;

    // Start is called before the first frame update
    void Start()
    {
        audioPlayerObj = GetComponent<EventBasedAudioPlayer>();
        playerObj = playerSharedData.PlayerRigidBody.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (!playerObj)
            return;

        //print("Distance = " + Vector3.Distance(playerObj.transform.position, gameObject.transform.position));
        if (!audioPlayerObj) {
            UnityEngine.Console.LogError("EventBasedAudioPlayer Script is missing from GameObject " + gameObject.name);
            return; 
        }

        if (!isPassBy) 
        {
            if (gameObject.transform.position.z - playerObj.transform.position.z  < 2)
            {
                isPassBy = true;
                audioPlayerObj.ShootAudioEvent();
            
            } 
        }
    }

    public void ResetScript()
    {
        isPassBy = false;
    }
}
