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
        }
    }

}
