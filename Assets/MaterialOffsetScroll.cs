using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialOffsetScroll : MonoBehaviour
{
    public float scrollSpeed;
    private Material _material;
    
    // Start is called before the first frame update
    void Start()
    {
        _material = GetComponent<Renderer>().materials[0];
    }

    // Update is called once per frame
    void Update()
    {
        var offset = Time.time * scrollSpeed;
        _material.SetTextureOffset("_MainTex", new Vector2(offset, 0));
    }
}
