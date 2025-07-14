using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BatEnable : MonoBehaviour
{
    public static BatEnable instance;
    [SerializeField] GameObject bat;
    [SerializeField] GameObject bat1;
    [SerializeField] ParticleSystem smokeParticle;

    public void Awake()
    {
        instance = this;    
    }
   
    public void enableAudio1()
    {
        this.gameObject.GetComponent<AudioSource>().Play();
    }
    public void ActiveBat()
    {
        bat.SetActive(true);
        bat1.SetActive(false);
    }
    public void ParticlePlay()
    {
        smokeParticle.gameObject.SetActive(true);
        smokeParticle.Play();
    }
    public void DeActiveBat()
    {
        bat1.SetActive(true);
        bat.SetActive(false);
    }
    public void SecondBatDeactive()
    {
        bat1.SetActive(false);
    }
}
