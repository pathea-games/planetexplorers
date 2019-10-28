//#define DbgMonsterGroup
using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using Pathea.PeEntityExt;
using SkillSystem;

public abstract class EntityGrpHandler : MonoBehaviour
{
	public abstract void OnGrpCreated(EntityGrp grp);
}
public class EntityGrp : PeEntity
{
	//static List<Vector2> _lstRndOfs = new List<Vector2> ();	//tmp used in gen entity group
	List<ISceneObjAgent> _lstAgents = new List<ISceneObjAgent>(); 
	[HideInInspector]
	public int _grpProtoId;
	public int _protoId;
	public int _cntMin;
	public int _cntMax;
    public int _atkMin;
	public int _atkMax;
	public float _radius;
	public float _sqrRejectRadius;
	[HideInInspector]
	public Action<PeEntity> handlerMonsterCreated = null;
	public List<ISceneObjAgent> memberAgents{ get { return _lstAgents; } }

	public static EntityGrp CreateMonsterGroup(int grpProtoId, Vector3 center,int colorType,int playerId, int entityId = -1,int buffId=0)
	{
		int noid = -1 == entityId ? Pathea.WorldInfoMgr.Instance.FetchNonRecordAutoId() : entityId;
#if DbgMonsterGroup
		MonsterGroupProtoDb.Item item = new MonsterGroupProtoDb.Item();
		item.protoID = 3;
		item.cntMinMax  = new int[]{3,5};
		item.radiusDesc = new float[]{15f, 1.5f};
		item.subProtoID = new int[0];
#else
		MonsterGroupProtoDb.Item item = MonsterGroupProtoDb.Get (grpProtoId);
		if (item == null)	return null;
#endif
		int cnt = 0;
		EntityGrp grp = EntityMgr.Instance.Create(noid, PeEntityCreator.GroupPrefabPath, center, Quaternion.identity, Vector3.one) as EntityGrp;
		if (grp == null)	return null;

		grp._grpProtoId = grpProtoId;
		// Random part
		grp._protoId = item.protoID;
		grp._cntMin = item.cntMinMax [0];
		grp._cntMax = item.cntMinMax [1];
        grp._atkMin = item.atkMinMax [0];
		grp._atkMax = item.atkMinMax [1];
		grp._radius = item.radiusDesc [0];
		grp._sqrRejectRadius = item.radiusDesc.Length > 1 ? (item.radiusDesc [1]*item.radiusDesc [1]) : 1;
		cnt = UnityEngine.Random.Range (grp._cntMin, grp._cntMax);
		if(grp._protoId > 0) {
			Vector3 pos = Vector3.zero;
			for (int i = 0; i < cnt; i++) {
				if(grp.GetRandPos(center, ref pos, 5)){
					SceneEntityPosAgent agent = MonsterEntityCreator.CreateAgent(pos, grp._protoId);
                    agent.ScenarioId = grp.scenarioId;
                    agent.spInfo = new MonsterEntityCreator.AgentInfo(grp,colorType,playerId,buffId);
					grp._lstAgents.Add(agent);
				}
			}
		}
		// Layout part
		cnt = item.subProtoID != null ? item.subProtoID.Length : 0;
		for (int i = 0; i < cnt; i++) {
			if(item.subProtoID[i] > 0){
				SceneEntityPosAgent agent = MonsterEntityCreator.CreateAgent(item.subPos[i] + center, item.subProtoID[i], item.subScl[i], Quaternion.Euler(item.subRot[i]));
                agent.ScenarioId = grp.scenarioId;
				agent.spInfo = new MonsterEntityCreator.AgentInfo(grp,colorType,playerId,buffId);
				grp._lstAgents.Add(agent);
			}
		}
		grp.StartCoroutine (grp.AddMemberAgents ()); //postpond member creation to avoid performance spike
		//SceneMan.AddSceneObjs (grp._lstAgents);
		return grp;
	}
	bool GetRandPos(Vector3 center, ref Vector3 pos, int nMaxTry)
	{
		int nTry = 0;
		while (true) {
			Vector2 ofs = UnityEngine.Random.insideUnitCircle;
			pos = center;
			pos.x += ofs.x * _radius;
			pos.z += ofs.y * _radius;
			pos.y = SceneEntityPosAgent.PosYTBD;

			int nCur = 0;
			for (; nCur < _lstAgents.Count; nCur++) {
				Vector3 dist = _lstAgents [nCur].Pos - pos;
				if (dist.x * dist.x + dist.z * dist.z < _sqrRejectRadius) {
					break;
				}
			}
			if(nCur >= _lstAgents.Count){
				return true;
			} else {				
				nTry++;
				if (nTry >= nMaxTry) {
					return false;
				}
			}
		}
	}

	IEnumerator AddMemberAgents()
	{
		yield return new WaitForSeconds (0.1f);
		int n = _lstAgents.Count;
		for (int i = 0; i < n; i++) {
			SceneMan.AddSceneObj (_lstAgents[i]);
			yield return new WaitForSeconds (0.1f);
		}
	}
	void OnDestroy()
	{
		StopAllCoroutines ();
		SceneMan.RemoveSceneObjs (_lstAgents);
	}

	public void RemoveAllAgent(){
		SceneMan.RemoveSceneObjs (_lstAgents);
	}
	public void OnMemberCreated(PeEntity e)
	{
        if (e != null)
        {
            e.transform.parent = transform;

            if (!PeGameMgr.IsMulti)
            {
                Pathea.LodCmpt entityLodCmpt = e.lodCmpt;
                if (entityLodCmpt != null)
                {
					entityLodCmpt.onDestroyEntity += OnMemberDestroy;
                }
            }

            if (handlerMonsterCreated != null)
                handlerMonsterCreated(e);
        }
	}
	void OnMemberDestroy(PeEntity e)
	{
		// Check to destroy group
		foreach (ISceneObjAgent agent in _lstAgents)
		{
			SceneEntityPosAgent sepa = agent as SceneEntityPosAgent;
			if (sepa != null)
			{
				if(sepa.entity != null && sepa.entity != e)
				{
					return;
				}
			}
			else
			{

//				PeCustom.AIAgent aiagent = agent as PeCustom.AIAgent;
//
//				if(aiagent.entity != null && aiagent.entity != e)
//				{
//					return;
//				}
				PeCustom.SceneEntityAgent sea = agent as PeCustom.SceneEntityAgent;
				if (sea.entity != null && sea.entity != e)
				{
					return;
				}
			}
		}
		Destroy (gameObject);
	}
}
