using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR

using UnityEditor;

#endif

public class Trafficar : MonoBehaviour, IResetObject<Trafficar>
{
    public Trafficar OriginalComponent { get; set; }

    [SerializeField] private UnityEvent playerIsInLineOfSight;
    [SerializeField] private UnityEvent playerIsOutOfSight;
    [SerializeField] private float lineOfSight;

    [SerializeField] private PlayerSharedData playerSharedData;
    public bool DoYourWork;
    [SerializeField] private bool isFastOnSameLane;
    [SerializeField] private Tyre_rotation tyreRotation;
    [SerializeField] private bool StillObstacle;
    [SerializeField] private float MovementSpeedpercentage; // animator "RedCar" is changing its value !! be Aware
    [SerializeField] private bool LaneSwitcher;
    [SerializeField] private int Direction;
    [SerializeField] private float destructionMoveSpeed;
    [SerializeField] private bool Jumpable;
    [SerializeField] private bool isVerySlow;
    [SerializeField] private bool distructable;
    [SerializeField] private float DelayInLaneSwitching;
    [SerializeField] private GameEvent policeConvertedToRamp;
    [SerializeField] private GameObject carBody, ramp, colliderObj, walkableObj, planeShadowObj;

    private float elapsedTime;
    private Vector3 throwAwayDirection;
    private bool isDestroying;
    private bool isInLineOfSight;

    //  private float speedMovement = 1.25f;

    // private void Start()
    // {
    //     elapsedTime = DateTime.Now;
    // }

#if UNITY_EDITOR

    #region Bounds Calculation

    private Bounds cachedLongestObstacleBounds { get; set; }

    private Bounds GetCachedLongestObstacleBounds
    {
        get
        {
            if (cachedLongestObstacleBounds.size == Vector3.zero)
            {
                RecalculateCachedLongestObstacleBounds();
            }

            return cachedLongestObstacleBounds;
        }
    }

    private void RecalculateCachedLongestObstacleBounds()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();
        Bounds obstacleBounds = new Bounds();

        foreach (Collider collider in colliders)
        {
            if (collider.GetType() != typeof(BoxCollider) &&
            collider.GetType() != typeof(MeshCollider))
            {
                UnityEngine.Console.LogError($"{collider.GetType()} is not supported by Traffic Car. Only BoxCollider and MeshCollider are supported.");
                continue;
            }

            if (collider.CompareTag("ObstacleMovementTrigger"))
                continue;

            if (collider.GetType() == typeof(BoxCollider))
            {
                obstacleBounds = CustomEncapsulateObstacleBounds(obstacleBounds, new Bounds(collider.transform.position, Vector3.Scale((collider as BoxCollider).size, collider.transform.lossyScale)));
            }
            else if (collider.GetType() == typeof(MeshCollider))
            {
                MeshRenderer meshRenderer = collider.GetComponent<MeshRenderer>();
                obstacleBounds = CustomEncapsulateObstacleBounds(obstacleBounds, meshRenderer.bounds);
            }
        }

        cachedLongestObstacleBounds = obstacleBounds;
    }

    private Bounds CustomEncapsulateObstacleBounds(Bounds mainBounds, Bounds boundsToEncapsulate)
    {
        if (mainBounds.extents == Vector3.zero)
        {
            mainBounds = boundsToEncapsulate;
        }
        else
        {
            mainBounds.Encapsulate(boundsToEncapsulate);
        }

        return mainBounds;
    }

    #endregion Bounds Calculation

    #region Gizmos

    private Transform _movementTriggerTransform { get; set; }

    private Transform MovementTriggerTransform
    {
        get
        {
            if (_movementTriggerTransform == null)
            {
                BoxCollider[] childrenColliders = GetComponentsInChildren<BoxCollider>();

                foreach (BoxCollider childCollider in childrenColliders)
                {
                    if (childCollider.CompareTag("ObstacleMovementTrigger"))
                    {
                        _movementTriggerTransform = childCollider.transform;
                        break;
                    }
                }
            }

            return _movementTriggerTransform;
        }
    }

    private void OnDrawGizmos()
    {
        //
        // IMPORTANT : THIS PREDICTS THE *WRONG* LOCATION IN ITS CURRENT STATE AND NEEDS TO BE FIXED
        //

        return;

        //
        //
        //

        if (Application.isPlaying)
            return;

        if (MovementTriggerTransform == null)
            return;

        // Shows a box, predicting the approximate location of a moving obstacle, where it will be colliding with the player
        float movementMagnitudePerOneUnitTriggerDistance = 0.01134f * MovementSpeedpercentage; // Considering that the speed of the obstacle is the current set 'Movement Speed Percentage'
        //UnityEngine.Console.LogError(movementMagnitudePerOneUnitTriggerDistance);
        float distanceBetweenTriggerAndObstacle = Mathf.Abs(transform.position.z - MovementTriggerTransform.position.z);
        //UnityEngine.Console.LogError(distanceBetweenTriggerAndObstacle);
        Vector3 predictedObstaclePosition = MovementTriggerTransform.position - (Vector3.forward * (distanceBetweenTriggerAndObstacle * movementMagnitudePerOneUnitTriggerDistance));

        Gizmos.color = new Color(1f, 0.2f, 0.2f);
        Gizmos.DrawWireCube(predictedObstaclePosition, GetCachedLongestObstacleBounds.size);

        if (Selection.Contains(gameObject))
        {
            Gizmos.color = new Color(1f, 1f, 0.2f);
        }
        else
        {
            Gizmos.color = new Color(1f, 0.6f, 0.2f);
        }

        Gizmos.DrawWireCube(predictedObstaclePosition, GetCachedLongestObstacleBounds.size * 0.95f);
    }

    #endregion Gizmos

