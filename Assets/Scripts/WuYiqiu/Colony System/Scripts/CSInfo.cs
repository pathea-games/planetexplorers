
/**************************************************************
 *                       [CSInfo.cs]
 *
 *    Colony System infomation Class
 *
 *    Contain information attributes for CS Objects
 *
 *
 **************************************************************/

//--------------------------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

//--------------------------------------------------------------

[System.Serializable]
public class CSInfo
{
	public float m_Durability	= 2000;
	public float m_RepairTime	= 1000;
	public float m_DeleteTime	= 1000;
	public float m_Power		= 255;
	public int 	 m_WorkersCnt   = 0;
	public int workSound = -1;
}

// Assembly information class
[System.Serializable]
public class CSAssemblyInfo : CSInfo
{	
	[System.Serializable]
	public class LevelData
	{
		public float radius;
		public int dwellingsCnt;
        //public int barracksCnt;
		public int storageCnt;
        //public int armoryCnt;
        //public int pubCnt;
		public int farmCnt;
		public int factoryCnt;
        //public int clinicCnt;
        //public int labCnt;
		public int EngineeringCnt;
		public int RepairMachineCnt;
		public int EnhanceMachineCnt;
		public int RecycleMachineCnt;
		public int coalPlantCnt;
        public int processingCnt;
        public int tradePostCnt = 1;
        public int trainCenterCnt = 1;
        public int medicalCheckCnt = 1;
        public int medicalTreatCnt = 1;
		public int medicalTentCnt = 1;
		public int fusionPlantCnt = 1;

		// 对怪物造成伤害
		public float damageCD;
		public float damage;
	
		// Upgrade Item  and Time
		public List<int> itemIDList= new List<int> ();
		public List<int> itemCnt = new List<int> ();
		public float upgradeTime;
	}
		
	public List<LevelData> m_Levels= new List<LevelData> ();
}

// Power Plant Information class
[System.Serializable]
public class CSPowerPlantInfo : CSInfo
{
	public float m_Radius;
	public float m_WorkedTime;
	public float m_ChargingRate; 
	public List<int> m_WorkedTimeItemID= new List<int> ();
	public List<int> m_WorkedTimeItemCnt = new List<int> ();
}

//[System.Serializable]
//public class CSFusionPlantInfo : CSInfo
//{
//	public float m_Radius;
//	public float m_WorkedTime;
//	public float m_ChargingRate; 
//	public List<int> m_WorkedTimeItemID= new List<int> ();
//	public List<int> m_WorkedTimeItemCnt = new List<int> ();
//}

//Storage information class
[System.Serializable]
public class CSStorageInfo : CSInfo
{
	public int m_MaxItem;
	public int m_MaxEquip;
	public int m_MaxRecource;
	public int m_MaxArmor;
}

//Engineer information class
[System.Serializable]
public class CSEnginnerInfo : CSInfo
{
	public float m_EnhanceBaseTime;
	public float m_PatchBaseTime;
	public float m_RecycleBaseTime;
}

//Repair Information class
[System.Serializable]
public class CSRepairInfo : CSInfo
{
	public float m_BaseTime;
}

//Enhance Information class
[System.Serializable]
public class CSEnhanceInfo : CSInfo
{
	public float m_BaseTime;
}

// Recycle Information class
[System.Serializable]
public class CSRecycleInfo : CSInfo
{
	public float m_BaseTime;
}

// Dwelling Information class
[System.Serializable]
public class CSDwellingsInfo : CSInfo
{
	
}

[System.Serializable]
public class CSFarmInfo : CSInfo
{

}

[System.Serializable]
public class CSFactoryInfo : CSInfo
{

}

[System.Serializable]
public class CSProcessingInfo:CSInfo
{

}

[System.Serializable]
public class CSTradeInfo:CSInfo
{

}

[System.Serializable]
public class CSTrainingInfo : CSInfo
{
    public float m_BaseTime=30;
}


[System.Serializable]
public class CSCheckInfo : CSInfo
{
    public float m_BaseTime = 40;
}

[System.Serializable]
public class CSTreatInfo : CSInfo
{
    public float m_BaseTime = 20;
}

[System.Serializable]
public class CSTentInfo : CSInfo
{
    public float m_BaseTime = 20;
}


public class CSInfoMgr
{
    #region load data from database
    #region FIXED_INFO_FOR_OBJECT

