﻿using System.Collections;
using UnityEngine;

public class Stretch : Constraint { 
	int indexV1;
	int indexV2; 
	vertex v1;
	vertex v2;
	float l0;
	float stiffness = 1f; 
	bool once = true; 

	public Stretch(int ia, int ib, vertex a, vertex b) { 
		indexV1 = ia; 
		indexV2 = ib;
		v1 = a;
		v2 = b;
		l0 = (v1.position - v2.position).magnitude;
	}

	public void projectConstraint(Vector3[] ps) { 
		Vector3 p1 = ps[indexV1];
		Vector3 p2 = ps[indexV2];
		if (once) { //PROBLEM DUE TO GENERATECOLLISIONCONSTRAINTS IS HERE
			Debug.Log(p1 + ", " + p2);
			once = false; 
		}
		float deltaP1Scale = -1f*(v1.w/(v1.w + v2.w))*((p1 - p2).magnitude - l0);
		deltaP1Scale /= (p1-p2).magnitude;
		float deltaP2Scale = -1f * deltaP1Scale;
		Vector3 deltaP1 = (p1 - p2);
		Vector3 deltaP2 = (p1 - p2);
		deltaP1.Scale(new Vector3(deltaP1Scale, deltaP1Scale, deltaP1Scale));
		deltaP2.Scale(new Vector3(deltaP2Scale,deltaP2Scale,deltaP2Scale));

		ps[indexV1] += deltaP1; 
		ps[indexV2] += deltaP2; 
	}

	public vertex[] getVertices () { 
		vertex[] toReturn = {v1,v2};
		return toReturn; 
	}

}

