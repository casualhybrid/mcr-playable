using UnityEngine;

public class LockMovementOnPlayer : MonoBehaviour
{
    [SerializeField] private PlayerData playerData;
    [SerializeField] private PlayerSharedData playerSharedData;
    [SerializeField] private bool yPosInLastGrounded;
    [SerializeField] private bool lockYToGround;

    [SerializeField] private bool x;
    [SerializeField] private bool y;
    [SerializeField] private bool z;

    private void Awake()
    {
        SetPosition();
    }

    private void LateUpdate()
    {
        SetPosition();
    }

    private void SetPosition()
    {
        float xPosition = !x ? playerSharedData.PlayerTransform.position.x : transform.position.x;
        float yPosition = !y ? playerSharedData.PlayerTransform.position.y : (yPosInLastGrounded) ? playerSharedData.LastGroundedYPosition : lockYToGround ? playerData.PlayerInformation[0].PlayerStartinPosition.y :  transform.position.y;
        float zPosition = !z ? playerSharedData.PlayerTransform.position.z : transform.position.z;

        transform.position = new Vector3(xPosition, yPosition, zPosition);
    }
}