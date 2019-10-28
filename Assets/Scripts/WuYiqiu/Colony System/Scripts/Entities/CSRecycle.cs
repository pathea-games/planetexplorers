using CSRecord;
using ItemAsset;
using ItemAsset.PackageHelper;
using System.Collections.Generic;
using UnityEngine;
using Pathea.Operate;
using Pathea;

public class CSRecycle : CSWorkerMachine
{
    public override bool IsDoingJob()
    {
        return IsRunning&&IsRecycling;
    }
	
	public override bool IsDoingJobOn
	{
		get{return IsRunning&&IsRecycling&&m_Counter.enabled;}
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

    public CSBuildingLogic BuildingLogic
    {
        get { return base.gameLogic.GetComponent<CSBuildingLogic>(); }
    }

    private CSRecycleData  m_RData;
	public  CSRecycleData  Data 	
	{ 
		get { 
			if (m_RData == null)
				m_RData = m_Data as CSRecycleData;
			return m_RData; 
		} 
	}
	
	//  information
	public CSRecycleInfo m_RInfo;
	public CSRecycleInfo Info
	{
		get
		{
			if (m_RInfo == null)
				m_RInfo = m_Info as CSRecycleInfo;
			return m_RInfo;
		}
	}
		
	// Repair Item object
	public Recycle	m_Item;

	// Delegate
	public delegate void NoParmDel();
	public NoParmDel onRecylced;

	public CSRecycle ()
	{
		m_Type = CSConst.etRecyle;
		
		// Init Workers
		m_Workers = new CSPersonnel[4];

		m_WorkSpaces = new PersonnelSpace[4];
		for (int i = 0; i < m_WorkSpaces.Length; i++)
			m_WorkSpaces[i] = new PersonnelSpace(this);

		m_Grade = CSConst.egtLow;
	}
	
	// Counter Script
	private CounterScript  m_Counter;
	
	
	public bool IsRecycling		{ get { return m_Counter != null;} }

	public float m_CostsTime;
	public float CostsTime { get { return m_CostsTime;} }


    public override float GetWorkerParam()
    {
        float workParam = 1;
        foreach (CSPersonnel person in m_Workers)
        {
            if (person != null)
            {
                workParam *= 1 - person.GetRecycleSkill;
            }
        }
        return workParam;
    }

    public float CountFinalTime()
    {
        int count = GetWorkingCount();
        //float finalTime = Info.m_BaseTime * (1.15f - 0.15f * count);
        float finalTime = Info.m_BaseTime * Mathf.Pow(0.82f, count);
        float workParam = GetWorkerParam();
        finalTime = finalTime * workParam;

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
	public void StartCounter ()
	{
        float finalTime = CountFinalTime();
        StartCounter(0, finalTime);
	}

	public void StartCounter (float curTime, float finalTime)
	{
		if (finalTime < 0F)
			return;
		
		if (m_Counter == null)
		{
			m_Counter = CSMain.Instance.CreateCounter("Recycle", curTime, finalTime);
		}
		else
		{
//			m_Counter.CurCounter 	= curTime;
//			m_Counter.FinalCounter	= finalTime;
			m_Counter.Init(curTime, finalTime);
		}

        if (!PeGameMgr.IsMulti)
        {
            m_Counter.OnTimeUp = OnRecycled;
        }
		
	}

	public void StopCounter()
	{
		CSMain.Instance.DestoryCounter(m_Counter);
		m_Counter = null;
	}
	
	private void OnRecycled ()
	{
		Dictionary<int, int> recycleItems = GetRecycleItems();
		if(null==recycleItems)
			return;

        List<ItemIdCount> resourceGot = new List<ItemIdCount>();
        foreach (KeyValuePair<int, int> kvp in recycleItems)
        {
            resourceGot.Add(new ItemIdCount(kvp.Key, kvp.Value));
        }

        if (resourceGot.Count <= 0) return;
        List<MaterialItem> materialList = CSUtils.ItemIdCountToMaterialItem(resourceGot);
        ItemPackage accessor = PeCreature.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>().package._playerPak;

        if (null == accessor) return;

        bool addToPackage = false;

        //lz-2016.12.27 尝试添加到玩家背包
        if (accessor.CanAdd(materialList))
        {
            accessor.Add(materialList);
            GameUI.Instance.mItemPackageCtrl.ResetItem();
            addToPackage = true;
            CSUtils.ShowTips(RecycleConst.INFORM_FINISH_TO_PACKAGE);
        }

        //lz-2016.12.27 尝试添加到基地存储箱
        if (!addToPackage && Assembly != null && Assembly.Storages != null)
        {
            foreach (CSCommon css in Assembly.Storages)
            {
                CSStorage storage = css as CSStorage;
                if (storage.m_Package.CanAdd(materialList))
                {
                    storage.m_Package.Add(materialList);
                    addToPackage = true;
                    CSUtils.ShowTips(RecycleConst.INFORM_FINISH_TO_STORAGE);
                    break;
                }
            }
        }

        //lz-2016.12.27 尝试生成一个小球放物品
        if (!addToPackage)
        {
            System.Random rand = new System.Random();

            List<ItemIdCount> itemIdNum = resourceGot.FindAll(it => it.count > 0);
            if (itemIdNum.Count <= 0)
                return;

            int[] items = CSUtils.ItemIdCountListToIntArray(itemIdNum);

            Vector3 resultPos = Position + new Vector3(0f, 0.72f, 0f);

            if (BuildingLogic != null)
                if (BuildingLogic.m_ResultTrans.Length > 0)
                {
                    Transform trans = BuildingLogic.m_ResultTrans[rand.Next(BuildingLogic.m_ResultTrans.Length)];
                    if (trans != null)
                        resultPos = trans.position;
                }
            while (RandomItemMgr.Instance.ContainsPos(resultPos))
            {
                resultPos += new Vector3(0, 0.01f, 0);
            }
            RandomItemMgr.Instance.GenProcessingItem(resultPos + new Vector3((float)rand.NextDouble() * 0.15f, 0, (float)rand.NextDouble() * 0.15f), items);
            addToPackage = true;
            CSUtils.ShowTips(RecycleConst.INFORM_FINISH_TO_RANDOMITEM);
        }

        if (addToPackage)
        {
            // Delete Item;
            ItemMgr.Instance.DestroyItem(m_Item.itemObj.instanceId);
            m_Item = null;

            // Call back
            if (onRecylced != null)
                onRecylced();
        }
    }
	
	public Dictionary<int, int> GetRecycleItems ()
	{
		if (m_Item == null)
            return null;
        
		Dictionary<int, int> recycleItems = new Dictionary<int, int>();
        //lz-2016.06.16 直接foreach可能会返回null的数据会报空对象
        MaterialItem[] materialItemArray=m_Item.GetRecycleItems();
        if(materialItemArray!=null&&materialItemArray.Length>0)
        {
            for (int i = 0; i < materialItemArray.Length; i++)
            {
                recycleItems[materialItemArray[i].protoId] = materialItemArray[i].count * m_Item.itemObj.stackCount;
            }
        }
        //float factor = m_Item.GetPriceFactor();
		
        //// Creation System
        //if (m_Item.prototypeId >= CreationData.s_ObjectStartID)
        //{
        //    foreach (KeyValuePair<int, int> kvp in m_Item.GetRecycleItems();)
        //        recycleItems[kvp.Key] = Mathf.CeilToInt( kvp.Value * factor );
        //}
        //else
        //{
        //    Pathea.Replicator.Formula ms = Pathea.Replicator.Formula.Mgr.Instance.FindByProductId(m_Item.prototypeId);
        //    //MergeSkill ms = MergeSkill.s_tblMergeSkills.Find( item0 => item0.m_productItemId == m_Item.mItemID);
        //    List<Pathea.Replicator.Formula.Material> m_maxNumberList = ms.materials;
        //    foreach (Pathea.Replicator.Formula.Material msmi in m_maxNumberList)
        //        recycleItems[msmi.itemId] =  Mathf.CeilToInt (msmi.itemCount * factor);
        //}
		
		return recycleItems;
	}

	#region CSCOMMEN_FUNC
//	protected override void OnAddWoker ()
//	{
//		if (IsRecycling)
//		{
//			int count = GetWorkingCount();
//			m_Counter.FinalCounter = Info.m_BaseTime * (1.15f - 0.15f*count);
//		}
//	}
	#endregion
	
	#region CSENTITY_FUNC

	public override void DestroySelf ()
	{
		base.DestroySelf ();

		if (m_Counter != null)
			GameObject.Destroy(m_Counter);
	}

	public override void CreateData ()
	{
        CSDefaultData ddata = null;
        bool isNew;
        if (GameConfig.IsMultiMode)
        {
            isNew = MultiColonyManager.Instance.AssignData(ID, CSConst.dtRecyle, ref ddata, _ColonyObj);
        }
        else
        {
            isNew = m_Creator.m_DataInst.AssignData(ID, CSConst.dtRecyle, ref ddata);
        }
		m_Data = ddata as CSRecycleData;
		
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
                m_Item = ItemMgr.Instance.Get(Data.m_ObjID).GetCmpt<Recycle>();
            }
			StartCounter(Data.m_CurTime, Data.m_Time);
		}

        //for (int i = 0; i < m_WorkSpaces.Length; i++)
        //{
        //    m_WorkSpaces[i].Pos = Position;
        //    m_WorkSpaces[i].m_Rot = Quaternion.identity;
        //}
	}
	
	public override void RemoveData ()
	{
		m_Creator.m_DataInst.RemoveObjectData(ID);

	}
	
	public override void Update ()
	{
		base.Update();
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
//			float count = GetWorkingCount();
			
			// Update
			if (IsRecycling)
            {
               // m_Counter.SetFinalCounter(CountFinalTime());
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
			Data.m_CurTime 	= m_Counter.CurCounter;
			Data.m_Time		= m_Counter.FinalCounter;
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
	
	public override bool NeedWorkers ()
	{
		return  (m_Item != null) && m_IsRunning;
	}

	#endregion
}
