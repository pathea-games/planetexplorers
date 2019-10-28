using UnityEngine;
using System.Collections;
using ItemAsset;
using System.Collections.Generic;
public partial class ColonyNetwork : AiNetwork
{
	public ColonyBase _ColonyObj;
    public CSEntity m_Entity;
	public int _ownerId;

	protected override void OnPEInstantiate(uLink.NetworkMessageInfo info)
	{
		_id = info.networkView.initialData.Read<int>();
		_externId = info.networkView.initialData.Read<int>();
		_ownerId = info.networkView.initialData.Read<int>();
		_teamId = info.networkView.initialData.Read<int>();

		_pos = transform.position;
		
        //prepare the data
		switch (ExternId)
        {
            case ColonyIDInfo.COLONY_ASSEMBLY:
                {
					PlayerNetwork.OnTeamChangedEventHandler += OnTeamChange;
					PlayerNetwork.OnLimitBoundsAdd(Id, new Bounds(_pos, new Vector3(128, 128, 128)));
					_ColonyObj = new ColonyAssembly(this);
					RPCServer(EPacketType.PT_CL_InitDataAssembly);
                }
                break;
            case ColonyIDInfo.COLONY_PPCOAL:
                {
                    _ColonyObj = new ColonyPPCoal(this);
                    RPCServer(EPacketType.PT_CL_InitDataPPCoal);
                }
                break;
            case ColonyIDInfo.COLONY_STORAGE:
                {
                    _ColonyObj = new ColonyStorage(this);
                    RPCServer(EPacketType.PT_CL_InitDataStorage);
                }
                break;
            case ColonyIDInfo.COLONY_REPAIR:
                {
                    _ColonyObj = new ColonyRepair(this);
                    RPCServer(EPacketType.PT_CL_InitDataRepair);
                }
                break;
            case ColonyIDInfo.COLONY_DWELLINGS:
                {
                    _ColonyObj = new ColonyDwellings(this);
                    RPCServer(EPacketType.PT_CL_InitDataDwellings);
                }
                break;
            case ColonyIDInfo.COLONY_ENHANCE:
                {
                    _ColonyObj = new ColonyEnhance(this);
                    RPCServer(EPacketType.PT_CL_InitDataEnhance);
                }
                break;
            case ColonyIDInfo.COLONY_RECYCLE:
                {
                    _ColonyObj = new ColonyRecycle(this);
                    RPCServer(EPacketType.PT_CL_InitDataRecycle);
                }
                break;
            case ColonyIDInfo.COLONY_FARM:
                {
                    _ColonyObj = new ColonyFarm(this);
                    RPCServer(EPacketType.PT_CL_InitDataFarm);
                }
                break;
            case ColonyIDInfo.COLONY_FACTORY:
                {
                    _ColonyObj = new ColonyFactory(this);
                    RPCServer(EPacketType.PT_CL_InitDataFactory);
                }
                break;
            case ColonyIDInfo.COLONY_PROCESSING:
                {
                    _ColonyObj = new ColonyProcessing(this);
                    RPCServer(EPacketType.PT_CL_InitDataProcessing);
                }
                break;
            case ColonyIDInfo.COLONY_TRADE:
                {
                    _ColonyObj = new ColonyTrade(this);
                    RPCServer(EPacketType.PT_CL_InitDataTrade);
                }
                break;
            case ColonyIDInfo.COLONY_CHECK:
                {
                    _ColonyObj = new ColonyCheck(this);
                    RPCServer(EPacketType.PT_CL_InitDataCheck);
                }
                break;
            case ColonyIDInfo.COLONY_TREAT:
                {
                    _ColonyObj = new ColonyTreat(this);
                    RPCServer(EPacketType.PT_CL_InitDataTreat);
                }
                break;
            case ColonyIDInfo.COLONY_TENT:
                {
                    _ColonyObj = new ColonyTent(this);
                    RPCServer(EPacketType.PT_CL_InitDataTent);
                }
                break;
            case ColonyIDInfo.COLONY_TRAIN:
                {
                    _ColonyObj = new ColonyTrain(this);
                    RPCServer(EPacketType.PT_CL_InitDataTrain);
                }
				break;
			case ColonyIDInfo.COLONY_FUSION:
			{
				_ColonyObj = new ColonyPPFusion(this);
				RPCServer(EPacketType.PT_CL_InitDataPPCoal);
			}
				break;
            default:
			Debug.LogError("ColonySystem itemid is wrong id = " + ExternId);
                break;
        }

		RPCServer (EPacketType.PT_CL_InitData);
    }
	protected override void OnPEStart ()
	{
		BindSkAction();

		BindAction(EPacketType.PT_CL_InitData, RPC_S2C_InitData);
		BindAction(EPacketType.PT_CL_Turn, RPC_S2C_Turn);
		BindAction(EPacketType.PT_CL_SyncItem, RPC_S2C_SyncItem);
		BindAction(EPacketType.PT_CL_SyncCreationFuel, RPC_S2C_SyncCreationFuel);
		BindAction(EPacketType.PT_CL_SyncCreationHP, RPC_S2C_SyncCreationHP);
		BindAction(EPacketType.PT_CL_RepairStart, RPC_S2C_RepairStart);
		BindAction(EPacketType.PT_CL_RepairEnd, RPC_S2C_RepairEnd);
		BindAction(EPacketType.PT_CL_BeginRecycle, RPC_S2C_BeginRecycle);
		BindAction(EPacketType.PT_CL_EndRecycle, RPC_S2C_EndRecycle);
		BindAction(EPacketType.PT_CL_SyncColonyDurability, RPC_S2C_SyncColonyDurability);
		BindAction(EPacketType.PT_CL_RemoveColonyEntity, RPC_S2C_RemoveColonyEntity);
		//assembly
		BindAction(EPacketType.PT_CL_InitDataAssembly, RPC_S2C_InitDataAssembly);
		BindAction(EPacketType.PT_CL_ASB_LevelUp, RPC_S2C_ASB_LevelUp);
        BindAction(EPacketType.PT_CL_ASB_LevelUpStart, RPC_S2C_ASB_LevelUpStart);
		BindAction(EPacketType.PT_CL_ASB_QueryTime, RPC_S2C_ASB_QueryTime);
		BindAction(EPacketType.PT_CL_ASB_HideShield, RPC_S2C_ASB_HideShield);
		BindAction(EPacketType.PT_CL_ASB_ShowTip, RPC_S2C_ASB_ShowTips);
        BindAction(EPacketType.PT_CL_Counter_Tick, RPC_S2C_CounterTick);

		//dwelling
        BindAction(EPacketType.PT_CL_InitDataDwellings, RPC_S2C_InitDataDwellings);
        BindAction(EPacketType.PT_CL_DWL_SyncNpc, RPC_S2C_DWL_SyncNpc);
		//enhance
		BindAction(EPacketType.PT_CL_InitDataEnhance, RPC_S2C_InitDataEnhance);
		BindAction(EPacketType.PT_CL_EHN_SetItem, RPC_S2C_EHN_SetItem);
		BindAction(EPacketType.PT_CL_EHN_Fetch, RPC_S2C_EHN_Fetch);
		BindAction(EPacketType.PT_CL_EHN_Start, RPC_S2C_EHN_Start);
		BindAction(EPacketType.PT_CL_EHN_Stop, RPC_S2C_EHN_Stop);
		BindAction(EPacketType.PT_CL_EHN_End, RPC_S2C_EHN_End);
		BindAction(EPacketType.PT_CL_EHN_SyncTime, RPC_S2C_EHN_SyncTime);
		//factory
		BindAction(EPacketType.PT_CL_InitDataFactory, RPC_S2C_InitDataFactory);
		BindAction(EPacketType.PT_CL_FCT_IsReady, RPC_S2C_FCT_IsReady);
		BindAction(EPacketType.PT_CL_FCT_AddCompoudList, RPC_S2C_FCT_AddCompoudList);
		BindAction(EPacketType.PT_CL_FCT_RemoveCompoudList, RPC_S2C_FCT_RemoveCompoudList);
		BindAction(EPacketType.PT_CL_FCT_SyncItem, RPC_S2C_FCT_SyncItem);
		BindAction(EPacketType.PT_CL_FCT_Fetch, RPC_S2C_FCT_Fetch);
		BindAction(EPacketType.PT_CL_FCT_Compoud, RPC_S2C_FCT_Compoud);
		BindAction(EPacketType.PT_CL_FCT_SyncAllItems, RPC_S2C_FCT_SyncAllItems);
		BindAction(EPacketType.PT_CL_FCT_GenFactoryCancel, RPC_S2C_FCT_GenFactoryCancel);
        //farm
		BindAction(EPacketType.PT_CL_InitDataFarm, RPC_S2C_InitDataFarm);
		BindAction(EPacketType.PT_CL_FARM_SetPlantSeed, RPC_S2C_FARM_SetPlantSeed);
		BindAction(EPacketType.PT_CL_FARM_SetPlantTool, RPC_C2S_FARM_SetPlantTool);
		BindAction(EPacketType.PT_CL_FARM_SetSequentialActive, RPC_S2C_FARM_SetSequentialActive);
		BindAction(EPacketType.PT_CL_FARM_SetAutoPlanting, RPC_S2C_FARM_SetAutoPlanting);
        BindAction(EPacketType.PT_CL_FARM_FetchSeedItem, RPC_S2C_FARM_FetchSeedItemResult);
        BindAction(EPacketType.PT_CL_FARM_FetchToolItem, RPC_S2C_FARM_FetchToolItemResult);
		//BindAction(EPacketType.PT_CL_FARM_GetSeed, RPC_S2C_FARM_SetAutoPlanting);//??
		BindAction(EPacketType.PT_CL_FARM_DeleteSeed, RPC_S2C_FARM_DeleteSeed);
        BindAction(EPacketType.PT_CL_FARM_DeletePlantTool, RPC_S2C_FARM_DeletePlantTool);
        BindAction(EPacketType.PT_CL_FARM_RestoreWater, RPC_S2C_FARM_RestoreWater);
        BindAction(EPacketType.PT_CL_FARM_RestoreClean, RPC_S2C_FARM_RestoreClean);
        BindAction(EPacketType.PT_CL_FARM_RestoreGetBack, RPC_S2C_FARM_RestoreGetBack);

        //PowerPlant
        BindAction(EPacketType.PT_CL_InitDataPowerPlanet, RPC_S2C_InitDataPowerPlanet);
        BindAction(EPacketType.PT_CL_POW_AddChargItem, RPC_S2C_POW_AddChargItem);
        BindAction(EPacketType.PT_CL_POW_GetChargItem, RPC_S2C_POW_GetChargItem);
        //ppcoal
        BindAction(EPacketType.PT_CL_InitDataPPCoal, RPC_S2C_InitDataPPCoal);
        BindAction(EPacketType.PT_CL_PPC_AddFuel, RPC_S2C_PPC_AddFuel);
        BindAction(EPacketType.PT_CL_PPC_WorkedTime, RPC_S2C_PPC_WorkedTime);
        BindAction(EPacketType.PT_CL_PPC_NoPower, RPC_S2C_PPC_NoPower);
        //recycle
        BindAction(EPacketType.PT_CL_InitDataRecycle, RPC_S2C_InitDataRecycle);
        BindAction(EPacketType.PT_CL_RCY_SetItem, RPC_S2C_RCY_SetItem);
        BindAction(EPacketType.PT_CL_RCY_Start, RPC_S2C_RCY_Start);
        BindAction(EPacketType.PT_CL_RCY_Stop, RPC_S2C_RCY_Stop);
        BindAction(EPacketType.PT_CL_RCY_FetchMaterial, RPC_S2C_RCY_FetchMaterial);
        BindAction(EPacketType.PT_CL_RCY_FetchItem, RPC_S2C_RCY_FetchItem);
        BindAction(EPacketType.PT_CL_RCY_SyncTime, RPC_S2C_RCY_SyncTime);
        BindAction(EPacketType.PT_CL_RCY_SyncRecycleItem, RPC_S2C_RCY_SyncRecycleItem);
        BindAction(EPacketType.PT_CL_RCY_End, RPC_S2C_RCY_End);
        BindAction(EPacketType.PT_CL_RCY_MatsToStorage, RPC_S2C_RCY_MatsToStorage);
        BindAction(EPacketType.PT_CL_RCY_MatsToResult, RPC_S2C_RCY_MatsToResult);
        //storage
        BindAction(EPacketType.PT_CL_InitDataStorage, RPC_S2C_InitDataStorage);
        BindAction(EPacketType.PT_CL_STO_Delete, RPC_S2C_STO_Delete);
        BindAction(EPacketType.PT_CL_STO_Store, RPC_S2C_STO_Store);
        BindAction(EPacketType.PT_CL_STO_Fetch, RPC_S2C_STO_FetchItem);
        BindAction(EPacketType.PT_CL_STO_Exchange, RPC_S2C_STO_Exchange);
        BindAction(EPacketType.PT_CL_STO_Split, RPC_S2C_STO_Split);
        BindAction(EPacketType.PT_CL_STO_Sort, RPC_S2C_STO_Sort);
		BindAction (EPacketType.PT_CL_STO_SyncItemList,RPC_S2C_STO_SyncItemList);
        //repair
        BindAction(EPacketType.PT_CL_InitDataRepair, RPC_S2C_InitDataRepair);
        BindAction(EPacketType.PT_CL_RPA_SetItem, RPC_S2C_RPA_SetItem);
        BindAction(EPacketType.PT_CL_RPA_Start, RPC_S2C_RPA_Start);
        BindAction(EPacketType.PT_CL_RPA_Stop, RPC_S2C_RPA_Stop);
        BindAction(EPacketType.PT_CL_RPA_End, RPC_S2C_RPA_End);
        BindAction(EPacketType.PT_CL_RPA_FetchItem, RPC_S2C_RPA_FetchItem);
        BindAction(EPacketType.PT_CL_RPA_SyncTime, RPC_S2C_RPA_SyncTime);
        //processing
        BindAction(EPacketType.PT_CL_InitDataProcessing, RPC_S2C_InitDataProcessing);
        BindAction(EPacketType.PT_CL_PRC_AddItem, RPC_S2C_PRC_AddItem);
        BindAction(EPacketType.PT_CL_PRC_RemoveItem, RPC_S2C_PRC_RemoveItem);
        BindAction(EPacketType.PT_CL_PRC_AddNpc, RPC_S2C_PRC_AddNpc);
		BindAction(EPacketType.PT_CL_PRC_RemoveNpc, RPC_S2C_PRC_RemoveNpc);
		BindAction(EPacketType.PT_CL_PRC_SetRound, RPC_S2C_PRC_SetRound);
        BindAction(EPacketType.PT_CL_PRC_SetAuto, RPC_S2C_PRC_SetAuto);
        BindAction(EPacketType.PT_CL_PRC_Start, RPC_S2C_PRC_StartTask);
        BindAction(EPacketType.PT_CL_PRC_Stop, RPC_S2C_PRC_StopTask);
        //BindAction(EPacketType.PT_CL_PRC_InitResultPos);
		BindAction(EPacketType.PT_CL_PRC_GenResult, RPC_S2C_PRC_GenResult);
		BindAction(EPacketType.PT_CL_PRC_FinishToStorage, RPC_S2C_PRC_FinishToStorage);
		BindAction(EPacketType.PT_CL_PRC_SyncAllCounter,RPC_S2C_PRC_SyncAllCounter);

        //trade
        BindAction(EPacketType.PT_CL_InitDataTrade, RPC_S2C_InitDataTrade);
//        BindAction(EPacketType.PT_CL_TRD_AddTown, RPC_S2C_TRD_AddTown);
//		BindAction(EPacketType.PT_CL_TRD_RemoveTown, RPC_S2C_TRD_RemoveTown);
//        BindAction(EPacketType.PT_CL_TRD_TryTrade, RPC_S2C_TRD_TryTrade);
//        BindAction(EPacketType.PT_CL_TRD_RefreshItem, RPC_S2C_TRD_RefreshItem);
		BindAction (EPacketType.PT_CL_TRD_BuyItem,RPC_S2C_BuyItem);
		BindAction (EPacketType.PT_CL_TRD_SellItem,RPC_S2C_SellItem);
		BindAction (EPacketType.PT_CL_TRD_RepurchaseItem,RPC_S2C_RepurchaseItem);
		BindAction (EPacketType.PT_CL_TRD_UpdateBuyItem,RPC_S2C_UpdateBuyItem);
		BindAction (EPacketType.PT_CL_TRD_UpdateRepurchaseItem,RPC_S2C_UpdateRepurchaseItem);
		BindAction (EPacketType.PT_CL_TRD_UpdateMoney,RPC_S2C_UpdateMoney);
        //check
        BindAction(EPacketType.PT_CL_InitDataCheck, RPC_S2C_InitDataCheck);
        BindAction(EPacketType.PT_CL_CHK_FindMachine, RPC_S2C_CHK_FindMachine);
        BindAction(EPacketType.PT_CL_CHK_SetDiagnose, RPC_S2C_CHK_SetDiagnose);
        BindAction(EPacketType.PT_CL_CHK_TryStart, RPC_S2C_CHK_TryStart);
        BindAction(EPacketType.PT_CL_CHK_RemoveDeadNpc, RPC_S2C_CHK_RemoveDeadNpc);
        BindAction(EPacketType.PT_CL_CHK_CheckFinish, RPC_S2C_CHK_CheckFinish);
        //treat
        BindAction(EPacketType.PT_CL_InitDataTreat, RPC_S2C_InitDataTreat);
        BindAction(EPacketType.PT_CL_TRT_FindMachine, RPC_S2C_TRT_FindMachine);
        BindAction(EPacketType.PT_CL_TRT_SetTreat, RPC_S2C_TRT_SetTreat);
		BindAction(EPacketType.PT_CL_TRT_TryStart, RPC_S2C_TRT_TryStart);
		BindAction(EPacketType.PT_CL_TRT_StartTreatCounter, RPC_S2C_TRT_StartTreatCounter);
        BindAction(EPacketType.PT_CL_TRT_SetItem, RPC_S2C_TRT_SetItem);
		BindAction(EPacketType.PT_CL_TRT_DeleteItem, RPC_S2C_TRT_DeleteItem);
        BindAction(EPacketType.PT_CL_TRT_RemoveDeadNpc, RPC_S2C_TRT_RemoveDeadNpc);
        BindAction(EPacketType.PT_CL_TRT_TreatFinish, RPC_S2C_TRT_TreatFinish);
		BindAction(EPacketType.PT_CL_TRT_ResetNpcToCheck, RPC_S2C_TRT_ResetNpcToCheck);
        //tent
        BindAction(EPacketType.PT_CL_InitDataTent, RPC_S2C_InitDataTent);
        BindAction(EPacketType.PT_CL_TET_FindMachine, RPC_S2C_TET_FindMachine);
        BindAction(EPacketType.PT_CL_TET_TryStart, RPC_S2C_TET_TryStart);
        BindAction(EPacketType.PT_CL_TET_SetTent, RPC_S2C_TET_SetTent);
        BindAction(EPacketType.PT_CL_TET_RemoveDeadNpc, RPC_S2C_TET_RemoveDeadNpc);
        BindAction(EPacketType.PT_CL_TET_TentFinish, RPC_S2C_TET_TentFinish);
        //train
        BindAction(EPacketType.PT_CL_InitDataTrain, RPC_S2C_InitDataTrain);
        BindAction(EPacketType.PT_CL_TRN_StartSkillTraining, RPC_S2C_TRN_StartSkillTraining);
        BindAction(EPacketType.PT_CL_TRN_StartAttributeTraining, RPC_S2C_TRN_StartAttributeTraining);
        BindAction(EPacketType.PT_CL_TRN_SetInstructor, RPC_S2C_TRN_SetInstructor);
        BindAction(EPacketType.PT_CL_TRN_SetTrainee, RPC_S2C_TRN_SetTrainee);
        BindAction(EPacketType.PT_CL_TRN_SkillTrainFinish, RPC_S2C_TRN_SkillTrainFinish);
        BindAction(EPacketType.PT_CL_TRN_AttributeTrainFinish, RPC_S2C_TRN_AttributeTrainFinish);
		BindAction(EPacketType.PT_CL_TRN_StopTraining,RPC_S2C_TRN_StopTraining);
		BindAction(EPacketType.PT_CL_TRN_SyncCounter,RPC_S2C_TRN_SyncCounter);

		BindAction(EPacketType.PT_InGame_SetController, RPC_S2C_SetController);
		BindAction(EPacketType.PT_InGame_LostController, RPC_S2C_LostController);
	}

