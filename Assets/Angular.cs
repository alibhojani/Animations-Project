using System.Collections;
using UnityEngine;

public class Angular : Constraint { 
	int indexV1;
	int indexV2; 
	int indexV3; 
	vertex v1;
	vertex v2;
	vertex v3;
	float min; 
	float max;
	float l0;
	float restLength;
	float stiffness = 1f; 
	bool disable = true;

	public Angular(int ia, int ib, int ic, vertex a, vertex b, vertex c, float min, float max) { 
		indexV1 = ia; 
		indexV2 = ib;
		indexV3 = ic;
		v1 = a;
		v2 = b;
		v3 = c; 
		this.min = min; 
		this.max = max; 
		restLength = (v3.transform.position - v1.transform.position).magnitude;
		l0 = restLength;
	}

	public void projectConstraint(Vector3[] ps) { 
		Vector3 p1 = ps[indexV1];
		Vector3 p2 = ps[indexV2];
		Vector3 p3 = ps[indexV3];
		Vector3 bToa = p1 - p2; 
		Vector3 bToc = p3 - p2; 
		float btoaLength = bToa.magnitude;
		float btocLength = bToc.magnitude;
		float futureAngle = Vector3.Angle(bToa, bToc);

		if (futureAngle > max + stiffness) {
			float dMax1 = Mathf.Tan(Mathf.Deg2Rad *(max/2f)) * btoaLength; 
			float dMax2 = Mathf.Tan(Mathf.Deg2Rad *(max/2f)) * btocLength;
			l0 = dMax1 + dMax2; 
			disable = false; 
		}

		else if (futureAngle < min - stiffness) { 
			float dMin1 = Mathf.Tan(Mathf.Deg2Rad * (min/2f)) * btoaLength; 
			float dMin2 = Mathf.Tan(Mathf.Deg2Rad *(min/2f)) * btocLength;
			l0 = dMin1 + dMin2;
			disable = false; 
		}

		if(!disable) {

			float deltaP1Scale = -1f*(v1.w/(v1.w + v3.w))*((p1 - p3).magnitude - l0);
			deltaP1Scale /= (p1-p3).magnitude;
			float deltaP3Scale = -1f * deltaP1Scale;
			Vector3 deltaP1 = (p1 - p3);
			Vector3 deltaP3 = (p1 - p3);
			deltaP1.Scale(new Vector3(deltaP1Scale, deltaP1Scale, deltaP1Scale));
			deltaP3.Scale(new Vector3(deltaP3Scale,deltaP3Scale,deltaP3Scale));

			ps[indexV1] += deltaP1; 
			ps[indexV3] += deltaP3; 

			disable = true; //after fixing "disable" this constraint.  
		}


		
	}

	public vertex[] getVertices () { 
		vertex[] toReturn = {v1,v2, v3};
		return toReturn; 
	}

}