    public static CSAssemblyInfo m_AssemblyInfo = new CSAssemblyInfo ();

    public static CSPowerPlantInfo m_ppCoal= new CSPowerPlantInfo();

    public static CSStorageInfo m_StorageInfo= new CSStorageInfo();

    public static CSEnginnerInfo m_EngineerInfo= new CSEnginnerInfo();

    public static CSRepairInfo m_RepairInfo= new CSRepairInfo();

    public static CSEnhanceInfo m_EnhanceInfo= new CSEnhanceInfo();

    public static CSRecycleInfo m_RecycleInfo= new CSRecycleInfo();

    public static CSDwellingsInfo m_DwellingsInfo= new CSDwellingsInfo();

    public static CSFarmInfo m_FarmInfo= new CSFarmInfo();

    public static CSFactoryInfo m_FactoryInfo= new CSFactoryInfo();

    public static CSProcessingInfo m_ProcessingInfo= new CSProcessingInfo();

    public static CSTradeInfo m_Trade = new CSTradeInfo();

    public static CSTrainingInfo m_Train = new CSTrainingInfo();

    public static CSCheckInfo m_Check = new CSCheckInfo();

    public static CSTreatInfo m_Treat = new CSTreatInfo();

	public static CSTentInfo m_Tent = new CSTentInfo();
	
	public static CSPowerPlantInfo m_ppFusion= new CSPowerPlantInfo();

    #endregion
    public static void LoadData()
    {
        Mono.Data.SqliteClient.SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("colonyinfo");
        while (reader.Read())
        {
            int id = Convert.ToInt32(reader.GetString(reader.GetOrdinal("id")));

            PublicAttr(GetInfo(id), reader);
            string str = reader.GetString(reader.GetOrdinal("_property"));
            if (str != "0")
            {
                SplitInfo(str, id);
			}
        }
    }

    static void PublicAttr(CSInfo info, Mono.Data.SqliteClient.SqliteDataReader reader)
    {
        info.m_Durability = Convert.ToSingle(reader.GetString(reader.GetOrdinal("durability")));
        info.m_RepairTime = Convert.ToSingle(reader.GetString(reader.GetOrdinal("repairtime")));
        info.m_DeleteTime = Convert.ToSingle(reader.GetString(reader.GetOrdinal("deletetime")));
        info.m_Power = Convert.ToSingle(reader.GetString(reader.GetOrdinal("power")));
		info.m_WorkersCnt = Convert.ToInt32(reader.GetString(reader.GetOrdinal("workerscnt")));
		info.workSound =  Convert.ToInt32(reader.GetString(reader.GetOrdinal("worksound")));
        if (Application.isEditor)
            info.m_DeleteTime = 20;
    }

