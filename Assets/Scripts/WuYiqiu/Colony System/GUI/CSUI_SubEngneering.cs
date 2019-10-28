using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using SkillAsset;

public class CSUI_SubEngneering : MonoBehaviour
{
    public int m_Type;
    public CSEntity m_Entity;

    // Delgate
    public delegate void ItemDelegate(ItemObject item);
    public ItemDelegate onEnhancedItemChanged;
    public ItemDelegate onRepairedItemChanged;
    public ItemDelegate onRecycleItemChanged;

    //item component
    public Strengthen m_enhanceItem;
    public Repair m_repairItem;
    public Recycle m_recycleItem;


    // Item Part
    [System.Serializable]
    public class ItemPart
    {
        public CSUI_Grid m_GridPrefab;
        public Transform m_Root;
        public UILabel m_Name;
        public UILabel m_CostsTime;
    }

    [SerializeField]
    private ItemPart m_ItemPart;

    private CSUI_Grid m_ItemGrid;
    public CSUI_Grid ItemGrid { get { return m_ItemGrid; } }

    // Item Property
    [System.Serializable]
    public class EnhancePropertyPart
    {
        public Transform m_Root;
        public UILabel m_Durability;
        public UILabel m_Atk;
        public UILabel m_Defense;
        public UILabel m_TimesEnhcance;
    }

    [SerializeField]
    private EnhancePropertyPart m_EnhanceProperty;

    [System.Serializable]
    public class RepairPropertyPart
    {
        public Transform m_Root;
        public UILabel m_Durability;
    }

    [SerializeField]
    private RepairPropertyPart m_RepairProperty;

    [System.Serializable]
    public class RecyclePropertyPart
    {
        public Transform m_Root;
        public UILabel m_Durability;
    }

    [SerializeField]
    private RecyclePropertyPart m_RecycleProperty;

    // NPC Part
    [System.Serializable]
    public class NPCPart
    {
        public UIGrid m_Root;
        public CSUI_NPCGrid m_Prefab;
        public UIButton m_AuttoSettleBtn;
        public UIButton m_DisbandAllBtn;
    }

    [SerializeField]
    private NPCPart m_NpcPart;

    private List<CSUI_NPCGrid> m_NpcGirds = new List<CSUI_NPCGrid>();


    // Popup hint
    public Dictionary<int, List<CSUI_PopupHint>> m_PopupHints = new Dictionary<int, List<CSUI_PopupHint>>();

    [System.Serializable]
    public class PopupHintsPart
    {
        public GameObject m_EnhanceHintGo;
        public GameObject m_RepairHintGo;
        public GameObject m_RecyleHintGo;
    }

    [SerializeField]
    private PopupHintsPart m_PopupHintsPart;

    public void SetEntity(CSCommon entity)
    {
        if (entity != null && entity.m_Type != CSConst.etEnhance
            && entity.m_Type != CSConst.etRepair
            && entity.m_Type != CSConst.etRecyle)
        {
            Debug.Log("The giving Entity is not allowed!");
            return;
        }

        m_Type = entity.m_Type;
        m_Entity = entity;
        CSUI_MainWndCtrl.Instance.mSelectedEnntity = m_Entity;

        if (m_Type == CSConst.etEnhance)
        {
            CSEnhance cse = m_Entity as CSEnhance;
            if (cse.m_Item == null)
            {
                SetItem(null);
            }
            else
            {
                SetItem(cse.m_Item.itemObj);
            }
        }
        else if (m_Type == CSConst.etRepair)
        {
            CSRepair csr = m_Entity as CSRepair;
            if (csr.m_Item == null)
            {
                SetItem(null);
            }
            else
            {
                SetItem(csr.m_Item.itemObj);
            }
        }
        else if (m_Type == CSConst.etRecyle)
        {
            CSRecycle csr = m_Entity as CSRecycle;
            if (csr.m_Item == null)
            {
                SetItem(null);
            }
            else
            {
                SetItem(csr.m_Item.itemObj);
            }
        }
    }

