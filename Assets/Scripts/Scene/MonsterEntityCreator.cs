using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Pathea;
using Pathea.PeEntityExt;
using SkillSystem;

public class MonsterEntityCreator
{
	public class AgentInfo : SceneEntityPosAgent.AgentInfo
	{
		public static new AgentInfo s_defAgentInfo = new AgentInfo ();

		public float _bsRate = -1;	//rate to create boss
		public EntityGrp _grp = null;
		public EntityMonsterBeacon _bcn = null;
		public int _colorType=-1;
		public int _playerId=-1;
		public int _buffId = 0;
		// interface
		public AgentInfo(float bsRate = -1){								_bsRate = bsRate;															}
		public AgentInfo(EntityGrp grp,int colorType=-1,int playerId=-1,int buffId=0){	_grp = grp;				_colorType = colorType;		_playerId = playerId;	_buffId=buffId;}
		public AgentInfo(EntityMonsterBeacon bcn){							_bcn = bcn;				_colorType = _bcn.CampColor;						}
		public AgentInfo(int colorType,int playerId,int buffId=0){			_colorType = colorType;	_playerId = playerId; _buffId=buffId;}

		public override void OnSuceededToCreate (SceneEntityPosAgent agent)
		{
			LodCmpt entityLodCmpt = agent.entity.lodCmpt;
			if(entityLodCmpt != null){
				entityLodCmpt.onDestruct += (e)=>{agent.DestroyEntity();};
			}
			if (_bsRate >= 0) {
				if (agent.entity.aliveEntity != null){
					agent.entity.aliveEntity.deathEvent += (a,b)=>{
						SceneMan.self.StartCoroutine(DelayBossReborn(agent));
					};
				}
			}
			if (_bcn != null) {
				SceneMan.RemoveSceneObj (agent);
			}
		}
		IEnumerator DelayBossReborn(SceneEntityPosAgent agent)
		{
			SceneMan.RemoveSceneObj(agent);
			yield return new WaitForSeconds (240);	// wait 4 min
			SceneMan.AddSceneObj(agent);
		}
	}

	public static event Action<PeEntity> commonCreateEvent;
	public static event Action<SkEntity, SkEntity> commonDeathEvent;
	public static void Init()
	{
		commonCreateEvent = null;
		commonDeathEvent = null;
		commonDeathEvent += RepProcessor.OnMonsterDeath;
	}

	public static SceneEntityPosAgent CreateAgent(Vector3 pos, int protoId, Vector3 scl, Quaternion rot)
	{
		SceneEntityPosAgent agent = new SceneEntityPosAgent(pos, scl, rot, EntityType.EntityType_Monster, protoId);
		agent.spInfo = AgentInfo.s_defAgentInfo;
		return agent;
	}

	public static SceneEntityPosAgent CreateAgent(Vector3 pos, int protoId = -1)
	{
		SceneEntityPosAgent agent = new SceneEntityPosAgent(pos, Vector3.one, Quaternion.identity, EntityType.EntityType_Monster, protoId);
		agent.spInfo = AgentInfo.s_defAgentInfo;
		return agent;
	}

	public static SceneEntityPosAgent CreateAdAgent(Vector3 pos, int protoId,int colorType,int playerId,int buffId=0,bool bride = true)
	{
		SceneEntityPosAgent agent = new SceneEntityPosAgent(pos, Vector3.one, Quaternion.identity, EntityType.EntityType_Monster, protoId);
		agent.spInfo = new MonsterEntityCreator.AgentInfo(colorType,playerId,buffId);
        agent.canRide = bride;
        return agent;
	}

	public static PeEntity CreateMonster(int protoId, Vector3 pos)
	{
		SceneEntityPosAgent agent = CreateAgent(pos, protoId);
		CreateMonster (agent);
		return agent.entity;
	}

    public static PeEntity CreateAdMonster(int protoId, Vector3 pos,int colorType,int playerId,int buffId=0,bool bride = true)
	{
		SceneEntityPosAgent agent = CreateAdAgent(pos, protoId,colorType,playerId,buffId,bride);
		CreateMonster (agent);
		return agent.entity;
	}
	public static PeEntity CreateMonster(int protoId, Vector3 pos, Vector3 scl, Quaternion rot)
	{
		SceneEntityPosAgent agent = CreateAgent(pos, protoId, scl, rot);
		CreateMonster (agent);
		return agent.entity;
	}

