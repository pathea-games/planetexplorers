using UnityEngine;
using System.Collections.Generic;
using System;
using ItemAsset;

public class ChannelNetwork : NetworkInterface
{
    public const int RoomWorldId = 100;
    public const int PlayerWorldId = 101;
    public const int MinWorldId = 200;
    private static List<ChannelNetwork> Channels = new List<ChannelNetwork>();

    private int mChannelId;
    public int ChannelId { get { return mChannelId; } }

    public static ChannelNetwork CurChannel
    {
        get
        {
            return Channels.Find(iter => iter.ChannelId == (null != PlayerNetwork.mainPlayer ? PlayerNetwork.mainPlayer.WorldId : PlayerWorldId));
        }
    }

    protected override void OnPEInstantiate(uLink.NetworkMessageInfo info)
    {
        mChannelId = info.networkView.initialData.Read<int>();
        Channels.Add(this);
    }

    protected override void OnPEStart()
    {
        BindAction(EPacketType.PT_Common_BlockData, ChunkManager.RPC_S2C_BlockData);
        BindAction(EPacketType.PT_Common_VoxelData, ChunkManager.RPC_S2C_VoxelData);
        BindAction(EPacketType.PT_InGame_SkillBlockRange, ChunkManager.RPC_S2C_BlockDestroyInRange);
        BindAction(EPacketType.PT_InGame_SkillVoxelRange, ChunkManager.RPC_S2C_TerrainDestroyInRange);
        BindAction(EPacketType.PT_InGame_AttackArea, RPC_S2C_FlagAttacked);
        BindAction(EPacketType.PT_Common_NativeTowerDestroyed, VArtifactTownManager.RPC_S2C_NativeTowerDestroyed);
        BindAction(EPacketType.PT_InGame_ItemObjectList, RPC_S2C_ItemList);
        BindAction(EPacketType.PT_InGame_ItemObject, RPC_S2C_Item);
        BindAction(EPacketType.PT_InGame_BlockRedo, ChunkManager.RPC_S2C_BuildBlock);
        BindAction(EPacketType.PT_InGame_BlockUndo, ChunkManager.RPC_S2C_BuildBlock);
        BindAction(EPacketType.PT_InGame_SKDigTerrain, ChunkManager.RPC_SKDigTerrain);
        BindAction(EPacketType.PT_InGame_SKChangeTerrain, ChunkManager.RPC_SKChangeTerrain);
        BindAction(EPacketType.PT_InGame_BattleInfo, BattleManager.RPC_S2C_BattleInfo);
        BindAction(EPacketType.PT_InGame_BattleInfos, BattleManager.RPC_S2C_BattleInfos);
        BindAction(EPacketType.PT_InGame_BattleOver, BattleManager.RPC_S2C_BattleOver);
        BindAction(EPacketType.PT_InGame_Plant_UpdateInfo, RPC_S2C_Plant_UpdateInfo);
        BindAction(EPacketType.PT_InGame_Plant_UpdateInfoList, RPC_S2C_Plant_UpdateInfoList);
        BindAction(EPacketType.PT_InGame_TowerDefense, RPC_S2C_TowerDefenseComplete);
        BindAction(EPacketType.PT_InGame_WeaponDurability, RPC_S2C_WeaponDurability);
        

        BindAction(EPacketType.PT_CustomEvent_Death, RPC_S2C_Death);
        BindAction(EPacketType.PT_CustomEvent_Damage, RPC_S2C_Damage);
        BindAction(EPacketType.PT_CustomEvent_UseItem, RPC_S2C_UseItem);
        BindAction(EPacketType.PT_CustomEvent_PutoutItem, RPC_S2C_PutOutItem);

		//dungeon Entry
		BindAction(EPacketType.PT_InGame_RemoveDunEntrance,RPC_S2C_RemoveDunEntrance);

		//dun item
		BindAction(EPacketType.PT_InGame_RandomIsoObj,RPC_S2C_RandomIsoObj);

		//town destroy
		BindAction(EPacketType.PT_Common_TownDestroyed,RPC_S2C_TownDestroy);

    }

    protected override void OnPEDestroy()
    {
        Channels.Remove(this);
    }

    //public static void SyncChannel(int channelId, params object[] args)
    //{
    //    ChannelNetwork net = Channels.Find(iter => iter.ChannelId == channelId);
    //    if (null != net)
    //        net.RPCServer(args);
    //}
    //public static void SyncRoomChannel(params object[] args)
    //{
    //    ChannelNetwork net = Channels.Find(iter => iter.ChannelId == RoomWorldId);
    //    if (null != net)
    //        net.RPCServer(args);
    //}

    //public static void SyncPlayerChannel(params object[] args)
    //{
    //    ChannelNetwork net = Channels.Find(iter => iter.ChannelId == PlayerWorldId);
    //    if (null != net)
    //        net.RPCServer(args);
    //}

