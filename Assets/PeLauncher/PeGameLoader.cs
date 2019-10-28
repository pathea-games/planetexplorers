using System.Collections.Generic;
using UnityEngine;

using Pathea.PeEntityExt;
using Pathea.PeEntityExtTrans;

namespace Pathea
{
    namespace GameLoader
    {
        abstract class ModuleLoader : PeLauncher.ILaunchable
        {
            bool mNew;

            public ModuleLoader(bool bNew)
            {
                mNew = bNew;
            }

            void PeLauncher.ILaunchable.Launch()
            {
                if (mNew)
                {
                    New();
                }
                else
                {
                    Restore();
                }
            }

            protected abstract void New();
            protected abstract void Restore();
        }

        class LoadDoodadShow : PeLauncher.ILaunchable
        {
            void PeLauncher.ILaunchable.Launch()
            {
                //ChangeDoodadShowVar(242, false);
                //ChangeDoodadShowVar(240, false);
                //ChangeDoodadShowVar(324, true);
                //ChangeDoodadShowVar(326, false);
                //ChangeDoodadShowVar(327, false);

                //for (int i = 461; i < 464; i++)
                //    ChangeDoodadShowVar(i, true);

                //for (int i = 456; i < 461; i++)
                //    ChangeDoodadShowVar(i, false);

                for (int i = 335; i < 342; i++)
                    ChangeDoodadShowVar(i, false);
            }

            private void ChangeDoodadShowVar(int n, bool tmp)
            {
                SceneDoodadLodCmpt doodadCmpt;
                PeEntity[] doodad = EntityMgr.Instance.GetDoodadEntities(n);
                if (doodad.Length > 0)
                {
                    doodadCmpt = doodad[0].GetComponent<SceneDoodadLodCmpt>();
                    if (doodadCmpt != null)
                        doodadCmpt.IsShown = tmp;

                    //doodadCmpt.SetShowVar(tmp);
                }
            }
        }

        class LoadSpecialScene : PeLauncher.ILaunchable
        {
            SingleGameStory.StoryScene mType = SingleGameStory.StoryScene.MainLand;
            public LoadSpecialScene(SingleGameStory.StoryScene type = SingleGameStory.StoryScene.MainLand) { mType = type; }
            void PeLauncher.ILaunchable.Launch()
            {
                if (mType == SingleGameStory.StoryScene.DienShip0)
                {
                    UnityEngine.Object obj = Resources.Load("Prefab/Other/DienShip");
                    if (obj == null)
                        return;
                    GameObject ship = Object.Instantiate(obj) as GameObject;
                    ship.transform.position = new Vector3(14798f, 3f, 8344f);
                }
                else if (mType == SingleGameStory.StoryScene.DienShip1)
                {
                    UnityEngine.Object obj = Resources.Load("Prefab/Other/DienShip");
                    if (obj == null)
                        return;
                    GameObject ship = Object.Instantiate(obj) as GameObject;
					ship.transform.position = new Vector3(16545.25f, 3.93f, 10645.7f);
                }
                else if (mType == SingleGameStory.StoryScene.DienShip2)
                {
                    UnityEngine.Object obj = Resources.Load("Prefab/Other/DienShip");
                    if (obj == null)
                        return;
                    GameObject ship = Object.Instantiate(obj) as GameObject;
                    ship.transform.position = new Vector3(2876f, 265.6f, 9750.3f);
                }
                else if (mType == SingleGameStory.StoryScene.DienShip3)
                {
                    UnityEngine.Object obj = Resources.Load("Prefab/Other/DienShip");
                    if (obj == null)
                        return;
                    GameObject ship = Object.Instantiate(obj) as GameObject;
                    ship.transform.position = new Vector3(13765.5f, 75.7f, 15242.7f);
                }
                else if (mType == SingleGameStory.StoryScene.DienShip4)
                {
                    UnityEngine.Object obj = Resources.Load("Prefab/Other/DienShip");
                    if (obj == null)
                        return;
                    GameObject ship = Object.Instantiate(obj) as GameObject;
                    ship.transform.position = new Vector3(12547.7f, 523.7f, 13485.5f);
                }
                else if (mType == SingleGameStory.StoryScene.DienShip5)
                {
                    UnityEngine.Object obj = Resources.Load("Prefab/Other/DienShip");
                    if (obj == null)
                        return;
                    GameObject ship = Object.Instantiate(obj) as GameObject;
                    ship.transform.position = new Vector3(7750.4f, 349.7f, 14712.8f);
                }
                else if (mType == SingleGameStory.StoryScene.L1Ship)
                {
                    UnityEngine.Object obj = Resources.Load("Prefab/Other/old_scene_boatinside");
                    if (obj == null)
                        return;
                    GameObject ship = Object.Instantiate(obj) as GameObject;
                    ship.transform.position = new Vector3(9661f, 88.8f, 12758f);
                }
                else if (mType == SingleGameStory.StoryScene.PajaShip)
                {
                    //paja_port_shipinside
                    UnityEngine.Object obj = Resources.Load("Prefab/Other/paja_port_shipinside");
                    if (obj == null)
                        return;
                    GameObject ship = Object.Instantiate(obj) as GameObject;
                    ship.transform.position = new Vector3(1471f, 101.3f, 7928.3f);
                    Quaternion q = Quaternion.identity;
                    q.eulerAngles = new Vector3(352, 55, 0);
                    ship.transform.rotation = q;
                }
                else if (mType == SingleGameStory.StoryScene.LaunchCenter)
                {
                    UnityEngine.Object obj = Resources.Load("Prefab/Other/paja_launch_center");
                    GameObject ship = Object.Instantiate(obj) as GameObject;
                    ship.transform.position = new Vector3(1713, 140, 10402);
                    Quaternion q = Quaternion.identity;
                    q.eulerAngles = new Vector3(0, 180, 0);
                    ship.transform.rotation = q;
                }
            }
        }

