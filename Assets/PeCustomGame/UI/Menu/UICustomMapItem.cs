// Custom game UI Map Seleted Item
// (c) by Wu Yiqiu

using UnityEngine;
using System.Collections;
using System;

public class UICustomMapItem : MonoBehaviour 
{
	[SerializeField] UISlicedSprite  fileDirIcon;
	[SerializeField] UISlicedSprite	 mapIcon;
	[SerializeField] UILabel		 nameLb;
	[SerializeField] UISlicedSprite  seletedSprite;

	private bool mIsSelected = false;
	public bool IsSelected	{	
		get { return mIsSelected; } 
		set
		{
			mIsSelected = value;
			if (!mIsSelected)
			{
				seletedSprite.enabled = false;
			}
			else
			{
				seletedSprite.enabled = true;
				seletedSprite.alpha = 0.75f;
			}

		}
	}
	public bool IsFile 
	{ 
		get { return fileDirIcon.enabled; }  
		set 
		{ 
			if (value)
				SetFileIcon();
			else
				SetMapIcon();
		}
	}
	public bool IsMap  
	{ 
		get { return mapIcon.enabled;}

		set
		{
			if (value)
				SetMapIcon ();
			else
				SetFileIcon ();
		}
	}

	public string nameStr 
	{
		get { return nameLb.text;}
		set { nameLb.text = value;}
	}

	public int index = -1;

	// 鼠标点击
	public event Action<UICustomMapItem> onClick;
	// 鼠标双击
	public event Action<UICustomMapItem> onDoubleClick;


	public void SetFileIcon ()
	{
		fileDirIcon.enabled = true;
		mapIcon.enabled = false;
	}

	public void SetMapIcon ()
	{
		fileDirIcon.enabled = false;
		mapIcon.enabled = true;
	}

	#region UI_ACTION
	void OnMouseOver()
	{
		if (IsSelected)
		{
			seletedSprite.alpha = 1;
		}
		else
			seletedSprite.alpha = 0.25f;
		seletedSprite.enabled = true;
	}

	void OnMouseOut()
	{
		if (IsSelected)
		{
			seletedSprite.alpha = 0.75f;
			seletedSprite.enabled = true;
		}
		else
		{
			seletedSprite.enabled = false;
			seletedSprite.alpha = 0f;
		}


	}

	void OnMouseDbClick()
	{
		if (onDoubleClick != null)
			onDoubleClick(this);
	}

	void OnMouseClick()
	{
		if (onClick != null)
			onClick(this);
	}
	#endregion

	void Awake()
	{
		if (IsSelected)
		{
			seletedSprite.alpha = 0.75f;
			seletedSprite.enabled = true;
		}
		else
		{
			seletedSprite.enabled = false;
		}
	}

}
