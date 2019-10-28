using ScenarioRTL.IO;
using System.IO;

namespace ScenarioRTL
{
	public abstract class StatementObject
	{
		private StatementRaw m_Raw;

		public string classname { get { return m_Raw.classname; } }
		public ParamRaw parameters { get { return m_Raw.parameters; } }
		public int order { get { return m_Raw.order; } }
		public Trigger trigger { get; private set; }
		public Mission mission { get { return trigger == null ? null : trigger.mission; } }
		public Scenario scenario { get { return trigger == null ? null : trigger.mission.scenario; } }
		protected VarScope scenarioVars { get { return trigger == null ? null : trigger.mission.scenario.Variables; } }
		protected VarScope missionVars { get { return trigger == null ? null : trigger.mission.scenario.MissionVariables[trigger.mission]; } }

		protected abstract void OnCreate();

		internal void Init (Trigger _trigger, StatementRaw _raw)
		{
			trigger = _trigger;
			m_Raw = _raw;

			OnCreate();
		}

		public override string ToString ()
		{
			return classname;
		}
	}
}
