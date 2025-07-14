using UnityEngine;
public class ShareNetwork : MonoBehaviour
{
    public string link = "https://play.google.com/store/apps/details?id=com.tb.minicar.rush.racing.drivinggames&hl=en&gl=US";
    public void ShareText()
    {
#if UNITY_ANDROID
        try
        {
            AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
            AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");
            intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
            intentObject.Call<AndroidJavaObject>("setType", "text/plain");
            intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), link);
            AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");
            currentActivity.Call("startActivity", intentObject);
          
        }
        catch (System.Exception) { }
#endif
    }
}
