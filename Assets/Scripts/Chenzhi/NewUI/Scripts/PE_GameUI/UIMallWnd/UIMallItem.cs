using UnityEngine;
using System.Collections;

public class UIMallItem : MonoBehaviour 
{
	//UIMailItem
	public UICheckbox 	mCheckBox;
	public UILabel 		mLbText;
	public UILabel		mLbPice;
	public int 			mIndex = 0;
	public UISprite     mSpr;
	public BoxCollider 	mCollider;
	public GameObject 	mBtnBuy;
	public GameObject   mBtnExport;
	public UISprite   	mSprDiscount;
	public UILabel		mLbDisCount;
	public UILabel		mLbCount;
	
	public delegate void MallItem(int index, UIMallItem item);
	public event MallItem e_ItemBuy;
	public event MallItem e_OnClick;
	public event MallItem e_ItemExport;

	public MallItemData mData;
	
	public void SetInfo( MallItemData data ,int index)
	{
		mLbText.text = data.GetName();
		mLbPice.text = data.GetPrice().ToString() + " [C8C800]PP";
		mSpr.spriteName = data.GetSprName();
		mSpr.MakePixelPerfect();
		mIndex = index;
		mCollider.enabled = true;
		mData = data;
		mLbCount.text = mData.GetCount().ToString();
	}

	public void ClearInfo()
	{
		mSpr.spriteName = "Null"; 
		mLbPice.text = "";
		mLbText.text = "";
		mCollider.enabled = false;
		mCheckBox.isChecked = false;
		mLbDisCount.text = "";
		mSprDiscount.enabled = false;
		mLbCount.text = "";
		mData = null;
	}
	

	void BtnBuy_OnClick()
	{
		if (e_ItemBuy != null)
			e_ItemBuy(mIndex,this);
	}


	void BtnExport_OnClick()
	{
		if (e_ItemExport != null)
			e_ItemExport(mIndex,this);
	}

	void MallItem_OnClick()
	{
		if (e_OnClick != null)
			e_OnClick(mIndex,this);
	}



	void Update()
	{
		if (UIMallWnd.Instance == null)
			return;
		if ( UIMallWnd.Instance.mCurrentTab == Mall_Tab.tab_Item 
		    ||  UIMallWnd.Instance.mCurrentTab == Mall_Tab.tab_Equip)
		{
			mBtnBuy.SetActive(false);
			mBtnExport.SetActive(Pathea.PeFlowMgr.Instance.curScene == Pathea.PeFlowMgr.EPeScene.GameScene);
		}
		else
		{
			mBtnBuy.SetActive(true);
			mBtnExport.SetActive(false);
		}


		if ( UIMallWnd.Instance.mCurrentTab == Mall_Tab.tab_Hot)
		{
			if (mData != null)
			{
				mSprDiscount.enabled = mData.ShowDiscount();
				mLbDisCount.enabled = mData.ShowDiscount();
				mLbDisCount.text = mData.GetDiscount() + "%";
			}
			else 
			{
				mSprDiscount.enabled = false;
				mLbDisCount.text = "";
			}
		}
		else 
		{
			mSprDiscount.enabled = false;
			mLbDisCount.text = "";
		}


		if ( mData != null )
		{
			mLbPice.enabled = (mData.GetPrice() == -1) ? false : true;
		}
		else
		{

			mBtnBuy.SetActive(false);
			mLbPice.text = "";
		}
	}
}
