
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea;

public class GameUI : MonoBehaviour
{
    static GameUI mInstance;
    public static GameUI Instance { get { return mInstance; } }

    //test
    public bool testUI = true;

    // uiCamera
    public Camera mUICamera;

    //Peinput
    public bool IsInput = false;

    public bool SystemUIIsOpen { get { if (null != mShowSystemWnds && mShowSystemWnds.Count > 0) return true; else return false; } }

    [SerializeField]
    Transform tsCenter = null;
    [SerializeField]
    Transform tsCenterOther = null;
    //[SerializeField]
    //Transform tsLeft = null;
    //[SerializeField]
    //Transform tsRightTop = null;
    //[SerializeField]
    //Transform tsRight = null;


    #region UI Prefab
    // UI Build
    [SerializeField]
    GameObject buildPrefab;

    [SerializeField]
    GameObject tipsWmdPrefab;
    [SerializeField]
    GameObject tipRecordsMgrPrefab;
    // wnd 
    [SerializeField]
    GameObject GameMainPrefab = null;
    [SerializeField]
    GameObject playerInfoPrefab = null;
    [SerializeField]
    GameObject ItemPackagePrefab = null;
    [SerializeField]
    GameObject worldMapPrefab = null;
    //[SerializeField]
    //GameObject limitWorldMapPrefab = null;
    [SerializeField]
    GameObject npcStoragePrefab = null;
    [SerializeField]
    GameObject SytemPrefab = null;
    [SerializeField]
    GameObject compoundWndPrefab = null;
    [SerializeField]
    GameObject servantWndPrefab = null;
    [SerializeField]
    GameObject npcGuiPrefab = null;
    [SerializeField]
    GameObject missionWndPrefab = null;
    [SerializeField]
    GameObject missionTrackWndPrefab = null;
    [SerializeField]
    GameObject itemGetWndPrefab = null;
    [SerializeField]
    GameObject itemOpGuiPrefab = null;
    [SerializeField]
    GameObject shopGuiPrefab = null;
    [SerializeField]
    GameObject itemBoxGuiPrefab = null;
    [SerializeField]
    GameObject repairWndGuiPrefab = null;
    [SerializeField]
    GameObject powerPlantSolarPrefab = null;
    [SerializeField]
    GameObject reviveGuiPrefab = null;
    [SerializeField]
    GameObject wareHouseGuiPrefab = null;
    [SerializeField]
    GameObject colonyWndPrefab = null;
    [SerializeField]
    GameObject phoneWndPrefab = null;
    [SerializeField]
    GameObject skillWndPrefab = null;
    [SerializeField]
    GameObject workshopPrefb = null;
    [SerializeField]
    GameObject informationPrefab = null;
    [SerializeField]
    GameObject railwayPonitPrefab = null;
    [SerializeField]
    GameObject mallWndPrefab = null;
    [SerializeField]
    GameObject AdministratorPrefab = null;
    [SerializeField]
    GameObject drivingPrefab = null;
    [SerializeField]
    GameObject stopwatchPrefab = null;
    [SerializeField]
    GameObject npcWndCustomPrefab = null;
    [SerializeField]
    GameObject missionWndCustomPrefab = null;
    [SerializeField]
    GameObject missionTrackCustomPrefab = null;
    [SerializeField]
    GameObject mKickstarterCtrlPrefab = null;
    [SerializeField]
    GameObject mNpcTalkHistoryPrefab = null;

    //lz-2018.01.04 物品追踪
    [SerializeField]
    GameObject itemsTrackWndPrefab = null;


    [SerializeField]
    BoxCollider mSystemUIMaskCollider = null;   //lz-2016.07.04 系统菜单打开的时候遮挡对系统菜单以外的ui操作


    #endregion

