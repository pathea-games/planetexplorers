using Pathea.GameLoader;
using UnityEngine;
using System.IO;

namespace Pathea
{
    public abstract class PlayerTypeLoader
    {
        PeGameMgr.ESceneMode mSceneMode;
        public PeGameMgr.ESceneMode sceneMode
        {
            get { return mSceneMode; }
            set { mSceneMode = value; }
        }

        public abstract void Load();
    }

    public class TutorialPlayerTypeLoader : PlayerTypeLoader
    {
        PeGameMgr.ETutorialMode mTutorialMode = PeGameMgr.ETutorialMode.Max;

        public PeGameMgr.ETutorialMode tutorialMode
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

        string GetDataPath()
        {
            return System.IO.Path.Combine(GameConfig.PEDataPath, "TutorialMode");
        }

        public override void Load()
        {

			TutorialGameData data = new TutorialGameData();
			if (!data.Load(GetDataPath()))
			{
				return;
			}


			TutorialLoader(data.yirdData, tutorialMode);
        }

        static void TutorialLoader(YirdData yirdData, PeGameMgr.ETutorialMode tutorialMode)
        {
            Debug.Log("Now Load Tutorial Scene, Mode is: " + tutorialMode.ToString());

			Debug.Log(System.DateTime.Now.ToString("G") + "[Start Game Mode] " + "tutorial****************");

            bool bNewGame = true;

            PeLauncher.Instance.Add(new LoadTutorialPlayerSpawnPos(bNewGame));	//TODO this is test position

            PeLauncher.Instance.Add(new ResetGlobalData());

            PeLauncher.Instance.Add(new LoadCamera());

			PeLauncher.Instance.Add(new LoadEditedTerrain(bNewGame, string.Empty));//yirdData.terrainPath));
            //PeLauncher.Instance.Add(new LoadEditedTerrain(bNewGame, yirdData.terrainPath));
			
			PeLauncher.Instance.Add(new LoadItemAsset(bNewGame));

            PeLauncher.Instance.Add(new LoadWaveSystem());

            PeLauncher.Instance.Add(new LoadCustomDoodad(bNewGame, yirdData.GetDoodads()));

            //PeLauncher.Instance.Add(new LoadCustomDragItem(bNewGame, customGame.GetItems()));

            PeLauncher.Instance.Add(new LoadCreature(bNewGame));

			PeLauncher.Instance.Add(new LoadEntityCreator(bNewGame));

            //PeLauncher.Instance.Add(new LoadCustomStory(bNewGame, customGame));

            PeLauncher.Instance.Add(new LoadGUI());

            PeLauncher.Instance.Add(new LoadCustomMap(bNewGame));

            PeLauncher.Instance.Add(new InitBuildManager(bNewGame)); //Add by  wang can

            PeLauncher.Instance.Add(new LoadTutorial());

			PeLauncher.Instance.Add(new LoadTutorialInitData(bNewGame));
        }
    }

    public class CreationPlayerTypeLoader : PlayerTypeLoader
    {
        public override void Load()
        {
            CreationModeLoader();
        }

        static void CreationModeLoader()
        {
            PeLauncher.Instance.Add(new OpenVCEditor());
        }
    }

    public class MultiPlayerTypeLoader:PlayerTypeLoader
    {
		const int VERSION_0000 = 0;
		const int VERSION_0001 = VERSION_0000 + 1;
		const int CURRENT_VERSION = VERSION_0001;
		
		string mGameName;
		string mYirdName;
		
		bool bNewGame = true;
		
		public string yirdName
		{
			get
			{
				return mYirdName;
			}
		}
		
		public string gameName
		{
			get
			{
				return mGameName;
			}
		}
		
		public void SetYirdName(string yirdName)
		{
			mYirdName = yirdName;
		}

		public void New(PeGameMgr.ESceneMode eSceneMode, string gameName)
		{
			bNewGame = true;
			sceneMode = eSceneMode;
			mGameName = gameName;
			mYirdName = null;
		}

