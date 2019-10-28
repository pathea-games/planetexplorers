using AiAsset;
using CustomData;
using ItemAsset;
using Pathea;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtTrans;
using SkillAsset;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TownData;
using UnityEngine;

public partial class AiAdNpcNetwork : AiNetwork
{
	protected int npcType;
	protected int originTeam = -1;
	protected int mLordPlayerId = -1;
    protected int mColonyLordPlayerId = -1;
    protected float rotY;

	public NpcMissionData useData;
	protected NpcCmpt _npcCmpt;
	protected Vector3 spawnPos;

	protected bool isStand;

	internal CreationNetwork DriveCreation { get; set; }
	internal int SeatIndex { get; set; }
	public int LordPlayerId { get { return mLordPlayerId; } }
	public bool _npcMissionInited = false;
    public bool bForcedServant = false;
    public int _mountId;
    private string customName;

    public NpcCmpt npcCmpt
	{
		get
		{
			if(_npcCmpt == null)
			{
				PeEntity entity = EntityMgr.Instance.Get(Id);
				if(entity != null)
					_npcCmpt = entity.GetCmpt<NpcCmpt>();
			}
			return _npcCmpt;
		}
	}

	protected override void OnPEInstantiate(uLink.NetworkMessageInfo info)
	{
		_id = info.networkView.initialData.Read<int>();
		_scale = info.networkView.initialData.Read<float>();
		_externId = info.networkView.initialData.Read<int>();
		npcType = info.networkView.initialData.Read<int>();
		authId = info.networkView.initialData.Read<int>();
        isStand = info.networkView.initialData.Read<bool>();
        rotY = info.networkView.initialData.Read<float>();
        bForcedServant = info.networkView.initialData.Read<bool>();
        customName = info.networkView.initialData.Read<string>();
       // mColonyLordPlayerId = info.networkView.initialData.Read<int>();

        _pos = spawnPos = transform.position;
		originTeam = _teamId = -1;
		mLordPlayerId = -1;
	}

	protected override void OnPEStart()
	{
		BindSkAction();

		BindAction(EPacketType.PT_NPC_InitData, RPC_S2C_InitData);
		BindAction(EPacketType.PT_InGame_SetController, RPC_S2C_SetController);
		BindAction(EPacketType.PT_InGame_LostController, RPC_S2C_LostController);
		BindAction(EPacketType.PT_NPC_HPChange, RPC_S2C_ApplyHpChange);
		BindAction(EPacketType.PT_NPC_Death, RPC_S2C_Death);
		BindAction(EPacketType.PT_NPC_Turn, RPC_S2C_Turn);
		BindAction(EPacketType.PT_NPC_Revive, RPC_S2C_NpcRevive);
		BindAction(EPacketType.PT_NPC_Dismiss, RPC_S2C_DismissByPlayer);
		BindAction(EPacketType.PT_NPC_Recruit, RPC_S2C_RecruitByPlayer);
		BindAction(EPacketType.PT_NPC_WorkState, RPC_S2C_SyncWorkState);
		BindAction(EPacketType.PT_NPC_Move, RPC_NPCMove);
        BindAction(EPacketType.PT_NPC_ForceMove, NPC_ForceMove);
		BindAction(EPacketType.PT_NPC_RotY, RPC_S2C_NetRotation);
		BindAction(EPacketType.PT_NPC_MissionFlag, RPC_MissionFlag);
		BindAction(EPacketType.PT_NPC_PacageIndex, RPC_PackageIndex);
		BindAction(EPacketType.PT_NPC_Money, RPC_S2C_SyncMoney);
        BindAction(EPacketType.PT_NPC_SyncTeamID, RPC_S2C_SyncTeamID);
        BindAction(EPacketType.PT_NPC_Equips, RPC_S2C_NpcEquips);
		BindAction(EPacketType.PT_NPC_Items, RPC_S2C_NpcItems);
		BindAction(EPacketType.PT_NPC_Mission, RPC_S2C_NpcMisstion);
		BindAction(EPacketType.PT_NPC_MissionState,RPC_S2C_MissionState);
		BindAction(EPacketType.PT_NPC_RequestAiOp,RPC_S2C_RequestAiOp);
		BindAction(EPacketType.PT_NPC_Mount,RPC_S2C_Mount);
		BindAction(EPacketType.PT_NPC_UpdateCampsite,RPC_S2C_UpdateCampsite);
		BindAction(EPacketType.PT_NPC_State,RPC_S2C_State);
		BindAction(EPacketType.PT_InGame_GetOnVehicle,RPC_S2C_GetOn);
		BindAction(EPacketType.PT_InGame_GetOffVehicle,RPC_S2C_GetOff);
		BindAction(EPacketType.PT_NPC_ExternData, RPC_S2C_ExternData);
		BindAction(EPacketType.PT_NPC_Skill, RPC_S2C_NpcSkill);

		BindAction(EPacketType.PT_AI_RifleAim, RPC_S2C_RifleAim);
		BindAction(EPacketType.PT_AI_IKPosWeight, RPC_S2C_SetIKPositionWeight);
		BindAction(EPacketType.PT_AI_IKPosition, RPC_S2C_SetIKPosition);
		BindAction(EPacketType.PT_AI_IKRotWeight, RPC_S2C_SetIKRotationWeight);
		BindAction(EPacketType.PT_AI_IKRotation, RPC_S2C_SetIKRotation);

		BindAction(EPacketType.PT_AI_BoolString, RPC_S2C_SetBool_String);
		BindAction(EPacketType.PT_AI_BoolInt, RPC_S2C_SetBool_Int);
		BindAction(EPacketType.PT_AI_VectorString, RPC_S2C_SetVector_String);
		BindAction(EPacketType.PT_AI_VectorInt, RPC_S2C_SetVector_Int);
		BindAction(EPacketType.PT_AI_IntString, RPC_S2C_SetInteger_String);
		BindAction(EPacketType.PT_AI_IntInt, RPC_S2C_SetInteger_Int);
		BindAction(EPacketType.PT_AI_LayerWeight, RPC_S2C_SetLayerWeight);
		BindAction(EPacketType.PT_AI_LookAtWeight, RPC_S2C_SetLookAtWeight);
		BindAction(EPacketType.PT_AI_LookAtPos, RPC_S2C_SetLookAtPosition);

		//CLN NPC
		BindAction(EPacketType.PT_CL_CLN_InitData, RPC_S2C_CLN_InitData);
		BindAction(EPacketType.PT_CL_CLN_SetState, RPC_S2C_CLN_SetState);
		BindAction(EPacketType.PT_CL_CLN_SetDwellingsID, RPC_S2C_CLN_SetDwellingsID);
		BindAction(EPacketType.PT_CL_CLN_SetWorkRoomID, RPC_S2C_CLN_SetWorkRoomID);
		BindAction(EPacketType.PT_CL_CLN_SetOccupation, RPC_S2C_CLN_SetOccupation);
		BindAction(EPacketType.PT_CL_CLN_SetWorkMode, RPC_S2C_CLN_SetWorkMode);
//		BindAction(EPacketType.PT_CL_CLN_SetGuardPos, RPC_S2C_CLN_SetGuardPos);
		BindAction(EPacketType.PT_CL_CLN_PlantGetBack, RPC_S2C_CLN_PlantGetBack);
		BindAction(EPacketType.PT_CL_CLN_RemoveNpc, RPC_S2C_CLN_RemoveNpc);
		BindAction(EPacketType.PT_CL_CLN_PlantPutOut, RPC_S2C_CLN_PlantPutOut);
		BindAction(EPacketType.PT_CL_CLN_PlantUpdateInfo, RPC_S2C_CLN_PlantUpdateInfo);
        BindAction(EPacketType.PT_CL_CLN_PlantClear, RPC_S2C_CLN_PlantClear);

        BindAction(EPacketType.PT_CL_CLN_PlantWater, RPC_S2C_CLN_PlantWater);
        BindAction(EPacketType.PT_CL_CLN_PlantClean, RPC_S2C_CLN_PlantClean);

        BindAction(EPacketType.PT_CL_CLN_SetProcessingIndex, RPC_S2C_CLN_SetProcessingIndex);
        BindAction(EPacketType.PT_CL_CLN_SetIsProcessing, RPC_S2C_CLN_SetIsProcessing);


		BindAction(EPacketType.PT_InGame_PutOnEquipment, RPC_S2C_PutOnEquipment);
		BindAction(EPacketType.PT_InGame_TakeOffEquipment, RPC_S2C_TakeOffEquipment);

		BindAction(EPacketType.PT_InGame_DeadObjItem, RPC_C2S_ResponseDeadObjItem);
		BindAction(EPacketType.PT_NPC_ResetPosition, RPC_S2C_ResetPosition);
        BindAction(EPacketType.PT_NPC_ForcedServant, RPC_S2C_ForcedServant);

        BindAction(EPacketType.PT_AI_SetBool, RPC_S2C_SetBool);
        BindAction(EPacketType.PT_AI_SetTrigger, RPC_S2C_SetTrigger);
        BindAction(EPacketType.PT_AI_SetMoveMode, RPC_S2C_SetMoveMode);
        BindAction(EPacketType.PT_AI_HoldWeapon, RPC_S2C_HoldWeapon);
        BindAction(EPacketType.PT_AI_SwitchHoldWeapon, RPC_S2C_SwitchHoldWeapon);
        BindAction(EPacketType.PT_AI_SwordAttack, RPC_S2C_SwordAttack);
        BindAction(EPacketType.PT_AI_TwoHandWeaponAttack, RPC_S2C_TwoHandWeaponAttack);
        BindAction(EPacketType.PT_AI_SetIKAim, RPC_S2C_SetIKAim);
        BindAction(EPacketType.PT_AI_Fadein, RPC_S2C_Fadein);
        BindAction(EPacketType.PT_AI_Fadeout, RPC_S2C_Fadeout);

        BindAction(EPacketType.PT_Common_ScenarioId, RPC_S2C_ScenarioId);
        BindAction(EPacketType.PT_NPC_SelfUseItem, RPC_S2C_SelfUseItem);

		BindAction(EPacketType.PT_NPC_AddEnemyLock, RPC_S2C_AddEnemyLock);
		BindAction(EPacketType.PT_NPC_RemoveEnemyLock, RPC_S2C_RemoveEnemyLock);
		BindAction(EPacketType.PT_NPC_ClearEnemyLocked, RPC_S2C_ClearEnemyLocked);

        RPCServer(EPacketType.PT_NPC_InitData);
	}

