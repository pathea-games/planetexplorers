
namespace Pathea
{
    public delegate void GamePasue(bool isPasue);

    public static class PeGameMgr
    {
        public enum EPlayerType
        {
            Single,
            Multiple,
            Creation,
            Tutorial,
            Max
        }

        public enum ESceneMode
        {
            Story,
            Adventure,
            Build,
            TowerDefense,
            Custom,
			Tutorial,
            Max
        }

        public enum EGameType
        {
            Cooperation = 0,
            VS,
            Survive,
            Max
        }

		public enum ETutorialMode
		{
			GatherCut,
			Replicate,
			DigBuild,
			Max
		}

		public enum EGameLevel
		{
			Easy,
			Normal,
		}

        public static GamePasue PasueEvent;

        static EPlayerType mPlayerType = EPlayerType.Single;
        static ESceneMode mSceneMode = ESceneMode.Story;
        static EGameType mGameType = EGameType.Cooperation;
		static ETutorialMode mTutorialMode = ETutorialMode.Max;
        static ArchiveMgr.ESave mLoadArchive = ArchiveMgr.ESave.New;
        static bool mUnlimitedRes = false;
        static bool mMonsterYes = true;
		static bool mPause;
		static EGameLevel mGameLevel = EGameLevel.Normal;

        //static YirdData mCustomGame = null;
		static int cullMask;
        static void BeforeLoad()
        {
			cullMask = UnityEngine.Camera.main.cullingMask;
			UnityEngine.Camera.main.cullingMask = 0;
			PeInput.enable = false;
        }
        static void PostLoad()
        {
            UnityEngine.Resources.UnloadUnusedAssets();
            ApplySystemSetting();

            PeSingletonRunner.Launch();

            if (mPlayerType == EPlayerType.Single)
            {
				AutoArchiveRunner.Init();
				if(yirdName!=AdventureScene.Dungen.ToString())
                	AutoArchiveRunner.Start();
            }
			UnityEngine.Camera.main.cullingMask = cullMask;
			PeInput.enable = true;
			targetYird = "";
        }
		
		static void ApplySystemSetting()
		{
			SystemSettingData.Instance.ApplySettings();
		}

        static PlayerTypeLoader CreatePlayerTypeLoader()
        {
            if (mPlayerType == EPlayerType.Creation)
            {
                return new CreationPlayerTypeLoader();
            }
            else if (mPlayerType == EPlayerType.Multiple)
            {
				if(MultiPlayerTypeArchiveMgr.Instance.multiScenario == null)
				{
					MultiPlayerTypeArchiveMgr.Instance.New();
				}
      

                MultiPlayerTypeLoader multiPlayerTypeLoader = MultiPlayerTypeArchiveMgr.Instance.multiScenario;

				// ???????????????????????????????????????????????BUG?????????
				// 影响多人除故事模式外的其它模式，在退出到大厅后无法进入游戏BUG。暂时屏蔽

                //if (!string.IsNullOrEmpty(yirdName))
                //{
                //    multiPlayerTypeLoader.SetYirdName(yirdName);
                //}
                //else
                    multiPlayerTypeLoader.New(sceneMode, gameName);
				return multiPlayerTypeLoader;
			}
			else if (mPlayerType == EPlayerType.Single)
            {
                ArchiveMgr.Instance.LoadAndCleanSwap(loadArchive);

                if (loadArchive == ArchiveMgr.ESave.New)
                {
                    SinglePlayerTypeArchiveMgr.Instance.New();

                    SinglePlayerTypeLoader singlePlayerTypeLoader = SinglePlayerTypeArchiveMgr.Instance.singleScenario;

                    singlePlayerTypeLoader.New(sceneMode, mapUID, gameName);
                }
                else
                {
                    SinglePlayerTypeArchiveMgr.Instance.Restore();

                    SinglePlayerTypeLoader singlePlayerTypeLoader = SinglePlayerTypeArchiveMgr.Instance.singleScenario;

					if (!string.IsNullOrEmpty(targetYird))
                    {
						singlePlayerTypeLoader.SetYirdName(targetYird);
                    }
                }

                return SinglePlayerTypeArchiveMgr.Instance.singleScenario;
            }
            else if (mPlayerType == EPlayerType.Tutorial)
            {
                TutorialPlayerTypeLoader tutorialScenario = new TutorialPlayerTypeLoader();

                tutorialScenario.tutorialMode = tutorialMode;

                return tutorialScenario;
            }
            else
            {
                return null;
            }
        }

