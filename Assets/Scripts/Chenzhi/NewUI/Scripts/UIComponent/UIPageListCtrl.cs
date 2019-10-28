using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CustomData;

public class PageListItem
{
	public List<string> mData;
	public Color mColor;
	public bool mEanbleICon;
}

public class UIPageListCtrl : MonoBehaviour
{
	public delegate void OnClickPageBtnFunc();
	public event OnClickPageBtnFunc PageBtnLeft;
	public event OnClickPageBtnFunc PageBtnRight;
	public event OnClickPageBtnFunc PageBtnLeftEnd;
	public event OnClickPageBtnFunc PageBtnRightEnd;

	public delegate void FuncForIndex(int index);
	public event FuncForIndex CheckItem;
	public event FuncForIndex DoubleClickItem;
	// Use this for initialization


	public string[] mHeaderTexts;
	public int[] mHeaderWidth;
	public GameObject mHerderPrefab;
	public GameObject mHerderContent;
	public GameObject mItemPrefab;
	public GameObject mListContent;
	public float mItemHeight;
	public GameObject mLeftBtns;
	public GameObject mRightBtns;
	public UILabel mPageTextLabel;

	public GameObject mSortUp;
	public GameObject mSortDn;
	public GameObject mSortDefault;
	public int mLockSortState = 0;
	
	public bool CanSelected;
	public int BtnMovePos = 12;
	public int mPagIndex = 0;
	public int mMaxPagIndex = 0;
	public int mMaxListCount = 9;




	public List<UIListItemCtrl> mItemCtrls = new List<UIListItemCtrl>();

	public int mSelectedIndex = -1;
	public List< PageListItem > mItems = new  List< PageListItem >();

	public int mUISeletedIndex = -1;


	public List<UIListHeaderItem> mHeaderItems = new List<UIListHeaderItem>();

	public bool AddItem(List<string> _data)
	{
		if(_data.Count > mHeaderTexts.Length)
			return false;
		PageListItem item = new PageListItem();
		item.mData = _data;
		item.mColor = Color.white;

		mItems.Add(item);
		return true;
	}


	public bool AddItem(List<string> _data,Color _color)
	{
		if(_data.Count > mHeaderTexts.Length)
			return false;
		PageListItem item = new PageListItem();
		item.mData = _data;
		item.mColor = _color;
		
		mItems.Add(item);
		return true;
	}

	public bool AddItem(PageListItem item)
	{
		if(item.mData.Count > mHeaderTexts.Length)
			return false;
		mItems.Add(item);
		return true;
	}

	public void SetColor(int _index,Color _color)
	{
		mItems[_index].mColor = _color;
		int uiIndex = _index - mMaxListCount * mPagIndex;
		if(uiIndex >=0 && uiIndex<mMaxListCount && mItemCtrls.Count != 0)
			mItemCtrls[uiIndex].SetColor(_color);
	}

	public void DeleteItem()
	{
		if(mSelectedIndex == -1)
			return;
		if(mSelectedIndex >= mItems.Count)
			return;

		mItems.RemoveAt(mSelectedIndex);
		UpdateList();
	}

	public void ClearSelected()
	{
		for (int i=0;i<mItemCtrls.Count;i++)
			mItemCtrls[i].SetSelected(false);
	}


	public void SetLockUIState(int sortState)
	{
		if (mSortUp == null  || mSortDn == null || mSortDefault == null )
			return;
		if (sortState == 0)
		{
			mSortUp.SetActive(false);
			mSortDn.SetActive(false);
			mSortDefault.SetActive(true);
		}
		else if (sortState == 1)
		{
			mSortUp.SetActive(true);
			mSortDn.SetActive(false);
			mSortDefault.SetActive(false);
		}
		else if (sortState == 2)
		{
			mSortUp.SetActive(false);
			mSortDn.SetActive(true);
			mSortDefault.SetActive(false);
		}

		mLockSortState = sortState;
	}


	public void UpdateList()
	{
		// No Init List
		if(mItemCtrls.Count != mMaxListCount)
			return;

		int count = 0;
		int index = 0;

		if(mItems.Count > 0 && mPagIndex == 0)
			mPagIndex = 1;


		if(mItems.Count <= mMaxListCount)
		{
			if(mItems.Count == 0)
				mMaxPagIndex = 0;
			else
				mMaxPagIndex = 1;
			count = mItems.Count;
			index = 0;

			if(mPagIndex > mMaxPagIndex)
				mPagIndex = mMaxPagIndex;
		}
		else
		{
			mMaxPagIndex = (mItems.Count-1) / mMaxListCount + 1;

			if(mPagIndex > mMaxPagIndex)
				mPagIndex = mMaxPagIndex;

			int temp = mItems.Count - (mPagIndex-1) * mMaxListCount;
			if(temp > mMaxListCount)
				count = mMaxListCount;
			else
				count = temp;

			index = (mPagIndex-1) * mMaxListCount;
		}


		if(mSelectedIndex >= mItems.Count || mItems.Count == 0)
		{
			mSelectedIndex = -1;
			mUISeletedIndex = -1;
		}


		for(int i=0; i<mMaxListCount ;i++)
		{
			if(i < count)
			{
				mItemCtrls[i].SetItemText(mItems[index + i].mData);
				mItemCtrls[i].SetColor(mItems[index + i].mColor);
				mItemCtrls[i].SetIconActive(mItems[index+i].mEanbleICon);

				if(CanSelected)
					mItemCtrls[i].SetActive(true);
				else
					mItemCtrls[i].SetActive(false);

				if(index + i == mSelectedIndex)
				{
					mItemCtrls[i].SetSelected(true);
					mUISeletedIndex = i;
				}
				else
				{
					mItemCtrls[i].SetSelected(false);
				}
			}
			else
			{
				mItemCtrls[i].ClearItemText();
				mItemCtrls[i].SetActive(false);
				mItemCtrls[i].SetSelected(false);
				mItemCtrls[i].SetIconActive(false);
			}
		}

		UpdatePagText();
	}
	

