using System.Collections.Generic;
using UnityEngine;

public class EditorPathScript : MonoBehaviour
{
    public Color LineColor = Color.white;

    [SerializeField] private List<Transform> PathObjs = new List<Transform>();

    public Vector3[] GetPath()
    {
        Vector3[] path = new Vector3[PathObjs.Count];
        for (int i = 0; i < PathObjs.Count; i++)
        {
            path[i] = PathObjs[i].position;
        }

        return path;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = LineColor;

        PathObjs.Clear();

        int k = 0;
        foreach (Transform PathObj in transform)
        {
            PathObjs.Add(PathObj);
        }

        for (int i = 0; i < PathObjs.Count; i++)
        {
            Vector3 pos = PathObjs[i].position;
            if (i > 0)
            {
                Vector3 previous = PathObjs[i - 1].position;
                Gizmos.DrawLine(previous, pos);
                Gizmos.DrawWireSphere(pos, 0.1f);
            }
        }
    }
}