    // GameMain
    [HideInInspector]
    public UIGameMenuCtrl mUIGameMenuCtrl = null;
    [HideInInspector]
    public UIMainMidCtrl mUIMainMidCtrl = null;
    [HideInInspector]
    public UIMinMapCtrl mUIMinMapCtrl = null;
    [HideInInspector]
    public UINPCTalk mNPCTalk = null;
    [HideInInspector]
    public UINpcSpeech mNPCSpeech = null;
    [HideInInspector]
    public UIServantTalk mServantTalk = null;
    [HideInInspector]
    public UIMissionTrackCtrl mMissionTrackWnd = null;
    // world map
    [HideInInspector]
    public UIWorldMapCtrl mUIWorldMap = null;
    // System
    [HideInInspector]
    public UISystemMenu mSystemMenu = null;
    [HideInInspector]
    public UIOption mOption = null;
    [HideInInspector]
    public UISaveLoad mSaveLoad = null;
    // Game Wnd
    [HideInInspector]
    public UIPlayerInfoCtrl mUIPlayerInfoCtrl = null;
    [HideInInspector]
    public UIItemPackageCtrl mItemPackageCtrl = null;
    [HideInInspector]
    public UINpcStorageCtrl mNpcStrageCtrl = null;
    [HideInInspector]
    public UICompoundWndControl mCompoundWndCtrl = null;
    [HideInInspector]
    public UIServantWnd mServantWndCtrl = null;
    [HideInInspector]
    public UINpcWnd mNpcWnd = null;
    [HideInInspector]
    public UIMissionWndCtrl mUIMissionWndCtrl = null;
    [HideInInspector]
    public UIItemGet mItemGet = null;
    [HideInInspector]
    public UIItemOp mItemOp = null;
    [HideInInspector]
    public UIShopWnd mShopWnd = null;
    [HideInInspector]
    public UIItemBox mItemBox = null;
    [HideInInspector]
    public UIRepairWnd mRepair = null;
    [HideInInspector]
    public UIPowerPlantSolar mPowerPlantSolar = null;
    [HideInInspector]
    public UIRevive mRevive = null;
    [HideInInspector]
    public UIWarehouse mWarehouse = null;
    [HideInInspector]
    public CSUI_MainWndCtrl mCSUI_MainWndCtrl = null;
    [HideInInspector]
    public UIBuildBlock mBuildBlock = null;
    [HideInInspector]
    public UIPhoneWnd mPhoneWnd = null;
    [HideInInspector]
    public UISkillWndCtrl mSkillWndCtrl = null;
    [HideInInspector]
    public UIWorkShopCtrl mWorkShopCtrl = null;
    [HideInInspector]
    public CSUI_TeamInfoMgr mTeamInfoMgr = null;
    [HideInInspector]
    public RailwayPointGui_N mRailwayPoint = null;
    [HideInInspector]
    public UIMallWnd mMallWnd;
    [HideInInspector]
    public UITips mTipsCtrl;
    [HideInInspector]
    public UITipRecordsMgr mTipRecordsMgr = null;
    // AdminUI
    [HideInInspector]
    public UIAdminstratorWnd mAdminstratorWnd = null;

    // Driving UI
    [HideInInspector]
    public UIDrivingCtrl mDrivingCtrl = null;

    [HideInInspector]
    public UIStopwatchList mStopwatchList = null;

    [HideInInspector]
    public UINpcDialog mNpcDialog = null;

    [HideInInspector]
    public UIMissionGoal mMissionGoal = null;

    [HideInInspector]
    public UIMissionTrack mCustomMissionTrack = null;

    [HideInInspector]
    public KickstarterCtrl mKickstarterCtrl = null;

    [HideInInspector]
    public NpcTalkHistoryWnd mNpcTalkHistoryWnd = null;

    [HideInInspector]
    public UIItemsTrackCtrl mItemsTrackWnd = null;

    List<UIBaseWidget> mShowSystemWnds;



    void Awake()
    {
        mInstance = this;
        mShowSystemWnds = new List<UIBaseWidget>();
        mSystemUIMaskCollider.enabled = false;
        InstantiateGameUI();
        OnCreateGameUI();
        InitNeedPlayOpenAudioList();
    }

