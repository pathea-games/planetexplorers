using UnityEngine;
using System.Collections;
using PeUIEffect;
using System.Collections.Generic;

public class UIEffctMgr : MonoBehaviour 
{
	static UIEffctMgr 							mInstance;
	public static UIEffctMgr 					Instance{ get{ return mInstance; } }

	public ShakeEffect 							mShakeEffect;

	void Awake()
	{
		mInstance = this;
		mShakeEffect.Init( GetComponentsInChildren<TsShakeEffect>(true) );
	}
	// test
	public void TestPlay()
	{
		mShakeEffect.Play(1);
	}

	[System.Serializable]
	public class ShakeEffect
	{
		public float mPitch = 1;
		
		List<TsShakeEffect> mList = new List<TsShakeEffect>();
		
		public void Init(TsShakeEffect[] ts)
		{
			mList.AddRange(ts); 
		}
		public void Play(float pitch)
		{
			pitch = Mathf.Clamp01(pitch);
			
			foreach(TsShakeEffect effect in mList)
			{
				effect.Play(mPitch * pitch);
			}
		}
	}
}
