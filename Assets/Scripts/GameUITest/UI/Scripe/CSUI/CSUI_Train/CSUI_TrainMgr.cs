using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using SkillAsset;
using System;
using Pathea;
using Pathea.PeEntityExt;
using Mono.Data.SqliteClient;
using SkillSystem;


public class CSUI_TrainMgr : MonoBehaviour
{

    private static CSUI_TrainMgr _instance;
    public static CSUI_TrainMgr Instance { get { return _instance; } }


    public bool TrainIsOpen { get { return gameObject.activeInHierarchy; } }
    public bool TraineeIsChecked
    {
        get
        {
            if (m_RefNpc == null || m_RefNpc.Occupation != 0)
                return false;
            else
                return true;
        }
    }
    [HideInInspector]
    public bool mTrainingLock = false;//学技能或者提升属性时为true


    [SerializeField]
    CSUI_SkillItem m_SkillPrefab;
    [SerializeField]
    UIGrid m_TraineeSkillRoot;
    [SerializeField]
    UIGrid m_InstructorSkillRoot;
    [SerializeField]
    UIPopupList uip;
    [SerializeField]
    UILabel uipLabel;


    public enum TypeEnu // 学员和教练枚举
    {
        Instructor, //教练
        Trainee  //学员   
    }

    public UIGrid m_NpcRootUI;
    public UIGrid m_TraineeRootUI;
    public UIGrid m_InstructorRootUI;
    public CSUI_NpcGridItem m_NpcGridPrefab;


    public CSUI_TrainNpcInfCtrl m_TrainNpcInfCtrl;
    public CSUI_TrainLearnPageCtrl m_TrainLearnPageCtrl;

    [SerializeField]
    UICheckbox mInsCheckBox;
    [SerializeField]
    UICheckbox mTraCheckBox;

    private CSPersonnel m_LearnPageSelectNpc;


    //属性
    private TypeEnu m_Type;
    public TypeEnu Type
    {
        get { return m_Type; }
        set
        {
            m_Type = value;

            m_TrainLearnPageCtrl.UpdateSetBtnsState(m_Type);

            if (value == TypeEnu.Trainee)
            {
                m_TraineeRootUI.gameObject.SetActive(true);
                m_InstructorRootUI.gameObject.SetActive(false);
                RefreshNPCGrids();
                //CSUI_NpcGridItem activeGrid = null;
                //for (int i = 0; i < m_TraineeRootUI.transform.childCount; ++i)
                //{
                //    UICheckbox cb = m_TraineeRootUI.transform.GetChild(i).gameObject.GetComponent<UICheckbox>();
                //    if (cb.isChecked)
                //    {
                //        activeGrid = cb.gameObject.GetComponent<CSUI_NpcGridItem>();
                //        break;
                //    }
                //}
                //ActiveNpcGrid = activeGrid;

            }
            else if (value == TypeEnu.Instructor)
            {
                m_TraineeRootUI.gameObject.SetActive(false);
                m_InstructorRootUI.gameObject.SetActive(true);
                RefreshNPCGrids();
                //CSUI_NpcGridItem activeGrid = null;
                //for (int i = 0; i < m_InstructorRootUI.transform.childCount; ++i)
                //{
                //    UICheckbox cb = m_InstructorRootUI.transform.GetChild(i).gameObject.GetComponent<UICheckbox>();
                //    if (cb.isChecked)
                //    {
                //        activeGrid = cb.gameObject.GetComponent<CSUI_NpcGridItem>();
                //        break;
                //    }
                //}
                //ActiveNpcGrid = activeGrid;
            }
        }
    }

    // Active NPC Grid
    private CSUI_NpcGridItem m_ActiveNpcGrid;
    public CSUI_NpcGridItem ActiveNpcGrid
    {
        get { return m_ActiveNpcGrid; }
        set
        {
            m_ActiveNpcGrid = value;
            UpdateNPCRef(m_ActiveNpcGrid == null ? null : m_ActiveNpcGrid.m_Npc);
        }
    }

    void UpdateNPCRef(CSPersonnel npc)
    {
        RefNpc = npc;
    }




    private CSPersonnel m_RefNpc;
    public CSPersonnel RefNpc
    {
        get { return m_RefNpc; }
        set
        {
            m_RefNpc = value;
            m_TrainNpcInfCtrl.Npc = value;
            //UpdateTraineeSkills();
            ////UpdateItemGrid();
            ////UpdateSkills();
            //if (m_RefNpc != null && m_RefNpc.Occupation != 0)
            //    GetInstructorSkill();
            //if (m_RefNpc != null && m_RefNpc.Occupation == 0)
            //    GetNpcPackage();
            //else
            //    ClearNpcPackage();

            GetNpcPackage();

            if (UpdateInfo != null)
                UpdateInfo();

        }
    }



    public event Action UpdateInfo = null;


	List<NpcAbility> _traiuneeSkillLis = new List<NpcAbility> ();
    private List<CSUI_SkillItem> m_TraineeSkillGrids = new List<CSUI_SkillItem>(1);
    
	List<NpcAbility> _instructorSkillLis = new List<NpcAbility> ();
	private List<CSUI_SkillItem> m_InstructorSkillGrids = new List<CSUI_SkillItem>(1);

    public void UpdateTraineeSkillsShow(CSPersonnel _trainee)
    {
        if (_trainee == null)
        {
            for (int i = 0; i < m_TraineeSkillGrids.Count; i++)
            {
                m_TraineeSkillGrids[i].SetSkill("Null");
            }
        }
        else
        {
            for (int i = 0; i < m_TraineeSkillGrids.Count; i++)
            {
                m_TraineeSkillGrids[i].SetSkill("Null");
            }
			_traiuneeSkillLis = GetNpcSkills(_trainee);
			if (_traiuneeSkillLis.Count == 0)
                return;
            for (int i = 0; i < m_TraineeSkillGrids.Count; i++)
            {
				if (i < _traiuneeSkillLis.Count)
					m_TraineeSkillGrids[i].SetSkill(_traiuneeSkillLis[i].icon, _traiuneeSkillLis[i]);
            }
        }
    }

    public void UpdateInstructorSkillsShow(CSPersonnel _instructor)
    {
        if (_instructor == null)
        {
            for (int i = 0; i < m_InstructorSkillGrids.Count; i++)
            {
                m_InstructorSkillGrids[i].SetSkill("Null");
            }
        }
        else
        {
            for (int i = 0; i < m_InstructorSkillGrids.Count; i++)
            {
                m_InstructorSkillGrids[i].SetSkill("Null");
            }
            _instructorSkillLis = GetNpcSkills(_instructor);
			if (_instructorSkillLis.Count == 0)
                return;
            for (int i = 0; i < m_InstructorSkillGrids.Count; i++)
            {
				if (i < _instructorSkillLis.Count)
					m_InstructorSkillGrids[i].SetSkill(_instructorSkillLis[i].icon, _instructorSkillLis[i]);
            }
        }
    }

