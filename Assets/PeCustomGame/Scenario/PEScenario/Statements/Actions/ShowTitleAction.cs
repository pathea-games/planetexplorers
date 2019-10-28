using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("SHOW TITLE", true)]
    public class ShowTitleAction : ScenarioRTL.Action
    {
        // 在此列举参数
		string title;
		string subtitle;
		float time;
		Color color;

        // 在此初始化参数
        protected override void OnCreate()
        {
			title = Utility.ToText(missionVars, parameters["title"]);
			subtitle = Utility.ToText(missionVars, parameters["subtitle"]);
			time = Utility.ToSingle(missionVars, parameters["time"]);
			color = Utility.ToColor(missionVars, parameters["color"]);
        }
        
        // 执行动作
        // 若为瞬间动作，返回true；
        // 若为持续动作，该函数会每帧被调用，直到返回true
        public override bool Logic()
        {
			if (time < 1)
				time = 1;
			if (PeCustomScene.Self != null && 
				PeCustomScene.Self.scenario != null &&
				PeCustomScene.Self.scenario.guiMgr != null)
			{
				PeCustomScene.Self.scenario.guiMgr.ShowTitle(title, subtitle, time, color);
			}
			return true;
        }
    }
}
