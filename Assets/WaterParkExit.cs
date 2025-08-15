using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using static UnityEditor.Experimental.GraphView.GraphView;

public class WaterParkExit : MonoBehaviour
{
    [SerializeField] GameObject water1;
    [SerializeField] GameObject water2;

    [SerializeField] GameObject mirrorBreak;
    [SerializeField] GameObject mirrorBrea2;

    [SerializeField] AudioSource audioSound;
    //[SerializeField] private PlayerSharedData PlayerSharedData;
    [SerializeField] private Rigidbody MyRigidbody;


    private void Start()
    {
        MyRigidbody = gameObject.transform.root.GetComponent<Rigidbody>();
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ParkExit"))
        {
            WaterParkEnter.isEnter = false;
            water1.SetActive(false);
            water2.SetActive(false);
        }
        if (other.CompareTag("FlyingRampExit"))
        {
            mirrorBreak.SetActive(false);
            mirrorBrea2.SetActive(false);
            PersistentAudioPlayer.Instance.PlayTechnologia();


            MyRigidbody = gameObject.transform.root.GetComponent<Rigidbody>();

            //StartCoroutine(Delay(true));

        }
    }
    IEnumerator Delay(bool isBefore)
    {
        if (isBefore)
        {
            Debug.LogError("ExitCollider");
            
            MyRigidbody.interpolation = RigidbodyInterpolation.None;  //zzzn
            MyRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;  //zzzn
            yield return new WaitForSecondsRealtime(20f);
            MyRigidbody.interpolation = RigidbodyInterpolation.Interpolate;  //zzzn
            MyRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
        else
        {
            MyRigidbody.interpolation = RigidbodyInterpolation.Interpolate;  //zzzn
            MyRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            yield return new WaitForSecondsRealtime(3);
            MyRigidbody.interpolation = RigidbodyInterpolation.None;  //zzzn
            MyRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;  //zzzn
        }
    }

}
