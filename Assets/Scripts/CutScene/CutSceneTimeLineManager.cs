using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[RequireComponent(typeof(PlayableDirector))]
public class CutSceneTimeLineManager : MonoBehaviour
{
    [SerializeField] private GameEvent cutSceneTimeLineFinished;

    // [SerializeField] private GameEvent onDependenciesLoaded;
    [SerializeField] private GameEvent playerSkeletonHasSpawned;

    [SerializeField] private GameEvent playerCharacterHasSpawned;

    [SerializeField] private PlayerSharedData playerSharedData;

    private PlayableDirector playableDirector;
    private IEnumerable<PlayableBinding> bindings;

    private bool isPlayerSkeletonBindingsSet, isPlayerCharacterBindingsSet;

    private void Awake()
    {
        playableDirector = GetComponent<PlayableDirector>();

        TimelineAsset timelineAsset = playableDirector.playableAsset as TimelineAsset;
        bindings = timelineAsset.outputs;/*.ToArray();*/

        //     playableDirector.RebuildGraph();
    }

    private void OnEnable()
    {
        playableDirector.stopped += HandlePlayableDirectorStopped;
        //  onDependenciesLoaded.TheEvent.AddListener(HandleDependenciesLoaded);
        playerSkeletonHasSpawned.TheEvent.AddListener(SetPlayerCarBindings);
        playerCharacterHasSpawned.TheEvent.AddListener(SetCharacterCarBindings);
    }

    private void OnDisable()
    {
        //  onDependenciesLoaded.TheEvent.RemoveListener(HandleDependenciesLoaded);
        playableDirector.stopped -= HandlePlayableDirectorStopped;
        playerSkeletonHasSpawned.TheEvent.RemoveListener(SetPlayerCarBindings);
        playerCharacterHasSpawned.TheEvent.RemoveListener(SetCharacterCarBindings);

        isPlayerSkeletonBindingsSet = false;
        isPlayerCharacterBindingsSet = false;
    }

    //private void SetBindings(GameEvent gameEvent)
    //{
    //    PlayableBinding playableBinding = bindings[1];

    //    playableDirector.SetGenericBinding(playableBinding.sourceObject, playerSharedData.PlayerAnimator);

    //    playableBinding = bindings[2];

    //    playableDirector.SetGenericBinding(playableBinding.sourceObject, playerSharedData.CharacterAnimator);
    //}

    private void SetPlayerCarBindings(GameEvent gameEvent)
    {
        PlayableBinding playableBinding = bindings.ElementAt(1);

        playableDirector.SetGenericBinding(playableBinding.sourceObject, playerSharedData.PlayerAnimator);

        isPlayerSkeletonBindingsSet = true;

        ReBuildDirectorIfBindingsSet();
    }

    private void SetCharacterCarBindings(GameEvent gameEvent)
    {
          PlayableBinding playableBinding = bindings.ElementAt(2);

          playableDirector.SetGenericBinding(playableBinding.sourceObject, playerSharedData.CharacterAnimator);

          TimelineAsset timelineAsset = playableDirector.playableAsset as TimelineAsset;
          var outputs = timelineAsset.GetOutputTracks().ToArray();

          AnimationTrack animationTrack = outputs[2] as AnimationTrack;
          var clips = animationTrack.GetClips().ToArray();

          AnimationPlayableAsset animationPlayableAsset = clips[0].asset as AnimationPlayableAsset;
          animationPlayableAsset.position = playerSharedData.CharacterAnimator.transform.localPosition;
  
        isPlayerCharacterBindingsSet = true;

        ReBuildDirectorIfBindingsSet();
    }

    private void ReBuildDirectorIfBindingsSet()
    {
        if (isPlayerCharacterBindingsSet && isPlayerSkeletonBindingsSet)
        {
          playableDirector.RebuildGraph();
        }
    }

    private void HandlePlayableDirectorStopped(PlayableDirector director)
    {
        cutSceneTimeLineFinished.RaiseEvent();
    }
}