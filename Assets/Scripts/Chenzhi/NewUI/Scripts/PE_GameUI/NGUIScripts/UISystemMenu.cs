using UnityEngine;
using System.Collections;

public class UISystemMenu : UIStaticWnd
{
	static UISystemMenu			mInstance;
	
	public static UISystemMenu 	Instance{get{return mInstance;}}
	
	public N_ImageButton			mSaveBtn;
	
	public N_ImageButton			mLoadBtn;
	
	public GameObject				mMultyWnd;
	
	void Awake ()
	{
		mInstance = this;
	}

	public bool isOpen()
	{
		return mWndCenter.activeSelf || mMultyWnd.activeSelf;
	}

	public override bool isShow
	{
		get{
			if(!GameConfig.IsMultiMode)
			{
				if (mWndCenter != null)
					return mWndCenter.activeSelf;
				else 
					return gameObject.activeSelf;
			}
			else
			{
				if (mMultyWnd != null)
                    return mMultyWnd.activeSelf;
				else 
					return gameObject.activeSelf;
			}
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
	
	public override void Show ()
	{
         if (null != e_OnShow)
            e_OnShow(this);

		PlayShowTween();
		if(GameConfig.IsMultiMode)
		{
			mWndCenter.SetActive(false);
			mMultyWnd.SetActive(true);
		}
		else
		{
			mWndCenter.SetActive(true);
			mMultyWnd.SetActive(false);
		}
	}
	
	protected override void OnHide()
	{
		mWndCenter.SetActive(false);
		mMultyWnd.SetActive(false);
		if(null != e_OnHide)
			e_OnHide(this);
	}
	
	public void OnSaveBtn()
	{
		UISaveLoad.Instance.Show();
		UISaveLoad.Instance.ToSaveWnd();
		Hide();
	}
	
	public void OnLoadBtn()
	{
		UISaveLoad.Instance.Show();
		UISaveLoad.Instance.ToLoadWnd();
		Hide();
	}
	
	void OnLobbyBtn()
	{
		MessageBox_N.ShowYNBox(PELocalization.GetString(8000078),PeSceneCtrl.Instance.GotoLobbyScene);
	}
	
	void OnOptionBtn()
	{
		Hide();
		UIOption.Instance.Show();
		UIOption.Instance.OnVideoBtn();
	}
	
	void OnMainMenuBtn()
	{
		Hide();
		if(GameConfig.IsMultiMode)
			MessageBox_N.ShowYNBox(PELocalization.GetString(8000079), PeSceneCtrl.Instance.GotoMainMenuScene,Show);
		else
			MessageBox_N.ShowYNBox(PELocalization.GetString(8000080), PeSceneCtrl.Instance.GotoMainMenuScene,Show);
	}
	
	void OnQuitGameBtn()
	{
		Hide();
		//if(GameConfig.IsMultiMode)
		//	Application.Quit();
		//else
			MessageBox_N.ShowYNBox(PELocalization.GetString(8000081), Application.Quit,Show);
	}
	
	void OnResumeBtn()
	{
		Hide();
	}
	
	public static bool IsSystemOping()
	{
		return (UISystemMenu.Instance != null && UISystemMenu.Instance.isOpen())
			|| (UISaveLoad.Instance != null && UISaveLoad.Instance.isShow)
			|| (UIOption.Instance != null && UIOption.Instance.isShow);	
	}
}
