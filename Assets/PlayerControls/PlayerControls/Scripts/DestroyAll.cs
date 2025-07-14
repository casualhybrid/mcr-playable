using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAll : MonoBehaviour
{
    // Start is called before the first frame update
   // List<GameObject> collisionobj = new List<GameObject>();
    [SerializeField] PlayerSharedData PlayerSharedData;

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.activeSelf && other.gameObject.tag == "obstacle" && (PlayerSharedData.IsBoosting || PlayerSharedData.IsDash))
        {
            other.gameObject.SetActive(false);
            StartCoroutine(TurnOff(other.gameObject));            
        }
    
    }

    private IEnumerator TurnOff(GameObject G)
    {
        yield return new WaitForSeconds(2);
        G.SetActive(true);
    }


    


}