#endif

    private void Update()
    {
        if (!StillObstacle && DoYourWork)
        {
            Move();
        }

        if (isDestroying)
        {
            DestroyCar();
        }


        // Line of sight
        float distFromPlayer = transform.position.z - playerSharedData.PlayerTransform.position.z;

        if (distFromPlayer <= lineOfSight && transform.position.x == playerSharedData.PlayerTransform.position.x)
        {
            if (!isInLineOfSight)
            {
                playerIsInLineOfSight.Invoke();
            }

            isInLineOfSight = true;
        }
        else
        {
            if (isInLineOfSight)
            {
                playerIsOutOfSight.Invoke();
            }

            isInLineOfSight = false;
        }
    }

    private float val;

    public void Move()
    {
        elapsedTime += Time.deltaTime;

        // if (isFastOnSameLane && transform.position.x == playerSharedData.PlayerTransform.position.x)
        //     speedMovement = 1.75f;
        // else {
        //     if (isVerySlow)
        //         speedMovement = 1f;
        //     else
        //         speedMovement = 1.25f;
        // }

        float temp = MovementSpeedpercentage * playerSharedData.ForwardSpeed;
        float zpos = transform.position.z + (Direction * temp * Time.deltaTime);
        transform.position = new Vector3(transform.position.x, transform.position.y, zpos);

        if (LaneSwitcher)
        {
            //   if (DateTime.Now >= elapsedTime)
            //   {
            val = Mathf.Lerp(transform.position.x, playerSharedData.PlayerTransform.position.x, Time.deltaTime);
            transform.position = new Vector3(val, transform.position.y, transform.position.z);
            //  }
        }
    }

    public void MoveCar() => DoYourWork = true;

    public void StopCar()
    {
        LaneSwitcher = false;
        DoYourWork = false;
    }

    public void ResetRotation() => transform.rotation = Quaternion.identity;

    public void ThrowMe()
    {
        // if my ref in list of ss do this
        if (distructable && !isDestroying)
        {
            Collider[] colliders = GetComponentsInChildren<Collider>();

            if (colliders != null)
            {
                for (int i = 0; i < colliders.Length; i++)
                {
                    colliders[i].enabled = false;
                }
            }

            ThrowCarAway();

            isDestroying = true;

            return;
        }
    }

    private void DestroyCar()
    {
        gameObject.transform.position += (throwAwayDirection * destructionMoveSpeed);
    }

    private void ThrowCarAway()
    {
        DoYourWork = false;

        float z = UnityEngine.Random.Range(-60, 60);
        float x = UnityEngine.Random.Range(-60, 60);
        float y = 0;

        Vector3 eulerAngles = new Vector3(x, y, z);

        Quaternion rotation = Quaternion.Euler(eulerAngles);

        throwAwayDirection = rotation * eulerAngles;

        Debug.DrawRay(transform.position, throwAwayDirection * 100f, Color.blue, 4f);
    }

    public void ConvertToRamp()
    {
        if (LaneSwitcher)
        {
            StopCar();
            ResetRotation();
          //  animator.enabled = false;
            carBody.SetActive(false);
            colliderObj.SetActive(false);
            planeShadowObj.SetActive(false);
            walkableObj.SetActive(false);
            ramp.SetActive(true);
        }
    }

    public void ResetTrafficCar()
    {
        // elapsedTime = DateTime.Now;
        Vector3 throwAwayDirection = Vector3.zero;
        isDestroying = false;

        DoYourWork = OriginalComponent.DoYourWork;
        StillObstacle = OriginalComponent.StillObstacle;
        MovementSpeedpercentage = OriginalComponent.MovementSpeedpercentage; // animator "RedCar" is changing its value !! be Aware
        LaneSwitcher = OriginalComponent.LaneSwitcher;
        Direction = OriginalComponent.Direction;
        destructionMoveSpeed = OriginalComponent.destructionMoveSpeed;
        Jumpable = OriginalComponent.Jumpable;
        distructable = OriginalComponent.distructable;
        DelayInLaneSwitching = OriginalComponent.DelayInLaneSwitching;
        isInLineOfSight = OriginalComponent.isInLineOfSight;

        transform.rotation = OriginalComponent.transform.rotation;
    }
}