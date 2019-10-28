using UnityEngine;
using System.Collections;

public class OpCubeCtrl : MonoBehaviour 
{
	public bool Active
	{
		get{return gameObject.activeSelf;}
		set{
			if(gameObject.activeSelf != value)
				gameObject.SetActive(value);
		}
	}
	
	public bool Enable
	{
		get { return mEnable; }
		set	{ mEnable = value; }
	}
	
	Color mTargetColor = new Color (0, 1, 0, 0.25f);
	
	bool mEnable = false;
	
	//float Size = 1f;
	
	// Update is called once per frame
	void Update ()
	{
		if(mEnable)
			mTargetColor = Color.Lerp(mTargetColor, new Color (0.156f, 0.553f, 0.518f, Random.Range(0.4f,0.5f)), 5f * Time.deltaTime);
		else
			mTargetColor = Color.Lerp(mTargetColor, new Color (0.72f, 0f, 0f, Random.Range(0.4f,0.5f)), 5f * Time.deltaTime);
		
		GetComponent<Renderer>().material.SetColor("_TintColor",mTargetColor);
	}
}