    void OnDestroy()
    {
        OnDestroyGameUI();
    }

    // add game ui
    void InstantiateGameUI()
    {
        // gameMain
        GameObject gameUI = AddUIPrefab(GameMainPrefab, gameObject.transform);
        mUIGameMenuCtrl = gameUI.GetComponentsInChildren<UIGameMenuCtrl>(true)[0];
        mUIGameMenuCtrl.Show();
        mUIMainMidCtrl = gameUI.GetComponentsInChildren<UIMainMidCtrl>(true)[0];
        mUIMainMidCtrl.Show();
        mUIMinMapCtrl = gameUI.GetComponentsInChildren<UIMinMapCtrl>(true)[0];
        mUIMinMapCtrl.Show();
        mNPCTalk = gameUI.GetComponentsInChildren<UINPCTalk>(true)[0];
        mNPCTalk.Hide();
        mNPCSpeech = gameUI.GetComponentInChildren<UINpcSpeech>();
        mServantTalk = gameUI.GetComponentsInChildren<UIServantTalk>(true)[0];
        mServantTalk.Hide();
        gameUI.SetActive(true);

        //UI System
        gameUI = AddUIPrefab(SytemPrefab, gameObject.transform);
        mSystemMenu = gameUI.GetComponentInChildren<UISystemMenu>();
        mSystemMenu.e_OnShow += OnSystemWndShow;
        mSystemMenu.e_OnHide += OnSystemWndHide;
        mOption = gameUI.GetComponentInChildren<UIOption>();
        mOption.e_OnShow += OnSystemWndShow;
        mOption.e_OnHide += OnSystemWndHide;
        mSaveLoad = gameUI.GetComponentInChildren<UISaveLoad>();
        mSaveLoad.e_OnShow += OnSystemWndShow;
        mSaveLoad.e_OnHide += OnSystemWndHide;
        gameUI.SetActive(true);

        //Map
        gameUI = AddUIPrefab(worldMapPrefab, gameObject.transform);
        mUIWorldMap = gameUI.GetComponent<UIWorldMapCtrl>();
        gameUI.SetActive(false);

        // UI Build
        gameUI = AddUIPrefab(buildPrefab, gameObject.transform);
        mBuildBlock = gameUI.GetComponent<UIBuildBlock>();
        gameUI.SetActive(false);

        // Tip end
        gameUI = AddUIPrefab(tipsWmdPrefab, gameObject.transform);
        mTipsCtrl = gameUI.GetComponent<UITips>();
        gameUI.SetActive(true);

        // TipRecords
        gameUI = AddUIPrefab(tipRecordsMgrPrefab, gameObject.transform);
        mTipRecordsMgr = gameUI.GetComponent<UITipRecordsMgr>();
        gameUI.SetActive(false);

        //wnd
        // player info
        gameUI = AddUIPrefab(playerInfoPrefab, tsCenter);
        mUIPlayerInfoCtrl = gameUI.GetComponent<UIPlayerInfoCtrl>();
        gameUI.transform.localPosition = UIDefaultPostion.Instance.pos_PlayerInfo;
        gameUI.SetActive(false);
        mUIPlayerInfoCtrl.transform.localPosition = UIDefaultPostion.Instance.pos_PlayerInfo;
        // Item Package
        gameUI = AddUIPrefab(ItemPackagePrefab, tsCenter);
        mItemPackageCtrl = gameUI.GetComponent<UIItemPackageCtrl>();
        gameUI.SetActive(false);
        mItemPackageCtrl.transform.localPosition = UIDefaultPostion.Instance.pos_ItemPackge;
        // Npc Strage
        gameUI = AddUIPrefab(npcStoragePrefab, tsCenter);
        mNpcStrageCtrl = gameUI.GetComponent<UINpcStorageCtrl>();
        gameUI.SetActive(false);
        mNpcStrageCtrl.transform.localPosition = UIDefaultPostion.Instance.pos_NpcStorage;
        // Compound Wnd
        gameUI = AddUIPrefab(compoundWndPrefab, tsCenter);
        mCompoundWndCtrl = gameUI.GetComponent<UICompoundWndControl>();
        gameUI.SetActive(false);
        mCompoundWndCtrl.transform.localPosition = UIDefaultPostion.Instance.pos_Compound;
        // Servant Wnd
        gameUI = AddUIPrefab(servantWndPrefab, tsCenter);
        mServantWndCtrl = gameUI.GetComponent<UIServantWnd>();
        gameUI.SetActive(false);
        mServantWndCtrl.transform.localPosition = UIDefaultPostion.Instance.pos_Servant;
        // Npc 
        gameUI = AddUIPrefab(npcGuiPrefab, tsCenter);
        mNpcWnd = gameUI.GetComponent<UINpcWnd>();
        gameUI.SetActive(false);
        mNpcWnd.transform.localPosition = UIDefaultPostion.Instance.pos_Npc;
        // Mission Wnd
        gameUI = AddUIPrefab(missionWndPrefab, tsCenter);
        mUIMissionWndCtrl = gameUI.GetComponent<UIMissionWndCtrl>();
        gameUI.SetActive(false);
        // MissionTrack Wnd
        gameUI = AddUIPrefab(missionTrackWndPrefab, tsCenterOther);
        mMissionTrackWnd = gameUI.GetComponent<UIMissionTrackCtrl>();
        mMissionTrackWnd.transform.localPosition = UIDefaultPostion.Instance.pos_MissionTruck;
        //		mMissionTrackWnd.transform.localPosition = new Vector3 (856,162,0);
        mMissionTrackWnd.transform.localPosition = new Vector3(Screen.width / 2 - 145, 35, 0);
        gameUI.SetActive(false);

        // Get Item Wnd
        gameUI = AddUIPrefab(itemGetWndPrefab, tsCenter);
        mItemGet = gameUI.GetComponent<UIItemGet>();
        gameUI.SetActive(false);
        // Item Option Wnd
        gameUI = AddUIPrefab(itemOpGuiPrefab, tsCenter);
        mItemOp = gameUI.GetComponent<UIItemOp>();
        gameUI.SetActive(false);
        // Shop Wnd
        gameUI = AddUIPrefab(shopGuiPrefab, tsCenter);
        mShopWnd = gameUI.GetComponent<UIShopWnd>();
        gameUI.SetActive(false);
        mShopWnd.transform.localPosition = UIDefaultPostion.Instance.pos_Shop;
        // Item Box
        gameUI = AddUIPrefab(itemBoxGuiPrefab, tsCenter);
        mItemBox = gameUI.GetComponent<UIItemBox>();
        gameUI.SetActive(false);
        // Repair Wnd
        gameUI = AddUIPrefab(repairWndGuiPrefab, tsCenter);
        mRepair = gameUI.GetComponent<UIRepairWnd>();
        gameUI.SetActive(false);
        mRepair.transform.localPosition = UIDefaultPostion.Instance.pos_Repair;
        // Power Plant Solar Wnd
        gameUI = AddUIPrefab(powerPlantSolarPrefab, tsCenter);
        mPowerPlantSolar = gameUI.GetComponent<UIPowerPlantSolar>();
        gameUI.SetActive(false);
        mPowerPlantSolar.transform.localPosition = UIDefaultPostion.Instance.pos_PowerPlant;
        // Revive Gui Wnd
        gameUI = AddUIPrefab(reviveGuiPrefab, tsCenter);
        mRevive = gameUI.GetComponent<UIRevive>();
        gameUI.SetActive(false);
        // Ware House Wnd
        gameUI = AddUIPrefab(wareHouseGuiPrefab, tsCenter);
        mWarehouse = gameUI.GetComponent<UIWarehouse>();
        gameUI.SetActive(false);
        mWarehouse.transform.localPosition = UIDefaultPostion.Instance.pos_WareHouse;
        //colony Wnd
        gameUI = AddUIPrefab(colonyWndPrefab, tsCenter);
        mCSUI_MainWndCtrl = gameUI.GetComponent<CSUI_MainWndCtrl>();
        gameUI.SetActive(false);
        //phone Wnd
        gameUI = AddUIPrefab(phoneWndPrefab, tsCenter);
        mPhoneWnd = gameUI.GetComponent<UIPhoneWnd>();
        gameUI.SetActive(false);
        // skill wnd
        gameUI = AddUIPrefab(skillWndPrefab, tsCenter);
        mSkillWndCtrl = gameUI.GetComponent<UISkillWndCtrl>();
        gameUI.SetActive(false);
        // workshop wnd
        gameUI = AddUIPrefab(workshopPrefb, tsCenter);
        mWorkShopCtrl = gameUI.GetComponent<UIWorkShopCtrl>();
        gameUI.SetActive(false);
        //information wnd
        gameUI = AddUIPrefab(informationPrefab, tsCenter);
        mTeamInfoMgr = gameUI.GetComponent<CSUI_TeamInfoMgr>();
        gameUI.SetActive(false);
        // railwayPoint
        gameUI = AddUIPrefab(railwayPonitPrefab, tsCenter);
        mRailwayPoint = gameUI.GetComponent<RailwayPointGui_N>();
        gameUI.SetActive(false);

        // mallWndPrefab
        if (Pathea.PeGameMgr.IsMulti)
        {
            gameUI = AddUIPrefab(mallWndPrefab, tsCenter);
            mMallWnd = gameUI.GetComponent<UIMallWnd>();
            gameUI.SetActive(false);
        }

        //UI Adminstrator
        //AdminUI
        gameUI = AddUIPrefab(AdministratorPrefab, tsCenter);
        mAdminstratorWnd = gameUI.GetComponent<UIAdminstratorWnd>();
        gameUI.SetActive(false);

        // Driving UI
        gameUI = AddUIPrefab(drivingPrefab, tsCenterOther);
        mDrivingCtrl = gameUI.GetComponent<UIDrivingCtrl>();
        gameUI.SetActive(false);

        // Custom stopwatch UI
        gameUI = AddUIPrefab(stopwatchPrefab, transform);
        mStopwatchList = gameUI.GetComponent<UIStopwatchList>();
        gameUI.SetActive(false);

        // Custom npc talk UI
        gameUI = AddUIPrefab(npcWndCustomPrefab, tsCenter);
        mNpcDialog = gameUI.GetComponent<UINpcDialog>();

        // Custom npc mission ui
        gameUI = AddUIPrefab(missionWndCustomPrefab, tsCenter);
        mMissionGoal = gameUI.GetComponent<UIMissionGoal>();

        // Custom MissionTrack ui
        gameUI = AddUIPrefab(missionTrackCustomPrefab, tsCenterOther);
        mCustomMissionTrack = gameUI.GetComponent<UIMissionTrack>();
        mCustomMissionTrack.transform.localPosition = UIDefaultPostion.Instance.pos_MissionTruck;
        //		mMissionTrackWnd.transform.localPosition = new Vector3 (856,162,0);
        mCustomMissionTrack.transform.localPosition = new Vector3(Screen.width / 2 - 145, 35, 0);

        //lz-2016.10.22 KickstarterCtrl
        gameUI = AddUIPrefab(mKickstarterCtrlPrefab, tsCenter);
        mKickstarterCtrl = gameUI.GetComponent<KickstarterCtrl>();
        gameUI.SetActive(false);

        //lz-2016.11.07 NpcTalkHistoryWnd
        gameUI = AddUIPrefab(mNpcTalkHistoryPrefab, tsCenter);
        mNpcTalkHistoryWnd = gameUI.GetComponent<NpcTalkHistoryWnd>();
        gameUI.SetActive(false);

        // itemsTrack Wnd
        gameUI = AddUIPrefab(itemsTrackWndPrefab, tsCenterOther);
        mItemsTrackWnd = gameUI.GetComponent<UIItemsTrackCtrl>();
        mItemsTrackWnd.transform.localPosition = mMissionTrackWnd.transform.localPosition + new Vector3(0, 170f, 0);
        gameUI.SetActive(false);
    }