    public static CSInfo GetInfo(int id)
    {
        switch (id)
        {
            case ColonyIDInfo.COLONY_ASSEMBLY:
                return m_AssemblyInfo;
            case ColonyIDInfo.COLONY_PPCOAL:
                return m_ppCoal;
            case ColonyIDInfo.COLONY_STORAGE:
                return m_StorageInfo;
            case ColonyIDInfo.COLONY_REPAIR:
                return m_RepairInfo;
            case ColonyIDInfo.COLONY_DWELLINGS:
                return m_DwellingsInfo;
            case ColonyIDInfo.COLONY_ENHANCE:
                return m_EnhanceInfo;
            case ColonyIDInfo.COLONY_RECYCLE:
                return m_RecycleInfo;
            case ColonyIDInfo.COLONY_FARM:
                return m_FarmInfo;
            case ColonyIDInfo.COLONY_FACTORY:
                return m_FactoryInfo;
            case ColonyIDInfo.COLONY_PROCESSING:
                return m_ProcessingInfo;
            case ColonyIDInfo.COLONY_TRADE:
                return m_Trade;
            case ColonyIDInfo.COLONY_TRAIN:
                return m_Train;
            case ColonyIDInfo.COLONY_CHECK:
                return m_Train;
            case ColonyIDInfo.COLONY_TREAT:
                return m_Treat;
            case ColonyIDInfo.COLONY_TENT:
                return m_Tent;
			case ColonyIDInfo.COLONY_FUSION:
				return m_ppFusion;
            default:
                Debug.LogError("ColonySystem itemid is wrong id = " + id);
                break;
        }
        return null;
    }
    static void SplitInfo(string str, int id)
    {
        if (str.Length == 0)
            return;
        switch (id)
        {
            case ColonyIDInfo.COLONY_ASSEMBLY:
                LoadAssemblyInfo(str);
                break;
            case ColonyIDInfo.COLONY_PPCOAL:
                LoadPPCoalInfo(str);
                break;
            case ColonyIDInfo.COLONY_STORAGE:
                LoadStorageInfo(str);
                break;
            case ColonyIDInfo.COLONY_REPAIR:
                LoadRepairInfo(str);
                break;
            case ColonyIDInfo.COLONY_DWELLINGS:
                LoadDwellingsInfo(str);
                break;
            case ColonyIDInfo.COLONY_ENHANCE:
                LoadEnhanceInfo(str);
                break;
            case ColonyIDInfo.COLONY_RECYCLE:
                LoadRecycleInfo(str);
                break;
            case ColonyIDInfo.COLONY_FARM:
                LoadFarmInfo(str);
                break;
            case ColonyIDInfo.COLONY_FACTORY:
                LoadFactoryInfo(str);
                break;
            case ColonyIDInfo.COLONY_PROCESSING:
                LoadProcessingInfo(str);
                break;
            case ColonyIDInfo.COLONY_TRADE:
                LoadTradeInfo(str);
                break;
            case ColonyIDInfo.COLONY_TRAIN:
                LoadTrainInfo(str);
                break;
            case ColonyIDInfo.COLONY_CHECK:
                LoadCheckInfo(str);
                break;
            case ColonyIDInfo.COLONY_TREAT:
                LoadTreatInfo(str);
                break;
            case ColonyIDInfo.COLONY_TENT:
                LoadTentInfo(str);
				break;
			case ColonyIDInfo.COLONY_FUSION:
				LoadFusionInfo(str);
				break;
            default:
                Debug.LogError("ColonyMgr itemid is wrong id = " + id);
                break;
        }
    }

    static void LoadAssemblyInfo(string str)
    {
        string[] strList = str.Split(';');
        for (int i = 0; i < strList.Length; i++)
        {
            CSAssemblyInfo.LevelData levelData = new CSAssemblyInfo.LevelData();
            string[] substrList = strList[i].Split(',');
            if (substrList.Length < 19)
            {
                Debug.LogError("LoadAssemblyInfo load error");
                return;
            }
            levelData.radius = Convert.ToSingle(substrList[0]);
            levelData.dwellingsCnt = Convert.ToInt32(substrList[1]);
            //levelData.barracksCnt = Convert.ToInt32(substrList[2]);
            levelData.storageCnt = Convert.ToInt32(substrList[2]);
            //levelData.armoryCnt = Convert.ToInt32(substrList[4]);
            //levelData.pubCnt = Convert.ToInt32(substrList[5]);
            levelData.farmCnt = Convert.ToInt32(substrList[3]);
            levelData.factoryCnt = Convert.ToInt32(substrList[4]);
            //levelData.clinicCnt = Convert.ToInt32(substrList[8]);
            //levelData.labCnt = Convert.ToInt32(substrList[9]);
            levelData.EngineeringCnt = Convert.ToInt32(substrList[5]);
            levelData.RepairMachineCnt = Convert.ToInt32(substrList[6]);
            levelData.EnhanceMachineCnt = Convert.ToInt32(substrList[7]);
            levelData.RecycleMachineCnt = Convert.ToInt32(substrList[8]);
			levelData.coalPlantCnt = Convert.ToInt32(substrList[9]);
            levelData.processingCnt = Convert.ToInt32(substrList[10]);
            levelData.tradePostCnt = Convert.ToInt32(substrList[11]);
            levelData.trainCenterCnt = Convert.ToInt32(substrList[12]);
            levelData.medicalCheckCnt = Convert.ToInt32(substrList[13]);
            levelData.medicalTreatCnt = Convert.ToInt32(substrList[14]);
			levelData.medicalTentCnt = Convert.ToInt32(substrList[15]);
			levelData.fusionPlantCnt = Convert.ToInt32(substrList[16]);

            string[] substrList1 = substrList[17].Split('|');
            for (int j = 0; j < substrList1.Length; j++)
                levelData.itemIDList.Add(Convert.ToInt32(substrList1[j]));

            string[] substrList2 = substrList[18].Split('|');
            for (int j = 0; j < substrList2.Length; j++)
                levelData.itemCnt.Add(Convert.ToInt32(substrList2[j]));

            levelData.upgradeTime = Convert.ToSingle(substrList[19]);
            if(Application.isEditor)
                levelData.upgradeTime = 20;//test
            m_AssemblyInfo.m_Levels.Add(levelData);
        }
    }

