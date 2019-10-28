using UnityEngine;
using System.Collections;
using CSRecord;
using ItemAsset;
using System.Collections.Generic;
using Pathea.Operate;

public class CSRepair : CSWorkerMachine
{
    public override bool IsDoingJob()
    {
        return IsRunning&&m_Counter!=null;
    }
	public override bool IsDoingJobOn{
		get{return IsRunning&&m_Counter!=null&&m_Counter.enabled;}
	}
    public override GameObject gameLogic
    {
        get { return base.gameLogic; }
        set
        {
            base.gameLogic = value;
            if (gameLogic != null)
            {
                PEMachine workmachine = gameLogic.GetComponent<PEMachine>();
                if (workmachine != null)
                {
                    for (int i = 0; i < m_WorkSpaces.Length; i++)
                    {
                        m_WorkSpaces[i].WorkMachine = workmachine;
                    }
                }
            }
        }
    }
    //public override GameObject gameObject
    //{
    //    get
    //    {
    //        return base.gameObject;
    //    }

    //    set
    //    {
    //        base.gameObject = value;
    //        if (m_Object != null)
    //        {
    //            PEMachine WorkPoints = m_Object.GetComponentInParent<PEMachine>();
    //            if (WorkPoints != null)
    //            {
    //                this.WorkPoints = WorkPoints;
    //                for (int i = 0; i < m_WorkSpaces.Length; i++)
    //                {
    //                    //m_WorkSpaces[i].Pos = WorkPoints.works[i].transform.position;
    //                    //m_WorkSpaces[i].m_Rot = WorkPoints.works[i].transform.rotation;
    //                    m_WorkSpaces[i].m_workMachine = WorkPoints;
    //                }
    //            }
    //        }
    //    }
    //}

    private CSRepairData m_RData;
    public CSRepairData Data
    {
        get
        {
            if (m_RData == null)
                m_RData = m_Data as CSRepairData;
            return m_RData;
        }
    }

    //  information
    public CSRepairInfo m_RInfo;
    public CSRepairInfo Info
    {
        get
        {
            if (m_RInfo == null)
                m_RInfo = m_Info as CSRepairInfo;
            return m_RInfo;
        }
    }

    // Repair Item object
    public Repair m_Item;

    public delegate void RepairedDel(Repair item);
    public RepairedDel onRepairedTimeUp;

    public CSRepair()
    {
        m_Type = CSConst.etRepair;

        // Init Workers
        m_Workers = new CSPersonnel[4];

        m_WorkSpaces = new PersonnelSpace[4];
        for (int i = 0; i < m_WorkSpaces.Length; i++)
            m_WorkSpaces[i] = new PersonnelSpace(this);

        m_Grade = CSConst.egtLow;
    }

    // Counter Script
    private CounterScript m_Counter = null;

    public bool IsRepairingM { get { return m_Counter != null; } }

    public float m_CostsTime;
    public float CostsTime { get { return m_CostsTime; } }

    public override float GetWorkerParam()
    {
        float workParam = 1;
        foreach (CSPersonnel person in m_Workers)
        {
            if (person != null)
            {
                workParam *= 1 - person.GetRepairSkill;
            }
        }
        return workParam;
    }

    public float CountFinalTime()
    {
        int count = GetWorkingCount();
        //float finalTime = Info.m_BaseTime * (1.15f - 0.15f * count);
		float finalTime = Info.m_BaseTime * Mathf.Pow(0.82f, count)*GetWorkerParam();

        return finalTime;
    }
	public override void RecountCounter(){
		if(m_Counter!=null)
		{
			float curPercent = m_Counter.CurCounter/m_Counter.FinalCounter;
			float finalNew = CountFinalTime();
			float curNew = finalNew*curPercent;
			StartCounter(curNew,finalNew);
		}
	}
    /// <summary>
    /// Log:lz-2016.04.25 这个方法用在营地修理机直接开启计时器
    /// </summary>
    public void CounterToRunning()
    {
        base.m_IsRunning = true;
    }

