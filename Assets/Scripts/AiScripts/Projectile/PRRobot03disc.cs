using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PRRobot03disc : PRGhost
{
	public WeaponTrail[] trails = new WeaponTrail[6];
	public new void Start()
	{
		base.Start();
		for(int i = 0; i < 6; i++)
		{
			trails[i].ClearTrail();
			trails[i].StartTrail(0.2f, 0.2f);
		}
	}

//	void LateUpdate()
//	{
//		for(int i = 0; i < 6; i++)
//		{
//			trails[i].Itterate(0f);
//			if (trails[i].time > 0f)
//				trails[i].UpdateTrail (Time.time, Mathf.Clamp(Time.deltaTime * 1f, 0f, 0.066f));
//		}
//	}
//

}
 