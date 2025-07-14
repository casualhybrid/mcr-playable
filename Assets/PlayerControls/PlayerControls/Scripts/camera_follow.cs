using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camera_follow : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float z_offset , y_offset;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate()
    {

        transform.position = new Vector3(target.position.x,  y_offset, target.position.z - z_offset);
    }
}