    GameObject AddUIPrefab(GameObject prefab, Transform parentTs)
    {
        GameObject o = GameObject.Instantiate(prefab) as GameObject;
        o.transform.parent = parentTs;
        o.layer = parentTs.gameObject.layer;
        o.transform.localPosition = Vector3.zero;
        o.transform.localScale = Vector3.one;
        UIBaseWnd wnd = o.GetComponent<UIBaseWnd>();
        if (wnd != null)
            wnd.mAnchor = parentTs.gameObject.GetComponent<UIAnchor>();
        UIAnchor[] achors = o.transform.GetComponentsInChildren<UIAnchor>(true);
        foreach (UIAnchor anchor in achors)
            anchor.uiCamera = mUICamera;
        return o;
    }

    void OnCreateGameUI()
    {
        UIBaseWidget[] uiList = GetComponentsInChildren<UIBaseWidget>(true);
        foreach (UIBaseWidget widget in uiList)
            widget.OnCreate();
    }

    void OnDestroyGameUI()
    {
        PeGameMgr.gamePause = false;
        UIBaseWidget[] uiList = GetComponentsInChildren<UIBaseWidget>(true);
        foreach (UIBaseWidget widget in uiList)
            widget.OnDelete();
    }

    void OnSystemWndShow(UIBaseWidget widget = null)
    {
        if (!mShowSystemWnds.Contains(widget))
        {
            //lz-2016.06.14 打开系统菜单的时候隐藏锁定UI
            if (null != WhiteCat.LockUI.instance)
                WhiteCat.LockUI.instance.HideWhenUIPopup();
            //lz-2016.07.04打开系统菜单的时候激活遮挡碰撞器
            if (null != mSystemUIMaskCollider)
                mSystemUIMaskCollider.enabled = true;

            mShowSystemWnds.Add(widget);
            PeGameMgr.gamePause = true;
        }
    }


