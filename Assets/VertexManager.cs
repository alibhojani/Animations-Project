using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class VertexManager : MonoBehaviour {
	List <vertex> vertices; 
	List <Constraint> constraints;
	ObjImporter importer; 
	Mesh m;
	public vertex prefab; 

	public float iterations = 20f;
	public float gravity = -9.81f; 
	bool startSimulation = false;
	public bool enableGravity = true; 

	void Start () {
		importer = new ObjImporter();
		m = importer.ImportFile("./Assets/plane.obj");
		vertices = new List<vertex>();
		GenerateVertices();
		constraints = new List<Constraint>();
		GenerateConstraints();
	}
	
	// Update is called once per frame
	void FixedUpdate (){
		if (startSimulation) {
			StartCoroutine("positionBasedSimulation");
		}
		DrawEdges();


	}

	void Update () { 
		if (Input.GetKeyUp (KeyCode.Space)) { 
			if (startSimulation) startSimulation = false; 
			else startSimulation = true;
		}
		if (Input.GetKeyUp(KeyCode.L)) SceneManager.LoadScene("test_scene");
	}

	/*
	 * if there exists a constraint b/w 2 vertices, draw a line (in the scene view) 
	 */
	void DrawEdges () { 
		for (int i = 0; i < constraints.Count; i++) { 
			vertex[] involved = constraints[i].getVertices();
			for (int j = 0; j < involved.Length; j++) { 
				for (int k = 0; k < involved.Length; k++) { 
					if (j != k) { 
						Debug.DrawLine(involved[j].transform.position, involved[k].transform.position);
					}
				}
			}
		}
	}

	IEnumerator positionBasedSimulation () { 
		CalculateExternalForces(); 
		DampVelocity(); 
		Vector3[] p = new Vector3[vertices.Count];
		for (int i = 0; i < vertices.Count; i++) { 
			p[i] = vertices[i].transform.position + (vertices[i].velocity * Time.fixedDeltaTime); //fixed delta time debatable.
		}

		//TODO:generate collision constraints 

		for (int i = 0; i < iterations; i++) { 
			for (int j = 0; j < constraints.Count; j++) { 
				constraints[j].projectConstraint(p);
			}
		}

		//velocity and position update in accordance with constraint projections 
		for (int i = 0; i < vertices.Count; i++) { 
			if (!float.IsNaN(p[i].x) && !float.IsNaN(p[i].y) && !float.IsNaN(p[i].z) ){
				vertices[i].velocity = (p[i] - vertices[i].transform.position)/Time.fixedDeltaTime; //fixed delta time debatable.
				vertices[i].transform.position = p[i];
			}
			else Debug.Log(i);
		}

		//TODO: velocity update for collisions goes here  
		yield return null;
	}


	void CalculateExternalForces () { 
		if (enableGravity) { 
			for (int i = 0; i < vertices.Count; i++) { 
				vertices[i].velocity += new Vector3(0f, gravity * Time.fixedDeltaTime, 0f); //fixed delta time debatable.
			}
		}
	}

	// TODO: implement according to paper 
	void DampVelocity () { 
		return;
	}

	void GenerateVertices () { 
		for (int i = 0; i < m.vertices.Length; i++) { 
			vertex v = GameObject.Instantiate(prefab, m.vertices[i], Quaternion.identity) as vertex; 
			vertices.Add(v);
			v.transform.SetParent(this.gameObject.transform);
		}
	}


	void GenerateConstraints () { 
		for (int i = 0; i < m.triangles.Length; i+=3) { //weird bug, get index out of bound but it works as expected :S 
			Constraint c1 = new Stretch(m.triangles[i], m.triangles[i+1], vertices[m.triangles[i]], vertices[m.triangles[i+1]]);
			Constraint c2 = new Stretch(m.triangles[i+1], m.triangles[i+2], vertices[m.triangles[i+1]], vertices[m.triangles[i+2]]);
			Constraint c3 = new Stretch(m.triangles[i+2], m.triangles[i], vertices[m.triangles[i+2]], vertices[m.triangles[i]]);
			constraints.Add(c1); constraints.Add(c2); constraints.Add(c3);
			if (i == 5) { 
				Constraint c = new FixedPoint(m.triangles[i], vertices[m.triangles[i]]);
				constraints.Add(c);
			}
		}

		/*for (int i = 0; i < m.triangles.Length; i+=3) { //weird bug, get index out of bound but it works as expected :S 
			float v1 = m.triangles[i];
			float v2 = m.triangles[i+1];
			float v3 = m.triangles[i+2];
			for (int j = 0; j < m.triangles.Length && j != i; j+=3) { 
				int count = 0; 
				if (v1 == m.triangles[j] || v1 == m.triangles[j+1] || v1 == m.triangles[j+2]) count ++; 
				if (v2 == m.triangles[j] || v2 == m.triangles[j+1] || v2 == m.triangles[j+2]) count ++; 
				if (v3 == m.triangles[j] || v3 == m.triangles[j+1] || v3 == m.triangles[j+2]) count ++; 
				if (count == 2) { 

				}
			}
		}*/
	
	}
		







}
