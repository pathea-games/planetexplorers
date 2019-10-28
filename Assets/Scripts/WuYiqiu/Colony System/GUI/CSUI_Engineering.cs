using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using Pathea;
using ItemAsset.PackageHelper;

public class CSUI_Engineering : MonoBehaviour
{
    private CSEnhance m_Enhance;
    private CSRepair m_Repair;
    private CSRecycle m_Recycle;

    private int m_CurType;
    private PeEntity player;
    private PlayerPackageCmpt playerPackageCmpt;

    public UIScrollBar mListScrolBar;
    // Menu
    [System.Serializable]
    public class MenuPart
    {
        public UICheckbox m_EnhanceCB;
        public UICheckbox m_RepairCB;
        public UICheckbox m_RecyleCB;
        public UIGrid m_Root;
    }

    [SerializeField]
    private MenuPart m_MenuPart;

    [SerializeField]
    public CSUI_SubEngneering m_SubEngneering;

    // Material
    [System.Serializable]
    public class MaterilPart
    {
        public CSUI_MaterialGrid m_Prefab;
        public UIGrid m_Root;
    }

    [SerializeField]
    private MaterilPart m_MatPart;

    private List<CSUI_MaterialGrid> m_MatList = new List<CSUI_MaterialGrid>();

    // Hanlde Button
    [System.Serializable]
    public class HandlePart
    {
        public N_ImageButton m_OKBtn;
        public N_ImageButton m_ResetBtn;
    }

    [SerializeField]
    private HandlePart m_Handle;

    [SerializeField]
    private GameObject m_SkillLock;

    // Popup hint for cut down the materials
    //private List<CSUI_PopupHint>  m_PopHints;

    public bool IsEmpty()
    {
        return (m_Enhance == null) && (m_Repair == null) && (m_Recycle == null);
    }

    public void CloseLock()
    {
        if (m_SkillLock != null)
            m_SkillLock.gameObject.SetActive(false);
    }

    public void Replace(List<CSEntity> commons,CSEntity selectEntity)
    {
        List<CSCommon> commons_list = new List<CSCommon>();
        if (m_Enhance != null)
            commons_list.Add(m_Enhance);
        if (m_Repair != null)
            commons_list.Add(m_Repair);
        if (m_Recycle != null)
            commons_list.Add(m_Recycle);

        bool replace = false;
        if (commons.Count != commons_list.Count)
            replace = true;
        else
        {
            for (int i = 0; i < commons.Count; i++)
            {
                if (!commons_list.Contains(commons[i] as CSCommon))
                {
                    replace = true;
                    break;
                }
            }
        }

        if (replace)
        {
            if (m_Enhance != null)
            {
                m_MenuPart.m_EnhanceCB.gameObject.SetActive(false);
                m_Enhance.onEnhancedTimeUp = null;
                m_Enhance = null;
            }

            if (m_Repair != null)
            {
                m_MenuPart.m_RepairCB.gameObject.SetActive(false);
                m_Repair.onRepairedTimeUp = null;
                m_Repair = null;
            }

            if (m_Recycle != null)
            {
                m_MenuPart.m_RecyleCB.gameObject.SetActive(false);
                m_Recycle = null;
            }

            foreach (CSCommon csc in commons)
            {
                AddMachine(csc);
            }

            CSUI_CommonIcon Icon = m_MenuPart.m_EnhanceCB.gameObject.GetComponent<CSUI_CommonIcon>();
            Icon.Common = m_Enhance as CSCommon;

            Icon = m_MenuPart.m_RepairCB.gameObject.GetComponent<CSUI_CommonIcon>();
            Icon.Common = m_Repair as CSCommon;

            Icon = m_MenuPart.m_RecyleCB.gameObject.GetComponent<CSUI_CommonIcon>();
            Icon.Common = m_Recycle as CSCommon;

        }

        if (null == selectEntity)
        {
            if (m_Enhance != null)
                m_CurType = CSConst.etEnhance;
            else if (m_Recycle != null)
                m_CurType = CSConst.etRecyle;
            else if (m_Repair != null)
                m_CurType = CSConst.etRepair;
        }
        else
        {
            //lz-2016.10.17 从那个Entity打开就选中哪个
            if (m_Enhance != null&& selectEntity== m_Enhance)
                m_CurType = CSConst.etEnhance;
            else if (m_Recycle != null && selectEntity == m_Recycle)
                m_CurType = CSConst.etRecyle;
            else if (m_Repair != null && selectEntity == m_Repair)
                m_CurType = CSConst.etRepair;
        }
        SelectByType();
    }

