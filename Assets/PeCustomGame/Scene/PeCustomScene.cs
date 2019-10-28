// Custom game mode  Scene main entry
// (c) by Wu Yiqiu

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Pathea;

namespace PeCustom
{

	public partial class PeCustomScene : GLBehaviour
	{
		static PeCustomScene  _self;
		
		public static PeCustomScene Self
		{
			get
			{
				if (_self == null)
				{
					GameObject go = Resources.Load("Prefab/Custom/PeCustomScene") as GameObject;
                    if (go == null)
					{
						Debug.LogError("The Custom Scene manager load Failed");
						return null;
					}
					
					 PeCustomScene.Instantiate(go);

				}
				
				return _self;
			}
			
		}

		public PeScenario scenario { get; private set; }

		// Data Source
		SpawnDataSource mSpawnData;
        public SpawnDataSource spawnData { get { return mSpawnData; } }

		// All Controllers
		List<ISceneController> mControllers = new List<ISceneController>(5);

		// Mono like Items
		List<IMonoLike> mMlItems = null;

		void AddMonoLikeItems (IMonoLike ml)
		{
			mMlItems.Add(ml);
		}

		#region INIT & RECORD

		public void SceneRestore(YirdData yird)
		{
			mSpawnData.Restore(yird);
			//Notify(ESceneNoification.SceneBegin, false);
		}

		public void SceneNew(YirdData yird)
		{
			mSpawnData.New(yird);
			//Notify(ESceneNoification.SceneBegin, true);
		}

        public void ScenarioInit(CustomGameData data)
        {
            scenario.Init(data.missionDir, data.curPlayer.ID);
        } 

        public void ScenarioRestore()
        {
            scenario.Restore();
        }

		#endregion

		public void Notify (ESceneNoification msg_type, params object[] data)
		{
			for (int i = 0; i < mControllers.Count; i++)
			{
				mControllers[i].OnNotification(msg_type, data);
			}
		}

        public void CreateAgent (SpawnPoint sp)
        {
            Notify(ESceneNoification.CreateAgent, sp);
        }

        public void RemoveSpawnPoint (SpawnPoint sp)
        {
            Notify(ESceneNoification.RemoveSpawnPoint, sp);
        }

        public void EnableSpawnPoint (SpawnPoint sp, bool enable)
        {
            Notify(ESceneNoification.EnableSpawnPoint, sp, enable);
        }

        public void MonsterDeadEvent (SceneEntityAgent agent, MonsterSpawnPoint msp)
        {

        }

		#region UNITY_INNER_FUNC

        void OnGUI()
        {
            for (int i = 0; i < mMlItems.Count; ++i)
                mMlItems[i].OnGUI();

            //if (GUI.Button(new Rect(150, 50, 100, 40), "Get Obj"))
            //{
            //    OBJECT obj = new OBJECT();
            //    obj.type = OBJECT.OBJECTTYPE.WorldObject;
            //    obj.Group = 0;
            //    obj.Id = 38;
            //    GameObject go = PeScenarioUtility.GetGameObject(obj);
            //    if (go != null)
            //    {
            //        Debug.Log("Find the item game object");
            //    }
            //}
            
        }

        void Awake ()
		{
			_self = this;


			mMlItems = new List<IMonoLike>(10);

			// Spawn DataSource & Controllerrc
			mSpawnData = new SpawnDataSource();
			SceneAgentsContoller sac = new SceneAgentsContoller();
			sac.Binder.Bind(mSpawnData);

			mControllers.Add(sac);

            scenario = new PeScenario();
            sac.Binder.Bind(mSpawnData);

            mControllers.Add(scenario);
            
		}

		void Start()
		{
			for (int i = 0; i < mMlItems.Count; ++i)
				mMlItems[i].Start();

		}

		void Update()
		{
			for (int i = 0; i < mMlItems.Count; ++i)
				mMlItems[i].Update();


		}

		void OnDestroy()
		{
			for (int i = 0; i < mMlItems.Count; ++i)
				mMlItems[i].OnDestroy();


		}

		#endregion

		public override void OnGL ()
		{

		}
	}
}
