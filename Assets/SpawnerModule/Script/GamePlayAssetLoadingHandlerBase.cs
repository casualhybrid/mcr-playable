using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class GamePlayAssetLoadingHandlerBase<T> : ScriptableObject where T : GamePlayPlayerAssets
{
    protected static readonly Dictionary<string, T> loadedAssetsDictionary = new Dictionary<string, T>();

    protected void OnEnable()
    {
        SceneManager.activeSceneChanged += HandleSceneChanged;
    }

    protected void OnDisable()
    {
        SceneManager.activeSceneChanged -= HandleSceneChanged;

        // Also unload assets when this object gets disabled
        UnloadAllAssets();
    }

    // Unload the assets upon scene changed as this scriptable object might not have gone disabled upon scene change
    protected void HandleSceneChanged(Scene from, Scene To)
    {
        UnloadAllAssets();
    }

    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="U"></typeparam>
    /// <param name="configData"></param>
    /// <param name="keyIndex"></param>
    /// <returns></returns>
    public virtual AsyncOperationSpawning<T> LoadAssets<U>(U configData) where U : IBasicAssetInfo
    {
        string keyIndex = configData.GetName;

        AsyncOperationSpawning<T> handle = new AsyncOperationSpawning<T>();

        // If the asset already exists in memory don't attempt to reload it
        var alreadyExistingAsset = CheckAndGetAlreadyLoadedAsset(keyIndex);
        if (alreadyExistingAsset != null)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                handle.Result = alreadyExistingAsset;
                handle.SendCompletionEvent();
            });
        
            return handle;
        }

        // Load the asset from disk
        int operationsCompleted = 0;
        T T = Activator.CreateInstance<T>();

        ResourceRequest resourceRequestDisplay = Resources.LoadAsync(configData.GetLoadingKeyDisplay);
        ResourceRequest resourceRequestGamePlay = Resources.LoadAsync(configData.GetLoadingKeyGamePlay);

        resourceRequestDisplay.completed += (asnycOperation) =>
        {
            operationsCompleted++;
            T.DisplayGameObject = resourceRequestDisplay.asset as GameObject;
            CheckForOperationCompletion(operationsCompleted, T, handle, keyIndex);
        };

        resourceRequestGamePlay.completed += (asnycOperation) =>
        {
            operationsCompleted++;
            T.GamePlayGameObject = resourceRequestGamePlay.asset as GameObject;
            CheckForOperationCompletion(operationsCompleted, T, handle, keyIndex);
        };

        return handle;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public T GetLoadedPlayerAssets(string key)
    {
        T T;

        loadedAssetsDictionary.TryGetValue(key, out T);

        if (T == null)
        {
            UnityEngine.Console.LogWarning($"{T.GetType()} of key {key} was requested but it's not loaded. This shouldn't happen");
        }

        return T;
    }


    /// <summary>
    ///
    /// </summary>
    /// <param name="key"></param>
    //public void UnloadAssetFromMemory(int key)
    //{
    //    T T;

    //    loadedAssetsDictionary.TryGetValue(key, out T);

    //    if (T == null)
    //    {
    //        UnityEngine.Console.LogWarning($"Attempt to unload asset of type {T.GetType()} was made but the key {key} isn't present");
    //        return;
    //    }

    //    if (T.DisplayGameObject != null)
    //    {
    //        Resources.UnloadAsset(T.DisplayGameObject as UnityEngine.Object);
    //    }

    //    if (T.GamePlayGameObject != null)
    //    {
    //        Resources.UnloadAsset(T.GamePlayGameObject as UnityEngine.Object);
    //    }

    //    loadedAssetsDictionary.Remove(key);
    //}

    /// <summary>
    ///
    /// </summary>
    public void UnloadAllAssets()
    {
        // So the collection won't be modified while enumerating
        //var loadedKeys = loadedAssetsDictionary.Keys.ToArray();

        //for (int i = 0; i < loadedKeys.Length; i++)
        //{
        //    UnloadAssetFromMemory(loadedKeys[i]);
        //}

   
        loadedAssetsDictionary.Clear();
    }

    protected virtual void CheckForOperationCompletion(int noOfOperationsComp, T T, AsyncOperationSpawning<T> handle, string keyIndex)
    {
        if (noOfOperationsComp != 2)
            return;

        if (!loadedAssetsDictionary.ContainsKey(keyIndex))
        {
            loadedAssetsDictionary.Add(keyIndex, T);
        }
        else
        {
            loadedAssetsDictionary[keyIndex] = T;
        }

        handle.Result = T;
        handle.SendCompletionEvent();
    }

    private T CheckAndGetAlreadyLoadedAsset(string key)
    {
        T T;

        loadedAssetsDictionary.TryGetValue(key, out T);

        // Make sure that key exists along with all the assets
        if (T != null && T.DisplayGameObject != null && T.GamePlayGameObject != null)
        {
            return T;
        }
        else
        {
            return null;
        }
    }
}