	public void ApplyDataToUI(){
		if (_instructorSkillLis.Count != 0){
			for (int i = 0; i < _instructorSkillLis.Count; i++)
			{
				if (i < m_InstructorSkillGrids.Count)
					m_InstructorSkillGrids[i].SetSkill(_instructorSkillLis[i].icon, _instructorSkillLis[i]);
			}
		}
		if (_traiuneeSkillLis.Count != 0){
			for (int i = 0; i < _traiuneeSkillLis.Count; i++)
			{
				if (i < m_TraineeSkillGrids.Count)
					m_TraineeSkillGrids[i].SetSkill(_traiuneeSkillLis[i].icon, _traiuneeSkillLis[i]);
			}
		}
		if (m_StudyList.Count != 0){
			for (int i = 0; i < m_StudyList.Count; i++)
			{
				if (i < m_StudySkillGrids.Count)
					m_StudySkillGrids[i].SetSkill(m_StudyList[i].icon, m_StudyList[i]);
			}
		}
	}



    private List<NpcAbility> GetNpcSkills(CSPersonnel _npc)//拿NPC技能
    {
        if (_npc == null)
            return null;
        List<NpcAbility> _skills;
        Ablities _skillId;
        _skillId = _npc.m_Npc.NpcCmpt.AbilityIDs;
        _skills = NpcAblitycmpt.FindAblitysById(_skillId);
        return _skills;
    }



    private List<NpcAbility> GetNpcLevelList(AblityType _type, CSPersonnel _npc)//拿到技能等级
    {
        if (_npc == null)
            return null;
        List<NpcAbility> _lis = NpcAblitycmpt.GetAbilityByType(_type);//_npc.m_Npc.GetCmpt<NpcCmpt>().Npcskillcmpt.GetAbilityByType(_type);
        return _lis;
    }


    //private List<int> mSkills = new List<int>();//当前选中一个教练时，教练有的技能的ID
    //private Dictionary<string, int> _skNameIdLis = new Dictionary<string, int>();//用来存放技能名字和ID，便于通过选择的名字找到技能
    public UILabel _skLabel;
    //List<NpcAbility> _skillLis = new List<NpcAbility>();//教练技能暂存
    //Dictionary<int, List<NpcAbility>> LevelDic = new Dictionary<int, List<NpcAbility>>();//教练技能

    //private void GetInstructorSkill()//得到教练技能
    //{
    //    if (m_RefNpc == null || m_RefNpc.Occupation == 0)
    //        return;
    //    uip.items.Clear();
    //    //uip.items.Add("InstructorSkills");
    //    _skLabel.text = "InstructorSkills";
    //    //mSkills.Clear();
    //    _skNameIdLis.Clear();
    //    //int[] _skillId = m_RefNpc.m_Npc.GetCmpt<NpcCmpt>().Abilities;//技能ID
    //    //if (_skillId.Length <= 0)
    //    //    return;
    //    _skillLis = GetNpcSkills(m_RefNpc);
    //    LevelDic.Clear();
    //    foreach (NpcAbility na in _skillLis)
    //    {
    //        LevelDic.Add(na.id, GetNpcLevelList(na.Type, m_RefNpc));
    //    }
    //    if (_skillLis == null)
    //        return;
    //    for (int i = 0; i < _skillLis.Count; i++)
    //    {
    //        NpcAbility na = _skillLis[i];
    //        string _skName = GetSkillNameBySkData(na);
    //        uip.items.Add(_skName);
    //        _skNameIdLis.Add(_skName, na.id);
    //        //mSkills.Add(_skillId[i]);
    //    }
    //    uip.items.Add("......");
    //}

    //private string GetSkillNameBySkData(NpcAbility _sd)//通过技能得到技能名字
    //{
    //    string _desc = PELocalization.GetString(_sd.desc);
    //    if (_desc == "")
    //        return null;
    //    string[] sArray = _desc.Split(new string[] { "[00FFFF]", "[-]" }, StringSplitOptions.RemoveEmptyEntries);
    //    return sArray[0];
    //}




    //uipopuplist调用
    //private void OnSelectionChange(string skillName)//选择一个教练的技能的方法
    //{
    //    NpcAbility _na = FindSkillByName(skillName);
    //    if (_na == null)
    //        return;
    //    ShowSkillLevel(_na);//这一步显示技能等级
    //    if (m_StudyList.Count > 0)
    //    {
    //        foreach (NpcAbility na in m_StudyList)
    //        {
    //            if (GetSkillNameBySkData(na) == skillName)
    //                return;
    //        }
    //    }
    //    if (m_SkillIndex < 5)
    //    {
    //        m_StudySkillGrids[m_SkillIndex].SetSkill(_na.icon, _na);
    //        m_SkillIndex++;
    //    }
    //    if (m_StudyList.Count < 5 && _na != null)
    //        m_StudyList.Add(_na);
    //    Debug.Log("需要学习的技能一共有：" + m_StudyList.Count.ToString());
    //}




    //private NpcAbility FindSkillByName(string skillName)//通过技能名字在我的技能mSkills中找到该技能
    //{
    //    if (_skNameIdLis.Count == 0)
    //        return null;
    //    int _skID = -1;
    //    if (_skNameIdLis.ContainsKey(skillName))
    //        _skID = _skNameIdLis[skillName];
    //    if (_skID == -1)
    //        return null;
    //    for (int i = 0; i < _skillLis.Count; i++)
    //    {
    //        if (_skillLis[i].id == _skID)
    //            return _skillLis[i];
    //    }
    //    return null;
    //}

    //public GameObject m_Novice, m_Elite, m_Master, m_Expert, m_Arrow1, m_Arrow2, m_Arrow3;
    //public UISprite m_NoviceSpr, m_EliteSpr, m_MasterSpr, m_ExpertSpr;

    //private void ShowSkillLevel(NpcAbility _na)
    //{
    //    if (LevelDic.Count == 0)
    //        return;
    //    List<NpcAbility> lis = LevelDic[_na.id];
    //    SkillLevel _level = _na.Level;
    //    List<NpcAbility> _lis = NeededLevelSkill(lis, _level);//排好序的技能，从中拿技能图标
    //    SkillLevelShowCtrl(_lis);
    //}