		class LoadRandomDungeon : PeLauncher.ILaunchable
		{
			void PeLauncher.ILaunchable.Launch()
			{
				UnityEngine.Object obj = Resources.Load("Prefab/RandomDunGen/RandomDungenMgr");
				Object.Instantiate(obj);
//				GameObject go = new GameObject("RandomDunGenMgr");
//				go.AddComponent<RandomDunGenMgr>();        
				RandomDungenMgr.Instance.Init();
				if(PeGameMgr.yirdName==AdventureScene.Dungen.ToString()){
					while(!RandomDungenMgr.Instance.GenDungeon()){ }
                    RandomDungenMgr.Instance.LoadPathFinding(); 
				}else{
					RandomDungenMgr.Instance.CreateInitTaskEntrance();
				}
				RandomDungenMgrData.initTaskEntrance.Clear();
			}
		}


        class ResetGlobalData : PeLauncher.ILaunchable
        {
            void PeLauncher.ILaunchable.Launch()
            {
				AIErodeMap.ResetErodeData();
                PeGameSummary.Mgr.Instance.Init();
				DigTerrainManager.ClearBlockInfo();

                //lz-2016.09.01 重置技能树可使用状态
                if (!Pathea.PeGameMgr.IsAdventure)
                {
                    RandomMapConfig.useSkillTree = false;
                    RandomMapConfig.openAllScripts = false;
                }
            }
        }

        //class StartUpdateSingleton : PeLauncher.ILaunchable
        //{
        //    void PeLauncher.ILaunchable.Launch()
        //    {
        //        PeSingletonMgr.Instance.Start();
        //    }
        //}

        class LoadCamera : PeLauncher.ILaunchable
        {
            void PeLauncher.ILaunchable.Launch()
            {
                Object.Instantiate(Resources.Load<GameObject>("Prefab/GameUI/MinmapCamera")).name = "MinmapCamera";
				Camera.main.gameObject.AddComponent<GlobalGLs>();
				Camera.main.transform.position = Pathea.PlayerSpawnPosProvider.Instance.GetPos();
				PeCamera.SetGlobalVector("Default Anchor", Pathea.PlayerSpawnPosProvider.Instance.GetPos());
				PeCamera.RecordHistory();
			}
		}
		
		class LoadEnvironment : PeLauncher.ILaunchable
        {
            void PeLauncher.ILaunchable.Launch()
            {
                PeEnv.Init();
            }
        }

        class LoadMusicStroy : PeLauncher.ILaunchable
        {
            public void Launch()
            {
                GameObject obj = Resources.Load("Prefab/Audio/bg_music_story") as GameObject;
                if (obj != null)
                {
                    GameObject.Instantiate(obj);
                }
            }
        }

        class LoadMonsterSiegeCamp : PeLauncher.ILaunchable
        {
            public void Launch()
            {
                //GameObject.Instantiate(Resources.Load("Prefab/MonsterSiege_Camp"));
            }
        }

        class LoadMusicAdventure : PeLauncher.ILaunchable
        {
            public void Launch()
            {
                GameObject obj = Resources.Load("Prefab/Audio/bg_music_adventure") as GameObject;
                if (obj != null)
                {
                    GameObject.Instantiate(obj);
                }
            }
        }

        class LoadMusicBuild : PeLauncher.ILaunchable
        {
            public void Launch()
            {
                GameObject obj = Resources.Load("Prefab/Audio/bg_music_build") as GameObject;
                if (obj != null)
                {
                    GameObject.Instantiate(obj);
                }
            }
        }

        //class LoadCustomCamera : LoadCamera
        //{
        //    Vector3 mPos;

        //    public LoadCustomCamera(Vector3 pos)
        //    {
        //        mPos = pos;
        //    }

        //    protected override Vector3 Position()
        //    {
        //        return mPos;
        //    }
        //}

        class LoadGameGraphInfo : PeLauncher.ILaunchable
        {
            Vector2 mSize;

            public LoadGameGraphInfo(Vector2 size)
            {
                mSize = size;
            }

            void PeLauncher.ILaunchable.Launch()
            {
                PeMappingMgr.Instance.Init(mSize);
            }
        }

        class LoadGUI : PeLauncher.ILaunchable
        {
            void PeLauncher.ILaunchable.Launch()
            {
                Load(); 
            }

