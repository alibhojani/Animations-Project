using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class VertexManager : MonoBehaviour {
	vertex[] vertices; 
	List <Constraint> constraints;

	public int iterations = 20;
	public float gravity = -9.81f; 
	//public float collisionDamping = 0.5f; //[0,1]
	public float impulseForce = 10f; 
	public bool enableGravity = true; 
	private bool startSimulation = false;
	private SpatialHash spatialHash;
	private List<colliderScript> colliders; 
	private float thickness;
	public GameObject prefab; 
	public GameObject boxCollider; 
	private GameObject[] gos; 

	void Start () {
		spatialHash = new SpatialHash(2); 
		GenerateVertices();
		/*for (int i = 0; i < vertices.Length; i++) { 
			spatialHash.Insert(vertices[i].transform.position, vertices[i]);
		}*/
		colliders = new List<colliderScript>();
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
		if (Input.GetKeyUp(KeyCode.L)) SceneManager.LoadScene("kyleTestScene");
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
					else if (constraints[i].GetType().Name.Equals ("Angular")) { 
						Debug.DrawLine(involved[0].transform.position, involved[2].transform.position, Color.magenta);
					}
				}
			}
		}
	}
	void positionBasedSimulation () { 
		CalculateExternalForces();
		//DampVelocity();

		Vector3[] p = new Vector3[vertices.Length];
		for (int i = 0; i < vertices.Length; i++) { 
			p[i] = vertices[i].transform.position + (vertices[i].velocity * Time.fixedDeltaTime); //fixed delta time debatable.
		}

	/*	//generate collision constraints 
		for (int i = 0; i < vertices.Length; i++) { 
			GenerateCollisionConstraints(vertices[i].transform.position, p[i]);
		}*/

		for (int i = 0; i < iterations; i++) { 
			for (int j = 0; j < constraints.Count; j++) { 
				constraints[j].projectConstraint(p);
			}
		}

		//velocity and position update in accordance with constraint projections 
		for (int i = 0; i < vertices.Length; i++) { 
			vertices[i].velocity = (p[i] - vertices[i].transform.position)/Time.fixedDeltaTime; //fixed delta time debatable.
			//vertices[i].transform.RotateAround(Vector3.zero, vertices[i].axis, vertices[i].angle);
			vertices[i].transform.position = p[i];
			///vertices[i].transform.RotateAround(Vector3.zero, vertices[i].axis, vertices[i].angle);
		}

		// velocity update 
		for (int i = 0; i < colliders.Count; i++){ 
			if (colliders[i].isColliding){ 
				colliders[i].v1.velocity += -colliders[i].v1.collisionDamping * colliders[i].v1.velocity;
				colliders[i].v2.velocity += -colliders[i].v1.collisionDamping * colliders[i].v2.velocity;
				colliders[i].isColliding = false; 
			}
		}
		//	yield return null;
	}


	void CalculateExternalForces () { 
		if (enableGravity) { 
			for (int i = 0; i < vertices.Length; i++) { 
				vertices[i].velocity += new Vector3(0f, gravity * Time.fixedDeltaTime, 0f); //fixed delta time debatable.
			}
		}


	}

