using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using SkillAsset;
using System;
using Pathea;
using Pathea.PeEntityExt;

public class CSUIMySkill
{
    public int id;
    public string name;
    public string iconImg;
    public MySkillGrade grade;
}
public enum MySkillGrade
{
    none = 0,
    Elementary,
    Intermediate,
    Advanced
}

public class CSUIMyNpc
{
    //private bool m_IsLearnOrTeach = false;//是否在学技能或教技能
    //public bool IsLearnOrTeach
    //{
    //    get { return m_IsLearnOrTeach; }
    //    set
    //    {
    //        m_IsLearnOrTeach = value;
    //    }
    //}

    private List<CSUIMySkill> m_OwnSkills = new List<CSUIMySkill>();//当前NPC拥有的技能
    public List<CSUIMySkill> OwnSkills
    {
        get { return m_OwnSkills; }
        set
        {
            m_OwnSkills = value;
        }
    }

    private int m_State = -1;//NPC工作状态
    public int State
    {
        get { return m_State; }
        set
        {
            m_State = value;
        }
    }


    private bool m_IsRandom = false;//是不是随机NPC
    public bool IsRandom
    {
        get { return m_IsRandom; }
        set
        {
            m_IsRandom = value;
        }
    }

    private bool m_HasOccupation = false;//有没有设定职业
    public bool HasOccupation
    {
        get { return m_HasOccupation; }
        set
        {
            m_HasOccupation = value;
        }
    }

    private string m_Name = "Free";//名字
    public string Name
    {
        get { return m_Name; }
        set
        {
            m_Name = value;
        }
    }

    private PeSex m_Sex = PeSex.Male;//性别
    public PeSex Sex
    {
        get { return m_Sex; }
        set
        {
            m_Sex = value;
        }
    }

    private Texture m_RandomNpcFace = null;//随机npc的头像
    public Texture RandomNpcFace
    {
        get { return m_RandomNpcFace; }
        set
        {
            m_RandomNpcFace = value;
        }
    }

    #region 属性


    private int m_Health = 0;//
    public int Health
    {
        get { return m_Health; }
        set
        {
            m_Health = value;
        }
    }

    private int m_HealthMax = 0;//
    public int HealthMax
    {
        get { return m_HealthMax; }
        set
        {
            m_HealthMax = value;
        }
    }

    private int m_Stamina = 0;//
    public int Stamina
    {
        get { return m_Stamina; }
        set
        {
            m_Stamina = value;
        }
    }

    private int m_Stamina_max = 0;//
    public int Stamina_max
    {
        get { return m_Stamina_max; }
        set
        {
            m_Stamina_max = value;
        }
    }

    private int m_Hunger = 0;//
    public int Hunger
    {
        get { return m_Hunger; }
        set
        {
            m_Hunger = value;
        }
    }


    private int m_Hunger_max = 0;//
    public int Hunger_max
    {
        get { return m_Hunger_max; }
        set
        {
            m_Hunger_max = value;
        }
    }


    private int m_Comfort = 0;//
    public int Comfort
    {
        get { return m_Comfort; }
        set
        {
            m_Comfort = value;
        }
    }

    private int m_Comfort_max = 0;//

    public int Comfort_max
    {
        get { return m_Comfort_max; }
        set
        {
            m_Comfort_max = value;
        }
    }

    private int m_Oxygen = 0;//
    public int Oxygen
    {
        get { return m_Oxygen; }
        set
        {
            m_Oxygen = value;
        }
    }

    private int m_Oxygen_max = 0;//
    public int Oxygen_max
    {
        get { return m_Oxygen_max; }
        set
        {
            m_Oxygen_max = value;
        }
    }

    private int m_Shield = 0;//
    public int Shield
    {
        get { return m_Shield; }
        set
        {
            m_Shield = value;
        }
    }

    private int m_Shield_max = 0;//
    public int Shield_max
    {
        get { return m_Shield_max; }
        set
        {
            m_Shield_max = value;
        }
    }

