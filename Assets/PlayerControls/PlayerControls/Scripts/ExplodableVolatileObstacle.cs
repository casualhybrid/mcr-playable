using UnityEngine;
using UnityEngine.Events;

public class ExplodableVolatileObstacle : MonoBehaviour
{
    [SerializeField] private PlayerSharedData playerSharedData;
    [SerializeField] private RequestPlayerGotHitChannel requestPlayerGotHitChannel;
    [SerializeField] private UnityEvent onNormalExplode = new UnityEvent();
    [SerializeField] private UnityEvent onDashExplode = new UnityEvent();
    [SerializeField] private Vector3 explosionBox;
    [SerializeField] private float explosionBoxCenterOffset;

    public HitDirection DirectionThePlayerHitFrom { get; set; }

    public void MakeScrappyExplode()
    {

        Collider[] col = new Collider[1];
        
        Vector3 center = transform.position;
        center.z -= explosionBoxCenterOffset;

        int detectedColider = Physics.OverlapBoxNonAlloc(center, explosionBox * .5f, col, Quaternion.identity, 1 << LayerMask.NameToLayer("DefaultHitBox"), QueryTriggerInteraction.Collide);

        if (detectedColider < 1)
        {
            ShowExplosionEffect();
            return;
        }

        ShowExplosionEffect();
        requestPlayerGotHitChannel.RaiseRequestToStumblePlayer();
    }

    private void ShowExplosionEffect()
    {
        if(playerSharedData.IsDash)
        {
            onDashExplode.Invoke();
        }
        else
        {
            onNormalExplode.Invoke();
        }
    }
}