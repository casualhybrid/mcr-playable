using System.Collections;
//using UnityEditor.Scripting;
using UnityEngine;

public class PersistentAudioPlayer : MonoBehaviour
{
    [SerializeField] AdsController adsController;
    public AudioSource audioSource;
    public float fadeDuration = 1f; // total time for fade
    public float targetVolume = 0.6f; // final volume after fade
    public float restoreDelay = 3f;
    public float restoreDuration = 1f;
    public static PersistentAudioPlayer Instance;
    public AudioSource gameplayAudio;
    public GameObject tumTumSound;
    private AudioSource tungTungAudio;

    public bool StopMusic = false;
    private AudioSource currentlyPlayingAudio = null;

    [SerializeField] GameObject carHitSound;
    private AudioSource carHitAudio;

    [Header("CoinSound")]
    [SerializeField] AudioSource audioSourceCoinSFX;
    [SerializeField] AudioClip clip;

    [SerializeField] AudioSource technologiaSound;
    [SerializeField] AudioSource panelsSounds;
    [SerializeField] AudioSource rewardSound;
    public static bool isSoundOff;
    // [SerializeField] SettingsManager settingsManager;
    void Awake()
    {
        Instance = this;
        // GameObject ko destroy hone se bacha lo
        DontDestroyOnLoad(gameObject);

        // AudioSource lazmi hona chahiye
        audioSource = GetComponent<AudioSource>();
        tungTungAudio = tumTumSound.GetComponent<AudioSource>();
        carHitAudio = carHitSound.GetComponent<AudioSource>();
        /*  if (audioSource != null)
          {
              audioSource.Play(); // Start mein sound play karo
          }
          else
          {
              Debug.LogWarning("AudioSource not found on: " + gameObject.name);
          }*/
        StopMusic = PlayerPrefs.GetFloat("music") == 0 ? false : true;
        if (!PlayerPrefs.HasKey("sound"))
            PlayerPrefs.SetFloat("sound", 1);
        if (PlayerPrefs.GetFloat("sound") == 1)
        {
            isSoundOff = false;
        }
        else
        {
            isSoundOff = true;
        }
    }
    public void Start()
    {
        //PlayAudio();
        CheckMusicStatus();
        //settingsManager.CheckSound();
    }
    public void CheckMusicStatus()
    {
        StopMusic = PlayerPrefs.GetFloat("music") == 0 ? false : true;
        if (StopMusic)
        {
            if (currentlyPlayingAudio != null)
            {
                currentlyPlayingAudio.enabled = true;

                if (!currentlyPlayingAudio.isPlaying)
                    currentlyPlayingAudio.Play();
            }
            else
            {
                audioSource.enabled = true;
                if (!audioSource.isPlaying)
                    audioSource.Play();

                currentlyPlayingAudio = audioSource;
            }
        }
        else
        {
            if (audioSource.isPlaying)
            {
                currentlyPlayingAudio = audioSource;
                audioSource.Stop();
            }

            if (gameplayAudio.isPlaying)
            {
                currentlyPlayingAudio = gameplayAudio;
                gameplayAudio.Stop();
            }

            audioSource.enabled = false;
            gameplayAudio.enabled = false;
        }
        PlayerPrefs.SetInt("FirstTimee", PlayerPrefs.GetInt("FirstTimee") + 1);
        if (PlayerPrefs.GetInt("FirstTimee") <= 1)
        {
            audioSource.enabled = true;
        }
    }
    public void PlayTechnologia()
    {
        if (!isSoundOff)
        {
            technologiaSound.Play();
            AudioFade(1.5f);
        }
            
    }
    public void PlayAudio()
    {
        panelsSounds.Stop();
        gameplayAudio.Stop();
        if (audioSource != null)
        {
            gameplayAudio.Stop();
            gameplayAudio.enabled = false;

            audioSource.enabled = true;

            if (!audioSource.isPlaying)
                audioSource.Play();

            currentlyPlayingAudio = audioSource;
           
            //Debug.LogError("Null Audio..........");
        }


    }
    public void ResetSound()
    {
        if (audioSource != null)
        {
            audioSource.Play();
            audioSource.enabled = true;

            gameplayAudio.enabled = false;

            /* if (!gameplayAudio.isPlaying)
                 gameplayAudio.Play();*/

            currentlyPlayingAudio = audioSource;
            //Debug.LogError("Null Audio..........");
            /*
                        gameplayAudio.Stop();
                        gameplayAudio.enabled = false;

                        audioSource.enabled = true;

                        if (!audioSource.isPlaying)
                            audioSource.Play();

                        currentlyPlayingAudio = audioSource;
                        Debug.LogError("Null Audio..........");*/
        }
    }
    public void PlayCoinSFX()
    {
        if (!isSoundOff)
            audioSourceCoinSFX.PlayOneShot(clip);
    }
    public void PlayTumTumSound()
    {
        /* tumTumSound.SetActive(true);
         //if (!tungTungAudio.isPlaying)
             tungTungAudio.Play();*/
        StartCoroutine(PlayTungtungSound());

    }
    public void InstantlyPlayTungTung()
    {
        if (!isSoundOff)
        {
            AudioFade(1.5f);
            carHitSound.SetActive(true);
            //if (!tungTungAudio.isPlaying)
            carHitAudio.Play();
        }
    }
    IEnumerator PlayTungtungSound()
    {
        yield return new WaitForSeconds(2f);
        tumTumSound.SetActive(true);
        //if (!tungTungAudio.isPlaying)
        if (!isSoundOff)
        {
            tungTungAudio.Play();
            AudioFade(1.5f);
        }
    }
    IEnumerator PlayMusic()
    {
        yield return new WaitForSeconds(2.5f);
        audioSource.Stop();
        gameplayAudio.Play();

    }
    public void PlayGameplayAudio()
    {
        if (gameplayAudio != null)
        {
            audioSource.Stop();
            audioSource.enabled = false;

            gameplayAudio.enabled = true;

            if (!gameplayAudio.isPlaying)
                gameplayAudio.Play();

            currentlyPlayingAudio = gameplayAudio;
        }
    }

