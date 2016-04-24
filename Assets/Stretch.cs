using System.Collections;
using UnityEngine;

public class Stretch : Constraint {
	int indexV1;
	int indexV2; 
	vertex v1;
	vertex v2;
	float l0;
	float stiffness = 1f; 
	float thickness = 0.1f;
	private GameObject boxCollider;
	private colliderScript script;


	public Stretch(int ia, int ib, vertex a, vertex b, GameObject col) { 
		indexV1 = ia; 
		indexV2 = ib;
		v1 = a;
		v2 = b;
		l0 = (v1.transform.position - v2.transform.position).magnitude;

		Vector3 boxPos = Vector3.MoveTowards(v1.transform.position, v2.transform.position, l0/2f);
		boxCollider = GameObject.Instantiate(col, boxPos, Quaternion.LookRotation(v1.transform.position - v2.transform.position)) as GameObject;
		boxCollider.transform.localScale = new Vector3(thickness, thickness, l0*3.5f);
		script = boxCollider.GetComponent<colliderScript>();
		script.v1 = v1; 
		script.v2 = v2; 
		script.position = boxPos; 
	}

	public void projectConstraint(Vector3[] ps) { 
		Vector3 p1 = ps[indexV1];
		Vector3 p2 = ps[indexV2];

		float deltaP1Scale = -1f*(v1.w/(v1.w + v2.w))*((p1 - p2).magnitude - l0);
		deltaP1Scale /= (p1-p2).magnitude;
		float deltaP2Scale = -1f * deltaP1Scale;
		Vector3 deltaP1 = (p1 - p2);
		Vector3 deltaP2 = (p1 - p2);
		deltaP1.Scale(new Vector3(deltaP1Scale, deltaP1Scale, deltaP1Scale));
		deltaP2.Scale(new Vector3(deltaP2Scale,deltaP2Scale,deltaP2Scale));

		ps[indexV1] += deltaP1; 
		ps[indexV2] += deltaP2; 


		//update boxCollider position and rotation 
		Vector3 boxPos = Vector3.MoveTowards(ps[indexV1], ps[indexV2], l0/2f);
		boxCollider.transform.position = boxPos;
		boxCollider.transform.rotation = Quaternion.LookRotation(ps[indexV2] - ps[indexV1]);
		script.position = boxPos;

		/*Quaternion a = 	Quaternion.LookRotation(v1.transform.position, v2.transform.position);
		Quaternion b = 	Quaternion.LookRotation(ps[indexV2] - ps[indexV1]);
		Vector3 axis = Vector3.Cross(v1.transform.position - v2.transform.position, ps[indexV2] - ps[indexV1]);
		v1.transform.RotateAround(v1.transform.position, axis, Quaternion.Angle(a,b));*/	
	}

	public vertex[] getVertices () { 
		vertex[] toReturn = {v1,v2};
		return toReturn; 
	}

	public colliderScript getColliderScript () { 
		return script;
	}

}

