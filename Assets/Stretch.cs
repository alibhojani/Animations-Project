using System.Collections;
using UnityEngine;

public class Stretch : Constraint {
	int indexV1;
	int indexV2; 
	vertex v1;
	vertex v2;
	float l0;
	float stiffness = 1f; 
	float thickness = 0.25f;
	private GameObject boxCollider;

	public Stretch(int ia, int ib, vertex a, vertex b, GameObject col) { 
		indexV1 = ia; 
		indexV2 = ib;
		v1 = a;
		v2 = b;
		l0 = (v1.transform.position - v2.transform.position).magnitude;

		Vector3 boxPos = Vector3.MoveTowards(v1.transform.position, v2.transform.position, l0/2f);
		boxCollider = GameObject.Instantiate(col, boxPos, Quaternion.LookRotation(v1.transform.position - v2.transform.position)) as GameObject;
		BoxCollider colliderComponent = boxCollider.GetComponent<BoxCollider>();
		boxCollider.transform.localScale = new Vector3(thickness, thickness, l0*3.5f);
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

		/*Vector3 psv1ToV1 = v1.transform.position - ps[indexV1];
		Vector3 psv2Temp = ps[indexV2] + psv1ToV1;
		Vector3 after = psv2Temp - v1.transform.position; //v1 to psv2Temp
		Vector3 before = v2.transform.position - v1.transform.position;

		v1.angle = Vector3.Angle(before, after);
		v1.axis = Vector3.Cross(before, after);*/

	}

	public vertex[] getVertices () { 
		vertex[] toReturn = {v1,v2};
		return toReturn; 
	}

}

