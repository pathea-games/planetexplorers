using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Mono.Data.SqliteClient;
using Pathea;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtTrans;
using SkillSystem;

public class SceneDoodadDesc
{
	public const int c_neutralCamp = 28;
	public const int c_neutralDamage = 28;
	// for story 0: can be attacked; 25: can only be attacked by player
	public const int c_playerAttackableCamp = 0;
	public const int c_playerAttackableDamage = 25;
	public int _id;
	public int _type;
	public int _protoId;
	public bool _isShown;
	public int _campId;
	public int _damageId;
	public Vector3 _pos;
	public Vector3 _scl;
	public Quaternion _rot;
	public static void GetCampDamageId(bool damagable, out int campId, out int damageId)
	{
		if (damagable) {
			campId = c_playerAttackableCamp;
			damageId = c_playerAttackableDamage;
		} else {
			campId = c_neutralCamp;
			damageId = c_neutralDamage;
		}
	}
}
public class StoryDoodadMap
{
	public static Dictionary<int, SceneDoodadDesc> s_dicDoodadData = new Dictionary<int, SceneDoodadDesc>(320);
	
	public static void LoadData()
	{
		SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("sceneAssetList");
		int idxId = reader.GetOrdinal ("ID");
		int idxType = reader.GetOrdinal ("type");
		int idxShown = reader.GetOrdinal ("IsProduce");
		int idxDamagable = reader.GetOrdinal ("IsDamage");
		int idxProtoId = reader.GetOrdinal ("PrototypeDoodad_Id");
		int idxPos = reader.GetOrdinal ("PosXYZ");
		int idxRot = reader.GetOrdinal ("RotXYZW");
		int idxScl = reader.GetOrdinal ("ScaleXYZ");
		while (reader.Read())
		{
			SceneDoodadDesc desc = new SceneDoodadDesc();
			desc._id =reader.GetInt32(idxId);
			desc._type = reader.GetInt32(idxType);	// save or not
			desc._protoId = reader.GetInt32(idxProtoId);
			desc._isShown = 0 != reader.GetInt32(idxShown);
			SceneDoodadDesc.GetCampDamageId(0 != reader.GetInt32(idxDamagable), out desc._campId, out desc._damageId);
			string[] strPos = reader.GetString(idxPos).Split(',');
			string[] strRot = reader.GetString(idxRot).Split(',');
			string[] strScl = reader.GetString(idxScl).Split(',');
			desc._pos = new Vector3(
				Convert.ToSingle(strPos[0]),
				Convert.ToSingle(strPos[1]),
				Convert.ToSingle(strPos[2]));
			desc._rot = new Quaternion(
				Convert.ToSingle(strRot[0]),
				Convert.ToSingle(strRot[1]),
				Convert.ToSingle(strRot[2]),
				Convert.ToSingle(strRot[3]));
			if(desc._rot.w > 2) //Quaternion should be normalized
			{
				desc._rot.eulerAngles = new Vector3(desc._rot.x, desc._rot.y, desc._rot.z);
			}
			desc._scl = new Vector3(
				Convert.ToSingle(strScl[0]),
				Convert.ToSingle(strScl[1]),
				Convert.ToSingle(strScl[2]));
			s_dicDoodadData.Add(desc._id, desc);
		}
	}
	public static SceneDoodadDesc Get(int id)
	{
		if(s_dicDoodadData.ContainsKey(id))
		{
			return s_dicDoodadData[id];
		}
		return null;
	}
}

