using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class testTrail6x : MonoBehaviour
{
	/*public List<testWeaponTrail> trails = new List<testWeaponTrail> ();

	float tempT = 0f;
	float t;

	void Start(){

		for (int j = 0; j < trails.Count; j++) {
			trails[j].FadeOut (0f);
		}
		StartAllTrails();
	}

	void Update(){

		t = Mathf.Clamp(Time.deltaTime * 1, 0, 0.066f);
		for (int j = 0; j < trails.Count; j++) {
			trails[j].Itterate (Time.time - t + tempT);
		}
		tempT -= t;
		for (int j = 0; j < trails.Count; j++)
		{
			//if (trails[j].time > 0)
			//{
				trails[j].UpdateTrail (Time.time, t);
			//}
		}
	}
    public void StartAllTrails() 
    {
        for (int i = 0; i < trails.Count; i++)
        {
            trails[i].ClearTrail();
//            trails[i].startColor = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
//            trails[i].endColor = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
            trails[i].StartTrail(0.2f, 0.2f);
        }
    }*/

	WeaponTrail wt;

	void Start()
	{
		wt = this.GetComponent<WeaponTrail>();
		wt.ClearTrail();
		wt.StartTrail(.2f,.2f);
	}

	void LateUpdate()
	{
		//wt.ClearTrail();
		
		if(null != wt)
		{
			float mAnimTime = Mathf.Clamp(Time.deltaTime * 1, 0, 0.066f);
			wt.Itterate (0f);
			if (wt.time > 0)
				wt.UpdateTrail (Time.time, mAnimTime);
		}
	}

	public void OpenTrail()
	{

	} 

	public void CloseTrail()
	{

	}
}