using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Pathea;
using ItemAsset;
using ItemAsset.SlotListHelper;

public partial class UIServantWnd : UIBaseWnd
{
    [SerializeField]
    UIPlayerInfoScall mScall;
    [SerializeField]
    UITexture mEqTex;
    [SerializeField]
    Grid_N mGridPrefab;
    [SerializeField]
    Transform mTsSkillGrids;
    [SerializeField]
    Transform mTsInteractionGrids;
    [SerializeField]
    Transform mTsInteraction2Grids;
    [SerializeField]
    Transform mTsPrivateItemGrids;
    [SerializeField]
    Transform mTsEquipmentGrids;
    [SerializeField]
    GameObject mPageInfo;
    [SerializeField]
    GameObject mPageInventory;
    [SerializeField]
    UILabel mLbName;
    [SerializeField]
    UISprite mSprSex;
    [SerializeField]
    UILabel mLbHealth;
    [SerializeField]
    UISlider mSdHealth;
    [SerializeField]
    UILabel mLbHealthBuff;
    //[SerializeField]
    //UILabel mLbStamina;
    //[SerializeField]
    //UISlider mSdStamina;
    //[SerializeField]
    //UILabel mLbStaminaBuff;
    [SerializeField]
    UILabel mLbHunger;
    [SerializeField]
    UISlider mSdHunger;
    [SerializeField]
    UILabel mLbHungerBuff;
    [SerializeField]
    UILabel mLbComfort;
    [SerializeField]
    UISlider mSdComfort;
    [SerializeField]
    UILabel mLbComfortBuff;
    //[SerializeField]
    //UILabel mLbOxygen;
    //[SerializeField]
    //UISlider mSdOxygen;
    //[SerializeField]
    //UILabel mLbOxygenBuff;
    [SerializeField]
    UILabel mLbShield;
    [SerializeField]
    UISlider mSdShield;
    [SerializeField]
    UILabel mLbShieldBuff;
    [SerializeField]
    UILabel mLbEnergy;
    [SerializeField]
    UISlider mSdEnergy;
    [SerializeField]
    UILabel mLbEnergyBuff;
    [SerializeField]
    UILabel mLbAttack;
    [SerializeField]
    UILabel mLbAttackBuff;
    [SerializeField]
    UILabel mLbDefense;
    [SerializeField]
    UILabel mLbDefenseBuff;
    [SerializeField]
    UILabel mLbMoney;
    [SerializeField]
    UILabel mLbPrivatePageText;
    [SerializeField]
    UILabel mLbNextServant;
    [SerializeField]
    UICheckbox mCkInventory;


    [SerializeField]
    GameObject mAttackBtn;
    [SerializeField]
    GameObject mDefenceBtn;
    [SerializeField]
    GameObject mRestBtn;
    [SerializeField]
    GameObject mStayBtn;

    //[SerializeField]
    //N_ImageButton mStayBtn;
    [SerializeField]
    N_ImageButton mWorkBtn;
    [SerializeField]
    N_ImageButton mCallBtn;
    [SerializeField]
    N_ImageButton mFreeBtn;


    //int mTabelIndex = 0;
    int mSkillGridCount = 5;
    int mInteractionGridCount = 15;
    int mInteraction2GridCount = 10;
    int mPrivateItemGridCount = 10;

    //lz-2016.10.19 属性封装
    AttributeInfo AttrHpInfo = new AttributeInfo(AttribType.Hp, AttribType.HpMax);
    //AttributeInfo AttrStaminaInfo = new AttributeInfo(AttribType.Stamina, AttribType.StaminaMax);
    AttributeInfo AttrHungerInfo = new AttributeInfo(AttribType.Hunger, AttribType.HungerMax);
    AttributeInfo AttrComfortInfo = new AttributeInfo(AttribType.Comfort, AttribType.ComfortMax);
    //AttributeInfo AttrOxygenInfo = new AttributeInfo(AttribType.Oxygen, AttribType.OxygenMax);
    AttributeInfo AttrShieldInfo = new AttributeInfo(AttribType.Shield, AttribType.ShieldMax);
    AttributeInfo AttrEnergyInfo = new AttributeInfo(AttribType.Energy, AttribType.EnergyMax);
    AttributeInfo AttrAtkInfo = new AttributeInfo(AttribType.Atk, AttribType.Atk);
    AttributeInfo AttrDefInfo = new AttributeInfo(AttribType.Def, AttribType.Def);
    [SerializeField]
    UILabel m_BuffStrsLb;
    //private string m_BuffStrFormat = "+{0}";

