
/**************************************************************
 *                       [CSEntiy.cs]
 *
 *    Colony System Entity Root Class.
 *
 *    All Colony System Object will Inherit it.
 *
 *
 **************************************************************/

//--------------------------------------------------------------

using UnityEngine;
using System.Collections;
using CSRecord;
using System.Collections.Generic;
using ItemAsset.PackageHelper;

//--------------------------------------------------------------
using Pathea;
using ItemAsset;

public struct CSEntityAttr
{
    public int m_InstanceId;//objectID
    public int m_protoId;
    public int m_Type;
    public Vector3 m_Pos;
    public GameObject m_Obj;
    public GameObject m_LogicObj;
    public int m_Power;
    public Bounds m_Bound;
    public ColonyBase m_ColonyBase;//multiMode only
}

// Colony System base entity class for inherit class
public abstract class CSEntity
{
	public int tipsCounter0 =0;
	public int tipsIntervalFrameDurabilityLow = 1800;


    public ColonyBase _ColonyObj;
    public ColonyNetwork _Net
    {
        get { 
			if(_ColonyObj==null)
				return null;
			return _ColonyObj._Network; }
    }

    public int ID = -1;
    public int m_Type;

    protected int m_Grade;	// 
    public int Grade { get { return m_Grade; } }

    public CSCreator m_Creator;

    public CSMgCreator m_MgCreator
    {
        get
        {
            return m_Creator as CSMgCreator;
        }
    }
    public bool IsMine
    {
        get { return m_Creator == CSMain.s_MgCreator; }
    }
	

    protected GameObject m_LogicObj;
    public virtual GameObject gameLogic
    {
        get
        {
            return m_LogicObj;
        }

        set
        {
            m_LogicObj = value;
        }
    }
	public virtual GameObject ModelObj{
		get{
			if(m_LogicObj!=null)
				if(m_LogicObj.GetComponent<CSBuildingLogic>()!=null)
					return m_LogicObj.GetComponent<CSBuildingLogic>().ModelObj;
			return null;
		}
	}
    public int logicId
    {
        get {if(gameLogic==null)
				return -1;
			if(gameLogic.GetComponent<CSBuildingLogic>()==null)
				return -1;
			return gameLogic.GetComponent<CSBuildingLogic>().id; }
    }
	public int PeEntityId
	{
		get{if(gameLogic==null)
				return -1;
			if(gameLogic.GetComponent<CSBuildingLogic>()==null)
				return -1; 
			if(gameLogic.GetComponent<CSBuildingLogic>()._peEntity==null)
				return -1; 
			return gameLogic.GetComponent<CSBuildingLogic>()._peEntity.Id;}
	}

	public bool InTest{
		get{
			if(gameLogic==null)
				return false;
			if(gameLogic.GetComponent<CSBuildingLogic>()==null)
				return false; 
			return gameLogic.GetComponent<CSBuildingLogic>().InTest;
		}
	}
    protected GameObject m_Object;
    public virtual GameObject gameObject
    {
        get
        {
            return m_Object;
        }

        set
        {
            m_Object = value;
        }
    }

    public string Name { get { return CSUtils.GetEntityName(m_Type); } }
	public int NameId {get{return CSUtils.GetEntityNameID(m_Type);}}
    public Vector3 Position { get { return m_Data.m_Position; } set { m_Data.m_Position = value; } }
    public int ItemID { get { return m_Data.ItemID; } set { m_Data.ItemID = value; } }
    public Bounds Bound { get { return m_Data.m_Bounds; } set { m_Data.m_Bounds = value; } }

    public Transform[] workTrans;
    public Transform[] resultTrans;
    public Pathea.Operate.PEPatients pePatient;

    // Info
    public CSInfo m_Info;
    public float MaxDurability
    {
        get { return m_Info.m_Durability; }
        set { m_Info.m_Durability = value; }
    }
    public float CurrentDurability
    {
        get { return BaseData.m_Durability; }
        set { BaseData.m_Durability = value; }
    }
	public float DurabilityPercent{
		get{ return CurrentDurability/MaxDurability;}
	}
    // Data
    protected CSObjectData m_Data;

