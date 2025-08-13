using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ApplyGravityIfNoContact : MonoBehaviour
{
    [SerializeField] private float gravity;

    [SerializeField] private Transform rootT;
    [SerializeField] private BoxCollider boxCollider;
    [SerializeField] private Rigidbody rigidBody;

    private float rayLength;
    private bool applyingGravity = false;
    private Vector3 rayOrigin;
    private float diffBetweenBoundingBoxCenterAndRootPivot;

    private /*readonly*/ List<Collider> collidersToIgnore = new List<Collider>();
    private ObjectDestruction belowObstacle;
    private Collider belowCollider;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    //private void Start()
    //{
    //    diffBetweenBoundingBoxCenterAndRootPivot = (boxCollider.bounds.center.y - rootT.position.y);
    //    diffBetweenBoundingBoxCenterAndRootPivot += (Mathf.Sign(diffBetweenBoundingBoxCenterAndRootPivot) * Mathf.Epsilon); //Buffer
    //    rayLength = boxCollider.bounds.extents.y + 0.05f;
    //}

    private void OnEnable()
    {
        diffBetweenBoundingBoxCenterAndRootPivot = (boxCollider.bounds.center.y - rootT.position.y);
        diffBetweenBoundingBoxCenterAndRootPivot += (Mathf.Sign(diffBetweenBoundingBoxCenterAndRootPivot) * Mathf.Epsilon); //Buffer
        rayLength = boxCollider.bounds.extents.y + 0.05f;

        applyingGravity = IsTheCarInAir();
    }

    private void OnDisable()
    {
        ClearBelowObstacle();
    }

    private void FixedUpdate()
    {
        if (!applyingGravity)
            return;

        Vector3 pos = rootT.position;
        float gravityToApply = gravity * Time.fixedDeltaTime;
        pos.y += gravityToApply;

        float colliderBottomPointYPos = (/*boxCollider.transform.position.y +*/ boxCollider.transform.TransformPoint(boxCollider.center).y + gravityToApply) - (boxCollider.size.y / 2);
        if (colliderBottomPointYPos < 0)
        {
            pos.y += Mathf.Abs(colliderBottomPointYPos); // Moves the object upwards such that only the bottom of the collider touches the ground
        }

        // NAN error sometimes?
        rigidBody.MovePosition(pos);
    }

    private void OnTriggerEnter(Collider other)
    {
        ObjectDestruction objectDestruction = other.transform.parent.GetComponentInChildren<ObjectDestruction>();

        if (objectDestruction != null && objectDestruction.isDestroying)
            return;

        if (other.gameObject.layer != LayerMask.NameToLayer("Stack") && other.gameObject.layer != LayerMask.NameToLayer("Walkable"))
            return;

        if (!applyingGravity)
            return;

        // If there's no car downwards
        if (IsTheCarInAir())
        {
         //   UnityEngine.Console.Log($"Trigger with {other.transform.parent.name} Entered But there's no landing surface downwards", this.gameObject);
            return;
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Stack") || other.gameObject.layer == LayerMask.NameToLayer("Walkable") /*|| other.gameObject.layer == LayerMask.NameToLayer("WalkableLimited")*/)
        {
            //   UnityEngine.Console.Log("Falling Gravity False", this.gameObject);

            applyingGravity = false;

            // UnityEngine.Console.Log($"Triggered with {other.transform.parent.name} While Falling Gravity OFF to position {rootT.position.y} frame {frame}", this.gameObject);

            // UnityEngine.Console.Log($"Top {other.bounds.max} and extent Y {boxCollider.bounds.extents.y}");

            //  UnityEngine.Console.Log("Bounds Position Before Adjustement " + boxCollider.bounds);

            float adjustedYPos = other.bounds.max.y + boxCollider.bounds.extents.y;
            adjustedYPos -= diffBetweenBoundingBoxCenterAndRootPivot;
            Vector3 adjustedPos = new Vector3(rootT.position.x, adjustedYPos, rootT.position.z);

            rigidBody.interpolation = RigidbodyInterpolation.None;
            rootT.position = adjustedPos;
            //  rigidBody.interpolation = RigidbodyInterpolation.Interpolate;  //zzzn

            //   UnityEngine.Console.Log($"Adjusted Positions Are {rootT.localPosition.y} GameObject {this.name}",this.gameObject);

            //   UnityEngine.Console.Log("Bounds Position After Adjustement " + boxCollider.bounds);

            //    UnityEngine.Console.Log("Adjusted Position Is " + adjustedPos);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        HandleObstacleContactEnded(other);
    }

    private void HandleContactCarDestroyed()
    {
        HandleObstacleContactEnded(belowCollider);
        ClearBelowObstacle();
    
    }

    private void HandleObstacleContactEnded(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Stack") && other.gameObject.layer != LayerMask.NameToLayer("Walkable") /*&& other.gameObject.layer != LayerMask.NameToLayer("WalkableLimited")*/)
            return;

        if (collidersToIgnore.Contains(other))
            return;

        ObjectDestruction objectDestruction = other.transform.parent.GetComponentInChildren<ObjectDestruction>();

        if (objectDestruction != null && objectDestruction.isDestroying)
        {
            collidersToIgnore.Add(other);
        }

        // UnityEngine.Console.Log($"Triggered Exit with {other.name} ",this.gameObject);

        if (!applyingGravity)
            applyingGravity = IsTheCarInAir();
        //   UnityEngine.Console.Log($"applying gravity is set to {applyingGravity} for {gameObject.name} because of {other.transform.parent.gameObject.name}", gameObject);

        //  UnityEngine.Console.Log("Gravity Applying Status : " + applyingGravity, this.gameObject);
    }

    private bool IsTheCarInAir()
    {
        rayOrigin = boxCollider.bounds.center;

        Ray ray = new Ray(rayOrigin, Vector3.down);
        RaycastHit[] raycastHits = new RaycastHit[1];
        int layerMask = 1 << LayerMask.NameToLayer("Stack") /*| 1 << LayerMask.NameToLayer("Walkable")*/ /*| 1 << LayerMask.NameToLayer("WalkableLimited")*/;
        Physics.RaycastNonAlloc(ray, raycastHits, rayLength, layerMask, QueryTriggerInteraction.Collide);

        //   UnityEngine.Console.Log($"InAirStatus {raycastHits[0].collider == null}", this.gameObject);

        if (raycastHits[0].collider == null )
        {
            Debug.DrawRay(rayOrigin, Vector3.down * rayLength, Color.red,1);
        }
        else
        {
          //  UnityEngine.Console.Log($"Detected Collider is {raycastHits[0].collider.name}", this.gameObject);
            Debug.DrawRay(rayOrigin, Vector3.down * rayLength, Color.green,1);

            ClearBelowObstacle();
            belowObstacle = raycastHits[0].collider.transform.parent.GetComponentInChildren<ObjectDestruction>(); 
            belowCollider = raycastHits[0].collider;
            belowObstacle.OnDestroyedCar.AddListener(HandleContactCarDestroyed);
        }

        return raycastHits[0].collider == null;
    }

    private void OnDrawGizmos()
    {
        rayOrigin = boxCollider.bounds.center;

        Gizmos.DrawSphere(rayOrigin, .1f);

        //  Ray ray = new Ray(rayOrigin, Vector3.down * rayLength);
        // Gizmos.DrawRay(ray);
        Gizmos.DrawLine(rayOrigin, rayOrigin + (Vector3.down * rayLength));
    }

    private void ClearBelowObstacle()
    {
        if (belowObstacle != null)
        {
            belowObstacle.OnDestroyedCar.RemoveListener(HandleContactCarDestroyed);
            belowObstacle = null;
        }

        belowCollider = null;
    }

    public void ResetScript()
    {
        ClearBelowObstacle();
        collidersToIgnore.Clear();
        StartCoroutine(WaitAndCheckIfCarIsInAir());
    }

    private IEnumerator WaitAndCheckIfCarIsInAir()
    {
        yield return new WaitForFixedUpdate();
        applyingGravity = IsTheCarInAir(); // should wait for a fixed update?
    }
}