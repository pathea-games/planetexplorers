using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using Pathea;
using PeEvent;
using System;

public class UIMainMidCtrl : UIStaticWnd
{
    [SerializeField]
    GameObject mNewGridPrefab = null;
    [SerializeField]
    GameObject mEngun;
    [SerializeField]
    GameObject mgun;
    [SerializeField]
    GameObject mBow;
    [SerializeField]
    UIGridBoxBars mBoxBar;
    [SerializeField]
    int mItemCount;
    [SerializeField]
    UIHealthBar mHp_0;
    [SerializeField]
    UIHealthBar mStaminaBar_1;
    [SerializeField]
    UISlider mScEnerger_2;
    [SerializeField]
    UISlider mScJect_3;
    [SerializeField]
    UISlider mComfort_4;
    [SerializeField]
    UISlider mScHunger_5;
    [SerializeField]
    GameObject mGun_6;
    [SerializeField]
    UISlider mScShild_8;
    [SerializeField]
    UISlider mScOxygen_9;
    [SerializeField]
    UILabel mGunLabel;
    [SerializeField]
    UISprite mComfortSprite; //lz-2016.07.19 显示舒适度每个阶段的表情
    [SerializeField]
    AnimationCurve mComfortAlphaCurve;  //lz-2016.07.27 舒适度Alpha动画曲线
    [SerializeField]
    UISprite mComfortForeground; 
    [SerializeField]
    float m_ComfortFallValue = 10f; //lz-2016.07.27 舒适度下降值，下降这么多播放一次动画
    [SerializeField]

    float m_lastComfort;
    float m_CurComfort;
    bool m_CurvePlay;
    float m_CurveTotalTime;

    static UIMainMidCtrl _instance = null;
    public static UIMainMidCtrl Instance { get { return _instance; } }

	static readonly float AttrLerpF = 5f;

    List<QuickBarItem_N> mItems = new List<QuickBarItem_N>();
    //bool mUpdateLink = false;

    ShortCutSlotList mCutSlotList = null;
    PlayerPackageCmpt mPackageCmpt;

    Motion_Equip mEquip;

    Motion_Equip equip
    {
        get
        {
            if (null == mEquip && null != GameUI.Instance.mMainPlayer)
                mEquip = GameUI.Instance.mMainPlayer.GetCmpt<Motion_Equip>();
            return mEquip;
        }
    }

    PeEntity mMainPlayer;
    PeEntity mainPlayer
    {
        get
        {
            if (null == mMainPlayer)
            {
                mMainPlayer = MainPlayer.Instance.entity;
                if (null != mMainPlayer)
                {
                    mMainPlayer.equipmentCmpt.changeEventor.Subscribe(SetCurUseItemByEvent);
                }
            }

            return mMainPlayer;
        }
    }

    public float EnergyValue { get { return mScEnerger_2.sliderValue; } set { mScEnerger_2.sliderValue = value; } }

    public float Ject_Value { get { return mScJect_3.sliderValue; } set { mScJect_3.sliderValue = value; } }

    private PEGun mPeGun;

    private PEGun peGun
    {
        get
        {
            if (null == mPeGun)
            {
                PeEntity mainPlayer = Pathea.PeCreature.Instance.mainPlayer;
                if (null != mainPlayer)
                {
                    if (null != mainPlayer.motionEquipment)
                        mPeGun = mainPlayer.motionEquipment.gun;
                }
            }
            return mPeGun;
        }
    }

    private PEBow mPEBow;

    private PEBow peBow
    {
        get
        {
            if (null == mPEBow)
            {
                PeEntity mainPlayer = Pathea.PeCreature.Instance.mainPlayer;
                if (null != mainPlayer)
                {
                    if (null != mainPlayer.motionEquipment)
                        mPEBow = mainPlayer.motionEquipment.bow;
                }
            }
            return mPEBow;
        }
    }

    void Awake()
    {
        _instance = this;
    }

    void Start()
    {
        InitGrid();
    }


