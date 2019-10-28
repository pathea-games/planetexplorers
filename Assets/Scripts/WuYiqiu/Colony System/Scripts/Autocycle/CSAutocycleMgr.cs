using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using System.Text.RegularExpressions;
using System;

public class CSAutocycleMgr:MonoBehaviour
{
	static CSAutocycleMgr mInstance;
	public static CSAutocycleMgr Instance{
		get{
			return mInstance;
		}
	}

	void Init(){
		core = CSMain.GetAssemblyEntity();
	}

	void Awake(){
		mInstance = this;
	}

	void Start(){
	}

	double lastTime=-1;
	int counter = 1;
	//test
	double medicineSupplyTime = 7200;
	void Update(){
		if(!PeGameMgr.IsSingle)
			return;
		if(core==null)
			Init ();
		if(core==null)
			return;
		counter++;
		if(counter%450==0)
		{
			counter=0;
			//--to do
			//1.collect requirements
			if(core.Storages==null){
				return;
			}
				
			CSFactory factory = core.Factory;

			List<CSCommon> storages = core.Storages;
			ClearRequirements();
			List<CSCommon> allCommon = core.GetBelongCommons();
			foreach(CSCommon cse in allCommon)
			{
				if((cse as CSPPCoal!=null) || (cse as CSFarm !=null)/*|| (cse as CSMedicalTreat !=null)*/){
					List<ItemIdCount> requirementsList = cse.GetRequirements();
					if(requirementsList!=null&&requirementsList.Count>0){
						if(cse as CSPPCoal!=null)
							ppcoalRequirements.Add(cse,requirementsList);
	//					if(cse as CSMedicalTreat!=null)
	//						treatRequirements.Add(cse,requirementsList);
						if(cse as CSFarm !=null)
							farmRequirements.Add(cse,requirementsList);
					}
				}
			}
			bool transferedFactoryItem=false;
			//--to do
			if(storages!=null){
				//0.transfer factoryEndItem
				if(factory!=null){
					List<ItemIdCount> replicateEndItem = factory.GetCompoudingEndItem();
					if(replicateEndItem.Count>0){
						List<int> protoList = CSStorage.GetAutoProtoIdList();
						foreach(ItemIdCount rIic in replicateEndItem)
						{
							if(protoList.Contains(rIic.protoId))
								if(CSUtils.AddToStorage(rIic.protoId,rIic.count,core)){
									factory.CountDownItem(rIic.protoId,rIic.count);
									transferedFactoryItem = true;
								}
						}
					}
				}

				List<ItemIdCount> requirementsList = storages[0].GetRequirements();
				if(requirementsList!=null&&requirementsList.Count>0){
					storageRequirements.Add(storages[0],requirementsList);
				}
			}

			//2.check doing requirements** one by one? && do requirements
			//1)check storage&factory Get supply
			List<ItemIdCount> itemsNeedToGet = new List<ItemIdCount> ();
			GetItemFromStorageAndFactory(ppcoalRequirements,storages,factory,ref itemsNeedToGet);
			GetItemFromStorageAndFactory(farmRequirements,storages,factory,ref itemsNeedToGet);
			GetItemFromStorageAndFactory(storageRequirements,storages,factory,ref itemsNeedToGet);

			if(itemsNeedToGet.Count>0){
				//3.analysis to processing or replicate
				//1).check is replicating
				if(factory!=null)
				{
					List<ItemIdCount> itemsInReplicating= new List<ItemIdCount>();
					List<ItemIdCount> compoundingList = factory.GetCompoudingItem();
					foreach(ItemIdCount needItem in itemsNeedToGet){
						ItemIdCount cItem = compoundingList.Find(it=>it.protoId==needItem.protoId);
						if(cItem!=null){
							if(cItem.count>=needItem.count){
								itemsInReplicating.Add(new ItemIdCount(cItem.protoId,needItem.count));
							}else{
								itemsInReplicating.Add(new ItemIdCount (cItem.protoId,cItem.count));
							}
						}
					}
					if(itemsInReplicating.Count>0)
					{
						foreach(ItemIdCount ic in itemsInReplicating){
							ItemIdCount decreaseItem = itemsNeedToGet.Find(it=>it.protoId==ic.protoId);
							if(decreaseItem.count>ic.count)
								decreaseItem.count-=ic.count;
							else
								itemsNeedToGet.Remove(decreaseItem);
						}
					}

				//2).put material into storage
					GetItemMaterialFromFactory(factory,itemsNeedToGet,ref transferedFactoryItem);
					if(transferedFactoryItem)
						ShowTips(ETipType.factory_to_storage);

				//3).count what need Processing/ replicate some
				//1-check can be replicated,if ok, replicate
					factory.CreateNewTaskWithItems(itemsNeedToGet);
				}
				//2-check the left things to resolve to resource
				//3-in 2,check resource already have, remove it
				List<ItemIdCount> resourceItems = ResolveItemsToProcess(itemsNeedToGet,core,factory);
//				foreach(ItemIdCount iic in resourceItems){
//					int gotCount =0;
//					foreach(CSCommon sc in storages){
//						CSStorage cst = sc as CSStorage;
//						int itemCount = cst.GetItemCount(iic.protoId);
//						if(itemCount>0)
//						{
//							if(itemCount>=iic.count-gotCount)
//							{
//								gotCount = iic.count;
//								break;
//							}else{
//								gotCount += itemCount;
//							}
//						}
//					}
//					iic.count-=gotCount;
//				}

				resourceItems.RemoveAll(it=>it.count<=0);
				if(resourceItems.Count>0){
					//3).check is Processing
					CSProcessing process = core.ProcessingFacility; 
					if(process!=null){
						List<ItemIdCount> processingList = process.GetItemsInProcessing();
						foreach(ItemIdCount iic in processingList){
							CSUtils.RemoveItemIdCount(resourceItems,iic.protoId,iic.count);
						}
					}
					if(resourceItems.Count>0){
						//4).assign processing task
						if(process!=null){
							process.CreateNewTaskWithItems(resourceItems);
							
						}
					}
				}
			}



			//meet desire
			ClearDesires();
			if(storages!=null){
				List<ItemIdCount> desireList = storages[0].GetDesires();
				if(desireList!=null&&desireList.Count>0){
					storageDesires.Add(storages[0],desireList);
					List<ItemIdCount> itemsDesireToGet = new List<ItemIdCount> ();
					GetItemFromStorageAndFactory(storageDesires,storages,factory,ref itemsDesireToGet);
					if(itemsDesireToGet.Count>0&&factory!=null){
						if(factory!=null)
						{
							List<ItemIdCount> itemsInReplicating= new List<ItemIdCount>();
							List<ItemIdCount> compoundingList = factory.GetCompoudingItem();
							foreach(ItemIdCount needItem in itemsDesireToGet){
								ItemIdCount cItem = compoundingList.Find(it=>it.protoId==needItem.protoId);
								if(cItem!=null){
									if(cItem.count>=needItem.count){
										itemsInReplicating.Add(new ItemIdCount(cItem.protoId,needItem.count));
									}else{
										itemsInReplicating.Add(new ItemIdCount (cItem.protoId,cItem.count));
									}
								}
							}
							if(itemsInReplicating.Count>0)
							{
								foreach(ItemIdCount ic in itemsInReplicating){
									ItemIdCount decreaseItem = itemsDesireToGet.Find(it=>it.protoId==ic.protoId);
									if(decreaseItem.count>ic.count)
										decreaseItem.count-=ic.count;
									else
										itemsDesireToGet.Remove(decreaseItem);
								}
							}
						}
						factory.CreateNewTaskWithItems(itemsDesireToGet);
					}
				}
			}


			//medical system
			if(core.MedicalCheck!=null&&core.MedicalTreat!=null&&core.MedicalTent!=null
			   &&core.MedicalCheck.IsRunning&&core.MedicalTreat.IsRunning&&core.MedicalTent.IsRunning
			   &&(core.MedicalCheck.WorkerCount+core.MedicalTreat.WorkerCount+core.MedicalTent.WorkerCount>0)){
				if(lastTime<0){
					lastTime=GameTime.PlayTime.Second;
				}else{
					core.MedicineResearchState+=GameTime.PlayTime.Second-lastTime;
					lastTime=GameTime.PlayTime.Second;
					if(storages!=null&&core.MedicineResearchState>=medicineSupplyTime){
						Debug.Log("supplyMedicineTime");
						core.MedicineResearchTimes++;
						List<ItemIdCount> supportMedicine = new List<ItemIdCount> ();
						//try supply
						foreach(MedicineSupply ms in CSMedicineSupport.AllMedicine){
							if(core.MedicineResearchTimes-ms.rounds*Mathf.FloorToInt(core.MedicineResearchTimes/ms.rounds)
							   <1)
								if(CSUtils.GetItemCountFromAllStorage(ms.protoId,core)<ItemAsset.ItemProto.GetItemData(ms.protoId).maxStackNum)
									supportMedicine.Add(new ItemIdCount (ms.protoId,ms.count));
						}
						if(supportMedicine.Count>0){
							if(CSUtils.AddItemListToStorage(supportMedicine,core)){
								ShowTips(ETipType.medicine_supply);
							}else{
								ShowTips(ETipType.storage_full);
							}
						}

						core.MedicineResearchState=0;
						if(core.MedicineResearchTimes==Int32.MaxValue)
							core.MedicineResearchTimes=0;
					}
				}
			}else{
				lastTime=GameTime.PlayTime.Second;
			}
		}
	}




