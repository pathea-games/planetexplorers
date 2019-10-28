using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIBuildMenuControl : UIStaticWnd 
{
	#region funcs delete event
	public delegate void OnClickFun();
	//public event OnClickFun BtnBrush = null;
	public event OnClickFun BtnSave = null;
	public event OnClickFun BtnDle = null;
	public event OnClickFun BtnB = null;
	public event OnClickFun BtnMenu = null;
	public event OnClickFun BtnSelectType = null;
	public event OnClickFun BtnSelectShape = null;
	public event OnClickFun BtnBlockSelect = null;
	public event OnClickFun BtnVoxelSelect = null;

	public delegate bool EventCanClick ();
	public event EventCanClick onCanClickSaveBtn;

	public delegate void EventFuncItem(int index);
	public event EventFuncItem MenuItemClick = null;
	public event EventFuncItem MenuItemGetDrag = null;
	//public event EventFuncItem MenuItemSetNull = null;

	public delegate void EventFuncCkBtn(bool isChecked);
	//public event EventFuncCkBtn CkBtnXuanQu = null;

	public delegate void ToolToipFunc(bool isShow,UIBuildWndItem.ItemType _ItemType,int _Index);
	public event ToolToipFunc ToolTip = null; 

	public delegate void EventBuildItemFunc(UIBuildWndItem item);
	public event EventBuildItemFunc onQuickBarFunc;
	public event EventBuildItemFunc onDropItem;

	public delegate void BrushItemClickEvent(UIBrushMenuItem.BrushType type);
	public event BrushItemClickEvent onBrushItemClick;

	public delegate void EventFunc();
	//public event EventFunc onQuickSwitchPointBrush;
	//public event EventFunc onQuickSwitchDiagonalBrush;
	//public event EventFunc onQuickSwitchBoxBrush;
	public event EventFunc onIntiMenuList;

	#endregion


	#region class member
	private Camera uiCamera;
	public UIGridBoxBars mBoxBars;

	public GameObject mWndMenuItem;
	public GameObject mDragItemContent;
	public GameObject mMenuItemRoot;
	
	public UIBuildSaveWndCtrl mSaveWndCtrl;
	public UIBuildWndContol mBulidWnd;
	public UIImageButton mBtnSave;

	public GameObject mPointBrushTypeList;
	public GameObject mDiagonalTypeList;
	public GameObject mBoxTypeList;
	public GameObject mSelectBrushTypeList;

	public UIMenuBrushBtn mPointBtn;
	public UIMenuBrushBtn mBoxBtn;
	public UIMenuBrushBtn mDiagonalBtn;
	public UIMenuBrushBtn mSelectBtn;
	public UIMenuBrushBtn mSelectBlockBtn;
	public UIMenuBrushBtn mSaveBtn;
	public UIMenuBrushBtn mDeleteBtn;

    public UIBuildSelectStateItem mTypeSelectedItem;
    public UIBuildSelectStateItem mShapeSelectedItem;

	public UICheckbox mBlockSelectCB;
	public UICheckbox mVoxelSelectCB;

	//private float[] mRightWndPos_x = {215,299}; 
	
	public List<UIBuildWndItem> mMenuList = new List<UIBuildWndItem>();
	#endregion


	public List<UIBuildMenuItemData> m_InitQuickBarData;

	public void ResetMenuButtonClickEvent (bool started)
	{
		UICheckbox[] check_boxs = mMenuItemRoot.GetComponentsInChildren<UICheckbox>(true);

		if (started)
		{
			foreach (UICheckbox check_box in check_boxs)
			{
				if (check_box.startsChecked)
				{
					UIButtonMessage mgs = check_box.gameObject.GetComponent<UIButtonMessage>();
					Invoke(mgs.functionName, 0.0f);
					break;
				}
			}
		}
		else
		{
			foreach (UICheckbox check_box in check_boxs)
			{
				if (check_box.isChecked)
				{
					UIButtonMessage mgs = check_box.gameObject.GetComponent<UIButtonMessage>();
					Invoke(mgs.functionName, 0.0f);

					break;
				}

			}
		}
	}

	public void ManualEnbleBtn (UIMenuBrushBtn btn)
	{
		btn.checkBox.isChecked = true;
	
		UIButtonMessage msg = btn.gameObject.GetComponent<UIButtonMessage>();
		if (msg != null)
			Invoke(msg.functionName, 0.0f);

	}
	

	#region public class funcs
	protected override void InitWindow()
	{
		base.InitWindow ();

	}


	#endregion


	#region  Unity_Inner_func
	void Awake()
	{
		if(mWndMenuItem == null)
			return;

		mBoxBars.Init(mWndMenuItem,10);
		mBoxBars.Reposition();
		mBoxBars.e_PageIndexChange += OnPageIndexChange;

		for(int i=0;i<mBoxBars.Items.Count;i++)
		{
			UIBuildWndItem item = mBoxBars.Items[i].GetComponent<UIBuildWndItem>();
			item.InitItem(UIBuildWndItem.ItemType.mMenu,i);
			int numberText = i+1;
			if(numberText == 10)
				numberText = 0;
			item.SetSpriteIndex(numberText.ToString());
			item.ClickItem += MenuItemOnClick;
			item.BeginDrag += OnBeginDragMeaterItem;
			item.Drag += OnDragMeaterItem;
			item.Drop += OnDropMeaterItem;
			item.OnGetDrag += OnGetDrag;
			item.ToolTip += OnToolTip;
			mMenuList.Add(item);
		}

		if (UIBlockSaver.Instance.First)
		{
			foreach (UIBuildMenuItemData data in m_InitQuickBarData)
			{
				UIBlockSaver.Instance.AddData(data);
				UIBlockSaver.Instance.First = false;
			}

		}
	}

	
	void OnDestory()
	{
		for(int i=0;i<mBoxBars.Items.Count;i++)
		{
			GameObject.Destroy(mBoxBars.Items[i]);
		}
		mBoxBars.Items.Clear ();
		mBoxBars.e_PageIndexChange -= OnPageIndexChange;
	}
	
	
	void OnGetDrag(UIBuildWndItem.ItemType mItemType,int mIndex)
	{
		if(mItemType == UIBuildWndItem.ItemType.mMenu && MenuItemGetDrag != null)
		{
			mIndex = mIndex  +  (mBoxBars.PageIndex - 1) * mBoxBars.ItemCount;
			MenuItemGetDrag(mIndex);
		}
	}

	bool m_Start = false;
	void Start ()
	{
		m_Start = true;
		ResetMenuButtonClickEvent(true);

		UpdateMenuItems(mBoxBars.PageIndex);
		if (onIntiMenuList != null)
			onIntiMenuList();
	}


	
	void Update () 
	{
		if (DragObejct != null)
		{
			if (Input.GetMouseButtonUp(0))
            {
                Destroy(DragObejct);
                DragObejct = null;

                if (DragItem != null)
				{
					if (DragItem.mItemType == UIBuildWndItem.ItemType.mMenu)
					{
						UIBlockSaver.Instance.RemoveData((mBoxBars.PageIndex - 1) * c_MaxMenuItems + DragItem.mIndex);
						UpdateMenuItems(mBoxBars.PageIndex);
						DragItem = null;
					}
				}
			}
		}

		// ShortCut
		if (PeInput.Get(PeInput.LogicFunction.QuickBar1))
		{
			if (onQuickBarFunc != null)
				onQuickBarFunc(mMenuList[0]);
			mMenuList[0].PlayGridEffect();
		}
		else if (PeInput.Get(PeInput.LogicFunction.QuickBar2))
		{
			if (onQuickBarFunc != null)
				onQuickBarFunc(mMenuList[1]);
			mMenuList[1].PlayGridEffect();
		}
		else if (PeInput.Get(PeInput.LogicFunction.QuickBar3))
		{
			if (onQuickBarFunc != null)
				onQuickBarFunc(mMenuList[2]);
			mMenuList[2].PlayGridEffect();
		}
		else if (PeInput.Get(PeInput.LogicFunction.QuickBar4))
		{
			if (onQuickBarFunc != null)
				onQuickBarFunc(mMenuList[3]);
			mMenuList[3].PlayGridEffect();
		}
		else if (PeInput.Get(PeInput.LogicFunction.QuickBar5))
		{
			if (onQuickBarFunc != null)
				onQuickBarFunc(mMenuList[4]);
			mMenuList[4].PlayGridEffect();
		}
		else if (PeInput.Get(PeInput.LogicFunction.QuickBar6))
		{
			if (onQuickBarFunc != null)
				onQuickBarFunc(mMenuList[5]);
			mMenuList[5].PlayGridEffect();
		}
		else if (PeInput.Get(PeInput.LogicFunction.QuickBar7))
		{
			if (onQuickBarFunc != null)
				onQuickBarFunc(mMenuList[6]);
			mMenuList[6].PlayGridEffect();
		}
		else if (PeInput.Get(PeInput.LogicFunction.QuickBar8))
		{
			if (onQuickBarFunc != null)
				onQuickBarFunc(mMenuList[7]);
			mMenuList[7].PlayGridEffect();
		}
		else if (PeInput.Get(PeInput.LogicFunction.QuickBar9))
		{
			if (onQuickBarFunc != null)
				onQuickBarFunc(mMenuList[8]);
			mMenuList[8].PlayGridEffect();
		}
		else if (PeInput.Get(PeInput.LogicFunction.QuickBar10))
		{
			if (onQuickBarFunc != null)
				onQuickBarFunc(mMenuList[9]);
			mMenuList[9].PlayGridEffect();
		}

		// Short Cut For Brush
		if (PeInput.Get(PeInput.LogicFunction.Build_Shortcut1))
		{
			if (!mPointBtn.checkBox.isChecked)
			{
				mPointBtn.checkBox.isChecked = true;
				BtnBrush1_OnClick();
			}
		}
		else if (PeInput.Get(PeInput.LogicFunction.Build_Shortcut2))
		{
			if (!mBoxBtn.checkBox.isChecked)
			{
				mBoxBtn.checkBox.isChecked = true;
				BtnBrush2_OnClick();
			}
		}
		else if (PeInput.Get(PeInput.LogicFunction.Build_Shortcut3))
		{
			if (!mDiagonalBtn.checkBox.isChecked)
			{
				UISkillWndCtrl uiSkill = GameUI.Instance.mSkillWndCtrl;
				if(uiSkill._SkillMgr != null)
				{
					if(!uiSkill._SkillMgr.CheckUnlockBuildBlockBevel())
					{
						return;
					}
				}

				mDiagonalBtn.checkBox.isChecked = true;
				BtnBrush3_OnClick();
			}
		}
		else if (PeInput.Get(PeInput.LogicFunction.Build_Shortcut4))
		{
			if (!mSelectBtn.checkBox.isChecked)
			{
				mSelectBtn.checkBox.isChecked = true;
				BtnBrush4_OnClick();
			}
		}
		else if (PeInput.Get(PeInput.LogicFunction.Build_Shortcut5))
		{
			if (!mSelectBlockBtn.checkBox.isChecked)
			{
				mSelectBlockBtn.checkBox.isChecked = true;
				BtnBrush5_OnClick();
			}
		}
		else if (PeInput.Get(PeInput.LogicFunction.Build_Shortcut6))
		{
			BtnSaveOnClick();
		}
		else if (PeInput.Get(PeInput.LogicFunction.Build_Shortcut7))
		{
			BtnMenuOnClick();
		}

		if (PeInput.Get(PeInput.LogicFunction.PrevQuickBar))
		{
			mBoxBars.BtnPageUpOnClick();
		}

		if (PeInput.Get(PeInput.LogicFunction.NextQuickBar))
		{
			mBoxBars.BtnPageDnOnClick();
		}

        //if (Input.GetKeyDown(KeyCode.Tab))
        //{
        //	if (mPointBtn.checkBox.isChecked)
        //	{
        //		if (onQuickSwitchPointBrush != null)
        //			onQuickSwitchPointBrush();
        //	}
        //	//else if (mDiagonalBtn.checkBox.isChecked)
        //	//{
        //	//	if (onQuickSwitchDiagonalBrush != null)
        //	//		onQuickSwitchDiagonalBrush();
        //	//}
        //	else if (mBoxBtn.checkBox.isChecked)
        //	{
        //		if (onQuickSwitchBoxBrush != null)
        //			onQuickSwitchBoxBrush();
        //	}
        //}

            //
            if (Pathea.PeCreature.Instance.mainPlayer != null)
		{
			Pathea.PackageCmpt pkg = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.PackageCmpt>();
			foreach (UIBuildWndItem item in mMenuList)
			{
				if (item.mTargetItemType == UIBuildWndItem.ItemType.mBlockMat)
					item.SetNumber(pkg.GetItemCount(item.ItemId).ToString());
				else if (item.mTargetItemType == UIBuildWndItem.ItemType.mVoxelType)
				{
					int proto_id = PEBuildingMan.GetVoxelItemProtoID( (byte)item.ItemId );
					item.SetNumber(pkg.GetItemCount(proto_id).ToString());
				}
				else if (item.mTargetItemType == UIBuildWndItem.ItemType.mBlockPattern)
				{
					item.SetNumber(mBulidWnd.mBlockPatternList[item.mTargetIndex].mNumber.text);
				}
				else 
				{
					item.SetNumber("");
				}
			}
		}

	}

	void LateUpdate ()
	{
		if (Input.GetMouseButtonDown(0) && !UIBrushMenuItem.MouseOnHover)
		{
			mPointBrushTypeList.gameObject.SetActive(false);
			mDiagonalTypeList.gameObject.SetActive(false);
			mBoxTypeList.gameObject.SetActive(false);
			mSelectBrushTypeList.gameObject.SetActive(false);
		}
	}
	

	//private UICheckbox _checkedBox = null;
	void OnEnable ()
	{
		if (m_Start)
		{
			ResetMenuButtonClickEvent(false);

			UpdateMenuItems(mBoxBars.PageIndex);
		}



	}

	void OnDisable ()
	{
		//_checkedBox = null;

	}
	#endregion
	

	#region Drag & Drop Event
	public GameObject DragObejct = null;
	public UIBuildWndItem DragItem = null;

	private void OnBeginDragMeaterItem(UIBuildWndItem.ItemType mItemType,int mIndex)
	{
		GameObject content = null;
	
	    if(mItemType == UIBuildWndItem.ItemType.mMenu )
		{	
			if(mIndex >=mMenuList.Count && mIndex<0)
				return;
			if(mMenuList[mIndex].mContentSprite.gameObject.activeSelf)
				content =mMenuList[mIndex].mContentSprite.gameObject;
			else
				content = mMenuList[mIndex].mContentTexture.gameObject;
			
			DragItem =  mMenuList[mIndex];
		}

		if(content != null)
		{
			DragObejct = GameObject.Instantiate(content) as GameObject; 
			DragObejct.transform.parent = mDragItemContent.transform;
			DragObejct.transform.localScale = new Vector3(48,48,1);
			DragObejct.transform.position = Input.mousePosition;
			
		}
	}
	
	private void OnDragMeaterItem(UIBuildWndItem.ItemType mItemType,int mIndex)
	{
		if(DragObejct != null)
		{
			Vector3 mousePos = Input.mousePosition;
			DragObejct.transform.localPosition = new Vector3(mousePos.x,mousePos.y,-15);
		}
	}
	
	private void OnDropMeaterItem(UIBuildWndItem.ItemType mItemType,int mIndex)
	{
		if(DragObejct == null)
			return;

		UIBuildWndItem item = QueryGetDragItem();;

		if (item != null)
		{
			UISprite sp =  DragObejct.GetComponent<UISprite>();
			if(sp != null)
				item.GetDrag(DragItem.mTargetItemType, DragItem.mIndex, sp.spriteName,DragItem.atlas);
			else
				item.GetDrag(DragItem.mTargetItemType, DragItem.mTargetIndex, DragItem.mContentTexture.mainTexture);

			if (DragItem.mItemType == UIBuildWndItem.ItemType.mMenu)
			{
				item.mTargetItemType = DragItem.mTargetItemType;
				item.SetItemID(DragItem.ItemId);
				item.mTargetIndex = DragItem.mTargetIndex;
			}
			else
			{
				item.mTargetItemType = DragItem.mItemType;
				item.SetItemID(DragItem.ItemId);
				item.mTargetIndex = DragItem.mIndex;
			}

			UIBlockSaver.Instance.SetData((mBoxBars.PageIndex - 1) * c_MaxMenuItems + item.mIndex, item);
		}

		if(DragItem.mItemType == UIBuildWndItem.ItemType.mMenu && item != DragItem)
		{
			DragItem.SetNullContent();

			UIBlockSaver.Instance.RemoveData((mBoxBars.PageIndex - 1) * c_MaxMenuItems + DragItem.mIndex);
		}

		if (onDropItem != null)
			onDropItem(item);

	}

	private UIBuildWndItem QueryGetDragItem()
	{
		if(uiCamera == null)
			uiCamera = GameUI.Instance.mUICamera;

		Ray ray = uiCamera.ScreenPointToRay(Input.mousePosition);
		RaycastHit rayHit;
		int queryIndex = -1;
		
		for (int i=0; i<mMenuList.Count; i++ )
		{
			BoxCollider bc = mMenuList[i].gameObject.GetComponent<BoxCollider>();
			bool ok =bc.Raycast(ray,out rayHit,1000);
			if (ok)
			{
				queryIndex = i;
				break;
			}
		}
		
		if(queryIndex == -1)
			return null;
		return mMenuList[queryIndex];
	}

	#endregion

	#region Page_Event & Func

	const int c_MaxMenuItems = 10;
	void OnPageIndexChange (int page_index)
	{
		if (page_index < 0)
			return;

		UpdateMenuItems(page_index);
	}
	
	void UpdateMenuItems (int page_index)
	{
		List<UIBuildMenuItemData> datas = UIBlockSaver.Instance.GetPageItemDatas(page_index - 1, c_MaxMenuItems);

		foreach (UIBuildWndItem item in mMenuList)
		{
			int index = datas.FindIndex( item0 => item.mIndex == item0.m_Index);
			if (index == -1)
				item.SetNullContent();
			else
			{
				item.mTargetItemType = (UIBuildWndItem.ItemType) datas[index].m_Type;
				item.mTargetIndex = datas[index].m_TargetIndex;
				item.InitItem(UIBuildWndItem.ItemType.mMenu, datas[index].m_IconName, "Icon", datas[index].m_Index);
				item.SetItemID(datas[index].m_ItemId);
				item.mSubsetIndex = datas[index].m_SubsetIndex;
			}
		}
	}
    #endregion

    #region OnClick func

    public event System.Action onMenuBtn;
    bool _firstClickMenuBtn = true;
 	void BtnMenuOnClick()
	{
		if (BtnMenu != null)
			BtnMenu();

		mBulidWnd.Show();
        mSaveWndCtrl.Hide();
        if (_firstClickMenuBtn)
        {
            mBulidWnd.ChangeCkMenu(0);
            _firstClickMenuBtn = false;
        }
        if (onMenuBtn != null)
            onMenuBtn.Invoke();
    }

	private void MenuItemOnClick(UIBuildWndItem.ItemType _itemType,int index)
	{
		//index = (mBoxBars.PageIndex) * mBoxBars.ItemCount + index;
		if(_itemType == UIBuildWndItem.ItemType.mMenu && MenuItemClick != null)
			MenuItemClick(index);
	
	}

	
	private void BtnBrush1_OnClick()
	{
		if (UIBuildBlock.Instance == null)
			return;
		UIBuildBlock.Instance.CreateBrush( UIBuildBlock.BrushType.bt_point);
		mSaveWndCtrl.Hide();
	}

	private void BtnBrush2_OnClick()
	{
		UIBuildBlock.Instance.CreateBrush( UIBuildBlock.BrushType.bt_box);
		mSaveWndCtrl.Hide();
	}

	private void BtnBrush3_OnClick()
	{
		UISkillWndCtrl uiSkill = GameUI.Instance.mSkillWndCtrl;
		if(uiSkill._SkillMgr != null)
		{
			if(!uiSkill._SkillMgr.CheckUnlockBuildBlockBevel())
			{
				mDiagonalBtn.checkBox.isChecked = false;
				return;
			}
		}
		
		mDiagonalBtn.checkBox.isChecked = true;
		UIBuildBlock.Instance.CreateBrush( UIBuildBlock.BrushType.bt_inclined);
		mSaveWndCtrl.Hide();
	}

	private void BtnBrush4_OnClick()
	{
//		if (_selectBrushType == 1)
//			UIBuildBlock.Instance.CreateBrush( UIBuildBlock.BrushType.bt_selectBlock);
//		else if (_selectBrushType == 0)
			UIBuildBlock.Instance.CreateBrush( UIBuildBlock.BrushType.bt_selectAll);
		
		mSaveWndCtrl.Hide();
	}
//	private void BtnBrush5_OnClick()
//	{
//		UIBuildBlock.Instance.CreateBrush( UIBuildBlock.BrushType.bt_selectVoxel);
//		mSaveWndCtrl.Hide();
//	}

	private void BtnBrush5_OnClick()
	{
		UIBuildBlock.Instance.CreateBrush( UIBuildBlock.BrushType.bt_selectBlock);
		mSaveWndCtrl.Hide();
	}
	
	private void BtnBrush6_OnClick()
	{
		UIBuildBlock.Instance.CreateBrush( UIBuildBlock.BrushType.bt_selectInclined);
		mSaveWndCtrl.Hide();
	}


	private void BtnSaveOnClick()
	{
		if (onCanClickSaveBtn != null )
		{
			if (onCanClickSaveBtn())
			{
				mSaveWndCtrl.Show();
				mBulidWnd.Hide();
			}
		}
		else
		{
			mSaveWndCtrl.Show();
			mBulidWnd.Hide();
		}

		if(BtnSave != null)
			BtnSave();
	}

	private void BtnDelOnClick()
	{
		if (BtnDle != null)
			BtnDle();
	}


	private void BtnBOnClick()
	{
		if(BtnB != null)
			BtnB();
	}

	private void OnSelectTypeItemClick()
	{
		if (BtnSelectType != null)
			BtnSelectType();
	}

	private void OnSelectShapeItemClick()
	{
		if (BtnSelectShape != null)
			BtnSelectShape();
	}

	//int _selectBrushType = 0; // 0 select all . 1 select detail
	public void SetBrushItemSprite (UIBrushMenuItem.BrushType type, Color checkColor)
	{
		if (type == UIBrushMenuItem.BrushType.pointAdd)
		{
			mPointBtn.bgSprite.spriteName = "build_point";
			mPointBtn.checkedSprite.spriteName = "build_point";
			mPointBtn.checkedSprite.color = checkColor;
			
		}
		else if (type == UIBrushMenuItem.BrushType.pointRemove)
		{
			mPointBtn.bgSprite.spriteName = "build_point_down";
			mPointBtn.checkedSprite.spriteName = "build_point_down";
			mPointBtn.checkedSprite.color = checkColor;
		}
		else if (type == UIBrushMenuItem.BrushType.boxAdd)
		{
			mBoxBtn.bgSprite.spriteName = "build_area";
			mBoxBtn.checkedSprite.spriteName = "build_area";
			mBoxBtn.checkedSprite.color = checkColor;
		}
		else if (type == UIBrushMenuItem.BrushType.boxRemove)
		{
			mBoxBtn.bgSprite.spriteName = "build_area_down";
			mBoxBtn.checkedSprite.spriteName = "build_area_down";
			mBoxBtn.checkedSprite.color = checkColor;
		}
		else if (type == UIBrushMenuItem.BrushType.diagonalXPos)
		{
			mDiagonalBtn.bgSprite.spriteName = "build_gjxie1";
			mDiagonalBtn.checkedSprite.spriteName = "build_gjxie1";
			mDiagonalBtn.checkedSprite.color = checkColor;
		}
		else if (type == UIBrushMenuItem.BrushType.diagonalXNeg)
		{
			mDiagonalBtn.bgSprite.spriteName = "build_gjxie";
			mDiagonalBtn.checkedSprite.spriteName = "build_gjxie";
			mDiagonalBtn.checkedSprite.color = checkColor;
		}
		else if (type == UIBrushMenuItem.BrushType.diagonalZPos)
		{
			mDiagonalBtn.bgSprite.spriteName = "build_gjxie2";
			mDiagonalBtn.checkedSprite.spriteName = "build_gjxie2";
			mDiagonalBtn.checkedSprite.color = checkColor;
		}
		else if (type == UIBrushMenuItem.BrushType.diagonalZNeg)
		{
			mDiagonalBtn.bgSprite.spriteName = "build_gjxie3";
			mDiagonalBtn.checkedSprite.spriteName = "build_gjxie3";
			mDiagonalBtn.checkedSprite.color = checkColor;
		}
		else if (type == UIBrushMenuItem.BrushType.SelectAll)
		{
			mSelectBtn.bgSprite.spriteName = "build_vx_all";
			mSelectBtn.checkedSprite.spriteName = "build_vx_all";
			mSelectBtn.checkBox.isChecked = true;
			mSelectBtn.checkedSprite.color = checkColor;
			//_selectBrushType = 0;
			BtnBrush4_OnClick();

		}
		else if (type == UIBrushMenuItem.BrushType.SelectDetail)
		{
			mSelectBtn.bgSprite.spriteName = "build_vx_point";
			mSelectBtn.checkedSprite.spriteName = "build_vx_point";
			mSelectBtn.checkBox.isChecked = true;
			mSelectBtn.checkedSprite.color = checkColor;
			//_selectBrushType = 1;
			BtnBrush4_OnClick();
		}

	}
	
	public void OnBrushItemClick (UIBrushMenuItem.BrushType type)
	{

		if (onBrushItemClick != null)
			onBrushItemClick(type);
	}

	void OnToolTip(bool isShow, UIBuildWndItem.ItemType _ItemType,int _Index)
	{
		if (ToolTip != null)
			ToolTip(isShow,_ItemType,_Index);
	}

	void OnVoxelSelect()
	{
		if (BtnVoxelSelect != null)
			BtnVoxelSelect();
	}

	void OnBlockSelect()
	{
		if (BtnBlockSelect != null)
			BtnBlockSelect();
	}
	#endregion
}
