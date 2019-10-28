/* Written for "Dawn of the Tyrant" by SixTimesNothing 
/* Please visit www.sixtimesnothing.com to learn more
/*
/* Note: This code is being released under the Artistic License 2.0
/* Refer to the readme.txt or visit http://www.perlfoundation.org/artistic_license_2_0
/* Basically, you can use this for anything you want but if you plan to change
/* it or redistribute it, you should read the license
*/

using UnityEngine;
using System;
using System.Collections;

[System.Serializable]
public class RiverNodeObject 
{
	public Vector3 position;
	public float width;
}

[ExecuteInEditMode()]
public class WaterToolScript : MonoBehaviour 
{
	public void CreateRiver()
	{
		GameObject riverObject = new GameObject();
		riverObject.name = "River";
		riverObject.AddComponent(typeof(MeshFilter));
		riverObject.AddComponent(typeof(MeshRenderer));
		riverObject.AddComponent<AttachedRiverScript>();
		if(transform.parent != null)
		{
			riverObject.transform.parent = transform.parent;
		}
		
		AttachedRiverScript ARS = (AttachedRiverScript)riverObject.GetComponent("AttachedRiverScript");
		ARS.riverObject = riverObject;
		ARS.parentTerrain = gameObject;
	}
}