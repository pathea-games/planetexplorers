// Custom game mode  Scene Agents Controller . 
// (c) by Wu Yiqiu

using UnityEngine;
using System.Collections.Generic;
using System.IO;

using Pathea.PeEntityExt;
using Pathea.PeEntityExtTrans;
using SkillSystem; 
using Pathea;

namespace PeCustom
{
	public partial class SceneAgentsContoller : PeCustomScene.SceneElement, IMonoLike, ISceneController
	{
		// Binder
		HashBinder mBinder = new HashBinder();
		public HashBinder Binder { get {return mBinder; } }


		/// <summary>
		/// Accept the notification for Views.
		/// </summary>
		/// <param name="msg_type">Msg_type.</param>
		/// <param name="data">Data.</param>
		public void OnNotification (ESceneNoification msg_type, params object[] data)
		{
			SpawnDataSource ds = mBinder.Get<SpawnDataSource>();

			switch (msg_type)
			{
                case ESceneNoification.SceneBegin:
                    if (PeGameMgr.IsSingle)
                        CreateAgents(ds);
                    break;
                case ESceneNoification.CreateAgent:
                    {
                        if (data.Length == 0)
                        {
                            Debug.LogError("Create Agent notification parameters error");
                            break;
                        }

                        SpawnPoint sp = data[0] as SpawnPoint;

                        CreateAgent(ds, sp);
                    }
                    break;

                #region SPAWNPOINT_CASE
                case ESceneNoification.RemoveSpawnPoint:
                    {
                        if (data.Length == 0)
                        {
                            Debug.LogError("Remove SpawnPoint notification parameters error");
                            break;
                        }

                        SpawnPoint sp = data[0] as SpawnPoint;
                        // Monster
                        if (sp as MonsterSpawnPoint != null)
                        {
                            MonsterSpawnPoint msp = sp as MonsterSpawnPoint;

                            // Destroy Entity First
                            if (msp.EntityID != -1)
                                CreatureMgr.Instance.Destory(sp.EntityID);
                            
                            // Remove Agent
                            if (msp.agent != null)
                            {
                                SceneMan.RemoveSceneObj(msp.agent);
                            }

                            // Remove Spawn Point
                            ds.RemoveMonster(msp.ID);
                        }
                        // Npc
                        else if (sp as NPCSpawnPoint != null)
                        {
                            NPCSpawnPoint nsp = sp as NPCSpawnPoint;

                            // Destroy Entity First
                            if (nsp.EntityID != -1)
                                CreatureMgr.Instance.Destory(sp.EntityID);

                            if (nsp.agent != null)
                                SceneMan.RemoveSceneObj(nsp.agent);

                            ds.RemoveMonster(nsp.ID);
                        }
                        // Doodad
                        else if (sp as DoodadSpawnPoint != null)
                        {
                            DoodadSpawnPoint dsp = sp as DoodadSpawnPoint;

                            if (dsp.EntityID != -1)
                                CreatureMgr.Instance.Destory(sp.EntityID);

                            if (dsp.agent != null)
                                SceneMan.RemoveSceneObj(dsp.agent);

                            ds.RemoveMonster(dsp.ID);
                        }
                        // Item
                        else if (sp as ItemSpwanPoint != null)
                        {
                            ItemSpwanPoint isp = sp as ItemSpwanPoint;

                            List<ISceneObjAgent> agents = SceneMan.GetSceneObjs<DragArticleAgent>();
                            for (int i = 0; i < agents.Count; i++)
                            {
                                DragArticleAgent drag_agent = agents[i] as DragArticleAgent;
                                if (drag_agent != null && drag_agent.itemDrag.itemObj.instanceId == isp.ItemObjId)
                                {
                                    ItemAsset.ItemMgr.Instance.DestroyItem(isp.ItemObjId);
                                    SceneMan.RemoveSceneObj(drag_agent);
                                    break;
                                }
                            }
                        }
                    }
                    break;
                case ESceneNoification.EnableSpawnPoint:
                    {
                        if (data.Length < 1)
                        {
                            Debug.LogError("Enable SpawnPoint notification parameters error");
                            break;
                        }

                        SpawnPoint sp = data[0] as SpawnPoint;
                        bool enable = (bool)data[1];

                        if (sp as MonsterSpawnArea != null)
                        {
                            MonsterSpawnArea area = sp as MonsterSpawnArea;
                            for (int i = 0; i < area.Spawns.Count; i++)
                            {
                                for (int j = 0; j < area.Spawns[i].spawnPoints.Count; j++)
                                {
                                    area.Spawns[i].spawnPoints[j].Enable = enable;
                                }
                            }
                        }
                        sp.Enable = enable;
                    }
                    break;
                #endregion

                #region CREATE_CASE
                case ESceneNoification.CreateMonster:
			    {
				    if (data.Length < 2)
				    {
					    Debug.LogError("The [CreateMonster] notification parameter is wrong");
					    break;
				    }

				    SceneEntityAgent agent = data[0] as SceneEntityAgent;
				    bool save = (bool)data[1];

                    bool need_check = true;
                    if (data.Length > 2)
                        need_check = (bool)data[2];

                    Vector3 pos = agent.spawnPoint.spawnPos;
				    if (!need_check || CheckPos(out pos, pos, agent.spawnPoint, agent.spawnArea))
				    {
					    agent.spawnPoint.spawnPos = pos;

					    // Is Group Root?
					    if (agent.groupPoints != null)
					    {
						    agent.entityGp = CreateMonsterGroup(agent.spawnPoint, agent.groupPoints, agent.spawnArea);
						    agent.entity = agent.entityGp;
                            agent.entity.scenarioId = agent.ScenarioId;

                            break;
					    }
                        
					    agent.entity = CreateMonster(agent.mstPoint, save);
                        agent.entity.scenarioId = agent.ScenarioId;

					    if (agent.entityGp != null)
						    agent.entity.transform.parent = agent.entityGp.transform;

					    Debug.Log("Create the Monster ");
				    }

			    }break;
			    case ESceneNoification.CreateNpc:
			    {
				    if (data.Length == 0)
				    {
					    Debug.LogError("The [CreateNpc] notification parameters are wrong");
					    break;
				    }

				    SceneEntityAgent agent = data[0] as SceneEntityAgent;
                    
                    bool need_check = true;
                    if (data.Length > 1)
                        need_check = (bool)data[1];

				    Vector3 pos = agent.spawnPoint.spawnPos;
				    if (!need_check || CheckPos(out pos, pos, agent.spawnPoint, agent.spawnArea))
				    {

					    agent.spawnPoint.spawnPos = pos;
					    agent.entity = CreateNpc(agent.spawnPoint as NPCSpawnPoint);
                        
					    if (agent.entity == null)
					    {
                            agent.entity.scenarioId = agent.ScenarioId;
                            Debug.LogError("[SceneEntityCreator]Failed to create npc:" + agent.protoId);
					    }
					    else
						    Debug.Log("Create the Npc [" + agent.entity.Id.ToString() + "]");
				    }

			    }break;
			    case ESceneNoification.CreateDoodad:
			    {
				    if (data.Length < 2)
				    {
					    Debug.LogError("The [CreateNpc] notification parameters are wrong");
					    break;
				    }

				    SceneStaticAgent agent = data[0] as SceneStaticAgent;
				    //bool is_save = (bool)data[1];

				    agent.entity = CreadteDoodad(agent.spawnPoint as DoodadSpawnPoint, agent.IsSave);
                    agent.entity.scenarioId = agent.ScenarioId;
                }
                break;
			    #endregion
                
                
			    #region DEAD_CASE
			    case ESceneNoification.MonsterDead:
			    {
				    if (data.Length == 0)
				    {
					    Debug.LogError("The [MonsterDead] notification parameters are wrong ");
					    break;
				    }

				    SceneEntityAgent agent = data[0] as SceneEntityAgent;

				    MonsterSpawnPoint msp = agent.mstPoint;
				    if (msp == null)
				    {
					    Debug.LogError("he [MonsterDead] notification : the point is not a MonsterSpawnPoint");
					    break;
				    }


				    msp.isDead = true;
				    msp.EntityID = -1;

				    Debug.Log("The monster [" + agent.entity.Id.ToString() + "] is Dead" );
				    agent.entity = null;

				    if (agent.spawnArea != null)
				    {
					    if (agent.spawnArea.MaxRespawnCount != 0)
					    {
						    AddMstDeadAgent(agent);
					    }
				    }
				    else if (msp.MaxRespawnCount != 0)
				    {
					    AddMstDeadAgent(agent);
				    }

			    }break;
                case ESceneNoification.DoodadDead:
                {
                        if (data.Length == 0)
                        {
                            Debug.LogError("The [DoodadDead] notification parameters are wrong ");
                            break;
                        }

                        SceneStaticAgent agent = data[0] as SceneStaticAgent;

                        DoodadSpawnPoint dsp = agent.spawnPoint as DoodadSpawnPoint;
                        if (dsp == null)
                        {
                            Debug.LogError("he [DoodadDead] notification : the point is not a DoodadSpawnPoint");
                            break;
                        }

                        dsp.isDead = true;
                        dsp.EntityID = -1;
                }
                break;
			    #endregion
			    case ESceneNoification.EntityDestroy:
			    {
				    if (data.Length < 2)
				    {
					    Debug.LogError("The [EntityDestroy] notification parameters are wrong ");
					    break;
				    }

				    SpawnPoint sp = data[0] as SpawnPoint;
                    PeEntity entity = data[1] as PeEntity;

                    bool remove_data = false;
                    if (data.Length > 2)
                        remove_data = (bool)data[2];

                     if (remove_data)
                     {
                         entity.Export();
                         CreatureMgr.Instance.Destory(sp.EntityID);
                         sp.EntityID = -1;
                     }
                     else
                     {
                        CreatureMgr.Instance.DestroyAndDontRemove(sp.EntityID);
                     }

			    }break;
			}
		}

