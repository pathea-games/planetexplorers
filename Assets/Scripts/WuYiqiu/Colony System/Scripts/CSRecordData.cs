using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using CustomData;
using Pathea;
// <CETC> Record data struct
namespace CSRecord
{
	public class CSDefaultData
	{
		public int ID;

		public int ItemID;
		
		public int dType;
		
		public CSDefaultData ()
		{
			dType = CSConst.dtDefault;
		}
	}
	
	public class CSObjectData : CSDefaultData
	{
		public bool     m_Alive;
		public string 	m_Name;
		public Vector3  m_Position;
		public float  	m_Durability;
		public float  	m_CurRepairTime;
		public float  	m_RepairTime;
		public float  	m_RepairValue;
		public float  	m_CurDeleteTime;
		public float  	m_DeleteTime;
		public Bounds   m_Bounds;
		
		public Dictionary<int, int>	m_DeleteGetsItem;
		
		public CSObjectData()
		{
			m_DeleteGetsItem = new Dictionary<int, int>();
			m_Bounds = new Bounds();
		}
	}
	
	public class CSAssemblyData : CSObjectData
	{
		public bool   m_ShowShield;
		public int 	  m_Level;
		public float  m_CurUpgradeTime;
		public float  m_UpgradeTime;
		public long   m_TimeTicks;
		public int  m_MedicineResearchTimes=0;
		public double  m_MedicineResearchState=0;

		public CSAssemblyData ()
		{
			m_ShowShield = true;
			dType = CSConst.dtAssembly;
		}
	}

	public class CSPowerPlanetData : CSObjectData
	{
		public Dictionary<int, int> m_ChargingItems;
        public bool bShowElectric;
		public CSPowerPlanetData ()
		{
            bShowElectric = true;
			dType = CSConst.dtPowerPlant;
			m_ChargingItems = new Dictionary<int, int>();
		}
	}
	
	public class CSPPCoalData : CSPowerPlanetData
	{
		public float m_CurWorkedTime;
		public float m_WorkedTime;
		
		public CSPPCoalData ()
		{
			dType = CSConst.dtppCoal;
		}
	}

	public class CSPPSolarData: CSPowerPlanetData
	{
		public CSPPSolarData()
		{
			dType = CSConst.dtppSolar;
		}
	}
	
	public class CSPPFusionData : CSPPCoalData
	{
		public CSPPFusionData ()
		{
			dType = CSConst.dtppFusion;
		}
	}
	public class CSStorageData : CSObjectData
	{
		public Dictionary<int, int>	m_Items; 
//		public Queue<string>	m_History;


		public Queue<HistoryStruct>	m_History;
		
		public CSStorageData ()
		{
			dType = CSConst.dtStorage;
			m_Items = new Dictionary<int, int>();
//			m_History = new Queue<string>();
			m_History  = new Queue<HistoryStruct>();
		}
	}
	
	public class CSEngineerData : CSObjectData
	{
		public int   m_EnhanceItemID;
		public float m_CurEnhanceTime;
		public float m_EnhanceTime;
		
		public int   m_PatchItemID;
		public float m_CurPatchTime;
		public float m_PatchTime;
		
		public int   m_RecycleItemID;
		public float m_CurRecycleTime;
		public float m_RecycleTime;
		
		public CSEngineerData ()
		{
			dType = CSConst.dtEngineer;
		}
	}
	
	public class CSEnhanceData : CSObjectData
	{
		public int 		m_ObjID=-1;
		public float	m_CurTime;
		public float	m_Time=-1;
		
		public CSEnhanceData ()
		{
			dType = CSConst.dtEnhance;
		}
	}
	
	public class CSRepairData : CSObjectData
	{
		public int  	m_ObjID=-1;
		public float	m_CurTime;
		public float	m_Time=-1;
		
		public CSRepairData ()
		{
			dType = CSConst.dtRepair;
		}
	}
	
	public class CSRecycleData : CSObjectData
	{
		public int	 	m_ObjID=-1;
		public float	m_CurTime;
		public float	m_Time=-1;
		
		public CSRecycleData ()
		{
			dType = CSConst.dtRecyle;
		}
	}

