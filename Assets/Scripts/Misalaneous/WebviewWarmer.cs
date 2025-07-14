using System;
using System.Collections;
using UnityEngine;

public static class WebviewWarmer
{
    public static bool IsOpertaionCompleted => isWarmedUP || isExceptionOccurred;
    public static bool isWarmedUP { get; private set; } = false;
    public static bool isWebViewWarmedUp { get; private set; } = false;
    public static bool isWebViewRegistered { get; private set; } = false;

    private static bool isExceptionOccurred = false;

    public static void PreWarming()
    {
        CoroutineRunner.Instance.StartCoroutine(PreWarmRoutine());
    }

    private static IEnumerator PreWarmRoutine()
    {
        if (IsOpertaionCompleted)
            yield break;

        AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity");

        AndroidJavaClass _pluginClass = null;
        AndroidJavaObject _pluginInstance = null;

        AndroidJavaObject webView = null;
        //   AndroidJavaClass mobileAds = null;

        activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
        {
            try
            {
                UnityEngine.Console.Log("Create web view");

                _pluginClass = new AndroidJavaClass("com.frosty.androidviewwarmer.WebviewWarmup");
                _pluginClass.SetStatic<AndroidJavaObject>("mainActivity", activity);
                _pluginInstance = _pluginClass.CallStatic<AndroidJavaObject>("getInstance");
                webView = _pluginInstance.Call<AndroidJavaObject>("WamupTheWebview");
                isWebViewWarmedUp = true;

            }
            catch (Exception e)
            {
                UnityEngine.Console.LogError("Exception occurred while creating webview " + e);
                isExceptionOccurred = true;
            }
            finally
            {
                _pluginClass?.Dispose();
                _pluginInstance?.Dispose();
                playerClass?.Dispose();
                activity?.Dispose();

            }
        }));


        yield return new WaitUntil(() => (isWebViewWarmedUp || isExceptionOccurred));

        UnityEngine.Console.Log($"Warmed Up Success {isWebViewWarmedUp} and {isExceptionOccurred}");

        isWarmedUP = isWebViewWarmedUp;
 

        //yield return new WaitForSeconds(3);

        //if (isExceptionOccurred)
        //{
        //    playerClass?.Dispose();
        //    activity?.Dispose();
        //    yield break;
        //}

        //activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
        //{
        //    try
        //    {
        //        UnityEngine.Console.Log("Register WebView");

        //        mobileAds = new AndroidJavaClass("com.google.android.gms.ads.MobileAds");
        //        mobileAds.CallStatic("registerWebView", webView);
        //        isWebViewRegistered = true;
        //    }
        //    catch(Exception e)
        //    {
        //        isExceptionOccurred = true;
        //        UnityEngine.Console.LogError("Exception occurred while registering webview " + e);

        //    }
        //    finally
        //    {
        //        isWarmedUP = true;
        //        mobileAds?.Dispose();
        //        webView?.Dispose();
        //    }
        //}));

        //yield return new WaitWhile(() => isWarmedUP || isExceptionOccurred);

        //playerClass?.Dispose();
        //activity?.Dispose();
    }
}