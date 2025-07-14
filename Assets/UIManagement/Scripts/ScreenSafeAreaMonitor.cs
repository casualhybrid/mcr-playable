using System;
using UnityEngine;

public class ScreenSafeAreaMonitor : Singleton<ScreenSafeAreaMonitor>
{
    public static event Action OnSafeAreaChanged;
    int lastKnownHeightToSafeHeightDiff;

    private void Update()
    {
        Rect safeAreaRect = Screen.safeArea;

        int totalHeight = Display.main.renderingHeight;
        int safeAreaHeight = (int)safeAreaRect.height;

        int h = totalHeight - safeAreaHeight;

        if(h != lastKnownHeightToSafeHeightDiff)
        {
            UnityEngine.Console.Log("Safe Area Changed");
            OnSafeAreaChanged?.Invoke();
        }

        lastKnownHeightToSafeHeightDiff = h;
    }
}