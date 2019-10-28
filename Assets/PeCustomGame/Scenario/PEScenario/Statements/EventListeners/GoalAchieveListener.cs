using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("GOAL ACHIEVE")]
    public class GoalAchieveListener : ScenarioRTL.EventListener
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
        
        // 打开事件监听
        public override void Listen()
        {
			if (PeCustomScene.Self != null &&
				PeCustomScene.Self.scenario != null &&
				PeCustomScene.Self.scenario.missionMgr != null)
			{
				PeCustomScene.Self.scenario.missionMgr.OnGoalAchieve += OnResponse;
			}
        }
        
        // 关闭事件监听
        public override void Close()
        {
			if (PeCustomScene.Self != null &&
				PeCustomScene.Self.scenario != null &&
				PeCustomScene.Self.scenario.missionMgr != null)
			{
				PeCustomScene.Self.scenario.missionMgr.OnGoalAchieve -= OnResponse;
			}
        }

		void OnResponse (int id, int mid)
        {
			if (id == goalId && mid == missionId)
				Post();
        }
    }
}
