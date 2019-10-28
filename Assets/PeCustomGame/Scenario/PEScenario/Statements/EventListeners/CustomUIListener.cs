using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("CUSTOM UI")]
	public class CustomUIListener : ScenarioRTL.EventListener
    {
        // 在此列举参数
        
        // 在此初始化参数
        protected override void OnCreate()
        {
        
        }
        
        // 打开事件监听
        public override void Listen()
        {
			if (PeCustomScene.Self != null && 
				PeCustomScene.Self.scenario != null &&
				PeCustomScene.Self.scenario.guiMgr != null)
			{
				PeCustomScene.Self.scenario.guiMgr.OnGUIDrawing += Post;
			}
        }
        
        // 关闭事件监听
        public override void Close()
        {
			if (PeCustomScene.Self != null && 
				PeCustomScene.Self.scenario != null &&
				PeCustomScene.Self.scenario.guiMgr != null)
			{
				PeCustomScene.Self.scenario.guiMgr.OnGUIDrawing -= Post;
			}
			else
			{
				Debug.LogError("Try to close eventlistener, but source has been destroyed");
			}
        }
    }
}
