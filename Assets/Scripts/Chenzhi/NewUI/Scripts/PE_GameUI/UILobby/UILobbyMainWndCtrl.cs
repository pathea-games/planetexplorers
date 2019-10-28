#define MAINMENU_ISO
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CustomData;
using System.IO;
using System.Linq;
using System;



public class PlayerInfo
{
    //	public AppearanceData 	mAppearanceData;
    //	public int[]			mEquitment;
    public string mName;
    public int mLevel;
    public int mSex;
    public float mWinnRate;
}

public class RoomPlayerInfo
{
    public PlayerInfo mPlayerInfo = new PlayerInfo();
    public int mId;
    public int mDelay;
    public int mFocreID;
    public int mRoleID;
    public int mState;
    public bool mRoomMaster;
    public int mSelfCreateItemID;
}

public class UILobbyMainWndCtrl : UIStaticWnd
{
    static UILobbyMainWndCtrl mInstance;
    public static UILobbyMainWndCtrl Instance { get { return mInstance; } }

    public Camera mUICamera;

    public GameObject mRoomWnd;
    public GameObject mButtomAuthor;
    public GameObject mTopLeftAuthor;
    public GameObject mUICenter;


    public GameObject mUIServerWnd;
    public GameObject mPassWordWnd;
    public GameObject mWorkShopPrfab;
    public GameObject mHostCreateWnd;

    public int mPlayerPageIndex;
    public UIPageListCtrl mPlayerList;
    public int mRoomListPage;
    public UIPageListCtrl mRoomList;
    private RoleInfo mRoleInfo = null;

    public UITexture mTxPlayerHerder;
    public UILabel mLbPlayerName;
    public UILabel mLbPlayerLv;
    public UIInput mRoomInput;
    public UIInput mPlayerInput;
    public UIInput mCheckPasswordInput;
    public UIInput mMsgText;

    public UITalkBoxCtrl mTalkBoxControl;


    private RecentRoomDataManager mRecentRoom_M;
    private List<ServerRegistered> _curServerList = new List<ServerRegistered>();
    private List<ServerRegistered> _serverListInter = new List<ServerRegistered>();
    private List<ServerRegistered> _serverListLan = new List<ServerRegistered>();
    //	private List<ServerRegistered> _serverListProxy = new List<ServerRegistered>();
    internal ServerRegistered mSelectServerData = null;

    private UIWorkShopCtrl mWorkShopCtrl = null;

    internal ServerRegistered mInviteServerData = null;

    public delegate void LobbyUIStart();
    public event LobbyUIStart e_LobbyUIStart = null;


    void Awake()
    {
        mInstance = this;

        PlayMusic();

        mPlayerPageIndex = 0;
        mRoomListPage = 0;
        mRoomList.CheckItem += roomListChickItem;
        mRoomList.DoubleClickItem += RoomListDoubleClickItem;
        //e_LobbyUIStart += GameClientLobby.Self.QueryLobbyExp;
    }


    public override void Show()
    {
        mButtomAuthor.gameObject.SetActive(true);
        base.Show();
    }

    protected override void OnHide()
    {
        mButtomAuthor.gameObject.SetActive(false);
        base.OnHide();
    }

    public void SetInviteServerData(long serverUID)
    {
        mInviteServerData = null;
        if (mInviteServerData == null)
        {
            mInviteServerData = _serverListInter.Find(
                delegate(ServerRegistered sr)
                {
                    return sr.ServerUID == serverUID;
                });
        }
        if (mInviteServerData == null)
        {
            mInviteServerData = _serverListLan.Find(
                delegate(ServerRegistered sr)
                {
                    return sr.ServerUID == serverUID;
                });
        }
        //		if( mInviteServerData == null)
        //		{
        //			mInviteServerData = _serverListProxy.Find(
        //				delegate(ServerRegistered sr)
        //				{
        //				return sr.ServerUID == serverUID;
        //			});
        //		}
    }

    public bool HaveServerID(long serverUID)
    {
        foreach (var item in _serverListInter)
        {
            if (item.ServerUID == serverUID)
                return true;
        }
        foreach (var item in _serverListLan)
        {
            if (item.ServerUID == serverUID)
                return true;
        }
        //		foreach( var item in _serverListProxy)
        //		{
        //			if(item.ServerUID == serverUID)
        //				return true;
        //		}
        return false;
    }
    //    void OnEnable()
    //	{
    //		GameClientNetwork.HostListReceivedEvent += OnHostListReceived;
    //	}
    //
    //	void OnDisable()
    //	{
    //		GameClientNetwork.HostListReceivedEvent -= OnHostListReceived;
    //		CancelInvoke("RequestHostList");
    //	}