    void UpdatePlayerAtrbute()
    {

        mHp_0.mMaxValue = GameUI.Instance.mMainPlayer.GetAttribute(AttribType.HpMax);
		mHp_0.mCurValue = Mathf.Lerp(mHp_0.mCurValue, GameUI.Instance.mMainPlayer.GetAttribute(AttribType.Hp), AttrLerpF * Time.deltaTime);
        mStaminaBar_1.mMaxValue = GameUI.Instance.mMainPlayer.GetAttribute(AttribType.StaminaMax);
		mStaminaBar_1.mCurValue = Mathf.Lerp(mStaminaBar_1.mCurValue, GameUI.Instance.mMainPlayer.GetAttribute(AttribType.Stamina), AttrLerpF * Time.deltaTime);

        //lz-2016.09.30 下面是处理最大值会等于0,0不可以做除数的问题
        float curValue,maxValue;
        curValue=maxValue = 0f;

        curValue = GameUI.Instance.mMainPlayer.GetAttribute(AttribType.Energy);
        maxValue = GameUI.Instance.mMainPlayer.GetAttribute(AttribType.EnergyMax);
        mScEnerger_2.sliderValue = maxValue <= 0 ? 0 : Convert.ToSingle(curValue / maxValue);

        curValue = GameUI.Instance.mMainPlayer.GetAttribute(AttribType.Comfort);
        maxValue = GameUI.Instance.mMainPlayer.GetAttribute(AttribType.ComfortMax);
        mComfort_4.sliderValue = maxValue <= 0 ? 0 : Convert.ToSingle(curValue / maxValue);

        curValue = GameUI.Instance.mMainPlayer.GetAttribute(AttribType.Hunger);
        maxValue = GameUI.Instance.mMainPlayer.GetAttribute(AttribType.HungerMax);
        mScHunger_5.sliderValue = maxValue <= 0 ? 0 : Convert.ToSingle(curValue / maxValue);

        curValue = GameUI.Instance.mMainPlayer.GetAttribute(AttribType.Shield);
        maxValue = GameUI.Instance.mMainPlayer.GetAttribute(AttribType.ShieldMax);
        mScShild_8.sliderValue = maxValue <= 0 ? 0 : Convert.ToSingle(curValue / maxValue);

        curValue = GameUI.Instance.mMainPlayer.GetAttribute(AttribType.Oxygen);
        maxValue = GameUI.Instance.mMainPlayer.GetAttribute(AttribType.OxygenMax);
        mScOxygen_9.sliderValue = maxValue <= 0 ? 0 : Convert.ToSingle(curValue / maxValue);

        //lz-2016.07.27 舒适度突然下降一个阶段值，就播放曲线动画
        m_CurComfort = GameUI.Instance.mMainPlayer.GetAttribute(AttribType.Comfort);
        if (m_CurComfort > m_lastComfort)
        {
            m_lastComfort = m_CurComfort;
        }
        else if (m_lastComfort - m_CurComfort > m_ComfortFallValue)
        {
            m_CurvePlay = true;
            m_lastComfort = m_CurComfort;
            m_CurveTotalTime = 0f;
        }
        else
        {
            m_lastComfort = m_CurComfort; 
        }

        if (m_CurvePlay)
        {
            m_CurveTotalTime += Time.deltaTime;
            mComfortForeground.alpha = mComfortAlphaCurve.Evaluate(m_CurveTotalTime);
            if (mComfortForeground.alpha >= 1f)
            {
                m_CurvePlay = false;
            }
        }

        //lz-2016.07.19 根据舒适度显示对应的表情图
        string smiliesSpr = "";
        if (mComfort_4.sliderValue < 0.2f)
            smiliesSpr = "face3";
        else if (mComfort_4.sliderValue < 0.5f)
            smiliesSpr = "face2";
        else
            smiliesSpr = "face1";
        if (smiliesSpr != "" && smiliesSpr != mComfortSprite.spriteName)
        {
            mComfortSprite.spriteName = smiliesSpr;
            mComfortSprite.MakePixelPerfect();
        }
    }

    void GetCutSlotList()
    {
        if (GameUI.Instance == null)
            return;
        if (GameUI.Instance.mMainPlayer == null)
            return;
        mPackageCmpt = GameUI.Instance.mMainPlayer.GetCmpt<PlayerPackageCmpt>();
        if (mPackageCmpt != null)
        {
            mCutSlotList = mPackageCmpt.shortCutSlotList;
            if (null != mCutSlotList)
            {
                mCutSlotList.onListUpdate += OnShortCutUpdate;
                OnShortCutUpdate();
            }
        }

    }

