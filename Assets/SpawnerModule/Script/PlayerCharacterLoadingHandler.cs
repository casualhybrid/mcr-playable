using System;
using UnityEngine;
using UnityEngine.Scripting;

public class AsyncOperationSpawning<T>
{
    public event Action<T> AssetsLoaded;

    public T Result { get; set; }

    public bool isDone { get; private set; }

    public void SendCompletionEvent()
    {
        isDone = true;
        AssetsLoaded?.Invoke(Result);
    }
}

public class GamePlayPlayerAssets
{
    public GameObject DisplayGameObject;
    public GameObject GamePlayGameObject;
}

[Preserve]
public class PlayerCharacterAssets : GamePlayPlayerAssets
{
    [Preserve]
    public PlayerCharacterAssets()
    { }
}

[CreateAssetMenu(fileName = "PlayerCharacterLoadingHandler", menuName = "ScriptableObjects/AssetsLoading/PlayerCharacterLoadingHandler")]
public class PlayerCharacterLoadingHandler : GamePlayAssetLoadingHandlerBase<PlayerCharacterAssets>
{
}