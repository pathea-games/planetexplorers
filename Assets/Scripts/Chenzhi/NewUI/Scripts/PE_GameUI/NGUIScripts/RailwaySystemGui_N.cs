using UnityEngine;
using System.Collections;

public class RailwaySystemGui_N : MonoBehaviour 
{
	static RailwaySystemGui_N mInstance;
	public static RailwaySystemGui_N Instance{ get { return mInstance; } }
	public UIBaseWnd mPointOpWnd;
	
	void Awake()
	{
		mInstance = this;
	}
}
