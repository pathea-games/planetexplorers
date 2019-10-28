using AiAsset;
using CustomData;
using ItemAsset;
using NaturalResAsset;
using Pathea;
using SkillAsset;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

//Reduce the amount of package
public struct BaseSyncAttr
{
	public Vector3 Pos { get; set; }
	public float EulerY { get; set; }
	public Vector3 Speed {get; set;}
}

public class AiNetwork : SkNetworkInterface
{
	protected int _externId;
	protected float _scale;
	protected float lastRotY;

    [SerializeField]
    public PeEntity _entity;
    protected PeTrans _viewTrans;
	protected Vector3 lastPosition;
	protected BaseSyncAttr _syncAttr;
	protected List<ItemObject> equipList = new List<ItemObject> ();
	protected ItemSample[] dropItems;

	protected int _groupId;
	protected int _tdId;
	protected int _dungeonId;
	protected int _colorType;
	protected int _playerId;
    protected int _fixId = -1;
	protected int _buffId;
    protected bool _canride = true;

	protected bool death;
	public int ExternId { get { return _externId; } }

	public float Scale { get { return _scale; } }

	internal AnimatorCmpt animatorCmpt
	{
		get
		{
			if(_animatorCmpt == null)
			{
				if (null == Runner || null == Runner.SkEntityPE)
					return null;
				_animatorCmpt = Runner.SkEntityPE.gameObject.GetComponent<AnimatorCmpt>();
			}
			return _animatorCmpt;
		}
	}

	AnimatorCmpt _animatorCmpt;

	Motion_Equip _equipment;

	Motion_Equip Equipment
	{
		get
		{
			if(_equipment == null)
			{
				if (null == Runner || null == Runner.SkEntityPE)
					return null;
				_equipment = Runner.SkEntityPE.gameObject.GetComponent<Motion_Equip>();
			}
			return _equipment;
		}
	}

	IKAimCtrl _iKAim;

	IKAimCtrl IKAim
	{
		get
		{
			if(_iKAim == null)
			{
				if (null == Runner || null == Runner.SkEntityPE)
					return null;
				_iKAim = Runner.SkEntityPE.gameObject.GetComponent<IKAimCtrl>();
			}
			return _iKAim;
		}
	}

	BiologyViewCmpt _view;
	BiologyViewCmpt View
	{
		get
		{
			if(_view == null)
			{
				if (null == Runner || null == Runner.SkEntityPE)
					return null;
				_view = Runner.SkEntityPE.gameObject.GetComponent<BiologyViewCmpt>();
			}
			return _view;
		}
	}

	protected override void OnPEInstantiate (uLink.NetworkMessageInfo info)
	{
		_id = info.networkView.initialData.Read<int> ();
		_externId = info.networkView.initialData.Read<int> ();
		_scale = info.networkView.initialData.Read<float> ();
		authId = info.networkView.initialData.Read<int> ();

		_groupId = info.networkView.initialData.Read<int>();
		_tdId = info.networkView.initialData.Read<int>();
		_dungeonId = info.networkView.initialData.Read<int>();
		_colorType = info.networkView.initialData.Read<int>();
		_playerId = info.networkView.initialData.Read<int>();

        _fixId = info.networkView.initialData.Read<int>();
		_buffId = info.networkView.initialData.Read<int>();

        //怪物能否乘骑标志
        _canride = info.networkView.initialData.Read<bool>();

		death = false;

		_pos = transform.position;
		rot = transform.rotation;

		_syncAttr.Pos = _pos;
		_syncAttr.EulerY = rot.eulerAngles.y;
	}

	private void OnAlterAttribs (int type)
	{
	}