	public void GetItemFromStorageAndFactory(Dictionary<CSCommon,List<ItemIdCount>> requirementsMachine,List<CSCommon> storages,CSFactory factory,ref List<ItemIdCount> itemsNeedToGet){
		foreach(KeyValuePair<CSCommon,List<ItemIdCount>> kvp in requirementsMachine){
			foreach(ItemIdCount iic in kvp.Value)
			{
				//1.storage,storageRequire
				int gotCount = 0;
				int getCountFromStorage = 0;
				if(kvp.Key as CSStorage==null){
					foreach(CSCommon sc in storages){
						CSStorage cst = sc as CSStorage;
						int itemCount = cst.GetItemCount(iic.protoId);
						if(itemCount>0)
						{
							if(itemCount>=iic.count-gotCount)
							{
								gotCount = iic.count;
								getCountFromStorage=iic.count;
								break;
							}else{
								gotCount += itemCount;
								getCountFromStorage +=itemCount;
							}
						}
					}
					if(gotCount==iic.count){
						if(kvp.Key.MeetDemand(iic.protoId,gotCount)){
							CSUtils.CountDownItemFromAllStorage(iic.protoId,gotCount,core);
						}
						continue;
					}
				}
				//2.factory
				int leftCount = iic.count-gotCount;
				int getCountFromFac = 0;
				if(factory!=null){
					int factoryItemCount = factory.GetCompoundEndItemCount(iic.protoId);
					if(factoryItemCount>0)
					{
						int countDownCount = 0;
						if(factoryItemCount>=leftCount)
						{
							countDownCount = leftCount;
						}else{
							countDownCount = factoryItemCount;
						}
						gotCount+=countDownCount;
						getCountFromFac+=countDownCount;
					}
				}
				
				if(gotCount==iic.count){
					if(kvp.Key.MeetDemand(iic.protoId,gotCount)){
						if(getCountFromStorage>0)
							CSUtils.CountDownItemFromAllStorage(iic.protoId,getCountFromStorage,core);
						if(getCountFromFac>0)
							factory.CountDownItem(iic.protoId,getCountFromFac);
					}
					continue;
				}else{
					if(gotCount>0)
					{
						if(kvp.Key.MeetDemand(iic.protoId,gotCount)){
							if(getCountFromStorage>0)
								CSUtils.CountDownItemFromAllStorage(iic.protoId,getCountFromStorage,core);
							if(getCountFromFac>0)
								factory.CountDownItem(iic.protoId,getCountFromFac);
						}
					}
				}
				
				leftCount = iic.count-gotCount;

				//3.add to needToGet
				ItemIdCount addNeed = itemsNeedToGet.Find(it=>it.protoId==iic.protoId);
				if(addNeed!=null)
					addNeed.count+=leftCount;
				else
					itemsNeedToGet.Add(new ItemIdCount(iic.protoId,leftCount));
			}
		}
	}

