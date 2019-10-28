using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class SimpleMenu_N : MonoBehaviour
{
	public MenuSelection_N	mCurSelection;
	
	List<MenuSelection_N>	mSelections;
	
	public UITweener		mExpansionTrigger;
	
	public UISlicedSprite	mExpansionBg;
	
	public UIScrollBar		mScrollBar;
	
	public UIPanel			mSelPanel;
	
	public int 				mSelNumPerPage = 10;

	public float 			mUpdateStep = 0.2f;
	
	//bool mExpansion;
	
	static SimpleMenu_N 	mExpansionMenu;
	
	public event OnMenuSelectionChange SelectedEvent;
	
	void Awake()
	{
		//mExpansion = false;
		mSelections = new List<MenuSelection_N>();
		mExpansionTrigger.transform.localPosition = (mCurSelection.Size.x + mExpansionBg.sprite.paddingLeft) * Vector3.right;
		mExpansionBg.transform.localPosition = mExpansionBg.sprite.paddingTop * Vector3.up;
	}
	
	public string CurSelection
	{
		get{ return mCurSelection.mTextLabel.text; }
		set{ mCurSelection.mTextLabel.text = value; }
	}

	public void PlayExpansion(bool forward)
	{
		if(forward)
		{
			mScrollBar.scrollValue = 0;
			mExpansionTrigger.transform.localScale = Vector3.one;
			mSelPanel.enabled = false;
			mUpdateStep = 0.1f;
		}
		else
		{
			mScrollBar.scrollValue = 0;
			mExpansionTrigger.transform.localScale = Vector3.zero;
		}
	}
	
	public void SetSelections(List<string> selections)
	{
		foreach(MenuSelection_N sel in mSelections)
		{
			Destroy(sel.gameObject);
		}
		mSelections.Clear();
		
		mCurSelection.ExpansionEnable = selections.Count > 1;
		
		if(selections.Count > 0)
			mCurSelection.SetInfo(selections[0], this, -1);
		else
			mCurSelection.SetInfo("", this, -1);
			
		if(selections.Count > 1)
		{
			if(selections.Count > mSelNumPerPage)
			{
				mScrollBar.gameObject.SetActive(true);
				mExpansionBg.transform.localScale = new Vector3(mExpansionBg.sprite.paddingLeft + mExpansionBg.sprite.paddingRight + mCurSelection.Size.x
					, mExpansionBg.sprite.paddingTop + mExpansionBg.sprite.paddingBottom + 10f * mCurSelection.Size.y, 1);
				mExpansionBg.transform.localScale += 10f * Vector3.right;
				mScrollBar.transform.localPosition = new Vector3(mExpansionBg.transform.localScale.x - 5f, -5, 0);
				mScrollBar.background.transform.localScale = new Vector3(4f, mExpansionBg.transform.localScale.y - 10f, 0);
			}
			else
			{
				mScrollBar.gameObject.SetActive(false);
				mExpansionBg.transform.localScale = new Vector3(mExpansionBg.sprite.paddingLeft + mExpansionBg.sprite.paddingRight + mCurSelection.Size.x
					, mExpansionBg.sprite.paddingTop + mExpansionBg.sprite.paddingBottom + (selections.Count) * mCurSelection.Size.y, 1);
			}
			mSelPanel.clipRange = new Vector4(mExpansionBg.transform.localScale.x / 2f, -mExpansionBg.transform.localScale.y / 2f
												, mExpansionBg.transform.localScale.x, mExpansionBg.transform.localScale.y);
			for(int i = 0; i < selections.Count; i++)
			{
				MenuSelection_N sel = Instantiate(mCurSelection) as MenuSelection_N;
				sel.transform.parent = mSelPanel.transform;
				sel.transform.localPosition = i * mCurSelection.Size.y * Vector3.down;
				sel.transform.localScale = Vector3.one;
				sel.SetInfo(selections[i], this, i);
				sel.ShowExpansion = false;
				mSelections.Add(sel);
			}
			mScrollBar.scrollValue = 0;
		}
	}

	void Update()
	{
		if(!mSelPanel.enabled)
		{
			mUpdateStep -= Time.deltaTime;
			if(mUpdateStep < 0)
				mSelPanel.enabled = true;
		}
	}
	

	public void OnSelectionChange(int index, string text)
	{
		if(null != mExpansionMenu && mExpansionMenu != this)
			mExpansionMenu.PlayExpansion(false);

		if(-1 == index)
		{
			if(this == mExpansionMenu)
			{
				PlayExpansion(false);
				mExpansionMenu = null;
			}
			else
			{
				PlayExpansion(true);
				mExpansionMenu = this;
			}
		}
		else
		{
			PlayExpansion(false);
			mExpansionMenu = null;
			if(null != SelectedEvent)
				SelectedEvent(index, text);
		}
	}
}
