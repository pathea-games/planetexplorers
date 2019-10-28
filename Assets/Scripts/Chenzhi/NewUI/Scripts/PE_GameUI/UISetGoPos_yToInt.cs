using UnityEngine;
using System.Collections;
using System;

public class UISetGoPos_yToInt : MonoBehaviour {

	// Use this for initialization

	public GameObject mGo;

	private SpringPanel mSp;
	void Start () 
	{
		mSp = mGo.GetComponent<SpringPanel>();
	}
	
	// Update is called once per frame
	int frame =0 ;
	void Update () 
	{
		frame ++; 
		if(mGo != null && (frame %10 == 0)  && mSp != null)
		{
			if(mSp.enabled == false)
			{
				Vector3 pos = mGo.transform.localPosition;
				int y = Convert.ToInt32(pos.y);
				mGo.transform.localPosition = new Vector3(pos.x,Convert.ToSingle(y),pos.z);
				frame = 0;
			}
		}
	}

}
