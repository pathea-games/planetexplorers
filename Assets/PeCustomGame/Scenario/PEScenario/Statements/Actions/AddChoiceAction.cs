using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("ADD CHOICE", true)]
    public class AddChoiceAction : ScenarioRTL.Action
    {
        // 在此列举参数
		int id;
		string text;
        
        // 在此初始化参数
        protected override void OnCreate()
        {
			id = Utility.ToInt(missionVars, parameters["id"]);
			text = Utility.ToText(missionVars, parameters["text"]);
        }
        
        // 执行动作
        // 若为瞬间动作，返回true；
        // 若为持续动作，该函数会每帧被调用，直到返回true
        public override bool Logic()
        {
            if (Pathea.PeGameMgr.IsMulti)
            {
                PlayerNetwork.RequestServer(EPacketType.PT_Custom_AddChoice, id, text);
            }
            else
            {
                if (PeCustomScene.Self != null && PeCustomScene.Self.scenario != null)
                {
                    PeCustomScene.Self.scenario.dialogMgr.AddChoose(id, text);
                }
            }

            return true;
        }
    }
}
