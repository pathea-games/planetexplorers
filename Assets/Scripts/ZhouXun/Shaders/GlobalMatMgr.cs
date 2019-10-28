using UnityEngine;
using System.Collections;

public class GlobalMatMgr : MonoBehaviour
{
	public Material EnergySheildMat;
	
	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		if ( EnergySheildMat != null )
		{
			EnergySheildMat.SetFloat("_TimeFactor", Time.time);
		}
	}
}
