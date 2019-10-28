using UnityEngine;
using System.Collections;
using uLink;
using System.Collections.Generic;
using System;
using TownData;
using System.Linq;
using Random = UnityEngine.Random;
using ItemAsset;
using Pathea;


public enum DoodadType
{
	DoodadType_None = 0,
	DoodadType_Drop = 1,
	DoodadType_Dead = 2,
	DoodadType_SceneBox = 3,
	DoodadType_SceneItem = 4,
	DoodadType_Repair = 5,
	DoodadType_Power = 6,
	DoodadType_RandomBuilding = 7,
	DoodadType_RandomBuilding_Repair = 8,
	DoodadType_RandomBuilding_Power = 9
}

public class MapObjNetwork : SkNetworkInterface {

	protected DoodadType objType;
	internal DoodadType ObjType { get { return objType; } }

    protected int _assetId;
    public int AssetId { get { return _assetId; } }

    protected int _protoTypeId;
    protected int _campId;
    public int CampId { get { return _campId; } }

    protected int _damageId;
    public int DamageId { get { return _damageId; } }

	protected int _dPlayerId;
	public int DPlayerId { get { return _dPlayerId; } }

	public int aimId { get; protected set; }

	protected uLink.NetworkView playerView;
	internal uLink.NetworkView PlayerView { get { return playerView; } }

    //private Player owner;
	private List<int> itemList = new List<int>();
	
	ItemBox itemBox;
	public WareHouseObject wareHouseObj;
	ItemDrop itemDrop;

    AiTowerSyncData _syncData;
    public bool isTower { get; private set; }

    PeEntity entity;
    string _sceneItemName;

    public static List<MapObjNetwork> mapObjNetworkMgr = new List<MapObjNetwork>();
    private static Transform ParentTrans;

    int townId;

	public static MapObjNetwork GetNet(int objId,int type)
	{
		if(objId != -1)
		{
			foreach(var iter in mapObjNetworkMgr)
			{
				if(iter._assetId == objId && (int)iter.objType == type)
					return iter;
			}
		}
		return null;
	}

	public static MapObjNetwork GetNet(int entityId)
	{
		foreach(var iter in mapObjNetworkMgr)
		{
			if(iter.Id == entityId)
			{
				return iter;
			}
		}
		return null;
	}

    public static MapObjNetwork GetNet(string objName)
    {
        if (objName.Length == 0)
            return null;
        foreach (var iter in mapObjNetworkMgr)
        {
            if (iter._sceneItemName == objName)
            {
                return iter;
            }
        }
        return null;
    }

    public static bool HadCreate(int boxid,int type)
	{
		if(boxid != -1)
		{
			foreach(var iter in mapObjNetworkMgr)
			{
				if(iter._assetId == boxid && (int)iter.objType == type)
					return true;
			}
		}
		return false;
	}

	protected override void OnPEStart ()
	{
		BindSkAction();
		BindAction(EPacketType.PT_MO_RequestItemList, RPC_S2C_RequestItemList);
		BindAction(EPacketType.PT_MO_ModifyItemList, RPC_S2C_ModifyItemList);
		BindAction(EPacketType.PT_MO_RemoveItem, RPC_S2C_RemoveItem);
		BindAction(EPacketType.PT_CL_SyncCreationHP, RPC_S2C_SyncCreationHP);
		BindAction(EPacketType.PT_MO_StartRepair, RPC_S2C_Repair);
        BindAction(EPacketType.PT_MO_StopRepair, RPC_S2C_StopRepair);
        BindAction(EPacketType.PT_MO_SyncRepairTime, RPC_S2C_SyncRepairTime);
        BindAction(EPacketType.PT_CL_SyncCreationFuel, RPC_S2C_SyncCreationFuel);
        BindAction(EPacketType.PT_Common_ScenarioId, RPC_S2C_ScenarioId);
        BindAction(EPacketType.PT_InGame_SetController, RPC_S2C_SetController);
        BindAction(EPacketType.PT_InGame_LostController, RPC_S2C_LostController);
        BindAction(EPacketType.PT_Tower_Target, RPC_Tower_Target);
        BindAction(EPacketType.PT_Tower_Fire, RPC_S2C_Fire);
		BindAction(EPacketType.PT_Tower_AimPosition, RPC_S2C_AimPosition);
		BindAction(EPacketType.PT_Tower_LostEnemy, RPC_S2C_LostEnemy);
		BindAction(EPacketType.PT_InGame_InitData, RPC_S2C_InitData);
	}

