using UnityEngine;
using System.Collections; 

interface Constraint {  
	void projectConstraint(Vector3[] ps); //key 
	vertex[] getVertices(); 
	colliderScript getColliderScript();
}