    // Use this for initialization
    void Start()
    {
#if MAINMENU_ISO
		BtnWorkShopsOnClick();
		if(mTopLeftAuthor != null)
			mTopLeftAuthor.transform.parent.gameObject.SetActive(false);
		if(mButtomAuthor != null)
			mButtomAuthor.SetActive(false);
		return;
#endif
        PeSteamFriendMgr.Instance.Init(mTopLeftAuthor.transform, mUICenter.transform, mUICamera);

        if (GameClientLobby.Self == null)
            return;
        mRoleInfo = GameClientLobby.role;
        SetRoleInfo();

        mRecentRoom_M = new RecentRoomDataManager(mRoleInfo.name);
        mRecentRoom_M.LoadFromFile();

        StartCoroutine(UpdatePlayerInfo());
        StartCoroutine(UpdateRoomInfo());

        InitRoomListSort();

        if (e_LobbyUIStart != null)
            e_LobbyUIStart();
    }

    void InitRoomListSort()
    {
        for (int i = 0; i < mRoomList.mHeaderItems.Count; i++)
        {
            if (i == 0 || i == 3)
            {
                mRoomList.mHeaderItems[i].InitSort(true);
                if (i == 0)
                    mRoomList.mHeaderItems[i].SetSortSatate(2);
                else
                    mRoomList.mHeaderItems[i].SetSortSatate(0);

                mRoomList.mHeaderItems[i].eSortOnClick += OnClickSort;
            }
            else
                mRoomList.mHeaderItems[i].InitSort(false);
        }

    }

    PeLobbyLevel lobbyLevel = null;

    void SetRoleInfo()
    {
        if (mRoleInfo == null)
            return;
        mLbPlayerName.text = mRoleInfo.name;
        ResetLevel();
        Texture2D mHeader = RoleHerderTexture.GetTexture();
        if (mHeader != null)
            mTxPlayerHerder.mainTexture = mHeader;
    }

    void Update()
    {
#if MAINMENU_ISO
		return;
#endif
        UpdateLobbyLevel();
    }

    void UpdateLobbyLevel()
    {
        if (lobbyLevel == null)
            return;
        if (mRoleInfo.lobbyExp >= lobbyLevel.exp + lobbyLevel.nextExp)
        {
            ResetLevel();
        }
    }

    void ResetLevel()
    {
        lobbyLevel = PeLobbyLevel.Mgr.Instance.GetLevel(mRoleInfo.lobbyExp);
        mLbPlayerLv.text = (lobbyLevel != null) ? lobbyLevel.level.ToString() : "0";
    }


    public void PlayMusic()
    {
        //		if(LogoGui_N.Instance != null)
        //			LogoGui_N.Instance.SetBGM("Sound/Music/PE_Multiplayer");
    }

    public void AddTalk(string name, string content)
    {
        if (mTalkBoxControl == null)
            return;

        //大厅聊天颜色区分
        if (GameClientLobby.role.name == name)
            mTalkBoxControl.AddMsg(name, content, "99C68B");
        else
            mTalkBoxControl.AddMsg(name, content, "EDB1A6");
    }

    IEnumerator UpdatePlayerInfo()
    {
        while (true)
        {
            RefreshPlayerList();

            yield return new WaitForSeconds(1.0f);
        }
    }

    public void RefreshPlayerList()
    {
        if (GameClientLobby.Self == null)
            return;

        if (mPlayerPageIndex == 0)
        {
            List<RoleInfoProxy> roles = GameClientLobby.Self.m_RolesInLobby;
            mPlayerList.mItems.Clear();
            for (int i = 0; i < roles.Count; i++)
            {
                List<string> mLsit = new List<string>();
                mLsit.Add(roles[i].name);
                //lz-2016.11.07 取消显示玩家等级
                //mLsit.Add(PeLobbyLevel.Mgr.Instance.GetLevel(roles[i].lobbyExp).level.ToString());
                //lz-2016.11.10 取消PlayTime显示
                //mLsit.Add(((int)(roles[i].winrate * 100f)).ToString() + "%");


                if (QueryPlayerText.Length > 0)
                {
                    if (QueryItem(QueryPlayerText, mLsit[0]))
                        mPlayerList.AddItem(mLsit);
                }
                else
                    mPlayerList.AddItem(mLsit);
            }
            mPlayerList.UpdateList();
        }
        else if (mPlayerPageIndex == 1)
        {
            mPlayerList.mItems.Clear();
            mPlayerList.UpdateList();
        }
    }

