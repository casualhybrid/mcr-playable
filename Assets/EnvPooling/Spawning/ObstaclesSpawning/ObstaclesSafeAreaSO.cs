using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "ObstaclesSafeArea", menuName = "ScriptableObjects/Obstacles/ObstaclesSafeArea")]
public class ObstaclesSafeAreaSO : ScriptableObject
{
    [ReadOnly][SerializeField] public List<float> obstaclesSafeAreaZPositionList;

    [Header("References")]
    [SerializeField] private PlayerData playerData;
    [SerializeField] private PlayerSharedData playerSharedData;
    [SerializeField] private Objectset spawnedSpecialPickupsSet;
    [SerializeField] private PlayerAeroplaneState playerAeroplaneState;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameEvent playerStartedFlying;

    [Header("Events")]
    [SerializeField] private GameEvent obstaclesSafeAreaIsUpdated;
    [SerializeField] private GameEvent thrustPickupIsSpawned;
    // [SerializeField] private GameEvent aeroplanePickupIsSpawned;

    public static float safeZoneLength => 20f * SpeedHandler.GameTimeScaleBeforeOverriden;

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += SceneChanged;
        thrustPickupIsSpawned.TheEvent.AddListener(HandleThrustPickupIsSpawned);
        playerStartedFlying.TheEvent.AddListener(HandleAeroplanePickupIsSpawned);
       // aeroplanePickupIsSpawned.TheEvent.AddListener(HandleAeroplanePickupIsSpawned);
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= SceneChanged;
        thrustPickupIsSpawned.TheEvent.RemoveListener(HandleThrustPickupIsSpawned);
        playerStartedFlying.TheEvent.RemoveListener(HandleAeroplanePickupIsSpawned);
        //  aeroplanePickupIsSpawned.TheEvent.RemoveListener(HandleAeroplanePickupIsSpawned);
    }

    private void SceneChanged(Scene a, Scene b)
    {
        Reset();
    }

    private void Reset()
    {
        obstaclesSafeAreaZPositionList.Clear();
    }

    private void HandleThrustPickupIsSpawned(GameEvent gameEvent)
    {
        Vector3 jumpLandingPos = spawnedSpecialPickupsSet.GetList[^1].transform.position;

        // for first half... this one is perfect
        jumpLandingPos.z += playerData.PlayerInformation[0].ThurstJumpLength * SpeedHandler.GameTimeScaleBeforeOverriden;

        // for second half... this one can be improved
        float overridenSpeedOffset = (SpeedHandler.GameTimeScaleBeforeOverriden - gameManager.GetMinimumSpeed) / 2;
        jumpLandingPos.z += playerData.PlayerInformation[0].ThurstJumpLength * (overridenSpeedOffset + gameManager.GetMinimumSpeed);

        // For debugging only
        //GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //cube.transform.position = new Vector3(jumpLandingPos.x, 0f, jumpLandingPos.z);
        //cube.transform.localScale = new Vector3(1f, 1f, /*obstaclesSafeAreaSO.ResetDistForInAirPickups = */ 7 * 2);
        //Destroy(cube, 20f);
        Debug.DrawRay(new Vector3(jumpLandingPos.x, 0f, jumpLandingPos.z), Vector3.up * 100f, Color.white, 15);

        AddObstaclesSafeAreaZPosition(jumpLandingPos.z);
    }

    private void HandleAeroplanePickupIsSpawned(GameEvent gameEvent)
    {
        //  Vector3 jumpLandingPos = spawnedSpecialPickupsSet.GetList[^1].transform.position;
        Vector3 jumpLandingPos = playerSharedData.PlayerTransform.position;

        // for straight movement
        jumpLandingPos.z += playerAeroplaneState.GetAeroplaneLifeTime;

        // for landing movement... this one can be improved
        float overridenSpeedOffset = (SpeedHandler.GameTimeScaleBeforeOverriden - gameManager.GetMinimumSpeed) / 2;
        jumpLandingPos.z += playerData.PlayerInformation[0].ThurstJumpLength * (overridenSpeedOffset + gameManager.GetMinimumSpeed);

        // For debugging only
        //GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //cube.transform.position = new Vector3(jumpLandingPos.x, 0f, jumpLandingPos.z);
        //cube.transform.localScale = new Vector3(1f, 1f, /*obstaclesSafeAreaSO.ResetDistForInAirPickups = */ 7 * 2);
        //Destroy(cube, 20f);
        Debug.DrawRay(new Vector3(jumpLandingPos.x, 0f, jumpLandingPos.z), Vector3.up * 100f, Color.white, 20);

        AddObstaclesSafeAreaZPosition(jumpLandingPos.z);
    }


    public void AddObstaclesSafeAreaZPosition(float zPosition)
    {
        // For debugging safe area
        //UnityEngine.Console.Log($"Adding new safe area Pickup: {spawnedSpecialPickupsSet.GetList[^1]}, {spawnedSpecialPickupsSet.GetList[^1].transform.position}, Safe area: {zPosition}");

        obstaclesSafeAreaZPositionList.Add(zPosition);
        obstaclesSafeAreaIsUpdated.RaiseEvent();
    }

    public bool CheckIfZPositionIsInsideSafeArea(float zPosition)
    {

        for (int i = obstaclesSafeAreaZPositionList.Count - 1; i >= 0; i--)
        {
            if (obstaclesSafeAreaZPositionList[i] < playerSharedData.PlayerTransform.position.z)
            {
                obstaclesSafeAreaZPositionList.RemoveAt(i);
                continue;
            }

            if (MathF.Abs(zPosition - obstaclesSafeAreaZPositionList[i]) < safeZoneLength)
            {
                return true;
            }
        }

        return false;
    }

    public float CheckIfZRangeIsInsideSafeArea(float zRangeStartPoint, float zRangeEndPoint)
    {
        float _safeZoneLength = safeZoneLength;

        for (int i = obstaclesSafeAreaZPositionList.Count - 1; i >= 0; i--)
        {
            float value = obstaclesSafeAreaZPositionList[i];

            if (value < playerSharedData.PlayerTransform.position.z)
            {
                obstaclesSafeAreaZPositionList.RemoveAt(i);
                continue;
            }

            float partOfSafeArea = _safeZoneLength / 3f;
            Bounds safeAreaBounds = new Bounds(new Vector3(0, 0, value + (partOfSafeArea * .5f)), new Vector3(3,3, _safeZoneLength - partOfSafeArea));

            float lengthOfAreaToCheck = zRangeEndPoint - zRangeStartPoint;
            Bounds areaToCheckBounds = new Bounds(new Vector3(0, 0, (zRangeEndPoint + zRangeStartPoint) * .5f), new Vector3(3,6, lengthOfAreaToCheck));

            //if ((MathF.Abs(zRangeStartPoint - value) < _safeZoneLength)
            //    || (MathF.Abs(zRangeEndPoint - value) < _safeZoneLength * .5f)
            //    || (zRangeStartPoint < value && value < zRangeEndPoint))
            //{
            //    return value;
            //}

            if (safeAreaBounds.Intersects(areaToCheckBounds))
            {
                return value;
            }
        }

        return -1;
    }

    public void FloatingPointResetObstaclesSafeAreaList(float movedOffset)
    {
        for (int i = 0; i < obstaclesSafeAreaZPositionList.Count; i++)
        {
            obstaclesSafeAreaZPositionList[i] -= movedOffset;
        }
    }
}
