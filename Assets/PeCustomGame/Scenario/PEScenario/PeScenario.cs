using UnityEngine;
using ScenarioRTL;
using System.IO;
using Pathea;

namespace PeCustom
{
    public class PeScenario : PeCustomScene.SceneElement, IMonoLike, Pathea.ISerializable, ISceneController
    {
        // Binder
        HashBinder mBinder = new HashBinder();
        public HashBinder Binder { get { return mBinder; } }

        public const string ArchiveKey = "CustomScenario";
        private Scenario m_Scenario;

        public Scenario core { get { return m_Scenario;} }

        public int playerId { get; private set; }

        // Sub-systems
        public TickMgr tickMgr { get; private set; }
		public CustomGUIMgr guiMgr { get; private set; }
		public StopwatchMgr stopwatchMgr { get; private set; }
        public MissionMgr missionMgr { get; private set; }
        public DialogMgr dialogMgr { get; private set; }

        private bool _isNew = true;
        private bool _initialized = false;
		public bool isRunning { get; private set; }

        public void Init (string file_path, int player_id)
        {
            if (!_initialized)
            {
                m_Scenario = Scenario.Create(file_path);
				if (m_Scenario != null)
				{
                    playerId = player_id;
                    InitSubSystems();
                    
                    ArchiveMgr.Instance.Register(ArchiveKey, this);
					_initialized = true;
					m_Scenario.SetAsDebugTarget();
				}
            }
        }

        void InitSubSystems()
        {
            tickMgr = new TickMgr();
            guiMgr = new CustomGUIMgr();
            stopwatchMgr = new StopwatchMgr();
            missionMgr = new MissionMgr(m_Scenario);
            dialogMgr = new DialogMgr();
        }

        /// <summary>
		/// Accept the notification for Views.
		/// </summary>
		/// <param name="msg_type">Msg_type.</param>
		/// <param name="data">Data.</param>
        public void OnNotification(ESceneNoification msg_type, params object[] data)
        {
            if (!_initialized)
                return;

            switch (msg_type)
            {
            case ESceneNoification.SceneBegin:
                // [INIT SCENARIO]
                InitUI();

                if (_isNew)
                {
                    missionMgr.RunMission(0);
                    Debug.Log("[Custom Scenario] Scene Begin ---- Run Mission (0)");
                }
                else
                {
                    missionMgr.ResumeMission();
                    Debug.Log("[Custom Scenario] Scene Begin ---- Resume Mission");
                }
				
                InitOther();
				isRunning = true;
                break;
			case ESceneNoification.SceneEnd:
				Close();
				break;
            }
        }

        public void InitUI()
        {
            GameUI.Instance.mStopwatchList.gameObject.SetActive(true);
            GameUI.Instance.mNpcDialog.Init();
            GameUI.Instance.mNPCSpeech.Init();
            GameUI.Instance.mMissionGoal.Init();
            GameUI.Instance.mCustomMissionTrack.Init();
        }

        public void InitOther ()
        {
            if (EntityMgr.Instance != null && EntityMgr.Instance.eventor != null)
            {
                EntityMgr.Instance.eventor.Subscribe(OnMouseClickEntity);
            }

            CustomGameData customData = CustomGameData.Mgr.Instance.curGameData;
            // Init force setting
            foreach (var force in customData.mForceDescs)
            {
                ForceSetting.Instance.AddForceDesc(force);
            }
            foreach (var player in customData.mPlayerDescs)
            {
                ForceSetting.Instance.AddPlayerDesc(player);
            }
        }

        public void CloseUI()
        {
            GameUI.Instance.mNpcDialog.Close();
            GameUI.Instance.mNPCSpeech.Close();
            GameUI.Instance.mMissionGoal.Close();
            GameUI.Instance.mCustomMissionTrack.Close();
        }

        void OnMouseClickEntity(object sender, EntityMgr.RMouseClickEntityEvent e)
        {
            if (e.entity == null)
                return;

            CommonCmpt cc = e.entity.commonCmpt;
            if (cc != null && cc.entityProto.proto == EEntityProto.Npc)
            {
                if (GameUI.Instance.mNpcDialog.dialogInterpreter.SetNpoEntity(e.entity))
                    GameUI.Instance.mNpcDialog.Show();
            }

        }
        #region IMonoLike
        public void Start ()
        {

        }

        public void Update ()
        {
            if (!_initialized)
                return;

			if (isRunning)
			{
            	m_Scenario.Update();

				tickMgr.Tick();
				stopwatchMgr.Update(Time.deltaTime);
				missionMgr.UpdateGoals();
			}
		}

		public static bool s_ShowTools = false;
        private string mNPCid = "";
		public void OnGUI ()
		{
			if (!_initialized)
				return;
			
			guiMgr.DrawGUI();

			if (s_ShowTools)
            {
                mNPCid = GUI.TextArea(new Rect(100, 130 + 140, 80, 20), mNPCid);
                if (GUI.Button(new Rect(10, 130 + 140, 70, 20), "EntityId")) 
                {
                    OBJECT obj;
                    obj.type = OBJECT.OBJECTTYPE.WorldObject;
                    obj.Group = 0;
                    obj.Id = System.Convert.ToInt32(mNPCid);
                    Pathea.PeEntity entity = PeScenarioUtility.GetEntity(obj);
                    if (entity != null) 
                    {
                        Vector3 pos = PeCreature.Instance.mainPlayer.position;
                        pos.z += 2;
                        pos.y += 1;

                        entity.peTrans.position = pos;
                        entity.NpcCmpt.FixedPointPos = pos;
                    }
                }
            }
		}

        public void OnDestroy ()
        {
			Close();
        }
        #endregion

		public void Close ()
		{
			if (_initialized)
			{
				m_Scenario.Close();
				m_Scenario = null;
				_initialized = false;
				isRunning = false;
			}
		}


        #region RECORD

        public void Restore ()
        {
			// 这里有点问题。。读档前除了Scenario.Create以外不能跑任何mission
			// 相关问题见 PeGameLoader_Custom.cs

            if (!_initialized)
                return;

            byte[] data = ArchiveMgr.Instance.GetData(ArchiveKey);
            if (data != null)
            {
                _isNew = false;

                using (MemoryStream ms_iso = new MemoryStream(data))
                {
                    BinaryReader r = new BinaryReader(ms_iso);
					r.ReadInt32();
                    m_Scenario.Import(r);
                    // TODO: 子系统的Import
					missionMgr.Import(r);
					stopwatchMgr.Import(r);
                    dialogMgr.Import(r);
                }

                Debug.Log("[Custom Scenario] restore the Data");
            }
        }

        void Pathea.ISerializable.Serialize(PeRecordWriter w)
        {
			if (_initialized)
			{
				w.Write(0);
            	w.Write(m_Scenario.Export());

				// TODO: 子系统的Export
				missionMgr.Export(w.binaryWriter);
				stopwatchMgr.Export(w.binaryWriter);
                dialogMgr.Export(w.binaryWriter);
			}
        }
        #endregion

    }
}
