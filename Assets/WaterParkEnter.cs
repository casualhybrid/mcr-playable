using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterParkEnter : MonoBehaviour
{
   // [SerializeField] private PlayerSharedData PlayerSharedData;
    [SerializeField] GameObject water;
    [SerializeField] GameObject water1;

    public static bool isEnter = false;

    public static WaterParkEnter Instance;

    private void Awake()
    {
        Instance = this;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Park"))
        {
            isEnter = true;
            water.SetActive(true);
            water1.SetActive(true);
        }
    }
    /*private void Update()
    {
        if (!isEnter)
        {
            water.SetActive(false);
            water1.SetActive(false);
            return;
        }
           
        if (PlayerSharedData.IsGrounded)
        {
            if(water&& water1)
            {
                water.SetActive(true);
                water1.SetActive(true);
            }
        }
        else
        {
            if (water && water1)
            {
                water.SetActive(false);
                water1.SetActive(false);
            }
        }

    }*/
    public void WaterON()
    {
        Debug.LogError("Ground");
       
        if (water && water1&&isEnter)
        {
            water.SetActive(true);
            water1.SetActive(true);
        }
    }
    public void WaterOFF()
    {
       
        if (water && water1 && isEnter)
        {
            water.SetActive(false);
            water1.SetActive(false);
        }
    }
    /* private void OnTriggerExit(Collider other)
     {
         if (other.CompareTag("Park"))
         {
             water.SetActive(false);
         }
     }*/
    /*   private void OnCollisionEnter(Collision collision)
       {
           if(collision.gameObject.CompareTag("Park"))
           {
               water.SetActive(true);
           }
       }
       private void OnCollisionExit(Collision collision)
       {
           if (collision.gameObject.CompareTag("Park"))
           {
               water.SetActive(false);
           }
       }*/
}
