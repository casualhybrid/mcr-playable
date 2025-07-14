using System.Collections;
using UnityEngine;

public class PoolPrewarmer : MonoBehaviour
{
    [SerializeField] private ObstaclePoolSO obstaclePoolSO;
    [SerializeField] private GeneralGameObjectPool coinPool;
    [SerializeField] private GeneralGameObjectPool doubleCoinPool;

    [SerializeField] private GameEvent onPoolsWarmedUp;

    private IEnumerator Start()
    {
        yield return null;

        Coroutine obstacleRoutine = obstaclePoolSO.PrewarmWithDelays(5);
        Coroutine coinRoutine = coinPool.PrewarmWithDelays(100);
        Coroutine doubleCoinRoutine = doubleCoinPool.PrewarmWithDelays(100 / 2);

        yield return obstacleRoutine;
        yield return coinRoutine;
        yield return doubleCoinRoutine;

        onPoolsWarmedUp.RaiseEvent();
    }
}