using UnityEngine;
using ScenarioRTL;

namespace PeCustom
{
    [Statement("SHOW SYSTEM UI", true)]
    public class ShowSysuiAction : ScenarioRTL.Action
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
			// TODO: show system ui
			switch (sysui)
			{
			case ESystemUI.WorldMap: break;
			case ESystemUI.CharacterInfo: GameUI.Instance.mUIPlayerInfoCtrl.Show(); break;
			case ESystemUI.MissionWindow: /*GameUI.Instance.mUIMissionWndCtrl.Show();*/ break;
			case ESystemUI.ItemPackage: GameUI.Instance.mItemPackageCtrl.Show(); break;
			case ESystemUI.Replicator: GameUI.Instance.mCompoundWndCtrl.Show(); break;
			case ESystemUI.Phone: GameUI.Instance.mPhoneWnd.Show(); break;
			case ESystemUI.CreationSystem: VCEditor.Open(); break;
			case ESystemUI.BuildMode: break;
			}
            return true;
        }
    }
}
