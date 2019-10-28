using UnityEngine;
using System.Collections;

public class UIOptionMenuItem: MonoBehaviour 
{
	[SerializeField] UILabel mLbText;
	[SerializeField] UISprite mMouseSpr;
	[SerializeField] Collider mCollider;

	public delegate void BaseMsgEvent(object sender);
	public event BaseMsgEvent e_OnClickItem = null;

	private int mIndex = -1;
	public int Index{get{return mIndex;}}
	public string Text {get{return mLbText.text;}}
	public object Data = null;

	bool mEnbale = true;
	public bool isEnable
	{
		get
		{
			return mEnbale;
		}
		set
		{
			mEnbale = value;
			mCollider.enabled = value;
			mLbText.color = value ? Color.white : Color.gray;
		}
	}

	public void Init(string text,int index)
	{
		mLbText.text = text;
		mIndex = index;
	}
	
	void OnMouseOver()
	{
		mMouseSpr.enabled = true;
	}
	
	void OnMouseOut()
	{
		mMouseSpr.enabled = false;
	}

	void OnSelectItem()
	{
		if (e_OnClickItem != null)
			e_OnClickItem(this);
	}
}
