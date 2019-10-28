using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Pathea;

public class UIGameMenuCtrl : UIStaticWnd
{
    private static UIGameMenuCtrl m_Instance;
    public static UIGameMenuCtrl Instance { get{ return m_Instance;}}

    public UIMenuList mMenuList = null;
    public BoxCollider mBtnCollider = null;
    public PeUIEffect.GameMenuScaleEffect mTweenEffect; //lz-2016.09.27 菜单打开和关闭的Tween合并到GameMenuScaleEffect统一控制避免逻辑混乱，并解决因为卡掉帧菜单的字和图标的Alpha没有到1菜单看不到或者看不清的情况
    [SerializeField]
    private Transform m_TutorialParent;
    [SerializeField]
    private UIWndTutorialTip_N m_TutorialPrefab;

    bool IsInitMenuList = false;

    private List<MenuItemInfo> mInfoList = new List<MenuItemInfo>();
    private Vector3 mMenuListPos = Vector3.zero;


    class MenuItemInfo
    {
        public string mItemText = "";
        public MenuItemFlag mFlag = MenuItemFlag.Flag_Null;
        public MenuItemFlag mParentFalg = MenuItemFlag.Flag_Null;
        public string mItemIcoStr = "";
        public int mKeyId = -1;
        public UIOption.KeyCategory mKeyCategory = UIOption.KeyCategory.Common;

        public MenuItemInfo(string itemText, MenuItemFlag flag, MenuItemFlag parentFlag, string icoStr, UIOption.KeyCategory category = UIOption.KeyCategory.Common, int id = -1)
        {
            mItemText = itemText;
            mFlag = flag;
            mParentFalg = parentFlag;
            mItemIcoStr = icoStr;
            mKeyCategory = category;
            mKeyId = id;
        }
    }

    public enum MenuItemFlag
    {
        Flag_Null,
        Flag_Storage,
        Flag_Admin,
        Flag_Workshop,
        Flag_Infomation,
        Flag_Friend,
        Flag_Mall,

        Flag_Online,

        Flag_Follower,
        Flag_Character,
        Flag_Mission,
        Flag_Phone,
        Flag_Colony,
        Flag_Replicatror,
        Flag_Creation,
        Flag_Skill,
        Flag_Inventory,
        Flag_Build,
        Flag_Options,

        Flag_Scan,
        Flag_Help,
        Flag_MonoRail,
        Flag_Diplomacy,
        Flag_Message,
        Flag_SpeciesWiki,
        Flag_Radio  //收音机
    }

    // Menu Item Ico String 
    //private string IcoStr_Null = "";
    //private string IcoStr_Storage = "listico_12_1";
    private string IcoStr_Admin = "listico_1_1";
    private string IcoStr_Workshop = "listico_4_1";
    private string IcoStr_Infomation = "listico_5_1";
    private string IcoStr_Friend = "listico_21_1";
    //private string IcoStr_Mall = "listico_20_1";
    private string IcoStr_OnLine = "listico_19_1";

    private string IcoStr_Follower = "listico_3_1";
    private string IcoStr_Character = "listico_2_1";
    private string IcoStr_Mission = "listico_6_1";
    private string IcoStr_Phone = "listico_7_1";
    private string IcoStr_Colony = "listico_8_1";
    private string IcoStr_Replicatror = "listico_9_1";
    private string IcoStr_Creation = "listico_10_1";
    private string IcoStr_Inventory = "listico_11_1";
    private string IcoStr_Build = "listico_17_1";
    private string IcoStr_Options = "listico_13_1";
    private string IcoStr_Scan = "listico_16_1";
    private string IcoStr_Help = "listico_14_1";
    private string IcoStr_MonoRail = "listico_15_1";
    private string IcoStr_Diplomacy = "listico_22_1";
    //lz-2016.06.12 添加一个Message选项，属于Phone界面的
    private string IcoStr_Message = "listico_23_1";
    //lz-2016.07.25 添加一个SpiciesWiki选项
    private string IcoStr_SpiciesWiki = "listico_24_1";
    //lz-2016.12.19 添加Radio
    private string IcoStr_Radio = "listico_25_1";

