using UnityEngine;
using System.Collections;
using Pathea.GameLoader;

namespace Pathea
{
    public abstract class SingleGame
    {
        bool mNewGame;

        protected bool newGame
        {
            get { return mNewGame; }
            private set { mNewGame = value; }
        }

        string mYirdName;

        public string yirdName
        {
            get { return mYirdName; }
            private set { mYirdName = value; }
        }

        public void Load(bool newGame, string yirdName)
        {
            //lz-2016.08.04 在load的时候把curType重置
            SingleGameStory.curType= SingleGameStory.StoryScene.MainLand;
            mNewGame = newGame;
            mYirdName = yirdName;
            LoadYird();

            Load();
        }

        void LoadYird()
        {
            if (mNewGame)
            {
                mYirdName = GetDefaultYirdName();
            }

            if (string.IsNullOrEmpty(mYirdName))
            {
                mYirdName = GetDefaultYirdName();
            }

            ArchiveMgr.Instance.LoadYird(mYirdName);
        }

        protected abstract void Load();

        protected abstract string GetDefaultYirdName();
    }

    public abstract class SingleGameOfficial : SingleGame
    {
        public const string YirdMain = "main";

        protected override string GetDefaultYirdName()
        {
            return YirdMain;
        }
    }

    public class SingleGameStory : SingleGameOfficial
    {
        public static StoryScene curType = StoryScene.MainLand;
        public enum StoryScene
        {
            MainLand,
            L1Ship,
            DienShip0,
            TrainingShip,
            PajaShip,
            LaunchCenter,
            DienShip1,
            DienShip2,
            DienShip3,
            DienShip4,
            DienShip5
        }

        protected override void Load()
        {
            if (yirdName == YirdMain)
            {
                LoadStory(newGame);
                curType = StoryScene.MainLand;
            }
            else if (yirdName == "DienShip0")
            {
                LoadYird(newGame, StoryScene.DienShip0);
                curType = StoryScene.DienShip0;
            }
            else if (yirdName == "DienShip1")
            {
                LoadYird(newGame, StoryScene.DienShip1);
                curType = StoryScene.DienShip1;
            }
            else if (yirdName == "DienShip2")
            {
                LoadYird(newGame, StoryScene.DienShip2);
                curType = StoryScene.DienShip2;
            }
            else if (yirdName == "DienShip3")
            {
                LoadYird(newGame, StoryScene.DienShip3);
                curType = StoryScene.DienShip3;
            }
            else if (yirdName == "DienShip4")
            {
                LoadYird(newGame, StoryScene.DienShip4);
                curType = StoryScene.DienShip4;
            }
            else if (yirdName == "DienShip5")
            {
                LoadYird(newGame, StoryScene.DienShip5);
                curType = StoryScene.DienShip5;
            }
            else if (yirdName == "L1Ship")
            {
                LoadYird(newGame, StoryScene.L1Ship);
                curType = StoryScene.L1Ship;
            }
            else if (yirdName == "PajaShip")
            {
                LoadYird(newGame, StoryScene.PajaShip);
                curType = StoryScene.PajaShip;
            }
            else if (yirdName == "LaunchCenter")
            {
                LoadYird(newGame, StoryScene.LaunchCenter);
                curType = StoryScene.LaunchCenter;
            }
        }

