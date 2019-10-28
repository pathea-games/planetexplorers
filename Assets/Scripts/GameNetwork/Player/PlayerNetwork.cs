using CustomData;
using ItemAsset;
using Pathea;
using SkillAsset;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TownData;
using UnityEngine;
using WhiteCat;
using SkillSystem;
using Pathea.Operate;

public class PlayerSynAttribute
{
    public Vector3 mv3Postion;
    public Vector3 mv3shootTarget;
    public float mfRotationY;
    public SpeedState mnPlayerState;
    public bool mbGrounded;
    public bool mbJumpFlag;
    public const float SyncMovePrecision = 0.1f;
}

public partial class PlayerNetwork : SkNetworkInterface, INetworkEvent
{
	#region Static Variables
	public static event Action OnTeamChangedEventHandler;
    static bool isTerrainDataOk;

	static Dictionary<int, Bounds> LimitBounds = new Dictionary<int, Bounds>();
	static List<PlayerNetwork> PlayerList = new List<PlayerNetwork>();
	static Dictionary<int, PlayerNetwork> PlayerDic = new Dictionary<int, PlayerNetwork>();
    #endregion Static Variables

    #region Static Properties

    public static int mainPlayerId { get; private set; }
    public static PlayerNetwork mainPlayer { get; private set; }
	public int BaseTeamId{
		get{return BaseNetwork.MainPlayer.TeamId;}
	}

    public bool _initOk = false;
    public bool _gameStarted = false;

    #endregion Static Properties

    #region Variables

    private BaseNetwork networkBase;
    private PeEntity entity;
    private PeTrans _transCmpt;
    public PeEntity PlayerEntity
    {
        get
        {
            return entity;
        }
    }

    public Vector3 PlayerPos
    {
        get
        {
            return _transCmpt.position;
        }
    }

    private PlayerSynAttribute mPlayerSynAttribute = new PlayerSynAttribute();

    PlayerArmorCmpt _playerArmor;

    public PlayerArmorCmpt PlayerArmor
    {
        get
        {
            if (_playerArmor == null)
                _playerArmor = entity.GetCmpt<PlayerArmorCmpt>();
            return _playerArmor;
        }
    }

    public int _curSceneId ;

    #endregion Variables

    #region Static Functions
    public static bool IsOnline(int id)
    {
        return Get(id) is PlayerNetwork;
    }

    public static void ResetTerrainState()
    {
        isTerrainDataOk = false;
    }

    static void OnTeamChanged(int teamId)
    {
		if (null == mainPlayer)
			return;

		PlayerAction((p) =>
		{
			if (ReferenceEquals(p, mainPlayer))
				return;

			if (null != p.PlayerEntity)
			{
				p.PlayerEntity.SendMsg(EMsg.Net_Destroy);

				EntityInfoCmpt cmpt = p.PlayerEntity.GetCmpt<EntityInfoCmpt>();
				if (null != cmpt)
				{
					if (p.TeamId == mainPlayer.TeamId)
						cmpt.mapIcon = PeMap.MapIcon.AllyPlayer;
					else
						cmpt.mapIcon = PeMap.MapIcon.OppositePlayer;

					p.PlayerEntity.SendMsg(EMsg.Net_Instantiate);
				}
			}
		});

		if (teamId == mainPlayer.TeamId)
		{
			if (null != OnTeamChangedEventHandler)
				OnTeamChangedEventHandler();
		}
	}

	public static void OnLimitBoundsAdd(int id, Bounds areaBounds)
	{
		if (!LimitBounds.ContainsKey(id))
			LimitBounds.Add(id, areaBounds);
	}

	public static void OnLimitBoundsDel(int id)
	{
		if (LimitBounds.ContainsKey(id))
			LimitBounds.Remove(id);
	}

	public static bool OnLimitBoundsCheck(Bounds targetBounds)
	{
		return LimitBounds.All((iter) =>
		{
			if (targetBounds.Intersects(iter.Value))
			{
				NetworkInterface net = NetworkInterface.Get(iter.Key);
				if (null == net)
					return true;

				if (net.TeamId == mainPlayer.originTeamId)
					return false;
			}

			return true;
		});
	}
    #endregion

    #region Properties

    public string RoleName { get; private set; }
    public byte Sex { get; private set; }
	public int originTeamId { get; private set; }
    public int colorIndex { get; private set; }
    internal CreationNetwork DriveCreation { get; set; }
    internal EVCComponent SeatType { get; set; }
    internal int SeatIndex { get; set; }
	public bool isOriginTeam { get { return originTeamId == TeamId; } }

	#endregion Properties

	#region Event
	public event Action<NetworkInterface> OnPrefabViewBuildEvent;
	public event Action<NetworkInterface> OnPrefabViewDestroyEvent;
	#endregion

	#region Inherited APIs

	protected override void OnPEInstantiate(uLink.NetworkMessageInfo info)
    {
		_curSceneId = -1;
        _id = info.networkView.initialData.Read<int>();
        _worldId = info.networkView.initialData.Read<int>();
        _teamId = info.networkView.initialData.Read<int>();

        networkBase = BaseNetwork.GetBaseNetwork(Id);
        if (null == networkBase)
        {
			Debug.LogErrorFormat("NetworkBase is null.id:{0}", Id);
            return;
        }

		originTeamId = networkBase.TeamId;
		colorIndex = networkBase.ColorIndex;
		RoleName = networkBase.RoleName;
        Sex = networkBase.Sex;
		authId = Id;
        name = RoleName + "_" + Id;
        AddPlayer();

        if (IsOwner)
        {
			mainPlayerId = Id;
			mainPlayer = this;
        }
    }

