using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class UIMapSelectItem : MonoBehaviour 
{
	[SerializeField] UILabel mLbText;
	[SerializeField] UISprite mBg_dir;
	[SerializeField] UISprite mBg_map;

	[SerializeField] GameObject mTexSelected;
	[SerializeField] UILabel mLbPathvalve;
	//[SerializeField] UITexture mTexSelected;

	public delegate void ItemOnDbClick(object sender);
	public event ItemOnDbClick e_ItemOnDbClick = null;

	public delegate void ItemOnClick(object sender);
	public event ItemOnClick e_ItemOnClick = null;

	private UIMapSelectWnd.ItemType mType;
	private Texture _screenshot;
	private Vector3 _size;

	public UIMapSelectWnd.ItemType Type { get {return mType;}  set { mType = value; EnableBg();} }

	public string Text { get {return mLbText.text;} set{mLbText.text = value;} }
	public Texture mTexture{ get {return _screenshot;} set{_screenshot = value;} }
	public Vector3 mSize{ get {return _size;} set{_size = value;} }

	public int index;

	//public bool canSelected =false;
	
/*	bool mSelected = false;

	public bool isSelected
	{
		set
		{
			mSelected = value;
			if (canSelected)
				mTexSelected.SetActive( mSelected);
		}
		get
		{
			return mSelected;
		}
	}*/

	void MapItemOver()
	{
		mTexSelected.SetActive(true);
	}

	void MapItemOut()
	{
		mTexSelected.SetActive(false);
	}


	void MapItemOnClick()
	{
		if (e_ItemOnClick != null)
		{
			e_ItemOnClick(this);		
		}

	}

	void MapItemOnDbClick()
	{
		if (e_ItemOnDbClick != null)
		{
			e_ItemOnDbClick(this);		
		}

	}

	void EnableBg()
	{
		if (mType == UIMapSelectWnd.ItemType.it_dir)
		{
			mBg_dir.enabled = true;
			mBg_map.enabled = false;
		}
		else 
		{
			mBg_dir.enabled = false;
			mBg_map.enabled = true;
		}
	}

}
