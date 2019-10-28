using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CSRecord;
using System.Linq;

public class CSAssembly : CSEntity
{
    public override bool IsDoingJob()
    {
        return true;
    }
    public CSAssemblyData m_AssemblyData;
    public CSAssemblyData Data
    {
        get
        {
            if (m_AssemblyData == null)
                m_AssemblyData = m_Data as CSAssemblyData;
            return m_AssemblyData;
        }
    }

    // Object information
    public CSAssemblyInfo m_AssemblyInfo;
    public CSAssemblyInfo Info
    {
        get
        {
            if (m_AssemblyInfo == null)
                m_AssemblyInfo = m_Info as CSAssemblyInfo;
            return m_AssemblyInfo;
        }
    }

    public Dictionary<CSConst.ObjectType, List<CSCommon>> m_BelongObjectsMap;

	public List<CSCommon> AllPowerPlants{
		get{List<CSCommon> app = new List<CSCommon> ();
			app.AddRange(m_BelongObjectsMap[CSConst.ObjectType.PowerPlant_Coal]);
			app.AddRange(m_BelongObjectsMap[CSConst.ObjectType.PowerPlant_Fusion]);
			return app;}
	}
	public static bool IsPowerPlant(CSConst.ObjectType type){
		return type==CSConst.ObjectType.PowerPlant_Coal||type == CSConst.ObjectType.PowerPlant_Fusion;
	}
    // Farm
    public CSFarm Farm
    {
        get
        {
            if (m_BelongObjectsMap[CSConst.ObjectType.Farm].Count == 0)
                return null;
            else
                return m_BelongObjectsMap[CSConst.ObjectType.Farm][0] as CSFarm;
        }
    }

    public List<CSCommon> Storages
    {
        get
        {
            if (m_BelongObjectsMap[CSConst.ObjectType.Storage].Count == 0)
                return null;
            else
                return m_BelongObjectsMap[CSConst.ObjectType.Storage];
        }
    }
	public CSFactory Factory
	{
		get{
			if (m_BelongObjectsMap[CSConst.ObjectType.Factory].Count == 0)
				return null;
			else
				return m_BelongObjectsMap[CSConst.ObjectType.Factory][0] as CSFactory;
		}
	}

    public List<CSCommon> Dwellings
    {
        get
        {
            if (m_BelongObjectsMap[CSConst.ObjectType.Dwelling].Count == 0)
                return null;
            else
                return m_BelongObjectsMap[CSConst.ObjectType.Dwelling];
        
        }
    }

    public CSProcessing ProcessingFacility
    {
        get
        {
            if (m_BelongObjectsMap[CSConst.ObjectType.Processing].Count == 0)
                return null;
            else
                return m_BelongObjectsMap[CSConst.ObjectType.Processing][0] as CSProcessing;
        }
    }
    public CSTraining TrainingCenter
    {
        get
        {
            if (m_BelongObjectsMap[CSConst.ObjectType.Train].Count == 0)
                return null;
            else
                return m_BelongObjectsMap[CSConst.ObjectType.Train][0] as CSTraining;
        }
    }
    public CSTrade TradePost
    {
        get
        {
            if (m_BelongObjectsMap[CSConst.ObjectType.Trade].Count == 0)
                return null;
            else
                return m_BelongObjectsMap[CSConst.ObjectType.Trade][0] as CSTrade;
        }
    }

    public CSMedicalCheck MedicalCheck
    {
        get
        {
            if (m_BelongObjectsMap[CSConst.ObjectType.Check].Count == 0)
                return null;
            else
                return m_BelongObjectsMap[CSConst.ObjectType.Check][0] as CSMedicalCheck;
        }
    }
    public CSMedicalTreat MedicalTreat
    {
        get
        {
            if (m_BelongObjectsMap[CSConst.ObjectType.Treat].Count == 0)
                return null;
            else
                return m_BelongObjectsMap[CSConst.ObjectType.Treat][0] as CSMedicalTreat;
        }
    }