    public CSObjectData BaseData { get { return m_Data; } }

    // Counter script reference
    protected CounterScript m_CSRepair;
    protected CounterScript m_CSDelete;

    public CounterScript CSRepair { get { return m_CSRepair; } }
    public CounterScript CSDelete { get { return m_CSDelete; } }

    public bool isRepairing { get { return (m_CSRepair != null); } }
    public bool isDeleting { get { return (m_CSDelete != null); } }

    // Running mask
    protected bool m_IsRunning = false;
    public bool IsRunning { get { return m_IsRunning; } }
    public delegate void OnDurabilityChange(float dura);
    public OnDurabilityChange onDuraChange;
	public abstract bool IsDoingJob();
	public virtual bool IsDoingJobOn{
		get{return IsRunning;}
	}
    public float DuraPercent
    {
        get { return BaseData.m_Durability / m_Info.m_Durability; }
        set
        {
            BaseData.m_Durability = Mathf.Max(m_Info.m_Durability * value, 0);
            
            //if (BaseData.m_Durability <= 0)
            //{
            //    m_Creator.RemoveEntity(ID);
            //}
            if (onDuraChange != null)
                onDuraChange(BaseData.m_Durability);
        }
    }

    // Soldier Who protect it
//    protected List<CSPersonnel> m_Soldiers = new List<CSPersonnel>();
//
//    public bool AddSoldier(CSPersonnel person)
//    {
//        if (!m_Soldiers.Contains(person))
//        {
//            m_Soldiers.Add(person);
//            person.GuardEntities=person.GetProtectedEntities();
//            return true;
//        }
//
//        return false;
//    }

//    public bool RemoveSoldier(CSPersonnel person)
//    {
//        if(m_Soldiers.Contains(person))
//        {
//            m_Soldiers.Remove(person);
//            person.GuardEntities = person.GetProtectedEntities();
//            return true;
//        }
//        return false;
//    }
//
//    public bool ContainSoldier(CSPersonnel person)
//    {
//        return m_Soldiers.Contains(person);
//    }



    #region EVENT_LESTENER

    public delegate void EventListnerDel(int event_id, CSEntity enti, object arg);
    event EventListnerDel m_EventLisetner;

    public void AddEventListener(EventListnerDel listener)
    {
        m_EventLisetner += listener;
    }

    public void RemoveEventListener(EventListnerDel listener)
    {
        m_EventLisetner -= listener;
    }

    public void ExcuteEvent(int event_type, object arg = null)
    {
        if (m_EventLisetner != null)
            m_EventLisetner(event_type, this, arg);
    }

    public int HealthState { get { return m_HealthState; } }

    private int m_HealthState;
    private float m_HurtTime;
    private float m_RestoreTime;
    private const float cHSRetainTime = 10f;

    public void SetHealthState(int health_state)
    {
        if (health_state == CSConst.ehtHurt)
        {
            m_HealthState |= health_state;
            m_HurtTime = 0;
        }
        else if (health_state == CSConst.ehtRestore)
        {
            m_HealthState |= health_state;
            m_RestoreTime = 0;
        }
    }

    #endregion

    public void AddDeleteGetsItem(int itemId, int count)
    {
        m_Data.m_DeleteGetsItem.Add(itemId, count);
    }
	public void ClearDeleteGetsItem()
	{
		m_Data.m_DeleteGetsItem.Clear();
	}
    #region REPAIR_COUNTER

    protected float m_RepairValue;

    public void StartRepairCounter()
    {
        float percent = (m_Info.m_Durability - m_Data.m_Durability) / m_Info.m_Durability;
        float finalTime = m_Info.m_RepairTime * percent;
        float repairVal = (m_Info.m_Durability - m_Data.m_Durability) / finalTime;

        StartRepairCounter(0F, finalTime, repairVal);
    }