    public void AddMachine(CSEntity entity)
    {
        if (entity.m_Type == CSConst.etEnhance)
        {
            if (m_Enhance != null)
            {
                if (!entity.IsRunning)
                {
                    Debug.LogWarning("The enhanced machine is already exist!");
                    return;
                }
            }

            m_Enhance = entity as CSEnhance;

            m_MenuPart.m_EnhanceCB.gameObject.SetActive(true);
            if (m_MenuPart.m_EnhanceCB.isChecked)
                m_SubEngneering.SetEntity(m_Enhance);

            m_Enhance.onEnhancedTimeUp = OnEnhancedTimeUp;
        }
        else if (entity.m_Type == CSConst.etRepair)
        {
            if (m_Repair != null)
            {
                if (!entity.IsRunning)
                {
                    Debug.LogWarning("The repair machine is already exist!");
                    return;
                }
            }

            m_Repair = entity as CSRepair;

            m_MenuPart.m_RepairCB.gameObject.SetActive(true);
            if (m_MenuPart.m_RepairCB.isChecked)
                m_SubEngneering.SetEntity(m_Repair);

            m_Repair.onRepairedTimeUp = OnRepairedTimeUp;

        }
        else if (entity.m_Type == CSConst.etRecyle)
        {
            if (m_Recycle != null)
            {
                if (!entity.IsRunning)
                {
                    Debug.LogWarning("The recycle machine is already exist!");
                    return;
                }
            }

            m_Recycle = entity as CSRecycle;

            m_MenuPart.m_RecyleCB.gameObject.SetActive(true);
            if (m_MenuPart.m_RecyleCB.isChecked)
                m_SubEngneering.SetEntity(m_Recycle);

            m_Recycle.onRecylced = OnRecycled;

        }

        m_MenuPart.m_Root.repositionNow = true;
    }

    public void RemoveMachine(CSEntity entity)
    {
        if (entity == m_Enhance)
        {
            if (m_MenuPart.m_EnhanceCB.isChecked)
            {

                if (m_MenuPart.m_RepairCB.gameObject.activeSelf)
                    m_CurType = CSConst.etRepair;
                else if (m_MenuPart.m_RecyleCB.gameObject.activeSelf)
                    m_CurType = CSConst.etRecyle;
                else
                    m_CurType = CSConst.etUnknow;

                OnEnable();

            }

            m_MenuPart.m_EnhanceCB.gameObject.SetActive(false);
            m_Enhance.onEnhancedTimeUp = null;
            m_Enhance = null;
        }
        else if (entity == m_Repair)
        {
            if (m_MenuPart.m_RepairCB.isChecked)
            {
                if (m_MenuPart.m_EnhanceCB.gameObject.activeSelf)
                    m_CurType = CSConst.etEnhance;
                else if (m_MenuPart.m_RecyleCB.gameObject.activeSelf)
                    m_CurType = CSConst.etRecyle;
                else
                    m_CurType = CSConst.etUnknow;

                OnEnable();
            }

            m_MenuPart.m_RepairCB.gameObject.SetActive(false);
            m_Repair.onRepairedTimeUp = null;
            m_Repair = null;

        }
        else if (entity == m_Recycle)
        {
            if (m_MenuPart.m_RecyleCB.isChecked)
            {
                if (m_MenuPart.m_EnhanceCB.gameObject.activeSelf)
                    m_CurType = CSConst.etEnhance;
                else if (m_MenuPart.m_RepairCB.gameObject.activeSelf)
                    m_CurType = CSConst.etRepair;
                else
                    m_CurType = CSConst.etUnknow;

                if (gameObject.activeInHierarchy)
                    OnEnable();
            }

            m_MenuPart.m_RecyleCB.gameObject.SetActive(false);
            m_Recycle = null;
        }


        m_MenuPart.m_Root.repositionNow = true;
    }

    #region CS_CALLBACK

    void OnRecycled()
    {
        if (m_CurType == m_Recycle.m_Type)
            m_SubEngneering.SetItem(null);
    }
    #endregion

    #region NGUI_CALLBACK

    bool IsUnLock(int type)
    {
        if (GameUI.Instance == null || GameUI.Instance.mSkillWndCtrl == null || GameUI.Instance.mSkillWndCtrl._SkillMgr == null)
            return true;
        return GameUI.Instance.mSkillWndCtrl._SkillMgr.CheckUnlockColony(type);
    }

    void OnEnhanceActivate(bool active)
    {
        if (!active)
            return;

        m_SubEngneering.SetEntity(m_Enhance);
        m_CurType = m_Enhance.m_Type;
        m_SkillLock.gameObject.SetActive(!IsUnLock(m_CurType));
    }

