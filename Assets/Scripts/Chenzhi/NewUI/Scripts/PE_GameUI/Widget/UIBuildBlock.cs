using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using Pathea;

public class UIBuildBlock : MonoBehaviour
{
    public System.Action onSaveIsoClick;
    public System.Action<BSIsoHeadData> onIsoExport;

    //temp justfor test
    static UIBuildBlock mInstance;
    public static UIBuildBlock Instance { get { return mInstance; } }

    public UIBuildMenuControl mMenuCtrl;
    public UIBuildWndContol mWndCtrl;
    public UIBuildSaveWndCtrl mSaveWnd;
    //lz-2016.11.02 建造的引导控制
    public UIBuildTutorialCtrl mBuildTutorialCtrl;

    public GameObject mTipsRoot;
    public UISprite mTipsNormSprite;
    public UISprite mTipsShowSprite;

	public Color menuItemAddColor = Color.green;
	public Color menuItemRemoveColor = Color.red;

    //int mCurrentBrushID = 1;
    //int mCurrentItemId = 30200001;
    //int mMatPage = 0;
    //int mBrushPage = 0;
    const int NumPerPage = 22;

    EBSBrushMode pointMode = EBSBrushMode.Add;
    EBSBrushMode boxMode = EBSBrushMode.Add;
    int diagonalRot = 0;

    #region UNITY_INNER_FUNC

    void Awake()
    {
        mInstance = this;
        mMenuCtrl.BtnB += BtnBOnClick;
        mMenuCtrl.BtnDle += DeleteOnClick;
		mMenuCtrl.BtnSelectShape += SelectShapeOnClick;
		mMenuCtrl.BtnSelectType += SelectTypeOnClick;
        mMenuCtrl.onCanClickSaveBtn += OnCanClickSaveBtn;
        mMenuCtrl.onQuickBarFunc += OnMenuQuickBarClick;
        mMenuCtrl.onBrushItemClick += OnBrushMenuItemClick;
        mMenuCtrl.MenuItemClick += OnMenuItemClick;
		mMenuCtrl.BtnBlockSelect += OnMenuBlockSelectClick;
		mMenuCtrl.BtnVoxelSelect += OnMenuVoxelSelectClick;
        //mMenuCtrl.onQuickSwitchPointBrush += OnMenuQuickSwitchPointBrush;
        //mMenuCtrl.onQuickSwitchDiagonalBrush += OnMenuQuickSwitchDiagonalBrush;
        //mMenuCtrl.onQuickSwitchBoxBrush += OnMenuQuickSwitchBoxBrush;
        mMenuCtrl.ToolTip += OnItemToolTip;
        mMenuCtrl.onIntiMenuList += OnInitMenuList;
        mMenuCtrl.onDropItem += OnMenuDropItem;
        mWndCtrl.onPageClick += OnPageClick;
        mWndCtrl.BtnDelete += OnIsoDeleteClick;
        mWndCtrl.BtnExport += OnIsoExportClick;
        mSaveWnd.btnSave += OnSaveIsoClick;
        mSaveWnd.OnWndClosed += OnSaveIsoClose;

        InitVoxelPage();
        InitBlockPage();
        InitBuildTips();

        if (GameUI.Instance.mSkillWndCtrl != null)
        {
            GameUI.Instance.mSkillWndCtrl.onRefreshTypeData += OnRefreshSkill;
            OnRefreshSkill(GameUI.Instance.mSkillWndCtrl);
        }


    }

    void OnDestroy()
    {
        if (GameUI.Instance.mSkillWndCtrl != null)
        {
            GameUI.Instance.mSkillWndCtrl.onRefreshTypeData -= OnRefreshSkill;
        }
    }

    public bool playTween = false;
    bool forward = true;

    void Update()
    {
        InitVoxelPage();
        InitBlockPage();

        if (playTween)
        {
            PlayTween(forward);
            forward = !forward;
            playTween = false;
        }


//        if (mWndCtrl.BlockPatternSelectIndex != -1)
//        {
//            mMenuCtrl.mShapeSelectedItem.SetIcon(mWndCtrl.mBlockPatternList[mWndCtrl.BlockPatternSelectIndex].mContentSprite.spriteName);
//        }
//        else
//            mMenuCtrl.mShapeSelectedItem.SetIcon("Null");

        if (mWndCtrl.BlockMatSelectIndex != -1)
        {
            mMenuCtrl.mTypeSelectedItem.SetIcon(mWndCtrl.mBlockMatList[mWndCtrl.BlockMatSelectIndex].mContentSprite.spriteName);
			if (mWndCtrl.BlockPatternSelectIndex != -1)
			{
				mMenuCtrl.mShapeSelectedItem.SetIcon(mWndCtrl.mBlockPatternList[mWndCtrl.BlockPatternSelectIndex].mContentSprite.spriteName);
			}

//			mMenuCtrl.mSelectBtn.gameObject.SetActive(true);
//			mMenuCtrl.mSelectBlockBtn.gameObject.SetActive(true);
			mMenuCtrl.mSelectBtn.boxCollier.enabled = true;
			mMenuCtrl.mSelectBlockBtn.boxCollier.enabled = true;
			mMenuCtrl.mDiagonalBtn.boxCollier.enabled = true;
			mMenuCtrl.mSaveBtn.boxCollier.enabled = true;
			mMenuCtrl.mDeleteBtn.boxCollier.enabled = true;
			mMenuCtrl.mSelectBtn.bgSprite.color = Color.white;
			mMenuCtrl.mSelectBlockBtn.bgSprite.color = Color.white;
			mMenuCtrl.mDiagonalBtn.bgSprite.color = Color.white;
			mMenuCtrl.mSaveBtn.bgSprite.color = Color.white;
			mMenuCtrl.mDeleteBtn.bgSprite.color = Color.white;

        }
		else if (mWndCtrl.TextureListSelectIndex != -1)
        {
            mMenuCtrl.mTypeSelectedItem.SetIcon(mWndCtrl.mTypeList[mWndCtrl.TypeSelectIndex].mContentSprite.spriteName);

			mMenuCtrl.mShapeSelectedItem.SetIcon("voxel");

			//mMenuCtrl.mSelectBtn.gameObject.SetActive(false);
			//mMenuCtrl.mSelectBlockBtn.gameObject.SetActive(false);

			mMenuCtrl.mSelectBtn.boxCollier.enabled = false;
			mMenuCtrl.mSelectBlockBtn.boxCollier.enabled = false;
			mMenuCtrl.mDiagonalBtn.boxCollier.enabled = false;
			mMenuCtrl.mSaveBtn.boxCollier.enabled = false;
			mMenuCtrl.mDeleteBtn.boxCollier.enabled = false;
			mMenuCtrl.mSelectBtn.bgSprite.color = Color.white * 0.6f;
			mMenuCtrl.mSelectBlockBtn.bgSprite.color = Color.white *0.6f;
			mMenuCtrl.mDiagonalBtn.bgSprite.color = Color.white * 0.6f;
			mMenuCtrl.mSaveBtn.bgSprite.color = Color.white * 0.6f;
			mMenuCtrl.mDeleteBtn.bgSprite.color = Color.white * 0.6f;
        }
        else
		{
			mMenuCtrl.mShapeSelectedItem.SetIcon("Null");
            mMenuCtrl.mTypeSelectedItem.SetIcon("Null");
		}
        
        if(mMenuCtrl.mPointBtn.checkBox.isChecked )
        {
            if (m_CurBrush is BSPointBrush)
            {
                if (m_CurBrush.mode == EBSBrushMode.Add)
					mMenuCtrl.SetBrushItemSprite(UIBrushMenuItem.BrushType.pointAdd, menuItemAddColor);
                else if (m_CurBrush.mode == EBSBrushMode.Subtract)
					mMenuCtrl.SetBrushItemSprite(UIBrushMenuItem.BrushType.pointRemove, menuItemRemoveColor);
            }
        }
        else if (mMenuCtrl.mBoxBtn.checkBox.isChecked)
        {
            if (m_CurBrush is BSBoxBrush)
            {
                if (m_CurBrush.mode == EBSBrushMode.Add)
					mMenuCtrl.SetBrushItemSprite(UIBrushMenuItem.BrushType.boxAdd, menuItemAddColor);
                else if (m_CurBrush.mode == EBSBrushMode.Subtract)
					mMenuCtrl.SetBrushItemSprite(UIBrushMenuItem.BrushType.boxRemove, menuItemRemoveColor);
            }
        }

		if (mWndCtrl.BlockMatSelectIndex != -1)
		{
			if (!mMenuCtrl.mBlockSelectCB.isChecked)
				mMenuCtrl.mBlockSelectCB.isChecked = true;
		}
		else
		{
			if (!mMenuCtrl.mVoxelSelectCB.isChecked)
				mMenuCtrl.mVoxelSelectCB.isChecked = true;
		}
    }

