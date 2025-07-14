using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DemoScript : MonoBehaviour
{
    public InventorySystem inventorySysObj;
    public Transform Parent;






    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

     void OnAnimatorMove()
    {
        Animator animator = GetComponent<Animator>();

        if (animator)
        {
            Vector3 newPosition = Parent.position;
            newPosition += animator.deltaPosition;

            Parent.transform.position = newPosition;

            Parent.transform.rotation *= animator.deltaRotation;


        }
    }
}