            void Load()
            {
                GameObject objTerrain = Object.Instantiate(Resources.Load<GameObject>("Prefab/GameUISystem")) as GameObject;

                objTerrain.transform.position = Vector3.zero;
                objTerrain.transform.rotation = Quaternion.identity;
                objTerrain.transform.localScale = Vector3.one;
            }
        }

		class LoadWorldCollider : PeLauncher.ILaunchable
		{
			void PeLauncher.ILaunchable.Launch()
			{
				Load();
			}
			
			void Load()
			{
				GameObject.Instantiate(Resources.Load<GameObject>("Prefab/Other/WorldColliders"));
			}
		}

        abstract class LoadMap : ModuleLoader
        {
            public LoadMap(bool bNew) : base(bNew) { }

            void LoadMapRunner()
            {
                new GameObject("MapRunner", typeof(PeMap.MapRunner));
            }

            protected override void New()
            {
                LoadMapRunner();

                PeMap.StaticPoint.Mgr.Instance.New();
                PeMap.UserLabel.Mgr.Instance.New();
                PeMap.MaskTile.Mgr.Instance.New();
				PeMap.TowerMark.Mgr.Instance.New();
            }

            protected override void Restore()
            {
                LoadMapRunner();

                PeMap.StaticPoint.Mgr.Instance.Restore();
                PeMap.UserLabel.Mgr.Instance.Restore();
                PeMap.MaskTile.Mgr.Instance.Restore();
				ReviveLabel.Mgr.Instance.Restore();
				PeMap.TowerMark.Mgr.Instance.Restore();
            }
        }

        class LoadCustomMap : LoadMap
        {
            public LoadCustomMap(bool bNew) : base(bNew) { }

            protected override void New()
            {
                base.New();

                PeMap.MapRunner.UpdateTile(false);
            }

            protected override void Restore()
            {
                base.Restore();

                PeMap.MapRunner.UpdateTile(false);
            }
        }

        class LoadStoryMap : LoadMap
        {
            public LoadStoryMap(bool bNew) : base(bNew) { }

            protected override void New()
            {
                DetectedTownMgr.Instance.RegistAtFirst();

                base.New();
                PeMap.StoryStaticPoint.Load();

                PeMap.MapRunner.UpdateTile(false);
            }

            protected override void Restore()
            {
                DetectedTownMgr.Instance.RegistAtFirst();

                base.Restore();

                PeMap.MapRunner.UpdateTile(false);
            }
        }

        //need update tile
        class LoadRandomMap : LoadMap
        {
            public LoadRandomMap(bool bNew) : base(bNew) { }

            protected override void New()
            {
                base.New();
                PeMap.MapRunner.UpdateTile(true);
            }

            protected override void Restore()
            {
                base.Restore();
                PeMap.MapRunner.UpdateTile(true);
            }
        }

        abstract class LoadGrass : ModuleLoader
        {
            public LoadGrass(bool bNew) : base(bNew) { }

            protected override void New()
            {
                GrassDataSLArchiveMgr.Instance.New();
                Load();
            }

            protected override void Restore()
            {
                GrassDataSLArchiveMgr.Instance.Restore();
                Load();
            }

            void Load()
            {
                string grassPrefabName = GetGrassPrefabName();

                GameObject objGrass = Object.Instantiate(Resources.Load<GameObject>(grassPrefabName)) as GameObject;

                if (null == objGrass)
                {
                    Debug.LogError("can't find grass prefab:" + grassPrefabName);
                    return;
                }

//                GrassMgr.Instance.m_CamTrans = Camera.main.transform;
            }

            protected abstract string GetGrassPrefabName();
        }

        class LoadGrassRandom : LoadGrass
        {
            public LoadGrassRandom(bool bNew) : base(bNew) { }

            protected override string GetGrassPrefabName()
            {
//                return "GrassRandomMgr";
				return "PE Random Grass System";
            }
        }

        class LoadEditedGrass : LoadGrass
        {
            public LoadEditedGrass(bool bNew, string dataDir)
                : base(bNew)
            {
//                GrassStoryDataMgr.originalSubTerrainDir = dataDir;
				PeGrassDataIO_Story.originalSubTerrainDir = dataDir;
            }

            protected override string GetGrassPrefabName()
            {
//                return "GrassManager";
				return "PE Grass System";
            }
        }

		class LoadVETreeProtos : ModuleLoader
		{
			public LoadVETreeProtos(bool bNew)
				: base(bNew)
			{}
			
			void Load()
			{
				string prefabName = "VETreeProtos";
				
				GameObject objTreeProtos = Object.Instantiate(Resources.Load<GameObject>(prefabName)) as GameObject;				
				if (null == objTreeProtos)
				{
					Debug.LogError("can't find tree protos prefab:" + prefabName);
					return;
				}
			}
			
			protected override void New()
			{
				Load();
			}
			
			protected override void Restore()
			{
				Load ();
			}
		}

        class LoadEditedTree : ModuleLoader
        {
            public LoadEditedTree(bool bNew, string dir)
                : base(bNew)
            {
                LSubTerrIO.OriginalSubTerrainDir = dir;
            }

