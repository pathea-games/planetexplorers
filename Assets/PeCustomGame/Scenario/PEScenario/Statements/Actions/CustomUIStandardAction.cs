using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("CUSTOM UI STANDARD", true)]
	public class CustomUIStandardAction : ScenarioRTL.Action
    {
        // 在此列举参数
		EUIType uitype;
		Rect rect;
		EUIAnchor anchor;
		string uitext = "";
		EUIStyle uistyle;
		string uievent = "";
        
        // 在此初始化参数
        protected override void OnCreate()
        {
			uitype = (EUIType)Utility.ToEnumInt(parameters["uitype"]);
			rect = Utility.ToRect(missionVars, parameters["rect"]);
			anchor = (EUIAnchor)Utility.ToEnumInt(parameters["anchor"]);
			uitext = Utility.ToText(missionVars, parameters["uitext"]);
			uistyle = (EUIStyle)Utility.ToEnumInt(parameters["uistyle"]);
			uievent = Utility.ToVarname(parameters["uievent"]);
        }
        
        // 执行动作
        // 若为瞬间动作，返回true；
        // 若为持续动作，该函数会每帧被调用，直到返回true
        public override bool Logic()
        {
			switch (uistyle)
			{
			case EUIStyle.Default:
				// TODO: 应用UIStyle
				break;
			}

			switch (anchor)
			{
			case EUIAnchor.Streched:
				rect.x = Mathf.Clamp(rect.x, -0.1f, 1.1f);
				rect.y = Mathf.Clamp(rect.y, -0.1f, 1.1f);
				rect.width = Mathf.Clamp01(rect.width);
				rect.height = Mathf.Clamp01(rect.height);
				rect = new Rect(rect.x * Screen.width, rect.y * Screen.height,
					rect.width * Screen.width, rect.height * Screen.height);
				break;
			case EUIAnchor.LowerLeft:
				rect.y = rect.y + Screen.height;
				break;
			case EUIAnchor.LowerCenter:
				rect.x = rect.x + Screen.width / 2;
				rect.y = rect.y + Screen.height;
				break;
			case EUIAnchor.LowerRight:
				rect.x = rect.x + Screen.width;
				rect.y = rect.y + Screen.height;
				break;
			case EUIAnchor.MiddleLeft:
				rect.y = rect.y + Screen.height / 2;
				break;
			case EUIAnchor.Center:
				rect.x = rect.x + Screen.width / 2;
				rect.y = rect.y + Screen.height / 2;
				break;
			case EUIAnchor.MiddleRight:
				rect.x = rect.x + Screen.width;
				rect.y = rect.y + Screen.height / 2;
				break;
			case EUIAnchor.UpperCenter:
				rect.x = rect.x + Screen.width / 2;
				break;
			case EUIAnchor.UpperRight:
				rect.x = rect.x + Screen.width;
				break;
			}

			switch (uitype)
			{
			case EUIType.Label:
				GUI.Label(rect, uitext);
				break;
			case EUIType.Button:
				if (GUI.Button(rect, uitext))
				{
					uievent = uievent.Trim();
					if (!string.IsNullOrEmpty(uievent))
					{
						if (PeCustomScene.Self != null && 
							PeCustomScene.Self.scenario != null &&
							PeCustomScene.Self.scenario.guiMgr != null)
						{
							PeCustomScene.Self.scenario.guiMgr.GUIResponse(uievent);
						}
					}
				}
				break;
			case EUIType.Box:
				GUI.Box(rect, uitext);
				break;
			}
            return true;
        }
    }
}
