using UnityEngine;
using Pathea.Maths;
using System.Collections;

public class RSManager : MonoBehaviour
{
	private static RSManager s_Instance = null;
	public static RSManager Instance { get { return s_Instance; } }

	// Use this for initialization
	void Awake ()
	{
		s_Instance = this;
	}

	void OnDestroy ()
	{
		s_Instance = null;
	}
	
	// Update is called once per frame
	void Update ()
	{

	}
}
