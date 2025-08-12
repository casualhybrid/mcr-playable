using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using VacuumShaders.CurvedWorld;

[RequireComponent(typeof(CurvedWorld_Controller))]
public class CurvedHandler : MonoBehaviour
{
    [SerializeField] private float minXCurve;
    [SerializeField] private float maxXCurve;
    [SerializeField] private float minYCurve;
    [SerializeField] private float maxYCurve;

    [SerializeField] private float transitionDuration;

    [SerializeField] private EnvironmentChannel environmentChannel;
    [SerializeField] private GameEvent onCutSceneStarted;

    public  CurvedWorld_Controller controller;

    private Sequence curveSequence;

    private readonly Stack<bool> lastKnownActiveStates = new Stack<bool>();

    private void Awake()
    {
        controller = GetComponent<CurvedWorld_Controller>();
        onCutSceneStarted.TheEvent.AddListener(EnableCurvedController);
    }

    private void OnDestroy()
    {
        onCutSceneStarted.TheEvent.RemoveListener(EnableCurvedController);
    }

    private void OnEnable()
    {
        environmentChannel.OnPlayerEnvironmentChanged += HandlePlayerEnvironmentChanged;
    }

    private void OnDisable()
    {
        environmentChannel.OnPlayerEnvironmentChanged -= HandlePlayerEnvironmentChanged;
    }

    private void EnableCurvedController(GameEvent gameEvent)
    { 
       controller.enabled = true;
    }

    public void RestartCurvedWorld()
    {
        if (!controller.enabled)
            return;

        controller.ForceUpdate();
    }

    private void HandlePlayerEnvironmentChanged(Environment env)
    {
        if (curveSequence != null)
        {
            curveSequence.Kill();
        }

        float randXCurve = UnityEngine.Random.Range(minXCurve, maxXCurve);
        float randYCurve = UnityEngine.Random.Range(minYCurve, maxYCurve);

        float curXCurve = controller._V_CW_Bend_X;
        float curYCurve = controller._V_CW_Bend_Y;

        Sequence _seq = DOTween.Sequence();
        _seq.Join(DOTween.To(() => curXCurve, (x) => { controller._V_CW_Bend_X = x; }, randXCurve, transitionDuration));
        _seq.Join(DOTween.To(() => curYCurve, (y) => { controller._V_CW_Bend_Y = y; }, randYCurve, transitionDuration));
    }

    public void DisableCurvedWorld()
    {
        lastKnownActiveStates.Push(controller.enabled);
        controller.enabled = false;
    }

    public void RestoreCurvedWorldToLastState()
    {
        try
        {
            controller.enabled = lastKnownActiveStates.Pop();
        }
        catch
        {
            /*throw new System.Exception($"Last known active state for curved world is empty!" +
                $". Failed to restore curved world state");*/
        }
    }
}