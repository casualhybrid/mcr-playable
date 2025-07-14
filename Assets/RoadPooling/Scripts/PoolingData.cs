
using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "pooling blocks list data", menuName = "poolingData")]
public class PoolingData : ScriptableObject
{
 public new string name = "pooling blocks list data";
    public List<GameObject> blocksList  = new List<GameObject>();


}