	public class CSFarmData: CSObjectData
	{
		public Dictionary<int, int> m_PlantSeeds;
		public Dictionary<int, int> m_Tools;
		public bool m_AutoPlanting;
		public bool m_SequentialPlanting;
		public CSFarmData ()
		{
			dType = CSConst.dtFarm;
			m_PlantSeeds = new Dictionary<int, int>();
			m_Tools = new Dictionary<int, int>();
		}
	}

	public class CSFactoryData: CSObjectData
	{
//		public Dictionary<int, CompoudItem> m_CompoudItems;
		public List<CompoudItem>  m_CompoudItems;

		public CSFactoryData()
		{
			dType = CSConst.dtFactory;
			m_CompoudItems = new List<CompoudItem>();
		}
	}
	
	public class CSDwellingsData : CSObjectData
	{
		public CSDwellingsData ()
		{
			dType = CSConst.dtDwelling;
		}
	}

    public class CSProcessingData : CSObjectData
    {
        public bool isAuto;
        public ProcessingTask[] mTaskTable;
        public CSProcessingData()
        {
            dType = CSConst.dtProcessing;
            isAuto = false;
            mTaskTable = new ProcessingTask[ProcessingConst.TASK_NUM];
        }

        public bool HasLine(int index)
        {
            if (mTaskTable[index] == null)
            {
                return false;
            }
            if (mTaskTable[index].itemList == null)
            {
                return false;
            }
            if (mTaskTable[index].itemList.Find(item => item != null) == null)
            {
                return false;
            }
            return true;
        }

        public int TaskCount
        {
            get{
                int count=0;
                for (int i = 0; i < mTaskTable.Length;i++ )
                {
                    if (HasLine(i))
                    {
                        count++;
                    }
                }
                return count;
            }
        }
    }

    public class CSTradeData : CSObjectData
    {
		public Dictionary<int,stShopData> mShopList;
        public CSTradeData()
		{
			dType = CSConst.dtTrade;
			mShopList = new Dictionary<int, stShopData>();
		}
    }

    public class CSTrainData : CSObjectData
    {
        public List<int> instructors = new List<int>();
        public List<int> trainees = new List<int>();
        public int instructorNpcId;
        public int traineeNpcId;
        public int trainingType;
        public List<int> LearningSkillIds= new List<int> ();
        public float m_CurTime=-1;
        public float m_Time = -1;
        public CSTrainData()
        {
            dType = CSConst.dtTrain;
        }
    }
    public class CSCheckData : CSObjectData
    {
        public List<int> npcIds = new List<int>();
        public float m_CurTime;
        public float m_Time=-1;
        public bool isNpcReady=false;
        public bool occupied=false;

        public CSCheckData()
        {
            dType = CSConst.dtCheck;
        }
    }

    public class CSTreatData : CSObjectData
    {
        public int m_ObjID=-1;
        public List<int> npcIds=new List<int> ();
        public float m_CurTime;
        public float m_Time=-1;
        public bool isNpcReady=false;
        public bool occupied=false;
        public CSTreatData()
        {
            dType = CSConst.dtTreat;
        }
    }
    public class CSTentData : CSObjectData
    {
        public List<int> npcIds=new List<int> ();
        public Sickbed[] allSickbeds;
        public CSTentData()
        {
            dType = CSConst.dtTent;
            allSickbeds = new Sickbed[CSMedicalTentConst.BED_AMOUNT_MAX];
            for (int i = 0; i < CSMedicalTentConst.BED_AMOUNT_MAX; i++)
            {
                allSickbeds[i] = new Sickbed(i);
            }
        }
    }


	// Personnel data
	public class CSPersonnelData : CSDefaultData
	{
		public int	m_State;
		public int  m_DwellingsID;
		public int  m_WorkRoomID;
        public int  m_Occupation;
		public int  m_WorkMode;
        public Vector3 m_GuardPos;
        public int m_ProcessingIndex=-1;
        public bool m_IsProcessing;
        public int m_TrainerType;
        public int m_TrainingType;
        public bool m_IsTraining;
	}

}