    void LateUpdate()
    {
        if (IsoRePos)
        {
            IsoRePos = false;
            mWndCtrl.ResetCostPostion();
        }
    }

    #endregion

    public void EnterBuildMode()
    {
        if (null != Pathea.PeCreature.Instance && null != Pathea.PeCreature.Instance.mainPlayer)
        {
            Pathea.MotionMgrCmpt mmc = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.MotionMgrCmpt>();
            if (null != mmc)
                mmc.DoAction(Pathea.PEActionType.Build);
        }

    }

    public void QuitBuildMode()
    {
        if (null != Pathea.PeCreature.Instance && null != Pathea.PeCreature.Instance.mainPlayer)
        {
            Pathea.MotionMgrCmpt mmc = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.MotionMgrCmpt>();
            if (null != mmc)
                mmc.EndAction(Pathea.PEActionType.Build);
        }
        else
            QuitBlock();

    }

    public void EnterBlock()
    {
        gameObject.SetActive(true);
        PeInput.ArrowAxisEnable = false;
        if (Key_wordMgr.Self != null)
            Key_wordMgr.Self.enableQuickKey = false;
    }

    public void QuitBlock()
    {
        PeInput.ArrowAxisEnable = true;
        gameObject.SetActive(false);
        CreateBrush(BrushType.bt_null);

        if (Key_wordMgr.Self != null)
            Key_wordMgr.Self.enableQuickKey = true;
    }


    #region SELF_FUNC
    public enum BrushType
    {
        bt_point,
        bt_box,
        bt_inclined,
        bt_selectBlock,
        bt_selectVoxel,
        bt_selectInclined,
        bt_iso,
		bt_selectAll,
        bt_null
    }

    private BrushType m_CurBrushType;
    private BSBrush m_CurBrush;

    public void CreateBrush(BrushType type)
    {
        if (PEBuildingMan.Self == null)
            return;

        m_CurBrushType = type;
        if (type == BrushType.bt_null)
            m_CurBrush = BuildingMan.CreateBrush(BuildingMan.EBrushType.None);
        else if (type == BrushType.bt_box)
        {
            m_CurBrush = BuildingMan.CreateBrush(BuildingMan.EBrushType.Box);
            m_CurBrush.mode = boxMode;
        }
        else if (type == BrushType.bt_inclined)
        {
            m_CurBrush = BuildingMan.CreateBrush(BuildingMan.EBrushType.B45Diagonal);
            BSB45DiagonalBrush bd = m_CurBrush as BSB45DiagonalBrush;
            if (bd != null)
            {
                bd.m_Rot = diagonalRot;
            }
        }
        else if (type == BrushType.bt_point)
        {
            m_CurBrush = BuildingMan.CreateBrush(BuildingMan.EBrushType.Point);
            m_CurBrush.mode = pointMode;
        }
        else if (type == BrushType.bt_selectBlock)
        {
            PEBuildingMan.Self.selectVoxel = false;
            m_CurBrush = BuildingMan.CreateBrush(BuildingMan.EBrushType.Select);
        }
        else if (type == BrushType.bt_selectVoxel)
        {
            PEBuildingMan.Self.selectVoxel = true;
            m_CurBrush = BuildingMan.CreateBrush(BuildingMan.EBrushType.Select);
        }
        else if (type == BrushType.bt_selectInclined)
            m_CurBrush = BuildingMan.CreateBrush(BuildingMan.EBrushType.None);
		else if (type == BrushType.bt_selectAll)
		{
			PEBuildingMan.Self.selectVoxel = false;
			m_CurBrush = BuildingMan.CreateBrush(BuildingMan.EBrushType.IsoSelectBrush);
		}
        else if (type == BrushType.bt_iso)
            m_CurBrush = BuildingMan.CreateBrush(BuildingMan.EBrushType.Iso);


    }
    #endregion


    #region INIT_VOXEL & BLOCK
    bool _initVoxelPage = false;

    List<int> m_VoxelProtoItems = null;
    List<int> m_VoxelTypeList = null;
    List<int> m_BlockProtoItems = null;

