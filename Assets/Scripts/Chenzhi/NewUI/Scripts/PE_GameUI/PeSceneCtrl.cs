using UnityEngine;
using System.Collections;
using Pathea;

public class PeSceneCtrl : MonoBehaviour 
{	
	static PeSceneCtrl	mInstance;
	public static PeSceneCtrl Instance{get{return mInstance;}}

	void Awake()
	{
		mInstance = this;
	}
	public void GotoMainMenuScene()
	{
		GameClientLobby.Disconnect();
		GameClientNetwork.Disconnect();
		if(PeFlowMgr.Instance.curScene == PeFlowMgr.EPeScene.GameScene&&RandomDungenMgrData.InDungeon&&PeGameMgr.IsSingleAdventure)
		{
			RandomDungenMgr.Instance.SaveInDungeon();
			RandomDungenMgr.Instance.DestroyDungeon();
		}
		PeFlowMgr.Instance.LoadScene(PeFlowMgr.EPeScene.MainMenuScene);
	}
	
	public void GotoRoleScene()
	{
		PeFlowMgr.Instance.LoadScene(PeFlowMgr.EPeScene.RoleScene);
	}
	
	public void GotoLobbyScene()
	{
		GameClientNetwork.Disconnect();
		PeFlowMgr.Instance.LoadScene(PeFlowMgr.EPeScene.LobbyScene);
	}
	
	public void GotoMultiRoleScene()
	{
		GameClientNetwork.Disconnect();
		PeFlowMgr.Instance.LoadScene(PeFlowMgr.EPeScene.MultiRoleScene);
	}


    /// <summary>
    /// 进入教程接口
    /// </summary>
    /// <param name="role"></param>
    public void GotToTutorial(CustomData.RoleInfo role)
    {
        //lw:多人出教程后，进入lobby
        TutorialExit.type = Pathea.PeGameMgr.playerType == Pathea.PeGameMgr.EPlayerType.Multiple ? 
            TutorialExit.TutorialType.MultiLobby :TutorialExit.TutorialType.Story;
      
        SystemSettingData.Instance.Tutorialed = true;
        string nickname = role.name;
        CustomCharactor.CustomDataMgr.Instance.Current = role.CreateCustomData();
        CustomCharactor.CustomDataMgr.Instance.Current.charactorName = nickname;

        Pathea.PeGameMgr.playerType = Pathea.PeGameMgr.EPlayerType.Tutorial;
        Pathea.PeGameMgr.sceneMode = Pathea.PeGameMgr.ESceneMode.Tutorial;
        Pathea.PeGameMgr.tutorialMode = Pathea.PeGameMgr.ETutorialMode.DigBuild;

        Pathea.PeFlowMgr.Instance.LoadScene(Pathea.PeFlowMgr.EPeScene.GameScene);
    }

    public void GotoGameSence()
	{
		PeFlowMgr.Instance.LoadScene(PeFlowMgr.EPeScene.GameScene);
	}
}