    void OnSystemWndHide(UIBaseWidget widget = null)
    {
        if (mShowSystemWnds.Remove(widget) && mShowSystemWnds.Count == 0)
        {
            //lz-2016.06.14 关闭系统菜单的时候显示锁定目标UI
            if (null != WhiteCat.LockUI.instance)
                WhiteCat.LockUI.instance.ShowWhenUIDisappear();
            //lz-2016.07.04 关闭系统菜单的时候取消遮挡碰撞器
            if (null != mSystemUIMaskCollider)
                mSystemUIMaskCollider.enabled = false;

            PeGameMgr.gamePause = false;
        }
    }


    #region public methods

    public void ChangeGameWndShowState()
    {
        tsCenter.gameObject.SetActive(!tsCenter.gameObject.activeSelf);

        tsCenterOther.gameObject.SetActive(!tsCenterOther.gameObject.activeSelf);
    }


    public void ShowGameWndAll()
    {
        ShowGameWnd();
        ShowGameWndOther();
    }

    public void ShowGameWnd()
    {
        tsCenter.gameObject.SetActive(true);
    }

    public void ShowGameWndOther()
    {
        tsCenterOther.gameObject.SetActive(true);
    }

    public void HideGameWndAll()
    {
        HideGameWnd();
        HideGameWndOther();
    }

