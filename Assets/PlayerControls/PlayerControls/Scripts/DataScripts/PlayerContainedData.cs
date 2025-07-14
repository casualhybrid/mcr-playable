using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerContainedData", menuName = "ScriptableObjects/PlayerContainedData")]
public class PlayerContainedData : ScriptableObject
{
    public Animator_controller Animator_controller;
    public InputChannel InputChannel;
    public AnimationChannel AnimationChannel;
    public PlayerData PlayerData;
    public SpeedHandler SpeedHandler;
    public PlayerBasicMovementShared PlayerBasicMovementShared;
    public PlayerCanceljump PlayerCanceljump;
    public PlayerJumpState PlayerJumpState;
    public PlayerNormalMovementState PlayerNormalMovementState;
    public PlayerSlideState PlayerSlideState;
    public PlayerBoostState PlayerBoostState;
    public PlayerDoubleBoostState PlayerDoubleBoostState;
    public PlayerThurstState PlayerThurstState;
    public PlayerDeathState PlayerDeathState;
    public PlayerAeroplaneState PlayerAeroplaneState;
    public CameraData CameraData;
    public PlayerBuildingRunState PlayerBuildingRunState;
    public float elapsedTimeRelaxationAfterFall;


    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
    }

    private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        ResetVariable();
    }

    private void ResetVariable()
    {
        elapsedTimeRelaxationAfterFall = 0;
    }
}