    public CSMedicalTent MedicalTent
    {
        get
        {
            if (m_BelongObjectsMap[CSConst.ObjectType.Tent].Count == 0)
                return null;
            else
                return m_BelongObjectsMap[CSConst.ObjectType.Tent][0] as CSMedicalTent;
        }
    }


    // The number of each Object Limits
    private Dictionary<CSConst.ObjectType, int> m_ObjLimitMap;

    // Upgarde Counter script
    private CounterScript m_CSUpgrade;

    public bool isUpgrading { get { return (m_CSUpgrade != null); } }

    // Attribute about level information
    public int Level { get { return Data.m_Level; } }
	public float Radius { get { return Info.m_Levels[Level].radius; } }
	public float LargestRadius { get { return Info.m_Levels[2].radius; } }
	public float UpgradeTime { get { return Info.m_Levels[Level].upgradeTime; } }
    public int[] UpgradeItems { get { return Info.m_Levels[Level].itemIDList.ToArray(); } }
    public int[] UpgradeItemCnt { get { return Info.m_Levels[Level].itemCnt.ToArray(); } }
    public float damageCD { get { return Info.m_Levels[Level].damageCD; } }
    public float damage { get { return Info.m_Levels[Level].damage; } }


    // in danger
    public bool IsDangerous
    {
        get
        {
            if (gameObject == null)
                return false;
            CSAssemblyObject csao = gameObject.GetComponent<CSAssemblyObject>();
            PolarShield csps = csao.CurEnergySheild;
            if (csps != null)
                return !csps.IsEmpty;
            return false;
        }
    }

    // Show or hide the shileder
    public bool bShowShield { get { return Data.m_ShowShield; } set { Data.m_ShowShield = value; } }

    // AI Erode Map
    private int m_ErodeMapId = 0;

    //medicineSupply
	public double MedicineResearchState{
		get{return Data.m_MedicineResearchState;}
		set{Data.m_MedicineResearchState=value;}
	}
	public int MedicineResearchTimes{
		get{return Data.m_MedicineResearchTimes;}
		set{Data.m_MedicineResearchTimes=value;}
	}
	public bool isSearchingClod = false;

	// Constructor
    public CSAssembly()
    {
        m_Type = CSConst.etAssembly;
        m_ObjLimitMap = new Dictionary<CSConst.ObjectType, int>();
		m_BelongObjectsMap = new Dictionary<CSConst.ObjectType, List<CSCommon>>();
		m_BelongObjectsMap.Add(CSConst.ObjectType.PowerPlant_Coal, new List<CSCommon>());
		m_BelongObjectsMap.Add(CSConst.ObjectType.PowerPlant_Fusion, new List<CSCommon>());
        m_BelongObjectsMap.Add(CSConst.ObjectType.Storage, new List<CSCommon>());
        m_BelongObjectsMap.Add(CSConst.ObjectType.Enhance, new List<CSCommon>());
        m_BelongObjectsMap.Add(CSConst.ObjectType.Repair, new List<CSCommon>());
        m_BelongObjectsMap.Add(CSConst.ObjectType.Recyle, new List<CSCommon>());
        m_BelongObjectsMap.Add(CSConst.ObjectType.Dwelling, new List<CSCommon>());
        m_BelongObjectsMap.Add(CSConst.ObjectType.Farm, new List<CSCommon>());
        m_BelongObjectsMap.Add(CSConst.ObjectType.Factory, new List<CSCommon>());
        m_BelongObjectsMap.Add(CSConst.ObjectType.Trade, new List<CSCommon>());
        m_BelongObjectsMap.Add(CSConst.ObjectType.Processing, new List<CSCommon>());
        m_BelongObjectsMap.Add(CSConst.ObjectType.Train, new List<CSCommon>());
        m_BelongObjectsMap.Add(CSConst.ObjectType.Check, new List<CSCommon>());
        m_BelongObjectsMap.Add(CSConst.ObjectType.Treat, new List<CSCommon>());
		m_BelongObjectsMap.Add(CSConst.ObjectType.Tent, new List<CSCommon>());
        m_Grade = CSConst.egtUrgent;

    }

