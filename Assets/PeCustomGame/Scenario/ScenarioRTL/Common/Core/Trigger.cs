using ScenarioRTL.IO;
using System.Collections.Generic;
using System.IO;

namespace ScenarioRTL
{
    public class Trigger
    {
        public int repeat { get; private set; }
        public int index { get; private set; }
        public bool enabled { get; private set; }

        public string name { get { return m_Raw.name; } }
        public bool multiThreaded { get { return m_Raw.multiThreaded; } }

        public List<EventListener> eventListeners { get; set; }

        public Mission mission { get; private set; }

        private TriggerRaw m_Raw;

		public List<Condition[]> m_Conditions;
		public List<Action[]> m_ActionCache;

        public Action[] GetActionCache (int grp_idx)
		{
			if (grp_idx >= 0 && grp_idx < m_ActionCache.Count ) 
				return m_ActionCache[grp_idx];
			else
				return null;
		}

        private int m_ThreadCounter = 0;
		public int threadCounter { get { return m_ThreadCounter; } }

        public Trigger (TriggerRaw _raw, Mission _mission, int _idx)
        {
            m_Raw = _raw;
            mission = _mission;
            index = _idx;
            repeat = _raw.repeat;
            enabled = true;
        }

        public bool isAlive { get { return repeat != 0; } }

        internal void InitConditions ()
        {
            if (m_Conditions == null)
            {
                m_Conditions = new List<Condition[]>();

                for (int i = 0; i < m_Raw.conditions.Count; i++)
                {
                    Condition[] cdts = new Condition[m_Raw.conditions[i].Length];
                    
                    for (int j = 0; j < cdts.Length; j++)
                    {
                        Condition cdt = Asm.CreateConditionInstance(m_Raw.conditions[i][j].classname);
                        if (cdt != null)
                        {
							cdt.Init(this, m_Raw.conditions[i][j]);
                            cdts[j] = cdt;
                        }
                    }

                    m_Conditions.Add(cdts);
                }
            }
			else
			{
				for (int i = 0; i < m_Conditions.Count; ++i)
					for (int j = 0; j < m_Conditions[i].Length; ++j)
						m_Conditions[i][j].Init(this, m_Raw.conditions[i][j]);
			}
        }

		internal void StartProcessCondition ()
        {
			ConditionThread.Create(this);
        }

        internal void StartProcessAction ()
        {
            repeat--;

            for (int i = 0; i < m_Raw.actions.Count; i++)
            {
                FillActionCache(i);

                ActionThread thread = new ActionThread(this, index, i, m_Raw.actions[i], m_ActionCache[i]);
                thread.ProcessAction();

                if (!thread.isFinished)
                {
                    mission.scenario.AddActionThread(thread);
                    RegisterActionThreadEvent(thread);
                }
            }

            if (m_ThreadCounter != 0 && !multiThreaded)
            {
                // Disable Condition
                enabled = false;
            }
        }

        internal void FillActionCache(int grp_idx)
        {
            _initActionCache();

            for (int i = 0; i < m_Raw.actions[grp_idx].Length; i++)
            {
                StatementRaw sr = m_Raw.actions[grp_idx][i];

                if (Asm.ActionIsRecyclable(sr.classname))
                {
                    if (m_ActionCache[grp_idx][i] == null)
                        m_ActionCache[grp_idx][i] = Asm.CreateActionInstance(sr.classname);

                    if (m_ActionCache[grp_idx][i] != null)
						m_ActionCache[grp_idx][i].Init(this, sr);
                }
            }
        }

        private void _initActionCache()
        {
            if (m_ActionCache != null)
                return;

            m_ActionCache = new List<Action[]>();
            for (int i = 0; i < m_Raw.actions.Count; i++)
            {
                Action[] actions = new Action[m_Raw.actions[i].Length];
                m_ActionCache.Add(actions);
            }
        }

        #region ACTION_THREAD_EVENT
        internal void RegisterActionThreadEvent (ActionThread thread)
        {
            m_ThreadCounter = System.Math.Min(0, m_ThreadCounter + 1);
            thread.onFinished += OnActionThreadFinished;
        }

        internal void UnregisterActionThreadEvent(ActionThread thread)
        {
            m_ThreadCounter = System.Math.Max(0, m_ThreadCounter - 1);
            thread.onFinished -= OnActionThreadFinished;
        }

        private void OnActionThreadFinished(ActionThread thread)
        {
            UnregisterActionThreadEvent(thread);

            if (m_ThreadCounter == 0)
            {
                if (!multiThreaded)
                {
                    enabled = true;
                }
            }
        }
        #endregion

        #region IO
        internal void Import (BinaryReader r)
        {
            repeat = r.ReadInt32();
            enabled = r.ReadBoolean();
        }

        internal void Export (BinaryWriter w)
        {
            w.Write(repeat);
            w.Write(enabled);
        }
        #endregion

		public override string ToString ()
		{
			if (m_Raw != null)
				return "Trigger [" + name + "]";
			else
				return "Unknown Trigger";
		}
    }
}
