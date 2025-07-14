using System.Collections.Generic;
using UnityEngine;

public class Tyre_rotation : MonoBehaviour
{
    [SerializeField] private Vector3 axis;
    [SerializeField] private Space spaceToRotateIn;
    public float speedToRotate { get; set; } = 1;
    [SerializeField] private float speedConstant;

    [Tooltip("tyres for rotation")]
    [SerializeField] private GameObject[] tyres;

    [SerializeField] private PlayerSharedData playerSharedData;

    // private Quaternion currentRotation;
    private readonly Dictionary<GameObject, Quaternion> rotationDictionary = new Dictionary<GameObject, Quaternion>();

    private void Awake()
    {
        //     currentRotation = tyres[0].transform.rotation ;

        for (int i = 0; i < tyres.Length; i++)
        {
            GameObject tyre = tyres[i];
            rotationDictionary.Add(tyre, tyre.transform.localRotation);
        }
    }

    private void LateUpdate()
    {
        for (int i = 0; i < tyres.Length; i++)
        {
            //Transform tyre = tyres[i].transform;
            //Quaternion rotation = Quaternion.AngleAxis(Time.deltaTime * speedConstant * speedToRotate, tyre.right);
            //currentRotation = rotation * currentRotation;
            //tyre.localRotation = currentRotation;

            GameObject tyre = tyres[i];
            Quaternion rotation = Quaternion.Euler(axis * Time.deltaTime * speedConstant * speedToRotate);
            Quaternion currentRotation = rotationDictionary[tyre];
            Quaternion finalRotation = rotation * currentRotation;
            rotationDictionary[tyre] = finalRotation;
            //  currentRotation = rotation * currentRotation;
            tyres[i].transform.localRotation = finalRotation;
        }
    }
}