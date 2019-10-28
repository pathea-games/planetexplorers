using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("GOAL ACHIEVED")]
    public class GoalAchievedCondition : ScenarioRTL.Condition
    {
        // 在此列举参数
		int goalId;
		int missionId;
		bool state;

        // 在此初始化参数
        protected override void OnCreate()
        {
			goalId = Utility.ToInt(missionVars, parameters["id"]);
			missionId = Utility.ToEnumInt(parameters["mission"]);
			state = Utility.ToBool(missionVars, parameters["bool"]);

			if (missionId == 0)
				missionId = mission.dataId;
        }
        
        // 判断条件
        public override bool? Check()
        {
			if (PeCustomScene.Self != null &&
				PeCustomScene.Self.scenario != null &&
				PeCustomScene.Self.scenario.missionMgr != null)
			{
				bool? achieved = PeCustomScene.Self.scenario.missionMgr.GoalAchieved(goalId, missionId);
				if (achieved == null)
					Debug.LogError("Goal is not set");
				return (achieved ?? false) == state;
			}
			else
			{
				Debug.LogError("MissionMgr is not ready");
				return false;
			}
        }
    }
}
