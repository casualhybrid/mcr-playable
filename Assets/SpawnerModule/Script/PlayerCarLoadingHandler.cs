using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class PlayerCarAssets : GamePlayPlayerAssets
{
    [Preserve]
    public PlayerCarAssets()
    {

    }
}

[CreateAssetMenu(fileName = "PlayerCarLoadingHandler", menuName = "ScriptableObjects/AssetsLoading/PlayerCarLoadingHandler")]
public class PlayerCarLoadingHandler : GamePlayAssetLoadingHandlerBase<PlayerCarAssets>
{
   
}