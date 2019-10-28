using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("HIDE SYSTEM UI", true)]
    public class HideSysuiAction : ScenarioRTL.Action
    {
        // 在此列举参数
		ESystemUI sysui;
        
        // 在此初始化参数
        protected override void OnCreate()
        {
			sysui = (ESystemUI)Utility.ToEnumInt(parameters["ui"]);
        }
        
        // 执行动作
        // 若为瞬间动作，返回true；
        // 若为持续动作，该函数会每帧被调用，直到返回true
        public override bool Logic()
        {
			// TODO: hide system ui
			switch (sysui)
			{
			case ESystemUI.WorldMap: break;
			case ESystemUI.CharacterInfo: GameUI.Instance.mUIPlayerInfoCtrl.Hide(); break;
			case ESystemUI.MissionWindow: /*GameUI.Instance.mUIMissionWndCtrl.Hide();*/ break;
			case ESystemUI.ItemPackage: GameUI.Instance.mItemPackageCtrl.Hide(); break;
			case ESystemUI.Replicator: GameUI.Instance.mCompoundWndCtrl.Hide(); break;
			case ESystemUI.Phone: GameUI.Instance.mPhoneWnd.Hide(); break;
			case ESystemUI.CreationSystem: VCEditor.Quit(); break;
			case ESystemUI.BuildMode: break;
			}
			return true;
        }
    }
}