    List<Grid_N> mSkillList;
    List<Grid_N> mInteractionList;
    List<Grid_N> mInteraction2List;
    List<Grid_N> mPrivateList;
    List<Grid_N> mEquipmentList;

    //	TakeEquipmentPhoto		mTakeEqPhotho;
    PeViewController _viewController;
    GameObject _viewModel;

    #region Servant

    public enum ServantIndex
    {
        Index_0 = 0,
        Index_1,
        Max
    }
    public ServantIndex mCurrentIndex { get; set; }

    //PlayerPackageCmpt playerPackageCmpt = null;
    PeEntity m_Servant = null;
    PeEntity servant
    {
        get { return m_Servant; }
        set
        {
            m_Servant = value;
            AttrHpInfo.SetEntity(value);
            //AttrStaminaInfo.SetEntity(value);
            AttrHungerInfo.SetEntity(value);
            AttrComfortInfo.SetEntity(value);
            //AttrOxygenInfo.SetEntity(value);
            AttrShieldInfo.SetEntity(value);
            AttrEnergyInfo.SetEntity(value);
            AttrAtkInfo.SetEntity(value);
            AttrDefInfo.SetEntity(value);
        }
    }

    ServantLeaderCmpt leaderCmpt = null;

    NpcCmpt m_NpcCmpt = null;

    NpcCmpt npcCmpt
    {
        get { return m_NpcCmpt; }
        set
        {
            //lz-2016.10.14 避免上一个m_Npc身上的事件没有移除
            if (null != m_NpcCmpt&&null!= m_NpcCmpt.Entity)
            {
                AbnormalConditionCmpt accOld = m_NpcCmpt.Entity.GetCmpt<AbnormalConditionCmpt>();
                if (accOld != null)
                {
                    accOld.evtStart -= AddNpcAbnormal;
                    accOld.evtEnd -= RemoveNpcAbnormal;
                }
            }

            m_NpcCmpt = value;

            //lz-2016.10.14 重新添加事件
            if (null != m_NpcCmpt && null != m_NpcCmpt.Entity)
            {
                AbnormalConditionCmpt accNew = m_NpcCmpt.Entity.GetCmpt<AbnormalConditionCmpt>();
                if (accNew != null)
                {
                    accNew.evtStart += AddNpcAbnormal;
                    accNew.evtEnd += RemoveNpcAbnormal;
                }
            }
        }
    }

    BiologyViewCmpt viewCmpt = null;
    CommonCmpt commonCmpt = null;
    EquipmentCmpt m_EquipmentCmpt = null;
    EquipmentCmpt equipmentCmpt
    {
        get { return m_EquipmentCmpt; }
        set
        {
            if (m_EquipmentCmpt != null)
            {
                m_EquipmentCmpt.onSuitSetChange -= UpdateSuitBuffTips;
            }
            m_EquipmentCmpt = value;
            if (m_EquipmentCmpt != null)
            {
                m_EquipmentCmpt.onSuitSetChange += UpdateSuitBuffTips;
                UpdateSuitBuffTips(m_EquipmentCmpt.matchDatas);
            }
            else
            {
                UpdateSuitBuffTips(null);
            }
        }
    }
    NpcPackageCmpt packageCmpt = null;
    EntityInfoCmpt entityInfoCmpt = null;

    SlotList mInteractionPackage = null;
    SlotList mInteraction2Package = null;
    SlotList mPrivatePakge = null;

    public bool ServantIsNotNull { get { return npcCmpt != null; } }

    void GetServentCmpt()
    {
        if (leaderCmpt != null)
        {
            NpcCmpt cmpt = leaderCmpt.GetServant((int)mCurrentIndex);
            if (cmpt != null && cmpt != npcCmpt)
            {
                viewCmpt = cmpt.Entity.biologyViewCmpt;
                commonCmpt = cmpt.Entity.commonCmpt;
                packageCmpt = cmpt.GetComponent<NpcPackageCmpt>();
                GetNpcPakgeSlotList();
                entityInfoCmpt = cmpt.Entity.enityInfoCmpt;

                // register changeEvent
                if (equipmentCmpt != null)
                    equipmentCmpt.changeEventor.Unsubscribe(EquipmentChangeEvent);
                equipmentCmpt = cmpt.Entity.equipmentCmpt;
                equipmentCmpt.changeEventor.Subscribe(EquipmentChangeEvent);

            }
            npcCmpt = cmpt;
            CheckWhtherCanGet();
            servant = (npcCmpt != null) ? npcCmpt.Entity : null;
        }
        else
        {
            npcCmpt = null;
            CheckWhtherCanGet();
        }

        if (npcCmpt == null)
        {
            viewCmpt = null;
            commonCmpt = null;
            equipmentCmpt = null;
            packageCmpt = null;
            entityInfoCmpt = null;

            mInteractionPackage = null;
            mPrivatePakge = null;

            mSprSex.spriteName = "null";
            ClearEqList();
            ClearNpcPackage();
        }
        mEqTex.enabled = (npcCmpt != null);
    }

