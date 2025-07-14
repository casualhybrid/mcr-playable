using System.Collections;
using UnityEngine;

public class CoinPickup : Pickup, IFloatingReset
{
    [SerializeField] private bool isRampBuildingCoin = false;
    [SerializeField] private bool isDoubleCoin;
    public bool isActive { get; private set; }
    public bool isMovingTowardsTarget { get; private set; }

    public Collider MainCollider;

    [SerializeField] private float moveVelocityPercentFromPlayer;
    [SerializeField] private PlayerSharedData playerRunTimeData;
    [SerializeField] private AudioPlayer coinAudioPlayer;
   /* [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip audioClip;*/

    public AudioPlayer GetCoinAudioPlayer => coinAudioPlayer;

    public bool IsDoubleCoin => isDoubleCoin;

    public bool ShoudNotOffsetOnRest { get; set; } = true;

    private void OnEnable()
    {
        MainCollider.enabled = true;
        isActive = true;
    }

    private void OnDisable()
    {
        ShoudNotOffsetOnRest = true;
        isActive = false;
        isMovingTowardsTarget = false;
    }

    public void MoveTowardsPlayer(Transform targetT)
    {
        isMovingTowardsTarget = true;
        float playerVelocityMag = playerRunTimeData.PlayerRigidBody.velocity.magnitude;
        float speed = (transform.position.z < playerRunTimeData.PlayerTransform.position.z) ? playerVelocityMag + MathExtensions.GetAmountByPercent(playerVelocityMag, moveVelocityPercentFromPlayer * 10f) :
            playerVelocityMag * 0.5f;
        Vector3 pos = Vector3.MoveTowards(transform.position, targetT.position, Time.deltaTime * speed);
        transform.position = pos;
    }

    public IEnumerator FinishMovingTowardsPlayer(Transform targetT)
    {
        while (true)
        {
            MoveTowardsPlayer(targetT);

            yield return null;
        }
    }

    public void OnBeforeFloatingPointReset()
    {
    }

    public void OnFloatingPointReset(float movedOffset)
    {
    }

    public void PlayCoinSFX()
    {
        var config = coinAudioPlayer.AudioInstanceConfig;
        config.parameterName = "State";
        config.parameterValue = playerRunTimeData.IsGrounded || playerRunTimeData.isJumpingPhase ? 1 : 2;
        /* audioSource.Play();
         Debug.Log("PlayCoinSfx");*/
        PersistentAudioPlayer.Instance.PlayCoinSFX();
        //coinAudioPlayer.ShootAudioEvent();
    }
}