    //private void SkillLevelShowCtrl(List<NpcAbility> _lis)
    //{
    //    Debug.Log("等级长度：" + _lis.Count);
    //    ClearSkillLevel();
    //    switch (_lis.Count)
    //    {
    //        case 1://chu ji
    //            m_Novice.SetActive(true);
    //            m_NoviceSpr.spriteName = _lis[0].icon;
    //            break;
    //        case 2://zhong ji
    //            m_Novice.SetActive(true);
    //            m_NoviceSpr.spriteName = _lis[0].icon;
    //            m_Elite.SetActive(true);
    //            m_EliteSpr.spriteName = _lis[1].icon;
    //            m_Arrow1.SetActive(true);
    //            break;
    //        case 3://gao ji
    //            m_Novice.SetActive(true);
    //            m_NoviceSpr.spriteName = _lis[0].icon;
    //            m_Elite.SetActive(true);
    //            m_EliteSpr.spriteName = _lis[1].icon;
    //            m_Master.SetActive(true);
    //            m_MasterSpr.spriteName = _lis[2].icon;
    //            m_Arrow1.SetActive(true);
    //            m_Arrow2.SetActive(true);
    //            break;
    //        case 4:// zhuan jia
    //            m_Novice.SetActive(true);
    //            m_NoviceSpr.spriteName = _lis[0].icon;
    //            m_Elite.SetActive(true);
    //            m_EliteSpr.spriteName = _lis[1].icon;
    //            m_Master.SetActive(true);
    //            m_MasterSpr.spriteName = _lis[2].icon;
    //            m_Expert.SetActive(true);
    //            m_ExpertSpr.spriteName = _lis[3].icon;
    //            m_Arrow1.SetActive(true);
    //            m_Arrow2.SetActive(true);
    //            m_Arrow3.SetActive(true);
    //            break;
    //    }
    //}

    //private List<NpcAbility> NeededLevelSkill(List<NpcAbility> _lis, SkillLevel _level)
    //{
    //    int level = (int)_level;
    //    List<NpcAbility> lis = new List<NpcAbility>();
    //    foreach (NpcAbility na in _lis)
    //        if ((int)na.Level <= level)
    //            lis.Add(na);
    //    lis.Sort(SortCompare);
    //    foreach (NpcAbility na in lis)
    //        Debug.Log((int)na.Level);
    //    return lis;
    //}

    //private int SortCompare(NpcAbility na1, NpcAbility na2)//从小到大的排序，如果要从大到小交换-1和1的位置
    //{
    //    int res = 0;
    //    if ((int)na1.Level < (int)na2.Level)
    //        return -1;
    //    if ((int)na1.Level > (int)na2.Level)
    //        return 1;
    //    return res;
    //}


    //private void ClearSkillLevel()//清除技能等级显示
    //{
    //    if (m_Novice.activeSelf)
    //        m_Novice.SetActive(false);
    //    if (m_Elite.activeSelf)
    //        m_Elite.SetActive(false);
    //    if (m_Master.activeSelf)
    //        m_Master.SetActive(false);
    //    if (m_Expert.activeSelf)
    //        m_Expert.SetActive(false);
    //    if (m_Arrow1.activeSelf)
    //        m_Arrow1.SetActive(false);
    //    if (m_Arrow2.activeSelf)
    //        m_Arrow2.SetActive(false);
    //    if (m_Arrow3.activeSelf)
    //        m_Arrow3.SetActive(false);
    //}

    private List<NpcAbility> m_StudyList = new List<NpcAbility>();//学习列表
    private int m_SkillIndex = 0;
    private List<CSUI_SkillItem> m_StudySkillGrids = new List<CSUI_SkillItem>();

    void SetStudyList(NpcAbility _npcAb)
    {
        if (mTrainingLock)
            return;

        if (_npcAb == null)
            return;
        CSPersonnel npc = GameUI.Instance.mCSUI_MainWndCtrl.TrainUI.m_TrainLearnPageCtrl.TraineeNpc;
        if (npc == null)
            return;

        NpcAblitycmpt nc = npc.m_Npc.GetCmpt<NpcCmpt>().Npcskillcmpt;
        int _id = nc.GetCanLearnId(_npcAb.id);
        if (_id == 0)
        {
            if (CSUI_MainWndCtrl.Instance != null)
                CSUI_MainWndCtrl.ShowStatusBar(PELocalization.GetString(CSTrainMsgID.SAME_OR_MORE_SKILL), 5.5f);
            return;
        }
        else
        {
            NpcAbility _na = NpcAblitycmpt.FindNpcAblityById(_id);
            if (m_StudyList.Contains(_na))
                return;
            m_StudyList.Add(_na);

            if (m_SkillIndex < 5)
            {
                m_StudySkillGrids[m_SkillIndex].SetSkill(_na.icon, _na);
                m_SkillIndex++;
            }
        }
    }


    public void ClearStudyList()
    {
        m_StudyList.Clear();
        m_SkillIndex = 0;
        foreach (CSUI_SkillItem grid in m_StudySkillGrids)
        {
            grid.SetSkill("Null", null);
        }
        HideAllDeleteBtn();
    }

    private void OnSkillGridDestroySelf(CSUI_SkillItem skillGrid)//点击技能删除按钮删除技能
    {
        if (mTrainingLock)
            return;

        skillGrid.DeleteIcon();
        m_StudyList.RemoveAt(skillGrid.m_Index);
        m_SkillIndex--;
        RefreshStudyList();
    }

    private void RefreshStudyList()//删除一个学习技能后，刷新学习技能列表中技能的位置
    {
        foreach (CSUI_SkillItem grid in m_StudySkillGrids)
        {
            grid.DeleteIcon();
        }
        for (int i = 0; i < m_StudyList.Count; i++)
        {
            m_StudySkillGrids[i].SetSkill(m_StudyList[i].icon, m_StudyList[i]);
        }
    }

    public void HideAllDeleteBtn()
    {
        if (m_StudySkillGrids.Count == 0)
            return;
        foreach (CSUI_SkillItem si in m_StudySkillGrids)
            si.OnHideBtn();
    }

    List<CSUI_NpcGridItem> m_TraineeGrids = new List<CSUI_NpcGridItem>();
    List<CSUI_NpcGridItem> m_InstructorGrids = new List<CSUI_NpcGridItem>();