    public bool playTween = false;
    bool forward = true;
    // Update is called once per frame
    void Update()
    {
        if (playTween)
        {
            PlayTween(forward);
            forward = !forward;
            playTween = false;
        }


        if (GameUI.Instance == null)
            return;

        UpdateShortCut();

        //TryRegister();//注册血量上限改变事件

        if (GameUI.Instance.mMainPlayer != null)
            UpdatePlayerAtrbute();

        UpdataGunNum();

        UpdataJetPack();


    }
    void LateUpdate()
    {
        UpdateReposition();
    }

    void InitGrid()
    {
        mBoxBar.Init(mNewGridPrefab.gameObject, mItemCount);
        if (mBoxBar.Items.Count > 0)
        {
            //log:lz-2016.05.04 第一个格子只作为显示用当前使用的武器，不能操作
            mItems.Add(mBoxBar.Items[0].GetComponent<QuickBarItem_N>());
            mItems[0].UpdateKeyInfo(-1);

            if (null != mainPlayer)
            {
                SetCurUseItem(mainPlayer.equipmentCmpt.mainHandEquipment);
            }
            else
            {
                MainPlayer.Instance.mainPlayerCreatedEventor.Subscribe((a, e) => { SetCurUseItem(mainPlayer.equipmentCmpt.mainHandEquipment); });

            }
            for (int i = 1; i < mItemCount; i++)
            {
                mItems.Add(mBoxBar.Items[i].GetComponent<QuickBarItem_N>());
                mItems[i].SetItemPlace(ItemPlaceType.IPT_HotKeyBar, i);
                mItems[i].SetGridMask(GridMask.GM_Any);
                mItems[i].UpdateKeyInfo(i);
                mItems[i].ItemIndex = i-1;
                mItems[i].onLeftMouseClicked = OnLeftMouseCliked;
                mItems[i].onRightMouseClicked = OnRightMouseCliked;
                mItems[i].onDropItem = OnDropItem;
            }
        }
        mBoxBar.e_PageIndexChange += (pageIndex) => this.OnShortCutUpdate();
    }

    void OnLeftMouseCliked(Grid_N grid)
    {
        if (grid.Item != null)
        {
            //lz-2016.08.10 不允许快捷栏操作当前包裹正在操作的Item
            if (GameUI.Instance.mItemPackageCtrl.EqualUsingItem(grid.Item))
                return;
            SelectItem_N.Instance.SetItemGrid(grid);
        }
    }

    void OnRightMouseCliked(Grid_N grid)
    {
        if (null != grid.Item)
        {
            //lz-2016.08.10 不允许快捷栏操作当前包裹正在操作的Item
            if (GameUI.Instance.mItemPackageCtrl.EqualUsingItem(grid.Item))
                return;
            int index = grid.ItemIndex + this.GetCurPageIndex();
            UseItem(mCutSlotList.GetItemObj(index));

            //mItems[index%10].SetItem(mCutSlotList.GetItemObj(index));
            //mItems[index%10].SetClick();
        }

    }


    /// <summary>
    /// 物品快捷栏的禁用和打开
    /// </summary>
    public bool quickBarForbiden
    {
        set
        {
            if (mItems.Count == 0) return;

            foreach (Grid_N grid in mItems)
            {
                grid.SetGridForbiden(value);
            }
        }
    }


    public void OnKeyDown_QuickBar(int inputIndex)
    {
        if (inputIndex < 0 || inputIndex >= (mItems.Count-1))
            return;

        if (mItems[inputIndex+1].IsForbiden())
            return;

        int index = inputIndex + this.GetCurPageIndex();
        ShortCutItem scItem= mCutSlotList.GetItem(index);
        if (scItem != null)
        {
            ItemObject item = mCutSlotList.GetItemObj(index);
            //lz-2016.08.10 不允许快捷栏操作当前包裹正在操作的Item
            if (GameUI.Instance.mItemPackageCtrl.EqualUsingItem(item))
                return;
            UseItem(item);
        }
    }

    //lz-2016.06.28 上一页
    public void OnKeyFunc_PrevQuickBar()
    {
        mBoxBar.BtnPageUpOnClick();
    }

    //lz-2016.06.28 下一页
    public void OnKeyFunc_NextQuickBar()
    {
        mBoxBar.BtnPageDnOnClick();
    }

    void UseItem(ItemObject itemObj)
    {
        if (itemObj == null)
            return;

        Pathea.UseItemCmpt useItem = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.UseItemCmpt>();
        
        if (null == useItem)
        {
            useItem = Pathea.PeCreature.Instance.mainPlayer.Add<Pathea.UseItemCmpt>();
        }
        if (useItem.Request(itemObj))
        {
            // mPackageCmpt.Remove(itemObj);
        }
    }