	protected override void OnPEStart ()
	{
		BindSkAction ();

		BindAction (EPacketType.PT_AI_InitData, RPC_S2C_InitData);
		BindAction (EPacketType.PT_AI_AnimatorState, RPC_S2C_ResponseAnimatorState);
		BindAction (EPacketType.PT_InGame_SetController, RPC_S2C_SetController);
		BindAction (EPacketType.PT_AI_Move, RPC_S2C_AiNetworkMovePostion);
		BindAction (EPacketType.PT_AI_RotY, RPC_S2C_NetRotation);
		BindAction (EPacketType.PT_AI_IKTarget, RPC_S2C_AiNetworkIKTarget);
		BindAction (EPacketType.PT_InGame_LostController, RPC_S2C_LostController);
		BindAction (EPacketType.PT_AI_Animation, RPC_S2C_PlayAnimation);
		BindAction (EPacketType.PT_AI_Equipment, RPC_S2C_SyncEquips);
		BindAction (EPacketType.PT_AI_ApplyHpChange, RPC_S2C_ApplyHpChange);
		BindAction (EPacketType.PT_AI_Death, RPC_S2C_Death);
		BindAction (EPacketType.PT_AI_Turn, RPC_S2C_Turn);
		BindAction (EPacketType.PT_AI_RifleAim, RPC_S2C_RifleAim);
		BindAction (EPacketType.PT_AI_IKPosWeight, RPC_S2C_SetIKPositionWeight);
		BindAction (EPacketType.PT_AI_IKPosition, RPC_S2C_SetIKPosition);
		BindAction (EPacketType.PT_AI_IKRotWeight, RPC_S2C_SetIKRotationWeight);
		BindAction (EPacketType.PT_AI_IKRotation, RPC_S2C_SetIKRotation);

		BindAction (EPacketType.PT_AI_BoolString, RPC_S2C_SetBool_String);
		BindAction (EPacketType.PT_AI_BoolInt, RPC_S2C_SetBool_Int);
		BindAction (EPacketType.PT_AI_VectorString, RPC_S2C_SetVector_String);
		BindAction (EPacketType.PT_AI_VectorInt, RPC_S2C_SetVector_Int);
		BindAction (EPacketType.PT_AI_IntString, RPC_S2C_SetInteger_String);
		BindAction (EPacketType.PT_AI_IntInt, RPC_S2C_SetInteger_Int);
		BindAction (EPacketType.PT_AI_LayerWeight, RPC_S2C_SetLayerWeight);
		BindAction (EPacketType.PT_AI_LookAtWeight, RPC_S2C_SetLookAtWeight);
		BindAction (EPacketType.PT_AI_LookAtPos, RPC_S2C_SetLookAtPosition);
		BindAction (EPacketType.PT_AI_SetBool, RPC_S2C_SetBool);
		BindAction (EPacketType.PT_AI_SetTrigger, RPC_S2C_SetTrigger);
		BindAction (EPacketType.PT_AI_SetMoveMode, RPC_S2C_SetMoveMode);
		BindAction (EPacketType.PT_AI_HoldWeapon, RPC_S2C_HoldWeapon);
		BindAction (EPacketType.PT_AI_SwitchHoldWeapon, RPC_S2C_SwitchHoldWeapon);
		BindAction (EPacketType.PT_AI_SwordAttack, RPC_S2C_SwordAttack);
		BindAction (EPacketType.PT_AI_TwoHandWeaponAttack, RPC_S2C_TwoHandWeaponAttack);
		BindAction (EPacketType.PT_AI_SetIKAim, RPC_S2C_SetIKAim);
		BindAction (EPacketType.PT_AI_Fadein, RPC_S2C_Fadein);
		BindAction (EPacketType.PT_AI_Fadeout, RPC_S2C_Fadeout);
		BindAction (EPacketType.PT_InGame_DeadObjItem, RPC_C2S_ResponseDeadObjItem);

        BindAction(EPacketType.PT_Common_ScenarioId, RPC_S2C_ScenarioId);
		BindAction(EPacketType.PT_AI_AvatarData, RPC_S2C_AvatarData);

        RPCServer (EPacketType.PT_AI_InitData);
	}

