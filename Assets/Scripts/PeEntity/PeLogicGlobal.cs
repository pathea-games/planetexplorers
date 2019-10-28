using UnityEngine;
using System.Collections;
using Pathea;
using SkillSystem;
using Pathea.PeEntityExt;

public class PeLogicGlobal : Singleton<PeLogicGlobal>
{
    IEnumerator DestroyEnumerator(SkEntity entity, float delayTime, float fadeoutTime = 3.0f)
    {
        yield return new WaitForSeconds(delayTime);

        if (entity != null)
        {
            ViewCmpt view = entity.GetComponent<ViewCmpt>();

            if(view != null)
            {
				if (view is BiologyViewCmpt)
				{
					(view as BiologyViewCmpt).Fadeout(fadeoutTime);
					yield return new WaitForSeconds(fadeoutTime);
				}
            }

			if (entity != null)
			{
				GameObject.Destroy(entity.gameObject);
                PeEventGlobal.Instance.DestroyEvent.Invoke(entity);
			}
		}
    }

    IEnumerator NpcRevive(SkEntity entity, float delayTime)
    {
		if (Pathea.PeGameMgr.IsMulti)
			yield break;

		PeEntity peentity = entity.GetComponent<PeEntity>();
		if(peentity == null)
			yield break;

		EntityInfoCmpt InfoCmpt = peentity.enityInfoCmpt;
		InfoCmpt.SetDelaytime(Time.time , delayTime);

		PESkEntity skentity = peentity.peSkEntity;
        yield return new WaitForSeconds(delayTime);
		
		if (entity != null && skentity != null && skentity.isDead)
        {
            MotionMgrCmpt motion = entity.GetComponent<MotionMgrCmpt>();
            if (motion != null)
            {
				while(true)
				{
					if(null == peentity || null == motion)
						yield break;
					//在复活中被任务设置成不能复活 ReviveTime = -1
					if(peentity.NpcCmpt.ReviveTime < 0)
						 break;

					PEActionParamB param = PEActionParamB.param;
					param.b = false;
					if(motion.DoAction(PEActionType.Revive, param))
					{
						entity.SetAttribute((int)AttribType.Hp, entity.GetAttribute((int)AttribType.HpMax) * 0.8f);
						break;
					}
					else
						yield return new WaitForSeconds(1);
				}
            }
        }
    }

	IEnumerator ServantRevive(PeEntity entity, float delayTime)
	{
		yield return new WaitForSeconds(delayTime);
		if (!PeGameMgr.IsMulti)
		{
			if(entity != null && entity.UseItem != null)
			{
				if(entity.UseItem.ReviveServent(false))
				{
					if (entity.motionMgr != null)
					{
						while(true)
						{
							if(null == entity || null == entity.motionMgr)
								yield break;
							PEActionParamB param = PEActionParamB.param;
							param.b = false;
							if(null == entity || entity.motionMgr.DoAction(PEActionType.Revive, param))
							{
								break;
							}
							else
								yield return new WaitForSeconds(1);
						}
					}
				}
			}
		}
		else
		{
            if (null != entity && null != entity.netCmpt)
            {
                AiAdNpcNetwork npc = (AiAdNpcNetwork)entity.netCmpt.network;
                if (null != npc && npc.LordPlayerId == PlayerNetwork.mainPlayerId)
                {
                    Vector3 pos = entity.position;
                    if (null != entity.NpcCmpt && !entity.NpcCmpt.CanRecive)
                        pos = PlayerNetwork.mainPlayer.PlayerPos + Vector3.one;

                    PlayerNetwork.RequestServantAutoRevive(entity.Id, pos);
                }
            }
		}
	}
		