    void InitVoxelPage()
    {
        if (!_initVoxelPage)
        {
            UISkillWndCtrl uiSkill = GameUI.Instance.mSkillWndCtrl;
            if (uiSkill._SkillMgr != null)
            {
                if (!uiSkill._SkillMgr.CheckUnlockBuildBlockVoxel())
                    return;
            }
        
            m_VoxelProtoItems = BSVoxelMatMap.GetAllProtoItems();
            foreach (int id in m_VoxelProtoItems)
            {
                ItemProto proto = ItemProto.Mgr.Instance.Get(id);
                int index = mWndCtrl.AddVoxelMatListItem(proto.icon[0], "Icon");
                mWndCtrl.mVoxelMatList[index].SetItemID(proto.id);
            }

            mWndCtrl.ToolTip += OnItemToolTip;
            mWndCtrl.TextureItemOnClick += OnVoxelMatItemClick;
            mWndCtrl.VoxelTypeOnClick += OnVoxelTypeClick;

            mWndCtrl.ResetVoxelPostion();
            _initVoxelPage = true;
        }


    }

    bool _initBlockPage = false;
    void InitBlockPage()
    {
        if (!_initBlockPage)
        {
            m_BlockProtoItems = BSBlockMatMap.GetAllProtoItems();

            foreach (int id in m_BlockProtoItems)
            {
                ItemProto proto = ItemProto.Mgr.Instance.Get(id);
                int index = mWndCtrl.AddBlockListItem(proto.icon[0], "Icon");
                mWndCtrl.mBlockMatList[index].SetItemID(id);

            }

            // Pattern type
            foreach (var kv in BSPattern.s_tblPatterns)
            {
                int index = mWndCtrl.AddBlockPatternItem(kv.Value.IconName, "Icon");
                mWndCtrl.mBlockPatternList[index].SetNumber(kv.Value.size.ToString());
                mWndCtrl.mBlockPatternList[index].SetItemID(kv.Key);
                if (kv.Value == PEBuildingMan.Self.Pattern)
                    mWndCtrl.BlockPatternSelectIndex = index;

            }



            mWndCtrl.ResetBlockPostion();

            mWndCtrl.ToolTip += OnItemToolTip;
            mWndCtrl.BlockItemOnClick += OnBlockMatClick;
            mWndCtrl.BlockPatternOnClick += OnBlockPatternClick;

            _initBlockPage = true;
        }
    }

    bool _initIosBlockPage = false;
    List<BSIsoHeadData> m_IsoHeaders = null;
    void InitIsoBlockPage()
    {
        UISkillWndCtrl uiSkill = GameUI.Instance.mSkillWndCtrl;
        if (uiSkill._SkillMgr != null)
        {
            if (!uiSkill._SkillMgr.CheckUnlockBuildBlockIso())
                return;
        }
        if (!_initIosBlockPage)
        {
            mWndCtrl.ClearIsoList();
            mWndCtrl.ClearCostList();

            m_IsoHeaders = new List<BSIsoHeadData>(PEBuildingMan.Self.ExtractTheHeaders());

            foreach (BSIsoHeadData header in m_IsoHeaders)
            {
                Texture2D tex = new Texture2D(32, 32, TextureFormat.ARGB32, false);
                tex.LoadImage(header.IconTex);

                mWndCtrl.AddIsoListItem(header.Name, tex);

            }

            mWndCtrl.IsoItemOnClick += OnIsoClick;

            mWndCtrl.ResetIsoPostion();
            _initIosBlockPage = true;
        }
    }

    #endregion

    #region Tween

    public WhiteCat.TweenInterpolator tipTweener;
    public WhiteCat.TweenInterpolator menuTweener;

    public delegate void DTweenFinished(bool forward);
    public event DTweenFinished onTweenFinished;

    public void PlayTween(bool forward)
    {
        if (forward)
        {
            if (tipTweener.speed < 0)
                tipTweener.ReverseSpeed();

            if (menuTweener.speed < 0)
                menuTweener.ReverseSpeed();
        }
        else
        {
            if (tipTweener.speed > 0)
                tipTweener.ReverseSpeed();

            if (menuTweener.speed > 0)
                menuTweener.ReverseSpeed();
        }

        tipTweener.isPlaying = true;
        menuTweener.isPlaying = true;

    }

    public void OnTweenFinish(bool forward)
    {
        if (onTweenFinished != null)
            onTweenFinished(true);
    }

    #endregion


    #region MENU_EVENT
    void BtnBOnClick()
    {
        QuitBuildMode();
    }

    void DeleteOnClick()
    {
        BSMiscBrush select_brush = m_CurBrush as BSMiscBrush;
        if (select_brush != null)
        {
            if (!select_brush.IsEmpty())
            {
                select_brush.DeleteVoxel();
            }
        }
		else
		{
			BSIsoSelectBrush iso_select_brush = m_CurBrush as BSIsoSelectBrush;
			if (iso_select_brush != null)
				iso_select_brush.DeleteVoxels();
		}
    }

	void SelectTypeOnClick()
	{
		if (mWndCtrl.BlockMatSelectIndex != -1)
		{
			mWndCtrl.Show();
			mWndCtrl.ChangeCkMenu(1);
		}
		else if (mWndCtrl.TypeSelectIndex != -1)
		{
			mWndCtrl.Show();
			mWndCtrl.ChangeCkMenu(0);
		}

	}

	void SelectShapeOnClick()
	{
		if (mWndCtrl.BlockMatSelectIndex != -1 && mWndCtrl.BlockPatternSelectIndex != -1)
		{
			mWndCtrl.Show();
			mWndCtrl.ChangeCkMenu(1);
		}
		else if (mWndCtrl.TypeSelectIndex != -1)
		{
			mWndCtrl.Show();
			mWndCtrl.ChangeCkMenu(0);
		}
	}

