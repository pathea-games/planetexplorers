using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class TreeGlowNode
{
	public Material Mat;
	public AnimationCurve Brightness;
}

public class TreeGlowMgr : MonoBehaviour
{
	public List<TreeGlowNode> TreeGlowList;
	
	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		//if ( GameGui_N.Instance == null || MainRightGui_N.Instance == null)
		if ( GameUI.Instance == null)
		{
			foreach ( TreeGlowNode tgn in TreeGlowList )
			{
				tgn.Mat.SetFloat("_Glow", 0);
			}			
		}
		else
		{
			float time = (float)(GameTime.Timer.TimeInDay);
			foreach ( TreeGlowNode tgn in TreeGlowList )
			{
				tgn.Mat.SetFloat("_Glow", tgn.Brightness.Evaluate(time));
			}
		}
	}
}