	public void GetItemMaterialFromFactory(CSFactory factory,List<ItemIdCount> itemsNeedToGet,ref bool transferedItem){
		foreach(ItemIdCount iic in itemsNeedToGet){
			Replicator.KnownFormula[] msList = UIGraphControl.GetReplicator().GetKnowFormulasByProductItemId(iic.protoId);
			if(msList==null||msList.Length==0)
				continue;

			Replicator.Formula ms = Replicator.Formula.Mgr.Instance.Find(msList[0].id);
			foreach(Replicator.Formula.Material mt in ms.materials){
				int itemCount = factory.GetCompoundEndItemCount(mt.itemId);
				if(itemCount>0)
				{
					if(CSUtils.AddToStorage(mt.itemId,itemCount,core)){
						factory.CountDownItem(mt.itemId,itemCount);
						transferedItem = true;
					}
				}
			}
		}
	}


	public CSAssembly core;

	public Dictionary<CSCommon,List<ItemIdCount>>  ppcoalRequirements = new Dictionary<CSCommon,List<ItemIdCount>>();
//	public Dictionary<CSCommon,List<ItemIdCount>>  treatRequirements = new Dictionary<CSCommon,List<ItemIdCount>>();
	public Dictionary<CSCommon,List<ItemIdCount>>  farmRequirements = new Dictionary<CSCommon,List<ItemIdCount>>();
	public Dictionary<CSCommon,List<ItemIdCount>>  storageRequirements = new Dictionary<CSCommon,List<ItemIdCount>>();
	public Dictionary<CSCommon,List<ItemIdCount>>  storageDesires = new Dictionary<CSCommon,List<ItemIdCount>>();