		// Monster Dead Agents List. 
		List<SceneEntityAgent> mMstDeadAgents = new List<SceneEntityAgent>(10);

		public void AddMstDeadAgent(SceneEntityAgent agent)
		{
			MonsterSpawnPoint msp = agent.mstPoint;

			if (msp != null)
			{
				if (agent.spawnArea != null)
				{
					if (agent.spawnArea.MaxRespawnCount != 0)
					{
						mMstDeadAgents.Add(agent);
					}
				}
				else if (agent.mstPoint.MaxRespawnCount != 0)
				{
					mMstDeadAgents.Add(agent);
				}
			}
		}


		void DestroyEntity()
		{

        }
		#region MONOLIKE_FUNC

        public void OnGUI()
        {

        }

		public void Start ()
		{

		}

		public void Update ()
		{
            // Entity Dead
			for (int i = mMstDeadAgents.Count - 1; i >= 0; i--)
			{
				SceneEntityAgent agent = mMstDeadAgents[i];
				MonsterSpawnPoint msp = agent.mstPoint;
				if (msp.UpdateRespawnTime(Time.deltaTime))
				{
					if (agent.spawnArea != null)
					{
						if (agent.spawnArea.MaxRespawnCount != 0)
							agent.spawnArea.MaxRespawnCount--;
						else
						{
							mMstDeadAgents.RemoveAt(i);
							continue;
						}
					}
					else
					{
						if (msp.MaxRespawnCount != 0)
							msp.MaxRespawnCount--;
						else
						{
							mMstDeadAgents.RemoveAt(i);
							continue;
						}
					}

					msp.isDead = false;
					agent.Respawn();
					Debug.Log("The Agent [" + agent.Id.ToString() + "] is respawned");
					mMstDeadAgents.RemoveAt(i);
				}
			}
		}

