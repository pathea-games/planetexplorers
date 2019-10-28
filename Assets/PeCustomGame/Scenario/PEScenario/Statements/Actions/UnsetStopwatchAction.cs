using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("UNSET STOPWATCH", true)]
	public class UnsetStopwatchAction : ScenarioRTL.Action
    {
        // 在此列举参数
		int id;
		      
        // 在此初始化参数
        protected override void OnCreate()
        {
			id = Utility.ToInt(missionVars, parameters["id"]);
        }
        
        // 执行动作
        // 若为瞬间动作，返回true；
        // 若为持续动作，该函数会每帧被调用，直到返回true
        public override bool Logic()
        {
			if (PeCustomScene.Self != null && 
				PeCustomScene.Self.scenario != null && 
				PeCustomScene.Self.scenario.stopwatchMgr != null)
			{
				PeCustomScene.Self.scenario.stopwatchMgr.UnsetStopwatch(id);
			}
			else
			{
				Debug.LogError("UnsetStopwatch - target is null");
			}
            return true;
        }
    }
}
