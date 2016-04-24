using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class vertex : MonoBehaviour {

	public float mass; 
	public float w; 
	public float collisionDamping = 0.7f;
	public Vector3 velocity; 

	public vertex (Vector3 pos) { 
		this.transform.position = pos; 
	}

	// Use this for initialization
	void Start () {
		velocity = new Vector3 ();
		mass = Random.Range(1f,10f);
		w = 1f/mass;
	}
	
	void OnTriggerEnter (Collider other) { 
		//velocity = -collisionDamping * velocity; 
		//if (other.tag.Equals("ground")) {
		//	velocity = -velocity;

	//	}
	}
}