    public void StartCounter()
    {
        float finalTime = CountFinalTime();
        StartCounter(0, finalTime);
    }


    public void StartCounter(float curTime, float finalTime)
    {
        if (finalTime < 0F)
            return;

        if (m_Counter == null)
        {
            m_Counter = CSMain.Instance.CreateCounter("RepairItem", curTime, finalTime);
        }
        else
        {
            //			m_Counter.CurCounter 	= curTime;
            //			m_Counter.FinalCounter	= finalTime;
            m_Counter.Init(curTime, finalTime);
        }
        if (!GameConfig.IsMultiMode)
        {
            m_Counter.OnTimeUp = OnRepairItemEnd;
        }

        //lz-2016.12.26 启动的时候就直接开始计时
        CounterToRunning();
    }

    public void StopCounter()
    {
        Data.m_CurTime = 0F;
        Data.m_Time = -1F;
        CSMain.Instance.DestoryCounter(m_Counter);
        m_Counter = null;
    }

    public void OnRepairItemEnd()
    {
        if (m_Item == null)
        {
            Debug.LogWarning("The Repair item is null, so cant be repaired!");
            return;
        }

        if (onRepairedTimeUp != null)
            onRepairedTimeUp(m_Item);
        if (!GameConfig.IsMultiMode)
        {
            Repairing();
        }
    }

    public void Repairing()
    {
        if (m_Item == null)
            return;

		m_Item.Do();
        //		if (m_Item.mItemData.mRepairEnable)
        //		{
        //			if (m_Item.mItemID < CreationData.s_ObjectStartID)
        //				m_Item.Durability = m_Item.GetProperty(ItemProperty.DurabilityMax);
        //			else
        //			{
        //				CreationData cdata = CreationMgr.GetCreation(m_Item.mObjectID);
        //				
        //				cdata.m_Hp = cdata.m_Attribute.m_Durability;
        //			}
        //		}
    }

    // Cost Item
    public Dictionary<int, int> GetCostsItems()
    {
        if (m_Item == null )
            return null;

        if (m_Item.GetValue().IsCurrentMax())
            return null;

        ////float factor = 1-item.lifePoint.percent;

        Dictionary<int, int> costItems = new Dictionary<int, int>();
		List<MaterialItem> materialItems = m_Item.GetRequirements();
		foreach (MaterialItem materialItem in materialItems) {
			costItems [materialItem.protoId] = materialItem.count;
		}
        //if (item.instanceId > CreationData.s_ObjectStartID)
        //{
        //    foreach (KeyValuePair<int, int> kvp in item.itemObj.mRepairRequireList)//repairrequireList.
        //    {
        //        if (kvp.Key > 30200000 && kvp.Key < 30300000)
        //            costItems[kvp.Key] = Mathf.CeilToInt(kvp.Value * factor);
        //    }
        //}
        //else
        //{
        //    foreach (KeyValuePair<int, int> kvp in item.prototypeData.mRepairRequireList)
        //        costItems[kvp.Key] = Mathf.CeilToInt(kvp.Value * factor);
        //}

        return costItems;
    }

    public float GetIncreasingDura()
    {
        if (m_Item == null)
        {
            return 0f;
        }
        else
        {
            return m_Item.GetValue().ExpendValue;
        }
            

        //if (item != null){
        //    return item.lifePoint.ExpendValue;
        //}else {
        //    return 0f;
        //}
        //if (m_Item.prototypeData.mRepairLevel == 1)
        //    return m_Item.GetProperty(ItemProperty.DurabilityMax) - m_Item.Durability;
        //else
        //{
        //    CreationData cdata = CreationMgr.GetCreation(m_Item.instanceId);
        //    if (cdata != null)
        //        return cdata.m_Attribute.m_Durability - cdata.m_Hp;
        //}
    }

