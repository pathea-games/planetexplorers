using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using ItemAsset;
using SkillAsset;
using AiAsset;
using Pathea;

public struct AiTowerSyncData
{
	public float ChassisY;
	public Vector3 PitchEuler;
}

public class AiTowerNetwork : AiNetwork
{
	private AiTowerSyncData _syncData;
	private TowerCmpt _towerCmpt;
	public int ownerId { get; protected set; }
	public int aimId { get; protected set; }

	protected override void OnPEInstantiate(uLink.NetworkMessageInfo info)
	{
		_id = info.networkView.initialData.Read<int>();
		_scale = info.networkView.initialData.Read<float>();
		ownerId = info.networkView.initialData.Read<int>();
		_teamId = info.networkView.initialData.Read<int>();

		death = false;
		authId = ownerId;
		_pos = transform.position;
		rot = transform.rotation;
	}

	protected override void OnPEStart()
	{
		PlayerNetwork.OnTeamChangedEventHandler += InitMapInfo;

		BindSkAction();

		BindAction(EPacketType.PT_Tower_InitData, RPC_Tower_InitData);
		BindAction(EPacketType.PT_Tower_Move, RPC_Tower_Move);
		BindAction(EPacketType.PT_Tower_Target, RPC_Tower_Target);
		BindAction(EPacketType.PT_Tower_AimPosition, RPC_S2C_AimPosition);
		BindAction(EPacketType.PT_Tower_Fire, RPC_S2C_Fire);
		BindAction(EPacketType.PT_Tower_LostEnemy, RPC_S2C_LostEnemy);
		BindAction(EPacketType.PT_InGame_SetController, RPC_S2C_SetController);
		BindAction(EPacketType.PT_InGame_LostController, RPC_S2C_LostController);
		BindAction(EPacketType.PT_InGame_InitData, RPC_S2C_InitData);

		RPCServer(EPacketType.PT_Tower_InitData);
	}

	protected override void OnPEDestroy ()
	{
		StopAllCoroutines();

		PlayerNetwork.OnTeamChangedEventHandler -= InitMapInfo;
		RemoveMapInfo();

        DragArticleAgent.Destory(Id);

		if (null != Runner)
		{
			Runner.InitNetworkLayer(null);
			Destroy(Runner.gameObject);
		}
	}

	public override void OnSpawned(GameObject obj)
	{
		base.OnSpawned(obj);

		_towerCmpt = _entity.GetCmpt<Pathea.TowerCmpt>();
		if (null == _towerCmpt)
			LogManager.Error("error tower cmpt:" + Id);

		RPCServer(EPacketType.PT_InGame_InitData);
	}

	void InitMapInfo()
	{
		if (null == ForceSetting.Instance)
			return;

		if (ForceSetting.Instance.Conflict(TeamId, PlayerNetwork.mainPlayerId))
			RemoveMapInfo();
		else
			AddMapInfo();
	}

	void AddMapInfo()
	{
		PeMap.TowerMark towerMask = new PeMap.TowerMark();
		towerMask.position = _pos;
		towerMask.ID = Id;

		ItemObject itemObj = ItemMgr.Instance.Get(Id);
		if (null != itemObj)
			towerMask.text = itemObj.protoData.GetName();

		PeEntity entity = EntityMgr.Instance.Get(Id);
		if (null != entity)
			towerMask.campId = Mathf.RoundToInt(entity.GetAttribute(AttribType.CampID));

		PeMap.LabelMgr.Instance.Add(towerMask);
		PeMap.TowerMark.Mgr.Instance.Add(towerMask);
	}

	void RemoveMapInfo()
	{
		PeMap.TowerMark findMask = PeMap.TowerMark.Mgr.Instance.Find(tower => Id == tower.ID);
		if(null != findMask)
		{
			PeMap.LabelMgr.Instance.Remove(findMask);
			PeMap.TowerMark.Mgr.Instance.Remove(findMask);
		}
	}

