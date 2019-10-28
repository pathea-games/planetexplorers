using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("SET KILL GOAL", true)]
    public class SetKillGoalAction : ScenarioRTL.Action
    {
        // 在此列举参数
		int goalId;
		string text;
		int missionId;
		OBJECT monster;   // MONSTER
		ECompare comp;
		int amt;

        // 在此初始化参数
        protected override void OnCreate()
        {
			goalId = Utility.ToInt(missionVars, parameters["id"]);
			text = Utility.ToText(missionVars, parameters["text"]);
			missionId = Utility.ToEnumInt(parameters["mission"]);
			monster = Utility.ToObject(parameters["monster"]);
			comp = Utility.ToCompare(parameters["compare"]);
			amt = Utility.ToInt(missionVars, parameters["amount"]);

			if (missionId == 0)
				missionId = mission.dataId;
        }
        
        // 执行动作
        // 若为瞬间动作，返回true；
        // 若为持续动作，该函数会每帧被调用，直到返回true
        public override bool Logic()
        {
			if (PeCustomScene.Self != null &&
				PeCustomScene.Self.scenario != null &&
				PeCustomScene.Self.scenario.missionMgr != null)
			{
				PeCustomScene.Self.scenario.missionMgr.SetKillGoal(goalId, text, missionId, monster, comp, amt);
			}
			else
			{
				Debug.LogError("MissionMgr is not ready");
			}
			return true;
        }
    }
}
