using UnityEngine;
//using System.Collections;
using System.Collections.Generic;

public class vertex : MonoBehaviour {

	public float mass; 
	public float w; 
	public Vector3 velocity; 
	public Vector3 dummyPos; 

	// Use this for initialization
	void Start () {
		velocity = new Vector3 (0f, 0f, 0f);
		mass = Random.Range(1f,10f);
		w = 1f/mass;
	}
	
	// Update is called once per frame
	void Update () {

	}


		


}
