using System.Collections;
using UnityEngine;

public class SoundsHandler : MonoBehaviour
{
    [SerializeField] private GameEvent cutSceneStartedEvent, cutSceneEndedEvent, pauseBtnEvent, resumeBtnEvent, gameOverEvent, gameReviveEvent;//, gameStartedEvent;
    [SerializeField] private GameObject cutSceneSnapShotObj, pauseScreenSnapshotObj, gameOverSnapshotObj,OffbgMusic;

    private void OnEnable()
    {
        cutSceneStartedEvent.TheEvent.AddListener(PlayCutSceneSnapShot);
        cutSceneEndedEvent.TheEvent.AddListener(StopCutSceneSnapShot);
        pauseBtnEvent.TheEvent.AddListener(PauseGameShanpshot);
        resumeBtnEvent.TheEvent.AddListener(ResumeGameShanpshot);
        //gameStartedEvent.TheEvent.AddListener(GameHasStarted);
        gameReviveEvent.TheEvent.AddListener(GameHasStarted);
        gameOverEvent.TheEvent.AddListener(GameHasOvered);
        OffbgMusic.SetActive(false);
       // PersistentAudioPlayer.Instance.PlayAudio();

    }

    void GameHasOvered(GameEvent theEvent) {
        StartCoroutine(WaitForGameOverSoundsDown());
    }

    IEnumerator WaitForGameOverSoundsDown() {
        yield return new WaitForSecondsRealtime(1.25f);
        gameOverSnapshotObj.SetActive(true);
    }

    void GameHasStarted(GameEvent theEvent) {
        gameOverSnapshotObj.SetActive(false);
    }

    void PauseGameShanpshot(GameEvent theEvent)
    {
        pauseScreenSnapshotObj.SetActive(true);
    }

    void ResumeGameShanpshot(GameEvent theEvent)
    {
        pauseScreenSnapshotObj.SetActive(false);
    }

    void PlayCutSceneSnapShot(GameEvent theEvent) {
        cutSceneSnapShotObj.SetActive(true);
    }

    void StopCutSceneSnapShot(GameEvent theEvent)
    {
        cutSceneSnapShotObj.SetActive(false);
    }

    private void OnDisable()
    {
        cutSceneStartedEvent.TheEvent.RemoveListener(PlayCutSceneSnapShot);
        cutSceneEndedEvent.TheEvent.RemoveListener(StopCutSceneSnapShot);
        pauseBtnEvent.TheEvent.RemoveListener(PauseGameShanpshot);
        resumeBtnEvent.TheEvent.RemoveListener(ResumeGameShanpshot);
        //gameStartedEvent.TheEvent.RemoveListener(GameHasStarted);
        gameReviveEvent.TheEvent.RemoveListener(GameHasStarted);
        gameOverEvent.TheEvent.RemoveListener(GameHasOvered);
    }
}