    public bool IsFull()
    {
        if (m_Item == null)
            return false;

        return m_Item.GetValue().IsCurrentMax();

        //if (m_Item.prototypeData.mRepairLevel != 0)
        //{
        //    if (m_Item.prototypeData.mRepairLevel == 1)
        //        return m_Item.Durability == m_Item.DurabilityMax;
        //    else if (m_Item.prototypeData.mRepairLevel == 2)
        //    {
        //        CreationData cdata = CreationMgr.GetCreation(m_Item.instanceId);
        //        if (cdata != null)
        //            return cdata.m_Hp == cdata.m_Attribute.m_Durability;
        //    }
        //    //			if (m_Item.mItemID < CreationData.s_ObjectStartID)
        //    //			{
        //    //				return m_Item.Durability == m_Item.DurabilityMax;
        //    //			}

        //}

        //return false;
    }

    #region CSENTITY_FUNC

    public override void DestroySelf()
    {
        base.DestroySelf();

        if (m_Counter != null)
            GameObject.Destroy(m_Counter);
    }

    public override void CreateData()
    {
        CSDefaultData ddata = null;
        bool isNew;
        if (GameConfig.IsMultiMode)
        {
            isNew = MultiColonyManager.Instance.AssignData(ID, CSConst.dtRepair, ref ddata, _ColonyObj);
        }
        else
        {
            isNew = m_Creator.m_DataInst.AssignData(ID, CSConst.dtRepair, ref ddata);
        }
        m_Data = ddata as CSRepairData;

        if (isNew)
        {
            Data.m_Name = CSUtils.GetEntityName(m_Type);
            Data.m_Durability = Info.m_Durability;
        }
        else
        {
            StartRepairCounter(Data.m_CurRepairTime, Data.m_RepairTime, Data.m_RepairValue);
            StartDeleteCounter(Data.m_CurDeleteTime, Data.m_DeleteTime);
            if (ItemMgr.Instance.Get(Data.m_ObjID) == null)
            {
                m_Item = null;
            }
            else
            {
                m_Item = ItemMgr.Instance.Get(Data.m_ObjID).GetCmpt<Repair>();
            }
            StartCounter(Data.m_CurTime, Data.m_Time);
        }

        //for (int i = 0; i < m_WorkSpaces.Length; i++)
        //{
        //    m_WorkSpaces[i].Pos = Position;
        //    m_WorkSpaces[i].m_Rot = Quaternion.identity;
        //}
    }

    public override void RemoveData()
    {
        m_Creator.m_DataInst.RemoveObjectData(ID);
    }

    public override void Update()
    {
        base.Update();

		if (m_Item != null)
			Data.m_ObjID = m_Item.itemObj.instanceId;
		else
			Data.m_ObjID = -1;

        if (!IsRunning)
        {
            if (m_Counter != null)
                m_Counter.enabled = false;
            return;
        }
        else
        {
            if (m_Counter != null)
                m_Counter.enabled = true;
        }
        if (m_Item != null)
        {
            //float count = GetWorkingCount();

            // Update
            if (IsRepairingM)
            {
                //Log:lz-2016.04.27 这个地方不要重新设置时间，不然会覆盖掉前面设置的时间
                //m_Counter.SetFinalCounter(CountFinalTime());
                m_CostsTime = m_Counter.FinalCounter - m_Counter.CurCounter;
            }
            else
                m_CostsTime = CountFinalTime();
        }
        else
            m_CostsTime = 0;

        // Enhance Counter
        if (m_Counter != null)
        {
            Data.m_CurTime = m_Counter.CurCounter;
            Data.m_Time = m_Counter.FinalCounter;
        }
        else
        {
            Data.m_CurTime = 0F;
            Data.m_Time = -1F;
        }
    }

    #endregion

    #region CSCOMMEN_FUNC

    public override bool NeedWorkers()
    {
        return (m_Item != null) && m_IsRunning;
    }

    #endregion
}
