using UnityEngine;
using UnityEditor;

public class UpdateKeyWindow : EditorWindow
{
    SerializedProperty keyValueProperty;
    string myString = "Hello World";

    public static void ShowWindow(SerializedObject serialObj,
        SerializedProperty propertyObj) {
        Debug.Log("Property Name = " + propertyObj.displayName + " || " +
            "Property Value = " + propertyObj.stringValue);

        UpdateKeyWindow windowObj = EditorWindow.GetWindow<UpdateKeyWindow>("Update Key");
        windowObj.keyValueProperty = propertyObj;
        windowObj.myString = propertyObj.stringValue;
        serialObj.ApplyModifiedProperties();
    }

    private void OnGUI()
    {
        GUILayout.Label("!! Lets Change the Key !!", 
            EditorStyles.boldLabel);

        myString = EditorGUILayout.TextField("New Key", myString);


        if (GUILayout.Button("Save Key"))
        {
            SerializedObject serialObj = keyValueProperty.serializedObject;
            keyValueProperty.stringValue = myString;

            serialObj.ApplyModifiedProperties();

            Debug.Log("Value Updated...");
            keyValueProperty = null;
            this.Close();
        }
    }
}
