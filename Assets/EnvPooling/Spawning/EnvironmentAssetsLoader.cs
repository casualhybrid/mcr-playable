using System;
using UnityEngine;

[CreateAssetMenu(fileName = "EnvironmentAssetsLoader", menuName = "ScriptableObjects/AssetsLoading/EnvironmentAssetsLoader")]
public class EnvironmentAssetsLoader : ScriptableObject
{
    public event Action<EnviornmentSO> onEnvironmentAssetLoaded;

    [SerializeField] private EnvironmentDataBase environmentDataBase;

    public void LoadEnvironmentAssets(Environment envToLoad)
    {
        string path = environmentDataBase.GetLoadingPathForEnvironment(envToLoad);
        ResourceRequest resourceRequest = Resources.LoadAsync<EnviornmentSO>(path);
        resourceRequest.completed += (handle) =>
        {
            EnviornmentSO environmentLoadedAsset = resourceRequest.asset as EnviornmentSO;

            if (environmentLoadedAsset == null)
                throw new System.Exception($"Failed to load environment asset from disk {envToLoad.name}");

            onEnvironmentAssetLoaded?.Invoke(environmentLoadedAsset);
        };
    }
}