        void LoadYird(bool bNewGame, StoryScene type = StoryScene.MainLand)
        {
			Debug.Log(System.DateTime.Now.ToString("G") + "[Start Game Mode] "+(!bNewGame ? "saved" : "new") + " story "+type.ToString());
			PeLauncher.Instance.Add(new ResetGlobalData());
			
			PeLauncher.Instance.Add(new LoadReputation(bNewGame));

            PeLauncher.Instance.Add(new LoadSpecialScene(type));

            //PeLauncher.Instance.Add(new LoadStoryPlayerSpawnPos(bNewGame, type));

            PeLauncher.Instance.Add(new LoadRandomItemMgr());

            PeLauncher.Instance.Add(new LoadCamera());

            PeLauncher.Instance.Add(new LoadEditedTerrain(bNewGame, ""));

            PeLauncher.Instance.Add(new LoadPathFinding());
			PeLauncher.Instance.Add(new LoadPathFindingEx());

            //add creation data to itemasset
            PeLauncher.Instance.Add(new LoadCreationData(bNewGame));
            //drag out scene agent will need this to create item.
            PeLauncher.Instance.Add(new LoadItemAsset(bNewGame));

            PeLauncher.Instance.Add(new LoadWaveSystem());

            if (type == StoryScene.PajaShip || type == StoryScene.LaunchCenter)
                PeLauncher.Instance.Add(new LoadEnvironment());

            PeLauncher.Instance.Add(new LoadWorldInfo(bNewGame));

            PeLauncher.Instance.Add(new LoadGameGraphInfo(new Vector2(18 * 1024, 18 * 1024)));

            PeLauncher.Instance.Add(new LoadStoryMap(bNewGame));

            PeLauncher.Instance.Add(new LoadCreature(bNewGame));

            //lz-2016.08.23 InGameAid
			PeLauncher.Instance.Add(new LoadInGameAid(bNewGame));
			
			PeLauncher.Instance.Add(new LoadNPCTalkHistory(bNewGame));

            PeLauncher.Instance.Add(new LoadGUI());
            //colony
            PeLauncher.Instance.Add(new LoadCSData(bNewGame));
            PeLauncher.Instance.Add(new LoadFarm());
            PeLauncher.Instance.Add(new LoadColony());
			
			PeLauncher.Instance.Add(new LoadEntityCreator(bNewGame));

            PeLauncher.Instance.Add(new LoadItemBox(bNewGame));

            PeLauncher.Instance.Add(new InitBuildManager(bNewGame));

            PeLauncher.Instance.Add(new LoadUiHelp(bNewGame));

            PeLauncher.Instance.Add(new LoadStory(bNewGame));

            PeLauncher.Instance.Add(new LoadMusicStroy());

			PeLauncher.Instance.Add(new LoadSingleStoryInitData(bNewGame));
			
			PeLauncher.Instance.Add(new LoadLootItem(bNewGame));
			
			PeLauncher.Instance.Add(new LoadSingleGameLevel(bNewGame));

            PeLauncher.Instance.Add(new LoadDoodadShow());

            PlayerPackageCmpt.LockStackCount = false;
        }

        static void LoadStory(bool bNewGame)
        {
			Debug.Log(System.DateTime.Now.ToString("G") + "[Start Game Mode] "+(!bNewGame ? "saved" : "new") + " story****************");

            PeLauncher.Instance.Add(new ResetGlobalData());

			PeLauncher.Instance.Add(new LoadReputation(bNewGame));

            PeLauncher.Instance.Add(new LoadStoryPlayerSpawnPos(bNewGame));

            PeLauncher.Instance.Add(new LoadRandomItemMgr());

            PeLauncher.Instance.Add(new LoadCamera());

            string terrainPath = GameConfig.PEDataPath + GameConfig.MapDataDir_Zip + "/";
            PeLauncher.Instance.Add(new LoadEditedTerrain(bNewGame, terrainPath));

            PeLauncher.Instance.Add(new LoadPathFinding());
			PeLauncher.Instance.Add(new LoadPathFindingEx());

            //add creation data to itemasset
            PeLauncher.Instance.Add(new LoadCreationData(bNewGame));
            //drag out scene agent will need this to create item.
            PeLauncher.Instance.Add(new LoadItemAsset(bNewGame));

            PeLauncher.Instance.Add(new LoadWaveSystem());

            string grassPath = GameConfig.PEDataPath + "VoxelData/SubTerrains";
            PeLauncher.Instance.Add(new LoadEditedGrass(bNewGame, grassPath));

            PeLauncher.Instance.Add(new LoadVETreeProtos(bNewGame));
            string treePath = GameConfig.PEDataPath + "VoxelData/SubTerrains/";
            PeLauncher.Instance.Add(new LoadEditedTree(bNewGame, treePath));

            PeLauncher.Instance.Add(new LoadEnvironment());

            PeLauncher.Instance.Add(new LoadWorldInfo(bNewGame));

            PeLauncher.Instance.Add(new LoadGameGraphInfo(new Vector2(18 * 1024, 18 * 1024)));

            PeLauncher.Instance.Add(new LoadStoryMap(bNewGame));

            PeLauncher.Instance.Add(new LoadRailway(bNewGame));

            PeLauncher.Instance.Add(new LoadCreature(bNewGame));

            //lz-2016.08.23 InGameAid
			PeLauncher.Instance.Add(new LoadInGameAid(bNewGame));
			
			PeLauncher.Instance.Add(new LoadNPCTalkHistory(bNewGame));

            PeLauncher.Instance.Add(new LoadGUI());
            //colony
            PeLauncher.Instance.Add(new LoadCSData(bNewGame));
            PeLauncher.Instance.Add(new LoadFarm());
            PeLauncher.Instance.Add(new LoadColony());
			
			PeLauncher.Instance.Add(new LoadEntityCreator(bNewGame));

            PeLauncher.Instance.Add(new LoadItemBox(bNewGame));

            PeLauncher.Instance.Add(new InitBuildManager(bNewGame));

            PeLauncher.Instance.Add(new LoadUiHelp(bNewGame));

            PeLauncher.Instance.Add(new LoadStory(bNewGame));

            PeLauncher.Instance.Add(new LoadMusicStroy());

			PeLauncher.Instance.Add(new LoadSingleStoryInitData(bNewGame));
			
			PeLauncher.Instance.Add(new LoadLootItem(bNewGame));
			
			PeLauncher.Instance.Add(new LoadWorldCollider());

			PeLauncher.Instance.Add(new LoadMonsterSiegeCamp());

			PeLauncher.Instance.Add(new LoadSingleGameLevel(bNewGame));

            //lw:添加坐骑存档
            PeLauncher.Instance.Add(new LoadMountsMonsterData(bNewGame));

            PlayerPackageCmpt.LockStackCount = false;
        }
    }
	public enum AdventureScene
	{
		MainAdventure,
		Dungen
	}

