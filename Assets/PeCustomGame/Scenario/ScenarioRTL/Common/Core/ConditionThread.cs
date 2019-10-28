using UnityEngine;
using System.Collections;

namespace ScenarioRTL
{
	// Condition Thread
	public class ConditionThread
	{
		private ConditionThread () {}

		public static ConditionThread Create (Trigger trigger)
		{
			ConditionThread ct = new ConditionThread();
			ct.trigger = trigger;

			bool? res = ct.Check();
			if (res == true)
				ct.Pass();
			else if (res == false)
				ct.Fail();
			else
				ct.Register();
			return ct;
		}

		private Trigger trigger;

		private void Register ()
		{
			trigger.mission.scenario.AddConditionThread(this);
		}

		public bool? Check ()
		{
			// 不能在Check里面直接调用Pass或者Fail，因为必须先删除Scenario内部的ConditionThread节点。
			for (int i = 0; i < trigger.m_Conditions.Count; i++)
			{
				bool? res = _checkConditions(trigger.m_Conditions[i]);
				if (res == true)
					return true;
				if (res == null)
					return null;
			}
			return false;
		}

		bool? _checkConditions (Condition[] conditions)
		{
			for (int i = 0; i < conditions.Length; i++)
			{
				if (conditions[i] == null)
					return false;
				bool? result = conditions[i].Check();
				if (result == null)
					return null;
				if (result == false)
					return false;
			}
			return true;
		}


		public void Pass ()
		{
			if (trigger.isAlive)
			{
				if (trigger.threadCounter == 0 || trigger.multiThreaded)
				{
					trigger.StartProcessAction();
				}
			}
			trigger = null;
		}

		public void Fail ()
		{
			trigger = null;
		}
	}
}