    public void StartRepairCounter(float curTime, float finalTime, float repairVal)
    {
        if (finalTime < 0F)
            return;
        if (m_CSRepair == null)
            m_CSRepair = CSMain.Instance.CreateCounter(Name + " Repair", curTime, finalTime);
        else
        {
            m_CSRepair.Init(curTime, finalTime);
        }

        m_CSRepair.OnTimeTick = OnRepairTick;
        m_RepairValue = repairVal;
    }

    void OnRepairTick(float deltaTime)
    {
        m_Data.m_Durability += m_RepairValue * deltaTime;
        m_Data.m_Durability = Mathf.Min(m_Data.m_Durability, m_Info.m_Durability);
        if (onDuraChange != null)
            onDuraChange(BaseData.m_Durability);
//        if (!GameConfig.IsMultiMode)
//        {
//            // Sync
//            SyncDura();
//        }
    }

    #endregion

    #region DELETE_COUNTER

    public void StartDeleteCounter()
    {
        float percent = m_Data.m_Durability / m_Info.m_Durability;
        float finalTime = m_Info.m_DeleteTime * percent;
        //float finalTime = 20;//test
        StartDeleteCounter(0, finalTime);
    }

    public void StartDeleteCounter(float curTime, float finalTime)
    {
        if (finalTime < 0F)
            return;

        if (m_CSDelete == null)
            m_CSDelete = CSMain.Instance.CreateCounter(Name + " Delete", curTime, finalTime);
        else
        {
            m_CSDelete.Init(curTime, finalTime);
        }

        if (!GameConfig.IsMultiMode)
            m_CSDelete.OnTimeUp = OnDeleteTimesUp;
    }

    private void OnDeleteTimesUp()
    {
        m_Creator.RemoveEntity(ID);
        if (Pathea.PeCreature.Instance.mainPlayer != null)
        {
            foreach (KeyValuePair<int, int> kvp in m_Data.m_DeleteGetsItem)
            {
                Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.PlayerPackageCmpt>().package.Add(kvp.Key, kvp.Value,true);
            }
        }
    }

    // Destory itsself
    public virtual void DestroySelf()
    {
		Debug.Log("Delete " + Name + "successfully");
		m_MgCreator.RemoveLogic(ID);
        DragArticleAgent.Destory(logicId);
		EntityMgr.Instance.Remove(PeEntityId);

        if (m_CSRepair != null)
            GameObject.Destroy(m_CSRepair);
        if (m_CSDelete != null)
            GameObject.Destroy(m_CSDelete);

        if (m_Object != null)
            GameObject.Destroy(m_Object);
        ExcuteEvent(CSConst.eetDestroy);
    }
    #endregion

    // Change the entity state : m_Running flag
    public virtual void ChangeState()
    {
        m_IsRunning = true;

    }

    // Create diferent CSObjectData for Object
    public abstract void CreateData();
   
    // Remove Data 
    public abstract void RemoveData();

