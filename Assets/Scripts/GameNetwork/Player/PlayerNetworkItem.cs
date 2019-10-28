using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using CustomData;
using SkillAsset;
using ItemAsset;
using TownData;
using Pathea;
/// <summary>
/// 物品相关的同步处理
/// by PuJi
/// </summary>
using NaturalResAsset;

using NetworkHelper;

public partial class PlayerNetwork
{
	#region Network Request
	public void RequestDragOut(int objId, Vector3 pos, Vector3 scale, Quaternion rot,byte terrainType)
	{
		RPCServer(EPacketType.PT_InGame_PutItem, objId, pos, scale, rot,terrainType);
	}

	public void RequestDragTower(int objId, Vector3 pos, Quaternion rot)
	{
		RPCServer(EPacketType.PT_InGame_PutOutTower, objId, pos, rot);
	}

	public void RequestDragFlag(int objId, Vector3 pos, Quaternion rot)
	{
		RPCServer(EPacketType.PT_InGame_PutOutFlag, objId, pos, rot);
	}

	public void RequestUseItem(int itemObjId)
	{
		RPCServer(EPacketType.PT_InGame_UseItem, itemObjId);
	}

	public void RequestNpcPutOnEquip(int npcId, int objId, ItemPlaceType place)
	{
		AiAdNpcNetwork npc = AiAdNpcNetwork.Get<AiAdNpcNetwork>(npcId);
		if (null == npc)
			return;

		RPCServer(EPacketType.PT_NPC_PutOnEquip, npcId, objId, place);
	}

	public void RequestNpcTakeOffEquip(int npcId, int objId, int destIndex)
	{
		AiAdNpcNetwork npc = AiAdNpcNetwork.Get<AiAdNpcNetwork>(npcId);
		if (null == npc)
			return;

		RPCServer(EPacketType.PT_NPC_TakeOffEquip, npcId, objId, destIndex);
	}

	public void RequestGiveItem2Npc(int tabIndex, int npcId, int objId, ItemPlaceType place)
	{
		AiAdNpcNetwork npc = AiAdNpcNetwork.Get<AiAdNpcNetwork>(npcId);
		if (null == npc)
			return;

		RPCServer(EPacketType.PT_NPC_GetItem, tabIndex, npcId, objId, place);
	}

	public void RequestGetItemFromNpc(int tabIndex, int npcId, int objId, int destIndex)
	{
		AiAdNpcNetwork npc = AiAdNpcNetwork.Get<AiAdNpcNetwork>(npcId);
		if (null == npc)
			return;

		RPCServer(EPacketType.PT_NPC_DeleteItem, tabIndex, npcId, objId, destIndex);
	}

    public void RequestGetAllItemFromNpc(int npcId, int tabIndex)
    {
        AiAdNpcNetwork npc = AiAdNpcNetwork.Get<AiAdNpcNetwork>(npcId);
        if (null == npc)
            return;

        RPCServer(EPacketType.PT_NPC_DeleteAllItem, npcId, tabIndex);
    }

    public void RequestNpcPackageSort(int npcId, int tabIndex)
    {
        AiAdNpcNetwork npc = AiAdNpcNetwork.Get<AiAdNpcNetwork>(npcId);
        if (null == npc)
            return;

        RPCServer(EPacketType.PT_NPC_SortPackage, npcId, tabIndex);
    }

    public void RequestChangeScene(int sceneId)
	{
        if (PeGameMgr.IsMultiStory)
        {
            MultiStorySceneObjectManager.instance.RequestChangeScene(Id, sceneId);
        }
		RPCServer(EPacketType.PT_InGame_CurSceneId, sceneId);
	}
	#endregion

	#region Action Callback APIs
	/// <summary>
    /// 初始化背包大小
    /// </summary>
    /// <param name="stream"></param>
    void RPC_S2C_InitPackage(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int itemMax = stream.Read<int>();
		int equipMax = stream.Read<int>();
		int resourceMax = stream.Read<int>();
        int armMax = stream.Read<int>();

		if (Equals(null, entity))
			return;

		Pathea.PlayerPackageCmpt pkg = entity.GetCmpt<Pathea.PlayerPackageCmpt>();
		if (null != pkg)
			pkg.package.ExtendPackage(itemMax, equipMax, resourceMax,armMax);
	}

	void RPC_S2C_PackageIndex(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] data = stream.Read<byte[]>();

		if (Equals(null, entity))
			return;

