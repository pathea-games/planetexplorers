using UnityEngine;
using System.Collections;

public class UIBuildSaveWndCtrl : UIBaseWnd 
{
	public delegate void ClickCancelFunc();
	public event ClickCancelFunc btnCanel = null;
	public event ClickCancelFunc OnWndClosed = null;
	public delegate bool ClickSaveFunc(string isoName);
	public event ClickSaveFunc btnSave = null;

 	[SerializeField]UILabel mInputIsoNmae;
	public UIBuildWndItem mSaveIsoItem;

	public string IsoName { get { return mInputIsoNmae.text;} }

	void OnEnable ()
	{

	}

	void OnDisable ()
	{
		if (OnWndClosed != null)
			OnWndClosed();
	}

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	public void SetIsoItemContent(Texture contentTexture)
	{
		mSaveIsoItem.InitItem(UIBuildWndItem.ItemType.mNull,contentTexture,-1);
	}

	public void SetIsoItemContent(string contentSprName,string atlas)
	{
		mSaveIsoItem.InitItem(UIBuildWndItem.ItemType.mNull,contentSprName,atlas,-1);
	}


	public string GetIsoName()
	{
		string isoName = mInputIsoNmae.text;
		if(isoName == "need iso name")
			isoName = string.Empty;
		return isoName;
	}

	void BtnSaveOnClick()
	{
		string isoName = mInputIsoNmae.text;

		if(isoName == "need iso name")
			isoName = string.Empty;
		if(isoName.Length == 0)
			return;

		if (btnSave == null || btnSave(isoName))
			this.gameObject.SetActive(false);
	}

	void BtnCancelOnClick()
	{
		if(btnCanel != null)
			btnCanel();
		this.gameObject.SetActive(false);
	}

	void BtnCloseOnClick()
	{
		BtnCancelOnClick();
	}
}