	//void Update()
	//{
	//	if (IsController)
	//		return;

	//	if (null != _viewTrans)
	//	{
	//		if (Vector3.Distance(_viewTrans.position, transform.position) >= 10.0f)
	//			_viewTrans.position = transform.position;
	//		else
	//			_viewTrans.position = Vector3.Lerp(_viewTrans.position, transform.position, Time.deltaTime * 5);

	//		_viewTrans.rotation = Quaternion.Slerp(_viewTrans.rotation, transform.rotation, Time.deltaTime * 5);
	//	}
	//}

	protected override void OnPEDestroy()
	{
		if (npcType == NPCType.TOWN_NPC ||
			npcType == NPCType.BUILDING_NPC)
		{
//			VArtifactTown town = VArtifactUtil.GetPosTown(transform.position);
			//if (null != town && null != Runner)
			//    town.RemoveTownNpc(Runner as AiNpcObject);
		}

		if (null != _entity)
		{
			_entity.NpcCmpt.OnAddEnemyLock -= OnAddEnemyLock;
			_entity.NpcCmpt.OnRemoveEnemyLock -= OnRemoveEnemyLock;
			_entity.NpcCmpt.OnClearEnemyLocked -= OnClearEnemyLocked;
		}

		base.OnPEDestroy();
	}

	//public override void OnPeMsg(EMsg msg, params object[] args)
	//{
	//	switch (msg)
	//	{
	//		case EMsg.Lod_Collider_Destroying:
	//			if (hasOwnerAuth)
	//			{
	//				if (npcCmpt.IsFollower)
	//					return;
	//			}
	//			break;
	//	}

	//	base.OnPeMsg(msg, args);
	//}

	internal void CreateAdNpc(byte[] customData, byte[] missionData, int missionState)
	{
		if (!NpcMissionDataRepository.m_AdRandMisNpcData.ContainsKey(ExternId))
			return;

		AdNpcData data = NpcMissionDataRepository.m_AdRandMisNpcData[ExternId];
		if (null == data)
			return;

		_entity = PeEntityCreator.Instance.CreateRandomNpcForNet(data.mRnpc_ID, Id, Vector3.zero, Quaternion.identity, Vector3.one);
		if (null == _entity)
			return;

		CustomCharactor.CustomData charactorData = new CustomCharactor.CustomData();
		charactorData.Deserialize(customData);
		PeEntityCreator.ApplyCustomCharactorData(_entity, charactorData);

		_viewTrans = _entity.peTrans;
		if (null == _viewTrans)
		{
			Debug.LogError("entity has no ViewCmpt");
			return;
		}

		_viewTrans.position = transform.position;
		_viewTrans.rotation = transform.rotation;
		_entity.SetBirthPos(spawnPos);//delete npc need
		_move = _entity.GetCmpt<Motion_Move>();

		NetCmpt net = _entity.GetCmpt<NetCmpt>();
		if (null == net)
			net = _entity.Add<NetCmpt>();

		net.network = this;

		EntityInfoCmpt entityInfo = _entity.GetCmpt<EntityInfoCmpt>();
		if (null != entityInfo)
			gameObject.name = string.Format("{0}, TemplateId:{1}, Id:{2}", entityInfo.characterName, ExternId, Id);

		// Init mission
		useData = new NpcMissionData();
		useData.Deserialize(missionData);

		for (int i = 0; i < data.m_CSRecruitMissionList.Count; i++)
			useData.m_CSRecruitMissionList.Add(data.m_CSRecruitMissionList[i]);

		NpcMissionDataRepository.AddMissionData(Id, useData);
		_entity.SetUserData(useData);

		if (npcType == NPCType.TOWN_NPC ||
			npcType == NPCType.BUILDING_NPC)
		{
//			VArtifactTown town = VArtifactUtil.GetPosTown(transform.position);
		}

        if (isStand)
        {
			VArtifactUtil.SetNpcStandRot(_entity, rotY, true);
        }

		OnSpawned(_entity.GetGameObject());
    }