	public void ClearRequirements(){
		ppcoalRequirements.Clear();
//		treatRequirements.Clear ();
		farmRequirements.Clear();
		storageRequirements.Clear();
	}
	public void ClearDesires(){
		storageDesires.Clear();
	}
	public void Check(){
	}
	public void SearchRequirement(){
	}

	public void AddRequirement(CSEntity entity){}

	public void FinishRequirement(){}
	public bool HasPPCoalRequirements{
		get{return ppcoalRequirements.Keys.Count>0;}
	}
	public bool HasFarmRequirements{
		get{return farmRequirements.Keys.Count>0;}
	}
	public bool HasStorageRequirements{
		get{return storageRequirements.Keys.Count>0;}
	}

	public void ShowReplicatorFor(List<int> productIdList){
		ShowTips(ETipType.replicate_for,ColonyNameID.STORAGE);
//		foreach(List<ItemIdCount> farmItems in farmRequirements.Values)
//		{
//			if(farmItems.Find(it=>productIdList.Contains(it.protoId))!=null)
//			{
//				ShowTips(ETipType.replicate_for,ColonyNameID.FARM);
//				return;
//			}
//		}

//		foreach(List<ItemIdCount> storageItems in storageRequirements.Values)
//		{
//			if(storageItems.Find(it=>productIdList.Contains(it.protoId))!=null)
//		   	{
//				ShowTips(ETipType.replicate_for,ColonyNameID.STORAGE);
//				return;
//			}
//		}
	}

	public void ShowProcessFor(List<ItemIdCount> processItems){
		ShowTips(ETipType.process_for_storage);
//		if(ppcoalRequirements.Keys.Count>0)
//		{
//			if(processItems.Find(it=>it.protoId==CSInfoMgr.m_ppCoal.m_WorkedTimeItemID[0])!=null){
//				ShowTips(ETipType.process_for_resource,ColonyNameID.PPCOAL);
//				return;
//			}
//		}
//
//		if(storageRequirements.Keys.Count>0){
//			List<int> protoIdList = CSStorage.GetAutoProtoIdList();
//			List<int> resourceList = resolveProtoIdToProcess(protoIdList);
//			if(processItems.Find(it=>!resourceList.Contains(it.protoId))==null)
//				ShowTips(ETipType.process_for_storage);
//			else
//				ShowTips(ETipType.process_for_resource,ColonyNameID.FARM);
//			return;
//		}
//		
//		ShowTips(ETipType.process_for_resource,ColonyNameID.FARM);
	}
	
	
	public void ShowTips(ETipType type,int replaceStrId=-1){
		string str="?";
		string replaceStr = PELocalization.GetString(replaceStrId);
		switch(type){
		case ETipType.process_for_resource:
			str= CSUtils.GetNoFormatString(PELocalization.GetString(AutoCycleTips.PROCESS_FOR_RESOURCE),replaceStr);
			break;
		case ETipType.process_for_storage:
			str= CSUtils.GetNoFormatString(PELocalization.GetString(AutoCycleTips.PROCESS_FOR_STORAGE),replaceStr);
			break;
		case ETipType.replicate_for:
			str= CSUtils.GetNoFormatString(PELocalization.GetString(AutoCycleTips.REPLICATE_FOR),replaceStr);
			break;
		case ETipType.storage_full:
			str= CSUtils.GetNoFormatString(PELocalization.GetString(AutoCycleTips.STORAGE_FULL),replaceStr);
			break;
		case ETipType.factory_to_storage:
			str= CSUtils.GetNoFormatString(PELocalization.GetString(AutoCycleTips.FACTORY_TO_STORAGE),replaceStr);
			break;
		case ETipType.medicine_supply:
			str= CSUtils.GetNoFormatString(PELocalization.GetString(AutoCycleTips.MEDICINE_SUPPLY),replaceStr);
			break;
		}
		new PeTipMsg(str,PeTipMsg.EMsgLevel.Norm,PeTipMsg.EMsgType.Colony);
		string[] infoInColony=Regex.Split(str,"\\[-\\] ",RegexOptions.IgnoreCase);
		if(infoInColony.Length<2)
			return;
		CSUI_MainWndCtrl.ShowStatusBar(infoInColony[1],10);
	}

