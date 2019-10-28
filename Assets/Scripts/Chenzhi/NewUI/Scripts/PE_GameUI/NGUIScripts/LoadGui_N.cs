using UnityEngine;
using System.Collections;

public class LoadGui_N
{
//	public UISprite mBg;
//	public UISprite mLoadSpr;
//	public UILabel	mCountText;
//	
//	float	mLoadTime;
//	float	mLoadElapseTime;
//	
//	void Awake ()
//	{
//		mLayerId = UILayerID.Load;
////		gameObject.SetActiveRecursively(false);
//		mLoadTime = 10;
//		mFadetime = 5;
//	}
//	
//	protected override void OnUpdate ()
//	{
//		mLoadElapseTime += Time.deltaTime;
//		if(mLoadElapseTime > mLoadTime)
//			mLoadElapseTime = mLoadTime;
//		float progress = mLoadElapseTime/mLoadTime;
//		mCountText.text = (int)(progress*100) + "%";
//		mLoadSpr.transform.localRotation = Quaternion.AngleAxis((int)(101*progress)*360/25,Vector3.forward);
//		if(mFadeState == FadeState.FadeOut)
//		{
//			float fadeProgress = 1 - mFadeElapseTime/mFadetime;
//			mBg.alpha = fadeProgress;
//			mLoadSpr.alpha = fadeProgress;
//			mCountText.text = "";
//		}
//		if(mLoadElapseTime == mLoadTime && string.Compare(Application.loadedLevelName,GameConfig.MainSceneName) == 0 && !Application.isLoadingLevel)
//			HideLayer();
//		base.OnUpdate ();
//	}
//	
//	public override void AwakeLayer ()
//	{
//		gameObject.SetActive(true);
//		Application.LoadLevelAsync(GameConfig.MainSceneName);
//		mLoadElapseTime = 0;
//		base.AwakeLayer ();
//	}
//	
//	public override void HideLayer ()
//	{
//		GUIMain.Instance.GUILayers[UILayerID.Game].AwakeLayer();
//		base.HideLayer ();
//	}
}