    void CheckWhtherCanGet()
    {
        //if (npcCmpt != null)
        //{
        //    if (npcCmpt.Npcskillcmpt.CurNpcAblitys.Count > 0)
        //    {
        //        foreach (NpcAbility abity in npcCmpt.Npcskillcmpt.CurNpcAblitys)
        //        {
        //            if (abity.IsGetItem())
        //            {
        //                mWorkBtn.disable = false;
        //                mCallBtn.disable = false;
        //                break;
        //            }
        //            mWorkBtn.disable = true;
        //            mCallBtn.disable = true;
        //        }
        //    }
        //    else
        //    {
        //        mWorkBtn.disable = true;
        //        mCallBtn.disable = true;
        //    }

        //}
        //else
        //{
        //    mWorkBtn.disable = false;
        //    mCallBtn.disable = false;
        //}
    }

    public void Refresh()
    {
        ServantLeaderCmpt loader = GameUI.Instance.mMainPlayer.GetCmpt<ServantLeaderCmpt>();
        if (loader != leaderCmpt)
        {
            if (leaderCmpt != null)
                leaderCmpt.changeEventor.Unsubscribe(ServentChange);
            leaderCmpt = loader;
            leaderCmpt.changeEventor.Subscribe(ServentChange);
            //playerPackageCmpt = GameUI.Instance.mMainPlayer.GetCmpt<PlayerPackageCmpt>();
        }
        GetServentCmpt();
        ReflashSkill();
        RefreshNpcAbnormal();
        if (npcCmpt == null)
            return;
        if (equipmentCmpt != null)
            UpdateEquipAndTex();
        if (packageCmpt != null)
            Reflashpackage();

    }

    public NpcCmpt GetNeedReviveServant()
    {
        ServantLeaderCmpt leader = GameUI.Instance.mMainPlayer.GetCmpt<ServantLeaderCmpt>();
        NpcCmpt mNpc = leader.GetServant((int)mCurrentIndex);
        if (mNpc == null)
            return null;
        if (mNpc.Alive.isDead)
            return mNpc;
        return null;
    }

    void ServentChange(object sender, ServantLeaderCmpt.ServantChanged arg)
    {
        Refresh();
    }

    #endregion


    #region WndFunc


    public override void Show()
    {
        base.Show();
        Refresh();
        if (_viewModel != null)
            _viewModel.gameObject.SetActive(true);
        _viewController.gameObject.SetActive(true);
    }

    public void Show(ServantIndex index)
    {
        mCurrentIndex = index;
        Show();
    }

    protected override void OnHide()
    {
        base.OnHide();
        if (_viewModel != null)
            _viewModel.gameObject.SetActive(false);
        _viewController.gameObject.SetActive(false);

    }

    protected override void InitWindow()
    {
        mCurrentIndex = ServantIndex.Index_0;
        base.InitWindow();
        base.SelfWndType = UIEnum.WndType.Servant;
    }

    public override void OnCreate()
    {
        base.OnCreate();
        _viewController = PeViewStudio.CreateViewController(ViewControllerParam.DefaultCharacter);
        _viewController.SetLocalPos(PeViewStudio.s_ViewPos);
		_viewController.name = "ViewController_Servant";
        _viewController.gameObject.SetActive(false);
        InitGrid();
    }