    protected override void OnPEDestroy()
    {
        base.OnPEDestroy();

        if (null != itemBox && null != ItemBoxMgr.Instance)
            ItemBoxMgr.Instance.RemoveItemMultiPlay(itemBox.mID);

        mapObjNetworkMgr.Remove(this);
        if (_sceneItemName == "1_larve_Q425")
        {
            GameObject goSpe = GameObject.Find("larve_Q425(Clone)");
            if (goSpe == null)
                return;

            Destroy(goSpe);
        }
        else if (_sceneItemName == "backpack")
        {
            GameObject go = GameObject.Find("backpack");
            if (go == null)
                return;

            Destroy(go);
        }
        else if (_sceneItemName == "Fruit_pack_1")
        {
            GameObject go = GameObject.Find("fruitpack");
            if (go == null)
                return;

            Destroy(go);
        }
        else if(_sceneItemName != null && _sceneItemName.Contains("language_sample_canUse(Clone):"))
        {
            GameObject go = GameObject.Find(_sceneItemName);
            if (go == null)
                return;

            Destroy(go);
        }
    }

    protected override void ResetContorller()
    {
		base.ResetContorller();

        if (hasOwnerAuth)
        {
			if (null != entity && null != entity.Tower && isTower)
				StartCoroutine(SyncMove());
        }
    }

    public override void InitForceData()
    {
        if (null != entity && isTower)
        {
            entity.SetAttribute(AttribType.CampID, CampId);
            entity.SetAttribute(AttribType.DamageID, DamageId);
        }
    }

    protected IEnumerator SyncMove()
    {
		_pos = transform.position;
		rot = transform.rotation;

		while (hasOwnerAuth)
        {
            if (null != entity && null != entity.Tower)
            {
                if (entity.Tower.ChassisY != _syncData.ChassisY || !entity.Tower.PitchEuler.Equals(_syncData.PitchEuler))
                {
                    _syncData.ChassisY = entity.Tower.ChassisY;
                    _syncData.PitchEuler = entity.Tower.PitchEuler;
                    //URPCServer(EPacketType.PT_Tower_Target, _syncData.ChassisY, _syncData.PitchEuler);
                }
            }

            yield return new WaitForSeconds(1 / uLink.Network.sendRate);
        }
    }

	IEnumerator WaitForTarget()
	{
		yield return new WaitForSeconds(3f);
		int oldId = -1;
		while (true)
		{
			if (oldId != aimId)
			{
				PeEntity tarEntity = EntityMgr.Instance.Get(aimId);
				if (null != tarEntity && tarEntity.hasView && null != entity && null != entity.Tower)
				{
					entity.Tower.Target = tarEntity.centerBone;
					oldId = aimId;
				}
			}

			yield return new WaitForSeconds(1f);
		}
	}

