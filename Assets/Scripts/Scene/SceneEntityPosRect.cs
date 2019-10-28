using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Pathea;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtTrans;

// For rnd-gen entities those are not saved
public class SceneEntityPosRect
{
	class RndNpcAgentInfo : NpcEntityCreator.AgentInfo
	{
		SceneEntityPosRect _posRect;
		public RndNpcAgentInfo(SceneEntityPosRect posRect) : base()
		{
			_posRect = posRect;
		}
		public override void OnSuceededToCreate (SceneEntityPosAgent agent)
		{
			base.OnSuceededToCreate (agent);
			_posRect._posRndNpcAgents.Remove (agent);
		}
	}
	const int CntNpc = 2;
	const int EntityCreationRadius = 128;
	static int CntMonAdv{ get{ return 6 + UnityEngine.Random.Range (0, 2); } }
	static int CntMonOth{ get { return 5 + UnityEngine.Random.Range (0, 2); } }
	public static int EntityNpcNum{ 	get { return (PeGameMgr.IsStory||PeGameMgr.IsCustom)  ? 0 : CntNpc; } }
	public static int EntityMonsterNum{ get { return (PeGameMgr.playerType == PeGameMgr.EPlayerType.Tutorial) ? 0 : (PeGameMgr.IsAdventure ? CntMonAdv : CntMonOth); } }
	public static void EntityPosToRectIdx(Vector3 pos, IntVector2 outIdx)
	{
		outIdx.x = Mathf.FloorToInt (pos.x / (2 * SceneEntityPosRect.EntityCreationRadius));
		outIdx.y = Mathf.FloorToInt (pos.z / (2 * SceneEntityPosRect.EntityCreationRadius));
	}
	public static Vector3 RectIdxToCenterPos(IntVector2 idx)
	{
		return new Vector3((idx.x*2+1)*SceneEntityPosRect.EntityCreationRadius, 
		                   0.0f, 
		                   (idx.y*2+1)*SceneEntityPosRect.EntityCreationRadius);
	}

	List<ISceneObjAgent> _posRndNpcAgents = new List<ISceneObjAgent>();
	List<ISceneObjAgent> _posRndMonsterAgents = new List<ISceneObjAgent>();
	List<int> _posFixedMonsterIds = new List<int>();
	RndNpcAgentInfo _rndNpcAgentInfo;
	IntVector2 _idx;
	public int CntNpcNotCreated{ get { return _posRndNpcAgents.Count; } }
	