    public void HideGameWnd()
    {
        tsCenter.gameObject.SetActive(false);
    }

    public void HideGameWndOther()
    {
        tsCenterOther.gameObject.SetActive(false);
    }

    [HideInInspector]
    //public Player mPlayer;
    public PeEntity mMainPlayer { get { return Pathea.PeCreature.Instance.mainPlayer; } }
    public bool bVoxelComplete { get { return VFVoxelTerrain.TerrainVoxelComplete; } }

    Pathea.PackageCmpt cmpt = null;
    public int playerMoney
    {
        get
        {
            if (mMainPlayer == null)
                return 0;
            if (cmpt == null)
                cmpt = mMainPlayer.GetCmpt<Pathea.PackageCmpt>();
            if (cmpt != null)
                return cmpt.money.current;
            return 0;
        }
        set
        {
            if (cmpt == null)
                cmpt = mMainPlayer.GetCmpt<Pathea.PackageCmpt>();
            cmpt.money.current = value;
        }
    }

    //lz-2016.05.28 获得玩家死亡状态
    public bool bMainPlayerIsDead
    {
        get
        {
            if (null!=Pathea.PeCreature.Instance&&null != Pathea.PeCreature.Instance.mainPlayer)
                return Pathea.PeCreature.Instance.mainPlayer.IsDeath();
            else
                return false;
        }
    }