    private void RefreshNPCGrids()
    {
        if (CSUI_MainWndCtrl.Instance == null) return;
        List<CSUI_NpcGridItem> npc_grids_list = null;
        if (Type == TypeEnu.Trainee)
        {
            npc_grids_list = m_TraineeGrids;
        }
        else
        {
            npc_grids_list = m_InstructorGrids;
        }

        if (CSUI_MainWndCtrl.Instance.Creator == null)
            return;
        CSPersonnel[] npcs = CSUI_MainWndCtrl.Instance.Creator.GetNpcs();

        int Len = 0;
        if (Type == TypeEnu.Trainee)//学员
        {
            for (int i = 0; i < npcs.Length; ++i)
            {
                if (npcs[i].Occupation == CSConst.potTrainer && npcs[i].trainerType == ETrainerType.Trainee)
                {
                    // Already has a CSUI_NPCGrid? just replace the npc reference!
                    if (Len < npc_grids_list.Count)
                        npc_grids_list[Len].m_Npc = npcs[i];
                    else
                    {
                        CSUI_NpcGridItem npcGrid = _createNPCGird(npcs[i], m_TraineeRootUI.transform);
                        npc_grids_list.Add(npcGrid);
                    }

                    Len++;
                }
            }
        }
        else                        //教练
        {
            for (int i = 0; i < npcs.Length; ++i)
            {
                if (npcs[i].Occupation == CSConst.potTrainer && npcs[i].trainerType == ETrainerType.Instructor)
                {
                    // Already has a CSUI_NPCGrid? just replace the npc reference!
                    if (Len < npc_grids_list.Count)
                        npc_grids_list[Len].m_Npc = npcs[i];
                    else
                    {
                        CSUI_NpcGridItem npcGrid = _createNPCGird(npcs[i], m_InstructorRootUI.transform);
                        npc_grids_list.Add(npcGrid);
                    }

                    Len++;
                }
            }
        }

        //Has redundant grid? just destroy it
        if (Len < npc_grids_list.Count)
        {
            for (int i = Len; i < npc_grids_list.Count; )
            {
                DestroyImmediate(npc_grids_list[i].gameObject);
                npc_grids_list.RemoveAt(i);
            }
        }
        if (npc_grids_list.Count != 0)
        {
            GameObject go = null;

            if (null != m_LearnPageSelectNpc)
            {
                for (int i = 0; i < npc_grids_list.Count; i++)
                {
                    if (m_LearnPageSelectNpc == npc_grids_list[i].m_Npc)
                    {
                        go = npc_grids_list[i].gameObject;
                        m_LearnPageSelectNpc = null;
                        break;
                    }
                }
            }

            if(null==go)
            {
                go = npc_grids_list[0].gameObject;
            }
            go.GetComponent<UICheckbox>().isChecked = true;
            OnNPCGridActive(npc_grids_list[0].gameObject, true);
        }
        else
            ActiveNpcGrid = null;

        m_TraineeRootUI.repositionNow = true;
        m_InstructorRootUI.repositionNow = true;
    }

    //lz-2016.11.06 点击当前设置的教师，选中教师列表中这个教师，并且显示信息
    void OnInstructorIconClick()
    {
        if (null != m_TrainLearnPageCtrl && null!=m_TrainLearnPageCtrl.InsNpc)
        { 
            if (null != ActiveNpcGrid && ActiveNpcGrid.m_Npc != m_TrainLearnPageCtrl.InsNpc)
            {
                m_LearnPageSelectNpc = m_TrainLearnPageCtrl.InsNpc;
                if (!mInsCheckBox.isChecked)
                {
                    mInsCheckBox.isChecked = true;
                }
                else
                {
                    Type = TypeEnu.Instructor;
                }
            }
        }
    }

    //lz-2016.11.06 点击当前设置的学员，选中学员列表中这个学员，并且显示信息
    void OnTraineeIconClick()
    {
        if (null != m_TrainLearnPageCtrl && null != m_TrainLearnPageCtrl.TraineeNpc)
        {
            if (null != ActiveNpcGrid && ActiveNpcGrid.m_Npc != m_TrainLearnPageCtrl.TraineeNpc)
            {
                m_LearnPageSelectNpc = m_TrainLearnPageCtrl.TraineeNpc;
                if (!mTraCheckBox.isChecked)
                {
                    mTraCheckBox.isChecked = true;
                }
                else
                {
                    Type = TypeEnu.Trainee;
                }
            }
        }
    }

    // CSUI_NPCGrid OnActive call back
    private void OnNPCGridActive(GameObject go, bool actvie)
    {
        if (!actvie) return;
        ActiveNpcGrid = go.GetComponent<CSUI_NpcGridItem>();
    }

    private CSUI_NpcGridItem _createNPCGird(CSPersonnel npc, Transform root)
    {
        CSUI_NpcGridItem npcGrid = Instantiate(m_NpcGridPrefab) as CSUI_NpcGridItem;
        npcGrid.transform.parent = root;
        CSUtils.ResetLoacalTransform(npcGrid.transform);
        npcGrid.m_UseDeletebutton = false;
        //npcGrid.OnDestroySelf = OnNPCGridDestroySelf;

        npcGrid.m_NpcNameLabel.enabled = false;
        npcGrid.m_Npc = npc;

        UICheckbox cb = npcGrid.gameObject.GetComponent<UICheckbox>();
        cb.radioButtonRoot = root;

        UIEventListener.Get(npcGrid.gameObject).onActivate = OnNPCGridActive;

        return npcGrid;
    }

    #region properties



    [SerializeField]
    UILabel mLbName;
    [SerializeField]
    UISprite mSprSex;
    [SerializeField]
    UILabel mLbHealth;
    [SerializeField]
    UISlider mSdHealth;
    [SerializeField]
    UILabel mLbStamina;
    [SerializeField]
    UISlider mSdStamina;
    [SerializeField]
    UILabel mLbHunger;
    [SerializeField]
    UISlider mSdHunger;
    [SerializeField]
    UILabel mLbComfort;
    [SerializeField]
    UISlider mSdComfort;
    [SerializeField]
    UILabel mLbOxygen;
    [SerializeField]
    UISlider mSdOxygen;
    [SerializeField]
    UILabel mLbShield;
    [SerializeField]
    UISlider mSdShield;
    [SerializeField]
    UILabel mLbEnergy;
    [SerializeField]
    UISlider mSdEnergy;
    [SerializeField]
    UILabel mLbAttack;
    [SerializeField]
    UILabel mLbDefense;

    #endregion

