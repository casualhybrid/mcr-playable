using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class MATS_CameraCulling : MonoBehaviour
{
    float[] LayerIndex;

    public float defaultDistance = 1000f;

    [Header("-------------------")]
    public int LayerID1;
    public float LayerDistance1 = 300f;
    [Header("-------------------")]
    public int LayerID2;
    public float LayerDistance2 = 300f;
    [Header("-------------------")]
    public int LayerID3;
    public float LayerDistance3 = 300f;
    [Header("-------------------")]
    public int LayerID4;
    public float LayerDistance4 = 300f;
    [Header("-------------------")]
    public int LayerID5;
    public float LayerDistance5 = 300f;
    void Start()
    {

        GetComponent<Camera>().farClipPlane = defaultDistance;

        LayerIndex = new float[32];

        for (int a = 0; a < LayerIndex.Length; a++)
            LayerIndex[a] = defaultDistance;


        LayerIndex[LayerID1] = LayerDistance1;
        LayerIndex[LayerID2] = LayerDistance2;
        LayerIndex[LayerID3] = LayerDistance3;
        LayerIndex[LayerID4] = LayerDistance4;
        LayerIndex[LayerID5] = LayerDistance5;

        GetComponent<Camera>().layerCullDistances = LayerIndex;

    }
}
