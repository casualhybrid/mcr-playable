using UnityEngine;

public class UIEffectsHandlerGamePlay : MonoBehaviour
{
    [SerializeField] private UIEffectsChannel uIEffectsChannel;
    [SerializeField] private PlayerSharedData playerSharedData;
    [SerializeField] private GameEvent diamondHasBeenPickedUp;
    [SerializeField] private GameEvent boostHasBeenPickedUp;
    [SerializeField] private GameEvent destructibleEnemyDestroyed;
    [SerializeField] private GameEvent nonDestructibleEnemyDestroyed;
    [SerializeField] private CameraData cameraData;

    private float timeWhenLastDestructionCoinEffectWasShown;

    private void Awake()
    {
        SubscribeEvents();
    }

    private void OnDestroy()
    {
        DeSubscribeEvents();
    }

    private void SubscribeEvents()
    {
        // diamondHasBeenPickedUp.TheEvent.AddListener(ShowDiamondPickedUpEffect);
        boostHasBeenPickedUp.TheEvent.AddListener(ShowBoostPickedUpEffect);
        destructibleEnemyDestroyed.TheEvent.AddListener(ShowCoinAddEffect);
        nonDestructibleEnemyDestroyed.TheEvent.AddListener(ShowCoinAddEffect);
    }

    private void DeSubscribeEvents()
    {
        //diamondHasBeenPickedUp.TheEvent.RemoveListener(ShowDiamondPickedUpEffect);
        boostHasBeenPickedUp.TheEvent.RemoveListener(ShowBoostPickedUpEffect);
        destructibleEnemyDestroyed.TheEvent.RemoveListener(ShowCoinAddEffect);
        nonDestructibleEnemyDestroyed.TheEvent.RemoveListener(ShowCoinAddEffect);

    }

    //private void ShowDiamondPickedUpEffect(GameEvent gameEvent)
    //{
    //    Vector2 screenPointPlayer = Camera.main.WorldToScreenPoint(playerSharedData.PlayerTransform.position);
    //    uIEffectsChannel.RaiseItemAddEffectRequest(screenPointPlayer, InventoryPositions.GetAnimDataForItem("AccountDiamonds").screenLocation, "AccountDiamonds", 1, true);
    //}

    private void ShowCoinAddEffect(GameEvent gameEvent)
    {
        float timeAtFrameBeg = Time.time;

        if(timeAtFrameBeg - timeWhenLastDestructionCoinEffectWasShown < .4f)
        {
          //  UnityEngine.Console.LogWarning("Ignoring Show Coin Request. Too Quick");
            return;
        }

        timeWhenLastDestructionCoinEffectWasShown = timeAtFrameBeg;
        Vector2 screenPointPlayer = cameraData.TheMainCamera.WorldToScreenPoint(playerSharedData.PlayerTransform.position);
        uIEffectsChannel.RaiseItemAddEffectRequest(screenPointPlayer, InventoryPositions.GetAnimDataForItem("AccountCoins").screenLocation, "AccountCoins", 1, true);
    }

    private void ShowBoostPickedUpEffect(GameEvent gameEvent)
    {
        Vector2 screenPointPlayer = cameraData.TheMainCamera.WorldToScreenPoint(playerSharedData.PlayerTransform.position);
        uIEffectsChannel.RaiseItemAddEffectRequest(screenPointPlayer, InventoryPositions.GetAnimDataForItem("GameBoost").screenLocation, "GameBoost", 1, true);
    }
}