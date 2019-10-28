using Pathea;
using SkillSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using WhiteCat;
using ItemAsset;

public enum EEntityState : int
{
	Null = 0x00,
    EEntityState_Death = 0x0000001,
    EEntityState_EquipmentHold = 0x0000002,
    EEntityState_HoldShield = 0x0000004,
    EEntityState_GunHold = 0x0000008,
    EEntityState_BowHold = 0x0000010,
    EEntityState_AimEquipHold = 0x0000020,
    EEntityState_HoldFlashLight = 0x0000040,
    EEntityState_TwoHandSwordHold = 0x0000080,
    EEntityState_Sit = 0x0000100,
    EEntityState_SLeep = 0x0000200,
    EEntityState_Cure = 0x0000400,
    EEntityState_Operation = 0x0000800,
}

public class SkNetworkInterface : NetworkInterface
{
	private delegate void SendActionEventHander (PEActionType type, PEActionParam obj);

	private Dictionary<PEActionType, SendActionEventHander> SendActionEvent = new Dictionary<PEActionType, SendActionEventHander> ();
	private MotionMgrCmpt _mtCmpt = null;
	internal Motion_Move _move;
	protected event Action OnSkAttrInitEvent;

	protected MotionMgrCmpt MtCmpt {
		get {
			if (_mtCmpt == null && runner != null) {
				SkAliveEntity en = runner.SkEntityPE as SkAliveEntity;
				if (en != null) {
					_mtCmpt = en.Entity.GetCmpt<MotionMgrCmpt> ();
				}
			}
			return _mtCmpt;
		}
	}

	private PeTrans _trans;

	protected PeTrans Trans {
		get {
			if (_trans == null && runner != null) {
				SkAliveEntity en = runner.SkEntityPE as SkAliveEntity;
				if (en != null) {
					_trans = en.Entity.GetCmpt<PeTrans> ();
				}
			}
			return _trans;
		}
	}

	private IKCmpt ikCmpt = null;

	private IKCmpt IkCmpt {
		get {
			if (ikCmpt == null && runner != null) {
				SkAliveEntity en = runner.SkEntityPE as SkAliveEntity;
				if (en != null) {
					ikCmpt = en.Entity.GetCmpt<IKCmpt> ();
				}
			}
			return ikCmpt;
		}
	}

	private Vector3 _oldIkPos;

