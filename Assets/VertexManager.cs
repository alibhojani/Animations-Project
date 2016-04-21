using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class VertexManager : MonoBehaviour {
	vertex[] vertices; 
	List <Constraint> constraints;

	public float iterations = 70f;
	public float gravity = -9.81f; 
	public float kDamping = 1f;
	public bool enableGravity = true; 
	private bool startSimulation = false;
	private int rows = 4;
	private int columns = 9;
	private List<vertex> triangles; 
	private SpatialHash spatialHash;
	private float thickness; 



	void Start () {
		spatialHash = new SpatialHash(2); 
		vertices = GetComponentsInChildren<vertex> ();
		for (int i = 0; i < vertices.Length; i++) { 
			spatialHash.Insert(vertices[i].transform.position, vertices[i]);
		}
		triangles = new List<vertex>();
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
					if (constraints[i].GetType().Name.Equals ("Stretch")) { 
						if (j != k) { 
							Debug.DrawLine(involved[j].transform.position, involved[k].transform.position);
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
		//DampVelocity();

		Vector3[] p = new Vector3[vertices.Length];
		for (int i = 0; i < vertices.Length; i++) { 
			p[i] = vertices[i].transform.position + (vertices[i].velocity * Time.fixedDeltaTime); //fixed delta time debatable.
		}

		//generate collision constraints 
		for (int i = 0; i < vertices.Length; i++) { 
			GenerateCollisionConstraints(vertices[i].transform.position, p[i]);
		}

		for (int i = 0; i < iterations; i++) { 
			for (int j = 0; j < constraints.Count; j++) { 
				constraints[j].projectConstraint(p);
			}
		}

		//velocity and position update in accordance with constraint projections 
		for (int i = 0; i < vertices.Length; i++) { 
			if (float.IsNaN(p[i].x) || float.IsNaN(p[i].y) || float.IsNaN(p[i].z)) continue;
			vertices[i].velocity = (p[i] - vertices[i].transform.position)/Time.fixedDeltaTime; //fixed delta time debatable.
			vertices[i].transform.position = p[i];
		}

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
		//initialize xcm and mass acc
		Vector3 xcm = new Vector3();
		Vector3 vcm = new Vector3(); 
		float massAccumulator = 0f; 

		for (int i = 0; i < vertices.Length; i++) { 
			Vector3 tempPos = vertices[i].transform.position;
			tempPos.Scale(new Vector3(vertices[i].mass, vertices[i].mass, vertices[i].mass));
			xcm += tempPos;
			Vector3 tempVel = vertices[i].velocity;
			tempVel.Scale(new Vector3(vertices[i].mass, vertices[i].mass, vertices[i].mass));
			vcm += tempVel;
			massAccumulator += vertices[i].mass;
		}

		xcm.Scale(new Vector3(1f/massAccumulator, 1f/massAccumulator, 1f/massAccumulator));
		vcm.Scale(new Vector3(1f/massAccumulator, 1f/massAccumulator, 1f/massAccumulator));
		if (float.IsNaN(vcm.x) || float.IsNaN(vcm.y) || float.IsNaN(vcm.z)) vcm = new Vector3();
		Vector3 L = new Vector3();
		Matrix4x4 I = new Matrix4x4();

		for (int i = 0; i < vertices.Length; i++) { 
			Vector3 ri = vertices[i].transform.position - xcm;
			Vector3 mv = vertices[i].velocity;
			mv.Scale(new Vector3(vertices[i].mass, vertices[i].mass, vertices[i].mass));
			L += Vector3.Cross(ri, mv);
			Matrix4x4 riSkewSem = new Matrix4x4();
			riSkewSem.SetRow(0, new Vector4(0, -ri.z, ri.y, 0));
			riSkewSem.SetRow(1, new Vector4(ri.z, 0, -ri.x, 0));
			riSkewSem.SetRow(2, new Vector4(-ri.y, ri.x, 0, 0));
			riSkewSem.SetRow(3, new Vector4(0, 0, 0, 1));
			Matrix4x4 riSkewSemT = riSkewSem.transpose;
			Matrix4x4 mul = riSkewSemT * riSkewSem;
			for (int j = 0; j < 3; j++) { 
				Vector4 temp = mul.GetRow(j);
				temp.Scale(new Vector4(vertices[i].mass, vertices[i].mass, vertices[i].mass));
				mul.SetRow(j, temp);
			}
			for (int j = 0; j < 3; j++) { 
				Vector4 temp = I.GetRow(j) + mul.GetRow(j);
				I.SetRow(j, temp);
			}
		}
		Matrix4x4 I_inverse = I.inverse;
		Vector3 W = I_inverse.MultiplyPoint(L);

		for (int i = 0; i <vertices.Length; i++) { 
			Vector3 ri = vertices[i].transform.position - xcm;
			Vector3 deltaVi = vcm + Vector3.Cross(W, ri) - vertices[i].velocity;
			deltaVi.Scale(new Vector3(kDamping, kDamping, kDamping));
			vertices[i].velocity += deltaVi;
		}
	}


	/*
	 * kind of an expensive method, but for my purposes we dont need a dynamic constraint generator, so be it
	 */
	void GenerateConstraints () { 
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
			//create triangle t1
			triangles.Add(vertices[i]); triangles.Add(vertices[i + columns+ 1]); triangles.Add(vertices[i+1]);
			//create triangle t2
			triangles.Add(vertices[i + columns+ 1]); triangles.Add(vertices[i]); triangles.Add(vertices[i + columns]);	
		}
			
	}

	void GenerateCollisionConstraints (Vector3 currentPos, Vector3 projectedPos) { 
		ArrayList nearBy = spatialHash.Query(projectedPos); 

	}
		







}
