using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;

public class PlayerCollision : MonoBehaviour
{
    [SerializeField] float timeDuration;
    [SerializeField] private AdsController adsController;
    [Header("Elastic Movement Settings")]
    public float moveDistance = 0.5f;     
    public float duration = 1f;
    [EnumToggleButtons] public Ease easeType = Ease.InOutElastic;
    [EnumToggleButtons] public LoopType loopType = LoopType.Yoyo;
    [Header("Subtle Rotation Settings")]
    public float rotationAmount = 5f;     // Small angle in degrees
    public float rotationDuration = 2f;

    [SerializeField] AudioSource audio;
    [SerializeField] ParticleSystem particles;

    [SerializeField] GameObject parent;


    private void Start()
    {
        float startY = transform.position.y;
        float initialYRotation = transform.eulerAngles.y;

        transform.DOMoveY(startY + moveDistance, duration)
            .SetEase(easeType)
            .SetLoops(-1, loopType);

        transform.DORotate(new Vector3(0, initialYRotation + rotationAmount, 0), rotationDuration)
           .SetEase(Ease.Linear)
           .SetLoops(-1, LoopType.Yoyo)
           .From(new Vector3(0, initialYRotation - rotationAmount, 0));
    }
    private bool hasTriggered = false; // Yeh flag lagaya hai

    /*private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return; // Pehle trigger ho chuka to kuch na karo

        if (other.CompareTag("Player"))
        {
            hasTriggered = true; // Ab is object ka trigger ho gaya hai

            PersistentAudioPlayer.Instance.AudioFade(timeDuration);

            if (audio)
                audio.enabled = true;

            StartCoroutine(Test());
        }
    }*/
    private void OnCollisionEnter(Collision collision)
    {
        if (hasTriggered) return;
        if (collision.gameObject.CompareTag("Player"))
        {
            hasTriggered = true; // Ab is object ka trigger ho gaya hai
            if (!PersistentAudioPlayer.isSoundOff)
            {
                PersistentAudioPlayer.Instance.AudioFade(timeDuration);

                if (audio)
                    audio.enabled = true;
            }
            StartCoroutine(Test());

            particles.Play();
            StartCoroutine(AfterDisbale());
        }
    }
    IEnumerator Test()
    {
        yield return new WaitForSeconds(6f);
        
        this.gameObject.SetActive(false);
    }
  /*  private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
           
           // adsController.ResumeFMODMaster();
        }
    }*/
    IEnumerator AfterDisbale()
    {
        yield return new WaitForSeconds(.1f);
        parent.SetActive(false);
    }

}
