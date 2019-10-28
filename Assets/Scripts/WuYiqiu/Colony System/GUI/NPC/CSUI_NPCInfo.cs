using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using SkillAsset;
using System;
using Pathea;
using Pathea.PeEntityExt;
using ItemAsset.SlotListHelper;

public class CSUI_NPCInfo : MonoBehaviour
{
    #region UI_WIDGET

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
    Transform m_Interaction1Root;
    [SerializeField]
    Transform m_Interaction2Root;
    [SerializeField]
    Transform m_PrivateRoot;
    [SerializeField]
    UIGrid m_SkillRoot;
    [SerializeField]
    int m_Interaction1Count = 15;
    [SerializeField]
    int m_Interaction2Count = 10;

    // NPC InfoPage
    [SerializeField]
    GameObject m_InfoPage;
    [SerializeField]
    GameObject m_InventoryPage;
    [SerializeField]
    GameObject m_WorkPage;
    [SerializeField]
    N_ImageButton m_CallBtn;

    public UICheckbox m_InfoCk;
    public UICheckbox m_InventoryCk;
    public UICheckbox m_WorkCk;


    #endregion
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
    //private string m_BuffStrFormat = "+{0}";

    public Grid_N m_GridPrefab;

    private CSPersonnel m_RefNpc;
    public CSPersonnel RefNpc
    {
        get { return m_RefNpc; }
        set
        {
            ///lz-2016.10.14 避免上一个m_Npc身上的事件没有移除
            if (null != m_RefNpc&& null != m_RefNpc.m_Npc) //lz-2016.10.23 错误 #5094 空对象
            {
                AbnormalConditionCmpt accOld = m_RefNpc.m_Npc.GetCmpt<AbnormalConditionCmpt>();
                if (accOld != null)
                {
                    accOld.evtStart -= AddNpcAbnormal;
                    accOld.evtEnd -= RemoveNpcAbnormal;
                }
            }

            m_RefNpc = value;

            //lz-2016.10.14 重新添加事件
            PeEntity npcEntity=null;
            if (null != m_RefNpc&& null!=m_RefNpc.m_Npc)
            {
                npcEntity = m_RefNpc.m_Npc;
                AbnormalConditionCmpt accNew = m_RefNpc.m_Npc.GetCmpt<AbnormalConditionCmpt>();
                if (accNew != null)
                {
                    accNew.evtStart += AddNpcAbnormal;
                    accNew.evtEnd += RemoveNpcAbnormal;
                }
            }
            AttrHpInfo.SetEntity(npcEntity);
            //AttrStaminaInfo.SetEntity(npcEntity);
            AttrHungerInfo.SetEntity(npcEntity);
            AttrComfortInfo.SetEntity(npcEntity);
            //AttrOxygenInfo.SetEntity(npcEntity);
            AttrShieldInfo.SetEntity(npcEntity);
            AttrEnergyInfo.SetEntity(npcEntity);
            AttrAtkInfo.SetEntity(npcEntity);
            AttrDefInfo.SetEntity(npcEntity);

            UpdateItemGrid();
            RefreshNpcAbnormal();
            UpdateSkills();
        }
    }

    private List<Grid_N> m_InteractionGrids1 = new List<Grid_N>(); //lz-2016.09.02 把包裹分成两个，和仆从界面一样
    private List<Grid_N> m_InteractionGrids2 = new List<Grid_N>();
    private List<Grid_N> m_PrivateGrids = new List<Grid_N>();
    private List<Grid_N> m_SkillGrids = new List<Grid_N>();

    SlotList mInteractionPackage1 = null;
    SlotList mInteractionPackage2 = null;
    SlotList mPrivatePakge = null;