    [HideInInspector]
    public bool bGetRoomInfo = false;
    //bool LockRoomList = false;
    IEnumerator UpdateRoomInfo()
    {
        mRoomListPage = 0;

        uLink.MasterServer.ipAddress = ClientConfig.ProxyIP;
        uLink.MasterServer.port = ClientConfig.ProxyPort;
        uLink.MasterServer.password = "patheahaha";
        uLink.MasterServer.updateRate = 4f;

        uLink.MasterServer.RequestHostList("PatheaGame");
        uLink.MasterServer.DiscoverLocalHosts("PatheaGame", 9900, 9915);
        yield return new WaitForSeconds(3f);

        while (true)
        {
            _serverListInter.Clear();
            _serverListLan.Clear();

            // Internet
            if (uLobby.Lobby.isConnected)
            {
                IEnumerable<uLobby.ServerInfo> lobbySrvs = uLobby.ServerRegistry.GetServers();
                foreach (uLobby.ServerInfo server in lobbySrvs)
                {
                    ServerRegistered reg = new ServerRegistered();
                    reg.AnalyseServer(server);
                    _serverListInter.Add(reg);
                }
            }

            uLink.HostData[] servers = uLink.MasterServer.PollHostList();
            foreach (uLink.HostData server in servers)
            {
                ProxyServerRegistered reg = new ProxyServerRegistered();
                reg.AnalyseServer(server, false);
                _serverListInter.Add(reg);
            }

            uLink.HostData[] datas = uLink.MasterServer.PollDiscoveredHosts();
            foreach (uLink.HostData data in datas)
            {
                ServerRegistered server = new ServerRegistered();
                server.AnalyseServer(data, true);
                _serverListLan.Add(server);
            }

            uLink.MasterServer.ClearHostList();
            uLink.MasterServer.ClearDiscoveredHosts();

            RefreshRoomList();

            uLink.MasterServer.RequestHostList("PatheaGame");
            uLink.MasterServer.DiscoverLocalHosts("PatheaGame", 9900, 9915);
            bGetRoomInfo = true;

            yield return new WaitForSeconds(5f);
        }
    }

    //	void OnHostListReceived(object sender, EventArgs args)
    //	{
    //		_serverListProxy.Clear();
    //
    //		uLink.HostData[] proxyServers = uLink.MasterServer.PollHostList();
    //		foreach (uLink.HostData data in proxyServers)
    //		{
    //			ProxyServerRegistered server = new ProxyServerRegistered();
    //			server.AnalyseServer(data);
    //			_serverListProxy.Add(server);
    //		}
    //
    //		RefreshRoomList();
    //
    //		Invoke("RequestHostList", 3f);
    //	}

    //	void RequestHostList()
    //	{
    //		uLink.MasterServer.ClearHostList();
    //		uLink.MasterServer.RequestHostList("PatheaGame");
    //	}

    void LockSortOnClick()
    {
        OnClickSort(100, mRoomList.mLockSortState);
    }

    public enum SortType
    {
        mLock = 0,
        mRoomNO = 1,
        mPalyerNO = 2
    }
    bool bSortDn = true;
    SortType mSortType = SortType.mRoomNO;

    void OnClickSort(int index, int sortSatae)
    {

        int newSortSate = -1;
        if (sortSatae == 0)
        {
            newSortSate = 2;
            bSortDn = true;
        }
        else if (sortSatae == 1)
        {
            newSortSate = 2;
            bSortDn = true;
        }
        else if (sortSatae == 2)
        {
            newSortSate = 1;
            bSortDn = false;
        }
        else
            return;

        if (index == 0)
        {
            mSortType = SortType.mRoomNO;
            mRoomList.mHeaderItems[3].SetSortSatate(0);
            mRoomList.mHeaderItems[0].SetSortSatate(newSortSate);
            mRoomList.SetLockUIState(0);
        }
        else if (index == 3)
        {
            mSortType = SortType.mPalyerNO;
            mRoomList.mHeaderItems[0].SetSortSatate(0);
            mRoomList.mHeaderItems[3].SetSortSatate(newSortSate);
            mRoomList.SetLockUIState(0);
        }
        else if (index == 100)// mLock
        {
            mSortType = SortType.mLock;
            mRoomList.mHeaderItems[0].SetSortSatate(0);
            mRoomList.mHeaderItems[3].SetSortSatate(0);
            mRoomList.SetLockUIState(newSortSate);
        }
        else
            return;

        RefreshRoomList();
    }