    protected override void OnPEStart()
    {
        BindSkAction();
        BindAction(EPacketType.PT_InGame_TerrainDataOk, RPC_S2C_TerrainDataOk);
        BindAction(EPacketType.PT_InGame_RequestInit, RPC_S2C_RequestInit);
        BindAction(EPacketType.PT_InGame_InitStatus, RPC_S2C_InitStatus);
        BindAction(EPacketType.PT_Common_InitAdminData, RPC_S2C_InitAdminData);
        BindAction(EPacketType.PT_InGame_InitPackage, RPC_S2C_InitPackage);
        BindAction(EPacketType.PT_InGame_PackageIndex, RPC_S2C_PackageIndex);
        BindAction(EPacketType.PT_InGame_InitShortcut, RPC_S2C_InitShortcut);
        BindAction(EPacketType.PT_InGame_InitLearntSkills, RPC_S2C_InitLearntSkills);
        BindAction(EPacketType.PT_InGame_PlayerMoney, RPC_S2C_PlayerMoney);
        BindAction(EPacketType.PT_InGame_CurSceneId, RPC_S2C_CurSceneId);
        BindAction(EPacketType.PT_InGame_MoneyType, RPC_S2C_MoneyType);
        BindAction(EPacketType.PT_InGame_MissionPackageIndex, RPC_S2C_MissionPackageIndex);

        BindAction(EPacketType.PT_InGame_RailwayData, RPC_S2C_SyncRailwayData);
        BindAction(EPacketType.PT_InGame_FarmInfo, RPC_S2C_FarmInfo);
        BindAction(EPacketType.PT_InGame_PlayerBattleInfo, RPC_S2C_PlayerBattleInfo);
        BindAction(EPacketType.PT_InGame_EquipedItem, RPC_S2C_EquipedItems);
        BindAction(EPacketType.PT_InGame_PutOnEquipment, RPC_S2C_PutOnEquipment);
        BindAction(EPacketType.PT_InGame_TakeOffEquipment, RPC_S2C_TakeOffEquipment);
        BindAction(EPacketType.PT_InGame_InitDataOK, RPC_S2C_InitDataOK);
        BindAction(EPacketType.PT_InGame_GetOnVehicle, RPC_S2C_GetOnVehicle);
        BindAction(EPacketType.PT_InGame_GetOffVehicle, RPC_S2C_GetOffVehicle);
        BindAction(EPacketType.PT_InGame_RepairVehicle, RPC_S2C_RepairVehicle);
        BindAction(EPacketType.PT_InGame_MakeMask, RPC_S2C_MakeMask);
        BindAction(EPacketType.PT_InGame_RemoveMask, RPC_S2C_RemoveMask);
        BindAction(EPacketType.PT_InGame_FastTransfer, RPC_S2C_FastTransfer);
        BindAction(EPacketType.PT_InGame_SwitchScene, RPC_S2C_SwitchScene);
        BindAction(EPacketType.PT_InGame_DelSceneObjects, RPC_S2C_DelSceneObjects);

        BindAction(EPacketType.PT_InGame_PlayerPosition, RPC_S2C_PlayerMovePosition);
        BindAction(EPacketType.PT_InGame_PlayerRot, RPC_S2C_PlayerMoveRotationY);
        BindAction(EPacketType.PT_InGame_PlayerState, RPC_S2C_PlayerMovePlayerState);
        BindAction(EPacketType.PT_InGame_PlayerOnGround, RPC_S2C_PlayerMoveGrounded);
        BindAction(EPacketType.PT_InGame_PlayerShootTarget, RPC_S2C_PlayerMoveShootTarget);
        BindAction(EPacketType.PT_InGame_GliderStatus, RPC_S2C_SyncGliderStatus);
        BindAction(EPacketType.PT_InGame_ParachuteStatus, RPC_S2C_SyncParachuteStatus);
        BindAction(EPacketType.PT_InGame_JetPackStatus, RPC_S2C_SyncJetPackStatus);

        BindAction(EPacketType.PT_InGame_GetAllDeadObjItem, RPC_S2C_GetDeadObjAllItems);
        BindAction(EPacketType.PT_InGame_GetDeadObjItem, RPC_S2C_GetDeadObjItem);
        BindAction(EPacketType.PT_InGame_GetItemBack, RPC_S2C_GetItemBack);
		BindAction(EPacketType.PT_InGame_PreGetItemBack, RPC_S2C_PreGetItemBack);
		BindAction(EPacketType.PT_InGame_GetLootItemBack, RPC_S2C_GetLootItemBack);
        BindAction(EPacketType.PT_InGame_NewItemList, RPC_S2C_NewItemList);
        BindAction(EPacketType.PT_InGame_PackageDelete, RPC_S2C_DeleteItemInPackage);
        BindAction(EPacketType.PT_InGame_UseItem, RPC_S2C_UseItem);
        BindAction(EPacketType.PT_InGame_PutItem, RPC_S2C_PutItem);
        BindAction(EPacketType.PT_InGame_Turn, RPC_S2C_Turn);
        BindAction(EPacketType.PT_InGame_PackageSplit, RPC_S2C_SplitItem);
        BindAction(EPacketType.PT_InGame_ExchangeItem, RPC_S2C_ExchangeItem);        
        BindAction(EPacketType.PT_InGame_SceneObject, RPC_S2C_SceneObject);
        BindAction(EPacketType.PT_Common_ErrorMsg, RPC_S2C_ErrorMsg);
        BindAction(EPacketType.PT_Common_ErrorMsgBox, RPC_S2C_ErrorMsgBox);
        BindAction(EPacketType.PT_Common_ErrorMsgCode, RPC_S2C_ErrorMsgCode);
        BindAction(EPacketType.PT_InGame_SendMsg, RPC_S2C_SendMsg);
        BindAction(EPacketType.PT_InGame_PlayerRevive, RPC_S2C_PlayerRevive);
        BindAction(EPacketType.PT_InGame_ApplyDamage, RPC_S2C_ApplyHpChange);
        BindAction(EPacketType.PT_InGame_PlayerDeath, RPC_S2C_Death);
        BindAction(EPacketType.PT_InGame_ApplyComfort, RPC_S2C_ApplyComfortChange);
        BindAction(EPacketType.PT_InGame_ApplySatiation, RPC_S2C_ApplySatiationChange);
        BindAction(EPacketType.PT_InGame_ItemRemove, RPC_S2C_RemoveItemFromPackage);
        BindAction(EPacketType.PT_InGame_PlayerReset, RPC_S2C_PlayerReset);
        BindAction(EPacketType.PT_InGame_SetShortcut, RPC_S2C_SetShortcut);

        BindAction(EPacketType.PT_InGame_SyncMission, RPC_S2C_SyncMissions);
        BindAction(EPacketType.PT_InGame_NewMission, RPC_S2C_CreateMission);
        BindAction(EPacketType.PT_InGame_AccessMission, RPC_S2C_AccessMission);
        BindAction(EPacketType.PT_InGame_MissionMonsterPos, RPC_S2C_CreateKillMonsterPos);
        BindAction(EPacketType.PT_InGame_MissionFollowPos, RPC_S2C_CreateFollowPos);
        BindAction(EPacketType.PT_InGame_MissionDiscoveryPos, RPC_S2C_CreateDiscoveryPos);
        BindAction(EPacketType.PT_InGame_MissionItemUsePos, RPC_S2C_SyncUseItemPos);
        BindAction(EPacketType.PT_InGame_DeleteMission, RPC_S2C_DeleteMission);
        BindAction(EPacketType.PT_InGame_CompleteTarget, RPC_S2C_CompleteTarget);
        BindAction(EPacketType.PT_InGame_ModifyMissionFlag, RPC_S2C_ModifyMissionFlag);
        BindAction(EPacketType.PT_InGame_CompleteMission, RPC_S2C_ReplyCompleteMission);
        BindAction(EPacketType.PT_InGame_MissionFailed, RPC_S2C_FailMission);
        BindAction(EPacketType.PT_InGame_AddNpcToColony, RPC_S2C_AddNpcToColony);
        BindAction(EPacketType.PT_InGame_MissionKillMonster, RPC_S2C_MissionKillMonster);
        BindAction(EPacketType.PT_InGame_SetMission, RPC_S2C_SetMission);
        BindAction(EPacketType.PT_InGame_Mission953, RPC_S2C_Mission953);
        BindAction(EPacketType.PT_InGame_LanguegeSkill, RPC_S2C_LanguegeSkill);
        BindAction(EPacketType.PT_InGame_MonsterBook, RPC_S2C_MonsterBook);
        BindAction(EPacketType.PT_InGame_EntityReach, RPC_S2C_EntityReach);
        BindAction(EPacketType.PT_InGame_RequestAdMissionData, RPC_S2C_RequestAdMissionData);
        BindAction(EPacketType.PT_InGame_SetCollectItemID, RPC_S2C_SetCollectItem);


        BindAction(EPacketType.PT_InGame_AddBlackList, RPC_S2C_AddBlackList);
        BindAction(EPacketType.PT_InGame_DelBlackList, RPC_S2C_DeleteBlackList);
        BindAction(EPacketType.PT_InGame_ClearBlackList, RPC_S2C_ClearBlackList);
        BindAction(EPacketType.PT_InGame_AddAssistant, RPC_S2C_AddAssistants);
        BindAction(EPacketType.PT_InGame_DelAssistant, RPC_S2C_DeleteAssistants);
        BindAction(EPacketType.PT_InGame_ClearAssistant, RPC_S2C_ClearAssistants);
        BindAction(EPacketType.PT_InGame_BuildLock, RPC_S2C_BuildLock);
        BindAction(EPacketType.PT_InGame_BuildUnLock, RPC_S2C_BuildUnLock);
        BindAction(EPacketType.PT_InGame_ClearBuildLock, RPC_S2C_ClearBuildLock);
        BindAction(EPacketType.PT_InGame_ClearVoxel, RPC_S2C_ClearVoxelData);
        BindAction(EPacketType.PT_InGame_ClearAllVoxel, RPC_S2C_ClearAllVoxelData);
        BindAction(EPacketType.PT_InGame_AreaLock, RPC_S2C_LockArea);
        BindAction(EPacketType.PT_InGame_AreaUnLock, RPC_S2C_UnLockArea);
        BindAction(EPacketType.PT_InGame_BlockLock, RPC_S2C_BuildChunk);
        BindAction(EPacketType.PT_InGame_LoginBan, RPC_S2C_JoinGame);
        BindAction(EPacketType.PT_InGame_CreateBuilding, RPC_S2C_BuildBuildingBlock);

        BindAction(EPacketType.PT_InGame_InitShop, RPC_S2C_InitNpcShop);
        BindAction(EPacketType.PT_InGame_RepurchaseIndex, RPC_S2C_SyncRepurchaseItemIDs);
        BindAction(EPacketType.PT_InGame_ChangeCurrency, RPC_S2C_ChangeCurrency);

        BindAction(EPacketType.PT_InGame_SkillCast, RPC_S2C_SkillCast);
        BindAction(EPacketType.PT_InGame_SkillShoot, RPC_S2C_SkillCastShoot);
        BindAction(EPacketType.PT_InGame_MergeSkillList, RPC_S2C_MergeSkillList);
        BindAction(EPacketType.PT_InGame_MetalScanList, RPC_S2C_MetalScanList);
        BindAction(EPacketType.PT_InGame_SynthesisSuccess, RPC_S2C_ReplicateSuccess);
        BindAction(EPacketType.PT_InGame_PersonalStorageStore, RPC_S2C_PersonalStorageStore);
        BindAction(EPacketType.PT_InGame_PersonalStroageDelete, RPC_S2C_PersonalStorageDelete);
        BindAction(EPacketType.PT_InGame_PersonalStorageFetch, RPC_S2C_PersonalStorageFetch);
        BindAction(EPacketType.PT_InGame_PersonalStorageSplit, RPC_S2C_PersonalStorageSplit);
        BindAction(EPacketType.PT_InGame_PersonalStorageExchange, RPC_S2C_PersonalStorageExchange);
        BindAction(EPacketType.PT_InGame_PersonalStorageSort, RPC_S2C_PersonalStorageSort);
        BindAction(EPacketType.PT_InGame_PersonalStorageIndex, RPC_S2C_PersonalStorageIndex);

        BindAction(EPacketType.PT_InGame_PublicStorageStore, RPC_S2C_PublicStorageStore);
        BindAction(EPacketType.PT_InGame_PublicStorageFetch, RPC_S2C_PublicStorageFetch);
        BindAction(EPacketType.PT_InGame_PublicStorageDelete, RPC_S2C_PublicStorageDelete);
        BindAction(EPacketType.PT_InGame_PublicStorageSplit, RPC_S2C_PublicStorageSplit);
        BindAction(EPacketType.PT_InGame_PublicStorageIndex, RPC_S2C_PublicStorageIndex);

        BindAction(EPacketType.PT_InGame_TownAreaArray, RPC_S2C_TownAreaList);
        BindAction(EPacketType.PT_InGame_CampAreaArray, RPC_S2C_CampAreaList);
        BindAction(EPacketType.PT_InGame_MaskAreaArray, RPC_S2C_MaskAreaList);

        BindAction(EPacketType.PT_InGame_TownArea, RPC_S2C_AddTownArea);
        BindAction(EPacketType.PT_InGame_CampArea, RPC_S2C_AddCampArea);

        BindAction(EPacketType.PT_InGame_ExploredArea, RPC_S2C_ExploredArea);
        BindAction(EPacketType.PT_InGame_ExploredAreaArray, RPC_S2C_ExploredAreas);
        //plant
        BindAction(EPacketType.PT_InGame_Plant_GetBack, RPC_S2C_Plant_GetBack);
        BindAction(EPacketType.PT_InGame_Plant_PutOut, RPC_S2C_Plant_PutOut);
        BindAction(EPacketType.PT_InGame_Plant_VFTerrainTarget, RPC_S2C_Plant_VFTerrainTarget);
        BindAction(EPacketType.PT_InGame_Plant_FarmInfo, RPC_S2C_Plant_FarmInfo);
        BindAction(EPacketType.PT_InGame_Plant_Water, RPC_S2C_Plant_Water);
        BindAction(EPacketType.PT_InGame_Plant_Clean, RPC_S2C_Plant_Clean);
        BindAction(EPacketType.PT_InGame_Plant_Clear, RPC_S2C_Plant_Clear);
        //railway
        BindAction(EPacketType.PT_InGame_Railway_AddPoint, RPC_S2C_Railway_AddPoint);
        BindAction(EPacketType.PT_InGame_Railway_PrePointChange, RPC_S2C_Railway_PrePointChange);
        BindAction(EPacketType.PT_InGame_Railway_NextPointChange, RPC_S2C_Raileway_NextPointChange);
        BindAction(EPacketType.PT_InGame_Railway_Recycle, RPC_S2C_Railway_Recycle);
        BindAction(EPacketType.PT_InGame_Railway_Route, RPC_S2C_Railway_Route);
        BindAction(EPacketType.PT_InGame_Railway_GetOnTrain, RPC_S2C_Railway_GetOnTrain);
        BindAction(EPacketType.PT_InGame_Railway_GetOffTrain, RPC_S2C_Railway_GetOffTrain);
        BindAction(EPacketType.PT_InGame_Railway_DeleteRoute, RPC_S2C_Railway_DeleteRoute);
        BindAction(EPacketType.PT_InGame_Railway_SetRouteTrain, RPC_S2C_Railway_SetRouteTrain);
        BindAction(EPacketType.PT_InGame_Railway_ChangeStationRot, RPC_S2C_Railway_ChangeStationRot);
        BindAction(EPacketType.PT_InGame_Railway_GetOffTrainEx, RPC_S2C_Railway_GetOffTrainEx);
        BindAction(EPacketType.PT_InGame_Railway_ResetPointName, RPC_S2C_Railway_ResetPointName);
        BindAction(EPacketType.PT_InGame_Railway_ResetRouteName, RPC_S2C_Railway_ResetRouteName);
        BindAction(EPacketType.PT_InGame_Railway_ResetPointTime, RPC_S2C_Railway_ResetPointTime);
        BindAction(EPacketType.PT_InGame_Railway_AutoCreateRoute, RPC_S2C_Railway_AutoCreateRoute);
        BindAction(EPacketType.PT_InGame_Railway_UpdateRoute, RPC_S2C_Railway_UpdateRoute);


        //accountitems
        BindAction(EPacketType.PT_InGame_AccItems_CreateItem, RPC_AccItems_CreateItem);
        //player skilltree
        BindAction(EPacketType.PT_InGame_SKTLevelUp, RPC_S2C_SKTLevelUp);
        BindAction(EPacketType.PT_Test_PutOnEquipment, RPC_S2C_Test_PutOnEquipment);

        //BindAction(EPacketType.PT_Test_AddItem, RequestAddItem);

        //randomItems
        BindAction(EPacketType.PT_InGame_RandomItem, RPC_S2C_GenRandomItem);
		BindAction(EPacketType.PT_InGame_RandomItemRare,RPC_S2C_GenRandomItemRare);
		BindAction(EPacketType.PT_InGame_RandomIsoCode,RPC_S2C_GetRandomIsoCode);
		BindAction(EPacketType.PT_InGame_RandomItemRareAry,RPC_S2C_RandomItemRareAry);

        BindAction(EPacketType.PT_InGame_RandomItemFetch, RPC_S2C_RandomItemFetch);
        BindAction(EPacketType.PT_InGame_RandomItemFetchAll, RPC_S2C_RandomItemFetchAll);
        //randomFeces
        BindAction(EPacketType.PT_InGame_RandomFeces, RPC_S2C_GenRandomFeces);
        //--new clicked
        BindAction(EPacketType.PT_InGame_RandomItemClicked, RPC_S2C_RandomItemClicked);
		BindAction(EPacketType.PT_InGame_RandomItemDestroy, RPC_S2C_RandomItemDestroy);
		BindAction(EPacketType.PT_InGame_RandomItemDestroyList, RPC_S2C_RandomItemDestroyList);

		//randomDunGen
		BindAction(EPacketType.PT_InGame_EnterDungeon,RPC_S2C_EnterDungeon);
		BindAction(EPacketType.PT_InGame_ExitDungeon,RPC_S2C_ExitDungeon);
		BindAction(EPacketType.PT_InGame_GenDunEntrance,RPC_S2C_GenDunEntrance);
		BindAction(EPacketType.PT_InGame_GenDunEntranceList,RPC_S2C_GenDunEntranceList);
		BindAction(EPacketType.PT_InGame_InitWhenSpawn,RPC_S2C_InitWhenSpawn);

        BindAction(EPacketType.PT_InGame_CreateTeam, RPC_S2C_CreateNewTeam);
        BindAction(EPacketType.PT_InGame_JoinTeam, RPC_S2C_JoinTeam);
        BindAction(EPacketType.PT_InGame_ApproveJoin, RPC_S2C_ApproveJoin);
        BindAction(EPacketType.PT_InGame_DenyJoin, RPC_S2C_DenyJoin);
        BindAction(EPacketType.PT_InGame_Invitation, RPC_S2C_Invitation);
        BindAction(EPacketType.PT_InGame_AcceptJoinTeam, RPC_S2C_AcceptJoinTeam);
        BindAction(EPacketType.PT_InGame_KickSB, RPC_S2C_KickSB);
        BindAction(EPacketType.PT_InGame_LeaderDeliver, RPC_S2C_LeaderDeliver);
        BindAction(EPacketType.PT_InGame_QuitTeam, RPC_S2C_QuitTeam);
        BindAction(EPacketType.PT_InGame_DissolveTeam, RPC_S2C_DissolveTeam);
        BindAction(EPacketType.PT_InGame_TeamInfo, RPC_S2C_TeamInfo);

        BindAction(EPacketType.PT_InGame_ReviveSB, RPC_S2C_ReviveSB);
        BindAction(EPacketType.PT_Common_FoundMapLable, RPC_S2C_FoundMapLable);
        BindAction(EPacketType.PT_InGame_SyncArmorInfo, RPC_S2C_SyncArmorInfo);
        BindAction(EPacketType.PT_InGame_ArmorDurability, RPC_S2C_ArmorDurability);
        BindAction(EPacketType.PT_InGame_SwitchArmorSuit, RPC_C2S_SwitchArmorSuit);
        BindAction(EPacketType.PT_InGame_EquipArmorPart, RPC_C2S_EquipArmorPart);
        BindAction(EPacketType.PT_InGame_RemoveArmorPart, RPC_C2S_RemoveArmorPart);
        BindAction(EPacketType.PT_InGame_SwitchArmorPartMirror, RPC_C2S_SwitchArmorPartMirror);
        BindAction(EPacketType.PT_InGame_SyncArmorPartPos, RPC_C2S_SyncArmorPartPos);
        BindAction(EPacketType.PT_InGame_SyncArmorPartRot, RPC_C2S_SyncArmorPartRot);
        BindAction(EPacketType.PT_InGame_SyncArmorPartScale, RPC_C2S_SyncArmorPartScale);

        //colonyMgr
        BindAction(EPacketType.PT_CL_MGR_InitData, RPC_S2C_Mgr_InitData);

        BindAction(EPacketType.PT_Custom_CheckResult, RPC_S2C_CustomCheckResult);
        BindAction(EPacketType.PT_Custom_AddQuest, RPC_S2C_CustomAddQuest);
        BindAction(EPacketType.PT_Custom_RemoveQuest, RPC_S2C_CustomRemoveQuest);
        BindAction(EPacketType.PT_Custom_AddChoice, RPC_S2C_CustomAddChoice);

        BindAction(EPacketType.PT_Custom_EnableSpawn, RPC_S2C_EnableSpawn);
        BindAction(EPacketType.PT_Custom_DisableSpawn, RPC_S2C_DisableSpawn);
        BindAction(EPacketType.PT_Custom_OrderTarget, RPC_S2C_OrderTarget);
        BindAction(EPacketType.PT_Custom_CancelOrder, RPC_S2C_CancelOrder);
        BindAction(EPacketType.PT_Custom_StartAnimation, RPC_S2C_PlayAnimation);
        BindAction(EPacketType.PT_Custom_StopAnimation, RPC_S2C_StopAnimation);
        BindAction(EPacketType.PT_Common_ScenarioId, RPC_S2C_ScenarioId);
		BindAction(EPacketType.PT_InGame_CurTeamId, RPC_S2C_CurTeamId);
		BindAction(EPacketType.PT_InGame_ClearGrass, RPC_S2C_ClearGrass);
		BindAction(EPacketType.PT_InGame_ClearTree, RPC_S2C_ClearTree);
		BindAction(EPacketType.PT_CheatingChecked, RPC_S2C_CheatingChecked);

        //lz-2017.02.15 Ride mosnter
        BindAction(EPacketType.PT_Mount_ReqMonsterCtrl, RPC_S2C_ReqMonsterCtrl);
        BindAction(EPacketType.PT_Mount_AddMountMonster, RPC_S2C_AddMountMonster);
        BindAction(EPacketType.PT_Mount_DelMountMonster, RPC_S2C_DelMountMonste);
        BindAction(EPacketType.PT_Mount_SyncPlayerRot, RPC_S2C_SyncPlayerRot);
        BindAction(EPacketType.PT_GM_Statistics, RPC_S2C_Statistics);

        if (IsOwner)
            RequestTerrainData();

        StartCoroutine(WaitForTerrainData());
    }