public class DoodadEntityCreator
{
	public class AgentInfo : SceneEntityPosAgent.AgentInfo
	{
		public static new AgentInfo s_defAgentInfo = new AgentInfo ();
		//in story mode, _id is index in story asset db(SceneAssetList)
		//in adventure mode, _id is town id now temporily
		public int _id;
		public bool _isShown;
		public int _campId;
		public int _damageId;
		public int _playerId;
		public AgentInfo(int id = -1, bool isShown = true, int campId = SceneDoodadDesc.c_neutralCamp, int damageId = SceneDoodadDesc.c_neutralDamage,int playerId =-1)
		{
			_id = id;
			_isShown = isShown;
			_campId = campId;
			_damageId = damageId;
			_playerId = playerId;
		}
	}
	public static event Action<SkEntity, SkEntity> commonDeathEvent;
	public static void Init()
	{
		commonDeathEvent = null;
		commonDeathEvent += RepProcessor.OnDoodadDeath;
	}
		
	public static SceneEntityPosAgent CreateAgent(Vector3 pos, int protoId, Vector3 scl, Quaternion rot)
	{
		SceneEntityPosAgent agent = new SceneEntityPosAgent(pos, scl, rot, EntityType.EntityType_Doodad, protoId);
		agent.spInfo = AgentInfo.s_defAgentInfo;
		return agent;
	}
	public static SceneEntityPosAgent CreateAgent(Vector3 pos, int protoId = -1)
	{
		SceneEntityPosAgent agent = new SceneEntityPosAgent(pos, Vector3.one, Quaternion.identity, EntityType.EntityType_Doodad, protoId);
		agent.spInfo = AgentInfo.s_defAgentInfo;
		return agent;
	}
	
	public static PeEntity CreateDoodad(int protoId, Vector3 pos)
	{
		SceneEntityPosAgent agent = CreateAgent(pos, protoId);
		CreateDoodad (agent);
		return agent.entity;
	}
	public static PeEntity CreateDoodad(int protoId, Vector3 pos, Vector3 scl, Quaternion rot)
	{
		SceneEntityPosAgent agent = CreateAgent(pos, protoId, scl, rot);
		CreateDoodad (agent);
		return agent.entity;
	}
	public static void CreateDoodad(SceneEntityPosAgent agent)
	{
		agent.entity = CreateDoodad(false, agent.spInfo as AgentInfo, agent.protoId, agent.Pos, agent.Scl, agent.Rot, agent.Id);
	}
	public static void CreateStoryDoodads(bool bNew)
	{
        if (PeGameMgr.IsMulti)
            return;
		foreach (KeyValuePair<int, SceneDoodadDesc> iter in StoryDoodadMap.s_dicDoodadData)
		{
			bool bSerializable = iter.Value._type > 0;
			if(!bNew && bSerializable){			
				// serializable doodad would be created from savedata loading
				continue;
			}

			AgentInfo spInfo = new AgentInfo(iter.Value._id, iter.Value._isShown, iter.Value._campId, iter.Value._damageId);
			if(!bSerializable){	// Add to scene
				SceneEntityPosAgent agent = CreateAgent(iter.Value._pos, iter.Value._protoId, iter.Value._scl, iter.Value._rot);
				agent.spInfo = spInfo;
				SceneMan.AddSceneObj(agent);
				continue;
			}

			// bNew and serializable
			CreateDoodad(true, spInfo, iter.Value._protoId, iter.Value._pos, iter.Value._scl, iter.Value._rot);
		}
	}

