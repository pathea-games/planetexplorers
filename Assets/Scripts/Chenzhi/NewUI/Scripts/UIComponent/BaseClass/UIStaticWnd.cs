using UnityEngine;
using System.Collections;

public abstract class UIStaticWnd : UIBaseWidget 
{
	public GameObject mWndCenter;

	public override bool isShow
	{
		get{
			if (mWndCenter != null)
				return mWndCenter.activeInHierarchy;
			else 
				return gameObject.activeInHierarchy;
		}
		set{
			if (gameObject != null)
			{
				if (value)
					Show();
				else 
					Hide();
			}
			
		}
	}

	public override void OnCreate ()
	{
		base.OnCreate ();

	}

	public override void Show()
	{
        if (!base.CanOpenWnd())
            return;

        if (e_OnShow != null)
            e_OnShow(this);

		PlayShowTween();
        base.PlayOpenSoundEffect();
		if(!mInit)
			InitWindow();
		if (mInit)
		{
			if (mWndCenter != null)
				mWndCenter.SetActive(true);
			else
				gameObject.SetActive(true);
		}
	}
	
//	public override void Hide()
//	{
//		if (mTweener != null)
//		{
//			mTweener.Play(false);
//		}
//		else
//		{
//			if (mWndCenter != null)
//				mWndCenter.SetActive(false);
//			else
//				gameObject.SetActive(false);
//			if (e_OnHide != null)
//				e_OnHide();
//		}
//
//	}

	protected override void OnHide ()
	{

		if (mWndCenter != null)
			mWndCenter.SetActive(false);
		else
			gameObject.SetActive(false);
		if (e_OnHide != null)
				e_OnHide(this);
	}

    protected override void OnShowFinish()
    {
        if (e_OnShowFinish != null)
            e_OnShowFinish();
    }
}