        public override void Load()
        {
            Money.Digital = true;
			MultiGame game = null;
            if (sceneMode == PeGameMgr.ESceneMode.Adventure)
            {
				game = new MultiGameAdventure();
            }
			else if(sceneMode == PeGameMgr.ESceneMode.Story)
			{
				game = new MultiGameStory();
			}
            else if (sceneMode == PeGameMgr.ESceneMode.Build)
            {
				game = new MultiGameBuild();
            }
			else if (sceneMode == PeGameMgr.ESceneMode.Custom)
			{
				game = new MultiGameCustom();
			}
			if (game != null)
			{
				game.Load(bNewGame, mYirdName);
				
				//mYirdName is null in new game
				mYirdName = game.yirdName;
			}
        }

		#region mulimport mulexport
		public void Import(byte[] buffer)
		{
			bNewGame = false;
			
			PETools.Serialize.Import(buffer, (r) =>
			                         {
				int version = r.ReadInt32();
				if (version > CURRENT_VERSION)
				{
					Debug.LogError("error version:" + version);
				}
				
				GameTime.Timer.Second = r.ReadDouble();
				if (version >= VERSION_0001)
				{
					GameTime.Timer.ElapseSpeed = r.ReadSingle();
					GameTime.Timer.ElapseSpeedBak = -1f;
				}
				else
				{
					GameTime.Timer.ElapseSpeed = GameTime.NormalTimeSpeed;
					GameTime.Timer.ElapseSpeedBak = -1f;
				}
				
				GameTime.PlayTime.Second = r.ReadInt32();
				
				sceneMode = (PeGameMgr.ESceneMode)r.ReadInt32();
				
				mGameName = PETools.Serialize.ReadNullableString(r);
				mYirdName = PETools.Serialize.ReadNullableString(r);
//				PeGameMgr.yirdName = mYirdName;
				
				Money.Digital = r.ReadBoolean();
			});
		}
		
		public void Export(BinaryWriter w)
		{
			w.Write((int)CURRENT_VERSION);
			
			w.Write((double)GameTime.Timer.Second);
			if(GameTime.Timer.ElapseSpeedBak < 0f){
				w.Write((float)GameTime.Timer.ElapseSpeed);
			} else {
				w.Write((float)GameTime.Timer.ElapseSpeedBak);
			}
			
			w.Write((int)GameTime.PlayTime.Second);
			
			w.Write((int)sceneMode);
			
			PETools.Serialize.WriteNullableString(w, mGameName);
			PETools.Serialize.WriteNullableString(w, mYirdName);
			
			w.Write((bool)Money.Digital);
		}
		#endregion
	}
	
	public class SinglePlayerTypeLoader : PlayerTypeLoader
	{
		const int VERSION_0000 = 0;
		const int VERSION_0001 = VERSION_0000 + 1;
		const int VERSION_0002 = VERSION_0001 + 1;
		const int CURRENT_VERSION = VERSION_0002;
		
		string mGameName;
		string mYirdName;
		string mUID;
		
		bool bNewGame = true;
		
		public string yirdName
		{
			get
			{
				return mYirdName;
			}
		}

        public string gameName
        {
            get
            {
                return mGameName;
            }
        }

		public string UID
		{
			get
			{
				return mUID;
			}
		}

        public void SetYirdName(string yirdName)
        {
            mYirdName = yirdName;
        }

        public override void Load()
        {
            SingleGame singleGame = null;

            if (sceneMode == PeGameMgr.ESceneMode.Story)
            {
                singleGame = new SingleGameStory();
            }
            else if (sceneMode == PeGameMgr.ESceneMode.Adventure)
            {
                singleGame = new SingleGameAdventure();
            }
            else if (sceneMode == PeGameMgr.ESceneMode.Build)
            {
                singleGame = new SingleGameBuild();                
            }
            else if (sceneMode == PeGameMgr.ESceneMode.Custom)
            {
                singleGame = new SingleGameCustom(mUID, mGameName);
            }
            else
            {
                Debug.LogError("error scene mode:"+sceneMode);
            }

            if (singleGame != null)
            {
                singleGame.Load(bNewGame, mYirdName);

                //mYirdName is null in new game
                mYirdName = singleGame.yirdName;
            }
        }

