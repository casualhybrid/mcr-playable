using System.Linq;
using UnityEngine;

public static class StackDestroyerShockWave
{
    private static Vector3 overLapBoxSizeForStackDestruction = new Vector3(.2f,0,.2f);

    public static void DestroyTheStack(Vector3 startingPosition, float endingHeight)
    {
      //  UnityEngine.Console.Log($"Destroying the STACK");

            Vector3 upperPos = startingPosition;
            float groundHeight = endingHeight;

            float midY = (groundHeight + upperPos.y) * .5f;
            Vector3 boxCenter = new Vector3(upperPos.x, midY, upperPos.z);

            overLapBoxSizeForStackDestruction.y = groundHeight + upperPos.y;
            //    Gizmos.DrawCube(boxCenter, size);

            Collider[] colliders = Physics.OverlapBox(boxCenter, overLapBoxSizeForStackDestruction * .5f, Quaternion.identity, 1 << LayerMask.NameToLayer("Obstacles"), QueryTriggerInteraction.Collide);
            var list = colliders.OrderBy((x) => upperPos.y - x.bounds.center.y).TakeWhile((x) =>
            {
                var od = x.GetComponent<ObjectDestruction>();
                return od != null && od.isDestroyDuringShockwave;
            }

            );

            foreach (var item in list)
            {
                var od = item.GetComponent<ObjectDestruction>();
                od.DestroyCar(ObstacleDestroyedWay.ShockWave);
            }
       
    }
}