    protected override void OnPEDestroy()
    {
        if (null != entity)
            entity.SendMsg(EMsg.Net_Destroy);

		base.OnPEDestroy();

		RemovePlayer();

		GroupNetwork.DelJoinRequest(this);
		GroupNetwork.RemoveFromTeam(TeamId, this);

		if (Id != PlayerNetwork.mainPlayerId)
        {
			OnTeamChanged(TeamId);

			string msg = PELocalization.GetString(8000168);
            new PeTipMsg(msg.Replace("Playername%", RoleName), PeTipMsg.EMsgLevel.Norm, PeTipMsg.EMsgType.Misc);
        }
        else
        {
            DetachUIEvent();
			if(Pathea.PeGameMgr.IsMultiStory)
			{
				Pathea.PeGameMgr.yirdName = null;
			}
        }
    }

	public override void OnPeMsg(EMsg msg, params object[] args)
	{
		switch (msg)
		{
			case EMsg.View_Prefab_Build:
				OnPrefabViewBuild();
				break;

			case EMsg.View_Prefab_Destroy:
				OnOnPrefabViewDestroy();
				break;
		}
	}

	public override void OnSpawned(GameObject obj)
    {
        base.OnSpawned(obj);

		if (null == entity.netCmpt)
			entity.netCmpt = entity.Add<NetCmpt>();

		entity.netCmpt.network = this;

		_learntSkills = runner.SkEntityPE.gameObject.GetComponent<SkillTreeUnitMgr>();

        if (_learntSkills != null)
            _learntSkills.SetNet(this);

        RequestData();

		//do sth. when spawn
    }

	public override void SetTeamId(int teamId)
	{
		base.SetTeamId(teamId);
		ForceSetting.AddPlayer(Id, TeamId, EPlayerType.Human, RoleName);

	}
	#endregion Inherited APIs

	#region EventHandler
	void OnPrefabViewBuild()
	{
		if (null != OnPrefabViewBuildEvent)
			OnPrefabViewBuildEvent(this);
	}

	void OnOnPrefabViewDestroy()
	{
		if (null != OnPrefabViewDestroyEvent)
			OnPrefabViewDestroyEvent(this);
	}
	#endregion

	public static PlayerNetwork GetNearestPlayer( Vector3 pos)
    {
        List<PlayerNetwork> players = PlayerNetwork.Get<PlayerNetwork>();
        if (players == null || players.Count == 0 || players[0] == null)
            return null;
        PlayerNetwork firstPlayer = players[0];
        foreach (var iter in players)
        {
            if (iter != null)
            {
                if (Vector3.Distance(iter.transform.position, pos) < Vector3.Distance(firstPlayer.transform.position, pos))
                    firstPlayer = iter;
            }
        }
        return firstPlayer;
    }

    #region Internal APIs
    void AddPlayer()
	{
		if (PlayerDic.ContainsKey(Id))
			PlayerDic.Remove(Id);

		PlayerDic.Add(Id, this);
		PlayerList.Add(this);
	}

	void RemovePlayer()
	{
		if (PlayerDic.ContainsKey(Id))
			PlayerDic.Remove(Id);

		PlayerList.Remove(this);
	}

	public static PlayerNetwork GetPlayer(int id)
	{
		if (!PlayerDic.ContainsKey(id))
			return null;

		return PlayerDic[id];
	}

	public static void PlayerAction(Action<PlayerNetwork> action)
    {
        foreach (PlayerNetwork p in PlayerList)
        {
            action(p);
        }
    }

    public void CreateMapObj(int objType, MapObj[] mapObj)
    {//objType  0 for trade ;1 for player dead
        if (mapObj == null || mapObj.Count() == 0)
            return;
        RPCServer(EPacketType.PT_InGame_CreateMapObj, objType, mapObj.ToArray(), true);
    }

    public void CreateSceneBox(int boxId)
    {
        RPCServer(EPacketType.PT_InGame_CreateSceneBox, boxId);
    }

    public void CreateSceneItem(string sceneItemName, Vector3 pos, string items, int idx = -1,bool precise = false)
    {
        RPCServer(EPacketType.PT_InGame_CreateSceneItem, sceneItemName, pos, items, idx, precise);
    }
    public void DestroySceneItem(Vector3 pos)
    {
        if (hasOwnerAuth)
            RPCServer(EPacketType.PT_MO_Destroy, pos);
    }

    IEnumerator HeartBeat()
	{
		while (true)
		{
			yield return new WaitForSeconds(2f);
			RPCServer(EPacketType.PT_InGame_HeartBeat);
		}
	}

    private IEnumerator PlayerMove()
    {
        while (true)
        {
            if (null != _transCmpt && _move != null)
            {
                if (Mathf.Abs(_transCmpt.position.x - mPlayerSynAttribute.mv3Postion.x) > PlayerSynAttribute.SyncMovePrecision || 
                    Mathf.Abs(_transCmpt.position.y - mPlayerSynAttribute.mv3Postion.y) > PlayerSynAttribute.SyncMovePrecision || 
                    Mathf.Abs(_transCmpt.position.z - mPlayerSynAttribute.mv3Postion.z) > PlayerSynAttribute.SyncMovePrecision ||
                    Mathf.Abs(_transCmpt.rotation.eulerAngles.y - mPlayerSynAttribute.mfRotationY) > PlayerSynAttribute.SyncMovePrecision)
                {
                    URPCServer(EPacketType.PT_InGame_PlayerPosition, _transCmpt.position, (byte)_move.speed, _transCmpt.rotation.eulerAngles.y, GameTime.Timer.Second);
                    mPlayerSynAttribute.mv3Postion = _transCmpt.position;
					mPlayerSynAttribute.mnPlayerState = _move.speed;
					mPlayerSynAttribute.mfRotationY = _transCmpt.rotation.eulerAngles.y;

					_pos = transform.position = _transCmpt.position;
                    rot = transform.rotation = _transCmpt.rotation;
                }
            }

            yield return new WaitForSeconds(1 / uLink.Network.sendRate);
        }
    }

    private void InitializePeEntity()
    {
        CustomCharactor.CustomData customData = networkBase.Role.CreateCustomData();
        UnityEngine.Assertions.Assert.IsNotNull(networkBase, "network base is null.");

        //entity = PeCreature.Instance.CreatePlayer(Id, customData);
        entity = PeEntityCreator.Instance.CreatePlayer(Id, Vector3.zero, Quaternion.identity, Vector3.one, customData);
        UnityEngine.Assertions.Assert.IsNotNull(entity, "player entity create failed.");

        if (IsProxy)
        {
            MainPlayerCmpt mainPlayerCmpt = entity.GetCmpt<MainPlayerCmpt>();
            if (null != mainPlayerCmpt)
            {
                entity.Remove(mainPlayerCmpt);
            }

			//EntityInfoCmpt c = entity.GetCmpt<EntityInfoCmpt>();
			//if (null != c)
			//{
			//	if (TeamId == BaseNetwork.MainPlayer.TeamId)
			//		c.mapIcon = PeMap.MapIcon.AllyPlayer;
			//	else
			//		c.mapIcon = PeMap.MapIcon.OppositePlayer;
			//}
		}
        else
        {
            MapCmpt c = entity.GetCmpt<MapCmpt>();
            if (null != c)
                entity.Remove(c);

            Pathea.MainPlayer.Instance.SetEntityId(Id);

            AttachUIEvent();
            _missionInited = false;
        }

        _transCmpt = entity.peTrans;
        if (null != _transCmpt)
            _transCmpt.position = transform.position;

        _move = entity.GetCmpt<Motion_Move>();
        if(_move!=null && mainPlayerId != Id)
            _move.NetMoveTo(transform.position, Vector3.zero, true);

        OnSkAttrInitEvent += InitForceData;
        OnSpawned(entity.GetGameObject());
    }

