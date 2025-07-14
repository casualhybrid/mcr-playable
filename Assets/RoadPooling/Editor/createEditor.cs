using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(create))]
public class createEditor : Editor
{


    private create _create;

    float thumbnailWidth = 75;
    float thumbnailHeight = 75;

    Vector2 scrollPosition_1, scrollPosition_2;



    Texture2D[] preview_pickups, preview_hurdles;
    public override void OnInspectorGUI()
    {


        base.DrawDefaultInspector();
        create myScript = (create)target;

        myScript.array_filling();





        if (GUILayout.Button("positionHolders"))
        {
            myScript.positionHolders();
        }


        GUILayout.Space(20f);
        GUILayout.Label("Spawn Pickups");



        scrollPosition_1 = GUILayout.BeginScrollView(scrollPosition_1, true, true, GUILayout.Width(500), GUILayout.Height(200));



        if (myScript.pickups_list != null)
        {

            Array.Resize(ref preview_pickups, myScript.pickups_list.Count);

            for (int i = 0; i < myScript.pickups_list.Count; i++)
            {


                preview_pickups[i] = AssetPreview.GetAssetPreview(myScript.pickups_list[i]);



                if (GUILayout.Button(preview_pickups[i], "button", GUILayout.Width(thumbnailWidth), GUILayout.Height(thumbnailHeight)))
                {

                    myScript.pickup_placement(i);
                }
            }
        }

        GUILayout.EndScrollView();

        GUILayout.Space(20f);
        GUILayout.Label("Spawn Hurdles");


        EditorGUILayout.BeginHorizontal();
        scrollPosition_2 = GUILayout.BeginScrollView(scrollPosition_2, true, false, GUILayout.Width(500), GUILayout.Height(200));
        if (myScript.hurdles_list != null)
        {

            Array.Resize(ref preview_hurdles, myScript.hurdles_list.Count);

            for (int i = 0; i < myScript.hurdles_list.Count; i++)
            {

                preview_hurdles[i] = AssetPreview.GetAssetPreview(myScript.hurdles_list[i]);

                if (GUILayout.Button(preview_hurdles[i], "button", GUILayout.Width(thumbnailWidth), GUILayout.Height(thumbnailHeight)))
                {

                    myScript.hurdles_placement(i);
                }
            }

        }

        GUILayout.EndScrollView();
        EditorGUILayout.EndHorizontal();

        myScript.folders_to_save = (create.MyEnum)EditorGUILayout.EnumPopup("select folder", myScript.folders_to_save);



        if (GUILayout.Button("create_prefab"))
        {
            create.instance.save_as_prefab();
        }


        serializedObject.ApplyModifiedProperties();
    }







    void OnSceneGUI()
    {
        _create = target as create;

        if (_create.editType.ToString() != "None")
        {
            if (Selection.activeGameObject != null)
            {
                if (Selection.activeGameObject.GetComponent<draw_gizmo>())
                {
                    Selection.activeGameObject.GetComponent<draw_gizmo>()._mycolor = Color.red;
                    add_in_list(Selection.activeGameObject);
                }
            }
        }

        else if (_create.editType.ToString() == "None")
        {
            clear_positions();
        }

    }

    void add_in_list(GameObject index)
    {
        if (!_create.selected_position_list.Contains(index))
        {
            _create.selected_position_list.Add(index);
        }
    }



    void clear_positions()
    {

        if (_create.selected_position_list != null)
            _create.selected_position_list.Clear();
    }
}