    public void AudioFade(float time)
    {
        if (gameplayAudio != null)
        {
            audioSourceCoinSFX.volume = .2f;
            adsController.PauseFMODMaster();
            restoreDelay = time;
            StopAllCoroutines();
            StartCoroutine(FadeAudioToTarget());
        }
    }

    private IEnumerator FadeAudioToTarget()
    {
        float startVolume = gameplayAudio.volume;
        float elapsed = 0f;

        Debug.Log("Fading audio from " + startVolume + " to " + targetVolume);

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            gameplayAudio.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / fadeDuration);
            yield return null;
        }

        gameplayAudio.volume = targetVolume;
        Debug.Log("Audio faded to " + gameplayAudio.volume);

        // Wait before restoring
        yield return new WaitForSeconds(restoreDelay);

        StartCoroutine(RestoreAudio());
    }

    private IEnumerator RestoreAudio()
    {
        float startVolume = gameplayAudio.volume;
        float elapsed = 0f;

        Debug.Log("Restoring audio from " + startVolume + " to 1");

        while (elapsed < restoreDuration)
        {
            elapsed += Time.deltaTime;
            gameplayAudio.volume = Mathf.Lerp(startVolume, 1f, elapsed / restoreDuration);
            yield return null;
        }

        gameplayAudio.volume = .5f;
        audioSourceCoinSFX.volume = 1f;
        adsController.ResumeFMODMaster();
        Debug.Log("Audio volume restored to " + gameplayAudio.volume);
    }

    public void LevelFailedAudio()
    {
        gameplayAudio.Stop();
        PlayAudio();
    }
    public void PanelSounds()
    {
        if (!isSoundOff)
        {
            StopMainMenuAudio();
            panelsSounds.Play();
        }

    }
    public void StopMainMenuAudio()
    {
        audioSource.Stop();
    }
    public void RewardSound()
    {
        if (!isSoundOff)
        {
            rewardSound.Play();
            AudioFade(1.5f);
        }
    }
}