	public void AddToItemlist(int objID)
	{
		if(objID <= 0) return;
		itemList.Add (objID);
	}
	protected override void OnPEInstantiate(uLink.NetworkMessageInfo info)
	{
		objType = (DoodadType)info.networkView.initialData.Read<int>();
		int playerId = info.networkView.initialData.Read<int>();
		_teamId = info.networkView.initialData.Read<int>();
		_assetId = info.networkView.initialData.Read<int>();
		_id		   = info.networkView.initialData.Read<int>();
		_protoTypeId	= info.networkView.initialData.Read<int>();
		Vector3 scale = info.networkView.initialData.Read<Vector3>();
		string param = info.networkView.initialData.Read<string>();
		SceneDoodadDesc doodad = null;

		_pos = transform.position;
		rot = transform.rotation;

		if (null == ParentTrans)
			ParentTrans = new GameObject("DoodadNetworkMgr").transform;

		transform.parent = ParentTrans;

		if (PeGameMgr.IsMultiStory)
            doodad = StoryDoodadMap.Get(_assetId);

		if (objType == DoodadType.DoodadType_Drop || objType == DoodadType.DoodadType_Dead)
		{
			itemBox = ItemBoxMgr.Instance.AddItemMultiPlay(OwnerView.viewID.id, playerId, _pos, this);
		}
		else if (objType == DoodadType.DoodadType_SceneBox)
		{
			if (doodad != null)
				entity = DoodadEntityCreator.CreateStoryDoodadNet(_assetId, Id);
			else
				entity = DoodadEntityCreator.CreateDoodadNet(_protoTypeId, _pos, scale, rot, Id);

			if (entity != null)
			{
				WareHouseObject script = entity.gameObject.GetComponent<WareHouseObject>();
				if (script != null)
					script._id = _assetId;
			}
		}
		else if (objType == DoodadType.DoodadType_SceneItem)
		{
			string[] str = param.Split('|');
			if (str.Length != 2)
				return;

			_sceneItemName = str[1];
			if (_sceneItemName == "ash_box")
				itemBox = ItemBoxMgr.Instance.AddItemMultiPlay(OwnerView.viewID.id, _assetId, _pos, this);
			else if (_sceneItemName == "ash_ball")
				itemBox = ItemBoxMgr.Instance.AddItemMultiPlay(OwnerView.viewID.id, _assetId, _pos, this);
			else
				RequestItemList();
		}
		else if (objType == DoodadType.DoodadType_Repair)
		{
			if (doodad != null)
				entity = DoodadEntityCreator.CreateStoryDoodadNet(_assetId, Id);
			else
				entity = DoodadEntityCreator.CreateDoodadNet(_protoTypeId, _pos, scale, rot, Id);
		}
		else if (objType == DoodadType.DoodadType_Power)
		{
			if (doodad != null)
				entity = DoodadEntityCreator.CreateStoryDoodadNet(_assetId, Id);
			else
				entity = DoodadEntityCreator.CreateDoodadNet(_protoTypeId, _pos, scale, rot, Id);
		}
		else if (objType == DoodadType.DoodadType_RandomBuilding
				|| objType == DoodadType.DoodadType_RandomBuilding_Repair
				|| objType == DoodadType.DoodadType_RandomBuilding_Power)
		{
			ExtractParam(param, out townId, out _campId, out _damageId, out _dPlayerId);
			if (doodad != null)
				entity = DoodadEntityCreator.CreateStoryDoodadNet(_assetId, Id);
			else
				entity = DoodadEntityCreator.CreateNetRandTerDoodad(Id, _protoTypeId, _pos, scale, rot, townId, _campId, _damageId, _dPlayerId);
		}
		else
		{
			if (doodad != null)
				entity = DoodadEntityCreator.CreateStoryDoodadNet(_assetId, Id);
			else
				entity = DoodadEntityCreator.CreateDoodadNet(_protoTypeId, _pos, scale, rot, Id);
		}

		if(entity != null)
			OnSpawned(entity.gameObject);

		mapObjNetworkMgr.Add(this);
        gameObject.name = string.Format("Mapobj assetId:{0}, protoTypeId:{1}, objType:{2}, entityId:{3}", _assetId, _protoTypeId, objType, _id);
    }
	/// <summary>
	/// DoodadType.DoodadType_RandomBuilding--Packs the parameter.
	/// </summary>
	/// <returns>The parameter.</returns>
	/// <param name="townId">Town identifier.</param>
	/// <param name="campId">Camp identifier.</param>
	/// <param name="damageId">Damage identifier.</param>
	public static string PackParam(int townId,int campId,int damageId,int dPlayerId){
		return townId.ToString()+","+campId.ToString()+","+damageId.ToString()+","+dPlayerId.ToString();
	}
	/// <summary>
	/// DoodadType.DoodadType_RandomBuilding--Extracts the parameter.
	/// </summary>
	/// <param name="param">Parameter.</param>
	/// <param name="townId">Town identifier.</param>
	/// <param name="campId">Camp identifier.</param>
	/// <param name="damageId">Damage identifier.</param>
	public static void ExtractParam(string param,out int townId,out int campId,out int damageId,out int dPlayerId){
		//List<int> result = new List<int> ();
		string[] strArry  = param.Split(',');
		if(strArry.Length!=4)
			Debug.LogError("doodadRandomBuilding param error: "+param);
		townId = Convert.ToInt32(strArry[0]);
		campId = Convert.ToInt32(strArry[1]);
		damageId = Convert.ToInt32(strArry[2]);
		dPlayerId = Convert.ToInt32(strArry[3]);
	}

    public override void OnSpawned(GameObject obj)
    {
        base.OnSpawned(obj);

		StartCoroutine(AuthorityCheckCoroutine());

		if (null == entity.netCmpt)
            entity.netCmpt = entity.Add<NetCmpt>();

        entity.netCmpt.network = this;

		RPCServer(EPacketType.PT_InGame_InitData);
	}

