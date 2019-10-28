using UnityEngine;
using System.Collections;

public class MeshColorController : MonoBehaviour 
{
	public Color mColor;
	
	public bool  mUpdate;
	
	void Start () 
	{
		Resetcolor();
	}
	void Update()
	{
		if(mUpdate)
		{
			mUpdate = false;
			Resetcolor();
		}
	}
	
	void Resetcolor()
	{
		MeshRenderer meshRender = GetComponent<MeshRenderer>();
		if(meshRender)
		{
			meshRender.material = new Material(meshRender.material);
			meshRender.material.color = mColor;
		}
	}
}
