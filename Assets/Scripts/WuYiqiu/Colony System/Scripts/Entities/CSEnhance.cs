using UnityEngine;
using System.Collections;
using CSRecord;
using ItemAsset;
using System.Collections.Generic;
using Pathea.Operate;
public class CSEnhance : CSWorkerMachine
{
    public override bool IsDoingJob()
    {
       return IsRunning && IsEnhancing;
    }
	public override bool IsDoingJobOn
	{
		get{return IsRunning && IsEnhancing&&m_Counter.enabled;}
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

    private CSEnhanceData m_EData;
    public CSEnhanceData Data
    {
        get
        {
            if (m_EData == null)
                m_EData = m_Data as CSEnhanceData;
            return m_EData;
        }
    }

    //  information
    public CSEnhanceInfo m_EInfo;
    public CSEnhanceInfo Info
    {
        get
        {
            if (m_EInfo == null)
                m_EInfo = m_Info as CSEnhanceInfo;
            return m_EInfo;
        }
    }

    // Enhance Item object
    public Strengthen m_Item;

    public delegate void EnhancedDel(Strengthen item);
    public EnhancedDel onEnhancedTimeUp;

    public CSEnhance()
    {
        m_Type = CSConst.etEnhance;

        // Init Workers
        m_Workers = new CSPersonnel[4];

        m_WorkSpaces = new PersonnelSpace[4];
        for (int i = 0; i < m_WorkSpaces.Length; i++)
        {
            m_WorkSpaces[i] = new PersonnelSpace(this);
        }

        m_Grade = CSConst.egtLow;
    }

    // Counter Script
    private CounterScript m_Counter;

    public bool IsEnhancing { get { return m_Counter != null; } }

    public float m_CostsTime;
    public float CostsTime { get { return m_CostsTime; } }

    public override float GetWorkerParam()
    {
        float workParam = 1;
        foreach (CSPersonnel person in m_Workers)
        {
            if (person != null)
            {
                workParam *= 1-person.GetEnhanceSkill;
            }
        }
        return workParam;
    }
    public float CountFinalTime()
    {
        int count = GetWorkingCount();
        //float finalTime = Info.m_BaseTime * (1.15f - 0.15f * count);
        float finalTime = Info.m_BaseTime * Mathf.Pow(0.82f, count);
        //float workParam = GetWorkerParam();
        finalTime = finalTime * GetWorkerParam();

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
            m_Counter = CSMain.Instance.CreateCounter("Enhance", curTime, finalTime);
        }
        else
        {
            m_Counter.Init(curTime, finalTime);
        }
        if (!GameConfig.IsMultiMode)
        {
            m_Counter.OnTimeUp = OnEnhanced;
        }
    }

    public void StopCounter()
    {
        CSMain.Instance.DestoryCounter(m_Counter);
        m_Counter = null;
    }

    public void OnEnhanced()
    {
        if (m_Item == null)
        {
            Debug.LogWarning("The Enhance item is null, so cant be enhanced!");
            return;
        }

        if (onEnhancedTimeUp != null)
            onEnhancedTimeUp(m_Item);

        if (!GameConfig.IsMultiMode)
        {
            //m_Item.Durability = m_Item.GetProperty(ItemProperty.DurabilityMax);
            if (null != m_Item)
            {
                m_Item.LevelUp();
            }
        }
    }

    // Cost Item
    public Dictionary<int, int> GetCostsItems()
    {
        if (m_Item == null)
            return null;

        Dictionary<int, int> costItems = new Dictionary<int, int>();
		MaterialItem[] kvpList= m_Item.GetMaterialItems();
		if(kvpList == null)
			return null;

		foreach (MaterialItem kvp in kvpList)
        {
                costItems[kvp.protoId] = Mathf.CeilToInt(kvp.count);
        }
        return costItems;

        //if (m_Item.instanceId > CreationData.s_ObjectStartID)
        //{
        //    Dictionary<int, int> costItems = new Dictionary<int, int>();
        //    foreach (KeyValuePair<int, int> kvp in m_Item.prototypeData.mRepairRequireList)
        //    {
        //        if (kvp.Key > 30200000 && kvp.Key < 30300000)
        //            costItems[kvp.Key] = Mathf.CeilToInt(kvp.Value);
        //    }
        //    return costItems;
        //}
        //else
        //    return m_Item.prototypeData.mStrengthenRequireList;

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
            isNew = MultiColonyManager.Instance.AssignData(ID, CSConst.dtEnhance, ref ddata, _ColonyObj);
        }
        else
        {
            isNew = m_Creator.m_DataInst.AssignData(ID, CSConst.dtEnhance, ref ddata);
        }
        m_Data = ddata as CSEnhanceData;

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
                m_Item = ItemMgr.Instance.Get(Data.m_ObjID).GetCmpt<Strengthen>();
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
        if (!IsRunning)
        {
            if(m_Counter!=null)
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

            // Update
            if (IsEnhancing)
            {
                //				m_Counter.FinalCounter = Info.m_BaseTime * (1.15f - 0.15f*count);
                m_Counter.SetFinalCounter(CountFinalTime());
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

        if (m_Item != null)
            Data.m_ObjID = m_Item.itemObj.instanceId;
        else
            Data.m_ObjID = -1;

    }

    #endregion

    #region CSCOMMEN_FUNC

    public override bool NeedWorkers()
    {
        return (m_Item != null) && m_IsRunning;
    }
    #endregion
}
