using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("SLEEPING")]
    public class SleepingCondition : ScenarioRTL.Condition
    {
        // 在此列举参数
        OBJECT player;  // PLAYER
        bool state;

        // 在此初始化参数
        protected override void OnCreate()
        {
            player = Utility.ToObject(parameters["player"]);
            state = Utility.ToBool(missionVars, parameters["state"]);
        }

        // 判断条件
        public override bool? Check()
        {
            if (player.type == OBJECT.OBJECTTYPE.Player)
            {
                Pathea.PeEntity entity = PeScenarioUtility.GetEntity(player);
                if (entity != null && entity.motionMgr != null)
                {
                    if (entity.motionMgr.IsActionRunning(Pathea.PEActionType.Sleep) == state)
                        return true;
                }
            }
            return false;
        }
    }
}
