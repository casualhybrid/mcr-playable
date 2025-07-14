using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GhostAction
{
    public Action actionToPerform;
    public bool isDone;

    public GhostAction(Action action)
    {
        actionToPerform = action;

        isDone = false;
    }
}

[RequireComponent(typeof(Rigidbody))]
public abstract class TutorialGhostSegmentAnimation : MonoBehaviour
{
    [SerializeField] protected readonly Queue<GhostAction> actionsToPerform = new Queue<GhostAction>();
    [SerializeField] protected readonly Dictionary<int, GhostAction> actionsToPerformDictionary = new Dictionary<int, GhostAction>();
    [SerializeField] protected TutorialSegmentData tutorialSegmentData;
    [SerializeField] protected bool repositionOnAnimationSelected = false;

    [ShowIf("repositionOnAnimationSelected")] [SerializeField] protected Vector3 ghostPositionOnAnimSwitched;

    public bool RepositionOnAnimationSelected => repositionOnAnimationSelected;
    public Vector3 GhostPositionOnAnimSwitched => ghostPositionOnAnimSwitched;

    protected Rigidbody rb;
    protected int ghostActionIndex;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public abstract void DoAnimation(Action OnComplete);

    public virtual void RewindGhost(Vector3 targetPosition, Action RewindCompleteCallBack, bool isLocal = false)
    {
        transform.DOKill();
        //   transform.DOMove(targetPosition, tutorialSegmentData.RewindTime).SetEase(Ease.Linear).OnComplete(()=> { RewindCompleteCallBack.Invoke(); });

        Sequence sequence = DOTween.Sequence();
        if (!isLocal)
        {
            sequence.Join(transform.DOMove(targetPosition, TutorialManager.RewindTime).SetEase(Ease.Linear));
        }
        else
        {
            sequence.Join(transform.DOLocalMove(targetPosition, TutorialManager.RewindTime).SetEase(Ease.Linear));
        }
        sequence.AppendInterval(TutorialManager.PauseTimeAfterRewind);
        sequence.AppendCallback(() => { RewindCompleteCallBack.Invoke(); });
    }

    public virtual void StopAllAnimations()
    {
        transform.DOKill();

        ClearGhostActions();
        AddActionsToQueue();
    }

    public virtual void OnAnimationDone()
    {
        ClearGhostActions();
    }

    public virtual void OnAnimationSelected()
    {
        ClearGhostActions();
        AddActionsToQueue();

        if (repositionOnAnimationSelected)
        {
            transform.localPosition = ghostPositionOnAnimSwitched;
        }
    }

    protected void AddActionToQueue(GhostAction action)
    {
        actionsToPerform.Enqueue(action);
        actionsToPerformDictionary.Add(ghostActionIndex++, action);
    }

    protected bool IsSpecificAnimationCompleted(int index)
    {
        GhostAction ghostAction;
        bool success = actionsToPerformDictionary.TryGetValue(index, out ghostAction);

        if (!success)
        {
            throw new System.Exception($"Failed getting ghost action from dictionary while checking if animation was completed {index}");
        }

        return ghostAction.isDone;
    }

    protected void MarkSpecificAnimationAsCompleted(int index)
    {
        GhostAction ghostAction;
        bool success = actionsToPerformDictionary.TryGetValue(index, out ghostAction);

        if (!success)
        {
            throw new System.Exception($"Failed getting ghost action from dictionary while marking animation as completed {index}");
        }

        ghostAction.isDone = true;
    }

    protected bool HasAllAnimationsCompleted()
    {
        bool completed = true;

        for (int i = 0; i < ghostActionIndex; i++)
        {
            if (!IsSpecificAnimationCompleted(i))
            {
                completed = false;
                break;
            }
        }

        return completed;
    }

    protected void ClearGhostActions()
    {
        ghostActionIndex = 0;
        actionsToPerform.Clear();
        actionsToPerformDictionary.Clear();
    }

    protected virtual void AddActionsToQueue()
    {
    }
}