		public void OnDestroy()
		{

		}
		#endregion




		#region CHECK_FUNC

		public static bool CheckPos(out Vector3 outPutPos, Vector3 curPos, SpawnPoint point, MonsterSpawnArea area)
		{
			outPutPos = curPos;
			MonsterProtoDb.Item protoItem =  MonsterProtoDb.Get(point.Prototype);

			bool pass = false;
			if (protoItem.movementField == MovementField.Land)
			{
				pass = CheckOnLand(out outPutPos, curPos, point, area);
			}
			else if (protoItem.movementField == MovementField.Sky)
			{
				pass = CheckOnSky(out outPutPos, curPos, point, area);
			}
			else if (protoItem.movementField == MovementField.water)
			{
				pass = CheckInWater(out outPutPos, curPos, point, area);
			}
			else if (protoItem.movementField == MovementField.Amphibian)
			{
				pass = CheckOnLand(out outPutPos, curPos, point, area);
				if (!pass)
					pass = CheckInWater(out outPutPos, curPos, point, area);
			}
			else if (protoItem.movementField == MovementField.All)
			{
				pass = CheckOnLand(out outPutPos, curPos, point, area);
				if (!pass)
					pass = CheckOnSky(out outPutPos, curPos, point, area);
				
				if (!pass)
					pass = CheckInWater(out outPutPos, curPos, point, area);
			}

			return pass;
		}