	public SceneEntityPosRect(int ix, int iz)
	{
		_rndNpcAgentInfo = new RndNpcAgentInfo (this);
		_idx = new IntVector2 (ix, iz);
	}
	public void Fill(int cntNpc, int cntMonster)
	{
		Vector3 center = RectIdxToCenterPos (_idx);
		bool bAvailable = false;
		SceneEntityPosAgent agent;
		System.Random rand = new System.Random ();
		for (int i = 0; i < cntNpc; i++){
			Vector3 point = !PeGameMgr.randomMap ? GetEntityPoint(center, out bAvailable) : GetNpcPointInRndTer(center, out bAvailable);
			if(bAvailable){
				if(VFDataRTGen.IsTownConnectionType(Mathf.RoundToInt(point.x),Mathf.RoundToInt(point.z))){
					agent = NpcEntityCreator.CreateAgent(point);
					agent.spInfo = _rndNpcAgentInfo;
					_posRndNpcAgents.Add(agent);	// npc entity would not be destroyed by scene
				}
				else
				{
					if(rand.NextDouble()<0.25){
						agent = NpcEntityCreator.CreateAgent(point);
						agent.spInfo = _rndNpcAgentInfo;
						_posRndNpcAgents.Add(agent);	// npc entity would not be destroyed by scene
					}
				}
			}
		}
		for (int i = 0; i < cntMonster; i++){
			Vector3 point = GetEntityPoint(center, out bAvailable);
			if(bAvailable && null == AIErodeMap.IsInErodeArea2D(point)){
				_posRndMonsterAgents.Add(MonsterEntityCreator.CreateAgent(point));
			}
		}
		SceneMan.AddSceneObjs (_posRndNpcAgents);
		SceneMan.AddSceneObjs (_posRndMonsterAgents);
		
		if (PeGameMgr.IsStory) {
			_posFixedMonsterIds = AISpawnPoint.Find (center.x - EntityCreationRadius, center.z - EntityCreationRadius, center.x + EntityCreationRadius, center.z + EntityCreationRadius);
			if (_posFixedMonsterIds.Count > 0) {
				SceneEntityCreatorArchiver.Instance.AddFixedSpawnPointToScene (_posFixedMonsterIds);
			}
		} else if (PeGameMgr.IsSingleAdventure) {
			SceneEntityPosRect.AddProcedualBossSpawnPointToScene(_idx);
		}
	}
	Vector3 GetNpcPointInRndTer(Vector3 cpos, out bool bSuc) // Special process to get creation pos for NPC.
	{
		Vector2 v2 = UnityEngine.Random.insideUnitCircle.normalized;
		v2 = v2 * UnityEngine.Random.Range(-SceneEntityPosRect.EntityCreationRadius, SceneEntityPosRect.EntityCreationRadius);
		IntVector2 iv = new IntVector2((int)(cpos.x+v2.x), (int)(cpos.z+v2.y));
		float fy = VFDataRTGen.GetPosTop(iv, out bSuc);			
		if (!bSuc){
			return Vector3.zero;
		}
		return new Vector3(iv.x, fy+1, iv.y);
	}
	Vector3 GetEntityPoint(Vector3 cpos, out bool bSuc)
	{
		Vector2 v2 = UnityEngine.Random.insideUnitCircle.normalized;
		v2 = v2 * UnityEngine.Random.Range(-SceneEntityPosRect.EntityCreationRadius, SceneEntityPosRect.EntityCreationRadius);
		bSuc = true;
		return new Vector3(cpos.x+v2.x, 
		                   SceneEntityPosAgent.PosYTBD, 
		                   cpos.z+v2.y);
	}

	#region SpawnPointForBossRuntime
	public static void AddProcedualBossSpawnPointToScene(IntVector2 idxPosRect)
	{
		#if false
		// (256*2)*(256*2) one
		int idxX = idxPosRect.x >> 1;
		int idxY = idxPosRect.y >> 1;
		int idx = ((idxPosRect.y - (idxY << 1))<<1) + (idxPosRect.y - (idxY << 1));
		int idxMax = 4;
		int seed = RandomMapConfig.RandSeed + idxX * 722 + idxY;	// Magic code
		System.Random rnd4BossGen = new System.Random(seed);
		int idx0 = rnd4BossGen.Next (idxMax);
		if (idx != idx0)
			return;
		#else
		// (256*4)*(256*4) one
		int idxX = idxPosRect.x >> 2;
		int idxY = idxPosRect.y >> 2;
		int idx = ((idxPosRect.y - (idxY << 2))<<2) + (idxPosRect.x - (idxX << 2));
		int idxMax = 16;
		int seed = RandomMapConfig.RandSeed + idxY * 722 + idxX;	// Magic code
		System.Random rnd4BossGen = new System.Random(seed);
		int idx0 = rnd4BossGen.Next (idxMax);
		int idx1 = rnd4BossGen.Next (idxMax);
		while(idx1 == idx0){
			idx1 = rnd4BossGen.Next (idxMax);
		}
		if (idx != idx0 && idx != idx1)
			return;
		#endif
		rnd4BossGen = new System.Random (seed + idx);
		float dx = (float)rnd4BossGen.NextDouble ();
		float dy = (float)rnd4BossGen.NextDouble ();
		Vector3 pos = new Vector3((dx+idxPosRect.x) * 2 * SceneEntityPosRect.EntityCreationRadius, 
		                          SceneEntityPosAgent.PosYTBD,
		                          (dy+idxPosRect.y) * 2 * SceneEntityPosRect.EntityCreationRadius);
		if(null == AIErodeMap.IsInScaledErodeArea2D(pos, 1.2f)){
			SceneEntityPosAgent agent = MonsterEntityCreator.CreateAgent(pos);
			agent.spInfo = new MonsterEntityCreator.AgentInfo ((float)rnd4BossGen.NextDouble());
			SceneMan.AddSceneObj(agent);
			Debug.Log ("<color=red>Boss Spawn Point</color>"+pos);
		}
	}
	#endregion	
}