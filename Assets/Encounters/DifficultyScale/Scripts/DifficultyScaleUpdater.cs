using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DifficultyScaleUpdater : MonoBehaviour
{
    #region Variables
    [Header("General")]
    private bool isInitialized;
    private float timePassedSinceLastDifficultyScaleRatingUpdate;
    private float lastSampleDistanceCoveredInMetres;

    [Header("Events")]
    [SerializeField] private GameEvent gameHasStarted;
    [SerializeField] private GameEvent playerCrashed;
    [SerializeField] private GameEvent playerRevived;

    [Header("References")]
    [SerializeField] private DifficultyScaleSO difficultyScale;
    [SerializeField] private GamePlaySessionData gamePlaySessionData;
    #endregion

    #region Unity Callbacks
    private void OnEnable()
    {
        gameHasStarted.TheEvent.AddListener(HandleGameHasStarted);
        playerCrashed.TheEvent.AddListener(HandleOnPlayerCrashed);
        playerRevived.TheEvent.AddListener(HandlePlayerRevived);
    }

    private void OnDisable()
    {
        gameHasStarted.TheEvent.RemoveListener(HandleGameHasStarted);
        playerCrashed.TheEvent.RemoveListener(HandleOnPlayerCrashed);
        playerRevived.TheEvent.RemoveListener(HandlePlayerRevived);
    }

    private void LateUpdate() // Done in late update because 'gamePlaySessionData.DistanceCoveredInMeters' is being updated in update
    {
        if (!isInitialized)
            return;

        timePassedSinceLastDifficultyScaleRatingUpdate += Time.deltaTime;

        if (timePassedSinceLastDifficultyScaleRatingUpdate > (1 / difficultyScale.difficultyRatingSamplingFrequency))
        {
            timePassedSinceLastDifficultyScaleRatingUpdate = timePassedSinceLastDifficultyScaleRatingUpdate % (1 / difficultyScale.difficultyRatingSamplingFrequency); // Because updating 2 times in a single frame doesn't contribute to anything

            float deltaDistance = gamePlaySessionData.DistanceCoveredInMeters - lastSampleDistanceCoveredInMetres;
            lastSampleDistanceCoveredInMetres = gamePlaySessionData.DistanceCoveredInMeters;

            difficultyScale.OnDifficultyRatingUpdate(deltaDistance);
        }
    }

    //private void OnGUI()
    //{
    //    GUI.Label(new Rect(Screen.width / 3, Screen.height / 2, 500f, 300), difficultyScale.isSkillPlacement ? "SKILL PLACEMENT" : "Normal", new GUIStyle() { fontSize = 70 });
    //}
    #endregion

    #region Event Handing
    private void HandleGameHasStarted(GameEvent gameEvent)
    {
        difficultyScale.InitializeCurrentDifficultySample();
        isInitialized = true;
    }

    private void HandlePlayerRevived(GameEvent gameEvent)
    {
        isInitialized = true;
    }

    private void HandleOnPlayerCrashed(GameEvent gameEvent)
    {
        isInitialized = false;
    }
    #endregion
}
