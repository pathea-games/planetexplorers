using UnityEngine;
using System.Collections.Generic;
using ScenarioRTL;
using System.IO;

namespace PeCustom
{
    public class MissionMgr
    {
        /// <summary> When mission run will call it. <int> == mission id </summary>
        public System.Action<int> onRunMission;
        /// <summary> When mission close will call it. <int> == mission id </summary>
        public System.Action<int, EMissionResult> onCloseMission;

        public System.Action<int> onResumeMission;

        public System.Action<int, bool> onMissionTrackChanged;

        public bool RunMission(int id)
        {
            if (m_Scenario.RunMission(id))
            {
                m_MissionIDs.Add(id);

                MissionProperty mp = new MissionProperty();
                mp.Parse(m_Scenario.GetMissionProperties(id), m_Scenario.GetMissionName(id));
                m_MissionProperties.Add(id, mp);
                if (!m_MissionTrack.ContainsKey(id))
                    m_MissionTrack.Add(id, true);

                if (onRunMission != null)
                    onRunMission(id);
                return true;
            }
            else
            {
                Debug.LogError("Run mission [" + id + "] failed !");
            }

            return false;
        }

        public bool CloseMission(int id, EMissionResult result)
        {
            if (m_Scenario.IsMissionRunning(id))
            {
                if (onCloseMission != null)
                    onCloseMission(id, result);

                if (m_Goals != null)
                {
                    FreeGoals(id);
                    m_Goals.Remove(id);
                }

                if (m_Scenario.CloseMission(id))
                {
                    m_MissionIDs.Remove(id);
                    m_MissionProperties.Remove(id);
                    m_MissionTrack.Remove(id);
                    return true;
                }
            }
            else
            {
                Debug.LogError("Close mission [" + id + "] failed !");
            }

            return false;
        }

        public void ResumeMission()
        {
            m_Scenario.Resume();
            int[] running_ids = m_Scenario.runningMissionIds;
            for (int i = 0; i < running_ids.Length; i++)
            {
                m_MissionIDs.Add(running_ids[i]);
                int id = m_MissionIDs[i];
                if (!m_MissionProperties.ContainsKey(id))
                {
                    MissionProperty mp = new MissionProperty();
                    mp.Parse(m_Scenario.GetMissionProperties(id), m_Scenario.GetMissionName(id));
                    m_MissionProperties.Add(id, mp);
                }
                if (!m_MissionTrack.ContainsKey(id))
                    m_MissionTrack.Add(id, true);

                if (onResumeMission != null)
                    onResumeMission(m_MissionIDs[i]);
            }
        }

        public MissionProperty GetMissionProperty(int id)
        {
            if (m_MissionProperties.ContainsKey(id))
                return m_MissionProperties[id];
            return null;
        }

        public bool MissionIsTracked(int mission_id)
        {
            if (m_MissionTrack.ContainsKey(mission_id))
            {
                return m_MissionTrack[mission_id];
            }
            else
                return false;
        }

        public void SetMissionIsTracked(int mission_id, bool isTracked)
        {
            if (m_MissionTrack.ContainsKey(mission_id))
            {
                if (m_MissionTrack[mission_id] != isTracked)
                {
                    m_MissionTrack[mission_id] = isTracked;

                    if (onMissionTrackChanged != null)
                        onMissionTrackChanged(mission_id, isTracked);
                }
            }
        }

        public MissionMgr(Scenario scenario)
        {
            m_Scenario = scenario;
            m_MissionIDs = new List<int>(10);
            m_MissionProperties = new Dictionary<int, MissionProperty>(10);
            m_MissionTrack = new Dictionary<int, bool>(10);
            InitGoals();
        }


        public void Free()
        {
            FreeGoals();
        }

        private Scenario m_Scenario;
        private List<int> m_MissionIDs;

        private Dictionary<int, MissionProperty> m_MissionProperties;
        private Dictionary<int, bool> m_MissionTrack;

        //
        // Mission Goal
        //

        // Key1 : Mission ID in scenario. Key2 : Goal ID
        Dictionary<int, SortedList<int, MissionGoal>> m_Goals;
        
        public System.Action<int, int> onSetMissionGoal;
        public System.Action<int, int> onUnsetMissionGoal;

		void InitGoals ()
		{
			m_Goals = new Dictionary<int, SortedList<int, MissionGoal>>(10);
		}

		void FreeGoals ()
		{
			if (m_Goals != null)
			{
				foreach (var kvp in m_Goals)
					FreeGoals(kvp.Key);
				m_Goals.Clear();
			}
		}

		void FreeGoals (int id)
		{
			if (m_Goals != null)
			{
				if (m_Goals.ContainsKey(id))
				{
					SortedList<int, MissionGoal> goal_list = m_Goals[id];
					foreach (var goal in goal_list)
						goal.Value.Free();
					goal_list.Clear();
				}
			}
		}

