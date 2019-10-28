using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using Pathea.PeEntityExt;
public class CSUI_NPCOccupation : MonoBehaviour
{
    #region UI_WIDGET

    [SerializeField]
    UIPopupList m_OccupationUI;

    [SerializeField]
    UISprite m_DwellerIconUI;
    [SerializeField]
    UISprite m_WorkerIconUI;
    [SerializeField]
    UISprite m_FarmerIconUI;
    [SerializeField]
    UISprite m_SoldierIconUI;
    [SerializeField]
    UISprite m_FollowerIcomUI;
    [SerializeField]
    UISprite m_ProcessorIcomUI;
    [SerializeField]
    UISprite m_DoctorIconUI;
    [SerializeField]
    UISprite m_InstructorIconUI;

    #endregion

    private CSPersonnel m_RefNpc;
    public CSPersonnel RefNpc
    {
        get { return m_RefNpc; }
        set
        {
            m_RefNpc = value;
            UpdatePopupList();
        }
    }

    public delegate void SelectItemDel(string item);
    public SelectItemDel onSelectChange;

    #region ACTIVE_PART

    private bool m_Active = true;
    public void Activate(bool active)
    {
        if (m_Active != active)
        {
            m_Active = active;
            _activate();
        }
        else
            m_Active = active;
    }

    private void _activate()
    {
        if (!m_Active)
        {
            m_OccupationUI.items.Clear();
            if (m_RefNpc != null)
                m_OccupationUI.items.Add(CSUtils.GetOccupaName(m_RefNpc.Occupation));
            else
                m_OccupationUI.items.Add(CSUtils.GetOccupaName(CSConst.potDweller));
        }
        else
            UpdatePopupList();
    }

    #endregion

    void Awake()
    {
        CSPersonnel.RegisterOccupaChangedListener(OnOccupationChanged);
    }

    void OnDestroy()
    {
        CSPersonnel.UnRegisterStateChangedListener(OnOccupationChanged);
    }

