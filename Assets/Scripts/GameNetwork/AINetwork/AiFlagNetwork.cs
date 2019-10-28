using UnityEngine;
using System.Collections;

public class AiFlagNetwork : SkNetworkInterface
{
	protected int _ownerId;

	public int OwnerId { get { return _ownerId; } }

	protected PeMap.StaticPoint _flagPos;

	protected override void OnPEInstantiate(uLink.NetworkMessageInfo info)
	{
		_id = info.networkView.initialData.Read<int>();
		_teamId = info.networkView.initialData.Read<int>();
		_ownerId = info.networkView.initialData.Read<int>();
		/*_externId = */info.networkView.initialData.Read<int>();

		gameObject.name = "netflag_" + Id;

		_pos = transform.position;
		PlayerNetwork.OnLimitBoundsAdd(Id, new Bounds(_pos, new Vector3(128, 128, 128)));
	}

	protected override void OnPEStart()
	{
		PlayerNetwork.OnTeamChangedEventHandler += OnResetFlag;
		BindSkAction();

		BindAction(EPacketType.PT_SO_InitData, RPC_SO_InitData);
		BindAction(EPacketType.PT_InGame_SetController, RPC_S2C_SetController);
		BindAction(EPacketType.PT_InGame_LostController, RPC_S2C_LostController);

		RPCServer(EPacketType.PT_SO_InitData);
	}

	protected override void OnPEDestroy()
	{
		PlayerNetwork.OnTeamChangedEventHandler -= OnResetFlag;
		PlayerNetwork.OnLimitBoundsDel(Id);
		base.OnPEDestroy();

		DragArticleAgent.Destory(Id);

		RemoveFlag();
	}

	void RemoveFlag()
	{
		if (null != _flagPos)
		{
			PeMap.LabelMgr.Instance.Remove(_flagPos);
			_flagPos = null;
		}

		if (!Pathea.PeGameMgr.IsStory && !Pathea.PeGameMgr.IsCustom)
		{
			int index = PeMap.MaskTile.Mgr.Instance.GetMapIndex(transform.position);
			PeMap.MaskTile mt = PeMap.MaskTile.Mgr.Instance.Get(index);
			if (null != mt)
			{
				mt.forceGroup = -1;
				PeMap.MaskTile.Mgr.Instance.Add(index, mt);
			}
		}
	}

	void AddFlag()
	{
		if (null == _flagPos)
		{
			_flagPos = new PeMap.StaticPoint();
			_flagPos.ID = Id;
			_flagPos.icon = PeMap.MapIcon.FlagIcon;
			_flagPos.fastTravel = true;
			_flagPos.text = "Flag_" + Id;
			_flagPos.position = _pos;

			PeMap.LabelMgr.Instance.Add(_flagPos);
		}

		if (!Pathea.PeGameMgr.IsStory && !Pathea.PeGameMgr.IsCustom)
		{
			int index = PeMap.MaskTile.Mgr.Instance.GetMapIndex(_pos);
			PeMap.MaskTile mt = PeMap.MaskTile.Mgr.Instance.Get(index);
			if (null != mt)
			{
				mt.forceGroup = TeamId;
			}
			else
			{
				Vector2 tilePos = PeMap.MaskTile.Mgr.Instance.GetCenterPos(index);
				byte type = PeMap.MaskTile.Mgr.Instance.GetType((int)tilePos.x, (int)tilePos.y);

				mt = new PeMap.MaskTile();
				mt.index = index;
				mt.forceGroup = TeamId;
				mt.type = type;
			}

			PeMap.MaskTile.Mgr.Instance.Add(index, mt);
		}
	}

	void OnResetFlag()
	{
		if (null == ForceSetting.Instance)
			return;

		if (ForceSetting.Instance.Conflict(TeamId, PlayerNetwork.mainPlayerId))
			RemoveFlag();
		else
			AddFlag();
	}

	void RPC_SO_InitData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		ItemAsset.ItemObject itemObj = stream.Read<ItemAsset.ItemObject>();
		_pos = transform.position = stream.Read<Vector3>();
		rot = transform.rotation = stream.Read<Quaternion>();

		if (null == itemObj)
			return;

		ItemAsset.Drag drag = itemObj.GetCmpt<ItemAsset.Drag>();
		if (null == drag)
			return;

		DragArticleAgent item = DragArticleAgent.Create(drag, _pos, transform.localScale, rot, Id, this);
		if (item.itemLogic != null)
		{
			DragItemLogicFlag flag = item.itemLogic as DragItemLogicFlag;
			if (flag != null)
			{
				OnSpawned(flag.gameObject);

				Pathea.PeEntity FlagEntity = flag.gameObject.GetComponent<Pathea.PeEntity>();
				if (null != FlagEntity)
				{
					Pathea.NetCmpt net = FlagEntity.GetCmpt<Pathea.NetCmpt>();
					if (null == net)
						net = FlagEntity.Add<Pathea.NetCmpt>();

					net.network = this;
				}
			}
		}

		OnResetFlag();
	}
}
