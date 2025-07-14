using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GhostFlyAwayAnimation : MonoBehaviour
{
    [SerializeField] private TutorialSegmentStateChannel tutorialSegmentStateChannel;
    [SerializeField] private SpeedHandler speedHandler;
    [SerializeField] private float maxPlaneHeight;
    [SerializeField] private float flyDuration;

    private Rigidbody rb;
    private Tween verticalTween;

    private bool isInitialized = false;
    private float yTarget;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        tutorialSegmentStateChannel.GhostAnimationsCompleted += DoAnimation;
        tutorialSegmentStateChannel.OnRewind += ResetValues;
    }

    private void OnDestroy()
    {
        tutorialSegmentStateChannel.GhostAnimationsCompleted -= DoAnimation;
        tutorialSegmentStateChannel.OnRewind -= ResetValues;

    }

    public void DoAnimation()
    {
        isInitialized = true;
        yTarget = transform.position.y;
        FlyVertically();
    }

    private void FlyVertically()
    {
        verticalTween = DOTween.To(() => transform.position.y, (x) => { yTarget = x; }, maxPlaneHeight, flyDuration).OnComplete(() =>
        {
            isInitialized = false;
        });
    }

    private void FixedUpdate()
    {
        if (!isInitialized)
            return;

        float speed = speedHandler.GetForwardSpeedBasedOnSpecificGameTimeScale(SpeedHandler.GameTimeScale);
        Vector3 targetPos = transform.position + (Vector3.forward * speed);
        targetPos.y = yTarget;

        rb.MovePosition(targetPos);
    }

    private void ResetValues()
    {
        verticalTween.Kill();
        isInitialized = false;
    }
}