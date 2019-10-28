using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class AiSceneStaticObjectNetwork : AiNetwork
{
	protected int _ownerId;

	public int OwnerId { get { return _ownerId; } }

	protected PeMap.StaticPoint _flagPos;

	protected override void OnPEInstantiate(uLink.NetworkMessageInfo info)
	{
		_id = info.networkView.initialData.Read<int>();
		_teamId = info.networkView.initialData.Read<int>();
		_ownerId = info.networkView.initialData.Read<int>();
		_externId = info.networkView.initialData.Read<int>();
		death = false;

		gameObject.name = "scenestatic_" + Id;
	}

	protected override void OnPEStart()
	{
		BindSkAction();

		BindAction(EPacketType.PT_SO_InitData, RPC_SO_InitData);
		BindAction(EPacketType.PT_InGame_SetController, RPC_S2C_SetController);
		BindAction(EPacketType.PT_InGame_LostController, RPC_S2C_LostController);

		RPCServer(EPacketType.PT_SO_InitData);
	}

	protected override void OnPEDestroy ()
	{
		StopAllCoroutines();

        DragArticleAgent.Destory(Id);

		if (null != _flagPos)
		{
			PeMap.LabelMgr.Instance.Remove(_flagPos);
			_flagPos = null;
		}

		int index = PeMap.MaskTile.Mgr.Instance.GetMapIndex(transform.position);
		PeMap.MaskTile mt = PeMap.MaskTile.Mgr.Instance.Get(index);
		if (null != mt)
		{
			mt.forceGroup = -1;
			PeMap.MaskTile.Mgr.Instance.Add(index, mt);
		}

		if (null == Runner)
			return;

		Destroy(Runner.gameObject);
	}

	void RPC_SO_InitData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		ItemAsset.ItemObject itemObj = stream.Read<ItemAsset.ItemObject>();
		transform.position = stream.Read<Vector3>();
		transform.rotation = stream.Read<Quaternion>();

		if (null == itemObj)
			return;

		ItemAsset.Drag drag = itemObj.GetCmpt<ItemAsset.Drag>();
		if (null == drag)
			return;

		DragArticleAgent item = DragArticleAgent.Create(drag, transform.position, transform.localScale, transform.rotation, Id, this);
		if (item.itemLogic != null)
		{
			DragItemLogicFlag flag = item.itemLogic as DragItemLogicFlag;
			if (flag != null)
			{
				_entity = Pathea.EntityMgr.Instance.Get(Id);
				OnSpawned(flag.gameObject);
			}
		}

		if (null == _flagPos)
		{
			_flagPos = new PeMap.StaticPoint();
			_flagPos.icon = PeMap.MapIcon.FlagIcon;
            _flagPos.fastTravel = true;
			_flagPos.text = "Flag_" + Id;
			//_flagPos.campId = TeamId;
			_flagPos.position = transform.position;

			PeMap.LabelMgr.Instance.Add(_flagPos);
		}

		StartCoroutine(RefreshFlag());
	}

	IEnumerator RefreshFlag()
	{
		while (null == PlayerNetwork.mainPlayer)
			yield return null;

		int index = PeMap.MaskTile.Mgr.Instance.GetMapIndex(transform.position);
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
