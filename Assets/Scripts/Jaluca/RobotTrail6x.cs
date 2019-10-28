using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class RobotTrail6x : MonoBehaviour
{
	public List<WeaponTrail> trails = new List<WeaponTrail> ();

	float tempT = 0f;
	float t;

	void Start(){

		for (int j = 0; j < trails.Count; j++) {
			trails[j].FadeOut (0f);
		}
	}

	void Update(){
		StartAllTrails();
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
    }

}