    protected virtual IEnumerator SyncMove()
    {
        _pos = transform.position;
        rot = transform.rotation;

        while (hasOwnerAuth)
        {
            if (null != _viewTrans)
            {
				_pos = transform.position = _viewTrans.position;
				rot = transform.rotation = _viewTrans.rotation;
            }

            if (Mathf.Abs(_syncAttr.Pos.x - _pos.x) > PlayerSynAttribute.SyncMovePrecision ||
				Mathf.Abs(_syncAttr.Pos.y - _pos.y) > PlayerSynAttribute.SyncMovePrecision ||
				Mathf.Abs(_syncAttr.Pos.z - _pos.z) > PlayerSynAttribute.SyncMovePrecision)
            {
                _syncAttr.Pos = _pos;
                URPCServer(EPacketType.PT_AI_Move, _pos);

                if (null != _move && null != _entity)
                {
                    if(_entity.Race == ERace.Mankind && _entity.proto == EEntityProto.Monster)
                        _move.AddNetTransInfo(_pos, rot.eulerAngles,_move.speed,GameTime.Timer.Second);
                    else
                        _move.NetMoveTo(_pos, Vector3.zero);
                }
            }

            if(null != _entity && !(_entity.proto == EEntityProto.Monster && _entity.Race == ERace.Mankind))
            {
                if (Mathf.Abs(_syncAttr.EulerY - rot.eulerAngles.y) > PlayerSynAttribute.SyncMovePrecision)
                {
                    _syncAttr.EulerY = rot.eulerAngles.y;
                    int rotEuler = VCUtils.CompressEulerAngle(rot.eulerAngles);
                    URPCServer(EPacketType.PT_AI_RotY, rotEuler);
                }
            }

            yield return new WaitForSeconds(1 / uLink.Network.sendRate);
        }
    }

	public override void OnSpawned(GameObject obj)
	{
		base.OnSpawned(obj);

		if (null == _entity.netCmpt)
			_entity.netCmpt = _entity.Add<NetCmpt>();

		_entity.netCmpt.network = this;

		StartCoroutine(AuthorityCheckCoroutine());
	}

	protected override void ResetContorller()
    {
		base.ResetContorller();

        if (hasOwnerAuth)
        {
			StartCoroutine(SyncMove());
        }
    }

	//	public override void OnPeMsg(EMsg msg, params object[] args)
	//	{
	//		switch (msg)
	//		{
	//			case EMsg.Lod_Collider_Destroying:
	//				canGetAuth = false;
	//				lastAuthId = authId;

	//				if (hasOwnerAuth)
	//				{
	//#if UNITY_EDITOR
	//					Debug.LogFormat("<color=blue>you lost [{0}]'s authority.</color>", Id);
	//#endif

	//					authId = -1;
	//					RPCServer(EPacketType.PT_InGame_LostController);
	//				}
	//				ResetContorller();
	//				break;

	//			case EMsg.Lod_Collider_Created:
	//				canGetAuth = true;
	//				if (!hasAuth)
	//				{
	//					RPCServer(EPacketType.PT_InGame_SetController);
	//				}
	//				else
	//				{
	//					ResetContorller();

	//#if UNITY_EDITOR
	//					PlayerNetwork p = PlayerNetwork.GetPlayer(authId);
	//					if (null != p)
	//						Debug.LogFormat("<color=blue>{0} got [{1}]'s authority.</color>", p.RoleName, Id);
	//#endif
	//				}
	//				break;
	//		}
	//	}

