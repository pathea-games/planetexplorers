using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CSRecord;
using ItemAsset;
using CustomData;
using Pathea;
using Pathea.Operate;
public class CSFactory : CSWorkerMachine
{
    public override bool IsDoingJob()
    {
        return m_Counter != null&&IsRunning;
    }
	
	public override bool IsDoingJobOn
	{
		get{return m_Counter != null&&IsRunning&&m_Counter.enabled;}
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

    private CSFactoryData m_FData;
    public CSFactoryData Data
    {
        get
        {
            if (m_FData == null)
                m_FData = m_Data as CSFactoryData;
            return m_FData;
        }
    }

    //  information
    public CSFactoryInfo m_FInfo;
    public CSFactoryInfo Info
    {
        get
        {
            if (m_FInfo == null)
                m_FInfo = m_Info as CSFactoryInfo;
            return m_FInfo;
        }
    }

    public class CCompoudItem
    {
        public ItemObject item;
        public float curTime;
        public float Time;
        public int itemID;
        public int itemCnt;

    }

    public const int c_CompoudItemCount = 8;


    public int CompoudItemsCount { get { return Data.m_CompoudItems.Count; } }

    protected CounterScript m_Counter;
    public int m_CurCompoundIndex = 0;

    public CSFactory()
    {
        m_Type = CSConst.etFactory;

        // Init Workers
        m_Workers = new CSPersonnel[4];

        m_WorkSpaces = new PersonnelSpace[4];
        for (int i = 0; i < m_WorkSpaces.Length; i++)
        {
            m_WorkSpaces[i] = new PersonnelSpace(this);
        }

        m_Grade = CSConst.egtLow;

    }

    public bool SetCompoudItem(int itemID, int count, float time)
    {
        if (Data.m_CompoudItems.Count >= c_CompoudItemCount)
            return false;

        CompoudItem data_ci = new CompoudItem();
        data_ci.curTime = 0;
        data_ci.time = time;
		if(Application.isEditor&&InTest){
			data_ci.time=5;
		}
        data_ci.itemID = itemID;
        data_ci.itemCnt = count;
        Data.m_CompoudItems.Add(data_ci);

        return true;

    }
	public bool SetCompoudItemAuto(int itemID, int count, float time)
	{
		if (Data.m_CompoudItems.Count >= c_CompoudItemCount-2)
			return false;
		
		CompoudItem data_ci = new CompoudItem();
		data_ci.curTime = 0;
		data_ci.time = time;
		if(Application.isEditor&&InTest){
			data_ci.time=5;
		}
		data_ci.itemID = itemID;
		data_ci.itemCnt = count;
		Data.m_CompoudItems.Add(data_ci);
		
		return true;
		
	}
	public bool GetTakeAwayCompoundItem(int index, out CompoudItem outCompoudItem){
		outCompoudItem = null;
		List<CompoudItem> ci_list = Data.m_CompoudItems;
		if (index >= ci_list.Count || index < 0)
			return false;
		
		if (ci_list[index].curTime >= ci_list[index].time)
		{
			outCompoudItem = ci_list[index];
			
			return true;
		}
		outCompoudItem = ci_list[index];
		return false;
	}
	public bool TakeAwayCompoudItem(int index){
		List<CompoudItem> ci_list = Data.m_CompoudItems;
		if (index >= ci_list.Count || index < 0)
			return false;
		
		if (ci_list[index].curTime >= ci_list[index].time)
		{
			ci_list.RemoveAt(index);
			m_CurCompoundIndex--;
			
			return true;
		}
		
		return false;
	}
	public bool TakeAwayCompoudItem(int index, out CompoudItem outCompoudItem)
    {
        outCompoudItem = null;
        List<CompoudItem> ci_list = Data.m_CompoudItems;
        if (index >= ci_list.Count || index < 0)
            return false;

        if (ci_list[index].curTime >= ci_list[index].time)
        {
            outCompoudItem = ci_list[index];
            ci_list.RemoveAt(index);
            m_CurCompoundIndex--;

            return true;
        }

        return false;
    }

    // Compoud Processing
    void StartCompoud()
    {
        int compoud_cnt = Data.m_CompoudItems.Count;
        if (compoud_cnt == 0 || m_CurCompoundIndex >= compoud_cnt)
            return;

        if (m_Counter != null)
            return;

        float final_time = Data.m_CompoudItems[m_CurCompoundIndex].time;
        float cur_time = Data.m_CompoudItems[m_CurCompoundIndex].curTime;
        final_time = FixFinalTime(final_time);
        _startCounter(cur_time, final_time);
    }
    public override float GetWorkerParam()
    {
        float workParam = 1;
        foreach (CSPersonnel person in m_Workers)
        {
            if (person != null)
            {
                workParam *= 1 - person.GetCompoundSkill;
            }
        }
        return workParam;
    }
    float FixFinalTime(float origin){
        int count = GetWorkingCount();
		return origin * Mathf.Pow(0.82f, count)* GetWorkerParam()*0.65f;
    }

	public override void RecountCounter(){
		if(m_Counter!=null)
		{
			float curPercent = m_Counter.CurCounter/m_Counter.FinalCounter;
			float finalNew = FixFinalTime(GetCurOriginTime());
			float curNew = finalNew*curPercent;
			_startCounter(curNew,finalNew);
		}
	}
	public float GetCurOriginTime(){
		Replicator.Formula ms = Replicator.Formula.Mgr.Instance.FindByProductId(Data.m_CompoudItems[m_CurCompoundIndex].itemID);
		return Data.m_CompoudItems[m_CurCompoundIndex].itemCnt*ms.timeNeed/ms.m_productItemCount;
	}
    void _startCounter(float curTime, float finalTime)
    {
		if(finalTime<0)
			return;
        if (m_Counter == null)
        {
            m_Counter = CSMain.Instance.CreateCounter("Compoud", curTime, finalTime);
        }
        else
        {
            m_Counter.Init(curTime, finalTime);
        }

        if (!GameConfig.IsMultiMode)
        {
            m_Counter.OnTimeUp = _onCompoudingEnd;
        }
    }


    void _onCompoudingEnd()
    {
        int compoud_cnt = Data.m_CompoudItems.Count;
        if(m_CurCompoundIndex >= compoud_cnt)
            return;
        Data.m_CompoudItems[m_CurCompoundIndex].curTime = Data.m_CompoudItems[m_CurCompoundIndex].time;
        m_CurCompoundIndex += 1;
    }

    void MultiMode_onCompoudingEnd(int index)
    {
        m_CurCompoundIndex = index;
        Data.m_CompoudItems[m_CurCompoundIndex].curTime = Data.m_CompoudItems[m_CurCompoundIndex].time;
        if (m_CurCompoundIndex < Data.m_CompoudItems.Count)
            m_CurCompoundIndex += 1;
    }

    public void MultiModeIsReady(int index)
    {
        if (m_Counter != null)
        {
            CSMain.Instance.DestoryCounter(m_Counter);
            m_Counter = null;
        }
        MultiMode_onCompoudingEnd(index);
    }
	public List<ItemIdCount> GetCompoudingEndItem(){
		List<ItemIdCount> finishList = new List<ItemIdCount> ();
		foreach(CompoudItem ci in Data.m_CompoudItems){
			if(ci.curTime>=ci.time)
			{
				ItemIdCount fItem = finishList.Find(it=>it.protoId==ci.itemID);
				if(fItem!=null)
					fItem.count+=ci.itemCnt;
				else
					finishList.Add(new ItemIdCount(ci.itemID,ci.itemCnt));
			}
		}
		return finishList;
	}
	public List<ItemIdCount> GetCompoudingItem(){
		List<ItemIdCount> doingItem = new List<ItemIdCount> ();
		foreach(CompoudItem ci in Data.m_CompoudItems){
			if(ci.curTime<ci.time)
			{
				ItemIdCount dItem = doingItem.Find(it=>it.protoId==ci.itemID);
				if(dItem!=null)
					dItem.count+=ci.itemCnt;
				else
					doingItem.Add(new ItemIdCount(ci.itemID,ci.itemCnt));
			}
		}
		return doingItem;
	}

	public int GetAllCompoundItemCount(int protoId){
		int result=0;
		foreach(CompoudItem ci in Data.m_CompoudItems){
			if(ci.itemID==protoId)
				result+=ci.itemCnt;
		}
		return result;
	}

	public int GetCompoundEndItemCount(int protoId){
		int result=0;
		foreach(CompoudItem ci in Data.m_CompoudItems){
			if(ci.curTime>=ci.time)
			{
				if(ci.itemID==protoId)
					result+=ci.itemCnt;
			}
		}
		return result;
	}

	public int GetCompoundingItemCount(int protoId){
		int result=0;
		foreach(CompoudItem ci in Data.m_CompoudItems){
			if(ci.curTime<ci.time)
			{
				if(ci.itemID==protoId)
					result+=ci.itemCnt;
			}
		}
		return result;
	}
	public bool CountDownItem(int protoId,int count){
		foreach(CompoudItem ci in Data.m_CompoudItems){
			if(ci.curTime>=ci.time)
			{
				if(ci.itemID==protoId){
					if(ci.itemCnt>count)
					{
						ci.itemCnt-=count;
						count=0;
					}else{
						count -=ci.itemCnt;
						ci.itemCnt=0;
					}
					
					if(count==0)
						break;
				}
			}
		}
		int removeCount = Data.m_CompoudItems.FindAll(it=>it.itemCnt==0).Count;
		Data.m_CompoudItems.RemoveAll(it=>it.itemCnt==0);
		m_CurCompoundIndex-=removeCount;
		if(count==0)
			return true;
		else
			return false;
	}

	public void OnCancelCompound(int index){
		if(index<Data.m_CompoudItems.Count){
			CompoudItem ci = Data.m_CompoudItems[index];
//			if(ci.IsFinished)
//				return;

			if(PeGameMgr.IsSingle){
				//1.storage add materials
				// 0)get materials
				List<ItemIdCount> materialList = new List<ItemIdCount> ();
				Replicator.Formula ms = Replicator.Formula.Mgr.Instance.FindByProductId(ci.itemID);
				foreach(Replicator.Formula.Material mt in ms.materials){
					materialList.Add(new ItemIdCount (mt.itemId,mt.itemCount*ci.itemCnt/ms.m_productItemCount));
				}
				// 1)storage not full
				if(!CSUtils.AddItemListToStorage(materialList,Assembly))
				// 2)storage full, gen object
				{
					System.Random rand = new System.Random ();
					Vector3 pos;
					if(gameLogic!=null)
						pos= Position+ gameLogic.transform.rotation*(new Vector3(0,0,4));
					else
						pos = Position+new Vector3(0,0,6);
					pos=pos+ new Vector3 ((float)rand.NextDouble()*0.1f,0,(float)rand.NextDouble()*0.1f);
					while (RandomItemMgr.Instance.ContainsPos(pos))
					{
						pos += new Vector3(0, 0.01f, 0);
					}
					RandomItemMgr.Instance.GenFactoryCancel(pos,CSUtils.ItemIdCountListToIntArray(materialList));
				}
				if(m_CurCompoundIndex==index){
					if (m_Counter != null)
					{
						CSMain.Instance.DestoryCounter(m_Counter);
						m_Counter = null;
					}
				}
				if(m_CurCompoundIndex>index)
					m_CurCompoundIndex--;
				Data.m_CompoudItems.Remove(ci);
			}else{
				_Net.RPCServer(EPacketType.PT_CL_FCT_GenFactoryCancel,index,ci);
				
				if(m_CurCompoundIndex==index){
					if (m_Counter != null)
					{
						CSMain.Instance.DestoryCounter(m_Counter);
						m_Counter = null;
					}
				}
				if(m_CurCompoundIndex>index)
					m_CurCompoundIndex--;
				Data.m_CompoudItems.Remove(ci);
			}

		}
	}
    #region CSENTITY_FUNC
    public override void DestroySelf()
    {
        base.DestroySelf();
    }

    public override void CreateData()
    {
        CSDefaultData ddata = null;
        bool isNew;
        if (GameConfig.IsMultiMode)
        {
            isNew = MultiColonyManager.Instance.AssignData(ID, CSConst.dtFactory, ref ddata, _ColonyObj);
        }
        else
        {
            isNew = m_Creator.m_DataInst.AssignData(ID, CSConst.dtFactory, ref ddata);
        }
        m_Data = ddata as CSFactoryData;

        if (isNew)
        {
            Data.m_Name = CSUtils.GetEntityName(m_Type);
            Data.m_Durability = Info.m_Durability;
        }
        else
        {
            StartRepairCounter(Data.m_CurRepairTime, Data.m_RepairTime, Data.m_RepairValue);
            StartDeleteCounter(Data.m_CurDeleteTime, Data.m_DeleteTime);

            // Clear old record
            for (int i = 0; i < Data.m_CompoudItems.Count; )
            {
                ItemProto item_data = ItemProto.GetItemData(Data.m_CompoudItems[i].itemID);
                if (item_data == null)
                    Data.m_CompoudItems.RemoveAt(i);
                else
                    i++;
            }
        }
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
            if (m_Counter != null)
                m_Counter.enabled = false;
            return;
        }
        else
        {
            if (m_Counter != null)
                m_Counter.enabled = true;
        }

        m_CurCompoundIndex = 0;
        if (m_CurCompoundIndex >= Data.m_CompoudItems.Count)
            return;

        while (Data.m_CompoudItems[m_CurCompoundIndex].curTime >= Data.m_CompoudItems[m_CurCompoundIndex].time)
        {
            m_CurCompoundIndex++;
            if (m_CurCompoundIndex >= Data.m_CompoudItems.Count)
            {
                m_CurCompoundIndex = Data.m_CompoudItems.Count;
                break;
            }
        }
		
		// Compoud Processing
		StartCompoud();

        if (m_Counter != null && Data.m_CompoudItems.Count > m_CurCompoundIndex && m_CurCompoundIndex >= 0)
        {
            Data.m_CompoudItems[m_CurCompoundIndex].curTime = m_Counter.CurCounter;
            Data.m_CompoudItems[m_CurCompoundIndex].time = m_Counter.FinalCounter;
		}
    }
    #endregion

