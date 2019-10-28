using System.Collections;
using ScenarioRTL.IO;


namespace ScenarioRTL
{
    // Action Thread
    public class ActionThread
    {
		public Trigger trigger { get; private set; }

        public int group { get; private set; }
        public int triggerIndex { get; private set; }

        private StatementRaw[] m_Raw;
        private Action[] m_ActionCaches;
		private int m_CurrIndex = 0;
        public int currIndex { get { return m_CurrIndex; } }

		private Action m_CurrAction = null;
        public Action currAction { get { return m_CurrAction; } }

        public System.Action<ActionThread> onFinished;

        public bool isFinished { get { return m_CurrIndex >= m_Raw.Length; } }

		public ActionThread (Trigger _trigger, int _triggerIdx, int _group, StatementRaw[] raw, Action[] action_caches, int cur_idx = 0)
        {
			trigger = _trigger;
            group = _group;
			triggerIndex = _triggerIdx;
            m_Raw = raw;
			m_ActionCaches = action_caches;
            m_CurrIndex = cur_idx;
        }

        public void ProcessAction()
        {
            while (m_Raw.Length > m_CurrIndex)
            {
				if (m_CurrAction == null)
					CreateCurrAction();

				if (m_CurrAction == null)
				{
					m_CurrIndex++;
				}
                else if (m_CurrAction.Logic())
                {
                    m_CurrAction = null;
                    m_CurrIndex++;
                }
				else
				{
                	break;
				}
            }

            if (isFinished)
            {
                if (onFinished != null)
                    onFinished(this);
            }
        }

        public void CreateCurrAction()
        {
            if (m_Raw.Length > m_CurrIndex)
            {
				if (m_ActionCaches[m_CurrIndex] == null)
					m_CurrAction = Asm.CreateActionInstance(m_Raw[m_CurrIndex].classname);
				else
					m_CurrAction = m_ActionCaches[m_CurrIndex];
				
				if (m_CurrAction != null)
					m_CurrAction.Init(trigger, m_Raw[m_CurrIndex]);
            }
        }
    }
}
