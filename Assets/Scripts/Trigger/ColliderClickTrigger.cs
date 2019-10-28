using UnityEngine;
using System.Collections;


public class ColliderClickTrigger : TriggerController 
{
	public Collider mCollider;
	
	public string	mFuncName = "OnClick";
	
	public bool		mLeftBtn = true;
	public bool		mRightBtn = false;
	
	protected override void InitDefault ()
	{
		base.InitDefault ();
		if(null == mCollider)
			mCollider = GetComponent<Collider>();
	}
	
	protected override bool CheckTrigger ()
	{
		if((Input.GetMouseButtonDown(0) && mLeftBtn) || (Input.GetMouseButtonDown(1) && mRightBtn))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hitinfo;
			if(Physics.Raycast(ray, out hitinfo, 1000f, -1 ^ (1 << Pathea.Layer.Player)^ (1 << Pathea.Layer.EnergyShield)))
				if(hitinfo.collider == mCollider)
						return true;
		}
		return false;
	}
	
	protected override void OnHitTraigger ()
	{
		mTrigerTarget.SendMessage(mFuncName,SendMessageOptions.DontRequireReceiver);
	}
	
}
