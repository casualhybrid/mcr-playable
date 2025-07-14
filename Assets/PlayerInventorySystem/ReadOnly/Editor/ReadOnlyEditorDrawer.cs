using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyEditorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label);
        GUI.enabled = true;

        GUILayout.BeginHorizontal("Box");


        if (GUILayout.Button("Copy Key"))
        {
            EditorGUIUtility.systemCopyBuffer = property.stringValue;
        }

        if (GUILayout.Button("Update Key"))
        {
            SerializedObject serialObj = property.serializedObject;
            UpdateKeyWindow.ShowWindow(serialObj, property);
        }


        GUILayout.EndHorizontal();
    }

}
