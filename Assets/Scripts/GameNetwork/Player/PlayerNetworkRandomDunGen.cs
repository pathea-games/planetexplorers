//------------------------------------------------------------------------------
// by Pugee
//------------------------------------------------------------------------------
using System;
using UnityEngine;
using Pathea;
using System.Linq;
using System.Collections.Generic;

public partial class PlayerNetwork 
{
	#region Network Request
	public void RequestEnterDungeon(Vector3 enterPos){
		RPCServer (EPacketType.PT_InGame_EnterDungeon,enterPos);
	}
	public void RequestExitDungeon(){
		RPCServer (EPacketType.PT_InGame_ExitDungeon);
	}
	public void RequestUploadDungeonSeed(Vector3 entrancePos,int seed){
		RPCServer (EPacketType.PT_InGame_UploadDungeonSeed,entrancePos,seed);
	}
	public void RequestGenDunEntrance(Vector3 entrancePos,int dungeonId){
		RPCServer (EPacketType.PT_InGame_GenDunEntrance,entrancePos,dungeonId);
	}
	#endregion

	#region Action Callback APIs
	void RPC_S2C_EnterDungeon(uLink.BitStream stream, uLink.NetworkMessageInfo info){
		bool success = stream.Read<bool>();
		if(!success){
			MessageBox_N.CancelMask(MsgInfoType.DungeonGeneratingMask);
			return;
		}
		Vector3 genPlayerPos = stream.Read<Vector3>();
		int seed = stream.Read<int>();
		int dungeonId = stream.Read<int>();
		int dungeonDataId = stream.Read<int>();
		RandomDungenMgr.Instance.LoadDataFromId(dungeonDataId);
		//UILoadScenceEffect.Instance.EnableProgress(true);
		RandomDungenMgrData.DungeonId = dungeonId;
		RandomDungenMgrData.SetPosByGenPlayerPos(genPlayerPos);
		int failCount=0;
		while(!RandomDungenMgr.Instance.GenDungeon(seed)){
			failCount++;
			Debug.Log("generation failed: "+failCount);
		}

        RandomDungenMgr.Instance.LoadPathFinding();
		RequestFastTravel(0,RandomDungenMgrData.revivePos,0);
//		FastTravel.TravelTo(RandomDungenMgrData.revivePos);
	}
	void RPC_S2C_ExitDungeon(uLink.BitStream stream, uLink.NetworkMessageInfo info){
		
		//UILoadScenceEffect.Instance.EnableProgress(true);
		RequestFastTravel(0,RandomDungenMgrData.enterPos,0);
//		FastTravel.TravelTo(RandomDungenMgrData.enterPos);
	}
	void RPC_S2C_GenDunEntrance(uLink.BitStream stream, uLink.NetworkMessageInfo info){
		Vector3 entrancePos = stream.Read<Vector3>();
		int id = stream.Read<int>();
		DungeonBaseData dbd = RandomDungeonDataBase.GetDataFromId(id);
		RandomDungenMgr.Instance.GenDunEntrance(entrancePos,dbd);
	}

	void RPC_S2C_GenDunEntranceList(uLink.BitStream stream, uLink.NetworkMessageInfo info){
		List<Vector3> entrancePosList = stream.Read<Vector3[]>().ToList();
		List<int> idList = stream.Read<int[]>().ToList();
		for(int i=0;i<entrancePosList.Count;i++){
			DungeonBaseData dbd = RandomDungeonDataBase.GetDataFromId(idList[i]);
			RandomDungenMgr.Instance.GenDunEntrance(entrancePosList[i],dbd);
		}
	}
	void RPC_S2C_InitWhenSpawn(uLink.BitStream stream, uLink.NetworkMessageInfo info){
		if(PeGameMgr.randomMap){
			if(!RandomDungenMgrData.InDungeon)
				RandomMapConfig.SetGlobalFogHeight();
		}
	}
	#endregion
}

