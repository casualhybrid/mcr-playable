using UnityEngine;

public static class LerpExtensions
{
    public static float EaseOutSineLerp(float start, float end, float t)
    {
        float x = Mathf.Sin((t * Mathf.PI) / 2);


        return ((end - start) * x) + start;
    }

    public static float EaseOutSine(float t)
    {
        return Mathf.Sin((t * Mathf.PI) / 2);
    }

    public static float EaseInSine(float t)
    {
        return 1 - Mathf.Cos((t * Mathf.PI) / 2);
    }

    public static float EaseInSineLerp(float start, float end, float t)
    {
        float x = 1 - Mathf.Cos((t * Mathf.PI) / 2);

        return ((end - start) * x) + start;
    }
}