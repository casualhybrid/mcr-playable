using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegacyAnimationPlayer : MonoBehaviour
{
    public void PlayDefaultAnimation(Animation animation)
    {
        animation.Play();
    }
}
