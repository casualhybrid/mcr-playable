using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class MATS_ColliderGizmoDrawer : MonoBehaviour
{
    [Header("Gizmo Settings")]
    public bool showGizmo = true;
    public Color gizmoColor = new Color(0f, 1f, 0f, 0.25f);
    public bool filled = true;

    [Header("Gizmo Mode")]
    public bool useColliderGizmo = true; // true = collider, false = mesh

    [Header("Mesh Gizmo Settings")]
    public Mesh specificMesh;
    public Vector3 meshScale = Vector3.one;

    private void OnDrawGizmos()
    {
        if (!showGizmo) return;

        Gizmos.color = gizmoColor;

        if (useColliderGizmo)
        {
            DrawColliderBounds();
        }
        else if (specificMesh != null)
        {
            DrawMeshGizmo();
        }
    }

    private void DrawColliderBounds()
    {
        Collider col = GetComponent<Collider>();
        Collider2D col2D = GetComponent<Collider2D>();

        if (col != null)
        {
            Bounds b = col.bounds;

            if (filled)
                Gizmos.DrawCube(b.center, b.size);
            else
                Gizmos.DrawWireCube(b.center, b.size);
        }
        else if (col2D != null)
        {
            Bounds b = col2D.bounds;

            if (filled)
                Gizmos.DrawCube(b.center, b.size);
            else
                Gizmos.DrawWireCube(b.center, b.size);
        }
    }

    private void DrawMeshGizmo()
    {
        // Apply transform + custom scale
        Matrix4x4 matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.Scale(transform.lossyScale, meshScale));
        Gizmos.matrix = matrix;

        if (filled)
            Gizmos.DrawMesh(specificMesh);
        else
            Gizmos.DrawWireMesh(specificMesh);
    }
}