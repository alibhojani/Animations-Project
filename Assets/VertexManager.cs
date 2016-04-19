using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class VertexManager : MonoBehaviour {
	vertex[] vertices; 
	List <Constraint> constraints;

	public float iterations = 20f;
	public float gravity = -9.81f; 
	bool startSimulation = false;
	public bool enableGravity = true; 

	void Start () {
		vertices = GetComponentsInChildren<vertex> ();
		constraints = new List<Constraint>();
		StartCoroutine("GenerateConstraints");
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
		StartCoroutine("CalculateExternalForces"); 
		StartCoroutine("DampVelocity"); 

		Vector3[] p = new Vector3[vertices.Length];
		for (int i = 0; i < vertices.Length; i++) { 
			p[i] = vertices[i].transform.position + (vertices[i].velocity * Time.fixedDeltaTime); //fixed delta time debatable.
		}

		//TODO:generate collision constraints 

		for (int i = 0; i < iterations; i++) { 
			for (int j = 0; j < constraints.Count; j++) { 
				constraints[j].projectConstraint(p);
			}
		}

		//velocity and position update in accordance with constraint projections 
		for (int i = 0; i < vertices.Length; i++) { 
			vertices[i].velocity = (p[i] - vertices[i].transform.position)/Time.fixedDeltaTime; //fixed delta time debatable.
			vertices[i].transform.position = p[i];
		}

		//TODO: velocity update for collisions goes here  
		yield return null;
	}


	IEnumerator CalculateExternalForces () { 
		if (enableGravity) { 
			for (int i = 0; i < vertices.Length; i++) { 
				vertices[i].velocity += new Vector3(0f, gravity * Time.fixedDeltaTime, 0f); //fixed delta time debatable.
			}
		}
		yield return null;
	}

	// TODO: implement according to paper 
	IEnumerator DampVelocity () { 
		yield return null;
	}


	//for now this is a silly thing that i have made up for quick testing; can make this better so that we 
	// just have to specify what kind of thing it is and it reads the constraints, vertices from a file 
	// and bingo
	IEnumerator GenerateConstraints () { 
		vertex firstVertex = vertices[0];
		FixedPoint fp = new FixedPoint(0, firstVertex);
		firstVertex.GetComponent<Renderer>().material.color = Color.black;
		constraints.Add(fp);

		for (int i = 0; i < vertices.Length; i++) { 
			for (int j = 0; j < vertices.Length; j++) { 
				if (i != j) { 
					if ((vertices[i].transform.position - vertices[j].transform.position).magnitude < 1.2f) {
						Stretch s = new Stretch(i,j,vertices[i], vertices[j]);
						constraints.Add(s);
					}
				}
			}
		}
		yield return null;
	}
		







}