    bool OnCanClickSaveBtn()
    {
        if (PEBuildingMan.Self == null)
            return true;


        if (!PEBuildingMan.Self.selectVoxel)
        {
			BSSelectBrush select_brush = m_CurBrush as BSSelectBrush;

			if (select_brush != null)
			{
            	if (!select_brush.IsEmpty())
            	{
            	    PEBuildingMan.Self.IsoCaputure.Computer.ClearDataDS();
            	    foreach (var kvp in select_brush.Selections)
            	    {
            	        BSVoxel voxel = BuildingMan.Blocks.Read(kvp.Key.x, kvp.Key.y, kvp.Key.z);
            	        PEBuildingMan.Self.IsoCaputure.Computer.AlterBlockInBuild(kvp.Key.x, kvp.Key.y, kvp.Key.z, voxel.ToBlock());
            	    }
				
            	    PEBuildingMan.Self.IsoCaputure.Computer.RebuildMesh();
            	    PEBuildingMan.Self.IsoCaputure.EnableCapture();
				
            	    mSaveWnd.SetIsoItemContent(PEBuildingMan.Self.IsoCaputure.photoRT);
            	    select_brush.canDo = false;
					return true;
            	}

			}

			BSIsoSelectBrush iso_select = m_CurBrush as BSIsoSelectBrush;
			//BSIsoSelectBrush iso_select = PEBuildingMan.Self.Manipulator.activeBrush as BSIsoSelectBrush;
			if (iso_select != null)
			{
				List<IntVector3> voxels = iso_select.GetSelectionPos();
				if (voxels.Count != 0)
				{
					PEBuildingMan.Self.IsoCaputure.Computer.ClearDataDS();
					for (int i = 0; i < voxels.Count; i++)
					{
						BSVoxel voxel = BuildingMan.Blocks.Read(voxels[i].x, voxels[i].y, voxels[i].z);
						PEBuildingMan.Self.IsoCaputure.Computer.AlterBlockInBuild(voxels[i].x, voxels[i].y, voxels[i].z, voxel.ToBlock());
					}

					PEBuildingMan.Self.IsoCaputure.Computer.RebuildMesh();
					PEBuildingMan.Self.IsoCaputure.EnableCapture();

					mSaveWnd.SetIsoItemContent(PEBuildingMan.Self.IsoCaputure.photoRT);
					return true;
				}
			}
			
        }

        return false;
    }

    // On Skill Refresh
    void OnRefreshSkill(UISkillWndCtrl uiSkill)
    {
        if (!RandomMapConfig.useSkillTree)
            return;

        // Block Material
        foreach (UIBuildWndItem item in mWndCtrl.mBlockMatList)
        {

            ItemProto proto = ItemProto.Mgr.Instance.Get(item.ItemId);

            if (uiSkill._SkillMgr != null)
            {

                if (!uiSkill._SkillMgr.CheckBuildBlockLevel(proto.level))
                {
                    item.IsActive = false;
                }
                else
                    item.IsActive = true;
            }
        }

        // Block Pattern
        foreach (UIBuildWndItem item in mWndCtrl.mBlockPatternList)
        {
            int index = item.mIndex;

            if (uiSkill._SkillMgr != null)
            {
                if (!uiSkill._SkillMgr.CheckBuildShape(index))
                {
                    item.IsActive = false;
                }
                else
                    item.IsActive = true;
            }
        }

        // Menu List
        foreach (UIBuildWndItem item in mMenuCtrl.mMenuList)
        {
            if (item.mTargetItemType == UIBuildWndItem.ItemType.mBlockMat)
            {
                ItemAsset.ItemProto proto = ItemAsset.ItemProto.GetItemData(item.ItemId);

                if (uiSkill._SkillMgr != null)
                {
                    if (!uiSkill._SkillMgr.CheckBuildBlockLevel(proto.level))
                    {
                        item.IsActive = false;
                    }
                    else
                        item.IsActive = true;
                }
            }
            else if (item.mTargetItemType == UIBuildWndItem.ItemType.mBlockPattern)
            {
                int index = item.mTargetIndex;

                if (uiSkill._SkillMgr != null)
                {
                    if (!uiSkill._SkillMgr.CheckBuildShape(index))
                    {
                        item.IsActive = false;
                    }
                    else
                        item.IsActive = true;
                }
            }
        }
    }


    void OnInitMenuList()
    {
        // Menu List
        UISkillWndCtrl uiSkill = GameUI.Instance.mSkillWndCtrl;

        if (uiSkill == null)
            return;

        foreach (UIBuildWndItem item in mMenuCtrl.mMenuList)
        {
            if (item.mTargetItemType == UIBuildWndItem.ItemType.mBlockMat)
            {
                ItemAsset.ItemProto proto = ItemAsset.ItemProto.GetItemData(item.ItemId);

                if (uiSkill._SkillMgr != null)
                {
                    if (!uiSkill._SkillMgr.CheckBuildBlockLevel(proto.level))
                    {
                        item.IsActive = false;
                    }
                    else
                        item.IsActive = true;
                }
            }
            else if (item.mTargetItemType == UIBuildWndItem.ItemType.mBlockPattern)
            {
                int index = item.mTargetIndex;

                if (uiSkill._SkillMgr != null)
                {
                    if (!uiSkill._SkillMgr.CheckBuildShape(index))
                    {
                        item.IsActive = false;
                    }
                    else
                        item.IsActive = true;
                }
            }
        }
    }

    void OnMenuDropItem(UIBuildWndItem item)
    {
        // Menu List
        UISkillWndCtrl uiSkill = GameUI.Instance.mSkillWndCtrl;

        if (uiSkill == null)
            return;

        if (item.mTargetItemType == UIBuildWndItem.ItemType.mBlockMat)
        {
            ItemAsset.ItemProto proto = ItemAsset.ItemProto.GetItemData(item.ItemId);
            if (!uiSkill._SkillMgr.CheckBuildBlockLevel(proto.level))
            {
                item.IsActive = false;
            }
            else
                item.IsActive = true;
        }
        else if (item.mTargetItemType == UIBuildWndItem.ItemType.mBlockPattern)
        {
            int index = item.mTargetIndex;

            if (uiSkill._SkillMgr != null)
            {
                if (!uiSkill._SkillMgr.CheckBuildShape(index))
                {
                    item.IsActive = false;
                }
                else
                    item.IsActive = true;
            }
        }
    }

    #region QuickBar_Event