    // Level
    public bool SetLevel(int Level)
    {
        if (Level >= Info.m_Levels.Count || Level < 0)

            return false;

        Data.m_Level = Level;
        m_ObjLimitMap[CSConst.ObjectType.Assembly] = 1;
        m_ObjLimitMap[CSConst.ObjectType.Storage] = Info.m_Levels[Data.m_Level].storageCnt;
		m_ObjLimitMap[CSConst.ObjectType.PowerPlant_Coal] = Info.m_Levels[Data.m_Level].coalPlantCnt;
		m_ObjLimitMap[CSConst.ObjectType.PowerPlant_Fusion] = Info.m_Levels[Data.m_Level].fusionPlantCnt;
        m_ObjLimitMap[CSConst.ObjectType.Engineer] = Info.m_Levels[Data.m_Level].EngineeringCnt;
        m_ObjLimitMap[CSConst.ObjectType.Enhance] = Info.m_Levels[Data.m_Level].EnhanceMachineCnt;
        m_ObjLimitMap[CSConst.ObjectType.Repair] = Info.m_Levels[Data.m_Level].RepairMachineCnt;
        m_ObjLimitMap[CSConst.ObjectType.Recyle] = Info.m_Levels[Data.m_Level].RecycleMachineCnt;
        m_ObjLimitMap[CSConst.ObjectType.Dwelling] = Info.m_Levels[Data.m_Level].dwellingsCnt;
        m_ObjLimitMap[CSConst.ObjectType.Factory] = Info.m_Levels[Data.m_Level].factoryCnt;
        m_ObjLimitMap[CSConst.ObjectType.Farm] = Info.m_Levels[Data.m_Level].farmCnt;
        m_ObjLimitMap[CSConst.ObjectType.Trade] = Info.m_Levels[Data.m_Level].tradePostCnt;
        m_ObjLimitMap[CSConst.ObjectType.Processing] = Info.m_Levels[Data.m_Level].processingCnt;
        m_ObjLimitMap[CSConst.ObjectType.Train] = Info.m_Levels[Data.m_Level].trainCenterCnt;
        m_ObjLimitMap[CSConst.ObjectType.Check] = Info.m_Levels[Data.m_Level].medicalCheckCnt;
        m_ObjLimitMap[CSConst.ObjectType.Treat] = Info.m_Levels[Data.m_Level].medicalTreatCnt;
        m_ObjLimitMap[CSConst.ObjectType.Tent] = Info.m_Levels[Data.m_Level].medicalTentCnt;
        return true;
    }

    // Upgrade 
    public void StartUpgradeCounter()
    {
        StartUpgradeCounter(0, UpgradeTime);
        //StartUpgradeCounter(0, 20);//test
    }

    public void StartUpgradeCounter(float curTime, float finalTime)
    {
        if (finalTime < 0F)
            return;

        if (m_CSUpgrade == null)
            m_CSUpgrade = CSMain.Instance.CreateCounter(Name + " Upgrade", curTime, finalTime);
        else
        {
            m_CSUpgrade.Init(curTime, finalTime);
        }
        if (!GameConfig.IsMultiMode)
        {
            m_CSUpgrade.OnTimeUp = OnUpgraded;
        }
    }

    public void OnUpgraded()
    {
        SetLevel(Data.m_Level + 1);
		ChangeState();
		RefreshErodeMap();
		RefreshAssemblyObject();
        // Event
        ExcuteEvent(CSConst.eetAssembly_Upgraded);
    }

//	public void RefreshClod(){
//		if(m_MgCreator!=null){
//			if(!isSearchingClod&&Farm!=null&&Farm.IsRunning&&m_MgCreator.m_Clod != null&&ModelObj!=null)
//				m_MgCreator.StartCoroutine(CSMain.Instance.SearchVaildClodForAssembly(this));
//		}
//	}