    public override void InitForceData()
    {
        ForceSetting.AddPlayer(Id, TeamId, EPlayerType.Human, RoleName);
        ForceSetting.AddPlayer(TeamId, TeamId, EPlayerType.Human, "Team" + TeamId);

		if (PeGameMgr.IsSurvive)
			ForceSetting.AddForce(TeamId, Pathea.PeGameMgr.EGameType.Survive);

		if (null != entity)
		{
			entity.SetAttribute(AttribType.DefaultPlayerID, Id);
			entity.SetAttribute(AttribType.CampID, TeamId);
			entity.SetAttribute(AttribType.DamageID, TeamId);
		}
    }

    void AttachUIEvent()
    {
        CSUI_TeamInfoMgr.CreatTeamEvent += RequestNewTeam;
        CSUI_TeamInfoMgr.JoinTeamEvent += RequestJoinTeam;
        CSUI_TeamInfoMgr.KickTeamEvent += RequestKickSB;
        CSUI_TeamInfoMgr.AcceptJoinTeamEvent += SyncAcceptJoinTeam;
        CSUI_TeamInfoMgr.OnAgreeJoinEvent += RequestApproveJoin;
        CSUI_TeamInfoMgr.OnDeliverToEvent += RequestLeaderDeliver;
        CSUI_TeamInfoMgr.OnMemberQuitTeamEvent += RequestQuitTeam;
        CSUI_TeamInfoMgr.OnInvitationEvent += RequestInvitation;
        CSUI_TeamInfoMgr.OnDissolveEvent += RequestDissolveTeam;
    }

    void DetachUIEvent()
    {
        CSUI_TeamInfoMgr.CreatTeamEvent -= RequestNewTeam;
        CSUI_TeamInfoMgr.JoinTeamEvent -= RequestJoinTeam;
        CSUI_TeamInfoMgr.KickTeamEvent -= RequestKickSB;
        CSUI_TeamInfoMgr.AcceptJoinTeamEvent -= SyncAcceptJoinTeam;
        CSUI_TeamInfoMgr.OnAgreeJoinEvent -= RequestApproveJoin;
        CSUI_TeamInfoMgr.OnDeliverToEvent -= RequestLeaderDeliver;
        CSUI_TeamInfoMgr.OnMemberQuitTeamEvent -= RequestQuitTeam;
        CSUI_TeamInfoMgr.OnInvitationEvent -= RequestInvitation;
        CSUI_TeamInfoMgr.OnDissolveEvent -= RequestDissolveTeam;
    }

    IEnumerator GetOnVehicle(int id)
    {
        while (true)
        {
            DriveCreation = Get<CreationNetwork>(id);
            if (null != DriveCreation)
            {
				if (DriveCreation.GetOn(Runner, SeatIndex))
				{
					if (SeatIndex == -1)
						DriveCreation.Driver = this;
					else
						DriveCreation.AddPassanger(this);

					break;
				}
            }
			else
			{
				break;
			}

            yield return null;
        }
    }

    public void GetOffVehicle(Vector3 pos, EVCComponent seatType)
    {
        if (null != DriveCreation)
        {
            DriveCreation.GetOff(pos, seatType);
            DriveCreation = null;
        }
    }

    private IEnumerator WaitForTerrainData()
    {
        while (!isTerrainDataOk)
            yield return null;

        RequestInitData();
    }

    public Vector3 GetCustomModePos()
    {
        Vector3 pos;

        if (null == entity)
        {
            ForceSetting.GetForcePos(TeamId, out pos);
        }
        else
        {
            if (!ForceSetting.GetScenarioPos(entity.scenarioId, out pos))
                ForceSetting.GetForcePos(TeamId, out pos);
        }

        return pos;
    }
    #endregion Internal APIs

    #region Network Event
    public event Action<int> OnCustomDeathEventHandler;
    public event Action<int, int, float> OnCustomDamageEventHandler;
    public event Action<ItemAsset.ItemObject> OnCustomUseItemEventHandler;
    public event Action<ItemAsset.ItemObject> OnCustomPutOutItemEventHandler;

    public void OnCustomDeath(int scenarioId)
    {
        if (null != OnCustomDeathEventHandler)
            OnCustomDeathEventHandler(scenarioId);
    }

    public void OnCustomDamage(int scenarioId, int casterScenarioId, float damage)
    {
        if (null != OnCustomDamageEventHandler)
            OnCustomDamageEventHandler(scenarioId, casterScenarioId, damage);
    }

    public void OnCustomUseItem(int customId, int itemInstanceId)
    {
        ItemObject item = ItemMgr.Instance.Get(itemInstanceId);
        if (null == item)
            return;

        if (null != OnCustomUseItemEventHandler)
            OnCustomUseItemEventHandler(item);
    }

    public void OnCustomPutoutItem(int customId, int itemInstanceId)
    {
        ItemObject item = ItemMgr.Instance.Get(itemInstanceId);
        if (null == item)
            return;

        if (null != OnCustomPutOutItemEventHandler)
            OnCustomPutOutItemEventHandler(item);
    }
    #endregion

    #region Network Request
    public void RequestTerrainData()
    {
        RPCServer(EPacketType.PT_InGame_TerrainData);
    }

    public void RequestInitData()
    {
        RPCServer(EPacketType.PT_InGame_RequestInit);
    }

    public void RequestData()
    {
        RPCServer(EPacketType.PT_InGame_RequestData, IsOwner, SteamMgr.steamId.m_SteamID);
    }

    public void SyncPostion(Vector3 pos)
    {
        URPCServer(EPacketType.PT_InGame_PlayerPosition, pos);
    }

    public void SyncRotY(float rotY)
    {
        URPCServer(EPacketType.PT_InGame_PlayerRot, rotY);
    }

    public void SyncSpeedState(SpeedState speed)
    {
        URPCServer(EPacketType.PT_InGame_PlayerState, (byte)speed);
    }

    public void SyncSpawnPos(byte[] binPos)
    {
        RPCServer(EPacketType.PT_AI_SpawnPos, binPos);
    }

    public void RequestAddItem(int objID, int splitNum)
    {
        RPCServer(EPacketType.PT_Test_AddItem, objID, splitNum);
    }

    public void RequestMoveNpc(int proid, Vector3 pos)
    {
        RPCServer(EPacketType.PT_Test_MoveNpc, proid, pos);
    }

    public void RequestSplitItem(int objID, int splitNum)
    {
        RPCServer(EPacketType.PT_InGame_PackageSplit, objID, splitNum);
    }

    public void RequestDeleteItem(int objID, int tabIndex, int itemIndex)
    {
        RPCServer(EPacketType.PT_InGame_PackageDelete, objID, tabIndex, itemIndex);
    }

    public void RequestSortPackage(int tabIndex)
    {
        RPCServer(EPacketType.PT_InGame_PackageSort, tabIndex);
    }

    public void RequestExchangeItem(ItemObject itemObj, int srcIndex, int destIndex)
    {
        if (null == itemObj)
            return;

        RPCServer(EPacketType.PT_InGame_ExchangeItem, itemObj.instanceId, srcIndex, destIndex);
    }

    public void RequestPutOnEquipment(ItemObject itemObj, int index)
    {
        if (null == itemObj)
            return;

        RPCServer(EPacketType.PT_InGame_PutOnEquipment, itemObj.instanceId, index);
    }

    public void RequestAddFountMapLable(int mapLableId)
    {
        RPCServer(EPacketType.PT_Common_FoundMapLable, mapLableId);
    }

    public void RequestTakeOffEquipment(ItemObject itemObj)
    {
        if (null == itemObj)
            return;

        if (null == itemObj.GetCmpt<ItemAsset.Equip>())
            return;

        if (!Pathea.PeGender.IsMatch(itemObj.protoData.equipSex, PeGender.Convert(Sex)))
        {
            string _info = PELocalization.GetString(8000093);
            MessageBox_N.ShowOkBox(_info);
            return;
        }

        RPCServer(EPacketType.PT_InGame_TakeOffEquipment, itemObj.instanceId);
    }

    public void RequestPublicStorageStore(int objID, int storageIndex)
    {
        RPCServer(EPacketType.PT_InGame_PublicStorageStore, objID, storageIndex);
    }

    public void RequestPublicStorageExchange(int objID, int srcIndex, int destIndex)
    {
        RPCServer(EPacketType.PT_InGame_PublicStorageExchange, objID, srcIndex, destIndex);
    }

    public void RequestPublicStorageFetch(int objID, int packageIndex)
    {
        RPCServer(EPacketType.PT_InGame_PublicStorageFetch, objID, packageIndex);
    }

    public void RequestPublicStroageDelete(int objID)
    {
        RPCServer(EPacketType.PT_InGame_PublicStorageDelete, objID);
    }

    public void RequestPublicStorageSort(int tab)
    {
        RPCServer(EPacketType.PT_InGame_PublicStorageSort, tab);
    }

    public void RequestPublicStorageSplite(int objID, int num)
    {
        RPCServer(EPacketType.PT_InGame_PublicStorageSplit, objID, num);
    }

    public void RequestPersonalStorageSort(int tabIndex)
    {
        RPCServer(EPacketType.PT_InGame_PersonalStorageSort, tabIndex);
    }

    public void RequestPersonalStorageSplit(int objID, int num)
    {
        RPCServer(EPacketType.PT_InGame_PersonalStorageSplit, objID, num);
    }

    public void RequestPersonalStorageStore(int objID, int dstIndex)
    {
        RPCServer(EPacketType.PT_InGame_PersonalStorageStore, objID, dstIndex);
    }

    public void RequestPersonalStorageExchange(int objID, int originIndex, int destIndex)
    {
        RPCServer(EPacketType.PT_InGame_PersonalStorageExchange, objID, originIndex, destIndex);
    }

    public void RequestPersonalStorageFetch(int objID, int dstIndex)
    {
        RPCServer(EPacketType.PT_InGame_PersonalStorageFetch, objID, dstIndex);
    }

    public void RequestPersonalStorageDelete(int objID)
    {
        RPCServer(EPacketType.PT_InGame_PersonalStroageDelete, objID);
    }

    public void RequestCreateBuildingWithItem(BuildingID buildingId, List<CreatItemInfo> itemInfoList, Vector3 root, int id, int rotation)
    {
        RPCServer(EPacketType.PT_InGame_CreateBuilding, buildingId, itemInfoList.ToArray(), root, id, rotation);
    }

    public void RequestMergeSkill(int mCurrentMergeId, int mCurrentNum)
    {
        RPCServer(EPacketType.PT_InGame_MergeSkill, mCurrentMergeId, mCurrentNum);
    }

    public void RequestReload(int id, int objId, int oldProtoId, int newProtoId, float magazineSize)
    {
        NetworkInterface net = NetworkInterface.Get(id);
        if (null != net && net.hasOwnerAuth)
            RPCServer(EPacketType.PT_InGame_WeaponReload, id, objId, oldProtoId, newProtoId, magazineSize);
    }

    public void RequestGunEnergyReload(int id, int weaponId, float num)
    {
        NetworkInterface net = NetworkInterface.Get(id);
        if (null != net && net.hasOwnerAuth)
            RPCServer(EPacketType.PT_InGame_GunEnergyReload, id, weaponId, num);
    }

    public void RequestBatteryEnergyReload(int id, int weaponId, float num)
    {
        NetworkInterface net = NetworkInterface.Get(id);
        if (null != net && net.hasOwnerAuth)
            RPCServer(EPacketType.PT_InGame_BatteryEnergyReload, id, weaponId, num);
    }

    public void RequestJetPackEnergyReload(int id, int weaponId, float num)
    {
        NetworkInterface net = NetworkInterface.Get(id);
        if (null != net && net.hasOwnerAuth)
            RPCServer(EPacketType.PT_InGame_JetPackEnergyReload, id, weaponId, num);
    }

    public void RequestWeaponDurability(int id, int weaponId)
    {
        NetworkInterface net = NetworkInterface.Get(id);
        if (null != net && net.hasOwnerAuth)
            RPCServer(EPacketType.PT_InGame_WeaponDurability, id, weaponId);
    }