    NpcPackageCmpt packageCmpt = null;
    NpcCmpt cmpt = null;
    // Use this for initialization
    void Start()
    {
        // Create Interaction Item Grid
        for (int i = 0; i < m_Interaction1Count; i++)
        {
            Grid_N grid = Instantiate(m_GridPrefab) as Grid_N;
            grid.transform.parent = m_Interaction1Root.transform;
            grid.transform.localPosition = Vector3.zero;
            grid.transform.localRotation = Quaternion.identity;
            grid.transform.localScale = Vector3.one;
            m_InteractionGrids1.Add(grid);
           
            m_InteractionGrids1[i].transform.localPosition = new Vector3(i % 5 * 60, -((int)i / 5) * 54, 0);
            grid.SetItemPlace(ItemPlaceType.IPT_ColonyServantInteractionPersonel, i);

            m_InteractionGrids1[i].onDropItem += OnDropItem_InterPackage;
            m_InteractionGrids1[i].onLeftMouseClicked += OnLeftMouseCliked_InterPackage;
            m_InteractionGrids1[i].onRightMouseClicked += OnRightMouseCliked_InterPackage;
        }

        for (int i = 0; i < m_Interaction2Count; i++)
        {
            Grid_N grid = Instantiate(m_GridPrefab) as Grid_N;
            grid.transform.parent = m_Interaction2Root.transform;
            grid.transform.localPosition = Vector3.zero;
            grid.transform.localRotation = Quaternion.identity;
            grid.transform.localScale = Vector3.one;
            m_InteractionGrids2.Add(grid);
            
            m_InteractionGrids2[i].transform.localPosition = new Vector3(i % 5 * 60, -((int)i / 5) * 54, 0);
            grid.SetItemPlace(ItemPlaceType.IPT_ColonyServantInteraction2Personel, i);

            m_InteractionGrids2[i].onDropItem += OnDropItem_InterPackage2;
            m_InteractionGrids2[i].onLeftMouseClicked += OnLeftMouseCliked_InterPackage2;
            m_InteractionGrids2[i].onRightMouseClicked += OnRightMouseCliked_InterPackage2;
        }

        //Creat Private Item Grid
        for (int i = 0; i < 10; i++)
        {
            Grid_N grid = Instantiate(m_GridPrefab) as Grid_N;
            grid.transform.parent = m_PrivateRoot.transform;
            grid.transform.localPosition = Vector3.zero;
            grid.transform.localRotation = Quaternion.identity;
            grid.transform.localScale = Vector3.one;
            m_PrivateGrids.Add(grid);
            if (i < 5)
                m_PrivateGrids[i].transform.localPosition = new Vector3(i * 60, 0, 0);
            else
                m_PrivateGrids[i].transform.localPosition = new Vector3((i - 5) * 60, -55, 0);
            grid.SetItemPlace(ItemPlaceType.IPT_ColonyServantInteractionPersonel, i);


        }

        // Create skill Grid
        for (int i = 0; i < 5; i++)
        {
            Grid_N grid = Instantiate(m_GridPrefab) as Grid_N;
            grid.transform.parent = m_SkillRoot.transform;
            grid.transform.localPosition = Vector3.zero;
            grid.transform.localRotation = Quaternion.identity;
            grid.transform.localScale = Vector3.one;

            grid.ItemIndex = i;
            m_SkillGrids.Add(grid);
        }
        m_SkillRoot.repositionNow = true;

        UIEventListener.Get(this.m_CallBtn.gameObject).onClick = this.OnCallBtn;
    }

    // Update is called once per frame
    void Update()
    {
		mLbName.text = (null==m_RefNpc)?"--":m_RefNpc.FullName;
        mSprSex.spriteName = (null == m_RefNpc) ? "man" : (m_RefNpc.Sex== PeSex.Male)? "man" : "woman";
        mSprSex.MakePixelPerfect();

        float cur, max;

        cur = AttrHpInfo.CurValue;
        max = AttrHpInfo.MaxValue;
        mLbHealth.text = AttrHpInfo.GetCur_MaxStr();
        mSdHealth.sliderValue = (max <= 0) ? 0 : Convert.ToSingle(cur) / max;
        mLbHealthBuff.text = AttrHpInfo.GetBuffStr();


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
        mLbAttack.text = string.Format((AttrAtkInfo.BuffValue > 0 ? AttributeInfo.GreenColFormat : AttributeInfo.YellowColFomat),cur);
        //mLbAttackBuff.text = AttrAtkInfo.GetBuffStr();

        cur = AttrDefInfo.CurValue;
        mLbDefense.text = string.Format((AttrDefInfo.BuffValue > 0 ? AttributeInfo.GreenColFormat : AttributeInfo.YellowColFomat), cur);
        //mLbDefenseBuff.text = AttrDefInfo.GetBuffStr();
    }

