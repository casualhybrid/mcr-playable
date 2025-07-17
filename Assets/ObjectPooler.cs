using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public GameObject firstObject;  // Reference to the first GameObject (Object 1)
    public GameObject secondObject; // Reference to the second GameObject (Object 2)
    public float moveSpeed = 5f;    // Speed at which both objects move in the Z direction
    public float resetPositionZ = -68.25f;  // Initial Z position of Object 1
    public float startPositionZ = -44.26f;  // Initial Z position of Object 2
    public float respawnTriggerZ = -60.5f;  // Z position at which Object 2 respawns behind Object 1

    void Start()
    {
        // Set initial positions of both objects
        firstObject.transform.position = new Vector3(firstObject.transform.position.x, firstObject.transform.position.y, resetPositionZ);
        secondObject.transform.position = new Vector3(secondObject.transform.position.x, secondObject.transform.position.y, startPositionZ);
    }

    void Update()
    {
        // Move both objects together in the Z direction
        MoveObjects();

        // Check if Object 1 has passed the respawn trigger position (Z = -60.5)
        if (firstObject.transform.position.z <= respawnTriggerZ)
        {
            RespawnSecondObject();
        }
    }

    void MoveObjects()
    {
        // Move both objects along the Z axis at the same speed
        firstObject.transform.position += Vector3.forward * moveSpeed * Time.deltaTime;
        secondObject.transform.position += Vector3.forward * moveSpeed * Time.deltaTime;
    }

    void RespawnSecondObject()
    {
        // Move second object behind the first object once it crosses Z = -60.5
        secondObject.transform.position = new Vector3(secondObject.transform.position.x, secondObject.transform.position.y, firstObject.transform.position.z - (resetPositionZ - startPositionZ));

        // Optional: Add any effects or logic when the object respawns, like resetting animations
    }
}