    public class SingleGameAdventure : SingleGameOfficial
	{
		public static AdventureScene curType = AdventureScene.MainAdventure;
		protected override string GetDefaultYirdName()
		{
			return AdventureScene.MainAdventure.ToString();
		}
		protected override void Load()
        {

			if (yirdName == AdventureScene.MainAdventure.ToString())
			{
				LoadAdventure(newGame);
				curType = AdventureScene.MainAdventure;
			}
			else if (yirdName == AdventureScene.Dungen.ToString())
			{
				LoadYird(newGame, AdventureScene.Dungen);
				curType = AdventureScene.Dungen;
			}
		}

		void LoadYird(bool bNewGame, AdventureScene type = AdventureScene.MainAdventure)
		{
			Debug.Log(System.DateTime.Now.ToString("G") + "[Start Game Mode] " + (!bNewGame ? "saved" : "new") + " adventure " + type.ToString());
			
			PeLauncher.Instance.Add(new ResetGlobalData());
			
			PeLauncher.Instance.Add(new LoadReputation(bNewGame));
//			
//			PeLauncher.Instance.Add(new LoadRandomItemMgr());
     //			
			PeLauncher.Instance.Add(new LoadRandomTerrainParam(bNewGame));
//			if(type==AdventureScene.MainAdventure)
				PeLauncher.Instance.Add(new LoadRandomTown());
			
//			PeLauncher.Instance.Add(new LoadAdventurePlayerSpawnPos(bNewGame));
			
			PeLauncher.Instance.Add(new LoadCamera());
			
			
			PeLauncher.Instance.Add(new LoadRandomTerrainWithTown(bNewGame));
//			PeLauncher.Instance.Add(new LoadEditedTerrain(bNewGame, ""));
			
			PeLauncher.Instance.Add(new LoadCreationData(bNewGame));
			PeLauncher.Instance.Add(new LoadItemAsset(bNewGame));
			
			//PeLauncher.Instance.Add(new LoadSceneStatic(bNewGame));
			
//			PeLauncher.Instance.Add(new LoadWaveSystem());
			
//			PeLauncher.Instance.Add(new LoadGrassRandom(bNewGame));
			
//			PeLauncher.Instance.Add(new LoadVETreeProtos(bNewGame));
//			PeLauncher.Instance.Add(new LoadRandomTree(bNewGame));
			
//			if (type == AdventureScene.MainAdventure)
				PeLauncher.Instance.Add(new LoadEnvironment());

			
			PeLauncher.Instance.Add(new LoadWorldInfo(bNewGame));
			
//			PeLauncher.Instance.Add(new LoadRailway(bNewGame));

			PeLauncher.Instance.Add(new LoadCreature(bNewGame));
			
			PeLauncher.Instance.Add(new LoadNPCTalkHistory(bNewGame));
			
			PeLauncher.Instance.Add(new LoadGUI());
			
			//colony            
			PeLauncher.Instance.Add(new LoadCSData(bNewGame));
			PeLauncher.Instance.Add(new LoadFarm());
			PeLauncher.Instance.Add(new LoadColony());

			PeLauncher.Instance.Add(new LoadEntityCreator(bNewGame));

			PeLauncher.Instance.Add(new LoadUiHelp(bNewGame));
			
			PeLauncher.Instance.Add(new LoadRandomMap(bNewGame));
			
			PeLauncher.Instance.Add(new InitBuildManager(bNewGame));
			
			PeLauncher.Instance.Add(new LoadRandomStory(bNewGame));
			
			PeLauncher.Instance.Add(new LoadMusicAdventure());
			
			PeLauncher.Instance.Add(new LoadSingleAdventureInitData(bNewGame));
			
			PeLauncher.Instance.Add(new LoadLootItem(bNewGame));
			
			PeLauncher.Instance.Add(new LoadWorldCollider());
			
			PeLauncher.Instance.Add(new LoadRandomDungeon());
			
			PeLauncher.Instance.Add(new LoadSingleGameLevel(bNewGame));

            PlayerPackageCmpt.LockStackCount = false;
		}	
		
