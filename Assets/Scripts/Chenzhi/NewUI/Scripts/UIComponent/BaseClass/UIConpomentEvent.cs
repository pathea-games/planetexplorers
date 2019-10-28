using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class UIConpomentEvent 
{	
	[SerializeField]
	public string functionName = "";
	public UIConpomentEvent()
	{
	
	}

	public void Send(GameObject target,object sender , bool includeChildren = false)
	{
		if (string.IsNullOrEmpty(functionName)) return;
		if (target == null) 
			return;
		if (includeChildren)
		{
			Transform[] transforms = target.GetComponentsInChildren<Transform>();
			
			for (int i = 0, imax = transforms.Length; i < imax; ++i)
			{
				Transform t = transforms[i];
				t.gameObject.SendMessage(functionName, sender, SendMessageOptions.DontRequireReceiver);
			}
		}
		else
			target.SendMessage(functionName, sender, SendMessageOptions.DontRequireReceiver);
	}
}