	void CreateCustomNpc(byte[] customData, byte[] missionData, int missionState)
	{
		Pathea.NpcProtoDb.Item item = Pathea.NpcProtoDb.Get(ExternId);
		if (null == item)
			return;

		if (!NpcMissionDataRepository.dicMissionData.ContainsKey(item.sort))
			return;

		NpcMissionData data = NpcMissionDataRepository.dicMissionData[item.sort];
		if (null == data)
			return;

		_entity = PeEntityCreator.Instance.CreateNpcForNet(Id, ExternId, Vector3.zero, Quaternion.identity, Vector3.one);
		if (null == _entity)
			return;

        //CustomCharactor.CustomData charactorData = new CustomCharactor.CustomData();
        //charactorData.Deserialize(customData);
        //PeEntityCreator.ApplyCustomCharactorData(_entity, charactorData);
        _entity.ExtSetName(new CharacterName(customName));

		_viewTrans = _entity.peTrans;
		if (null == _viewTrans)
		{
			Debug.LogError("entity has no ViewCmpt");
			return;
		}

		_viewTrans.position = transform.position;
		_viewTrans.rotation = transform.rotation;
		_entity.SetBirthPos(spawnPos);//delete npc need

		_move = _entity.GetCmpt<Motion_Move>();

		NetCmpt net = _entity.GetCmpt<NetCmpt>();
		if (null == net)
			net = _entity.Add<NetCmpt>();

		net.network = this;

		EntityInfoCmpt entityInfo = _entity.GetCmpt<EntityInfoCmpt>();
		if (null != entityInfo)
			gameObject.name = string.Format("{0}, TemplateId:{1}, Id:{2}", entityInfo.characterName, ExternId, Id);

		// Init mission
		useData = new NpcMissionData();
		useData.Deserialize(missionData);

		for (int i = 0; i < data.m_CSRecruitMissionList.Count; i++)
			useData.m_CSRecruitMissionList.Add(data.m_CSRecruitMissionList[i]);

		NpcMissionDataRepository.AddMissionData(Id, useData);
		_entity.SetUserData(useData);


		if (npcType == NPCType.TOWN_NPC ||
			npcType == NPCType.BUILDING_NPC)
		{
//			VArtifactTown town = VArtifactUtil.GetPosTown(transform.position);
		}

		if (isStand)
		{
			VArtifactUtil.SetNpcStandRot(_entity, rotY, true);
		}

		OnSpawned(_entity.GetGameObject());
    }

	internal void CreateStoryNpc(byte[] customData, byte[] missionData, int missionState)
	{
		if (!NpcMissionDataRepository.dicMissionData.ContainsKey(Id))
			return;
		NpcMissionData data = NpcMissionDataRepository.dicMissionData[Id];
		if (null == data)
			return;


		_entity = PeEntityCreator.Instance.CreateNpcForNet(Id, ExternId, Vector3.zero, Quaternion.identity, Vector3.one);
		if (null == _entity)
			return;

		_viewTrans = _entity.peTrans;
		if (null == _viewTrans)
		{
			Debug.LogError("entity has no ViewCmpt");
			return;
		}

		_viewTrans.position = transform.position;
		_viewTrans.rotation = transform.rotation;
		//_entity.SetBirthPos(spawnPos);//delete npc need

		_move = _entity.GetCmpt<Motion_Move>();

		NetCmpt net = _entity.GetCmpt<NetCmpt>();
		if (null == net)
			net = _entity.Add<NetCmpt>();
		
		net.network = this;

		EntityInfoCmpt entityInfo = _entity.GetCmpt<EntityInfoCmpt>();
		if (null != entityInfo)
			gameObject.name = string.Format("{0}, TemplateId:{1}, Id:{2}", entityInfo.characterName, ExternId, Id);
		
		// Init mission
		useData = new NpcMissionData();
		useData.Deserialize(missionData);
		//int misid = AdRMRepository.GetRandomMission(data.m_QCID, useData.m_CurMissionGroup);
		//if (misid != 0)
		//	useData.m_RandomMission = misid;

		for (int i = 0; i < data.m_CSRecruitMissionList.Count; i++)
			useData.m_CSRecruitMissionList.Add(data.m_CSRecruitMissionList[i]);
		
		NpcMissionDataRepository.AddMissionData(Id, useData);
		_entity.SetUserData(useData);
		

		if (npcType == NPCType.TOWN_NPC ||
		    npcType == NPCType.BUILDING_NPC)
		{
//			VArtifactTown town = VArtifactUtil.GetPosTown(transform.position);
		}
		if (isStand)
		{
			VArtifactUtil.SetNpcStandRot(_entity, rotY, true);
		}

		OnSpawned(_entity.GetGameObject());
    }

