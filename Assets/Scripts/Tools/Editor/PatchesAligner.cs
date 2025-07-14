using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PatchesAligner : EditorWindow
{
    [SerializeField] private List<GameObject> patchesToAlign;

    private SerializedObject serializedObject;
    private SerializedProperty serializedPatchesToAlignProperty;

    [MenuItem("Tools/PatchesAligner")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(PatchesAligner));
    }

    private void Awake()
    {
        patchesToAlign?.Clear();
        serializedObject = new SerializedObject(this);
        serializedPatchesToAlignProperty = serializedObject.FindProperty("patchesToAlign");
    }

    private void OnDisable()
    {
        patchesToAlign?.Clear();
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.PropertyField(serializedPatchesToAlignProperty);

        EditorGUILayout.Space(200);

        if (GUILayout.Button("AlignPatches"))
        {
            AlignPatches();
        }
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }

    //private void AlignPatches()
    //{
    //    float lastEndingZPos = 0;

    //    foreach (var item in patchesToAlign)
    //    {
    //        Patch patch = item.GetComponent<Patch>();

    //        if(patch == null)
    //        {
    //            throw new System.Exception($"Patch Behaviour is not attached to the gameobject {item.name}");
    //        }

    //        Vector3 patchPos;

    //        if (item.transform.GetSiblingIndex() != 0)
    //        {
    //            patchPos = patch.transform.position;
    //            patchPos.z = lastEndingZPos - patch.DiffBWPivotAndColliderMinBoundPoint;
    //            patch.transform.position = patchPos;
    //        }
    //        else
    //        {
    //            patchPos = patch.transform.position;
    //        }

    //        lastEndingZPos = patchPos.z + patch.DiffBWPivotAndColliderMaxBoundPoint;
    //    }
    //}

    private void AlignPatches()
    {
        Transform rootT = patchesToAlign.Count > 0 ? patchesToAlign[0].transform.root : null;
        float lastEndingZPos = 0;

        for (int i = 0; i < patchesToAlign.Count; i++)
        {
            GameObject item = patchesToAlign[i];

            BoxCollider boxCollider = item.GetComponentInChildren<BoxCollider>();

            if (boxCollider == null)
            {
                throw new System.Exception($"No Box collider found for size detection on the gameobject {item.name}");
            }

            float DiffBWPivotAndColliderMinBoundPoint = boxCollider.bounds.min.z - item.transform.position.z;
            float DiffBWPivotAndColliderMaxBoundPoint = boxCollider.bounds.max.z - item.transform.position.z;

            Vector3 patchPos;

            if (i != 0)
            {
                patchPos = item.transform.position;
                patchPos.z = lastEndingZPos - DiffBWPivotAndColliderMinBoundPoint;

                //Undo.RecordObject(item, "Changed Patch Position");
                item.transform.position = patchPos;
                // PrefabUtility.RecordPrefabInstancePropertyModifications(item);
            }
            else
            {
                //  Undo.RecordObject(item, "Changed Patch Position");
                patchPos = item.transform.position;
                //  PrefabUtility.RecordPrefabInstancePropertyModifications(item);
            }

            lastEndingZPos = patchPos.z + DiffBWPivotAndColliderMaxBoundPoint;
        }

        if (patchesToAlign.Count > 0)
        {
            EditorUtility.SetDirty(rootT);
        }

        Debug.Log("Parches ");
    }
}