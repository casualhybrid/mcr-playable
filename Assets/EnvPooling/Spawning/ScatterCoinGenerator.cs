using System.Collections.Generic;
using TheKnights.SaveFileSystem;
using UnityEngine;

public class ScatterCoinGenerator : MonoBehaviour
{
    [Tooltip("The radius of the circle in which the coins will be scattered in")]
    [SerializeField] public float radius;

    [SerializeField] public float zSpawnOffset;
    [SerializeField] public float gameTimeScalePercentToConsiderForCoinsOrigin;
    [SerializeField] public float distanceToFirstBounceSpeedWeightage = 1;

    [SerializeField] public GameObject coin;

    [SerializeField] public int destructibleVehicleCoinsMin;
    [SerializeField] public int destructibleVehicleCoinsMax;

    [SerializeField] public int nonDestructibleVehicleCoinsMin;
    [SerializeField] public int nonDestructibleVehicleCoinsMax;

    [SerializeField] public PlayerSharedData playerShatedData;

    [SerializeField] public GameEvent onDestructibleDestroyed;
    [SerializeField] public GameEvent onNonDestructibleDestroyed;

    [SerializeField] public float coinJumpHeightMin;
    [SerializeField] public float coinJumpHeightMax;
    [SerializeField] public float coinJumpSpeed;
    [SerializeField] public float bouncedSpeedReductionNormalizedPercentAfterFirstBounce;

    [SerializeField] public float heightLossPerBounceNormalizedPercent;

    [SerializeField] public float minCoinJumpDelay;
    [SerializeField] public float maxCoinJumpDelay;

    [Tooltip("Doesn't make any sense I know")]
    [SerializeField] public float bounceCompletionFrequency;

    [SerializeField] public GeneralGameObjectPool coinPool;
    [SerializeField] private SaveManager saveManager;

    private readonly Stack<ScatterCoinBatch> scatterCoinBatches = new Stack<ScatterCoinBatch>();

    private void Awake()
    {
        onDestructibleDestroyed.TheEvent.AddListener(GenerateDestructibleScatterCoins);
        onNonDestructibleDestroyed.TheEvent.AddListener(GenerateNonDestructibleScatterCoins);
    }

    private void OnDestroy()
    {
        onDestructibleDestroyed.TheEvent.RemoveListener(GenerateDestructibleScatterCoins);
        onNonDestructibleDestroyed.TheEvent.RemoveListener(GenerateNonDestructibleScatterCoins);
    }

    public void GenerateDestructibleScatterCoins(GameEvent gameEvent)
    {
        if (!saveManager.MainSaveFile.TutorialHasCompleted)
            return;

        ScatterCoinBatch scatterCoinBatch = CreateScatterBatchInstance();
        scatterCoinBatch.GenerateDestructibleScatterCoins(gameEvent);
    }

    public void GenerateNonDestructibleScatterCoins(GameEvent gameEvent)
    {
        if (!saveManager.MainSaveFile.TutorialHasCompleted)
            return;

        ScatterCoinBatch scatterCoinBatch = CreateScatterBatchInstance();
        scatterCoinBatch.GenerateNonDestructibleScatterCoins(gameEvent);
    }

    public void BatchHasFinished(ScatterCoinBatch scatterCoinBatch)
    {
        scatterCoinBatches.Push(scatterCoinBatch);
    }

    private ScatterCoinBatch CreateScatterBatchInstance()
    {
        ScatterCoinBatch scatterBatch;

        if (scatterCoinBatches.TryPop(out scatterBatch))
        {
            scatterBatch.Initialize(this);
            return scatterBatch;
        }

        GameObject scatterGameObject = new GameObject("ScatterObj", typeof(ScatterCoinBatch));
        ScatterCoinBatch scatterCoinBatch = scatterGameObject.GetComponent<ScatterCoinBatch>();
        scatterCoinBatch.Initialize(this);
        return scatterCoinBatch;
    }
}