    public void RequestArmorDurability(int id, int[] equipIds, float damage, SkEntity caster)
    {
        NetworkInterface net = NetworkInterface.Get(id);
        if (null != net && null != caster && caster.IsController())
            RPCServer(EPacketType.PT_InGame_ArmorDurability, id, equipIds, damage);
    }

    public void RequestAttrChanged(int entityId, int objId, float costNum, int bulletProtoId)
    {
        NetworkInterface net = NetworkInterface.Get(entityId);
        if (null != net && net.hasOwnerAuth)
            RPCServer(EPacketType.PT_InGame_ItemAttrChanged, entityId, objId, costNum, bulletProtoId);
    }

    public void RequestThrow(int entityId, int objId, float costNum)
    {
        NetworkInterface net = NetworkInterface.Get(entityId);
        if (null != net && net.hasOwnerAuth)
            RPCServer(EPacketType.PT_InGame_EquipItemCost, entityId, objId, costNum);
    }
    public void RequestItemCost(int entityId, int objId, float costNum)
    {
        NetworkInterface net = NetworkInterface.Get(entityId);
        if (null != net && net.hasOwnerAuth)
            RPCServer(EPacketType.PT_InGame_PackageItemCost, entityId, objId, costNum);
    }

    public void RequestRedo(int opType, IntVector3[] indexes, BSVoxel[] oldvoxels, BSVoxel[] voxels, EBSBrushMode mode, int dsType, float scale)
    {
        byte[] data = PETools.Serialize.Export(w =>
       {
           BufferHelper.Serialize(w, opType);
           BufferHelper.Serialize(w, (int)mode);
           BufferHelper.Serialize(w, dsType);
           BufferHelper.Serialize(w, scale);
           BufferHelper.Serialize(w, indexes.Length);
           for (int i = 0; i < indexes.Length; i++)
           {
               BufferHelper.Serialize(w, indexes[i]);
               BufferHelper.Serialize(w, oldvoxels[i]);
               BufferHelper.Serialize(w, voxels[i]);
           }
       });

        RPCServer(EPacketType.PT_InGame_BlockRedo, data);
    }

    public static IEnumerator RequestCreateAdNpc(int npcId, Vector3 pos)
    {
        while (null == mainPlayer)
            yield return null;

        mainPlayer.RPCServer(EPacketType.PT_NPC_CreateAd, npcId, pos);
    }

    public static IEnumerator RequestCreateAdMainNpc(int npcId, Vector3 pos)
    {
        while (null == mainPlayer)
            yield return null;

        mainPlayer.RPCServer(EPacketType.PT_NPC_CreateAdMainNpc, npcId, pos);
    }

    public void RequestCreateStRdNpc(int npcId, Vector3 pos, int protoId)
    {
        if (NetworkInterface.Get(npcId) == null)
        {
            if (npcId > 10000)
            {
                return;
                //Debug.LogError("error NpcID npcID = " + npcId);
            }
            RPCServer(EPacketType.PT_NPC_CreateStRd, npcId, pos, protoId);
        }
    }

    public void RequestCreateStNpc(int npcId, Vector3 pos, int protoId)
    {
        if (NetworkInterface.Get(npcId) == null)
        {
            if (npcId > 10000)
            {
                return;
                //Debug.LogError("error NpcID npcID = " + npcId);
            }
            RPCServer(EPacketType.PT_NPC_CreateSt, npcId, pos, protoId);
        }
    }

    public void RequestTownNpc(Vector3 pos, int key, int type = 0, bool isStand = false, float rotY = 0)
    {

        RPCServer(EPacketType.PT_NPC_CreateTown, pos, key, type, isStand, rotY);
    }


    public static IEnumerator RequestCreateGroupAi(int aiId, Vector3 pos)
    {
        while (null == mainPlayer)
            yield return null;

        mainPlayer.RPCServer(EPacketType.PT_AI_SpawnGroupAI, aiId, pos);
    }

    public static IEnumerator RequestCreateAi(int aiId, Vector3 pos, int groupId, int tdId,int dungeonId,int colorType=-1,int playerId=-1,int buffId=0)
    {
        while (null == mainPlayer)
            yield return null;

        mainPlayer.RPCServer(EPacketType.PT_AI_SpawnAI, aiId, pos, groupId, tdId,dungeonId,colorType,playerId,buffId);
    }

    public void RequestSetShortcuts(int itemId, int srcIndex, int destIndex, ItemPlaceType place)
    {
        RPCServer(EPacketType.PT_InGame_SetShortcut, itemId, srcIndex, destIndex, place);
    }

    public void RequestSendMsg(EMsgType msgtype, string msg)
    {
        RPCServer(EPacketType.PT_InGame_SendMsg, msgtype, msg);
    }

    public void RequestDeadObjAllItems(int id)
    {
        RPCServer(EPacketType.PT_InGame_GetAllDeadObjItem, id);
    }

    public void RequestDeadObjItem(int id, int index, int itemId)
    {
        RPCServer(EPacketType.PT_InGame_GetDeadObjItem, id, index, itemId);
    }

    public void RequestFastTravel(int wrapType, Vector3 pos, int cost)
    {
        RPCServer(EPacketType.PT_InGame_FastTransfer, pos, cost, wrapType);
    }

    public void RequestGetItemBack(int objId)
    {
        RPCServer(EPacketType.PT_InGame_GetItemBack, objId);
    }

	public static void PreRequestGetItemBack(int objId)
	{
		if (null != mainPlayer)
			mainPlayer.RPCServer(EPacketType.PT_InGame_PreGetItemBack, objId);
	}

	public void RequestGetLootItemBack(int objId,bool bTimeout)
	{
		RPCServer(EPacketType.PT_InGame_GetLootItemBack, objId,bTimeout);
	}

    public void RequestGetItemListBack(int objId, ItemSample[] items)
    {
        RPCServer(EPacketType.PT_InGame_GetItemListBack, objId, items, false);
    }

    static void RequestNewTeam()
    {
        if (null == mainPlayer)
            return;

        mainPlayer.RPCServer(EPacketType.PT_InGame_CreateTeam);
    }

    static void RequestJoinTeam(int teamId, bool freeJoin)
    {
        if (null == mainPlayer)
            return;

        mainPlayer.RPCServer(EPacketType.PT_InGame_JoinTeam, teamId, freeJoin);
    }

    static void RequestKickSB(PlayerNetwork player)
    {
        if (null == player || null == mainPlayer)
            return;

        mainPlayer.RPCServer(EPacketType.PT_InGame_KickSB, player.Id);
    }

    static void RequestApproveJoin(bool isAgree, PlayerNetwork player)
    {
        if (null == mainPlayer || null == player)
            return;

		if (isAgree)
			mainPlayer.RPCServer(EPacketType.PT_InGame_ApproveJoin, player.Id);
		else
			mainPlayer.RPCServer(EPacketType.PT_InGame_DenyJoin, player.Id);

		GroupNetwork.DelJoinRequest(player);
	}

    static void SyncAcceptJoinTeam(int inviterId, int teamId)
    {
        mainPlayer.RPCServer(EPacketType.PT_InGame_AcceptJoinTeam, inviterId, teamId);
    }

    public static void RequestInvitation(int id)
    {
        if (null == mainPlayer)
            return;

        mainPlayer.RPCServer(EPacketType.PT_InGame_Invitation, id);
    }

    public static void RequestDissolveTeam()
    {
        if (null == mainPlayer)
            return;

        mainPlayer.RPCServer(EPacketType.PT_InGame_DissolveTeam);
    }

    public static void RequestLeaderDeliver(int id)
    {
        if (null == mainPlayer)
            return;

        mainPlayer.RPCServer(EPacketType.PT_InGame_LeaderDeliver, id);
    }

    public static void RequestQuitTeam()
    {
        if (null == mainPlayer)
            return;

        mainPlayer.RPCServer(EPacketType.PT_InGame_QuitTeam);
    }

    public static void RequestReviveSB(int id)
    {
        if (null == mainPlayer)
            return;

        mainPlayer.RPCServer(EPacketType.PT_InGame_ReviveSB, id);
    }

    public static void RequestApprovalRevive(int id)
    {
        if (null == mainPlayer)
            return;

        mainPlayer.RPCServer(EPacketType.PT_InGame_ApprovalRevive, id);
    }

    public void RequestGameStarted(bool gameStarted)
    {
        _gameStarted = gameStarted;
        RPCServer(EPacketType.PT_InGame_GameStarted);
    }

    public static void RequestDismissNpc(int npcId)
    {
        if (null != mainPlayer)
            mainPlayer.RPCServer(EPacketType.PT_NPC_Dismiss, npcId);
    }

    public static void SyncAbnormalConditionStart(int entityId, int type, byte[] data)
    {
        if (null != mainPlayer)
        {
            if (null == data)
                mainPlayer.RPCServer(EPacketType.PT_InGame_AbnormalConditionStart, entityId, type, 0);
            else
                mainPlayer.RPCServer(EPacketType.PT_InGame_AbnormalConditionStart, entityId, type, 1, data);
        }
    }

    public static void SyncAbnormalConditionEnd(int entityId, int type)
    {
        if (null != mainPlayer)
            mainPlayer.RPCServer(EPacketType.PT_InGame_AbnormalConditionEnd, entityId, type);
    }

    public static void RequestServantRevive(int npcId, Vector3 pos)
    {
        if (null == mainPlayer)
            return;

        mainPlayer.RPCServer(EPacketType.PT_NPC_ServentRevive, npcId, pos);
    }

    public static void RequestServantAutoRevive(int npcId, Vector3 pos)
    {
        if (null == mainPlayer)
            return;

        mainPlayer.RPCServer(EPacketType.PT_NPC_Revive, npcId, pos);
    }

    public static void RequestSwitchArmorSuit(int newSuitIndex)
    {
        if (null != mainPlayer)
            mainPlayer.RPCServer(EPacketType.PT_InGame_SwitchArmorSuit, newSuitIndex);
    }

    public static void RequestEquipArmorPart(int itemID, int typeValue, int boneGroup, int boneIndex)
    {
        if (null != mainPlayer)
            mainPlayer.RPCServer(EPacketType.PT_InGame_EquipArmorPart, itemID, typeValue, boneGroup, boneIndex);
    }

    public static void RequestRemoveArmorPart(int boneGroup, int boneIndex, bool isDecoration)
    {
        if (null != mainPlayer)
            mainPlayer.RPCServer(EPacketType.PT_InGame_RemoveArmorPart, boneGroup, boneIndex, isDecoration);
    }

    public static void RequestSwitchArmorPartMirror(int boneGroup, int boneIndex, bool isDecoration)
    {
        if (null != mainPlayer)
            mainPlayer.RPCServer(EPacketType.PT_InGame_SwitchArmorPartMirror, boneGroup, boneIndex, isDecoration);
    }

    public static void SyncArmorPartPos(int boneGroup, int boneIndex, bool isDecoration, Vector3 position)
    {
        if (null != mainPlayer)
            mainPlayer.RPCServer(EPacketType.PT_InGame_SyncArmorPartPos, boneGroup, boneIndex, isDecoration, position);
    }

    public static void SyncArmorPartRot(int boneGroup, int boneIndex, bool isDecoration, Quaternion rotation)
    {
        if (null != mainPlayer)
            mainPlayer.RPCServer(EPacketType.PT_InGame_SyncArmorPartRot, boneGroup, boneIndex, isDecoration, rotation);
    }

    public static void SyncArmorPartScale(int boneGroup, int boneIndex, bool isDecoration, Vector3 scale)
    {
        if (null != mainPlayer)
            mainPlayer.RPCServer(EPacketType.PT_InGame_SyncArmorPartScale, boneGroup, boneIndex, isDecoration, scale);
    }

    public static void RequestNpcRecruit(int npcId,bool findPlayer = false)
    {
        if (null != mainPlayer)
            mainPlayer.RPCServer(EPacketType.PT_NPC_Recruit, npcId, findPlayer);
    }