	internal void CreateAi()
	{
        _entity = PeEntityCreator.Instance.CreateMonsterNet(Id, ExternId, Vector3.zero, Quaternion.identity, Vector3.one, Scale,_buffId);
		if (null == _entity)
			return;

		if (_fixId != -1)
		{
			SceneEntityCreatorArchiver.Instance.SetEntityByFixedSpId(_fixId, _entity);
           // if (_entity.monster) _entity.monster.Ride(false);
        }

        _entity.monster.Ride(_canride);

        _viewTrans = _entity.GetCmpt<PeTrans>();
		if (null == _viewTrans)
		{
			Debug.LogError("entity has no ViewCmpt");
			return;
		}

		_viewTrans.position = transform.position;

		_move = _entity.GetCmpt<Motion_Move>();

		NetCmpt net = _entity.GetCmpt<NetCmpt>();
		if (null == net)
			net = _entity.Add<NetCmpt>();

		net.network = this;

		MonsterProtoDb.Item data = MonsterProtoDb.Get(ExternId);
		if (null == data)
			gameObject.name = string.Format("TemplateId:{0}, Id:{1}", ExternId, Id);
		else
			gameObject.name = string.Format("{0}, TemplateId:{1}, Id:{2}", data.name, ExternId, Id);

		if (-1 != _groupId)
			AIGroupNetWork.OnMonsterAdd(_groupId, this, _entity);

		if (-1 != _tdId)
        {
            AiTowerDefense.OnMonsterAdd(_tdId, this, _entity);
            if (_entity.monster) _entity.monster.Ride(false);
        }

        if (-1 != _dungeonId)
		{
            _entity.SetAttribute(AttribType.CampID, DungeonMonster.CAMP_ID);
            _entity.SetAttribute(AttribType.DamageID, DungeonMonster.DAMAGE_ID);

            if (_entity.monster) _entity.monster.Ride(false);
        }

		if (-1 != _colorType)
		{
			PeEntityCreator.InitMonsterSkinRandomNet(_entity, _colorType);
		}

		if (-1 != _playerId)
		{
            _entity.SetAttribute(AttribType.DefaultPlayerID, _playerId);
		}

		OnSpawned (_entity.GetGameObject ());
        if (_entity.Race == ERace.Mankind && _entity.proto == EEntityProto.Monster)
            _move.AddNetTransInfo(transform.position, rot.eulerAngles, _move.speed, GameTime.Timer.Second);
        else
            _move.NetMoveTo(transform.position, Vector3.zero);
   //     if (_move is Motion_Move_Motor)
			//(_move as Motion_Move_Motor).NetMoveTo( transform.position,Vector3.zero);
	}

	void InitDeadItems (ItemDropPeEntity dropEntity, ItemSample[] items)
	{
        if (null != dropEntity)
        {
            dropEntity.RemoveDroppableItemAll();

            if (null != items)
            {
                foreach (ItemSample item in items)
                    dropEntity.AddDroppableItem(item);
            }
        }
		
	}

	#region requestnet
	public void RequestSetBool(int str,bool b)
	{
		RPCServer(EPacketType.PT_AI_SetBool,str,b);
	}
    public void RequestSetBool(string str, bool b)
    {
        RPCServer(EPacketType.PT_AI_SetBool, str, b);
    }
    public void RequestSetTrigger(string str)
	{
		RPCServer(EPacketType.PT_AI_SetTrigger,str);
	}
	public void RequestSetMoveMode(int mode)
	{
		RPCServer(EPacketType.PT_AI_SetMoveMode,mode);
	}
//	public void RequestHoldWeapon(int mode)
//	{
//		RPCServer(EPacketType.PT_AI_HoldWeapon,mode);
//	}
//	public void RequestSwitchHoldWeapon(int mode)
//	{
//		RPCServer(EPacketType.PT_AI_SwitchHoldWeapon,mode);
//	}
	public void RequestSwordAttack(Vector3 dir)
	{
		RPCServer(EPacketType.PT_AI_SwordAttack,dir);
	}

	public void RequestTwoHandWeaponAttack(Vector3 dir, int handType = 0, int time = 0)
	{
		RPCServer(EPacketType.PT_AI_TwoHandWeaponAttack,dir,handType,time);
	}

//	public void RequestSetIKAim(Vector3 dir, int handType = 0, int time = 0)
//	{
//		RPCServer(EPacketType.PT_AI_SetIKAim);
//	}
	public void RequestFadein(float time)
	{
		RPCServer(EPacketType.PT_AI_Fadein,time);
	}

	public void RequestFadeout(float time)
	{
		RPCServer(EPacketType.PT_AI_Fadeout,time);
	}

	#endregion

	#region Action Callback APIs

	protected virtual void RPC_S2C_InitData (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		_pos = transform.position = stream.Read<Vector3> ();
		rot = transform.rotation = stream.Read<Quaternion> ();
		authId = stream.Read<int> ();
	}

