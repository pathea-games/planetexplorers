//#define DbgNearGen
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtTrans;
using SkillSystem;

public class SceneEntityMissionPointMonsterKill : ISceneEntityMissionPoint
{
	static PlayerMission playerMission{ get { return MissionManager.Instance.m_PlayerMission; } }

	int _idxTarId = -1;
	TypeMonsterData _data = null;
	List<ISceneObjAgent> _agents = new List<ISceneObjAgent>();
	public int MissionId{ get; set; }
	public int TargetId{ get; set; }
	public bool GenMonsterInMission{ get; set; }
	public bool Start()
	{
		MissionCommonData data = MissionManager.GetMissionCommonData(MissionId);
		for (int i = 0; i < data.m_TargetIDList.Count; i++)
		{
			if (data.m_TargetIDList[i] == TargetId)
			{
				_idxTarId = i;
				break;
			}
		}		
		if(_idxTarId == -1)
			return false;
		_data = MissionManager.GetTypeMonsterData(TargetId);
		if (_data == null)
			return false;
        Vector3 referToPos;
        switch (_data.m_mr.refertoType)
        {
            case ReferToType.None:
                referToPos = PeCreature.Instance.mainPlayer.position;
                break;
            case ReferToType.Player:
                referToPos = PeCreature.Instance.mainPlayer.position;
                break;
            case ReferToType.Town:
                VArtifactUtil.GetTownPos(_data.m_mr.referToID, out referToPos);
                break;
            case ReferToType.Npc:
                referToPos = EntityMgr.Instance.Get(MissionManager.Instance.m_PlayerMission.adId_entityId[_data.m_mr.referToID]).position;
                break;
            default:
                referToPos = PeCreature.Instance.mainPlayer.position;
                break;
        }
        if (referToPos == Vector3.zero)
            return false;
        if(PeGameMgr.IsSingle || PeGameMgr.IsTutorial)
        {
            if (_data.type == 2)
                DoodadEntityCreator.commonDeathEvent += OnMonsterDeath;
            else
                MonsterEntityCreator.commonDeathEvent += OnMonsterDeath;
        }
        
		GenMonsterInMission = !PeGameMgr.IsStory;
		if (GenMonsterInMission) {
#if  DbgNearGen
			Vector2 v2 = Vector2.zero;
#else
			Vector2 v2 = UnityEngine.Random.insideUnitCircle.normalized * _data.m_mr.radius1;
#endif
			Vector3 center = referToPos + new Vector3 (v2.x, 0.0f, v2.y);
			//for (int i = 0; i < _data.m_MonsterList.Count; i++) {
			//	int num = _data.m_MonsterList[i].type;
   //             int protoId = _data.m_MonsterList[i].npcs[UnityEngine.Random.Range(0, _data.m_MonsterList[i].npcs.Count)];
			//	for (int j = 0; j < num; j++) {
			//		Vector3 pos = AiUtil.GetRandomPosition (center, 0, _data.m_mr.radius2);
			//		pos.y = SceneEntityPosAgent.PosYTBD;	// let posagent to set y
			//		SceneEntityPosAgent agent = MonsterEntityCreator.CreateAgent(pos, protoId);
			//		agent.spInfo = new MonsterEntityCreator.AgentInfo(EntityMonsterBeacon.GetSpBeacon4MonsterKillTask());
			//		_agents.Add (agent);
			//		SceneMan.AddSceneObj (agent);
			//	}
			//}

            for (int i = 0; i < _data.m_CreMonList.Count; i++)
            {
                for (int j = 0; j < _data.m_CreMonList[i].monNum; j++)
                {
                    Vector3 pos = AiUtil.GetRandomPosition(center, 0, _data.m_mr.radius2);
                    pos.y = SceneEntityPosAgent.PosYTBD;    // let posagent to set y
                    int protoId = _data.m_CreMonList[i].monID;
                    if (_data.m_CreMonList[i].type == 1)
                        protoId |= EntityProto.IdGrpMask;
                    SceneEntityPosAgent agent = MonsterEntityCreator.CreateAgent(pos, protoId);
                    agent.spInfo = new MonsterEntityCreator.AgentInfo(EntityMonsterBeacon.GetSpBeacon4MonsterKillTask());
                    agent.canRide = false;
                    _agents.Add(agent);
                    SceneMan.AddSceneObj(agent);
                }
            }
		}
		return true;
	}
	public void Stop()
	{
		SceneMan.RemoveSceneObjs(_agents);
		_agents.Clear();
		MonsterEntityCreator.commonDeathEvent -= OnMonsterDeath;
        DoodadEntityCreator.commonDeathEvent -= OnMonsterDeath;
	}
	void OnMonsterDeath(SkEntity cur, SkEntity caster)
	{
        if (_data.m_mustByPlayer == true && caster is Pathea.Projectile.SkProjectile)
        {
            GameObject go = ((Pathea.Projectile.SkProjectile)caster).Caster();
            if (go != null)
            {
                CommonCmpt cc = go.GetComponent<CommonCmpt>();
                if (cc != null && cc.entityProto.proto != EEntityProto.Player)
                    return;
            }
        }
		SkAliveEntity skAlive = cur as SkAliveEntity;
		if(skAlive != null && _data != null)
		{
			CommonCmpt cc = skAlive.Entity.commonCmpt;
			if (cc == null)						return;
			
			int protoId = cc.entityProto.protoId;
			bool bFin = true;
			for (int m = 0; m < _data.m_MonsterList.Count; m++)
			{
				int idx = _idxTarId * 10 + m;
				string tmp = PlayerMission.MissionFlagMonster + idx;
				string value = playerMission.GetQuestVariable(MissionId, tmp);
				string[] tmplist = value.Split('_');
				if(tmplist.Length < 2)
				{
					Debug.LogError("[TaskMonsterKill]:Wrong Quest Var:"+value);
				}
				int num = tmplist.Length >= 2 ? Convert.ToInt32(tmplist[1]) : 0;
				if(_data.m_MonsterList[m].npcs.Contains(protoId))
				{
					num += 1;
					value = tmplist[0] + "_" + num.ToString();
					playerMission.ModifyQuestVariable(MissionId, tmp, value);
				}
				if(num < _data.m_MonsterList[m].type)
				{
					bFin = false;
				}
			}

			if(bFin)
			{
                MissionManager.Instance.CompleteTarget(TargetId, MissionId, true);
			}
		}
	}
}