    void SortRoomList()
    {
        // xian id pai xu
        _curServerList.Sort(delegate(ServerRegistered _one, ServerRegistered _two)
        {

            if (object.ReferenceEquals(_one, null))
            {
                if (object.ReferenceEquals(_two, null))
                    return 0;

                return -1;
            }

            if (_one.ServerID == _two.ServerID)
                return 0;
            else if (_one.ServerID > _two.ServerID)
                return 1;
            else
                return -1;
        });


        int returnValue = bSortDn ? 1 : -1;

        _curServerList.Sort(delegate(ServerRegistered _one, ServerRegistered _two)
        {

            if (object.ReferenceEquals(_one, null))
            {
                if (object.ReferenceEquals(_two, null))
                    return 0;

                return -1;
            }

            if (mSortType == SortType.mLock)
            {
                if (_one.PasswordStatus == _two.PasswordStatus)
                    return 0;
                else if (_one.PasswordStatus > _two.PasswordStatus)
                    return returnValue;
                else
                    return -returnValue;
            }
            else if (mSortType == SortType.mPalyerNO)
            {
                if (_one.CurConn == _two.CurConn)
                    return 0;
                else if (_one.CurConn > _two.CurConn)
                    return returnValue;
                else
                    return -returnValue;
            }
            else
            {
                if (_one.ServerID == _two.ServerID)
                    return 0;
                else if (_one.ServerID > _two.ServerID)
                    return returnValue;
                else
                    return -returnValue;
            }
        });

    }

    private long roomUID;
    public long RoomUID
    {
        get
        {
            return roomUID;
        }
    }
    private int checkIndex = -1;
    void roomListChickItem(int index)
    {
        if (mRoomListPage == 0 || mRoomListPage == 1)
        {
            //lz-2016.11.15 Crush bug
            if (index>=0&&index < _curServerList.Count)
            {
                roomUID = _curServerList[index].ServerUID;
                mSelectServerData = _curServerList[index];
                checkIndex = index;
            }
            else
                checkIndex = -1;
        }
        else if (mRoomListPage == 2)
        {
            //lz-2016.11.15 Crush bug
            if (index >= 0 && index < mRecentRoom_M.mRecentRoomList.Count)
            {
                roomUID = mRecentRoom_M.mRecentRoomList[index].mUID;

                ServerRegistered mServerData = _curServerList.Find(
                    delegate(ServerRegistered sr)
                    {
                        return sr.ServerUID == roomUID;
                    });
                mSelectServerData = mServerData;
                checkIndex = index;
            }
            else
                checkIndex = -1;
        }
    }


    void RoomListDoubleClickItem(int index)
    {
        BtnJoinOnClick();
    }

    List<string> ServerDataToList(ServerRegistered mServerData)
    {
        List<string> mLsit = new List<string>();

        if (mServerData.ServerID <= -1)
        {
            mLsit.Add("[6666FF]OFFICIAL");
            mLsit.Add("[6666FF]" + mServerData.ServerName);
        }
        else
        {
            if (mServerData.UseProxy)
            {
                mLsit.Add("[99CC00]Proxy");
                mLsit.Add("[99CC00]" + mServerData.ServerName);
            }
            else
            {
                mLsit.Add(mServerData.ServerID.ToString());
                mLsit.Add(mServerData.ServerName);
            }
        }

        mLsit.Add(mServerData.ServerMasterName);
        mLsit.Add(mServerData.CurConn.ToString() + "/" + mServerData.LimitedConn.ToString());

        Pathea.PeGameMgr.EGameType type = (Pathea.PeGameMgr.EGameType)mServerData.GameType;
        string strTemp;
        switch (type)
        {
            case Pathea.PeGameMgr.EGameType.Cooperation:
                strTemp = "Cooperation";
                break;
            case Pathea.PeGameMgr.EGameType.VS:
                strTemp = "VS";
                break;
            case Pathea.PeGameMgr.EGameType.Survive:
                strTemp = "Survive";
                break;
            default:
                strTemp = "Cooperation";
                break;
        }
        mLsit.Add(strTemp);


        Pathea.PeGameMgr.ESceneMode mode = (Pathea.PeGameMgr.ESceneMode)mServerData.GameMode;
        switch (mode)
        {
            case Pathea.PeGameMgr.ESceneMode.Adventure:
                strTemp = "Adventure";
                break;
            case Pathea.PeGameMgr.ESceneMode.Build:
                strTemp = "Build";
                break;
            case Pathea.PeGameMgr.ESceneMode.Custom:
                strTemp = "Custom";
                break;
            case Pathea.PeGameMgr.ESceneMode.Story:
                strTemp = "Story";
                break;
            default:
                strTemp = "Adventure";
                break;
        }
        mLsit.Add(strTemp);

        mLsit.Add(mServerData.Ping.ToString());

        if (((EServerStatus)mServerData.ServerStatus & EServerStatus.Prepared) == EServerStatus.Prepared)
            strTemp = "Waiting";
        else
            strTemp = "InProgress";

        mLsit.Add(strTemp);
        mLsit.Add(mServerData.ServerVersion);

        return mLsit;
    }

