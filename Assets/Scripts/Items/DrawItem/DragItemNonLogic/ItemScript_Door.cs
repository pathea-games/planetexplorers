using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemScript_Door : ItemScript_State 
{	
	public int mOpenSoundID;
	public int mCloseSoundID;
	
	public bool IsOpen
	{
		get{ return mSubState > 0; }
	}
	
	public void OpenDoor()
    {
		SetState(1);
    }
	
	public void ShutDoor()
    {
		SetState(0);
    }
	
	// 1: Open 0:Close
	protected override void ApplyState (int state)
	{
		base.ApplyState (state);
		if(IsOpen)
		{
            gameObject.GetComponent<Animation>().CrossFade("Open");
			AudioManager.instance.Create(transform.position, mOpenSoundID);
		}
		else
		{
            gameObject.GetComponent<Animation>().CrossFade("Close");
			AudioManager.instance.Create(transform.position, mCloseSoundID);
		}
	}
}
