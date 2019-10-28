using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("ADD QUEST", true)]
    public class AddQuestAction : ScenarioRTL.Action
    {
		// 在此列举参数
		int id;
		string text;
		OBJECT obj;	// NPOBJECT

		// 在此初始化参数
		protected override void OnCreate()
		{
			id = Utility.ToInt(missionVars, parameters["id"]);
			text = Utility.ToText(missionVars, parameters["text"]);
			obj = Utility.ToObject(parameters["object"]);
		}
        
        // 执行动作
        // 若为瞬间动作，返回true；
        // 若为持续动作，该函数会每帧被调用，直到返回true
        public override bool Logic()
        {
            if (Pathea.PeGameMgr.IsMulti)
            {
                byte[] data = PETools.Serialize.Export(w =>
                {
                    BufferHelper.Serialize(w, obj);
                    BufferHelper.Serialize(w, id);
                    BufferHelper.Serialize(w, text);
                });

                PlayerNetwork.RequestServer(EPacketType.PT_Custom_AddQuest, data);
            }
            else
            {
                if (PeCustomScene.Self != null && PeCustomScene.Self.scenario != null
                    && PeCustomScene.Self.scenario.dialogMgr != null)
                {
                    PeCustomScene.Self.scenario.dialogMgr.SetQuest(obj.Group, obj.Id, id, text);
                }
            }

            return true;
        }
    }
}