	internal void CreateRdNpc(byte[] customData, byte[] missionData, int missionState)
	{
		if (!NpcMissionDataRepository.dicMissionData.ContainsKey(Id))
			return;
		
		NpcMissionData data = NpcMissionDataRepository.dicMissionData[Id];
		if (null == data)
			return;
		
		_entity = PeEntityCreator.Instance.CreateRandomNpcForNet(data.m_Rnpc_ID, Id, Vector3.zero, Quaternion.identity, Vector3.one);
		//PeEntity entity = PeCreature.Instance.CreateRandomNpc(Id);
		if (null == _entity)
			return;
		
		CustomCharactor.CustomData charactorData = new CustomCharactor.CustomData();
		charactorData.Deserialize(customData);
		PeEntityCreator.ApplyCustomCharactorData(_entity, charactorData);

		_viewTrans = _entity.peTrans;
		if (null == _viewTrans)
		{
			Debug.LogError("entity has no ViewCmpt");
			return;
		}

		_viewTrans.position = transform.position;
		_viewTrans.rotation = transform.rotation;
		//_entity.SetBirthPos(spawnPos);//delete npc need

		_move = _entity.GetCmpt<Motion_Move>();

		NetCmpt net = _entity.GetCmpt<NetCmpt>();
		if (null == net)
			net = _entity.Add<NetCmpt>();
		
		net.network = this;

		EntityInfoCmpt entityInfo = _entity.GetCmpt<EntityInfoCmpt>();
		if (null != entityInfo)
			gameObject.name = string.Format("{0}, TemplateId:{1}, Id:{2}", entityInfo.characterName, ExternId, Id);
		
		// Init mission
		useData = new NpcMissionData();
		useData.Deserialize(missionData);
		//int misid = AdRMRepository.GetRandomMission(data.m_QCID, useData.m_CurMissionGroup);
		//if (misid != 0)
		//	useData.m_RandomMission = misid;

		//useData.m_RandomMission = 9009;
		
		for (int i = 0; i < data.m_CSRecruitMissionList.Count; i++)
			useData.m_CSRecruitMissionList.Add(data.m_CSRecruitMissionList[i]);

        NpcMissionDataRepository.AddMissionData(Id, useData);
		_entity.SetUserData(useData);
		
		//PeMousePickCmpt pmp = entity.GetCmpt<PeMousePickCmpt>();
		//if (pmp == null)
		//    return;
		
		//pmp.mousePick.eventor.Subscribe(EntityCreateMgr.Instance.NpcMouseEventHandler);
		
		if (npcType == NPCType.TOWN_NPC ||
		    npcType == NPCType.BUILDING_NPC)
		{
//			VArtifactTown town = VArtifactUtil.GetPosTown(transform.position);
			//if (null != town)
			//{
			//    town.AddTownNpc(npc);
			//}
		}
		if (isStand)
		{
			VArtifactUtil.SetNpcStandRot(_entity, rotY, true);
		}

		OnSpawned(_entity.GetGameObject());
    }

	public override void OnSpawned(GameObject obj)
	{
		base.OnSpawned(obj);

		_entity.NpcCmpt.OnAddEnemyLock += OnAddEnemyLock;
		_entity.NpcCmpt.OnRemoveEnemyLock += OnRemoveEnemyLock;
		_entity.NpcCmpt.OnClearEnemyLocked += OnClearEnemyLocked;
	}

	void InitExternData(byte[] externData)
	{
		PETools.Serialize.Import(externData, r =>
		{
			string givenName = BufferHelper.ReadString(r);
			string familyName = BufferHelper.ReadString(r);

			if (null != _entity)
				_entity.ExtSetName(new CharacterName(givenName, familyName));

			/*int autoReviveTime = */BufferHelper.ReadInt32(r);
			/*bool isStand = */BufferHelper.ReadBoolean(r);
			/*float rotY = */BufferHelper.ReadSingle(r);
			/*int missionState = */BufferHelper.ReadInt32(r);
			/*int money = */BufferHelper.ReadInt32(r);
			/*int protoId = */BufferHelper.ReadInt32(r);
		});
	}

	protected override IEnumerator SyncMove()
	{
		_pos = transform.position;
		rot = transform.rotation;

		while (hasOwnerAuth)
		{
            NpcMove();
            //yield return new WaitForSeconds(1 / uLink.Network.sendRate);
            yield return new WaitForSeconds(1 / 5);
		}
	}

	protected override void CheckAuthority()
	{
		if (hasOwnerAuth)
		{
			if (null == npcCmpt || npcCmpt.IsFollower)
				return;
		}

		base.CheckAuthority();
	}

	public void NpcMove()
    {
        _pos = transform.position;
        rot = transform.rotation;

        if (null != _viewTrans)
        {
			_pos = transform.position = _viewTrans.position;
			rot = transform.rotation = _viewTrans.rotation;
        }

        if (null == _move)
            return;

        if (Vector3.SqrMagnitude(_move.velocity - _syncAttr.Speed) > PlayerSynAttribute.SyncMovePrecision ||
			Mathf.Abs(_syncAttr.Pos.x - _pos.x) > PlayerSynAttribute.SyncMovePrecision ||
			Mathf.Abs(_syncAttr.Pos.y - _pos.y) > PlayerSynAttribute.SyncMovePrecision ||
			Mathf.Abs(_syncAttr.Pos.z - _pos.z) > PlayerSynAttribute.SyncMovePrecision ||
			Mathf.Abs(_syncAttr.EulerY - rot.eulerAngles.y) > PlayerSynAttribute.SyncMovePrecision)
        {
            int rotEuler = VCUtils.CompressEulerAngle(rot.eulerAngles);
            URPCServer(EPacketType.PT_NPC_Move, _pos, ((byte)_move.speed), rotEuler, GameTime.Timer.Second);
            _syncAttr.Pos = _pos;
            _syncAttr.Speed = _move.velocity;
            _syncAttr.EulerY = rot.eulerAngles.y;

            if(_move is Motion_Move_Human)
                (_move as Motion_Move_Human).NetMovePos = _pos;
        }
    }
	// change force when recruit by player
	public override void InitForceData()
	{
		if (!PeGameMgr.IsCustom)
		{
			if (null != _entity)
			{
				if (-1 != mLordPlayerId)//∆Õ¥”À˘ ÙID
					_entity.SetAttribute(AttribType.DefaultPlayerID, mLordPlayerId, false);
				else
                {
                    mColonyLordPlayerId = mColonyLordPlayerId == -1 ? (TeamId == PlayerNetwork.mainPlayer.TeamId || (TeamId != -1 && !ForceSetting.Instance.Conflict(TeamId, PlayerNetwork.mainPlayer.Id))) ? PlayerNetwork.mainPlayer.Id : -1 : mColonyLordPlayerId;
                    if (mColonyLordPlayerId != -1)//
                        _entity.SetAttribute(AttribType.DefaultPlayerID, mColonyLordPlayerId, false);                      
                    else
                        _entity.SetAttribute(AttribType.DefaultPlayerID, 2f, false);
                }
				
                if (PeGameMgr.IsAdventure)
				{
					_entity.SetAttribute(AttribType.CampID, 5, false);
					_entity.SetAttribute(AttribType.DamageID, 5, false);
				}
			}
		}

		ResetTarget();
    }

	void OnAddEnemyLock(int id)
	{
		if (hasOwnerAuth)
			RPCServer(EPacketType.PT_NPC_AddEnemyLock, id);
	}

	void OnRemoveEnemyLock(int id)
	{
		if (hasOwnerAuth)
			RPCServer(EPacketType.PT_NPC_RemoveEnemyLock, id);
	}

	void OnClearEnemyLocked()
	{
		if (hasOwnerAuth)
			RPCServer(EPacketType.PT_NPC_ClearEnemyLocked);
	}