		static bool CheckOnLand (out Vector3 outPutPos, Vector3 curPos, SpawnPoint point, MonsterSpawnArea area)
		{
			outPutPos = curPos;
			
			MonsterSpawnPoint msp = point as MonsterSpawnPoint;
			
			if (msp == null ||  area == null )
			{
				int h = 1000;
				RaycastHit[] rchs  = Physics.RaycastAll(new Vector3(curPos.x, curPos.y + h, curPos.z), Vector3.down, h * 2, SceneMan.DependenceLayer);
				
				if (rchs != null && rchs.Length != 0)
				{
					float dis = (curPos - rchs[0].point).sqrMagnitude;
					RaycastHit rch =  rchs[0];
					for (int i = 1; i < rchs.Length; i++)
					{
                        float _dis = (curPos - rchs[i].point).sqrMagnitude;

                        if (dis > _dis)
						{
							rch = rchs[i];
							//dis = dis;
						}
					}
					
					
					outPutPos = rch.point;
					outPutPos.y += 1;
					
					
					return true;
					
				}
			}
			else
			{
				Vector3[] area_bounds_v = new Vector3[8];
				area_bounds_v[0] = area.spawnPos + area.Rotation *  new Vector3(area.Scale.x, area.Scale.y, area.Scale.z) * 0.5f;
				area_bounds_v[1] = area.spawnPos + area.Rotation *  new Vector3(-area.Scale.x, area.Scale.y, area.Scale.z) * 0.5f;
				area_bounds_v[2] = area.spawnPos + area.Rotation *  new Vector3(area.Scale.x, -area.Scale.y, area.Scale.z) * 0.5f;
				area_bounds_v[3] = area.spawnPos + area.Rotation *  new Vector3(-area.Scale.x, -area.Scale.y, area.Scale.z) * 0.5f;
				area_bounds_v[4] = area.spawnPos + area.Rotation *  new Vector3(area.Scale.x, area.Scale.y, -area.Scale.z) * 0.5f;
				area_bounds_v[5] = area.spawnPos + area.Rotation *  new Vector3(-area.Scale.x, area.Scale.y, -area.Scale.z) * 0.5f;
				area_bounds_v[6] = area.spawnPos + area.Rotation *  new Vector3(area.Scale.x, -area.Scale.y, -area.Scale.z) * 0.5f;
				area_bounds_v[7] = area.spawnPos + area.Rotation *  new Vector3(-area.Scale.x, -area.Scale.y, -area.Scale.z) * 0.5f;
				
				float[] area_y = new float[8];
				for (int i = 0; i < 8; i++)
					area_y[i] = area_bounds_v[i].y;
				
				float area_y_min = Mathf.Min(area_y) - 4;
				float area_y_max = Mathf.Max(area_y) + 4;
				
				RaycastHit[] rchs = Physics.RaycastAll(new Vector3(curPos.x, area_y_max, curPos.z), Vector3.down, area_y_max - area_y_min, SceneMan.DependenceLayer);
				
				if (rchs != null && rchs.Length != 0)
				{
					float dis = 100000;
					for (int i = 0; i < rchs.Length; i++)
					{
						if (dis > rchs[i].distance )
						{
							Vector3 _pos =  area.spawnPos + Quaternion.Inverse(area.Rotation) * (rchs[i].point - area.spawnPos);
							
							if ( msp.bound.Contains(_pos))
							{
								outPutPos = rchs[i].point;
								dis = rchs[i].distance;
							}
							else
							{
								if (outPutPos.y > rchs[i].point.y)
									outPutPos.y = rchs[i].point.y + 0.5f;
								dis = rchs[i].distance;
							}
						}
					}
					
					
					return true;
				}
				
				
			}
			
			return false;
			
		}

