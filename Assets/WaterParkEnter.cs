using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterParkEnter : MonoBehaviour
{
   // [SerializeField] private PlayerSharedData PlayerSharedData;
    [SerializeField] GameObject water;
    [SerializeField] GameObject water1;

    [SerializeField] GameObject mirrorBreak;
    [SerializeField] GameObject mirrorBrea2;

    public static bool isEnter = false;
    public static bool isFlying = true;

    public static WaterParkEnter Instance;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        //Debug.LogError("FalseDone");
        isEnter = false;
        water.SetActive(false);
        water1.SetActive(false);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Park"))
        {
            isEnter = true;
            water.SetActive(true);
            water1.SetActive(true);
        }
        if (other.CompareTag("FlyingRamp"))
        {
            Debug.LogError("Flying?");
            mirrorBreak.SetActive(true);
            mirrorBrea2.SetActive(true);
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
        //Debug.LogError("Ground");
       
        if (water && water1&&isEnter&&isFlying)
        {
            water.transform.parent.gameObject.SetActive(true);
          //  Debug.LogError(water.transform.parent.gameObject.name);
            water.SetActive(true);
            water1.transform.parent.gameObject.SetActive(true);
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
