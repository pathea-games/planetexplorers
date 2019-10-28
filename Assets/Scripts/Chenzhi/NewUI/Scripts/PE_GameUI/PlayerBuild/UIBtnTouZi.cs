using UnityEngine;
using System.Collections;

public class UIBtnTouZi : MonoBehaviour 
{
	public delegate void OnClickFunc();
	public event OnClickFunc e_EndRun = null;

	[SerializeField] UISprite mCotentSpr;
	[SerializeField] Collider mCollider;
	[SerializeField] int mFrameCount;

	string[] mSprites = new string[4];
	bool isRun = false;
	int tempFrame = 0 ; 


	// Use this for initialization
	void Start () 
	{
		mCollider.enabled = !isRun; 

		mSprites[0] = "touzi_1";
		mSprites[1] = "touzi_2";
		mSprites[2] = "touzi_3";
		mSprites[3] = "touzi_4";
	}

	void BtnRandomOnClick()
	{
		isRun = true;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (isRun)
			tempFrame ++;

		if (tempFrame > 0)
		{
			if (tempFrame < mFrameCount)
				mCotentSpr.spriteName = mSprites[1];
			else if (tempFrame < 2 * mFrameCount)
				mCotentSpr.spriteName = mSprites[2];
			else if (tempFrame < 3 * mFrameCount)
				mCotentSpr.spriteName = mSprites[3];
			else 
			{
				mCotentSpr.spriteName = mSprites[0];
				tempFrame = 0;
				isRun = false;
				mCotentSpr.color = new Color(0.8f,0.8f,0.8f,1);
				if (e_EndRun != null)
					e_EndRun();
			}
		}
		mCollider.enabled = !isRun; 
	}
}
