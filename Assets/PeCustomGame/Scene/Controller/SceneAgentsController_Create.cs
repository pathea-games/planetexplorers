// Custom game mode:  Scene Agents Controller partial class, Here are create Function . 
// (c) by Wu Yiqiu

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Pathea.PeEntityExt;
using Pathea.PeEntityExtTrans;
using Pathea;

namespace PeCustom
{
	public partial class SceneAgentsContoller : PeCustomScene.SceneElement, IMonoLike, ISceneController
	{
		#region CREATE_AGENTS
		
		/// <summary>
		/// Creates all the agents from data source
		/// </summary>
		void CreateAgents (SpawnDataSource ds)
		{
			
			foreach (KeyValuePair<int, MonsterSpawnPoint> kvp in ds.monsters)
			{
				SceneEntityAgent agent = _createMstAgent(kvp.Value, true, null, null, !kvp.Value.isDead);
                agent.ScenarioId = kvp.Value.ID;
				kvp.Value.agent = agent;
	
				AddMstDeadAgent(agent);
			}
			
			
			foreach (KeyValuePair<int, MonsterSpawnArea> kvp in ds.areas)
			{
				for (int i = 0; i < kvp.Value.Spawns.Count; i++)
				{
					MonsterSpawnArea.SocialSpawns mss = kvp.Value.Spawns[i];
					
					if (!mss.isSocial)
					{
						for (int j = 0; j < mss.spawnPoints.Count; j++)
						{
							MonsterSpawnPoint sp = mss.spawnPoints[j];
							SceneEntityAgent agent = _createMstAgent(sp,  true, kvp.Value, null, !sp.isDead);
                            agent.ScenarioId = kvp.Value.ID;
                            sp.agent = agent;
							
							AddMstDeadAgent(agent);
						}
					}
					else
					{
						_createMstAgent(mss.centerSP, false, kvp.Value, mss.spawnPoints.ToArray());
					}
				}
			}
			
			CreateNpcAgents(ds.npcs);
			CreateDoodadAgents(ds.doodads);
			
			
			foreach (KeyValuePair<int, EffectSpwanPoint> kvp in ds.effects)
			{
				SceneStaticEffectAgent agent = SceneStaticEffectAgent.Create(kvp.Value.Prototype, kvp.Value.spawnPos, kvp.Value.Rotation, kvp.Value.Scale);
                agent.ScenarioId = kvp.Value.ID;
                SceneMan.AddSceneObj(agent);
			}

            foreach (KeyValuePair<int, ItemSpwanPoint> kvp in ds.items)
            {
                if (kvp.Value.isNew)
                {
                    DragArticleAgent agent = DragArticleAgent.PutItemByProroId(kvp.Value.Prototype
                                                                               , kvp.Value.spawnPos
                                                                               , kvp.Value.Scale
                                                                               , kvp.Value.Rotation
                                                                               , kvp.Value.CanPickup
                                                                               , kvp.Value.IsTarget);
                    if (agent != null)
                    {
                        agent.ScenarioId = kvp.Value.ID;
                        kvp.Value.isNew = false;
                        kvp.Value.ItemObjId = agent.itemDrag.itemObj.instanceId;
                    }
                }
            }

        }

        /// <summary>
        /// Create a agent for a spawn point and add sp to the spawn data source
        /// </summary>
        void CreateAgent (SpawnDataSource ds, SpawnPoint sp)
        {
            // Create Monster Agent
            if (sp as MonsterSpawnPoint != null)
            {
                MonsterSpawnPoint msp = sp as MonsterSpawnPoint;

                if (ds.AddMonster(msp))
                {
                    SceneEntityAgent agent = _createMstAgent(msp, true, null, null, !msp.isDead);
                    msp.agent = agent;
                    agent.ScenarioId = sp.ID;
                    AddMstDeadAgent(agent);
                }
                else
                {
                    Debug.LogError("Add Monster spawn point error");
                }
            }
            // Create Npc Agent
            else if (sp as NPCSpawnPoint != null)
            {
                NPCSpawnPoint nsp = sp as NPCSpawnPoint;
                if (ds.AddNpc(nsp))
                {
                    SceneEntityAgent agent = new SceneEntityAgent(nsp);
                    nsp.agent = agent;
                    agent.ScenarioId = sp.ID;
                    SceneMan.AddSceneObj(agent);
                }
                else
                {
                    Debug.LogError("Add npc spawn point error");
                }
            }
            // Create Doodad Agent
            else if (sp as DoodadSpawnPoint != null)
            {
                DoodadSpawnPoint dsp = sp as DoodadSpawnPoint;
                if (ds.AddDoodad(dsp))
                {
                    SceneStaticAgent agent = new SceneStaticAgent(dsp, true);
                    dsp.agent = agent;
                    agent.ScenarioId = sp.ID;
                    SceneMan.AddSceneObj(agent);
                }
                else
                {
                    Debug.LogError("Add doodad spawn point error");
                }
            }
            // Create Item 
            else if (sp as ItemSpwanPoint != null)
            {
                ItemSpwanPoint isp = sp as ItemSpwanPoint;
                if (ds.AddItem(isp))
                {
                    DragArticleAgent agent = DragArticleAgent.PutItemByProroId(isp.Prototype
                                                                               , isp.spawnPos
                                                                               , isp.Scale
                                                                               , isp.Rotation
                                                                               , isp.CanPickup
                                                                               , isp.IsTarget);
                    if (agent != null)
                    {
                        isp.isNew = false;
                        isp.ItemObjId = agent.itemDrag.itemObj.instanceId;
                        agent.ScenarioId = sp.ID;
                    }
                }
                else
                {
                    Debug.LogError("Add item spawn point error");
                }
              
            }
            // Create Effect
            else if (sp as EffectSpwanPoint != null)
            {

            }
        }
		