            void Load()
            {
                string prefabName = "Layered SubTerrain Group";

                GameObject objGrass = Object.Instantiate(Resources.Load<GameObject>(prefabName)) as GameObject;

                if (null == objGrass)
                {
                    Debug.LogError("can't find tree prefab:" + prefabName);
                    return;
                }

                LSubTerrainMgr.Instance.CameraTransform = Camera.main.transform;
				LSubTerrainMgr.Instance.VEditor = VoxelEditor.Get();
                LSubTerrSL.Init();
            }

            protected override void New()
            {
                Load();
                LSubTerrSLArchiveMgr.Instance.New();
            }

            protected override void Restore()
            {
                Load();
                LSubTerrSLArchiveMgr.Instance.Restore();
            }
        }

        class LoadRandomTree : ModuleLoader
        {
            public LoadRandomTree(bool bNew) : base(bNew) { }
            void Load()
            {
                string prefabName = "Layered Random SubTerrain Group";

                GameObject objGrass = Object.Instantiate(Resources.Load<GameObject>(prefabName)) as GameObject;

                if (null == objGrass)
                {
                    Debug.LogError("can't find tree prefab:" + prefabName);
                    return;
                }

                RSubTerrainMgr.Instance.CameraTransform = Camera.main.transform;
				RSubTerrainMgr.Instance.VEditor = VoxelEditor.Get ();
                RSubTerrSL.Init();
            }

            protected override void New()
            {
                Load();
                RSubTerrSLArchiveMgr.Instance.New();
            }

            protected override void Restore()
            {
                Load();
                RSubTerrSLArchiveMgr.Instance.Restore();
            }
        }

        class LoadPathFinding : PeLauncher.ILaunchable
        {
			const string dataPathStory = "Prefab/Pathfinder";		// terrain's grid graph with story cave's point graph
			const string dataPathStd = "Prefab/PathfinderStd";	// terrain's grid graph

            void DestoryOldPathFinder()
            {
                if(AstarPath.active != null)
                {
                    if (AstarPath.active.transform.parent != null)
                        GameObject.Destroy(AstarPath.active.transform.parent.gameObject);
                    else
                        GameObject.Destroy(AstarPath.active.gameObject);
                }
            }

            void PeLauncher.ILaunchable.Launch()
            {
                DestoryOldPathFinder();

				string dataPath = PeGameMgr.IsStory ? dataPathStory : dataPathStd;
                GameObject obj = Resources.Load(dataPath) as GameObject;
                if (obj != null)
                {
					//System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
					//sw.Reset();
					//sw.Start();
					AstarPath.SkipOptScanOnStartUp = true;
                    GameObject.Instantiate(obj);
					AstarPath.SkipOptScanOnStartUp = false;
					//sw.Stop();
					//Debug.LogError("Pathfinder Inst takes "+sw.ElapsedMilliseconds);

					//sw.Reset();
					//sw.Start();
					if(AstarPath.active != null && AstarPath.active.graphs.Length > 1){
						AstarPath.active.Scan(0x02);
					}
					//sw.Stop();
					//Debug.LogError("Scan02 Inst takes "+sw.ElapsedMilliseconds);
                }
            }
        }
		class LoadPathFindingEx : PeLauncher.ILaunchable
		{
			void PeLauncher.ILaunchable.Launch()
			{
				//System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
				//sw.Reset();
				//sw.Start();
				if (AstarPath.active != null)
				{
					AstarPath.active.Scan(0xfd);
				}
				//sw.Stop();
				//Debug.LogError("Scan02 Inst takes "+sw.ElapsedMilliseconds);
			}
		}

        abstract class LoadTerrain : ModuleLoader
        {
            public LoadTerrain(bool bNew) : base(bNew) { }

            protected override void New()
            {
                Load();

                VoxelTerrainArchiveMgr.Instance.New();
                SceneMan.self.New();
            }
            protected override void Restore()
            {
                Load();

                VoxelTerrainArchiveMgr.Instance.Restore();
                SceneMan.self.Restore();
            }

            protected virtual void Load()
            {
                Object.Instantiate(Resources.Load<GameObject>("Prefab/WindZone"));
                GameObject objTerrain = Object.Instantiate(Resources.Load<GameObject>("SceneManager")) as GameObject;

                objTerrain.transform.position = Vector3.zero;
                objTerrain.transform.rotation = Quaternion.identity;
                objTerrain.transform.localScale = Vector3.one;
            }
        }

        class LoadEditedTerrain : LoadTerrain
        {
            public LoadEditedTerrain(bool bNew, string path)
                : base(bNew)
            {
                VFVoxelTerrain.MapDataPath_Zip = path;
            }

            protected override void Load()
            {
                //0=128M,1=256M,2=512M,3=1KM. 8.8 reverted
				SceneMan.MaxLod = ((int)SystemSettingData.Instance.TerrainLevel);
				PeGrassSystem.Refresh(SystemSettingData.Instance.GrassDensity, (int)SystemSettingData.Instance.GrassLod);
                base.Load();
            }
        }