    protected override IEnumerator SyncMove()
    {
        while (hasOwnerAuth)
        {
            if (null != _towerCmpt)
            {
                if (!death)
                {
                    if (_towerCmpt.ChassisY != _syncData.ChassisY || !_towerCmpt.PitchEuler.Equals(_syncData.PitchEuler))
                    {
                        _syncData.ChassisY = _towerCmpt.ChassisY;
                        _syncData.PitchEuler = _towerCmpt.PitchEuler;
                        //URPCServer(EPacketType.PT_Tower_Target, _syncData.ChassisY, _syncData.PitchEuler);
                    }
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
				if (null != tarEntity && tarEntity.hasView && null != _towerCmpt)
				{
					_towerCmpt.Target = tarEntity.centerBone;
					oldId = aimId;
				}
			}

			yield return new WaitForSeconds(1f);
		}
	}

	public override void InitForceData ()
	{
        if (Runner == null)
            return;

        if(PeGameMgr.IsMultiStory)
        {
            if (null != Runner && null != Runner.SkEntityBase)
                Runner.SkEntityBase.SetAttribute((int)AttribType.DefaultPlayerID, 1);
        }
        else
        {
			if (-1 != TeamId && null != Runner && null != Runner.SkEntityBase)
			{
				Runner.SkEntityBase.SetAttribute((int)AttribType.DefaultPlayerID, TeamId);
				Runner.SkEntityBase.SetAttribute((int)AttribType.DamageID, TeamId);
				Runner.SkEntityBase.SetAttribute((int)AttribType.CampID, TeamId);
			}
        }
	}

	#region netrequest
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
	#endregion

	#region Action Callback APIs
	void RPC_Tower_InitData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		_pos = transform.position = stream.Read<Vector3>();
		rot = transform.rotation = stream.Read<Quaternion>();
		ItemObject itemObj = stream.Read<ItemObject>();

		if (null == itemObj)
		{
			LogManager.Error("Invalid tower item");
			return;
		}

        //[zhujiangbo]
//        int itemProtoId = itemObj.protoId;
		ItemAsset.Drag drag = itemObj.GetCmpt<ItemAsset.Drag>();
		if (null == drag)
			return;

		DragTowerAgent item = new DragTowerAgent(drag, transform.position, Vector3.one, transform.rotation, Id, this);
		item.Create();
		SceneMan.AddSceneObj(item);

		_entity = EntityMgr.Instance.Get(Id);
		if (null == _entity)
			return;

		Pathea.TowerProtoDb.Item tower = Pathea.TowerProtoDb.Get(itemObj.protoData.towerEntityId);
		if (null != tower)
			gameObject.name = tower.name + "_" + Id;

		OnSpawned(_entity.GetGameObject());

		//OnSkAttrInitEvent += InitForceData;

		InitMapInfo();
	}

	void RPC_Tower_Move(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		//if (IsController)
		//	return;

		//Vector3 position = stream.Read<Vector3>();
	}

	void RPC_Tower_Target(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (hasOwnerAuth)
			return;

        if (null == _towerCmpt)
            return;

        float rotY;
        Vector3 pitchEuler;
        if (stream.TryRead<float>(out rotY) && stream.TryRead<Vector3>(out pitchEuler))
        {
            _towerCmpt.ApplyChassis(rotY);
            _towerCmpt.ApplyPitchEuler(pitchEuler);
        }
	}

	void RPC_S2C_AimPosition(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (hasOwnerAuth)
			return;

		aimId = stream.Read<int>();
		PeEntity tarEntity = EntityMgr.Instance.Get(aimId);
		if (null == tarEntity)
			return;

		if (null != _entity && null != _towerCmpt)
			_towerCmpt.Target = tarEntity.centerBone;
	}

	void RPC_S2C_Fire(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (hasOwnerAuth)
			return;

		int skEntityId = stream.Read<int>();
        PeEntity tarEntity = EntityMgr.Instance.Get(skEntityId);
        if (null == tarEntity || tarEntity.skEntity == null)
            return;

		if (null != _entity && null != _towerCmpt)
			_towerCmpt.Fire(tarEntity.skEntity);
	}

	void RPC_S2C_LostEnemy(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		aimId = -1;
		if (null != _entity && null != _towerCmpt)
			_towerCmpt.Target = null;
	}

	protected override void RPC_S2C_InitData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		aimId = stream.Read<int>();
		StartCoroutine(WaitForTarget());
	}
	#endregion
}
