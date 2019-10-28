using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("STOPWATCH")]
    public class StopwatchCondition : ScenarioRTL.Condition
    {
        // 在此列举参数
		int id;
		ECompare comp;
		double amt;
        
        // 在此初始化参数
        protected override void OnCreate()
        {
			id = Utility.ToInt(missionVars, parameters["id"]);
			comp = Utility.ToCompare(parameters["compare"]);
			amt = Utility.ToDouble(missionVars, parameters["amount"]);
        }
        
        // 判断条件
        public override bool? Check()
        {
			if (PeCustomScene.Self != null && 
				PeCustomScene.Self.scenario != null && 
				PeCustomScene.Self.scenario.stopwatchMgr != null)
				return PeCustomScene.Self.scenario.stopwatchMgr.CompareStopwatch(id, comp, amt);
			return Utility.Compare(0.0, amt, comp);
        }
    }
}
