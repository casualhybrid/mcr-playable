using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace TheKnights.SceneLoadingSystem
{
    [CreateAssetMenu(fileName = "SceneLoader", menuName = "ScriptableObjects/SceneLoading/SceneLoader", order = 1)]
    public class SceneLoader : ScriptableObject
    {
        [Tooltip("Auto disables the loading canvas object when scene has finished loading")]
        [SerializeField] private bool turnOffLoadingCanvasOnLoadingComplete;

        [Tooltip("Pauses right before loading the scene and waits for the execute pending loading tasks signal to proceed (Useful for displaying Ads at loading screen)")]
        [SerializeField] private bool pauseSceneLoadingAndListenForSignal = true;

        [Tooltip("The gameObject resposible for executing scene loading.Should have LoadTheSceneMonoBehaviour attached to it")]
        [SerializeField] private GameObject theSceneLoadingObject;

        [SerializeField] private GameEvent loadingScreenAppeared;
        [SerializeField] private GameEvent loadingComplete;

        // The instantiated loading object in the scene
        private GameObject sceneLoadingObjectSpawned;

        // Contains all "ADDITIVE" scenes that are currently loaded
        // private Dictionary<TheScene, AsyncOperationHandle> loadedScenesDictionary = new Dictionary<TheScene, AsyncOperationHandle>();

        // The MonoBehaviour responsible for executiong coroutines and loading scenes
        private LoadTheSceneMonoBehaviour loader;

        public ISceneLoadCallBacks loadingCanvasListener { get; private set; }

        private Action pendingSceneLoadingOperation;

        public bool IsTheLoadingCanvasActive
        {
            get {

                if (loader == null)
                    return false;

                Canvas canvas = loader.GetTheLoadingCanvas;

                if (canvas == null)
                    return false;

                return canvas.enabled;
            }
        }

        private IEnumerator WaitForFrameAndExecute(Action actionToPerform)
        {
            yield return null;
            actionToPerform();
        }

        /// <summary>
        /// Loads the scenes according to there configuration
        /// </summary>
        /// <param name="theScene">Array of scenes to be loaded</param>
        /// <param name="listeners">Collection of listeners listening for scene load operation events</param>
        /// <param name="enableLoadingCanvas">Should the loading canvas appear?</param>
        public AsyncSceneOperationHandle LoadTheScene(TheScene[] theScene, List<ISceneLoadCallBacks> listeners = null, bool enableLoadingCanvas = false, bool bypassPauseBeforeLoading = false)
        {
            Application.backgroundLoadingPriority = ThreadPriority.High;

            AsyncSceneOperationHandle sceneHandle = new AsyncSceneOperationHandle();
            // Sets scene loading prefab reference if null
            SetSceneLoaderMonoBehaviour();

            Action action = () =>
            {
                if (theScene == null || theScene.Length == 0)
                {
                    HandleSceneOperationCompletion(true, new System.Exception("No Scene To Load"), sceneHandle, listeners);
                    return;
                }

                LinkedList<TheScene> scenesToBeLoadedCollection = new LinkedList<TheScene>();

                for (int i = 0; i < theScene.Length; i++)
                {
                    TheScene scene = theScene[i];

                    if (scene.GetSceneLoadMode == UnityEngine.SceneManagement.LoadSceneMode.Single)
                    {
                        // The additive scenes will automatically be unloaded by addressable system
                        //   loadedScenesDictionary?.Clear();

                        scenesToBeLoadedCollection.AddFirst(scene);
                    }
                    else
                    {
                        scenesToBeLoadedCollection.AddLast(scene);
                    }

                    //if (loadedScenesDictionary.ContainsKey(scene))
                    //{
                    //    UnityEngine.Console.LogWarning("Trying to load an already loaded scene " + scene.name);
                    //    continue;
                    //}
                }

                if (scenesToBeLoadedCollection.Count == 0)
                {
                    UnityEngine.Console.LogWarning("No scenes were loaded");
                    HandleSceneOperationCompletion(true, new System.Exception("No Scene Operations To Load"), sceneHandle, listeners);
                    return;
                }

                if (loader.GetTheLoadingCanvas != null)
                {
                    loader.GetTheLoadingCanvas.enabled = enableLoadingCanvas;
                }

                // We should add the main loading script as listener here
                if (enableLoadingCanvas)
                {
                    listeners = AddMainSceneLoaderAsListener(listeners);

                    if (pauseSceneLoadingAndListenForSignal && !bypassPauseBeforeLoading)
                    {
                        pendingSceneLoadingOperation = () => { loader.StartCoroutine(LoadTheSceneRoutine(scenesToBeLoadedCollection, sceneHandle, listeners)); };
                        loadingScreenAppeared.RaiseEvent();
                        return;
                    }

                    loadingScreenAppeared.RaiseEvent();
                }

                loader.StartCoroutine(LoadTheSceneRoutine(scenesToBeLoadedCollection, sceneHandle, listeners));
            };

            loader.StartCoroutine(WaitForFrameAndExecute(action));

            return sceneHandle;
        }

        /// <summary>
        /// Execute any pending loading operations which were paused
        /// </summary>
        public void ExecutePendingSceneLoadingOperations()
        {
            pendingSceneLoadingOperation?.Invoke();
            pendingSceneLoadingOperation = null;
        }

        // Registers the main loading script for callbacks
        private List<ISceneLoadCallBacks> AddMainSceneLoaderAsListener(List<ISceneLoadCallBacks> listeners)
        {
            if (listeners == null)
            {
                listeners = new List<ISceneLoadCallBacks>() { loadingCanvasListener };
            }
            else
            {
                listeners.Add(loadingCanvasListener);
            }

            return listeners;
        }

        /// <summary>
        /// Disables the presistent loading canvas in the scene
        /// </summary>
        public void TurnOffLoadingCanvas()
        {
            if (loader == null)
            {
                loader = FindObjectOfType<LoadTheSceneMonoBehaviour>();
            }

            if (loader.GetTheLoadingCanvas != null)
            {
                loadingComplete.RaiseEvent();
                loader.GetTheLoadingCanvas.enabled = false;

                // UnityEngine.Console.Log("Disabling Canbas");
            }
            else
            {
                //  UnityEngine.Console.LogWarning("LoadTheSceneMonoBehvaiour Is Null");
            }
        }

        /// <summary>
        /// Unloads every scene currently loaded
        /// </summary>
        public AsyncSceneOperationHandle UnloadAllScenes(List<ISceneLoadCallBacks> listeners = null)
        {
            return null;
            //  return UnloadTheScenes(loadedScenesDictionary.Keys.ToArray(), listeners);
        }

        /// <summary>
        /// Unloads the specified scenes from the memory
        /// </summary>
        /// <param name="theScene">Array of scenes to be unloaded</param>
        public AsyncSceneOperationHandle UnloadTheScenes(TheScene[] theScenes, List<ISceneLoadCallBacks> listeners = null)
        {
            AsyncSceneOperationHandle sceneHandle = new AsyncSceneOperationHandle();

            SetSceneLoaderMonoBehaviour();

            Action actionToPerform = () =>
            {
                if (theScenes == null || theScenes.Length == 0)
                {
                    HandleSceneOperationCompletion(true, new System.Exception("No Scene Operations To Unload"), sceneHandle, listeners);
                    return;
                }

                //  Dictionary<AsyncOperationHandle, TheScene> asyncOperationHandleDictionary = new Dictionary<AsyncOperationHandle, TheScene>();

                //  for (int i = 0; i < theScenes.Length; i++)
                //  {
                //  AsyncOperationHandle handle;
                //   TheScene scene = theScenes[i];
                //loadedScenesDictionary.TryGetValue(scene, out handle);

                //if (!handle.IsValid())
                //{
                //    if (loadedScenesDictionary.ContainsKey(scene))
                //    {
                //        loadedScenesDictionary.Remove(scene);
                //    }

                //    continue;
                //}

                //   AsyncOperationHandle unLoadHandle = Addressables.UnloadSceneAsync(handle, false);
                // asyncOperationHandleDictionary.Add(unLoadHandle, scene);
                //  };

                loader.StartCoroutine(UnloadTheSceneRoutine(theScenes, sceneHandle, listeners));
            };
            loader.StartCoroutine(WaitForFrameAndExecute(actionToPerform));
            return sceneHandle;
        }

        private IEnumerator UnloadTheSceneRoutine(TheScene[] scenesToUnload, AsyncSceneOperationHandle scenehandle, List<ISceneLoadCallBacks> listeners = null)
        {
            yield return null;

            bool error = false;
            System.Exception exception = null;

            for (int i = 0; i < scenesToUnload.Length; i++)
            {
                TheScene scene = scenesToUnload[i];
                AsyncOperation handle = SceneManager.UnloadSceneAsync(scene.GetSceneKey);

                while (!handle.isDone)
                {
                    yield return null;
                }

                //if (handle.Status == AsyncOperationStatus.Failed || handle.Status == AsyncOperationStatus.None)
                //{
                //    UnityEngine.Console.LogWarning("Failed to unload scene because of " + handle.OperationException);
                //    exception = handle.OperationException;
                //    error = true;
                //}
                //else
                //{
                //    UnityEngine.Console.Log("Unloaded Scene " + scene.GetSceneKey);
                //}

                //if (loadedScenesDictionary.ContainsKey(scene))
                //{
                //    loadedScenesDictionary.Remove(scene);
                //}

                //   Addressables.Release(handle);
            }

            HandleSceneOperationCompletion(error, exception, scenehandle, listeners);
        }

        private IEnumerator LoadTheSceneRoutine(LinkedList<TheScene> scenesToBeLoadedCollection, AsyncSceneOperationHandle scenehandle, List<ISceneLoadCallBacks> listeners = null)
        {
            yield return null;

            InvokeSceneLoadingStartedOnListeners(listeners);

            float totalProgress, Progress, loadingProgress;
            totalProgress = Progress = loadingProgress = 0;

            bool error = false;
            System.Exception exception = null;

            foreach (TheScene scene in scenesToBeLoadedCollection)
            {
                //  AsyncOperationHandle handle = Addressables.LoadSceneAsync(scene.GetSceneKey, scene.GetSceneLoadMode);
                AsyncOperation handle = SceneManager.LoadSceneAsync(scene.GetSceneKey, scene.GetSceneLoadMode);

                while (!handle.isDone)
                {
                    yield return null;
                    Progress = (scene.IncludeLoadingProgress) ? handle.progress / scenesToBeLoadedCollection.Count : 0;
                    loadingProgress = Progress + totalProgress;

                    if (listeners != null)
                    {
                        for (int k = 0; k < listeners.Count; k++)
                        {
                            listeners[k].OnLoadingProgressChanged(loadingProgress);
                        }
                    }

                    scenehandle.PercentComplete = loadingProgress;
                }

                totalProgress += Progress;

                //if (handle.Status == AsyncOperationStatus.Failed || handle.Status == AsyncOperationStatus.None)
                //{
                //    UnityEngine.Console.LogWarning("Failed to load scene because of " + handle.OperationException);
                //    exception = handle.OperationException;
                //    error = true;
                //}
                //else if (scene.GetSceneLoadMode == UnityEngine.SceneManagement.LoadSceneMode.Additive)
                //{
                //    loadedScenesDictionary.Add(scene, handle);
                //}
            }

            HandleSceneOperationCompletion(error, exception, scenehandle, listeners);

            if (turnOffLoadingCanvasOnLoadingComplete)
            {
                TurnOffLoadingCanvas();
            }

            Application.backgroundLoadingPriority = ThreadPriority.BelowNormal;
        }

        private void InvokeSceneLoadingStartedOnListeners(List<ISceneLoadCallBacks> listeners)
        {
            if (listeners != null)
            {
                for (int k = 0; k < listeners.Count; k++)
                {
                    //   FindObjectOfType<LoadTheSceneMonoBehaviour>().SceneOperationCompleted();
                    listeners[k].SceneLoadingStarted();
                }
            }
        }

        private void HandleSceneOperationCompletion(bool error, System.Exception exception, AsyncSceneOperationHandle scenehandle, List<ISceneLoadCallBacks> listeners = null)
        {
            if (error)
            {
                scenehandle.Status = AsyncOperationStatus.Failed;
                scenehandle.OperationException = exception;

                UnityEngine.Console.LogWarning("Loading Scene Operation Has Failed");

                if (listeners != null)
                {
                    for (int k = 0; k < listeners.Count; k++)
                    {
                        listeners[k].SceneOperationFailed();
                        //FindObjectOfType<LoadTheSceneMonoBehaviour>().SceneOperationFailed();
                    }
                }
            }
            else
            {
                scenehandle.Status = AsyncOperationStatus.Succeeded;

                if (listeners != null)
                {
                    for (int k = 0; k < listeners.Count; k++)
                    {
                        //   FindObjectOfType<LoadTheSceneMonoBehaviour>().SceneOperationCompleted();
                        listeners[k].SceneOperationCompleted();
                    }
                }
            }

            scenehandle.IsDone = true;
            scenehandle.SendCompletionEvent();
        }

        public ISceneLoadCallBacks SetSceneLoaderMonoBehaviour()
        {
            if (sceneLoadingObjectSpawned == null)
            {
                sceneLoadingObjectSpawned = Instantiate(theSceneLoadingObject);
            }

            if (loader == null)
            {
                loader = sceneLoadingObjectSpawned.GetComponent<LoadTheSceneMonoBehaviour>();
                loadingCanvasListener = loader;
            }

            
            if (loader == null)
            {
                throw new System.Exception("Please attach LoadTheSceneMonoBehaviour on the loading prefab");
            }

            return loadingCanvasListener;
        }
    }
}