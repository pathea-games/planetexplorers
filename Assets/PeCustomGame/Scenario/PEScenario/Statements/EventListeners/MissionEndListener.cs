using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("MISSION END")]
	public class MissionEndListener : ScenarioRTL.EventListener
    {
        // 在此列举参数
		int missionId;
		EMissionResult result;

        // 在此初始化参数
        protected override void OnCreate()
        {
			missionId = Utility.ToEnumInt(parameters["mission"]);
			result = (EMissionResult)Utility.ToEnumInt(parameters["result"]);
        }
        
        // 打开事件监听
        public override void Listen()
        {
			if (PeCustomScene.Self != null &&
				PeCustomScene.Self.scenario != null &&
				PeCustomScene.Self.scenario.missionMgr != null)
			{
				PeCustomScene.Self.scenario.missionMgr.onCloseMission += OnResponse;
			}
        }
        
        // 关闭事件监听
        public override void Close()
        {
			if (PeCustomScene.Self != null &&
				PeCustomScene.Self.scenario != null &&
				PeCustomScene.Self.scenario.missionMgr != null)
			{
				PeCustomScene.Self.scenario.missionMgr.onCloseMission -= OnResponse;
			}
			else
			{
				Debug.LogError("Try to close eventlistener, but source has been destroyed");
			}
        }

		public void OnResponse (int mId, EMissionResult res)
		{
			if (missionId == -1 || // Any mission
				mission != null && missionId == 0 && mId == mission.dataId || // Current mission
				mission != null && missionId == -2 && mId != mission.dataId || // Any other mission
				missionId == mId) // Specified mission
			{
				if (result == res || result == EMissionResult.Any) // Check result
					Post();
			}
		}
    }
}
