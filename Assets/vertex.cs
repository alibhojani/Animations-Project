using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class vertex : MonoBehaviour {

	public float mass; 
	public float w; 
	public Vector3 velocity; 
	public Vector3 position; 

	public vertex (Vector3 pos) { 
		position = pos; 
	}

	// Use this for initialization
	void Start () {
		velocity = new Vector3 ();
		mass = Random.Range(1f,10f);
		w = 1f/mass;
	}

	public override string ToString ()
	{
		return position.ToString();
	}
	
	// Update is called once per frame
	void Update () {

	}
}