    public void StopCounter()
    {
        CSMain.Instance.DestoryCounter(m_CSUpgrade);
        m_CSUpgrade = null;
    }

    public void SetCounter(float curCounter)
    {
        if (null != m_CSUpgrade)
            m_CSUpgrade.SetCurCounter(curCounter);
    }

    public bool IsOutOfLimit(CSConst.ObjectType type)
    {
        if (m_ObjLimitMap[type] <= m_BelongObjectsMap[type].Count)
            return true;

        return false;
    }

    public int GetEntityCnt(CSConst.ObjectType type)
    {
        return m_BelongObjectsMap[type].Count;
    }

    public int GetLimitCnt(CSConst.ObjectType type)
    {
        return m_ObjLimitMap[type];
    }

    public bool InRange(Vector3 pos)
    {
        float dist = Vector3.Distance(pos, Position);
        if (dist > Radius)
            return false;
        return true;
    }

	public bool InLargestRange(Vector3 pos)
	{
		float dist = Vector3.Distance(pos, Position);
		if (dist > Info.m_Levels[2].radius)
			return false;
		return true;
	}
	
	public bool OutOfCount(CSConst.ObjectType type)
    {
        int count = m_BelongObjectsMap[type].Count;

        if (count < m_ObjLimitMap[type])
            return true;
        return false;
    }

    public int[] GetLevelUpItem()
    {
        return Info.m_Levels[Level].itemIDList.ToArray();
    }

    public int[] GetLevelUpItemCnt()
    {
        return Info.m_Levels[Level].itemCnt.ToArray();
    }

    public int GetMaxLevel()
    {
        return Info.m_Levels.Count - 1;
    }

    // Ai Erode
    public void InitErodeMap(Vector3 pos, float radius)
    {
        m_ErodeMapId = AIErodeMap.AddErode(pos, radius);
    }

    public void RemoveErodeMap()
    {
        AIErodeMap.RemoveErode(m_ErodeMapId);
    }

    public void RefreshErodeMap()
    {
        AIErodeMap.UpdateErode(m_ErodeMapId, Position, Radius);
    }

	public void RefreshAssemblyObject(){
		if(gameObject!=null){
			CSAssemblyObject csao = gameObject.GetComponent<CSAssemblyObject>();
			if(csao!=null)
				csao.RefreshObject();
		}
	}

    public List<CSCommon> GetBelongCommons()
    {
        List<CSCommon> commons = new List<CSCommon>();

        foreach (List<CSCommon> csc in m_BelongObjectsMap.Values)
        {
            commons.AddRange(csc);
        }
        return commons;
    }

    public void AddBelongBuilding(CSConst.ObjectType type, CSCommon building)
	{
        m_BelongObjectsMap[type].Add(building);
        ExcuteEvent(CSConst.eetAssembly_AddBuilding, building);
    }

    public void RemoveBelongBuilding(CSConst.ObjectType type, CSCommon building)
    {
		m_BelongObjectsMap[type].Remove(building);
        ExcuteEvent(CSConst.eetAssembly_RemoveBuilding, building);
    }

    #region MANAGE_COMMON_FUNC