	protected virtual void RPC_S2C_ResponseAnimatorState (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (hasOwnerAuth)
			return;

		AiObject ai = Runner as AiObject;
		if (ai == null)
			return;

		/*bool rifleState = */stream.Read<bool> ();
		byte[] data = stream.Read<byte[]> ();

		//if (ai is AiPuja)
		//    ((AiPuja)ai).RifleAim(rifleState);

		//if (ai is AiPaja)
		//    ((AiPaja)ai).BazookaAim(rifleState);

		using (MemoryStream ms = new MemoryStream(data))
		using (BinaryReader reader = new BinaryReader(ms)) {
			int count = BufferHelper.ReadInt32 (reader);
			for (int i = 0; i < count; i++) {
				string name = BufferHelper.ReadString (reader);
				bool state = BufferHelper.ReadBoolean (reader);
				ai.SetBool (name, state);
			}
		}
	}

	protected override void RPC_S2C_SetController (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
        authId = stream.Read<int> ();
        lastAuthId = authId;
        ResetContorller ();

#if UNITY_EDITOR
		//PlayerNetwork p = PlayerNetwork.GetPlayer(authId);
		//if (null != p)
		//	Debug.LogFormat("<color=blue>{0} got [{1}]'s authority.</color>", p.RoleName, Id);
#endif
	}

	protected void RPC_S2C_AiNetworkMovePostion (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
        _pos = transform.position = stream.Read<Vector3>();
        if (!hasOwnerAuth)
        {
            if (null != _move)
            {
                if (null != _entity && _entity.Race == ERace.Mankind && _entity.proto == EEntityProto.Monster)
                    _move.AddNetTransInfo(_pos, rot.eulerAngles, _move.speed, GameTime.Timer.Second);
                else
                {
                    //lz-2017.02.17 如果我看不到这只怪了，并且这只怪是坐骑，就更改它的位置
                    if(null!=_entity && !_entity.hasView && _entity.IsMount)
                        _move.NetMoveTo(_pos, Vector3.zero,true);
                    else
                        _move.NetMoveTo(_pos, Vector3.zero);
                }
            }
        }
	}

	protected void RPC_S2C_NetRotation (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
        int rotY = stream.Read<int>();
        Vector3 euler = VCUtils.UncompressEulerAngle(rotY);
        rot = transform.rotation = Quaternion.Euler(euler);

        if (!hasOwnerAuth)
        {
            if (null != _move)
                _move.NetRotateTo(euler);
        }
	}

	protected virtual void RPC_S2C_AiNetworkIKTarget (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (hasOwnerAuth)
			return;

        Vector3 aimTarget;
        if (stream.TryRead<Vector3>(out aimTarget))
        {
            if (null == Runner)
                return;

            //if (Runner is AiNative)
            //{
            //    AiNative ai = Runner as AiNative;
            //    if (null != ai.aimer)
            //        ai.aimer.aimTarget = aimTarget;
            //}
        }
	}

	protected override void RPC_S2C_LostController (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		authId = -1;
        ResetContorller();

        if (canGetAuth)
            RPCServer(EPacketType.PT_InGame_SetController);
	}

	protected virtual void RPC_S2C_PlayAnimation (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		/*string name = */stream.Read<string> ();

		if (hasOwnerAuth)
			return;

		//AiMonster ai = Runner as AiMonster;
		//if (ai != null)
		//    ai.PlayAiAnimation(name);
	}

	protected virtual void RPC_S2C_SyncEquips(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int[] equipIDs = stream.Read<int[]>();

		if (null == _entity)
			return;

		EquipmentCmpt equipmentCmpt = _entity.GetCmpt<EquipmentCmpt>();
		if (null == equipmentCmpt)
			return;

		foreach (int id in equipIDs)
		{
			if (-1 == id)
				continue;

			ItemObject equip = ItemMgr.Instance.Get(id);
			if (null != equip)
				equipmentCmpt.PutOnEquipment(equip);
		}
	}

	protected virtual void RPC_S2C_ApplyHpChange (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		float damage = stream.Read<float> ();
		int life = stream.Read<int> ();
		int casterId = stream.Read<int> ();

		CommonInterface caster = null;
		NetworkInterface network = Get (casterId);
		if (null != network)
			caster = network.Runner;

		if (null != Runner)
			Runner.NetworkApplyDamage (caster, damage, life);
	}

