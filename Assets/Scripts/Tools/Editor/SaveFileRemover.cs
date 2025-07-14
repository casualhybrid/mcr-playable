using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class SaveFileRemover : Editor
{
    [MenuItem("Tools/RemoveSaveFile")]
    public static void RemoveSaveFile()
    {
        string path = Application.persistentDataPath;

        if (!Directory.Exists(path))
        {
            Debug.LogError($"Failed removing saved files as no directory exists at {path}");
            return;
        }

        var files = Directory.EnumerateFiles(path);

        bool exception = false;
        string filePath = "";

        foreach (var file in files)
        {
            try
            {
                exception = false;
                filePath = file;
                File.Delete(file);
            }
            catch (Exception e)
            {
                exception = true;
                Debug.LogError($"Failed to delete file {filePath} because of {e} ");
            }
            finally
            {
                if (!exception)
                {
                    Debug.Log($"Save file successfully deleted {filePath}");
                }
            }
        }
    }
}