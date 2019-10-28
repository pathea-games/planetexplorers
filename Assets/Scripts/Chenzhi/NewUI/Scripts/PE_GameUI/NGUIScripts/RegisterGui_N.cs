using UnityEngine;
using System.Collections;
/*
public class RegisterGui_N : StaticWindowGUIBase 
{
	static RegisterGui_N 	mInstance;
	public static RegisterGui_N Instance 	{ get { return mInstance; } }
	
	public UIInput			mAccountName;
	public UIInput			mPassword;
	public UIInput			mComfirm;
	public UIInput			mEmail;
	
	public UISprite			mMask;
	
	// Use this for initialization
	void Start () {
		mInstance = this;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public override void AwakeWindow ()
	{
		base.AwakeWindow ();
		mAccountName.text = "";
		mPassword.text = "";
		mComfirm.text = "";
		mEmail.text = "";
		mMask.gameObject.SetActive(false);
	}
	
	void OnRegister(string text)
	{
	}
	
	void OnRegisterBtn()
	{
	}
	
	void OnQuitBtn()
	{
		// Only do samething when left mouse click
		if(Input.GetMouseButtonUp(0))
		{
			HideWindow();
			LoginGui_N.Instance.AwakeWindow();
		}
	}
	
	public void HideMask()
	{
		mMask.gameObject.SetActive(false);
	}
}*/
