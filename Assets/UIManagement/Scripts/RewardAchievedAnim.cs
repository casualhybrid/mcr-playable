using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RewardAchievedAnim : MonoBehaviour
{
    [SerializeField] private InventorySystem inventoryObj;
    [SerializeField] private GamePlaySessionInventory sessionInventory;
    [SerializeField] private string inventoryKey;
    [SerializeField] private RectTransform targetPosition;
    [SerializeField] private AnimationCurve effectCurve;
    [SerializeField] private Vector3 distractedPos;
    [SerializeField] private GameObject objToTurnOff, rewardObjPrefab;
    [SerializeField] private float animTime = 0.5f;
    [SerializeField] private TextMeshProUGUI topBarCoinsTxt;
    [SerializeField] private AudioPlayer rewardCollected;
    [SerializeField] private ParticleSystem coinGlowParticle;
    private Vector3 initPos;

    private List<Tween> currentTweens = new List<Tween>();

    private readonly Stack<GameObject> availableCoinObjects = new Stack<GameObject>();

    private void Awake()
    {
        initPos = transform.position;
    }

    private void OnDisable()
    {
        ResetAnimation();
    }

    public void ResetAnimation()
    {
        StopAllCoroutines();

        for (int i = 0; i < currentTweens.Count; i++)
        {
            Tween t = currentTweens[i];
            if (t != null)
            {
                t.Kill();
            }
        }

        currentTweens?.Clear();

        foreach (Transform child in transform)
        {
            child.localScale = rewardObjPrefab.transform.localScale;
            availableCoinObjects.Push(child.gameObject);
        }
    }

    public void PlayDoubleCoinsAnimation()
    {
        ResetAnimation();
        topBarCoinsTxt.text = (sessionInventory.GetSessionIntKeyData(inventoryKey) * 2).ToString();
        targetPosition.gameObject.SetActive(true);

        PlayCoinAnimation();
    }

    public void Initialize()
    {
        topBarCoinsTxt.text = sessionInventory.GetSessionIntKeyData(inventoryKey).ToString();
        PlayCoinAnimation();
    }

    private void PlayCoinAnimation()
    {
        coinGlowParticle.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        coinGlowParticle.Play();
        objToTurnOff.GetComponent<CanvasGroup>().alpha = 1;

        int noOfCoins = (int)(sessionInventory.GetSessionIntKeyData(inventoryKey) / 10f);
       // float animTimeInit = ((sessionInventory.GetSessionIntKeyData(inventoryKey) / 20f) * 0.025f) + 0.35f;

        StartCoroutine(StartAnimEffect(noOfCoins));
    }

    private IEnumerator StartAnimEffect(int noOfCoins)
    {
        //  UnityEngine.Console.Log("animTime = " + animTimeInit + " || noOfCoins = " + noOfCoins);

        if (noOfCoins < 5)
            noOfCoins = 5;
        if (noOfCoins > 10)
            noOfCoins = 15;

        for (int i = 0; i < noOfCoins; i++)
        {
            GameObject instantiatedCoin;
            Vector3 pos = gameObject.transform.position +
                new Vector3(UnityEngine.Random.Range(-40, 40), UnityEngine.Random.Range(-30, 30), 0);

            if (availableCoinObjects.Count > 0)
            {
                instantiatedCoin = availableCoinObjects.Pop();
                Transform coinT = instantiatedCoin.transform;
                instantiatedCoin.SetActive(true);
                coinT.position = pos;
                coinT.rotation = gameObject.transform.rotation;
                
            }
            else
            {
                instantiatedCoin = Instantiate(rewardObjPrefab, pos,
                gameObject.transform.rotation, gameObject.transform);
            }

            yield return null;
        }

        // Exception
        foreach (Transform child in transform)
        {
            rewardCollected.ShootAudioEvent();
            Tween t = child.DOMove(transform.position + distractedPos,
                animTime).From(initPos).SetEase(effectCurve).OnComplete(() =>
                {
                    Tween moveTween = child.DOMove(targetPosition.position, animTime).SetEase(Ease.OutQuad).
                    OnComplete(() =>
                    {
                        Tween scaleTween = child.DOScale(0, animTime).OnComplete(() =>
                        {
                            child.gameObject.SetActive(false);
                        });

                        currentTweens.Add(scaleTween);
                    }).SetUpdate(true);

                    currentTweens.Add(moveTween);
                }).SetUpdate(true);

            currentTweens.Add(t);

            yield return new WaitForSecondsRealtime((animTime / 2f));
        }

        yield return new WaitForSecondsRealtime(animTime);

        Tween t1 = objToTurnOff.GetComponent<CanvasGroup>().DOFade(0, animTime).OnComplete(() =>
        {
            objToTurnOff.SetActive(false);
        }).SetUpdate(true);

        currentTweens.Add(t1);
    }
}