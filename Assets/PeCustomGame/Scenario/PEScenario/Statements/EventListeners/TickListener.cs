using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("TICK CYCLE")]
	public class TickListener : ScenarioRTL.EventListener
    {
        // 在此列举参数
		int n;
        
        // 在此初始化参数
        protected override void OnCreate()
        {
			n = Utility.ToInt(missionVars, parameters["n"]);
			if (n < 1) n = 1;
        }
        
        // 打开事件监听
        public override void Listen()
        {
			if (PeCustomScene.Self != null && 
				PeCustomScene.Self.scenario != null &&
				PeCustomScene.Self.scenario.tickMgr != null)
			{
				PeCustomScene.Self.scenario.tickMgr.OnTick += OnResponse;
			}
        }
        
        // 关闭事件监听
        public override void Close()
        {
			if (PeCustomScene.Self != null && 
				PeCustomScene.Self.scenario != null &&
				PeCustomScene.Self.scenario.tickMgr != null)
			{
				PeCustomScene.Self.scenario.tickMgr.OnTick -= OnResponse;
			}
			else
			{
				Debug.LogError("Try to close eventlistener, but source has been destroyed");
			}
        }

		private void OnResponse (int tick)
		{
			if (Pathea.PeGameMgr.IsMulti)
				tick /= 4;
			if (tick % n == 0)
				Post();
		}
    }
}