		//地形随机的故事模式
		static void LoadAdventure(bool bNewGame)
		{
			Debug.Log(System.DateTime.Now.ToString("G") + "[Start Game Mode] " + (!bNewGame ? "saved" : "new") + " adventure****************");

            PeLauncher.Instance.Add(new RandomMission(bNewGame));
			
			PeLauncher.Instance.Add(new ResetGlobalData());
			
			PeLauncher.Instance.Add(new LoadReputation(bNewGame));

            PeLauncher.Instance.Add(new LoadRandomItemMgr());

            PeLauncher.Instance.Add(new LoadRandomTerrainParam(bNewGame));

            PeLauncher.Instance.Add(new LoadRandomTown());

            PeLauncher.Instance.Add(new LoadAdventurePlayerSpawnPos(bNewGame));

            PeLauncher.Instance.Add(new LoadCamera());

            PeLauncher.Instance.Add(new LoadRandomTerrainWithTown(bNewGame));

            PeLauncher.Instance.Add(new LoadPathFinding());
			PeLauncher.Instance.Add(new LoadPathFindingEx());

            PeLauncher.Instance.Add(new LoadCreationData(bNewGame));
            PeLauncher.Instance.Add(new LoadItemAsset(bNewGame));

            //PeLauncher.Instance.Add(new LoadSceneStatic(bNewGame));

            PeLauncher.Instance.Add(new LoadWaveSystem());

            PeLauncher.Instance.Add(new LoadGrassRandom(bNewGame));

            PeLauncher.Instance.Add(new LoadVETreeProtos(bNewGame));
            PeLauncher.Instance.Add(new LoadRandomTree(bNewGame));

            PeLauncher.Instance.Add(new LoadEnvironment());

            PeLauncher.Instance.Add(new LoadWorldInfo(bNewGame));

            PeLauncher.Instance.Add(new LoadRailway(bNewGame));

            PeLauncher.Instance.Add(new LoadCreature(bNewGame));			
			
			PeLauncher.Instance.Add(new LoadNPCTalkHistory(bNewGame));

            PeLauncher.Instance.Add(new LoadGUI());

            //colony            
            PeLauncher.Instance.Add(new LoadCSData(bNewGame));
            PeLauncher.Instance.Add(new LoadFarm());
            PeLauncher.Instance.Add(new LoadColony());
			
			PeLauncher.Instance.Add(new LoadEntityCreator(bNewGame));

            PeLauncher.Instance.Add(new LoadUiHelp(bNewGame));

            PeLauncher.Instance.Add(new LoadRandomMap(bNewGame));

            PeLauncher.Instance.Add(new InitBuildManager(bNewGame));

            PeLauncher.Instance.Add(new LoadRandomStory(bNewGame));

            PeLauncher.Instance.Add(new LoadMusicAdventure());

			PeLauncher.Instance.Add(new LoadSingleAdventureInitData(bNewGame));
			
			PeLauncher.Instance.Add(new LoadLootItem(bNewGame));
			PeLauncher.Instance.Add(new LoadWorldCollider());
			
			PeLauncher.Instance.Add(new LoadSingleGameLevel(bNewGame));
			
			PeLauncher.Instance.Add(new LoadRandomDungeon());

            //lw:添加坐骑存档
            PeLauncher.Instance.Add(new LoadMountsMonsterData(bNewGame));

            PlayerPackageCmpt.LockStackCount = false;
        }
    }

