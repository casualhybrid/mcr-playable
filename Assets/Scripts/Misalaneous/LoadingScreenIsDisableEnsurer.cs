using System.Collections;
using System.Collections.Generic;
using TheKnights.SceneLoadingSystem;
using UnityEngine;
using UnityEngine.Events;

public class LoadingScreenIsDisableEnsurer : MonoBehaviour
{
    [SerializeField] private GameEvent onLoadingComplete;
    [SerializeField] private SceneLoader sceneLoader;
    [SerializeField] private UnityEvent onSceneLoaderIsInActiveEnsured;

    private void OnEnable()
    {
        if(!sceneLoader.IsTheLoadingCanvasActive)
        {
            onSceneLoaderIsInActiveEnsured.Invoke();
        }
        else
        {
            onLoadingComplete.TheEvent.AddListener(RaiseSceneLoaderInActiveEnsured);
        }
    }

    private void OnDisable()
    {
       onLoadingComplete.TheEvent.RemoveListener(RaiseSceneLoaderInActiveEnsured);
    }


    private void RaiseSceneLoaderInActiveEnsured(GameEvent gameEvent)
    {
        onSceneLoaderIsInActiveEnsured.Invoke();
    }
}