    public void SetServantInfo(string name, PeSex sex, int health, int healthMax, int stamina, int stamina_max, int hunger, int hunger_max, int comfort, int comfort_max,
                                int oxygen, int oxygen_max, int shield, int shield_max, int energy, int energy_max, int attack, int defense)
    {
        mLbName.text = name;
        mSprSex.spriteName = sex == PeSex.Male ? "man" : "woman";

        mLbHealth.text = Convert.ToString(health) + "/" + Convert.ToString(healthMax);
        mSdHealth.sliderValue = (healthMax > 0) ? Convert.ToSingle(health) / healthMax : 0;

        mLbStamina.text = Convert.ToString(stamina) + "/" + Convert.ToString(stamina_max);
        mSdStamina.sliderValue = (stamina_max > 0) ? Convert.ToSingle(stamina) / stamina_max : 0;

        mLbHunger.text = Convert.ToString(hunger) + "/" + Convert.ToString(hunger_max);
        mSdHunger.sliderValue = (hunger_max > 0) ? Convert.ToSingle(hunger) / hunger_max : 0;

        mLbComfort.text = Convert.ToString(comfort) + "/" + Convert.ToString(comfort_max);
        mSdComfort.sliderValue = (comfort_max > 0) ? Convert.ToSingle(comfort) / comfort_max : 0;

        mLbOxygen.text = Convert.ToString(oxygen) + "/" + Convert.ToString(oxygen_max);
        mSdOxygen.sliderValue = (oxygen_max > 0) ? Convert.ToSingle(oxygen) / oxygen_max : 0;

        mLbShield.text = Convert.ToString(shield) + "/" + Convert.ToString(shield_max);
        mSdShield.sliderValue = (shield_max > 0) ? Convert.ToSingle(shield) / shield_max : 0;

        mLbEnergy.text = Convert.ToString(energy) + "/" + Convert.ToString(energy_max);
        mSdEnergy.sliderValue = (energy_max > 0) ? Convert.ToSingle(energy) / energy_max : 0;

        mLbAttack.text = Convert.ToString(attack);
        mLbDefense.text = Convert.ToString(defense);

    }

    public void SetSprSex(string sex)
    {
        mSprSex.spriteName = sex;
    }

    //按钮调用方法
    private void PageTraineeOnActive(bool active)
    {
        if (active)
            Type = TypeEnu.Trainee;
    }
    private void PageInstructorOnActive(bool active)
    {
        if (active)
            Type = TypeEnu.Instructor;

    }

    [SerializeField]
    Grid_N mGridPrefab;
    [SerializeField]
    Transform mTsInteractionGrids;
    [SerializeField]
    Transform mTsPrivateItemGrids;

    private void InitGrid()
    {
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
            mInteractionList[i].SetItemPlace(ItemPlaceType.IPT_ConolyServantInteractionTrain, i);


            //mInteractionList[i].onDropItem += OnDropItem_InterPackage;
            //mInteractionList[i].onLeftMouseClicked += OnLeftMouseCliked_InterPackage;
            //mInteractionList[i].onRightMouseClicked += OnRightMouseCliked_InterPackage;
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
    }


    [SerializeField]
    UIGrid m_StudyRoot;

    #region UNITY_INNER

    void Awake()
    {
        _instance = this;
        InitGrid();
        // Create Trainee Skill Grid
        for (int i = 0; i < 5; i++)
        {
            CSUI_SkillItem grid = Instantiate(m_SkillPrefab) as CSUI_SkillItem; ;
            grid.transform.parent = m_TraineeSkillRoot.transform;
            grid.transform.localPosition = Vector3.zero;
            grid.transform.localRotation = Quaternion.identity;
            grid.transform.localScale = Vector3.one;
            grid.m_Index = i;
            grid._ableToClick = false;
            m_TraineeSkillGrids.Add(grid);
        }
        m_TraineeSkillRoot.repositionNow = true;

        //Creat Instructor Skill Grid
        for (int i = 0; i < 5; i++)
        {
            CSUI_SkillItem grid = Instantiate(m_SkillPrefab) as CSUI_SkillItem; ;
            grid.transform.parent = m_InstructorSkillRoot.transform;
            grid.transform.localPosition = Vector3.zero;
            grid.transform.localRotation = Quaternion.identity;
            grid.transform.localScale = Vector3.one;
            grid.m_Index = i;
            grid._ableToClick = false;
            grid.onLeftMouseClicked += SetStudyList;
            m_InstructorSkillGrids.Add(grid);
        }
        m_InstructorSkillRoot.repositionNow = true;

        //Creat study skill Grid
        for (int i = 0; i < 5; i++)
        {
            CSUI_SkillItem grid = Instantiate(m_SkillPrefab) as CSUI_SkillItem;
            grid.transform.parent = m_StudyRoot.transform;
            grid.transform.localPosition = Vector3.zero;
            grid.transform.localRotation = Quaternion.identity;
            grid.transform.localScale = Vector3.one;
            grid.OnDestroySelf += OnSkillGridDestroySelf;
            grid.m_Index = i;
			m_StudySkillGrids.Add(grid);
        }
        m_StudyRoot.repositionNow = true;
    }

    void OnEnable()
    {
        m_TrainLearnPageCtrl.InsNpc = null;
        m_TrainLearnPageCtrl.TraineeNpc = null;

        if (mInsCheckBox.isChecked)
            Type = TypeEnu.Instructor;
        else if (mTraCheckBox.isChecked)
            Type = TypeEnu.Trainee;

        //if (m_Type == null)
        //{
        //    Debug.Log("第一次进来");
        //    return;
        //}
        //RefreshNPCGrids();
    }

    void Start()
	{
		ApplyDataToUI();
    }



    [SerializeField]
    GameObject m_LearnSkillPage;//技能学习节点
    [SerializeField]
    GameObject m_UpgradePage;//属性提升节点
    [SerializeField]
    UITexture mInstructorFace_Stats;

    [SerializeField]
    UITexture mInstructorFace_Skill;

    [SerializeField]
    UITexture mTraineeFace_Stats;
    [SerializeField]
    UITexture mTraineeFace_Skill;



    //private CSPersonnel mTraineeStats = new CSPersonnel();//属性提升的学员
    //private CSPersonnel mInstructorStats = new CSPersonnel();//属性提升的教官
    //private CSPersonnel mTraineeSkill = new CSPersonnel();//学习技能的学员
    //private CSPersonnel mInstructorSkill = new CSPersonnel();//技能学习的教官

