using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeUpDownSlowMo : MonoBehaviour
{
    [SerializeField] private TutorialSegmentStateChannel tutorialSegmentStateChannel;
    [SerializeField] private InputChannel inputChannel;
    [SerializeField] private PlayerSharedData playerSharedData;
    [SerializeField] public LayerMask walkableLayers;

    private void OnDisable()
    {
        inputChannel.SwipeUpOccured.RemoveListener(EndTheFirstSlowMo);
        inputChannel.SwipeUpOccured.RemoveListener(EndTheSecondSlowMo);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (TutorialManager.IsTutorialRewinding)
            return;

        if (playerSharedData.PlayerTransform.GetComponent<PlayerStateMachine>().IsJumpAllowedIfTransitionExists())
        {
            inputChannel.SwipeUpOccured.AddListener(EndTheFirstSlowMo);
            inputChannel.DisableAllButThisInputEvent(inputChannel.SwipeUpOccured);
            tutorialSegmentStateChannel.SendSlowMoRequestEvent();
        }

        StartCoroutine(WaitTillGroundSmashIsAllowed());
    }


    private void EndTheFirstSlowMo()
    {
        inputChannel.EnableAllDisabledInputs();
        tutorialSegmentStateChannel.SendDisableTheActiveHints();
        inputChannel.SwipeUpOccured.RemoveListener(EndTheFirstSlowMo);
        tutorialSegmentStateChannel.SendSlowMoEndRequestEvent();

    }

    private void EndTheSecondSlowMo()
    {
        inputChannel.EnableAllDisabledInputs();
        inputChannel.SwipeDownOccured.RemoveListener(EndTheSecondSlowMo);
        tutorialSegmentStateChannel.SendSlowMoEndRequestEvent();

    }

    //private void Update()
    //{
    //    if (playerSharedData.PlayerTransform == null)
    //        return;

    //    RaycastHit[] raycastHits = new RaycastHit[1];
    //    Vector3 center = playerSharedData.Playercollider.transform.TransformPoint(playerSharedData.Playercollider.center);
    //    Vector3 size = playerSharedData.Playercollider.bounds.size / 2f;
    //    size.y = 0.1f;

    //    Physics.BoxCastNonAlloc(center, size, Vector3.down,
    // raycastHits, Quaternion.identity, 200, 1 << LayerMask.NameToLayer("Walkable") | 1 << LayerMask.NameToLayer("WalkableLimited"));

    //    if(raycastHits[0].collider != null)
    //    ExtDebug.DrawBoxCastOnHit(center, size, Quaternion.identity, Vector3.down, raycastHits[0].distance, Color.magenta);
    //}

    private IEnumerator WaitTillGroundSmashIsAllowed()
    {
        while(true)
        {
            RaycastHit hit;
            Vector3 center = playerSharedData.Playercollider.transform.TransformPoint(playerSharedData.Playercollider.center);
            Vector3 size = playerSharedData.Playercollider.bounds.size / 2f;
            size.y = 0f;
            size.z *= 4f;

           Physics.BoxCast(center, size, Vector3.down,
     out hit, Quaternion.identity, 200, walkableLayers, QueryTriggerInteraction.Collide);

            if(hit.collider != null && hit.collider.gameObject.layer == LayerMask.NameToLayer("Walkable") && hit.collider.gameObject.CompareTag("ground"))
            {
             //   Debug.DrawRay(center, Vector3.right * 100f, Color.black);
               // ExtDebug.DrawBoxCastOnHit(center, size, Quaternion.identity, Vector3.down, hit.distance, Color.magenta);
              //  Debug.Break();
                break;
            }

            yield return new WaitForFixedUpdate();

        }

        EndTheFirstSlowMo();

        if (playerSharedData.CurrentStateName != PlayerState.PlayerCanceljump && playerSharedData.CurrentStateName != PlayerState.PlayerSlideState)
        {
            inputChannel.DisableAllButThisInputEvent(inputChannel.SwipeDownOccured);
            tutorialSegmentStateChannel.SendPlaySpecificHint(TutorialGesture.SwipeDown, true);
            inputChannel.SwipeDownOccured.AddListener(EndTheSecondSlowMo);
            tutorialSegmentStateChannel.SendSlowMoRequestEvent();
        }


    }
}