    void RefreshRoomList()
    {
        if (mRoomList == null)
            return;

        _curServerList.Clear();
        if (mRoomListPage == 0)
        {
            foreach (ServerRegistered server in _serverListInter)
            {
                if (QueryRoomText.Length > 0)
                {
                    if (QueryItem(QueryRoomText, server.ServerID.ToString()) || QueryItem(QueryRoomText, server.ServerName))
                        _curServerList.Add(server);
                }
                else
                    _curServerList.Add(server);
            }

        }
        else if (mRoomListPage == 1)
        {
            foreach (ServerRegistered lan in _serverListLan)
            {
                if (QueryRoomText.Length > 0)
                {
                    if (QueryItem(QueryRoomText, lan.ServerID.ToString()) || QueryItem(QueryRoomText, lan.ServerName))
                        _curServerList.Add(lan);
                }
                else
                    _curServerList.Add(lan);
            }
        }
        else if (mRoomListPage == 2)
        {
            foreach (ServerRegistered lan in _serverListLan)
                _curServerList.Add(lan);
            foreach (ServerRegistered server in _serverListInter)
                _curServerList.Add(server);
        }

        SortRoomList();

        if (mRoomListPage == 0 || mRoomListPage == 1)
        {
            mRoomList.mItems.Clear();
            int SeletedIndex = -1;
            for (int i = 0; i < _curServerList.Count; i++)
            {
                List<string> mLsit = ServerDataToList(_curServerList[i]);

                PageListItem item = new PageListItem();
                item.mData = mLsit;
                item.mColor = Color.white;
                item.mEanbleICon = (_curServerList[i].PasswordStatus == 1);

                mRoomList.AddItem(item);

                if (roomUID == _curServerList[i].ServerUID)
                    SeletedIndex = i;
            }

            mRoomList.mSelectedIndex = SeletedIndex;
            mRoomList.UpdateList();
        }

        else if (mRoomListPage == 2)  // recent
        {
            mRoomList.mItems.Clear();
            int SeletedIndex = -1;


            for (int i = 0; i < mRecentRoom_M.mRecentRoomList.Count; i++)
            {
                long UID = mRecentRoom_M.mRecentRoomList[i].mUID;

                ServerRegistered mServerData = _curServerList.Find(
                    delegate(ServerRegistered sr)
                    {
                        return sr.ServerUID == UID;
                    });

                // Can Find
                if (mServerData != null)
                {
                    List<string> mLsit = ServerDataToList(mServerData);

                    PageListItem item = new PageListItem();
                    item.mData = mLsit;
                    item.mColor = Color.white;
                    item.mEanbleICon = (mServerData.PasswordStatus == 1);

                    if (QueryRoomText.Length > 0 && mLsit.Count >= 2)
                    {
                        if (QueryItem(QueryRoomText, mLsit[0]) || QueryItem(QueryRoomText, mLsit[1]))
                            mRoomList.AddItem(item);
                    }
                    else
                        mRoomList.AddItem(item);
                }
                else
                {

                    List<string> mLsit = new List<string>();
                    mLsit.Add("");
                    mLsit.Add(mRecentRoom_M.mRecentRoomList[i].mRoomName);
                    mLsit.Add(mRecentRoom_M.mRecentRoomList[i].mCreator);
                    mLsit.Add("");
                    mLsit.Add("");
                    mLsit.Add("");
                    mLsit.Add("");
                    mLsit.Add("");
                    mLsit.Add(mRecentRoom_M.mRecentRoomList[i].mVersion);

                    PageListItem item = new PageListItem();
                    item.mData = mLsit;
                    item.mColor = Color.gray;
                    item.mEanbleICon = false;

                    if (QueryRoomText.Length > 0 && mLsit.Count >= 2)
                    {
                        if (QueryItem(QueryRoomText, mLsit[1]))
                            mRoomList.AddItem(item);
                    }
                    else
                        mRoomList.AddItem(item);
                }

                if (roomUID == UID)
                    SeletedIndex = i;
            }

            mRoomList.mSelectedIndex = SeletedIndex;
            mRoomList.UpdateList();
        }
    }

    private bool QueryItem(string text, string ItemName)
    {
        try
        {
            if (text.Trim().Length == 0)
                return true;

            ItemName = ItemName.ToLower();

            Regex r = new Regex(text); // 定义一个Regex对象实例
            Match m = r.Match(ItemName); // 在字符串中匹配

            if (m.Success)
                return true;
            else
                return false;
        }
        catch
        {
            return false;
        }
    }

    void SendMsg()
    {
        string chatStr = mMsgText.text;
        //lz-2016.12.12 发消息的时候加入语言识别，方便显示处理
        chatStr += SystemSettingData.Instance.IsChinese ? UITalkBoxCtrl.LANGE_CN : UITalkBoxCtrl.LANGE_OTHER;

        LobbyInterface.LobbyRPC(ELobbyMsgType.SendMsg,
                                      EMsgType.ToAll,
                                      GameClientLobby.role.steamId,
                                      GameClientLobby.role.name,
                                      chatStr);

        mMsgText.text = "";


        Invoke("GetInputFocus", 0.1f);
    }

