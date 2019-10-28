using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("NEVER")]
    public class NeverCondition : ScenarioRTL.Condition
    {
        // 在此列举参数
        
        // 在此初始化参数
        protected override void OnCreate()
        {
        
        }
        
        // 判断条件
        public override bool? Check()
        {
            return false;
        }
    }
}
