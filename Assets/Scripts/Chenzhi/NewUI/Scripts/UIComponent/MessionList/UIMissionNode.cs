using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIMissionNode : MonoBehaviour 
{
	public bool enableCkTag
	{
		get
		{
			if (mCheckBoxTag == null)
				return false;
			return mCheckBoxTag.gameObject.activeSelf;
		}
		set
		{
			if (mCheckBoxTag == null)
				return;
			mCheckBoxTag.gameObject.SetActive(value);
		}
	}
	public bool enableBtnDelete
	{
		get
		{
			if (mBtnDelete == null)
				return false;
			return mBtnDelete.gameObject.activeSelf;
		}
		set
		{
			if (mBtnDelete == null)
				return;
			mBtnDelete.gameObject.SetActive(value);
		}
	}

	public bool Selected
	{
		get
		{
			return mSpSelected.enabled;
		}
		set
		{
			if (!mCanSelected)
				mSpSelected.enabled = false;
			else
				mSpSelected.enabled = value;
		}
	}

	public UICheckbox mCheckBoxTag;
	public UIButton mBtnDelete;
    public UIButton mBtnDetail;  //lz-2016.08.17 点这个按钮可以到大人任务界面定位到此任务
	public UILabel mLbTitle;
	public UISprite mSpSelected;
    public UISprite mSpState;
	public TweenScale mTeewn;
	public UITable  mTable;
	[SerializeField] UIButtonTween mBtnTween;

	[HideInInspector]
	public bool 	mCanSelected;
	[HideInInspector]
    public UIMissionNode mParent { get { return m_Parent; } set { if (null!=mBtnDetail)mBtnDetail.gameObject.SetActive(value == null); this.m_Parent = value; } }
	[HideInInspector]
	public List<UIMissionNode> mChilds = new List<UIMissionNode>();
	[HideInInspector]
	public UITable  mTablePartent = null;
	[HideInInspector]
	public object mData = null;
	
	//private bool _IsExpand = false;
	// events
	public delegate void BaseMsgEvent(object sender);
	public event BaseMsgEvent e_OnClick = null;
	public event BaseMsgEvent e_BtnDelete = null;
	public delegate void CheckMsgEvent(object sender,bool isChecked);
	public event CheckMsgEvent e_CheckedTg = null;

    private UIMissionNode m_Parent;
	
	void Update()
	{
		mSpState.enabled = (mChilds.Count>0) ? true : false;
		mSpState.spriteName = mTeewn.gameObject.activeSelf ? "mission_open" : "mission_closed";
	}
	
	void ItemOnClick()
	{		
		if (!mCanSelected)
			return;

		if (e_OnClick != null)
			e_OnClick(this);
	}

	void ItemBtnDeleteOnlick()
	{
		if (e_BtnDelete != null)
			e_BtnDelete(this);
	}

    void ItemBtnDetailOnClick()
    {
        if (!GameUI.Instance.mUIMissionWndCtrl.isShow)
            GameUI.Instance.mUIMissionWndCtrl.Show();
        GameUI.Instance.mUIMissionWndCtrl.SelectMissionNodeByData(this.mData);
    }

	void ItemCheckedTag(bool isChecked)
	{
		if (e_CheckedTg != null)
			e_CheckedTg(this,isChecked);
	}

	
	public void ChangeExpand()
	{
		Invoke("DoChangeExpand",0.2f);
	}

	void DoChangeExpand()
	{
		mBtnTween.Play(true);
	}

}