    void OnMenuQuickBarClick(UIBuildWndItem item)
    {
        if (item.mTargetItemType != UIBuildWndItem.ItemType.mNull
            && item.mTargetItemType != UIBuildWndItem.ItemType.mMenu
            && !item.IsActive)
        {
            //lz-2016.10.31 Insufficient skill to use this item.
            new PeTipMsg(PELocalization.GetString(8000854), PeTipMsg.EMsgLevel.Warning, PeTipMsg.EMsgType.Misc);
            return;
        }


        switch (item.mTargetItemType)
        {
            case UIBuildWndItem.ItemType.mVoxelType:
                {
                    if (PEBuildingMan.Self != null)
                    {
                        if (!GameUI.Instance.mSkillWndCtrl._SkillMgr.CheckUnlockBuildBlockVoxel())
                        {
                            return;
                        }
                        PEBuildingMan.Self.Manipulator.MaterialType = (byte)item.ItemId;
                        PEBuildingMan.Self.Pattern = BSPattern.DefaultV1;

						mWndCtrl.DisselectBlock();
                        // Refresh the selected voxel mat for control window
                        int itemID = BSVoxelMatMap.GetItemID(item.ItemId);
                        for (int i = 0; i < mWndCtrl.mVoxelMatList.Count; i++)
                        {
                            if (mWndCtrl.mVoxelMatList[i].ItemId == itemID)
                            {
                                mWndCtrl.TextureListSelectIndex = i;

                                OnVoxelMatItemClick(i);

                                mWndCtrl.TypeSelectIndex = item.mTargetIndex;
                                break;
                            }
                        }
						
						ChangeBrushToBox();
                    }

                } break;
            case UIBuildWndItem.ItemType.mBlockMat:
                {
                    if (PEBuildingMan.Self != null)
                    {
                        PEBuildingMan.Self.Manipulator.MaterialType = (byte)PEBuildingMan.GetBlockMaterialType(item.ItemId);

                        if (PEBuildingMan.Self.Pattern == null || PEBuildingMan.Self.Pattern.type == EBSVoxelType.Voxel)
                        {
                            if (mWndCtrl.BlockPatternSelectIndex == -1)
                                PEBuildingMan.Self.Pattern = BSPattern.DefaultB1;
                            else
                            {
                                if (mWndCtrl.mBlockPatternList.Count > mWndCtrl.BlockPatternSelectIndex && mWndCtrl.TextureListSelectIndex > -1)
                                    PEBuildingMan.Self.Pattern = BSPattern.s_tblPatterns[mWndCtrl.mBlockPatternList[mWndCtrl.BlockPatternSelectIndex].ItemId];
                            }


                        }
						
						mWndCtrl.DisselectVoxel();

                        // Refresh the selected block mat for control window
                        for (int i = 0; i < mWndCtrl.mBlockMatList.Count; i++)
                        {
                            if (mWndCtrl.mBlockMatList[i].ItemId == item.ItemId)
                            {
                                mWndCtrl.BlockMatSelectIndex = i;
                                OnBlockMatClick(i);
                                break;
                            }
                        }

						ChangeBrushToBox();
                    }
                } break;
            case UIBuildWndItem.ItemType.mBlockPattern:
                {
                    if (PEBuildingMan.Self != null)
                    {

                        if (PEBuildingMan.Self.Pattern == null)
                        {
                            OnBlockMatClick(0);
                        }
                        else if (PEBuildingMan.Self.Pattern.type == EBSVoxelType.Voxel)
                        {
                            if (mWndCtrl.mVoxelMatList.Count > mWndCtrl.TextureListSelectIndex && mWndCtrl.TextureListSelectIndex > -1)
                                PEBuildingMan.Self.Manipulator.MaterialType = (byte)PEBuildingMan.GetBlockMaterialType(mWndCtrl.mVoxelMatList[mWndCtrl.TextureListSelectIndex].ItemId);
                        }

						mWndCtrl.DisselectVoxel();

                        int i = 0;
                        foreach (var kvp in BSPattern.s_tblPatterns)
                        {
                            if (i == item.mTargetIndex)
                            {
								if (mWndCtrl.TextureListSelectIndex != -1)
								{
									int itemId = mWndCtrl.mVoxelMatList[mWndCtrl.TextureListSelectIndex].ItemId;
									PEBuildingMan.Self.Manipulator.MaterialType = (byte)PEBuildingMan.GetBlockMaterialType(itemId);
									int find_index = mWndCtrl.mBlockMatList.FindIndex(item0 => item0.ItemId == itemId);
									if (find_index != -1)
										mWndCtrl.BlockMatSelectIndex = find_index;
									else
										mWndCtrl.BlockMatSelectIndex = 0;
								}
								else if (mWndCtrl.BlockMatSelectIndex == -1)
								{
									PEBuildingMan.Self.Manipulator.MaterialType = (byte)PEBuildingMan.GetBlockMaterialType(mWndCtrl.mBlockMatList[0].ItemId);
									mWndCtrl.BlockMatSelectIndex = 0;
								}
							
                                PEBuildingMan.Self.Pattern = kvp.Value;

                                break;
                            }
                            i++;
                        }

                        if (item.mTargetIndex != -1)
                            mWndCtrl.BlockPatternSelectIndex = item.mTargetIndex;
						
						ChangeBrushToBox();
                    }
                } break;
            default:
                break;
        }
    }

    //void OnMenuQuickSwitchPointBrush()
    //{
    //    if (pointMode == EBSBrushMode.Add)
    //        mMenuCtrl.OnBrushItemClick(UIBrushMenuItem.BrushType.pointRemove);
    //    else if (pointMode == EBSBrushMode.Subtract)
    //        mMenuCtrl.OnBrushItemClick(UIBrushMenuItem.BrushType.pointAdd);
    //}

    //void OnMenuQuickSwitchDiagonalBrush()
    //{
    //    if (diagonalRot == 0)
    //        mMenuCtrl.OnBrushItemClick(UIBrushMenuItem.BrushType.diagonalZPos);
    //    else if (diagonalRot == 1)
    //        mMenuCtrl.OnBrushItemClick(UIBrushMenuItem.BrushType.diagonalXNeg);
    //    else if (diagonalRot == 2)
    //        mMenuCtrl.OnBrushItemClick(UIBrushMenuItem.BrushType.diagonalZNeg);
    //    else if (diagonalRot == 3)
    //        mMenuCtrl.OnBrushItemClick(UIBrushMenuItem.BrushType.diagonalXPos);
    //}

    //void OnMenuQuickSwitchBoxBrush()
    //{
    //    if (boxMode == EBSBrushMode.Add)
    //        mMenuCtrl.OnBrushItemClick(UIBrushMenuItem.BrushType.boxRemove);
    //    else if (boxMode == EBSBrushMode.Subtract)
    //        mMenuCtrl.OnBrushItemClick(UIBrushMenuItem.BrushType.boxAdd);
    //}


    #endregion

    #endregion



    #region Save_Wnd_Event

