using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

public class DashInvincibleWindowController : SerializedMonoBehaviour
{
    [SerializeField] private float invincibilityDuration;

    [SerializeField] private PlayerSharedData playerSharedData;

    [SerializeField] private GameEvent playerStartedDash;
    [SerializeField] private GameEvent playerStartedJumping;
    [SerializeField] private GameEvent playerHasDodged;
    [SerializeField] private GameEvent playerFinishedDodging;
    [SerializeField] private GameEvent playerStartedSliding;


    private Coroutine dashInvincibilityRoutine;
    private bool pauseDodgleInvincibility = false;

    private void Awake()
    {
        SubscribeToEvents();
    }

    private void OnDestroy()
    {
        UnSubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        playerStartedDash.TheEvent.AddListener(ResetVariables);
        playerStartedJumping.TheEvent.AddListener(HandlePlayerJumpingDashInvincibility);
        playerHasDodged.TheEvent.AddListener(HandlePlayerDodgedDashInvincibility);
        playerStartedSliding.TheEvent.AddListener(HandlePlayerSlidingDashInvincibility);

        playerFinishedDodging.TheEvent.AddListener(HandlePlayerFinishedDodging);
    }

    private void UnSubscribeToEvents()
    {
        playerStartedDash.TheEvent.RemoveListener(ResetVariables);
        playerStartedJumping.TheEvent.RemoveListener(HandlePlayerJumpingDashInvincibility);
        playerHasDodged.TheEvent.RemoveListener(HandlePlayerDodgedDashInvincibility);
        playerStartedSliding.TheEvent.RemoveListener(HandlePlayerSlidingDashInvincibility);

        playerFinishedDodging.TheEvent.RemoveListener(HandlePlayerFinishedDodging);
    }

    private void ResetVariables(GameEvent gameEvent)
    {
        pauseDodgleInvincibility = false;
        playerSharedData.isInInvincibleZoneDuringDash = false;

        if (dashInvincibilityRoutine != null)
        {
            StopCoroutine(dashInvincibilityRoutine);
        }
    }

    private void HandlePlayerJumpingDashInvincibility(GameEvent gameEvent)
    {
        pauseDodgleInvincibility = false;

        CreateInvincibilityWindowForDash();
    }

    private void HandlePlayerDodgedDashInvincibility(GameEvent gameEvent)
    {
        if (pauseDodgleInvincibility)
            return;

        pauseDodgleInvincibility = true;

        CreateInvincibilityWindowForDash();
    }

    private void HandlePlayerSlidingDashInvincibility(GameEvent gameEvent)
    {
        pauseDodgleInvincibility = false;

        CreateInvincibilityWindowForDash();
    }

    private void CreateInvincibilityWindowForDash()
    {
        if (!playerSharedData.IsDash)
            return;

        if (dashInvincibilityRoutine != null)
        {
            StopCoroutine(dashInvincibilityRoutine);
        }

        dashInvincibilityRoutine = StartCoroutine(CreateInvincibilityWindowForDashRoutine());
    }

    private IEnumerator CreateInvincibilityWindowForDashRoutine()
    {
        playerSharedData.isInInvincibleZoneDuringDash = true;
        yield return new WaitForSeconds(invincibilityDuration);
        playerSharedData.isInInvincibleZoneDuringDash = false;
    }

    private void HandlePlayerFinishedDodging(GameEvent gameEvent)
    {
        pauseDodgleInvincibility = false;
    }
}