    void InitGrid()
    {
        mSkillList = new List<Grid_N>();
        for (int i = 0; i < mSkillGridCount; i++)
        {
            mSkillList.Add(Instantiate(mGridPrefab) as Grid_N);
            mSkillList[i].gameObject.name = "SkillGrid" + i;
            mSkillList[i].transform.parent = mTsSkillGrids;
            mSkillList[i].transform.localPosition = new Vector3(35 + i * 60, -38, 0);
            mSkillList[i].transform.localRotation = Quaternion.identity;
            mSkillList[i].transform.localScale = Vector3.one;
        }

        mInteractionList = new List<Grid_N>();
        for (int i = 0; i < mInteractionGridCount; i++)
        {
            mInteractionList.Add(Instantiate(mGridPrefab) as Grid_N);
            mInteractionList[i].gameObject.name = "Interaction" + i;
            mInteractionList[i].transform.parent = mTsInteractionGrids;
            //if (i < 5)
            //    mInteractionList[i].transform.localPosition = new Vector3(i * 60, 0, 0);
            //else
            //    mInteractionList[i].transform.localPosition = new Vector3((i - 5) * 60, -55, 0);
            mInteractionList[i].transform.localPosition = new Vector3(i % 5 * 60, -((int)i / 5) * 54, 0);
            mInteractionList[i].transform.localRotation = Quaternion.identity;
            mInteractionList[i].transform.localScale = Vector3.one;
            mInteractionList[i].SetItemPlace(ItemPlaceType.IPT_ServantInteraction, i);

            mInteractionList[i].onDropItem += OnDropItem_InterPackage;
            mInteractionList[i].onLeftMouseClicked += OnLeftMouseCliked_InterPackage;
            mInteractionList[i].onRightMouseClicked += OnRightMouseCliked_InterPackage;
        }

        mInteraction2List = new List<Grid_N>();
        for (int i = 0; i < mInteraction2GridCount; i++)
        {
            mInteraction2List.Add(Instantiate(mGridPrefab) as Grid_N);
            mInteraction2List[i].gameObject.name = "Interaction" + i;
            mInteraction2List[i].transform.parent = mTsInteraction2Grids;
            //if (i < 5)
            //    mInteractionList[i].transform.localPosition = new Vector3(i * 60, 0, 0);
            //else
            //    mInteractionList[i].transform.localPosition = new Vector3((i - 5) * 60, -55, 0);
            mInteraction2List[i].transform.localPosition = new Vector3(i % 5 * 60, -((int)i / 5) * 54, 0);
            mInteraction2List[i].transform.localRotation = Quaternion.identity;
            mInteraction2List[i].transform.localScale = Vector3.one;
            mInteraction2List[i].SetItemPlace(ItemPlaceType.IPT_ServantInteraction2, i);

            mInteraction2List[i].onDropItem += OnDropItem_InterPackage2;
            mInteraction2List[i].onLeftMouseClicked += OnLeftMouseCliked_InterPackage2;
            mInteraction2List[i].onRightMouseClicked += OnRightMouseCliked_InterPackage2;
        }

        mPrivateList = new List<Grid_N>();
        for (int i = 0; i < mPrivateItemGridCount; i++)
        {
            mPrivateList.Add(Instantiate(mGridPrefab) as Grid_N);
            mPrivateList[i].gameObject.name = "PrivateItem" + i;
            mPrivateList[i].transform.parent = mTsPrivateItemGrids;
            if (i < 5)
                mPrivateList[i].transform.localPosition = new Vector3(i * 60, 0, 0);
            else if (i < 10)
                mPrivateList[i].transform.localPosition = new Vector3((i - 5) * 60, -55, 0);
            else
                mPrivateList[i].transform.localPosition = new Vector3((i - 10) * 60, -110, 0);
            mPrivateList[i].transform.localRotation = Quaternion.identity;
            mPrivateList[i].transform.localScale = Vector3.one;
        }

        // InitEquipments
        string[] mSpName = new string[10];
        mSpName[0] = "Icons_helmet";
        mSpName[1] = "Icons_coat";
        mSpName[2] = "Icons_gloves";
        mSpName[3] = "Icons_battery";
        mSpName[4] = "Icons_arms";
        mSpName[5] = "Icons_back";
        mSpName[6] = "Icons_pants";
        mSpName[7] = "Icons_shoes";
        mSpName[8] = "Icons_glass";
        mSpName[9] = "Icons_dun";

        mEquipmentList = new List<Grid_N>();
        for (int i = 0; i < 10; i++)
        {
            mEquipmentList.Add(Instantiate(mGridPrefab) as Grid_N);
            mEquipmentList[i].gameObject.name = "HotKey" + i;
            mEquipmentList[i].transform.parent = mTsEquipmentGrids;
            if (i < 5)
                mEquipmentList[i].transform.localPosition = new Vector3(-112, 176 - i % 5 * 58, -2);
            else
                mEquipmentList[i].transform.localPosition = new Vector3(112, 176 - i % 5 * 58, -2);
            mEquipmentList[i].transform.localRotation = Quaternion.identity;
            mEquipmentList[i].transform.localScale = Vector3.one;
            mEquipmentList[i].SetItemPlace(ItemPlaceType.IPT_ServantEqu, i);
            mEquipmentList[i].SetGridMask((GridMask)(1 << i));
            mEquipmentList[i].mScriptIco.spriteName = mSpName[i];
            mEquipmentList[i].mScriptIco.MakePixelPerfect();

            mEquipmentList[i].onDropItem += OnDropItem_Equip;
            mEquipmentList[i].onLeftMouseClicked += OnLeftMouseCliked_Equip;
            mEquipmentList[i].onRightMouseClicked += OnRightMouseCliked_Equip;
        }
    }