    protected override void OnPEDestroy()
    {
		StopAllCoroutines();

		switch (ExternId)
		{
			case ColonyIDInfo.COLONY_ASSEMBLY:
				{
					PlayerNetwork.OnTeamChangedEventHandler -= OnTeamChange;
					PlayerNetwork.OnLimitBoundsDel(Id);
				}
				break;
		}

        DragArticleAgent.Destory(Id);
		
		if (null != Runner)
			Destroy(Runner.gameObject);
    }

	protected override IEnumerator SyncMove()
	{
		yield break;
	}

	public override void InitForceData()
	{
		if (null != _entity)
		{
			_entity.SetAttribute(Pathea.AttribType.DefaultPlayerID, TeamId, false);
			_entity.SetAttribute(Pathea.AttribType.CampID, TeamId, false);
			_entity.SetAttribute(Pathea.AttribType.DamageID, TeamId, false);
		}
	}

	protected override void RPC_S2C_InitData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		ItemObject item = stream.Read<ItemObject>();
		float rotY = stream.Read<float>();
		
		rot = transform.rotation = Quaternion.Euler(0, rotY, 0);

        //InitData parameter member 
        //and add data to creator
        _ColonyObj._RecordData.m_Position = transform.position;//network's position
		_ColonyObj._RecordData.ItemID = ExternId;
        _ColonyObj._RecordData.ID = Id;