    public bool SetItem(ItemObject item)
    {
        if (m_Entity == null)
            return false;

        if (m_ItemGrid == null)
            InitItemGrid();

        if (item == null)
        {
            m_ItemGrid.m_Grid.SetItem(null);
            m_enhanceItem = null;
            m_repairItem = null;
            m_recycleItem = null;
            OnItemChanged(null);
            return true;
        }

        if (m_Type == CSConst.etEnhance)
        {
            Strengthen enhanceItem = item.GetCmpt<Strengthen>();
            if (enhanceItem == null)
            {
                return false;
            }

            m_ItemGrid.m_Grid.SetItem(item);
            m_enhanceItem = enhanceItem;
            m_repairItem = null;
            m_recycleItem = null;
            OnItemChanged(item);
        }
        else if (m_Type == CSConst.etRepair)
        {
            Repair repairItem = item.GetCmpt<Repair>();
            if (repairItem == null)
            {
                return false;
            }

            m_ItemGrid.m_Grid.SetItem(item);
            m_enhanceItem = null;
            m_repairItem = repairItem;
            m_recycleItem = null;
            OnItemChanged(item);
        }
        else if (m_Type == CSConst.etRecyle)
        {
            Recycle recycleItem = item.GetCmpt<Recycle>();
            if (recycleItem == null)
            {
                return false;
            }
            m_ItemGrid.m_Grid.SetItem(item);
            m_enhanceItem = null;
            m_repairItem = null;
            m_recycleItem = recycleItem;
            OnItemChanged(item);
        }

        return true;
    }


    public void UpdatePopupHintInfo(CSEntity entiy)
    {
        if (entiy == null)
            return;


        int type = entiy.m_Type;

        if (type == CSConst.etEnhance)
        {
            //            CSEnhance cse = entiy as CSEnhance;
            if (null == m_enhanceItem)
            {
                return;
            }

            float curValue = 0.0f;
            float nextValue = 0.0f;

            if (!m_PopupHints.ContainsKey(type))
                m_PopupHints.Add(type, new List<CSUI_PopupHint>());

            foreach (CSUI_PopupHint ph in m_PopupHints[type])
            {
                if (ph != null && ph.gameObject != null)
                    Destroy(ph.gameObject);
            }

            m_PopupHints[type].Clear();

            Vector3 pos = m_EnhanceProperty.m_Durability.transform.position;
            //item.GetLevelUpValue(ItemProperty.DurabilityMax, out curValue, out nextValue);
            curValue = m_enhanceItem.GetCurMaxDurability();
            nextValue = m_enhanceItem.GetNextMaxDurability();
            string str = " + " + Mathf.FloorToInt(nextValue - curValue).ToString();
            CSUI_PopupHint phTmp = CSUI_MainWndCtrl.CreatePopupHint(pos, m_PopupHintsPart.m_EnhanceHintGo.transform, new Vector3(10, -2, -6), str, true);
            m_PopupHints[type].Add(phTmp);

            pos = m_EnhanceProperty.m_Atk.transform.position;
            //item.GetLevelUpValue(ItemProperty.Atk, out curValue, out nextValue);
            curValue = m_enhanceItem.GetCurLevelProperty(Pathea.AttribType.Atk);
            nextValue = m_enhanceItem.GetNextLevelProperty(Pathea.AttribType.Atk);
            str = " + " + Mathf.FloorToInt(nextValue - curValue).ToString();
            phTmp = CSUI_MainWndCtrl.CreatePopupHint(pos, m_PopupHintsPart.m_EnhanceHintGo.transform, new Vector3(10, -2, -6), str, true);
            m_PopupHints[type].Add(phTmp);

            pos = m_EnhanceProperty.m_Defense.transform.position;
            //item.GetLevelUpValue(ItemProperty.Def, out curValue, out nextValue);
            curValue = m_enhanceItem.GetCurLevelProperty(Pathea.AttribType.Def);
            nextValue = m_enhanceItem.GetNextLevelProperty(Pathea.AttribType.Def);
            str = " + " + Mathf.FloorToInt(nextValue - curValue).ToString();
            phTmp = CSUI_MainWndCtrl.CreatePopupHint(pos, m_PopupHintsPart.m_EnhanceHintGo.transform, new Vector3(10, -2, -6), str, true);
            m_PopupHints[type].Add(phTmp);

        }
        else if (type == CSConst.etRepair)
        {
            CSRepair repair = entiy as CSRepair;

            if (!m_PopupHints.ContainsKey(type))
                m_PopupHints.Add(type, new List<CSUI_PopupHint>());

            foreach (CSUI_PopupHint ph in m_PopupHints[type])
            {
                if (ph != null && ph.gameObject != null)
                    Destroy(ph.gameObject);
            }

            m_PopupHints[type].Clear();

            Vector3 pos = m_EnhanceProperty.m_Durability.transform.position;
            float val = repair.GetIncreasingDura();
            string str = " + " + Mathf.FloorToInt(val).ToString();
            CSUI_PopupHint phTmp = CSUI_MainWndCtrl.CreatePopupHint(pos, m_PopupHintsPart.m_RepairHintGo.transform, new Vector3(10, -2, 0), str, true);
            m_PopupHints[type].Add(phTmp);
        }
        else if (type == CSConst.etRecyle)
        {

        }
    }