		Pathea.PlayerPackageCmpt pkg = entity.GetCmpt<Pathea.PlayerPackageCmpt>();
		if (null == pkg)
			return;

		PETools.Serialize.Import(data, r =>
			{
				int count = BufferHelper.ReadInt32(r);
				for (int i = 0; i < count; i++)
				{
					int key = BufferHelper.ReadInt32(r);
					int id = BufferHelper.ReadInt32(r);
					int tab = key >> 16;
					int index = key & 0x0000FFFF;

					pkg.package.ResetPackageItems(tab, index, id,false);
				}
			});

		if (IsOwner)
			GameUI.Instance.mItemPackageCtrl.ResetItem();
	}

	void RPC_S2C_MissionPackageIndex(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] data = stream.Read<byte[]>();

		if (Equals(null, entity))
			return;

		Pathea.PlayerPackageCmpt pkg = entity.GetCmpt<Pathea.PlayerPackageCmpt>();
        if (null == pkg)
            return;

        PETools.Serialize.Import(data, r =>
        {
            int count = BufferHelper.ReadInt32(r);
            for (int i = 0; i < count; i++)
            {
                int key = BufferHelper.ReadInt32(r);
                int id = BufferHelper.ReadInt32(r);
                int tab = key >> 16;
                int index = key & 0x0000FFFF;

                pkg.package.ResetPackageItems(tab, index, id,true);
            }
        });

        if (IsOwner)
            GameUI.Instance.mItemPackageCtrl.ResetItem();
	}

	void RPC_S2C_EquipedItems(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		ItemObject[] equips = stream.Read<ItemObject[]>();

		if (Equals(null, entity))
			return;

		Pathea.EquipmentCmpt cmpt = entity.GetCmpt<Pathea.EquipmentCmpt>();
		if (null != cmpt)
			cmpt.ApplyEquipment(equips);
	}

	void RPC_S2C_PutOnEquipment(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		ItemObject[] equips = stream.Read<ItemObject[]>();

		if (Equals(null, entity))
			return;

		Pathea.EquipmentCmpt cmpt = entity.GetCmpt<Pathea.EquipmentCmpt>();
		if (null != cmpt)
		{
			foreach (ItemObject equip in equips)
				cmpt.PutOnEquipment(equip, false, null, true);
		}
	}

	void RPC_S2C_TakeOffEquipment(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int[] equipIds = stream.Read<int[]>();

		if (Equals(null, entity))
			return;

		Pathea.EquipmentCmpt cmpt = entity.GetCmpt<Pathea.EquipmentCmpt>();
		if (null != cmpt)
		{
			foreach (int equipId in equipIds)
			{
				ItemObject equip = ItemMgr.Instance.Get(equipId);
				cmpt.TakeOffEquipment(equip, false, null, true);
			}
		}
	}

    /// <summary>
    /// initial the shortCut
    /// </summary>
    /// <param name="stream"></param>
	void RPC_S2C_InitShortcut(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] data = stream.Read<byte[]>();

		if (Equals(null, entity))
			return;

		Dictionary<int, int> shortcut = new Dictionary<int, int>();
		PETools.Serialize.Import(data, r =>
		{
			int count = BufferHelper.ReadInt32(r);
			for (int i = 0; i < count; i++)
			{
				int index = BufferHelper.ReadInt32(r);
				int objId = BufferHelper.ReadInt32(r);
				shortcut[index] = objId;
			}
		});

		Pathea.PlayerPackageCmpt cmpt = entity.GetCmpt<Pathea.PlayerPackageCmpt>();
		if (null == cmpt)
			return;

		foreach (KeyValuePair<int, int> kv in shortcut)
		{
			ItemObject item = ItemMgr.Instance.Get(kv.Value);
			cmpt.shortCutSlotList.PutItemObj(item, kv.Key);
		}
	}

	void RPC_S2C_PlayerMoney(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int money = stream.Read<int>();

		if (Equals(null, entity))
			return;

		Pathea.PackageCmpt package = entity.GetCmpt<Pathea.PackageCmpt>();
		if (null == package)
			return;

		package.money.current = money;
	}

    void RPC_S2C_MoneyType(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
		int currency = stream.Read<int> ();
		if(currency == (int)CustomData.EMoneyType.Digital)
		{
			Money.Digital = true;
			GameUI.Instance.mShopWnd.mMeatSprite.gameObject.SetActive(false);
			GameUI.Instance.mShopWnd.mMoneySprite.gameObject.SetActive(true);
			GameUI.Instance.mItemPackageCtrl.nMoneyRoot.SetActive(true);
		}
		else if(currency == (int)CustomData.EMoneyType.Meat)
		{
			Money.Digital = false;
			GameUI.Instance.mShopWnd.mMeatSprite.gameObject.SetActive(true);
			GameUI.Instance.mShopWnd.mMoneySprite.gameObject.SetActive(false);
			GameUI.Instance.mItemPackageCtrl.nMoneyRoot.SetActive(false);
		}
    }

    void RPC_S2C_CurSceneId(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		_curSceneId = stream.Read<int>();
		if(Pathea.PeGameMgr.IsMultiStory)
		{
            if (mainPlayerId == Id)
            {
                SceneDoodadLodCmpt doodadCmpt;
                PeEntity[] doodad;

                doodad = EntityMgr.Instance.GetDoodadEntities(242);
                if (doodad.Length > 0)
                {
                    doodadCmpt = doodad[0].GetComponent<SceneDoodadLodCmpt>();
                    if (doodadCmpt != null)
                        doodadCmpt.IsShown = true;
                }
                doodad = EntityMgr.Instance.GetDoodadEntities(240);
                if (doodad.Length > 0)
                {
                    doodadCmpt = doodad[0].GetComponent<SceneDoodadLodCmpt>();
                    if (doodadCmpt != null)
                        doodadCmpt.IsShown = true;
                }

                doodad = EntityMgr.Instance.GetDoodadEntities(324);
                if (doodad.Length > 0)
                {
                    doodadCmpt = doodad[0].GetComponent<SceneDoodadLodCmpt>();
                    if (doodadCmpt != null)
                        doodadCmpt.IsShown = true;
                }

                doodad = EntityMgr.Instance.GetDoodadEntities(326);
                if (doodad.Length > 0)
                {
                    doodadCmpt = doodad[0].GetComponent<SceneDoodadLodCmpt>();
                    if (doodadCmpt != null)
                        doodadCmpt.IsShown = true;
                }

                doodad = EntityMgr.Instance.GetDoodadEntities(327);
                if (doodad.Length > 0)
                {
                    doodadCmpt = doodad[0].GetComponent<SceneDoodadLodCmpt>();
                    if (null != doodadCmpt)
                        doodadCmpt.IsShown = true;
                }
                for (int i = 461; i < 464; i++)
                {
                    doodad = EntityMgr.Instance.GetDoodadEntities(i);
                    if (doodad.Length > 0)
                    {
                        doodadCmpt = doodad[0].GetComponent<SceneDoodadLodCmpt>();
                        if (null != doodadCmpt)
                            doodadCmpt.IsShown = true;
                    }
                }
            }
            MultiStorySceneObjectManager.instance.RequestChangeScene(Id, _curSceneId);
        }
	}

	void RPC_S2C_FarmInfo(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
        /*ItemObject[] itemobj = */stream.Read<ItemObject[]>();
        byte[] data = stream.Read<byte[]>();

		if (null == FarmManager.Instance)
			return;

        List<FarmPlantInitData> initList = FarmManager.Instance.ImportPlantData(data);

        foreach (FarmPlantInitData plantData in initList)
        {
            ItemObject itemObj = ItemMgr.Instance.Get(plantData.mPlantInstanceId);
            DragArticleAgent dragItem = DragArticleAgent.Create(itemObj.GetCmpt<Drag>(), plantData.mPos, Vector3.one, plantData.mRot, plantData.mPlantInstanceId);
            
            FarmPlantLogic plant = dragItem.itemLogic as FarmPlantLogic;
            plant.InitDataFromPlant(plantData);
            FarmManager.Instance.AddPlant(plant);
            plant.UpdateInMultiMode();
        }
	}

	void RPC_S2C_GetDeadObjAllItems(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int netId = stream.Read<int>();

		AiNetwork ai = AiNetwork.Get<AiNetwork>(netId);
		if (null == ai || null == ai.Runner)
			return;

		ItemDropPeEntity dropEntity = ai.Runner.GetComponent<ItemDropPeEntity>();
		if (null == dropEntity)
			return;

		dropEntity.RemoveDroppableItemAll();

		if (null != GameUI.Instance.mItemGet)
			GameUI.Instance.mItemGet.Reflash();
	}

	void RPC_S2C_GetDeadObjItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int netId = stream.Read<int>();
		int index = stream.Read<int>();
		int itemId = stream.Read<int>();

		AiNetwork ai = AiNetwork.Get<AiNetwork>(netId);
		if (null == ai || null == ai.Runner)
			return;

		ItemDropPeEntity dropEntity = ai.Runner.GetComponent<ItemDropPeEntity>();
		if (null == dropEntity)
			return;

		ItemSample item = dropEntity.Get(index);
		if (null == item || item.protoId != itemId)
			return;

		dropEntity.RemoveDroppableItem(item);

		if (null != GameUI.Instance.mItemGet)
			GameUI.Instance.mItemGet.Reflash();
	}

	void RPC_S2C_GetItemBack(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int objId = stream.Read<int>();
		DragItemAgent obj = DragItemAgent.GetById(objId);
		DragItemAgent.Destory(obj);
	}

	void RPC_S2C_PreGetItemBack(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>();

		NetworkInterface netObj = NetworkInterface.Get(id);
		if (null == netObj)
			return;

		if (ForceSetting.Instance.AllyForce(netObj.TeamId, TeamId))
			return;

		//if (null == mainPlayer || null == mainPlayer.entity)
		//	return;

		PeEntity objEntity = EntityMgr.Instance.Get(id);
		if (null == objEntity)
			return;

		int playerID = (int)objEntity.GetAttribute(AttribType.DefaultPlayerID);
		List<PeEntity> entities = EntityMgr.Instance.GetEntities(_pos, 64f, playerID, false, entity);
		for (int i = 0; i < entities.Count; i++)
		{
			if (!entities[i].Equals(entity) && entities[i].target != null)
				entities[i].target.TransferHatred(entity, 5);
		}

		//AiTowerNetwork.OnTransferHatred(PlayerEntity, Id, playerID, 5);
	}

	void RPC_S2C_GetLootItemBack(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int objId = stream.Read<int>();
		LootItemMgr.Instance.NetFetch(objId,this.Id);
	}

	void RPC_S2C_NewItemList(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		ItemSample[] newItems = stream.Read<ItemSample[]>();

		if (null == newItems || newItems.Length <= 0)
			return;

		foreach (ItemSample item in newItems)
		{
			if (null == item)
				continue;

			if (null != MissionManager.Instance)
				MissionManager.Instance.ProcessCollectMissionByID(item.protoId);
//			GlobalShowGui_N.Instance.AddShow(item);

			ItemProto protoData = ItemProto.Mgr.Instance.Get(item.protoId);
			if (protoData == null)
				continue;
			
			string msg = protoData.GetName() + " X " + item.stackCount.ToString();
			/*PeTipMsg tips = */new PeTipMsg(msg, protoData.icon[0], PeTipMsg.EMsgLevel.Norm, PeTipMsg.EMsgType.Misc);
		}
	}

	/// <summary>
	/// 删除背包中的物品
	/// </summary>
	/// <param name="stream"></param>
	void RPC_S2C_DeleteItemInPackage(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int _objId;

		stream.TryRead<int>(out _objId);

		ItemObject obj = ItemMgr.Instance.Get(_objId);
		if (null == obj)
			return;

		Pathea.PlayerPackageCmpt pkg = entity.GetCmpt<Pathea.PlayerPackageCmpt>();
		if (null != pkg)
			pkg.Remove(obj);

		if (IsOwner)
		{
			GameUI.Instance.mItemPackageCtrl.ResetItem();
			//MainMidGui_N.Instance.UpdateLink();
		}

		//  if (null != PlayerFactory.mMainPlayer && null != PlayerFactory.mMainPlayer.m_PlayerMission)
		//   PlayerFactory.mMainPlayer.m_PlayerMission.CheckItemMissionList(nItemID);

	}

	void RPC_S2C_UseItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int itemObjId = stream.Read<int>();
		ItemObject item = ItemMgr.Instance.Get(itemObjId);
		if(item != null)
		{
			Pathea.UseItemCmpt useItem = entity.GetCmpt<Pathea.UseItemCmpt>();
			if(useItem != null)
				useItem.UseFromNet(item);
		}

	}

	void RPC_S2C_SplitItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>();
		int index = stream.Read<int>();

		ItemObject item = ItemMgr.Instance.Get(id);
		if (null == item)
			return;

		Pathea.PlayerPackageCmpt pkg = entity.GetCmpt<Pathea.PlayerPackageCmpt>();
		if (null == pkg)
			return;

		pkg.package.PutItem(item, index, (ItemPackage.ESlotType)item.protoData.tabIndex);
	}

	void RPC_S2C_ExchangeItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>();
		int destIndex = stream.Read<int>();
		int destId = stream.Read<int>();
		int srcIndex = stream.Read<int>();

		Pathea.PlayerPackageCmpt pkg = entity.GetCmpt<Pathea.PlayerPackageCmpt>();
		if (null == pkg)
			return;

		ItemObject item = ItemMgr.Instance.Get(id);
		if (null == item)
			return;

		if (-1 == destId)
		{
			pkg.Remove(item);
		}
		else
		{
			ItemObject destItem = ItemMgr.Instance.Get(destId);
			if (null != destItem)
				pkg.package.PutItem(destItem, srcIndex, (ItemPackage.ESlotType)item.protoData.tabIndex);
		}

		pkg.package.PutItem(item, destIndex, (ItemPackage.ESlotType)item.protoData.tabIndex);
	}

    //[Obsolete]
	void RPC_S2C_PutItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>();
		Vector3 pos = stream.Read<Vector3>();
		Vector3 scale = stream.Read<Vector3>();
		Quaternion rot = stream.Read<Quaternion>();

		ItemObject item = ItemMgr.Instance.Get(id);
		if (null == item)
			return;

		Drag drag = item.GetCmpt<Drag>();
		if (null == drag)
			return;

