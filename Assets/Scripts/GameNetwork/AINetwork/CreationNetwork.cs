using Pathea;
using SkillAsset;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public delegate void DestroyHandler();

public class CreationNetwork : SkNetworkInterface
{
	protected int objectID;

	public int ObjectID { get { return objectID; } }

	internal float HP { get; set; }

	internal float MaxHP { get; set; }

	internal float Fuel { get; set; }

	internal float MaxFuel { get; set; }

	int _ownerId = -1;

	private WhiteCat.BehaviourController cc;
	internal DestroyHandler OnDestroyHandle;

	internal PlayerNetwork Driver { get; set; }

	internal List<SkNetworkInterface> Passangers = new List<SkNetworkInterface>();

	private DragArticleAgent drawItem;

	private PeTrans _viewTrans;

    WhiteCat.CreationSkEntity _entity;
    bool _bLock = false;

    protected override void OnPEInstantiate(uLink.NetworkMessageInfo info)
	{
		_id = info.networkView.initialData.Read<int>();
		_ownerId = info.networkView.initialData.Read<int>();
		_teamId = info.networkView.initialData.Read<int>();

		_pos = transform.position;
		rot = transform.rotation;
	}

	protected override void OnPEStart()
	{
		BindSkAction();

		BindAction(EPacketType.PT_CR_InitData, RPC_S2C_InitData);
		BindAction(EPacketType.PT_InGame_SetController, RPC_S2C_SetController);
		BindAction(EPacketType.PT_InGame_LostController, RPC_S2C_LostController);
		BindAction(EPacketType.PT_CR_ChargeFuel, RPC_S2C_ChargeFuel);
		BindAction(EPacketType.PT_CR_SkillCast, RPC_S2C_SkillCast);
		BindAction(EPacketType.PT_CR_SkillCastShoot, RPC_S2C_SkillCastShoot);
		BindAction(EPacketType.PT_CR_ApplyHpChange, RPC_S2C_ApplyHpChange);
		BindAction(EPacketType.PT_CR_Death, RPC_S2C_Death);
		BindAction(EPacketType.PT_CR_SyncEnergyDelta, RPC_S2C_SyncEnergyDelta);
		BindAction(EPacketType.PT_CR_SyncPos, RPC_S2C_SyncPos);
        BindAction(EPacketType.PT_InGame_MissionMoveAircraft, RPC_S2C_MissionMoveAircraft);

        RPCServer(EPacketType.PT_CR_InitData);
	}

	public override void OnSpawned(GameObject go)
	{
		base.OnSpawned(go);

		cc = go.GetComponent<WhiteCat.BehaviourController>();

//		cc.creationData.m_Hp = HP;
//		cc.creationData.m_Fuel = Fuel;
		_viewTrans = go.GetComponent<PeTrans> ();
		_viewTrans.position = transform.position;

		StartCoroutine(AuthorityCheckCoroutine());
	}

	protected override void CheckAuthority()
	{
		if (hasOwnerAuth)
		{
			if (Driver != null && Driver.Id == authId)
				return;
		}

		base.CheckAuthority();
	}

//	public override void OnPeMsg(EMsg msg, params object[] args)
//    {
//        switch (msg)
//        {
//            case EMsg.Lod_Collider_Destroying:
//                canGetAuth = false;
//				lastAuthId = authId;

//				if (hasOwnerAuth)
//				{
//					if (Driver != null && Driver.Id == authId)
//						return;

//					RPCServer(EPacketType.PT_InGame_LostController);
//					authId = -1;

//#if UNITY_EDITOR
//					Debug.LogFormat("<color=blue>you lost [{0}]'s authority.</color>", Id);
//#endif
//				}
				
//				ResetContorller();
//				break;

//            case EMsg.Lod_Collider_Created:
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
//        }
//    }

	private IEnumerator SyncMove()
	{
		_pos = transform.position;
		rot = transform.rotation;

		while (true)
		{
            if (_bLock)
            {
                yield return new WaitForSeconds(1 / uLink.Network.sendRate);
                continue;
            }
               
            if (hasOwnerAuth)
			{
				if (null != Runner)
				{
					_pos = transform.position = _viewTrans.position;
					rot = transform.rotation = _viewTrans.rotation;
				}

				if (null != cc)
				{
					if (cc.enabled)
					{
						var data = cc.C2S_GetData();
						if (data != null)
						{
							RPCServer(EPacketType.PT_CR_SyncPos, data);
						}
					}
				}
				else
				{
					RPCServer(EPacketType.PT_CR_Death);
					yield break;
				}
			}
			if(cc != null)
			{
				float energyDelta = 0;
				cc.GetAndResetDeltaEnergy(ref energyDelta);
				if(energyDelta != 0)
					RPCServer(EPacketType.PT_CR_SyncEnergyDelta,energyDelta);
			}
			yield return new WaitForSeconds(1 / uLink.Network.sendRate);
		}
	}

