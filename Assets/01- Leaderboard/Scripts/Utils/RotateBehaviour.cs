using UnityEngine;
using System.Collections;

public class RotateBehaviour : MonoBehaviour
{
	public Vector3 RotationAmount;
	
	// Update is called once per frame
	void Update ()
	{
		transform.Rotate (RotationAmount * Time.deltaTime);
	}
}
