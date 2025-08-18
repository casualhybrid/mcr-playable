using Cinemachine;
using Klak.Wiring;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class FloatingPointReset : MonoBehaviour
{
    [SerializeField] private float distanceThresholdForResetting;
    [SerializeField] private PlayerSharedData playerSharedData;
    [SerializeField] private float originZ = 0;
    [SerializeField] private CinemachineBrain cinemachineBrain;
    [SerializeField] private CinemachineVirtualCamera mainCineMachineCamera;

    public UnityEvent OnFloatingResetDone;


    private float currentDistanceFromOrigin;

   

    private void LateUpdate()
    {
        if (!GameManager.IsGameStarted)
            return;


        Transform playerT = playerSharedData.PlayerTransform;
        currentDistanceFromOrigin = playerT.position.z - originZ;

        if (currentDistanceFromOrigin < distanceThresholdForResetting)
            return;

        if (playerSharedData.CurrentStateName != PlayerState.PlayerNormalMovementState)
            return;

        if (!playerSharedData.IsGrounded || playerSharedData.IsDash || playerSharedData.IsBoosting || playerSharedData.isJumpingPhase)
            return;

        if (cinemachineBrain.IsBlending || cinemachineBrain.ActiveVirtualCamera != mainCineMachineCamera as ICinemachineCamera)
            return;

        OffsetTheEnvironment();

        //        CoroutineRunner.Instance.WaitTillFrameEndAndExecute(OffsetTheEnvironment);

        OnFloatingResetDone.Invoke();


    }

    private void OffsetTheEnvironment()
    {
        float offsetToSubract = currentDistanceFromOrigin;
        var objectsToReset = FindObjectsOfType<MonoBehaviour>().OfType<IFloatingReset>().ToArray();

        for (int i = 0; i < objectsToReset.Length; i++)
        {
            IFloatingReset objectToRest = objectsToReset[i] as IFloatingReset;
            StartCoroutine(Delay(true));
            objectToRest.OnBeforeFloatingPointReset();

            MonoBehaviour objectToResetBehaviour = objectToRest as MonoBehaviour;
            Transform objectToResetTranform = objectToResetBehaviour.transform;

            if (!objectToRest.ShoudNotOffsetOnRest)
            {
                Vector3 objToRestPos = objectToResetTranform.position;
                objToRestPos.z -= offsetToSubract;

                objectToResetTranform.position = objToRestPos;

                UnityEngine.Console.Log($"MATSSSS  Resetting Offset of {objectToResetBehaviour.gameObject.name} to {objToRestPos.z}");
            }

            objectToRest.OnFloatingPointReset(offsetToSubract);
        }
    }



    IEnumerator Delay(bool isBefore)
    {
        if (isBefore)
        {

            playerSharedData.PlayerRigidBody.interpolation = RigidbodyInterpolation.None;  //zzzn
            playerSharedData.PlayerRigidBody.collisionDetectionMode = CollisionDetectionMode.Discrete;  //zzzn
            yield return new WaitForSecondsRealtime(3);
            playerSharedData.PlayerRigidBody.interpolation = RigidbodyInterpolation.Interpolate;  //zzzn
            playerSharedData.PlayerRigidBody.collisionDetectionMode = CollisionDetectionMode.Continuous;

        }
        
    }

}