    private int m_Energy = 0;//
    public int Energy
    {
        get { return m_Energy; }
        set
        {
            m_Energy = value;
        }
    }

    private int m_Energy_max = 0;//
    public int Energy_max
    {
        get { return m_Energy_max; }
        set
        {
            m_Energy_max = value;
        }
    }

    private int m_Strength = 0;//
    public int Strength
    {
        get { return m_Strength; }
        set
        {
            m_Strength = value;
        }
    }

    private int m_Strength_max = 0;//
    public int Strength_max
    {
        get { return m_Strength_max; }
        set
        {
            m_Strength_max = value;
        }
    }

    private int m_Attack = 0;//
    public int Attack
    {
        get { return m_Attack; }
        set
        {
            m_Attack = value;
        }
    }

    private int m_Defense = 0;//
    public int Defense
    {
        get { return m_Defense; }
        set
        {
            m_Defense = value;
        }
    }
    #endregion

    #region 作为教官能提升的属性


    private string m_AddHealth = "";//
    public string AddHealth
    {
        get { return m_AddHealth; }
        set
        {
            m_AddHealth = value;
        }
    }
    private string m_AddStrength = "";//
    public string AddStrength
    {
        get { return m_AddStrength; }
        set
        {
            m_AddStrength = value;
        }
    }
    private string m_AddHunger = "";//
    public string AddHunger
    {
        get { return m_AddHunger; }
        set
        {
            m_AddHunger = value;
        }
    }
    private string m_AddStamina = "";//
    public string AddStamina
    {
        get { return m_AddStamina; }
        set
        {
            m_AddStamina = value;
        }
    }
    private string m_AddOxygen = "";//
    public string AddOxygen
    {
        get { return m_AddOxygen; }
        set
        {
            m_AddOxygen = value;
        }
    }

    #endregion
}

public class CSUI_Train : MonoBehaviour
{

    #region Properties
    [SerializeField]
    UIGrid m_TraineeRootUI;//学员节点
    [SerializeField]
    UIGrid m_InstructorRootUI;//教练节点
    [SerializeField]
    GameObject m_InfoPage;//学员信息节点
    [SerializeField]
    GameObject m_InventoryPage;//学员存货节点
    [SerializeField]
    GameObject m_LearnSkillPage;//技能学习节点
    [SerializeField]
    GameObject m_UpgradePage;//属性提升节点

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
    [SerializeField]
    UIGrid m_ItemRoot;
    [SerializeField]
    UIGrid m_SkillRoot;
    [SerializeField]
    UIGrid m_StudyRoot;


    [SerializeField]
    UITexture mTraineeFace_Skill;
    [SerializeField]
    UITexture mInstructorFace_Skill;
    [SerializeField]
    UITexture mTraineeFace_Stats;
    [SerializeField]
    UITexture mInstructorFace_Stats;

    [SerializeField]
    UIPopupList uip;

    [SerializeField]
    UILabel mMaxHealth;
    [SerializeField]
    UILabel mMaxStrength;
    [SerializeField]
    UILabel mMaxHunger;
    [SerializeField]
    UILabel mMaxStamina;
    [SerializeField]
    UILabel mMaxOxygen;
    [SerializeField]
    UILabel mTrainingTime;
    [SerializeField]
    UILabel mAddHealth;
    [SerializeField]
    UILabel mAddStrength;
    [SerializeField]
    UILabel mAddHunger;
    [SerializeField]
    UILabel mAddStamina;
    [SerializeField]
    UILabel mAddOxygen;

    //预制件
    [SerializeField]
    CSUI_MyNpcItem m_NpcGridPrefab;//NPC
    [SerializeField]
    CSUI_Grid m_GridPrefab;
    [SerializeField]
    CSUI_SkillItem m_StudySkillPrefab;

    #endregion

    //枚举
    public enum TypeEnu // 学员和教练枚举
    {
        Trainee,   //学员
        Instructor //教练
    }