    void UpdateItemGrid()
    {
        if (m_InteractionGrids1.Count == 0||m_InteractionGrids2.Count==0 || m_PrivateGrids.Count == 0)//格子数量为0
            return;

        //先清空格子
        for (int i = 0; i < m_Interaction1Count; i++)
            m_InteractionGrids1[i].SetItem(null);

        for (int i = 0; i < m_Interaction2Count; i++)
            m_InteractionGrids2[i].SetItem(null);

        for (int i = 0; i < 10; i++)
            m_PrivateGrids[i].SetItem(null);

        if (m_RefNpc == null)//当传进来的npc为空，则直接返回
            return;

        //lz-2016.11.22 错误 #6885 Crush bug
        if (m_RefNpc != null&& null!=m_RefNpc.m_Npc)
        {
            cmpt = m_RefNpc.m_Npc.GetCmpt<NpcCmpt>();
            if (null != cmpt)
            {
                packageCmpt = cmpt.GetComponent<NpcPackageCmpt>();
                if (null != packageCmpt)
                {
                    if (mInteractionPackage1 != null)
                        mInteractionPackage1.eventor.Unsubscribe(InteractionpackageChange);
                    mInteractionPackage1 = packageCmpt.GetSlotList();
                    mInteractionPackage1.eventor.Subscribe(InteractionpackageChange);

                    if (mInteractionPackage2 != null)
                        mInteractionPackage2.eventor.Unsubscribe(Interactionpackage2Change);
                    mInteractionPackage2 = packageCmpt.GetHandinList();
                    mInteractionPackage2.eventor.Subscribe(Interactionpackage2Change);

                    if (mPrivatePakge != null)
                        mPrivatePakge.eventor.Unsubscribe(PrivatepackageChange);
                    mPrivatePakge = packageCmpt.GetPrivateSlotList();
                    mPrivatePakge.eventor.Subscribe(PrivatepackageChange);
                }
            }
        }

        Reflashpackage();
    }

    void Reflashpackage()
    {
        ReflashInteractionpackage();
        ReflashInteractionpackage2();
        ReflashPrivatePackage();
    }

    void InteractionpackageChange(object sender, SlotList.ChangeEvent arg)
    {
        ReflashInteractionpackage();
    }
    void ReflashInteractionpackage()
    {
        ClearInteractionpackage1();
        for (int i = 0; i < m_InteractionGrids1.Count; i++)
        {
            if (mInteractionPackage1 == null)
            {
                m_InteractionGrids1[i].SetItem(null);
            }
            else
            {
                m_InteractionGrids1[i].SetItem(mInteractionPackage1[i]);
            }
        }
    }

    void Interactionpackage2Change(object sender, SlotList.ChangeEvent arg)
    {
        ReflashInteractionpackage2();
    }
    void ReflashInteractionpackage2()
    {
        ClearInteractionpackage2();
        for (int i = 0; i < m_InteractionGrids2.Count; i++)
        {
            if (mInteractionPackage2 == null)
            {
                m_InteractionGrids2[i].SetItem(null);
            }
            else
            {
                m_InteractionGrids2[i].SetItem(mInteractionPackage2[i]);
            }
        }
    }

    void ClearInteractionpackage1()
    {
        foreach (Grid_N item in m_InteractionGrids1)
            item.SetItem(null);
    }