    bool OnSaveIsoClick(string iso_name)
    {
        BSMiscBrush select_brush = m_CurBrush as BSMiscBrush;
		BSIsoSelectBrush iso_select = m_CurBrush as BSIsoSelectBrush;
		if (select_brush != null || iso_select != null)
        {
            if (iso_name != "")
            {
                // Icon
                int width = PEBuildingMan.Self.IsoCaputure.photoRT.width;
                int height = PEBuildingMan.Self.IsoCaputure.photoRT.height;
                Texture2D iconTex = new Texture2D(width, height, TextureFormat.ARGB32, false);

                RenderTexture.active = PEBuildingMan.Self.IsoCaputure.photoRT;

                iconTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                iconTex.Apply();

                RenderTexture.active = null;


                BSIsoData iso = null;

				if (select_brush != null && select_brush.SaveToIso(iso_name, iconTex.EncodeToPNG(), out iso))
                {
                    _initIosBlockPage = false;
                    InitIsoBlockPage();
                    if (onSaveIsoClick != null)
                        onSaveIsoClick();
                    return true;
                }
				else if (iso_select != null && iso_select.SaveToIso(iso_name, iconTex.EncodeToPNG(), out iso))
				{
					_initIosBlockPage = false;
					InitIsoBlockPage();
					if (onSaveIsoClick != null)
						onSaveIsoClick();
					return true;
				}
                else
                {
                    MessageBox_N.ShowOkBox(PELocalization.GetString(8000494));
                }

            }
            else
                MessageBox_N.ShowOkBox(PELocalization.GetString(8000494));
        }

        return false;

    }

    void OnSaveIsoClose()
    {
        PEBuildingMan.Self.IsoCaputure.DisableCapture();
        BSSelectBrush select_brush = m_CurBrush as BSSelectBrush;
        if (select_brush != null)
            select_brush.canDo = true;
    }

    #endregion

    #region Build_Wnd_Event

    void OnPageClick(int type)
    {
        // Voxel
        if (type == 0)
        {
            InitVoxelPage();
        }
        else if (type == 1)
        {
            InitBlockPage();
        }
        else if (type == 2)
        {
            InitIsoBlockPage();
        }
    }

    int _deleteIndex = -1;
    void OnIsoDeleteClick(int index)
    {
        if (index == -1)
            return;

        _deleteIndex = index;
        MessageBox_N.ShowYNBox(PELocalization.GetString(8000501), OnSureToDeleteIso);


    }

    void OnSureToDeleteIso()
    {
        string iso_name = m_IsoHeaders[_deleteIndex].Name;
        string FilePath = GameConfig.GetUserDataPath() + BuildingMan.s_IsoPath + iso_name + ".biso";

        if (System.IO.File.Exists(FilePath))
        {
            System.IO.File.Delete(FilePath);

            mWndCtrl.ClearCostList();

            mWndCtrl.IsoListSelectIndex = -1;
            m_IsoHeaders.RemoveAt(_deleteIndex);
            mWndCtrl.RemoveIsoItem(_deleteIndex);

            mWndCtrl.ResetIsoPostion();

            Debug.Log("Delete the " + iso_name + " successfully");
        }
    }

    void OnIsoExportClick(int index)
    {
        BSIsoHeadData head = m_IsoHeaders[index];
        CreateBrush(BrushType.bt_iso);
        BSIsoBrush brush = m_CurBrush as BSIsoBrush;
        brush.File_Name = head.Name;
        brush.Gen = true;
        brush.onCancelClick += OnIsoCancelClick;
	
		mWndCtrl.Hide();

        if (onIsoExport != null)
            onIsoExport(head);
    }

    void OnIsoCancelClick()
    {
        mMenuCtrl.ResetMenuButtonClickEvent(false);
		mWndCtrl.Show();
		mWndCtrl.ChangeCkMenu(2);
    }

    #endregion

    #region Item_event
    void OnItemToolTip(bool show, UIBuildWndItem.ItemType item_type, int item_index)
    {
        if (item_type == UIBuildWndItem.ItemType.mVoxelMat)
        {
            ItemProto proto = ItemProto.Mgr.Instance.Get(m_VoxelProtoItems[item_index]);
            UITooltip.ShowText(proto.GetName());
        }
        else if (item_type == UIBuildWndItem.ItemType.mBlockMat)
        {
            ItemProto proto = ItemProto.Mgr.Instance.Get(m_BlockProtoItems[item_index]);
            UITooltip.ShowText(proto.GetName());
        }
        else if (item_type == UIBuildWndItem.ItemType.mMenu)
        {
            UIBuildWndItem item = mMenuCtrl.mBoxBars.Items[item_index].GetComponent<UIBuildWndItem>();
            if (item != null)
            {
                if (item.mTargetItemType == UIBuildWndItem.ItemType.mVoxelMat)
                {
                    ItemProto proto = ItemProto.Mgr.Instance.Get(m_VoxelProtoItems[item.mTargetIndex]);
                    UITooltip.ShowText(proto.GetName());
                }
                else if (item.mTargetItemType == UIBuildWndItem.ItemType.mBlockMat)
                {
                    ItemProto proto = ItemProto.Mgr.Instance.Get(m_BlockProtoItems[item.mTargetIndex]);
                    UITooltip.ShowText(proto.GetName());
                }
                else if (item.mTargetItemType == UIBuildWndItem.ItemType.mVoxelType)
                {

                    int proto_id = BSVoxelMatMap.GetItemID(item.ItemId);
                    ItemProto proto = ItemProto.Mgr.Instance.Get(proto_id);
                    UITooltip.ShowText(proto.GetName());
                }
            }
        }
        else if (item_type == UIBuildWndItem.ItemType.mVoxelType)
        {

            int proto_id = BSVoxelMatMap.GetItemID(m_VoxelTypeList[item_index]);
            ItemProto proto = ItemProto.Mgr.Instance.Get(proto_id);
            UITooltip.ShowText(proto.GetName());
        }
    }

    void OnVoxelMatItemClick(int index)
    {
        if (mWndCtrl.TextureListSelectIndex == -1 || mWndCtrl.TextureListSelectIndex >= m_VoxelProtoItems.Count)
            return;

        if (m_VoxelProtoItems == null)
            InitVoxelPage();

        if (m_VoxelProtoItems == null)
            return;

        m_VoxelTypeList = BSVoxelMatMap.GetMaterialIDs(m_VoxelProtoItems[mWndCtrl.TextureListSelectIndex]);

        List<string> icons = new List<string>();
        foreach (int type_id in m_VoxelTypeList)
        {
            BSVoxelMatMap.MapData data = BSVoxelMatMap.GetMapData(type_id);
			if(data != null)		icons.Add(data.icon);
        }

        PEBuildingMan.Self.Pattern = BSPattern.DefaultV1;

        mWndCtrl.RefreshTypeLisItem(icons.ToArray(), "Icon");

        for (int i = 0; i < mWndCtrl.mTypeList.Count; i++)
        {
            mWndCtrl.mTypeList[i].SetItemID(m_VoxelTypeList[i]);
        }

        if (mWndCtrl.TypeSelectIndex > -1)
        {
            if (mWndCtrl.TypeSelectIndex >= mWndCtrl.mTypeList.Count)
                mWndCtrl.TypeSelectIndex = 0;

            PEBuildingMan.Self.Manipulator.MaterialType = (byte)mWndCtrl.mTypeList[mWndCtrl.TypeSelectIndex].ItemId;
        }
        else
        {
            mWndCtrl.TypeSelectIndex = 0;
            OnVoxelTypeClick(mWndCtrl.TypeSelectIndex);
        }
			
        mWndCtrl.DisselectBlock();

		ChangeBrushToBox();
    }

