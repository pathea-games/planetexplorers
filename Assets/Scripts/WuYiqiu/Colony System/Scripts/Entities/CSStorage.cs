using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CSRecord;
using ItemAsset;
using CustomData;
using Pathea.Operate;
using ItemAsset.PackageHelper;
using Pathea;


public class CSStorageHistoryAttr
{
	public string m_TimeStr;
	public string m_TimeStrColor = "[00FFFF]";
	public string m_NpcName;
	public string m_NameColorStr = "[FFFF00]";
	public string m_ItemStr;

	public int m_Day;

	public enum EType
	{
		NpcAddSth,
		NpcUseSth
	}
	public EType m_Type;
}

public class CSStorage : CSElectric
{
    public override bool IsDoingJob()
    {
        return IsRunning;
    }

	//protoId
	public const int CYCLE_PROTOID_LIFE = ProtoTypeId.HERBAL_JUICE;
	public const int CYCLE_PROTOID_STAMINA =  ProtoTypeId.NUTS;
	public const int CYCLE_PROTOID_COMFORT = ProtoTypeId.COMFORT_INJECTION_1;
	public const int CYCLE_PROTOID_OTHER_0 = ProtoTypeId.ARROW;
	public const int CYCLE_PROTOID_OTHER_1 = ProtoTypeId.BULLET;
	public const int CYCLE_PROTOID_BATTERY = ProtoTypeId.BATTERY;
	public const int CYCLE_PROTOID_OTHER_3 = ProtoTypeId.HEAT_PACK;

    public const int CYCLE_PROTOID_CHARCOAL = ProtoTypeId.CHARCOAL;
    public const int CYCLE_PROTOID_TORCH = ProtoTypeId.TORCH;
    public const int CYCLE_PROTOID_FLOUR = ProtoTypeId.FLOUR;


    public static int CYCLE_PROTOID_PPCOAL = CSInfoMgr.m_ppCoal.m_WorkedTimeItemID[0];
	public static int CYCLE_PROTOID_PPFUSION = CSInfoMgr.m_ppFusion.m_WorkedTimeItemID[0];
	
	public static int CYCLE_PROTOID_WATER = ProtoTypeId.WATER;
	public static int CYCLE_PROTOID_INSECTICIDE = ProtoTypeId.INSECTICIDE;

	//min
	public const int CYCLE_MIN_PER_NPC_LIFE = 5;
	public const int CYCLE_MIN_PER_NPC_STAMINA = 5;
	public const int CYCLE_MIN_PER_NPC_COMFORT = 2;
	
	public const int CYCLE_MIN_OTHER_0 = 400;
	public const int CYCLE_MIN_OTHER_1 = 140;
	public const int CYCLE_MIN_BATTERY = 2;
	public const float BATTERY_AVAILABLE = 0.2f;
	public const int CYCLE_MIN_OTHER_3 = 25;

	public const int CYCLE_MIN_PPCOAL = 50;
	public const int CYCLE_MIN_PPFUSION = 50;

	public const int CYCLE_MIN_WATER = 100;
	public const int CYCLE_MIN_INSECTICIDE = 100;

	//add
	public const int CYCLE_ADD_MIN_LIFE= 20;
	public const int CYCLE_ADD_MIN_STAMINA = 50;
	public const int CYCLE_ADD_MIN_COMFORT = 10;

	public const int CYCLE_ADD_MIN_OTHER_0 = 400;
	public const int CYCLE_ADD_MIN_OTHER_1 = 140;
	public const int CYCLE_ADD_MIN_BATTERY = 1;
	public const int CYCLE_ADD_MIN_OTHER_3 = 25;

	public const int CYCLE_ADD_MIN_PPCOAL = 50;
	public const int CYCLE_ADD_MIN_PPFUSION = 100;
	
	public const int CYCLE_ADD_MIN_WATER = 88;
	public const int CYCLE_ADD_MIN_INSECTICIDE = 88;

	public const float MAX_ADD_PERCENT_FOR_PER_NPC = 0.4f;

	public const int CYCLE_DESIRE_MIN_COUNT = 10;
	public const int CYCLE_DESIRE_ADD_COUNT = 20;

    //public override GameObject gameLogic
    //{
    //    get { return base.gameLogic; }
    //    set { base.gameLogic = value;
            
