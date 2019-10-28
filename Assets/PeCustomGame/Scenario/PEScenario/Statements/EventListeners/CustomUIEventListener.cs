using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("CUSTOM UI EVENT")]
    public class CustomUIEventListener : ScenarioRTL.EventListener
    {
        // 在此列举参数
		string eventname = "";
        
        // 在此初始化参数
        protected override void OnCreate()
        {
			eventname = Utility.ToVarname(parameters["eventname"]);
        }
        
        // 打开事件监听
        public override void Listen()
        {
			if (PeCustomScene.Self != null && 
				PeCustomScene.Self.scenario != null &&
				PeCustomScene.Self.scenario.guiMgr != null)
			{
				PeCustomScene.Self.scenario.guiMgr.OnGUIResponse += OnResponse;
			}
        }
        
        // 关闭事件监听
        public override void Close()
        {
			if (PeCustomScene.Self != null && 
				PeCustomScene.Self.scenario != null &&
				PeCustomScene.Self.scenario.guiMgr != null)
			{
				PeCustomScene.Self.scenario.guiMgr.OnGUIResponse -= OnResponse;
			}
			else
			{
				Debug.LogError("Try to close eventlistener, but source has been destroyed");
			}
        }

		private void OnResponse (string evtname)
		{
			if (eventname == evtname)
				Post();
		}
    }
}
