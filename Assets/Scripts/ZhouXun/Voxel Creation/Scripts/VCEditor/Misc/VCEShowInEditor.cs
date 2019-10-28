using UnityEngine;
using System.Collections;

public class VCEShowInEditor : MonoBehaviour
{
	// Use this for initialization
	void Start ()
	{
		GetComponent<Renderer>().enabled = false;
	}
	
	// Update is called once per frame
	void Update ()
	{
		VCEComponentTool ctool = VCUtils.GetComponentOrOnParent<VCEComponentTool>(this.gameObject);
		if ( ctool != null )
		{
			GetComponent<Renderer>().enabled = ctool.m_InEditor;
		}
		else
		{
			GameObject.Destroy(this.gameObject);
		}
	}
}