	protected virtual void RPC_S2C_Death (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		/*int casterId = */stream.Read<int> ();
		death = true;

		if (null == Runner || null == Runner.SkEntityPE)
			return;

		ItemDropPeEntity dropEntity = Runner.SkEntityPE.gameObject.GetComponent<ItemDropPeEntity> ();
		if (null == dropEntity)
			dropEntity = Runner.SkEntityPE.gameObject.AddComponent<ItemDropPeEntity> ();

        InitDeadItems(dropEntity, dropItems);
	}

	protected virtual void RPC_S2C_Turn (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		float rotY = stream.Read<float> ();
		transform.rotation = Quaternion.Euler (0, rotY, 0);

		if (null != Runner)
			_viewTrans.rotation = transform.rotation;
	}

	protected void RPC_S2C_RifleAim (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (hasOwnerAuth)
			return;

		if (null == Runner)
			return;

		/*bool rifleState = */stream.Read<bool> ();

		//if (Runner is AiPuja)
		//    ((AiPuja)Runner).RifleAim(rifleState);

		//if (Runner is AiPaja)
		//    ((AiPaja)Runner).BazookaAim(rifleState);
	}

	protected void RPC_S2C_SetIKPositionWeight (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (hasOwnerAuth || null == Runner)
			return;
		AvatarIKGoal goal = stream.Read<AvatarIKGoal> ();
		float value = stream.Read<float> ();

		AiObject ai = Runner as AiObject;
		if (ai != null) {
			switch (goal) {
			case AvatarIKGoal.LeftFoot:
				ai.SetLeftFootIKWeight (value);
				break;

			case AvatarIKGoal.LeftHand:
				ai.SetLeftHandIKWeight (value);
				break;

			case AvatarIKGoal.RightFoot:
				ai.SetRightFootIKWeight (value);
				break;

			case AvatarIKGoal.RightHand:
				ai.SetRightHandIKWeight (value);
				break;

			default:
				break;
			}
		}
	}

	protected void RPC_S2C_SetIKPosition (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (hasOwnerAuth || null == Runner)
			return;
		AvatarIKGoal goal = stream.Read<AvatarIKGoal> ();
		Vector3 goalPosition = stream.Read<Vector3> ();
		AiObject ai = Runner as AiObject;
		if (ai != null) {
			switch (goal) {
			case AvatarIKGoal.LeftFoot:
				ai.SetLeftFootIKPosition (goalPosition);
				break;

			case AvatarIKGoal.LeftHand:
				ai.SetLeftHandIKPosition (goalPosition);
				break;

			case AvatarIKGoal.RightFoot:
				ai.SetRightFootIKPosition (goalPosition);
				break;

			case AvatarIKGoal.RightHand:
				ai.SetRightHandIKPosition (goalPosition);
				break;

			default:
				break;
			}
		}
	}

	protected void RPC_S2C_SetIKRotationWeight (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (hasOwnerAuth || null == Runner)
			return;
		AvatarIKGoal goal = stream.Read<AvatarIKGoal> ();
		float value = stream.Read<float> ();
		AiObject ai = Runner as AiObject;
		if (ai != null) {
			ai.SetIKRotationWeight (goal, value);
		}
	}

	protected void RPC_S2C_SetIKRotation (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (hasOwnerAuth || null == Runner)
			return;
		AvatarIKGoal goal = stream.Read<AvatarIKGoal> ();
		Quaternion goalPosition = stream.Read<Quaternion> ();
		AiObject ai = Runner as AiObject;
		if (ai != null) {
			ai.SetIKRotation (goal, goalPosition);
		}
	}

	//[RPC]
	//protected virtual void RPC_S2C_SetFloat_String(uLink.BitStream stream)
	//{
	//	if (IsController || null == Runner) return;
	//	string name = stream.Read<string>();
	//	float value = stream.Read<float>();

	//	AiObject ai = Runner as AiObject;
	//	if (ai != null)
	//	{
	//		ai.SetFloat(name,value);
	//	}
	//}