    void GetInputFocus()
    {
        mMsgText.selected = true;
    }



    // UI evnet func
    string QueryRoomText = string.Empty;
    void BtnQueryRoomOnClick()
    {
        QueryRoomText = mRoomInput.text;
        QueryRoomText = QueryRoomText.Replace("*", "");
        QueryRoomText = QueryRoomText.Replace("$", "");
        QueryRoomText = QueryRoomText.Replace("(", "");
        QueryRoomText = QueryRoomText.Replace(")", "");
        QueryRoomText = QueryRoomText.Replace("@", "");
        QueryRoomText = QueryRoomText.Replace("^", "");
        QueryRoomText = QueryRoomText.Replace("[", "");
        QueryRoomText = QueryRoomText.Replace("]", "");
        QueryRoomText = QueryRoomText.Replace(" ", "");
        mRoomInput.text = QueryRoomText;

        // text change to lower
        QueryRoomText = QueryRoomText.ToLower();

        RefreshRoomList();
    }
    void BtnClearQueryRoomOnClick()
    {
        QueryRoomText = "";
        mRoomInput.text = "";
        RefreshRoomList();
    }



    string QueryPlayerText = string.Empty;
    void BtnSearchPlayerOnClick()
    {
        QueryPlayerText = mPlayerInput.text;
        QueryPlayerText = QueryPlayerText.Replace("*", "");
        QueryPlayerText = QueryPlayerText.Replace("$", "");
        QueryPlayerText = QueryPlayerText.Replace("(", "");
        QueryPlayerText = QueryPlayerText.Replace(")", "");
        QueryPlayerText = QueryPlayerText.Replace("@", "");
        QueryPlayerText = QueryPlayerText.Replace("^", "");
        QueryPlayerText = QueryPlayerText.Replace("[", "");
        QueryPlayerText = QueryPlayerText.Replace("]", "");
        QueryPlayerText = QueryPlayerText.Replace(" ", "");
        mPlayerInput.text = QueryPlayerText;

        // text change to lower
        QueryPlayerText = QueryPlayerText.ToLower();

        RefreshPlayerList();
    }

    void BtnClearPlayerOnClick()
    {
        QueryPlayerText = "";
        mPlayerInput.text = "";
        RefreshPlayerList();
    }



    void ListTitleAllOnActive(bool isActive)
    {
        if (isActive)
        {
            mPlayerPageIndex = 0;
            mPlayerList.UpdateList();
        }
    }

    void ListTitleFriendsOnActive(bool isActive)
    {
        if (isActive)
        {
            mPlayerPageIndex = 1;
            mPlayerList.UpdateList();
        }
    }

    void ListTitleInternetOnActive(bool isActive)
    {
        if (isActive)
        {
            mRoomListPage = 0;
            roomUID = 0;
            mRoomList.ClearSelected();
            RefreshRoomList();
        }
    }

    void ListTitleLanOnActive(bool isActive)
    {
        if (isActive)
        {
            mRoomListPage = 1;
            roomUID = 0;
            mRoomList.ClearSelected();
            RefreshRoomList();
        }
    }


    void ListTitleRecentOnActive(bool isActive)
    {
        if (isActive)
        {
            mRoomListPage = 2;
            roomUID = 0;
            mRoomList.ClearSelected();
            RefreshRoomList();
        }
    }

    void BtnCharacterOnClick()
    {

        if (Input.GetMouseButtonUp(0) && GameClientLobby.Self != null)
        {
            GameClientLobby.Self.BackToRole();
        }
    }


    void BtnMainMenuOnClick()
    {
        if (Input.GetMouseButtonUp(0))
        {
            MessageBox_N.ShowYNBox(PELocalization.GetString(8000052), delegate
            {
                StopCoroutine(UpdatePlayerInfo());
                StopCoroutine(UpdateRoomInfo());
                PeSceneCtrl.Instance.GotoMainMenuScene();
            });
        }
    }

    void BtnRefreshOnClick()
    {
        //if (Input.GetMouseButtonUp(0))
        //{
        //	UpdateRoomInfo();
        //}
    }

    void BtnDeleteOnClick()
    {
        if (Input.GetMouseButtonUp(0))
        {

            //if(LockRoomList)
            //	return;

            if (mRoomListPage == 0 || mRoomListPage == 1)
            {
                if (mRoomList.mSelectedIndex > -1 && mRoomList.mSelectedIndex < _curServerList.Count)
                {
                    if (!GameClientLobby.role.name.Equals(_curServerList[mRoomList.mSelectedIndex].ServerMasterName))
                        return;

                    LobbyInterface.LobbyRPC(ELobbyMsgType.CloseServer, _curServerList[mRoomList.mSelectedIndex].ServerID, SteamMgr.steamId.m_SteamID);
                    MessageBox_N.ShowMaskBox(MsgInfoType.ServerDeleteMask, PELocalization.GetString(8000058), 15f);
                }
            }
            else if (mRoomListPage == 2)
            {
                if (roomUID != 0)
                    mRecentRoom_M.DeleteItem(roomUID);
            }
            roomUID = 0;
            RefreshRoomList();
            if (checkIndex != -1)
                roomListChickItem(checkIndex);
        }
    }

