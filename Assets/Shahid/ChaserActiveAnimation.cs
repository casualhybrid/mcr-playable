using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ChaserActiveAnimation : MonoBehaviour
{
    [SerializeField] CutSceneCore instance;
    // Start is called before the first frame update
    [SerializeField] GameObject originalChaser;
    [SerializeField] GameObject chaserAnimation;
    public GameObject cam;
    public GameObject normalCam;
    public bool isAnimation;
    static bool firtTime;
    public static ChaserActiveAnimation Instance;
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        if (!firtTime)
        {
            firtTime = true;
            if (isAnimation)
                StartCoroutine(CharacterActive(2.5f));
        }
        else
        {
            chaserAnimation.SetActive(true);
        }
    }
    public void ActiveChaser()
    {
        originalChaser.SetActive(true);
        chaserAnimation.SetActive(false);
        Debug.Log("ActiveChaser");
    }
    public void OnCamActive()
    {
        instance.ChangeRushPlayerAfterCutSceneState(false);
        cam.SetActive(true);
        Debug.Log("ActiveCam");
    }
    public void DisableCam()
    {
        normalCam.SetActive(false);
        Debug.Log("disableCam");
    }
   
    IEnumerator CharacterActive(float time)
    {
        yield return new WaitForSeconds(time);
        chaserAnimation.SetActive(true);
        Debug.Log("characterShow");
    }
    public void ChaserActive()
    {
        chaserAnimation.SetActive(true);
    }
}
