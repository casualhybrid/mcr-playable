using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RequestPlayerGotHitChannel", menuName = "ScriptableObjects/RequestPlayerGotHitChannel")]
public class RequestPlayerGotHitChannel : ScriptableObject
{
    public event Action OnRequestPlayerToGetStumble;

   public void RaiseRequestToStumblePlayer()
    {
        OnRequestPlayerToGetStumble?.Invoke();
    }

}
