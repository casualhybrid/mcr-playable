using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MATS_ToastManager : MonoBehaviour
{
    AndroidJavaObject currentActivity;
    public void Start()
    {
        Time.timeScale = 1.0f;
#if UNITY_EDITOR

#else
        //currentActivity androidjavaobject must be assigned for toasts to access currentactivity;
        AndroidJavaClass UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        currentActivity = UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
#endif
    }
    public void SendToast(string message, bool isLongToast)
    {

#if UNITY_EDITOR
        Debug.Log("Toast Message " + message);
#else
        if (!isLongToast)
        {
            AndroidJavaObject context = currentActivity.Call<AndroidJavaObject>("getApplicationContext");
            AndroidJavaClass Toast = new AndroidJavaClass("android.widget.Toast");
            AndroidJavaObject javaString = new AndroidJavaObject("java.lang.String", message);
            AndroidJavaObject toast = Toast.CallStatic<AndroidJavaObject>("makeText", context, javaString, Toast.GetStatic<int>("LENGTH_SHORT"));
            toast.Call("show");
        }
        else
        {
            AndroidJavaObject context = currentActivity.Call<AndroidJavaObject>("getApplicationContext");
            AndroidJavaClass Toast = new AndroidJavaClass("android.widget.Toast");
            AndroidJavaObject javaString = new AndroidJavaObject("java.lang.String", message);
            AndroidJavaObject toast = Toast.CallStatic<AndroidJavaObject>("makeText", context, javaString, Toast.GetStatic<int>("LENGTH_LONG"));
            toast.Call("show");
        }
#endif
    }



    }