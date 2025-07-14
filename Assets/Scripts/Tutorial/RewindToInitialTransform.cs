using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

public class RewindToInitialTransform : MonoBehaviour
{
    [SerializeField] private UnityEvent OnRewindDone;
    [ShowIf("isUsingGlobalRewindDisabled")] [SerializeField] private float rewindDuration;
    [SerializeField] private bool useGlobalTutorialRewindDuration = true;

    public bool isUsingGlobalRewindDisabled => !useGlobalTutorialRewindDuration;

    private Vector3 initialLocalPosition;
    private Vector3 initialLocalRotation;

    private float rewindDurationToUse;

    private void OnEnable()
    {
        rewindDurationToUse = useGlobalTutorialRewindDuration ? TutorialManager.RewindTime : rewindDuration;

        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation.eulerAngles;
    }

    public void DoRewind()
    {
        UnityEngine.Console.Log($"Do Rewind Obstacle");

        Sequence sequence = DOTween.Sequence();
        sequence.Join(transform.DOLocalMove(initialLocalPosition, rewindDurationToUse));
        sequence.Join(transform.DOLocalRotate(initialLocalRotation, rewindDurationToUse));
        sequence.AppendCallback(() =>
        {
           UnityEngine.Console.Log($"Rewind Done {name}");
            OnRewindDone.Invoke();
        });
    }
}