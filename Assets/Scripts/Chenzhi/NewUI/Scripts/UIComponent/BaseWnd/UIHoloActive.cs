using UnityEngine;
using System.Collections;

public class UIHoloActive : MonoBehaviour 
{
	UIHolographicHandler mHandler = null;
	float Intensity;
	float Speed;
	float Twinkle;
	[SerializeField] float deIntensity = 0.1f;
	[SerializeField] float deSpeed = 0.05f;
	[SerializeField] float deTwinkle = 0.1f;
	// Use this for initialization
	void Start () 
	{
		mHandler = GetComponent<UIHolographicHandler>();
		if (mHandler != null)
		{
			Intensity = mHandler.Intensity;
			Speed = mHandler.Speed;
			Twinkle = mHandler.Twinkle;
		}
	}


	public bool mActive 
	{
		set
		{
			if (value)
			{
				if (mHandler != null)
				{
					mHandler.Intensity = Intensity;
					mHandler.Speed = Speed;
					mHandler.Twinkle = Twinkle;
				}
			}
			else
			{
				if (mHandler != null)
				{
					mHandler.Intensity = deIntensity;
					mHandler.Speed = deSpeed;
					mHandler.Twinkle = deTwinkle;
				}
			}
		}
	}
//	
//	// Update is called once per frame
//	void Update () 
//	{
//	
//	}
}
