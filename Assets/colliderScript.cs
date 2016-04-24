using UnityEngine;
using System.Collections;

public class colliderScript : MonoBehaviour {

	public vertex v1;
	public vertex v2;
	public Vector3 position; 
	public bool isColliding = false; 
	public bool once = true; 

	void OnTriggerEnter (Collider other) { 
		isColliding = true; 
		if(once) { 
			Debug.Log("ISCOLLIDING");
			once = false;
		}
	}

	void OnTriggerExit (Collider other) { 
		isColliding = false; 
	}
}
