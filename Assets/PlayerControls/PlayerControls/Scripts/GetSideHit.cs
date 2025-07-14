using UnityEngine;

public enum HitDirection { None, Top, Bottom, Forward, Back, Left, Right, TopEdge}

public static class GetSideHit
{
    /************************************************************
    ** Make sure to add rigidbodies to your objects.
    ** Place this script on your object not object being hit
    ** this will only work on a Cube being hit
    ** it does not consider the direction of the Cube being hit
    ** remember to name your C# script "GetSideHit"
    ************************************************************/

    public static HitDirection ReturnDirection(Transform hitObject, Vector3 theobject)
    {
        HitDirection hitDirection = HitDirection.None;

        Vector3 objectToHitObject = (hitObject.position - theobject);

        //   Vector3 objectToHitObject = (new Vector3(hitObject.position.x, 0, hitObject.position.z) - new Vector3(theobject.position.x, 0, theobject.position.z)).normalized;
        //   objectToHitObject.y = 0;
        //    Vector3 forwardVector = new Vector3(hitObject.forward.x, 0, hitObject.forward.z);
        //    float dot = Vector3.Dot(forwardVector, objectToHitObject);
        ////  UnityEngine.Console.Log("Initial Dot " + dot);

        //    // We are treating -1 as forward too
        //    if (dot >= 0.75f)
        //    {
        //        hitDirection = HitDirection.Forward;
        //    }
        //    else
        //    {
        //        float dotsecond = Vector3.Dot(hitObject.right, objectToHitObject);

        //    //    UnityEngine.Console.Log("Secondary dot " + dotsecond);

        //        if (dotsecond < 0)
        //        {
        //            hitDirection = HitDirection.Right;
        //        }
        //        else if (dotsecond >= 0)
        //        {
        //            hitDirection = HitDirection.Left;
        //        }
        //    }

        RaycastHit MyRayHit;
        Ray MyRay = new Ray(theobject, objectToHitObject);

        Debug.DrawRay(theobject, objectToHitObject * (Vector3.Distance(hitObject.position, theobject) + Mathf.Epsilon), Color.yellow, 5f);


    //    Debug.DrawRay(theobject, objectToHitObject * , Color.yellow, 5);

        if (Physics.Raycast(MyRay, out MyRayHit, Vector3.Distance(hitObject.position, theobject) + Mathf.Epsilon, 1 << LayerMask.NameToLayer("Obstacles"), QueryTriggerInteraction.Collide))
        {
        //    UnityEngine.Console.Log("RayCollided" );

            if (MyRayHit.collider != null)
            {
                Transform transform = MyRayHit.collider.transform;

                Vector3 MyNormal = MyRayHit.normal;

                //  MyNormal = transform.TransformDirection(MyNormal);

                if (MyNormal == transform.up) { hitDirection = HitDirection.Top; }
                if (MyNormal == -transform.up) { hitDirection = HitDirection.Bottom; }
                if (MyNormal == transform.forward) { hitDirection = HitDirection.Back; }
                if (MyNormal == -transform.forward) { hitDirection = HitDirection.Forward; }
                if (MyNormal == transform.right) { hitDirection = HitDirection.Right; }
                if (MyNormal == -transform.right) { hitDirection = HitDirection.Left; }

          //   Debug.DrawRay(theobject, MyNormal, Color.magenta, 5);
             

                if (hitDirection == HitDirection.None)
                {
                    UnityEngine.Console.LogWarning($"One of the Raycast {theobject} Was successful but no collision direction could be found. Normal was  {MyNormal}");
                }
            }
        }
     //   else
      //  {
       //     UnityEngine.Console.LogWarning("RayCast Missed " + theobject);
        //}
  // UnityEngine.Console.Log("FACE IS " + hitDirection);
        return hitDirection;
    }
}