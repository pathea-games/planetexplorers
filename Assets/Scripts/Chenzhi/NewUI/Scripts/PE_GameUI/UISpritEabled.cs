using UnityEngine;
using System.Collections;

public class UISpritEabled : MonoBehaviour 
{
	public float mColorAl = 0.1f;
	
	public UISlicedSprite mSprite1;
	public UISlicedSprite mSprite2;

	bool IsShow = false;
	// Use this for initialization
	void Start () 
	{
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(mSprite2 == null || mSprite1 == null)
			return;
		if(IsShow == true && mSprite1.enabled == false)
		{
			mSprite1.enabled = true;
			mSprite2.enabled = true;
		}
		else if(IsShow == false && mSprite1.enabled == true)
		{
			mSprite1.enabled = false;
			mSprite2.enabled = false;
		}

	}


//	void FixedUpdate()
//	{
//		if(mSprite2 == null || mSprite1 == null)
//			return;
//		Color color = mSprite1.color;
//		if(IsShow == true && color.a <= 1)
//		{
//			color.a += mColorAl; 
//			mSprite1.color = color;
//			mSprite2.color = color;
//		}
//		else if(IsShow == false && color.a>mColorAl)
//		{
//			color.a -= mColorAl; 
//			if(color.a < mColorAl)
//				color.a = 0.01f;
//			mSprite1.color = color;
//			mSprite2.color = color;
//		}
//	}


	void SpritOnMouseOver()
	{
		IsShow = true;
	}


	void SpritOnMouseOut()
	{
		IsShow = false;
	}
}