    static void LoadDwellingsInfo(string str)
    {
    }

    static void LoadEnhanceInfo(string str)
    {
        string[] strList = str.Split(';');
        if (strList.Length < 1)
        {
            Debug.LogError("LoadEnhanceInfo load error");
            return;
        }
        m_EnhanceInfo.m_BaseTime = Convert.ToSingle(strList[0]);
//        if(Application.isEditor)
//            m_EnhanceInfo.m_BaseTime = 20;//test
    }

    static void LoadFarmInfo(string str)
    {
    }

    static void LoadPowerPlantInfo(CSPowerPlantInfo info, string str)
    {
        string[] strList = str.Split(';');
        if (strList.Length < 5)
        {
            Debug.LogError("LoadPowerPlantInfo load error");
            return;
        }
		info.m_Radius = Convert.ToSingle(strList[0]);
		info.m_WorkedTime = Convert.ToSingle(strList[1]);
		info.m_ChargingRate = Convert.ToSingle(strList[2]);
        string[] substrList = strList[3].Split(',');
        for (int j = 0; j < substrList.Length; j++)
			info.m_WorkedTimeItemID.Add(Convert.ToInt32(substrList[j]));

        string[] substrList1 = strList[4].Split(',');
        for (int j = 0; j < substrList1.Length; j++)
			info.m_WorkedTimeItemCnt.Add(Convert.ToInt32(substrList1[j]));
//		if(Application.isEditor)
//			info.m_WorkedTime=60;//for test
    }

    static void LoadRecycleInfo(string str)
    {
        string[] strList = str.Split(';');
        if (strList.Length < 1)
        {
            Debug.LogError("LoadRecycleInfo load error");
            return;
        }
        m_RecycleInfo.m_BaseTime = Convert.ToSingle(strList[0]);
//        if(Application.isEditor)
//            m_RecycleInfo.m_BaseTime = 20;//test
    }

    static void LoadRepairInfo(string str)
    {
        string[] strList = str.Split(';');
        if (strList.Length < 1)
        {
            Debug.LogError("LoadRepairInfo load error");
            return;
        }
        m_RepairInfo.m_BaseTime = Convert.ToSingle(strList[0]);
//        if(Application.isEditor)
//            m_RepairInfo.m_BaseTime = 20;//test
    }

    static void LoadStorageInfo(string str)
    {
        string[] strList = str.Split(';');
        if (strList.Length < 3)
        {
            Debug.LogError("LoadStorageInfo load error");
            return;
        }
        m_StorageInfo.m_MaxItem = Convert.ToInt32(strList[0]);
        m_StorageInfo.m_MaxEquip = Convert.ToInt32(strList[1]);
        m_StorageInfo.m_MaxRecource = Convert.ToInt32(strList[2]);
		m_StorageInfo.m_MaxArmor = Convert.ToInt32(strList[3]);
    }

    static void LoadPPCoalInfo(string str)
    {
        LoadPowerPlantInfo(m_ppCoal, str);
    }

	static void LoadFusionInfo(string str){
		LoadPowerPlantInfo(m_ppFusion, str);
	}

    static void LoadFactoryInfo(string str)
    {
    }
    static void LoadProcessingInfo(string str)
    {
    }

    static void LoadTradeInfo(string str)
    {

    }
    static void LoadTrainInfo(string str)
	{
		string[] strList = str.Split(';');
		if (strList.Length < 1)
		{
			Debug.LogError("LoadTrainInfo load error");
			return;
		}
		m_Train.m_BaseTime = Convert.ToSingle(strList[0]);
//		if(Application.isEditor)
//			m_Train.m_BaseTime=30;
    }
    static void LoadCheckInfo(string str)
    {

    }
    static void LoadTreatInfo(string str)
    {

    }
    static void LoadTentInfo(string str)
    {

    }
    #endregion
}