    #region MultiMode
    public void MultiModeTakeAwayCompoudItem(int index)
    {
        List<CompoudItem> ci_list = Data.m_CompoudItems;
        if (index >= ci_list.Count || index < 0)
            return;
        ci_list.RemoveAt(index);
        if (m_CurCompoundIndex > 0)
        {
            m_CurCompoundIndex--;
        }
    }
	public void SetAllItems(CompoudItem[] itemList){
		Data.m_CompoudItems.Clear();
		for (int i = 0; i < itemList.Length; i++)
		{
			Data.m_CompoudItems.Add(itemList[i]);
		}

		m_CurCompoundIndex = 0;
		if (m_CurCompoundIndex >= Data.m_CompoudItems.Count)
			return;
		
		while (Data.m_CompoudItems[m_CurCompoundIndex].curTime >= Data.m_CompoudItems[m_CurCompoundIndex].time)
		{
			m_CurCompoundIndex++;
			if (m_CurCompoundIndex >= Data.m_CompoudItems.Count)
			{
				m_CurCompoundIndex = Data.m_CompoudItems.Count;
				break;
			}
		}
		if(m_CurCompoundIndex >= Data.m_CompoudItems.Count){
			if (m_Counter != null)
				CSMain.Instance.DestoryCounter(m_Counter);
		}
		else{
			float final_time = Data.m_CompoudItems[m_CurCompoundIndex].time;
			float cur_time = Data.m_CompoudItems[m_CurCompoundIndex].curTime;
			_startCounter(cur_time,final_time);
		}
	}
    #endregion