    void ClearInteractionpackage2()
    {
        foreach (Grid_N item in m_InteractionGrids2)
            item.SetItem(null);
    }

    void PrivatepackageChange(object sender, SlotList.ChangeEvent arg)
    {
        ReflashPrivatePackage();
    }

    void ReflashPrivatePackage()
    {
        ClearPrivatePackage();

        for (int i = 0; i < m_PrivateGrids.Count; i++)
        {
            if (mPrivatePakge == null)
            {
                m_PrivateGrids[i].SetItem(null);
            }
            else
            {
                m_PrivateGrids[i].SetItem(mPrivatePakge[i]);
            }
        }
    }

    void ClearPrivatePackage()
    {
        foreach (Grid_N item in m_PrivateGrids)
            item.SetItem(null);
    }

    public bool SetInteractionItemWithIndex(ItemObject itemObj, int index = -1)
    {

        if (index == -1)
            return mInteractionPackage1.Add(itemObj);
        else
        {
            if (index < 0 || index > mInteractionPackage1.Count)
                return false;
            if (mInteractionPackage1 != null)
                mInteractionPackage1[index] = itemObj;
        }
        return true;
    }

    public bool SetInteraction2ItemWithIndex(ItemObject itemObj, int index = -1)
    {

        if (index == -1)
            return mInteractionPackage2.Add(itemObj);
        else
        {
            if (index < 0 || index > mInteractionPackage2.Count)
                return false;
            if (mInteractionPackage2 != null)
                mInteractionPackage2[index] = itemObj;
        }
        return true;
    }

    #region btn event
    void BtnTakeAllOnClick()
    {
        if (m_RefNpc == null || mInteractionPackage1 == null)
            return;

        if (!m_RefNpc.IsRandomNpc)
            return;

        //lz-2016.09.2 以前没做网络模式
        if (PeGameMgr.IsMulti)
        {
            PlayerNetwork.mainPlayer.RequestGetAllItemFromNpc(m_RefNpc.NPC.Id, 0);
            return;
        }

        PlayerPackageCmpt package = GameUI.Instance.mMainPlayer.GetCmpt<PlayerPackageCmpt>();
        List<ItemObject> itemList = mInteractionPackage1.ToList();
        if (package.CanAddItemList(itemList))
        {
            package.AddItemList(itemList);
            mInteractionPackage1.Clear();
        }
    }


    void BtnTakeAllInventory2OnClick()
    {
        if (m_RefNpc == null || mInteractionPackage2 == null)
            return;

        if (PeGameMgr.IsMulti)
        {
            PlayerNetwork.mainPlayer.RequestGetAllItemFromNpc(m_RefNpc.NPC.Id, 1);
            return;
        }

        PlayerPackageCmpt package = GameUI.Instance.mMainPlayer.GetCmpt<PlayerPackageCmpt>();
        List<ItemObject> itemList = mInteractionPackage2.ToList();
        if (package.CanAddItemList(itemList))
        {
            package.AddItemList(itemList);
            mInteractionPackage2.Clear();
        }
    }

    void BtnResortInteractive1BoxOnClick()
    {
        if (m_RefNpc == null)
            return;

        if (PeGameMgr.IsMulti)
        {
            PlayerNetwork.mainPlayer.RequestNpcPackageSort(m_RefNpc.NPC.Id, 0);
            return;
        }

        this.ClearInteractionpackage1();
        this.mInteractionPackage1.Reduce();
        this.mInteractionPackage1.Sort();
        for (int i = 0; i < m_Interaction1Count; i++)
        {
            if (mInteractionPackage1 == null || mInteractionPackage1[i] == null)
            {
                m_InteractionGrids1[i].SetItem(null);
            }
            else
            {
                m_InteractionGrids1[i].SetItem(mInteractionPackage1[i]);
            }
        }
    }