	int tempResult = 0;
	private void UpdatePagText()
	{

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
	}


	private void InitHerderInfo()
	{
		if (mHeaderTexts.Length != mHeaderWidth.Length)
			return;

		mHeaderItems.Clear();
		float pos_x = 0;
		for (int i=0;i<mHeaderTexts.Length;i++)
		{
			GameObject o = GameObject.Instantiate(mHerderPrefab) as GameObject;
			o.transform.parent = mHerderContent.transform;
			o.transform.localRotation = Quaternion.identity;
			o.transform.localScale = Vector3.one;
			UIListHeaderItem herder =  o.GetComponent<UIListHeaderItem>();
			herder.Init(mHeaderTexts[i],pos_x,mHeaderWidth[i],i);
			pos_x += mHeaderWidth[i];

			mHeaderItems.Add(herder);
		}
	}

	private void InitList()
	{
		for(int i=0;i<mMaxListCount;i++)
		{
			GameObject o = GameObject.Instantiate(mItemPrefab) as GameObject;
			o.transform.parent = mListContent.transform;
			o.transform.localRotation = Quaternion.identity;
			o.transform.localScale = Vector3.one;
			o.transform.localPosition = new Vector3(0,-mItemHeight*i,0);

			UIListItemCtrl item =  o.GetComponent<UIListItemCtrl>();
			item.InitItem(mHeaderWidth);
			item.mIndex = i;
			item.SetActive(false);
			item.mIndex = mItemCtrls.Count;
			item.ListItemChecked += UICheckItem;
			item.listItemDoubleClick += UIDoubleClickItem;

			mItemCtrls.Add(item);  
		}
	}

	void Awake()
	{
		InitHerderInfo();
		InitList();
	}

	void Start() 
	{


// -----------------------------   text code -------------------------------------

//		for (int i=0;i< 2000; i ++)
//		{
//
//			// text.Count must <= mHeaderTexts.Length
//			List<string> text = new List<string>();
//			text.Add("server name");
//			text.Add("123456");
//			text.Add(i.ToString());
//
//			AddItem(text);
//		}
//
//
//		UpdateList();

// ------------------------------------------------------------------------------


	}

	// Update is called once per frame
	void Update () 
	{
	
	}

	private void BtnLeftEndOnClick()
	{
		if(mPagIndex <= 1)
			return;
		mPagIndex = 1;
		UpdateList();

		if(PageBtnLeftEnd != null)
			PageBtnLeftEnd();
	}

	private void BtnLeftOnClick()
	{
		if(mPagIndex <= 1)
			return;
		mPagIndex --;
		UpdateList();

		if(PageBtnLeft != null)
			PageBtnLeft();
	}


	private void BtnRightEndOnClick()
	{
		if(mPagIndex >= mMaxPagIndex)
			return;
		mPagIndex = mMaxPagIndex;
		UpdateList();

		if(PageBtnRightEnd != null)
			PageBtnRightEnd();
	}

	private void BtnRightOnClick()
	{

		if(mPagIndex >= mMaxPagIndex)
			return;
		mPagIndex ++;
		UpdateList();

		if(PageBtnRight != null)
			PageBtnRight();
	}




	private void UICheckItem(int UIIndex)
	{
		if(mUISeletedIndex >=0 && mUISeletedIndex<mItemCtrls.Count)
			mItemCtrls[mUISeletedIndex].SetSelected(false);

		int index = UIIndex + mMaxListCount * (mPagIndex-1);
		mUISeletedIndex = UIIndex;
		mItemCtrls[UIIndex].SetSelected(true);
		mSelectedIndex = index;

		if(CheckItem != null)
			CheckItem(index);

	}


	private void UIDoubleClickItem(int UIIndex)
	{
		int index = UIIndex + mMaxListCount * (mPagIndex-1);
		//Debug.Log("Check " + index.ToString());
		mUISeletedIndex = UIIndex;
		mSelectedIndex = index;
		
		if(DoubleClickItem != null)
			DoubleClickItem(index);
	}

//	void OnSubmit(string inputString)
//	{
//		GameClientLobby.Self.LobbyRPC("RPC_C2L_SendMsg",
//		                              EMSGTYPE.ToAll,
//		                              GameClientLobby.Self.Role.account,
//		                              GameClientLobby.Self.Role.name,
//		                              inputString);
//
//	}
}
