using UnityEngine;
using System.Collections;

public class colliderScript : MonoBehaviour {

	public vertex v1;
	public vertex v2;
	public Vector3 position; 
	public bool isColliding = false; 

	void OnTriggerEnter (Collider other) { 
		isColliding = true; 
	}

	void OnTriggerExit (Collider other) { 
		isColliding = false; 
	}
}