	public static PeEntity CreateDungeonMonster(int protoId, Vector3 pos,int dungeonId,int buffId = 0,int colorType=-1,int playerId=-1){
		if(PeGameMgr.IsSingle){
			PeEntity m =  CreateAdMonster(protoId,pos,colorType,playerId,buffId,false);
			if(m!=null){
				m.SetAttribute(Pathea.AttribType.CampID, DungeonMonster.CAMP_ID);
				m.SetAttribute(Pathea.AttribType.DamageID, DungeonMonster.DAMAGE_ID);
			}else{
				Debug.LogError("createDungeonMonsterFailed!");
			}
			return m;
		}
		else{
			SceneEntityPosAgent agent = CreateAdAgent(pos, protoId,colorType,playerId,buffId,false);
			agent.entity = null;
			// Compute agent.protoId
			EntityGrp grp;
			EntityMonsterBeacon bcn;
			float exScale = -1.0f;
			if(!ParseAgentInfo (agent, out grp, out bcn, ref exScale,ref colorType,ref playerId,ref buffId)){
				return agent.entity;
			}
			NetworkManager.WaitCoroutine (PlayerNetwork.RequestCreateAi (agent.protoId, agent.Pos, null == grp ? -1 : grp.Id, null == bcn ? -1 : bcn.Id,dungeonId,colorType,playerId,buffId));
			return agent.entity;
		}
	}

    public static void AttachMonsterDeathEvent(PeEntity entity)
    {
        Pathea.SkAliveEntity skAlive = entity.GetCmpt<Pathea.SkAliveEntity>();
        if (skAlive != null)
        {
            skAlive.deathEvent += skAlive.OnDeathProcessBuff;
            skAlive.deathEvent += (a, b) => {
                if (commonDeathEvent != null)
                    commonDeathEvent(a, b);
            };
        }
    }
	public static void CreateMonster (SceneEntityPosAgent agent)			// treat proid as type in monsterBesiegeSpawn
	{
		agent.entity = null;
		// Compute agent.protoId
		EntityGrp grp;
		EntityMonsterBeacon bcn;
		float exScale = -1.0f;
		int colorType =-1;
		int playerId = -1;
		int buffId = 0;
		if(!ParseAgentInfo (agent, out grp, out bcn, ref exScale,ref colorType,ref playerId,ref buffId)){
			return;
		}

		if (NetworkInterface.IsClient) {
			NetworkManager.WaitCoroutine (PlayerNetwork.RequestCreateAi (agent.protoId, agent.Pos, null == grp ? -1 : grp.Id, null == bcn ? -1 : bcn.Id,-1,colorType,playerId,buffId));
			return;
		}

		// If grp need network sync, put this statement down 
		if ((agent.protoId & EntityProto.IdGrpMask) != 0) {
			agent.entity = EntityGrp.CreateMonsterGroup (agent.protoId & ~EntityProto.IdGrpMask, agent.Pos,colorType,playerId,-1,buffId);
			return;
		}

		Vector3 pos = new Vector3(agent.Pos.x, agent.Pos.y, agent.Pos.z);   
		MonsterProtoDb.Item protoData = MonsterProtoDb.Get (agent.protoId);
		if (protoData != null && bcn == null && !agent.FixPos) {	// exclude fixpos
			float hOffset = protoData.hOffset;
			if(hOffset < 0.0f){
				float depth = VFVoxelWater.self.UpToWaterSurface(agent.Pos.x, agent.Pos.y, agent.Pos.z);
				if(depth <= 0){
					Debug.LogError ("[SceneEntityCreator]Failed to create water monster becasue not in water:" + agent.protoId);
					return;
				}
				pos.y += depth + hOffset;
			} else if(hOffset > 2.0f){
				float depth = VFVoxelWater.self.UpToWaterSurface(agent.Pos.x, agent.Pos.y, agent.Pos.z);
				if(depth <= 0){
					RaycastHit hitinfo;
					float rayLen = 128.0f;
					if(Physics.Raycast(pos + rayLen*Vector3.up, Vector3.down, out hitinfo, rayLen, SceneMan.DependenceLayer)){	// avoid from birds genterating in cave
						pos.y = hitinfo.point.y;
					}
					pos.y += hOffset;
				} else {
					pos.y += depth + hOffset;
				}
			} else{
				float depth = VFVoxelWater.self.UpToWaterSurface(agent.Pos.x, agent.Pos.y, agent.Pos.z);
				if(depth <= 0){
					pos.y += hOffset;
				} else {
					Debug.LogError ("[SceneEntityCreator]Failed to create land monster becasue in water:" + agent.protoId);
					return;
				}
			}
		}
		int id  = Pathea.WorldInfoMgr.Instance.FetchNonRecordAutoId ();	// not save
        agent.entity = PeEntityCreator.Instance.CreateMonster (id, agent.protoId, pos, agent.Rot, agent.Scl, exScale, colorType,buffId);
		if (null == agent.entity) {
			Debug.LogError ("[SceneEntityCreator]Failed to create monster:" + agent.protoId);
			return;
		}

        agent.entity.monster.Ride(agent.canRide);
        //colorType,playerId
        //if(colorType>=0)
        //	PeEntityCreator.InitMonsterSkinRandom(agent.entity,colorType);
        if (playerId>=0)
			agent.entity.SetAttribute(AttribType.DefaultPlayerID,playerId);

        if (commonCreateEvent != null) {	commonCreateEvent (agent.entity);		}
		if (grp != null) {					grp.OnMemberCreated (agent.entity);		}
		if (bcn != null) {					bcn.OnMonsterCreated (agent.entity);	}
		return;
	}

