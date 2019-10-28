using UnityEngine;
using System.Collections;
using Pathea.GameLoader;

namespace Pathea
{
	public abstract class MultiGame
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
			SingleGameStory.curType = SingleGameStory.StoryScene.MainLand;
			mNewGame = newGame;
			mYirdName = yirdName;
			LoadYird();
			
			Load();
		}
		
		void LoadYird()
		{

			if (string.IsNullOrEmpty(mYirdName))
			{
				mYirdName = GetDefaultYirdName();
			}
			
			ArchiveMgr.Instance.LoadYird(mYirdName);
		}
		
		protected abstract void Load();
		
		protected abstract string GetDefaultYirdName();
	}
	
	public abstract class MultiGameOfficial : MultiGame
	{
		public const string YirdMain = "main";
		
		protected override string GetDefaultYirdName()
		{
			return YirdMain;
		}
	}
	
	public class MultiGameStory : MultiGameOfficial
	{
		public static SingleGameStory.StoryScene curType = SingleGameStory.StoryScene.MainLand;

		protected override void Load()
		{
			if (yirdName == YirdMain)
			{
				LoadStory(true);
				curType = SingleGameStory.StoryScene.MainLand;
			}
			else if (yirdName == "DienShip0")
			{
				LoadYird(true, SingleGameStory.StoryScene.DienShip0);
				curType = SingleGameStory.StoryScene.DienShip0;
			}
			else if (yirdName == "L1Ship")
			{
				LoadYird(true, SingleGameStory.StoryScene.L1Ship);
				curType = SingleGameStory.StoryScene.L1Ship;
			}
		}
		
		void LoadYird(bool bNewGame = true, SingleGameStory.StoryScene type = SingleGameStory.StoryScene.MainLand)
		{
			PeLauncher.Instance.Add(new LoadRandomItemMgr());
			
			PeLauncher.Instance.Add(new ResetGlobalData());

			PeLauncher.Instance.Add(new LoadSpecialScene(type));
			
			PeLauncher.Instance.Add(new LoadCamera());
			

			PeLauncher.Instance.Add(new LoadEditedTerrain(bNewGame, ""));

            PeLauncher.Instance.Add(new LoadPathFinding());
			PeLauncher.Instance.Add(new LoadPathFindingEx());

            //add creation data to itemasset
            PeLauncher.Instance.Add(new LoadCreationData(bNewGame));			
			//drag out scene agent will need this to create item.
			PeLauncher.Instance.Add(new LoadItemAsset(bNewGame));
			
			PeLauncher.Instance.Add(new LoadWaveSystem());			

			PeLauncher.Instance.Add(new LoadWorldInfo(bNewGame));
			
			PeLauncher.Instance.Add(new LoadGameGraphInfo(new Vector2(18 * 1024, 18 * 1024)));
			
			PeLauncher.Instance.Add(new LoadStoryMap(bNewGame));

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
			
			PeLauncher.Instance.Add(new LoadMultiCreature());
			
			PeLauncher.Instance.Add(new LoadWorldCollider());
			
			PlayerPackageCmpt.LockStackCount = false;
		}
		
		static void LoadStory(bool bNewGame = true)
		{
			Debug.Log(System.DateTime.Now.ToString("G") + "[Start Game Mode] "+"multi story client");
			
			PeLauncher.Instance.Add(new LoadRandomItemMgr());
			
			PeLauncher.Instance.Add(new ResetGlobalData());
			
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
			
			PeLauncher.Instance.Add (new LoadVETreeProtos (bNewGame));
			string treePath = GameConfig.PEDataPath + "VoxelData/SubTerrains/";
			PeLauncher.Instance.Add(new LoadEditedTree(bNewGame, treePath));
			
			PeLauncher.Instance.Add(new LoadEnvironment());
			
			PeLauncher.Instance.Add(new LoadWorldInfo(bNewGame));
			
			PeLauncher.Instance.Add(new LoadGameGraphInfo(new Vector2(18 * 1024, 18 * 1024)));
			
			PeLauncher.Instance.Add(new LoadStoryMap(bNewGame));
			
			PeLauncher.Instance.Add(new LoadRailway(bNewGame));

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
			
			//PeLauncher.Instance.Add(new LoadMultiStory());
			
			PeLauncher.Instance.Add(new LoadMultiCreature());
			
			PeLauncher.Instance.Add(new LoadWorldCollider());
			
			PlayerPackageCmpt.LockStackCount = false;
		}
	}
	
	public class MultiGameAdventure : MultiGameOfficial
	{
		protected override void Load()
		{
			LoadAdventure();
		}
		
		//地形随机的故事模式
		static void LoadAdventure(bool bNewGame = true)
		{
			Debug.Log(System.DateTime.Now.ToString("G") + "[Start Game Mode] "+"multi adventure client");
			PeLauncher.Instance.Add(new ResetGlobalData());

			PeLauncher.Instance.Add(new LoadRandomTown());
			
			PeLauncher.Instance.Add(new LoadRandomItemMgr());
			
			PeLauncher.Instance.Add(new LoadMultiPlayerSpawnPos());
			
			PeLauncher.Instance.Add(new LoadCamera());
			
			PeLauncher.Instance.Add(new LoadRandomTerrainWithTown(true));

			PeLauncher.Instance.Add(new LoadPathFinding());			
			PeLauncher.Instance.Add(new LoadPathFindingEx());

            PeLauncher.Instance.Add(new LoadWaveSystem());
			
			PeLauncher.Instance.Add(new LoadGrassRandom(true));
			
			PeLauncher.Instance.Add(new LoadVETreeProtos(true));
			PeLauncher.Instance.Add(new LoadRandomTree(true));
			
			PeLauncher.Instance.Add(new LoadEnvironment());
			
			PeLauncher.Instance.Add(new LoadRailway(true));

			PeLauncher.Instance.Add(new LoadGUI());
			
			//colony
			PeLauncher.Instance.Add(new LoadCSData(true));
			PeLauncher.Instance.Add(new LoadFarm());
			PeLauncher.Instance.Add(new LoadColony());
			
			PeLauncher.Instance.Add(new LoadEntityCreator(true));

			PeLauncher.Instance.Add(new LoadUiHelp(true));
			
			PeLauncher.Instance.Add(new LoadItemBox(true));
			
			PeLauncher.Instance.Add(new LoadRandomMap(true));
			
			PeLauncher.Instance.Add(new InitBuildManager(true));
			
			PeLauncher.Instance.Add(new LoadMultiStory());
			
			PeLauncher.Instance.Add(new LoadMultiCreature());
			
			PeLauncher.Instance.Add(new LoadWorldCollider());

			PeLauncher.Instance.Add(new LoadRandomDungeon());

			PeLauncher.Instance.Add(new LoadMusicAdventure());


			PlayerPackageCmpt.LockStackCount = false;
		}
	}
	
	public class MultiGameBuild : MultiGameOfficial
	{
		protected override void Load()
		{
			LoadBuild();
		}
		
		//资源无限，无怪，无npc的adventure
		static void LoadBuild(bool bNewGame = true)
		{
			Debug.Log(System.DateTime.Now.ToString("G") + "[Start Game Mode] "+"multi build client");
			PeLauncher.Instance.Add(new ResetGlobalData());

			PeLauncher.Instance.Add(new LoadMultiPlayerSpawnPos());
			
			PeLauncher.Instance.Add(new LoadCamera());
			
			PeLauncher.Instance.Add(new LoadRandomItemMgr());
			
			PeLauncher.Instance.Add(new LoadRandomTerrainWithTown(true));

			PeLauncher.Instance.Add(new LoadPathFinding());			
			PeLauncher.Instance.Add(new LoadPathFindingEx());

            PeLauncher.Instance.Add(new LoadWaveSystem());
			
			PeLauncher.Instance.Add(new LoadGrassRandom(true));
			
			PeLauncher.Instance.Add (new LoadVETreeProtos (true));
			PeLauncher.Instance.Add(new LoadRandomTree(true));
			
			PeLauncher.Instance.Add(new LoadEnvironment());
			
			PeLauncher.Instance.Add(new LoadRailway(true));

			PeLauncher.Instance.Add(new LoadGUI());
			
			//colony
			PeLauncher.Instance.Add(new LoadCSData(true));
			PeLauncher.Instance.Add(new LoadFarm());
			PeLauncher.Instance.Add(new LoadColony());
			
			PeLauncher.Instance.Add(new LoadEntityCreator(true));

			PeLauncher.Instance.Add(new LoadUiHelp(true));
			
			PeLauncher.Instance.Add(new LoadItemBox(true));
			
			PeLauncher.Instance.Add(new LoadRandomMap(true));
			
			PeLauncher.Instance.Add(new InitBuildManager(true));
			
			PeLauncher.Instance.Add(new LoadMultiStory());
			
			PeLauncher.Instance.Add(new LoadMultiCreature());
			
			PeLauncher.Instance.Add(new LoadWorldCollider());
			
			PlayerPackageCmpt.LockStackCount = false;
		}
	}
	
	public class MultiGameCustom : MultiGameOfficial
	{
		
		protected override void Load()
		{
			LoadCustom();
		}
		
		//        static void LoadCustom(bool bNewGame, YirdData yirdData)
		static void LoadCustom(bool bNewGame = true)
		{
			CustomGameData customData = CustomGameData.Mgr.Instance.GetCustomData(Pathea.PeGameMgr.mapUID);
            if (null != customData)
                CustomGameData.Mgr.Instance.curGameData = customData;

            YirdData yirdData = customData.curYirdData;
			if (null == yirdData)
			{
				Debug.LogError("custom game data is null");
				return;
			}
			Debug.Log(System.DateTime.Now.ToString("G") + "[Start Game Mode] "+"multi custom client");
			PeLauncher.Instance.Add(new ResetGlobalData());
			PeLauncher.Instance.Add(new LoadMultiPlayerSpawnPos());
			PeLauncher.Instance.Add(new LoadCamera());
			PeLauncher.Instance.Add(new LoadEditedTerrain(bNewGame, yirdData.terrainPath));
			PeLauncher.Instance.Add(new LoadPathFinding());
			PeLauncher.Instance.Add(new LoadPathFindingEx());
			PeLauncher.Instance.Add(new LoadWaveSystem());
            PeLauncher.Instance.Add(new LoadEditedGrass(bNewGame, yirdData.grassPath));
			PeLauncher.Instance.Add(new LoadVETreeProtos(bNewGame));
			PeLauncher.Instance.Add(new LoadEditedTree(bNewGame, yirdData.treePath));
			PeLauncher.Instance.Add(new LoadEnvironment());
            PeLauncher.Instance.Add(new LoadFarm());
            PeLauncher.Instance.Add(new LoadColony());
            PeLauncher.Instance.Add(new LoadGUI());
			PeLauncher.Instance.Add(new LoadUiHelp(bNewGame));
			PeLauncher.Instance.Add(new InitBuildManager(bNewGame));
            PeLauncher.Instance.Add(new LoadMultiCustomCreature());
            //PeLauncher.Instance.Add(new LoadWorldCollider());
            PeLauncher.Instance.Add(new LoadCustomEntityCreator(bNewGame, yirdData));
            PeLauncher.Instance.Add(new LoadMultiCustom(bNewGame, customData));
            PlayerPackageCmpt.LockStackCount = false;
		}
	}
	
}