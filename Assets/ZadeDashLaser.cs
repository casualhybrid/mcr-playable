using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ZadeDashLaser : MonoBehaviour
{
    [SerializeField] private LayerMask detectionLayer;
    [SerializeField] private ParticleSystem laserParticle;

    private void OnEnable()
    {
        laserParticle.Play();
        //UnityEngine.Console.LogError("Lasser Started");

        AnalyticsManager.CustomData("PlayerFiredLaser");
    }


    private void FixedUpdate()
    {
        Collider[] colliders = Physics.OverlapBox(transform.position, transform.localScale / 7, Quaternion.identity, detectionLayer);
        foreach (Collider collider in colliders)
        {
            ObjectDestruction objectDestruction = collider.GetComponent<ObjectDestruction>();
            if (objectDestruction != null && objectDestruction.isDestroyingDuringLaser)
            {
                objectDestruction.DestroyCar();
            }
        }
       
    }

    //void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    //Check that it is being run in Play Mode, so it doesn't try to draw this in Editor mode
    //   // if (isLaserStarted)
    //  //  {
    //        //Draw a cube where the OverlapBox is (positioned where your GameObject is as well as a size)
    //        Gizmos.DrawWireCube(transform.position, transform.localScale / 7);
    //  //  }
    //}
}
