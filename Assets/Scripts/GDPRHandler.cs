using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "GDPRHandler", menuName = "ScriptableObjects/Ads/GDPRController")]

public class GDPRHandler : ScriptableObject
{
  //  ConsentForm _consentForm;
    //public bool TestBool;
    bool GDPRConsentRequired = false;
    bool GDPRConsentAquired = false;


    public Coroutine InitializeGDPR()
    {
        return CoroutineRunner.Instance.StartCoroutine(WaitForGDPRConsent());
    }
    bool canShowGDPR => SystemInfo.systemMemorySize > 1500;
    public IEnumerator WaitForGDPRConsent()
    {
        //if (canShowGDPR)
        //{
        //    yield return new WaitForSeconds(1);
        //    var debugSettings = new ConsentDebugSettings
        //    {
        //        TestDeviceHashedIds =
        //        new List<string>
        //        {
        //    "TEST-DEVICE-HASHED-ID"
        //        }
        //    };

        //    ConsentRequestParameters request = new ConsentRequestParameters
        //    {
        //        ConsentDebugSettings = debugSettings,
        //        TagForUnderAgeOfConsent = false
        //    };
        //    // Check the current consent information status.
        //    ConsentInformation.Update(request, OnConsentInfoUpdated);


        //    //yield return new WaitUntil(() => GDPRConsentAquired == true);
        //    yield return new WaitForSecondsRealtime(2);
        //    GDPRConsentAquired = true;
        //}

        yield return null;
    }

    //void OnConsentInfoUpdated(FormError error)
    //{
    //    GDPRConsentRequired = PlayerPrefs.GetInt("GDPRConsentAquired") == 0 && Application.internetReachability != NetworkReachability.NotReachable && ConsentInformation.ConsentStatus == ConsentStatus.Required && ConsentInformation.ConsentStatus != ConsentStatus.Obtained;

    //    Debug.Log($"GDPR: {GDPRConsentRequired} Internet:{Application.internetReachability != NetworkReachability.NotReachable} " +
    //        $"Form Available:{ConsentInformation.IsConsentFormAvailable()} Consent Status: {ConsentInformation.ConsentStatus} -- acquired {GDPRConsentAquired}");
    //    if (GDPRConsentRequired)
    //    {

    //        if (error != null)
    //        {
    //            // Handle the error.
    //            UnityEngine.Debug.LogError(error);
    //            return;
    //        }

    //        // If the error is null, the consent information state was updated.
    //        // You are now ready to check if a form is available.
    //        if (ConsentInformation.IsConsentFormAvailable())
    //        {
    //            LoadConsentForm();
    //        }
    //    }
    //    else
    //    {
    //        GDPRConsentAquired = true;
    //    }
    //}

    void LoadConsentForm()
    {
        // Loads a consent form.
        //ConsentForm.Load(OnLoadConsentForm);
    }


    //void OnLoadConsentForm(ConsentForm consentForm, FormError error)
    //{
    //    if (error != null)
    //    {
    //        // Handle the error.
    //        UnityEngine.Debug.LogError(error);
    //        return;
    //    }

    //    // The consent form was loaded.
    //    // Save the consent form for future requests.
    //    _consentForm = consentForm;

    //    // You are now ready to show the form.
    //    if (ConsentInformation.ConsentStatus == ConsentStatus.Required)
    //    {
    //        _consentForm.Show(OnShowForm);
    //    }
    //}

    //void OnShowForm(FormError error)
    //{
    //    if (error != null)
    //    {
    //        // Handle the error.
    //        UnityEngine.Debug.LogError(error);
    //        return;
    //    }
    //    if (ConsentInformation.ConsentStatus == ConsentStatus.Obtained)
    //    {
    //        PlayerPrefs.SetInt("GDPRConsentAquired", 1);
    //        GDPRConsentAquired = true;
    //    }
    //    else
    //    {
    //        // Handle dismissal by reloading form.
    //        LoadConsentForm();
    //    }
    //}
}
