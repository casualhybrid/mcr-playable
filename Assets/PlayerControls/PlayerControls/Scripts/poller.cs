using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class poller : MonoBehaviour
{
    [SerializeField] GameObject refrence;
    [SerializeField] GameObject[] obj_to_pool;
    [SerializeField] float dist_to_add;
    int counter = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = refrence.transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("pool_trigger"))
        {

            obj_to_pool[counter].transform.position = new Vector3(obj_to_pool[counter].transform.position.x, obj_to_pool[counter].transform.position.y, obj_to_pool[counter].transform.position.z + dist_to_add);
            if (counter < 2)
                counter++;
            else
                counter = 0;
        }
    }
}
