using System.Collections.Generic;
using ScenarioRTL.IO;
using System.IO;

namespace ScenarioRTL
{
    public class Scenario
    {
        /// <summary>Create a scenario and load all the missions under the specified file path.</summary>
        /// <param name="file_path">A directory path of all the missions' xml files.</param>
        public static Scenario Create (string file_path)
        {
            if (!Directory.Exists(file_path))
                return null;

            Scenario scenario = new Scenario();

            scenario.LoadDir(file_path);

            return scenario;
        }

		/// <summary>Run a mission. Scenario must be initialized before calling this method.</summary>
		/// <param name="dataid">Specify mission ID.</param>
		public bool RunMission (int id)
        {
			if (!m_MissionRaws.ContainsKey(id))
                return false;

			if (m_RunningMissions.ContainsKey(id))
				return false;

			int instid = m_MaxMissionId + 1;
			m_MaxMissionId = instid;

			Mission mis = new Mission(instid, m_MissionRaws[id], this);
			m_MissionInsts.Add(instid, mis);
			m_MissionVariables[mis] = m_Variables.CreateChild();
            mis.Init();
            mis.Run();
			m_RunningMissions[id] = mis;

            return true;
        }      

		/// <summary>Close a mission.</summary>
		/// <param name="id">Specify mission ID.</param>
		public bool CloseMission (int id)
		{
			if (!m_MissionRaws.ContainsKey(id))
				return false;
			
			if (!m_RunningMissions.ContainsKey(id))
				return false;
			
			Mission mis = m_RunningMissions[id];
			mis.Close();
			m_RunningMissions.Remove(id);

			return true;
		}

		public bool IsMissionRunning (int id)
		{
			if (!m_MissionRaws.ContainsKey(id))
				return false;

			if (!m_RunningMissions.ContainsKey(id))
				return false;


			return m_RunningMissions[id].enabled;
		}

		public bool IsMissionActive (int id)
		{
			if (!m_MissionRaws.ContainsKey(id))
				return false;

			return m_RunningMissions.ContainsKey(id);
		}

		/// <summary>Get the name of a specified mission.</summary>
		/// <param name="id">Specify mission ID.</param>
		public string GetMissionName (int id)
		{
			if (!m_MissionRaws.ContainsKey(id))
				return "";

			return m_MissionRaws[id].name;
		}

		/// <summary>Get the raw property data from a mission, the property data can be customized in the xml-attribute.</summary>
		/// <param name="id">Specify mission ID.</param>
		public ParamRaw GetMissionProperties (int id)
		{
			if (!m_MissionRaws.ContainsKey(id))
				return null;

			return m_MissionRaws[id].properties;
		}

        /// <summary>Load a single xml file, and restore it as a mission raw data.</summary>
        /// <param name="xmlpath">Specify the path of xml file.</param>
        public void LoadFile (string xmlpath)
        {
            MissionRaw raw = MissionRaw.Create(xmlpath);
            m_MissionRaws[raw.id] = raw;
        }

		/// <summary>Load all the missions under the specified file path.</summary>
		/// <param name="dir">A directory contains the missions' xml files.</param>
        public void LoadDir(string dir)
        {
			string[] files = Directory.GetFiles(dir, "*.xml", SearchOption.AllDirectories);
            foreach (string f in files)
                LoadFile(f);
        }

        public void Resume()
        {
            m_Pause = false;

            foreach (var kvp in m_RunningMissions)
            {
                kvp.Value.Resume();
            }
        }

        public void Pause ()
        {
            m_Pause = true;

            foreach (var kvp in m_RunningMissions)
            {
                kvp.Value.Pause();
            }

        }

        /// <summary>Update the scenario logic.</summary>
        public void Update ()
        {
            if (!m_Pause)
            {
                for (int i = 0; i < m_ActionThreads.Count;)
                {
                    m_ActionThreads[i].ProcessAction();

                    if (m_ActionThreads[i].isFinished)
                        m_ActionThreads.RemoveAt(i);
                    else
                        i++;
                }
				for (int i = 0; i < m_ConditionThreads.Count;)
				{
					ConditionThread ct = m_ConditionThreads[i];
					bool? res = ct.Check();

					if (res != null)
						m_ConditionThreads.RemoveAt(i);
					else
						i++;
					
					if (res == true)
						ct.Pass();
					else if (res == false)
						ct.Fail();
				}
            }
        }

		/// <summary>Close the scenario.</summary>
		public void Close ()
		{
			foreach (var kvp in m_MissionInsts)
			{
				if (kvp.Value != null)
					kvp.Value.Close();
			}
			m_MissionInsts.Clear();
		}

        #region IO
        const int VERSION = 0x00000001;

		/// <summary>Restore the scenario data from a byte buffer</summary>
        public bool Import (byte[] data)
        {
            if (data == null)
                return false;

            using (MemoryStream ms_iso = new MemoryStream(data))
            {
                BinaryReader r = new BinaryReader(ms_iso);
                Import(r);

                ms_iso.Close();
            }

            return true;
        }

		/// <summary>Store a scenario data to a byte buffer</summary>
        public byte[] Export ()
        {
            byte[] data = null;
            using (MemoryStream ms_iso = new MemoryStream())
            {
                BinaryWriter w = new BinaryWriter(ms_iso);
                Export(w);

                data = ms_iso.ToArray();
                ms_iso.Close();
            }

            return data;
        }