    public class SingleGameBuild : SingleGameOfficial
    {
        protected override void Load()
        {
            LoadBuild(newGame);
        }

        //资源无限，无怪，无npc的adventure
        static void LoadBuild(bool bNewGame)
        {
			Debug.Log(System.DateTime.Now.ToString("G") + "[Start Game Mode] " + (!bNewGame ? "saved" : "new") + " Build****************");

            //ItemAsset.ItemPackage.SetIgnoreDelete(true);

			PeLauncher.Instance.Add(new ResetGlobalData());
			
			PeLauncher.Instance.Add(new LoadReputation(bNewGame));

            PeLauncher.Instance.Add(new LoadRandomItemMgr());

            PeLauncher.Instance.Add(new LoadRandomTerrainParam(bNewGame));

            PeLauncher.Instance.Add(new LoadPathFinding());
			PeLauncher.Instance.Add(new LoadPathFindingEx());

            PeLauncher.Instance.Add(new LoadCreationData(bNewGame));
            PeLauncher.Instance.Add(new LoadItemAsset(bNewGame));

            PeLauncher.Instance.Add(new LoadBuildPlayerSpawnPos(bNewGame));

            PeLauncher.Instance.Add(new LoadCamera());

            PeLauncher.Instance.Add(new LoadRandomTerrainWithoutTown(bNewGame));

            PeLauncher.Instance.Add(new LoadWaveSystem());

            PeLauncher.Instance.Add(new LoadGrassRandom(bNewGame));

            PeLauncher.Instance.Add(new LoadVETreeProtos(bNewGame));
            PeLauncher.Instance.Add(new LoadRandomTree(bNewGame));

            PeLauncher.Instance.Add(new LoadEnvironment());

            PeLauncher.Instance.Add(new LoadWorldInfo(bNewGame));

            PeLauncher.Instance.Add(new LoadRailway(bNewGame));

            PeLauncher.Instance.Add(new LoadCreature(bNewGame));

			PeLauncher.Instance.Add(new LoadNPCTalkHistory(bNewGame));

            PeLauncher.Instance.Add(new LoadGUI());

            //colony
            PeLauncher.Instance.Add(new LoadCSData(bNewGame));
            PeLauncher.Instance.Add(new LoadFarm());
            PeLauncher.Instance.Add(new LoadColony());

            PeLauncher.Instance.Add(new LoadUiHelp(bNewGame));

            PeLauncher.Instance.Add(new LoadMusicBuild());

            PeLauncher.Instance.Add(new LoadRandomMap(bNewGame));

            PeLauncher.Instance.Add(new InitBuildManager(bNewGame));

			PeLauncher.Instance.Add(new LoadSingleBuildInitData(bNewGame));
			
			PeLauncher.Instance.Add(new LoadWorldCollider());

			PeLauncher.Instance.Add(new LoadForceSetting(bNewGame));

            PlayerPackageCmpt.LockStackCount = true;
        }
    }

    public class SingleGameCustom : SingleGame
    {
        //string mGameName;
		string mUID;
        public SingleGameCustom(string UID, string gameName)
        {
			mUID = UID;
            //mGameName = gameName;
        }