    public static void RequestReqMonsterCtrl(int monsterEntityID)
    {
        if (null != mainPlayer)
            mainPlayer.RPCServer(EPacketType.PT_Mount_ReqMonsterCtrl, monsterEntityID);
    }
    public static void RequestAddRideMonster(int monsterEntityID)
    {
        if (null != mainPlayer)
            mainPlayer.RPCServer(EPacketType.PT_Mount_AddMountMonster, monsterEntityID);
    }
    public static void RequestDelMountMonster(int monsterEntityID)
    {
        if (null != mainPlayer)
            mainPlayer.RPCServer(EPacketType.PT_Mount_DelMountMonster, monsterEntityID);
    }
    public void RequestSyncRotation(Vector3 rotation)
    {
        URPCServer(EPacketType.PT_Mount_SyncPlayerRot,rotation);
    }

    public static void RequestServer(params object[] objs)
    {
        if (null != mainPlayer)
            mainPlayer.RPCServer(objs);
    }

    public static void RequestuseItem(int itemObjId)
    {
        if (null != mainPlayer)
            mainPlayer.RequestUseItem(itemObjId);
    }
    #endregion Network Request

    #region Action Callback APIs

    private void RPC_S2C_InitDataOK(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        _gameStarted = stream.Read<bool>();

        NetCmpt net = entity.GetCmpt<NetCmpt>();
        if (null != net)
            net.SetController(IsOwner);

        if (null != mainPlayer && TeamId == mainPlayer.TeamId)
            entity.SendMsg(EMsg.Net_Instantiate);

        if (IsOwner)
        {
            RaycastHit rayHit;
            if (Physics.Raycast(transform.position + 500f * Vector3.up, Vector3.down, out rayHit, 1000f,
                                (1 << Pathea.Layer.VFVoxelTerrain)
                + (1 << Pathea.Layer.GIEProductLayer)
                + (1 << Pathea.Layer.Unwalkable)
                + (1 << Pathea.Layer.SceneStatic)))
                transform.position = rayHit.point;

            _pos = mPlayerSynAttribute.mv3Postion = transform.position;
            StartCoroutine(PlayerMove());
			StartCoroutine(HeartBeat());
        }
        else
        {
            string msg = PELocalization.GetString(8000167);
            new PeTipMsg(msg.Replace("Playername%", RoleName), PeTipMsg.EMsgLevel.Norm, PeTipMsg.EMsgType.Misc);
        }

        ServerAdministrator.ProxyPlayerAdmin(this);
        RPCServer(EPacketType.PT_InGame_SKDAQueryEntityState);
        RequestAbnormalCondition();
        _initOk = true;
        if (PeGameMgr.IsMultiStory)
        {
            MultiStorySceneObjectManager.instance.RequestChangeScene(Id, _curSceneId);
        }
    }

    private void RPC_S2C_SceneObject(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        SceneObject[] objs = stream.Read<SceneObject[]>();

        foreach (SceneObject obj in objs)
        {
            switch (obj.Type)
            {
                case ESceneObjType.ITEM:
                    {
                        ItemObject item = ItemMgr.Instance.Get(obj.Id);
                        if (null == item)
                            continue;

                        Drag drag = item.GetCmpt<Drag>();
                        if (null == drag)
                            continue;

                        if (item.protoId == 1339)
                            KillNPC.ashBox_inScene++;

                        //lz-2018.02.05 飞船特殊处理
                        if (item.protoId == 1529)
                        {
                             GameObject go = (GameObject)UnityEngine .MonoBehaviour.Instantiate(Resources.Load(item.protoData.resourcePath), obj.Pos, obj.Rot);
                             go.name = "McTalk";
                        }
                        else
                        {
                            DragArticleAgent agent = DragArticleAgent.Create(drag, obj.Pos, obj.Scale, obj.Rot, obj.Id);
                            if (null != agent)
                            {
                                agent.ScenarioId = obj.ScenarioId;
                            }
                        }
                    }
                    break;

                case ESceneObjType.DOODAD:
                    {
                        SceneEntityPosAgent agent = DoodadEntityCreator.CreateAgent(obj.Pos, obj.ProtoId, obj.Scale, obj.Rot);
                        if (null != agent)
                        {
                            agent.ScenarioId = obj.ScenarioId;
                            agent.Id = obj.Id;
                            SceneMan.AddSceneObj(agent);
                        }
                    }
                    break;

                case ESceneObjType.EFFECT:
                    {
                        SceneStaticEffectAgent agent = SceneStaticEffectAgent.Create(obj.ProtoId, obj.Pos, obj.Rot, obj.Scale, obj.Id);
                        if (null != agent)
                        {
                            agent.ScenarioId = obj.ScenarioId;
                            SceneMan.AddSceneObj(agent);
                        }
                    }
                    break;
				case ESceneObjType.DROPITEM:
					{
                        if(ItemMgr.Instance.Get(obj.Id) == null)
                        {
                            Debug.LogError("LootItem is null id = " + obj.Id);
                            return;
                        }
					LootItemMgr.Instance.NetAddLootItem(obj.Pos,obj.Id);
					}
					break;
            }
        }
    }

    private void RPC_S2C_InitStatus(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        /*int state = */stream.Read<int>();
        /*bool onGround = */stream.Read<bool>();
        /*bool jump = */stream.Read<bool>();
        /*Vector3 shootPos = */stream.Read<Vector3>();

        //		if (null != player)
        //		{
        //			player.SetProxyShootPosition(shootPos);
        //			player.SetPlayerMoveGround(onGround);
        //			player.SetPlayerMoveState(state);
        //			player.SetPlayerMoveJump(jump);
        //		}
    }

    private void RPC_S2C_GetOnVehicle(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int id = stream.Read<int>();
        SeatIndex = stream.Read<int>();
        StartCoroutine(GetOnVehicle(id));
    }

    private void RPC_S2C_GetOffVehicle(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        Vector3 pos = stream.Read<Vector3>();
        EVCComponent seatType = stream.Read<EVCComponent>();

        GetOffVehicle(pos, seatType);

        if (null != MtCmpt)
            MtCmpt.EndAction(PEActionType.Drive);
        if (null != Trans)
            Trans.position = pos;
    }
    private void RPC_S2C_RepairVehicle(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int creationID = stream.Read<int>();
        CreationNetwork creation = (CreationNetwork)NetworkManager.Get(creationID);
        if (creation != null)
        {
            //repair

        }
    }

    private void RPC_S2C_TerrainDataOk(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        isTerrainDataOk = true;
    }

    private void RPC_S2C_RequestInit(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        transform.position = stream.Read<Vector3>();
        transform.rotation = Quaternion.Euler(0, stream.Read<float>(), 0);
        //float rotY = stream.Read<float>();
        //Quaternion rotation = Quaternion.Euler(0, rotY, 0);

        InitializePeEntity();
    }

    private void RPC_S2C_FastTransfer(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        Vector3 position = stream.Read<Vector3>();
		if(!IsOwner){
			transform.position = position;
			if(null != _move)
				_move.NetMoveTo(position, Vector3.zero, true);
		}

        if (IsOwner)
        {
            FastTravel.TravelTo(position);

            Pathea.ServantLeaderCmpt leader = entity.GetCmpt<Pathea.ServantLeaderCmpt>();
            if (null == leader)
                return;

            foreach (Pathea.NpcCmpt npc in leader.mFollowers)
            {
                if (null == npc)
                    continue;

                PeTrans trans = npc.Entity.peTrans;
                if (null == trans)
                    continue;
                trans.position = PETools.PEUtil.GetRandomPosition(position, 1.5f, 3);
                (npc.Net as AiAdNpcNetwork).NpcMove();
            }
            foreach (var iter in leader.mForcedFollowers)
            {
                if (null == iter)
                    continue;

                PeTrans trans = iter.Entity.peTrans;
                if (null == trans)
                    continue;
                trans.position = PETools.PEUtil.GetRandomPosition(position, 1.5f, 3);
                (iter.Net as AiAdNpcNetwork).NpcMove();
            }

            GC.Collect();
            Resources.UnloadUnusedAssets();
        }
    }

    void RPC_S2C_SwitchScene(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int worldIndex = stream.Read<int>();
        Vector3 pos = stream.Read<Vector3>();

        Pathea.FastTravelMgr.Instance.TravelTo(worldIndex, pos);
    }

    void RPC_S2C_DelSceneObjects(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int[] objIds = stream.Read<int[]>();
        foreach (int id in objIds)
        {
            ISceneObjAgent agent = SceneMan.GetSceneObjById(id);
            if (!Equals(null, agent))
                SceneMan.RemoveSceneObj(agent);
			LootItemMgr.Instance.RemoveLootItem(id);
        }
    }

    private void RPC_S2C_PlayerMovePosition(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
		_pos = stream.Read<Vector3>();
        byte moveState = stream.Read<byte>();
        float rotY = stream.Read<float>();
        double time = stream.Read<double>();

        rot = Quaternion.Euler(0, rotY, 0);
        transform.position = _pos;        
        transform.rotation = rot;
        if (_move == null)
            return;

        _move.AddNetTransInfo(_pos, rot.eulerAngles, (SpeedState)moveState, time);
    }

    private void RPC_S2C_PlayerMoveRotationY(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        float rotY;
        if (stream.TryRead<float>(out rotY))
        {
            rot = Quaternion.Euler(0, rotY, 0);
            transform.rotation = rot;

            if (null != entity && null != _transCmpt)
                _transCmpt.rotation = rot;
        }
    }

    private void RPC_S2C_PlayerMovePlayerState(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        byte state;
        if (stream.TryRead<byte>(out state))
        {
            if (_move != null)
                _move.speed = (SpeedState)state;
        }

        //if(null != player)
        //    player.SetPlayerMoveState(state);
    }

    private void RPC_S2C_PlayerMoveGrounded(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        //bool onGround = stream.Read<bool>();
        //if(null != player)
        //    player.SetPlayerMoveGround(onGround);
    }

    private void RPC_S2C_PlayerMoveShootTarget(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        /*Vector3 shootPos = */stream.Read<Vector3>();
        //if (null != player)
        //    player.SetProxyShootPosition(shootPos);
    }

    private void RPC_S2C_SyncGliderStatus(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        /*bool status = */stream.Read<bool>();

        //if (null != player) {
        //    if (player.mPlayerDataBlock.mPlayerEqupmentManager._JetPack is Glider)
        //    {
        //        Glider glider = player.mPlayerDataBlock.mPlayerEqupmentManager._JetPack as Glider;
        //        if (status)
        //            glider.StartGlide();
        //        else
        //            glider.EndGlide();
        //    }
        //}
    }

    private void RPC_S2C_SyncParachuteStatus(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        /*bool status = */stream.Read<bool>();

        //if (null != player) {
        //    if (player.mPlayerDataBlock.mPlayerEqupmentManager._Parachute is Parachute)
        //    {
        //        Parachute parachute = player.mPlayerDataBlock.mPlayerEqupmentManager._Parachute as Parachute;
        //        if (status)
        //            parachute.StartParachute();
        //        else
        //            parachute.EndParachute();
        //    }
        //}
    }

    private void RPC_S2C_SyncJetPackStatus(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        /*bool status = */stream.Read<bool>();

        //if (null != player) {
        //    if (player.mPlayerDataBlock.mPlayerEqupmentManager._JetPack is JetPack) {
        //        JetPack jetPack = player.mPlayerDataBlock.mPlayerEqupmentManager._JetPack as JetPack;
        //        jetPack.Boost (status);
        //    }
        //}
    }

    private void RPC_S2C_PlayerRevive(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        float hp = stream.Read<float>();
        float oxygen = stream.Read<float>();
        float stamina = stream.Read<float>();
        transform.position = stream.Read<Vector3>();
        transform.rotation = stream.Read<Quaternion>();
        if (runner == null || runner.SkEntityPE == null)
            return;
        runner.SkEntityPE.SetAttribute(AttribType.Hp, hp);
        runner.SkEntityPE.SetAttribute(AttribType.Oxygen, oxygen);
        runner.SkEntityPE.SetAttribute(AttribType.Stamina, stamina);

        if (null != entity && null != _move)
            _move.NetMoveTo(transform.position, Vector3.zero, true);

        MotionMgrCmpt motionMgrCmpt = runner.SkEntityPE.Entity.GetCmpt<MotionMgrCmpt>();
        if (motionMgrCmpt != null)
		{
			PEActionParamB param = PEActionParamB.param;
			param.b = true;
			motionMgrCmpt.DoActionImmediately(PEActionType.Revive, param);
		}
        if (Equals(mainPlayer, this))
            GameUI.Instance.mRevive.Hide();
    }

