using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("CHOOSE")]
    public class ChooseListener : ScenarioRTL.EventListener
    {
        // 在此列举参数
		int id;
        
        // 在此初始化参数
        protected override void OnCreate()
        {
			id = Utility.ToInt(missionVars, parameters["id"]);
        }
        
        // 打开事件监听
        public override void Listen()
        {
            if (GameUI.Instance != null && GameUI.Instance.mNPCSpeech != null)
            {
                GameUI.Instance.mNPCSpeech.speechInterpreter.onChoiceClick += OnSelectChoice;
            }
        }
        
        // 关闭事件监听
        public override void Close()
        {
            if (GameUI.Instance != null && GameUI.Instance.mNPCSpeech != null)
            {
                GameUI.Instance.mNPCSpeech.speechInterpreter.onChoiceClick -= OnSelectChoice;
            }
        }

        void OnSelectChoice (int choice_id)
        {
            if (choice_id == id)
            {
                Post();
                //GameUI.Instance.mNPCSpeech.speechInterpreter.onChoiceClick -= OnSelectChoice;
            }
        }
    }
}