	//[RPC]
	//protected virtual void RPC_S2C_SetFloat_Int(uLink.BitStream stream)
	//{
	//	if (IsController || null == Runner) return;
	//	int id = stream.Read<int>();
	//	float value = stream.Read<float>();
	//	AiObject ai = Runner as AiObject;
	//	if (ai != null)
	//	{
	//		ai.SetFloat(id, value);
	//	}
	//}

	//[RPC]
	//protected virtual void RPC_S2C_SetFloat_String_1(uLink.BitStream stream)
	//{
	//	if (IsController || null == Runner) return;
	//	string name = stream.Read<string>();
	//	float value = stream.Read<float>();
	//	float dampTime = stream.Read<float>();
	//	float deltaTime = stream.Read<float>();

	//	AiObject ai = Runner as AiObject;
	//	if (ai != null)
	//	{
	//		ai.SetFloat(name, value, dampTime, deltaTime);
	//	}

	//}

	//[RPC]
	//protected virtual void RPC_S2C_RPC_C2S_SetFloat_Int_1(uLink.BitStream stream)
	//{
	//	if (IsController || null == Runner) return;
	//	int id = stream.Read<int>();
	//	float value = stream.Read<float>();
	//	float dampTime = stream.Read<float>();
	//	float deltaTime = stream.Read<float>();
	//	AiObject ai = Runner as AiObject;
	//	if (ai != null)
	//	{
	//		ai.SetFloat(id, value, dampTime, deltaTime);
	//	}
	//}

	protected virtual void RPC_S2C_SetBool_String (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (hasOwnerAuth || null == Runner)
			return;
		string name = stream.Read<string> ();
		bool value = stream.Read<bool> ();
		AiObject ai = Runner as AiObject;
		if (ai != null) {
			ai.SetBool (name, value);
		}
	}

	protected virtual void RPC_S2C_SetBool_Int (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (hasOwnerAuth || null == Runner)
			return;
		int id = stream.Read<int> ();
		bool value = stream.Read<bool> ();
		AiObject ai = Runner as AiObject;
		if (ai != null) {
			ai.SetBool (id, value);
		}
	}

	protected virtual void RPC_S2C_SetVector_String (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (hasOwnerAuth || null == Runner)
			return;
		string name = stream.Read<string> ();
		Vector3 value = stream.Read<Vector3> ();

		AiObject ai = Runner as AiObject;
		if (ai != null) {
			ai.SetVector (name, value);
		}
	}

	protected virtual void RPC_S2C_SetVector_Int (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (hasOwnerAuth || null == Runner)
			return;
		int id = stream.Read<int> ();
		Vector3 value = stream.Read<Vector3> ();

		AiObject ai = Runner as AiObject;
		if (ai != null) {
			ai.SetVector (id, value);
		}
	}

	protected virtual void RPC_S2C_SetInteger_String (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (hasOwnerAuth || null == Runner)
			return;
		string name = stream.Read<string> ();
		int value = stream.Read<int> ();
		AiObject ai = Runner as AiObject;
		if (ai != null) {
			ai.SetInteger (name, value);
		}
	}

	protected virtual void RPC_S2C_SetInteger_Int (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (hasOwnerAuth || null == Runner)
			return;
		int id = stream.Read<int> ();
		int value = stream.Read<int> ();
		AiObject ai = Runner as AiObject;
		if (ai != null) {
			ai.SetInteger (id, value);
		}
	}

	protected virtual void RPC_S2C_SetLayerWeight (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (hasOwnerAuth || null == Runner)
			return;
		int layerIndex = stream.Read<int> ();
		float weight = stream.Read<float> ();

		AiObject ai = Runner as AiObject;
		if (ai != null) {
			ai.SetLayerWeight (layerIndex, weight);
		}
	}

	protected virtual void RPC_S2C_SetLookAtWeight (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (hasOwnerAuth || null == Runner)
			return;
		float weight = stream.Read<float> ();
		AiObject ai = Runner as AiObject;
		if (ai != null) {
			ai.LookAtWeight (weight);
		}
	}