	#region Interface
	public void CreateNewTaskWithItems(List<ItemIdCount> allItemsList){
		if(Assembly==null||Assembly.Storages==null)
			return;
		
		List<int> replicatingItems = new List<int> ();
		List<ItemIdCount> allMaterialItemsInNeed = new List<ItemIdCount> ();
		foreach(ItemIdCount iic in allItemsList){
			//1.get the formula/find the best formula for the item,
			List<ItemIdCount> materialList;
			int productItemCount;
			ReplicateItem(iic,replicatingItems,out materialList,out productItemCount);
			//--to do:product not enough, but material can be replicate
			if(iic.count>0){
				//level01
				foreach(ItemIdCount mIic in materialList){
					mIic.count=mIic.count * Mathf.CeilToInt(iic.count*1.0f/ productItemCount);//needCount
					CSUtils.AddItemIdCount(allMaterialItemsInNeed,mIic.protoId,mIic.count);
				}
			}
		}

		ReplicateMaterialRecursion(0,2,replicatingItems,allMaterialItemsInNeed);

		if(replicatingItems.Count>0)
			CSAutocycleMgr.Instance.ShowReplicatorFor(replicatingItems);
		//3.update allItemsLIst
		allItemsList.RemoveAll(it=>it.count==0);
	}

	public void ReplicateItem(ItemIdCount iic,List<int> replicatingItems,out List<ItemIdCount> materialList,out int productItemCount){
		Replicator.KnownFormula[] msList = UIGraphControl.GetReplicator().GetKnowFormulasByProductItemId(iic.protoId);
		materialList = new List<ItemIdCount> ();
		productItemCount = 0;
		if(msList==null||msList.Length==0)
			return;

		//--to do: temp,only use formula 01?
		Replicator.Formula ms = Replicator.Formula.Mgr.Instance.Find(msList[0].id);
		foreach(Replicator.Formula.Material mt in ms.materials){
			materialList.Add(new ItemIdCount (mt.itemId,mt.itemCount));
		}
		productItemCount = ms.m_productItemCount;
		int productCount =Mathf.CeilToInt(iic.count*1.0f/ ms.m_productItemCount);
		
		//2.replicate it and count down material in storage
		int countCanGet = CSUtils.GetMaterialListCount(materialList,Assembly);
		if(countCanGet==0)
			return;
		if(countCanGet>=productCount)
		{
			if(SetCompoudItemAuto(iic.protoId,productCount*ms.m_productItemCount,ms.timeNeed*productCount)){
				iic.count=0;
				foreach(ItemIdCount countDownItem in materialList){
					CSUtils.CountDownItemFromFactoryAndAllStorage(countDownItem.protoId,countDownItem.count*productCount,Assembly);
				}
				replicatingItems.Add(iic.protoId);
			}
		}else{
			if(SetCompoudItemAuto(iic.protoId,countCanGet*ms.m_productItemCount,ms.timeNeed*countCanGet)){
				iic.count-=countCanGet*ms.m_productItemCount;
				foreach(ItemIdCount countDownItem in materialList){
					CSUtils.CountDownItemFromFactoryAndAllStorage(countDownItem.protoId,countDownItem.count*countCanGet,Assembly);
				}
				replicatingItems.Add(iic.protoId);
			}
		}
	}