	public override void OnSpawned (GameObject obj)
	{
		base.OnSpawned (obj);

		//SkAliveEntity en = runner.SkEntityPE as SkAliveEntity;
		if (runner.SkEntityBase != null) {
			RPCServer (EPacketType.PT_InGame_SKSyncInitAttrs);
			RPCServer (EPacketType.PT_InGame_SKDAQueryEntityState);
		}
	}
	public bool IsStaticNet ()
	{

		if (this is SubTerrainNetwork || this is VoxelTerrainNetwork)
			return true;
		return false;
	}
	protected void BindSkAction ()
	{
		BindAction (EPacketType.PT_InGame_SKSyncAttr, RPC_SKSyncAttr);
		BindAction (EPacketType.PT_InGame_SKStartSkill, RPC_SKStartSkill);
		BindAction (EPacketType.PT_InGame_SKStopSkill, RPC_SKStopSkill);
		BindAction (EPacketType.PT_InGame_SKBLoop, RPC_SKBLoop);
		BindAction (EPacketType.PT_InGame_SKSyncInitAttrs, RPC_SKSyncInitAttrs);
		BindAction (EPacketType.PT_InGame_SKFellTree, RPC_SKFellTree);
		BindAction (EPacketType.PT_InGame_SKIKPos, RPC_SKIKPos);

		//doaction

		BindAction (EPacketType.PT_InGame_SKDAVVF, RPC_SKDAVVF);
		BindAction (EPacketType.PT_InGame_SKDAVFNS, RPC_SKDAVFNS);
		BindAction (EPacketType.PT_InGame_SKDANO, RPC_SKDANO);
		BindAction (EPacketType.PT_InGame_SKDAV, RPC_SKDAV);
		BindAction (EPacketType.PT_InGame_SKDAVVN, RPC_SKDAVVN);
		BindAction (EPacketType.PT_InGame_SKDAVQNS, RPC_SKDAVQNS);
		BindAction (EPacketType.PT_InGame_SKDAVQ, RPC_SKDAVQ);
		BindAction (EPacketType.PT_InGame_SKDAS, RPC_SKDAS);
		BindAction (EPacketType.PT_InGame_SKDAVQS, RPC_SKDAVQS);
		BindAction (EPacketType.PT_InGame_SKDAVVNN, RPC_SKDAVVNN);
		BindAction (EPacketType.PT_InGame_SKDAN, RPC_SKDAN);
		BindAction (EPacketType.PT_InGame_SKDAB, RPC_SKDAB);
		BindAction (EPacketType.PT_InGame_SKDAVQN, RPC_SKDAVQN);
		BindAction (EPacketType.PT_InGame_SKDAVFVFS, RPC_SKDAVFVFS);
		BindAction (EPacketType.PT_InGame_SKDAVQSN, RPC_SKDAVQSN);
		BindAction (EPacketType.PT_InGame_SKDAEndAction, RPC_SKDAEndAction);
		BindAction (EPacketType.PT_InGame_SKDAEndImmediately, RPC_SKDAEndImmediately);
		BindAction (EPacketType.PT_InGame_SKDAQueryEntityState, RPC_SKDAQueryEntityState);
        BindAction(EPacketType.PT_InGame_SKDAVQSNS, RPC_SKDAVQSNS);//2017.02.15 骑

        SendActionEvent.Add (PEActionType.Repulsed, SendSKDARepulsed);
		SendActionEvent.Add (PEActionType.Wentfly, SendSKDAWentfly);
		SendActionEvent.Add (PEActionType.Knocked, SendSKDAKnocked);
		SendActionEvent.Add (PEActionType.Death, SendSKDADeath);
		SendActionEvent.Add (PEActionType.GetUp, SendSKDAGetUp);
		SendActionEvent.Add (PEActionType.Revive, SendSKDARevive);
		SendActionEvent.Add (PEActionType.Step, SendSKDAStep);
		SendActionEvent.Add (PEActionType.EquipmentHold, SendSKDASwordHold);
		//SendActionEvent.Add (PEActionType.SwordAttack, SendSKDASwordAttack);
		SendActionEvent.Add (PEActionType.HoldShield, SendSKDAHoldShield);
		SendActionEvent.Add (PEActionType.EquipmentPutOff, SendSKDASwordPutOff);
		SendActionEvent.Add (PEActionType.Fall, SendSKDAFall);
		SendActionEvent.Add (PEActionType.GunHold, SendSKDAGunHold);
		//SendActionEvent.Add(PEActionType.GunFire, SendSKDAGunFire);
		SendActionEvent.Add (PEActionType.GunReload, SendSKDAGunReload);
		SendActionEvent.Add (PEActionType.GunPutOff, SendSKDAGunPutOff);
		SendActionEvent.Add (PEActionType.BowHold, SendSKDABowHold);
		SendActionEvent.Add (PEActionType.BowPutOff, SendSKDABowPutOff);
		SendActionEvent.Add (PEActionType.BowShoot, SendSKDABowShoot);
		SendActionEvent.Add (PEActionType.BowReload, SendSKDABowReload);
		SendActionEvent.Add (PEActionType.Sleep, SendSKDASleep);
		SendActionEvent.Add (PEActionType.Eat, SendSKDAEat);
		SendActionEvent.Add (PEActionType.AimEquipHold, SendSKDAToolHold);
		SendActionEvent.Add (PEActionType.AimEquipPutOff, SendSKDAToolPutOff);
		SendActionEvent.Add (PEActionType.Dig, SendSKDADig);
		SendActionEvent.Add (PEActionType.Fell, SendSKDAFell);
		SendActionEvent.Add (PEActionType.Gather, SendSKDAGather);
		SendActionEvent.Add (PEActionType.PickUpItem, SendSKDAPickUpItem);
//		SendActionEvent.Add(PEActionType.GetOnVehicle, SendSKDAGetOnVehicle);
		SendActionEvent.Add (PEActionType.JetPack, SendSKDAJetPack);
		SendActionEvent.Add (PEActionType.Parachute, SendSKDAParachute);
		SendActionEvent.Add (PEActionType.Glider, SendSKDAGlider);
		SendActionEvent.Add (PEActionType.Climb, SendSKDAClimb);
		SendActionEvent.Add (PEActionType.Build, SendSKDABuild);
		SendActionEvent.Add (PEActionType.HoldFlashLight, SendSKDAHoldFlashLight);
		SendActionEvent.Add (PEActionType.Stuned, SendSKDAStuned);
		SendActionEvent.Add (PEActionType.Sit, SendSKDASit);
		SendActionEvent.Add (PEActionType.Operation, SendSKDAOperation);
		SendActionEvent.Add (PEActionType.Draw, SendSKDADraw);
		SendActionEvent.Add (PEActionType.TwoHandSwordHold, SendSKDATwoHandSwordHold);
		SendActionEvent.Add (PEActionType.TwoHandSwordPutOff, SendSKDATwoHandSwordPutOff);
		//SendActionEvent.Add (PEActionType.TwoHandSwordAttack, SendSKDATwoHandSwordAttack);
//		SendActionEvent.Add (PEActionType.Jump, SendSKDAJump);
		SendActionEvent.Add (PEActionType.Cure, SendSKDACure);
		SendActionEvent.Add (PEActionType.Leisure, SendSKDATalk);
		SendActionEvent.Add (PEActionType.AlienDeath, SendSKDAAlienDeath);
        SendActionEvent.Add(PEActionType.Ride, SendSKDARide); //2017.02.15 骑

        BindAction(EPacketType.PT_InGame_SkOnDamage, RPC_S2C_SkOnDamage);
		BindAction(EPacketType.PT_InGame_AbnormalConditionStart, RPC_S2C_AbnormalConditionStart);
		BindAction(EPacketType.PT_InGame_AbnormalConditionEnd, RPC_S2C_AbnormalConditionEnd);
		BindAction(EPacketType.PT_InGame_AbnormalCondition, RPC_S2C_AbnormalCondition);
        BindAction(EPacketType.PT_InGame_Jump, RPC_S2C_Jump);

		InvokeRepeating ("SendIk", 0, 0.1f);
	}
	internal void UpdateDriverStatus (CreationNetwork creation)
	{
		if(creation != null)
			transform.position = creation.transform.position;
		//_trans.position = creation.transform.position;
		
		//		if (null != player)
		//			player.DriverMove(transform.position);
	}
	private void SendIk ()
	{
		if (!hasOwnerAuth)
			return;
		if (IkCmpt != null && IkCmpt.aimActive) {
			if (_oldIkPos != IkCmpt.aimTargetPos) {
				_oldIkPos = IkCmpt.aimTargetPos;
				RPCServer (EPacketType.PT_InGame_SKIKPos, _oldIkPos);
			}
		}
	}

