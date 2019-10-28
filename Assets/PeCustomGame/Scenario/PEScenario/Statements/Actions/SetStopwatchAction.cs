using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("SET STOPWATCH", true)]
	public class SetStopwatchAction : ScenarioRTL.Action
    {
        // 在此列举参数
		int id;
		string name = "";
		EFunc func_time;
		double amt;
		EFunc func_speed;
		float speed;

        // 在此初始化参数
        protected override void OnCreate()
        {
			id = Utility.ToInt(missionVars, parameters["id"]);
			name = Utility.ToText(missionVars, parameters["string"]);
			func_time = Utility.ToFunc(parameters["funct"]);
			amt = Utility.ToDouble(missionVars, parameters["amount"]);
			func_speed = Utility.ToFunc(parameters["funcs"]);
			speed = Utility.ToSingle(missionVars, parameters["speed"]);
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
				PeCustomScene.Self.scenario.stopwatchMgr.SetStopwatch(id, name, func_time, amt, func_speed, speed);
			}
			else
			{
				Debug.LogError("SetStopwatch - target is null");
			}
            return true;
        }
    }
}