    // Object update function
    public virtual void Update()
    {
        // Repair Counter
        if (m_CSRepair != null)
        {
            m_Data.m_CurRepairTime = m_CSRepair.CurCounter;
            m_Data.m_RepairTime = m_CSRepair.FinalCounter;
            m_Data.m_RepairValue = m_RepairValue;
        }
        else
        {
            m_Data.m_CurRepairTime = 0F;
            m_Data.m_RepairTime = -1F;

        }

        // Delet Counter 
        if (m_CSDelete != null)
        {
            m_Data.m_CurDeleteTime = m_CSDelete.CurCounter;
            m_Data.m_DeleteTime = m_CSDelete.FinalCounter;
        }
        else
        {
            m_Data.m_CurDeleteTime = 0F;
            m_Data.m_DeleteTime = -1F;
        }

        // Health state 
        if (m_HurtTime >= cHSRetainTime)
            m_HealthState &= (~CSConst.ehtHurt);
        else
            m_HurtTime += Time.deltaTime;

        if (m_RestoreTime >= cHSRetainTime)
            m_HealthState &= (~CSConst.ehtRestore);
        else
            m_HurtTime += Time.deltaTime;
		if(PeGameMgr.IsSingle||(PeGameMgr.IsMulti&&_Net!=null&&_Net.TeamId==BaseNetwork.MainPlayer.TeamId))
		{
			if(DurabilityPercent<=0.1f){
				if(tipsCounter0%tipsIntervalFrameDurabilityLow==0)
				{
					//CSUtils.ShowTips(ColonyStatusWarning.DURABILITY_LOW,Name);
					CSUtils.ShowTips(ColonyStatusWarning.DURABILITY_LOW,Name);
					tipsCounter0=0;
				}
				tipsCounter0++;
			}else{
				tipsCounter0=0;
			}
		}
    }

    public virtual void AfterTurn90Degree()
    {

    }
    // Damage the entity, Invoke by ColonyRunner.cs. see it !
//    public virtual void OnDamaged(GameObject caster, float damge)
//    {
//        BaseData.m_Durability -= damge;
//
//        if (BaseData.m_Durability < -0.001f)
//            BaseData.m_Durability = -0.001f;
//
//
//        for (int i = 0; i < m_Soldiers.Count; i++)
//        {
//            m_Soldiers[i].ProtectedEntityDamaged(this, caster, damge);
//        }
//
//    }

    #region HELP_FUNC

//    private void SyncDura()
//    {
//        // Sync
//        CSMgCreator mgCreator = m_Creator as CSMgCreator;
//        if (mgCreator != null)
//        {
//            if (mgCreator.SimulatorMgr.ContainSimulator(ID))
//            {
//                CSSimulator csf = mgCreator.SimulatorMgr.GetSimulator(ID);
//                csf.SyncHP(DuraPercent);
//            }
//        }
//    }

    public bool ContainPoint(Vector3 pos)
    {
        return Bound.Contains(pos);
    }

//    public bool Hurt(float damage)
//    {
//        if (m_Creator == null)
//            return false;
//
//        //		BaseData.m_Durability -= damage;
//        //
//        //		if (BaseData.m_Durability < 0.0f)
//        //		{
//        //			m_Creator.RemoveEntity(ID);
//        //		}
//        //
//        //		SyncDura();
//
//        CSMgCreator mgCreator = m_Creator as CSMgCreator;
//        if (mgCreator != null)
//        {
//            mgCreator.SimulatorMgr.ApplyDamage(damage);
//        }
//
//        ExcuteEvent(CSConst.eetHurt);
//
//        return true;
//    }
    #endregion


    // 
    public void OnLifeChanged(float life_percent)
    {
        if (BaseData == null)
        {
            Debug.LogError("The data is not exsit!");
            return;
        }

        //		if (life < 0.001f)
        //		{
        //			Debug.Log(" ==== The [" + Name + "] 's life is less than zero. == ");
        //			m_Creator.RemoveEntity(ID);
        //		}
        //
        //		BaseData.m_Durability = life;
        DuraPercent = life_percent;
    }

	#region virual func
    public virtual void DestroySomeData()
    {

    }

    public virtual void UpdateDataToUI()
    {

    }

	public virtual void StopWorking(int npcid){

	}

	public virtual List<ItemIdCount> GetRequirements(){
		return null;
	}

	
	public virtual List<ItemIdCount> GetDesires(){
		return null;
	}
	
	public virtual bool MeetDemand(ItemIdCount supplyItem){
		return true;
	}
	public virtual bool MeetDemand(int protoId,int count){
		return true;
	}

	public virtual bool MeetDemands(List<ItemIdCount> supplyItems){
		return true;
	}

	public virtual void InitAfterAllDataReady(){

	}
	#endregion
}