    //public static void Sync(string func, params object[] args)
    //{
    //    ChannelNetwork net = Channels.Find(iter => iter.ChannelId == PlayerWorldId);
    //    if (null != net)
    //        net.RPC(func, args);
    //}


    private void RPC_S2C_ItemList(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        stream.Read<ItemAsset.ItemObject[]>();
		Pathea.PeEntity mainplayer = Pathea.MainPlayer.Instance.entity;
		if(null != mainplayer)
			(mainplayer.packageCmpt as Pathea.PlayerPackageCmpt).NetOnItemUpdate();
    }

    private void RPC_S2C_Item(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
		ItemAsset.ItemObject item = stream.Read<ItemAsset.ItemObject>();
		Pathea.PeEntity mainplayer = Pathea.MainPlayer.Instance.entity;
		if(null != mainplayer)
			(mainplayer.packageCmpt as Pathea.PlayerPackageCmpt).NetOnItemUpdate(item);
    }

    void RPC_S2C_FlagAttacked(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        /*int flagId = */stream.Read<int>();
        /*int casterId = */stream.Read<int>();

        //lz-2016.10.31 Your flag is under attack!
        string msg = PELocalization.GetString(8000855)+" [" + System.DateTime.Now.ToLongTimeString() + "]";
        new PeTipMsg(msg, PeTipMsg.EMsgLevel.Warning);
    }

    private void RPC_S2C_Plant_UpdateInfo(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        FarmPlantLogic plant = stream.Read<FarmPlantLogic>();
        if (plant != null)
        {
            plant.UpdateInMultiMode();
        }
    }

    private void RPC_S2C_Plant_UpdateInfoList(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        FarmPlantLogic[] plants = stream.Read<FarmPlantLogic[]>();
        foreach (FarmPlantLogic p in plants)
        {
            if (p != null)
            {
                p.UpdateInMultiMode();
            }
        }
    }


    void RPC_S2C_TowerDefenseComplete(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        if (UITowerInfo.Instance != null)
            UITowerInfo.Instance.Hide();
    }
    void RPC_S2C_WeaponDurability(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        float hp = stream.Read<float>();
        int objId = stream.Read<int>();
        ItemObject item = ItemMgr.Instance.Get(objId);
        if(item != null)
        {
            Durability d = item.GetCmpt<Durability>();
            if(d != null)
                d.floatValue.current = hp;
        }
    }
    #region Custom Event
    void RPC_S2C_Death(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        stream.Read<int>();
        int scenarioId = stream.Read<int>();

        if (null != PlayerNetwork.mainPlayer)
            PlayerNetwork.mainPlayer.OnCustomDeath(scenarioId);
    }

    void RPC_S2C_Damage(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        stream.Read<int>();
        int scenarioId = stream.Read<int>();
        stream.Read<int>();
        int casterScenarioId = stream.Read<int>();
        float damage = stream.Read<float>();

        if (null != PlayerNetwork.mainPlayer)
            PlayerNetwork.mainPlayer.OnCustomDamage(scenarioId, casterScenarioId, damage);
    }

    void RPC_S2C_UseItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        stream.Read<int>();
        int scenarioId = stream.Read<int>();
        int itemInstanceId = stream.Read<int>();

        if (null != PlayerNetwork.mainPlayer)
            PlayerNetwork.mainPlayer.OnCustomUseItem(scenarioId, itemInstanceId);
    }

    void RPC_S2C_PutOutItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        stream.Read<int>();
        int scenarioId = stream.Read<int>();
        int itemInstanceId = stream.Read<int>();

        if (null != PlayerNetwork.mainPlayer)
            PlayerNetwork.mainPlayer.OnCustomPutoutItem(scenarioId, itemInstanceId);
    }

	void RPC_S2C_RemoveDunEntrance(uLink.BitStream stream, uLink.NetworkMessageInfo info){
		Vector3[] entranceList = stream.Read<Vector3[]>();

		foreach(Vector3 entrancePos in entranceList){
			RandomDungenMgr.Instance.DestroyEntrance(entrancePos);
		}
	}

	void RPC_S2C_RandomIsoObj(uLink.BitStream stream, uLink.NetworkMessageInfo info){
		Vector3 rioPos = stream.Read<Vector3>();
		int objInstanceId = stream.Read<int>();
		RandomItemObj rio = RandomItemMgr.Instance.GetRandomItemObj(rioPos);
		if(rio!=null){
			rio.AddRareInstance(objInstanceId);
			if (Application.isEditor) 
				Debug.LogError("<color=yellow>A Rare RandomItem is Ready!" + rioPos + " </color>");
		}
	}

	void RPC_S2C_TownDestroy(uLink.BitStream stream, uLink.NetworkMessageInfo info){
		int townId = stream.Read<int>();
		VArtifactTownManager.Instance.OnTownDestroyed(townId);
		VArtifactTownManager.Instance.SetCaptured(townId);
	}
    #endregion
}