    //float mTimer = 0f;
	//int count=0;
    void Update()
    {
//		count++;
//		if(count%120==0){
//			ApplyDataToUI();
//			count=0;
//		}

        //mTimer += Time.deltaTime;

        //if (mTimer >= 0.5f)
        //{
        //    mTimer = 0f;
        //    RefreshAfterTraining();
        //}




        //if (m_RefNpc == null || m_RefNpc.NPC == null || m_RefNpc.Occupation != 0)
        //{
        //    SetServantInfo("--", PeSex.Male, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        //    mLbMoney.text = "--";
        //}
        // playerInfo
        //else if (m_RefNpc.Occupation == 0)//学员
        //{

        //    SetServantInfo(m_RefNpc.Name, m_RefNpc.Sex, (int)m_RefNpc.GetAttribute(AttribType.Hp), (int)m_RefNpc.GetAttribute(AttribType.HpMax), (int)m_RefNpc.GetAttribute(AttribType.Stamina), (int)m_RefNpc.GetAttribute(AttribType.StaminaMax), (int)m_RefNpc.GetAttribute(AttribType.Hunger), (int)m_RefNpc.GetAttribute(AttribType.HungerMax), (int)m_RefNpc.GetAttribute(AttribType.Comfort), (int)m_RefNpc.GetAttribute(AttribType.ComfortMax), (int)m_RefNpc.GetAttribute(AttribType.Oxygen), (int)m_RefNpc.GetAttribute(AttribType.OxygenMax), (int)m_RefNpc.GetAttribute(AttribType.Shield), (int)m_RefNpc.GetAttribute(AttribType.ShieldMax), (int)m_RefNpc.GetAttribute(AttribType.Energy), (int)m_RefNpc.GetAttribute(AttribType.EnergyMax), (int)m_RefNpc.GetAttribute(AttribType.Atk), (int)m_RefNpc.GetAttribute(AttribType.Def));

        //    if (_package != null)
        //        mLbMoney.text = _package.money.current.ToString();
        //}

        if (Input.GetKeyDown(KeyCode.P))
        {


            //string s1 = PELocalization.GetString(2200017);
            //Debug.Log(s1);
            //string s2 = PELocalization.GetString(2200018);
            //Debug.Log(s2);
            //string s3 = PELocalization.GetString(2200019);
            //Debug.Log(s3);

            //string[] sArray = s1.Split(new string[] { "[00FFFF]", "[-]" }, StringSplitOptions.RemoveEmptyEntries);
            //Debug.Log(sArray[0]);
            //foreach (SkData sd in m_StudyList)
            //{
            //    Debug.Log(sd._id);
            //}

            //NpcPackageCmpt _package = null;
            //if (m_RefNpc != null)
            //    _package = m_RefNpc.m_Npc.GetCmpt<NpcCmpt>().GetComponent<NpcPackageCmpt>();
            //if (_package != null)
            //{
            //    Debug.Log("拿到包裹了");
            //    Debug.Log(_package);
            //}
            //if (mInteractionPackage != null)
            //    mInteractionPackage.eventor.Unsubscribe(InteractionpackageChange);

            //mInteractionPackage = _package.GetSlotList();
            //if (mInteractionPackage == null)
            //    Debug.Log("空的");

            //ReflashInteractionpackage();

            //if (mPrivatePakge != null)
            //    mPrivatePakge.eventor.Unsubscribe(PrivatepackageChange);

            //mPrivatePakge = _package.GetPrivateSlotList();
            //mPrivatePakge.eventor.Subscribe(PrivatepackageChange);

            //ReflashPrivatePackage();

            //mLbMoney.text = _package.money.current.ToString();
            //Debug.Log(_package.money.current.ToString());

        }


        //#region 技能和属性
        //if (m_RefNpc != null)
        //{
        //    if (m_RefNpc.trainerType == ETrainerType.Instructor)//教练
        //    {
        //        if (m_UpgradePage.activeSelf)//属性提升
        //        {
        //            if (m_RefNpc.State == 10)//假设这个状态是在教学
        //            {
        //                mInstructorStats = null;
        //                mInstructorFace_Stats.mainTexture = null;
        //                mInstructorFace_Stats.enabled = false;
        //                //SetInstructorInfo("", "", "", "", "");
        //            }
        //            else if (m_RefNpc.State != 10)
        //            {
        //                if (mInstructorStats != m_RefNpc)
        //                    mInstructorStats = m_RefNpc;
        //                mInstructorFace_Stats.mainTexture = m_RefNpc.RandomNpcFace;
        //                mInstructorFace_Stats.enabled = true;
        //                //SetInstructorInfo(m_RefNpc.AddHealth, m_RefNpc.AddStrength, m_RefNpc.AddHunger, m_RefNpc.AddStamina, m_RefNpc.AddOxygen);
        //            }
        //        }
        //        else if (m_LearnSkillPage.activeSelf)//技能学习
        //        {
        //            mInstructorFace_Skill.mainTexture = m_RefNpc.RandomNpcFace;
        //            mInstructorFace_Skill.enabled = true;
        //        }
        //    }
        //    else if (m_RefNpc.trainerType == ETrainerType.Trainee)//学员
        //    {
        //        if (m_UpgradePage.activeSelf)//属性提升
        //        {
        //            if (m_RefNpc.State == 10)//假设这个状态是在学习
        //            {
        //                mTraineeStats = null;
        //                mTraineeFace_Stats.mainTexture = null;
        //                mTraineeFace_Stats.enabled = false;
        //                //SetTraineeInfo(0, 0, 0, 0, 0);
        //            }
        //            else if (m_RefNpc.State != 10)
        //            {
        //                if (mTraineeStats != m_RefNpc)
        //                    mTraineeStats = m_RefNpc;
        //                mTraineeFace_Stats.mainTexture = m_RefNpc.RandomNpcFace;
        //                mTraineeFace_Stats.enabled = true;
        //                //SetTraineeInfo(m_RefNpc.HealthMax, m_RefNpc.Strength_max, m_RefNpc.Hunger_max, m_RefNpc.Stamina_max, m_RefNpc.Oxygen_max);
        //            }
        //        }
        //        else if (m_LearnSkillPage.activeSelf)//技能学习
        //        {
        //            if (m_RefNpc.m_Npc.GetCmpt<NpcCmpt>().Abilities.Count != 5)//还没学满5个技能
        //            {
        //                if (mTraineeSkill != m_RefNpc)
        //                {
        //                    mTraineeSkill = m_RefNpc;
        //                    Debug.Log("学员技能学习的头像该有");
        //                    mTraineeFace_Skill.mainTexture = m_RefNpc.RandomNpcFace;
        //                    mTraineeFace_Skill.enabled = true;
        //                    Debug.Log(mTraineeFace_Skill.mainTexture);
        //                }
        //                if (mInstructorSkill != m_RefNpc)
        //                {
        //                    mInstructorSkill = m_RefNpc;
        //                }
        //            }
        //        }
        //    }
        //}
        //#endregion


        //if (uipLabel.text == "" || uipLabel.text == "InstructorSkills" || uipLabel.text == "......")//对技能等级显示的清除
        //{
        //    ClearSkillLevel();
        //}

        //if (packageCmpt != null)
        //{
        //    mLbPrivatePageText.text = mPageIndex.ToString() + " / " + mMaxPageIndex.ToString();
        //    mLbMoney.text = packageCmpt.money.current.ToString();
        //}
        //else
        //{
        //    mLbPrivatePageText.text = "0 / 0";
        //    mLbMoney.text = "--";
        //}

    }