	protected override void ResetContorller()
	{
		base.ResetContorller();

		if (hasOwnerAuth)
		{
			if(cc != null)
				cc.ResetHost(PeCreature.Instance.mainPlayer.Id);
		}
		else
		{
			if(cc != null)
				cc.ResetHost(-1);
		}
	}

	public override void InitForceData ()
	{
		if(null != Runner && null != Runner.SkEntityBase)
		{
            if(cc is WhiteCat.CarrierController)
            {
                if (null == Driver)
                    Runner.SkEntityBase.SetAttribute((int)AttribType.DefaultPlayerID, (float)99);
                else
                    Runner.SkEntityBase.SetAttribute((int)AttribType.DefaultPlayerID, (float)Driver.Id);
            }			
		}
	}

	void SendBaseAttr()
	{
		if(hasOwnerAuth)
		{
			if (null != Runner)
			{
				if (null != Runner.SkEntityBase)
				{
					byte[] data = Runner.SkEntityBase.Export();
					if(data != null && data.Length > 0)
					{
						RPCServer(EPacketType.PT_InGame_SKSendBaseAttrs,data);
					}
				}
			}
		}
	}

    public bool GetOn(CommonInterface runner, int seatIndex)
    {
        if (null != Runner && null != Runner.SkEntityBase && runner != null)
        {
            PeEntity entity = Runner.SkEntityBase.GetComponent<PeEntity>();
            if (null != entity && null != runner.SkEntityPE && null != runner.SkEntityPE.Entity)
            {
                PassengerCmpt passenger = runner.SkEntityPE.Entity.GetCmpt<PassengerCmpt>();
                WhiteCat.CarrierController drivingController = entity.GetComponent<WhiteCat.CarrierController>();
                if (null != passenger && null != drivingController)
                {
                    passenger.GetOn(drivingController, seatIndex, false);
                    return true;

                }
            }
        }

        return false;
    }

    public void GetOff(Vector3 pos)
    {
        Passangers.Remove(this);
        foreach (var iter in Passangers)
        {
            if (iter != null && !iter.hasOwnerAuth)
                iter._move.NetMoveTo(pos, Vector3.zero);
        }
    }

    public void GetOff(Vector3 pos, EVCComponent seatType)
    {
        if (seatType != EVCComponent.cpSideSeat)
        {
            if (Driver != null && !Driver.hasOwnerAuth)
                Driver._move.NetMoveTo(pos, Vector3.zero);

            Driver = null;
        }
        else
        {
            GetOff(pos);
        }
    }

    public void AddPassanger(SkNetworkInterface pass)
    {
        if (!Passangers.Contains(pass))
            Passangers.Add(pass);
    }

    private void RPC_S2C_InitData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        ItemAsset.ItemObject item = stream.Read<ItemAsset.ItemObject>();
        _pos = transform.position = stream.Read<Vector3>();
        rot = transform.rotation = stream.Read<Quaternion>();
        HP = stream.Read<float>();
        MaxHP = stream.Read<float>();
        Fuel = stream.Read<float>();
        MaxFuel = stream.Read<float>();
		authId = stream.Read<int>();
        _ownerId = stream.Read<int>();

        if (null == item)
        {
            Debug.LogWarning("CreationNetwork RPC_S2C_InitData null item.");
            return;
        }

        DragArticleAgent agent = DragArticleAgent.Create(DragArticleAgent.CreateItemDrag(item.protoId), _pos, Vector3.one, rot, _id, this, true);
        if (agent != null && null != agent.itemLogic && null != agent.itemLogic.gameObject)
        {
            OnSpawned(agent.itemLogic.gameObject);
            _entity = agent.itemLogic.gameObject.GetComponent<WhiteCat.CreationSkEntity>();
            if (_entity != null)
            {
                NetCmpt net = _entity.GetComponent<NetCmpt>();
                if (null == net)
                    net = _entity.gameObject.AddComponent<NetCmpt>();
                net.network = this;
            }
        }
        else
        {
            Debug.LogWarningFormat("CreationNetwork RPC_S2C_InitData invalide agent:{0}", item.protoId);
        }