        public void New(PeGameMgr.ESceneMode eSceneMode, string uid, string gameName)
        {
            bNewGame = true;
            sceneMode = eSceneMode;
            mGameName = gameName;
            mYirdName = null;
			mUID = uid;

            InitNew(eSceneMode);
        }

        static void InitNew(Pathea.PeGameMgr.ESceneMode eSceneMode)
        {
            //GameTime.PlayTime.Reset();

            if (eSceneMode == Pathea.PeGameMgr.ESceneMode.Story)
            {
                GameTime.Timer.Day = GameTime.StoryModeStartDay;
                GameTime.Timer.ElapseSpeed = GameTime.NormalTimeSpeed;
                Money.Digital = false;
            }
            else if (eSceneMode == Pathea.PeGameMgr.ESceneMode.Adventure)
            {
                GameTime.Timer.Day = GameTime.StoryModeStartDay;
                GameTime.Timer.ElapseSpeed = GameTime.NormalTimeSpeed;
                Money.Digital = true;
            }
            else if (eSceneMode == Pathea.PeGameMgr.ESceneMode.Build)
            {
                GameTime.Timer.Day = GameTime.BuildModeStartDay;
                GameTime.Timer.ElapseSpeed = 0;
                Money.Digital = true;
            }
            else if (eSceneMode == PeGameMgr.ESceneMode.Custom)
            {
                GameTime.Timer.Day = GameTime.StoryModeStartDay;
                GameTime.Timer.ElapseSpeed = GameTime.NormalTimeSpeed;
                Money.Digital = true;
            }

        }

        #region import export
        public void Import(byte[] buffer)
        {
            bNewGame = false;

            PETools.Serialize.Import(buffer, (r) =>
            {
                int version = r.ReadInt32();
                if (version > CURRENT_VERSION)
                {
                    Debug.LogError("error version:" + version);
                }

                GameTime.Timer.Second = r.ReadDouble();
                if (version >= VERSION_0001)
                {
                    GameTime.Timer.ElapseSpeed = r.ReadSingle();
					GameTime.Timer.ElapseSpeedBak = -1f;
                }
                else
                {
                    GameTime.Timer.ElapseSpeed = GameTime.NormalTimeSpeed;
					GameTime.Timer.ElapseSpeedBak = -1f;
                }

                GameTime.PlayTime.Second = r.ReadInt32();

                sceneMode = (PeGameMgr.ESceneMode)r.ReadInt32();

                mGameName = PETools.Serialize.ReadNullableString(r);
                mYirdName = PETools.Serialize.ReadNullableString(r);
				if (version >= VERSION_0002)
				{
					mUID = PETools.Serialize.ReadNullableString(r);
				}
                //PeGameMgr.yirdName = mYirdName;

                Money.Digital = r.ReadBoolean();
            });
        }

		public void Export(BinaryWriter w)
        {
            w.Write((int)CURRENT_VERSION);

            w.Write((double)GameTime.Timer.Second);
			if(GameTime.Timer.ElapseSpeedBak < 0f){
				w.Write((float)GameTime.Timer.ElapseSpeed);
			} else {
				w.Write((float)GameTime.Timer.ElapseSpeedBak);
			}

            w.Write((int)GameTime.PlayTime.Second);

            w.Write((int)sceneMode);

            PETools.Serialize.WriteNullableString(w, mGameName);
			if(mYirdName==AdventureScene.Dungen.ToString()){
				Debug.LogError("save yird = dungen!!!");
				mYirdName = AdventureScene.MainAdventure.ToString();
			}
			PETools.Serialize.WriteNullableString(w, mYirdName);
			PETools.Serialize.WriteNullableString(w, mUID);

            w.Write((bool)Money.Digital);
        }
        #endregion        
    }


}