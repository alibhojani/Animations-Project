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
	private int rows = 4;
	private int columns = 9;
	bool once = true; 
	Mesh mesh; 


	void Start () {
		mesh = GetComponentInChildren<MeshFilter>().mesh;
	
		vertices = new vertex[mesh.vertices.Length];
		for (int i = 0; i < mesh.vertices.Length; i++) { 
			vertex v = new vertex(mesh.vertices[i]);
			vertices[i] = v;
		}
		//Debug.Log(vertices[0].ToString());
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
			if (startSimulation) {
				startSimulation = false; 
				Debug.Log("Simulation Stopped!");
			}
			else {
				startSimulation = true;
				Debug.Log("Simulation Started!");
			}
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
					if (constraints[i].GetType().Name.Equals ("Stretch")) { 
						if (j != k) { 
							Debug.DrawLine(involved[j].position, involved[k].position);
						}
					}
					else if (constraints[i].GetType().Name.Equals ("Bend")) { 
						Debug.DrawLine(involved[0].transform.position, involved[1].transform.position, Color.magenta);
					}
				}
			}
		}
	}

	IEnumerator positionBasedSimulation () { 
		CalculateExternalForces();
		DampVelocity();
	
		Vector3[] p = new Vector3[vertices.Length];
		for (int i = 0; i < vertices.Length; i++) { 
			p[i] = vertices[i].position + (vertices[i].velocity * Time.fixedDeltaTime); //fixed delta time debatable.
		}
		/*if (once) { 
			for (int i = 0; i < p.Length; i++) { 
				Debug.Log(p[i]);
			}
			once = false;
		}*/
		//TODO:generate collision constraints 

		for (int i = 0; i < iterations; i++) { 
			for (int j = 0; j < constraints.Count; j++) { 
				constraints[j].projectConstraint(p);
			}
		}

		//velocity and position update in accordance with constraint projections 
		for (int i = 0; i < vertices.Length; i++) { 
			vertices[i].velocity = (p[i] - vertices[i].position)/Time.fixedDeltaTime; //fixed delta time debatable.
			vertices[i].position = p[i];
		}

		Vector3[] updatedVertices = new Vector3[vertices.Length];
		for (int i = 0; i < vertices.Length; i++) { 
			updatedVertices[i] = vertices[i].position;
		}
		mesh.vertices = updatedVertices;
		mesh.RecalculateNormals();

		//TODO: velocity update for collisions goes here  
		yield return null;
	}


	void CalculateExternalForces () { 
		if (enableGravity) { 
			for (int i = 0; i < vertices.Length; i++) { 
				vertices[i].velocity += new Vector3(0f, gravity * Time.fixedDeltaTime, 0f); //fixed delta time debatable.
			}
		}

	}

	// TODO: implement according to paper 
	void DampVelocity () { 
		
	}


	/*
	 * kind of an expensive method, but for my purposes we dont need a dynamic constraint generator, so be it
	 */
	/*void GenerateConstraints () { 
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
		//diagonal edges 
		for (int i = 0; i < (rows-1)*(columns-1)+2; i++) {
			if (i%columns == (columns-1)) continue;
			Stretch s = new Stretch(i, i+columns+1, vertices[i], vertices[i + columns+ 1]);
			Bend b = new Bend(i, i+columns+1, i+1, i+columns, 
				vertices[i], vertices[i+columns+1], vertices[i+1], vertices[i+columns]);
			constraints.Add(s);
			constraints.Add(b);
		}

	}*/
	
	void GenerateConstraints () { 
		for (int i = 0; i < mesh.triangles.Length; i+=3) { 
			int index = i;
			Constraint c1 = new Stretch(mesh.triangles[index], mesh.triangles[index+1], vertices[mesh.triangles[index]], vertices[mesh.triangles[index+1]]);
			Constraint c2 = new Stretch(mesh.triangles[index+1], mesh.triangles[index+2], vertices[mesh.triangles[index+1]], vertices[mesh.triangles[index+2]]);
			Constraint c3 = new Stretch(mesh.triangles[index+2], mesh.triangles[index], vertices[mesh.triangles[index+2]], vertices[mesh.triangles[index]]);
			constraints.Add(c1); constraints.Add(c2); constraints.Add(c3);
		}
	}







}
