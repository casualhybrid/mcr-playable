using System.Collections;
using UnityEngine;

public class ArmorPickupHandler : MonoBehaviour
{
    [SerializeField] private GameObject armor;
    [SerializeField] private GameEvent armourPicked;
    [SerializeField] private GameEvent armourUsed;
    [SerializeField] private GameEvent playerStartedSliding;
    [SerializeField] private GameEvent playerEndedSliding;

    [SerializeField] private PlayerSharedData playerData;
    [SerializeField] private RequestTogglePlayerMeshChannel requestTogglePlayerMeshChannel;
    private Animator animator;

    private Coroutine armourOffRoutine;

    private void Awake()
    {
        animator = armor.GetComponent<Animator>();
        animator.keepAnimatorStateOnDisable = false;
    }

    private void Start()
    {
        armourPicked.TheEvent.AddListener(showArmour);
        armourUsed.TheEvent.AddListener(HideArmour);
    }

    private void SubscribeEvents()
    {
        playerStartedSliding.TheEvent.AddListener(PlaySlidingAnimation);
        playerEndedSliding.TheEvent.AddListener(PlayerEndedAnimation);
    }

    private void DeSubscribeEvents()
    {
        playerStartedSliding.TheEvent.RemoveListener(PlaySlidingAnimation);
        playerEndedSliding.TheEvent.RemoveListener(PlayerEndedAnimation);
    }

    private void OnDestroy()
    {
        DeSubscribeEvents();

        armourPicked.TheEvent.RemoveListener(showArmour);
        armourUsed.TheEvent.RemoveListener(HideArmour);
    }

    public void RequestDisableMeshes()
    {
        if (!playerData.IsArmour)
            return;

        requestTogglePlayerMeshChannel.RaiseRequestToDisableAllSkinnedMeshes();
    }

    private void PlaySlidingAnimation(GameEvent gameEvent)
    {
        UnityEngine.Console.Log("Slide Anim");

        animator.SetBool("isSliding", true);
    }

    private void PlayerEndedAnimation(GameEvent gameEvent)
    {
        UnityEngine.Console.Log("UP Anim");

        animator.SetBool("isSliding", false);
    }

    private void showArmour(GameEvent gameEvent)
    {
        if (playerData.IsArmour)
            return;

        // StartCoroutine(armourOn());
        armourOn();
    }

    private void HideArmour(GameEvent gameEvent)
    {
        if (armourOffRoutine != null)
        {
            StopCoroutine(armourOffRoutine);
        }

        armourOffRoutine = StartCoroutine(armourOf());
    }

    private void armourOn()
    {
        if (armourOffRoutine != null)
        {
            StopCoroutine(armourOffRoutine);
        }

        armor.transform.localScale = Vector3.one;
        playerData.IsArmour = true;
        armor.SetActive(true);

        SubscribeEvents();
    }

    private IEnumerator armourOf()
    {
        animator.SetBool("armourUsed", true);
        playerData.IsArmour = false;
        requestTogglePlayerMeshChannel.RaiseRequestToEnableAllSkinnedMeshes();
        DeSubscribeEvents();
        yield return new WaitForSeconds(.45f);
        animator.SetBool("armourUsed", false);
        armor.SetActive(false);
    }
}