        abstract class LoadRandomTerrain : LoadTerrain
        {
            public LoadRandomTerrain(bool bNew) : base(bNew) { }

            protected override void Load()
            {
                SceneMan.MaxLod = (int)SystemSettingData.Instance.RandomTerrainLevel;
				PeGrassSystem.Refresh(SystemSettingData.Instance.GrassDensity, (int)SystemSettingData.Instance.GrassLod);
                base.Load();
            }
        }

        class LoadRandomTerrainWithTown : LoadRandomTerrain
        {
            public LoadRandomTerrainWithTown(bool bNew) : base(bNew) { }

            protected override void Load()
            {
                VFDataRTGen.townAvailable = true;
                base.Load();
            }

            void LoadSupport()
            {
                Object.Instantiate(Resources.Load<GameObject>("RandomTerrainSupport")).name = "RandomTerrainSupport";
            }

            void NewSupport()
            {
                LoadSupport();
                VArtifactTownArchiveMgr.Instance.New();
                TownNpcArchiveMgr.Instance.New();
                VABuildingArchiveMgr.Instance.New();
            }

            void RestoreSupport()
            {
                LoadSupport();
                VArtifactTownArchiveMgr.Instance.Restore();
                TownNpcArchiveMgr.Instance.Restore();
                VABuildingArchiveMgr.Instance.Restore();
            }

            protected override void New()
            {
                NewSupport();

                base.New();
            }

            protected override void Restore()
            {
                RestoreSupport();

                base.Restore();
            }
        }

        class LoadRandomTerrainWithoutTown : LoadRandomTerrain
        {
            public LoadRandomTerrainWithoutTown(bool bNew) : base(bNew) { }

            protected override void Load()
            {
                VFDataRTGen.townAvailable = false;
                base.Load();
            }
        }

        class LoadRandomTown : PeLauncher.ILaunchable
        {
            void PeLauncher.ILaunchable.Launch()
            {
                GameObject go = new GameObject("VArtifactTownManager");
                go.AddComponent<VArtifactTownManager>();             
                VArtifactTownManager.Instance.InitISO();
				if(PeGameMgr.IsSingleAdventure&&PeGameMgr.yirdName==AdventureScene.MainAdventure.ToString())
				{
					Object.Instantiate(Resources.Load("Prefab/MonsterSiege_Town"));
				}
				VArtifactTownManager.Instance.GenTown();
			}
        }

        class LoadRandomItemMgr : PeLauncher.ILaunchable
        {
            void PeLauncher.ILaunchable.Launch()
            {
                GameObject go = new GameObject("RandomItemMgr");
                go.AddComponent<RandomItemMgr>();
            }
        }
        
        class LoadEntityCreator : ModuleLoader
        {
            public LoadEntityCreator(bool bNew)
                : base(bNew)
            {

            }
            void LoadPrefab()
            {
                Object.Instantiate(Resources.Load("Prefab/Mission/EntityCreateMgr"));
            }
            protected override void New()
            {
                LoadPrefab();
                EntityCreateMgr.Instance.New();
				SceneEntityCreator.self.New ();
            }
            protected override void Restore()
            {
                LoadPrefab();
				SceneEntityCreator.self.Restore ();
            }
        }

        class LoadWaveSystem : PeLauncher.ILaunchable
        {
            void LoadPrefab()
            {
                Object.Instantiate(Resources.Load("Prefab/Wave Scene"));
            }

            void PeLauncher.ILaunchable.Launch()
            {
                LoadPrefab();
            }
        }

        class LoadWorldInfo : ModuleLoader
        {
            public LoadWorldInfo(bool bNew) : base(bNew) { }
            protected override void New()
            {
                WorldInfoMgr.Instance.New();
            }

            protected override void Restore()
            {
                WorldInfoMgr.Instance.Restore();
            }
        }

        class LoadCreature : ModuleLoader
        {
            public LoadCreature(bool bNew) : base(bNew) { }

            protected override void New()
            {
                PeCreature.Instance.New();
                MainPlayer.Instance.New();

				Vector3 pos = GetPlayerSpawnPos();
				Debug.Log("player init pos:" + pos);
				MainPlayer.Instance.CreatePlayer(
                                WorldInfoMgr.Instance.FetchRecordAutoId(),
								pos, Quaternion.identity, Vector3.one,
                                CustomCharactor.CustomDataMgr.Instance.Current
                            );
            }

            protected override void Restore()
            {
                PeCreature.Instance.Restore();
                MainPlayer.Instance.Restore();
            }

            Vector3 GetPlayerSpawnPos()
            {
                return Pathea.PlayerSpawnPosProvider.Instance.GetPos();
                //return new Vector3(12231f, 123f, 6097f);
                //return new Vector3(12227f, 121.5f, 6095f);
            }
        }

        //class LoadCustomCreature : LoadCreature
        //{
        //    Vector3 mPos;

        //    public LoadCustomCreature(bool bNew, Vector3 pos)
        //        : base(bNew)
        //    {
        //        mPos = pos;
        //    }

