using System;
using UnityEngine;

public class GameBenchmark : MonoBehaviour
{
    public static event Action<float> OnGameBenchmarkDone;

    [Tooltip("The duration of benchmark in seconds")]
    [SerializeField] private float benchmarkDuration = 30;

    [SerializeField] private GameEvent cutsceneStarted;

    private float averageFPS;
    private float numberOfFrames;

    private bool initialized;
    private float elapsedBenchmarkDuration;

    private void Awake()
    {
        cutsceneStarted.TheEvent.AddListener(Initialize);
    }

    private void OnDestroy()
    {
        cutsceneStarted.TheEvent.RemoveListener(Initialize);
    }

    private void Initialize(GameEvent gameEvent = null)
    {
        initialized = true;
        this.enabled = true;
    }

    private void Update()
    {
        if (!initialized)
            return;

        float currentFPS = 1f / (Time.deltaTime);

        float x = (averageFPS * numberOfFrames) + currentFPS;
        numberOfFrames++;
        averageFPS = x / numberOfFrames;

        elapsedBenchmarkDuration += Time.deltaTime;

        if(elapsedBenchmarkDuration >= benchmarkDuration)
        {
            UnityEngine.Console.Log($"Game benchmark completed with average FPS {averageFPS}");
            OnGameBenchmarkDone?.Invoke(averageFPS);

            Destroy(this.gameObject);
        }
    }
}