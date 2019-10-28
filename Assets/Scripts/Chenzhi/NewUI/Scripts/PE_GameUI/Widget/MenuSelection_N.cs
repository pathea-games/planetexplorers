using UnityEngine;
using System.Collections;

public class MenuSelection_N : MonoBehaviour 
{
	public UISlicedSprite mBgSpr;
	public UILabel	mTextLabel;
	public UISprite	mExpansionSpr;
	
	UIButton		mButton;
	//SimpleMenu_N	mMenu;
	//int				mIndex;
	
	public bool ExpansionEnable
	{
		set 
		{
			mExpansionSpr.enabled = value;
			mButton.isEnabled = value;
			mTextLabel.color = value ? Color.white : Color.gray;
		} 
	}

	public bool ShowExpansion
	{
		set { mExpansionSpr.enabled = value; }
	}
	
	public Vector3 Size{ get { return mBgSpr.transform.localScale; } }
	
	void Awake()
	{
		mTextLabel.transform.localPosition = new Vector3(mBgSpr.transform.localScale.x / 2f, -mBgSpr.transform.localScale.y / 2f, 0);
		mExpansionSpr.transform.localPosition = new Vector3(mBgSpr.transform.localScale.x - 2f, -mBgSpr.transform.localScale.y / 2f, 0);
		BoxCollider BC = GetComponent<BoxCollider>();
		BC.size = mBgSpr.transform.localScale;
		BC.center = new Vector3(BC.size.x / 2f, -BC.size.y / 2f, -1f);
		mButton = GetComponent<UIButton>();
	}
		
	public void SetInfo(string content, SimpleMenu_N menu, int index)
	{
		mTextLabel.text = content;
		//mMenu = menu;
		//mIndex = index;
	}
	
	void OnClick()
	{
		//mMenu.OnSelectionChange(mIndex, mTextLabel.text);
	}
}
