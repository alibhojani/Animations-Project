using System.Collections;
using UnityEngine;


public class FixedPoint : Constraint {
	Vector3 pos;
	vertex v; 
	int indexV;

	public FixedPoint (int iA, vertex a) { 
		v = a;
		pos = v.transform.position; 
		indexV = iA;
	}

	public void projectConstraint(Vector3[] ps) { 
		ps[indexV] = pos; //make sure always there
	}

	public vertex[] getVertices () { 
		vertex[] toReturn = {v};
		return toReturn; 
	}

	public colliderScript getColliderScript (){ 
		return null;
	}

}