    void OnVoxelTypeClick(int index)
    {
        if (PEBuildingMan.Self == null)
            return;
        UIBuildWndItem item = null;
        if (mWndCtrl.TextureListSelectIndex > -1)
        {
            item = mWndCtrl.mVoxelMatList[mWndCtrl.TextureListSelectIndex];
            item.mSubsetIndex = index;

            PEBuildingMan.Self.Manipulator.MaterialType = (byte)m_VoxelTypeList[mWndCtrl.TypeSelectIndex];
            PEBuildingMan.Self.Pattern = BSPattern.DefaultV1;

        }
    }

    void OnBlockMatClick(int index)
    {

        if (PEBuildingMan.Self == null)
            return;

        if (!mWndCtrl.mBlockMatList[index].IsActive)
        {
            //lz-2016.10.31 Insufficient skill to use this item.
            new PeTipMsg(PELocalization.GetString(8000854), PeTipMsg.EMsgLevel.Warning, PeTipMsg.EMsgType.Misc);
            return;
        }

        mWndCtrl.DisselectVoxel();

        PEBuildingMan.Self.Manipulator.MaterialType = (byte)PEBuildingMan.GetBlockMaterialType(mWndCtrl.mBlockMatList[index].ItemId);

        //lw:2017.4.6 Crash修复
        if (GameUI.Instance.mSkillWndCtrl != null && GameUI.Instance.mSkillWndCtrl._SkillMgr != null
            && !GameUI.Instance.mSkillWndCtrl._SkillMgr.CheckBuildShape(mWndCtrl.BlockPatternSelectIndex))
        {
            PEBuildingMan.Self.Pattern = BSPattern.DefaultB1;
        }
        else
        {
            if (mWndCtrl.BlockPatternSelectIndex == -1)
                PEBuildingMan.Self.Pattern = BSPattern.DefaultB1;
            else
                PEBuildingMan.Self.Pattern = BSPattern.s_tblPatterns[mWndCtrl.mBlockPatternList[mWndCtrl.BlockPatternSelectIndex].ItemId];
        }

		ChangeBrushToBox();
			
    }

    void OnBlockPatternClick(int index)
    {
        if (PEBuildingMan.Self == null)
            return;

        if (!mWndCtrl.mBlockPatternList[index].IsActive)
        {
            //lz-2016.10.31 Insufficient skill to use this item.
            new PeTipMsg(PELocalization.GetString(8000854), PeTipMsg.EMsgLevel.Warning, PeTipMsg.EMsgType.Misc);
            return;
        }


        int i = 0;
        foreach (var kvp in BSPattern.s_tblPatterns)
        {
            if (i == index)
            {
                if (mWndCtrl.BlockMatSelectIndex == -1)
                {

                    // Voxel Mat is selected ?  Swicth it to block mat !
                    if (mWndCtrl.TextureListSelectIndex != -1)
                    {
                        int itemId = mWndCtrl.mVoxelMatList[mWndCtrl.TextureListSelectIndex].ItemId;
                        PEBuildingMan.Self.Manipulator.MaterialType = (byte)PEBuildingMan.GetBlockMaterialType(itemId);
                        int find_index = mWndCtrl.mBlockMatList.FindIndex(item0 => item0.ItemId == itemId);
                        if (find_index != -1)
                            mWndCtrl.BlockMatSelectIndex = find_index;
                        else
                            mWndCtrl.BlockMatSelectIndex = 0;

						mWndCtrl.DisselectVoxel();
                    }
                    else
                    {
                        PEBuildingMan.Self.Manipulator.MaterialType = (byte)PEBuildingMan.GetBlockMaterialType(mWndCtrl.mBlockMatList[0].ItemId);
                        mWndCtrl.BlockMatSelectIndex = 0;
                    }
                }
                ItemAsset.ItemProto item = ItemAsset.ItemProto.GetItemData(mWndCtrl.mBlockMatList[mWndCtrl.BlockMatSelectIndex].ItemId);
                if (!GameUI.Instance.mSkillWndCtrl._SkillMgr.CheckBuildBlockLevel(item.level))
                {
                    PEBuildingMan.Self.Manipulator.MaterialType = 2;
                }
                PEBuildingMan.Self.Pattern = kvp.Value;

				ChangeBrushToBox();
                break;
            }
            i++;
        }
    }

    bool IsoRePos = false;

    void OnIsoClick(int index)
    {
        if (mWndCtrl.IsoListSelectIndex > -1)
        {
            mWndCtrl.ClearCostList();

            BSIsoHeadData header = m_IsoHeaders[mWndCtrl.IsoListSelectIndex];
            foreach (var kvp in header.costs)
            {
                int item_id = PEBuildingMan.GetBlockItemProtoID(kvp.Key);

                //				if (item_id < PEBuildingMan.c_MinItemProtoID || item_id > PEBuildingMan.c_MaxItemProtoID)
                //					continue;
                if (item_id == -1)
                    return;

                ItemProto proto = ItemProto.Mgr.Instance.Get(item_id);
                Pathea.PlayerPackageCmpt pkg = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.PlayerPackageCmpt>();
                int player_cnt = pkg.GetItemCount(item_id);
                int final_cnt = Mathf.Clamp(player_cnt, 0, 9999);
				mWndCtrl.AddCostListItem(proto.GetName(), Mathf.CeilToInt(kvp.Value / 4.0f).ToString() + '/' + final_cnt.ToString(), proto.icon[0], "Icon");
            }
            IsoRePos = true;
            //mWndCtrl.ResetCostPostion();
        }
    }