		/// <summary>Restore the scenario data from a opening stream</summary>
        public void Import (BinaryReader r)
        {
            int version = r.ReadInt32();

            switch (version)
            {
                case 0x00000001:
                {
                    m_Pause = true;
                    int count = r.ReadInt32();
					m_MaxMissionId = 0;
                    for (int i = 0; i < count; i++)
                    {
                        int instId = r.ReadInt32();
						int dataId = r.ReadInt32();
						bool enabled = r.ReadBoolean();

						if (!m_MissionRaws.ContainsKey(dataId))
							throw new System.Exception("Mission raw data of id [" + dataId.ToString() + "] is missing");

						Mission mis = new Mission(instId, m_MissionRaws[dataId], this);
                        mis.Init();
                        
                        int trg_cnt = r.ReadInt32();

                        if (trg_cnt != mis.triggers.Length)
                            throw new System.Exception("Trigger count is not correct");

                        for (int j = 0; j < trg_cnt; j++)
                        {
                            mis.triggers[j].Import(r);
                        }

						VarScope vs = Variables.CreateChild();
						vs.Import(r);
						MissionVariables[mis] = vs;

						m_MissionInsts.Add(instId, mis);
						if (enabled)
						{
							//mis.Resume();
							m_RunningMissions.Add(dataId, mis);
						}

						if (instId > m_MaxMissionId)
							m_MaxMissionId = instId;
                    }

					Variables.Import(r);

                    // restore action thread
                    count = r.ReadInt32();

                    for (int i = 0; i < count; i++)
                    {
                        int instId = r.ReadInt32();
						int dataId = r.ReadInt32();
                        int trigger_idx = r.ReadInt32();
                        int group = r.ReadInt32();
                        int cur_idx = r.ReadInt32();

						if (!m_MissionRaws.ContainsKey(dataId))
							throw new System.Exception("Mission raw data of id [" + dataId.ToString() + "] is missing");

						if (!m_MissionInsts.ContainsKey(instId))
							throw new System.Exception("Mission instance of instId [" + dataId.ToString() + "] is missing");
                        

						MissionRaw mis_raw = m_MissionRaws[dataId];
						ActionThread thread = null;
						Mission mis = m_MissionInsts[instId];
						mis.triggers[trigger_idx].FillActionCache(group);

						thread = new ActionThread(mis.triggers[trigger_idx], trigger_idx, group, 
							mis_raw.triggers[trigger_idx].actions[group], mis.triggers[trigger_idx].GetActionCache(group), cur_idx);

						mis.triggers[trigger_idx].RegisterActionThreadEvent(thread);

						AddActionThread(thread);
						
                        thread.CreateCurrAction();
                        if (thread.currAction != null)
                            thread.currAction.RestoreState(r);
                    }
                }
				break;
            }
        }

		/// <summary>Store the scenario data from a opening stream</summary>
        public void Export (BinaryWriter w)
        {
            w.Write(VERSION);

            w.Write(m_MissionInsts.Count);	// Mission instance count
            foreach (var kvp in m_MissionInsts)
            {
				w.Write(kvp.Value.instId);	// Mission instId
                w.Write(kvp.Value.dataId);	// Mission dataId
				w.Write(kvp.Value.enabled); // Mission enabled

				w.Write(kvp.Value.triggers.Length); // Trigger count
                foreach (var trigger in kvp.Value.triggers)
                    trigger.Export(w);	// Triggers

				MissionVariables[kvp.Value].Export(w); // Mission variables
            }

			Variables.Export(w);	// Scenario variables

			w.Write(m_ActionThreads.Count);
            foreach (var thread in m_ActionThreads)
            {
				w.Write(thread.trigger.mission.instId);
				w.Write(thread.trigger.mission.dataId);
				w.Write(thread.triggerIndex);
                w.Write(thread.group);
                
                w.Write(thread.currIndex);
                if (thread.currAction != null)
                    thread.currAction.StoreState(w);
            }
        }
        #endregion

        public int[] runningMissionIds
        {
            get
            {
                int[] ids = new int[m_RunningMissions.Count];
                int i = 0;
                foreach (var kvp in m_RunningMissions)
                {
                    ids[i] = kvp.Key;
                    i++;
                }

                return ids;
            }
        }
        private bool m_Pause = false;

		// Mission raw data list, key: dataId
		private Dictionary<int, MissionRaw> m_MissionRaws = new Dictionary<int, MissionRaw>();

		// Mission maxid
		private int m_MaxMissionId = 0;
		// Mission instance list, key: instanceId
		private Dictionary<int, Mission> m_MissionInsts = new Dictionary<int, Mission>();
		// Running mission instances, key: dataId
		private Dictionary<int, Mission> m_RunningMissions = new Dictionary<int, Mission>();

		// Thread list
		private List<ConditionThread> m_ConditionThreads = new List<ConditionThread>();
		private List<ActionThread> m_ActionThreads = new List<ActionThread>();

		// Variables
		private VarScope m_Variables = new VarScope();
		private VarCollection<Mission> m_MissionVariables = new VarCollection<Mission>();

		// Constructor
		private Scenario () { Asm.Init(); }

		public VarScope Variables { get { return m_Variables; } }
		public VarCollection<Mission> MissionVariables { get { return m_MissionVariables; } }

		internal void AddActionThread(ActionThread thread)
		{
			m_ActionThreads.Add(thread);
		}

		internal void AddConditionThread(ConditionThread thread)
		{
			m_ConditionThreads.Add(thread);
		}

		// Debug target
		public static DebugTarget debugTarget { get; private set; }

		/// <summary>Set this scenario as the debug target</summary>
		public void SetAsDebugTarget ()
		{
			// debugger
			debugTarget = new DebugTarget();
			debugTarget.missionRaws = m_MissionRaws;
			debugTarget.missionInsts = m_MissionInsts;
			debugTarget.runningMissions = m_RunningMissions;
			debugTarget.actionThreads = m_ActionThreads;
			debugTarget.scenarioVars = Variables;
			debugTarget.missionVars = MissionVariables;
		}
    }
}
