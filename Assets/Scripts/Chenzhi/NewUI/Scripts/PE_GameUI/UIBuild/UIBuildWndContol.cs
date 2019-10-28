using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class UIBuildWndContol : UIBaseWnd 
{
	#region funcs delete event
	public delegate void OnClickFun();
	public event OnClickFun BtnClose = null;
	public event OnClickFun CkControl2 = null;

	public delegate void OnPageClickFunc(int index);
	public event OnPageClickFunc onPageClick = null;

		
	public delegate void ItemClickFunc(int mIndex);
	public event ItemClickFunc BlockItemOnClick = null;
	public event ItemClickFunc BlockPatternOnClick = null;
	public event ItemClickFunc TextureItemOnClick = null;
	public event ItemClickFunc IsoItemOnClick = null;
	public event ItemClickFunc VoxelTypeOnClick = null;
	public event ItemClickFunc BtnDelete = null;
	public event ItemClickFunc BtnExport = null;

	public delegate void ToolToipFunc(bool isShow,UIBuildWndItem.ItemType _ItemType,int _Index);
	public event ToolToipFunc ToolTip = null;
	#endregion
	

	#region class member
	[HideInInspector]
	public Camera uiCamera;
	public UIBuildMenuControl mMeunControl;

	public GameObject mPageBlack;
	public GameObject mPageTexture;
	public GameObject mPageSave;
	public UICheckbox mCbBlock;
	public UICheckbox mCbTexture;
	public UICheckbox mCbSave;
	public UICheckbox mCbControl2;
	public GameObject mDefoultWndItem; 
	public GameObject mIsoWndItem;
	public GameObject mCostItem;
	public GameObject mBlockMatGrid;
	public GameObject mBlockPatternGrid;
	public GameObject mVoxelMatGrid;
	public GameObject mVoxelTypeGrid;
	public GameObject mIsoGrid;
	public GameObject mCostGrid;
	public GameObject mDragItemContent;
	public UILabel 	  mLbTitle;


	// Block
	public List<UIBuildWndItem> mBlockMatList = new List<UIBuildWndItem>();  
	public List<UIBuildWndItem> mBlockPatternList = new List<UIBuildWndItem>();
	 
	public List<UIBuildWndItem> mIsoList = new List<UIBuildWndItem>();
	public List<UIBuildWndItem> mCostList = new List<UIBuildWndItem>();

	// Voxel
	public List<UIBuildWndItem> mVoxelMatList = new List<UIBuildWndItem>();
	public List<UIBuildWndItem> mTypeList = new List<UIBuildWndItem>();
	
	private int mBlockMatListSelectIndex = 0;
	public int BlockMatSelectIndex
	{
		get{ return mBlockMatListSelectIndex;}
		set
		{
			if(-1 != mBlockMatListSelectIndex && mBlockMatListSelectIndex < mBlockMatList.Count)
				mBlockMatList[mBlockMatListSelectIndex].SetSelect(false);
			
            if (value != -1 && value < mBlockMatList.Count)
            {
                mBlockMatList[value].SetSelect(true);
                mBlockMatListSelectIndex = value;
            }
            else
                mBlockMatListSelectIndex = -1;
        }
	}

	private int mBlockPatternListSelectIndex = 0;
	public int BlockPatternSelectIndex
	{
		get { return mBlockPatternListSelectIndex; }
		set
		{
			if ( -1 != mBlockPatternListSelectIndex && mBlockPatternListSelectIndex < mBlockPatternList.Count)
				mBlockPatternList[mBlockPatternListSelectIndex].SetSelect(false);

            if (value != -1 && value < mBlockPatternList.Count)
            {
                mBlockPatternList[value].SetSelect(true);
                mBlockPatternListSelectIndex = value;
            }
            else
                mBlockPatternListSelectIndex = -1;
        }
	}


	private int mVoxelMatListSelectIndex = -1;
	public int TextureListSelectIndex
	{
		get{ return mVoxelMatListSelectIndex;}
		set
		{
			if(-1 != mVoxelMatListSelectIndex && mVoxelMatListSelectIndex < mVoxelMatList.Count)
				mVoxelMatList[mVoxelMatListSelectIndex].SetSelect(false);

            if (value != -1 && value < mVoxelMatList.Count)
            {
                mVoxelMatList[value].SetSelect(true);
                mVoxelMatListSelectIndex = value;
            }
            else
                mVoxelMatListSelectIndex = -1;

        }
	}


	private int mIsoListSelectIndex = -1;
	public int IsoListSelectIndex
	{
		get{ return mIsoListSelectIndex;}
		set
		{
			if(-1 != mIsoListSelectIndex && mIsoListSelectIndex < mIsoList.Count)
				mIsoList[mIsoListSelectIndex].SetSelect(false);

            if (value != -1 && value < mIsoList.Count)
            {
                mIsoListSelectIndex = value;
                mIsoList[value].SetSelect(true);
            }
            else
                mIsoListSelectIndex = -1;

        }
	}


	private int mCostSelectIndex = -1;
	public int CostSelectIndex
	{
		get{ return mCostSelectIndex;}
		set
		{
			if(-1 != mCostSelectIndex && mCostSelectIndex < mCostList.Count)
				mCostList[mCostSelectIndex].SetSelect(false);
			
            if (value != -1 && value < mCostList.Count)
            {
                mCostList[value].SetSelect(true);
                mCostSelectIndex = value;
            }
            else
            {
                mCostSelectIndex = -1;
            }
		}
	}

	private int mTypeSelectIndex = -1;
	public int TypeSelectIndex
	{
		get { return mTypeSelectIndex; }
		set
		{

			if (mTypeSelectIndex != -1 && mTypeSelectIndex < mTypeList.Count)
				mTypeList[mTypeSelectIndex].SetSelect(false);
			
            if (value != -1 && value < mTypeList.Count)
            {
                mTypeList[value].SetSelect(true);
                mTypeSelectIndex = value;
            }
            else
            {
                mTypeSelectIndex = -1;
            }
		}
	}

	#endregion

	public override void Show ()
	{
		base.Show ();
        
		ResetBlockPostion();
		ResetVoxelPostion();

    }

	#region funcs Class Interface
	public bool GetControl2isChecked()
	{
		return mCbControl2.isChecked;
	}

	protected override void InitWindow()
	{
		if ( GameUI.Instance != null)
			uiCamera = GameUI.Instance.mUICamera;
		base.InitWindow ();
	}


	
	public int AddBlockListItem(Texture _contentTexture)
	{
		UIBuildWndItem item = AddBuildItem(mDefoultWndItem,mBlockMatGrid.transform);
		int index = mBlockMatList.Count;
		item.InitItem(UIBuildWndItem.ItemType.mBlockMat,_contentTexture,index);

		item.ClickItem += WndItemOnClick;
		item.BeginDrag += OnBeginDragMeaterItem;
		item.Drag += OnDragMeaterItem;
		item.Drop += OnDropMeaterItem;
		item.ToolTip += OnToolTip;

		mBlockMatList.Add(item);
		return index;
	}

	public int AddBlockListItem( string _spriteName,string _AtlasName = "Button")
	{
		UIBuildWndItem item = AddBuildItem(mDefoultWndItem,mBlockMatGrid.transform);
		int index = mBlockMatList.Count;
		item.InitItem(UIBuildWndItem.ItemType.mBlockMat,_spriteName,_AtlasName,index);

		item.ClickItem += WndItemOnClick;
		item.BeginDrag += OnBeginDragMeaterItem;
		item.Drag += OnDragMeaterItem;
		item.Drop += OnDropMeaterItem;
		item.ToolTip += OnToolTip;

		mBlockMatList.Add(item);
		return index;
	}

	public int AddBlockPatternItem (string _spriteName,string _AtlasName = "Button")
	{
		UIBuildWndItem item = AddBuildItem(mDefoultWndItem, mBlockPatternGrid.transform);
		int index = mBlockPatternList.Count;
		item.InitItem(UIBuildWndItem.ItemType.mBlockPattern,_spriteName,_AtlasName,index);

		item.ClickItem += WndItemOnClick;
		item.BeginDrag += OnBeginDragMeaterItem;
		item.Drag += OnDragMeaterItem;
		item.Drop += OnDropMeaterItem;
		item.ToolTip += OnToolTip;
		
		mBlockPatternList.Add(item);
		return index;
	}

//	public int AddBlockMatListItem (string _spriteName, string _AtlasName = "Button")
//	{
//		UIBuildWndItem item = AddBuildItem(mDefoultWndItem,mBlockGrid.transform);
//		int index = mBlockMatList.Count;
//		item.InitItem(UIBuildWndItem.ItemType.mTexture,_spriteName,_AtlasName,index);
//		
//		item.ClickItem += WndItemOnClick;
//		item.BeginDrag += OnBeginDragMeaterItem;
//		item.Drag += OnDragMeaterItem;
//		item.Drop += OnDropMeaterItem;
//		item.ToolTip += OnToolTip;
//		
//		mBlockMatList.Add(item);
//	}

	public int AddVoxelMatListItem( Texture _contentTexture)
	{
		UIBuildWndItem item = AddBuildItem(mDefoultWndItem,mVoxelMatGrid.transform);
		int index = mVoxelMatList.Count;
		item.InitItem(UIBuildWndItem.ItemType.mVoxelMat,_contentTexture,index);

		item.ClickItem += WndItemOnClick;
//		item.BeginDrag += OnBeginDragMeaterItem;
//		item.Drag += OnDragMeaterItem;
//		item.Drop += OnDropMeaterItem;
		item.ToolTip += OnToolTip;

		mVoxelMatList.Add(item);
		return index;
	}
	public int AddVoxelMatListItem( string _spriteName, string _AtlasName = "Button")
	{
		UIBuildWndItem item = AddBuildItem(mDefoultWndItem,mVoxelMatGrid.transform);
		int index = mVoxelMatList.Count;
		item.InitItem(UIBuildWndItem.ItemType.mVoxelMat,_spriteName,_AtlasName,index);

		item.ClickItem += WndItemOnClick;
//		item.BeginDrag += OnBeginDragMeaterItem;
//		item.Drag += OnDragMeaterItem;
//		item.Drop += OnDropMeaterItem;
		item.ToolTip += OnToolTip;

		mVoxelMatList.Add(item);
		return index;
	}

	public void RefreshTypeLisItem(string[] _spriteName, string _AtlasName = "Button")
	{
		if (_spriteName.Length > mTypeList.Count)
		{
			int count = _spriteName.Length - mTypeList.Count;
			for (int i = 0; i < count; i++)
			{
				UIBuildWndItem item = AddBuildItem(mDefoultWndItem, mVoxelTypeGrid.transform);
				mTypeList.Add(item);
			}
		}
		else if (_spriteName.Length < mTypeList.Count)
		{
//			int count = mTypeList.Count - _spriteName.Length;
			for (int i = mTypeList.Count - 1; i >= _spriteName.Length; i--)
			{
				mTypeList[i].transform.parent = null;
				Destroy(mTypeList[i].gameObject);
				mTypeList.RemoveAt(i);
			}
		}

		for (int i = 0; i < mTypeList.Count; i++)
		{
			mTypeList[i].InitItem(UIBuildWndItem.ItemType.mVoxelType,_spriteName[i],_AtlasName,i);

			mTypeList[i].ClickItem += WndItemOnClick;
			mTypeList[i].BeginDrag += OnBeginDragMeaterItem;
			mTypeList[i].Drag += OnDragMeaterItem;
			mTypeList[i].Drop += OnDropMeaterItem;
			mTypeList[i].ToolTip += OnToolTip;
		}

		UIGrid grid =  mVoxelTypeGrid.GetComponent<UIGrid>();
		grid.repositionNow = true;

	}
	

	public int AddIsoListItem(string _itemName, Texture _contentTexture)
	{
		UIBuildWndItem item = AddBuildItem(mIsoWndItem,mIsoGrid.transform);
		int index = mIsoList.Count;
		item.InitItem(UIBuildWndItem.ItemType.mIso,_contentTexture,index);
		item.SetText(_itemName);

		item.ClickItem += WndItemOnClick;
		item.ToolTip += OnToolTip;

		mIsoList.Add(item);
		return index;
	}

	public int AddIsoListItem( string _itemName, string _spriteName,string _AtlasName = "Button")
	{
		UIBuildWndItem item = AddBuildItem(mIsoWndItem,mIsoGrid.transform);
		int index = mIsoList.Count;
		item.InitItem(UIBuildWndItem.ItemType.mIso, _spriteName, _AtlasName, index);
		item.SetText(_itemName);

		item.ClickItem += WndItemOnClick;
		item.ToolTip += OnToolTip;

		mIsoList.Add(item);
		return index;
	}



	public int AddCostListItem(string _itemText, string _itemNumber, Texture _contentTexture)
	{
		UIBuildWndItem item = AddBuildItem(mCostItem,mCostGrid.transform);
		int index = mCostList.Count;
		item.InitItem(UIBuildWndItem.ItemType.mCost,_contentTexture,index);
		item.SetText(_itemText);
		item.SetNumber(_itemNumber);
		mCostList.Add(item);
		return index;
	}
	public int AddCostListItem(string _itemText, string _itemNumber,  string _spriteName, string _AtlasName = "Button")
	{
		UIBuildWndItem item = AddBuildItem(mCostItem,mCostGrid.transform);
		int index = mCostList.Count;
		item.InitItem(UIBuildWndItem.ItemType.mCost,_spriteName,_AtlasName,index);
		item.SetText(_itemText);
		item.SetNumber(_itemNumber);
		mCostList.Add(item);
		return index;
	}
	

	

	UIBuildWndItem AddBuildItem( GameObject _ItemPrefab , Transform _parent)
	{
		GameObject o = GameObject.Instantiate(_ItemPrefab) as GameObject;
		o.transform.parent = _parent;
		o.transform.localPosition = Vector3.zero;
		o.transform.localScale = new Vector3(1,1,1);
		o.SetActive(true);
		UIBuildWndItem item = o.GetComponent<UIBuildWndItem>();
		return item;
	}


	public void RemoveIsoItem (int index)
	{
		if (index < -1)
			return;

		Destroy( mIsoList[index].gameObject);
		mIsoList[index].transform.parent = null;
		mIsoList.RemoveAt(index);

		for (int i = 0; i < mIsoList.Count; i++)
		{
			mIsoList[i].mIndex = i;
		}
	}

	public void ClearBlockList()
	{
		for(int i=0; i<mBlockMatList.Count; i++)
		{
			mBlockMatList[i].transform.parent = null;
			Destroy(mBlockMatList[i].gameObject);
		}
		mBlockMatList.Clear();
	}

	public void ClearTextureList()
	{
		for(int i=0; i<mVoxelMatList.Count; i++)
		{
			mVoxelMatList[i].transform.parent = null;
			Destroy(mVoxelMatList[i].gameObject);
		}
		mVoxelMatList.Clear();
	}

	public void ClearIsoList()
	{
		for(int i=0; i<mIsoList.Count; i++)
		{
			mIsoList[i].transform.parent = null;
			Destroy(mIsoList[i].gameObject);
		}
		mIsoList.Clear();
	}

	public void ClearCostList()
	{
		for(int i=0; i<mCostList.Count; i++)
		{
			mCostList[i].transform.parent = null;
			Destroy(mCostList[i].gameObject);
		}
		mCostList.Clear();
	}

	public void ResetBlockPostion()
	{
		UIGrid grid =  mBlockMatGrid.GetComponent<UIGrid>();
		grid.repositionNow = true;

		grid =  mBlockPatternGrid.GetComponent<UIGrid>();
		grid.repositionNow = true;
	}
	

	public void ResetVoxelPostion()
	{
		UIGrid grid =  mVoxelMatGrid.GetComponent<UIGrid>();
		grid.repositionNow = true;
	}
	

	public void ResetIsoPostion()
	{
		UIGrid grid =  mIsoGrid.GetComponent<UIGrid>();
		grid.repositionNow = true;
	}

	public void ResetCostPostion()
	{
		UIGrid grid =  mCostGrid.GetComponent<UIGrid>();
		grid.repositionNow = true;
	}


	public void DisselectVoxel ()
	{
		foreach (UIBuildWndItem item in mVoxelMatList)
		{
			item.SetSelect(false);
		}

		mVoxelMatListSelectIndex = -1;

		string[] str = new string[0];
		RefreshTypeLisItem(str, "Icon");

		mTypeSelectIndex = -1;
	}

	public void DisselectBlock ()
	{
		foreach (UIBuildWndItem item in mBlockMatList)
		{
			item.SetSelect(false);
		}
	
		mBlockMatListSelectIndex = -1;
	}

	#endregion



	#region ui pubilc class , not use to game logic 
	public void WndItemOnClick(UIBuildWndItem.ItemType _itemType,int _index)
	{
		if(_itemType == UIBuildWndItem.ItemType.mBlockMat)
		{
			if (mBlockMatList[_index].IsActive)
			{
				if(mBlockMatListSelectIndex >-1)
					mBlockMatList[mBlockMatListSelectIndex].SetSelect(false);
				mBlockMatListSelectIndex = _index;
				mBlockMatList[mBlockMatListSelectIndex].SetSelect(true);

				if(BlockItemOnClick != null)
					BlockItemOnClick(_index);
			}
		}
		else if (_itemType == UIBuildWndItem.ItemType.mBlockPattern)
		{
			if (mBlockPatternList[_index].IsActive)
			{
				if(mBlockPatternListSelectIndex >-1)
					mBlockPatternList[mBlockPatternListSelectIndex].SetSelect(false);
				mBlockPatternListSelectIndex = _index;
				mBlockPatternList[mBlockPatternListSelectIndex].SetSelect(true);
			

				if (BlockPatternOnClick != null)
					BlockPatternOnClick(_index);
			}
		}
		else if(_itemType == UIBuildWndItem.ItemType.mVoxelMat)
		{
			if (mVoxelMatList[_index].IsActive)
			{
				if(mVoxelMatListSelectIndex >-1)
					mVoxelMatList[mVoxelMatListSelectIndex].SetSelect(false);
				mVoxelMatListSelectIndex = _index;
				mVoxelMatList[mVoxelMatListSelectIndex].SetSelect(true);

				if(TextureItemOnClick != null)
					TextureItemOnClick(_index);
			}
		}
		else if(_itemType == UIBuildWndItem.ItemType.mIso)
		{
			if (mIsoList[_index].IsActive)
			{
				if(mIsoListSelectIndex >-1)
					mIsoList[mIsoListSelectIndex].SetSelect(false);
				mIsoListSelectIndex = _index;
				mIsoList[mIsoListSelectIndex].SetSelect(true);
				
				if(IsoItemOnClick != null)
					IsoItemOnClick(_index);
			}
		}
		else if (_itemType == UIBuildWndItem.ItemType.mVoxelType)
		{
			if (mTypeList[_index].IsActive)
			{
				if(mTypeSelectIndex >-1 && mTypeSelectIndex < mTypeList.Count)
					mTypeList[mTypeSelectIndex].SetSelect(false);
				mTypeSelectIndex = _index;
				mTypeList[mTypeSelectIndex].SetSelect(true); 

				if (VoxelTypeOnClick != null)
					VoxelTypeOnClick(_index);
			}
		}
		//Debug.Log (_itemType + "," + _index);
	}

	public void ChangeCkMenu(int index)
	{
		if(index == 0)
			mCbTexture.isChecked = true;
		if(index == 1)
			mCbBlock.isChecked = true;
		if(index == 2)
			mCbSave.isChecked = true;

		ChangePage(index);
	}
	#endregion



//	public Texture _contentTexture; // text code
	#region prvate class funcs
	// Use this for initialization
	void Start () 
	{
		// -------------------------------- text code ----------------------------------
//		for (int i=0;i<45;i++)
//		{
//			AddBlockListItem(_contentTexture);
//			AddVoxelMatListItem("A","Icon");
//		}
//		for (int i=0;i<20;i++)
//		{
//			AddIsoListItem(_contentTexture);
//			AddCostListItem("item name","15","A","Icon");
//		}
		//----------------------------------------------------------------------------
	}

	void Update()
	{
		if (Pathea.PeCreature.Instance.mainPlayer == null)
			return;
		// Has player enough items ?
		Pathea.PackageCmpt pkg = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.PackageCmpt>();
		if (mCbTexture.isChecked)
		{
			foreach (UIBuildWndItem item in mVoxelMatList)
			{
				item.SetNumber(pkg.GetItemCount(item.ItemId).ToString());
			}
		}
		else if (mCbBlock.isChecked)
		{
			foreach (UIBuildWndItem item in mBlockMatList)
			{
				item.SetNumber(pkg.GetItemCount(item.ItemId).ToString());
			}
		}

	}

	void ChangePage(int index)
	{
		// open PageTexture
		if(index == 0)
		{
			mPageBlack.SetActive(false);
			mPageTexture.SetActive(true);	
			mPageSave.SetActive(false);
//			mLbTitle.text = "Voxel";

		}
		// open PageBlock 
		else if(index == 1)
		{
			mPageBlack.SetActive(true);
			mPageTexture.SetActive(false);	
			mPageSave.SetActive(false);
//			mLbTitle.text = "Block";
		}
		// open PageSave
		else if(index == 2)
		{
			mPageBlack.SetActive(false);
			mPageTexture.SetActive(false);	
			mPageSave.SetActive(true);
//			mLbTitle.text = "Building ISO";
		}
	}


	void OnBeginDragMeaterItem(UIBuildWndItem.ItemType mItemType,int mIndex)
	{
		//Debug.Log("OnBeginDragMeaterItem:" + mItemType + "," + mIndex);

		GameObject content = null;
		if(mItemType == UIBuildWndItem.ItemType.mBlockMat )
		{	
			if(mIndex >=mBlockMatList.Count && mIndex<0)
				return;
		 	if(mBlockMatList[mIndex].mContentSprite.gameObject.activeSelf)
				content = mBlockMatList[mIndex].mContentSprite.gameObject;
			else
				content = mBlockMatList[mIndex].mContentTexture.gameObject;

			mMeunControl.DragItem =  mBlockMatList[mIndex];
		}
		else if (mItemType == UIBuildWndItem.ItemType.mBlockPattern)
		{
			if(mIndex >= mBlockPatternList.Count && mIndex<0)
				return;
			if(mBlockPatternList[mIndex].mContentSprite.gameObject.activeSelf)
				content = mBlockPatternList[mIndex].mContentSprite.gameObject;
			else
				content = mBlockPatternList[mIndex].mContentTexture.gameObject;
			
			mMeunControl.DragItem =  mBlockPatternList[mIndex];
		}

		else if(mItemType == UIBuildWndItem.ItemType.mVoxelMat )
		{	
			if(mIndex >=mVoxelMatList.Count && mIndex<0)
				return;
			if(mVoxelMatList[mIndex].mContentSprite.gameObject.activeSelf)
				content = mVoxelMatList[mIndex].mContentSprite.gameObject;
			else
				content = mVoxelMatList[mIndex].mContentTexture.gameObject;

			mMeunControl.DragItem =  mVoxelMatList[mIndex];
		}
		else if(mItemType == UIBuildWndItem.ItemType.mVoxelType )
		{	
			if(mIndex >= mTypeList.Count && mIndex<0)
				return;
			if(mTypeList[mIndex].mContentSprite.gameObject.activeSelf)
				content = mTypeList[mIndex].mContentSprite.gameObject;
			else
				content = mTypeList[mIndex].mContentTexture.gameObject;
			
			mMeunControl.DragItem =  mTypeList[mIndex];
		}
			
		if(content != null)
		{
			if (mMeunControl.DragObejct != null)
			{
				Destroy(mMeunControl.DragObejct);
				mMeunControl.DragObejct = null;
			}
			mMeunControl.DragObejct = GameObject.Instantiate(content) as GameObject; 
			mMeunControl.DragObejct.transform.parent = mDragItemContent.transform;
			mMeunControl.DragObejct.transform.localScale = new Vector3(48,48,1);
			mMeunControl.DragObejct.transform.localPosition = Vector3.zero;
			Vector3 pos = Input.mousePosition;
			pos.z = mMeunControl.DragObejct.transform.position.z;
			mMeunControl.DragObejct.transform.position = pos;

		}
	}
	
	void OnDragMeaterItem(UIBuildWndItem.ItemType mItemType,int mIndex)
	{
		//Debug.Log("OnDragMeaterItem:" + mItemType + "," + mIndex);
		if(mMeunControl.DragObejct != null)
		{
			Vector3 mousePos = Input.mousePosition;
			mMeunControl.DragObejct.transform.localPosition = new Vector3(mousePos.x,mousePos.y,-15);
		}
	}

	void OnDropMeaterItem(UIBuildWndItem.ItemType mItemType,int mIndex)
	{
//		if(mMeunControl.DragObejct == null)
//			return;
//		//Debug.Log("OnDragMeaterItem:" + mItemType + "," + mIndex);
//		UIBuildWndItem item = QueryGetDragItem();
//		if(item != null)
//		{
//			UISprite sp =  mMeunControl.DragObejct.GetComponent<UISprite>();
//			if(sp != null)
//				item.GetDrag(mItemType,mIndex, DragItem.mContentSprite.spriteName,DragItem.atlas);
//			else
//				item.GetDrag(mItemType,mIndex, DragItem.mContentTexture.mainTexture);
//		}
//		GameObject.Destroy(DragObejct);
//		DragObejct = null;
	}
	#endregion


	#region btn OnClick Funcs

	private UIBuildWndItem QueryGetDragItem()
	{
		if(uiCamera == null)
			return null;
		Ray ray = uiCamera.ScreenPointToRay(Input.mousePosition);
		RaycastHit rayHit;
		int queryIndex = -1;

		for (int i=0; i<mMeunControl.mMenuList.Count; i++ )
		{
			BoxCollider bc = mMeunControl.mMenuList[i].gameObject.GetComponent<BoxCollider>();
			bool ok =bc.Raycast(ray,out rayHit,1000);
			if (ok)
			{
				queryIndex = i;
				break;
			}
		}
		 
		if(queryIndex == -1)
			return null;
		return mMeunControl.mMenuList[queryIndex];
	}

	void BtnTex_OnClick()
	{
		ChangeCkMenu(0);
		if (onPageClick != null)
			onPageClick(0);
	}

	void BtnBlock_OnClick()
	{
		ChangeCkMenu(1);
		if (onPageClick != null)
			onPageClick(1);
	}
	
	void BtnISO_OnClick()
	{
		ChangeCkMenu(2);

		if (onPageClick != null)
			onPageClick(2);
	}


	void BtnCloseOnClick()
	{
		this.Hide();

		if(BtnClose != null)
			BtnClose();
	}

	void BtnDeleteOnClick()
	{
		//Debug.Log("BtnDelete OnClick");
		if(-1 != mIsoListSelectIndex && BtnDelete != null)
			BtnDelete(mIsoListSelectIndex);
	}

	void BtnExportOnClick()
	{
		//Debug.Log("BtnExport OnClick");
		if(-1 != mIsoListSelectIndex && BtnExport != null)
			BtnExport(mIsoListSelectIndex);
	}

	void CkControl2OnClick()
	{
		//Debug.Log ("CkControl2 OnClick");
		if(CkControl2 != null)
			CkControl2();
	}

	void OnToolTip(bool isShow, UIBuildWndItem.ItemType _ItemType,int _Index)
	{
		if (ToolTip != null)
			ToolTip(isShow,_ItemType,_Index);
	}
	
	#endregion


}
