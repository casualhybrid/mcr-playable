using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public static class MyExtensions
{
    public static float elapse_time(ref float elapse_time, float speed, float increasePerFrame = 1)
    {
        elapse_time += (Time.fixedDeltaTime / increasePerFrame);

        float my_elapse_time = elapse_time / speed;
        return my_elapse_time;
    }

    public static float NormalElapseTime(ref float elapse_time, float speed)
    {
        elapse_time += Time.deltaTime;
        float my_elapse_time = elapse_time / speed;
        return my_elapse_time;
    }

    public static float lerp_anything(ref float elapse_time, float speed, float start_pos, float end_pos)
    {
        elapse_time += Time.deltaTime;
        float my_elapse_time = elapse_time / speed;
        float lerp = Mathf.Lerp(start_pos, end_pos, my_elapse_time);
        return lerp;
    }

    public static float RangeMapping(float current, float inputMax, float inputMin, float outputMin, float outputMax)
    {
        return ((current - inputMin) / (inputMax - inputMin)) * (outputMax - outputMin) + outputMin; ;
    }

    public static float Lerp3(float a, float b, float c, float t, float midValue)
    {
        if (t <= midValue)
        {
            return Mathf.Lerp(a, b, t * 8);
        }
        else
        {
            return Mathf.Lerp(b, c, t * 4);
        }
    }

    public static float CalculateTime(ref float time)
    {
        time -= Time.deltaTime;
        return time;
    }

    public static float EaseOutExpo(float start, float end, float value)
    {
        end -= start;
        return end * (-Mathf.Pow(2, -10 * value) + 1) + start;
    }

    public static float EaseOutCubic(float value)
    {
        return 1 - Mathf.Pow(1 - value, 3);
    }

    public static float EaseInCubic(float start, float end, float value)
    {
        start -= start;
        return end * value * value * value + start;
    }

    public static float EaseInOutExpo(float start, float end, float value)
    {
        value /= .5f;
        end -= start;
        if (value < 1) return end * 0.5f * Mathf.Pow(2, 10 * (value - 1)) + start;
        value--;
        return end * 0.5f * (-Mathf.Pow(2, -10 * value) + 2) + start;
    }

    public static float EaseInOutQuad(float start, float end, float value)
    {
        value /= .5f;
        end -= start;
        if (value < 1)
            return end * 0.5f * value * value + start;
        value--;
        return -end * 0.5f * (value * (value - 2) - 1) + start;
    }

    public static float EaseInOutCubic(float start, float end, float value)
    {
        value /= .5f;
        end -= start;
        if (value < 1) return end * 0.5f * value * value * value + start;
        value -= 2;
        return end * 0.5f * (value * value * value + 2) + start;
    }

    public static float easeInOutQuad(float x)
    {
        return x < 0.5 ? 2 * x * x : 1 - Mathf.Pow(-2 * x + 2, 2) / 2;
    }

    public static float EaseInQuad(float start, float end, float value)
    {
        end -= start;
        return end * value * value + start;
    }

    public static int GetChildCountDepthLevelOne(this Transform t)
    {
        int count = 0;

        foreach (Transform item in t)
        {
            count++;
        }

        return count;
    }

    public static float easeInQuad(float x)
    {
        return x * x;
    }

    public static Texture2D ResizeTexture(this Texture2D textureToResize, GraphicsFormat graphicsFormat, int width, int height)
    {
        RenderTexture activeRenderTexture = RenderTexture.active;
        RenderTexture renderTexture = new RenderTexture(width, height, 24, graphicsFormat);
        Graphics.Blit(textureToResize, renderTexture);
        RenderTexture.active = renderTexture;
        Texture2D texture2D = new Texture2D(width, height, graphicsFormat, TextureCreationFlags.None);
        texture2D.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        RenderTexture.active = activeRenderTexture;
        UnityEngine.Object.Destroy(renderTexture);
        return texture2D;
    }

    public static uint Concat(this uint a, uint b)
    {
        uint
          pow = 1;

        while (pow < b)
        {
            pow = ((pow << 2) + pow) << 1;
            a = ((a << 2) + a) << 1;
        }

        return a + b;
    }
}