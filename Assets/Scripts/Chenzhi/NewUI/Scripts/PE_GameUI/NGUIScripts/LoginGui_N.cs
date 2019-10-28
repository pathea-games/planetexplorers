using UnityEngine;
using System.Collections;
/*
public class LoginGui_N : StaticWindowGUIBase 
{
	static LoginGui_N mInstance;
	
	public static LoginGui_N Instance 	{ get { return mInstance; } }
	
	public UIInput			mAccountName;
	public UIInput			mPassword;
	public UICheckbox		mRememberPassword;
	
	public UISprite			mMask;
	
	// Use this for initialization
	void Start () {
		mInstance = this;
	}
	
	public override void AwakeWindow ()
	{
		base.AwakeWindow ();
		mAccountName.text = PlayerPrefs.HasKey("AccountName") ? PlayerPrefs.GetString("AccountName") : "";
		mRememberPassword.isChecked = PlayerPrefs.HasKey("RememberPwd") ? (PlayerPrefs.GetInt("RememberPwd") == 1) : false;
		if(mRememberPassword.isChecked)
			mPassword.text = PlayerPrefs.HasKey("AccountPwd") ? PlayerPrefs.GetString("AccountPwd") : "";
		else
			mPassword.text = "";
		mPassword.isPassword = true;
		mMask.gameObject.SetActive(false);
	}
	
	void OnQuitBtn()
	{
		// Only do samething when left mouse click
		if(Input.GetMouseButtonUp(0))
		{
			HideWindow();
			TitleMenuGui_N.Instance.Show();
		}
	}
	
	public void HideMask()
	{
		mMask.gameObject.SetActive(false);
	}
}*/