	void ResetTarget()
	{
		if (hasOwnerAuth)
		{
			if (null != _entity)
			{
				TargetCmpt targetCmpt = _entity.GetCmpt<TargetCmpt>();
				if (null != targetCmpt)
					targetCmpt.ClearEnemy();
			}
		}
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

    public bool WaitForInitNpc()
    {
        if (!PlayerNetwork.mainPlayer._initOk || StroyManager.Instance == null)
            return false;
        StroyManager.Instance.InitMission(Id);
        _npcMissionInited = true;
        if (null != _viewTrans && PeGameMgr.IsMultiStory)
            _viewTrans.rotation = transform.rotation;
        MissionManager.Instance.m_PlayerMission.UpdateNpcMissionTex(_entity);
        return true;
    }

    public bool WaitForMountNpc()
    {
        if (EntityMgr.Instance.Get(_mountId) == null || npcCmpt == null)
            return false;
        else
        {
            PeTrans trans = EntityMgr.Instance.Get(_mountId).GetComponent<PeTrans>();
            if (trans == null)
                return false;
            Transform tr = PETools.PEUtil.GetChild(trans.existent, "CarryUp");
            if (tr == null)
                return false;
            npcCmpt.MountID = _mountId;
            return true;
        }
            
    }

    public bool WaitForInitNpcMission()
    {
        if (null == PlayerNetwork.mainPlayer || !PlayerNetwork.mainPlayer._initOk)
            return false;

        _npcMissionInited = true;
        MissionManager.Instance.m_PlayerMission.UpdateNpcMissionTex(_entity);
        return true;
    }

    public void GetOffVehicle(Vector3 pos)
    {
        if (null != DriveCreation)
        {
            DriveCreation.GetOff(pos);
            DriveCreation = null;
        }
    }

	private void GetData(byte[] buffer)
	{
		if (buffer == null)
			return;

		using (MemoryStream ms = new MemoryStream(buffer))
		using (BinaryReader br = new BinaryReader(ms))
		{
			BufferHelper.ReadInt32(br);
			BufferHelper.ReadInt32(br);

			BufferHelper.ReadInt32(br);
			BufferHelper.ReadInt32(br);
			BufferHelper.ReadSingle(br);
			BufferHelper.ReadSingle(br);
			BufferHelper.ReadSingle(br);
			BufferHelper.ReadInt32(br);

			int count = BufferHelper.ReadInt32(br);
			for (int i = 0; i < count; i++)
				BufferHelper.ReadInt32(br);

			count = BufferHelper.ReadInt32(br);
			for (int i = 0; i < count; i++)
				BufferHelper.ReadInt32(br);

			count = BufferHelper.ReadInt32(br);
			for (int i = 0; i < count; i++)
				BufferHelper.ReadInt32(br);

			BufferHelper.ReadString(br);
			BufferHelper.ReadString(br);

			count = BufferHelper.ReadInt32(br);
			for (int i = 0; i < count; i++)
				BufferHelper.ReadInt32(br);

			Color tmpColor;
			BufferHelper.ReadColor(br, out tmpColor);
			BufferHelper.ReadColor(br, out tmpColor);
			BufferHelper.ReadColor(br, out tmpColor);

			BufferHelper.ReadSingle(br);
			BufferHelper.ReadSingle(br);

			useData = new NpcMissionData();
			useData.mCurComMisNum = BufferHelper.ReadByte(br);
			useData.mCompletedMissionCount = BufferHelper.ReadInt32(br);
			useData.m_RandomMission = BufferHelper.ReadInt32(br);
			useData.m_RecruitMissionNum = BufferHelper.ReadInt32(br);
			useData.m_Rnpc_ID = BufferHelper.ReadInt32(br);
			useData.m_CurMissionGroup = BufferHelper.ReadInt32(br);
			useData.m_CurGroupTimes = BufferHelper.ReadInt32(br);
			useData.m_QCID = BufferHelper.ReadInt32(br);
			BufferHelper.ReadVector3(br, out useData.m_Pos);

			count = BufferHelper.ReadInt32(br);
			for (int i = 0; i < count; i++)
			{
				int id = BufferHelper.ReadInt32(br);
				useData.m_MissionList.Add(id);
			}

			count = BufferHelper.ReadInt32(br);
			for (int i = 0; i < count; i++)
			{
				int id = BufferHelper.ReadInt32(br);
				useData.m_MissionListReply.Add(id);
			}

			count = BufferHelper.ReadInt32(br);
			for (int i = 0; i < count; i++)
			{
				int id = BufferHelper.ReadInt32(br);
				useData.m_RecruitMissionList.Add(id);
			}
		}
	}

	private void RequestNpcEquips()
	{
		RPCServer(EPacketType.PT_NPC_Equips);
	}

	private void RequestNpcItems()
	{
		RPCServer(EPacketType.PT_NPC_Items);
	}

	private void RequestExternData()
	{
		RPCServer(EPacketType.PT_NPC_ExternData);
	}

	private void RequestNpcSkill()
	{
		RPCServer(EPacketType.PT_NPC_Skill);
	}

	public void RequestNpcUseItem(int objId)
	{
		RPCServer(EPacketType.PT_NPC_SelfUseItem, objId);
	}

	public void RequestResetPosition(Vector3 pos)
	{
		RPCServer(EPacketType.PT_NPC_ResetPosition, pos);
	}

	public void RequestMount(Transform trans)
	{
		RPCServer(EPacketType.PT_NPC_Mount,trans.position,trans.rotation.y);
	}
	public void RequestUpdateCampsite( bool val)
	{
		RPCServer(EPacketType.PT_NPC_UpdateCampsite,val);
	}
	public void RequestState(int state)
	{
		RPCServer(EPacketType.PT_NPC_State,state);
	}
	public void RequestGetOn(int creationId,int index)
	{
		RPCServer(EPacketType.PT_InGame_GetOnVehicle,creationId,index);
	}

	public void RequestGetOff()
	{
		RPCServer(EPacketType.PT_InGame_GetOffVehicle);
	}

	#region Action Callback APIs

	protected override void RPC_S2C_InitData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		_pos = transform.position = stream.Read<Vector3>();
		rot = transform.rotation = stream.Read<Quaternion>();

		byte[] customData = stream.Read<byte[]>();
		byte[] missionData = stream.Read<byte[]>();
		/*bool death = */
		stream.Read<bool>();
		authId = stream.Read<int>();
		int missionState = stream.Read<int>();
		int boolName = stream.Read<int>();
		bool boolValue = stream.Read<bool>();
		bForcedServant = stream.Read<bool>();


		_syncAttr.Pos = _pos;
		_syncAttr.EulerY = rot.eulerAngles.y;

		if (PeGameMgr.IsMultiStory)
		{
			if (Id >= 9200)
				CreateRdNpc(customData, missionData, missionState);
			else
			{
				CreateStoryNpc(customData, missionData, missionState);
			}
			GlobalBehaviour.RegisterEvent(WaitForInitNpc);
		}
		else
		{
			if (PeGameMgr.IsCustom)
			{
				CreateCustomNpc(customData, missionData, missionState);
			}
			else
			{
				CreateAdNpc(customData, missionData, missionState);
				GlobalBehaviour.RegisterEvent(WaitForInitNpcMission);
			}
		}

		if (_move is Motion_Move_Human)
			(_move as Motion_Move_Human).NetMovePos = _pos;

		RequestNpcEquips();

		OnSkAttrInitEvent += InitForceData;
		if (boolName != 0)
		{
			if (animatorCmpt != null)
				animatorCmpt.SetBool(boolName, boolValue);
		}
		if (!hasOwnerAuth && _move != null)
			_move.NetMoveTo(_pos, Vector3.zero, true);

        if(Id < 9000 && !PeGameMgr.IsMultiCustom)
            MissionManager.Instance.m_PlayerMission.adId_entityId[Id] = Id;
	}
	