    [SerializeField]
    UILabel mLbMoney;
    [SerializeField]
    UILabel mLbPrivatePageText;

    #endregion






    #region Package Func

    SlotList mInteractionPackage = null;
    SlotList mPrivatePakge = null;
    NpcPackageCmpt packageCmpt = null;
    NpcPackageCmpt _package = null;


    int mInteractionGridCount = 25;
    int mPrivateItemGridCount = 10;

    List<Grid_N> mInteractionList;
    List<Grid_N> mPrivateList;


    NpcCmpt npcCmpt = null;



    private void GetNpcPackage()
    {
        if (m_RefNpc == null)
        {
            mInteractionPackage = null;
            mPrivatePakge = null;
            _package = null;
        }
        if (m_RefNpc != null)
        {
            _package = m_RefNpc.m_Npc.GetCmpt<NpcCmpt>().GetComponent<NpcPackageCmpt>();
            mInteractionPackage = _package.GetSlotList();
            mPrivatePakge = _package.GetPrivateSlotList();
        }

        //if (mInteractionPackage != null)
        //    mInteractionPackage.eventor.Unsubscribe(InteractionpackageChange);
        //mInteractionPackage = _package.GetSlotList();
        //mInteractionPackage.eventor.Subscribe(InteractionpackageChange);

        //if (mPrivatePakge != null)
        //    mPrivatePakge.eventor.Unsubscribe(PrivatepackageChange);
        //mPrivatePakge = _package.GetPrivateSlotList();
        //mPrivatePakge.eventor.Subscribe(PrivatepackageChange);

        //if (mInteractionList == null)
        //    return;
        // Reflashpackage();
    }








    //***************************************************************

    void GetServentCmpt()
    {
        NpcCmpt cmpt = m_RefNpc.m_Npc.GetCmpt<NpcCmpt>();
        if (cmpt != null && cmpt != npcCmpt)
        {
            packageCmpt = cmpt.GetComponent<NpcPackageCmpt>();
            GetNpcPakgeSlotList();
        }
        npcCmpt = cmpt;
        servant = (npcCmpt != null) ? npcCmpt.Entity : null;
        if (npcCmpt == null)
        {
            packageCmpt = null;
            mInteractionPackage = null;
            mPrivatePakge = null;
            ClearNpcPackage();
        }
    }

    //PlayerPackageCmpt playerPackageCmpt = null;

    void Reflash()
    {
        //playerPackageCmpt = GameUI.Instance.mMainPlayer.GetCmpt<PlayerPackageCmpt>();
        GetServentCmpt();
        if (npcCmpt == null)
            return;
        if (packageCmpt != null)
            Reflashpackage();
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
        if (m_RefNpc == null || mInteractionPackage == null)
            return;
        if (!m_RefNpc.IsRandomNpc)
            return;
        PlayerPackageCmpt package = GameUI.Instance.mMainPlayer.GetCmpt<PlayerPackageCmpt>();
        List<ItemObject> itemList = mInteractionPackage.ToList();
        if (package.CanAddItemList(itemList))
        {
            package.AddItemList(itemList);
            mInteractionPackage.Clear();
        }
    }



    public bool SetItemWithIndex(ItemObject itemObj, int index = -1)
    {

        if (index == -1)
            return mInteractionPackage.Add(itemObj);
        else
        {
            if (index < 0 || index > mInteractionPackage.Count)
                return false;
            if (mInteractionPackage != null)
                mInteractionPackage[index] = itemObj;
        }
        return true;
    }

    void GetNpcPakgeSlotList()
    {
        if (mInteractionPackage != null)
            mInteractionPackage.eventor.Unsubscribe(InteractionpackageChange);

        mInteractionPackage = packageCmpt.GetSlotList();
        mInteractionPackage.eventor.Subscribe(InteractionpackageChange);

        if (mPrivatePakge != null)
            mPrivatePakge.eventor.Unsubscribe(PrivatepackageChange);

        mPrivatePakge = packageCmpt.GetPrivateSlotList();
        mPrivatePakge.eventor.Subscribe(PrivatepackageChange);
        // reset equipment receiver
        //servantEqReceiver.ResetPackage(packageCmpt);
    }

    public void Reflashpackage()
    {
        ReflashInteractionpackage();
        ReflashPrivatePackage();
        ReflashNpcMoney();
    }

    void ReflashNpcMoney()
    {
        if (_package == null)
            return;
        mLbMoney.text = _package.money.current.ToString();
    }

    void ClearNpcPackage()
    {
        ClearInteractionpackage();
        ClearPrivatePackage();
    }

    //Interactionpackage
    void InteractionpackageChange(object sender, SlotList.ChangeEvent arg)
    {
        ReflashInteractionpackage();
    }
    void ReflashInteractionpackage()
    {
        ClearInteractionpackage();
        for (int i = 0; i < mInteractionGridCount; i++)
        {
            if (mInteractionPackage == null)
            {
                mInteractionList[i].SetItem(null);
            }
            else
            {
                mInteractionList[i].SetItem(mInteractionPackage[i]);
            }
        }
    }

    void ClearInteractionpackage()
    {
        if (mInteractionList == null)
            return;
        foreach (Grid_N item in mInteractionList)
            item.SetItem(null);
    }

    //Privatepackage
    int mPageIndex = 1;
    int mMaxPageIndex { get { return mPrivatePakge == null ? 1 : mPrivatePakge.Count / mPrivateItemGridCount; } }
    void PrivatepackageChange(object sender, SlotList.ChangeEvent arg)
    {
        ReflashPrivatePackage();
    }

    void ReflashPrivatePackage()
    {
        ClearPrivatePackage();
        int startIndex = (mPageIndex - 1) * mPrivateItemGridCount;

        for (int i = 0; i < mPrivateList.Count; i++)
        {
            if (mPrivatePakge == null)
            {
                mPrivateList[i].SetItem(null);
            }
            else
            {
                mPrivateList[i].SetItem(mPrivatePakge[startIndex + i]);
            }
        }
    }

    void ClearPrivatePackage()
    {
        if (mPrivateList == null)
            return;
        foreach (Grid_N item in mPrivateList)
            item.SetItem(null);
    }
    #endregion

    PeEntity servant { get; set; }

    #region ui event
    public void OnLeftMouseCliked_InterPackage(Grid_N grid)
    {
        if (null == m_RefNpc || m_RefNpc.Occupation != 0)
            return;
        SelectItem_N.Instance.SetItem(grid.ItemObj, grid.ItemPlace, grid.ItemIndex);
    }