    void OnEntityDeath(SkEntity entity, SkEntity caster)
    {
        CommonCmpt common = entity.GetComponent<CommonCmpt>();
        if (common != null)
        {
			if (common.entityProto.proto == EEntityProto.Doodad)
			{
				DestroyEntity(entity, 30.0f);
			}

            if (common.entityProto.proto == EEntityProto.Monster)
            {
                MonsterHandbookData.AddMhByKilledMonsterID(common.entityProto.protoId);
                if (common.GetComponent<TowerCmpt>() == null)
                {
                    float reviveTime = 10;
                    PeEntity mon = entity.GetComponent<PeEntity>();
                    if (mon != null)
                    {
						if (StroyManager.Instance != null)
						{
                            if (StroyManager.Instance.m_RecordKillMons.Count != 0)
                            {
                                foreach (var item in StroyManager.Instance.m_RecordKillMons.Values)
                                {
                                    if (item.type == KillMons.Type.fixedId && SceneEntityCreatorArchiver.Instance.GetEntityByFixedSpId(item.monId) == mon)
                                    {
                                        reviveTime = (item.reviveTime == 0 ? reviveTime : item.reviveTime);
                                        break;
                                    }
                                    else if (item.type == KillMons.Type.protoTypeId && Vector3.Distance(mon.position, item.center) <= item.radius
                                        && (item.monId == -999 ? true : common.entityProto.protoId == item.monId))
                                    {
                                        reviveTime = (item.reviveTime == 0 ? reviveTime : item.reviveTime);
                                        break;
                                    }
                                }
                            }
						}
                    }
                    DestroyEntity(entity, reviveTime);
                }
            }

            NpcCmpt npc = entity.GetComponent<NpcCmpt>();
            if (common.entityProto.proto == EEntityProto.Npc)
            {
                if (GameUI.Instance != null)
                {
                    if (GameUI.Instance.mNpcWnd.IsOpen() && GameUI.Instance.mNpcWnd.m_CurSelNpc.commonCmpt == common)
                        GameUI.Instance.mNpcWnd.Hide();
                }
                if (npc != null && npc.Type != ENpcType.Follower && npc.ReviveTime > 0)
                    ReviveEntity(entity, 10.0f);
                if (npc.ReviveTime <= 0)
                {
                    PeEntity npcentity = npc.GetComponent<PeEntity>();
                    if (MissionManager.Instance != null && MissionManager.Instance.m_PlayerMission != null)
                        MissionManager.Instance.m_PlayerMission.SetMissionState(npcentity, NpcMissionState.Max); 

                    NpcMissionData missionData = npcentity.GetUserData() as NpcMissionData;
                    if (missionData != null)
                        missionData.m_MissionList.Clear();
                }
            }
            else if (common.entityProto.proto == EEntityProto.RandomNpc)
            {
                if (npc != null && !npc.IsServant)
                {
                    if (npc.ReviveTime > 0)
                    {
                        if (PeGameMgr.IsMultiStory)
                        {
                            if (entity._net is AiAdNpcNetwork)
                            {
                                int tempid = ((AiAdNpcNetwork)(entity._net)).ExternId;
                                RandomNpcDb.Item item = RandomNpcDb.Get(tempid);
                                if (item != null)
                                {
                                    if (item.reviveTime != -1)
                                    {
                                        ReviveEntity(entity, item.reviveTime);
                                    }
                                }
                            }
                        }
                        else
                            ReviveEntity(entity, npc.ReviveTime);
                    }
                }

				//follower revive
//				if (npc != null && npc.IsServant)
//				{
//					ReviveEntity(entity, npc.ReviveTime + npc.FollowerReviceTime);
//				}

            }
        }
    }

    void OnEntityPickup(SkEntity entity)
    {
        CommonCmpt common = entity.GetComponent<CommonCmpt>();
        if (common != null)
        {            
            if (common.entityProto.proto == EEntityProto.RandomNpc || common.entityProto.proto == EEntityProto.Npc)
            {
                if (!ServantLeaderCmpt.Instance.ContainsServant(common.GetComponent<NpcCmpt>()))
                    DestroyEntity(entity);
            }
			else //if (common.entityProto.proto == EEntityProto.Monster)
			{
				DestroyEntity(entity);

				PeEntity Pentity = entity.GetComponent<PeEntity>();
				if(Pentity != null)
					LootItemDropPeEntity.RemovePeEntity(Pentity);

			}
        }
    }

    void OnEntityRevive(SkEntity entity)
    {
        CommonCmpt common = entity.GetComponent<CommonCmpt>();
        if (common != null)
        {
            if (common.entityProto.proto == EEntityProto.Npc || common.entityProto.proto == EEntityProto.RandomNpc)
            {
                ReviveEntity(entity, 0.0f);
            }
        }
    }

    void OnEntityDestroy(SkEntity entity)
    {
        PeEntity peEntity = entity.GetComponent<PeEntity>();
        if (peEntity != null)
        {
			LootItemDropPeEntity.RemovePeEntity(peEntity);
            PeCreature.Instance.Destory(peEntity.Id);
        }
    }

//    void OnMainPlayerAttack(PeEntity entity, AttackMode mode)
//    {
//        if(mode.type != AttackType.Melee)
//            return;
//
//        Collider[] colliders = Physics.OverlapSphere(entity.position, mode.maxRange + 2.0f, 1 << Pathea.Layer.AIPlayer);
//        foreach (Collider item in colliders)
//        {
//            PeEntity e = item.transform.GetComponentInParent<PeEntity>();
//            if(e != null)
//            {
//                PETargetHelper.Instance.RegisterContinues(entity, e, 5.0f);
//            }
//        }
//    }

    public void Init()
    {
        PeEventGlobal.Instance.DeathEvent.AddListener(OnEntityDeath);
        PeEventGlobal.Instance.PickupEvent.AddListener(OnEntityPickup);
        PeEventGlobal.Instance.ReviveEvent.AddListener(OnEntityRevive);
        PeEventGlobal.Instance.DestroyEvent.AddListener(OnEntityDestroy);
//        PeEventGlobal.Instance.MainPlayerAttack.AddListener(OnMainPlayerAttack);
    }

    public void DestroyEntity(SkEntity entity, float delayTime = 0.0f, float fadeoutTime = 3.0f)
    {
        //Debug.Log("Destroy entity : " + entity.name);
        if(delayTime >= 0)
            StartCoroutine(DestroyEnumerator(entity, delayTime, fadeoutTime));
    }

    public void ReviveEntity(SkEntity entity, float delayTime = 0.0f)
    {
        //Debug.Log("Revive entity : " + entity.name);
        StartCoroutine(NpcRevive(entity, delayTime));

    }

	public void ServantReviveAtuo(PeEntity entity,float delayTime = 0.0f)
	{
		StartCoroutine(ServantRevive(entity, delayTime));
	}

}