    public int GetCurServantId
    {
        get
        {
            return npcCmpt == null ? -1 : npcCmpt.Entity.Id;
        }
    }

    /// <summary> 获取EquipmentCmpt</summary>
    public EquipmentCmpt GetCurServantEquipCmpt()
    {
        return (npcCmpt == null) ? null : npcCmpt.Entity.equipmentCmpt;
    }

    void Start()
    {
        // Test code
        if (!mInit)
            InitGrid();
    }

    void ClearSkilllist()
    {
        for (int i = 0; i < mSkillList.Count; i++)
        {
            mSkillList[i].SetSkill(0, i);
        }
    }

    void ReflashSkill()
    {
        if (npcCmpt == null || npcCmpt.Npcskillcmpt == null)
        {
            ClearSkilllist();
            return;
        }

        if (npcCmpt.Alive == null)
            return;

        int index = 0;
        ClearSkilllist();
        if (npcCmpt.Npcskillcmpt.CurNpcAblitys != null)
        {
            foreach (NpcAbility abity in npcCmpt.Npcskillcmpt.CurNpcAblitys)
            {
                if (abity != null)
                {
                    mSkillList[index].SetSkill(abity.skillId, index, npcCmpt.Alive, abity.icon, abity.desc);
                    index++;
                }
            }
        }

        //lz-2016.06.08 如果有采集技能，就激活Work按钮，没有就隐藏
        mWorkBtn.gameObject.SetActive(npcCmpt.Npcskillcmpt.HasCollectSkill());
    }

    //lz-2016.06.15 如果npc死亡，这几个按钮就不能操作
    void UpdateOpBtnState()
    {
        bool active = (null != npcCmpt && !npcCmpt.Entity.IsDeath());
        this.mFreeBtn.isEnabled = active;
        this.mCallBtn.isEnabled = active;
        if (this.mWorkBtn.gameObject.activeSelf)
        {
            this.mWorkBtn.isEnabled = active;
        }
    }