        //    protected override Vector3 GetPlayerSpawnPos()
        //    {
        //        //return new Vector3(1644f, 82.7f, 846f);
        //        return mPos;
        //    }
        //}

        //class LoadRandomCreature : LoadCreature
        //{
        //    public LoadRandomCreature(bool bNew) : base(bNew) { }

        //    protected override Vector3 GetPlayerSpawnPos()
        //    {
        //        if (PeGameMgr.IsSingleAdventure)
        //        {
        //            return VArtifactTownManager.Instance.playerStartPos;
        //        }
        //        IntVector2 posXZ = VArtifactUtil.GetSpawnPos();
        //        Vector3 pos = new Vector3(posXZ.x, VFDataRTGen.GetPosTop(posXZ), posXZ.y);

        //        return pos;
        //    }
        //}

        class LoadMultiCreature : PeLauncher.ILaunchable
        {
            void PeLauncher.ILaunchable.Launch()
            {
				Vector3 pos ;
				if(PeGameMgr.IsMultiStory)
					pos = new Vector3(12227f, 121.5f, 6095f);
				else
				{
					VArtifactUtil.GetSpawnPos();
					//IntVector2 posXZ = new IntVector2(0, 0);
					pos = Pathea.PlayerSpawnPosProvider.Instance.GetPos();
				}
				BaseNetwork.MainPlayer.RequestPlayerLogin(pos);
            }
        }

        class LoadMultiCustomCreature : PeLauncher.ILaunchable
        {
            void PeLauncher.ILaunchable.Launch()
            {
                BaseNetwork.MainPlayer.RequestPlayerLogin(Vector3.zero);
            }
        }

        abstract class LoadScenario : ModuleLoader
        {
            public LoadScenario(bool bNew) : base(bNew) { }

            protected override void New()
            {
                Init();
            }

            protected override void Restore()
            {
                Init();
            }

            protected virtual void Init()
            {
                Object.Instantiate(Resources.Load<GameObject>("Prefab/Mission/MissionManager"));

                ForceSetting.Instance.Load(Resources.Load("ForceSetting/ForceSettings_Story") as TextAsset);
            }
        }

        class LoadForceSetting : ModuleLoader
        {
            public LoadForceSetting(bool bNew) : base(bNew) { }

            protected override void New()
            {
                Init();
            }

            protected override void Restore()
            {
                Init();
            }

            protected virtual void Init()
            {
                ForceSetting.Instance.Load(Resources.Load("ForceSetting/ForceSettings_Story") as TextAsset);
            }
        }

        class LoadSingleStoryInitData : ModuleLoader
        {
            public LoadSingleStoryInitData(bool bNew) : base(bNew) { }

            protected override void New()
            {
                SingleGameInitData.AddStoryInitData();
            }

            protected override void Restore()
            {

            }
        }

        class LoadSingleAdventureInitData : ModuleLoader
        {
            public LoadSingleAdventureInitData(bool bNew) : base(bNew) { }

            protected override void New()
            {
                SingleGameInitData.AddAdventureInitData();
            }

            protected override void Restore()
            {

            }
        }

        class LoadSingleBuildInitData : ModuleLoader
        {
            public LoadSingleBuildInitData(bool bNew) : base(bNew) { }

            protected override void New()
            {
                SingleGameInitData.AddBuildInitData();
            }

            protected override void Restore()
            {

            }
        }

		class LoadSingleGameLevel : ModuleLoader
		{
			public LoadSingleGameLevel(bool bNew) : base(bNew) { }

			protected override void New() {}

			protected override void Restore()
			{                
				PeGameSummary s = PeGameSummary.Mgr.Instance.Get();
				
				if (s != null)
				{
					PeGameMgr.gameLevel = s.gameLevel;
				}
			}
		}

		class LoadTutorialInitData : ModuleLoader
		{
			public LoadTutorialInitData(bool bNew) : base(bNew) { }
			
			protected override void New()
			{
				SingleGameInitData.AddTutorialInitData();
			}
			
			protected override void Restore()
			{
				
			}
		}

        class LoadStory : LoadScenario
        {
            public LoadStory(bool bNew) : base(bNew) {}

            //protected override void Init()
            //{
            //    base.Init();

            //    //StoryOperatableItemDb.Instance.Load();
            //    //ForceSetting.Instance.Load(Resources.Load("ForceSetting/ForceSettings_Story") as TextAsset);
            //}

            protected override void New()
            {
                base.New();
                //if (mType == SingleGameStory.StoryScene.MainLand)
                //    MisRepositoryArchiveMgr.Instance.New();
                //else if (mType == SingleGameStory.StoryScene.DienShip)
                //    MisRepositoryArchiveMgr.Instance.DienNew();
                MisRepositoryArchiveMgr.Instance.New();
                RMRepositoryArchiveMgr.Instance.New();
                NpcUserDataArchiveMgr.Instance.New();
                EntityCreatedArchiveMgr.Instance.New();

                //PeSingleTestInitData s = new PeSingleTestInitData();
                //s.AddTestData();
            }