    void BtnResortInteractive2BoxOnClick()
    {
        if (m_RefNpc == null)
            return;

        if (PeGameMgr.IsMulti)
        {
            PlayerNetwork.mainPlayer.RequestNpcPackageSort(m_RefNpc.NPC.Id, 1);
            return;
        }

        this.ClearInteractionpackage2();
        this.mInteractionPackage2.Reduce();
        this.mInteractionPackage2.Sort();
        for (int i = 0; i < m_Interaction2Count; i++)
        {
            if (mInteractionPackage2 == null || mInteractionPackage2[i] == null)
            {
                m_InteractionGrids2[i].SetItem(null);
            }
            else
            {
                m_InteractionGrids2[i].SetItem(mInteractionPackage2[i]);
            }
        }
    }

    #endregion

    #region itemOp mehtod
    public void OnDropItem_InterPackage(Grid_N grid)
    {
        if (m_RefNpc == null)
            return;
        if (!m_RefNpc.IsRandomNpc)
            return;
        if (grid.ItemObj != null)
            return;

        if (PeGameMgr.IsMulti)//多人模式
        {
            switch (SelectItem_N.Instance.Place)
            {
                case ItemPlaceType.IPT_ConolyServantEquPersonel:
                    //lz-2016.11.09 检测是否可以脱装备
                    if (SelectItem_N.Instance.RemoveOriginItem())
                    {
                        PlayerNetwork.mainPlayer.RequestGiveItem2Npc((int)grid.ItemPlace, m_RefNpc.m_Npc.Id, SelectItem_N.Instance.ItemObj.instanceId, SelectItem_N.Instance.Place);
                    }
                    break;
                case ItemPlaceType.IPT_ColonyServantInteraction2Personel:
                case ItemPlaceType.IPT_Bag:
                    {
                        PlayerNetwork.mainPlayer.RequestGiveItem2Npc((int)grid.ItemPlace, m_RefNpc.m_Npc.Id, SelectItem_N.Instance.ItemObj.instanceId, SelectItem_N.Instance.Place);
                    }
                    break;
            }

            SelectItem_N.Instance.SetItem(null);
        }
        else //单机
        {
            switch (SelectItem_N.Instance.Place)
            {

                case ItemPlaceType.IPT_ConolyServantEquPersonel:
                case ItemPlaceType.IPT_ColonyServantInteraction2Personel:
                case ItemPlaceType.IPT_Bag:
                case ItemPlaceType.IPT_ColonyServantInteractionPersonel:
                    //lz-2016.11.09 检测是否可以脱装备
                    if (SelectItem_N.Instance.RemoveOriginItem())
                    {
                        SetInteractionItemWithIndex(SelectItem_N.Instance.ItemObj, grid.ItemIndex);
                        SelectItem_N.Instance.RemoveOriginItem();
                        SelectItem_N.Instance.SetItem(null);
                    }
                    break;
                default:
                    SelectItem_N.Instance.SetItem(null);
                    break;
            }
        }
    }

    public void OnLeftMouseCliked_InterPackage(Grid_N grid)
    {
        if (null == m_RefNpc)
            return;
        if (!m_RefNpc.IsRandomNpc)
            return;
        SelectItem_N.Instance.SetItem(grid.ItemObj, grid.ItemPlace, grid.ItemIndex);
    }

    public void OnRightMouseCliked_InterPackage(Grid_N grid)
    {
        if (m_RefNpc == null)
            return;

        if (!m_RefNpc.IsRandomNpc)
            return;

        Pathea.UseItemCmpt useItem = m_RefNpc.m_Npc.GetCmpt<Pathea.UseItemCmpt>();
        if (null == useItem)
            useItem = m_RefNpc.m_Npc.Add<Pathea.UseItemCmpt>();

        if (true == useItem.Request(grid.ItemObj))
        {
            GameUI.Instance.mCSUI_MainWndCtrl.PersonnelUI.m_NPCEquipUI.UpdateEquipAndTex();
        }
    }

