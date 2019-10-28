using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CSRecord;
using ItemAsset;
using Pathea;

public class CSPPCoal : CSPowerPlant
{
	int fuelID =  CSInfoMgr.m_ppCoal.m_WorkedTimeItemID[0];
	public virtual int FuelID{
		get{return fuelID;}
	}
	int fuelMaxCount = CSInfoMgr.m_ppCoal.m_WorkedTimeItemCnt[0];
	public virtual int FuelMaxCount{
		get{return fuelMaxCount;}
	}
	float autoPercent = 0.2f;
	public virtual float AutoPercent{
		get{return autoPercent;}
	}
	int autoCount = 15;
	public virtual int AutoCount{
		get{return autoCount;}
	}

    public override bool IsDoingJob()
    {
        return IsRunning;
    }
    CSPPCoalData m_PPData;
    public CSPPCoalData Data
    {
        get
        {
            if (m_PPData == null)
                m_PPData = m_Data as CSPPCoalData;
            return m_PPData;
        }
    }
	public float RestTime { 
		get{return Mathf.Max(Data.m_WorkedTime - Data.m_CurWorkedTime, 0);}
	}
	public float RestPercent{
		get{
			return RestTime /Data.m_WorkedTime;
		}
	}
    public CSPPCoal()
    {
        m_Type = CSConst.etppCoal;
    }

    // Work Counter Script
    private CounterScript m_CSWorking;


    public void StartWorkingCounter()
    {
        StartWorkingCounter(0, Info.m_WorkedTime);
    }
	public void StartWorkingCounter(float curTime){
		StartWorkingCounter(curTime, Info.m_WorkedTime);
	}
    public void StartWorkingCounter(float curTime, float finalTime)
    {
        if (finalTime < 0F)
            return;


        if (m_CSWorking == null)
            m_CSWorking = CSMain.Instance.CreateCounter(Name + " Working", curTime, finalTime);
        else
        {
            m_CSWorking.Init(curTime, finalTime);
        }

        if (!GameConfig.IsMultiMode)
        {
            m_CSWorking.OnTimeUp = OnWorked;
        }
        ChangeState();
    }
    public void StopCounter()
    {
        CSMain.Instance.DestoryCounter(m_CSWorking);
        m_CSWorking = null;
    }

    public void OnWorked()
    {
        m_IsRunning = false;
        DetachAllElectrics();
    }

    public override bool isWorking()
    {
        return m_CSWorking != null;
    }

    #region CSENTITY_FUNC

    public override void DestroySelf()
    {
        base.DestroySelf();
        if (m_CSWorking != null)
            GameObject.DestroyImmediate(m_CSWorking);
    }

    public override void CreateData()
    {
        CSDefaultData ddata = null;

        bool isNew;
        if (GameConfig.IsMultiMode)
        {
            isNew = MultiColonyManager.Instance.AssignData(ID, CSConst.etppCoal, ref ddata, _ColonyObj);
        }
        else
        {
            isNew = m_Creator.m_DataInst.AssignData(ID, CSConst.etppCoal, ref ddata);
        }

        m_Data = ddata as CSPPCoalData;

        if (isNew)
        {
            Data.m_Name = CSUtils.GetEntityName(m_Type);
            Data.m_Durability = Info.m_Durability;
            StartWorkingCounter();
        }
        else
        {
            StartRepairCounter(Data.m_CurRepairTime, Data.m_RepairTime, Data.m_RepairValue);
            StartDeleteCounter(Data.m_CurDeleteTime, Data.m_DeleteTime);

            StartWorkingCounter(Data.m_CurWorkedTime, Data.m_WorkedTime);

            // Get Charging Items
            foreach (KeyValuePair<int, int> kvp in Data.m_ChargingItems)
            {
				ItemObject itemObj =  ItemMgr.Instance.Get(kvp.Value);
				if(itemObj != null)
                	m_ChargingItems[kvp.Key] = ItemMgr.Instance.Get(kvp.Value).GetCmpt<Energy>();
            }
        }
    }

    public override void RemoveData()
    {
        m_Creator.m_DataInst.RemoveObjectData(ID);

    }
	
	int tipsCounterPPcoal;
	const int tipsIntervalFramePPcoal= 2400;
    public override void Update()
    {
        base.Update();

        // Worked Counter
        if (m_CSWorking != null)
        {
            Data.m_CurWorkedTime = m_CSWorking.CurCounter;
            Data.m_WorkedTime = m_CSWorking.FinalCounter;
        }
        else
        {
            Data.m_CurWorkedTime = 0F;
            Data.m_WorkedTime = -1F;
        }

        // Is working
        if (m_CSWorking != null)
            m_CSWorking.enabled = (Assembly != null);
		if(PeGameMgr.IsSingle||(PeGameMgr.IsMulti&&_Net!=null&&_Net.TeamId==BaseNetwork.MainPlayer.TeamId))
		{
			if(RestPercent<=0.05f){
				if(tipsCounterPPcoal%tipsIntervalFramePPcoal==0)
				{
					switch(m_Type){
					case CSConst.etppFusion:
						CSUtils.ShowTips(ColonyStatusWarning.PPFUSION_RUNNING_LOW,NameId);
						break;
					case CSConst.etppCoal:
						CSUtils.ShowTips(ColonyStatusWarning.PPCOAL_RUNNING_LOW,NameId);
						break;
					}
					//				CSUtils.ShowTips("Test: Power Plant's energy is lower than 5%!");
					tipsCounterPPcoal=0;
				}
				tipsCounterPPcoal++;
			}else{
				tipsCounterPPcoal=0;
			}
		}
    }

	public override List<ItemIdCount> GetRequirements ()
	{
		List<ItemIdCount> requireList= new List<ItemIdCount> ();
		float restTime = Mathf.Max(Data.m_WorkedTime - Data.m_CurWorkedTime, 0);
		float percent = restTime / Info.m_WorkedTime;
		if(percent<AutoPercent)
			requireList.Add(new ItemIdCount (FuelID,AutoCount));
		return requireList;
	}
	public override bool MeetDemand(int protoId,int count){
		if(count<=0)
			return true;
		float percentPer = 1.0f/FuelMaxCount;
		float percentAdd = count*percentPer;
		float timeAdd = percentAdd*Info.m_WorkedTime;
		float curWorkedTime = Mathf.Max(0,Data.m_CurWorkedTime-timeAdd);
		
		StartWorkingCounter(curWorkedTime);
		return true;
	}
	public override bool MeetDemand(ItemIdCount supplyItem){
		return MeetDemand(supplyItem.protoId,supplyItem.count);
	}
    #endregion
}
