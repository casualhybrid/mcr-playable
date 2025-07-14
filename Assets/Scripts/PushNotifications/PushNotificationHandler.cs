using System;
using TheKnights.SaveFileSystem;
using Unity.Notifications.Android;
using UnityEngine;

public class PushNotificationHandler : MonoBehaviour
{
    [SerializeField] private SaveManager saveManager;

    private static bool hasScheduledInActivityNotificationThisSession;

    private void Start()
    {
        if (saveManager.MainSaveFile.TutorialHasCompleted)
        {
            PushNotificationsManager.RequestNotificationPermission();
        }

        if (!hasScheduledInActivityNotificationThisSession)
        {
            hasScheduledInActivityNotificationThisSession = true;
            PushNotificationsManager.RegisterInActivityPeriodNotification(false);
        }

        bool hasScheduledDailyRewardOnce = PlayerPrefs.GetInt("ScheduledDailyRewardNotificationOnce", 0) == 1;

        if (!hasScheduledDailyRewardOnce)
        {
            PushNotificationsManager.RegisterDailyRewardNotification(false);
            PlayerPrefs.SetInt("ScheduledDailyRewardNotificationOnce", 1);
        }

        AndroidNotificationCenter.OnNotificationReceived += receivedNotificationHandler;

    }

    private void receivedNotificationHandler(AndroidNotificationIntentData data)
    {
        var msg = "Notification received : " + data.Id + "\n";
        msg += "\n Notification received: ";
        msg += "\n .Title: " + data.Notification.Title;
        msg += "\n .Body: " + data.Notification.Text;
        msg += "\n .Channel: " + data.Channel;
        UnityEngine.Console.Log(msg);
    }
}