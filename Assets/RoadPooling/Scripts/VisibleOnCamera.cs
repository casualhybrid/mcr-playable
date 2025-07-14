using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisibleOnCamera : MonoBehaviour
{
    [SerializeField] private GameEvent cutSceneStartedEvent;
    [SerializeField] private  bool inCam = false;
    Trafficar trafficcar;
    void OnEnable()
    {
        inCam = false;
        trafficcar = transform.parent.transform.gameObject.GetComponent<Trafficar>();
     cutSceneStartedEvent.TheEvent.AddListener(TruckMove);
    }

    private void OnDisable()
    {
      cutSceneStartedEvent.TheEvent.RemoveListener(TruckMove);
    }
    void OnBecameVisible()
    {

        inCam = true;
    }
    void OnBecameInvisible()
    {

        inCam = false;
    }

    void TruckMove(GameEvent gameEvent)
    {
        if (inCam)
        {
           
            trafficcar.DoYourWork = true;
          
        }
        else
        {
            transform.parent.transform.gameObject.SetActive(false);
        }
    }


}
