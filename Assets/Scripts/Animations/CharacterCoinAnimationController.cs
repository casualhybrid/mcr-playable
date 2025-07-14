using UnityEngine;

public class CharacterCoinAnimationController : MonoBehaviour
{
    [SerializeField] private GameObject coinGameObject;

    private Vector3 startingCoinLocalPos;
    private Animator coinAnimator;

    private void Awake()
    {
        startingCoinLocalPos = coinGameObject.transform.localPosition;
        coinAnimator = coinGameObject.GetComponent<Animator>();
    }

    public void InitializeTheCoin()
    {
        UnityEngine.Console.Log("Initializing Coin");

        coinGameObject.transform.localPosition = startingCoinLocalPos;
        coinGameObject.transform.localRotation = Quaternion.identity;
        coinGameObject.SetActive(true);
        coinAnimator.Play("CoinFlip");
    }

    public void CoinHasBeenCaught()
    {
        UnityEngine.Console.Log("Coin caught");

        coinAnimator.Play("Default");
        coinGameObject.SetActive(false);
    }
}