    void Update()
    {
        if (null == entityInfoCmpt)
        {
            mLbName.text = "--";
            mLbName.MakePixelPerfect();
        }
        else
        {
            mLbName.text = entityInfoCmpt.characterName.fullName;
            mLbName.MakePixelPerfect();
        }

        if (null == commonCmpt)
        {
            mSprSex.spriteName = "man";
            mSprSex.MakePixelPerfect();
        }
        else
        {
            mSprSex.spriteName = commonCmpt.sex == PeSex.Male ? "man" : "woman";
            mSprSex.MakePixelPerfect();
        }

        float cur, max;

        cur = AttrHpInfo.CurValue;
        max = AttrHpInfo.MaxValue;
        mLbHealth.text = AttrHpInfo.GetCur_MaxStr();
        mSdHealth.sliderValue = (max <= 0) ? 0 : Convert.ToSingle(cur) / max;
        mLbHealthBuff.text = AttrHpInfo.GetBuffStr();

        //lz-2016.11.1 npc不消耗体力和氧气，去除显示
        //cur = AttrStaminaInfo.CurValue;
        //max = AttrStaminaInfo.MaxValue;
        //mLbStamina.text = AttrStaminaInfo.GetCur_MaxStr();
        //mSdStamina.sliderValue = (max <= 0) ? 0 : Convert.ToSingle(cur) / max;
        //mLbStaminaBuff.text = AttrStaminaInfo.GetBuffStr();

        cur = AttrHungerInfo.CurValue;
        max = AttrHungerInfo.MaxValue;
        mLbHunger.text = AttrHungerInfo.GetCur_MaxStr();
        mSdHunger.sliderValue = (max <= 0) ? 0 : Convert.ToSingle(cur) / max;
        mLbHungerBuff.text = AttrHungerInfo.GetBuffStr();


        cur = AttrComfortInfo.CurValue;
        max = AttrComfortInfo.MaxValue;
        mLbComfort.text = AttrComfortInfo.GetCur_MaxStr();
        mSdComfort.sliderValue = (max <= 0) ? 0 : Convert.ToSingle(cur) / max;
        mLbComfortBuff.text = AttrComfortInfo.GetBuffStr();

        //cur = AttrOxygenInfo.CurValue;
        //max = AttrOxygenInfo.MaxValue;
        //mLbOxygen.text = AttrOxygenInfo.GetCur_MaxStr();
        //mSdOxygen.sliderValue = (max <= 0) ? 0 : Convert.ToSingle(cur) / max;
        //mLbOxygenBuff.text = AttrOxygenInfo.GetBuffStr();

        cur = AttrShieldInfo.CurValue;
        max = AttrShieldInfo.MaxValue;
        mLbShield.text = AttrShieldInfo.GetCur_MaxStr();
        mSdShield.sliderValue = (max <= 0) ? 0 : Convert.ToSingle(cur) / max;
        mLbShieldBuff.text = AttrShieldInfo.GetBuffStr();

        cur = AttrEnergyInfo.CurValue;
        max = AttrEnergyInfo.MaxValue;
        mLbEnergy.text = AttrEnergyInfo.GetCur_MaxStr();
        mSdEnergy.sliderValue = (max <= 0) ? 0 : Convert.ToSingle(cur) / max;
        mLbEnergyBuff.text = AttrEnergyInfo.GetBuffStr();

        cur = AttrAtkInfo.CurValue;
        mLbAttack.text = string.Format((AttrAtkInfo.BuffValue > 0 ? AttributeInfo.GreenColFormat : AttributeInfo.YellowColFomat), cur);
        //mLbAttackBuff.text = AttrAtkInfo.GetBuffStr();

        cur = AttrDefInfo.CurValue;
        mLbDefense.text = string.Format((AttrDefInfo.BuffValue > 0 ? AttributeInfo.GreenColFormat : AttributeInfo.YellowColFomat), cur);
        //mLbDefenseBuff.text = AttrDefInfo.GetBuffStr();

        this.UpdateOpBtnState();

        mSprSex.enabled = !(servant == null);

        if (packageCmpt != null)
        {
            mLbPrivatePageText.text = mPageIndex.ToString() + " / " + mMaxPageIndex.ToString();
            mLbMoney.text = packageCmpt.money.current.ToString();
        }
        else
        {
            mLbPrivatePageText.text = "0 / 0";
            mLbMoney.text = "--";
        }
        mLbNextServant.text = ((int)mCurrentIndex + 1).ToString() + "/" + ((int)ServantIndex.Max).ToString();

        ShowBattle();

    }
    #endregion

    #region uievent

    /// <summary>更新套装Buff提示</summary>
    void UpdateSuitBuffTips(List<SuitSetData.MatchData> datas)
    {
        string tips = "";
        if (null != datas && datas.Count > 0)
        {
            SuitSetData.MatchData data = new SuitSetData.MatchData();
            for (int i = 0; i < datas.Count; i++)
            {
                data = datas[i];
                if (null != data.tips && data.tips.Length > 0)
                {
                    for (int j = 0; j < data.tips.Length; j++)
                    {
                        if (0 != data.tips[j] && data.activeTipsIndex >= j)
                        {
                            tips += PELocalization.GetString(data.tips[j]) + "\n";
                        }
                    }
                }
            }
        }
        if (tips.Length > 0)
        {
            tips = tips.Substring(0, tips.Length - 1);
        }
        m_BuffStrsLb.text = tips;
        m_BuffStrsLb.MakePixelPerfect();
    }
    void BtnNextOnClick()
    {
        mCurrentIndex = (mCurrentIndex == ServantIndex.Index_0) ? ServantIndex.Index_1 : ServantIndex.Index_0;
        Refresh();
    }

    void BtnInfoOnClick(bool isActive)
    {
        if (isActive)
        {
            //mTabelIndex = 0;
            mPageInfo.SetActive(true);
            mPageInventory.SetActive(false);
            Refresh();
        }
    }

    void BtnInvetoryOnClick(bool isActive)
    {
        if (isActive)
        {
            //mTabelIndex = 1;
            mPageInfo.SetActive(false);
            mPageInventory.SetActive(true);
            Refresh();
        }
    }