    void OnBrushMenuItemClick(UIBrushMenuItem.BrushType type)
    {

        if (type == UIBrushMenuItem.BrushType.pointAdd)
        {
			mMenuCtrl.SetBrushItemSprite(type, menuItemAddColor);
            pointMode = EBSBrushMode.Add;
            if (m_CurBrush as BSPointBrush != null)
                m_CurBrush.mode = pointMode;

            mMenuCtrl.ManualEnbleBtn(mMenuCtrl.mPointBtn);
        }
        else if (type == UIBrushMenuItem.BrushType.pointRemove)
        {
			mMenuCtrl.SetBrushItemSprite(type, menuItemRemoveColor);
            pointMode = EBSBrushMode.Subtract;
            if (m_CurBrush as BSPointBrush != null)
                m_CurBrush.mode = pointMode;

            mMenuCtrl.ManualEnbleBtn(mMenuCtrl.mPointBtn);
        }
        else if (type == UIBrushMenuItem.BrushType.boxAdd)
        {
			mMenuCtrl.SetBrushItemSprite(type, menuItemAddColor);
            boxMode = EBSBrushMode.Add;
            if (m_CurBrush as BSBoxBrush != null)
                m_CurBrush.mode = boxMode;

            mMenuCtrl.ManualEnbleBtn(mMenuCtrl.mBoxBtn);
        }
        else if (type == UIBrushMenuItem.BrushType.boxRemove)
        {
			mMenuCtrl.SetBrushItemSprite(type, menuItemRemoveColor);
            boxMode = EBSBrushMode.Subtract;
            if (m_CurBrush as BSBoxBrush != null)
                m_CurBrush.mode = boxMode;
            mMenuCtrl.ManualEnbleBtn(mMenuCtrl.mBoxBtn);
        }
        else if (type == UIBrushMenuItem.BrushType.diagonalXPos)
        {
            diagonalRot = 0;
            BSB45DiagonalBrush db = m_CurBrush as BSB45DiagonalBrush;
            if (db != null)
            {
                db.m_Rot = diagonalRot;
            }

            mMenuCtrl.ManualEnbleBtn(mMenuCtrl.mDiagonalBtn);
        }
        else if (type == UIBrushMenuItem.BrushType.diagonalXNeg)
        {
			mMenuCtrl.SetBrushItemSprite(type, menuItemAddColor);
            diagonalRot = 2;
            BSB45DiagonalBrush db = m_CurBrush as BSB45DiagonalBrush;
            if (db != null)
            {
                db.m_Rot = diagonalRot;
            }

            mMenuCtrl.ManualEnbleBtn(mMenuCtrl.mDiagonalBtn);
        }
        else if (type == UIBrushMenuItem.BrushType.diagonalZPos)
        {
			mMenuCtrl.SetBrushItemSprite(type, menuItemAddColor);
            diagonalRot = 1;
            BSB45DiagonalBrush db = m_CurBrush as BSB45DiagonalBrush;
            if (db != null)
            {
                db.m_Rot = diagonalRot;
            }

            mMenuCtrl.ManualEnbleBtn(mMenuCtrl.mDiagonalBtn);
        }
        else if (type == UIBrushMenuItem.BrushType.diagonalZNeg)
        {
			mMenuCtrl.SetBrushItemSprite(type, menuItemAddColor);
            diagonalRot = 3;
            BSB45DiagonalBrush db = m_CurBrush as BSB45DiagonalBrush;
            if (db != null)
            {
                db.m_Rot = diagonalRot;
            }

            mMenuCtrl.ManualEnbleBtn(mMenuCtrl.mDiagonalBtn);
        }
		else if (type == UIBrushMenuItem.BrushType.SelectAll)
		{
			mMenuCtrl.SetBrushItemSprite(type, menuItemAddColor);
			if (m_CurBrush as BSIsoSelectBrush != null)
				CreateBrush(BrushType.bt_selectAll);
		}
		else if (type == UIBrushMenuItem.BrushType.SelectDetail)
		{
			mMenuCtrl.SetBrushItemSprite(type, menuItemAddColor);
			if (m_CurBrush as BSMiscBrush != null)
				CreateBrush(BrushType.bt_selectBlock);
			
		}
	

    }

    void OnMenuItemClick(int index)
    {
        if (Input.GetMouseButtonUp(1))
        {
            OnMenuQuickBarClick(mMenuCtrl.mMenuList[index]);
            mMenuCtrl.mMenuList[index].PlayGridEffect();
        }
    }

	void OnMenuBlockSelectClick ()
	{
		if (mWndCtrl.BlockMatSelectIndex == -1)
		{
			mWndCtrl.BlockMatSelectIndex = 0;
			OnBlockMatClick(0);
			mWndCtrl.BlockPatternSelectIndex = 0;
			OnBlockPatternClick(0);
		}
	}

	void OnMenuVoxelSelectClick ()
	{
		if (mWndCtrl.TextureListSelectIndex == -1)
		{
			mWndCtrl.TextureListSelectIndex = 0;
			OnVoxelMatItemClick(0);
			mWndCtrl.TypeSelectIndex = 0;
			OnVoxelTypeClick(0);
		}
	}

    #endregion


    #region Other_UI_Event

    bool _showTips = true;
    const string SAVESHOWTIPKEY = "UIBuildTips";

    void InitBuildTips()
    {
        if (null != UIRecentDataMgr.Instance)
        {
            _showTips = (UIRecentDataMgr.Instance.GetIntValue(SAVESHOWTIPKEY, _showTips ? 1 : 0) > 0 ? true : false);
            SetTipsState(_showTips);
        }
    }
    
    void OnTipsBtnClick()
    {
        SetTipsState(!_showTips);
    }

    void SetTipsState(bool isShowTips)
    {
        mTipsRoot.SetActive(isShowTips);
        if (isShowTips)
        {
            mTipsNormSprite.gameObject.SetActive(true);
            mTipsShowSprite.gameObject.SetActive(false);
        }
        else
        {

            mTipsShowSprite.gameObject.SetActive(true);
            mTipsNormSprite.gameObject.SetActive(false);
        }
        if (_showTips != isShowTips)
        {
            _showTips = isShowTips;
            if (null != UIRecentDataMgr.Instance)
            {
                UIRecentDataMgr.Instance.SetIntValue(SAVESHOWTIPKEY, isShowTips ? 1 : 0);
            }
        }
    }
    #endregion

	void ChangeBrushToBox()
	{
		if (m_CurBrushType == BrushType.bt_selectAll || m_CurBrushType == BrushType.bt_selectBlock
			|| m_CurBrushType == BrushType.bt_inclined)
		{
			mMenuCtrl.mBoxBtn.checkBox.isChecked = true;
			UIBuildBlock.Instance.CreateBrush( UIBuildBlock.BrushType.bt_box);
		}
	}

    #region BuildTutorial 
    
    public void ShowAllBuildTutorial()
    {
        //lz-2016.11.02 检测对话触发buildTutorial
        if (PeGameMgr.IsTutorial)
        {
            if(!gameObject.activeSelf)
                EnterBuildMode();
            mBuildTutorialCtrl.ShowAllBuildTutorial();
        }
    }
    #endregion

}
