using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("SELECT QUEST")]
    public class SelectQuestListener : ScenarioRTL.EventListener
    {
		// 在此列举参数
		int id;
		OBJECT obj;	// NPOBJECT

		// 在此初始化参数
		protected override void OnCreate()
		{
			id = Utility.ToInt(missionVars, parameters["id"]);
			obj = Utility.ToObject(parameters["object"]);
		}
        
        // 打开事件监听
        public override void Listen()
        {
            if (GameUI.Instance != null && GameUI.Instance.mNpcDialog != null)
            {
                GameUI.Instance.mNpcDialog.dialogInterpreter.onQuestClick += OnQuestSelect;
            }
        }
        
        // 关闭事件监听
        public override void Close()
        {
            if (GameUI.Instance != null && GameUI.Instance.mNpcDialog != null)
            {
                GameUI.Instance.mNpcDialog.dialogInterpreter.onQuestClick -= OnQuestSelect;
            }
        }

        void OnQuestSelect (int world_index, int npo_id, int quest_id)
        {
            if (world_index == obj.Group &&
                obj.Id == npo_id && quest_id == id)
            {
               Post();
            }
        }
    }
}