        OnSkAttrInitEvent += SendBaseAttr;
        StartCoroutine(SyncMove());
	}

	protected override void RPC_S2C_SetController(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		authId = stream.Read<int>();
		_teamId = stream.Read<int>();

		ResetContorller();
		SendBaseAttr();
        if (_entity == null)
            return;
        var robot = _entity.GetComponent<WhiteCat.AIBehaviourController>();
        if (robot) robot.SetCreater(_ownerId);

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

	private void RPC_S2C_SyncEnergyDelta(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
        float energy;
        if (stream.TryRead<float>(out energy))
        {
            if (cc != null)
                cc.SetEnergy(energy);
        }
	}

	private void RPC_S2C_SyncPos(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (hasOwnerAuth || _bLock )
            return;

		if(cc != null)
		{
            byte[] data;
            if (stream.TryRead<byte[]>(out data))
            {
                cc.S2C_SetData(data);

                _pos = transform.position = cc.transform.position;
                rot = transform.rotation = cc.transform.rotation;

                if (null != Driver) Driver.UpdateDriverStatus(this);
                foreach (SkNetworkInterface passanger in Passangers)
                {
                    if (passanger != null)
                        passanger.UpdateDriverStatus(this);
                }                    
            }
		}
	}

	private void RPC_S2C_ChargeFuel(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		float fuel = stream.Read<float>();
		if (null != cc) cc.SetEnergy(fuel);
	}

	protected virtual void RPC_S2C_SkillCast(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{

	}

	protected virtual void RPC_S2C_SkillCastShoot(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int skillID = stream.Read<int>();
		Vector3 pos = stream.Read<Vector3>();

		DefaultPosTarget target = new DefaultPosTarget(pos);

		SkillRunner caster = Runner as SkillRunner;
		if (null != caster && !caster.IsController)
			caster.RunEffOnProxy(skillID, target);
	}

	protected void RPC_S2C_ApplyHpChange(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		float damage = stream.Read<float>();
		int life = stream.Read<int>();
		uLink.NetworkViewID viewID = stream.Read<uLink.NetworkViewID>();

		CommonInterface caster = null;
		uLink.NetworkView view = uLink.NetworkView.Find(viewID);
		if (null != view)
		{
			NetworkInterface network = view.GetComponent<NetworkInterface>();
			if (null != network && null != network.Runner)
				caster = network.Runner;
		}

		if (null != Runner)
			Runner.NetworkApplyDamage(caster, damage, life);
	}

	protected void RPC_S2C_Death(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
//		if (cc == null || (cc as WhiteCat.DrivingController) == null)
//			return;
//		(cc as WhiteCat.DrivingController ).OnDead();
	}
    bool _physicsEnabled = false;
    protected void RPC_S2C_MissionMoveAircraft(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        bool moveOrReturn = stream.Read<bool>();
        int index = stream.Read<int>();
        if (moveOrReturn)
        {
            _bLock = true;
            cc.networkEnabled = false;

            UnityEngine.Object o = Resources.Load("Cutscene Clips/PathClip" + (index + 1));
            if (o == null)
                return;
            GameObject go = GameObject.Instantiate(o) as GameObject;
            MoveByPath mbp = _entity.gameObject.AddComponent<MoveByPath>();
            mbp.SetDurationDelay(15f, 0);
            mbp.StartMove(go, WhiteCat.RotationMode.ConstantUp, WhiteCat.TweenMethod.Linear);
            _physicsEnabled = cc.physicsEnabled;
            cc.physicsEnabled = false;
            cc.creationController.visible = true;
        }
        else
        {
            stream.Read<Vector3>();
            _bLock = true;

            GameObject zj = _entity.gameObject;
            if (zj == null)
                return;
            UnityEngine.Object o = Resources.Load("Cutscene Clips/PathClip" + (index + 5));
            if (o == null)
                return;
            GameObject go = GameObject.Instantiate(o) as GameObject;
            MoveByPath mbp = zj.AddComponent<MoveByPath>();
            mbp.SetDurationDelay(15f, 0);
            mbp.AddEndListener(delegate ()
            {
                if (_physicsEnabled)
                    cc.physicsEnabled = true;

                _bLock = false;
                cc.networkEnabled = true;
            });
            mbp.StartMove(go, WhiteCat.RotationMode.ConstantUp, WhiteCat.TweenMethod.Linear);

        }
    }

    
}