            protected override void Restore()
            {
                base.Restore();

                MisRepositoryArchiveMgr.Instance.Restore();
                RMRepositoryArchiveMgr.Instance.Restore();
                NpcUserDataArchiveMgr.Instance.Restore();
                EntityCreatedArchiveMgr.Instance.Restore();
            }
        }

        class LoadRandomStory : LoadScenario
        {
            public LoadRandomStory(bool bNew) : base(bNew) { }
            protected override void Init()
            {
                base.Init();
            }
            protected override void New()
            {
                base.New();
                MisRepositoryArchiveMgr.Instance.New();
                NpcUserDataArchiveMgr.Instance.New();
                EntityCreatedArchiveMgr.Instance.New();
            }

            protected override void Restore()
            {
                base.Restore();
                MisRepositoryArchiveMgr.Instance.Restore();
                NpcUserDataArchiveMgr.Instance.Restore();
                EntityCreatedArchiveMgr.Instance.Restore();
            }
        }

        class LoadMultiStory : LoadScenario
        {
            public LoadMultiStory() : base(true) { }
        }

        class LoadCreationData : ModuleLoader
        {
            public LoadCreationData(bool bNew) : base(bNew) { }

            protected override void New()
            {
                CreationDataArchiveMgr.Instance.New();
            }

            protected override void Restore()
            {
                CreationDataArchiveMgr.Instance.Restore();
            }
        }

        class OpenVCEditor : PeLauncher.ILaunchable
        {
            void PeLauncher.ILaunchable.Launch()
            {
                VCEditor.Open();
                VCEditor.OnCloseFinally += delegate()
                {
                    Debug.Log("vceditor closed");
                };
            }
        }

        class LoadItemAsset : ModuleLoader
        {
            public LoadItemAsset(bool bNew) : base(bNew) { }

            protected override void New()
            {
                ItemAssetArchiveMgr.Instance.New();
            }

            protected override void Restore()
            {
                ItemAssetArchiveMgr.Instance.Restore();
            }
        }

        class LoadWeatherSys : PeLauncher.ILaunchable
        {
            void PeLauncher.ILaunchable.Launch()
            {
                //NVWeatherSys.
                throw new System.NotImplementedException();
            }
        }

        class LoadRailway : ModuleLoader
        {
            public LoadRailway(bool bNew) : base(bNew) { }

            void LoadPrefab()
            {
                new GameObject("RailwayManager", typeof(RailwayRunner));
            }

            protected override void New()
            {
                LoadPrefab();
                Railway.Manager.Instance.New();
            }

            protected override void Restore()
            {
                LoadPrefab();
                Railway.Manager.Instance.Restore();
            }
        }

        class LoadItemBox : ModuleLoader
        {
            public LoadItemBox(bool bNew) : base(bNew) { }

            protected override void New()
            {
                ItemBoxMgr.Instance.New();
            }

            protected override void Restore()
            {
                ItemBoxMgr.Instance.Restore();
            }
        }

        class InitBuildManager : ModuleLoader
        {
            public InitBuildManager(bool newGame) : base(newGame) { }

            protected override void New()
            {
                LaunchBuildSystem();
                UIBlockSaver.Instance.New();
            }

            protected override void Restore()
            {
                LaunchBuildSystem();
                UIBlockSaver.Instance.Restore();
            }

            void LaunchBuildSystem()
            {
                Object.Instantiate(Resources.Load("Prefab/Building System")).name = "Building System";
            }
        }

        class LoadFarm : PeLauncher.ILaunchable
        {
            void PeLauncher.ILaunchable.Launch()
            {
                new GameObject("FarmManager", typeof(FarmManager));
            }
        }

        class LoadColony : PeLauncher.ILaunchable
        {
            void PeLauncher.ILaunchable.Launch()
            {
                Object.Instantiate(Resources.Load<GameObject>("Prefabs/CSMain")).name = "CSMain";
            }
        }

        class LoadCSData : ModuleLoader
        {
            public LoadCSData(bool bNew) : base(bNew) { }

            protected override void New()
            {
                CSDataMgrArchiveMgr.Instance.New();
            }

            protected override void Restore()
            {
                CSDataMgrArchiveMgr.Instance.Restore();
            }
        }

        class LoadUiHelp : ModuleLoader
        {
            public LoadUiHelp(bool bNew) : base(bNew) { }

            protected override void New()
            {
                UiHelpArchiveMgr.Instance.New();
            }

            protected override void Restore()
            {
                UiHelpArchiveMgr.Instance.Restore();
            }
        }
        class LoadTutorial : PeLauncher.ILaunchable
        {
            void PeLauncher.ILaunchable.Launch()
            {
                TrainingScene.TrainingRoomLoader.LoadTrainingRoom();
            }
        }

        class Dummy : PeLauncher.ILaunchable
        {
            void PeLauncher.ILaunchable.Launch()
            {

            }
        }

        class LoadRandomTerrainParam : ModuleLoader
        {
            public LoadRandomTerrainParam(bool bNew) : base(bNew) { }

            protected override void New()
            {
                RandomMapConfigArchiveMgr.Instance.New();
            }

            protected override void Restore()
            {
                RandomMapConfigArchiveMgr.Instance.Restore();
            }
        }