    //属性
    private TypeEnu m_Type;
    public TypeEnu Type
    {
        get { return m_Type; }
        set
        {
            m_Type = value;
            if (value == TypeEnu.Trainee)
            {
                m_TraineeRootUI.gameObject.SetActive(true);
                m_InstructorRootUI.gameObject.SetActive(false);
                RefreshNPCGrids();
                CSUI_MyNpcItem activeGrid = null;
                for (int i = 0; i < m_TraineeRootUI.transform.childCount; ++i)
                {
                    UICheckbox cb = m_TraineeRootUI.transform.GetChild(i).gameObject.GetComponent<UICheckbox>();
                    if (cb.isChecked)
                    {
                        activeGrid = cb.gameObject.GetComponent<CSUI_MyNpcItem>();
                        break;
                    }
                }
                ActiveNpcGrid = activeGrid;
            }
            else if (value == TypeEnu.Instructor)
            {
                m_TraineeRootUI.gameObject.SetActive(false);
                m_InstructorRootUI.gameObject.SetActive(true);
                RefreshNPCGrids();
                CSUI_MyNpcItem activeGrid = null;
                for (int i = 0; i < m_InstructorRootUI.transform.childCount; ++i)
                {
                    UICheckbox cb = m_InstructorRootUI.transform.GetChild(i).gameObject.GetComponent<UICheckbox>();
                    if (cb.isChecked)
                    {
                        activeGrid = cb.gameObject.GetComponent<CSUI_MyNpcItem>();
                        break;
                    }
                }
                ActiveNpcGrid = activeGrid;
            }
        }
    }

    private CSUI_MyNpcItem m_ActiveNpcGrid;
    public CSUI_MyNpcItem ActiveNpcGrid
    {
        get { return m_ActiveNpcGrid; }
        set
        {
            m_ActiveNpcGrid = value;
            UpdateNPCRef(m_ActiveNpcGrid == null ? null : m_ActiveNpcGrid.m_Npc);
        }
    }

    private static CSUI_Train _instance;
    public static CSUI_Train Instance
    {
        get { return _instance; }
    }

    // 数据列表
    private List<CSUI_MyNpcItem> m_TraineeGrids = new List<CSUI_MyNpcItem>();
    private List<CSUI_MyNpcItem> m_InstructorGrids = new List<CSUI_MyNpcItem>();
    private List<CSUIMyNpc> allRandomNpcs = new List<CSUIMyNpc>();//所有的随机NPC

    //private List<CSUI_Grid> m_ItemGrids = new List<CSUI_Grid>();
    private List<CSUI_SkillItem> m_SkillGrids = new List<CSUI_SkillItem>();
    private List<CSUI_SkillItem> m_StudySkillGrids = new List<CSUI_SkillItem>();
    private List<CSUIMySkill> m_StudyList = new List<CSUIMySkill>();//学习列表
    private List<CSUIMySkill> mSkills = new List<CSUIMySkill>();//当前选中一个教练时，教练有的技能

    private CSUIMyNpc mTraineeSkill = new CSUIMyNpc();//学习技能的学员
    private CSUIMyNpc mTraineeStats = new CSUIMyNpc();//属性提升的学员
    private CSUIMyNpc mInstructorStats = new CSUIMyNpc();//属性提升的教官
    private int m_SkillIndex = 0;
    private CSUIMyNpc m_RefNpc;


    #region 普通方法  按钮调用方法   CALLBACK

    private void RefreshNPCGrids()//刷新NPC节点
    {
        if (CSUI_MainWndCtrl.Instance == null) return;
        if (GetAllRandomNpcsEvent != null)
            GetAllRandomNpcsEvent();
        if (Type == TypeEnu.Trainee)
        {
            m_TraineeGrids.Clear();
            NpcGridDestroy(m_TraineeRootUI.transform);
            for (int i = 0; i < allRandomNpcs.Count; ++i)
            {
                if (!allRandomNpcs[i].HasOccupation)//学员
                {
                    m_TraineeGrids.Add(CreateNPCGird(allRandomNpcs[i], m_TraineeRootUI.transform));
                }
            }
            m_TraineeRootUI.repositionNow = true;
        }
        if (Type == TypeEnu.Instructor)
        {
            m_InstructorGrids.Clear();
            NpcGridDestroy(m_InstructorRootUI.transform);
            for (int i = 0; i < allRandomNpcs.Count; ++i)
            {
                if (allRandomNpcs[i].HasOccupation)//教练
                {
                    m_InstructorGrids.Add(CreateNPCGird(allRandomNpcs[i], m_InstructorRootUI.transform));
                }
            }
            m_InstructorRootUI.repositionNow = true;
        }
    }

