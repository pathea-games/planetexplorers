using UnityEngine;
using System.Collections;

public class S_StealthControl : MonoBehaviour {
	SkinnedMeshRenderer smr;
	public bool anti_stealth = false;
	float fadeTime1;
	float fadeTime2;
	float dt1 = 0f;
	float dt2 = 0f;

	void Start () {
		smr = transform.GetComponent<SkinnedMeshRenderer>();
		Material mat = Resources.Load("Materials/Stealth") as Material;
		Texture tx = smr.GetComponent<Renderer>().material.GetTexture(0);
		mat.SetTexture(0, tx);
		smr.GetComponent<Renderer>().material = mat;
		fadeTime1 = smr.GetComponent<Renderer>().material.GetFloat("_HidTime") * (1 + smr.GetComponent<Renderer>().material.GetFloat("_Tail")) + 1f;
		fadeTime2 = smr.GetComponent<Renderer>().material.GetFloat("_AprTime") + smr.GetComponent<Renderer>().material.GetFloat("_FlsTime") + 1f;
	}

	void Update () {
		if (anti_stealth) {
			if (dt2 < fadeTime2) 
			{		
				dt2 += Time.deltaTime;
				smr.GetComponent<Renderer>().material.SetFloat("_Dt2", dt2);
			}
			else
				MonoBehaviour.Destroy(this);
		} 
		else 
		{
			if (dt1 < fadeTime1) 
			{
				dt1 += Time.deltaTime;
				smr.GetComponent<Renderer>().material.SetFloat("_Dt1", dt1);
			}
		}
	}
}