        #region public

        public static void Run()
        {
            BeforeLoad();
			PeLauncher.Instance.endLaunch = delegate()
            {
                if (IsMulti && !NetworkInterface.IsClient)
                    return true;

                //if (!VFVoxelTerrain.TerrainVoxelComplete)
                if (!VFVoxelTerrain.TerrainColliderComplete)
                {
                    return false;
                }

                SceneMan.self.StartWork();

                PeEntity mainPlayer = MainPlayer.Instance.entity;
                if (null == mainPlayer)
                {
                    return false;
                }

                MotionMgrCmpt motion = mainPlayer.GetCmpt<MotionMgrCmpt>();
                if(motion == null)
                {
                    return false;
                }

                if (motion.freezePhyStateForSystem)
				{
					UnityEngine.Vector3 safePos;
					if(PeGameMgr.IsMulti)
					{
						if(PlayerNetwork.mainPlayer != null && PlayerNetwork.mainPlayer._curSceneId == (int)Pathea.SingleGameStory.StoryScene.MainLand)
						{
							if(PETools.PE.FindHumanSafePos(mainPlayer.position, out safePos, 10))
							{
								mainPlayer.position = safePos;
							}
							else
							{
								mainPlayer.position += 10 * UnityEngine.Vector3.up;
							}
						}
					}
					else
					{
						if(PETools.PE.FindHumanSafePos(mainPlayer.position, out safePos, 10))
						{
							mainPlayer.position = safePos;
						}
						else
						{
							mainPlayer.position += 10 * UnityEngine.Vector3.up;
						}
					}


                    return false;
                }

                if (mSceneMode == ESceneMode.Custom)
                    PeCustom.PeCustomScene.Self.Notify(PeCustom.ESceneNoification.SceneBegin);

                PostLoad();
				System.GC.Collect();
                return true;
            };

            PlayerTypeLoader scenario = CreatePlayerTypeLoader();

            if (scenario == null)
            {
                return;
            }

            scenario.Load();

            SinglePlayerTypeLoader singlePlayerTypeLoader = scenario as SinglePlayerTypeLoader;
            if (singlePlayerTypeLoader != null)
            {
                //the data is from archive in single player mode
                sceneMode = singlePlayerTypeLoader.sceneMode;
                yirdName = singlePlayerTypeLoader.yirdName;
                gameName = singlePlayerTypeLoader.gameName;
            }
            else
            {
                MultiPlayerTypeLoader multiPlayerTypeLoader = scenario as MultiPlayerTypeLoader;
                if (multiPlayerTypeLoader != null)
                {
                    //the data is from archive in single player mode
                    sceneMode = multiPlayerTypeLoader.sceneMode;
                    yirdName = multiPlayerTypeLoader.yirdName;
                    gameName = multiPlayerTypeLoader.gameName;
                }

            }

            //zhujiangbo:bug here if a game switch from edited terrain to random terrain
            VFVoxelTerrain.RandomMap = randomMap;

            PeLauncher.Instance.StartLoad();
        }

        public static EPlayerType playerType
        {
            get
            {
                return mPlayerType;
            }
            set
            {
                mPlayerType = value;
            }
        }

        public static ESceneMode sceneMode
        {
            get
            {
                return mSceneMode;
            }
            set
            {
                mSceneMode = value;
            }
        }

		public static EGameLevel gameLevel
		{			
			get
			{
				return mGameLevel;
			}
			set
			{
				mGameLevel = value;
			}
		}

        public static EGameType gameType
        {
            get
            {
                return mGameType;
            }
            set
            {
                mGameType = value;
            }
        }

		public static ETutorialMode tutorialMode
		{
			get
			{
				return mTutorialMode;
			}
			set
			{
				mTutorialMode = value;
			}
		}

        public static ArchiveMgr.ESave loadArchive
        {
            get
            {
                return mLoadArchive;
            }
            set
            {
                mLoadArchive = value;
            }
        }

        public static bool unlimitedRes
        {
            get
            {
                return mUnlimitedRes;
            }
            set
            {
                mUnlimitedRes = value;
            }
        }

        public static bool monsterYes
        {
            get
            {
                return mMonsterYes;
            }
            set
            {
                mMonsterYes = value;
            }
        }


		public static bool gamePause
		{
			get{ return mPause; }
			set
			{
                if (mPause != value)
                {
                    mPause = value;
                    if (!IsMulti)
                        UnityEngine.Time.timeScale = mPause ? 0.0001f : 1f;

                    if(PasueEvent != null)
                    {
                        PasueEvent(mPause);
                    }
                }
			}
		}