	protected virtual void RPC_S2C_SetLookAtPosition (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (hasOwnerAuth || null == Runner)
			return;
		Vector3 lookAtPosition = stream.Read<Vector3> ();
		AiObject ai = Runner as AiObject;
		if (ai != null && lookAtPosition != Vector3.zero) {
			ai.LookAtPosition (lookAtPosition);
		}
	}

    protected virtual void RPC_S2C_SetBool(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (hasOwnerAuth || null == Runner)
			return;
		int name = stream.Read<int> ();
        //string name = stream.Read<string>();
        bool b = stream.Read<bool> ();
		if(animatorCmpt != null)
			animatorCmpt.SetBool(name,b);
	}

    protected virtual void RPC_S2C_SetTrigger(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (hasOwnerAuth || null == Runner)
			return;
		string str = stream.Read<string> ();
		if(animatorCmpt != null)
			animatorCmpt.SetTrigger(str);
	}

    protected virtual void RPC_S2C_SetMoveMode(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (hasOwnerAuth || null == Runner)
			return;
		int mode = stream.Read<int> ();
		if(_move != null)
			_move.mode = (MoveMode)mode;
	}

    protected virtual void RPC_S2C_HoldWeapon(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (hasOwnerAuth || null == Runner)
			return;

	}

    protected virtual void RPC_S2C_SwitchHoldWeapon(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (hasOwnerAuth || null == Runner)
			return;
		
	}

    protected virtual void RPC_S2C_SwordAttack(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (hasOwnerAuth || null == Runner)
			return;
		Vector3 v3 = stream.Read<Vector3>();
		if(Equipment != null)
			Equipment.SwordAttack(v3);
	}

    protected virtual void RPC_S2C_TwoHandWeaponAttack(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (hasOwnerAuth || null == Runner)
			return;
		Vector3 v3 = stream.Read<Vector3>();
		int type = stream.Read<int>();
		int time = stream.Read<int>();
		if(Equipment != null)
			Equipment.TwoHandWeaponAttack(v3,type,time);
	}

    protected virtual void RPC_S2C_SetIKAim(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (hasOwnerAuth || null == Runner)
			return;
	}

    protected virtual void RPC_S2C_Fadein(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (hasOwnerAuth || null == Runner)
			return;
		float time = stream.Read<float>();
		if(View != null)
			View.Fadein(time);
	}

    protected virtual void RPC_S2C_Fadeout(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (hasOwnerAuth || null == Runner)
			return;
		float time = stream.Read<float>();
		if(View != null)
			View.Fadeout(time);
	}

	protected void RPC_C2S_ResponseDeadObjItem (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
        int count = stream.Read<int>();
        if (count != 0)
		    dropItems = stream.Read<ItemSample[]> ();
	}

	protected void RPC_S2C_ResetPosition (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		_pos = transform.position = stream.Read<Vector3> ();

		if (null == _move)
			return;

        //_move.NetMoveTo(pos, Vector3.zero, true);
        if (_entity.Race == ERace.Mankind && _entity.proto == EEntityProto.Monster)
            _move.AddNetTransInfo(_pos, rot.eulerAngles, _move.speed, GameTime.Timer.Second);
        else
            _move.NetMoveTo(_pos, Vector3.zero,true);
        if (this is AiAdNpcNetwork && PeGameMgr.IsMultiStory)
        {
            (this as AiAdNpcNetwork).npcCmpt.FixedPointPos = _pos;
        }
	}

    protected void RPC_S2C_ScenarioId(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int scenarioId = stream.Read<int>();

        PeEntity entity = EntityMgr.Instance.Get(Id);
        if (null != entity)
            entity.scenarioId = scenarioId;
    }

	protected void RPC_S2C_AvatarData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CreateAi();

		if (ExternId >= PeEntityCreator.HumanMonsterMask)
		{
			byte[] customData = stream.Read<byte[]>();

			CustomCharactor.CustomData charactorData = new CustomCharactor.CustomData();
			charactorData.Deserialize(customData);

			if (null != _entity)
				PeEntityCreator.ApplyCustomCharactorData(_entity, charactorData);
		}

		RPCServer(EPacketType.PT_AI_ExternData);
	}
#endregion Action Callback APIs
}