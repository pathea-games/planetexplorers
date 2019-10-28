#define MAINMENU_ISO
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class TitleMenuGui_N : UIStaticWnd
{
	static TitleMenuGui_N	mInstance;
	public static TitleMenuGui_N Instance{get{return mInstance;}}
	
	public GameObject 	mMainMenu;
	public GameObject 	mSingleMenu;

    public GameObject mGraphWaringWnd;
    public UILabel mGraphWaringLbl;

    public UIButton		mContinueBtn;
	public UIButton		mLoadBtn;
	
	public Transform	mWndTran;
	
	public GameObject	mControlWnd;
    public GameObject   mMask;
    public GameObject   mFixeBlurryWnd;
    public UICheckbox   mFixBlurryCB;
	
	public UILabel		mVersionLabel;

	public GameObject  mPEModel;
	public UICustomGameSelectWnd  mCustomSelectWnd;

    
	public List<Transform> menuMain;
	public List<Transform> menuSingle;

	UILabel labMp = null;

    void Start ()
	{
		mInstance = this;
        //Record.m_Auto = Record.RecordOther(ref Otherlist);
        //if(Otherlist.Count < 1 || Record.m_Auto == -1)
        //    mContinueBtn.isEnabled = false;
        //if(Otherlist.Count < 1)
        //    mLoadBtn.isEnabled = false;

//		LogoGui_N.Instance.SetBGM("Sound/Music/PE_MainMenu");
		
		//mWndTran.localPosition = new Vector3((int)-Screen.width/4 + 50f,50f,0);
        Pathea.PeGameMgr.playerType = Pathea.PeGameMgr.EPlayerType.Multiple;
        Pathea.PeGameMgr.gameLevel = Pathea.PeGameMgr.EGameLevel.Normal;

        //lz-2016.09.13 提示
        if(!string.IsNullOrEmpty(GlobalBehaviour.BadGfxDeviceName))
        {
            mGraphWaringLbl.text = "  "+PELocalization.GetString(8000691).Replace("$A$", GlobalBehaviour.BadGfxDeviceName);
            mGraphWaringWnd.SetActive(true);
        }
        else
        {
            mGraphWaringWnd.SetActive(false);
            CheckControlWndNeedShow();
        }
        
        InitVersion();
		mCustomSelectWnd.onOpen += OnCustomWndOpen;
		mCustomSelectWnd.onClose += OnCustomWndClose;

		//w destroy multiplayer
#if DemoVersion
		GameObject.Destroy(menuMain[1].gameObject);
		menuMain.RemoveAt(1);
		for(int i = 0; i < menuMain.Count; ++i)
			menuMain[i].localPosition = new Vector3(0, 120 - i * 40, 0);
		GameObject.Destroy(menuSingle[3].gameObject);
		menuSingle.RemoveAt(3);
		GameObject.Destroy(menuSingle[1].gameObject);
		menuSingle.RemoveAt(1);
		for(int i = 0; i < menuSingle.Count; ++i)
			menuSingle[i].localPosition = new Vector3(0, 80 - i * 40, 0);
#endif
	}
	
    void Update ()
    {		
#if MAINMENU_ISO
		if(labMp == null)
		{
			labMp = menuMain[1].GetComponentInChildren<UILabel>();
			if(labMp != null)
			{
				labMp.text = "ISO";
				if(SystemSettingData.Instance.IsChinese)
				{
					labMp.font = UIFontMgr.Instance.mChinese[1];
				}
			}
		}
#endif

        if (SystemSettingData.Instance != null)
        {
            if (mControlWnd.activeSelf)
            {
                if (SystemSettingData.Instance.FixBlurryFont != mFixBlurryCB.isChecked)
                {
                    SystemSettingData.Instance.FixBlurryFont = mFixBlurryCB.isChecked;
                    if (UIFontMgr.Instance != null)
                    {
                        UILabel[] labels = UIFontMgr.Instance.gameObject.GetComponentsInChildren<UILabel>(true);
                        for (int i = 0; i < labels.Length; i++)
                        {
                            labels[i].MakePixelPerfect();
                        }
                    }


                }
            }
        }
    }

	void InitVersion()
	{
		mVersionLabel.text = GameConfig.GameVersion;
	}
	
    //lz-2016.07.06 吴哥说取消选择操作模式功能
    //void OnControlMode1()
    //{
    //    SystemSettingData.Instance.mMMOControlType = true;
    //    SystemSettingData.Instance.ApplyKeySetting();
    //    mControlWnd.SetActive(false);
    //}
    //void OnControlMode2()
    //{
    //    SystemSettingData.Instance.mMMOControlType = false;
    //    SystemSettingData.Instance.ApplyKeySetting();
    //    mControlWnd.SetActive(false);
    //}

	void OnSingleplayerBtn()
	{
        Pathea.PeGameMgr.playerType = Pathea.PeGameMgr.EPlayerType.Single;
	}

    public void OnMultiplayerBtn()
    {
        Pathea.PeGameMgr.playerType = Pathea.PeGameMgr.EPlayerType.Multiple;
        // 临时加入
        RandomMapConfig.useSkillTree = false;

#if MAINMENU_ISO
		PeSceneCtrl.Instance.GotoLobbyScene();
		return;
#endif

#if SteamVersion && !LOCALTEST
        GameClientLobby.ConnectToLobby();
        MessageBox_N.ShowMaskBox(MsgInfoType.LobbyLoginMask, PELocalization.GetString(8000118), 30);
#elif LOCALTEST
        GameClientLobby.ConnectToLobby();
#else
        MessageBox_N.ShowOkBox(PELocalization.GetString(8000117));
#endif
    }

	void OnCreateBtn()
	{
		VCEditor.Open();
	}

	void OnTutorialBtn()
	{
        UIPlayerBuildCtrl.MainmenuToTutorial();
	}

	void OnOptionsBtn()
	{
		Hide();
		UIOption.Instance.mParentWnd = this;
		UIOption.Instance.Show();
		UIOption.Instance.OnVideoBtn();
	}

    void OnCreditsBtn()
	{
        Application.LoadLevel("GameCredits");
	}
	
	void OnBoardBtn()
	{
		Application.OpenURL("http://board.pathea.net/");
	}
	
	void OnQuitBtn()
	{
		Application.Quit();
	}
	
	void OnStoryBtn()
	{
		Pathea.PeGameMgr.loadArchive = Pathea.ArchiveMgr.ESave.New;
		Pathea.PeGameMgr.sceneMode = Pathea.PeGameMgr.ESceneMode.Story;
	}
	
	void OnAdventureBtn()
	{
		Pathea.PeGameMgr.loadArchive = Pathea.ArchiveMgr.ESave.New;
		Pathea.PeGameMgr.sceneMode = Pathea.PeGameMgr.ESceneMode.Adventure;
	}

    void OnEasyModeBtn()
    {
        Pathea.PeGameMgr.gameLevel = Pathea.PeGameMgr.EGameLevel.Easy;
        this.LoadRoleScene();
    }

    void OnNormalModeBtn()
    {
        Pathea.PeGameMgr.gameLevel = Pathea.PeGameMgr.EGameLevel.Normal;
        this.LoadRoleScene();
    }

    void LoadRoleScene()
    {
        Pathea.PeFlowMgr.Instance.LoadScene(Pathea.PeFlowMgr.EPeScene.RoleScene);
    }

	void OnBuildBtn()
	{
		Pathea.PeGameMgr.loadArchive = Pathea.ArchiveMgr.ESave.New;
		Pathea.PeGameMgr.sceneMode = Pathea.PeGameMgr.ESceneMode.Build;
		Pathea.PeFlowMgr.Instance.LoadScene(Pathea.PeFlowMgr.EPeScene.RoleScene);
	}
	
	void OnContinueBtn()
	{
		Pathea.ArchiveMgr.ESave idxToContinue = Pathea.ArchiveMgr.ESave.Auto1;
		string dir = Pathea.ArchiveMgr.Instance.Load(idxToContinue);
		Pathea.PeGameSummary summary = null;		
		if(!string.IsNullOrEmpty(dir))
		{
			summary = Pathea.PeGameSummary.Mgr.Instance.Get();
		}

        if (summary == null)
        {
			Debug.Log("<color=aqua>Failed continue archive:" + idxToContinue + "</color>");
            return;
        }

		Pathea.PeGameMgr.playerType = Pathea.PeGameMgr.EPlayerType.Single;
		Pathea.PeGameMgr.loadArchive = idxToContinue;
        Pathea.PeFlowMgr.Instance.LoadScene(Pathea.PeFlowMgr.EPeScene.GameScene, false);
	}

	void OnCoustomBtn()
	{

		mCustomSelectWnd.Open();
//		Pathea.PeGameMgr.loadArchive = Pathea.ArchiveMgr.ESave.New;
//		Pathea.PeGameMgr.sceneMode = Pathea.PeGameMgr.ESceneMode.Custom;
//		Pathea.PeFlowMgr.Instance.LoadScene(Pathea.PeFlowMgr.EPeScene.RoleScene);
	}
	
	void OnLoadBtn()
	{
		Hide();
		//GameConfig.IsNewGame = false;
		//SaveLoadGui_N.Instance.LoadGameFromMenu = true;
		UISaveLoad.Instance.Show();
		UISaveLoad.Instance.ToLoadWnd();
		UISaveLoad.Instance.HideSaveTabBtn();
	}

	void OnCustomWndOpen ()
	{
        if (mPEModel != null)
		    mPEModel.gameObject.SetActive(false);
	}

	void OnCustomWndClose ()
	{
        if (mPEModel != null)
		    mPEModel.gameObject.SetActive(true);
	}

    void OnFixBlurryClose ()
    {
        mFixeBlurryWnd.SetActive(false);
        //lz-2016.07.06 操作完FixBlurry就直接可以进游戏了，不用选择操作模式,默认是Control1
        //mMask.transform.localPosition = new Vector3(0, 0, 5);
        SystemSettingData.Instance.mMMOControlType = true;
        SystemSettingData.Instance.ApplyKeySetting();
        mControlWnd.SetActive(false);

    }

    void CheckControlWndNeedShow()
    {
        if (!SystemSettingData.Instance.mHasData)
        {
            mControlWnd.SetActive(true);
            SystemSettingData.Instance.mHasData = true;
            mFixBlurryCB.isChecked = SystemSettingData.Instance.FixBlurryFont;
        }
    }

    void OnGraphWaringWndClose()
    {
        mGraphWaringWnd.SetActive(false);
        CheckControlWndNeedShow();
    }
}
