using UnityEngine;
using System.Collections;

public class MeshDeformationTest : MonoBehaviour {
 
	Mesh planeMesh; 
	Vector3 speed;

	// Use this for initialization
	void Start () {
		planeMesh = GetComponentInChildren<MeshFilter>().mesh;

	}
	
	// Update is called once per frame
	void Update () {
		Vector3[] vertices = planeMesh.vertices;
		for (int i = 0; i < planeMesh.vertices.Length; i++) { 
			speed = new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 3f), Random.Range(-3f, 3f));
			vertices[i] += speed * Time.deltaTime;
		}
		planeMesh.vertices = vertices; 
		planeMesh.RecalculateNormals();

	}
}
