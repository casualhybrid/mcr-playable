using Firebase;
using Firebase.Extensions;
using System;
using UnityEngine;

public static class FireBaseInitializer
{
    /// <summary>
    /// Is The FireBase App Initialized and ready to be used
    /// </summary>
    public static bool isInitialized { get; private set; }

    public static event Action OnFireBaseInitialized;

    public static event Action OnFireBaseFailedToInitialize;

    /// <summary>
    /// Is the FireBase App initialization process completed irrelevant of the result
    /// </summary>
    public static bool IsFireBaseInitializationProcessDone;

    //private void Start()
    //{
    //    InitializeFireBaseSDK();
    //}

    public static void InitializeFireBaseSDK()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(Task =>
        {
            IsFireBaseInitializationProcessDone = true;

            // UnityEngine.Console.Log("FireBaseDependencies Checked");

            var dependencyStatus = Task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                //   app = Firebase.FirebaseApp.DefaultInstance;

                // Set a flag here to indicate whether Firebase is ready to use by your app.
                isInitialized = true;

                //  Firebase.FirebaseApp.LogLevel = Firebase.LogLevel.Debug;

                OnFireBaseInitialized?.Invoke();

                UnityEngine.Console.Log("FireBase Successfully Initialized");
            }
            else
            {
                UnityEngine.Console.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
                OnFireBaseFailedToInitialize?.Invoke();
            }
        });
    }
}