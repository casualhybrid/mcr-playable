using System.Collections.Generic;
using UnityEngine;

// it is a data container , all the  blocks are in it
public class RoadPooling : MonoBehaviour
{
    [SerializeField] private int noOfPoolingObjs;
    [SerializeField] private PoolingData poolingdata;

    [HideInInspector] public List<GameObject> blocks;
    [HideInInspector] public GameObject currentBlock;
    [HideInInspector] public List<GameObject> BlocksInScene;

    public GameObject CutSceneEnv;

    private void OnEnable()
    {
    //    IntelligentDecisionMaker.decision.AddListener(S);
    }

    private void OnDisable()
    {
     //   IntelligentDecisionMaker.decision.RemoveListener(S);
    }

    private void S(int val)
    {
        GenerateBlock(val);
    }

    //private void Start()
    //{
    //    foreach (var item in poolingdata.blocksList)
    //    {
    //        blocks.Add(item);
    //    }

    //    currentBlock = blocks[0];
    //    GenerateBlock(0, true);
    //}

    private GameObject obj;

    public void GenerateBlock(int val, bool first = false)
    {
        int value = Random.Range(0, poolingdata.blocksList.Count);
        obj = Instantiate(blocks[val]);

        if (first)
        {
            // obj.transform.position = new Vector3(0, 0, -20f);
            obj.transform.position = new Vector3(0, 0, CutSceneEnv.GetComponent<BoxCollider>().bounds.size.z / 2
      + CutSceneEnv.transform.position.z
      + obj.GetComponent<BoxCollider>().bounds.size.z / 2);
        }
        else
        {
            obj.transform.position = new Vector3(0, 0,
     currentBlock.GetComponent<BoxCollider>().bounds.size.z / 2
     + currentBlock.transform.position.z
     + obj.GetComponent<BoxCollider>().bounds.size.z / 2
 );
        }

        currentBlock = obj;

        maintainer(currentBlock);
    }

    // pool/instentiate do whatever

    public void maintainer(GameObject obj)
    {
        BlocksInScene.Add(obj);
        if (BlocksInScene.Count > noOfPoolingObjs)
        {
            GameObject ob = BlocksInScene[0];
            BlocksInScene.Remove(ob);
            Destroy(ob);
        }
    }
}