    // ...mark.................................
    private string IcoStr_Skill = "listico_1_1";

    void Awake()
    {
        m_Instance = this;
        Init();
    }

    // Use this for initialization
    void Start()
    {
        mMenuList.Hide();
    }

    void Init()
    {
        //lz-2016.06.28 悬浮在gamemenu按钮上提示快捷键
        mBtnCollider.gameObject.AddComponent<ShowToolTipItem_N>().mTipContent= PELocalization.GetString(2000060) + "[4169e1][~][-]";
        //str = new PeInput.KeyJoySettingPair(PeInput.SettingsAll[0][(int)PeInput.ESettingsGeneral.GameMenu])._key.ToStr();

        mInfoList.Clear();
        if (Pathea.PeGameMgr.IsMulti)
        {
            mInfoList.Add(new MenuItemInfo(NewUIText.mMenuOnline.GetString(), MenuItemFlag.Flag_Online, MenuItemFlag.Flag_Null, IcoStr_OnLine));

            if (!Pathea.PeGameMgr.IsSurvive)
            {
                //mInfoList.Add(new MenuItemInfo(NewUIText.mMenuStorage.GetString(),MenuItemFlag.Flag_Storage,MenuItemFlag.Flag_Online,IcoStr_Storage));
            }
            mInfoList.Add(new MenuItemInfo(NewUIText.mMenuAdmin.GetString(), MenuItemFlag.Flag_Admin, MenuItemFlag.Flag_Online, IcoStr_Admin));
            mInfoList.Add(new MenuItemInfo(NewUIText.mMenuWorkshop.GetString(), MenuItemFlag.Flag_Workshop, MenuItemFlag.Flag_Online, IcoStr_Workshop));
            mInfoList.Add(new MenuItemInfo(NewUIText.mMenuInformation.GetString(), MenuItemFlag.Flag_Infomation, MenuItemFlag.Flag_Online, IcoStr_Infomation));
            //lz-2016.11.10 取消商店菜单显示
            //mInfoList.Add(new MenuItemInfo(NewUIText.mMenuMall.GetString(), MenuItemFlag.Flag_Mall, MenuItemFlag.Flag_Online, IcoStr_Mall));
			mInfoList.Add(new MenuItemInfo(NewUIText.mMenuFriend.GetString(), MenuItemFlag.Flag_Friend, MenuItemFlag.Flag_Online, IcoStr_Friend, UIOption.KeyCategory.Common, (int)PeInput.ESettingsGeneral.FriendsMenu));

        }

		mInfoList.Add(new MenuItemInfo(NewUIText.mMenuFollower.GetString(), MenuItemFlag.Flag_Follower, MenuItemFlag.Flag_Null, IcoStr_Follower, UIOption.KeyCategory.Common, (int)PeInput.ESettingsGeneral.FollowersMenu));
		mInfoList.Add(new MenuItemInfo(NewUIText.mMenuCharacter.GetString(), MenuItemFlag.Flag_Character, MenuItemFlag.Flag_Null, IcoStr_Character, UIOption.KeyCategory.Common, (int)PeInput.ESettingsGeneral.CharacterStats));
		mInfoList.Add(new MenuItemInfo(NewUIText.mMenuMission.GetString(), MenuItemFlag.Flag_Mission, MenuItemFlag.Flag_Null, IcoStr_Mission, UIOption.KeyCategory.Common, (int)PeInput.ESettingsGeneral.Mission));

		mInfoList.Add(new MenuItemInfo(NewUIText.mMenuPhone.GetString(), MenuItemFlag.Flag_Phone, MenuItemFlag.Flag_Null, IcoStr_Phone, UIOption.KeyCategory.Common, (int)PeInput.ESettingsGeneral.HandheldPC));
        mInfoList.Add(new MenuItemInfo(NewUIText.mMenuHelp.GetString(), MenuItemFlag.Flag_Help, MenuItemFlag.Flag_Phone, IcoStr_Help));
        mInfoList.Add(new MenuItemInfo(NewUIText.mMenuScan.GetString(), MenuItemFlag.Flag_Scan, MenuItemFlag.Flag_Phone, IcoStr_Scan));
        mInfoList.Add(new MenuItemInfo(NewUIText.mMenuMonoRail.GetString(), MenuItemFlag.Flag_MonoRail, MenuItemFlag.Flag_Phone, IcoStr_MonoRail));
        mInfoList.Add(new MenuItemInfo(NewUIText.mMenuDiplomacy.GetString(), MenuItemFlag.Flag_Diplomacy, MenuItemFlag.Flag_Phone, IcoStr_Diplomacy));
        mInfoList.Add(new MenuItemInfo(NewUIText.mMenuMessage.GetString(), MenuItemFlag.Flag_Message, MenuItemFlag.Flag_Phone, IcoStr_Message));
        mInfoList.Add(new MenuItemInfo(NewUIText.mMenuSpeciesWiki.GetString(), MenuItemFlag.Flag_SpeciesWiki, MenuItemFlag.Flag_Phone, IcoStr_SpiciesWiki));
        mInfoList.Add(new MenuItemInfo(NewUIText.mMenuRadio.GetString(), MenuItemFlag.Flag_Radio, MenuItemFlag.Flag_Phone, IcoStr_Radio));

        //lz-2016.08.03 教程, Custom 模式禁止打开基地UI
        if (!Pathea.PeGameMgr.IsTutorial && !Pathea.PeGameMgr.IsCustom && !Pathea.PeGameMgr.IsMultiCustom)
        {
            mInfoList.Add(new MenuItemInfo(NewUIText.mMenuColony.GetString(), MenuItemFlag.Flag_Colony, MenuItemFlag.Flag_Null, IcoStr_Colony, UIOption.KeyCategory.Common, (int)PeInput.ESettingsGeneral.ColonyMenu));
        }
		mInfoList.Add(new MenuItemInfo(NewUIText.mMenuReplicator.GetString(), MenuItemFlag.Flag_Replicatror, MenuItemFlag.Flag_Null, IcoStr_Replicatror, UIOption.KeyCategory.Common, (int)PeInput.ESettingsGeneral.ReplicationMenu));
		mInfoList.Add(new MenuItemInfo(NewUIText.mMenuCreation.GetString(), MenuItemFlag.Flag_Creation, MenuItemFlag.Flag_Null, IcoStr_Creation, UIOption.KeyCategory.Common, (int)PeInput.ESettingsGeneral.CreationSystem));

        if (Pathea.PeGameMgr.IsAdventure)
        {
            if (RandomMapConfig.useSkillTree)
				mInfoList.Add(new MenuItemInfo(NewUIText.mMenuSkill.GetString(), MenuItemFlag.Flag_Skill, MenuItemFlag.Flag_Null, IcoStr_Skill, UIOption.KeyCategory.Common, (int)PeInput.ESettingsGeneral.SkillMenu));
        }

		mInfoList.Add(new MenuItemInfo(NewUIText.mMenuInventory.GetString(), MenuItemFlag.Flag_Inventory, MenuItemFlag.Flag_Null, IcoStr_Inventory, UIOption.KeyCategory.Common, (int)PeInput.ESettingsGeneral.Inventory));
		mInfoList.Add(new MenuItemInfo(NewUIText.mMenuBuild.GetString(), MenuItemFlag.Flag_Build, MenuItemFlag.Flag_Null, IcoStr_Build, UIOption.KeyCategory.Construct, (int)PeInput.ESettingsBuildMd.BuildMode));
        mInfoList.Add(new MenuItemInfo(NewUIText.mMenuOptions.GetString(), MenuItemFlag.Flag_Options, MenuItemFlag.Flag_Null, IcoStr_Options, UIOption.KeyCategory.Common));

        InitMenuList();

        mMenuList.RefreshHotKeyName();
    }