//         if (item.protoId == 1339)
//             KillNPC.ashBox_inScene++;


		/*DragArticleAgent dragItem = */DragArticleAgent.Create(drag, pos, scale, rot, id);		
	}

	void RPC_S2C_Turn(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int objId = stream.Read<int>();
        Quaternion rot = stream.Read<Quaternion>();
        DragItemAgent dia = DragItemAgent.GetById(objId);
        dia.rotation = rot;

        ItemObject item = dia.itemDrag.itemObj;
		if (null == item)
			return;

        //ISceneObjAgent obj = SceneMan.GetSceneObjById(objId);
        //if (obj.Equals(null))
        //    return;
	}

	void RPC_S2C_RemoveItemFromPackage(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int instanceId = stream.Read<int>();

        //if (null != player)
        //    player.GetItemPackage().RemoveItem(objID);
		ItemObject item = ItemMgr.Instance.Get(instanceId);
		if(item!=null){
			Pathea.PlayerPackageCmpt pkg = entity.GetCmpt<Pathea.PlayerPackageCmpt>();
			if (null != pkg)
				pkg.Remove(item);
		}
		if (IsOwner)
		{
			GameUI.Instance.mItemPackageCtrl.ResetItem();
		}
	}



	void RPC_S2C_PublicStorageIndex(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int tab = stream.Read<int>();
		int[] itemIDs = stream.Read<int[]>();

		SlotList itemList = GroupNetwork.GetSlotList((ItemPackage.ESlotType)tab);
		for (int i = 0; i < itemIDs.Length; i++)
		{
			if (-1 == itemIDs[i])
			{
				itemList[i] = null;
				continue;
			}

			ItemObject item = ItemMgr.Instance.Get(itemIDs[i]);
			itemList[i] = item;
		}

		//GameGui_N.Instance.mPublicInventoryGui.SetItempackage(GroupNetwork._Items);
	}

	void RPC_S2C_PublicStorageStore(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		/*string roleName = */stream.Read<string>();
		/*int objID = */stream.Read<int>();
	}

	void RPC_S2C_PublicStorageFetch(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		/*string roleName = */stream.Read<string>();
		/*int objID = */stream.Read<int>();
	}

	void RPC_S2C_PublicStorageDelete(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		/*string roleName = */stream.Read<string>();
		/*int objID = */stream.Read<int>();
	}

	void RPC_S2C_PublicStorageSplit(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		/*string roleName = */stream.Read<string>();
		/*int objID = */stream.Read<int>();
	}

	#region Personal Storage
	void RPC_S2C_PersonalStorageIndex(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int tab = stream.Read<int>();
		int[] ids = stream.Read<int[]>();

		NpcStorage storage = NpcStorageMgr.GetStorage(Id);
		if (null == storage)
			return;

		storage.Package.Clear((ItemPackage.ESlotType)tab);
		SlotList itemList = storage.Package.GetSlotList((ItemPackage.ESlotType)tab);

		for (int i = 0; i < ids.Length; i++)
		{
			ItemObject item = ItemMgr.Instance.Get(ids[i]);
			itemList[i] = item;
		}

		storage.Reset();
	}

	void RPC_S2C_PersonalStorageStore(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int objID = stream.Read<int>();
		int dstIndex = stream.Read<int>();

		ItemObject item = ItemMgr.Instance.Get(objID);
		if (null == item)
			return;

		NpcStorage storage = NpcStorageMgr.GetStorage(Id);
		if (null == storage)
			return;

		storage.Package.PutItem(item, dstIndex);
		storage.Reset();
	}

	void RPC_S2C_PersonalStorageDelete(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int objID = stream.Read<int>();
		ItemObject item = ItemMgr.Instance.Get(objID);
		if (null == item)
			return;

		NpcStorage storage = NpcStorageMgr.GetStorage(Id);
		if (null == storage)
			return;

		storage.Package.RemoveItem(item);
		storage.Reset();
	}

	void RPC_S2C_PersonalStorageFetch(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int objID = stream.Read<int>();
		/*int dstIndex = */stream.Read<int>();

		ItemObject item = ItemMgr.Instance.Get(objID);
		if (null == item)
			return;

		NpcStorage storage = NpcStorageMgr.GetStorage(Id);
		if (null == storage)
			return;

		storage.Package.RemoveItem(item);
		storage.Reset();
	}

	void RPC_S2C_PersonalStorageSplit(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int objID = stream.Read<int>();
		int dstIndex = stream.Read<int>();

		ItemObject item = ItemMgr.Instance.Get(objID);
		if (null == item)
			return;

		NpcStorage storage = NpcStorageMgr.GetStorage(Id);
		if (null == storage)
			return;

		storage.Package.PutItem(item, dstIndex);
		storage.Reset();
	}

	void RPC_S2C_PersonalStorageExchange(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int objID = stream.Read<int>();
		int dstIndex = stream.Read<int>();
		int dstObjID = stream.Read<int>();
		int srcIndex = stream.Read<int>();

		ItemObject item = ItemMgr.Instance.Get(objID);
		if (null == item)
			return;

		NpcStorage storage = NpcStorageMgr.GetStorage(Id);
		if (null == storage)
			return;

		ItemObject dstObj = ItemMgr.Instance.Get(dstObjID);
		if (null != dstObj)
			storage.Package.PutItem(dstObj, srcIndex);
		else
			storage.Package.RemoveItem(item);

		storage.Package.PutItem(item, dstIndex);

		storage.Reset();
	}

	void RPC_S2C_PersonalStorageSort(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int tabIndex = stream.Read<int>();
		int[] ids = stream.Read<int[]>();

		NpcStorage storage = NpcStorageMgr.GetStorage(Id);
		if (null == storage)
			return;

        storage.Package.Clear((ItemPackage.ESlotType)tabIndex);
		SlotList itemList = storage.Package.GetSlotList((ItemPackage.ESlotType) tabIndex);

		for (int i = 0; i < ids.Length; i++)
		{
			ItemObject item = ItemMgr.Instance.Get(ids[i]);
			itemList[i] = item;
		}

		storage.Reset();
	}
	#endregion

	#region Build Building
	public static void RPC_S2C_BuildBuildingBlock(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        //BuildingID buildingId;
        //Vector3 root;
        //int id;
        //int rotation;

        //stream.TryRead<BuildingID>(out buildingId);
        //stream.TryRead<Vector3>(out root);
        //stream.TryRead<int>(out id);
        //stream.TryRead<int>(out rotation);
        //if (VABuildingManager.Instance.mCreatedNpcItemBuildingIndex.ContainsKey(buildingId))
        //{
        //    return;
        //}


        //Vector3 rootSize;
        //List<Vector3> npcPositionList;
        //List<CreatItemInfo> itemInfoList;
        //Dictionary<int, int> npcIdNum;

        //Dictionary<IntVector3, B45Block> retBuild = BuildBlockManager.self.BuildBuilding(root, id, rotation, out rootSize, out npcPositionList, out itemInfoList, out npcIdNum);

        //foreach (IntVector3 index in retBuild.Keys)
        //{
        //    Block45Man.self.DataSource.SafeWrite(retBuild[index], index.x, index.y, index.z, 0);
        //    //LogManager.Debug("index: " + index);
        //    //LogManager.Debug("BlockType: " + retBuild[index].blockType);
        //    //LogManager.Debug("MaterialType: " + retBuild[index].materialType);
        //}
        //VABuildingManager.Instance.mCreatedNpcItemBuildingIndex.Add(buildingId, 0);
    }

	public static void RPC_S2C_SyncRailwayData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte [] data = stream.Read<byte[]> ();
		if(data.Length > 0)
			Railway.Manager.Instance.Import (data);
	}
    #endregion

	#endregion
}