	public void ReplicateMaterialRecursion(int curDepth,int maxDepth,List<int> replicatingItems,List<ItemIdCount> materialList ){
		if(curDepth>=maxDepth)
			return;
		else{
			List<ItemIdCount> nextMaterialList = new List<ItemIdCount> ();
			foreach(ItemIdCount mIic in materialList){
				int ownMaterialCount = CSUtils.GetItemCountFromAllStorage(mIic.protoId, Assembly)+GetAllCompoundItemCount(mIic.protoId);
				if(ownMaterialCount>0){
					if(mIic.count<=ownMaterialCount)
						continue;
					else
						mIic.count-=ownMaterialCount;
				}
				List<ItemIdCount> mMaterialList;
				int mProductCount;
				ReplicateItem(mIic,replicatingItems,out mMaterialList,out mProductCount);
				if(mIic.count>0){
					//level01
					foreach(ItemIdCount mMIic in mMaterialList){
						mMIic.count=mMIic.count * Mathf.CeilToInt(mIic.count*1.0f/ mProductCount);//needCount
						CSUtils.AddItemIdCount(nextMaterialList,mMIic.protoId,mMIic.count);
					}
				}
			}
			if(nextMaterialList.Count==0)
				return;
			curDepth++;
			ReplicateMaterialRecursion(curDepth,maxDepth,replicatingItems,nextMaterialList);
		}
	}
	#endregion
}