        MultiColonyManager.Instance.AddDataToCreator(this, TeamId);
//        if(Application.isEditor)
//            Debug.Log("<color=red>Get colony InitData!</color>");

        if (null == item)
            return;

        Drag drag = item.GetCmpt<Drag>();
        if (null == drag)
            return;

		DragArticleAgent dragItem = DragArticleAgent.Create(drag, transform.position, transform.localScale, transform.rotation, Id, this);

        if (dragItem.itemLogic != null)
        {
            CSBuildingLogic csbl = dragItem.itemLogic as CSBuildingLogic;
            if (csbl != null)
            {
                csbl.InitInMultiMode(m_Entity,_ownerId);
				OnTeamChange();
				_entity = Pathea.EntityMgr.Instance.Get(Id);
                OnSpawned(csbl.gameObject);
            }
        }
	}

    protected override void RPC_S2C_Turn(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
        transform.rotation = stream.Read<Quaternion>();

        //if (null != _viewTrans)
        //    _viewTrans.rotation = transform.rotation;
        CSBuildingLogic csbl = Runner.GetComponent<CSBuildingLogic>();
        if (csbl != null)
        {
            DragItemAgent dia = DragItemAgent.GetById(csbl.id);
            dia.rotation = transform.rotation;
        }

        if (null != Runner)
        {
			CSEntityObject ceo = Runner.GetComponentInChildren<CSEntityObject>();
            if (ceo != null)
            {
                if(csbl!=null)
                    ceo.Init(csbl, ceo.m_Creator, false);
            }

            DragItemMousePickColony itemscript = Runner.GetComponentInChildren<DragItemMousePickColony>();
            if(itemscript!=null)
                itemscript.OnItemOpGUIHide();
        }
			
	}

    void RPC_S2C_SyncItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
        //ItemObject item = stream.Read<ItemObject>();
		
        //ItemObject existItem = ItemManager.Instance.GetItemByID(item.instanceId);
        //if (null != existItem)
        //{
        //    existItem.stackableCount = item.stackableCount;
			
        //    foreach (KeyValuePair<ItemProperty, float> kv in item.mItemProperty)
        //        existItem.mItemProperty[kv.Key] = kv.Value;
        //}
        //else
        //{
        //    ItemManager.Instance.AddItem(item);
        //}
	}

    void RPC_S2C_SyncCreationFuel(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int objId = stream.Read<int>();
		float fuel = stream.Read<float>();

		var itemObject = ItemAsset.ItemMgr.Instance.Get(objId);
		var energyCmpt = itemObject.GetCmpt<ItemAsset.Energy>();
		energyCmpt.floatValue.current = fuel;
	}

    void RPC_S2C_SyncCreationHP(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int objId = stream.Read<int>();
		float hp = stream.Read<float>();

		var itemObject = ItemAsset.ItemMgr.Instance.Get(objId);
		var lifeCmpt = itemObject.GetCmpt<ItemAsset.LifeLimit>();
		lifeCmpt.floatValue.current = hp;
    }


    void RPC_S2C_RepairStart(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
        _ColonyObj._RecordData.m_CurRepairTime = stream.Read<float>();
        _ColonyObj._RecordData.m_RepairTime = stream.Read<float>();
        _ColonyObj._RecordData.m_RepairValue = stream.Read<float>();
        m_Entity.StartRepairCounter(_ColonyObj._RecordData.m_CurRepairTime, _ColonyObj._RecordData.m_RepairTime, _ColonyObj._RecordData.m_RepairValue);
        CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mStartToRepair.GetString(), m_Entity.Name));
	}

    void RPC_S2C_RepairEnd(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
        _ColonyObj._RecordData.m_Durability = stream.Read<float>();
        _ColonyObj._RecordData.m_RepairTime =-1;
        _ColonyObj._RecordData.m_RepairValue = 0;
	}

    void RPC_S2C_BeginRecycle(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
        _ColonyObj._RecordData.m_DeleteTime = stream.Read<float>();

        m_Entity.StartDeleteCounter(0, _ColonyObj._RecordData.m_DeleteTime);
        CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mStartToDelete.GetString(), m_Entity.Name));
	}


    void RPC_S2C_EndRecycle(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		bool bsuc = stream.Read<bool>();
        _ColonyObj._RecordData.m_CurDeleteTime = -1;
        _ColonyObj._RecordData.m_DeleteTime = -1;
        if (bsuc)
        {
            m_Entity.m_Creator.RemoveEntity(Id);
        }
    }

    void RPC_S2C_SyncColonyDurability(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        float durability = stream.Read<float>();
        _ColonyObj._RecordData.m_Durability = durability;
    }


    void RPC_S2C_RemoveColonyEntity(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int objID = stream.Read<int>();
		if(BelongToOwner)
			CSMain.s_MgCreator.RemoveEntity(objID,false);
		else
        	MultiColonyManager.GetCreator(TeamId).RemoveEntity(objID,false);
    }

	public bool BelongToOwner{
		get{ return TeamId == BaseNetwork.MainPlayer.TeamId;}
	}
}

