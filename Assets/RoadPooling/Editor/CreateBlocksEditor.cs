using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CreateBlocks))]
public class CreateBlocksEditor : Editor
{
    float thumbnailWidth = 75;
    float thumbnailHeight = 75;

    Texture2D[] previewChunks;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();
        CreateBlocks createBlock = (CreateBlocks)target;

        createBlock.ArrayFilling();

        GUILayout.Label("lets make some blocks");


        createBlock.PositionArranger();
        if (createBlock.chunksAvailablelist != null)
        {
            Array.Resize(ref previewChunks, createBlock.chunksAvailablelist.Count);

            for (int i = 0; i < createBlock.chunksAvailablelist.Count; i++)
            {
                previewChunks[i] = AssetPreview.GetAssetPreview(createBlock.chunksAvailablelist[i]);
                if (GUILayout.Button(previewChunks[i], "button", GUILayout.Width(thumbnailWidth), GUILayout.Height(thumbnailHeight)))
                {
                    createBlock.AddChunk(i);
                }
            }
        }
        if (createBlock.Chunks != null)
        {
            if (Selection.activeGameObject != null)
            {
                if (createBlock.Chunks.Contains(Selection.activeGameObject))
                {
                    if (GUILayout.Button("delete it"))
                    {

                        createBlock.RemoveObject();
                    }
                }
            }
        }

        createBlock.folders_to_save = (CreateBlocks.MyEnum)EditorGUILayout.EnumPopup("select folder", createBlock.folders_to_save);
        if (GUILayout.Button("create_prefab"))
        {
            createBlock.save_as_prefab();
        }


        serializedObject.ApplyModifiedProperties();
    }
    private void OnSceneGUI()
    {
        CreateBlocks createBlock = (CreateBlocks)target;
        if (GameObject.Find("blockParent") == null)
        {
            if (createBlock.Chunks != null)
                createBlock.Chunks.Clear();
        }


    }

}
