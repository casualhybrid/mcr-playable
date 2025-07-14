using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


public class CreateBlocks : MonoBehaviour
{
  //  [HideInInspector]
    public GameObject blockParent, Intelli;
    [HideInInspector]
    public List<GameObject> chunksAvailablelist;
    [HideInInspector]
    public List<GameObject> Chunks = new List<GameObject>();
    public enum MyEnum
    {
        easyblockfolder,
        mediumblockfolder,
        hardblockfolder
    };
    [HideInInspector]
    public MyEnum folders_to_save = new MyEnum();


    public PoolingData DataObject;
    void Start()
    {

    }



    public void AddChunk(int val)
    {
        if (GameObject.Find("blockParent") == null)
        {
            blockParent = new GameObject("blockParent");

        }

        GameObject obj = PrefabUtility.InstantiatePrefab(chunksAvailablelist[val]) as GameObject;

        // obj.transform.SetParent(blockParent.transform);

        Chunks.Add(obj);


        m_Collider = obj.GetComponent<Collider>();

        //Fetch the size of the Collider volume
        m_Size = m_Collider.bounds.size;

        //Output to the console the size of the Collider volume

    }
    Collider m_Collider;
    Vector3 m_Size;
    public void PositionArranger()
    {
        if (Chunks == null)
            return;

        Chunks.RemoveAll(x => x == null);

        for (int i = 0; i < Chunks.Count; i++)
        {


            if (Chunks.IndexOf(Chunks[i]) == 0)
            {

                Chunks[i].transform.position = Vector3.zero;

            }
            else
            {

                //Chunks[i].transform.position = new Vector3(
                //    Chunks[i].transform.position.x,
                //    Chunks[i].transform.position.y,
                //    Chunks[Chunks.IndexOf(Chunks[i]) - 1].transform.localScale.z / 2 +
                //    Chunks[Chunks.IndexOf(Chunks[i]) - 1].transform.position.z +
                //    Chunks[Chunks.IndexOf(Chunks[i])].transform.localScale.z / 2);



                Chunks[i].transform.position = new Vector3(
                    Chunks[i].transform.position.x,
                    Chunks[i].transform.position.y,
                    Chunks[Chunks.IndexOf(Chunks[i]) - 1].GetComponent<Collider>().bounds.size.z / 2 +
                    Chunks[Chunks.IndexOf(Chunks[i]) - 1].transform.position.z +
                    Chunks[Chunks.IndexOf(Chunks[i])].GetComponent<Collider>().bounds.size.z / 2
                    );


            }
        }
    }



    public void RemoveObject()
    {
        Chunks.Remove(Selection.activeGameObject);
        DestroyImmediate(Selection.activeGameObject);

    }
    float temp_collider_size;
    float pos, pos_colider;
    public void save_as_prefab()
    {
        temp_collider_size = 0f;
        pos = 0.0f;
        pos_colider = 0.0f;


        string ss = "Assets/RoadPooling/ScriptableObjects/pooling blocks list data.asset";
        DataObject = (PoolingData)AssetDatabase.LoadAssetAtPath(ss, typeof(PoolingData));



        BlockFinialization();




        //if (AssetDatabase.IsValidFolder("Assets/ali/finalblocks/" + folders_to_save.ToString()))
        //{

        //}
        //else
        //{

        //    AssetDatabase.CreateFolder("Assets/ali/finalblocks", folders_to_save.ToString());
        //}

        string localPath = "Assets/RoadPooling/Prefabs/Props/Blocks/" + blockParent.name + ".prefab";
       


        localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);

        PrefabUtility.SaveAsPrefabAssetAndConnect(blockParent, localPath, InteractionMode.UserAction);


    
        LoadAllAssetsAtPath("Assets/RoadPooling/Prefabs/Props/Blocks");


    }

    void BlockFinialization()
    {


        for (int i = 0; i < Chunks.Count; i++)
        {
            temp_collider_size = temp_collider_size + Chunks[i].GetComponent<BoxCollider>().bounds.size.z;

            pos = pos + Chunks[i].GetComponent<BoxCollider>().bounds.size.z;

        }

        pos = pos / 2;

        blockParent.transform.position = new Vector3(0, 0, pos - Chunks[0].GetComponent<BoxCollider>().bounds.size.z / 2);


        for (int j = 0; j < Chunks.Count; j++)
        {
            Chunks[j].transform.SetParent(blockParent.transform);
        }


        blockParent.AddComponent<BoxCollider>();
        blockParent.GetComponent<BoxCollider>().size = new Vector3(0, 0, temp_collider_size);

        Intelli = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/RoadPooling/Prefabs/intelli.prefab", typeof(GameObject));
        GameObject temp = PrefabUtility.InstantiatePrefab(Intelli) as GameObject;
        temp.transform.SetParent(blockParent.transform);
        temp.transform.localPosition = new Vector3(0, 0, Chunks[0].transform.localPosition.z);


    }

    void LoadAllAssetsAtPath(string path)
    {


        if (Directory.Exists(path))
        {
            string[] assets = Directory.GetFiles(path);
            foreach (string assetPath in assets)
            {
                if (assetPath.Contains(".prefab") && !assetPath.Contains(".meta"))
                {

                    Debug.Log("Loaded " + assetPath);
                    if (!DataObject.blocksList.Contains(AssetDatabase.LoadMainAssetAtPath(assetPath) as GameObject))
                        DataObject.blocksList.Add(AssetDatabase.LoadMainAssetAtPath(assetPath) as GameObject);

                }
            }
        }

    }



    public void ArrayFilling()
    {
        chunksAvailablelist = new List<GameObject>();

        string[] _name = AssetDatabase.FindAssets("t:GameObject", new string[] { "Assets/RoadPooling/Prefabs/Props/Chunks" });
        for (int i = 0; i < _name.Length; i++)
        {
            string lAssetPath = AssetDatabase.GUIDToAssetPath(_name[i]);
            if (!chunksAvailablelist.Contains(AssetDatabase.LoadAssetAtPath<GameObject>(lAssetPath)))
                chunksAvailablelist.Add(AssetDatabase.LoadAssetAtPath<GameObject>(lAssetPath));
        }


    }
}