    //--未实现的接口
    public bool bReflashUI { get { return refalhUI; } }
    public bool bPlayerOnCarrier { get { return false; } }
    public bool bPlayerOnTrain { get { return false; } }

    // test
    public bool refalhUI = true;

    #endregion


    //lz-2016.08.31 UI后期需要加播放声音的统一从这里调用
    #region Audio PlayMethods

    private const int m_PutOnEquipAudioID = 1683;
    private const int m_TakeOffEquipAudioID = 1684;
    private const int m_CompoundAudioID = 1685;


    public void PlayPutOnEquipAudio()
    {
        if (null != AudioManager.instance)
            AudioManager.instance.Create(Vector3.zero, m_PutOnEquipAudioID);
    }

    public void PlayTakeOffEquipAudio()
    {
        if (null != AudioManager.instance)
            AudioManager.instance.Create(Vector3.zero, m_TakeOffEquipAudioID);
    }

    private AudioController m_CompoundAudioCtrl;
    public void PlayCompoundAudioEffect()
    {
        if (null == m_CompoundAudioCtrl)
        {
            m_CompoundAudioCtrl = AudioManager.instance.Create(Vector3.zero, m_CompoundAudioID, null, false, false);
        }
        if (null != m_CompoundAudioCtrl && !m_CompoundAudioCtrl.isPlaying)
            m_CompoundAudioCtrl.PlayAudio();
    }

    public void StopCompoundAudioEffect()
    {
        if (null != m_CompoundAudioCtrl)
        {
            m_CompoundAudioCtrl.StopAudio();
        }
    }

    private AudioController wndOpenAudioCtrl = null;
    private const int m_WndOpenAudioID = 914;
    private const int m_NpcTalkWndOpenAudioID = 4104;
    public void PlayWndOpenAudioEffect(UIBaseWidget widget)
    {
        if (null != widget && NeedPlayAudioWndList.Contains(widget))
        {
            PlayWndOpenAudioEffect();
        }
    }

    public void PlayWndOpenAudioEffect()
    {
        if (null == wndOpenAudioCtrl)
            wndOpenAudioCtrl = AudioManager.instance.Create(Vector3.zero, m_WndOpenAudioID, null, false, false);
        if (null != wndOpenAudioCtrl && !wndOpenAudioCtrl.isPlaying)
            wndOpenAudioCtrl.PlayAudio();
    }

    /// <summary> 对话窗口打开的声音和其他窗口不一样</summary>
    public void PlayNpcTalkWndOpenAudioEffect()
    {
        AudioManager.instance.Create(Vector3.zero, m_NpcTalkWndOpenAudioID);
    }

    #endregion

    #region need play audio wnd


    public List<UIBaseWidget> NeedPlayAudioWndList = new List<UIBaseWidget>();

