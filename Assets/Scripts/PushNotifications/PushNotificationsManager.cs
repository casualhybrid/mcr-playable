using System;
using System.Collections;
#if UNITY_ANDROID
using Unity.Notifications.Android;
#endif

#if UNITY_IOS
using Unity.Notifications.iOS;
#endif

using UnityEngine;

public static class PushNotificationsManager
{
    private class NotificationIDs
    {
        public const int DailyRewardReminder = 0;
        public const int InActivityReminder = 1;
    }

    public static void RequestNotificationPermission()
    {

#if UNITY_EDITOR
        return;

#elif UNITY_IOS
        CoroutineRunner.Instance.StartCoroutine(RequestAuthorizationIOS());
#elif UNITY_ANDROID

        int apiLevel;

        try
        {
            var operatingSystem = SystemInfo.operatingSystem;
            apiLevel = int.Parse(operatingSystem.Substring(operatingSystem.IndexOf("-") + 1, 2));
        }
        catch
        {
            UnityEngine.Console.LogError("Failed to parse android operating system for push notification permission");
            apiLevel = 0;
        }

        if (apiLevel >= 33)
        {
            CoroutineRunner.Instance.StartCoroutine(RequestAuthorizationAndroid());
        }
#endif
    }

#if UNITY_ANDROID
    #region Android

    private class AndroidNotificationChannels
    {
        public const string GeneralChannel = "GeneralReminder";
    }

    private static IEnumerator RequestAuthorizationAndroid()
    {
        var request = new PermissionRequest();
        while (request.Status == PermissionStatus.RequestPending)
            yield return null;
        // here use request.Status to determine users response
    }

    private static void CreateReminderNotificationChannel()
    {
        var channel = new AndroidNotificationChannel()
        {
            Id = AndroidNotificationChannels.GeneralChannel,
            Name = "General Reminders",
            Importance = Importance.Default,
            Description = "Reminds you about pending rewards",
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);

        UnityEngine.Console.Log("Notification Channel Created");
    }

    private static void RegisterNextDailyRewardNotificationAndroid(bool askForPermissionIfReq = true)
    {
        CreateReminderNotificationChannel();

        if (askForPermissionIfReq)
        {
            RequestNotificationPermission();
        }

        var notification = new AndroidNotification();
        notification.Title = "Get your free daily reward!";
        notification.Text = "Your free mystery box is waiting to be opened";
        notification.FireTime = System.DateTime.Now.AddDays(1);

        UpdateInActivityNotification(NotificationIDs.DailyRewardReminder, notification, AndroidNotificationChannels.GeneralChannel);
    }

    private static void RegisterInActivityNotificationAndroid(bool askForPermissionIfReq = true)
    {
        CreateReminderNotificationChannel();

        if (askForPermissionIfReq)
        {
            RequestNotificationPermission();
        }

        var notification = new AndroidNotification();
        notification.Title = "Beat the chase!";
        notification.Text = "Grab the wheel and outrun the chaser!";
        notification.FireTime = System.DateTime.Now.AddDays(3); ;

        UpdateInActivityNotification(NotificationIDs.InActivityReminder, notification, AndroidNotificationChannels.GeneralChannel);
    }

    private static void UpdateInActivityNotification(int id, AndroidNotification newNotification, string channelID)
    {
        var notificationStatus = AndroidNotificationCenter.CheckScheduledNotificationStatus(id);

        if (notificationStatus == NotificationStatus.Scheduled)
        {
            AndroidNotificationCenter.UpdateScheduledNotification(id, newNotification, channelID);
            return;
        }
        else 
        {
            AndroidNotificationCenter.CancelScheduledNotification(id);
        }

        UnityEngine.Console.Log($"Notification Registered. Current Status {notificationStatus}");

        AndroidNotificationCenter.SendNotificationWithExplicitID(newNotification, channelID, id);

        UnityEngine.Console.Log($"Notification status after {AndroidNotificationCenter.CheckScheduledNotificationStatus(id)}");
    }

    #endregion Android
#endif

#if UNITY_IOS
    #region IOS

    private static IEnumerator RequestAuthorizationIOS()
    {
        if (iOSNotificationCenter.GetNotificationSettings().AuthorizationStatus == AuthorizationStatus.Authorized)
            yield break;

        var authorizationOption = AuthorizationOption.Alert | AuthorizationOption.Badge;
        using (var req = new AuthorizationRequest(authorizationOption, false))
        {
            while (!req.IsFinished)
            {
                yield return null;
            };

            string res = "\n RequestAuthorization:";
            res += "\n finished: " + req.IsFinished;
            res += "\n granted :  " + req.Granted;
            res += "\n error:  " + req.Error;
            res += "\n deviceToken:  " + req.DeviceToken;
            UnityEngine.Console.Log(res);
        }
    }

    private static void RegisterNextDailyRewardNotificationIOS()
    {
        RequestNotificationPermission();

        string identifier = NotificationIDs.DailyRewardReminder.ToString();

        iOSNotificationCenter.RemoveScheduledNotification(identifier);

        var timeTrigger = new iOSNotificationTimeIntervalTrigger()
        {
            TimeInterval = new TimeSpan(1, 0, 0, 0),
            Repeats = false
        };

        var notification = new iOSNotification()
        {
            // You can specify a custom identifier which can be used to manage the notification later.
            // If you don't provide one, a unique string will be generated automatically.
            Identifier = identifier,
            Title = "Your daily reward is waiting!",
            Body = "Scheduled at: " + DateTime.Now.ToShortDateString() + " triggered in 5 seconds",
            Subtitle = "Get your free daily reward before it expires!",
            //  ShowInForeground = true,
            // ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
            CategoryIdentifier = "category_a",
            ThreadIdentifier = "thread1",
            Trigger = timeTrigger,
        };

        iOSNotificationCenter.ScheduleNotification(notification);
    }

    private static void RegisterInActivityNotificationIOS()
    {
        RequestNotificationPermission();

        string identifier = NotificationIDs.InActivityReminder.ToString();

        iOSNotificationCenter.RemoveScheduledNotification(identifier);

        var calendarTrigger = new iOSNotificationCalendarTrigger()
        {
            // Year = 2020,
            // Month = 6,
            Day = 3,
            // Hour = 12,
            // Minute = 0,
            // Second = 0
            // Repeats = false
        };

        var notification = new iOSNotification()
        {
            // You can specify a custom identifier which can be used to manage the notification later.
            // If you don't provide one, a unique string will be generated automatically.
            Identifier = identifier,
            Title = "Cars are waiting to be driven",
            Body = "Scheduled at: " + DateTime.Now.ToShortDateString() + " triggered in 5 seconds",
            Subtitle = "Have fun in this game",
            //  ShowInForeground = true,
            // ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
            CategoryIdentifier = "category_a",
            // ThreadIdentifier = "thread1",
            Trigger = calendarTrigger,
        };

        iOSNotificationCenter.ScheduleNotification(notification);
    }

    #endregion IOS
#endif

    public static void RegisterInActivityPeriodNotification(bool askForPermissionIfReq = true)
    {
#if UNITY_IOS
        RegisterInActivityNotificationIOS();
#elif UNITY_ANDROID
        RegisterInActivityNotificationAndroid(askForPermissionIfReq);
#endif
    }

    public static void RegisterDailyRewardNotification(bool askForPermissionIfReq = true)
    {
#if UNITY_IOS
        RegisterNextDailyRewardNotificationIOS();
#elif UNITY_ANDROID
        RegisterNextDailyRewardNotificationAndroid(askForPermissionIfReq);
#endif
    }
}