    void BtnHostOnClick()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (!UnityEngine.Network.HavePublicAddress())
                MessageBox_N.ShowOkBox(PELocalization.GetString(8000057), ShowUIServerWnd);
            else
                ShowUIServerWnd();
        }
    }

    void ShowUIServerWnd()
    {
        mRoomWnd.SetActive(false);
        mUIServerWnd.SetActive(true);
    }

    void BtnJoinOnClick()
    {
        if (Input.GetMouseButtonUp(0) && 0 != roomUID)
        {
            //if(LockRoomList)
            //	return;

            if (mSelectServerData == null)
            {
                MessageBox_N.ShowOkBox(UIMsgBoxInfo.mRoomIsClose.GetString());
                return;
            }


            bool mNeedPaseWord = (mSelectServerData.PasswordStatus == 1);
            if (mNeedPaseWord)
            {
                mPassWordWnd.SetActive(true);
            }
            else
            {
                ConnectServer(false, mSelectServerData);
            }

            ServerRegistered mServerData = _curServerList.Find(
                delegate(ServerRegistered sr)
                {
                    return sr.ServerUID == roomUID;
                });

			if (mServerData != null)
			{
				Debug.Log(mServerData.ServerUID);
				mRecentRoom_M.AddItem(mServerData.ServerUID, mServerData.ServerName, mServerData.ServerMasterName, mServerData.ServerVersion);
			}
        }
    }

    public void JoinToServerByInvite()
    {
        if (mInviteServerData == null)
        {
            MessageBox_N.ShowOkBox(UIMsgBoxInfo.mRoomIsClose.GetString());
            return;
        }


        bool mNeedPaseWord = (mInviteServerData.PasswordStatus == 1);
        if (mNeedPaseWord)
        {
            mPassWordWnd.SetActive(true);
        }
        else
        {
            ConnectServer(false, mInviteServerData);
            //SteamFriendPrcMgr.Instance.InviteClear();
        }

        ServerRegistered mServerData = _curServerList.Find(
            delegate(ServerRegistered sr)
            {
                return sr.ServerUID == roomUID;
            });

        if (mServerData != null)
            mRecentRoom_M.AddItem(mServerData.ServerUID, mServerData.ServerName, mServerData.ServerMasterName, mServerData.ServerVersion);
    }

    void OnPasswordOkBtn()
    {
        if (mInviteServerData != null)
        {
            ConnectServer(true, mInviteServerData);
            SetInviteServerData(-1);
        }
        else if (null != mSelectServerData)
        {
            ConnectServer(true, mSelectServerData);
        }
        mPassWordWnd.SetActive(false);
    }

    void ConnectServer(bool needPasswold, ServerRegistered data)
    {
        if (null != data)
        {
            if (data.GameMode == (int)Pathea.PeGameMgr.ESceneMode.Custom)
            {
                if (string.IsNullOrEmpty(data.UID) || string.IsNullOrEmpty(data.MapName))
                    return;

                string filePath = Path.Combine(GameConfig.CustomDataDir, data.MapName);
                Pathea.CustomGameData.Mgr.Instance.curGameData = Pathea.CustomGameData.Mgr.Instance.GetCustomData(data.UID, filePath);
                if (null == Pathea.CustomGameData.Mgr.Instance.curGameData)
                    return;

                Pathea.PeGameMgr.mapUID = data.UID;
                ScenarioIntegrityCheck check = ScenarioMapUtils.CheckIntegrityByPath(filePath);
                StartCoroutine(ProcessIntegrityCheck(check, needPasswold, data));
            }
            else
            {
                Connect(needPasswold, data);
            }
        }
    }

    void Connect(bool needPasswold, ServerRegistered data)
    {
        MyServerManager.LocalIp = data.IPAddress;
        MyServerManager.LocalPort = data.Port;
        MyServerManager.LocalPwd = needPasswold ? mCheckPasswordInput.text : "";
        MyServerManager.LocalHost = data;

        GameClientNetwork.Connect();

        PeSteamFriendMgr.Instance.mMyServerUID = data.ServerUID;
    }

    IEnumerator ProcessIntegrityCheck(ScenarioIntegrityCheck check, bool needPasswold, ServerRegistered data)
    {
        while (true)
        {
            if (check.integrated == true)
            {
                Connect(needPasswold, data);
                break;
            }
            else if (check.integrated == false)
            {
                MessageBox_N.ShowOkBox(PELocalization.GetString(8000484));
                yield break;
            }

            yield return null;
        }
    }

    void OnPasswordCancelBtn()
    {
        mPassWordWnd.SetActive(false);
        SetInviteServerData(-1);
    }

    void BtnInputOnClick()
    {
        SendMsg();
    }

    void OnSubmit(string inputString)
    {
        SendMsg();
    }


    GameObject mLastWnd;
    void BtnWorkShopsOnClick()
    {
        if (mWorkShopCtrl == null)
        {
            GameObject workShop = GameObject.Instantiate(mWorkShopPrfab) as GameObject;
            workShop.transform.transform.parent = mUICenter.transform;
            workShop.transform.localPosition = new Vector3(0, 40, -20);
            workShop.transform.localScale = Vector3.one;
            mWorkShopCtrl = workShop.GetComponent<UIWorkShopCtrl>();
            if (mWorkShopCtrl == null)
                return;
            workShop.SetActive(true);
            mWorkShopCtrl.e_BtnClose += WorkShopOnClose;
            if (mRoomWnd.activeSelf)
            {
                mRoomWnd.SetActive(false);
                mLastWnd = mRoomWnd;
            }
            if (mUIServerWnd.activeSelf)
            {
                mUIServerWnd.SetActive(false);
                mLastWnd = mUIServerWnd;
            }
            if (mHostCreateWnd.activeSelf)
            {
                mHostCreateWnd.SetActive(false);
                mLastWnd = mHostCreateWnd;
            }

            if (RoomGui_N.Instance.isShow)
            {
                RoomGui_N.Instance.gameObject.SetActive(false);
                mLastWnd = RoomGui_N.Instance.gameObject;
            }

            if (mMallWnd != null && mMallWnd.isShow)
            {
                mMallWnd.gameObject.SetActive(false);
                mLastWnd = mMallWnd.gameObject;
            }
        }
    }
    void WorkShopOnClose()
    {
        if (mWorkShopCtrl != null) {
#if MAINMENU_ISO
			GameObject.Destroy (mWorkShopCtrl.gameObject);
			mWorkShopCtrl = null;
			PeSceneCtrl.Instance.GotoMainMenuScene();
			return;
#endif
			if (mLastWnd != null && mLastWnd != mWorkShopCtrl.gameObject) {
				mLastWnd.SetActive (true);
			} else {
				mRoomWnd.SetActive (true);
			}
			GameObject.Destroy (mWorkShopCtrl.gameObject);
			mWorkShopCtrl = null;
		}
    }

    void BtnFriendsOnClick()
    {
        if (!PeSteamFriendMgr.Instance.mFriendWnd.isShow)
            PeSteamFriendMgr.Instance.mFriendWnd.Show();
        else
            PeSteamFriendMgr.Instance.mFriendWnd.Hide();
    }

    #region MallWnd
    [SerializeField]
    GameObject mallWndPrefab;
    UIMallWnd mMallWnd = null;

    void BtnMallOnClick()
    {
        if (mMallWnd == null)
        {
            GameObject mallWnd = GameObject.Instantiate(mallWndPrefab) as GameObject;
            mallWnd.transform.transform.parent = mUICenter.transform;
            mallWnd.transform.localPosition = new Vector3(0, 40, -10);
            mallWnd.transform.localScale = Vector3.one;
            mMallWnd = mallWnd.GetComponent<UIMallWnd>();
            mMallWnd.e_OnHide += MallWndOnClose;
        }

        if (mRoomWnd.activeSelf)
        {
            mRoomWnd.SetActive(false);
            mLastWnd = mRoomWnd;
        }
        if (mUIServerWnd.activeSelf)
        {
            mUIServerWnd.SetActive(false);
            mLastWnd = mUIServerWnd;
        }
        if (mHostCreateWnd.activeSelf)
        {
            mHostCreateWnd.SetActive(false);
            mLastWnd = mHostCreateWnd;
        }
        //luwei
        if (mWorkShopCtrl != null)
        {
            GameObject.Destroy(mWorkShopCtrl.gameObject);
            mWorkShopCtrl = null;

            mLastWnd = mRoomWnd;

        }

        if (RoomGui_N.Instance.isShow)
        {
            RoomGui_N.Instance.gameObject.SetActive(false);
            mLastWnd = RoomGui_N.Instance.gameObject;
        }

        mMallWnd.gameObject.SetActive(true);
    }

	void MallWndOnClose(UIBaseWidget widget = null)
    {
        if (mLastWnd != null && mLastWnd != mMallWnd.gameObject)
            mLastWnd.SetActive(true);
        else
            mRoomWnd.SetActive(true);
    }

    #endregion

}