    public void OnDropItem_InterPackage2(Grid_N grid)
    {
        if (m_RefNpc == null)
            return;
        if (!m_RefNpc.IsRandomNpc)
            return;
        if (grid.ItemObj != null)
            return;

        if (PeGameMgr.IsMulti)//多人模式
        {
            switch (SelectItem_N.Instance.Place)
            {
                case ItemPlaceType.IPT_ConolyServantEquPersonel:
                    //lz-2016.11.09 检测是否可以脱装备
                    if (SelectItem_N.Instance.RemoveOriginItem())
                    {
                        PlayerNetwork.mainPlayer.RequestGiveItem2Npc((int)grid.ItemPlace, m_RefNpc.m_Npc.Id, SelectItem_N.Instance.ItemObj.instanceId, SelectItem_N.Instance.Place);
                    }
                    break;
                case ItemPlaceType.IPT_Bag:
                case ItemPlaceType.IPT_ColonyServantInteractionPersonel:
                    {
                        PlayerNetwork.mainPlayer.RequestGiveItem2Npc((int)grid.ItemPlace, m_RefNpc.m_Npc.Id, SelectItem_N.Instance.ItemObj.instanceId, SelectItem_N.Instance.Place);
                    }
                    break;
            }

            SelectItem_N.Instance.SetItem(null);
        }
        else //单机
        {
            switch (SelectItem_N.Instance.Place)
            {
                case ItemPlaceType.IPT_ConolyServantEquPersonel:
                case ItemPlaceType.IPT_ColonyServantInteractionPersonel:
                case ItemPlaceType.IPT_Bag:
                case ItemPlaceType.IPT_ColonyServantInteraction2Personel:
                    //lz-2016.11.09 检测是否可以脱装备
                    if (SelectItem_N.Instance.RemoveOriginItem())
                    {
                        SetInteraction2ItemWithIndex(SelectItem_N.Instance.ItemObj, grid.ItemIndex);
                        SelectItem_N.Instance.SetItem(null);
                    }
                    break;
                default:
                    SelectItem_N.Instance.SetItem(null);
                    break;
            }
        }
    }

    public void OnLeftMouseCliked_InterPackage2(Grid_N grid)
    {
        if (null == m_RefNpc)
            return;
        if (!m_RefNpc.IsRandomNpc)
            return;
        SelectItem_N.Instance.SetItem(grid.ItemObj, grid.ItemPlace, grid.ItemIndex);
    }

    public void OnRightMouseCliked_InterPackage2(Grid_N grid)
    {
        if (m_RefNpc == null)
            return;

        if (!m_RefNpc.IsRandomNpc)
            return;

        Pathea.UseItemCmpt useItem = m_RefNpc.m_Npc.GetCmpt<Pathea.UseItemCmpt>();
        if (null == useItem)
            useItem = m_RefNpc.m_Npc.Add<Pathea.UseItemCmpt>();

        if (true == useItem.Request(grid.ItemObj))
        {
            GameUI.Instance.mCSUI_MainWndCtrl.PersonnelUI.m_NPCEquipUI.UpdateEquipAndTex();
        }
    }
    #endregion

    void ClearSkill()
    {
        for (int i = 0; i < m_SkillGrids.Count; i++)
        {
            m_SkillGrids[i].SetSkill(0, i);
        }
    }

    void UpdateSkills()
    {

        if (m_RefNpc == null || m_RefNpc.NPC == null)
        {
            ClearSkill();
            return;
        }


        //if (!m_RefNpc.IsRandomNpc)
        //    return;

        if (m_RefNpc.SkAlive == null)
            return;

        int index = 0;
        ClearSkill();
        if (m_RefNpc.Npcabliys != null)
        {
            foreach (NpcAbility abity in m_RefNpc.Npcabliys)
            {
                if (abity != null)
                {
                    m_SkillGrids[index].SetSkill(abity.skillId, index, m_RefNpc.SkAlive, abity.icon, abity.desc);
                    index++;
                }
            }
        }

        //--to do: wait
        //// Skill
        //if (npcRandom.mRandomSkillList != null)
        //{
        //    List<int> skillList = npcRandom.mRandomSkillList;
        //    for (int i = 0; i < skillList.Count; i++)
        //    {
        //        if(i >= m_SkillGrids.Count)
        //            break;

        //        EffSkill skill = EffSkill.s_tblEffSkills.Find(iterSkill1 => EffSkill.MatchId(iterSkill1, skillList[i]));
        //        if (skill == null)
        //            continue;

        //        m_SkillGrids[i].m_Grid.SetSkill(skillList[i], i, skill.m_name[1]);
        //    }
        //}
    }