    // Use this for initialization
    void Start()
    {
        _activate();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdatePopupList()
    {
        if (!m_Active)
        {
            _activate();
            return;
        }

        m_OccupationUI.items.Clear();
        if (m_RefNpc == null)
        {
            m_OccupationUI.items.Add("None");
            return;
        }

        //Soldier
        List<CSEntity> entities = m_RefNpc.GetProtectedEntities();
        if (entities != null)
            m_OccupationUI.items.Add(CSUtils.GetOccupaName(CSConst.potSoldier));

        //Farm
        CSMgCreator mgCreator = RefNpc.m_Creator as CSMgCreator;
        if (null != mgCreator && mgCreator.Assembly != null)
        {
            int farmCnt = mgCreator.Assembly.GetEntityCnt(CSConst.ObjectType.Farm);
            if (farmCnt != 0)
                m_OccupationUI.items.Add(CSUtils.GetOccupaName(CSConst.potFarmer));
        }

        //Doctor
        CSCreator creator = CSUI_MainWndCtrl.Instance.Creator;
        if (creator == null)
            return;
        Dictionary<int, CSCommon> commons = creator.GetCommonEntities();
        foreach (KeyValuePair<int, CSCommon> kvp in commons)
        {
            if (kvp.Value.Assembly != null && kvp.Value.WorkerMaxCount > 0 && (kvp.Value as CSHealth) != null)
            {
                m_OccupationUI.items.Add(CSUtils.GetOccupaName(CSConst.potDoctor));
                break;
            }
        }

        //Worker
        foreach (KeyValuePair<int, CSCommon> kvp in commons)
        {
            if (kvp.Value.Assembly != null && kvp.Value.WorkerMaxCount > 0 && (kvp.Value as CSWorkerMachine) != null)
            {
                m_OccupationUI.items.Add(CSUtils.GetOccupaName(CSConst.potWorker));
                break;
            }
        }

        //Train
        if (CSUI_MainWndCtrl.Instance.m_Menu.m_TrainingMI.IsShow)
            m_OccupationUI.items.Add(CSUtils.GetOccupaName(CSConst.potTrainer));

        //Processor
        if (CSUI_MainWndCtrl.Instance.m_Menu.m_CollectMI.IsShow)
            m_OccupationUI.items.Add(CSUtils.GetOccupaName(CSConst.potProcessor));

        m_OccupationUI.items.Add(CSUtils.GetOccupaName(CSConst.potDweller));
        //m_OccupationUI.items.Add(CSUtils.GetOccupaName(CSConst.potWorker));
        //m_OccupationUI.items.Add(CSUtils.GetOccupaName(CSConst.potFarmer));
        //m_OccupationUI.items.Add(CSUtils.GetOccupaName(CSConst.potSoldier));
        //if (m_RefNpc.IsRandomNpc)//--to do: test
        if (m_RefNpc.IsRandomNpc)
            m_OccupationUI.items.Add(CSUtils.GetOccupaName(CSConst.potFollower));
        //m_OccupationUI.items.Add(CSUtils.GetOccupaName(CSConst.potProcessor));
        //m_OccupationUI.items.Add(CSUtils.GetOccupaName(CSConst.potDoctor));
        //m_OccupationUI.items.Add(CSUtils.GetOccupaName(CSConst.potTrainer));

        ShowStatusTips = false;
        m_OccupationUI.selection = CSUtils.GetOccupaName(m_RefNpc.m_Occupation);
        ShowStatusTips = true;
    }

    #region CALL_BACK

    bool ShowStatusTips = true;
    void OnSelectionChange(string item)
    {
        if (null==m_RefNpc||null == m_RefNpc.NPC)
        {
            return;
        }

        //lz-2016.09.14 npc死亡之后不可以切换职业
        if (m_RefNpc.NPC.aliveEntity.isDead)
        {
            CSUI_StatusBar.ShowText(UIMsgBoxInfo.mNpdDeadNotChangeProfession.GetString(), Color.red, 5.5f);
            m_OccupationUI.selection = CSUtils.GetOccupaName(m_RefNpc.m_Occupation);
            return;
        }
        HideAllProfessionalSpr();
        if (item == CSUtils.GetOccupaName(CSConst.potDweller))
        {
            if (m_RefNpc.TrySetOccupation(CSConst.potDweller))
            {
                m_DwellerIconUI.enabled = true;
                if (onSelectChange != null)
                    onSelectChange(item);
                if (ShowStatusTips)
					CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mProfessionForDweller.GetString(), m_RefNpc.FullName), 5.5f);
            }
            else
            {
                //lz-2016.10.26 不能去直接设置m_OccupationUI.textLabel.text的值，那样m_OccupationUI.selection没有被改变，影响UIPoupList的正常功能
                m_OccupationUI.selection = CSUtils.GetOccupaName(m_RefNpc.m_Occupation);
                if (ShowStatusTips)
					CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mProfessionFailed.GetString(), m_RefNpc.FullName), 5.5f);
            }
        }
        else if (item == CSUtils.GetOccupaName(CSConst.potWorker))
        {
            if (m_RefNpc.TrySetOccupation(CSConst.potWorker))
            {
                m_WorkerIconUI.enabled = true;
                if (onSelectChange != null)
                    onSelectChange(item);
                if (ShowStatusTips)
					CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mProfessionForWorker.GetString(), m_RefNpc.FullName), 5.5f);
            }
            else
            {
                m_OccupationUI.selection = CSUtils.GetOccupaName(m_RefNpc.m_Occupation);
                if (ShowStatusTips)
					CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mProfessionFailed.GetString(), m_RefNpc.FullName), 5.5f);
            }
        }
        else if (item == CSUtils.GetOccupaName(CSConst.potFarmer))
        {
            if (CSMain.s_MgCreator.Farmers.Count >= CSFarm.MAX_WORKER_COUNT){

                m_OccupationUI.selection = CSUtils.GetOccupaName(m_RefNpc.m_Occupation);
				if (ShowStatusTips)
					CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(PELocalization.GetString(ColonyMessage.OCCUPATION_NPC_TOO_MANY), m_RefNpc.FullName), 5.5f);
                return;
			}

            if (m_RefNpc != null)
            {
                if (m_RefNpc.TrySetOccupation(CSConst.potFarmer))
                {
                    m_FarmerIconUI.enabled = true;
                    if (onSelectChange != null)
                        onSelectChange(item);
                    if (ShowStatusTips)
						CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mProfessionForFarmer.GetString(), m_RefNpc.FullName), 5.5f);
                }
                else
                {
                    m_OccupationUI.selection = CSUtils.GetOccupaName(m_RefNpc.m_Occupation);
                    if (ShowStatusTips)
						CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mProfessionFailed.GetString(), m_RefNpc.FullName), 5.5f);
                }
            }
        }
        else if (item == CSUtils.GetOccupaName(CSConst.potSoldier))
        {
            if (m_RefNpc != null)
            {
                if (m_RefNpc.TrySetOccupation(CSConst.potSoldier))
                {
                    m_SoldierIconUI.enabled = true;
                    if (onSelectChange != null)
                        onSelectChange(item);
                    if (ShowStatusTips)
						CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mProfessionForSolider.GetString(), m_RefNpc.FullName), 5.5f);
                }
                else
                {
                    m_OccupationUI.selection = CSUtils.GetOccupaName(m_RefNpc.m_Occupation);
                    if (ShowStatusTips)
						CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mProfessionFailed.GetString(), m_RefNpc.FullName), 5.5f);
                }
            }
        }
        else if (item == CSUtils.GetOccupaName(CSConst.potFollower))
        {
            if (m_RefNpc != null)
            {
                if (m_RefNpc.TrySetOccupation(CSConst.potFollower))
                {
                    m_FollowerIcomUI.enabled = true;
                    if (onSelectChange != null)
                        onSelectChange(item);
                    if (ShowStatusTips)
						CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mProfessionForFollower.GetString(), m_RefNpc.FullName), 5.5f);
                }
                else
                {
                    m_OccupationUI.selection = CSUtils.GetOccupaName(m_RefNpc.m_Occupation);
                    if (ShowStatusTips)
						CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mProfessionFailed.GetString(), m_RefNpc.FullName), 5.5f);
                }
            }
        }
        else if (item == CSUtils.GetOccupaName(CSConst.potProcessor))
        {
            if (CSMain.s_MgCreator.Processors.Count >= ProcessingConst.WORKER_AMOUNT_MAX)
			{
                m_OccupationUI.selection = CSUtils.GetOccupaName(m_RefNpc.m_Occupation);
				if (ShowStatusTips)
					CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(PELocalization.GetString(ColonyMessage.OCCUPATION_NPC_TOO_MANY), m_RefNpc.FullName), 5.5f);
                return;
			}
            if (m_RefNpc != null)
            {
                if (m_RefNpc.TrySetOccupation(CSConst.potProcessor))
                {
                    m_ProcessorIcomUI.enabled = true;
                    if (onSelectChange != null)
                        onSelectChange(item);
                    if (ShowStatusTips)
						CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mProfessionForProcessor.GetString(), m_RefNpc.FullName), 5.5f);
                }
                else
                {
                    m_OccupationUI.selection = CSUtils.GetOccupaName(m_RefNpc.m_Occupation);
                    if (ShowStatusTips)
						CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mProfessionFailed.GetString(), m_RefNpc.FullName), 5.5f);
                }
            }
        }
        else if (item == CSUtils.GetOccupaName(CSConst.potDoctor))
        {
            if (m_RefNpc != null)
            {
                if (m_RefNpc.TrySetOccupation(CSConst.potDoctor))
                {
                    m_DoctorIconUI.enabled = true;
                    if (onSelectChange != null)
                        onSelectChange(item);
                    if (ShowStatusTips)
						CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mProfessionForDoctor.GetString(), m_RefNpc.FullName), 5.5f);
                }
                else
                {
                    m_OccupationUI.selection = CSUtils.GetOccupaName(m_RefNpc.m_Occupation);
                    if (ShowStatusTips)
						CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mProfessionFailed.GetString(), m_RefNpc.FullName), 5.5f);
                }
            }
        }
        else if (item == CSUtils.GetOccupaName(CSConst.potTrainer))
        {
            if (m_RefNpc != null)
            {
                if (m_RefNpc.TrySetOccupation(CSConst.potTrainer))
                {
                    m_InstructorIconUI.enabled = true;
                    if (onSelectChange != null)
                        onSelectChange(item);
                    if (ShowStatusTips)
						CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mProfessionForInstructor.GetString(), m_RefNpc.FullName), 5.5f);
                }
                else
                {
                    m_OccupationUI.selection = CSUtils.GetOccupaName(m_RefNpc.m_Occupation);
                    if (ShowStatusTips)
						CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mProfessionFailed.GetString(), m_RefNpc.FullName), 5.5f);
                }
            }
        }

        if (GameConfig.IsMultiMode)
        {
            //--to do: wait
            //if(m_RefNpc.m_Npc.Netlayer is AiAdNpcNetwork)
            //    ((AiAdNpcNetwork)m_RefNpc.m_Npc.Netlayer).SetClnOccupation(m_RefNpc.m_Occupation);
        }
    }

    void HideAllProfessionalSpr()
    {
        m_DwellerIconUI.enabled = false;
        m_WorkerIconUI.enabled = false;
        m_FarmerIconUI.enabled = false;
        m_SoldierIconUI.enabled = false;
        m_FollowerIcomUI.enabled = false;
        m_ProcessorIcomUI.enabled = false;
        m_DoctorIconUI.enabled = false;
        m_InstructorIconUI.enabled = false;
    }

    void OnOccupationChanged(CSPersonnel person, int prvState)
    {
        if (person != m_RefNpc || !person.IsRandomNpc)
            return;
        if (m_OccupationUI.selection != CSUtils.GetOccupaName(person.Occupation))
        {
            m_OccupationUI.selection = CSUtils.GetOccupaName(person.Occupation);
        }
    }

    // On Click the popupList
    void OnPopupListClick()
    {
        if (!m_Active)
        {
            CSUI_StatusBar.ShowText(UIMsgBoxInfo.mCantHandlePersonnel.GetString(), Color.red, 5.5f);
        }
    }

    #endregion
}
