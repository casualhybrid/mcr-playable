using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public struct IntRange
{
    public int Min;
    public int Max;
    public float Weight;
    

    public IntRange(int min, int max, float weight) : this()
    {
        this.Min = min;
        this.Max = max;
        this.Weight = weight;
    }
}

public struct FloatRange
{
    public float Min;
    public float Max;
    public float Weight;

    public FloatRange(int min, int max, float weight) : this()
    {
        this.Min = min;
        this.Max = max;
        this.Weight = weight;
    }
}

public static class RandomNumGenerator
{

    public static int GetRandomNum(params IntRange[] ranges)
    {
        if (ranges.Length == 0) throw new System.ArgumentException("At least one range must be included.");
        if (ranges.Length == 1) return Random.Range(ranges[0].Max, ranges[0].Min);

        float total = 0f;
        for (int i = 0; i < ranges.Length; i++) total += ranges[i].Weight;

        float r = Random.value;
        float s = 0f;

        int cnt = ranges.Length - 1;
        for (int i = 0; i < cnt; i++)
        {
            s += ranges[i].Weight / total;
            if (s >= r)
            {
                return Random.Range(ranges[i].Max, ranges[i].Min);
            }
        }

        return Random.Range(ranges[cnt].Max, ranges[cnt].Min);
    }

    public static float GetRandomNum(params FloatRange[] ranges)
    {
        if (ranges.Length == 0) throw new System.ArgumentException("At least one range must be included.");
        if (ranges.Length == 1) return Random.Range(ranges[0].Max, ranges[0].Min);

        float total = 0f;
        for (int i = 0; i < ranges.Length; i++) total += ranges[i].Weight;

        float r = Random.value;
        float s = 0f;

        int cnt = ranges.Length - 1;
        for (int i = 0; i < cnt; i++)
        {
            s += ranges[i].Weight / total;
            if (s >= r)
            {
                return Random.Range(ranges[i].Max, ranges[i].Min);
            }
        }

        return Random.Range(ranges[cnt].Max, ranges[cnt].Min);
    }
}
 