using Cinemachine;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class ModifyCameraDampingAfterDelay : MonoBehaviour
{
    private CinemachineVirtualCamera cinemachineVirtualCamera;
    private Cinemachine3rdPersonFollow cinemachineTransposer;
    private Vector3 startingDamping;

    [SerializeField] private float delay;
    [SerializeField] private Vector3 dampingAfterDelay;
    [SerializeField] private float timeToDampen;
    [SerializeField] private Ease dampenEase = Ease.Linear;

    private void Awake()
    {
        cinemachineVirtualCamera = GetComponent<CinemachineVirtualCamera>();
        cinemachineTransposer = cinemachineVirtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();

        startingDamping = cinemachineTransposer.Damping;
    }

    private void OnEnable()
    {
        cinemachineTransposer.Damping = startingDamping;
        ChangeCameraDampingAfterDelay();
    }

    private void OnDisable()
    {
        cinemachineTransposer.Damping = startingDamping;
    }

    private void ChangeCameraDampingAfterDelay()
    {
        DOTween.To(() => startingDamping, (x) => { cinemachineTransposer.Damping = x; }, dampingAfterDelay, timeToDampen).SetDelay(delay).SetUpdate(UpdateType.Fixed).SetEase(dampenEase);
    }
}