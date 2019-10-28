using UnityEngine;
using System.Collections;
using Pathea;
using Pathea.PeEntityExt;

public class UIPhoneWnd : UIBaseWnd
{
    public enum PageSelect
    {
        Null = -1,
        Page_Help = 0,
        Page_Scan,
        Page_Rail,
        Page_Diplomacy,
        Page_Message,
        Page_MonsterHandbook, //lz-2016.07.20 怪物图鉴
        Page_Radio,           //lz-2016.12.02 收音机
        Max
    }
    [SerializeField]
    UICheckbox mCkHelp;
    [SerializeField]
    UICheckbox mCkScan;
    [SerializeField]
    UICheckbox mCkRail;
    [SerializeField]
    UICheckbox mCkCamp;
    [SerializeField]
    UICheckbox mCkMsg;
    [SerializeField]
    UICheckbox mCkMh;
    [SerializeField]
    UICheckbox mCkRadio;
    [SerializeField]
    UILabel mCkHelpText;
    [SerializeField]
    UILabel mCkScanText;
    [SerializeField]
    UILabel mCkRailText;
    [SerializeField]
    UILabel mCkCampText;
    [SerializeField]
    UILabel mCkMsgText;
    [SerializeField]
    UILabel mCkMhText;
    [SerializeField]
    UILabel mCkRadioText;
    [SerializeField]
    UIGrid mCksGrid;
    [SerializeField]
    float mCheckIntervalTime = 0.3f;

    public bool OpenRadio=false;

    public UIHelpCtrl mUIHelp;
    public UIScanCtrl mUIScan;
    public UIMonoRailCtrl mUIRail;
    public UICampCtrl mUICamp;
    public UIAllianceCtrl mUIAlliance; //lz-2016.09.08 这个联盟是除了Story模式外用
    public UIMessageCtrl mUIMessage;
    public UIMonsterHandbookCtrl mUIMonsterHandbook;
    public UIRadioCtrl mUIRadioCtrl;
    [HideInInspector]
    public PageSelect CurSelectPage;
    const string SELECTPAGEKEY = "UIPhoneSelectPage";

    //private bool bUpdateOnce = false;
    private UIBaseWidget selectPage;
    private UIBaseWidget curDiplomacyWnd;
    private float m_StartTime;

    #region mono methods
   
    void Update()
    {
        mCkHelpText.color = mCkHelp.isChecked ? Color.white : Color.black;
        mCkScanText.color = mCkScan.isChecked ? Color.white : Color.black;
        mCkRailText.color = mCkRail.isChecked ? Color.white : Color.black;
        mCkCampText.color = mCkCamp.isChecked ? Color.white : Color.black;
        mCkMsgText.color = mCkMsg.isChecked ? Color.white : Color.black;
        mCkMhText.color = mCkMh.isChecked ? Color.white : Color.black;
        mCkRadioText.color = mCkRadio.isChecked ? Color.white : Color.black;

        if (Time.realtimeSinceStartup - m_StartTime > mCheckIntervalTime)
        {
            UpdateCksActiveState();
            m_StartTime = Time.realtimeSinceStartup;
        }
    }

    public void OnGUI()
    {
        //		if (GUI.Button(new Rect(200, 200, 50, 50), "test"))
        //		{
        //			UIWidget[] widgets = gameObject.GetComponentsInChildren<UIWidget>();
        //			foreach (UIWidget w in widgets)
        //				w.MarkAsChanged();
        //		}
    }
    #endregion

