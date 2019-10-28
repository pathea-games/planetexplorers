using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("RUN MISSION", true)]
	public class RunMissionAction : ScenarioRTL.Action
    {
        // 在此列举参数
		int missionId;
		OBJECT player;
		bool owner;

        // 在此初始化参数
        protected override void OnCreate()
        {
			missionId = Utility.ToEnumInt(parameters["mission"]);
			player = Utility.ToObject(parameters["player"]);

			if (PeCustomScene.Self != null &&
				PeCustomScene.Self.scenario != null)
			{
				int curr_player = PeCustomScene.Self.scenario.playerId;
				owner = PeScenarioUtility.OwnerCheck(curr_player, curr_player, player);
			}
			else
			{
				owner = false;
			}
        }
        
        // 执行动作
        // 若为瞬间动作，返回true；
        // 若为持续动作，该函数会每帧被调用，直到返回true
        public override bool Logic()
        {
			if (owner && missionId > 0)
			{
				if (PeCustomScene.Self != null &&
					PeCustomScene.Self.scenario != null &&
					PeCustomScene.Self.scenario.missionMgr != null)
				{
					PeCustomScene.Self.scenario.missionMgr.RunMission(missionId);
				}
			}
            return true;
        }
    }
}
