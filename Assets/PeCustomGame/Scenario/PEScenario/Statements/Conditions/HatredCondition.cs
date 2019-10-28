using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("HATRED")]
    public class HatredCondition : ScenarioRTL.Condition
    {
        // 在此列举参数
        OBJECT obj1;  // ENTITY
        OBJECT obj2;  // NPOBJECT
        bool state;
    
        // 在此初始化参数
        protected override void OnCreate()
        {
            obj1 = Utility.ToObject(parameters["object1"]);
            obj2 = Utility.ToObject(parameters["object2"]);
            state = Utility.ToBool(missionVars, parameters["state"]);
        }
        
        // 判断条件
        public override bool? Check()
        {
            Pathea.PeEntity self = PeScenarioUtility.GetEntity(obj1);
            Pathea.PeEntity attacker = PeScenarioUtility.GetEntity(obj2);
            if(self == null || attacker == null || attacker.target == null)
			    return false;
            if (attacker.target.HasHatred(self))
                return state;
            else
                return !state;
        }
    }
}