    // NpcPackage privatePackage
    void BtnLeftOnClick()
    {
        if (mPageIndex > 1)
            mPageIndex -= 1;
        ReflashPrivatePackage();
    }
    void BtnRightOnClick()
    {

        if (mPageIndex < mMaxPageIndex)
            mPageIndex += 1;
        ReflashPrivatePackage();
    }

    void BtnTakeAllOnClick()
    {
        if (servant == null)
            return;

        if (PeGameMgr.IsMulti)
        {
            PlayerNetwork.mainPlayer.RequestGetAllItemFromNpc(servant.Id, 0);
            return;
        }

        PlayerPackageCmpt package = GameUI.Instance.mMainPlayer.GetCmpt<PlayerPackageCmpt>();
        List<ItemObject> itemList = mInteractionPackage.ToList();
        if (package.CanAddItemList(itemList))
        {
            package.AddItemList(itemList);
            mInteractionPackage.Clear();
        }
    }

    void BtnTakeAllInventory2OnClick()
    {
        if (servant == null)
            return;

        if (PeGameMgr.IsMulti)
        {
            PlayerNetwork.mainPlayer.RequestGetAllItemFromNpc(servant.Id, 1);
            return;
        }

        PlayerPackageCmpt package = GameUI.Instance.mMainPlayer.GetCmpt<PlayerPackageCmpt>();
        List<ItemObject> itemList = mInteraction2Package.ToList();
        if (package.CanAddItemList(itemList))
        {
            package.AddItemList(itemList);
            mInteraction2Package.Clear();
        }
    }

    void BtnResortInteractive1BoxOnClick()
    {
        if (servant == null)
            return;

        if (PeGameMgr.IsMulti)
        {
            PlayerNetwork.mainPlayer.RequestNpcPackageSort(servant.Id, 0);
            return;
        }

        this.ClearInteractionpackage();
        this.mInteractionPackage.Reduce();
        this.mInteractionPackage.Sort();
        for (int i = 0; i < mInteractionGridCount; i++)
        {
            if (mInteractionPackage == null || mInteractionPackage[i] == null)
            {
                mInteractionList[i].SetItem(null);
            }
            else
            {
                mInteractionList[i].SetItem(mInteractionPackage[i]);
            }
        }
    }

    void BtnResortInteractive2BoxOnClick()
    {
        if (servant == null)
            return;

        if (PeGameMgr.IsMulti)
        {
            PlayerNetwork.mainPlayer.RequestNpcPackageSort(servant.Id, 1);
            return;
        }

        this.ClearInteractionpackage2();
        this.mInteraction2Package.Reduce();
        this.mInteraction2Package.Sort();
        for (int i = 0; i < mInteraction2GridCount; i++)
        {
            if (mInteraction2Package == null || mInteraction2Package[i] == null)
            {
                mInteraction2List[i].SetItem(null);
            }
            else
            {
                mInteraction2List[i].SetItem(mInteraction2Package[i]);
            }
        }
    }

    void BtnStayOnClick()
    {
        if (npcCmpt == null)
            return;

        npcCmpt.FollowerSentry = !npcCmpt.FollowerSentry;
    }

    void BtnWorkOnClick()
    {
        if (npcCmpt == null)
            return;

        npcCmpt.FollowerWork = true;
    }

    void BtnCallOnClick()
    {
        if (npcCmpt == null)
            return;

        npcCmpt.FollowerWork = false;
        npcCmpt.ServantCallBack();
    }

    void BtnFreeOnClick()
    {
        MessageBox_N.ShowYNBox(PELocalization.GetString(8000031), FreeServant);
    }
    void FreeServant()
    {
        if (npcCmpt == null || leaderCmpt == null)
            return;

        if (Pathea.PeGameMgr.IsMulti)
        {
            if (null != npcCmpt)
                leaderCmpt.OnFreeNpc(npcCmpt.Entity.Id);
        }
        else
        {
            leaderCmpt.RemoveServant(npcCmpt);
        }
    }


    //luwei

    void OnAttackChosebtn()
    {
        if (npcCmpt != null)
        {
            npcCmpt.Battle = ENpcBattle.Attack;
        }
    }

    void OnDefenceChoseBtn()
    {
        if (npcCmpt != null)
        {
            npcCmpt.Battle = ENpcBattle.Defence;
        }
    }

    void OnRestChoseBtn()
    {
        if (npcCmpt != null)
        {
            npcCmpt.Battle = ENpcBattle.Passive;
        }
    }

    void OnStayChoosebtn()
    {
        if (npcCmpt != null)
        {
            npcCmpt.Battle = ENpcBattle.Stay;
        }
    }