	#region helper
	static bool ParseAgentInfo(SceneEntityPosAgent agent, out EntityGrp grp, out EntityMonsterBeacon bcn, ref float exScale,ref int colorType,ref int playerId, ref int buffId)	// bValidAgent
	{
		AgentInfo info = agent.spInfo as AgentInfo;
		grp = info != null ? info._grp : null;
		bcn = info != null ? info._bcn : null;
		colorType = info!=null?info._colorType:-1;
		playerId = info!=null?info._playerId:-1;
		buffId = info!=null?info._buffId:0;
		if (bcn != null) {
			agent.protoId = MonsterEntityCreator.GetMonsterProtoIDForBeacon (agent.protoId, agent.Pos, ref exScale);			
			if ((agent.protoId & EntityProto.IdAirborneAllMask) != 0) 
			{
				bcn.AddAirborneReq(agent);
				return false;
			}
		}
		if (0 > agent.protoId){
		    agent.protoId = (info == null || info._bsRate < 0) 
				? GetMonsterProtoID (agent.Pos, ref exScale)
				: GetBossMonsterProtoID(agent.Pos, info._bsRate, ref exScale);
		}
		return agent.protoId >= 0;
	}
	static int GetMonsterProtoID (Vector3 pos, ref float fScale)
	{
		int pathID = 0, typeID = 0;
		typeID = (int)AiUtil.GetPointType (pos);
		if (PeGameMgr.IsStory) {
			int mapid = PeMappingMgr.Instance.GetAiSpawnMapId (new Vector2 (pos.x, pos.z));
			pathID = AISpeciesData.GetRandomAI (AISpawnDataStory.GetAiSpawnData (mapid, typeID));
		} else if (PeGameMgr.IsAdventure) {
			int mapID = AiUtil.GetMapID (pos);
			int areaID = AiUtil.GetAreaID (pos);
			pathID = AISpawnDataAdvSingle.GetPathIDScale (mapID, areaID, typeID, ref fScale);
		}		
		return pathID;
	}
	static int GetBossMonsterProtoID(Vector3 pos, float rndVal, ref float fScale)
	{
		int pathID = 0, typeID = 0;
		typeID = (int)AiUtil.GetPointType (pos);
		int mapID = AiUtil.GetMapID (pos);
		int areaID = AiUtil.GetAreaID (pos);
		pathID = AISpawnDataAdvSingle.GetBossPathIDScale (mapID, areaID, typeID, rndVal, ref fScale);
		return pathID;
	}
	static int GetMonsterProtoIDForBeacon (int bcnProtoId, Vector3 pos, ref float fScale)
	{
		if (!EntityMonsterBeacon.IsBcnMonsterProtoId(bcnProtoId))	// not encoded
			return bcnProtoId;

		int spType, lvl, spawnType;
		EntityMonsterBeacon.DecodeBcnMonsterProtoId (bcnProtoId, out spType, out lvl, out spawnType);

		int terType = 0;
		int areaType = -1;
		AISpawnTDWavesData.TDMonsterData md = null;
		if (spType < EntityMonsterBeacon.TowerDefenseSpType_Beg) {
			PointType pt = AiUtil.GetPointType (pos);
			switch (pt) {
			default:
			case PointType.PT_Ground:
				terType = 0;
				break;
			case PointType.PT_Water:
				terType = 1;
				break;
			case PointType.PT_Slope:
				terType = 2;
				break;
			case PointType.PT_Cave:
				terType = 3;
				break;
			}
			areaType = PeGameMgr.IsStory ? PeMappingMgr.Instance.GetAiSpawnMapId (new Vector2 (pos.x, pos.z)) : AiUtil.GetMapID (pos);
            if (PeGameMgr.IsAdventure && areaType == 5)
                areaType = 2;
		}
		int opPlayerId = -1;
		if (MainPlayerCmpt.gMainPlayer != null) {
			SkAliveEntity skPlayer = MainPlayerCmpt.gMainPlayer.Entity.aliveEntity;
			if(skPlayer != null){
				opPlayerId = (int)skPlayer.GetAttribute(AttribType.DefaultPlayerID);
			}
		}
		md = AISpawnTDWavesData.GetMonsterProtoId (!PeGameMgr.IsStory, spType, lvl, spawnType, areaType, terType, opPlayerId);
		if (null != md) {
			// md.ProtoId |= EntityProto.IdAirbornePujaMask;	// test airborne
			if (md.IsAirbornePuja)	md.ProtoId |= EntityProto.IdAirbornePujaMask;
			if (md.IsAirbornePaja)	md.ProtoId |= EntityProto.IdAirbornePajaMask;
			if (md.IsGrp)			md.ProtoId |= EntityProto.IdGrpMask;
			return md.ProtoId;
		}		
		return -1;
	}
	#endregion
}