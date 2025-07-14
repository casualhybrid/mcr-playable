using UnityEditor;
using UnityEngine;
[ExecuteInEditMode]
public class draw_gizmo : MonoBehaviour
{
    public Color _mycolor;
    void OnDrawGizmos()
    {
        if (create.instance != null)
        {

            if (!create.instance.selected_position_list.Contains(Selection.activeGameObject))
            {
                _mycolor = Color.green;
            }
            Gizmos.color = _mycolor;
            Gizmos.DrawSphere(transform.position, 0.1f);


        }
    }





}