    //            PEMachine workmachine = gameLogic.GetComponent<PEMachine>();
    //            if(workmachine!= null)
    //            {
    //                for (int i = 0; i < m_WorkSpaces.Length; i++)
    //                {
    //                    m_WorkSpaces[i].m_workMachine = workmachine;
    //                }
    //            }
    //        }
    //}
    //public override GameObject gameObject
    //{
    //    get { return base.gameObject; }
    //    set 
    //    {
    //        base.gameObject = value;
    //        if (m_Object != null)
    //        {
    //            CSStorageObject csso = m_Object.GetComponent<CSStorageObject>();
    //            if (m_Object.transform.parent != null)
    //            {
    //                GameObject go = m_Object.transform.parent.gameObject;
    //                if (go != null && gameLogic == null)
    //                {
    //                    gameLogic = go;
    //                    PEMachine workmachine = gameLogic.GetComponent<PEMachine>();
    //                    if(workmachine!= null)
    //                    {
    //                        for (int i = 0; i < m_WorkSpaces.Length; i++)
    //                        {
    //                            m_WorkSpaces[i].m_workMachine = workmachine;
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //    }
    //}



	private CSStorageData m_SData;
	public  CSStorageData Data 	
	{ 
		get { 
			if (m_SData == null)
				m_SData = m_Data as CSStorageData;
			return m_SData; 
		} 
	}
	
	//  information
	public  CSStorageInfo m_SInfo;
	public  CSStorageInfo Info
	{
		get
		{
			if (m_SInfo == null)
				m_SInfo = m_Info as CSStorageInfo;
			return m_SInfo;
		}
	}
	
	// 
	public ItemPackage m_Package;


	public CSStorage ()
	{
		m_Type  	= CSConst.etStorage;
		m_Package 	= new ItemPackage();
		// Init Workers
		m_Workers   = new CSPersonnel[2];

		m_WorkSpaces = new PersonnelSpace[2];
		for (int i = 0; i < m_WorkSpaces.Length; i++)
			m_WorkSpaces[i] = new PersonnelSpace(this);

		m_Grade = CSConst.egtLow;
	}

	// History
	const int c_HistorMaxCount = 20;
	public const string c_TimeColorStr = "[00FFFF]";
	public const string c_NameColorStr = "[FFFF00]";
	//private int m_DayFlag = -1;
	
	
	public HistoryStruct[] GetHistory()
	{
		return Data.m_History.ToArray();
	}
	

	public void AddHistory(CSStorageHistoryAttr historyAttr)
	{
		HistoryStruct hs = new HistoryStruct();
		hs.m_Day = historyAttr.m_Day;
		switch (historyAttr.m_Type)
		{
		case CSStorageHistoryAttr.EType.NpcAddSth:
		{
			string str = historyAttr.m_TimeStrColor + historyAttr.m_TimeStr + UIMsgBoxInfo.mStorageHistory_1.GetString();
			hs.m_Value = CSUtils.GetNoFormatString(str,  historyAttr.m_NameColorStr + historyAttr.m_NpcName + "[FFFFFF]", historyAttr.m_ItemStr);
		}
			break;
		case CSStorageHistoryAttr.EType.NpcUseSth:
		{
			string str = historyAttr.m_TimeStrColor + historyAttr.m_TimeStr + UIMsgBoxInfo.mStorageHistory_2.GetString();
			hs.m_Value = CSUtils.GetNoFormatString(str,  historyAttr.m_NameColorStr + historyAttr.m_NpcName + "[FFFFFF]", historyAttr.m_ItemStr);
		}
			break;
		default:
			break;
		}

		Data.m_History.Enqueue(hs);

		ExcuteEvent(CSConst.eetStorage_HistoryEnqueue, hs);

		if (Data.m_History.Count > c_HistorMaxCount)
		{
			HistoryStruct  str = Data.m_History.Dequeue();

			ExcuteEvent(CSConst.eetStorage_HistoryDequeue, str);
			Debug.Log("Storage history [" +str + "] remove.");
		}

	}

	public ItemObject FindSpecifiedItem ()
	{
		List<ItemObject> items = new List<ItemObject>(m_Package.GetSlotList(ItemPackage.ESlotType.Item));
		return items.Find((item0) =>
            {
                if(item0 == null)
                {
                    return false;
                }

                ItemAsset.Consume consume = item0.GetCmpt<ItemAsset.Consume>();
                return null != consume;
            });
	}