        class LoadMultiPlayerSpawnPos : PeLauncher.ILaunchable
        {
            void PeLauncher.ILaunchable.Launch()
            {
				if (null == BaseNetwork.MainPlayer)
					return;

				Vector3 pos = BaseNetwork.MainPlayer._pos;

                if (BaseNetwork.MainPlayer.UseNewPos)
                {
                    IntVector2 posXZ = VArtifactUtil.GetSpawnPos();
                    pos = new Vector3(posXZ.x, VFDataRTGen.GetPosTop(posXZ), posXZ.y);
                }

                Pathea.PlayerSpawnPosProvider.Instance.SetPos(pos);
            }
        }

        abstract class LoadPlayerSpawnPos : ModuleLoader
        {
            public LoadPlayerSpawnPos(bool bNew) : base(bNew) { }

            protected override void Restore()
            {                
                PeGameSummary s = PeGameSummary.Mgr.Instance.Get();

                if (s != null)
                {
                    SetPos(s.playerPos);
                }
            }

            protected void SetPos(Vector3 pos)
            {
                PlayerSpawnPosProvider.Instance.SetPos(pos);
            }
        }

        class LoadStoryPlayerSpawnPos : LoadPlayerSpawnPos
        {
            SingleGameStory.StoryScene mType = SingleGameStory.StoryScene.MainLand;
            public LoadStoryPlayerSpawnPos(bool bNew, SingleGameStory.StoryScene type = SingleGameStory.StoryScene.MainLand) : base(bNew) { mType = type; }

            protected override void New()
            {
                if (mType == SingleGameStory.StoryScene.MainLand)
                    SetPos(new Vector3(12227f, 121.5f, 6095f));
                else if (mType == SingleGameStory.StoryScene.DienShip0)
                    SetPos(new Vector3(14798.09f, 20.98818f, 8246.396f));
                else if (mType == SingleGameStory.StoryScene.L1Ship)
                    SetPos(new Vector3(9649.354f, 90.488f, 12744.77f));
            }
        }

        class LoadAdventurePlayerSpawnPos : LoadPlayerSpawnPos
        {
            public LoadAdventurePlayerSpawnPos(bool bNew) : base(bNew) { }

            protected override void New()
            {
                SetPos(VArtifactTownManager.Instance.playerStartPos);
            }
        }

        class LoadBuildPlayerSpawnPos : LoadPlayerSpawnPos
        {
            public LoadBuildPlayerSpawnPos(bool bNew) : base(bNew) { }

            protected override void New()
            {
                SetPos(GetPlayerSpawnPos());
            }

            Vector3 GetPlayerSpawnPos()
            {
                IntVector2 posXZ = VArtifactUtil.GetSpawnPos();
                Vector3 pos = new Vector3(posXZ.x, VFDataRTGen.GetPosTop(posXZ), posXZ.y);

                return pos;
            }
        }

       

		class LoadTutorialPlayerSpawnPos : LoadPlayerSpawnPos
		{
			public LoadTutorialPlayerSpawnPos(bool bNew) : base(bNew){}

			protected override void New()
			{
				SetPos(new Vector3(27f, 2f, 11f));
			}
		}

		
		class LoadReputation : ModuleLoader
		{
			public LoadReputation(bool bNew) : base(bNew) { }
			
			protected override void New()
			{
				ReputationSystem.Instance.New();
			}
			
			protected override void Restore()
			{
				ReputationSystem.Instance.Restore();
			}
		}

		class LoadNPCTalkHistory : ModuleLoader
		{
			public LoadNPCTalkHistory(bool bNew) : base(bNew) { }
			
			protected override void New()
			{
				NPCTalkHistroy.Instance.New();
			}
			
			protected override void Restore()
			{
				NPCTalkHistroy.Instance.Restore();
			}
		}

        class RandomMission : ModuleLoader
        {
            public RandomMission(bool bNew) : base(bNew) { }

            protected override void New()
            {

                AdRMRepositoryArchiveMgr.Instance.New();
            }

            protected override void Restore()
            {
                AdRMRepositoryArchiveMgr.Instance.Restore();
            }
        }

		class LoadLootItem : ModuleLoader
		{
			public LoadLootItem(bool bNew) : base(bNew) { }

			protected override void New()
			{
				LootItemMgr.Instance.New();
			}
			
			protected override void Restore()
			{
				LootItemMgr.Instance.Restore();
			}
		}

        class LoadInGameAid : ModuleLoader
        {
            public LoadInGameAid(bool bNew) : base(bNew)
            {
            }

            protected override void New()
            {
                InGamAidArchiveMgr.Instance.New();
            }

            protected override void Restore()
            {
                InGamAidArchiveMgr.Instance.Restore();
            }
        }

        class LoadMountsMonsterData : ModuleLoader
        {
            public LoadMountsMonsterData(bool bNew) : base(bNew)
            { }

            protected override void New()
            {
                MountsArchiveMgr.Instance.New();
            }

            protected override void Restore()
            {
                MountsArchiveMgr.Instance.Restore();
            }
        }
    }
}