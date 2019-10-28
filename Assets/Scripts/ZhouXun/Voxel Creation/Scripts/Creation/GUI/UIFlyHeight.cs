using UnityEngine;
using System.Collections;

public class UIFlyHeight : MonoBehaviour 
{
	[SerializeField] UILabel mLbValue;
	[SerializeField] UISprite mLbUp;
	[SerializeField] UISprite mLbDn;
	[SerializeField] Color mSpColor;
	[SerializeField] float mUpdateTime;

	public int Value {get{return  mValue;} set{mValue = value;mLbValue.text = mValue.ToString();} }
	private int mValue = 0;
	private float time = 0;
	// Update is called once per frame
	void Update () 
	{
		if (mLbValue == null || mLbUp == null || mLbDn == null)
			return;
		if (time > mUpdateTime)
		{
			if (mValue > 0)
			{
				mLbUp.color = (mLbUp.color== mSpColor) ? Color.white : mSpColor;
				mLbDn.color = Color.white;
			}
			else if (mValue == 0)
			{
				mLbUp.color = Color.white;
				mLbDn.color = Color.white;
			}
			else 
			{
				mLbUp.color = Color.white;
				mLbDn.color = (mLbDn.color== mSpColor) ? Color.white : mSpColor;
			}
			time = 0;
		}
		time += Time.deltaTime;
	}
}