    public void OnRightMouseCliked_InterPackage(Grid_N grid)
    {
        if (null == servant)
            return;

        Pathea.UseItemCmpt useItem = servant.GetCmpt<Pathea.UseItemCmpt>();
        if (null == useItem)
            useItem = servant.Add<Pathea.UseItemCmpt>();

        if (true == useItem.Request(grid.ItemObj))
        {
            //mInteractionPackage[grid.ItemIndex] = null;
            //ReflashInteractionpackage();
            //				Reflash();
        }
    }

    public void OnDropItem_InterPackage(Grid_N grid)
    {
        if (null == m_RefNpc || m_RefNpc.Occupation != 0)
            return;

        if (grid.ItemObj != null)
            return;

        if (PeGameMgr.IsMulti)
        {
            switch (SelectItem_N.Instance.Place)
            {
                case ItemPlaceType.IPT_ServantEqu:
                case ItemPlaceType.IPT_Bag:
                    {
                        PlayerNetwork.mainPlayer.RequestGiveItem2Npc((int)grid.ItemPlace, servant.Id, SelectItem_N.Instance.ItemObj.instanceId, SelectItem_N.Instance.Place);
                    }
                    break;
            }

            SelectItem_N.Instance.SetItem(null);
        }
        else
        {
            switch (SelectItem_N.Instance.Place)
            {
                case ItemPlaceType.IPT_ServantEqu:
                case ItemPlaceType.IPT_ServantInteraction:
                case ItemPlaceType.IPT_Bag:
                case ItemPlaceType.IPT_ConolyServantInteractionTrain:
                    {
                        SetItemWithIndex(SelectItem_N.Instance.ItemObj, grid.ItemIndex);
                        SelectItem_N.Instance.RemoveOriginItem();
                        grid.SetItem(SelectItem_N.Instance.ItemObj);
                        SelectItem_N.Instance.SetItem(null);
                    } break;
                default:
                    SelectItem_N.Instance.SetItem(null);
                    break;
            }
        }
    }
    #endregion



    List<int> GetSkillIds(List<NpcAbility> _lis)
    {
        List<int> lis = new List<int>(1);
        if (_lis.Count == 0)
            return lis;
        foreach (NpcAbility na in _lis)
        {
            lis.Add(na.id);
        }
        return lis;
    }

    List<int> GetLastList(ETrainingType _trainingtype)
    {
        List<int> lis = new List<int>();

        if (_trainingtype == ETrainingType.Skill)
        {
            lis = GetSkillIds(m_StudyList);
            return lis;
        }
        else if (_trainingtype == ETrainingType.Attribute)
        {
            return lis;
        }
        else
            return lis;

    }

    #region Interface

    public event System.Action<ETrainingType, List<int>, CSPersonnel, CSPersonnel> OnStartTrainingEvent;//开始训练事件

    public event System.Action OnStopTrainingEvent;//停止训练事件



    private void OnStartBtn()
    {
        if (OnStartTrainingEvent != null&& null!=m_TrainLearnPageCtrl.InsNpc&&null!=m_TrainLearnPageCtrl.TraineeNpc)
        {
            OnStartTrainingEvent(m_TrainLearnPageCtrl.TrainingType, GetLastList(m_TrainLearnPageCtrl.TrainingType), m_TrainLearnPageCtrl.InsNpc, m_TrainLearnPageCtrl.TraineeNpc);
        }
    }

    private void OnStopBtn()
    {
        if (OnStopTrainingEvent != null)
        {
            OnStopTrainingEvent();
        }
    }

    /// <summary>
    /// 返回的列表长度为0时，代表没得技能
    /// </summary>
    /// <returns></returns>
    public List<int> GetStudyList()
    {
        List<int> lis = new List<int>();
        lis = GetSkillIds(m_StudyList);
        return lis;
    }

    /// <summary>
    /// 设置开始和停止按钮的开关(_state为true时，表示正在学习中，此时start按钮关闭，stop按钮打开)
    /// </summary>
    /// <param name="_state"></param>
    public void SetBtnState(bool _state)
    {
        m_TrainLearnPageCtrl.mStartBtn.gameObject.SetActive(!_state);
        m_TrainLearnPageCtrl.UpdateStatBtnState();
        m_TrainLearnPageCtrl.mStopBtn.SetActive(_state);
    }

    /// <summary>
    /// 训练完成刷新界面接口
    /// </summary>
    /// <param name="_ins"></param>
    /// <param name="_tra"></param>
    public void RefreshAfterTraining()
    {
        m_TrainLearnPageCtrl.InsNpc = m_TrainLearnPageCtrl.InsNpc;
        m_TrainLearnPageCtrl.TraineeNpc = m_TrainLearnPageCtrl.TraineeNpc;
    }
	public void RefreshAfterTraining(CSPersonnel csp_instructor,CSPersonnel csp_trainee)
	{
        m_TrainLearnPageCtrl.InsNpc = csp_instructor;
        m_TrainLearnPageCtrl.TraineeNpc = csp_trainee;
	}
    /// <summary>
    /// 设置学习技能列表接口
    /// </summary>
    /// <param name="_lis"></param>
    public void SetStudyListInterface(List<int> _lis)
    {
        if (_lis.Count == 0)
            return;
        m_SkillIndex = 0;
        m_StudyList.Clear();
        for (int i = 0; i < _lis.Count; i++)
        {
            NpcAbility item = NpcAblitycmpt.FindNpcAblityById(_lis[i]);
            m_StudyList.Add(item);
            if (m_SkillIndex < 5)
            {
				if(m_StudySkillGrids.Count>m_SkillIndex)
                	m_StudySkillGrids[m_SkillIndex].SetSkill(item.icon, item);
                m_SkillIndex++;
            }
        }
    }

    public UILabel mTrainTimeLab;
    private int _minute, _second;//分、秒

    /// <summary>
    /// 技能学习时间显示,属性提升也是调用这个接口
    /// </summary>
    /// <param name="_time"></param>
    public void LearnSkillTimeShow(float _time)
    {
        if (mTrainTimeLab != null)
        {
            _minute = (int)(_time / 60);
            _second = (int)(_time - _minute * 60);
            mTrainTimeLab.text = TimeTransition(_minute).ToString() + ":" + TimeTransition(_second).ToString();
        }
    }
    /// <summary>
    /// 时间转换
    /// </summary>
    /// <param name="_number"></param>
    /// <returns></returns>
    private string TimeTransition(int _number)
    {
        if (_number < 10)
            return "0" + _number.ToString();
        else
            return _number.ToString();
    }

    #endregion
}