	protected override void RPC_S2C_ApplyHpChange(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		float damage = stream.Read<float>();
		int life = stream.Read<int>();
		int casterId = stream.Read<int>();

		CommonInterface caster = null;
		NetworkInterface network = Get(casterId);
		if (null != network)
			caster = network.Runner;

		if (null != Runner)
			Runner.NetworkApplyDamage(caster, damage, life);
	}

	protected override void RPC_S2C_Turn(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		float rotY = stream.Read<float>();
		rot = transform.rotation = Quaternion.Euler(0, rotY, 0);

		if (null != _viewTrans)
			_viewTrans.rotation = transform.rotation;
	}

	private void RPC_S2C_NpcRevive(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		float hp = stream.Read<Single>();
		_pos = transform.position = stream.Read<Vector3>();

		death = false;

		if (null == _entity)
			return;

        _entity.SetAttribute(AttribType.Hp, hp);

        if (null != _move)
            _move.NetMoveTo(transform.position, Vector3.zero, true);

		MotionMgrCmpt motionMgrCmpt = Runner.SkEntityPE.Entity.GetCmpt<MotionMgrCmpt>();
		if (motionMgrCmpt != null)
		{
			PEActionParamB param = PEActionParamB.param;
			param.b = true;
			motionMgrCmpt.DoActionImmediately(PEActionType.Revive, param);
		}

		//if(PeGameMgr.IsMultiStory)
		//{
		//	StroyManager.Instance.InitMission(Id);
		//}
		//NpcRandom npc = NpcManager.Instance.GetNpcRandom(objectID);
		//if (null != npc)
		//    npc.NetWorkRevive(hp, spawnPos);
	}

	private void RPC_S2C_DismissByPlayer(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		_pos = transform.position = stream.Read<Vector3>();
		int newTeamId = stream.Read<int>();

		PeEntity entity = EntityMgr.Instance.Get(Id);
		if (null != entity)
		{
			if (null != PeCreature.Instance.mainPlayer
				&& mLordPlayerId == PeCreature.Instance.mainPlayer.Id)
			{
				entity.SetFollower(false);
			}
            else
            {
                NpcCmpt cmpt = entity.GetCmpt<NpcCmpt>();
                if (null != cmpt)
                    cmpt.SetServantLeader(null);
            }

			entity.Dismiss();
            if(_pos.x > 10 && PeGameMgr.IsMultiStory)
                entity.NpcCmpt.FixedPointPos = _pos;
		}

        entity.NpcCmpt.Req_MoveToPosition(_pos, 1, true, SpeedState.Run);
		_teamId = newTeamId;
        mLordPlayerId = -1;

        InitForceData();
    }

	private void RPC_S2C_RecruitByPlayer(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		mLordPlayerId = authId = stream.Read<int>();
		_teamId = stream.Read<int>();
        bool bforce = stream.Read<bool>();

        ResetContorller();

		PeEntity entity = EntityMgr.Instance.Get(Id);
		if (null != entity)
		{
            if (null != PeCreature.Instance.mainPlayer
                && mLordPlayerId == PeCreature.Instance.mainPlayer.Id)
            {
                if(!bforce)
                {
                    entity.SetFollower(true);                    
                }                    
            }
            else
            {
                PeEntity lord = EntityMgr.Instance.Get(mLordPlayerId);
                if (null != lord)
                {
                    ServantLeaderCmpt leaderCmpt = lord.GetCmpt<ServantLeaderCmpt>();
                    NpcCmpt cmpt = entity.GetCmpt<NpcCmpt>();
                    if (null != cmpt)
                        cmpt.SetServantLeader(leaderCmpt);
                }
            }

			entity.SetBirthPos(entity.position);
			entity.CmdStopTalk();
			StroyManager.Instance.RemoveReq(entity, EReqType.Dialogue);
			entity.Recruit();
			entity.SetShopIcon(null);
		}
	}

	private void RPC_S2C_SyncWorkState(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (null == Runner)
			return;

		int state;
		Vector3 pos;
		float y;
		stream.TryRead<int>(out state);
		stream.TryRead<Vector3>(out pos);
		stream.TryRead<float>(out y);
		if (!hasOwnerAuth)
		{
			//NpcRandom npc = Runner as NpcRandom;
			//if (null != npc)
			//{
			//    switch (state)
			//    {
			//        case 0:
			//            {
			//                break;
			//            }
			//        case 1:
			//            {
			//                break;
			//            }
			//    }
			//    transform.position = pos;
			//    transform.rotation = Quaternion.Euler(0, y, 0);

			//    npc.SyncWorkState(state, pos);
			//}
		}
	}

    private void RPC_NPCMove(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        _pos = transform.position = stream.Read<Vector3>();
        byte moveState = stream.Read<byte>();
        int rotY = stream.Read<int>();
        double time = stream.Read<double>();

        Vector3 euler = VCUtils.UncompressEulerAngle(rotY);
        rot = transform.rotation = Quaternion.Euler(euler);

        if (!hasOwnerAuth)
        {
           
            if (null != _move)
            {
                //if (PeGameMgr.IsMulti && MissionManager.Instance.HadCompleteMission(18) && !MissionManager.Instance.HadCompleteMission(27) && Id == 9008)
                //{
                //    return;
                //}
                _move.AddNetTransInfo(_pos, transform.rotation.eulerAngles, (SpeedState)moveState, time);
            }
                
			if (null != npcCmpt)
			{
	            RQFollowPath req = npcCmpt.Req_GetRequest(EReqType.FollowPath) as RQFollowPath;
	            if (null != req)
	            {
	                if (IsReached(req.path[req.path.Length - 1], _pos, true))
	                {
	                    if (!req.isLoop)
	                        npcCmpt.Req_Remove(EReqType.FollowPath);
	                }
	            }
			}
        }
    }

    void NPC_ForceMove(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        _pos = transform.position = stream.Read<Vector3>();
        stream.Read<byte>();
        int rotY = stream.Read<int>();
        stream.Read<double>();

        Vector3 euler = VCUtils.UncompressEulerAngle(rotY);
        rot = transform.rotation = Quaternion.Euler(euler);

        if(_entity.peTrans.position == _pos)
            _entity.peTrans.position = _pos;
        if(_entity.peTrans.rotation == rot)
            _entity.peTrans.rotation = rot;
    }