    private void UpdateNPCRef(CSUIMyNpc npc)
    {
        if (npc != null)
            m_RefNpc = npc;
        if (m_RefNpc != null && !m_RefNpc.HasOccupation)
            UpdateSkills();
        if (m_RefNpc != null && m_RefNpc.HasOccupation)
            GetInstructorSkill(m_RefNpc.OwnSkills);
    }

    private void GetInstructorSkill(List<CSUIMySkill> skills)//得到教练技能的接口
    {
        if (skills.Count < 0)
            return;
        mSkills.Clear();
        foreach (CSUIMySkill ms in skills)
            mSkills.Add(ms);
        uip.items.Clear();
        foreach (CSUIMySkill ms in skills)
            uip.items.Add(ms.name);
        Debug.Log("uip.items的长度：" + uip.items.Count);
    }

    private CSUI_MyNpcItem CreateNPCGird(CSUIMyNpc npc, Transform root)//生成一个npc
    {
        CSUI_MyNpcItem npcGrid = Instantiate(m_NpcGridPrefab) as CSUI_MyNpcItem;
        npcGrid.transform.parent = root;
        CSUtils.ResetLoacalTransform(npcGrid.transform);
        npcGrid.m_UseDeletebutton = true;
        npcGrid.m_Npc = npc;
        UICheckbox cb = npcGrid.gameObject.GetComponent<UICheckbox>();
        cb.radioButtonRoot = root;
        UIEventListener.Get(npcGrid.gameObject).onActivate = OnNPCGridActive;
        return npcGrid;
    }

    private void NpcGridDestroy(Transform gridTr)//删除NPC方法
    {
        for (int i = 0; i < gridTr.childCount; i++)
        {
            Destroy(gridTr.GetChild(i).gameObject);
        }
    }

    private void UpdateSkills()
    {
        for (int i = 0; i < m_SkillGrids.Count; i++)
        {
            //if (i < m_RefNpc.OwnSkills.Count)
                //m_SkillGrids[i].SetSkill(m_RefNpc.OwnSkills[i]);
        }
    }

