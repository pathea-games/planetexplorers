using System.Collections;
using ScenarioRTL.IO;
using System.Collections.Generic;


namespace ScenarioRTL
{
    public class Mission
    {
		public int instId { get; private set; }
        public int dataId { get { return m_Raw.id; } }
        public ParamRaw properties { get { return m_Raw.properties; } }
		public bool enabled { get; private set; }

        private MissionRaw m_Raw;

        private Trigger[] m_Triggers;
        public Trigger[] triggers { get { return m_Triggers; } }

		private List<EventListener> m_EventListeners = null;

        public Scenario scenario { get; private set; }

        public Mission(int instid, MissionRaw raw, Scenario scene)
        {
			instId = instid;
            m_Raw = raw;
            scenario = scene;
        }

        /// <summary> initialize the mission. Get all triggers and event listeners </summary>
        internal void Init ()
        {
			m_EventListeners = new List<EventListener>(16);
            m_Triggers = new Trigger[m_Raw.triggers.Length];
            
            for (int i = 0; i < m_Raw.triggers.Length; i++)
            {
                TriggerRaw tr = m_Raw.triggers[i];
                m_Triggers[i] = new Trigger(m_Raw.triggers[i], this, i);
                
				List<EventListener> list = new List<EventListener>();
                for (int j = 0; j < tr.events.Length; ++j)
                {
                    EventListener listener = Asm.CreateEventListenerInstance(tr.events[j].classname);

                    if (listener == null)
                        continue;

					listener.Init(m_Triggers[i], tr.events[j]);

					m_EventListeners.Add(listener);
                    list.Add(listener);
                }
                m_Triggers[i].eventListeners = list;
            }

            // Sort by order
			m_EventListeners.Sort((lhs, rhs) => { return lhs.order - rhs.order; });
        }

        internal void Run ()
        {
            // Event start to listen
			for (int i = 0; i < m_EventListeners.Count; ++i)
				m_EventListeners[i].Listen();

			enabled = true;
        }

		internal void Resume ()
		{
			// Event start to listen
			for (int i = 0; i < m_EventListeners.Count; ++i)
				m_EventListeners[i].Listen();

			enabled = true;
		}

        internal void Close ()
        {
			// Close event
			for (int i = 0; i < m_EventListeners.Count; ++i)
				m_EventListeners[i].Close();

			enabled = false;
        }

        internal void Pause ()
        {
            // Close event
            for (int i = 0; i < m_EventListeners.Count; ++i)
                m_EventListeners[i].Close();

            enabled = false;
        }

		public override string ToString ()
		{
			string s = enabled ? "[Running]" : "[Disabled]";
			if (m_Raw != null)
				return "Mission Instance [" + instId + "] : [" + dataId + " " + m_Raw.name + "] " + s;
			else
				return "Unknown Mission";
		}
    }
}