    //public override void OnPeMsg(EMsg msg, params object[] args)
    //{
    //    switch (msg)
    //    {
    //        case EMsg.Net_Instantiate:
    //            {
    //                _campId = (int)args[0];
    //                _damageId = (int)args[1];
    //                StartCoroutine(AuthorityCheckCoroutine());
    //                isTower = true;
    //            }
    //            break;
    //    }
    //}

    public void GetAllItem()
	{
		RPCServer (EPacketType.PT_MO_GetAllItem);
	}
	public void GetItem(int itemID)
	{
		if(itemID <= 0) return;
		RPCServer (EPacketType.PT_MO_GetItem,itemID);
	}
	public void RequestItemList()
	{
		RPCServer (EPacketType.PT_MO_RequestItemList);
	}
	public void InsertItemList(int [] itemlist)
	{
		if( itemlist.Count() == 0 )
			return;
		RPCServer (EPacketType.PT_MO_InsertItemList,itemlist.ToArray());
	}
	public void InsertItemList(int itemId,int index)
	{
		RPCServer (EPacketType.PT_MO_InsertItemList,itemId,index);
	}
	public void RequestRepair(int itemObj)
	{
		RPCServer(EPacketType.PT_MO_StartRepair,itemObj);
	}
    public void RequestStopRepair(int itemObj)
    {
        RPCServer(EPacketType.PT_MO_StopRepair, itemObj);
    }
    public void RequestRepairTime()
    {
        RPCServer(EPacketType.PT_MO_SyncRepairTime);
    }
    public void RequestInitData()
    {
        RPCServer(EPacketType.PT_Common_ScenarioId);
    }

	public void RequestAimTarget(int skEntityId)
	{
		RPCServer(EPacketType.PT_Tower_AimPosition, skEntityId);
	}

	public void RequestFire(int skEntityId)
    {
        RPCServer(EPacketType.PT_Tower_Fire, skEntityId);
    }