	protected void RequestAbnormalCondition()
	{
		RPCServer(EPacketType.PT_InGame_AbnormalCondition);
	}

    public void RequestJump(double jumpTime)
    {
        RPCServer(EPacketType.PT_InGame_Jump,jumpTime);
    }
    void OnAttrChange(AttribType type, float value, int casterId, float dValue)
    {
        switch (type)
        {
            case AttribType.Hp:
                {
                    OnDamage(casterId, dValue);                   
                }
                break;

            
        }
    }
    void OnDamage(int casterId,float damage)
    {
        NetworkInterface caster = Get<NetworkInterface>(casterId);
        SkEntity entity = null != caster ? null != caster.Runner ? caster.Runner.SkEntityBase : null : null;

        if (Runner != null)
        {
            PESkEntity skEntity = Runner.SkEntityBase as PESkEntity;
            if (skEntity != null)
            {
                skEntity.DispatchHPChangeEvent(entity, damage);
            }
        }
    }
    #region send do action

    public void SendDoAction (PEActionType type, PEActionParam obj)
	{
		if (!hasOwnerAuth)
			return;
		if (SendActionEvent.ContainsKey (type)) {
			SendActionEvent [type] (type, obj);
		}
	}

	public void SendEndAction (PEActionType type)
	{
		if (!hasOwnerAuth)
			return;
		if (SendActionEvent.ContainsKey (type))
			RPCServer (EPacketType.PT_InGame_SKDAEndAction, type);
	}

	public void SendEndImmediately (PEActionType type)
	{
		if (!hasOwnerAuth)
			return;
		if (SendActionEvent.ContainsKey (type))
			RPCServer (EPacketType.PT_InGame_SKDAEndImmediately, type);
	}

	private void SendSKDARepulsed (PEActionType type, PEActionParam obj)
	{
		PEActionParamVVF param = obj as PEActionParamVVF;
		RPCServer (EPacketType.PT_InGame_SKDAVVF, type, param.vec1, param.vec2, param.f);
	}

	private void SendSKDAWentfly (PEActionType type, PEActionParam obj)
	{
		PEActionParamVFNS param = obj as PEActionParamVFNS;
		RPCServer (EPacketType.PT_InGame_SKDAVFNS, type, param.vec, param.f, param.n, param.str);
	}

