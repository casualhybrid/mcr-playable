using GoogleMobileAds.Ump.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif

public class UMPManager : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool _isDebug;
    [SerializeField] private List<string> _testDeviceIds;

    public static UMPManager Instance;

    private Action _callback;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        StartCoroutine(InitAfterDelay(3f));
    }

    /// <summary>
    /// Initialize UMP. Pauses the game while the consent form is shown.
    /// </summary>
    public void InitUMP(Action callback = null)
    {
        _callback = callback;

#if UNITY_ANDROID
        StartCoroutine(InitAfterDelay(3f)); // wait 3s after MAX
#elif UNITY_IOS
        StartCoroutine(WaitForATTDetermined());
#endif
    }

    private IEnumerator InitAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Init();
    }

#if UNITY_IOS
    private IEnumerator WaitForATTDetermined()
    {
        var status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
        while (status == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
        {
            status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
            yield return null;
        }

        Init();
    }
#endif

    private void Init()
    {
        ConsentRequestParameters request;

        if (_isDebug)
        {
            var debugSettings = new ConsentDebugSettings
            {
                DebugGeography = DebugGeography.EEA,
                TestDeviceHashedIds = _testDeviceIds,
            };

            request = new ConsentRequestParameters
            {
                TagForUnderAgeOfConsent = false,
                ConsentDebugSettings = debugSettings,
            };
        }
        else
        {
            request = new ConsentRequestParameters
            {
                TagForUnderAgeOfConsent = false,
            };
        }

        ConsentInformation.Update(request, OnConsentInfoUpdated);
    }

    private void OnConsentInfoUpdated(FormError consentError)
    {
        if (consentError != null)
        {
            Debug.LogWarning("UMP Consent info update failed: " + consentError.Message);
            _callback?.Invoke();
            return;
        }

        // Pause the game before showing consent form
        if (PlayerPrefs.GetInt("UMP", 0) == 0)
        {
            PauseGame();
        }



        ConsentForm.LoadAndShowConsentFormIfRequired((FormError formError) =>
        {
            // Resume after form closes
            ResumeGame();

            if (formError != null)
            {
                Debug.LogWarning("UMP Consent form failed: " + formError.Message);
                _callback?.Invoke();
                return;
            }

            if (ConsentInformation.CanRequestAds())
            {
                Debug.Log("UMP: Consent granted. Ads can be requested.");
            }
            else
            {
                Debug.Log("UMP: Consent not granted, ads limited.");
            }

            _callback?.Invoke();
        });
    }

    private void PauseGame()
    {
        Debug.Log("UMP -> Pausing game");
        Time.timeScale = 0f;
        AudioListener.pause = true;
    }

    private void ResumeGame()
    {
        Debug.Log("UMP -> Resuming game");
        Time.timeScale = 1f;
        AudioListener.pause = false;
        PlayerPrefs.SetInt("UMP", 1);
    }
}
