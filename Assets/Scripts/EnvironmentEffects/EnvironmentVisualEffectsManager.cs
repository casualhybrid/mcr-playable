using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentVisualEffectsManager : SerializedMonoBehaviour
{

    [SerializeField] private Transform roadCrackT;
    [SerializeField] private Dictionary<Environment, Transform> shockWaveRoadCrackRelativeToEnv;

    [SerializeField] private PlayerSharedData playerSharedData;

    [SerializeField] private GameEvent playerTouchedGroundAfterBuildingRun;
    [SerializeField] private GameEvent playerHasDoneShockWave;

    private void OnEnable()
    {
        playerTouchedGroundAfterBuildingRun.TheEvent.AddListener(CrackTheGroundUnderPlayer);
        playerHasDoneShockWave.TheEvent.AddListener(ShockCrackTheGroundUnderPlayer);
    }

    private void OnDisable()
    {
        playerTouchedGroundAfterBuildingRun.TheEvent.RemoveListener(CrackTheGroundUnderPlayer);
        playerHasDoneShockWave.TheEvent.RemoveListener(ShockCrackTheGroundUnderPlayer);
    }

    private void CrackTheGroundUnderPlayer(GameEvent gameEvent)
    {
        roadCrackT.gameObject.SetActive(true);

        Vector3 crackPosition = playerSharedData.PlayerTransform.position;
        crackPosition.y = roadCrackT.position.y;
        roadCrackT.position = crackPosition;

        StartCoroutine(DisableGroundBuildingShockAfterDelay());
    }


    private void ShockCrackTheGroundUnderPlayer(GameEvent gameEvent)
    {
        if (playerSharedData.playerCurrentEnvironment == null)
            return;

        Transform crack = null;

        shockWaveRoadCrackRelativeToEnv.TryGetValue(playerSharedData.playerCurrentEnvironment, out crack);

        if (crack == null)
            return;

        crack.gameObject.SetActive(true);

        Vector3 crackPosition = playerSharedData.PlayerTransform.position;
        crackPosition.y = crack.position.y;
        crackPosition.z += (1.23f * SpeedHandler.GameTimeScale);
        crack.position = crackPosition;

    }

    private IEnumerator DisableGroundBuildingShockAfterDelay()
    {
        yield return new WaitForSeconds(5f);
        roadCrackT.gameObject.SetActive(false);
    }
}