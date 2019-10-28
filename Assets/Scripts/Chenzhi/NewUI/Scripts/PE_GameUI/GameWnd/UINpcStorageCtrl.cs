using UnityEngine;
using System.Collections;

public class UINpcStorageCtrl :UIBaseWnd 
{

	public GameObject mContent;

	public UISprite mContentSprite;
	public UITexture mContentTexture;
	public UILabel mNpcNameText;

	public UICheckbox[] PageTitles; 
	
	static UINpcStorageCtrl mInstance;
	public static UINpcStorageCtrl Instance
	{
		get { return mInstance; }
	}

	// interface 
	public event OnGuiBtnClicked btnClose;
	public event OnGuiBtnClicked btnPageItem;
	public event OnGuiBtnClicked btnPageEquipment;
	public event OnGuiBtnClicked btnPageResource;
	[SerializeField] PeUIEffect.UISpriteScaleEffect effect;

	private int mPageIndex = 0;

	public override void Show ()
	{
		if (effect != null)
			effect.Play();
		base.Show ();
	}

	public void SetNpcName(string strName)
	{
		mNpcNameText.text = strName + " Stroage";
	}

	public void SetICO(string _sprName)
	{
		if(mContentSprite == null)
			return;

		mContentSprite.spriteName = _sprName;
		mContentSprite.gameObject.SetActive(true);
		mContentTexture.gameObject.SetActive(false);
	}
	
	public void SetICO(Texture _contentTexture)
	{
		if(mContentTexture == null)
			return;
		
		mContentTexture.mainTexture = _contentTexture;
		mContentTexture.gameObject.SetActive(true);
		mContentSprite.gameObject.SetActive(false);
	}

	public bool SetTabIndex(int index)
	{
		if (index <0 || index >=3)
			return false;
		if (index == mPageIndex)
			return false;
		PageTitles[index].isChecked = true;
		return true;
	}
	
	protected override void InitWindow ()
	{
		base.InitWindow ();
		mInstance = this;
	}
	
	void BtnCloseOnClick()
	{
		if (btnClose != null)
			btnClose();
		Hide();
	}

	void BtnPageItemOnClick(bool isActive)
	{
		if(isActive)
		{
			mPageIndex = 0;
			if (btnPageItem != null)
				btnPageItem();
		}
	}

	void BtnPageEquipmentOnClick(bool isActive)
	{
		if(isActive)
		{
			mPageIndex = 1;
			if (btnPageEquipment != null)
				btnPageEquipment();
		}

	}

	void BtnPageResourceOnClick(bool isActive)
	{
		if(isActive)
		{
			mPageIndex = 2;
			if (btnPageResource != null)
				btnPageResource();
		}
	}
}
