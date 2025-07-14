using UnityEngine;

public class ObjToPool : MonoBehaviour
{
    [SerializeField] private CutSceneCore CutSceneManager;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CutSceneManager.PoolRoad();
        }
    }
}