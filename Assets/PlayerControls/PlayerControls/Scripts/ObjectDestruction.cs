using TheKnights.SaveFileSystem;
using UnityEngine;
using UnityEngine.Events;

public enum ObstacleDestroyedWay
{
    None, Dash, Boost, Laser, ShockWave, Armour, Forced
}

public class ObjectDestruction : MonoBehaviour, IResetObject<ObjectDestruction>, IDestructibleObstacle
{
    [SerializeField] public UnityEvent OnDestroyedCar;

    [SerializeField] private PlayerSharedData PlayerSharedData;
    [SerializeField] private bool destroyDuringDash, destroyDuringBoost, destroyDuringShockwave, destroyDuringLaser;
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private GameEvent playerFinishedBoostingEvent;

    public ObjectDestruction OriginalComponent { get; set; }

    public Transform parentTransform;

    public bool isDestroyableDuringDash => destroyDuringDash;
    public bool isDestroyingDuringBoost => destroyDuringBoost;
    public bool isDestroyingDuringLaser => destroyDuringLaser;
    public bool isDestroyDuringShockwave => destroyDuringShockwave;

    public HitDirection DirectionThePlayerHitFrom { get; set; }

    public bool isDestroying
    {
        get
        {
            return parentObstacle.IsDestroying;
        }
        set
        {
            parentObstacle.IsDestroying = value;
        }
    }

    private AudioPlayer destructionSFx;

    private CustomTag customTag;

    private Obstacle parentObstacle;

    private bool isInitializedOnce;

    private void Awake()
    {
        customTag = GetComponent<CustomTag>();
        destructionSFx = GetComponent<AudioPlayer>();
        parentObstacle = parentTransform.GetComponent<Obstacle>();

        isInitializedOnce = true;
    }

    private void OnEnable()
    {
        playerFinishedBoostingEvent.TheEvent.AddListener(HandlePlayerFinishedBoosting);
    }

    private void OnDisable()
    {
        playerFinishedBoostingEvent.TheEvent.RemoveListener(HandlePlayerFinishedBoosting);
    }

    private void HandlePlayerFinishedBoosting(GameEvent theEvent)
    {
        if (PlayerSharedData.CurrentStateName == PlayerState.PlayerAeroplaneState || PlayerSharedData.WallRunBuilding
            || PlayerSharedData.CurrentStateName == PlayerState.PlayerThurstState || parentObstacle.IsThisObstaclePartOfCustomEncounter)
            return;

        float zDist = transform.position.z - PlayerSharedData.PlayerTransform.position.z;
        if (GameManager.BoostExplosionDistance >= zDist && zDist >= -(GameManager.BoostExplosionDistance / 5))
        {
            DestroyCar();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Obstacles"))
        {
            Trafficar tempTrafficar = other.transform.parent.GetComponent<Trafficar>();
            if (tempTrafficar != null)
            {
                tempTrafficar.ConvertToRamp();
            }
        }
    }

    public void DestroyCar(HitDirection hitDirection)
    {
        bool isNonDestructibleFromFront = customTag != null && customTag.HasTag("NoHitFromFront");

        bool destroyableDuringBoost = (PlayerSharedData.IsBoosting && destroyDuringBoost) && !(isNonDestructibleFromFront &&
             (hitDirection == HitDirection.Forward || hitDirection == HitDirection.Bottom || hitDirection == HitDirection.Top));

        bool destroyableDuringDash = PlayerSharedData.IsDash && destroyDuringDash;
        bool destroyableDuringArmour = PlayerSharedData.IsArmour;

        if (destroyableDuringBoost || destroyableDuringDash || destroyableDuringArmour)
        {
            ObstacleDestroyedWay obstacleDestroyedWay = ObstacleDestroyedWay.None;

            if (destroyableDuringBoost)
            {
                obstacleDestroyedWay = ObstacleDestroyedWay.Boost;
            }
            else if (destroyableDuringDash)
            {
                obstacleDestroyedWay = ObstacleDestroyedWay.Dash;
            }
            else if (destroyableDuringArmour)
            {
                obstacleDestroyedWay = ObstacleDestroyedWay.Armour;
            }

            DestroyCar(obstacleDestroyedWay);
        }
    }

    public bool DestroyCar(ObstacleDestroyedWay obstacleDestroyedWay = ObstacleDestroyedWay.Forced)
    {
        if (parentObstacle.IsDestroying)
            return false;

        destructionSFx.ShootAudioEvent();
        PlayerSharedData.LastObstacleDestroyed = this;
        parentObstacle.DestroyTheObstacle(obstacleDestroyedWay);

        OnDestroyedCar.Invoke();

        return true;
    }

    public void ResetScript()
    {
        if (!isInitializedOnce)
            return;

        destroyDuringDash = OriginalComponent.destroyDuringDash;
        destroyDuringBoost = OriginalComponent.destroyDuringBoost;
        destroyDuringShockwave = OriginalComponent.destroyDuringShockwave;
        destroyDuringLaser = OriginalComponent.destroyDuringLaser;

        isDestroying = false;
    }

    public void HandleGotHitByShockWave()
    {
        DestroyCar(ObstacleDestroyedWay.ShockWave);
    }

    public void HandleGotHitByLaser()
    {
        DestroyCar(ObstacleDestroyedWay.Laser);
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void OnDrawGizmosSelected()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        Vector3 center = transform.TransformPoint(boxCollider.center);
        Vector3 maxAllowedTop = center + (Vector3.up * (boxCollider.bounds.size.y / 6));

        Gizmos.DrawRay(new Ray(maxAllowedTop, -transform.forward * 50f));
    }
}