        protected override string GetDefaultYirdName()
        {
            YirdData customData = CustomGameData.Mgr.Instance.GetYirdData(mUID);
            if (customData == null)
            {
                return null;
            }

            return customData.name;
        }

        protected override void Load()
        {
            //            YirdData customData = CustomGameData.Mgr.Instance.GetYirdData(mGameName, yirdName);
            ////
            //            LoadCustom(newGame, customData);
            ScenarioMapDesc desc = ScenarioMapUtils.GetMapByUID(mUID, GameConfig.CustomDataDir);
            CustomGameData customData = CustomGameData.Mgr.Instance.GetCustomData(mUID, desc.Path);
			if (customData != null)
			{
				CustomGameData.Mgr.Instance.curGameData = customData;
				LoadCustom(newGame, customData);
			}
			else
			{
				Debug.LogError("Error");
			}
        }

//        static void LoadCustom(bool bNewGame, YirdData yirdData)
		static void LoadCustom(bool bNewGame, CustomGameData customData)
        {
			YirdData yirdData = customData.curYirdData;
            if (null == yirdData)
            {
                Debug.LogError("custom game data is null");
                return;
            }


			Debug.Log(System.DateTime.Now.ToString("G") + "[Start Game Mode] " + (!bNewGame ? "saved" : "new") + " Custom, path:" + yirdData.terrainPath + "****************");

			PeLauncher.Instance.Add(new ResetGlobalData());
			
			PeLauncher.Instance.Add(new LoadReputation(bNewGame));

            PeLauncher.Instance.Add(new LoadRandomItemMgr());

			PeLauncher.Instance.Add(new LoadCustomPlayerSpawnPos(bNewGame, customData.curPlayer.StartLocation));
            
            PeLauncher.Instance.Add(new LoadCamera());

            PeLauncher.Instance.Add(new LoadEditedTerrain(bNewGame, yirdData.terrainPath));

            PeLauncher.Instance.Add(new LoadPathFinding());
			PeLauncher.Instance.Add(new LoadPathFindingEx());

            PeLauncher.Instance.Add(new LoadCreationData(bNewGame));
            PeLauncher.Instance.Add(new LoadItemAsset(bNewGame));

            PeLauncher.Instance.Add(new LoadWaveSystem());

			// Grass
            PeLauncher.Instance.Add(new LoadEditedGrass(bNewGame, yirdData.grassPath));

			// Tree
            PeLauncher.Instance.Add(new LoadVETreeProtos(bNewGame));
            PeLauncher.Instance.Add(new LoadEditedTree(bNewGame, yirdData.treePath));

            PeLauncher.Instance.Add(new LoadEnvironment());

            PeLauncher.Instance.Add(new LoadWorldInfo(bNewGame));

            //PeLauncher.Instance.Add(new LoadCustomMap(bNewGame));

            PeLauncher.Instance.Add(new LoadCustomDoodad(bNewGame, yirdData.GetDoodads()));

            PeLauncher.Instance.Add(new LoadCustomDragItem(yirdData.GetItems()));

			PeLauncher.Instance.Add(new LoadCustomSceneEffect(yirdData.GetEffects()));

			PeLauncher.Instance.Add(new LoadCustomCreature(bNewGame));

            PeLauncher.Instance.Add(new LoadCustomEntityCreator(bNewGame, yirdData));
			
			PeLauncher.Instance.Add(new LoadNPCTalkHistory(bNewGame));

            PeLauncher.Instance.Add(new LoadGUI());

			PeLauncher.Instance.Add(new LoadWorldCollider());

            PeLauncher.Instance.Add(new LoadUiHelp(bNewGame));

            PeLauncher.Instance.Add(new InitBuildManager(bNewGame));

            PeLauncher.Instance.Add(new LoadCustomStory(bNewGame, customData));

			PeLauncher.Instance.Add(new LoadSingleCustomInitData(bNewGame));
			
			PeLauncher.Instance.Add(new LoadLootItem(bNewGame));
			
			PeLauncher.Instance.Add(new LoadWorldCollider());

            PlayerPackageCmpt.LockStackCount = false;
        }
    }

}