    internal bool IsReached(Vector3 pos, Vector3 targetPos, bool Is3D = false, float radiu = 2.0f)
    {
        float sqrDistanceH = PETools.PEUtil.Magnitude(pos, targetPos, Is3D);
        return sqrDistanceH < radiu;
    }

    void RPC_S2C_NpcMisstion(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] missionData = stream.Read<byte[]>();

		AdNpcData data = NpcMissionDataRepository.m_AdRandMisNpcData[ExternId];
		if (null == data)
			return;

		useData.Deserialize(missionData);
		//int misid = AdRMRepository.GetRandomMission(data.mQC_ID, useData.m_CurMissionGroup);
		//if (misid != 0)
			//useData.m_RandomMission = misid;
	}

	void RPC_S2C_MissionState(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PeEntity npcEntity = EntityMgr.Instance.Get(Id);
		if (null == npcEntity)
			return;
		npcEntity.SetState((NpcMissionState) stream.Read<int>());
	}

	void RPC_S2C_RequestAiOp(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int type = stream.Read<int>();
		if(npcCmpt == null)
		{
			return;
		}
		if(hasOwnerAuth)
		{
			switch((EReqType)type)
			{
			case EReqType.Animation:
				npcCmpt.Req_PlayAnimation(stream.Read<string>(),stream.Read<float>(), stream.Read<bool>());
				break;
			case EReqType.FollowPath:
                npcCmpt.Req_FollowPath(stream.Read<Vector3[]>(), stream.Read<bool>(), SpeedState.Run, true);                   
                break;
			case EReqType.FollowTarget:
				npcCmpt.Req_FollowTarget(stream.Read<int>(),stream.Read<Vector3>(),stream.Read<Int32>(),stream.Read<float>(),true);
				break;
			case EReqType.MoveToPoint:
				npcCmpt.Req_MoveToPosition( stream.Read<Vector3>(),stream.Read<float>(),stream.Read<bool>(),(SpeedState)stream.Read<int>());
                if( PeGameMgr.IsMultiStory)
                    npcCmpt.FixedPointPos = _pos;
                break;
            case EReqType.TalkMove:
                npcCmpt.Req_TalkMoveToPosition(stream.Read<Vector3>(), stream.Read<float>(), stream.Read<bool>(), (SpeedState)stream.Read<int>());
                break;
            case EReqType.Rotate:
				npcCmpt.Req_Rotation(stream.Read<Quaternion>());
				break;
			case EReqType.Salvation:
				npcCmpt.Req_Salvation( stream.Read<int>(),stream.Read<bool>());
				break;
			case EReqType.Translate:
                _pos = transform.position = stream.Read<Vector3>();
                stream.Read<bool>();
                if (PeGameMgr.IsMultiStory)
                    npcCmpt.FixedPointPos = _pos;
                transform.position = _pos;
                if (_move != null)
                    _move.NetMoveTo(_pos, Vector3.zero, true);
                break;
			case EReqType.UseSkill:
				npcCmpt.Req_UseSkill();
				break;
            case EReqType.Remove:
                {

                }
                break;
                default:
				break;
			}
		}
 		else
 		{
 			switch((EReqType)type)
 			{
 			case EReqType.Animation:
 
 				break;
 			case EReqType.FollowPath:
                {
                    npcCmpt.Req_FollowPath(stream.Read<Vector3[]>(), stream.Read<bool>(), SpeedState.Run, true);                       
                }
                break;
 			case EReqType.FollowTarget:
 
 				break;
 			case EReqType.MoveToPoint:
                if (PeGameMgr.IsMultiStory)
                    npcCmpt.FixedPointPos = _pos;
                break;
            case EReqType.TalkMove:

                break;
            case EReqType.Rotate:

 				break;
 			case EReqType.Salvation:
 
 				break;
 			case EReqType.Translate:
				_pos = transform.position = stream.Read<Vector3>();
                stream.Read<bool>();
                if (PeGameMgr.IsMultiStory)
                    npcCmpt.FixedPointPos = _pos;
                if (_move != null)
                    _move.NetMoveTo(_pos,Vector3.zero, true);
                StroyManager.Instance.EntityReach(_entity, true, true);
                break;
 			case EReqType.UseSkill:
 
 				break;
            case EReqType.Remove:
                {
                    EReqType rtype = (EReqType)stream.Read<int>();                    
                    if(rtype == EReqType.FollowPath)
                    {
                        Vector3[] vpos = stream.Read<Vector3[]>();
                        RQFollowPath req = npcCmpt.Req_GetRequest(rtype) as RQFollowPath;
                        if(req != null)
                        {
                            if (req.Equal(vpos))
                                npcCmpt.Req_Remove(rtype);
                        }
                    }    
                    else if(rtype == EReqType.FollowTarget)
                    {
                        RQFollowTarget req = npcCmpt.Req_GetRequest(rtype) as RQFollowTarget;
                        if (req != null)
                        {
                            npcCmpt.Req_Remove(rtype);
                        }
                    }
                }
                break;
                default:
 				break;
 			}
 		}
	}


	void 	RPC_S2C_Mount(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (hasOwnerAuth)
			return;
		_mountId = stream.Read<int>();
        if (npcCmpt != null)
        {
            if (_mountId == 0)
                npcCmpt.MountID = _mountId;
            else if (EntityMgr.Instance.Get(_mountId) != null)
                npcCmpt.MountID = _mountId;
            else
                GlobalBehaviour.RegisterEvent(WaitForMountNpc);
        }
        else
            Debug.LogError("npccmpt is null,mount failed");
	}

	void 	RPC_S2C_UpdateCampsite(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (hasOwnerAuth)
			return;
		bool bv = stream.Read<bool>();
		if(npcCmpt != null)
			npcCmpt.UpdateCampsite = bv;
	}

	void RPC_S2C_State(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (hasOwnerAuth)
			return;
		int state = stream.Read<int>();
		if(npcCmpt != null)
			npcCmpt.State = (ENpcState)state;
	}

	void RPC_S2C_GetOn(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int entityId = stream.Read<int>();
		SeatIndex = stream.Read<int>();
        StartCoroutine(GetOnVehicle(entityId));
    }

	private void RPC_S2C_GetOff(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		_pos = transform.position = stream.Read<Vector3>();

        GetOffVehicle(_pos);

        if (null != MtCmpt)
        {
            MtCmpt.EndAction(PEActionType.Drive);
        }
			
		if (null != Trans)
			Trans.position = _pos;
	}


	private void RPC_MissionFlag(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (null == Runner)
			return;
		string name;
		int flag;
		int nMissionID;
		stream.TryRead<string>(out name);
		stream.TryRead<int>(out flag);
		stream.TryRead<int>(out nMissionID);

		//NpcRandom npc = Runner as NpcRandom;
		//if (null != npc)
		//    npc.SetMissionFlag(flag, nMissionID, name);
	}

	private void RPC_PackageIndex(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
        int tabIndex = stream.Read<int>();
		int count = stream.Read<int>();

		PeEntity entity = EntityMgr.Instance.Get(Id);
		if (null == entity)
			return;

		Pathea.NpcPackageCmpt cmpt = entity.GetCmpt<Pathea.NpcPackageCmpt>();
		if (null == cmpt)
			return;

        if (0 == tabIndex)
        {
            cmpt.Clear();
            if (0 != count)
            {
                int[] itemIds = stream.Read<int[]>();

                foreach (int id in itemIds)
                {
                    ItemObject item = ItemMgr.Instance.Get(id);
                    if (null == item)
                        continue;

                    cmpt.AddToNet(item);
                }
            }
        }
        else
        {
            cmpt.ClearHandin();

            if (0 != count)
            {
                int[] itemIds = stream.Read<int[]>();

                foreach (int id in itemIds)
                {
                    ItemObject item = ItemMgr.Instance.Get(id);
                    if (null == item)
                        continue;

                    cmpt.AddToNetHandin(item);
                }
            }
        }
	}

	private void RPC_S2C_SyncMoney(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int money = stream.Read<int>();

		PeEntity entity = EntityMgr.Instance.Get(Id);
		if (null == entity)
			return;

		PackageCmpt cmpt = entity.GetCmpt<PackageCmpt>();
		if (null == cmpt)
			return;

		cmpt.money.current = money;

		if (null != GameUI.Instance.mShopWnd)
			GameUI.Instance.mShopWnd.ResetItem();
	}

	private void RPC_S2C_SyncTeamID(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		_teamId = stream.Read<int>();
		ResetTarget();
	}

    private void RPC_S2C_PutOnEquipment(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		ItemObject equip = stream.Read<ItemObject>();

		PeEntity entity = EntityMgr.Instance.Get(Id);
		if (null == entity)
			return;

		Pathea.EquipmentCmpt cmpt = entity.GetCmpt<Pathea.EquipmentCmpt>();
		if (null != cmpt)
			cmpt.PutOnEquipment(equip, false, null, true);
	}

	private void RPC_S2C_TakeOffEquipment(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int equipId = stream.Read<int>();

		ItemObject equip = ItemMgr.Instance.Get(equipId);
		if (null == equip)
			return;

		PeEntity entity = EntityMgr.Instance.Get(Id);
		if (null == entity)
			return;

		Pathea.EquipmentCmpt cmpt = entity.GetCmpt<Pathea.EquipmentCmpt>();
		if (null != cmpt)
			cmpt.TakeOffEquipment(equip, false, null, true);
	}

	void RPC_S2C_ExternData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] externData = stream.Read<byte[]>();
		InitExternData(externData);
		RequestNpcSkill();
	}

	private void RPC_S2C_NpcEquips(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
        int equipCount = stream.Read<int>();
        if (0 != equipCount)
        {
            ItemObject[] equips = stream.Read<ItemObject[]>();

            PeEntity entity = EntityMgr.Instance.Get(Id);
            if (null == entity)
                return;

            Pathea.EquipmentCmpt cmpt = entity.GetCmpt<Pathea.EquipmentCmpt>();
            if (null != cmpt && null != equips)
                cmpt.ApplyEquipment(equips);
        }

		RequestNpcItems();
	}

	private void RPC_S2C_NpcItems(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
        int tabIndex = stream.Read<int>();
        int itemCount = stream.Read<int>();

        if (0 != itemCount)
        {
            ItemObject[] items = stream.Read<ItemObject[]>();

            PeEntity entity = EntityMgr.Instance.Get(Id);
            if (null != entity)
            {
                Pathea.NpcPackageCmpt cmpt = entity.GetCmpt<Pathea.NpcPackageCmpt>();
                if (null != cmpt)
                {
                    if (0 == tabIndex)
                        cmpt.Clear();
                    else
                        cmpt.ClearHandin();

                    foreach (ItemObject item in items)
                    {
                        if (0 == tabIndex)
                            cmpt.AddToNet(item);
                        else
                            cmpt.AddToNetHandin(item);
                    }
                }
            }
        }

		RequestExternData();
    }

	void RPC_S2C_NpcSkill(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int count = stream.Read<int>();
		if (0 != count)
		{
			int[] skills = stream.Read<int[]>();

			PeEntity entity = EntityMgr.Instance.Get(Id);
			if (null == entity)
				return;

			NpcCmpt cmpt = entity.GetCmpt<NpcCmpt>();
			for (int i = 0; i < skills.Length; i++)
				cmpt.AddAbility(skills[i]);
		}

		RequestAbnormalCondition();
    }

    void RPC_S2C_ForcedServant(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
        _op = stream.Read<bool>();
        int playerId = stream.Read<int>();        
        bForcedServant = _op;
        if (playerId == PlayerNetwork.mainPlayerId)
            StartCoroutine(WaitForMainPlayer());
    }


    protected void RPC_S2C_SelfUseItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int itemObjId = stream.Read<int>();
        ItemObject item = ItemMgr.Instance.Get(itemObjId);
        if (item != null)
        {
            Pathea.UseItemCmpt useItem = _entity.GetCmpt<Pathea.UseItemCmpt>();
            if (useItem != null)
                useItem.UseFromNet(item);
        }
    }

	protected override void RPC_S2C_LostController(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		base.RPC_S2C_LostController(stream, info);

        if (lastAuthId == PlayerNetwork.mainPlayerId && _entity != null)
		{
			_entity.requestCmpt.RequsetProtect();
			PeCreature.Instance.mainPlayer.GetCmpt<ServantLeaderCmpt>().SevantLostController(PeCreature.Instance.mainPlayer.peTrans);
			NpcMgr.ColonyNpcLostController(_entity);
			lastAuthId = authId;
		}
	}

	void RPC_S2C_AddEnemyLock(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>();

		PeEntity entity = EntityMgr.Instance.Get(id);
		if (null == entity)
			return;

		if (!hasOwnerAuth && null != _entity)
			_entity.NpcCmpt.AddEnemyLocked(entity);

	}
	void RPC_S2C_RemoveEnemyLock(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>();

		PeEntity entity = EntityMgr.Instance.Get(id);
		if (null == entity)
			return;

		if (!hasOwnerAuth && null != _entity)
			_entity.NpcCmpt.RemoveEnemyLocked(entity);
	}
	void RPC_S2C_ClearEnemyLocked(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!hasOwnerAuth && null != _entity)
			_entity.NpcCmpt.ClearLockedEnemies();
	}

    #endregion Action Callback APIs
    bool _op = false;
    IEnumerator WaitForMainPlayer()
    {
        while (null == PeCreature.Instance.mainPlayer)
            yield return null;
        ServantLeaderCmpt leader = PeCreature.Instance.mainPlayer.GetCmpt<ServantLeaderCmpt>();
        if (_op)
            leader.AddForcedServant(npcCmpt, true);
        else
            leader.RemoveForcedServant(npcCmpt);
        
    }
}