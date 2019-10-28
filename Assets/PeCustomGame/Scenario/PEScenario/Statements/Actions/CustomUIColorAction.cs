using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("CUSTOM UI COLOR", true)]
	public class CustomUIColorAction : ScenarioRTL.Action
    {
        // 在此列举参数
		Color uicolor;

        // 在此初始化参数
        protected override void OnCreate()
        {
			uicolor = Utility.ToColor(missionVars, parameters["uicolor"]);
        }
        
        // 执行动作
        // 若为瞬间动作，返回true；
        // 若为持续动作，该函数会每帧被调用，直到返回true
        public override bool Logic()
        {
			GUI.color = uicolor;
            return true;
        }
    }
}
