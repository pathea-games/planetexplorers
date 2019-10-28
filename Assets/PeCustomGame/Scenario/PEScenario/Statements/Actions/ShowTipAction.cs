using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("SHOW TIP", true)]
    public class ShowTipAction : ScenarioRTL.Action
    {
		// 在此列举参数
		string text;

		// 在此初始化参数
		protected override void OnCreate()
		{
			text = Utility.ToText(missionVars, parameters["text"]);
		}
        
        // 执行动作
        // 若为瞬间动作，返回true；
        // 若为持续动作，该函数会每帧被调用，直到返回true
        public override bool Logic()
        {
			new PeTipMsg(text, PeTipMsg.EMsgLevel.Norm, PeTipMsg.EMsgType.Misc);
            return true;
        }
    }
}