	public void Remove(ItemObject item)
	{
		m_Package.RemoveItem(item);

		ExcuteEvent(CSConst.eetStorage_PackageRemoveItem, item);
	}
	public bool AddItemObj(int instanceId){
		bool success = false;
		ItemObject item = ItemMgr.Instance.Get(instanceId);
		if(item!=null){
			if(m_Package.CanAdd(item.protoId,1)){
				m_Package.AddItem(item);
				success = true;
			}
		}
		return success;
	}
	public bool RemoveItemObj(int instanceId)
	{
		bool success = false;
		ItemObject item = ItemMgr.Instance.Get(instanceId);
		if(item!=null){
			success =m_Package.RemoveItem(item);
			ExcuteEvent(CSConst.eetStorage_PackageRemoveItem, item);
		}
		return success;
	}
	#region interface
	public int GetItemCount(int protoId){
		return m_Package.GetCount(protoId);
	}
	public bool CountDownItem(int protoId,int count){
		bool flag = m_Package.Destroy(protoId,count);
		UpdateDataToUI();
		return flag;
	}

	public bool CanAdd(int protoId,int count){
		return m_Package.CanAdd(protoId,count);
	}
	public bool CanAdd(List<ItemIdCount> itemList){
		List<MaterialItem> miList = CSUtils.ItemIdCountToMaterialItem(itemList);
		return m_Package.CanAdd(miList);
	}
	public bool Add(int protoId,int count){
		bool flag =m_Package.Add(protoId,count);
		UpdateDataToUI();
		return flag;
	}
	public bool Add(List<ItemIdCount> itemList){
		List<MaterialItem> miList = CSUtils.ItemIdCountToMaterialItem(itemList);
		bool flag = m_Package.Add(miList);
		UpdateDataToUI();
		return flag;
	}
	public static List<int> GetAutoProtoIdList(){
		List<int> protoList = new List<int> ();
		protoList.Add(CYCLE_PROTOID_LIFE);
		int[] staminaItemList = NpcEatDb.GetEatIDs(EEatType.Hunger);
		protoList.AddRange(staminaItemList);
		protoList.Add(CYCLE_PROTOID_COMFORT);
		protoList.Add(CYCLE_PROTOID_OTHER_0);
		protoList.Add(CYCLE_PROTOID_OTHER_1);
		protoList.Add(CYCLE_PROTOID_BATTERY);
		protoList.Add(CYCLE_PROTOID_OTHER_3);
		protoList.Add(CYCLE_PROTOID_PPCOAL);
		protoList.Add(CYCLE_PROTOID_PPFUSION);
		protoList.Add(CYCLE_PROTOID_WATER);
		protoList.Add(CYCLE_PROTOID_INSECTICIDE);
        //lw:2017.3.1添加火把和碳、面粉
        protoList.Add(CYCLE_PROTOID_CHARCOAL);
        protoList.Add(CYCLE_PROTOID_TORCH);
        protoList.Add(CYCLE_PROTOID_FLOUR);
        return protoList;
	}

	#endregion

	#region CSENTITY_FUNC
	
	public override void DestroySelf ()
	{
		base.DestroySelf ();
	}
	