        public static bool randomMap
        {
            get
            {
                if((EPlayerType.Multiple == playerType) && sceneMode != ESceneMode.Story && sceneMode != ESceneMode.Custom)
                {
                    return true;
                }
                else if (EPlayerType.Single == playerType)
                {
                    if (sceneMode == ESceneMode.Adventure || sceneMode == ESceneMode.Build)
                    {
						if(yirdName!=AdventureScene.Dungen.ToString())
                        	return true;
                    }
                }

                return false;
            }
        }

        public static string gameName
        {
            get;
            set;
        }

		public static string targetYird
		{
			get;
			set;
		}

        public static string yirdName
        {
            get;
            set;
        }

		public static string mapUID
		{
			get;
			set;
		}

        #endregion

        #region EPlayerType
        public static bool IsSingle
        {
            get
            {
                return mPlayerType == EPlayerType.Single;
            }
        }
        public static bool IsMulti
        {
            get
            {
                return mPlayerType == EPlayerType.Multiple;
            }
        }
        #endregion

        #region ESceneMode
        public static bool IsStory
        {
            get
            {
                return mSceneMode == ESceneMode.Story || mSceneMode == ESceneMode.Tutorial;
            }
        }

        public static bool IsAdventure
        {
            get
            {
                return mSceneMode == ESceneMode.Adventure;
            }
        }

        public static bool IsTowerDefense
        {
            get
            {
                return mSceneMode == ESceneMode.TowerDefense;
            }
        }

        public static bool IsBuild
        {
            get
            {
                return mSceneMode == ESceneMode.Build;
            }
        }
		public static bool IsTutorial
		{
			get
			{
				return mSceneMode == ESceneMode.Tutorial;
			}
		}

		public static bool IsCustom
		{
			get
			{
				return mSceneMode == ESceneMode.Custom;
			}
		}
        #endregion

        #region EGameType
        public static bool IsCooperation
        {
            get
            {
                return mGameType == EGameType.Cooperation;
            }
        }
        public static bool IsSurvive
        {
            get
            {
                return mGameType == EGameType.Survive;
            }
        }
        public static bool IsVS
        {
            get
            {
                return mGameType == EGameType.VS;
            }
        }
        #endregion

        #region composition
        public static bool IsSingleStory
        {
            get
            {
                return IsSingle && IsStory;
            }
        }

        public static bool IsSingleAdventure
        {
            get
            {
                return IsSingle && IsAdventure;
            }
        }

        public static bool IsSingleBuild
        {
            get
            {
                return IsSingle && IsBuild;
            }
        }

        public static bool IsMultiAdventure
        {
            get
            {
                return IsMulti && IsAdventure;
            }
        }

		public static bool IsMultiStory
		{
			get
			{
				return IsMulti && IsStory;
			}
		}

		public static bool IsMultiCustom
		{
			get
			{
				return IsMulti && IsCustom;
			}
		}

        public static bool IsMultiBuild
        {
            get
            {
                return IsMulti && IsBuild;
            }
        }

        public static bool IsMultiTowerDefense
        {
            get
            {
                return IsMulti && IsTowerDefense;
            }
        }

        public static bool IsMultiCoop
        {
            get
            {
                return IsMulti && IsCooperation;
            }
        }

        public static bool IsMultiVS
        {
            get
            {
                return IsMulti && IsVS;
            }
        }

        public static bool IsMultiSurvive
        {
            get
            {
                return IsMulti && IsSurvive;
            }
        }

        public static bool IsMultiAdventureCoop
        {
            get
            {
                return IsMulti && IsAdventure && IsCooperation;
            }
        }

        public static bool IsMultiAdventureVS
        {
            get
            {
                return IsMulti && IsAdventure && IsVS;
            }
        }

        public static bool IsMultiAdventureSurvive
        {
            get
            {
                return IsMulti && IsAdventure && IsSurvive;
            }
        }

        public static bool IsMultiBuildCoop
        {
            get
            {
                return IsMulti && IsBuild && IsCooperation;
            }
        }

        public static bool IsMultiBuildVS
        {
            get
            {
                return IsMulti && IsBuild && IsMultiVS;
            }
        }

        public static bool IsMultiBuildSurvive
        {
            get
            {
                return IsMulti && IsBuild && IsSurvive;
            }
        }

        #endregion
    }
}