	public static PeEntity CreateRandTerDoodad(int protoId, Vector3 pos, Vector3 scl, Quaternion rot, int townId = -1, int campId = SceneDoodadDesc.c_neutralCamp, int damageId = SceneDoodadDesc.c_neutralDamage,int playerId=-1)
	{
		return CreateDoodad(true, new AgentInfo(townId, true, campId, damageId, playerId), protoId, pos, scl, rot);
	}
//	public static PeEntity CreateRandTerDoodadWithPlayerId(int protoId, Vector3 pos, Vector3 scl, Quaternion rot, int townId = -1, int campId = SceneDoodadDesc.c_neutralCamp, int damageId = SceneDoodadDesc.c_neutralDamage)
//	{
//		return CreateDoodad(true, new AgentInfo(townId, true, campId, damageId,playerId), protoId, pos, scl, rot);
//	}
    public static PeEntity CreateNetRandTerDoodad(int entityId, int protoId, Vector3 pos, Vector3 scl, Quaternion rot, int townId = -1, int campId = SceneDoodadDesc.c_neutralCamp, int damageId = SceneDoodadDesc.c_neutralDamage,int playerId = -1)
    {
        return CreateDoodad(true, new AgentInfo(townId, true, campId, damageId, playerId), protoId, pos, scl, rot, entityId);
    }
//	public static PeEntity CreateNetRandTerDoodadWithPlayerId(int entityId, int protoId, Vector3 pos, Vector3 scl, Quaternion rot, int townId = -1, int campId = SceneDoodadDesc.c_neutralCamp, int damageId = SceneDoodadDesc.c_neutralDamage,int playerId = -1)
//	{
//		return CreateDoodad(true, new AgentInfo(townId, true, campId, damageId), protoId, pos, scl, rot, entityId);
//	}
    public static PeEntity CreateStoryDoodadNet( int assetId,int entityId )
	{
		SceneDoodadDesc doodad = StoryDoodadMap.Get(assetId);
		if(null == doodad)
			return null;
		AgentInfo spInfo = new AgentInfo(doodad._id, doodad._isShown, doodad._campId, doodad._damageId);
		return CreateDoodad(true, spInfo, doodad._protoId, doodad._pos, doodad._scl, doodad._rot,entityId);;
	}

	public static PeEntity CreateDoodadNet(int protoId, Vector3 pos, Vector3 scl, Quaternion rot, int id = -1, int campId = SceneDoodadDesc.c_neutralCamp, int damageId = SceneDoodadDesc.c_neutralDamage)
	{
        AgentInfo spInfo = new AgentInfo(-1, true, campId, damageId);
        PeEntity entity = CreateDoodad(true, spInfo, protoId, pos, scl, rot, id);
		if (null != entity)
			spInfo._damageId = (int)entity.GetAttribute(AttribType.DamageID);

		return entity;
	}
	// private
	static PeEntity CreateDoodad(bool bSerializable, AgentInfo spInfo, int protoId, Vector3 pos, Vector3 scl, Quaternion rot, int id = -1)
	{
		PeEntity entity = null;
        if (PeGameMgr.IsMulti && -1 != id)
		{
			entity = PeEntityCreator.Instance.CreateDoodad(id, protoId, pos, rot, scl);
        }
		else
		{
			entity = bSerializable
					? PeCreature.Instance.CreateDoodad(Pathea.WorldInfoMgr.Instance.FetchRecordAutoId(), protoId, pos, rot, scl)
					: PeEntityCreator.Instance.CreateDoodad(Pathea.WorldInfoMgr.Instance.FetchNonRecordAutoId(), protoId, pos, rot, scl);
		}

		if (null == entity)
		{
			Debug.LogError("Failed to create doodad!");
			return null;
		}			
		if (spInfo != null) {
			SceneDoodadLodCmpt lod = entity.GetCmpt<SceneDoodadLodCmpt> ();
			if (lod != null) {
				lod.Index = spInfo._id;
				lod.IsShown = spInfo._isShown;
				lod.SetDamagable(spInfo._campId, spInfo._damageId,spInfo._playerId);
			}
		}
		return entity;
	}	
	public static void OnDoodadDeath(SkEntity a, SkEntity b){
		if (commonDeathEvent != null) {
			commonDeathEvent(a, b);
		}

		//--to do
		if(PeGameMgr.IsAdventure)
		{
			SceneDoodadLodCmpt lod = a.gameObject.GetComponent<SceneDoodadLodCmpt>();
			if(lod!=null&&lod.Index>=0){
				PeEntity entity = a.gameObject.GetComponent<PeEntity>();
				if(entity!=null){
					bool isSignalTower = a.GetComponentInChildren<MonsterSummoner>()!=null;
					VArtifactTownManager.Instance.OnBuildingDeath(lod.Index,entity.Id,isSignalTower);
				}
			}
		}
	}
}
