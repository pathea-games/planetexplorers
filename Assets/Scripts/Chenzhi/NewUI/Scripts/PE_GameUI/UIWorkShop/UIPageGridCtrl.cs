using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class UIPageGridCtrl : MonoBehaviour 
{

	public event OnGuiIndexBaseCallBack ClickLocalFloder = null;
	public event OnGuiIndexBaseCallBack ClickLocalIso = null;
	public event OnGuiIndexBaseCallBack ClickWorkShop = null;
	public event OnGuiIndexBaseCallBack	ClickUpload = null;
	

	public event OnGuiIndexBaseCallBack ClickWorkShopBtnReLoad = null;
	public event OnGuiIndexBaseCallBack ClickUpLoadBtnReLoad = null;


	public event OnGuiIndexBaseCallBack ClickWorkShopBtnDing = null;
	public event OnGuiIndexBaseCallBack ClickWorkShopBtnCai = null;


	public event OnGuiIndexBaseCallBack DoubleClickLocalFloder = null;
	public event OnGuiIndexBaseCallBack DoubleClickLocalIso = null;



	public delegate void UpdateItem(int index_0);
	public event UpdateItem mUpdateGrid;

	public GameObject mLeftBtns;
	public GameObject mRightBtns;
	public UILabel mPageTextLabel;
	public GameObject mGridItem;
	public UIGrid mGrid;


	
	public int BtnMovePos = 12;
	public int mPagIndex = 0;
	public int mMaxPagIndex = 0;
    public bool IsLocalPage = false; //log:lz-2016.05.23 因为LocalPage和其他界面显示的格子列数量要多一列
    public int mMaxGridCount { get { return (int)(IsLocalPage ? UIWorkShopCtrl.GetCurLocalShowCount() : UIWorkShopCtrl.GetCurRequestCount()); } }

	
	public List<UIWorkShopGridItem> mUIItems = new List<UIWorkShopGridItem>();
	public int mSelectedIndex = -1;
	public int mUISeletedIndex = -1;
	int itemCount;


    #region mono methods
    void Awake()
	{
		InitGrid();
	}


	// Use this for initialization
	void Start () 
	{
	
	}
    #endregion

    #region private methods
    public void InitGrid()
	{
        mGrid.maxPerLine = IsLocalPage ? UIWorkShopCtrl.GetCurColumnCount() + 1 : UIWorkShopCtrl.GetCurColumnCount();
		for (int i=0;i<mMaxGridCount;i++)
		{
			GameObject o = GameObject.Instantiate(mGridItem) as GameObject;
			o.transform.parent = mGrid.gameObject.transform;
			o.transform.localRotation = Quaternion.identity;
			o.transform.localScale = Vector3.one;
			o.transform.localPosition = new Vector3(0,0,0);
			
			UIWorkShopGridItem item =  o.GetComponent<UIWorkShopGridItem>();
			item.index = i;
			item.mClickItem += OnClickItem;
			item.mDoubleClickItem += OnDoubleClickItem;
			item.mBtnReloadOnClick += OnBtnReloadOnClick;
			item.mBtnDingOnClick += OnClickWorkShopBtnDing;
			item.mBtnCaiOnClick += OnClickWorkShopBtnCai;
			mUIItems.Add(item);  
		}
	}
    #endregion

    #region public methods
    public void ReSetGrid( int _itemCount)
	{
		itemCount = _itemCount;
	}


	int tempResult = 0;
	public void _UpdatePagText()
	{


		if(itemCount > 0 && mPagIndex == 0)
			mPagIndex = 1;

		if(itemCount <= mMaxGridCount)
		{
			if(itemCount == 0)
				mMaxPagIndex = 0;
			else
				mMaxPagIndex = 1;
			
			if(mPagIndex > mMaxPagIndex)
				mPagIndex = mMaxPagIndex;
		}
		else
		{
			mMaxPagIndex = (itemCount-1) / mMaxGridCount + 1;
			
			if(mPagIndex > mMaxPagIndex)
				mPagIndex = mMaxPagIndex;
		}

		int Result = 1;
		int MaxResult = 1;
		
		if(mPagIndex>9)
		{
			int temp = mPagIndex;
			while (temp >= 10)
			{
				temp =  temp/10;
				Result++;
			}
		}
		
		if(mMaxPagIndex>9)
		{
			int temp = mMaxPagIndex;
			while (temp >= 10)
			{
				temp =  temp/10;
				MaxResult++;
			}
		}
		
		if(tempResult != MaxResult-1)
		{
			Vector3 btnPos_r =  mRightBtns.transform.localPosition;
			btnPos_r.x +=  BtnMovePos* ( (MaxResult-1) - tempResult);
			mRightBtns.transform.localPosition = btnPos_r;
			
			Vector3 btnPos_l =  mLeftBtns.transform.localPosition;
			btnPos_l.x -=  BtnMovePos* ( (MaxResult-1) - tempResult);
			mLeftBtns.transform.localPosition = btnPos_l;
			
			tempResult = MaxResult-1;
		}
		
		string indexText="";
		int m = MaxResult - Result;
		while ( m > 0)
		{
			indexText += "0";
			m--;
		}
		
		mPageTextLabel.text =  indexText + mPagIndex.ToString() + "/" + mMaxPagIndex.ToString();

		mSelectedIndex = -1;
      	mUISeletedIndex = -1;

		int index_0 = 0;
        if (mPagIndex != 0)
        {
            index_0 = mMaxGridCount * (mPagIndex - 1);
            //lz-2016.07.07 如果页数为0，就不调用更新事件，不然会一直关闭右边的信息界面
            if (mUpdateGrid != null)
                mUpdateGrid(index_0);
        }
	}

    public UIWorkShopGridItem GetWorkShopItemByFileName(string fileName)
    {
        if (mUIItems == null)
        {
            return null;
        }
        else
        {
            return this.mUIItems.FirstOrDefault(a => a!=null&&a.IsoFileName.Equals(fileName));
        }
    }
    #endregion

    #region private methods
    private void BtnLeftEndOnClick()
	{
		if(mPagIndex <= 1)
			return;
		mPagIndex = 1;
		_UpdatePagText();
	}
	
	private void BtnLeftOnClick()
	{
		if(mPagIndex <= 1)
			return;
		mPagIndex --;
		_UpdatePagText();
	}
	
	
	private void BtnRightEndOnClick()
	{
		if(mPagIndex >= mMaxPagIndex)
			return;
		mPagIndex = mMaxPagIndex;
		_UpdatePagText();
	}
	
	private void BtnRightOnClick()
	{
		
		if(mPagIndex >= mMaxPagIndex)
			return;
		mPagIndex ++;
		_UpdatePagText();
	}


	private void OnClickItem(WorkGridItemType mType,int UIIndex)
	{
		if (mPagIndex <= 0)
			return;

		int index = UIIndex + mMaxGridCount * (mPagIndex-1);

		if (UIIndex != mUISeletedIndex)
		{
			if(mUISeletedIndex >=0 && mUISeletedIndex < mUIItems.Count)
				mUIItems[mUISeletedIndex].SetSelected(false);
			
			mUISeletedIndex = UIIndex;
			mUIItems[UIIndex].SetSelected(true);
		}

		mSelectedIndex = index;


		if (mType == WorkGridItemType.mLocalFloder && ClickLocalFloder != null)
			ClickLocalFloder(index);
		else if (mType == WorkGridItemType.mLocalIcon && ClickLocalIso != null)
			ClickLocalIso(index);
		else if (mType == WorkGridItemType.mWorkShop && ClickWorkShop != null)
			ClickWorkShop(index);
		else if (mType == WorkGridItemType.mUpLoad && ClickUpload != null)
			ClickUpload(index);
	}


	private void OnDoubleClickItem(WorkGridItemType mType,int UIIndex)
	{
		if (mPagIndex <= 0)
			return;

		int index = UIIndex + mMaxGridCount * (mPagIndex-1);
		mSelectedIndex = index;
		
		
		if (mType == WorkGridItemType.mLocalFloder && DoubleClickLocalFloder != null)
			DoubleClickLocalFloder(mSelectedIndex);
		else if (mType == WorkGridItemType.mLocalIcon && DoubleClickLocalIso != null)
			DoubleClickLocalIso(mSelectedIndex);
	}



	private void OnBtnReloadOnClick(WorkGridItemType mType,int UIIndex)
	{
		if (mPagIndex <= 0)
			return;
		
		int index = UIIndex + mMaxGridCount * (mPagIndex-1);
		mSelectedIndex = index;

		if (mType == WorkGridItemType.mWorkShop && ClickWorkShopBtnReLoad != null)
			ClickWorkShopBtnReLoad(index);
		else if (mType == WorkGridItemType.mUpLoad && ClickUpLoadBtnReLoad != null)
			ClickUpLoadBtnReLoad(index);
	}

	private void OnClickWorkShopBtnDing(WorkGridItemType mType,int UIIndex)
	{
		if (mPagIndex <= 0)
			return;
		
		int index = UIIndex + mMaxGridCount * (mPagIndex-1);
		mSelectedIndex = index;
		
		if (mType == WorkGridItemType.mWorkShop && ClickWorkShopBtnDing != null)
			ClickWorkShopBtnDing(index);
	}

	private void OnClickWorkShopBtnCai(WorkGridItemType mType,int UIIndex)
	{
		if (mPagIndex <= 0)
			return;
		
		int index = UIIndex + mMaxGridCount * (mPagIndex-1);
		mSelectedIndex = index;
		
		if (mType == WorkGridItemType.mWorkShop && ClickWorkShopBtnCai != null)
			ClickWorkShopBtnCai(index);
    }
    #endregion
}
