using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("TIME OUT")]
	public class TimeoutListener : ScenarioRTL.EventListener
    {
        // 在此列举参数
		int id;
        
        // 在此初始化参数
        protected override void OnCreate()
        {
			id = Utility.ToInt(missionVars, parameters["id"]);
        }
        
        // 打开事件监听
        public override void Listen()
        {
			if (PeCustomScene.Self != null && 
				PeCustomScene.Self.scenario != null &&
				PeCustomScene.Self.scenario.stopwatchMgr != null)
			{
				PeCustomScene.Self.scenario.stopwatchMgr.OnTimeout += OnResponse;
			}
        }
        
        // 关闭事件监听
        public override void Close()
        {
			if (PeCustomScene.Self != null && 
				PeCustomScene.Self.scenario != null &&
				PeCustomScene.Self.scenario.stopwatchMgr != null)
			{
				PeCustomScene.Self.scenario.stopwatchMgr.OnTimeout -= OnResponse;
			}
			else
			{
				Debug.LogError("Try to close eventlistener, but source has been destroyed");
			}
        }

		private void OnResponse (int _id)
		{
			if (id == _id)
				Post();
		}
    }
}