    private void SetServantInfo(string name, PeSex sex, int health, int healthMax, int stamina, int stamina_max, int hunger, int hunger_max, int comfort, int comfort_max,
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

    private void OnSelectionChange(string skillName)//选择一个教练的技能的方法
    {
        //CSUIMySkill _ms = FindSkillByName(skillName);
        //if (m_StudyList.Count > 0)
        //{
        //    foreach (CSUIMySkill ms in m_StudyList)
        //    {
        //        if (ms.name == skillName)
        //            return;
        //    }
        //}
        //if (m_SkillIndex < 5)
        //{
        //    m_StudySkillGrids[m_SkillIndex].SetIcon(_ms.iconImg);
        //    m_SkillIndex++;
        //}
        //if (m_StudyList.Count < 5 && _ms != null)
        //    m_StudyList.Add(_ms);
        //Debug.Log("需要学习的技能一共有：" + m_StudyList.Count.ToString());
        //foreach (CSUIMySkill ms in m_StudyList)
        //    Debug.Log("这些技能是：" + ms.name);
    }

    private CSUIMySkill FindSkillByName(string skillName)//通过技能名字在我的技能mSkills中找到该技能
    {
        CSUIMySkill _ms = new CSUIMySkill();
        if (mSkills.Count == 0)
            return null;
        for (int i = 0; i < mSkills.Count; i++)
        {
            if (mSkills[i].name == skillName)
                _ms = mSkills[i];
        }
        return _ms;
    }

    private void RefreshStudyList()//删除一个学习技能后，刷新学习技能列表中技能的位置
    {
        foreach (CSUI_SkillItem grid in m_StudySkillGrids)
        {
            grid.DeleteIcon();
        }
        for (int i = 0; i < m_StudyList.Count; i++)
        {
            m_StudySkillGrids[i].SetIcon(m_StudyList[i].iconImg);
        }
        uip.selection = "";
    }

    public void HideAllDeleteBtn()
    {
        if (m_StudySkillGrids.Count == 0)
            return;
        foreach (CSUI_SkillItem si in m_StudySkillGrids)
            si.OnHideBtn();
    }

    private void SetTraineeInfo(int health_max, int strength_max, int hunger_max, int stamina_max, int oxygen_max)
    {
        mMaxHealth.text = Convert.ToString(health_max);
        mMaxStrength.text = Convert.ToString(strength_max);
        mMaxHunger.text = Convert.ToString(hunger_max);
        mMaxStamina.text = Convert.ToString(stamina_max);
        mMaxOxygen.text = Convert.ToString(oxygen_max);
        //mTrainingTime.text = Convert.ToString(_mTrainingTime);
    }

    private void SetInstructorInfo(string health_add, string strength_add, string hunger_add, string stamina_add, string oxygen_add)
    {
        mAddHealth.text = health_add;
        mAddStrength.text = strength_add;
        mAddHunger.text = hunger_add;
        mAddStamina.text = stamina_add;
        mAddOxygen.text = oxygen_add;
    }

    //按钮调用方法
    private void PageTraineeOnActive(bool active)
    {
        //if (active)
        //    Type = TypeEnu.Trainee;
    }
    private void PageInstructorOnActive(bool active)
    {
        //if (active)
        //    Type = TypeEnu.Instructor;
    }
    private void PageInfoOnActive(bool active)
    {
        m_InfoPage.SetActive(active);
    }
    private void PageInvetoryOnActive(bool active)
    {
        m_InventoryPage.SetActive(active);
    }
    private void PageLearnSkillOnActive(bool active)
    {
        m_LearnSkillPage.SetActive(active);
    }
    private void PageUpgradeStatsOnActive(bool active)
    {
        m_UpgradePage.SetActive(active);
    }
    private void OnBtnStartLearnSkill()
    {
        Debug.Log("************开始学习技能************");
        if (StartStudyEvent != null)
        {
            StartStudyEvent(mTraineeSkill, m_StudyList);
            Debug.Log("************开始学习技能************");
        }
    }
    private void OnBtnStartUpgradeStats()
    {
        Debug.Log("************开始提升属性************");
        if (StartUpgradeEvent != null)
        {
            StartUpgradeEvent(mTraineeStats, mInstructorStats);
            Debug.Log("************开始提升属性************");
        }
    }

    //callback
    private void OnNPCGridActive(GameObject go, bool actvie)
    {
        if (!actvie) return;
        ActiveNpcGrid = go.GetComponent<CSUI_MyNpcItem>();
    }

    private void OnSkillGridDestroySelf(CSUI_SkillItem skillGrid)//点击技能删除按钮删除技能
    {
        Debug.Log("关闭");
        skillGrid.DeleteIcon();
        m_StudyList.RemoveAt(skillGrid.m_Index);
        m_SkillIndex--;
        RefreshStudyList();
    }



    #endregion

    #region 公用部分  UNITY_INNER

    void Awake()
    {
        _instance = this;
    }

    void Start()
    {
        InitGrid();
        m_ItemRoot.repositionNow = true;
        // Create skill Grid
        for (int i = 0; i < 5; i++)
        {
            CSUI_SkillItem grid = Instantiate(m_StudySkillPrefab) as CSUI_SkillItem; ;
            grid.transform.parent = m_SkillRoot.transform;
            grid.transform.localPosition = Vector3.zero;
            grid.transform.localRotation = Quaternion.identity;
            grid.transform.localScale = Vector3.one;
            grid.m_Index = i;
            m_SkillGrids.Add(grid);
        }
        m_SkillRoot.repositionNow = true;
        //Creat study skill Grid
        for (int i = 0; i < 5; i++)
        {
            CSUI_SkillItem grid = Instantiate(m_StudySkillPrefab) as CSUI_SkillItem;
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

    int kk = 0;
    void Update()
    {
        #region 临时用测试
        if (Input.GetKeyDown(KeyCode.P))
        {
            uip.items.Clear();
            uip.items = new List<string>() { "White" + kk.ToString(), "Red" + kk.ToString(), "Green" + kk.ToString(), "Grey" + kk.ToString() };
            kk++;
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            CreatSkill1();
            GetInstructorSkill(skilllistTest);

        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            CreatSkill2();
            GetInstructorSkill(skilllistTest);

        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            //uip.items.Clear();
            uip.selection = "";
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            uip.items.Add("1234");
            Debug.Log(uip.items.Count.ToString());
        }
        #endregion

        #region  学员信息
        if (m_RefNpc == null)
        {
            SetServantInfo("--", PeSex.Male, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            mInstructorFace_Stats.mainTexture = null;
            mInstructorFace_Stats.enabled = false;
            mInstructorFace_Skill.mainTexture = null;
            mInstructorFace_Skill.enabled = false;
            mTraineeFace_Stats.mainTexture = null;
            mTraineeFace_Stats.enabled = false;
            mTraineeFace_Skill.mainTexture = null;
            mTraineeFace_Skill.enabled = false;
            SetInstructorInfo("", "", "", "", "");
            SetTraineeInfo(0, 0, 0, 0, 0);
        }
        else
        {
            if (!m_RefNpc.HasOccupation)//学员
            {
                SetServantInfo(m_RefNpc.Name, m_RefNpc.Sex, m_RefNpc.Health, m_RefNpc.HealthMax,
                    m_RefNpc.Stamina, m_RefNpc.Stamina_max, m_RefNpc.Hunger, m_RefNpc.Hunger_max,
                    m_RefNpc.Comfort, m_RefNpc.Comfort_max, m_RefNpc.Oxygen, m_RefNpc.Oxygen_max,
                    m_RefNpc.Shield, m_RefNpc.Shield_max, m_RefNpc.Energy, m_RefNpc.Energy_max,
                    m_RefNpc.Attack, m_RefNpc.Defense);
            }
        }
        #endregion

        #region 技能和属性
        if (m_RefNpc != null)
        {
            if (m_RefNpc.HasOccupation)//教练
            {
                if (m_UpgradePage.activeSelf)//属性提升
                {
                    if (m_RefNpc.State == 10)//假设这个状态是在教学
                    {
                        mInstructorStats = null;
                        mInstructorFace_Stats.mainTexture = null;
                        mInstructorFace_Stats.enabled = false;
                        SetInstructorInfo("", "", "", "", "");
                    }
                    else if (m_RefNpc.State != 10)
                    {
                        if (mInstructorStats != m_RefNpc)
                            mInstructorStats = m_RefNpc;
                        mInstructorFace_Stats.mainTexture = m_RefNpc.RandomNpcFace;
                        mInstructorFace_Stats.enabled = true;
                        SetInstructorInfo(m_RefNpc.AddHealth, m_RefNpc.AddStrength, m_RefNpc.AddHunger, m_RefNpc.AddStamina, m_RefNpc.AddOxygen);
                    }
                }
                else if (m_LearnSkillPage.activeSelf)//技能学习
                {
                    mInstructorFace_Skill.mainTexture = m_RefNpc.RandomNpcFace;
                    mInstructorFace_Skill.enabled = true;
                }
            }
            else if (!m_RefNpc.HasOccupation)//学员
            {
                if (m_UpgradePage.activeSelf)//属性提升
                {
                    if (m_RefNpc.State == 10)//假设这个状态是在学习
                    {
                        mTraineeStats = null;
                        mTraineeFace_Stats.mainTexture = null;
                        mTraineeFace_Stats.enabled = false;
                        SetTraineeInfo(0, 0, 0, 0, 0);
                    }
                    else if (m_RefNpc.State != 10)
                    {
                        if (mTraineeStats != m_RefNpc)
                            mTraineeStats = m_RefNpc;
                        mTraineeFace_Stats.mainTexture = m_RefNpc.RandomNpcFace;
                        mTraineeFace_Stats.enabled = true;
                        SetTraineeInfo(m_RefNpc.HealthMax, m_RefNpc.Strength_max, m_RefNpc.Hunger_max, m_RefNpc.Stamina_max, m_RefNpc.Oxygen_max);
                    }
                }
                else if (m_LearnSkillPage.activeSelf)//技能学习
                {
                    if (m_RefNpc.OwnSkills.Count != 5)//还没学满5个技能
                    {
                        if (mTraineeSkill != m_RefNpc)
                            mTraineeSkill = m_RefNpc;
                        Debug.Log("学员技能学习的头像该有");
                        mTraineeFace_Skill.mainTexture = m_RefNpc.RandomNpcFace;
                        mTraineeFace_Skill.enabled = true;
                        Debug.Log(mTraineeFace_Skill.mainTexture);
                    }
                }
            }
        }
        #endregion
    }


    #endregion

    #region  Inventory  部分   这部分暂时没实现功能

    //List<Grid_N> mSkillList;
    List<Grid_N> mInteractionList;
    List<Grid_N> mPrivateList;

    int mInteractionGridCount = 10;
    int mPrivateItemGridCount = 10;

    [SerializeField]
    Grid_N mGridPrefab;

    [SerializeField]
    Transform mTsInteractionGrids;
    [SerializeField]
    Transform mTsPrivateItemGrids;

    // ui event
    public void OnDropItem_InterPackage(Grid_N grid)
    {
        Debug.Log("丢掉物品");
    }
    public void OnLeftMouseCliked_InterPackage(Grid_N grid)
    {
        Debug.Log("点击左键");
    }
    public void OnRightMouseCliked_InterPackage(Grid_N grid)
    {
        Debug.Log("点击右键");
    }
    private void InitGrid()
    {
        mInteractionList = new List<Grid_N>();
        for (int i = 0; i < mInteractionGridCount; i++)
        {
            mInteractionList.Add(Instantiate(mGridPrefab) as Grid_N);
            mInteractionList[i].gameObject.name = "Interaction" + i;
            mInteractionList[i].transform.parent = mTsInteractionGrids;
            if (i < 5)
                mInteractionList[i].transform.localPosition = new Vector3(i * 60, 0, 0);
            else
                mInteractionList[i].transform.localPosition = new Vector3((i - 5) * 60, -55, 0);
            mInteractionList[i].transform.localRotation = Quaternion.identity;
            mInteractionList[i].transform.localScale = Vector3.one;
            mInteractionList[i].SetItemPlace(ItemPlaceType.IPT_ServantInteraction, i);

            mInteractionList[i].onDropItem += OnDropItem_InterPackage;
            mInteractionList[i].onLeftMouseClicked += OnLeftMouseCliked_InterPackage;
            mInteractionList[i].onRightMouseClicked += OnRightMouseCliked_InterPackage;
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

    #endregion

    #region 测试数据
    List<CSUIMySkill> skilllistTest = new List<CSUIMySkill>();
    void CreatSkill1()
    {
        skilllistTest.Clear();
        for (int i = 0; i < 5; i++)
        {
            CSUIMySkill ms = new CSUIMySkill();
            ms.name = "skill" + (i + 1).ToString();
            ms.iconImg = "npc_big_GerdyHooke";
            skilllistTest.Add(ms);
            Debug.Log(ms.name);
        }
        Debug.Log("skilllistTest" + skilllistTest.Count);
        Debug.Log("技能生成完成");
    }
    void CreatSkill2()
    {
        skilllistTest.Clear();
        for (int i = 0; i < 5; i++)
        {
            CSUIMySkill ms = new CSUIMySkill();
            ms.name = "0skill" + (i + 1).ToString();
            ms.iconImg = "npc_big_GerdyHooke";
            skilllistTest.Add(ms);
            Debug.Log(ms.name);
        }
        Debug.Log("skilllistTest" + skilllistTest.Count);
        Debug.Log("技能生成完成");
    }

    #endregion

    #region 提供接口

    //事件接口
    public delegate void GetAllNpcsDel();
    public event GetAllNpcsDel GetAllRandomNpcsEvent;//得到所有的NPC的事件

    public delegate void StartStudyDel(CSUIMyNpc trainee, List<CSUIMySkill> studyList);
    public event StartStudyDel StartStudyEvent;//开始学习技能

    public delegate void StartUpgradeDel(CSUIMyNpc trainee, CSUIMyNpc instructor);
    public event StartUpgradeDel StartUpgradeEvent;//开始提升属性


    public void GetAllRandomNpcsMethod(List<CSUIMyNpc> _allNpcs) //得到所有随机npc接口
    {
        if (_allNpcs.Count > 0)
            allRandomNpcs = _allNpcs;
        Debug.Log("allRandomNpcs1:" + allRandomNpcs.Count);
    }

    public void AddPersonnel(CSUIMyNpc npc)//增加一个npc
    {
        if (npc == null)
            Debug.LogWarning("The giving npc is null.");
        if (!npc.IsRandom)
            return;
        // Trainee Npc
        if (!npc.HasOccupation)
        {
            CSUI_MyNpcItem npcGrid = CreateNPCGird(npc, m_TraineeRootUI.transform);
            m_TraineeRootUI.repositionNow = true;
            m_TraineeGrids.Add(npcGrid);
        }
        // Instructor Npc
        else
        {
            CSUI_MyNpcItem npcGrid = CreateNPCGird(npc, m_InstructorRootUI.transform);
            m_InstructorRootUI.repositionNow = true;
            m_InstructorGrids.Add(npcGrid);
        }
    }

    public void RemovePersonnel(CSUIMyNpc npc)//减少一个npc
    {
        if (npc == null)
            Debug.LogWarning("The giving npc is null");
        if (!npc.IsRandom)
            return;
        //学员
        if (!npc.HasOccupation)
        {
            int index = m_TraineeGrids.FindIndex(item0 => item0.m_Npc == npc);
            if (index != -1)
            {
                // If this icon is checked
                if (m_TraineeGrids[index].gameObject.GetComponent<UICheckbox>().isChecked)
                {
                    if (m_TraineeGrids.Count == 1)
                    {
                        ActiveNpcGrid = null;
                    }
                    else
                    {
                        int newIndex = index == 0 ? 1 : index - 1;
                        m_TraineeGrids[newIndex].gameObject.GetComponent<UICheckbox>().isChecked = true;
                    }
                }
                DestroyImmediate(m_TraineeGrids[index].gameObject);
                m_TraineeGrids.RemoveAt(index);
                m_TraineeRootUI.repositionNow = true;
            }
            else
                Debug.LogWarning("The giving npc is not a Settler");
        }
        // 教练
        else
        {
            int index = m_InstructorGrids.FindIndex(item0 => item0.m_Npc == npc);
            if (index != -1)
            {
                // If this icon is checked
                if (m_InstructorGrids[index].gameObject.GetComponent<UICheckbox>().isChecked)
                {
                    if (m_InstructorGrids.Count == 1)
                    {
                        ActiveNpcGrid = null;
                    }
                    else
                    {
                        int newIndex = index == 0 ? 1 : index - 1;
                        m_InstructorGrids[newIndex].gameObject.GetComponent<UICheckbox>().isChecked = true;
                    }
                }
                DestroyImmediate(m_InstructorGrids[index].gameObject);
                m_InstructorGrids.RemoveAt(index);
                m_InstructorRootUI.repositionNow = true;
            }
            else
                Debug.LogWarning("The giving npc is not a Settler");
        }
    }

    #endregion   
}
