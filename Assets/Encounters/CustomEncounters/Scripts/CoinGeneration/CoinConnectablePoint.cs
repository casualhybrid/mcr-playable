using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public class CoinConnectablePoint : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerData playerData;
    public bool shouldReadjustToGroundAfterSpawn;

#if UNITY_EDITOR

#region Editor Tools
    [Button("Add Jump Height")]
    public void AddJumpHeight()
    {
        Undo.RecordObject(gameObject.transform, "Added jump height");
        transform.position += Vector3.up * playerData.PlayerInformation[0].jump_height;
    }

#region Alignment to columns
    [ButtonGroup]
    public void AlignToLeftColumn()
    {
        AlignToColumn(-1);
    }

    [ButtonGroup]
    public void AlignToMiddleColumn()
    {
        AlignToColumn(0);
    }

    [ButtonGroup]
    public void AlignToRightColumn()
    {
        AlignToColumn(1);
    }

    private void AlignToColumn(int columnXPos)
    {
        if (transform.position.x == columnXPos)
        {
            return;
        }

        Undo.RecordObject(gameObject.transform, "Aligned to column with X position " + columnXPos);
        transform.position += Vector3.right * (- transform.position.x + columnXPos);
    }
#endregion

    [Button("Align to ground")]
    public void AlignToGroundWithUndo()
    {
        Undo.RecordObject(gameObject.transform, "Aligned to ground");
        PrefabUtility.RecordPrefabInstancePropertyModifications(gameObject.transform);

        AlignToGround();
    }
#endregion

#endif

#region Functionality
    public void AlignToGround()
    {
        LayerMask groundLayers = 1 << LayerMask.NameToLayer("Obstacles") | 1 << LayerMask.NameToLayer("Walkable") | 1 << LayerMask.NameToLayer("WalkableLimited");
        float raycastOriginYOffset = 0.001f;
        float overlapSphereRadius = 0.001f;

        Collider[] collidersOverlapped = new Collider[10];

        RaycastHit lastSuccessfulHitData = new RaycastHit();
        bool hasRaycastHit;

        bool hasAnyRaycastHit = false;

        // If point is inside a collider, raycast would always fail so to avoid it, move below the collider
        int collidersOverlappedCount = gameObject.scene.GetPhysicsScene().OverlapSphere(transform.position, overlapSphereRadius, collidersOverlapped, groundLayers, QueryTriggerInteraction.Collide);

        if (collidersOverlappedCount > 0)
        {
            foreach(Collider colliderOverlapped in collidersOverlapped)
            {
                if (colliderOverlapped == null)
                    continue;

                if (colliderOverlapped.bounds.Contains(transform.position))
                {
                    transform.position = new Vector3(transform.position.x, colliderOverlapped.bounds.center.y - (colliderOverlapped.bounds.extents.y + (raycastOriginYOffset * 2f)), transform.position.z);
                    break;
                }
            }
        }

        // Move up till the bottom of the hightest obstacle
        do
        {
            hasRaycastHit = gameObject.scene.GetPhysicsScene().Raycast(transform.position + (Vector3.up * raycastOriginYOffset), Vector3.up, out RaycastHit raycastUpHitData, Mathf.Infinity, groundLayers, QueryTriggerInteraction.Collide);

            if (hasRaycastHit)
            {
                UnityEngine.Console.LogError("Hit");
                transform.position += Vector3.up * (raycastUpHitData.distance + raycastOriginYOffset);

                lastSuccessfulHitData = raycastUpHitData;
                hasAnyRaycastHit = true;
            }
        }
        while (hasRaycastHit);

        // Move up above the highest obstacle
        if (hasAnyRaycastHit) // Makes sure lastSuccessfulHitData is not null
        {
            if (lastSuccessfulHitData.collider != null)
            {
                UnityEngine.Console.LogError($"{lastSuccessfulHitData.collider.bounds.size.y}");
            }

            transform.position += (Vector3.up * lastSuccessfulHitData.collider.bounds.size.y);
        }

        // Move down on top of the highest obstacle
        hasRaycastHit = gameObject.scene.GetPhysicsScene().Raycast(transform.position + (Vector3.up * raycastOriginYOffset), Vector3.down, out RaycastHit raycastDownHitData, Mathf.Infinity, groundLayers, QueryTriggerInteraction.Collide);

        if (hasRaycastHit)
        {
            transform.position -= Vector3.up * (raycastDownHitData.distance - raycastOriginYOffset);

            hasAnyRaycastHit = true;
        }

        // If no raycast hit, move down to the ground
        if (!hasAnyRaycastHit)
        {
            Vector3 currentPos = transform.position;
            currentPos.y = 0;
            transform.position = currentPos;
        }
    }
#endregion

#region Unity Callbacks
    private void Start()
    {
        if(shouldReadjustToGroundAfterSpawn)
        {
            AlignToGround();
        }
    }
#endregion
}