	private void SendSKDAKnocked (PEActionType type, PEActionParam obj)
	{
		RPCServer (EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDADeath (PEActionType type, PEActionParam obj)
	{
		RPCServer (EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDAGetUp (PEActionType type, PEActionParam obj)
	{
		RPCServer (EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDARevive (PEActionType type, PEActionParam obj)
	{
		PEActionParamB param = obj as PEActionParamB;
		RPCServer (EPacketType.PT_InGame_SKDAB, type, param.b);
	}

	private void SendSKDAStep (PEActionType type, PEActionParam obj)
	{
		PEActionParamV param = obj as PEActionParamV;
		RPCServer (EPacketType.PT_InGame_SKDAV, type, param.vec);
	}

	private void SendSKDASwordHold (PEActionType type, PEActionParam obj)
	{
		RPCServer (EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDASwordAttack (PEActionType type, PEActionParam obj)
	{
		PEActionParamVVN param = obj as PEActionParamVVN;
		RPCServer (EPacketType.PT_InGame_SKDAVVN, type, param.vec1, param.vec2, param.n);
	}

	private void SendSKDAHoldShield (PEActionType type, PEActionParam obj)
	{
		RPCServer (EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDASwordPutOff (PEActionType type, PEActionParam obj)
	{
		RPCServer (EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDAFall (PEActionType type, PEActionParam obj)
	{
		RPCServer (EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDAGunHold (PEActionType type, PEActionParam obj)
	{
		RPCServer (EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDAGunFire (PEActionType type, PEActionParam obj)
	{
		PEActionParamB param = obj as PEActionParamB;
		RPCServer (EPacketType.PT_InGame_SKDAB, type, param.b);
	}

	private void SendSKDAGunReload (PEActionType type, PEActionParam obj)
	{
		PEActionParamN param = obj as PEActionParamN;
		RPCServer (EPacketType.PT_InGame_SKDAN, type, param.n);
	}

	private void SendSKDAGunPutOff (PEActionType type, PEActionParam obj)
	{
		RPCServer (EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDABowHold (PEActionType type, PEActionParam obj)
	{
		RPCServer (EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDABowPutOff (PEActionType type, PEActionParam obj)
	{
		RPCServer (EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDABowShoot (PEActionType type, PEActionParam obj)
	{
		RPCServer (EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDABowReload (PEActionType type, PEActionParam obj)
	{
		PEActionParamN param = obj as PEActionParamN;
		RPCServer (EPacketType.PT_InGame_SKDAN, type, param.n);
	}

	private void SendSKDASleep (PEActionType type, PEActionParam obj)
	{
		PEActionParamVQNS param = obj as PEActionParamVQNS;
		RPCServer (EPacketType.PT_InGame_SKDAVQNS, type, param.vec, param.q, param.n, param.str);
	}

	private void SendSKDAEat (PEActionType type, PEActionParam obj)
	{
		PEActionParamS param = obj as PEActionParamS;
		RPCServer (EPacketType.PT_InGame_SKDAS, type, param.str);
	}

	private void SendSKDAToolHold (PEActionType type, PEActionParam obj)
	{
		RPCServer (EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDAToolPutOff (PEActionType type, PEActionParam obj)
	{
		RPCServer (EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDADig (PEActionType type, PEActionParam obj)
	{
		PEActionParamV param = obj as PEActionParamV;
		RPCServer (EPacketType.PT_InGame_SKDAV, type, param.vec);//,(Vector3)obj[0],(Vector3)obj[1],param.q[2]);
	}

	private void SendSKDAFell (PEActionType type, PEActionParam obj)
	{
		RPCServer (EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDAGather (PEActionType type, PEActionParam obj)
	{
		RPCServer (EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDAPickUpItem (PEActionType type, PEActionParam obj)
	{
		PEActionParamVQ param = obj as PEActionParamVQ;
		RPCServer (EPacketType.PT_InGame_SKDAVQ, type, param.vec, param.q);
	}

	private void SendSKDAGetOnVehicle (PEActionType type, PEActionParam obj)
	{
		PEActionParamS param = obj as PEActionParamS;
		RPCServer (EPacketType.PT_InGame_SKDAS, type, param.str);
	}

	private void SendSKDAJetPack (PEActionType type, PEActionParam obj)
	{
		RPCServer (EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDAParachute (PEActionType type, PEActionParam obj)
	{
		RPCServer (EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDAGlider (PEActionType type, PEActionParam obj)
	{
		RPCServer (EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDAClimb (PEActionType type, PEActionParam obj)
	{
		PEActionParamVQN param = obj as PEActionParamVQN;
		RPCServer (EPacketType.PT_InGame_SKDAVQN, type, param.vec, param.q, param.n);
	}

	private void SendSKDABuild (PEActionType type, PEActionParam obj)
	{
		RPCServer (EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDAHoldFlashLight (PEActionType type, PEActionParam obj)
	{
		RPCServer (EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDAStuned (PEActionType type, PEActionParam obj)
	{
		RPCServer (EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDASit (PEActionType type, PEActionParam obj)
	{
		PEActionParamVQSN param = obj as PEActionParamVQSN;
		RPCServer (EPacketType.PT_InGame_SKDAVQSN, type, param.vec, param.q, param.str, param.n);
	}

	private void SendSKDAOperation (PEActionType type, PEActionParam obj)
	{
		PEActionParamVQS param = obj as PEActionParamVQS;
		RPCServer (EPacketType.PT_InGame_SKDAVQS, type, param.vec, param.q, param.str);
	}

	private void SendSKDADraw (PEActionType type, PEActionParam obj)
	{
		RPCServer (EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDATwoHandSwordHold (PEActionType type, PEActionParam obj)
	{
		RPCServer (EPacketType.PT_InGame_SKDANO, type);
	}

	private void SendSKDATwoHandSwordPutOff (PEActionType type, PEActionParam obj)
	{
		RPCServer (EPacketType.PT_InGame_SKDANO, type);
	}
    
    private void SendSKDATwoHandSwordAttack (PEActionType type, PEActionParam obj)
	{
		PEActionParamVVNN param = obj as PEActionParamVVNN;
		RPCServer (EPacketType.PT_InGame_SKDAVVNN, type, param.vec1, param.vec2, param.n1, param.n2);
	}

	private void SendSKDAJump (PEActionType type, PEActionParam obj)
	{
		RPCServer (EPacketType.PT_InGame_SKDANO, type);
	}
	private void SendSKDACure (PEActionType type, PEActionParam obj)
	{
		PEActionParamVFVFS param = obj as PEActionParamVFVFS;
		RPCServer (EPacketType.PT_InGame_SKDAVFVFS, type, param.vec1, param.f1, param.vec2, param.f2, param.str);
	}

	private void SendSKDATalk (PEActionType type, PEActionParam obj)
	{
		PEActionParamS param = obj as PEActionParamS;
		RPCServer (EPacketType.PT_InGame_SKDAS, type, param.str);
	}

	private void SendSKDAAlienDeath (PEActionType type, PEActionParam obj)
	{
		RPCServer (EPacketType.PT_InGame_SKDANO, type);
	}

    private void SendSKDARide(PEActionType type, PEActionParam obj)
    {
        PEActionParamVQSNS paramVQSNS = obj as PEActionParamVQSNS;
        RPCServer(EPacketType.PT_InGame_SKDAVQSNS, type, paramVQSNS.vec, paramVQSNS.q, paramVQSNS.strAnima, paramVQSNS.enitytID, paramVQSNS.boneStr);
    }

    #endregion send do action

    #region skill

    protected void RPC_SKSyncAttr (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int indexData = (int)stream.Read<byte> ();
		float valueData = stream.Read<float> ();
		bool bRaw = stream.Read<bool> ();
        int caster = stream.Read<int>();

		if (runner != null && runner.SkEntityBase != null)
		{
			float oldData = runner.SkEntityBase.GetAttribute(indexData, !bRaw);
			runner.SkEntityBase._attribs.FromNet = true;
			runner.SkEntityBase.SetAttribute(indexData, valueData, false, bRaw, caster);
			runner.SkEntityBase._attribs.FromNet = false;
			if (!bRaw)
			{
				OnAttrChange((AttribType)indexData, runner.SkEntityBase.GetAttribute(indexData), caster, valueData - oldData);
			}
		}
       
//		if(indexData == 25)
//			Debug.LogError("Sync attk value = " + valueData);
//		if(indexData == 1)
//			Debug.LogError("Sync hp value = " + valueData);
    }

	protected void RPC_SKStartSkill (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (hasOwnerAuth)
			return;
		int id = stream.Read<int> ();
		int targetid = stream.Read<int> ();
		bool sendPara = stream.Read<bool> ();
		ISkParaNet netPara = null;
		if (sendPara) {
			float[] para = stream.Read<float[]> ();
			netPara = SkillSystem.SkParaNet.FromFloatArray (para);
		}
		if(runner == null)
			return;
		else if (runner.SkEntityPE != null) {
			Pathea.MotionMgrCmpt motionMgr = runner.SkEntityPE.Entity.GetCmpt<Pathea.MotionMgrCmpt> ();
			if (null != motionMgr)
			{
				Action_HandChangeEquipHold actionHand = motionMgr.GetAction<Action_HandChangeEquipHold>();
				if(null != actionHand)
					actionHand.ChangeHoldState (true);
			}

			CommonInterface com = CommonInterface.GetComByNetID (targetid);
			if (com != null)
				runner.SkEntityBase.StartSkill (com.SkEntityBase, id, netPara);
			else
				runner.SkEntityBase.StartSkill (null, id, netPara);
		} else if (runner.SkEntityBase != null) {
			CommonInterface com = CommonInterface.GetComByNetID (targetid);
			if (com != null)
				runner.SkEntityBase.StartSkill (com.SkEntityBase, id, netPara);
			else
				runner.SkEntityBase.StartSkill (null, id, netPara);
		}
	}

	protected void RPC_SKStopSkill (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int> ();
		if (runner != null && runner.SkEntityBase != null) {
			runner.SkEntityBase.CancelSkillById (id);
		}
	}

	protected void RPC_SKBLoop (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int skId = stream.Read<int> ();
		int targetId = stream.Read<int> ();
		bool bLoop = stream.Read<bool> ();
		if (runner != null && runner.SkEntityPE != null) {
			if (runner.SkEntityBase.GetSkInst (skId) != null) {
				runner.SkEntityBase.SetBLoop (bLoop, skId);
			} else {
				CommonInterface com = CommonInterface.GetComByNetID (targetId);
				if (com != null)
					runner.SkEntityBase.StartSkill (com.SkEntityBase, runner.SkEntityBase.GetId ());
				else
					runner.SkEntityBase.StartSkill (null, runner.SkEntityBase.GetId ());
			}
		}
	}

	protected void RPC_SKSyncInitAttrs (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] data = stream.Read<byte[]> ();
		if (runner != null && runner.SkEntityBase != null) {
			runner.SkEntityBase._attribs.FromNet = true;
			runner.SkEntityBase.Import (data);
			runner.SkEntityBase._attribs.FromNet = false;
		} else {
			Debug.LogError ("RPC_SKSyncInitAttrs runner or SkEntityPE is null");
		}

		if (Runner != null && Runner.SkEntityBase != null)
			Runner.SkEntityBase._attribs.LockModifyBySingle = false;

		if (null != OnSkAttrInitEvent)
			OnSkAttrInitEvent ();
	}

	protected void RPC_SKFellTree (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		/*int typeIndex = */stream.Read<int> ();
		Vector3 pos = stream.Read<Vector3> ();
		/*float maxHP = */stream.Read<float> ();
		float hp = stream.Read<float> ();

		TreeInfo tree = null;// = RSubTerrainMgr.TreesAtPosF(pos);
		if (null != LSubTerrainMgr.Instance) {
			int x = Mathf.FloorToInt (pos.x);
			int y = Mathf.FloorToInt (pos.y);
			int z = Mathf.FloorToInt (pos.z);
			List<GlobalTreeInfo> tree_list = LSubTerrainMgr.Picking (new IntVector3 (x, y, z), true);
			if (tree_list.Count > 0) {
				foreach (var iter in tree_list) {
					if (hp <= 0) {
                        PeEntity entity = EntityMgr.Instance.Get(Id);
                        SkEntity skEntity = null == entity ? null : entity.skEntity;
                        SkEntitySubTerrain.Instance.OnTreeCutDown(skEntity, iter);
                        DigTerrainManager.RemoveTree (iter);
						if (IsOwner)
							SkEntitySubTerrain.Instance.SetTreeHp (iter.WorldPos, hp);
					} else {
						if (IsOwner)
							SkEntitySubTerrain.Instance.SetTreeHp (iter.WorldPos, hp);
					}
				}
			}
		} else if (null != RSubTerrainMgr.Instance) {
			RSubTerrSL.AddDeletedTree (pos);
			tree = RSubTerrainMgr.TreesAtPosF (pos);
			if (null != tree) { 
				GlobalTreeInfo gTree = new GlobalTreeInfo (-1, tree);
				if (hp <= 0) {
                    PeEntity entity = EntityMgr.Instance.Get(Id);
                    SkEntity skEntity = null == entity ? null : entity.skEntity;
                    SkEntitySubTerrain.Instance.OnTreeCutDown(skEntity, gTree);
                    DigTerrainManager.RemoveTree (gTree);
					if (IsOwner)
						SkEntitySubTerrain.Instance.SetTreeHp (gTree.WorldPos, hp);
				} else {
					if (IsOwner)
						SkEntitySubTerrain.Instance.SetTreeHp (gTree.WorldPos, hp);
				}
			}
		}
	}
	#endregion skill

	#region do action

	private void RPC_SKDAVVF (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType proType = stream.Read<PEActionType> ();
		PEActionParamVVF param = PEActionParamVVF.param;
		param.vec1 = stream.Read<Vector3> ();
		param.vec2 = stream.Read<Vector3> ();
		param.f = stream.Read<float> ();
		if (MtCmpt != null)
			MtCmpt.DoActionImmediately ((PEActionType)proType, param);
	}

	private void RPC_SKDAVFNS (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType proType = stream.Read<PEActionType> ();
		PEActionParamVFNS param = PEActionParamVFNS.param;
		param.vec = stream.Read<Vector3> ();
		param.f = stream.Read<float> ();
		param.n = stream.Read<int> ();
		param.str = stream.Read<string> ();
		if (MtCmpt != null)
			MtCmpt.DoActionImmediately ((PEActionType)proType, param);
	}

	private void RPC_SKDANO (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType proType = stream.Read<PEActionType> ();
		if (MtCmpt != null)
			MtCmpt.DoActionImmediately ((PEActionType)proType);
	}

	private void RPC_SKDAV (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType proType = stream.Read<PEActionType> ();		
		PEActionParamV param = PEActionParamV.param;
		param.vec = stream.Read<Vector3> ();
		if (MtCmpt != null)
			MtCmpt.DoActionImmediately ((PEActionType)proType, param);
	}

	private void RPC_SKDAVVN (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType proType = stream.Read<PEActionType> ();
		PEActionParamVVN param = PEActionParamVVN.param;
		param.vec1 = stream.Read<Vector3> ();
		param.vec2 = stream.Read<Vector3> ();
		param.n = stream.Read<int> ();
		if (MtCmpt != null)
			MtCmpt.DoActionImmediately ((PEActionType)proType, param);
	}

	private void RPC_SKDAVQNS (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType proType = stream.Read<PEActionType> ();
		PEActionParamVQNS param = PEActionParamVQNS.param;
		param.vec = stream.Read<Vector3> ();
		param.q = stream.Read<Quaternion> ();
		param.n = stream.Read<int> ();
		param.str = stream.Read<string> ();
		if (MtCmpt != null)
			MtCmpt.DoActionImmediately ((PEActionType)proType, param);
	}

	private void RPC_SKDAVQ (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType proType = stream.Read<PEActionType> ();
		PEActionParamVQ param = PEActionParamVQ.param;
		param.vec = stream.Read<Vector3> ();
		param.q = stream.Read<Quaternion> ();
		if (MtCmpt != null)
			MtCmpt.DoActionImmediately ((PEActionType)proType, param);
	}

	private void RPC_SKDAS (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType proType = stream.Read<PEActionType> ();
		PEActionParamS param = PEActionParamS.param;
		param.str = stream.Read<string> ();
		if (MtCmpt != null)
			MtCmpt.DoActionImmediately ((PEActionType)proType, param);
	}


	private void RPC_SKDAVQS (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType proType = stream.Read<PEActionType> ();		
		PEActionParamVQS param = PEActionParamVQS.param;
		param.vec = stream.Read<Vector3> ();
		param.q = stream.Read<Quaternion> ();
		param.str = stream.Read<string> ();

		if (MtCmpt != null)
			MtCmpt.DoActionImmediately ((PEActionType)proType, param);
	}

	private void RPC_SKDAVQSN (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType proType = stream.Read<PEActionType> ();
		PEActionParamVQSN param = PEActionParamVQSN.param;
		param.vec = stream.Read<Vector3> ();
		param.q = stream.Read<Quaternion> ();
		param.str = stream.Read<string> ();
		param.n = stream.Read<int>();
		if (MtCmpt != null)
			MtCmpt.DoActionImmediately ((PEActionType)proType, param);
	}

	private void RPC_SKDAVVNN (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType proType = stream.Read<PEActionType> ();
		PEActionParamVVNN param = PEActionParamVVNN.param;
		param.vec1 = stream.Read<Vector3> ();
		param.vec2 = stream.Read<Vector3> ();
		param.n1 = stream.Read<int> ();
		param.n2 = stream.Read<int> ();

		if (MtCmpt != null)
			MtCmpt.DoActionImmediately ((PEActionType)proType, param);
	}

	private void RPC_SKDAN (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType proType = stream.Read<PEActionType> ();
		PEActionParamN param = PEActionParamN.param;
		param.n = stream.Read<int> ();
		if (MtCmpt != null)
			MtCmpt.DoActionImmediately ((PEActionType)proType, param);
	}
	private void RPC_SKDAB (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType proType = stream.Read<PEActionType> ();
		PEActionParamB param = PEActionParamB.param;
		param.b = stream.Read<bool> ();
		
		if (MtCmpt != null)
			MtCmpt.DoActionImmediately ((PEActionType)proType, param);
	}

	private void RPC_SKDAVQN (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType proType = stream.Read<PEActionType> ();
		PEActionParamVQN param = PEActionParamVQN.param;
		param.vec = stream.Read<Vector3> ();
		param.q = stream.Read<Quaternion> ();
		param.n = stream.Read<int> ();

		if (MtCmpt != null)
			MtCmpt.DoActionImmediately ((PEActionType)proType, param);
	}

	private void RPC_SKDAVFVFS (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType proType = stream.Read<PEActionType> ();
		PEActionParamVFVFS param = PEActionParamVFVFS.param;
		param.vec1 = stream.Read<Vector3> ();
		param.f1 = stream.Read<float> ();
		param.vec2 = stream.Read<Vector3> ();
		param.f2 = stream.Read<float> ();
		param.str = stream.Read<string> ();		
		if (MtCmpt != null)
			MtCmpt.DoActionImmediately ((PEActionType)proType, param);
	}

	private void RPC_SKDAEndAction (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType proType = stream.Read<PEActionType> ();
		if (MtCmpt != null)
			MtCmpt.EndAction ((PEActionType)proType);
	}

	private void RPC_SKDAEndImmediately (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType proType = stream.Read<PEActionType> ();
		if (MtCmpt != null)
			MtCmpt.EndImmediately ((PEActionType)proType);
	}

	private void RPC_SKDAQueryEntityState (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int eEntityState = stream.Read<int> ();
		PEActionType proType = PEActionType.None;
		if (MtCmpt != null) {
            if ((eEntityState & (int)EEntityState.EEntityState_Death) != 0)
            {
                proType = PEActionType.Death;
            }
            else if ((eEntityState & (int)EEntityState.EEntityState_EquipmentHold) != 0)
            {
                proType = PEActionType.EquipmentHold;
            }
            else if ((eEntityState & (int)EEntityState.EEntityState_HoldShield) != 0)
            {
                proType = PEActionType.HoldShield;
            }
            else if ((eEntityState & (int)EEntityState.EEntityState_GunHold) != 0)
            {
                proType = PEActionType.GunHold;
            }
            else if ((eEntityState & (int)EEntityState.EEntityState_BowHold) != 0)
            {
                proType = PEActionType.BowHold;
            }
            else if ((eEntityState & (int)EEntityState.EEntityState_AimEquipHold) != 0)
            {
                proType = PEActionType.AimEquipHold;
            }
            else if ((eEntityState & (int)EEntityState.EEntityState_HoldFlashLight) != 0)
            {
                proType = PEActionType.HoldFlashLight;
            }
            else if ((eEntityState & (int)EEntityState.EEntityState_TwoHandSwordHold) != 0)
            {
                proType = PEActionType.TwoHandSwordHold;
            }
            else if ((eEntityState & (int)EEntityState.EEntityState_Sit) != 0)
            {
                proType = PEActionType.Sit;
				PEActionParamVQSN paramVQSN = PEActionParamVQSN.param;
				paramVQSN.vec = transform.position;
				paramVQSN.q = transform.rotation;
				paramVQSN.n = 0;
				paramVQSN.str = "SitOnChair";
				MtCmpt.DoActionImmediately (PEActionType.Sit, paramVQSN);
				return;
            }
            else if ((eEntityState & (int)EEntityState.EEntityState_SLeep) != 0)
            {
                proType = PEActionType.Sleep;
				PEActionParamVQNS paramVQNS = PEActionParamVQNS.param;
				paramVQNS.vec = transform.position;
				paramVQNS.q = transform.rotation;
				paramVQNS.n = 0;
				paramVQNS.str = "Sleep";
				MtCmpt.DoActionImmediately (PEActionType.Sleep, paramVQNS);
				return;
            }
            else if ((eEntityState & (int)EEntityState.EEntityState_Cure) != 0)
            {
                proType = PEActionType.Cure;
            }
            else if ((eEntityState & (int)EEntityState.EEntityState_Operation) != 0)
            {
                proType = PEActionType.Operation;
            }
            if (proType != PEActionType.None)
				MtCmpt.DoActionImmediately (proType);
		}
	}

    private void RPC_SKDAVQSNS(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        PEActionType proType = stream.Read<PEActionType>();
        PEActionParamVQSNS param = PEActionParamVQSNS.param;
        param.vec = stream.Read<Vector3>();
        param.q = stream.Read<Quaternion>();
        param.strAnima = stream.Read<string>();
        param.enitytID = stream.Read<int>();
        param.boneStr = stream.Read<string>();

        PeEntity player = EntityMgr.Instance.Get(Id);
        //2017.02.17 我可以看见这个玩家的时候再执行行为
        if (null != player && player.hasView)
        {
            if (MtCmpt != null)
                MtCmpt.DoActionImmediately((PEActionType)proType, param);
        }
    }

    #endregion do action

    #region IK

    private void RPC_SKIKPos (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 v1 = stream.Read<Vector3> ();

		if (IkCmpt != null)
			IkCmpt.aimTargetPos = v1;
	}

	#endregion IK

	protected void RPC_S2C_AbnormalConditionStart (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (hasOwnerAuth)
			return;

		int entityId = stream.Read<int>();
		int type = stream.Read<int>();
		int flag = stream.Read<int>();

		PeEntity entity = EntityMgr.Instance.Get(entityId);
		if (null == entity)
			return;

		AbnormalConditionCmpt acc = entity.GetCmpt<AbnormalConditionCmpt>();
		if (null == acc)
			return;

		if (1 == flag)
		{
			byte[] stateData = stream.Read<byte[]>();
			acc.NetApplyState((PEAbnormalType)type, stateData);
		}
		else
		{
			acc.NetApplyState((PEAbnormalType)type, null);
		}
	}
	
	protected void RPC_S2C_AbnormalConditionEnd (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (hasOwnerAuth)
			return;

		int entityId = stream.Read<int>();
		int type = stream.Read<int>();

		PeEntity entity = EntityMgr.Instance.Get(entityId);
		if (null == entity)
			return;

		AbnormalConditionCmpt acc = entity.GetCmpt<AbnormalConditionCmpt>();
		if (null == acc)
			return;

		acc.NetEndState((PEAbnormalType)type);
	}

	protected void RPC_S2C_AbnormalCondition(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] binData = stream.Read<byte[]>();

		PeEntity entity = EntityMgr.Instance.Get(Id);
		if (null == entity)
			return;

		AbnormalConditionCmpt acc = entity.GetCmpt<AbnormalConditionCmpt>();
		if (null == acc)
			return;

		PETools.Serialize.Import(binData, r =>
		{
			/*int cmptType = */BufferHelper.ReadInt32(r);
			int count = BufferHelper.ReadInt32(r);
			for (int i = 0; i < count; i++)
			{
				int type = BufferHelper.ReadInt32(r);
				byte[] stateData = BufferHelper.ReadBytes(r);
				acc.NetApplyState((PEAbnormalType)type, stateData);
			}
		});
	}

    protected void RPC_S2C_Jump(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        if (hasOwnerAuth)
            return;
        double jumpTime = stream.Read<double>();
        if (_move != null && _move is Motion_Move_Human)
            (_move as Motion_Move_Human).NetJump(jumpTime);
        
    }


    protected void RPC_S2C_SkOnDamage (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int casterId = stream.Read<int> ();
		float damage = stream.Read<float> ();

		NetworkInterface caster = Get<NetworkInterface> (casterId);
		SkEntity entity = null != caster ? null != caster.Runner ? caster.Runner.SkEntityBase : null : null;

		//if (null != Runner && null != Runner.SkEntityPE && IsController)
		//	Runner.SkEntityPE.Entity.SendMsg (EMsg.Battle_HPChange, entity, damage);

        if(Runner != null)
        {
            PESkEntity skEntity = Runner.SkEntityBase as PESkEntity;
            if(skEntity != null)
            {
                skEntity.DispatchHPChangeEvent(entity, damage);
            }
        }
	}

	protected virtual void RPC_S2C_LostController(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{

	}

	protected virtual void RPC_S2C_SetController(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{

	}

	protected virtual void CheckAuthority()
	{
		if (null == PlayerNetwork.mainPlayer)
			return;

		if (Mathf.Abs(PlayerNetwork.mainPlayer._pos.x - _pos.x) <= 128 &&
			Mathf.Abs(PlayerNetwork.mainPlayer._pos.y - _pos.y) <= 128 &&
			Mathf.Abs(PlayerNetwork.mainPlayer._pos.z - _pos.z) <= 128)
		{
			canGetAuth = true;
			
			if (!hasAuth && lastRequestTime < Time.time)
			{
				RPCServer(EPacketType.PT_InGame_SetController);
				lastRequestTime = Time.time + 3;
			}
			else
			{
				lastRequestTime = 0;
			}
		}
		else
		{
			canGetAuth = false;
			lastRequestTime = 0;

			if (hasOwnerAuth)
			{
				RPCServer(EPacketType.PT_InGame_LostController);
				authId = -1;
				ResetContorller();

				//if (LogFilter.logDebug) Debug.LogFormat("<color=blue>you lost [{0}]'s authority.</color>", Id);
			}
		}
	}

	float lastRequestTime;
	protected virtual IEnumerator AuthorityCheckCoroutine()
	{
		authId = -1;
		lastRequestTime = 0;

		while (true)
		{
			CheckAuthority();
			yield return null;
		}
	}

	protected virtual void ResetContorller()
	{
		PeEntity entity = EntityMgr.Instance.Get(Id);
		if (null != entity)
		{
			NetCmpt net = entity.GetCmpt<NetCmpt>();
			if (null != net)
				net.SetController(hasOwnerAuth);
		}

		if (hasOwnerAuth)
			enabled = false;
		else
			enabled = true;

		InitForceData();
	}
}