    void OnRepairActivate(bool active)
    {
        if (!active)
            return;

        m_SubEngneering.SetEntity(m_Repair);
        m_CurType = m_Repair.m_Type;
        m_SkillLock.gameObject.SetActive(!IsUnLock(m_CurType));
    }

    void OnRecycleActivate(bool active)
    {
        if (!active)
            return;

        m_SubEngneering.SetEntity(m_Recycle);
        m_CurType = m_Recycle.m_Type;
        m_SkillLock.gameObject.SetActive(!IsUnLock(m_CurType));
    }

    void OnOKBtn()
    {
        if (m_CurType == CSConst.etEnhance)
            MessageBox_N.ShowYNBox(PELocalization.GetString(8000097), StartToWork);
        else if (m_CurType == CSConst.etRepair)
            MessageBox_N.ShowYNBox(PELocalization.GetString(8000098), StartToWork);
        else if (m_CurType == CSConst.etRecyle)
            MessageBox_N.ShowYNBox(PELocalization.GetString(8000099), StartToWork);
    }

    void StartToWork()
    {
        if (!GameConfig.IsMultiMode)
        {
            //lw:2017.4.6 crash PeCreature.Instance.mainPlayer可能为null
            if (PeCreature.Instance != null && PeCreature.Instance.mainPlayer != null)
            {
                PlayerPackageCmpt pkg = PeCreature.Instance.mainPlayer.packageCmpt as PlayerPackageCmpt;
                if (pkg != null)
                {
                    ItemPackage accessor = pkg.package._playerPak;
                    if (m_CurType == CSConst.etEnhance)
                    {
                        //lw:2017.4.6 crash 
                        if (m_Enhance != null && m_Enhance.m_Item != null && m_Enhance.m_Item.protoData != null)
                        {
                            // take out material from player
                            foreach (CSUI_MaterialGrid matGrid in m_MatList)
                                accessor.Destroy(matGrid.ItemID, matGrid.NeedCnt);

                            m_Enhance.StartCounter();

                            CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mStartToEnhance.GetString(), m_Enhance.m_Item.protoData.GetName()));

                            // Popoup hints
                            foreach (CSUI_MaterialGrid mg in m_MatList)
                            {
                                Vector3 pos = mg.transform.position;
                                CSUI_MainWndCtrl.CreatePopupHint(pos, transform, new Vector3(10, -2, -8), " - " + mg.NeedCnt, false);
                            }
                        }
                        
                    }
                    else if (m_CurType == CSConst.etRepair)
                    {
                        if(null != m_Repair && m_Repair.m_Item != null && m_Repair.m_Item.protoData != null)
                        {
                            // take out material from player
                            foreach (CSUI_MaterialGrid matGrid in m_MatList)
                                accessor.Destroy(matGrid.ItemID, matGrid.NeedCnt);

                            m_Repair.StartCounter();
                            CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mStartToRepair.GetString(), m_Repair.m_Item.protoData.GetName()));
                        }
                    }
                    else if (m_CurType == CSConst.etRecyle)
                    {
                        if(m_Recycle != null && m_Recycle.m_Item != null && m_Recycle.m_Item.protoData !=null)
                        {
                            m_Recycle.StartCounter();
                            CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mStartToRecycle.GetString(), m_Recycle.m_Item.protoData.GetName()));
                        }
                    }
                }
            }
        }
       else  
       {
            //multimode
            if (m_CurType == CSConst.etEnhance)
            {
                m_Enhance._ColonyObj._Network.EHN_Start();
            }
            else if (m_CurType == CSConst.etRepair)
            {
                m_Repair._ColonyObj._Network.RPA_Start();
            }
            else if (m_CurType == CSConst.etRecyle)
            {
                m_Recycle._ColonyObj._Network.RCY_Start();
            }
        }



    }

    void OnResetBtn()
    {
        MessageBox_N.ShowYNBox(PELocalization.GetString(8000100), StopWork);
    }

    void StopWork()
    {
        if (!GameConfig.IsMultiMode)
        {
            if (m_CurType == CSConst.etEnhance)
            {
                m_Enhance.StopCounter();
            }
            else if (m_CurType == CSConst.etRepair)
            {
                m_Repair.StopCounter();
            }
            else if (m_CurType == CSConst.etRecyle)
            {
                m_Recycle.StopCounter();
            }
        }
        else
        {
            //multimode
            if (m_CurType == CSConst.etEnhance)
            {
                m_Enhance._ColonyObj._Network.EHN_Stop();
            }
            else if (m_CurType == CSConst.etRepair)
            {
                m_Repair._ColonyObj._Network.RPA_Stop();
            }
            else if (m_CurType == CSConst.etRecyle)
            {
                m_Recycle._ColonyObj._Network.RCY_Stop();
            }
        }

    }


    #endregion

    #region SUB_ENGNEERING_CALLBACK

    void OnEnhanceItemChanged(ItemObject item)
    {
        ClearMaterials();

        if (item == null)
            return;


        Dictionary<int, int> costsList = m_Enhance.GetCostsItems();

        if (costsList != null)
        {
            foreach (KeyValuePair<int, int> kvp in costsList)
            {
                if (kvp.Value != 0)
                    AddMaterials(kvp.Key, kvp.Value);
            }

            m_MatPart.m_Root.repositionNow = true;
        }

    }

    void OnRepairItemChanged(ItemObject item)
    {
        ClearMaterials();

        if (item == null)
            return;

        Dictionary<int, int> costsList = m_Repair.GetCostsItems();

        if (costsList != null)
        {
            foreach (KeyValuePair<int, int> kvp in costsList)
            {
                if (kvp.Value != 0)
                    AddMaterials(kvp.Key, kvp.Value);
            }

            m_MatPart.m_Root.repositionNow = true;
        }
    }

    void OnRecycleItemChanged(ItemObject item)
    {
        ClearMaterials();

        if (item == null)
            return;

        Dictionary<int, int> getsList = m_Recycle.GetRecycleItems();

        if (getsList != null)
        {
            foreach (KeyValuePair<int, int> kvp in getsList)
            {
                if (kvp.Value != 0)
                    AddMaterials(kvp.Key, kvp.Value, false);
            }

            m_MatPart.m_Root.repositionNow = true;
        }
    }

    #endregion

    #region MACHINE_CALLBACK

    // Enhanced time up
    void OnEnhancedTimeUp(Strengthen item)
    {
        m_SubEngneering.UpdatePopupHintInfo(m_Enhance);
        m_SubEngneering.ItemGrid.PlayGlow(true);
    }

    void OnRepairedTimeUp(Repair item)
    {
        //item.Do();
        m_SubEngneering.UpdatePopupHintInfo(m_Repair);
    }

    #endregion

    void ClearMaterials()
    {
        foreach (CSUI_MaterialGrid mg in m_MatList)
        {
            if (mg.gameObject != null)
                GameObject.DestroyImmediate(mg.gameObject);
        }

        m_MatList.Clear();
        mListScrolBar.scrollValue = 0f;
    }

    void AddMaterials(int item_id, int item_count, bool bUseColors = true)
    {
        CSUI_MaterialGrid mg = Instantiate(m_MatPart.m_Prefab) as CSUI_MaterialGrid;
        mg.transform.parent = m_MatPart.m_Root.transform;
        mg.transform.localPosition = Vector3.zero;
        mg.transform.localRotation = Quaternion.identity;
        mg.transform.localScale = Vector3.one;
        mg.ItemID = item_id;
        mg.NeedCnt = item_count;
        mg.bUseColors = bUseColors;

        m_MatList.Add(mg);
    }

    void OnEnable()
    {
        SelectByType();
    }

    void SelectByType()
    {
        if (m_CurType == CSConst.etEnhance)
        {
            m_MenuPart.m_EnhanceCB.isChecked = true;
            OnEnhanceActivate(true);
        }
        else if (m_CurType == CSConst.etRepair)
        {
            m_MenuPart.m_RepairCB.isChecked = true;
            OnRepairActivate(true);
        }
        else if (m_CurType == CSConst.etRecyle)
        {
            m_MenuPart.m_RecyleCB.isChecked = true;
            OnRecycleActivate(true);
        }
    }

    void Awake()
    {
        m_SubEngneering.onEnhancedItemChanged = OnEnhanceItemChanged;
        m_SubEngneering.onRepairedItemChanged = OnRepairItemChanged;
        m_SubEngneering.onRecycleItemChanged = OnRecycleItemChanged;
    }

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (PeCreature.Instance.mainPlayer == null)
            return;
        if (playerPackageCmpt == null || player == null || player != PeCreature.Instance.mainPlayer)
        {
            player = PeCreature.Instance.mainPlayer;
            playerPackageCmpt = player.GetCmpt<PlayerPackageCmpt>();
        }

        bool bEnoughMats = true;
        if (m_CurType != CSConst.etRecyle)
        {
            foreach (CSUI_MaterialGrid mg in m_MatList)
            {
                mg.ItemNum = playerPackageCmpt.package.GetCount(mg.ItemID);
                if (mg.ItemNum < mg.NeedCnt)
                    bEnoughMats = false;
            }
        }
        else
        {
            foreach (CSUI_MaterialGrid mg in m_MatList)
                mg.ItemNum = 0;
        }

        // Enable or Disable the Handle button
        if (m_CurType == CSConst.etEnhance)
        {
            if (m_Enhance.m_Item == null || !m_Enhance.IsRunning)
            {
                m_Handle.m_OKBtn.isEnabled = false;
                m_Handle.m_ResetBtn.isEnabled = false;
            }
            else
            {
                if (!m_Enhance.isDeleting && !m_Enhance.IsEnhancing && bEnoughMats)
                    m_Handle.m_OKBtn.isEnabled = true;
                else
                    m_Handle.m_OKBtn.isEnabled = false;

                if (bEnoughMats)
                    m_Handle.m_ResetBtn.isEnabled = !m_Handle.m_OKBtn.isEnabled;
                else
                    m_Handle.m_ResetBtn.isEnabled = false;
            }
        }
        else if (m_CurType == CSConst.etRepair)
        {
            if (m_Repair.m_Item == null || !m_Repair.IsRunning||m_Repair.m_Item.GetValue().ExpendValue == 0f)
            {
                m_Handle.m_OKBtn.isEnabled = false;
                m_Handle.m_ResetBtn.isEnabled = false;
            }
            else
            {
                if (m_Repair.m_Item != null && m_Repair.IsFull())
                    m_Handle.m_OKBtn.isEnabled = false;
                else if (!m_Repair.isDeleting && !m_Repair.IsRepairingM && bEnoughMats)
                    m_Handle.m_OKBtn.isEnabled = true;
                else
                    m_Handle.m_OKBtn.isEnabled = false;

                if (bEnoughMats)
                    m_Handle.m_ResetBtn.isEnabled = !m_Handle.m_OKBtn.isEnabled;
                else
                    m_Handle.m_ResetBtn.isEnabled = false;
            }
        }
        else if (m_CurType == CSConst.etRecyle)
        {
            if (m_Recycle.m_Item != null && !m_Recycle.isDeleting
                && !m_Recycle.IsRecycling && m_Recycle.IsRunning)
                m_Handle.m_OKBtn.isEnabled = true;
            else
                m_Handle.m_OKBtn.isEnabled = false;

            if (m_Recycle.m_Item == null || !m_Recycle.IsRunning)
            {
                m_Handle.m_OKBtn.isEnabled = false;
                m_Handle.m_ResetBtn.isEnabled = false;
            }
            else
            {
                if (!m_Recycle.isDeleting && !m_Recycle.IsRecycling)
                    m_Handle.m_OKBtn.isEnabled = true;
                else
                    m_Handle.m_OKBtn.isEnabled = false;

                m_Handle.m_ResetBtn.isEnabled = !m_Handle.m_OKBtn.isEnabled;
            }
        }
    }


    #region multimode
    public void StartWorkerResult(int type, CSEntity m_entity, string rolename)
    {
        if (type == m_CurType)
        {
            if (m_CurType == CSConst.etEnhance)
            {
                if ((CSEntity)m_Enhance != m_entity)
                {
                    return;
                }
                CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mStartToEnhance.GetString(), m_Enhance.m_Item.protoData.GetName()));

                if (PeCreature.Instance.mainPlayer.GetCmpt<EntityInfoCmpt>().characterName.givenName == rolename)
                {
                    // Popoup hints
                    foreach (CSUI_MaterialGrid mg in m_MatList)
                    {
                        Vector3 pos = mg.transform.position;
                        CSUI_MainWndCtrl.CreatePopupHint(pos, transform, new Vector3(10, -2, -8), " - " + mg.NeedCnt, false);
                    }
                }

            }
            else if (m_CurType == CSConst.etRepair)
            {
                if ((CSEntity)m_Repair != m_entity)
                {
                    return;
                }
                CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mStartToRepair.GetString(), m_Repair.m_Item.protoData.GetName()));
            }
            else if (m_CurType == CSConst.etRecyle)
            {
                if ((CSEntity)m_Recycle != m_entity)
                {
                    return;
                }
                CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mStartToRecycle.GetString(), m_Recycle.m_Item.protoData.GetName()));
            }
//            CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mStartToRepair.GetString(), m_Repair.m_Item.protoData.GetName()));
        }
    }
    #endregion
}
