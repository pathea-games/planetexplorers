using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("COMPARE")]
	public class CompareCondition : ScenarioRTL.Condition
    {
        // 在此列举参数
		Var lhs, rhs;
		ECompare comp;
        
        // 在此初始化参数
        protected override void OnCreate()
        {
			lhs = Utility.ToVar(missionVars, parameters["lhs"]);
			comp = Utility.ToCompare(parameters["compare"]);
			rhs = Utility.ToVar(missionVars, parameters["rhs"]);
        }
        
        // 判断条件
        public override bool? Check()
        {
			return Utility.CompareVar(lhs, rhs, comp);
        }
    }
}