	public void RequestEnemyLost()
	{
		RPCServer(EPacketType.PT_Tower_LostEnemy);
	}
    #region RPC
    public void RPC_S2C_RequestItemList(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int[] item = stream.Read<int[]>();
		itemList.Clear ();
		foreach(int itemid in item)
		{
			itemList.Add(itemid);
		}
		if(objType == DoodadType.DoodadType_Dead || objType == DoodadType.DoodadType_Drop)
			itemBox.OnRequestItemList(itemList);
		else if(objType == DoodadType.DoodadType_SceneBox)
		{
			//if(wareHouseObj == null)
			//	wareHouseObj = WareHouseManager.GetWareHouseObject(_assetId);
			//if(wareHouseObj != null && wareHouseObj.ItemPak == null)
			//{
			//	wareHouseObj.InitForNet( this );
			//}
			if(wareHouseObj == null)
            {
                wareHouseObj = WareHouseManager.GetWareHouseObject(_assetId);
                if (wareHouseObj != null)
                {
                    wareHouseObj.InitForNet(this);
                }
            }

            if (wareHouseObj != null)
				wareHouseObj.ResetItemByIdList(itemList);
            
		}
		else if(objType == DoodadType.DoodadType_SceneItem)
		{
			if(_sceneItemName == "backpack")
			{
				if(itemList.Count >0)
					itemDrop = StroyManager.CreateBackpack(transform.position,itemList,this);
			}
			else if(_sceneItemName == "pajaLanguage")
			{
				if(itemList.Count >0)
					itemDrop = StroyManager.CreatePajaLanguage(transform.position,itemList,this);
			}
			else if(_sceneItemName == "probe")
			{
				if(itemList.Count >0)
					itemDrop = StroyManager.CreateProbe(transform.position,itemList,this);
			}
			else if(_sceneItemName == "hugefish_bone")
			{
				if(itemList.Count >0)
					itemDrop = StroyManager.CreateHugefish_bone(transform.position,itemList,this);
			}
			else if(_sceneItemName == "1_larve_Q425")
			{
				itemDrop = StroyManager.Createlarve_Q425(transform.position);
			}
			else if(_sceneItemName == "ash_box")
			{
				itemBox.OnRequestItemList(itemList, true);

			}
			else if(_sceneItemName == "ash_ball")
			{
				itemBox.OnRequestItemList(itemList,true);
			}
            else if(_sceneItemName.Contains("language_sample_canUse(Clone):"))
            {
                if(itemList.Count > 0)
                {
                    itemDrop = StroyManager.CreateLanguageSampleNet(_sceneItemName,transform.position, itemList, this);
                }
            }
            else if(_sceneItemName.Contains("coelodonta_rhino_bone"))
            {
                if (itemList.Count > 0)
                {
                    itemDrop = StroyManager.CreateAndHeraNest_indexNet(_sceneItemName, transform.position, itemList, this);
                }
            }
            else if (_sceneItemName.Contains("lepus_hare_bone"))
            {
                if (itemList.Count > 0)
                {
                    itemDrop = StroyManager.CreateAndHeraNest_indexNet(_sceneItemName, transform.position, itemList, this);
                }
            }
            else if (_sceneItemName.Contains("andhera_queen_egg"))
            {
                if (itemList.Count > 0)
                {
                    itemDrop = StroyManager.CreateAndHeraNest_indexNet(_sceneItemName, transform.position, itemList, this);
                }
            }
        }
		else if(objType == DoodadType.DoodadType_Repair||objType==DoodadType.DoodadType_RandomBuilding_Repair)
		{
			if(itemList.Count > 0)
				GameUI.Instance.mRepair.SetItemByNet(this,itemList[0]);
			else
				GameUI.Instance.mRepair.SetItemByNet(this,-1);
		}
		else if(objType == DoodadType.DoodadType_Power||objType==DoodadType.DoodadType_RandomBuilding_Power)
		{
			GameUI.Instance.mPowerPlantSolar.OnMultiOpenDropCallBack(this,itemList.ToArray());
		}
	}
	public void RPC_S2C_ModifyItemList(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int[] item = stream.Read<int[]>();
		itemList.Clear ();
		foreach(int itemid in item)
		{
			itemList.Add(itemid);
		}
		if(objType == DoodadType.DoodadType_Dead || objType == DoodadType.DoodadType_Drop)
			itemBox.ResetItem (itemList);
		else if(objType == DoodadType.DoodadType_SceneBox)
		{
			wareHouseObj.ResetItemByIdList(itemList);
		}
		else if(objType == DoodadType.DoodadType_SceneItem)
		{
			if(itemDrop != null)
			{
				itemDrop.RemoveDroppableItemAll();
				foreach(int itemid in item)
				{
					ItemObject itemObj = ItemAsset.ItemMgr.Instance.Get(itemid);
					if(itemObj != null)
						itemDrop.AddItem(itemObj);
				}
			}
		}
		else if(objType == DoodadType.DoodadType_Repair||objType==DoodadType.DoodadType_RandomBuilding_Repair)
		{
			if(itemList.Count > 0)
				GameUI.Instance.mRepair.DropItemByNet(this,itemList[0]);
		}
		else if(objType == DoodadType.DoodadType_Power||objType==DoodadType.DoodadType_RandomBuilding_Power)
		{
			GameUI.Instance.mPowerPlantSolar.OnMultiOpenDropCallBack(this,itemList.ToArray());
		}
	}

