using UnityEngine;
using System.Collections;
using uLink;
using System.Collections.Generic;
public class AIGroupNetWork : NetworkInterface
{
	protected int _externId;
	protected int _tdId;
	protected int _dungeonId;
	protected int _colorType;
	protected int _playerId;
	protected int _buffId;
	protected List<AiNetwork> _aiList = new List<AiNetwork>();

	public int ExternId { get { return _externId; } }

	protected override void OnPEDestroy()
	{
		if (null == Runner)
			return;

		Runner.InitNetworkLayer(null);

		if (Runner != null)
			Destroy(Runner.gameObject);
	}

	protected override void OnPEInstantiate(uLink.NetworkMessageInfo info)
	{
		_id = info.networkView.initialData.Read<int>();
		_externId = info.networkView.initialData.Read<int>();
		authId = info.networkView.initialData.Read<int>();
		_tdId = info.networkView.initialData.Read<int>();
		_dungeonId = info.networkView.initialData.Read<int>();
		_colorType = info.networkView.initialData.Read<int>();
		_playerId = info.networkView.initialData.Read<int>();
		_buffId = info.networkView.initialData.Read<int>();

		_aiList.Clear();

		EntityGrp.CreateMonsterGroup(ExternId & ~Pathea.EntityProto.IdGrpMask, transform.position,_colorType,_playerId, Id,_buffId);

        Pathea.PeEntity entity = Pathea.EntityMgr.Instance.Get(Id);
        if (null == entity)
            return;

        OnSpawned(entity.GetGameObject());
    }

	public static void OnMonsterAdd(int id, AiNetwork ai, Pathea.PeEntity entity)
	{
		EntityGrp grpEntity = Pathea.EntityMgr.Instance.Get(id) as EntityGrp;
		if (null != grpEntity)
			grpEntity.OnMemberCreated(entity);
	}

	void AddAiObj(AiNetwork ai)
	{
		if (null == ai)
			return;

		if (!_aiList.Contains(ai))
			_aiList.Add(ai);
	}

	void DelAiObj(AiNetwork ai)
	{
		if (null == ai)
			return;

		if (_aiList.Contains(ai))
			_aiList.Remove(ai);
	}
}
