//using System.Reflection;
//using UnityEditor;

//public class ClearConsoleScript
//{
//    [MenuItem("Tools/Clear Console _c")] // C
//    static void ClearConsole()
//    {
//        var assembly = Assembly.GetAssembly(typeof(SceneView));
//        var type = assembly.GetType("UnityEditor.LogEntries");
//        var method = type.GetMethod("Clear");
//        method.Invoke(new object(), null);
//    }
//}
