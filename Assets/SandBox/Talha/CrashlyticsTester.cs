using Firebase.Crashlytics;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Diagnostics;

public class CrashlyticsTester : MonoBehaviour
{

    int updatesBeforeException;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    // Use this for initialization
    void Start()
    {
        StartCoroutine(Test());
        updatesBeforeException = 0;
    }

    private IEnumerator Test()
    {
        yield return new WaitForSeconds(40);
        Crashlytics.LogException(new Exception("Test Exception"));
        Crashlytics.Log("Test Exception Occurred");
      //  Utils.ForceCrash(ForcedCrashCategory.FatalError);
    }

    // Update is called once per frame
    void Update()
    {
        // Call the exception-throwing method here so that it's run
        // every frame update
        throwExceptionEvery60Updates();
    }

    // A method that tests your Crashlytics implementation by throwing an
    // exception every 60 frame updates. You should see non-fatal errors in the
    // Firebase console a few minutes after running your app with this method.
    void throwExceptionEvery60Updates()
    {
        if (updatesBeforeException > 0)
        {
            Crashlytics.LogException(new Exception("Test Exception"));
            updatesBeforeException--;
        }
        else
        {
            // Set the counter to 60 updates
            updatesBeforeException = 60;

            // Throw an exception to test your Crashlytics implementation
            throw new System.Exception("test exception please ignore");
        }
    }
}