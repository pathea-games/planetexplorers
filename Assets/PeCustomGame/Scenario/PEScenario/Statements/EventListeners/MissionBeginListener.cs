using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("MISSION BEGIN")]
	public class MissionBeginListener : ScenarioRTL.EventListener
    {
        // 在此列举参数
		int missionId;
        
        // 在此初始化参数
        protected override void OnCreate()
        {
			missionId = Utility.ToEnumInt(parameters["mission"]);
        }
        
        // 打开事件监听
        public override void Listen()
        {
			if (PeCustomScene.Self != null &&
				PeCustomScene.Self.scenario != null &&
				PeCustomScene.Self.scenario.missionMgr != null)
			{
				PeCustomScene.Self.scenario.missionMgr.onRunMission += OnResponse;
			}
        }
        
        // 关闭事件监听
        public override void Close()
        {
			if (PeCustomScene.Self != null &&
				PeCustomScene.Self.scenario != null &&
				PeCustomScene.Self.scenario.missionMgr != null)
			{
				PeCustomScene.Self.scenario.missionMgr.onRunMission -= OnResponse;
			}
			else
			{
				Debug.LogError("Try to close eventlistener, but source has been destroyed");
			}
        }

		public void OnResponse (int mId)
		{
			if (missionId == -1 || // Any mission
				mission != null && missionId == 0 && mId == mission.dataId || // Current mission
				mission != null && missionId == -2 && mId != mission.dataId || // Any other mission
				missionId == mId) // Specified mission
			{
				Post();
			}
		}
    }
}
