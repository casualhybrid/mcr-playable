using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if (UNITY_EDITOR)
#endif

[RequireComponent(typeof(ResetObject))]
public class Obstacle : MonoBehaviour, IFloatingReset
{
    [SerializeField] public UnityEvent OnDestroyedCarThroughDash;
    [SerializeField] public UnityEvent OnDestroyedCarThroughBoost;
    [SerializeField] public UnityEvent OnDestroyedCarThroughLaser;
    [SerializeField] public UnityEvent OnDestroyedCarThroughShockWave;
    [SerializeField] public UnityEvent OnDestroyedCarThroughArmour;

    public bool IsDestroying { get; set; }
    public bool ShoudNotOffsetOnRest { get; set; }
    public bool IsThisObstaclePartOfCustomEncounter { get; set; }

    public event Action<Obstacle> ObstacleHasFinished;

    [SerializeField] private float destructionMoveSpeed;
    [SerializeField] private float destructionRotateSpeed = 300;
    [SerializeField] private BoxCollider boxCollider;
    [SerializeField] private int instanceID = 0;
    [SerializeField] private ObjectDestruction objectDestruction;
    [SerializeField] private PlayerSharedData playerSharedData;
    [SerializeField] private PlayerData playerData;
    [SerializeField] private SpeedHandler speedHandler;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameEvent obstaclesSafeAreaIsUpdated;
    [SerializeField] private ObstaclesSafeAreaSO obstaclesSafeAreaSO;

    private ResetObject resetObject;
    public float GetObstacleLength => boxCollider.size.z;

    private bool isReset;

    public int InstanceID => instanceID;

    private Vector3 obstacleInitialPosition;
    private bool obstacleInitialActiveState;
    private Vector3 throwAwayDirection;

    private void Awake()
    {
        resetObject = GetComponent<ResetObject>();
    }

    private void Start()
    {
        obstacleInitialPosition = transform.localPosition;
        obstacleInitialActiveState = gameObject.activeSelf;
    }

    private void OnEnable()
    {
        isReset = false;
        obstaclesSafeAreaIsUpdated.TheEvent.AddListener(HandleObstaclesSafeAreaIsUpdated);
    }

    private void OnDisable()
    {
        obstaclesSafeAreaIsUpdated.TheEvent.RemoveListener(HandleObstaclesSafeAreaIsUpdated);
        ShoudNotOffsetOnRest = false;
    }

    private void HandleObstaclesSafeAreaIsUpdated(GameEvent gameEvent)
    {
        // For debugging safe area
        //UnityEngine.Console.Log($"Obstacle transform when safe area is updated {gameObject}, {transform.position.z}");
        if (obstaclesSafeAreaSO.CheckIfZPositionIsInsideSafeArea(transform.position.z))
        {
            isReset = true;
            ObstacleHasFinished?.Invoke(this);
        }
        //else
        //{
        //    // For debugging safe area
        //    Debug.DrawRay(transform.position, Vector3.up * 10f, Color.blue, 10f);
        //}
    }

    private void OnValidate()
    {
#if (UNITY_EDITOR)
        if (transform.root == transform && instanceID == 0)
        {
            instanceID = GetInstanceID();
        }
#endif
    }

    private void Update()
    {
        if (IsDestroying)
        {
            DestroyObj();
        }

        if (playerSharedData.PlayerTransform == null)
            return;

        if (isReset)
            return;

        float distFromPlayer = transform.position.z - playerSharedData.PlayerTransform.position.z;

        if (distFromPlayer <= -8f)
        {
            SendObstacleFinishedEvent(3f);
        }
    }

    public void DestroyThisObstacle(ObstacleDestroyedWay obstacleDestroyedWay = ObstacleDestroyedWay.Forced)
    {
        objectDestruction.DestroyCar(obstacleDestroyedWay);
    }

    public void DestroyTheObstacle(ObstacleDestroyedWay obstacleDestroyedWay)
    {
        if (IsDestroying)
            return;

        ThrowCarAway();

        IsDestroying = true;

        switch (obstacleDestroyedWay)
        {
            case ObstacleDestroyedWay.Armour:
                OnDestroyedCarThroughArmour.Invoke();
                break;

            case ObstacleDestroyedWay.Boost:
                OnDestroyedCarThroughBoost.Invoke();
                break;

            case ObstacleDestroyedWay.Dash:
                OnDestroyedCarThroughDash.Invoke();
                AnalyticsManager.CustomData("GamePlayScreen_ObstacleDestroyed", new Dictionary<string, object> { { "DestructionMethod", obstacleDestroyedWay.ToString() }, { "ObstacleName", name } });
                break;

            case ObstacleDestroyedWay.Laser:
                OnDestroyedCarThroughLaser.Invoke();
                break;

            case ObstacleDestroyedWay.ShockWave:
                OnDestroyedCarThroughShockWave.Invoke();
                AnalyticsManager.CustomData("GamePlayScreen_ObstacleDestroyed", new Dictionary<string, object> { { "DestructionMethod", obstacleDestroyedWay.ToString() }, { "ObstacleName", name } });
                break;

            default:
                UnityEngine.Console.LogWarning($"The obstacle was destroyed way wasn't handled {obstacleDestroyedWay}");
                break;
        }
    }

    private void ThrowCarAway()
    {
        int rand = UnityEngine.Random.Range(0, 2);

        float z = rand == 0 ? UnityEngine.Random.Range(-50, -75) : UnityEngine.Random.Range(50, 75);

        float x = UnityEngine.Random.Range(0, 60);
        float y = 0;

        Vector3 eulerAngles = new Vector3(x, y, z);

        Quaternion rotation = Quaternion.Euler(eulerAngles);

        throwAwayDirection = rotation * Vector3.up;

        Debug.DrawRay(transform.position, throwAwayDirection * 100f, Color.blue, 4f);
    }

    private void DestroyObj()
    {
        transform.position += (throwAwayDirection * destructionMoveSpeed * Time.deltaTime);
        transform.Rotate(throwAwayDirection, Time.deltaTime * destructionRotateSpeed, Space.World);
    }

    public void SendObstacleFinishedEvent(float delay)
    {
        // UnityEngine.Console.Log("Returning Obstacle To Pool");
        isReset = true;
        StartCoroutine(SendObstacleFinishedEventAfterDuration(delay));
    }

    private IEnumerator SendObstacleFinishedEventAfterDuration(float delay)
    {
        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }

        ObstacleHasFinished?.Invoke(this);
    }

    public void ResetObstaclePositionAndState()
    {
        transform.localPosition = obstacleInitialPosition;
        gameObject.SetActive(obstacleInitialActiveState);
    }

    public void RestTheObstacle()
    {
        throwAwayDirection = Vector2.zero;
        IsDestroying = false;
        resetObject.ResetTheObject();
    }

    public void OnFloatingPointReset(float movedOffset)
    {
    }

    public void OnBeforeFloatingPointReset()
    {
    }
}