		SceneEntityAgent _createMstAgent(MonsterSpawnPoint _point, bool is_saved, MonsterSpawnArea _area = null, MonsterSpawnPoint[] _groupPoints = null, bool save_to_scene = true) 
        {
            SceneEntityAgent agent = new SceneEntityAgent(_point, is_saved, _area, _groupPoints);
            
            if (save_to_scene)
                SceneMan.AddSceneObj(agent);
            
            return agent;
        }
        
        
        public void CreateNpcAgents (Dictionary<int, NPCSpawnPoint> points)
        {
            
            foreach (KeyValuePair<int, NPCSpawnPoint> kvp in points)
            {
                SceneEntityAgent agent = new SceneEntityAgent(kvp.Value);
                agent.ScenarioId = kvp.Value.ID;
                kvp.Value.agent = agent;
                SceneMan.AddSceneObj(agent);
            }
        }
        
        public void CreateDoodadAgents (Dictionary<int, DoodadSpawnPoint> points)
        {
            foreach (KeyValuePair<int, DoodadSpawnPoint> kvp in points)
            {
                SceneStaticAgent agent = new SceneStaticAgent(kvp.Value, true);
                agent.ScenarioId = kvp.Value.ID;
                kvp.Value.agent = agent;
                SceneMan.AddSceneObj(agent);
			}
		}

		#endregion

		#region CREATE_ENTITY
		
		PeEntity CreateMonster (MonsterSpawnPoint point, bool save)
		{
			PeEntity entity = null;
			
			if (save)
			{
				if (point.EntityID == -1)
				{
					int id = Pathea.WorldInfoMgr.Instance.FetchRecordAutoId();
					entity = CreatureMgr.Instance.CreateMonster(CustomGameData.Mgr.Instance.curGameData.WorldIndex, id, point.Prototype);
					point.EntityID = entity.Id;
					
					entity.ExtSetPos(point.spawnPos);
					entity.ExtSetRot(point.Rotation);
				}
				else
				{
                    //entity = EntityMgr.Instance.Get(point.EntityID);
                    entity = CreatureMgr.Instance.CreateMonster(CustomGameData.Mgr.Instance.curGameData.WorldIndex, point.EntityID, point.Prototype);
     //               if (entity == null)
					//{
					//	Debug.LogError("Cant Find the Entity [ID " +  entity.ToString() + "]");
					//}
					
				}
			}
			else
			{
				int id = Pathea.WorldInfoMgr.Instance.FetchNonRecordAutoId();
				entity = PeEntityCreator.Instance.CreateMonster(id, point.Prototype, Vector3.zero, Quaternion.identity, Vector3.one);
				point.EntityID = entity.Id;
				entity.ExtSetPos(point.spawnPos);								
				entity.ExtSetRot(point.Rotation);
			}

            if (entity != null)
            {
                entity.scenarioId = point.ID;
                entity.SetAttribute(AttribType.DefaultPlayerID, point.PlayerIndex);
                PeScenarioEntity pse = entity.gameObject.GetComponent<PeScenarioEntity>();
                if (pse == null)
                    pse = entity.gameObject.AddComponent<PeScenarioEntity>();
                pse.spawnPoint = point;
            }

			return entity;
		}
		
		const string sMonsterGroupPath = "Prefab/Custom/CustomMonsterGroup";
		EntityGrp CreateMonsterGroup(SpawnPoint grp_sp, MonsterSpawnPoint[] points, MonsterSpawnArea area)
		{
			int noid = Pathea.WorldInfoMgr.Instance.FetchNonRecordAutoId();
			EntityGrp grp = EntityMgr.Instance.Create(noid, sMonsterGroupPath, Vector3.zero, Quaternion.identity, Vector3.one) as EntityGrp;


            if (grp == null)
			{
				Debug.LogError ("Load Prefab Error");
				return null;
			}
			
			grp._protoId = grp_sp.Prototype;
			grp._cntMin = area.AmountPerSocial;
			grp._cntMax = area.AmountPerSocial;
			
			
			for (int i = 0; i < points.Length; i++)
			{
				MonsterSpawnPoint sp = points[i];
				
				Pathea.WorldInfoMgr.Instance.FetchNonRecordAutoId();
				SceneEntityAgent agent = new SceneEntityAgent(sp, false, area, null);
                agent.ScenarioId = area.ID;
                grp.scenarioId = area.ID;
                agent.entityGp = grp;
				sp.agent = agent;
				
				AddMstDeadAgent(agent);
				
				if (!sp.isDead)
					SceneMan.AddSceneObj(agent);
			}
			
			return grp;
		}
		
