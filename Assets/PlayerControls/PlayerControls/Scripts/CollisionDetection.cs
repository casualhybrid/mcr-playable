using FSM;
using UnityEngine;

public class CollisionDetection : MonoBehaviour
{
    [SerializeField] private bool detectCollision;
    [SerializeField] private PlayerSharedData PlayerSharedData;
    [SerializeField] private PlayerContainedData PlayerContainedData;

    private bool pogo = false;

    // Start is called before the first frame update
    private void Start()
    {
        Time.timeScale = 1;
    }

    private void OnTriggerEnter(Collider other)
    {



    }

    private void OnTriggerStay(Collider other)
    {
 

    }

    private void OnTriggerExit(Collider other)
    {
    
   
    }

    public void DetectCollisions()
    {
        detectCollision = !detectCollision;
    }
}