    public void SetCurUseItemByEvent(object sender, EquipmentCmpt.EventArg arg)
    {
        if (0 != (arg.itemObj.protoData.equipPos & (1 << 4)))
        {
            this.SetCurUseItem(arg.isAdd ? arg.itemObj : null);
        }
    }

    void SetCurUseItem(ItemObject itemObj)
    {
        if (null != mItems && mItems.Count > 0)
        {
            mItems[0].SetItem(itemObj);
            mItems[0].mScriptIco.spriteName = (null == itemObj) ? "itemhand" : "itemhand_get";
            mItems[0].mScriptIco.MakePixelPerfect();
        }
    }

    public delegate void OnDropItemTask();
    public event OnDropItemTask e_OnDropItemTask = null;

    public void OnDropItem(Grid_N grid)
    {
        //lz-2016.11.14 Crush bug
        if (null == SelectItem_N.Instance || null==SelectItem_N.Instance.ItemSample || null == GameUI.Instance|| null==mCutSlotList|| null==grid)
            return;

        //lz-2016.08.10 不允许快捷栏操作当前包裹正在操作的Item 
        if (null!=GameUI.Instance.mItemPackageCtrl && GameUI.Instance.mItemPackageCtrl.EqualUsingItem(SelectItem_N.Instance.ItemSample))
            return;

        if (e_OnDropItemTask != null&& SelectItem_N.Instance.ItemSample.protoId == 916)
            e_OnDropItemTask();

        if (GameConfig.IsMultiMode)
        {
            int selectIndex = SelectItem_N.Instance.Index + GetCurPageIndex();
            int gridIndex = grid.ItemIndex + GetCurPageIndex();

            int srcIndex = -1;
            int objId = -1;
            if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_HotKeyBar)
            {
                ShortCutItem item = mCutSlotList.GetItem(selectIndex);
                if (null != item)
                {
                    srcIndex = selectIndex;
                    objId = item.itemInstanceId;
                }
            }
            else
            {
                if (SelectItem_N.Instance.ItemObj == null)
                {
                    Debug.Log("UIMainMidCtrl.OnDropItem SelectItem_N.Instance.ItemObj == null");
                    return;
                }
                srcIndex = -1;
                objId = SelectItem_N.Instance.ItemObj.instanceId;
            }

            if (null != PlayerNetwork.mainPlayer)
                PlayerNetwork.mainPlayer.RequestSetShortcuts(objId, srcIndex, gridIndex, SelectItem_N.Instance.Place);

            SelectItem_N.Instance.SetItem(null);
        }
        else
        {
            switch (SelectItem_N.Instance.Place)
            {
                case ItemPlaceType.IPT_HotKeyBar:
                    if (grid.Item == null)
                    {
                        int selectIndex = SelectItem_N.Instance.Index + GetCurPageIndex();
                        int gridIndex = grid.ItemIndex + GetCurPageIndex();
                        mCutSlotList.PutItem(mCutSlotList.GetItem(selectIndex), gridIndex);
                        mCutSlotList.PutItem(null, selectIndex);
                    }
                    else
                    {
                        int selectIndex = SelectItem_N.Instance.Index + GetCurPageIndex();
                        ShortCutItem selectedCut = mCutSlotList.GetItem(selectIndex);
                        int gridIndex = grid.ItemIndex + GetCurPageIndex();
                        ShortCutItem gridCut = mCutSlotList.GetItem(gridIndex);

                        if (selectedCut != null)
                            mCutSlotList.PutItem(selectedCut, gridIndex);

                        mCutSlotList.PutItem(gridCut, selectIndex);
                    }
                    SelectItem_N.Instance.SetItem(null);
                    break;
                case ItemPlaceType.IPT_Equipment:
                    break;
                case ItemPlaceType.IPT_Bag:
                    {
                        int pakageIndex = 0;
                        bool isMission = false;
                        if (null != GameUI.Instance.mItemPackageCtrl)
                        {
                            if (!GameUI.Instance.mItemPackageCtrl.isMission)
                                pakageIndex = ItemPackage.CodeIndex((ItemPackage.ESlotType)GameUI.Instance.mItemPackageCtrl.CurrentPickTab, SelectItem_N.Instance.Index);
                            else
                            {
                                pakageIndex = ItemPackage.CodeIndex(ItemPackage.ESlotType.Item, SelectItem_N.Instance.Index);
                                isMission = true;
                            }
                            if(null!= mPackageCmpt)
                                mPackageCmpt.PutItemToShortCutList(pakageIndex, grid.ItemIndex + GetCurPageIndex(), isMission);
                        }
                        SelectItem_N.Instance.SetItem(null);
                    }
                    break;
            }
        }
    }

    int GetCurPageIndex()
    {
        //log:lz-2016.05.04 第一个格子不计算
        return (mBoxBar.PageIndex - 1) * (mBoxBar.ItemCount - 1);
    }

    public void SetItemWithIndex(ItemObject grid, int index, bool fromHotBar = false)
    {
        mItems[index].SetItem(grid);
    }

    public void RemoveItemWithIndex(int index)
    {
        if (mCutSlotList == null)
            GetCutSlotList();

        if (mCutSlotList == null)
            return;
        //log: lz-2016.06.03 移除Item的时候是移除mCutSlotList的数据，而index是grid的index，需要加上页数
        index += this.GetCurPageIndex();

        if (GameConfig.IsMultiMode)
        {
            PlayerNetwork.mainPlayer.RequestSetShortcuts(-1, index, -1, ItemPlaceType.IPT_Null);
        }
        else
        {
            mCutSlotList.PutItem(null, index);
        }
    }

    public void BtnBuildOnClick()
    {
        if (Pathea.SingleGameStory.curType == Pathea.SingleGameStory.StoryScene.MainLand
                    || Pathea.SingleGameStory.curType == Pathea.SingleGameStory.StoryScene.TrainingShip)
            GameUI.Instance.mBuildBlock.EnterBuildMode();
        else
            new PeTipMsg("[C8C800]" + PELocalization.GetString(82209004), PeTipMsg.EMsgLevel.Norm, PeTipMsg.EMsgType.Stroy);
    }

    public void OnHealthBtnClick()
    {
        GameUI.Instance.mUIPlayerInfoCtrl.Show();
    }

    void OnShortCutUpdate()
    {
        if (mCutSlotList != null)
        {
            int starIndex = this.GetCurPageIndex();
            ItemObject Iobj;
            for (int i = 1; i <mBoxBar.ItemCount; i++)
            {
                //log:lz-2016.05.04 第一个格子不计算，数据下标前推
                ShortCutItem item = mCutSlotList.GetItem(starIndex + (i-1));
                mItems[i].SetItem(item);

                Iobj = mCutSlotList.GetItemObj(starIndex + (i-1));
                mItems[i].SetDurabilityBg(Iobj);
            }
        }
    }

    void UpdateShortCut()
    {
        if (mCutSlotList == null)
            GetCutSlotList();
    }

    void UpdataGunNum()
    {

        if (UIDrivingCtrl.Instance != null && UIDrivingCtrl.Instance.IsShow)
        {
            mGun_6.SetActive(false);
            return;
        }


        int Value = 0;
        int Size = 0;

        if (peGun != null)
        {
            Value = (int)peGun.magazineValue;
            Size = (int)peGun.magazineSize;
            switch (peGun.m_AmmoType)
            {
                case AmmoType.Bullet:
                    {
                        mEngun.SetActive(false);
                        mgun.SetActive(true);
                        mBow.SetActive(false);
                        if (null != PeCreature.Instance.mainPlayer)
                        {
                            int packageCount = PeCreature.Instance.mainPlayer.packageCmpt.GetItemCount(peGun.curItemID);
                            if (Size == 99999)
                                mGunLabel.text = string.Format("{0} ({1})",Value, packageCount);
                            else
                                mGunLabel.text = string.Format("{0}/{1} ({2})", Value, Size, packageCount);
                        }
                    }
                    break;

                case AmmoType.Energy:
                    {
                        mEngun.SetActive(true);
                        mgun.SetActive(false);
                        mBow.SetActive(false);
                        if (Size == 99999)
                            mGunLabel.text = Value.ToString();
                        else
                            mGunLabel.text = Value.ToString() + "/" + Size.ToString();
                    }
                    break;
            }
            
        }
        else if (peBow != null && null != PeCreature.Instance.mainPlayer)
        {
            mEngun.SetActive(false);
            mgun.SetActive(false);
            mBow.SetActive(true);
            Size = PeCreature.Instance.mainPlayer.packageCmpt.GetItemCount(peBow.curItemID);
            mGunLabel.text = Size.ToString();
        }
        else
        {
            mGun_6.SetActive(false);
            return;
        }

        //mGunLabel.text = Value.ToString() + "/" + Size.ToString();
        //mGunLabel.text = mGunLabel.text.Replace("99999", "-");
        //        if (Size == 99999)
        //        {
        //            mGunLabel.text = Value.ToString() + "/" + "-";
        //        }
        mGunLabel.gameObject.SetActive(true);
        mGun_6.SetActive(true);

    }

    void UpdataJetPack()
    {
        if (null != equip)
            mScJect_3.sliderValue = equip.jetPackEnMax > 0 ? equip.jetPackEnCurrent / equip.jetPackEnMax : 0;
        else
            mScJect_3.sliderValue = 0;
    }


    #region TWEEN

    public WhiteCat.TweenInterpolator tweener;

    public delegate void DTweenFinished(bool forward);
    public event DTweenFinished onTweenFinished;

    public void PlayTween(bool forward)
    {
        tweener.isPlaying = true;

        if (forward)
        {
            if (tweener.speed < 0)
                tweener.ReverseSpeed();
        }
        else
        {
            if (tweener.speed > 0)
                tweener.ReverseSpeed();
        }

    }

    public void OnTweenFinish(bool forward)
    {
        if (onTweenFinished != null)
            onTweenFinished(true);
    }


    #endregion

    #region buff

    public UIGrid m_BuffGrid;
    public CSUI_BuffItem m_BuffPrefab;

    private List<CSUI_BuffItem> m_BuffList = new List<CSUI_BuffItem>();
    private List<string> m_IconList = new List<string>();

    public void AddBuffShow(string _icon, string _describe)//添加一个buff图标
    {

        if (m_IconList.Contains(_icon))//如果已经有这种图标，就返回
        {
            m_IconList.Add(_icon);
            return;
        }
        else                          //如果没有，就生成
            m_IconList.Add(_icon);

        CSUI_BuffItem grid = Instantiate(m_BuffPrefab) as CSUI_BuffItem;
        if (!grid.gameObject.activeSelf)
            grid.gameObject.SetActive(true);
        grid.transform.parent = m_BuffGrid.transform;
        CSUtils.ResetLoacalTransform(grid.transform);
        grid.SetInfo(_icon, _describe);
        m_BuffList.Add(grid);
        m_Reposition = true;
    }

    public void DeleteBuffShow(string _icon)   //删除一个buff图标
    {
        List<CSUI_BuffItem> list = m_BuffList.FindAll(i => i._icon == _icon);

        if (list == null)
            return;
        if (list.Count == 1)
        {
            Destroy(list[0].gameObject);
            m_BuffList.Remove(list[0]);
            m_IconList.Remove(_icon);
        }
        else if (list.Count > 1)
        {
            m_IconList.Remove(_icon);
        }

        m_Reposition = true;

    }

    bool m_Reposition = false;

    void UpdateReposition()
    {
        if (m_Reposition)
        {
            m_Reposition = false;
            m_BuffGrid.repositionNow = true;
        }
    }

    #endregion

    #region Tutorial 

    [SerializeField]
    private Transform m_QuickBarTutorialParent;
    [SerializeField]
    private UIWndTutorialTip_N m_QuickBarTutorialPrefab;
    [SerializeField]
    private Transform m_FullQuickBarTutorialParent;
    [SerializeField]
    private UIWndTutorialTip_N m_FullQuickBarTutorialPrefab;

    /// <summary>lz-2016.11.07 只显示快捷栏的Tutorial</summary>
    public void ShowQuickBarTutorial()
    {
        if (PeGameMgr.IsTutorial)
        {
            GameObject go = Instantiate(m_QuickBarTutorialPrefab.gameObject);
            go.transform.parent = m_QuickBarTutorialParent;
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.transform.localRotation = Quaternion.identity;
        }
    }

    /// <summary>lz-2016.11.07 显示整个快捷栏窗口的Tutorial</summary>
    public void ShowFullQuickBarTutorial()
    {
        if (PeGameMgr.IsTutorial)
        {
            GameObject go = Instantiate(m_FullQuickBarTutorialPrefab.gameObject);
            go.transform.parent = m_FullQuickBarTutorialParent;
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.transform.localRotation = Quaternion.identity;
        }
    }

    #endregion
}
