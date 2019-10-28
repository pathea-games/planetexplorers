using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LightCtrl : MonoBehaviour
{
	public Light mLight;
	public float mLifetime;
	public List<float> mIntensity;
	public List<float> mRange;
	public List<Color> mColor;
	
	float mCurrentTime;
	
	void Update()
	{
		mCurrentTime += Time.deltaTime;
		if(mCurrentTime < mLifetime)
		{
			if(mIntensity.Count == 0)
				mLight.intensity = 1;
			else if(mIntensity.Count == 1)
				mLight.intensity = mIntensity[0];
			else
			{
				int		Count = mIntensity.Count - 1;
				float	dT = mLifetime/Count;
				float	cT = mCurrentTime%dT/dT;
				int		index = (int)(mCurrentTime/dT);
				mLight.intensity = Mathf.Lerp(mIntensity[index],mIntensity[index+1],cT);
			}
			
			if(mRange.Count == 0)
				mLight.range = 1;
			else if(mRange.Count == 1)
				mLight.range = mRange[0];
			else
			{
				int		Count = mRange.Count - 1;
				float	dT = mLifetime/Count;
				float	cT = mCurrentTime%dT/dT;
				int		index = (int)(mCurrentTime/dT);
				mLight.range = Mathf.Lerp(mRange[index],mRange[index+1],cT);
			}
			
			if(mColor.Count != 0)
			{
				if(mColor.Count == 1)
					mLight.color = mColor[0];
				else
				{
					int		Count = mColor.Count - 1;
					float	dT = mLifetime/Count;
					float	cT = mCurrentTime%dT/dT;
					int		index = (int)(mCurrentTime/dT);
					mLight.color = Color.Lerp(mColor[index],mColor[index+1],cT);
				}
			}			
		}
		else
			Destroy(gameObject);
	}
}
