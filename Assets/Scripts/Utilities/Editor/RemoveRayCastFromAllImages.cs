using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class RemoveRayCastFromAllImages : EditorWindow
{
    [SerializeField] private Transform parentT;

    private SerializedObject serializedObject;
    private SerializedProperty parentTSerializedProperty;

    [MenuItem("Tools/RayCastRemoverFromImages")]
    private static void Init()
    {
        RemoveRayCastFromAllImages window = (RemoveRayCastFromAllImages)EditorWindow.GetWindow(typeof(RemoveRayCastFromAllImages));
        window.Show();
    }

    private void OnEnable()
    {
        serializedObject = new SerializedObject(this);
    }

    private void OnGUI()
    {
        serializedObject.Update();

        parentTSerializedProperty = serializedObject.FindProperty("parentT");
        EditorGUILayout.PropertyField(parentTSerializedProperty);

        if (GUILayout.Button("Remove"))
        {
            TraverseHierarchy(parentT);
        }

        serializedObject.ApplyModifiedProperties();
    }


    private void TraverseHierarchy(Transform parent)
    {
        //foreach (Transform child in parent)
        //{
        //    Debug.Log("ChildName " + child.name);
        //    TraverseHierarchy(child);
        //}

        var images = parent.GetComponentsInChildren<Image>(true);

        foreach (var image in images)
        {
            if(image.GetComponent<Button>() == null && image.transform.parent.GetComponent<Button>() == null)
            {
                image.raycastTarget = false;
            }
        }
    }
}
