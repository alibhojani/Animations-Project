using System.Collections;
using UnityEngine;

public class Bend : Constraint { 
	int indexV1;
	int indexV2; 
	int indexV3;
	int indexV4;
	vertex v1;
	vertex v2; 
	vertex v3; 
	vertex v4;
	Vector3 n1; 
	Vector3 n2;
	float initialAngle; 
	float d; 
	float stiffness = 1f; 


	public Bend(int ia, int ib, int ic, int id, vertex a, vertex b, vertex c, vertex d) { 
		indexV1 = ia; 
		indexV2 = ib;
		indexV3 = ic; 
		indexV4 = id;
		v1 = a;
		v2 = b;
		v3 = c;
		v4 = d;
		Vector3 V = v3.position - v1.position;
		Vector3 W = v2.position - v1.position; 
		float Nx = (V.y * W.z) - (V.z * W.y);
		float Ny = (V.z * W.x) - (V.x * W.z);
		float Nz = (V.x * W.y) - (V.y * W.x);
		n1 = new Vector3(Nx, Ny, Nz);
		n1.Normalize();

		V = v2.position - v1.position;
		W = v4.position - v1.position; 
		Nx = (V.y * W.z) - (V.z * W.y);
		Ny = (V.z * W.x) - (V.x * W.z);
		Nz = (V.x * W.y) - (V.y * W.x);
		n2 = new Vector3(Nx, Ny, Nz);
		n2.Normalize();
		this.d = Vector3.Dot(n1,n2);
		initialAngle = Mathf.Acos(this.d);

	}

	public void projectConstraint(Vector3[] ps) { 
		Vector3 p1 = ps[indexV1];
		Vector3 p2 = ps[indexV2];
		Vector3 p3 = ps[indexV3];
		Vector3 p4 = ps[indexV4];

		Vector3 q3 = Vector3.Cross(n1,p2); q3.Scale(new Vector3(d,d,d)); q3 += Vector3.Cross(p2,n2);
		float temp = 1f/(Vector3.Cross(p2,p3).magnitude);
		q3.Scale(new Vector3(temp, temp, temp));

		Vector3 q4 = Vector3.Cross(n2,p2); q4.Scale(new Vector3(d,d,d)); q4 += Vector3.Cross(p2,n1);
		temp = 1f/(Vector3.Cross(p2,p4).magnitude);
		q4.Scale(new Vector3(temp, temp, temp));

		Vector3 q2 = Vector3.Cross(n1,p3); q2.Scale(new Vector3(d,d,d)); q2 += Vector3.Cross(p3,n2);
		temp = -1f/(Vector3.Cross(p2,p3).magnitude);
		q2.Scale(new Vector3(temp,temp,temp)); 

		Vector3 q2Temp = Vector3.Cross(n2,p4); q2Temp.Scale(new Vector3(d,d,d)); q2Temp += Vector3.Cross(p4,n1);
		temp = -1f/(Vector3.Cross(p2,p4).magnitude);
		q2Temp.Scale(new Vector3(temp, temp, temp)); 

		q2 += q2Temp;
		q2Temp = q2;
		q2Temp.Scale(new Vector3(-1f,-1f,-1f));

		Vector3 q1 = q2Temp - q3 - q4;

		Vector3[] qs = {q1,q2,q3,q4};
		Vector3[] ps2 = {p1,p2,p3,p4};
		vertex[] vs2 = {v1,v2,v3,v4};

		float summationTerm = 0f;  
		for (int i = 0; i < vs2.Length; i++) { 
			float t = vs2[i].w * Mathf.Pow(qs[i].magnitude, 2);
			summationTerm += t;
		}

		for (int i = 0; i < vs2.Length; i++){ 
			float deltaPiScale = (vs2[i].w * Mathf.Sqrt(1 - Mathf.Pow(d,2)) * (Mathf.Acos(d) - initialAngle))/summationTerm;
			Vector3 deltaPi = qs[i];
			deltaPi.Scale(new Vector3(deltaPiScale, deltaPiScale, deltaPiScale));
			ps2[i] += deltaPi;
		}
		//actual update 
		ps[indexV1] = p1;
		ps[indexV2] = p2;
		ps[indexV3] = p3;
		ps[indexV4] = p4;

	}

	public vertex[] getVertices () { 
		vertex[] toReturn = {v1,v2,v3,v4};
		return toReturn; 
	}

}