    private void RPC_S2C_PlayerReset(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        if (!IsOwner)
            return;

        //if (null != player)
        //{
        //    if (player.GetItemNum(31200001) <= 0)
        //    {
        //        IntVector2 posXZ = RandomTownUtil.Instance.GetSpawnPos();
        //        Vector3 warpPos = new Vector3(posXZ.x, VFDataRTGen.GetPosHeight(posXZ), posXZ.y);
        //        RPCServer(EPacketType.PT_InGame_PlayerReset, warpPos);
        //    }
        //    else
        //    {
        //        GameUI.Instance.mReviveGui.Show();
        //        GameUI.Instance.mReviveGui.SetPlayerMode(true);
        //    }
        //}
    }

    void RPC_S2C_SetShortcut(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        Pathea.PlayerPackageCmpt cmpt = entity.GetCmpt<Pathea.PlayerPackageCmpt>();
        if (null == cmpt)
            return;

        int srcIndex = stream.Read<int>();
        int srcId = stream.Read<int>();
        int destIndex = stream.Read<int>();
        int destId = stream.Read<int>();

        if (-1 != srcIndex)
        {
            ItemObject item = ItemMgr.Instance.Get(srcId);
            if (null == item)
                cmpt.shortCutSlotList.PutItem(null, srcIndex);
            else
                cmpt.shortCutSlotList.PutItemObj(item, srcIndex);
        }

        if (-1 != destIndex)
        {
            ItemObject item = ItemMgr.Instance.Get(destId);
            if (null == item)
                cmpt.shortCutSlotList.PutItem(null, destIndex);
            else
                cmpt.shortCutSlotList.PutItemObj(item, destIndex);
        }
    }

    private void RPC_S2C_SendMsg(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        /*EMsgType msgType = */stream.Read<EMsgType>();
        string msg = stream.Read<string>();

        if (null != UITalkwithctr.Instance && UITalkwithctr.Instance.isShow)
        {
            if (Id == mainPlayer.Id)
                UITalkwithctr.Instance.AddTalk(RoleName, msg, "99C68B");
            else
                UITalkwithctr.Instance.AddTalk(RoleName, msg, "EDB1A6");
        }
        //if (null != GameGui_N.Instance)
        //GameGui_N.Instance.mChatGUI.AddChat(NetworkBase.name, msg, (int)msgType);
    }

    private void RPC_S2C_ApplyHpChange(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        float damage;
        float life;
        uLink.NetworkViewID viewID;

        stream.TryRead<float>(out damage);
        stream.TryRead<float>(out life);
        stream.TryRead<uLink.NetworkViewID>(out viewID);

        CommonInterface caster = null;
        uLink.NetworkView view = uLink.NetworkView.Find(viewID);
        if (null != view)
        {
            NetworkInterface network = view.GetComponent<NetworkInterface>();
            if (null != network && null != network.Runner)
                caster = network.Runner;
        }

        if (null != Runner)
            Runner.NetworkApplyDamage(caster, damage, (int)life);
    }

    private void RPC_S2C_ApplyComfortChange(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        /*float comfortChange = */stream.Read<float>();
        //if (null != player)
        //    player.NetworkComfortChange(comfortChange);
    }

    private void RPC_S2C_ApplySatiationChange(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        /*float satiationChange = */stream.Read<float>();
        //if (null != player)
        //    player.NetworkSatiationChange(satiationChange);
    }

    private void RPC_S2C_Death(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int casterId = stream.Read<int>();
        NetworkInterface net = NetworkInterface.Get(casterId);
        if (net as SkNetworkInterface)
        {
            if (net.Runner != null && net.Runner.SkEntityPE != null)
            {
                //SkAliveEntity entity = net.Runner.SkEntityPE;
            }
        }
        //		float damage;
        //		float life;
        //		uLink.NetworkViewID viewID;
        //
        //		stream.TryRead<float>(out damage);
        //		stream.TryRead<float>(out life);
        //		stream.TryRead<uLink.NetworkViewID>(out viewID);
        //
        //		CommonInterface caster = null;
        //		uLink.NetworkView view = uLink.NetworkView.Find(viewID);
        //		if (null != view)
        //		{
        //			NetworkInterface network = view.GetComponent<NetworkInterface>();
        //			if (null != network && null != network.Runner)
        //				caster = network.Runner;
        //		}

        //Player player = Runner as Player;
        //if (null != player)
        //    player.NetworkDeath(caster);
    }

    private void RPC_S2C_ErrorMsgBox(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        string errInfo = stream.Read<string>();
        MessageBox_N.ShowOkBox(errInfo);
    }

    private void RPC_S2C_ErrorMsg(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        string errInfo = stream.Read<string>();
        new PeTipMsg(errInfo, PeTipMsg.EMsgLevel.Error);
    }

    private void RPC_S2C_ErrorMsgCode(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int strIndex = stream.Read<int>();
        string errInfo = PELocalization.GetString(strIndex);
        new PeTipMsg(errInfo, PeTipMsg.EMsgLevel.Error);
    }

    private void RPC_S2C_Test_PutOnEquipment(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        /*int objid = */stream.Read<int>();
        EquipmentCmpt temp = entity.GetGameObject().GetComponent<EquipmentCmpt>();
        if (temp != null)
        {
            ItemObject item = ItemMgr.Instance.CreateItem(1);
            temp.PutOnEquipment(item, false, null, true);
        }
    }

    void RPC_S2C_CreateNewTeam(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
    }

    void RPC_S2C_JoinTeam(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int id = stream.Read<int>();

        PlayerNetwork player = PlayerNetwork.Get<PlayerNetwork>(id);
        if (null == player)
            return;

        GroupNetwork.AddJoinRequest(TeamId, player);

        if (null != CSUI_TeamInfoMgr.Intance)
            CSUI_TeamInfoMgr.Intance.JoinApply(TeamId);

        OnTeamChanged(TeamId);
    }

    void RPC_S2C_ApproveJoin(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
    }

    void RPC_S2C_DenyJoin(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int id = stream.Read<int>();
        PlayerNetwork player = Get<PlayerNetwork>(id);
        if (null != player)
            //lz-2016.10.31 You have been denied joining!
            new PeTipMsg(PELocalization.GetString(8000856), PeTipMsg.EMsgLevel.Warning);
    }

    void RPC_S2C_Invitation(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int id = stream.Read<int>();
        PlayerNetwork inviter = PlayerNetwork.Get<PlayerNetwork>(id);
        if (null == inviter)
            return;

        if (null != CSUI_TeamInfoMgr.Intance)
            CSUI_TeamInfoMgr.Intance.Invitation(inviter);
    }

    void RPC_S2C_KickSB(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        //lz-2016.10.31 You have been kicked out of the team!
        new PeTipMsg(PELocalization.GetString(8000857), PeTipMsg.EMsgLevel.Warning);
	}

    void RPC_S2C_LeaderDeliver(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
    }

    void RPC_S2C_QuitTeam(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
    }

    void RPC_S2C_DissolveTeam(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
    }

    void RPC_S2C_TeamInfo(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int teamId = stream.Read<int>();
        int leaderId = stream.Read<int>();

		TeamData td = GroupNetwork.AddToTeam(teamId, this);
		if (null == td)
			return;

		td.SetLeader(leaderId);
		int curTeamId = TeamId;
		SetTeamId(teamId);

		if (-1 != curTeamId && teamId != curTeamId)
		{
			if (Id == mainPlayerId)
				GroupNetwork.ClearJoinRequest();

			ForceSetting.RemoveAllyForce(originTeamId, curTeamId);
			GroupNetwork.RemoveFromTeam(curTeamId, this);

			OnTeamChanged(curTeamId);

			if (null != CSUI_TeamInfoMgr.Intance)
				CSUI_TeamInfoMgr.Intance.RefreshTeamGrid(curTeamId);
		}

		GroupNetwork.AddToTeam(teamId, this);
		ForceSetting.AddAllyForce(originTeamId, teamId);

		OnTeamChanged(teamId);

		if (null != CSUI_TeamInfoMgr.Intance)
			CSUI_TeamInfoMgr.Intance.RefreshTeamGrid(teamId);
	}

    void RPC_S2C_AcceptJoinTeam(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
    }

    void RPC_S2C_ReviveSB(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int id = stream.Read<int>();

        PlayerNetwork p = Get<PlayerNetwork>(id);
        if (null == p)
            return;

        // TODO:approval or deny
        MessageBox_N.ShowYNBox(string.Format(PELocalization.GetString(8000503),p.RoleName), () => RequestApprovalRevive(id));
    }

    void RPC_S2C_FoundMapLable(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int[] mapLables = stream.Read<int[]>();
        for (int i = 0; i < mapLables.Length; i++)
        {
            PeMap.StaticPoint.StaticPointBeFound(mapLables[i]);
        }
    }

    void RPC_S2C_SyncArmorInfo(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        byte[] data = stream.Read<byte[]>();
        if (data.Length > 0)
        {
            using (MemoryStream ms = new MemoryStream(data))
            using (BinaryReader r = new BinaryReader(ms))
            {
                if (PlayerArmor != null)
                {
                    PlayerArmor.Deserialize(r);
                    PlayerArmor.Init(this);
                }
            }
        }
    }

    void RPC_S2C_ArmorDurability(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int entityId = stream.Read<int>();
        int[] equipIds = stream.Read<int[]>();
        float[] fValue = stream.Read<float[]>();
        for (int i = 0; i < equipIds.Length; i++)
        {
            ItemObject item = ItemMgr.Instance.Get(equipIds[i]);
            if (item != null)
            {
                NetworkInterface net = NetworkInterface.Get(entityId);
                if (net != null && net is PlayerNetwork)
                {
                    Durability d = item.GetCmpt<Durability>();

                    if (d != null) d.floatValue.current = fValue[i];

                    if (item.protoData.tabIndex == 3 && d.floatValue.current <= 0f)
                    {
                        (net as PlayerNetwork).PlayerArmor.RemoveBufferWhenBroken(item);
                    }
                }
            }
        }
    }

    void RPC_C2S_SwitchArmorSuit(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int newSuitIndex = stream.Read<int>();
        bool sucess = stream.Read<bool>();

        PlayerArmor.S2C_SwitchArmorSuit(newSuitIndex, sucess);
    }

    void RPC_C2S_EquipArmorPart(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int itemID = stream.Read<int>();
        int typeValue = stream.Read<int>();
        int boneGroup = stream.Read<int>();
        int boneIndex = stream.Read<int>();
        bool sucess = stream.Read<bool>();

        PlayerArmor.S2C_EquipArmorPartFromPackage(itemID, typeValue, boneGroup, boneIndex, sucess);
    }

    void RPC_C2S_RemoveArmorPart(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int boneGroup = stream.Read<int>();
        int boneIndex = stream.Read<int>();
        bool isDecoration = stream.Read<bool>();
        bool sucess = stream.Read<bool>();

        PlayerArmor.S2C_RemoveArmorPart(boneGroup, boneIndex, isDecoration, sucess);
    }

    void RPC_C2S_SwitchArmorPartMirror(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int boneGroup = stream.Read<int>();
        int boneIndex = stream.Read<int>();
        bool isDecoration = stream.Read<bool>();
        bool sucess = stream.Read<bool>();

        PlayerArmor.S2C_SwitchArmorPartMirror(boneGroup, boneIndex, isDecoration, sucess);
    }

    void RPC_C2S_SyncArmorPartPos(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int boneGroup = stream.Read<int>();
        int boneIndex = stream.Read<int>();
        bool isDecoration = stream.Read<bool>();
        Vector3 pos = stream.Read<Vector3>();

        PlayerArmor.S2C_SyncArmorPartPosition(boneGroup, boneIndex, isDecoration, pos);
    }

    void RPC_C2S_SyncArmorPartRot(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int boneGroup = stream.Read<int>();
        int boneIndex = stream.Read<int>();
        bool isDecoration = stream.Read<bool>();
        Quaternion rot = stream.Read<Quaternion>();

        PlayerArmor.S2C_SyncArmorPartRotation(boneGroup, boneIndex, isDecoration, rot);
    }