    /// <summary>
    /// lz-2016.09.13 需要播放打开界面声音的面板
    /// </summary>
    void InitNeedPlayOpenAudioList()
    {
        NeedPlayAudioWndList.Add(mTipRecordsMgr);

        NeedPlayAudioWndList.Add(mSystemMenu);
        NeedPlayAudioWndList.Add(mOption);
        NeedPlayAudioWndList.Add(mSaveLoad);

        NeedPlayAudioWndList.Add(mServantWndCtrl);
        NeedPlayAudioWndList.Add(mUIPlayerInfoCtrl);
        NeedPlayAudioWndList.Add(mUIMissionWndCtrl);
        NeedPlayAudioWndList.Add(mCustomMissionTrack.GetBaseWnd());
        NeedPlayAudioWndList.Add(mPhoneWnd);
        NeedPlayAudioWndList.Add(mCSUI_MainWndCtrl);
        NeedPlayAudioWndList.Add(mCompoundWndCtrl);
        NeedPlayAudioWndList.Add(mItemPackageCtrl);
        NeedPlayAudioWndList.Add(mItemOp);
        NeedPlayAudioWndList.Add(mNpcWnd);
    }
    #endregion

    #region Tutorial Check

    private const int m_BulidTalkID = 4076;
    private const int m_FullQuickBarTalkID = 4343;
    private const int m_GameMenuTalkID = 4344;
    private const int m_MinMapTalkID = 4345;
    private const int m_MissionTrackTalkID = 4346;

    /// <summary>通过对话ID检测触犯Tutorial</summary>
    public void CheckTalkIDShowTutorial(int talkID)
    {
        if (PeGameMgr.IsTutorial)
        {
            switch (talkID)
            {
                case m_BulidTalkID:
                    mBuildBlock.ShowAllBuildTutorial();
                    break;
                case m_FullQuickBarTalkID:
                    mUIMainMidCtrl.ShowFullQuickBarTutorial();
                    break;
                case m_GameMenuTalkID:
                    if (null != UIGameMenuCtrl.Instance)
                        UIGameMenuCtrl.Instance.ShowWndTutorial();
                    break;
                case m_MinMapTalkID:
                    if (null != UIMinMapCtrl.Instance)
                        UIMinMapCtrl.Instance.ShowMapTutorial();
                    break;
                case m_MissionTrackTalkID:
                    if (null != UIMinMapCtrl.Instance)
                        UIMinMapCtrl.Instance.ShowMissionTrackTutorial();
                    break;
                default:
                    break;
            }
            
        }
    }

    /// <summary>通过任务ID检测触犯Tutorial</summary>
    public void CheckMissionIDShowTutorial(int missionID)
    {
        if (PeGameMgr.IsTutorial)
        {
            switch (missionID)
            {
                case TrainingScene.TrainingRoomConfig.ChangeControlModeID:
                    //lz-2016.11.03 739显示MissionTrackWnd的窗体引导提示
                    mMissionTrackWnd.ShowWndTutorial();
                    break;
                case TrainingScene.TrainingRoomConfig.PutMed:
                    //lz-2016.11.03 744任务的时候标记背包中ProtoID为916的物品
                    mItemPackageCtrl.AddTutorialItemProtoID(916);
                    //lz-2016.11.03 744任务的时候显示快捷栏引导提示
                    mUIMainMidCtrl.ShowQuickBarTutorial();
                    break;
                case TrainingScene.TrainingRoomConfig.EquipKnife:
                    //lz-2016.11.03 745任务的时候标记背包中ProtoID为33的物品
                    mItemPackageCtrl.AddTutorialItemProtoID(33);
                    break;
                case TrainingScene.TrainingRoomConfig.CutID:
                    //lz-2016.11.03 748任务的时候标记背包中ProtoID为1527的物品
                    mItemPackageCtrl.AddTutorialItemProtoID(1527);
                    break;
                case TrainingScene.TrainingRoomConfig.DigID:
                    //lz-2016.11.03 751任务的时候标记背包中ProtoID为12的物品
                    mItemPackageCtrl.AddTutorialItemProtoID(12);
                    break;
                case TrainingScene.TrainingRoomConfig.Replicator:
                    //lz-2016.11.03 757任务的时候标记背包中ProtoID为406的物品
                    mItemPackageCtrl.AddTutorialItemProtoID(406);
                    break;
                default:
                    break;
            }
        }
    }

    #endregion
}
