using Cinemachine;
using System.Collections;
using UnityEngine;

public class CutSceneCameraManager : MonoBehaviour
{
    [SerializeField] private GameEvent playerSkeletonSpawned;
    [SerializeField] private PlayerSharedData playerSharedData;

    private IEnumerator Start()
    {
        yield return null;
        CinemachineVirtualCamera cinemachineVirtualCamera = GetComponentInChildren<CinemachineVirtualCamera>(true);
        cinemachineVirtualCamera.enabled = true;


    }

    private void OnEnable()
    {
        playerSkeletonSpawned.TheEvent.AddListener(HandleSkeletonSpawned);
    }

    private void OnDisable()
    {
        playerSkeletonSpawned.TheEvent.RemoveListener(HandleSkeletonSpawned);
    }

    private void HandleSkeletonSpawned(GameEvent gameEvent)
    { 
        CinemachineVirtualCamera[] cinemachineVirtualCameras = GetComponentsInChildren<CinemachineVirtualCamera>(true);

        for (int i = 0; i < cinemachineVirtualCameras.Length; i++)
        {
            CinemachineVirtualCamera cinemachineVirtualCamera = cinemachineVirtualCameras[i];
            cinemachineVirtualCamera.m_LookAt = playerSharedData.CarSkeleton.MidPointAnimatedT;
            cinemachineVirtualCamera.m_Follow = playerSharedData.CarSkeleton.MidPointAnimatedT;
        }
    }
}