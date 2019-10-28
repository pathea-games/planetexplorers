using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("MAYBE")]
	public class MaybeCondition : ScenarioRTL.Condition
    {
        // 在此列举参数
		float p = 0;

        // 在此初始化参数
        protected override void OnCreate()
        {
			p = Utility.ToSingle(missionVars, parameters["p"]);
        }
        
        // 判断条件
        public override bool? Check()
        {
			return Random.value * 100 < p;
        }
    }
}