    void InitMenuList()
    {
        mMenuList.Items.Clear();

        UIMenuListItem parent = null;

        for (int i = 0; i < mInfoList.Count; i++)
        {
            if (mInfoList[i].mParentFalg == MenuItemFlag.Flag_Null)
                parent = null;
            else
            {
                parent = mMenuList.Items.Find(
                    delegate(UIMenuListItem li)
                    {
                        return li.mMenuItemFlag == mInfoList[i].mParentFalg;
                    });

            }

            UIMenuListItem item = mMenuList.AddItem(parent, mInfoList[i].mItemText, mInfoList[i].mFlag, mInfoList[i].mItemIcoStr);
            item.KeyId = mInfoList[i].mKeyId;
            item.mCategory = mInfoList[i].mKeyCategory;
        }

        int menuListPos_y = Convert.ToInt32(mMenuList.rootPanel.spBg.transform.localScale.y / 2) + 26;
        mMenuListPos = new Vector3(-130, menuListPos_y, 0);
        mMenuList.transform.localPosition = mMenuListPos;

        TweenPosition tp = mMenuList.GetComponent<TweenPosition>();
        tp.from = mMenuListPos;

        IsInitMenuList = true;
    }

    public override void Show()
    {
        BtnMenuOnClick();
        mTweenEffect.Play(true);
    }


