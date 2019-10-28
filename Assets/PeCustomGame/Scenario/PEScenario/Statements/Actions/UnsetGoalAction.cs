using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("UNSET GOAL", true)]
    public class UnsetGoalAction : ScenarioRTL.Action
    {
        // 在此列举参数
		int goalId;
		int missionId;

        // 在此初始化参数
        protected override void OnCreate()
        {
			goalId = Utility.ToInt(missionVars, parameters["id"]);
			missionId = Utility.ToEnumInt(parameters["mission"]);

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
				PeCustomScene.Self.scenario.missionMgr.UnsetGoal(goalId, missionId);
			}
			else
			{
				Debug.LogError("MissionMgr is not ready");
			}
            return true;
        }
    }
}