	public static List<ItemIdCount> ResolveItemsToProcess(List<ItemIdCount> itemsNeedToGet,CSAssembly core=null,CSFactory factory=null){

		List<ItemIdCount> resourceItems = new List<ItemIdCount> ();
		List<ItemIdCount> ItemsOwnRecord = new List<ItemIdCount> ();
		foreach(ItemIdCount iic in itemsNeedToGet){
			if(CSProcessing.CanProcessItem(iic.protoId)){
				resourceItems.Add(iic);
				continue;
			}
			List<ItemIdCount> needReplicate = new List<ItemIdCount> ();
			needReplicate.Add(iic);
			do{
				List<ItemIdCount> tempNeed = new List<ItemIdCount> ();
				tempNeed.AddRange(needReplicate);
				foreach(ItemIdCount tempIic in tempNeed){
					Replicator.KnownFormula[] msList = UIGraphControl.GetReplicator().GetKnowFormulasByProductItemId(tempIic.protoId);
					if(msList==null||msList.Length==0)
					{
						needReplicate.Remove(tempIic);
						//Debug.LogError("can't get "+tempIic.protoId+"for colony");
						continue;
					}
					//--to do: temp_ only use script 01
					Replicator.Formula ms = Replicator.Formula.Mgr.Instance.Find(msList[0].id);
					foreach(Replicator.Formula.Material mt in ms.materials){
						int needCount = mt.itemCount*Mathf.CeilToInt(tempIic.count*1.0f/ms.m_productItemCount);
						if(core!=null){
							int ownMaterialCount = CSUtils.GetItemCountFromAllStorage(mt.itemId, core);
							if(factory!=null)
								ownMaterialCount += factory.GetAllCompoundItemCount(mt.itemId);

							ItemIdCount ownRecord = ItemsOwnRecord.Find(it=>it.protoId==mt.itemId);
							if(ownMaterialCount>0)
							{
								if(ownRecord==null||ownRecord.count<ownMaterialCount)
								{
									int leftRecordCount=0;
									if(ownRecord==null)
										leftRecordCount = ownMaterialCount;
									else{
										leftRecordCount = ownMaterialCount-ownRecord.count;
									}
									int addRecordCount =0;
									if(needCount>leftRecordCount){
										needCount-=leftRecordCount;
										addRecordCount = leftRecordCount;
									}else{
										needCount=0;
										addRecordCount = needCount;
									}
									if(ownRecord==null)
										ItemsOwnRecord.Add(new ItemIdCount(mt.itemId,addRecordCount));
									else
										ownRecord.count+=addRecordCount;
									if(needCount==0)
										continue;
								}
							}
						}

						if(CSProcessing.CanProcessItem(mt.itemId)){
							CSUtils.AddItemIdCount(resourceItems,mt.itemId,needCount);
						}else{
							CSUtils.AddItemIdCount(needReplicate,mt.itemId,needCount);
						}
					}
					needReplicate.Remove(tempIic);
				}
			}while(needReplicate.Count>0);
		}

		return resourceItems;
	}

	public static List<int> resolveProtoIdToProcess(List<int> protoIds){
		List<int> resourceId = new List<int> ();
		foreach(int protoId in protoIds){
			if(CSProcessing.CanProcessItem(protoId)){
				resourceId.Add(protoId);
				continue;
			}
			List<int> needReplicate = new List<int> ();
			needReplicate.Add(protoId);
			do{
				List<int> tempNeed = new List<int> ();
				tempNeed.AddRange(needReplicate);
				foreach(int tempProtoId in tempNeed){
					Replicator.KnownFormula[] msList = UIGraphControl.GetReplicator().GetKnowFormulasByProductItemId(tempProtoId);
					if(msList==null||msList.Length==0)
					{
						needReplicate.Remove(tempProtoId);
						//Debug.LogError("can't get "+tempProtoId+" for colony");
						continue;
					}
					//--to do: temp_ only use script 01
					Replicator.Formula ms = Replicator.Formula.Mgr.Instance.Find(msList[0].id);
					foreach(Replicator.Formula.Material mt in ms.materials){
						if(CSProcessing.CanProcessItem(mt.itemId)){
							if(!resourceId.Contains(mt.itemId))
								resourceId.Add(mt.itemId);
						}else{
							if(!needReplicate.Contains(mt.itemId))
								needReplicate.Add(mt.itemId);
						}
					}
					needReplicate.Remove(tempProtoId);
				}
			}while(needReplicate.Count>0);
		}
		return resourceId;
	}
}
public enum ETipType{
	process_for_resource,
	process_for_storage,
	replicate_for,
	storage_full,
	factory_to_storage,
	medicine_supply
}