		static bool CheckInWater (out Vector3 outPutPos, Vector3 curPos, SpawnPoint point, MonsterSpawnArea area)
		{
			outPutPos = curPos;
			
			if ( PETools.PE.PointInWater(curPos) > 0.52f && PETools.PE.PointInTerrain(curPos) < 0.52f)
			{
				return true;
			}
			
			MonsterSpawnPoint msp = point as MonsterSpawnPoint;
			
			if (msp == null ||  area == null )
			{
				int ofs = 5;
				Vector3 pos = new Vector3(curPos.x, curPos.y - ofs, curPos.z);
				
				for (int i = 0; i < ofs * 2; i++)
				{
					pos.y += 1;
					
					if (PETools.PE.PointInWater(pos) > 0.52f && PETools.PE.PointInTerrain(pos) < 0.52f)
					{
						outPutPos = pos;
						return true;
					}
				}
			}
			else
			{
				int xmin = Mathf.FloorToInt( msp.bound.center.x - msp.bound.extents.x);
				int xmax = Mathf.FloorToInt( msp.bound.center.x + msp.bound.extents.x);
				int ymin = Mathf.FloorToInt( msp.bound.center.y - msp.bound.extents.y);
				int ymax = Mathf.FloorToInt( msp.bound.center.y + msp.bound.extents.y);
				int zmin = Mathf.FloorToInt( msp.bound.center.z - msp.bound.extents.z);
				int zmax = Mathf.FloorToInt( msp.bound.center.z + msp.bound.extents.z);
				
				int cnt = 50;
				while (cnt > 0)
				{
					cnt--;
					
					Vector3 pos = new Vector3( Random.Range(xmin, xmax),
					                          Random.Range(ymin, ymax),
					                          Random.Range(zmin, zmax));
					
					pos = area.spawnPos + area.Rotation * (pos - area.spawnPos);
					
					if ( PETools.PE.PointInWater(pos) > 0.52f)
					{
						outPutPos = pos;
						return true;
					}
				}
			}
			
			return false;
		}


		static bool CheckOnSky(out Vector3 outPutPos, Vector3 curPos, SpawnPoint point, MonsterSpawnArea area)
		{
			outPutPos = curPos;

			if (PETools.PE.PointInTerrain(outPutPos) < 0.52f && PETools.PE.PointInWater(outPutPos) < 0.52f)
			{
				return true;
			}
			
			MonsterSpawnPoint msp = point as MonsterSpawnPoint;
			
			if (msp == null ||  area == null )
			{
				int ofs = 5;
				Vector3 pos = new Vector3(curPos.x, curPos.y + ofs, curPos.z);
				
				for (int i = 0; i < ofs * 2; i++)
				{
					pos.y -= 1;
					
					if (PETools.PE.PointInTerrain(pos) < 0.52f && PETools.PE.PointInWater(outPutPos) < 0.52f)
					{
						outPutPos = pos;
						return true;
					}
				}
			}
			else
			{
				
				Vector3[] v = new Vector3[8];
				Vector3 center = msp.bound.center;
				Vector3 ext = msp.bound.size;
				v[0] = area.spawnPos + area.Rotation * (area.spawnPos - (center + new Vector3(ext.x, ext.y, ext.z)));
				v[1] = area.spawnPos + area.Rotation * (area.spawnPos - (center + new Vector3(-ext.x, ext.y, ext.z)));
				v[2] = area.spawnPos + area.Rotation * (area.spawnPos - (center + new Vector3(ext.x, -ext.y, ext.z)));
				v[3] = area.spawnPos + area.Rotation * (area.spawnPos - (center + new Vector3(-ext.x, -ext.y, ext.z)));
				v[4] = area.spawnPos + area.Rotation * (area.spawnPos - (center + new Vector3(ext.x, ext.y, -ext.z)));
				v[5] = area.spawnPos + area.Rotation * (area.spawnPos - (center + new Vector3(-ext.x, ext.y, -ext.z)));
				v[6] = area.spawnPos + area.Rotation * (area.spawnPos - (center + new Vector3(ext.x, -ext.y, -ext.z)));
				v[7] = area.spawnPos + area.Rotation * (area.spawnPos - (center + new Vector3(-ext.x, -ext.y, -ext.z)));
				
				float[] yArr = new float[8];
				for (int i = 0; i < 8; i++)
					yArr[i] = v[i].y;
				
				float yMin = Mathf.Min(yArr);
				float yMax = Mathf.Max(yArr);
				
				//RaycastHit rch;
				
				int step = 2;
				for (float h = yMax; h > yMin - 1; h  -= step)
				{
					Vector3 pos = new Vector3(outPutPos.x, h, outPutPos.z);
					if (PETools.PE.PointInTerrain(pos) < 0.52f 
					    && PETools.PE.PointInWater(pos) < 0.52f
					    && area.PointIn(pos))
					{
						outPutPos = pos;
						return true;
					}
				}
				
			}
			
			
			
			return false;
		}

		#endregion

	}
}