using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using System;
using Pathea;
using ItemAsset.PackageHelper;

public class CSUI_Hospital : MonoBehaviour
{


    static List<int> MedicineLis = new List<int>() { 27, 81 };

    /// <summary>
    /// 判断这个物品能不能放到这个药品的格子里来
    /// </summary>
    /// <param name="_id"></param>
    /// <returns></returns>
    public static bool ItemCheck(int _id)
    {
        if (MedicineLis.Contains(_id))
            return true;
        return false;
    }



    private static CSUI_Hospital m_Instance;
    public static CSUI_Hospital Instance { get { return m_Instance; } }

    public UISprite m_CheckSpr, m_TreatSpr, m_TentSpr;//医疗器械头像

    /// <summary>
    /// 设置检查仪器图片
    /// </summary>
    public void SetCheckIcon()
    {
        if (m_CheckSpr != null)
        {
            m_CheckSpr.spriteName = "element_building_medicalcheck";
            m_CheckSpr.MakePixelPerfect();
        }
    }
    /// <summary>
    /// 设置治疗仪器图片
    /// </summary>
    public void SetTreatIcon()
    {
        if (m_TreatSpr != null)
        {
            m_TreatSpr.spriteName = "task_medical_lab";
            m_TreatSpr.MakePixelPerfect();
        }
    }
    /// <summary>
    /// 设置帐篷图片
    /// </summary>
    public void SetTentIcon()
    {
        if (m_TentSpr != null)
        {
            m_TentSpr.spriteName = "task_quarantine_tent";
            m_TentSpr.MakePixelPerfect();
        }
    }
    /// <summary>
    /// 清除检查仪器图片
    /// </summary>
    public void ClearCheckIcon()
    {
        if (m_CheckSpr != null)
        {
            m_CheckSpr.spriteName = "Null";
            m_CheckSpr.MakePixelPerfect();
        }
        //lz-2016.09.26 清除仪器的时候同时清除在这个仪器上工作的医生、病人、时间
        ExamineDoc = null;
        ExaminedPatient = null;
        CheckTimeShow(0f);
    }
    /// <summary>
    /// 清除治疗仪器图片
    /// </summary>
    public void ClearTreatIcon()
    {
        if (m_TreatSpr != null)
        {
            m_TreatSpr.spriteName = "Null";
            m_TreatSpr.MakePixelPerfect();
        }
        //lz-2016.09.26 清除仪器的时候同时清除在这个仪器上工作的医生、病人、时间
        TreatDoc = null;
        TreatmentPatient = null;
        TreatTimeShow(0f);
    }
    /// <summary>
    /// 清除帐篷图片
    /// </summary>
    public void ClearTentIcon()
    {
        if (m_TentSpr != null)
        {
            m_TentSpr.spriteName = "Null";
            m_TentSpr.MakePixelPerfect();
        }
        //lz-2016.09.26 清除仪器的时候同时清除在这个仪器上工作的医生
        Nurse = null;
    }
    /// <summary>
    ///清除所有医生的头像
    /// </summary>
    private void ClearMachineIcon()
    {
        ClearCheckIcon();
        ClearTreatIcon();
        ClearTentIcon();
    }

    public CSUI_NpcGridItem m_ExamineDoc, m_TreatDoc, m_Nurse;//医生节点
    //private CSPersonnel m_ExamineDoc, m_TreatDoc, m_Nurse;
    /// <summary>
    /// 设置检查的医生头像
    /// </summary>
    public CSPersonnel ExamineDoc
    {
        set
        {
            if (m_ExamineDoc != null)
            {
                m_ExamineDoc.m_Npc = value;
            }
        }
    }
    /// <summary>
    /// 设置治疗的医生头像
    /// </summary>
    public CSPersonnel TreatDoc
    {
        set
        {
            if (m_TreatDoc != null)
            {
                m_TreatDoc.m_Npc = value;
            }
        }
    }
    /// <summary>
    /// 设置看护的医生头像
    /// </summary>
    public CSPersonnel Nurse
    {
        set
        {
            if (m_Nurse != null)
            {
                m_Nurse.m_Npc = value;
            }
        }
    }

