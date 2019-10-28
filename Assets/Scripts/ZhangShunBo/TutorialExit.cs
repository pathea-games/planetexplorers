using UnityEngine;
using System.Collections;

public class TutorialExit : MonoBehaviour {

    public enum TutorialType
    {
        Story,
        Mainmenu,
        MultiLobby
    }

    public static TutorialType type = TutorialType.Story;
    bool isShow = false;

    void OnTriggerEnter(Collider target)
    {
        if (null == target.GetComponentInParent<Pathea.MainPlayerCmpt>())
            return;
        if (isShow == true)
            return;

        isShow = true;

        if (MissionManager.Instance.HadCompleteMission(756))
            MessageBox_N.ShowYNBox(PELocalization.GetString(8000505), SceneTranslate, SetFalse);
        else
            MessageBox_N.ShowYNBox(PELocalization.GetString(8000506), SceneTranslate, SetFalse);

    }

    void SceneTranslate()
    {
        if (type == TutorialType.Story)
        {
            IntroRunner.movieEnd = (() =>
            {
                Debug.Log("<color=aqua>intro movie end.</color>");
                Pathea.PeGameMgr.playerType = Pathea.PeGameMgr.EPlayerType.Single;				
				Pathea.PeGameMgr.sceneMode = Pathea.PeGameMgr.ESceneMode.Story;
                PeSceneCtrl.Instance.GotoGameSence();
            });

            Pathea.PeFlowMgr.Instance.LoadScene(Pathea.PeFlowMgr.EPeScene.Intro);
            SetFalse();
        }else if(type == TutorialType.MultiLobby)
        {
            //lw:2017.3.8多人模式下出教程模式，进入lobby
            Pathea.PeGameMgr.playerType = Pathea.PeGameMgr.EPlayerType.Multiple;
            GameClientLobby.Self.TryEnterLobby(MLPlayerInfo.Instance.GetRoleInfo(MLPlayerInfo.Instance.mRolesCtrl.GetSelectedIndex()).mRoleInfo.roleID);
        }
        else
        {
            PeSceneCtrl.Instance.GotoMainMenuScene();
        }
    }

    void SetFalse() { isShow = false; }
}