        public SortedList<int, MissionGoal> GetGoals (int mission_id)
        {
            if (m_Goals.ContainsKey(mission_id))
            {
                return m_Goals[mission_id];
            }
            
            return null;
        }

        public MissionGoal GetGoal (int goal_id, int mission_id)
        {
            if (m_Goals.ContainsKey(mission_id))
            {
                if (m_Goals[mission_id].ContainsKey(goal_id))
                    return m_Goals[mission_id][goal_id];
            }

            return null;
        }

		public void UpdateGoals ()
		{
			foreach (var kvp in m_Goals)
			{
				SortedList<int, MissionGoal> goal_list = m_Goals[kvp.Key];

				for (int i = 0; i < goal_list.Count; ++i)
					goal_list.Values[i].Update();
			}
		}

		public void SetBoolGoal (int id, string text, int missionId, bool achieved)
		{
			if (!m_Scenario.IsMissionActive(missionId))
				return;
			if (!m_Goals.ContainsKey(missionId))
				m_Goals[missionId] = new SortedList<int, MissionGoal>(4);
			var goal_list = m_Goals[missionId];
			if (!goal_list.ContainsKey(id))
			{
				goal_list[id] = new MissionGoal_Bool();
				goal_list[id].onAchieve = onGoalAchieve;
				goal_list[id].Init();
			}
			if (!(goal_list[id] is MissionGoal_Bool))
			{
				goal_list[id].Free();
				goal_list[id] = new MissionGoal_Bool();
				goal_list[id].onAchieve = onGoalAchieve;
				goal_list[id].Init();
			}

			var goal = goal_list[id] as MissionGoal_Bool;
			goal.id = id;
			goal.text = text;
			goal.missionId = missionId;
			goal.achieved = achieved;

            if (onSetMissionGoal != null)
                onSetMissionGoal(id, missionId);
                
		}

		public void SetItemGoal (int id, string text, int missionId, OBJECT item, ECompare compare, int amount)
		{
			if (!m_Scenario.IsMissionActive(missionId))
				return;
			if (!m_Goals.ContainsKey(missionId))
				m_Goals[missionId] = new SortedList<int, MissionGoal>(4);
			var goal_list = m_Goals[missionId];
			if (!goal_list.ContainsKey(id))
			{
				goal_list[id] = new MissionGoal_Item();
				goal_list[id].onAchieve = onGoalAchieve;
				goal_list[id].Init();
			}
			if (!(goal_list[id] is MissionGoal_Item))
			{
				goal_list[id].Free();
				goal_list[id] = new MissionGoal_Item();
				goal_list[id].onAchieve = onGoalAchieve;
				goal_list[id].Init();
			}

			var goal = goal_list[id] as MissionGoal_Item;
			goal.id = id;
			goal.text = text;
			goal.missionId = missionId;
			goal.item = item;
			goal.compare = compare;
			goal.target = amount;

            if (onSetMissionGoal != null)
                onSetMissionGoal(id, missionId);
        }

		public void SetKillGoal (int id, string text, int missionId, OBJECT monster, ECompare compare, int amount)
		{
			if (!m_Scenario.IsMissionActive(missionId))
				return;
			if (!m_Goals.ContainsKey(missionId))
				m_Goals[missionId] = new SortedList<int, MissionGoal>(4);
			var goal_list = m_Goals[missionId];
			if (!goal_list.ContainsKey(id))
			{
				goal_list[id] = new MissionGoal_Kill();
				goal_list[id].onAchieve = onGoalAchieve;
				goal_list[id].Init();
			}
			if (!(goal_list[id] is MissionGoal_Kill))
			{
				goal_list[id].Free();
				goal_list[id] = new MissionGoal_Kill();
				goal_list[id].onAchieve = onGoalAchieve;
				goal_list[id].Init();
			}

			var goal = goal_list[id] as MissionGoal_Kill;
			goal.id = id;
			goal.text = text;
			goal.missionId = missionId;
			goal.monster = monster;
			goal.compare = compare;
			goal.target = amount;

            if (onSetMissionGoal != null)
                onSetMissionGoal(id, missionId);
        }

		public void UnsetGoal (int id, int missionId)
		{
			if (m_Goals.ContainsKey(missionId))
			{
				var goal_list = m_Goals[missionId];
				if (goal_list.ContainsKey(id))
				{
					goal_list[id].Free();
					goal_list.Remove(id);
				}

				if (goal_list.Count == 0)
					m_Goals.Remove(missionId);

                if (onUnsetMissionGoal != null)
                    onUnsetMissionGoal(id, missionId);
            }
		}

