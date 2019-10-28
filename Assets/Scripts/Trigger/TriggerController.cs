using UnityEngine;
using System.Collections;

public class TriggerController : MonoBehaviour 
{
	public GameObject mTrigerTarget; //Target  SendMessage to
	
	void Start()
	{
		InitDefault();
	}
	
	void Update()
	{
		if(CheckTrigger())
			OnHitTraigger();
	}
	
	//Called in start;
	protected virtual void InitDefault()
	{
		if(null == mTrigerTarget)
			mTrigerTarget = gameObject;
	}
	
	protected virtual bool CheckTrigger()
	{
		return false;
	}
	
	protected virtual void OnHitTraigger()
	{
		
	}
	
}