    void ShowBattle()
    {
        if (npcCmpt == null)
        {
            if (mAttackBtn.activeSelf)
                mAttackBtn.SetActive(false);
            if (mDefenceBtn.activeSelf)
                mDefenceBtn.SetActive(false);
            if (mRestBtn.activeSelf)
                mRestBtn.SetActive(false);
            if (mStayBtn.activeSelf)
                mStayBtn.SetActive(false);
            return;
        }

        switch (npcCmpt.Battle)
        {
            case ENpcBattle.Attack:
                {
                    mAttackBtn.SetActive(true);
                    mDefenceBtn.SetActive(false);
                    mRestBtn.SetActive(false);
                    mStayBtn.SetActive(false);
                }
                break;
            case ENpcBattle.Defence:
                {
                    mAttackBtn.SetActive(false);
                    mDefenceBtn.SetActive(true);
                    mRestBtn.SetActive(false);
                    mStayBtn.SetActive(false);
                }
                break;
            case ENpcBattle.Passive:
                {
                    mAttackBtn.SetActive(false);
                    mDefenceBtn.SetActive(false);
                    mRestBtn.SetActive(true);
                    mStayBtn.SetActive(false);
                }
                break;
            case ENpcBattle.Stay:
                {
                    mAttackBtn.SetActive(false);
                    mDefenceBtn.SetActive(false);
                    mRestBtn.SetActive(false);
                    mStayBtn.SetActive(true);
                }
                break;
            default:
                break;
        }

        return;
    }
    #endregion

    void OnGUI()
    {
        if (Pathea.EntityMgr.Instance == null || !Application.isEditor)
            return;

        if (GUI.Button(new Rect(300, 100, 90, 20), "AddServant"))
        {
            PeEntity entity = EntityMgr.Instance.Get(9008);
            if (entity == null)
            {
                Debug.Log("Get entity failed!");
                return;
            }
            NpcCmpt cmpt = entity.NpcCmpt;
            if (cmpt != null)
                CSMain.SetNpcFollower(entity);
        }
    }

    #region Abnormal

    [SerializeField]
    UIGrid mAbnormalGrid;
    [SerializeField]
    CSUI_BuffItem mAbnormalPrefab;

    bool mReposition = false;

    private List<CSUI_BuffItem> mAbnormalList = new List<CSUI_BuffItem>(1);

    void RefreshNpcAbnormal()
    {
        RemoveAllAbnormal();
        if (npcCmpt == null)
        {
            return;
        }

        List<PEAbnormalType> abList = npcCmpt.Entity.Alnormal.GetActiveAbnormalList();

        if (abList.Count == 0)
            return;

        for (int i = 0; i < abList.Count; i++)
        {
            AddNpcAbnormal(abList[i]);
        }
    }

    void AddNpcAbnormal(PEAbnormalType type)
    {
        AbnormalData data = AbnormalData.GetData(type);
        //lz-2016.08.26 异常状态是0的图标不显示
        if (null == data || data.iconName == "0")
            return;
        CSUI_BuffItem item = Instantiate(mAbnormalPrefab) as CSUI_BuffItem;
        if (!item.gameObject.activeSelf)
            item.gameObject.SetActive(true);
        item.transform.parent = mAbnormalGrid.transform;
        CSUtils.ResetLoacalTransform(item.transform);
        item.SetInfo(data.iconName, data.description);
        mAbnormalList.Add(item);
        mReposition = true;
    }

    void RemoveNpcAbnormal(PEAbnormalType type)
    {
        AbnormalData data = AbnormalData.GetData(type);
        //lz-2016.08.26 异常状态是0的图标不显示
        if (null == data || data.iconName == "0")
            return;
        CSUI_BuffItem item = mAbnormalList.Find(i => i._icon == data.iconName);
        if (item == null)
            return;
        Destroy(item.gameObject);
        mAbnormalList.Remove(item);
        mReposition = true;
    }

    void RemoveAllAbnormal()
    {
        if (mAbnormalList.Count == 0)
            return;
        for (int i = 0; i < mAbnormalList.Count; i++)
        {
            Destroy(mAbnormalList[i].gameObject);
            mAbnormalList.Remove(mAbnormalList[i]);
        }
    }

    void UpdateReposition()
    {
        if (mReposition)
        {
            mReposition = false;
            mAbnormalGrid.repositionNow = true;
        }
    }

    protected override void LateUpdate()
    {
        UpdateReposition();
    }
    #endregion

}
