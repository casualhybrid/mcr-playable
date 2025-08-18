using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using deVoid.UIFramework;
using Knights.UISystem;
using UnityEngine.UI;
using UnityEngine.Events;

public class SettingsManager : AWindowController
{
    public SettingsEventChannelSO OnMusicSettingsChanged, OnSoundsSettingsChanged;
    [SerializeField] Slider hapticsSlider, musicslider, soundslider;


    bool hapticsBool,musicBool, soundBool;
    [SerializeField] GameObject settingPanel;
    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        
    }
    private void OnEnable()
    {

        hapticsBool = PlayerPrefs.GetFloat("haptic") ==0 ? false : true;
        musicBool = PlayerPrefs.GetFloat("music") == 0 ? false : true;
        soundBool = PlayerPrefs.GetFloat("sound") == 0 ? false : true;

        hapticsSlider.value = PlayerPrefs.GetFloat("haptic");
        musicslider.value = PlayerPrefs.GetFloat("music");
        soundslider.value = PlayerPrefs.GetFloat("sound");
        PersistentAudioPlayer.isSoundOff = soundslider.value == 0 ? false : true;

    }

    private void Start()
    {
        SoundSpriteHandle();
    }
    public void CheckSound()
    {
        PersistentAudioPlayer.isSoundOff = soundslider.value == 0 ? false : true;
        Debug.Log("Hello");
    }
  /*  private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            FadeMusicEffect();
        }
    }*/
    public void HapticsSetting()
    {
        PlayerPrefs.SetFloat("haptic", hapticsSlider.value);
    }

    public void MusicSetting()
    {
        PlayerPrefs.SetFloat("music", musicslider.value);
        OnMusicSettingsChanged.RaiseEvent(musicslider.value == 0 ? false : true);
    }
/*    }
    public void TestMusicFade()
    {
        musicslider.value = .5f;
        PlayerPrefs.SetFloat("music", musicslider.value);
        Debug.Log("Test"+PlayerPrefs.GetFloat("music"));

        StartCoroutine(test());
    }

    IEnumerator test()
    {
        yield return new WaitForSeconds(2f);
        musicslider.value = 1f;
        PlayerPrefs.SetFloat("music", musicslider.value);
    }*/
    public void SoundSettings()
    {
        PlayerPrefs.SetFloat("sound", soundslider.value);
        OnSoundsSettingsChanged.RaiseEvent(soundslider.value == 0 ? false : true);

    }
    public void SettingClose()
    {
        if(PlayerPrefs.GetFloat("sound")==1)
        {
            SoundButton();
            SoundButton();

        }
        settingPanel.SetActive(false);
    }
    public void HapticsButton()
    {
        hapticsBool = !hapticsBool;
      
        PlayerPrefs.SetFloat("haptic", hapticsBool ? 1:0);

        hapticsSlider.value = hapticsBool ? 1f : 0f;
        if (PlayerPrefs.GetFloat("haptic") == 0)
        {
            hapticsSlider.transform.GetChild(0).gameObject.SetActive(true);
        }
        else
        {
            hapticsSlider.transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    public void MusicButton()
    {
        musicBool = !musicBool;
        //musicBool = false;
       // Debug.Log("Music : " + musicBool);
        PlayerPrefs.SetFloat("music", musicBool ? 1 : 0);
        PersistentAudioPlayer.Instance.StopMusic = musicBool;
        musicslider.value = musicBool ? 1f : 0f;
       // Debug.Log(PlayerPrefs.GetFloat("music"));
        if(PlayerPrefs.GetFloat("music")==0)
        {
            musicslider.transform.GetChild(0).gameObject.SetActive(true);
            SoundButton();
            SoundButton();
        }
        else
        {
            Debug.LogError("Yha");
           /* PlayerPrefs.SetFloat("sound", soundBool ? 1 : 0);
            soundslider.value = soundBool ? 1f : 0f;*/
            musicslider.transform.GetChild(0).gameObject.SetActive(false);
            /* OnSoundsSettingsChanged.RaiseEvent(soundslider.value == 0 ? false : true);*/
            SoundButton();
            SoundButton();

        }
        OnMusicSettingsChanged.RaiseEvent(musicslider.value == 0 ? false : true);
        Debug.Log(PlayerPrefs.GetFloat("music"));
        Debug.Log(PlayerPrefs.GetFloat("sound"));
        PersistentAudioPlayer.Instance.CheckMusicStatus();
    }
    void SoundSpriteHandle()
    {
        if (PlayerPrefs.GetFloat("music") == 0)
        {
            musicslider.transform.GetChild(0).gameObject.SetActive(true);
        }
        else
        {
            musicslider.transform.GetChild(0).gameObject.SetActive(false);
        }
        if (PlayerPrefs.GetFloat("sound") == 0)
        {
            soundslider.transform.GetChild(0).gameObject.SetActive(true);
        }
        else
        {
            soundslider.transform.GetChild(0).gameObject.SetActive(false);
        }
        if (PlayerPrefs.GetFloat("haptic") == 0)
        {
            hapticsSlider.transform.GetChild(0).gameObject.SetActive(true);
        }
        else
        {
            hapticsSlider.transform.GetChild(0).gameObject.SetActive(false);
        }
    }
   /* public void FadeMusicEffect()
    {
        StartCoroutine(FadeMusicRoutine());
    }

    private IEnumerator FadeMusicRoutine()
    {
        float duration = 0.3f;
        float currentTime = 0f;
        float start = 1f;
        float mid = 0.5f;

        // Fade to 0.5
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float value = Mathf.Lerp(start, mid, currentTime / duration);
            musicslider.value = value;
            PlayerPrefs.SetFloat("music", value);
            yield return null;
        }

        currentTime = 0f;

        // Fade back to 1
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float value = Mathf.Lerp(mid, start, currentTime / duration);
            musicslider.value = value;
            PlayerPrefs.SetFloat("music", value);
            yield return null;
        }

        PlayerPrefs.SetFloat("music", 1f);
    }*/

    public void SoundButton()
    {

        soundBool = !soundBool;
        PersistentAudioPlayer.isSoundOff=soundslider.value == 0 ? false : true;
        PlayerPrefs.SetFloat("sound", soundBool ? 1 : 0);
        soundslider.value = soundBool ? 1f : 0f;
        if (PlayerPrefs.GetFloat("sound") == 0)
        {
            soundslider.transform.GetChild(0).gameObject.SetActive(true);
        }
        else
        {
            soundslider.transform.GetChild(0).gameObject.SetActive(false);
        }
        OnSoundsSettingsChanged.RaiseEvent(soundslider.value == 0 ? false : true);

    }

}