    void RPC_C2S_SyncArmorPartScale(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int boneGroup = stream.Read<int>();
        int boneIndex = stream.Read<int>();
        bool isDecoration = stream.Read<bool>();
        Vector3 scale = stream.Read<Vector3>();

        PlayerArmor.S2C_SyncArmorPartScale(boneGroup, boneIndex, isDecoration, scale);
    }

    void RPC_S2C_Mgr_InitData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
//        int teamId = stream.Read<int>();
//        List<CSTreatment> cstList = stream.Read<CSTreatment[]>().ToList();
//
//        CSMgCreator creator = MultiColonyManager.GetCreator(teamId);
//        creator.InitMultiCSTreatment(cstList);
		byte[] packData = stream.Read<byte[]>();
		CSMgCreator creator = MultiColonyManager.GetCreator(BaseTeamId);
		creator.InitMultiData(packData);

    }

    void RPC_S2C_ScenarioId(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int scenarioId = stream.Read<int>();

        PeEntity entity = EntityMgr.Instance.Get(Id);
        if (null != entity)
            entity.scenarioId = scenarioId;
    }

    void RPC_S2C_CustomCheckResult(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int requestId = stream.Read<int>();
        bool result = stream.Read<bool>();
        ScenarioRTL.ConditionReq.AlterReq(requestId, result);
    }

    void RPC_S2C_CustomAddQuest(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        byte[] data = stream.Read<byte[]>();

        BufferHelper.Import(data, r =>
        {
            PeCustom.OBJECT obj;
            BufferHelper.ReadObject(r, out obj);
            int id = r.ReadInt32();
            string text = r.ReadString();

            PeCustom.PeCustomScene.Self.scenario.dialogMgr.SetQuest(obj.Group, obj.Id, id, text);
        });
    }

    void RPC_S2C_CustomRemoveQuest(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        byte[] data = stream.Read<byte[]>();

        BufferHelper.Import(data, r =>
        {
            PeCustom.OBJECT obj;
            BufferHelper.ReadObject(r, out obj);
            int id = r.ReadInt32();

            PeCustom.PeCustomScene.Self.scenario.dialogMgr.RemoveQuest(obj.Group, obj.Id, id);
        });
    }

    void RPC_S2C_CustomAddChoice(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int id = stream.Read<int>();
        string text = stream.Read<string>();

        PeCustom.PeCustomScene.Self.scenario.dialogMgr.AddChoose(id, text);
    }

    void RPC_S2C_EnableSpawn(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        byte[] data = stream.Read<byte[]>();

        PETools.Serialize.Import(data, r =>
        {
            PeCustom.OBJECT spawn;
            BufferHelper.ReadObject(r, out spawn);
            PeCustom.PeScenarioUtility.EnableSpawnPoint(spawn, true);
        });
    }

    void RPC_S2C_DisableSpawn(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        byte[] data = stream.Read<byte[]>();

        PETools.Serialize.Import(data, r =>
        {
            PeCustom.OBJECT spawn;
            BufferHelper.ReadObject(r, out spawn);
            PeCustom.PeScenarioUtility.EnableSpawnPoint(spawn, false);
        });
    }

    void RPC_S2C_OrderTarget(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        byte[] data = stream.Read<byte[]>();

        PETools.Serialize.Import(data, r =>
        {
            PeCustom.OBJECT obj;
            PeCustom.OBJECT tar;
            PeCustom.ECommand cmd;

            BufferHelper.ReadObject(r, out obj);
            BufferHelper.ReadObject(r, out tar);
            cmd = (PeCustom.ECommand)r.ReadByte();

            Pathea.PeEntity entity = PeCustom.PeScenarioUtility.GetEntity(obj);
            Pathea.PeEntity target = PeCustom.PeScenarioUtility.GetEntity(tar);
            if (entity != null && target != null)
            {
                if ((entity.proto == Pathea.EEntityProto.Npc ||
                    entity.proto == Pathea.EEntityProto.RandomNpc ||
                    entity.proto == Pathea.EEntityProto.Monster) &&
                    entity.requestCmpt != null)
                {
                    if (cmd == PeCustom.ECommand.MoveTo)
                        entity.requestCmpt.Register(Pathea.EReqType.FollowTarget, target.Id);
                    else if (cmd == PeCustom.ECommand.FaceAt)
                        entity.requestCmpt.Register(Pathea.EReqType.Dialogue, "", target.peTrans);
                }
            }
        });
    }

    void RPC_S2C_CancelOrder(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        byte[] data = stream.Read<byte[]>();

        PETools.Serialize.Import(data, r =>
        {
            PeCustom.OBJECT obj;
            BufferHelper.ReadObject(r, out obj);

            Pathea.PeEntity entity = PeCustom.PeScenarioUtility.GetEntity(obj);
            if (entity != null && entity.requestCmpt != null)
            {
                if (entity.requestCmpt.Contains(Pathea.EReqType.Dialogue))
                    entity.requestCmpt.RemoveRequest(Pathea.EReqType.Dialogue);
                if (entity.requestCmpt.Contains(Pathea.EReqType.MoveToPoint))
                    entity.requestCmpt.RemoveRequest(Pathea.EReqType.MoveToPoint);
                if (entity.requestCmpt.Contains(Pathea.EReqType.FollowTarget))
                    entity.requestCmpt.RemoveRequest(Pathea.EReqType.FollowTarget);
            }
        });
    }

    void RPC_S2C_PlayAnimation(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        byte[] data = stream.Read<byte[]>();

        PETools.Serialize.Import(data, r =>
        {
            PeCustom.OBJECT obj;
            string name;

            BufferHelper.ReadObject(r, out obj);
            name = r.ReadString();

            Pathea.PeEntity entity = PeCustom.PeScenarioUtility.GetEntity(obj);
            if (null == entity)
                return;

            if (obj.isCurrentPlayer)
            {
                PeCustom.PlayAnimAction.playerAniming = true;
                entity.animCmpt.AnimEvtString += delegate (string param)
                {
                    if (param == "OnCustomAniEnd")
                    {
                        if (obj.isCurrentPlayer)
                            PeCustom.PlayAnimAction.playerAniming = false;
                    }
                };
            }

            string type = name.Split('_')[name.Split('_').Length - 1];
            if (type == "Once")
                entity.animCmpt.SetTrigger(name);
            else if (type == "Muti")
                entity.animCmpt.SetBool(name, true);
        });
    }


    void RPC_S2C_StopAnimation(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        byte[] data = stream.Read<byte[]>();

        PETools.Serialize.Import(data, r =>
        {
            PeCustom.OBJECT obj;
            BufferHelper.ReadObject(r, out obj);

            Pathea.PeEntity entity = PeCustom.PeScenarioUtility.GetEntity(obj);
            if (null == entity)
                return;

            //TODO:把ParametersBool设置为false(等待动作表)
            entity.animCmpt.SetTrigger("Custom_ResetAni");
        });
    }
    
	void RPC_S2C_CurTeamId(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int curTeamId = stream.Read<int>();
		ForceSetting.Instance.InitGameForces(curTeamId);
	}

	void RPC_S2C_ClearGrass(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] grassData = stream.Read<byte[]>();
		PETools.Serialize.Import(grassData, (r) =>
		{
			int count = r.ReadInt32();

			for (int i = 0; i < count; i++)
			{
				Vector3 pos;
				BufferHelper.ReadVector3(r, out pos);
				DigTerrainManager.DeleteGrass(pos);
			}
		});
	}

	void RPC_S2C_ClearTree(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] grassData = stream.Read<byte[]>();
		PETools.Serialize.Import(grassData, (r) =>
		{
			int count = r.ReadInt32();

			for (int i = 0; i < count; i++)
			{
				Vector3 pos;
				BufferHelper.ReadVector3(r, out pos);
				DigTerrainManager.DeleteTree(pos);
			}
		});
	}

	void RPC_S2C_CheatingChecked(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		new PeTipMsg(PELocalization.GetString(8000895), PeTipMsg.EMsgLevel.Warning);
		MessageBox_N.ShowOkBox(PELocalization.GetString(8000895));
	}

    private void RPC_S2C_ReqMonsterCtrl(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int playerEntityID = stream.Read<int>();
        int monsterEntityID = stream.Read<int>();
        PeEntity player = EntityMgr.Instance.Get(playerEntityID);
        PeEntity monsterEntity = EntityMgr.Instance.Get(monsterEntityID);
        if (player && player.operateCmpt && monsterEntity)
        {
            if (monsterEntity && monsterEntity.biologyViewCmpt && monsterEntity.biologyViewCmpt.biologyViewRoot && monsterEntity.biologyViewCmpt.biologyViewRoot.modelController)
            {
                MousePickRides rides = monsterEntity.biologyViewCmpt.biologyViewRoot.modelController.GetComponent<MousePickRides>();
                if (rides)
                {
                   // rides.ExecRide(player);
                    PlayerPackageCmpt pack = player.packageCmpt as PlayerPackageCmpt;
                    ItemAsset.ItemObject item = pack.package.FindItemByProtoId(MousePickRides.RideItemID);
                    if (null != item && rides.ExecRide(player))
                    {
                        RequestItemCost(playerEntityID, item.instanceId,1);
                       // player.UseItem.Request(item);
                    }
                }
            }

        }
    }

    private void RPC_S2C_AddMountMonster(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int playerID = stream.Read<int>();
        int monsterID = stream.Read<int>();
        AiNetwork aiMonster = AiNetwork.Get<AiNetwork>(monsterID);
        if (null != aiMonster && aiMonster._entity)
            aiMonster._entity.SetAttribute(AttribType.DefaultPlayerID, playerID);
        //lz-2017.02.17 收到有人骑坐骑，本地建立关系
        PeEntity player = EntityMgr.Instance.Get(playerID);
        PeEntity monster = EntityMgr.Instance.Get(monsterID);
        if (player && player.mountCmpt && monster)
            player.mountCmpt.SetMount(monster); 
    }

    private void RPC_S2C_DelMountMonste(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int playerID = stream.Read<int>();
        int monsterID = stream.Read<int>();
        int backupTeamID = stream.Read<int>();
        AiNetwork aiMonster = AiNetwork.Get<AiNetwork>(monsterID);
        if (null != aiMonster && aiMonster._entity)
            aiMonster._entity.SetAttribute(AttribType.DefaultPlayerID, backupTeamID);
        //lz-2017.02.17 收到有人下坐骑，本地解除关系
        PeEntity player = EntityMgr.Instance.Get(playerID);
        if (player && player.mountCmpt)
            player.mountCmpt.DelMount();
    }

    private void RPC_S2C_SyncPlayerRot(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        Vector3 rot;
        if (stream.TryRead<Vector3>(out rot))
        {
            base.rot = Quaternion.Euler(rot);
            transform.rotation = base.rot;

            if (null != entity && null != _transCmpt)
                _transCmpt.rotation = base.rot;
        }
    }

    void RPC_S2C_Statistics(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        string msg = stream.Read<string>();
        if (null != UITalkwithctr.Instance && UITalkwithctr.Instance.isShow)
        {
            UITalkwithctr.Instance.AddTalk("System", msg, "99C68B");
        }            
    }

    #endregion Action Callback APIs

    ////��������(ɾ�����ߣ���ӵ���)
    //[RPC]
    //void RPC_S2C_OperatingItem(uLink.BitStream stream)
    //{
    //	int nItemID= stream.Read<int>();
    //	if (null != PlayerFactory.mMainPlayer && null != PlayerFactory.mMainPlayer.m_PlayerMission)
    //	{
    //		PlayerFactory.mMainPlayer.m_PlayerMission.CheckItemMissionList(nItemID);
    //	}
    //}

    //[RPC]
    //void RPC_S2C_SyncSPTerrainRects(uLink.BitStream stream)
    //{
    //	SPTerrainRect[] SPTerrainRects = stream.Read<SPTerrainRect[]>();
    //	foreach(SPTerrainRect spTerrain in SPTerrainRects )
    //	{
    //		SPTerrainEvent.instance.AddSPTerrainRect(spTerrain);
    //	}
    //}
}