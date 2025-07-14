using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "EnvironmentData", menuName = "ScriptableObjects/EnvironmentData")]
public class EnvironmentData : ScriptableObject
{
    #region Variables

    [Header("General")]
    public float minimumSafeZoneLength = 30;

    public bool firstRampBuildingHasBeenSpawned { get; set; } = false;
    public float nextEnvSwitchPatchZPosition { get; set; }
    public float distanceCoveredByActiveEnvironment { get; set; }
    public float distanceBetweenCurrentRampBuildingAndPlayer { get; private set; }

    [Header("References")]
    [SerializeField] private PlayerSharedData playerSharedData;

    #endregion Variables

    #region Unity Callbacks

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += HandleActiveSceneChanged;
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= HandleActiveSceneChanged;
    }

    #endregion Unity Callbacks

    #region Event Handling

    private void HandleActiveSceneChanged(Scene arg0, Scene arg1)
    {
        firstRampBuildingHasBeenSpawned = false;
        nextEnvSwitchPatchZPosition = 0;
        distanceCoveredByActiveEnvironment = 0;
    }

    #endregion Event Handling

    #region Stuff Being Updated

    public void UpdatedistanceBetweenPlayerAndCurrentRampBuilding()
    {
        if (!firstRampBuildingHasBeenSpawned)
            return;

        distanceBetweenCurrentRampBuildingAndPlayer = nextEnvSwitchPatchZPosition - playerSharedData.PlayerTransform.position.z;
    }

    #endregion Stuff Being Updated
}