using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TheKnights.Purchasing.Editor
{
    [CustomEditor(typeof(IAPManager))]
    public class IAPManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.HelpBox("Generates a static class containing all of the IAP Product ID's as specified in dictionary keys", MessageType.Info);

            if (GUILayout.Button("Generate"))
            {
                GenerateProductIDsScript();
            }
        }

        private void GenerateProductIDsScript()
        {
            string copyPath = Application.dataPath + "/Scripts/ProductIDs.cs";

            List<string> textList = new List<string>();
            textList.Add("public class ProductIDs");
            textList.Add("{");

            IAPManager iAPManager = target as IAPManager;
            string[] keysArray = iAPManager.GetIAPDictionaryKeys;

            for (int i = 0; i < keysArray.Length; i++)
            {
                string doubleColon = "\"";
                string idName = keysArray[i];

                textList.Add("public const string " + idName + "=" + doubleColon + idName + doubleColon + ";");
            }

            textList.Add("}");

            File.WriteAllLines(copyPath, textList.ToArray());
        }
    }
}