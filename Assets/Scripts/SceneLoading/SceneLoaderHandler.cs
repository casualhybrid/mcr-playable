//using TheKnights.AdsSystem;
using System.Collections;
using TheKnights.SceneLoadingSystem;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "SceneLoaderHandler", menuName = "ScriptableObjects/SceneLoading/SceneLoaderHandler")]
public class SceneLoaderHandler : ScriptableObject
{
    [SerializeField] private UnityEvent OnBeforeResourcesUnloadOnCutSceneEnded;

    [SerializeField] private SceneLoader SceneLoader;

    [SerializeField] private GameEvent sceneLoadingStarted;

    [SerializeField] private GameEvent cutSceneHasStartedgame;

    [Header("Scenes")]
    [SerializeField] private TheScene gamePlayScene;

    [SerializeField] private TheScene uiAdditiveScene;

    [SerializeField] private AdsController adsController;

   // public bool isLoadingFromGameOverPanel { get; private set; }

    private void OnEnable()
    {
        cutSceneHasStartedgame.TheEvent.AddListener(UnloadUIAdditiveScene);
    }

    private void OnDisable()
    {
        cutSceneHasStartedgame.TheEvent.RemoveListener(UnloadUIAdditiveScene);
    }


    public void LoadGamePlayScene()
    {
        adsController.LoadStaticInterstitialAD();
        sceneLoadingStarted.RaiseEvent();
        SceneLoader.LoadTheScene(new TheScene[] { gamePlayScene, uiAdditiveScene }, null, true);
    }

    public ISceneLoadCallBacks SpawnAndSetSceneLoadingCanvas()
    {
       return SceneLoader.SetSceneLoaderMonoBehaviour();
    }

    //public void LoadGamePlaySceneFromGameOverPanel()
    //{
    //    sceneLoadingStarted.RaiseEvent();
    //    SceneLoader.LoadTheScene(new TheScene[] { gamePlayScene, uiAdditiveScene }, null, true);
    //}

    private void UnloadUIAdditiveScene(GameEvent gameEvent)
    {
       CoroutineRunner.Instance.StartCoroutine(UnloadUIAdditiveScene());
    }

    private IEnumerator UnloadUIAdditiveScene()
    {
        yield return new WaitForSeconds(4);

        var handle = SceneLoader.UnloadTheScenes(new TheScene[] { uiAdditiveScene });
        handle.Completed += (h) =>
        {
            OnBeforeResourcesUnloadOnCutSceneEnded.Invoke();
            Resources.UnloadUnusedAssets();
        };
    }
}