    void BtnMenuOnClick()
    {
        if (!IsInitMenuList)
            return;

        if (!mMenuList.IsShow || !mMenuList.gameObject.activeSelf)
        {
            base.PlayOpenSoundEffect();
        }

        if (!mMenuList.IsShow)
            mMenuList.Show();

        mTweenEffect.Play();
    }

    void HideMenuList()
    {
        //lz-2016.11.14 错误 #5903 Crush bug
        if (null != UICamera.mainCamera)
        {
            Ray ray = UICamera.mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            bool ok = mBtnCollider.Raycast(ray, out rayHit, 300);
            if (!ok && !mMenuList.mouseMoveOn)
                mTweenEffect.Play(false);
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (mMenuList.gameObject.activeSelf)
        {
            if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
            {
                if (mMenuList.gameObject.transform.localScale == Vector3.one)
                    HideMenuList();
            }
        }
        //int width = Screen.width;
        //if (width > 1920)
        //    width = 1920;
        //int screen_x = width / 2 - 90;
        //if (gameObject.transform.localPosition.x != screen_x)
        //{
        //    Vector3 pos = gameObject.transform.localPosition;
        //    gameObject.transform.localPosition = new Vector3(screen_x, pos.y, pos.z);
        //}
    }

    void MenuItemOnClick(object sender)
    {
        UIMenuListItem item = sender as UIMenuListItem;
        if (item == null)
            return;
        if (item.mMenuItemFlag ==  MenuItemFlag.Flag_Null)
            return;

        if (item.mMenuItemFlag == MenuItemFlag.Flag_Storage)
        {

        }
        //AdminUI
        else if (item.mMenuItemFlag == MenuItemFlag.Flag_Admin)
        {
            GameUI.Instance.mAdminstratorWnd.Show();
        }

        else if (item.mMenuItemFlag == MenuItemFlag.Flag_Workshop)
        {
            GameUI.Instance.mWorkShopCtrl.Show();
        }
        else if (item.mMenuItemFlag == MenuItemFlag.Flag_Infomation)
        {
            GameUI.Instance.mTeamInfoMgr.Show();
        }
        else if (item.mMenuItemFlag == MenuItemFlag.Flag_Friend)
        {

        }
        else if (item.mMenuItemFlag == MenuItemFlag.Flag_Mall)
        {
            if (GameUI.Instance.mMallWnd != null)
                GameUI.Instance.mMallWnd.Show();
        }
        else if (item.mMenuItemFlag == MenuItemFlag.Flag_Follower)
        {
            GameUI.Instance.mServantWndCtrl.Show();
        }
        else if (item.mMenuItemFlag == MenuItemFlag.Flag_Character)
        {
            GameUI.Instance.mUIPlayerInfoCtrl.Show();
        }
        else if (item.mMenuItemFlag == MenuItemFlag.Flag_Mission)
        {
            if (Pathea.PeGameMgr.IsCustom)
                GameUI.Instance.mMissionGoal.Show();
            else
                GameUI.Instance.mUIMissionWndCtrl.Show();
        }
        else if (item.mMenuItemFlag == MenuItemFlag.Flag_Phone)
        {
            GameUI.Instance.mPhoneWnd.Show();
        }
        else if (item.mMenuItemFlag == MenuItemFlag.Flag_Colony)
        {
            GameUI.Instance.mCSUI_MainWndCtrl.Show();
        }
        else if (item.mMenuItemFlag == MenuItemFlag.Flag_Replicatror)
        {
            GameUI.Instance.mCompoundWndCtrl.Show();
        }
        else if (item.mMenuItemFlag == MenuItemFlag.Flag_Creation)
        {
            //lz-2016.07.04 系统菜单打开的时候不允许打开创建系统
            if (null != GameUI.Instance && !GameUI.Instance.SystemUIIsOpen)
                VCEditor.Open();
        }
        else if (item.mMenuItemFlag == MenuItemFlag.Flag_Inventory)
        {
            GameUI.Instance.mItemPackageCtrl.Show();
        }
        else if (item.mMenuItemFlag == MenuItemFlag.Flag_Build)
        {
            if (Pathea.SingleGameStory.curType == Pathea.SingleGameStory.StoryScene.MainLand
                    || Pathea.SingleGameStory.curType == Pathea.SingleGameStory.StoryScene.TrainingShip)
                GameUI.Instance.mBuildBlock.EnterBuildMode();
            else
                new PeTipMsg("[C8C800]" + PELocalization.GetString(82209004), PeTipMsg.EMsgLevel.Norm, PeTipMsg.EMsgType.Stroy);
        }
        else if (item.mMenuItemFlag == MenuItemFlag.Flag_Options)
        {
            GameUI.Instance.mSystemMenu.Show();
        }
        else if (item.mMenuItemFlag == MenuItemFlag.Flag_Scan)
        {
            GameUI.Instance.mPhoneWnd.Show(UIPhoneWnd.PageSelect.Page_Scan);
        }
        else if (item.mMenuItemFlag == MenuItemFlag.Flag_Help)
        {
            GameUI.Instance.mPhoneWnd.Show(UIPhoneWnd.PageSelect.Page_Help);
        }
        else if (item.mMenuItemFlag == MenuItemFlag.Flag_MonoRail)
        {
            GameUI.Instance.mPhoneWnd.Show(UIPhoneWnd.PageSelect.Page_Rail);
        }
        else if (item.mMenuItemFlag == MenuItemFlag.Flag_Diplomacy)
        {
            GameUI.Instance.mPhoneWnd.Show(UIPhoneWnd.PageSelect.Page_Diplomacy);
        }
        //lz-2016.06.12
        else if (item.mMenuItemFlag == MenuItemFlag.Flag_Message)
        {
            GameUI.Instance.mPhoneWnd.Show(UIPhoneWnd.PageSelect.Page_Message);
        }
        //lz-2016.10.18 点击怪物图鉴打开相应的UI
        else if (item.mMenuItemFlag == MenuItemFlag.Flag_SpeciesWiki)
        {
            GameUI.Instance.mPhoneWnd.Show(UIPhoneWnd.PageSelect.Page_MonsterHandbook);
        }
        //lz-2016.12.19 点击Radio打开手机界面音乐播放器
        else if (item.mMenuItemFlag == MenuItemFlag.Flag_Radio)
        {
            GameUI.Instance.mPhoneWnd.Show(UIPhoneWnd.PageSelect.Page_Radio);
        }
        else if (item.mMenuItemFlag == MenuItemFlag.Flag_Skill)
        {
            GameUI.Instance.mSkillWndCtrl.Show();
        }
        else
            return;

        //HideMenuList();
        GameUI.Instance.ShowGameWndAll();
        mTweenEffect.Play(false);
    }

    void OnHideWndBtn()
    {
        //lz-2016.08.09  这个隐藏UI的功能不要了
        //GameUI.Instance.ChangeGameWndShowState();
    }

    #region Tutorial 

    /// <summary>lz-2016.11.03 显示这个窗口的Tutorial</summary>
    public void ShowWndTutorial()
    {
        if (PeGameMgr.IsTutorial)
        {
            GameObject go = Instantiate(m_TutorialPrefab.gameObject);
            go.transform.parent = m_TutorialParent;
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.transform.localRotation = Quaternion.identity;
        }
    }

    #endregion
}
