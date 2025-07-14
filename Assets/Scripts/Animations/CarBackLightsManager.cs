using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CarBackLightsManager : MonoBehaviour
{
    [SerializeField] private GameEvent playerHasCrashed;
    [SerializeField] private GameEvent playerHasRevived;
    [SerializeField] private TutorialSegmentStateChannel tutorialSegmentStateChannel;

    [SerializeField] private float timeToGoMaxBright;

    private List<Light> backLights;

    private float maxLightsIntensity;

    private void Awake()
    {
        var lights = GetComponentsInChildren<Light>(true);

        if (lights == null || lights.Length == 0)
        {
            this.gameObject.SetActive(false);
            return;
        }

        backLights = lights.ToList();

        playerHasCrashed.TheEvent.AddListener(TurnOnBackLights);
        playerHasRevived.TheEvent.AddListener(TurnOffBackLights);
        tutorialSegmentStateChannel.OnRewind += HandleTutorialRewind;
    }

    private void Start()
    {
        for (int i = 0; i < backLights.Count; i++)
        {
            Light light = backLights[i];
            light.enabled = false;
            maxLightsIntensity = light.intensity;
        }
    }

    private void OnDestroy()
    {
        playerHasCrashed.TheEvent.RemoveListener(TurnOnBackLights);
        playerHasRevived.TheEvent.RemoveListener(TurnOffBackLights);
        tutorialSegmentStateChannel.OnRewind -= HandleTutorialRewind;
    }

    private void TurnOnBackLights(GameEvent gameEvent)
    {
        float intensity = 0;

        DOTween.To(() => intensity, (x) =>
        {
            for (int i = 0; i < backLights.Count; i++)
            {
                Light light = backLights[i];
                light.enabled = true;
                light.intensity = x;
            }
        }, maxLightsIntensity, timeToGoMaxBright);
    }

    private void HandleTutorialRewind()
    {
        TurnOffBackLights(null);
    }
    private void TurnOffBackLights(GameEvent gameEvent)
    {
        for (int i = 0; i < backLights.Count; i++)
        {
            Light light = backLights[i];
            light.enabled = false;
            light.intensity = 0;
        }
    }
}