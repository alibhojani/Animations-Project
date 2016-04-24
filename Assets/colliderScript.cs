using UnityEngine;
using System.Collections;

public class colliderScript : MonoBehaviour {

	public vertex v1;
	public vertex v2;
	public Vector3 position; 
	public bool isColliding = false; 

	void OnTriggerEnter (Collider other) { 
		if (!other.tag.Equals("ground"))isColliding = true; 
	}

	void OnTriggerExit (Collider other) { 
		if (!other.tag.Equals("ground"))isColliding = false; 
	}
}