		PeEntity CreateNpc(NPCSpawnPoint point)
		{
			PeEntity entity = null;
			
			if (point.EntityID == -1)
			{
				int id  = Pathea.WorldInfoMgr.Instance.FetchRecordAutoId ();
                entity = CreatureMgr.Instance.CreateNpc(CustomGameData.Mgr.Instance.curGameData.WorldIndex, id, point.Prototype);
				point.EntityID = entity.Id;
				entity.ExtSetName(new CharacterName(point.Name));
				
				PeTrans view = entity.peTrans;
				if (null == view)
				{
					Debug.LogError("[SceneEntityCreator]No viewCmpt in npc:" + point.Prototype);
					return null;
				}
				
				view.position = point.spawnPos;
				view.rotation = point.Rotation;
				view.scale = point.Scale;
            }
			else
			{
                entity = CreatureMgr.Instance.CreateNpc(CustomGameData.Mgr.Instance.curGameData.WorldIndex, point.EntityID, point.Prototype);
                if (entity == null)
				{
					Debug.LogError("Cant Finde the Entity [ID " +  point.EntityID.ToString() + "]");
				}
			}
			
			if (entity != null)
			{
                entity.scenarioId = point.ID;
                entity.SetAttribute(AttribType.DefaultPlayerID, point.PlayerIndex);

                entity.SetBirthPos(point.spawnPos);
                PeScenarioEntity pse = entity.gameObject.GetComponent<PeScenarioEntity>();
                if (pse == null)
                    pse = entity.gameObject.AddComponent<PeScenarioEntity>();
                pse.spawnPoint = point;
            }
			
			return entity;
		}
		
		PeEntity CreadteDoodad (DoodadSpawnPoint point, bool is_save)
		{
			PeEntity entity = null;
			
			if (is_save)
			{
				if (point.EntityID == -1)
				{
					int id = Pathea.WorldInfoMgr.Instance.FetchRecordAutoId();
					entity = CreatureMgr.Instance.CreateDoodad(CustomGameData.Mgr.Instance.curGameData.WorldIndex, id, point.Prototype);
                    point.EntityID = entity.Id;
                    
                    _initDoodad(entity, point);
                }
                else
                {
                    //PeEntity ent = EntityMgr.Instance.Get(point.EntityID);
                    entity = CreatureMgr.Instance.CreateDoodad(CustomGameData.Mgr.Instance.curGameData.WorldIndex, point.EntityID, point.Prototype);
                    if (entity == null)
                    {
                        Debug.LogError("Cant Find the Entity [ID " + point.EntityID.ToString() + "]");
                    }
                }
            }
            else
            {
                Pathea.WorldInfoMgr.Instance.FetchNonRecordAutoId();
				entity = PeEntityCreator.Instance.CreateDoodad(Pathea.WorldInfoMgr.Instance.FetchNonRecordAutoId(), point.Prototype, Vector3.zero, Quaternion.identity, Vector3.one);
                point.EntityID = entity.Id;
                
                _initDoodad(entity, point);
                
            }

            if (entity != null)
            {
                entity.scenarioId = point.ID;
                PeScenarioEntity pse = entity.gameObject.GetComponent<PeScenarioEntity>();
                if (pse == null)
                    pse = entity.gameObject.AddComponent<PeScenarioEntity>();
                pse.spawnPoint = point;
            }
            
            return entity;
            
        }
        
        bool _initDoodad(PeEntity entity, DoodadSpawnPoint sp)
        {
            if (entity == null)
            {
                Debug.LogError("The entity that given is null");
                return false;
            }
            
            PeTrans view = entity.peTrans;
            if (null == view)
            {
                Debug.LogError("[SceneEntityCreator]No viewCmpt in doodad:" + sp.Prototype);
                return false;
            }
            
            view.position = sp.spawnPos;
            view.rotation = sp.Rotation;
            view.scale = sp.Scale;
            
            SceneDoodadLodCmpt lod = entity.GetCmpt<SceneDoodadLodCmpt> ();
            if (lod != null) 
            {
                lod.IsShown = sp.Visible;
                lod.IsDamagable = sp.IsTarget;
            }
			
			return true;
		}
		#endregion
	}

}