	public void RPC_S2C_RemoveItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int itemID = stream.Read<int>();
		int index = itemList.FindIndex (iter=> (iter == itemID) );
		if ( index >= 0 && objType != DoodadType.DoodadType_Power&&objType!=DoodadType.DoodadType_RandomBuilding_Power)
			itemList.RemoveAt( index);
		if(objType == DoodadType.DoodadType_Dead || objType == DoodadType.DoodadType_Drop)
			itemBox.RemoveItem(itemID);
		else if(objType == DoodadType.DoodadType_SceneBox)
		{
            if (wareHouseObj == null)
            {
                wareHouseObj = WareHouseManager.GetWareHouseObject(_assetId);
                if (wareHouseObj != null)
                {
                    wareHouseObj.InitForNet(this);
                }
            }
            if (wareHouseObj != null)
                wareHouseObj.RemoveItemById(itemID);
            else
                Debug.LogError("warehouse is null！！！");
		}
		else if(objType == DoodadType.DoodadType_SceneItem)
		{
			if(itemDrop != null)
			{
				ItemObject itemObj = ItemAsset.ItemMgr.Instance.Get(itemID);
				if(itemObj != null)
					itemDrop.RemoveDroppableItem(itemObj);
			}
		}
		else if(objType == DoodadType.DoodadType_Repair||objType==DoodadType.DoodadType_RandomBuilding_Repair)
		{
			GameUI.Instance.mRepair.DropItemByNet(this,-1);
		}
		else if(objType == DoodadType.DoodadType_Power||objType==DoodadType.DoodadType_RandomBuilding_Power)
		{
			if(itemList.Count > index && index >=0 )
			{
				itemList[index] = -1;
				GameUI.Instance.mPowerPlantSolar.OnMultiRemoveCallBack(this,index,itemID);
			}
		}
	}
	void RPC_S2C_Repair(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if(objType == DoodadType.DoodadType_Repair||objType==DoodadType.DoodadType_RandomBuilding_Repair)
		{
			GameUI.Instance.mRepair.UpdateItemForNet(this);
		}
	}
    void RPC_S2C_StopRepair(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int objId = stream.Read<int>();
        if (objType == DoodadType.DoodadType_Repair||objType==DoodadType.DoodadType_RandomBuilding_Repair)
        {
            GameUI.Instance.mRepair.ResetItemByNet(this, objId);
        }
    }
    void RPC_S2C_SyncRepairTime(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        float curTime = stream.Read<float>();
        float totalTime = stream.Read<float>();
        if (objType == DoodadType.DoodadType_Repair||objType==DoodadType.DoodadType_RandomBuilding_Repair)
        {
            GameUI.Instance.mRepair.SetCounterByNet(this, curTime, totalTime);
        }
    }
    void RPC_S2C_SyncCreationHP(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int objId = stream.Read<int>();
		float hp = stream.Read<float>();
		
		var itemObject = ItemAsset.ItemMgr.Instance.Get(objId);
		var lifeCmpt = itemObject.GetCmpt<ItemAsset.LifeLimit>();
		lifeCmpt.floatValue.current = hp;
	}
	void RPC_S2C_SyncCreationFuel(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int objId = stream.Read<int>();
		float fuel = stream.Read<float>();
		
		var itemObject = ItemAsset.ItemMgr.Instance.Get(objId);
		var energyCmpt = itemObject.GetCmpt<ItemAsset.Energy>();
		energyCmpt.floatValue.current = fuel;
	}
	
    void RPC_S2C_ScenarioId(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int scenarioId = stream.Read<int>();

        if (null != entity)
            entity.scenarioId = scenarioId;
    }

    protected override void RPC_S2C_SetController(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
		authId = stream.Read<int>();
        ResetContorller();

//#if UNITY_EDITOR
//		PlayerNetwork p = PlayerNetwork.GetPlayer(authId);
//		if (null != p)
//			Debug.LogFormat("<color=blue>{0} got [{1}]'s authority.</color>", p.RoleName, Id);
//#endif
	}

	protected override void RPC_S2C_LostController(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
		authId = -1;
        ResetContorller();

		if (canGetAuth)
			RPCServer(EPacketType.PT_InGame_SetController);
	}

    protected void RPC_Tower_Target(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        if (hasOwnerAuth)
            return;

        if (null == entity || null == entity.Tower)
            return;

        float rotY;
        Vector3 pitchEuler;
        if (stream.TryRead<float>(out rotY) && stream.TryRead<Vector3>(out pitchEuler))
        {
            entity.Tower.ApplyChassis(rotY);
            entity.Tower.ApplyPitchEuler(pitchEuler);
        }
    }

	void RPC_S2C_AimPosition(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (hasOwnerAuth)
			return;

		aimId = stream.Read<int>();
		PeEntity tarEntity = EntityMgr.Instance.Get(aimId);
		if (null == tarEntity || tarEntity.skEntity == null)
			return;

		if (null != entity && null != entity.Tower)
			entity.Tower.Target = tarEntity.centerBone;
	}

	void RPC_S2C_LostEnemy(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (null != entity && null != entity.Tower)
			entity.Tower.Target = null;
	}

	void RPC_S2C_InitData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		aimId = stream.Read<int>();
		StartCoroutine(WaitForTarget());
	}

	void RPC_S2C_Fire(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        if (hasOwnerAuth)
            return;

        int skEntityId = stream.Read<int>();
        PeEntity tarEntity = EntityMgr.Instance.Get(skEntityId);
        if (null == tarEntity || tarEntity.skEntity == null)
            return;

        if (null != entity && null != entity.Tower)
            entity.Tower.Fire(tarEntity.skEntity);
    }
    #endregion
}