    #region override methods
    protected override void InitWindow()
    {
        base.InitWindow();
        InitRadioData();

        CurSelectPage = PageSelect.Page_Help;

        if (null != UIRecentDataMgr.Instance)
        {
            //lz-2016.12.20 恢复玩家操作的页面
            int selectIntPage =UIRecentDataMgr.Instance.GetIntValue(SELECTPAGEKEY, (int)PageSelect.Page_Help);
            if (selectIntPage > (int)PageSelect.Null && selectIntPage < (int)PageSelect.Max)
            {
                CurSelectPage = (PageSelect)selectIntPage;
                if (!CheckUnlockByPageSelect(CurSelectPage))
                {
                    //lz-2016.12.20 如果恢复的页面没有解锁，就设回帮助页面
                    CurSelectPage = PageSelect.Page_Help;
                }
            }
        }
        
        curDiplomacyWnd = (PeGameMgr.IsStory ? (UIBaseWidget)mUICamp : (UIBaseWidget)mUIAlliance);
        CancelAllCkStartsChecked();
        switch (CurSelectPage)
        {
            case PageSelect.Page_Help:
                mCkHelp.startsChecked = true;
                selectPage = mUIHelp;
                break;
            case PageSelect.Page_Scan:
                mCkScan.startsChecked = true;
                selectPage = mUIScan;
                break;
            case PageSelect.Page_Rail:
                mCkRail.startsChecked = true;
                selectPage = mUIRail;
                break;
            case PageSelect.Page_Diplomacy:
                mCkCamp.startsChecked = true;
                selectPage = mUICamp;
                break;
            case PageSelect.Page_Message:
                mCkMsg.startsChecked = true;
                selectPage = mUIMessage;
                break;
            case PageSelect.Page_MonsterHandbook:
                mCkMh.startsChecked = true;
                selectPage = mUIMonsterHandbook;
                break;
            case PageSelect.Page_Radio:
                mCkRadio.startsChecked = true;
                selectPage = mUIRadioCtrl;
                break;
            default:
                break;
        }
        m_StartTime = Time.realtimeSinceStartup;
    }

    #endregion

    #region private methods

    void CancelAllCkStartsChecked()
    {
        mCkHelp.startsChecked = false;
        mCkScan.startsChecked = false;
        mCkRail.startsChecked = false;
        mCkCamp.startsChecked = false;
        mCkMsg.startsChecked = false;
        mCkMh.startsChecked = false;
        mCkRadio.startsChecked = false;
    }

    bool CheckUnlockByPageSelect(PageSelect pageSelect)
    {
        //lz-2016.06.12 这几种的开启条件都可以写在这里检测
        switch (pageSelect)
        {
            case PageSelect.Page_Help:
                return true;
            case PageSelect.Page_Scan:
                return true;
            case PageSelect.Page_Rail:
                return true;
            case PageSelect.Page_Diplomacy:
                //lz-2016.09.19 Adventure模式声望系统常开
                if (PeGameMgr.IsAdventure)
                    return true;
                else if (ReputationSystem.Instance != null && PeCreature.Instance.mainPlayer != null&&PeGameMgr.IsStory)
                {
                    return ReputationSystem.Instance.GetActiveState((int)PeCreature.Instance.mainPlayer.GetAttribute(AttribType.DefaultPlayerID));
                }
                return false;
            case PageSelect.Page_Message:
                return true;
            case PageSelect.Page_MonsterHandbook:
                //lz-2016.10.18 Adventure默认开启，Story检测开启
                if (PeGameMgr.IsAdventure||(PeGameMgr.IsStory && null != StroyManager.Instance&& StroyManager.Instance.enableBook))
                    return true;
                else
                    return false;
            case PageSelect.Page_Radio:
                return CheckOpenRadio();
            case PageSelect.Max:
                return false;
        }
        return false;
    }

    #region 检测收音机开启条件
    const int m_OpenRadioMissionID = 10047;
    const int m_ChenZhenID = 9007;
    NpcMissionData m_ChenZhenMissionData;
    public bool InitRadio = false;
    public void InitRadioData()
    {
        if (!InitRadio)
        {
            PeEntity chenZhenEntity = EntityMgr.Instance.Get(m_ChenZhenID);
            if (chenZhenEntity)
            {
                m_ChenZhenMissionData = chenZhenEntity.GetUserData() as NpcMissionData;
            }
            InitRadio = true;
        }
    }
    public bool CheckOpenRadio()
    {
        //lz-2016.12.15 Story模式检测任务
        if (PeGameMgr.IsStory)
        {
            //lz-2016.12.15 完成任务就开启
            if (MissionManager.Instance&&MissionManager.Instance.m_PlayerMission.HadCompleteMission(m_OpenRadioMissionID))
            {
                OpenRadio = true;
            }
            else
            {
                if (null == m_ChenZhenMissionData||m_ChenZhenMissionData.m_MissionList.Contains(m_OpenRadioMissionID))
                {
                    OpenRadio = false;
                }
                else
                {
                    //lz-2016.12.15 旧存档没有这个任务直接开启
                    OpenRadio = true;
                }
            }
        }
        else
        {
            OpenRadio = true;
        }
        return OpenRadio;
    }

    #endregion

