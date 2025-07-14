using UnityEngine;
using System;
using Console = UnityEngine.Console;

public class Examples : MonoBehaviour
{
    private void Awake()
    {
        UnityEngine.Console.Log("Hello World!");
        Console.LogFormat("Hell{0} W{1}rld!", 0, 0);
        UnityEngine.Console.Log("UI", "Hello World!");
        Console.LogFormat("UI", "Hell{0} W{1}rld!", 0, 0);

        UnityEngine.Console.LogWarning("Hello World!");
        Console.LogWarningFormat("Hell{0} W{1}rld!", 0, 0);
        UnityEngine.Console.LogWarning("Audio", "Hello World!");
        Console.LogWarningFormat("Audio", "Hell{0} W{1}rld!", 0, 0);

        UnityEngine.Console.LogError("Hello World!");
        Console.LogErrorFormat("Hell{0} W{1}rld!", 0, 0);
        UnityEngine.Console.LogError("Physics", "Hello World!");
        Console.LogErrorFormat("Physics", "Hell{0} W{1}rld!", 0, 0);

        Console.LogException(new NullReferenceException());
        Console.LogException("Analytics", new NullReferenceException());
    }
}