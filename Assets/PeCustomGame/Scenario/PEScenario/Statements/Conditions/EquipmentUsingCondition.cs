using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("EQUIPMENT USING STATE")]
    public class EquipmentUsingCondition : ScenarioRTL.Condition
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
				if (entity != null && entity.motionEquipment != null)
				{
					return (entity.motionEquipment.Weapon != null) == state;
				}
			}
			return !state;
        }
    }
}