    void UpdateCksActiveState()
    {
        //mCkHelp.gameObject.SetActive(?);
        //mCkScan.gameObject.SetActive(?);
        //mCkRail.gameObject.SetActive(?);
        //mCkMsg.gameObject.SetActive(?);
        mCkCamp.gameObject.SetActive(this.CheckUnlockByPageSelect(PageSelect.Page_Diplomacy));
        mCkMh.gameObject.SetActive(this.CheckUnlockByPageSelect(PageSelect.Page_MonsterHandbook));
        mCkRadio.gameObject.SetActive(this.CheckUnlockByPageSelect(PageSelect.Page_Radio));
        this.UpdateCksPos();
    }

    //lz-2016.06.12 根据ck的多少居中显示
    void UpdateCksPos()
    {
        mCksGrid.Reposition();
        UICheckbox[] chArray = mCksGrid.GetComponentsInChildren<UICheckbox>(false);
        int count = (null==chArray) ? 0 : chArray.Length;
        mCksGrid.transform.localPosition = new Vector3(-mCksGrid.cellWidth * (count - 1) * 0.5f, mCksGrid.transform.localPosition.y, mCksGrid.transform.localPosition.z);
    }

    void DelayShow()
    {
        Show();
    }

    void ChangePage(PageSelect page)
    {
        if (!this.CheckUnlockByPageSelect(page))
            return;

        if (selectPage != null)
            selectPage.Hide();
        CurSelectPage = page;

        if (null != UIRecentDataMgr.Instance)
        {
            //lz-2016.12.20 存储玩家操作的是那个页面
            UIRecentDataMgr.Instance.SetIntValue(SELECTPAGEKEY, (int)page);
        }

        switch (page)
        {
            case PageSelect.Page_Help:
                {
                    //			if (mUIScan.isShow)
                    //				mUIScan.Hide();
                    //			if (mUIRail.isShow)
                    //				mUIRail.Hide();

                    mUIHelp.Show();
                    selectPage = mUIHelp;
                    if (!mCkHelp.isChecked)
                        mCkHelp.isChecked = true;
                } break;
            case PageSelect.Page_Scan:
                {
                    //			if (mUIHelp.isShow)
                    //				mUIHelp.Hide();
                    //			if (mUIRail.isShow)
                    //				mUIRail.Hide();
                    mUIScan.Show();
                    selectPage = mUIScan;
                    if (!mCkScan.isChecked)
                        mCkScan.isChecked = true;
                } break;
            case PageSelect.Page_Rail:
                {
                    //			if (mUIHelp.isShow)
                    //				mUIHelp.Hide();
                    //			if (mUIScan.isShow)
                    //				mUIScan.Hide();

                    mUIRail.Show();
                    selectPage = mUIRail;
                    if (!mCkRail.isChecked)
                        mCkRail.isChecked = true;
                } break;
            case PageSelect.Page_Diplomacy:
                {
                    curDiplomacyWnd.Show();
                    selectPage = curDiplomacyWnd;
                    if (!mCkCamp.isChecked)
                        mCkCamp.isChecked = true;
                } break;
            case PageSelect.Page_Message:
                {
                    mUIMessage.Show();
                    selectPage = mUIMessage;
                    mCkMsg.isChecked = true;
                }
                break;
            case PageSelect.Page_MonsterHandbook:
                {
                    mUIMonsterHandbook.Show();
                    selectPage = mUIMonsterHandbook;
                    mCkMh.isChecked = true;
                }
                break;
            case PageSelect.Page_Radio:
                {
                    mUIRadioCtrl.Show();
                    selectPage = mUIRadioCtrl;
                    mCkRadio.isChecked = true;
                }
                break;
            default:
                break;
        }
    }

    void OnHelpBtn()
    {
        ChangePage(PageSelect.Page_Help);
    }

    void OnScanBtn()
    {
        ChangePage(PageSelect.Page_Scan);
    }

    void OnRailBtn()
    {
        ChangePage(PageSelect.Page_Rail);
    }

    void OnCampBtn()
    {
        ChangePage(PageSelect.Page_Diplomacy);
    }

    void OnMsgBtn()
    {
        ChangePage(PageSelect.Page_Message);
    }

    void OnMonsterHandbookBtn()
    {
        ChangePage(PageSelect.Page_MonsterHandbook);
    }

    void OnRadioBtn()
    {
        ChangePage(PageSelect.Page_Radio);
    }

    #endregion

    #region public methods
    public void Show(PageSelect page)
    {
        base.Show();
        UpdateCksActiveState();
        ChangePage(page);
    }

    public override void Show()
    {
        base.Show();
        Show(CurSelectPage);
    }

    #endregion
    
}
