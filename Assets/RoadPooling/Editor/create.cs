
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class create : MonoBehaviour
{
    public static create instance;


    public Vector3 initial_point = new Vector3(-7.5f, 0, 5);
    [Space, Space]
    public GameObject chunk, IntelliChunk;
    [Space, Space]
    //[HideInInspector]
    public GameObject point;
    [Space, Space]
    public float distance_btwn_row = 35;
    public float distance_btwn_col = 7;
    public int number_of_rows = 5, number_of_col = 3;

    [HideInInspector]
    public string[] pickups_info_array;


    string[] pickups_name_split;
    [HideInInspector]
    public List<GameObject> positions_list;


    [Space, Space]
    [HideInInspector]
    public List<GameObject> pickups_list;
    [Space, Space]
    [HideInInspector]
    public List<GameObject> hurdles_list;

    GameObject obj_parent_pos, obj_parent_pickup, obj_parent_hurdle = null;

    [HideInInspector]
    public MyEnum folders_to_save = new MyEnum();

    public List<GameObject> selected_position_list;

    public enum EditType
    {
        Add_inlist, None
    }

    public EditType editType;


    private void OnEnable()
    {

        point = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/RoadPooling/Prefabs/EditorOnly/point.prefab", typeof(GameObject));


    }
    private void Update()
    {
        instance = this;
    }

    public void array_filling()
    {
        pickups_list = new List<GameObject>();
        hurdles_list = new List<GameObject>();
        string[] pickup_name = AssetDatabase.FindAssets("t:GameObject", new string[] { "Assets/RoadPooling/Prefabs/Props/Pickups" });
        for (int i = 0; i < pickup_name.Length; i++)
        {
            string lAssetPath = AssetDatabase.GUIDToAssetPath(pickup_name[i]);
            if (!pickups_list.Contains(AssetDatabase.LoadAssetAtPath<GameObject>(lAssetPath)))
                pickups_list.Add(AssetDatabase.LoadAssetAtPath<GameObject>(lAssetPath));
        }
        string[] hurdles_name = AssetDatabase.FindAssets("t:GameObject", new string[] { "Assets/RoadPooling/Prefabs/Props/Hurldes" });
        for (int i = 0; i < hurdles_name.Length; i++)
        {
            string lAssetPath = AssetDatabase.GUIDToAssetPath(hurdles_name[i]);
            if (!hurdles_list.Contains(AssetDatabase.LoadAssetAtPath<GameObject>(lAssetPath)))
                hurdles_list.Add(AssetDatabase.LoadAssetAtPath<GameObject>(lAssetPath));
        }

    }


    public enum MyEnum
    {
        easyfolder,
        mediumfolder,
        hardfolder
    };


    public void positionHolders() // still have to work for initial location of first object
    {
        if (chunk == null)
        {
            return;
        }


        float distance_btwn_row_temp = distance_btwn_row;
        float distance_btwn_col_temp = distance_btwn_col;

        if (GameObject.Find("position_holder") == null)
        {
            obj_parent_pos = new GameObject("position_holder");
            // set position of parent
            obj_parent_pos.transform.position = new Vector3(initial_point.x, initial_point.y, initial_point.z);
            obj_parent_pos.transform.parent = chunk.transform;
        }


        for (int i = 0; i < number_of_rows; i++)
        {
            if (i == 0)
            {
                distance_btwn_row_temp = obj_parent_pos.transform.position.z;
            }
            GameObject point_temp = PrefabUtility.InstantiatePrefab(point) as GameObject;

            point_temp.transform.position = new Vector3(obj_parent_pos.transform.position.x, 0, distance_btwn_row_temp);
            point_temp.transform.rotation = Quaternion.identity;
            point_temp.transform.SetParent(obj_parent_pos.transform);

            for (int j = 0; j < number_of_col - 1; j++)
            {
                if (j == 0)
                {
                    point_temp.name = "r" + i + "c" + j;
                    positions_list.Add(point_temp);
                    distance_btwn_col_temp = distance_btwn_col_temp + point_temp.transform.position.x;
                }
                GameObject point_temp_1 = Instantiate(point, new Vector3(distance_btwn_col_temp, 0, distance_btwn_row_temp), Quaternion.identity, obj_parent_pos.transform);
                distance_btwn_col_temp = distance_btwn_col + distance_btwn_col_temp;
                point_temp_1.name = "r" + i + "c" + (j + 1);
                positions_list.Add(point_temp_1);
            }
            distance_btwn_col_temp = distance_btwn_col;
            distance_btwn_row_temp = distance_btwn_row + distance_btwn_row_temp;

        }


    }




    //public void select_position(int val)
    //{
    //    for (int i = 0; i < positions_list.Count; i++)
    //    {
    //        if (i == val)
    //            selected_position = positions_list[i];
    //    }

    //}




    //public void pickup_placement()
    //{
    //    GameObject obj_parent = new GameObject("pickups");
    //    obj_parent.transform.parent = chunk.transform;
    //    for (int i = 0; i < pickups_info_array.Length; i++)
    //    {
    //        pickups_name_split = pickups_info_array[i].Split('*');



    //        for (int j = 0; j < positions_list.Count; j++)
    //        {
    //            if (string.Compare(positions_list[j].name, pickups_name_split[0]) == 0)
    //            {

    //                print(pickups_name_split[0]);

    //                for (int k = 0; k < pickups_list.Count; k++)
    //                {
    //                    if (string.Compare(pickups_list[k].name, pickups_name_split[1]) == 0)
    //                    {
    //                        Instantiate(pickups_list[k], positions_list[j].transform.position, positions_list[j].transform.rotation, obj_parent.transform);
    //                    }
    //                }

    //            }
    //        }
    //    }
    //}



    public void pickup_placement(int val)
    {

        if (GameObject.Find("pickups") == null)
        {
            obj_parent_pickup = new GameObject("pickups");
            obj_parent_pickup.transform.parent = chunk.transform;
        }


        for (int j = 0; j < positions_list.Count; j++)
        {
            for (int i = 0; i < pickups_list.Count; i++)
            {

                for (int k = 0; k < selected_position_list.Count; k++)
                {


                    if (selected_position_list.Contains(positions_list[j]))
                    {
                        Debug.Log(positions_list[j]);
                        GameObject TempPickUp = PrefabUtility.InstantiatePrefab(pickups_list[val]) as GameObject;
                        TempPickUp.transform.position = positions_list[j].transform.position;
                        TempPickUp.transform.rotation = positions_list[j].transform.rotation;
                        TempPickUp.transform.SetParent(obj_parent_pickup.transform);
                        selected_position_list.Remove(positions_list[j]);
                        break;
                    }
                }

            }
            Selection.activeGameObject = null;
        }

    }

    public void hurdles_placement(int val)
    {

        if (GameObject.Find("hurdles") == null)
        {
            obj_parent_hurdle = new GameObject("hurdles");
            obj_parent_hurdle.transform.parent = chunk.transform;
        }


        for (int j = 0; j < positions_list.Count; j++)
        {
            for (int i = 0; i < hurdles_list.Count; i++)
            {


                for (int k = 0; k < selected_position_list.Count; k++)
                {

                    if (selected_position_list.Contains(positions_list[j]))
                    {
                        Debug.Log(positions_list[j]);
                        GameObject TempHurdle = PrefabUtility.InstantiatePrefab(hurdles_list[val]) as GameObject;
                        TempHurdle.transform.position = positions_list[j].transform.position;
                        TempHurdle.transform.rotation = positions_list[j].transform.rotation;
                        TempHurdle.transform.SetParent(obj_parent_hurdle.transform);
                        selected_position_list.Remove(positions_list[j]);
                        break;
                    }

                }

            }
            Selection.activeGameObject = null;
        }
    }

    public void save_as_prefab()
    {

        if (GameObject.Find("position_holder") != null)
        {
            GameObject temp = GameObject.Find("position_holder");
            DestroyImmediate(temp);
        }
        if (positions_list != null)
            positions_list.Clear();


        Transform root = chunk.transform;

        Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
        Bounds bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; ++i)
        {
            bounds.Encapsulate(renderers[i].bounds.min);
            bounds.Encapsulate(renderers[i].bounds.max);
        }

        Debug.Log("Object spans from " + bounds.min + " to " + bounds.max);
        float val = Mathf.Abs(bounds.min.z) + bounds.max.z;
        chunk.AddComponent<BoxCollider>();
        //chunk.GetComponent<BoxCollider>().size = new Vector3(0, 0,chunk.GetComponent<Mesh>().bounds.size.z);
        // chunk.GetComponent<BoxCollider>().size = new Vector3(0, 0, rend.bounds.max.z);
        chunk.GetComponent<BoxCollider>().size = new Vector3(0, 0, val);




        IntelliChunk = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/RoadPooling/Prefabs/intellichunk.prefab", typeof(GameObject));
        GameObject IntelliChunkTemp = PrefabUtility.InstantiatePrefab(IntelliChunk) as GameObject;
        // hard coded value 12
        IntelliChunkTemp.transform.position = new Vector3(IntelliChunkTemp.transform.position.x, IntelliChunkTemp.transform.position.y, IntelliChunkTemp.transform.position.z);
        IntelliChunkTemp.transform.SetParent(chunk.transform);

        IntelliChunkTemp.transform.localPosition = new Vector3(0, 0, chunk.transform.localPosition.z - 12);

        for (int i = 0; i < obj_parent_hurdle.transform.childCount; i++)
        {
            if (obj_parent_hurdle.transform.GetChild(i).GetComponent<Trafficar>())
            {
                IntelliChunkTemp.GetComponent<intellichunk>().TrafficCars.Add(obj_parent_hurdle.transform.GetChild(i).gameObject);
            }
        }


        //if (AssetDatabase.IsValidFolder("Assets/ali/finalpatches/" + folders_to_save.ToString() + "/"))
        //{
        //    Debug.Log("gve");
        //}
        //else
        //{
        //    Debug.Log("aasd");
        //    AssetDatabase.CreateFolder("Assets/ali/finalpatches", folders_to_save.ToString());
        //}
        //  AssetDatabase.CreateFolder("Assets", folder);

        string localPath = "Assets/RoadPooling/Prefabs/Props/Chunks/"+chunk.name+".prefab";
        localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);
        chunk.transform.localPosition = Vector3.zero;
        chunk.transform.localEulerAngles = Vector3.zero;
        PrefabUtility.SaveAsPrefabAssetAndConnect(chunk, localPath, InteractionMode.UserAction);
    }

    Vector2 scrollPosition = Vector2.zero;

}