    void OnItemChanged(ItemObject item)
    {
        if (m_Type == CSConst.etEnhance)
        {
            CSEnhance cse = m_Entity as CSEnhance;
            cse.m_Item = m_enhanceItem;

            m_EnhanceProperty.m_Root.gameObject.SetActive(true);
            m_RepairProperty.m_Root.gameObject.SetActive(false);
            m_RecycleProperty.m_Root.gameObject.SetActive(false);

            if (onEnhancedItemChanged != null)
                onEnhancedItemChanged(item);
        }
        else if (m_Type == CSConst.etRepair)
        {
            CSRepair csr = m_Entity as CSRepair;
            csr.m_Item = m_repairItem;

            m_EnhanceProperty.m_Root.gameObject.SetActive(false);
            m_RepairProperty.m_Root.gameObject.SetActive(true);
            m_RecycleProperty.m_Root.gameObject.SetActive(false);

            if (onRepairedItemChanged != null)
                onRepairedItemChanged(item);
        }
        else if (m_Type == CSConst.etRecyle)
        {
            CSRecycle csr = m_Entity as CSRecycle;
            csr.m_Item = m_recycleItem;

            m_EnhanceProperty.m_Root.gameObject.SetActive(false);
            m_RepairProperty.m_Root.gameObject.SetActive(false);
            m_RecycleProperty.m_Root.gameObject.SetActive(true);

            if (onRecycleItemChanged != null)
                onRecycleItemChanged(item);
        }
    }

    void InitItemGrid()
    {
        m_ItemGrid = Instantiate(m_ItemPart.m_GridPrefab) as CSUI_Grid;
        m_ItemGrid.transform.parent = m_ItemPart.m_Root;
        m_ItemGrid.transform.localPosition = Vector3.zero;
        m_ItemGrid.transform.localRotation = Quaternion.identity;
        m_ItemGrid.transform.localScale = Vector3.one;

        m_ItemGrid.m_Active = true;
        m_ItemGrid.onCheckItem = OnGridCheckItem;
        m_ItemGrid.OnItemChanged = OnGirdItemChanged;

        if (GameConfig.IsMultiMode)
        {
            m_ItemGrid.OnDropItemMulti = OnDropItemMulti;
            m_ItemGrid.OnLeftMouseClickedMulti = OnLeftMouseClickedMulti;
            m_ItemGrid.OnRightMouseClickedMulti = OnRightMouseClickedMulti;
        }
    }

    #region GIRD_N