    //************检查的npc*************//
    public CSUI_NpcGridItem m_ExaminedPatient;
    //private CSPersonnel ExaminedNpc;
    public UILabel ExamineResultLabel;

    /// <summary>
    /// 设置需要检查的病人
    /// </summary>
    public CSPersonnel ExaminedPatient
    {
        set
        {
            m_ExaminedPatient.m_Npc = value;
            //ExaminedNpc = value;
        }
    }

    //public void ExamineResult(string _diseaseName, string _medicine)
    //{
    //    ExamineResultLabel.text = "检查结果：" + ExaminedNpc.Name + "患有" + _diseaseName + "需要" + _medicine;
    //}

    public UILabel m_CheckTimeLabel;//检查时间Label
    private int _minute, _second;//时、分、秒

    /// <summary>
    /// 检查时间显示
    /// </summary>
    /// <param name="_time"></param>
    public void CheckTimeShow(float _time)
    {
        if (m_CheckTimeLabel != null)
        {
            _minute = (int)(_time / 60);
            _second = (int)(_time - _minute * 60);
            m_CheckTimeLabel.text = TimeTransition(_minute).ToString() + ":" + TimeTransition(_second).ToString();
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

    ///////////////////需要的药品的显示////////////////////////

    public CSUI_MedicineGrid m_MedicineAboutCheck1, m_MedicineAboutCheck2;

    //public void NeededMedicine(CSTreatment _tt)
    //{
    //    if (m_MedicineAboutCheck1 != null)
    //        m_MedicineAboutCheck1.transform.parent.gameObject.SetActive(false);
    //    if (m_MedicineAboutCheck2 != null)
    //        m_MedicineAboutCheck2.transform.parent.gameObject.SetActive(false);
    //    List<ItemIdCount> _medicineList;//需要的药品列表
    //    if (_tt.medicineList == null)
    //        return;
    //    _medicineList = _tt.medicineList;

    //    PlayerPackageCmpt playerPackageCmpt = PeCreature.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();

    //    if (_medicineList.Count == 0)
    //        return;
    //    else if (_medicineList.Count == 1)//一种药
    //    {
    //        if (m_MedicineAboutCheck1 != null)
    //            m_MedicineAboutCheck1.transform.parent.gameObject.SetActive(true);
    //        m_MedicineAboutCheck1.m_Grid.SetItem(_medicineList[0].protoId);
    //        m_MedicineAboutCheck1.NeedCnt = _medicineList[0].count;
    //        m_MedicineAboutCheck1.ItemNum = playerPackageCmpt.package.GetCount(_medicineList[0].protoId);
    //    }
    //    else if (_medicineList.Count == 2)//两种药
    //    {
    //        if (m_MedicineAboutCheck1 != null)
    //            m_MedicineAboutCheck1.transform.parent.gameObject.SetActive(true);
    //        if (m_MedicineAboutCheck2 != null)
    //            m_MedicineAboutCheck2.transform.parent.gameObject.SetActive(true);
    //        m_MedicineAboutCheck1.m_Grid.SetItem(_medicineList[0].protoId);
    //        m_MedicineAboutCheck1.NeedCnt = _medicineList[0].count;
    //        m_MedicineAboutCheck1.ItemNum = playerPackageCmpt.package.GetCount(_medicineList[0].protoId);
    //        m_MedicineAboutCheck2.m_Grid.SetItem(_medicineList[1].protoId);
    //        m_MedicineAboutCheck2.NeedCnt = _medicineList[1].count;
    //        m_MedicineAboutCheck2.ItemNum = playerPackageCmpt.package.GetCount(_medicineList[1].protoId);
    //    }
    //}

    //public void ClearNeededMedicine()
    //{
    //    if (m_MedicineAboutCheck1 != null)
    //        m_MedicineAboutCheck1.transform.parent.gameObject.SetActive(false);
    //    if (m_MedicineAboutCheck2 != null)
    //        m_MedicineAboutCheck2.transform.parent.gameObject.SetActive(false);
    //}



    ///////////   治疗   ///////////
    public UILabel TreatmentLabel;
    public CSUI_NpcGridItem m_TreatmentPatient;
    //private CSPersonnel TreatmentNpc;

    /// <summary>
    /// 设置需要治疗的病人
    /// </summary>
    public CSPersonnel TreatmentPatient
    {
        set
        {
            m_TreatmentPatient.m_Npc = value;
            //TreatmentNpc = value;
        }
    }


    public UILabel m_TreatTimeLabel;//治疗时间Label
    private int _minute1, _second1;//时、分、秒

    /// <summary>
    /// 治疗的时间显示
    /// </summary>
    /// <param name="_time"></param>
    public void TreatTimeShow(float _time)
    {
        if (m_TreatTimeLabel != null)
        {
            _minute1 = (int)(_time / 60);
            _second1 = (int)(_time - _minute1 * 60);

            m_TreatTimeLabel.text = TimeTransition(_minute1).ToString() + ":" + TimeTransition(_second1).ToString();
        }
    }

    public CSUI_MedicineGrid m_MedicineAboutTreat_Show, m_MedicineAboutTreat_Use;

    /// <summary>
    /// 治疗药物设置
    /// </summary>
    /// <param name="_ic"></param>
    public void TreatMedicineShow(ItemIdCount _ic)
    {
        if (m_MedicineAboutTreat_Show == null)
            return;
        if (!m_MedicineAboutTreat_Show.transform.parent.gameObject.activeSelf)
            m_MedicineAboutTreat_Show.transform.parent.gameObject.SetActive(true);
        ItemSample _ip = new ItemSample();
        _ip.protoId = _ic.protoId;
        //ItemProto _protoData = _ip.protoData;
        m_MedicineAboutTreat_Show.m_Grid.SetItem(_ip);
        m_MedicineAboutTreat_Show.NeedCnt = _ic.count;
        //if (!hasUsedMedicine)//没得药品放在里面
        //    m_MedicineAboutTreat_Show.m_Grid.SetGridForbiden(true);
    }

    /// <summary>
    ///  清除治疗药物
    /// </summary>
    public void ClearTreatMedicine()
    {
        if (m_MedicineAboutTreat_Show == null)
            return;
        m_MedicineAboutTreat_Show.m_Grid.SetItem(null);
        //m_MedicineAboutTreat_Show.m_Grid.SetGridForbiden(false);
    }

    public delegate void MedicineDragDel(ItemObject _io, bool _inorout);
    public event MedicineDragDel MedicineDragEvent;

    //private bool hasUsedMedicine = false;//标记是否有药品放在里面

    void OnPutMedicineIn(Grid_N grid)
    {
        if (MedicineDragEvent != null)
        {
            MedicineDragEvent(grid.ItemObj, true);  //true为拖进
            //m_MedicineAboutTreat_Show.m_Grid.SetGridForbiden(false);
            m_MedicineAboutTreat_Show.m_Grid.mItemspr.enabled = false;
            //hasUsedMedicine = true;//有药品放进来了
        }
    }
    void OnPutMedicineOut(Grid_N grid)
    {
        if (MedicineDragEvent != null)
        {
            MedicineDragEvent(grid.ItemObj, false);  //false为拖出
            //m_MedicineAboutTreat_Show.m_Grid.SetGridForbiden(true);
            m_MedicineAboutTreat_Show.m_Grid.mItemspr.enabled = true;
            //hasUsedMedicine = false;//药品拖出去了
        }
    }

    public delegate void MedicineRealOp(ItemPackage _ip, bool _isMis, int _tabIndex, int _index, int _instanceId, bool _inorout);
    public MedicineRealOp mMedicineRealOp;

    //Grid_N mGrid = null;
    //ItemObject itemObj = null;
    bool _mission = false;

    void MedRealOp(Grid_N grid)//grid是药品格子
    {
        if (grid.ItemObj != null || !ItemCheck(SelectItem_N.Instance.ItemObj.protoData.itemClassId))
        {
            SelectItem_N.Instance.SetItem(null);
            return;
        }

        //mGrid = grid;
        //itemObj = SelectItem_N.Instance.ItemObj;

        UIItemPackageCtrl package = GameUI.Instance.mItemPackageCtrl;
        ItemPackage itemP = package.ItemPackage;
        if (itemP == null)
            return;

        _mission = package.isMission;
        int _tabIdex = 0;

        if (!_mission)//不是任务物品
        {
            _tabIdex = package.CurrentPickTab;
        }
        else//任务物品
        {
            _tabIdex = 0;
        }

        int _index = SelectItem_N.Instance.Index;



        if (mMedicineRealOp != null)
            mMedicineRealOp(itemP, _mission, _tabIdex, _index, SelectItem_N.Instance.ItemObj.instanceId, true);//true表示放入药品
    }

    /// <summary>
    /// 医疗所药品格子的设置
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="inorout"></param>
    public void SetLocalGrid(ItemObject obj, bool inorout = true)
    {
        if (inorout)//放进
        {
            m_MedicineAboutTreat_Use.m_Grid.SetItem(obj);
            SelectItem_N.Instance.SetItem(null);
            OnPutMedicineIn(m_MedicineAboutTreat_Use.m_Grid);
        }
        else//拿出
        {
            OnPutMedicineOut(m_MedicineAboutTreat_Use.m_Grid);
            m_MedicineAboutTreat_Use.m_Grid.SetItem(obj);
            SelectItem_N.Instance.SetItem(null);
        }
    }



    //**********静养************//


    public UIGrid m_Grid;
    private List<CSUI_NpcGridItem> m_PatientGrids = new List<CSUI_NpcGridItem>();

    public void RefreshPatientGrids(List<CSPersonnel> _patients)
    {
        if (CSUI_MainWndCtrl.Instance == null) return;

        int Len = 0;
        for (int i = 0; i < _patients.Count; ++i)
        {
            // Already has a CSUI_NPCGrid? just replace the npc reference!
            if (Len < m_PatientGrids.Count)
                m_PatientGrids[Len].m_Npc = _patients[i];
            else
            {
                CSUI_NpcGridItem npcGrid = _createNPCGird(_patients[i], m_Grid.transform);
                m_PatientGrids.Add(npcGrid);
            }
            Len++;
        }

        //Has redundant grid? just destroy it
        if (Len < m_PatientGrids.Count)
        {
            for (int i = Len; i < m_PatientGrids.Count;)
            {
                DestroyImmediate(m_PatientGrids[i].gameObject);
                m_PatientGrids.RemoveAt(i);
            }
        }
        m_Grid.repositionNow = true;
    }

    public CSUI_NpcGridItem m_NpcGridPrefab;

    private CSUI_NpcGridItem _createNPCGird(CSPersonnel npc, Transform root)
    {
        CSUI_NpcGridItem npcGrid = Instantiate(m_NpcGridPrefab) as CSUI_NpcGridItem;
        npcGrid.transform.parent = root;
        CSUtils.ResetLoacalTransform(npcGrid.transform);
        npcGrid.m_UseDeletebutton = false;
        npcGrid.m_Npc = npc;
        //npcGrid.ShowNpcName();
        UICheckbox cb = npcGrid.gameObject.GetComponent<UICheckbox>();
        cb.radioButtonRoot = root;
        return npcGrid;
    }


    //****************更新npc节点的时间********************//

    public void UpdateNpcGridTime(CSPersonnel _npc, float _seconds)
    {
        if (m_PatientGrids.Count <= 0)
            return;
        foreach (CSUI_NpcGridItem item in m_PatientGrids)
        {
            if (item.m_Npc == _npc)
                item.NpcGridTimeShow(_seconds);
        }
    }


    #region UNITY_INNER

    void Awake()
    {
        m_Instance = this;
        //m_MedicalHistoryItem = new CSUI_MedicalHistoryItem[] { };
    }

    void Start()
    {

        m_MedicineAboutTreat_Use.m_Grid.transform.FindChild("Bg").gameObject.SetActive(false);
        m_MedicineAboutTreat_Show.m_Grid.SetGridForbiden(false);

        //m_MedicineAboutTreat_Use.MedicineDragInEvent += OnPutMedicineIn;
        //m_MedicineAboutTreat_Use.MedicineDragOutEvent += OnPutMedicineOut;

        m_MedicineAboutTreat_Use.mRealOp += MedRealOp;
    }

    //float mtimer = 0.0f;

    void Update()
    {
        //药品用完了
        if (m_MedicineAboutTreat_Use.m_Grid != null && m_MedicineAboutTreat_Use.m_Grid.Item != null)
        {
            if (m_MedicineAboutTreat_Use.m_Grid.Item.GetCount() <= 0)
                m_MedicineAboutTreat_Use.m_Grid.SetItem(null);
        }

        //格子灰色的显示
        if (m_MedicineAboutTreat_Show.m_Grid.Item == null && m_MedicineAboutTreat_Use.m_Grid.Item == null)//都为空
        {
            m_MedicineAboutTreat_Show.m_Grid.SetGridForbiden(false);
        }
        else if (m_MedicineAboutTreat_Show.m_Grid.Item != null && m_MedicineAboutTreat_Use.m_Grid.Item != null)//都不为空
        {
            m_MedicineAboutTreat_Show.m_Grid.SetGridForbiden(false);
            //m_MedicineAboutTreat_Show.m_Grid.mItemspr.enabled = true;
            //m_MedicineAboutTreat_Use.m_Grid.mItemspr.enabled = true;
        }
        else if (m_MedicineAboutTreat_Show.m_Grid.Item == null && m_MedicineAboutTreat_Use.m_Grid.Item != null)
        {
            m_MedicineAboutTreat_Show.m_Grid.SetGridForbiden(false);
            //m_MedicineAboutTreat_Use.m_Grid.mItemspr.enabled = true;
        }
        else if (m_MedicineAboutTreat_Show.m_Grid.Item != null && m_MedicineAboutTreat_Use.m_Grid.Item == null)
        {
            m_MedicineAboutTreat_Show.m_Grid.SetGridForbiden(true);
            //m_MedicineAboutTreat_Show.m_Grid.mItemspr.enabled = true;
        }
    }
    #endregion


    #region 病历部分

    void OnActivate(bool active)
    {
        if (active)
        {
            GetMedicalHistoryList();
            Test();
        }
    }

    void OnEnable()
    {
        //ItemProto.Mgr.Instance.Get(1).icon
    }

    //repair
    public UICheckbox m_Checkbox, m_Treatbox, m_Tentbox;
    private List<CSEntity> m_hospitalEnties = null;
    public int m_CheckedPartType = CSConst.dtCheck;

    public void RefleshMechine(int csConstType, List<CSEntity> csEntities,CSEntity selectEnity)
    {
        m_hospitalEnties = csEntities;
        if (selectEnity != null)
        {
            m_CheckedPartType = selectEnity.m_Type;
            CSUI_MainWndCtrl.Instance.mSelectedEnntity = selectEnity;
            return;
        }

        int index = GetMechineIndex(csConstType);
        if(index >= 0)
        {
            m_CheckedPartType = csConstType;
            CSUI_MainWndCtrl.Instance.mSelectedEnntity = m_hospitalEnties[index];
        }
    }

    public int GetMechineIndex(int csConstType)
    {
        if (m_hospitalEnties == null || m_hospitalEnties.Count <= 0)
            return -1;

        for(int i=0;i<m_hospitalEnties.Count;i++)
        {
            if (csConstType == m_hospitalEnties[i].m_Type)
                return i;
        }
        return -1;
    }

    void OnCheckActive(bool active)
    {
        int index = GetMechineIndex(CSConst.dtCheck);
        if (index < 0)
            return;

        m_Checkbox.isChecked = active;
        RefleshMechine(CSConst.dtCheck, m_hospitalEnties, m_hospitalEnties[index]);
    }

    void OnTreatActive(bool active)
    {
        int index = GetMechineIndex(CSConst.dtTreat);
        if (index < 0)
            return;

        m_Treatbox.isChecked = active;
        RefleshMechine(CSConst.dtTreat,m_hospitalEnties, m_hospitalEnties[index]);     
    }

    void OnTentActive(bool active)
    {
        int index = GetMechineIndex(CSConst.dtTent);
        if (index < 0)
            return;

        m_Tentbox.isChecked = active;
        RefleshMechine(CSConst.dtTent, m_hospitalEnties, m_hospitalEnties[index]);
    }

    

    void Test()
    {
        MedicalHistoryList.Clear();
        for (int i = 0; i < 7; i++)
        {
            CSTreatment item = new CSTreatment();
            item.npcName = "LongLongName" + i.ToString();
            item.diseaseName = "dis" + i.ToString();
            item.treatName = "fangan" + i.ToString();
            MedicalHistoryList.Add(item);
        }
    }

    List<CSTreatment> MedicalHistoryList = new List<CSTreatment>();

    void GetMedicalHistoryList()
    {
        if (CSMain.GetTreatmentList() == null)
            return;
        MedicalHistoryList = CSMain.GetTreatmentList();
    }

    int m_CurrentPageIndex = 1;

    int m_PageCount = 2;

    void OnFirstPageClick(bool active)
    {
        if (!active)
            return;
        m_CurrentPageIndex = 1;
        GetMedicalHistoryList();
        //Test();
        RefreshMedicalHistory();
    }

    void OnSecondPageClick(bool active)
    {
        if (!active)
            return;
        m_CurrentPageIndex = 2;
        GetMedicalHistoryList();
        //Test();
        RefreshMedicalHistory();
    }

    void OnThirdPageClick(bool active)
    {
        if (!active)
            return;
        m_CurrentPageIndex = 3;
        GetMedicalHistoryList();
        //Test();
        RefreshMedicalHistory();
    }

    void OnForthPageClick(bool active)
    {
        if (!active)
            return;
        m_CurrentPageIndex = 4;
        GetMedicalHistoryList();
        //Test();
        RefreshMedicalHistory();
    }

    public CSUI_MedicalHistoryItem[] m_MedicalHistoryItem;

    void RefreshMedicalHistory()
    {
        //lz-2016.10.16 错误 #4472 空对象
        if (null == m_MedicalHistoryItem || m_MedicalHistoryItem.Length <= 0)
            return;
        ClearMedicalHistory();
        if (MedicalHistoryList == null || MedicalHistoryList.Count <= 0)
            return;
        //Debug.Log("MedicalHistoryList.Count:" + MedicalHistoryList.Count);
        int Len = 0;
        for (int i = (m_CurrentPageIndex - 1) * m_PageCount; i < MedicalHistoryList.Count; i++)
        {
            if (i >= m_PageCount * m_CurrentPageIndex)
                return;
            if (Len >= m_MedicalHistoryItem.Length)
                return;
            //Debug.Log("m_MedicalHistoryItem.Count:" + m_MedicalHistoryItem.Length);

            //lz-2016.10.16 错误 #4472 空对象
            if (null == MedicalHistoryList[i] || MedicalHistoryList[i].medicineList.Count <= 0)
                continue;
            if (null == ItemProto.Mgr.Instance)
                return;
            ItemProto item=ItemProto.Mgr.Instance.Get(MedicalHistoryList[i].medicineList[0].protoId);
            if (null == item|| item.icon.Length<=0)
            {
                continue;
            }

            m_MedicalHistoryItem[Len].SetInfo(MedicalHistoryList[i].npcName, MedicalHistoryList[i].diseaseName, MedicalHistoryList[i].treatName, MedicalHistoryList[i].needTreatTimes, item.icon[0], MedicalHistoryList[i].medicineList[0].count);
            Len++;
        }
    }

    void ClearMedicalHistory()
    {
        foreach (CSUI_MedicalHistoryItem item in m_MedicalHistoryItem)
            item.ClearInfo();
    }

    /// <summary>
    /// 刷新病历接口
    /// </summary>
    public void RefreshGrid()
    {
        GetMedicalHistoryList();
        RefreshMedicalHistory();
    }
    #endregion
}