    public int AttachCommonEntity(CSCommon csc)
    {
        if (csc.Assembly == this)
            return CSConst.rrtUnkown;

        if (!InRange(csc.Position))
            return CSConst.rrtOutOfRadius;

        CSConst.ObjectType type = (CSConst.ObjectType)csc.m_Type;
        if (IsOutOfLimit(type))
            return CSConst.rrtOutOfRange;

        //		m_BelongObjectsMap[(CSConst.ObjectType) csc.m_Type].Add(csc);
        AddBelongBuilding((CSConst.ObjectType)csc.m_Type, csc);
        csc.Assembly = this;

        CSElectric cse = csc as CSElectric;
        if (cse != null)
        {
            foreach (CSCommon power in AllPowerPlants)
            {
                CSPowerPlant cspp = power as CSPowerPlant;
                cspp.AttachElectric(cse);
				if (cse.IsRunning)
                    break;
            }
        }

        csc.ChangeState();

        return CSConst.rrtSucceed;
    }

    public void DetachCommonEntity(CSCommon csc)
    {
        csc.Assembly = null;
        //		m_BelongObjectsMap[(CSConst.ObjectType) csc.m_Type].Remove(csc);
        RemoveBelongBuilding((CSConst.ObjectType)csc.m_Type, csc);

        CSElectric cse = csc as CSElectric;
        if (cse != null)
        {
            if (cse.m_PowerPlant != null)
                cse.m_PowerPlant.DetachElectric(cse);
        }
        csc.ChangeState();
    }

    #endregion

    #region CSENTITY_FUNCTION

    public override void ChangeState()
    {
        m_IsRunning = true;

        Dictionary<int, CSCommon> commonEntities = m_Creator.GetCommonEntities();
		List<int> entityKeys = commonEntities.Keys.ToList();
		entityKeys.Sort();
		foreach (int key in entityKeys)
        {
			if (commonEntities[key].Assembly == null)
				AttachCommonEntity(commonEntities[key]);
        }

        //AIErodeMap.AddErode(AddErode)
    }

    public override void DestroySelf()
    {
        base.DestroySelf();

        // Detach all Electrics
        foreach (List<CSCommon> cscs in m_BelongObjectsMap.Values)
        {
            CSCommon[] cscsTemp = cscs.ToArray();
            foreach (CSCommon csc in cscsTemp)
                DetachCommonEntity(csc);
        }

        if (m_CSUpgrade != null)
            GameObject.Destroy(m_CSUpgrade);


    }

    public override void CreateData()
    {
        CSDefaultData ddata = null;
        bool isNew;
        if (GameConfig.IsMultiMode)
        {
            isNew = MultiColonyManager.Instance.AssignData(ID, CSConst.dtAssembly, ref ddata, _ColonyObj);
        }
        else
        {
            isNew = m_Creator.m_DataInst.AssignData(ID, CSConst.dtAssembly, ref ddata);
        }

        m_Data = ddata as CSAssemblyData;
        
        if (isNew)
        {
            Data.m_Name = CSUtils.GetEntityName(m_Type);
            Data.m_Durability = Info.m_Durability;

            SetLevel(0);
        }
        else
        {
            StartRepairCounter(Data.m_CurRepairTime, Data.m_RepairTime, Data.m_RepairValue);
            StartDeleteCounter(Data.m_CurDeleteTime, Data.m_DeleteTime);
            StartUpgradeCounter(Data.m_CurUpgradeTime, Data.m_UpgradeTime);

            SetLevel(Data.m_Level);
        }
    }

    public override void RemoveData()
    {
        m_Creator.m_DataInst.RemoveObjectData(ID);
    }

//    public override void OnDamaged(GameObject caster, float damge)
//    {
//        base.OnDamaged(caster, damge);
//
//        if (caster == null || gameObject == null)
//            return;
//    }

    public override void Update()
    {
        base.Update();

        Data.m_TimeTicks = m_Creator.Timer.Tick;

        // Upgrade
        if (m_CSUpgrade != null)
        {
            Data.m_CurUpgradeTime = m_CSUpgrade.CurCounter;
            Data.m_UpgradeTime = m_CSUpgrade.FinalCounter;
        }
        else
        {
            Data.m_CurUpgradeTime = 0F;
            Data.m_UpgradeTime = -1F;
        }
    }
    #endregion
}