    /// <summary>
    /// check if this grid can put item in
    /// </summary>
    /// <param name="item">target_item</param>
    /// <param name="check_type">operation_type</param>
    /// <returns></returns>
    bool OnGridCheckItem(ItemObject item, CSUI_Grid.ECheckItemType check_type)
    {
        if (!CSUI_MainWndCtrl.IsWorking())
            return false;

        if (!m_Entity.IsRunning)
        {
            CSCommon com = m_Entity as CSCommon;
            if (com == null)
                CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mCantWorkWithoutElectricity.GetString(), CSUtils.GetEntityName(m_Type)), Color.red);
            else
            {
                if (com.Assembly == null)
                    CSUI_MainWndCtrl.ShowStatusBar("The machine is invalid.", Color.red);
                else
                    CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mCantWorkWithoutElectricity.GetString(), CSUtils.GetEntityName(m_Type)), Color.red);
            }

            return false;
        }

        if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_HotKeyBar)
            return false;

        if (m_Type == CSConst.etEnhance)
        {
            if ((m_Entity as CSEnhance).IsEnhancing)
            {
                if (m_enhanceItem != null)
                    CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mHasBeenEnhancingTheItem.GetString(), m_enhanceItem.protoData.GetName()), Color.red);
                return false;
            }

            if (item != null)
            {
                //if ((item.prototypeData.m_OpType & ItemOperationType.EquipmentItem) == 0
                //    && item.prototypeId < CreationData.s_ObjectStartID)
                //{
                //    return false;
                //}

                Strengthen sItem = item.GetCmpt<Strengthen>();
                if (null != sItem)
                {
                    if (sItem.strengthenTime >= 100)
                    {
                        CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(PELocalization.GetString(ColonyMessage.CANNOT_ENHANCE_MORE), item.protoData.GetName()), Color.red);
                        return false;
                    }
                }
                else
                {
                    CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(PELocalization.GetString(ColonyMessage.CANNOT_ENHANCE_ITEM), item.protoData.GetName()), Color.red);
                    return false;
                }
            }
        }
        else if (m_Type == CSConst.etRepair)
        {
            if ((m_Entity as CSRepair).IsRepairingM)
            {
                if (m_repairItem != null)
                    CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mHasBeenRepairingTheItem.GetString(), m_repairItem.protoData.GetName()), Color.red);
                return false;
            }

            if (item != null)
            {
                Repair sItem = item.GetCmpt<Repair>();
                if (sItem == null)
                {
                    CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mNotRequireRepair.GetString(), item.protoData.GetName()), Color.red);
                    return false;
                }
            }
        }
        else if (m_Type == CSConst.etRecyle)
        {
            if ((m_Entity as CSRecycle).IsRecycling)
            {
                if (m_recycleItem != null)
                    CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mHasBeenRecyclingTheItem.GetString(), m_recycleItem.protoData.GetName()), Color.red);
                return false;
            }

            if (item != null)
            {
                Recycle sItem = item.GetCmpt<Recycle>();
                //if (item.prototypeId > CreationData.s_ObjectStartID)
                //{
                //    //				return true;
                //}
                //else
                //{
                //    Pathea.Replicator.Formula ms = Pathea.Replicator.Formula.Mgr.Instance.FindByProductId(item.prototypeId);
                //    //MergeSkill ms = MergeSkill.s_tblMergeSkills.Find(
                //    //    delegate(MergeSkill hh)
                //    //    {
                //    //        return hh.m_productItemId == item.mItemID;
                //    //    });

                if (sItem == null || sItem.GetRecycleItems() == null)
                {
                    if (sItem != null)
                    {
                        Debug.LogError(item.nameText + " " + item.protoId + " should not have Recycle!");
                    }
                    CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mCantRecycle.GetString(), item.protoData.GetName()), Color.red);
                    return false;
                }
                //}
            }
        }

        return true;
    }

    void OnGirdItemChanged(ItemObject item, ItemObject oldItem, int index)
    {
        if (oldItem != null)
        {
            if (item == null)
                CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mTakeAwayFromMachine.GetString(), oldItem.protoData.GetName(), CSUtils.GetEntityName(m_Type)));
            else if (item == oldItem)
            {
                //log:lz-2016.04.14: 这里点击的是自己Item不做操作
                //CSUI_MainWndCtrl.ShowStatusBar(UIMsgBoxInfo.mNotEnoughGrid.GetString(), Color.red);
            }
            else
                CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mPutIntoMachine.GetString(), item.protoData.GetName(), CSUtils.GetEntityName(m_Type)));
        }
        else if (item != null)
            CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mPutIntoMachine.GetString(), item.protoData.GetName(), CSUtils.GetEntityName(m_Type)));

        if (item != null)
        {
            m_enhanceItem = item.GetCmpt<Strengthen>();
            m_repairItem = item.GetCmpt<Repair>();
            m_recycleItem = item.GetCmpt<Recycle>();
        }
        else
        {
            m_enhanceItem = null;
            m_repairItem = null;
            m_recycleItem = null;
        }

        OnItemChanged(item);
    }

    #endregion

    #region NGUI_CALLBACK

    void OnAutoSettleBtn()
    {
        CSCommon csc = m_Entity as CSCommon;
        if (csc != null)
        {
            csc.AutoSettleWorkers();
            //			CSUI_Main.ShowStatusBar("Auto settle some workers in this machine!");
        }
    }

    void OnDisbandAllBtn()
    {
        CSCommon csc = m_Entity as CSCommon;

        if (csc != null)
        {
            csc.ClearWorkers();
            //			CSUI_Main.ShowStatusBar("Disband all workers who work for this machine");
        }
    }

    #endregion

    #region UNITY_INNER_FUNC

    // Use this for initialization
    void Start()
    {
        // Create Npc Icon grid
        for (int i = 0; i < 4; i++)
        {
            CSUI_NPCGrid npcGrid = Instantiate(m_NpcPart.m_Prefab) as CSUI_NPCGrid;
            npcGrid.transform.parent = m_NpcPart.m_Root.transform;
            npcGrid.transform.localPosition = Vector3.zero;
            npcGrid.transform.localRotation = Quaternion.identity;
            npcGrid.transform.localScale = Vector3.one;
            npcGrid.NpcIconRadio = m_NpcPart.m_Root.transform;
            m_NpcGirds.Add(npcGrid);
        }
        m_NpcPart.m_Root.repositionNow = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_ItemGrid != null)
        {
            ItemObject item = m_ItemGrid.m_Grid.ItemObj;
            if (item == null)
            {
                m_ItemPart.m_Name.text = "";
                if (m_Type == CSConst.etEnhance)
                {
                    m_EnhanceProperty.m_Durability.text = "0 ([00BBFF] +0 [ffffff])";
                    m_EnhanceProperty.m_Atk.text = "0 ([00BBFF] +0 [ffffff])";
                    m_EnhanceProperty.m_Defense.text = "0 ([00BBFF] +0 [ffffff])";
                    m_EnhanceProperty.m_TimesEnhcance.text = "0";
                }
                else if (m_Type == CSConst.etRepair)
                    m_RepairProperty.m_Durability.text = "0 ([00BBFF] +0 [ffffff])";
                else if (m_Type == CSConst.etRecyle)
                    m_RecycleProperty.m_Durability.text = "0 ([00BBFF] +0 [ffffff])";

                m_ItemPart.m_CostsTime.text = "00:00";

                m_PopupHintsPart.m_EnhanceHintGo.SetActive(false);
                m_PopupHintsPart.m_RepairHintGo.SetActive(false);
                m_PopupHintsPart.m_RecyleHintGo.SetActive(false);
            }
            else
            {
				m_ItemPart.m_Name.text = item.protoData.name;
                // Item property
                float curValue = 0.0f;
                float nextValue = 0.0f;

                if (m_Type == CSConst.etEnhance)
                {
                    if (m_enhanceItem != null)
                    {
                        curValue = m_enhanceItem.GetCurLevelProperty(Pathea.AttribType.Atk);
                        nextValue = m_enhanceItem.GetNextLevelProperty(Pathea.AttribType.Atk);
                        m_EnhanceProperty.m_Atk.text = Mathf.FloorToInt(curValue).ToString() + " ([00BBFF] + " + Mathf.FloorToInt((nextValue - curValue)).ToString() + "[ffffff])";

                        curValue = m_enhanceItem.GetCurLevelProperty(Pathea.AttribType.Def);
                        nextValue = m_enhanceItem.GetNextLevelProperty(Pathea.AttribType.Def);
                        m_EnhanceProperty.m_Defense.text = Mathf.FloorToInt(curValue).ToString() + " ([00BBFF] + " + Mathf.FloorToInt((nextValue - curValue)).ToString() + "[ffffff])";

                        curValue = m_enhanceItem.GetCurMaxDurability();
                        nextValue = m_enhanceItem.GetNextMaxDurability();

                        WhiteCat.CreationItemClass isoType = WhiteCat.CreationItemClass.None;

                        if (null != m_enhanceItem.itemObj)
                            isoType = WhiteCat.CreationHelper.GetCreationItemClass(m_enhanceItem.itemObj);

                        if (isoType != WhiteCat.CreationItemClass.None)
                        {
                            //lz-2017.01.16 只有一部分ISO物品耐久需要乘一个系数才是显示耐久,其他的直接显示原数
                            switch (isoType)
                            {
                                case WhiteCat.CreationItemClass.Sword:
                                case WhiteCat.CreationItemClass.Axe:
                                case WhiteCat.CreationItemClass.Bow:
                                case WhiteCat.CreationItemClass.Shield:
                                case WhiteCat.CreationItemClass.HandGun:
                                case WhiteCat.CreationItemClass.Rifle:
                                    curValue *= WhiteCat.PEVCConfig.equipDurabilityShowScale;
                                    nextValue *= WhiteCat.PEVCConfig.equipDurabilityShowScale;
                                    break;
                            }
                        }

                        m_EnhanceProperty.m_Durability.text = Mathf.CeilToInt(curValue).ToString() + " ([00BBFF] + " + Mathf.CeilToInt((nextValue - curValue)).ToString() + "[ffffff])";

                        // Times Ehance
                        m_EnhanceProperty.m_TimesEnhcance.text = m_enhanceItem.strengthenTime.ToString();
                    }

                    CSEnhance cse = m_Entity as CSEnhance;
                    m_ItemPart.m_CostsTime.text = CSUtils.GetRealTimeMS((int)cse.CostsTime);

                    m_PopupHintsPart.m_EnhanceHintGo.SetActive(true);
                    m_PopupHintsPart.m_RepairHintGo.SetActive(false);
                    m_PopupHintsPart.m_RecyleHintGo.SetActive(false);
                }
                else if (m_Type == CSConst.etRepair)
                {
                    if (null != m_repairItem)
                    {
                        curValue = m_repairItem.GetValue().current;
                        float expendValue = m_repairItem.GetValue().ExpendValue;
                        WhiteCat.CreationItemClass isoType = WhiteCat.CreationItemClass.None;

                        if (null != m_repairItem.itemObj)
                            isoType = WhiteCat.CreationHelper.GetCreationItemClass(m_repairItem.itemObj);

                        if (isoType!= WhiteCat.CreationItemClass.None)
                        {
                            //lz-2016.10.25 只有一部分ISO物品耐久需要乘一个系数才是显示耐久,其他的直接显示原数
                            switch (isoType)
                            {
                                case WhiteCat.CreationItemClass.Sword:
                                case WhiteCat.CreationItemClass.Axe:
                                case WhiteCat.CreationItemClass.Bow:
                                case WhiteCat.CreationItemClass.Shield:
                                case WhiteCat.CreationItemClass.HandGun:
                                case WhiteCat.CreationItemClass.Rifle:
                                    curValue *= WhiteCat.PEVCConfig.equipDurabilityShowScale;
                                    expendValue *= WhiteCat.PEVCConfig.equipDurabilityShowScale;
                                    break;
                            }
                        }
                        else
                        {
                            //lz-2016.10.24 其他物品显示的时候要小于100倍的显示，就和ToolTip上面的防御值一样了
                            curValue *= 0.01f;
                            expendValue *= 0.01f;
                        }
                        m_RepairProperty.m_Durability.text = Mathf.CeilToInt(curValue).ToString()
                            + " ([00BBFF] + " + Mathf.CeilToInt(expendValue).ToString() + "[ffffff])";
                    }

                    CSRepair csr = m_Entity as CSRepair;
                    m_ItemPart.m_CostsTime.text = CSUtils.GetRealTimeMS((int)csr.CostsTime);

                    m_PopupHintsPart.m_EnhanceHintGo.SetActive(false);
                    m_PopupHintsPart.m_RepairHintGo.SetActive(true);
                    m_PopupHintsPart.m_RecyleHintGo.SetActive(false);
                }
                else if (m_Type == CSConst.etRecyle)
                {
                    CSRecycle csr = m_Entity as CSRecycle;
                    m_ItemPart.m_CostsTime.text = CSUtils.GetRealTimeMS((int)csr.CostsTime);

                    if (null != m_recycleItem)
                    {
                        if (m_recycleItem.GetCurrent() == null)
                            curValue = 0;
                        else
                            curValue = m_recycleItem.GetCurrent().current;//todocolony
                    }

                    WhiteCat.CreationItemClass isoType = WhiteCat.CreationItemClass.None;

                    if (null != m_recycleItem.itemObj)
                        isoType = WhiteCat.CreationHelper.GetCreationItemClass(m_recycleItem.itemObj);

                    if (isoType != WhiteCat.CreationItemClass.None)
                    {
                        //lz-2017.01.16 只有一部分ISO物品耐久需要乘一个系数才是显示耐久,其他的直接显示原数
                        switch (isoType)
                        {
                            case WhiteCat.CreationItemClass.Sword:
                            case WhiteCat.CreationItemClass.Axe:
                            case WhiteCat.CreationItemClass.Bow:
                            case WhiteCat.CreationItemClass.Shield:
                            case WhiteCat.CreationItemClass.HandGun:
                            case WhiteCat.CreationItemClass.Rifle:
                                curValue *= WhiteCat.PEVCConfig.equipDurabilityShowScale;
                                break;
                        }
                    }
                    m_RecycleProperty.m_Durability.text = Mathf.CeilToInt(curValue).ToString();

                    m_PopupHintsPart.m_EnhanceHintGo.SetActive(false);
                    m_PopupHintsPart.m_RepairHintGo.SetActive(false);
                    m_PopupHintsPart.m_RecyleHintGo.SetActive(true);
                }
            }
        }

        // Worker
        CSCommon csc = m_Entity as CSCommon;
        if (csc != null)
        {
            int workCount = csc.WorkerCount;
            for (int i = 0; i < m_NpcGirds.Count; i++)
            {
                if (i < workCount)
                    m_NpcGirds[i].m_Npc = csc.Worker(i);
                else
                    m_NpcGirds[i].m_Npc = null;
            }

            m_NpcPart.m_AuttoSettleBtn.isEnabled = (workCount != csc.WorkerMaxCount);

            m_NpcPart.m_DisbandAllBtn.isEnabled = (workCount != 0);
        }
    }

    #endregion

    #region MultiMode
    public void OnDropItemMulti(Grid_N grid, int m_Index)
    {
        ItemObject itemObj = SelectItem_N.Instance.ItemObj;
        if (m_Type == CSConst.etEnhance)
        {
            m_Entity._ColonyObj._Network.EHN_SetItem(itemObj);
        }
        else if (m_Type == CSConst.etRepair)
        {
            m_Entity._ColonyObj._Network.RPA_SetItem(itemObj.instanceId);
        }
        else if (m_Type == CSConst.etRecyle)
        {
            m_Entity._ColonyObj._Network.RCY_SetItem(itemObj);
        }
        //3.do
        SelectItem_N.Instance.SetItem(null);
    }
    public void OnLeftMouseClickedMulti(Grid_N grid, int m_Index)
    {
        if (m_Type == CSConst.etEnhance)
        {
        }
        else if (m_Type == CSConst.etRepair)
        {
        }
        else if (m_Type == CSConst.etRecyle)
        {
        }
    }
    public void OnRightMouseClickedMulti(Grid_N grid, int m_Index)
    {
        if (m_Type == CSConst.etEnhance)
        {
            m_Entity._ColonyObj._Network.EHN_Fetch();
        }
        else if (m_Type == CSConst.etRepair)
        {
            m_Entity._ColonyObj._Network.RPA_FetchItem();
        }
        else if (m_Type == CSConst.etRecyle)
        {
            m_Entity._ColonyObj._Network.RCY_FetchItem();
        }
    }

    public void SetResult(bool success, int objId, CSEntity entity)
    {
        
        if (success)
        {
            if (m_Entity == entity)
			{
				ItemObject item = ItemMgr.Instance.Get(objId);
				ItemObject oldItem = m_ItemGrid.m_Grid.ItemObj;
				CSUI_Grid UIgrid = m_ItemGrid;
                UIgrid.m_Grid.SetItem(item);
                if (UIgrid.OnItemChanged != null)
                    UIgrid.OnItemChanged(item, oldItem, UIgrid.m_Index);
            }
        }
    }

    public void FetchResult(bool success, CSEntity entity)
    {
        if (success)
        {
            if (m_Entity == entity)
            {
				ItemObject item = null;
				ItemObject oldItem = m_ItemGrid.m_Grid.ItemObj;
				CSUI_Grid UIgrid = m_ItemGrid;
                GameUI.Instance.mItemPackageCtrl.ResetItem();
                UIgrid.m_Grid.SetItem(item);
                if (UIgrid.OnItemChanged != null)
                    UIgrid.OnItemChanged(item, oldItem, UIgrid.m_Index);
            }
        }
    }
    #endregion
}
