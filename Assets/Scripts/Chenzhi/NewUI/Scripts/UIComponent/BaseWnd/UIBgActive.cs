using UnityEngine;
using System.Collections;

public class UIBgActive : MonoBehaviour 
{
	[SerializeField] UISprite mMainBg_1;
	[SerializeField] UISprite mMainBg_2;

	bool mActive;

	public bool bActive
	{
		set
		{
			mActive = value;
		}
	}

	void Update()
	{
		if (mActive)
		{
			mMainBg_1.color = Color.Lerp(mMainBg_1.color,new Color(1,1,1,1),2 * Time.deltaTime);
			mMainBg_2.color =  Color.Lerp(mMainBg_2.color,new Color(0,0.13f,1,0.3f),2 *Time.deltaTime);
		}
		else
		{
			mMainBg_2.color = Color.Lerp(mMainBg_2.color,new Color(0,0.13f,1,0), 2 *Time.deltaTime);
			mMainBg_1.color =  Color.Lerp(mMainBg_1.color,new Color(0.925f,0.725f,0.93f,1f),2 * Time.deltaTime);
		}
	}

}