    #region CALL_BACKE

    bool OnCheckItemGrid(ItemObject item, CSUI_Grid.ECheckItemType check_type)
    {
        if (m_RefNpc == null)
            return false;

        return true;
    }

    void OnItemGridChanged(ItemObject item, ItemObject oldItem, int index)
    {
        //--to do: wait npcpackage
        //if (oldItem != null)
        //    RefNpc.m_Npc.RemoveFromBag(oldItem);

        //if (item != null)
        //    RefNpc.m_Npc.AddToBag(item);

        if (oldItem != null)
        {
            if (item == null)
				CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mTakeAwayItemFromNpc.GetString(), oldItem.protoData.GetName(), m_RefNpc.FullName));
            else if (item == oldItem)
                CSUI_MainWndCtrl.ShowStatusBar(UIMsgBoxInfo.mNotEnoughGrid.GetString(), Color.red);
            else
				CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mPutItemToNpc.GetString(), item.protoData.GetName(), m_RefNpc.FullName));
        }
        else if (item != null)
			CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mPutItemToNpc.GetString(), item.protoData.GetName(), m_RefNpc.FullName));
    }

    void OnGridsExchangeItem(Grid_N grid, ItemObject item)
    {
        //--to do: wait npcpackage
        //if (grid.ItemObj != null)
        //    RefNpc.m_Npc.RemoveFromBag(grid.ItemObj);

        //if (item != null)
        //    RefNpc.m_Npc.AddToBag(item);

        grid.SetItem(item);
    }

    void PageInfoOnActive(bool active)
    {
        m_InfoPage.SetActive(active);
        UpdateSkills();
    }
    void PageInvetoryOnActive(bool active)
    {
        m_InventoryPage.SetActive(active);
        UpdateSkills();
    }
    void PageWorkOnActive(bool active)
    {
        m_WorkPage.SetActive(active);
        UpdateSkills();
    }


    //lz-2016.08.15 增加基地NPC召回按钮
    void OnCallBtn(GameObject go)
    {
        if (null != m_RefNpc && null != m_RefNpc.NPC)
        {
            //m_RefNpc.NPC
            ECsNpcState state;
            if (!NpcMgr.CallBackColonyNpcToPlayer(m_RefNpc.NPC, out state))
            {
                switch (state)
                {
                    case ECsNpcState.None:
                        break;
                    case ECsNpcState.Working:
					CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(PELocalization.GetString(82201077), m_RefNpc.FullName));
                        break;
                    case ECsNpcState.InMission:
					CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(PELocalization.GetString(82201078), m_RefNpc.FullName));
                        break;
                    case ECsNpcState.OutOfRadiu:
                        CSUI_MainWndCtrl.ShowStatusBar(PELocalization.GetString(82201079));
                        break;
                }

            }
            else
            {
                CSUI_MainWndCtrl.ShowStatusBar(PELocalization.GetString(82201080));
            }
        }
    }


    #endregion

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
        if (m_RefNpc == null)
        {
            return;
        }
        List<PEAbnormalType> abList = new List<PEAbnormalType>();
        //lz-2016.12.09 错误 #7109 Crash bug
        if (null!=cmpt && null!=cmpt.Entity&& null!=cmpt.Entity.Alnormal)
            abList.AddRange(cmpt.Entity.Alnormal.GetActiveAbnormalList());

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

    void LateUpdate()
    {
        UpdateReposition();
    }


    #endregion
}