	public override void CreateData ()
	{
        CSDefaultData ddata = null;
        bool isNew;
        if (GameConfig.IsMultiMode)
        {
            isNew = MultiColonyManager.Instance.AssignData(ID, CSConst.dtStorage, ref ddata, _ColonyObj);
        }
        else
        {
            isNew = m_Creator.m_DataInst.AssignData(ID, CSConst.dtStorage, ref ddata);
        }
		m_Data = ddata as CSStorageData;
		
		if (isNew)
		{
			Data.m_Name = CSUtils.GetEntityName(m_Type);
			Data.m_Durability = Info.m_Durability;
			
		}
		else
		{
			StartRepairCounter(Data.m_CurRepairTime, Data.m_RepairTime, Data.m_RepairValue);
			StartDeleteCounter(Data.m_CurDeleteTime, Data.m_DeleteTime);
			
			foreach (KeyValuePair<int, int> kvp in Data.m_Items)
			{
				ItemObject ioject = ItemMgr.Instance.Get(kvp.Value);
				if (ioject != null)
				{
                    SlotList slotList = m_Package.GetSlotList(ioject.protoId);
					int listCnt = slotList.Count;
					int key = kvp.Key % listCnt;
                    slotList[key] = ioject;
				}
			}
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

		foreach (KeyValuePair<int, int> kvp in Data.m_Items)
		{
			ItemMgr.Instance.DestroyItem(kvp.Key);
		}
	}
	
	public override void Update ()
	{
		base.Update ();
		
		// Update Package Item for Data
		for (int i = 0; i < 4; i++)
		{
			SlotList itemList = m_Package.GetSlotList((ItemPackage.ESlotType)i);
			for (int j = 0; j < itemList.Count; j++)
			{
				int key = i * itemList.Count + j;
				
				if (Data.m_Items.ContainsKey(key))
				{
					if (itemList[j] == null)
						Data.m_Items.Remove(key);
					else
						Data.m_Items[key] = itemList[j].instanceId;
					
				}
				else
				{
					if (itemList[j] != null)
						Data.m_Items.Add(key, itemList[j].instanceId);
				}
			}
		}
	}
	public override void UpdateDataToUI ()
	{
		//--to do:
		if(GameUI.Instance!=null)
//		GameUI.Instance.mCSUI_MainWndCtrl!=null&&
//		GameUI.Instance.mCSUI_MainWndCtrl.StorageUI!=null&&
//		GameUI.Instance.mCSUI_MainWndCtrl.StorageUI.StorageMainUI!=null)
			GameUI.Instance.mCSUI_MainWndCtrl.StorageUI.StorageMainUI.RestItems();
	}
	public override List<ItemIdCount> GetRequirements ()
	{
		int lifeItemCount =0;
		int staminaItemCount = 0;
		int comfortItemCount = 0;
		int[] lifeItemList = NpcEatDb.GetEatIDs(EEatType.Hp);//--to do
		int[] staminaItemList = NpcEatDb.GetEatIDs(EEatType.Hunger);
		int[] comortItemList = NpcEatDb.GetEatIDs(EEatType.Comfort);
		foreach(int protoId in lifeItemList)
			lifeItemCount += CSUtils.GetItemCountFromAllStorage(protoId,Assembly);
		foreach(int protoId in staminaItemList)
			staminaItemCount += CSUtils.GetItemCountFromAllStorage(protoId,Assembly);
		foreach(int protoId in comortItemList)
			comfortItemCount += CSUtils.GetItemCountFromAllStorage(protoId,Assembly);
		List<ItemIdCount> requireList = new List<ItemIdCount> ();
		if(lifeItemCount<CYCLE_MIN_PER_NPC_LIFE*m_MgCreator.GetNpcCount)
			requireList.Add(new ItemIdCount (CYCLE_PROTOID_LIFE, Mathf.Max(CYCLE_ADD_MIN_LIFE,Mathf.CeilToInt(CYCLE_MIN_PER_NPC_LIFE*m_MgCreator.GetNpcCount*MAX_ADD_PERCENT_FOR_PER_NPC))));
        if(staminaItemCount<CYCLE_MIN_PER_NPC_STAMINA*m_MgCreator.GetNpcCount){
			requireList.Add(new ItemIdCount (CYCLE_PROTOID_STAMINA, Mathf.Max(CYCLE_ADD_MIN_STAMINA,Mathf.CeilToInt(CYCLE_MIN_PER_NPC_STAMINA*m_MgCreator.GetNpcCount*MAX_ADD_PERCENT_FOR_PER_NPC))));
		}
        if(comfortItemCount<CYCLE_MIN_PER_NPC_COMFORT*m_MgCreator.GetNpcCount)
            requireList.Add(new ItemIdCount (CYCLE_PROTOID_COMFORT, Mathf.Max(CYCLE_ADD_MIN_COMFORT,Mathf.CeilToInt(CYCLE_MIN_PER_NPC_COMFORT*m_MgCreator.GetNpcCount*MAX_ADD_PERCENT_FOR_PER_NPC))));
		if(CYCLE_MIN_OTHER_0>CSUtils.GetItemCountFromAllStorage(CYCLE_PROTOID_OTHER_0,Assembly))
			requireList.Add(new ItemIdCount(CYCLE_PROTOID_OTHER_0,CYCLE_ADD_MIN_OTHER_0));
		if(CYCLE_MIN_OTHER_1>CSUtils.GetItemCountFromAllStorage(CYCLE_PROTOID_OTHER_1,Assembly))
			requireList.Add(new ItemIdCount(CYCLE_PROTOID_OTHER_1,CYCLE_ADD_MIN_OTHER_1));
		//battery
		List<ItemObject> allBattery = CSUtils.GetItemListInStorage(CYCLE_PROTOID_BATTERY,Assembly);
		if(allBattery.Count<CYCLE_MIN_BATTERY)
			requireList.Add(new ItemIdCount(CYCLE_PROTOID_BATTERY,CYCLE_ADD_MIN_BATTERY));
		else{
			int availableBattery = 0;
			foreach(ItemObject bat in allBattery){
				Energy eg = bat.GetCmpt<Energy>();
				if(eg!=null&&eg.energy.percent > BATTERY_AVAILABLE)
					availableBattery++;
			}
			if(availableBattery<CYCLE_MIN_BATTERY)
		    	requireList.Add(new ItemIdCount(CYCLE_PROTOID_BATTERY,CYCLE_ADD_MIN_BATTERY));
		}
		if(CYCLE_MIN_OTHER_3>CSUtils.GetItemCountFromAllStorage(CYCLE_PROTOID_OTHER_3,Assembly))
			requireList.Add(new ItemIdCount(CYCLE_PROTOID_OTHER_3,CYCLE_ADD_MIN_OTHER_3));
		
		if(CYCLE_MIN_PPCOAL>CSUtils.GetItemCountFromAllStorage(CYCLE_PROTOID_PPCOAL,Assembly))
			requireList.Add(new ItemIdCount(CYCLE_PROTOID_PPCOAL,CYCLE_ADD_MIN_PPCOAL));
		if(CYCLE_MIN_PPFUSION>CSUtils.GetItemCountFromAllStorage(CYCLE_PROTOID_PPFUSION,Assembly))
			requireList.Add(new ItemIdCount(CYCLE_PROTOID_PPFUSION,CYCLE_ADD_MIN_PPFUSION));

		
		if(CYCLE_MIN_WATER>CSUtils.GetItemCountFromAllStorage(CYCLE_PROTOID_WATER,Assembly))
			requireList.Add(new ItemIdCount(CYCLE_PROTOID_WATER,CYCLE_ADD_MIN_WATER));
		if(CYCLE_MIN_INSECTICIDE>CSUtils.GetItemCountFromAllStorage(CYCLE_PROTOID_INSECTICIDE,Assembly))
			requireList.Add(new ItemIdCount(CYCLE_PROTOID_INSECTICIDE,CYCLE_ADD_MIN_INSECTICIDE));
		return requireList;
	}

	public override bool MeetDemand(int protoId,int count){
		if(CSUtils.CanAddToStorage(protoId,count,Assembly)){
			CSUtils.AddToStorage(protoId,count,Assembly);
			return true;
		}
		else{
			CSAutocycleMgr.Instance.ShowTips(ETipType.storage_full);
			return false;
		}
	}

	public override bool MeetDemand(ItemIdCount supplyItem){
		if(CSUtils.CanAddToStorage(supplyItem.protoId,supplyItem.count,Assembly)){
			CSUtils.AddToStorage(supplyItem.protoId,supplyItem.count,Assembly);
			return true;
		}
		else{
			CSAutocycleMgr.Instance.ShowTips(ETipType.storage_full);
			return false;
		}
	}

	public override bool MeetDemands(List<ItemIdCount> supplyItems){
		if(CSUtils.CanAddListToStorage(supplyItems,Assembly)){
			CSUtils.AddItemListToStorage(supplyItems,Assembly);
			return true;
		}
		else{
			CSAutocycleMgr.Instance.ShowTips(ETipType.storage_full);
			return false;
		}
	}

	public override List<ItemIdCount> GetDesires()
	{
		List<ItemIdCount> desireList = new List<ItemIdCount> ();
		int[] staminaItemList = NpcEatDb.GetEatIDs(EEatType.Hunger);

		foreach(int protoId in staminaItemList)
			if(CSUtils.GetItemCountFromAllStorage(protoId,Assembly)<CYCLE_DESIRE_MIN_COUNT)	
				desireList.Add(new ItemIdCount (protoId,CYCLE_DESIRE_ADD_COUNT));

		return desireList;
	}
	#endregion
	
}
