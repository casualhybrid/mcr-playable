using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathExtensions
{
    public static float ReverseNormalize(float x, float max, float min)
    {
        return (max - min) / (max - x) ;

    }

    public static float ValuesBetweenPointsBasedOnNormalizedValue(float normalizedValue, float min, float max)
    {
        return min + ((max - min) * normalizedValue);
    }

    public static float GetAmountByPercent(float value, float percent)
    {
        return (value * percent) / 100f;
    }
}