/*	// TODO: implement according to paper 
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
*/
	void GenerateVertices () {
		vertices = new vertex[13];
		GameObject neck = GameObject.Find("Neck");
		GameObject hip = GameObject.Find("Hip");
		GameObject leftKnee = GameObject.Find("Left_Knee_Joint_01");
		GameObject rightKnee = GameObject.Find("Right_Knee_Joint_01");
		GameObject leftElbow = GameObject.Find("Left_Forearm_Joint_01");
		GameObject rightElbow = GameObject.Find("Right_Forearm_Joint_01"); 
		GameObject leftHand = GameObject.Find("Left_Wrist_Joint_01");
		GameObject rightHand = GameObject.Find("Right_Wrist_Joint_01");
		GameObject leftShoulder = GameObject.Find("Left_Shoulder_Joint_01");
		GameObject rightShoulder = GameObject.Find("Right_Shoulder_Joint_01");
		GameObject leftFoot  = GameObject.Find("Left_Ankle_Joint_01");
		GameObject rightFoot  = GameObject.Find("Right_Ankle_Joint_01");
		GameObject head = GameObject.Find("Head");
		gos = new  GameObject[] {neck, leftShoulder, rightShoulder, leftElbow, rightElbow, leftHand, rightHand, hip, leftKnee, rightKnee,   
			leftFoot, rightFoot, head};
		for (int i = 0; i < gos.Length; i++) { 
			GameObject temp = GameObject.Instantiate(prefab, gos[i].transform.position, gos[i].transform.rotation) as GameObject;
			vertex tempVertex = temp.GetComponent<vertex>();
			//tempVertex.transform.SetParent(gos[i].transform.parent);
			gos[i].transform.SetParent(temp.transform);
			vertices[i] = tempVertex;
		}
	}

	//for kyle 
	void GenerateConstraints () { 
		//FixedPoint headHang = new FixedPoint(12, vertices[0]);
		Stretch headToNeck = new Stretch (12,0,vertices[12], vertices[0], boxCollider);
		Stretch neckToLeftShoulder = new Stretch (0,1,vertices[0], vertices[1], boxCollider);
		Stretch neckToRightShoulder = new Stretch (0,2,vertices[0], vertices[2], boxCollider);
		Stretch leftShoulderToLeftElbow = new Stretch(1,3,vertices[1],vertices[3], boxCollider);
		Stretch rightShoulderToRightElbow = new Stretch(2,4,vertices[2],vertices[4], boxCollider);
		Stretch leftElbowToLeftHand = new Stretch(3,5,vertices[3],vertices[5], boxCollider);
		Stretch rightElbowToRightHand = new Stretch(4,6,vertices[4],vertices[6], boxCollider);
		Stretch leftShoulderToHip = new Stretch(1,7,vertices[1],vertices[7], boxCollider);
		Stretch rightShoulderToHip = new Stretch(2,7,vertices[2],vertices[7], boxCollider);
		Stretch hipToLeftKnee = new Stretch (7,8, vertices[7],vertices[8], boxCollider);
		Stretch hipToRightKnee = new Stretch (7,9, vertices[7],vertices[9], boxCollider);
		Stretch leftKneeToLeftFoot = new Stretch (8,10, vertices[8],vertices[10], boxCollider);
		Stretch rightKneeToRightFoot = new Stretch (9,11, vertices[9],vertices[11], boxCollider);
		Stretch leftShoulderToRightShoulder = new Stretch (1,2,vertices[1],vertices[2],boxCollider);
		//constraints.Add(headHang);
		constraints.Add(headToNeck);
		constraints.Add(neckToLeftShoulder); constraints.Add(neckToRightShoulder);
		constraints.Add(leftShoulderToLeftElbow);constraints.Add(rightShoulderToRightElbow);
		constraints.Add(leftElbowToLeftHand);constraints.Add(rightElbowToRightHand );
		constraints.Add(leftShoulderToHip);constraints.Add(rightShoulderToHip);
		constraints.Add(hipToLeftKnee);constraints.Add(hipToRightKnee);
		constraints.Add(leftKneeToLeftFoot);constraints.Add(rightKneeToRightFoot);
		constraints.Add(leftShoulderToRightShoulder);

		for(int i = 0; i < constraints.Count; i++) { 
			if (constraints[i].getColliderScript()!= null) colliders.Add(constraints[i].getColliderScript());
		}

		//angular constraints 
		Angular leftShoulderElbowHand = new Angular(1,3,5, vertices[1], vertices[3], vertices[5], 40f, 85f);
		Angular rightShoulderElbowHand = new Angular(2,4,6, vertices[2], vertices[4], vertices[6], 40f, 85f);
		Angular leftKneeHipRightKnee = new Angular (8,7,9,vertices[8], vertices[7], vertices[9], 20f, 40f);
		constraints.Add(leftShoulderElbowHand); constraints.Add(rightShoulderElbowHand); 
		constraints.Add(leftKneeHipRightKnee);
	}


}