		public bool? GoalAchieved (int id, int missionId)
		{
			if (m_Goals.ContainsKey(missionId))
			{
				var goal_list = m_Goals[missionId];
				if (goal_list.ContainsKey(id))
					return goal_list[id].achieved;
			}
			return null;
		}

		void onGoalAchieve (int id, int missionId)
		{
			if (OnGoalAchieve != null)
				OnGoalAchieve(id, missionId);
		}

		#region IO

        public void Import (BinaryReader r)
        {
            r.ReadInt32();
            ImportGoals(r);
            ImportMisc(r);
        }

        public void Export (BinaryWriter w)
        {
            w.Write(0);
            ExportGoals(w);
            ExportMisc(w);
        }

		void ImportGoals (BinaryReader r)
		{
			r.ReadInt32();

			FreeGoals();
			InitGoals();
			int count = 0;
			while (true)
			{
				int gcls = r.ReadInt32();
				if (gcls == 1)
				{
					int id = r.ReadInt32();
					int missionId = r.ReadInt32();
					string text = r.ReadString();
					bool achieved = r.ReadBoolean();
					SetBoolGoal(id, text, missionId, achieved);
				}
				else if (gcls == 2)
				{
					int id = r.ReadInt32();
					int missionId = r.ReadInt32();
					string text = r.ReadString();
					OBJECT obj;
					obj.type = (OBJECT.OBJECTTYPE)r.ReadInt32();
					obj.Group = r.ReadInt32();
					obj.Id = r.ReadInt32();
					ECompare compare = (ECompare)r.ReadInt32();
					int target = r.ReadInt32();
					SetItemGoal(id, text, missionId, obj, compare, target);
				}
				else if (gcls == 3)
				{
					int id = r.ReadInt32();
					int missionId = r.ReadInt32();
					string text = r.ReadString();
					OBJECT obj;
					obj.type = (OBJECT.OBJECTTYPE)r.ReadInt32();
					obj.Group = r.ReadInt32();
					obj.Id = r.ReadInt32();
					ECompare compare = (ECompare)r.ReadInt32();
					int target = r.ReadInt32();
					SetKillGoal(id, text, missionId, obj, compare, target);
					int current = r.ReadInt32();
					if (m_Goals.ContainsKey(missionId) && m_Goals[missionId].ContainsKey(id))
					{
						MissionGoal g = m_Goals[missionId][id];
						if (g != null && g as MissionGoal_Kill != null)
							(g as MissionGoal_Kill).current = current;
					}
				}

				if (gcls == -1)
					break;
				if (++count > 1024)
					break;
			}

            
		}

		void ExportGoals (BinaryWriter w)
		{
			w.Write(0);

			foreach (var kvp in m_Goals)
			{
				var goal_list = kvp.Value;
				for (int i = 0; i < goal_list.Count; ++i)
				{
					MissionGoal goal = goal_list.Values[i];
					if (goal is MissionGoal_Bool)
					{
						MissionGoal_Bool _goal = goal as MissionGoal_Bool;
						w.Write(1);
						w.Write(_goal.id);
						w.Write(_goal.missionId);
						w.Write(_goal.text);
						w.Write(_goal.achieved);
					}
					else if (goal is MissionGoal_Item)
					{
						MissionGoal_Item _goal = goal as MissionGoal_Item;
						w.Write(2);
						w.Write(_goal.id);
						w.Write(_goal.missionId);
						w.Write(_goal.text);
						w.Write((int)(_goal.item.type));
						w.Write(_goal.item.Group);
						w.Write(_goal.item.Id);
						w.Write((int)_goal.compare);
						w.Write(_goal.target);
					}
					else if (goal is MissionGoal_Kill)
					{
						MissionGoal_Kill _goal = goal as MissionGoal_Kill;
						w.Write(3);
						w.Write(_goal.id);
						w.Write(_goal.missionId);
						w.Write(_goal.text);
						w.Write((int)(_goal.monster.type));
						w.Write(_goal.monster.Group);
						w.Write(_goal.monster.Id);
						w.Write((int)_goal.compare);
						w.Write(_goal.target);
						w.Write(_goal.current);
					}
					else
					{
						w.Write(0);
					}
				}
			}
            w.Write(-1);

           
			
		}

        void ImportMisc(BinaryReader r)
        {
            r.ReadInt32();
            int count = r.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                m_MissionTrack.Add(r.ReadInt32(), r.ReadBoolean());
            }
        }

        void ExportMisc(BinaryWriter w)
        {
            w.Write(0);
            w.Write(m_MissionTrack.Count);
            foreach (var kvp in m_MissionTrack)
            {
                w.Write(kvp.Key);
                w.Write(kvp.Value);
            }
        }
        #endregion

        public event System.Action<int, int> OnGoalAchieve;
	}
}