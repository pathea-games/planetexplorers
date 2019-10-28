using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea;

public class MyPlayer //玩家
{
    //private bool m_IsMyself = false;//是不是玩家自己
    //public bool IsMyself
    //{
    //    get { return m_IsMyself; }
    //    set { m_IsMyself = value; }
    //}

    private bool m_IsRequest = false;//是不是发出加队请求
    public bool IsRequest
    {
        get { return m_IsRequest; }
        set { m_IsRequest = value; }
    }

    private bool m_IsCaptain = false;//玩家是不是队长
    public bool IsCaptain
    {
        get { return m_IsCaptain; }
        set { m_IsCaptain = value; }
    }


    private string m_TeamName = "";//玩家所在队伍的队名
    public string TeamName
    {
        get { return m_TeamName; }
        set { m_TeamName = value; }
    }

    private int m_TeamNumber = 0;//玩家所在队伍的编号
    public int TeamNumber
    {
        get { return m_TeamNumber; }
        set { m_TeamNumber = value; }
    }

    private string m_PlayerName = "";//玩家名字
    public string PlayerName
    {
        get { return m_PlayerName; }
        set { m_PlayerName = value; }
    }

    private int m_Kill = 0;//玩家击杀数
    public int Kill
    {
        get { return m_Kill; }
        set { m_Kill = value; }
    }

    private int m_Death = 0;//玩家死亡数
    public int Death
    {
        get { return m_Death; }
        set { m_Death = value; }
    }

    private int m_Score = 0;//玩家积分
    public int Score
    {
        get { return m_Score; }
        set { m_Score = value; }
    }

}

public class TeamClass
{
    public string _teamName = "";
    public int _totalKill = 0;
    public int _totalDeath = 0;
    public int _totalScore = 0;
}



public enum MyGameType //游戏类型
{
    Cooperation,
    VS,
    Survival
}

public enum MyItemType
{
    One,
    Two,
    Three
}

public class CSUI_TeamInfoMgr : UIBaseWnd
{
    private static CSUI_TeamInfoMgr m_Instance;
    public static CSUI_TeamInfoMgr Intance
    {
        get { return m_Instance; }
    }




    #region  ****************************************************************************

    #region Properties
    [SerializeField]
    GameObject mIntegrationPage;
    [SerializeField]
    GameObject mPlayersPage;
    [SerializeField]
    GameObject mTroopsPage;

    [SerializeField]
    UIGrid mIntegrationGrid;
    [SerializeField]
    UIGrid mPlayersGrid;
    [SerializeField]
    UIGrid mTroopsGrid;

    [SerializeField]
    GameObject mIntegrationPrefab;
    [SerializeField]
    GameObject mPlayersPrefab;
    [SerializeField]
    GameObject mTroopsPrefab;

    [SerializeField]
    GameObject mTroopsBtn;

    [SerializeField]
    N_ImageButton mKickBtn;
    [SerializeField]
    N_ImageButton mReferredBtn;
    [SerializeField]
    N_ImageButton mBreakBtn;
    [SerializeField]
    N_ImageButton mQuitBtn;


    public GameObject mYesSpr;
    public GameObject mNoSpr;


    public UILabel mIntegrationPageCountText;
    public UILabel mPlayersPageCountText;
    public UILabel mTroopsPageCountText;


    public N_ImageButton _creatTeam;
    public N_ImageButton _invitation;
    public N_ImageButton _joinTeam;

    //private int mMaxListCount1 = 10;
    //private int mMaxListCount2 = 11;

    private bool m_FreeJoin = false;//是否自由加入

    //private List<CSUI_TeamListItem> mIntegrationItemCtrls = new List<CSUI_TeamListItem>();

    //private PlayerNetwork _MySelf;//玩家自己

    //public delegate void SendMeDel(MyPlayer mp);
    //public event SendMeDel SendMeEvent;

    //public void SetMyself(MyPlayer me)
    //{
    //    if (me != null)
    //        MySelf = me;
    //}


    #endregion

    public enum MyType
    {
        Integration,
        Players,
        Troops
    }




    private MyType mBtnType;
    public MyType BtnType
    {
        get { return mBtnType; }

        set
        {
            mBtnType = value;

            switch (value)
            {
                case MyType.Integration:
                    InfoSum();
                    if (mTotalTeamList == null)
                        return;
                    int a = mTotalTeamList.Count % IntegrationPerPageCount;
                    int b = mTotalTeamList.Count / IntegrationPerPageCount;
                    if (a == 0)//说明每页都是满的
                        IntegrationPageCount = b;
                    else//最后一页不是满的
                        IntegrationPageCount = b + 1;//求出总页数了
                    CreatIntegration(IntegrationPageIndex);

                    break;
                case MyType.Players:
                    GetAllPlayers();
                    if (AllPlayers == null)
                        return;
                    int c = AllPlayers.Count % PlayersPerPageCount;
                    int d = AllPlayers.Count / PlayersPerPageCount;
                    if (c == 0)//说明每页都是满的
                        PlayersPageCount = d;
                    else//最后一页不是满的
                        PlayersPageCount = d + 1;//求出总页数了
                    CreatPlayers(PlayersPageIndex);

                    break;
                case MyType.Troops:
					RefreshTeamGrid(PlayerNetwork.mainPlayer.TeamId);
                    break;

                default:
                    break;

            }
        }
    }



    private MyGameType mGameType;
    public MyGameType GameType
    {
        get { return mGameType; }

        set
        {
            mGameType = value;
            if (value == MyGameType.Survival)
            {
                _creatTeam.gameObject.SetActive(true);
                _joinTeam.gameObject.SetActive(true);
            }
            else
            {
                _creatTeam.gameObject.SetActive(true);
                _joinTeam.gameObject.SetActive(true);
            }
        }
    }



    private List<TeamInfo> mTotalTeamList = new List<TeamInfo>();//总共的队伍





    //Integration
    private int IntegrationPageIndex = 0;//当前页码
    private int IntegrationPageCount = 0;//总页数
    private int IntegrationPerPageCount = 12;//每页数量

    private List<CSUI_TeamListItem> IntegrationPageList;
    //private List<UICheckbox> IntegrationCheckBox = new List<UICheckbox>();

    //Players
    private int PlayersPageIndex = 0;//当前页码
    private int PlayersPageCount = 0;//总页数
    private int PlayersPerPageCount = 12;//每页数量

    private List<CSUI_TeamListItem> PlayersPageList;
    //private List<UICheckbox> PlayersCheckBox = new List<UICheckbox>();

    //Troops
    private int TroopsPageIndex = 0;//当前页码
    private int TroopsPageCount = 0;//总页数
    private int TroopsPerPageCount = 11;//每页数量

    private List<CSUI_TeamListItem> TroopsPageList;
    //private List<UICheckbox> TroopsCheckBox = new List<UICheckbox>();

    private void InitGrid()
    {
        //integration
        IntegrationPageList = new List<CSUI_TeamListItem>();
        for (int i = 0; i < IntegrationPerPageCount; i++)
        {
            GameObject o = GetNewGameObject(mIntegrationPrefab,mIntegrationGrid.transform);
            CSUI_TeamListItem item = o.GetComponent<CSUI_TeamListItem>();
            item.mType = MyItemType.One;
            item.mIndex = i;
            item.ItemChecked += UICheckItem;

            IntegrationPageList.Add(item);
        }
        mIntegrationGrid.repositionNow = true;

        //players

        PlayersPageList = new List<CSUI_TeamListItem>();
        for (int i = 0; i < PlayersPerPageCount; i++)
        {
            GameObject o = GetNewGameObject(mPlayersPrefab, mPlayersGrid.transform);
            CSUI_TeamListItem item = o.GetComponent<CSUI_TeamListItem>();
            item.mType = MyItemType.Two;
            item.mIndex = i;
            item.ItemChecked += UICheckItem;
            item.ItemCheckedPlayer += UICheckItemPlayer;

            PlayersPageList.Add(item);
        }
        mPlayersGrid.repositionNow = true;

        //Troops
        TroopsPageList = new List<CSUI_TeamListItem>();
        for (int i = 0; i < TroopsPerPageCount; i++)
        {
            GameObject o = GetNewGameObject(mTroopsPrefab, mTroopsGrid.transform);
            CSUI_TeamListItem item = o.GetComponent<CSUI_TeamListItem>();
			item.mType = MyItemType.Three;
			item.mIndex = i;
			item.ItemChecked += UICheckItem;
            item.ItemCheckedPlayer += UICheckItemPlayer;
            TroopsPageList.Add(item);
        }
        mTroopsGrid.repositionNow = true;
    }

    //lz-2016.07.06 实例东西的步骤都是一样的，应该写个Util脚本来通用
    GameObject GetNewGameObject(GameObject prefab,Transform parentTrans)
    {
        GameObject o = GameObject.Instantiate(prefab) as GameObject;
        o.transform.parent = parentTrans;
        //lz-2016.07.06 这里要记得把位置置零,否则会跑歪，或者z影响到层叠状态
        o.transform.localPosition = Vector3.zero;
        o.transform.localScale = Vector3.one;
        o.transform.localRotation = Quaternion.identity;
        return o;
    }



    //private void ClearCheckedIntegration()
    //{
    //    foreach (UICheckbox uc in IntegrationCheckBox)
    //        if (uc.isChecked)
    //            uc.isChecked = false;
    //}
    //private void ClearCheckedPlayers()
    //{
    //    foreach (UICheckbox uc in PlayersCheckBox)
    //        if (uc.isChecked)
    //            uc.isChecked = false;
    //}

    private void CreatIntegration(int pageIndex)//integration
    {
        ClearSelected();
        List<TeamInfo> lis = new List<TeamInfo>();

        if (pageIndex < IntegrationPageCount - 1)//没到最后一页
            lis = mTotalTeamList.GetRange(pageIndex * IntegrationPerPageCount, IntegrationPerPageCount);
        else if (pageIndex == IntegrationPageCount - 1)//到了最后一页
        {
            if (IntegrationPageCount * IntegrationPerPageCount > mTotalTeamList.Count)//最后一页未满
                lis = mTotalTeamList.GetRange(pageIndex * IntegrationPerPageCount, mTotalTeamList.Count % IntegrationPerPageCount);
            else if (IntegrationPageCount * IntegrationPerPageCount == mTotalTeamList.Count)//最后一页满的
                lis = mTotalTeamList.GetRange(pageIndex * IntegrationPerPageCount, IntegrationPerPageCount);
        }

		foreach (CSUI_TeamListItem i in IntegrationPageList)
		{
			i.ClearInfo();
		}
        
        for (int i = 0; i < lis.Count; i++)
        {
            IntegrationPageList[i].SetInfo(lis[i]._group, lis[i]._killCount, lis[i]._deathCount, lis[i]._point);
        }

        mIntegrationPageCountText.text = (IntegrationPageIndex + 1).ToString() + "/" + IntegrationPageCount.ToString();
    }

    private void CreatPlayers(int pageIndex)//players
    {
        ClearSelected();

        //lz-2016.10.23 错误 #5104 crash bug
        if (null == AllPlayers || AllPlayers.Count <= 0)
            return;

        List<PlayerNetwork> lis = new List<PlayerNetwork>();

        if (pageIndex < PlayersPageCount - 1)//没到最后一页
            lis = AllPlayers.GetRange(pageIndex * PlayersPerPageCount, PlayersPerPageCount);
        else if (pageIndex == PlayersPageCount - 1)//到了最后一页
        {
            if (PlayersPageCount * PlayersPerPageCount > AllPlayers.Count)//最后一页未满
                lis = AllPlayers.GetRange(pageIndex * PlayersPerPageCount, AllPlayers.Count % PlayersPerPageCount);
            else if (PlayersPageCount * PlayersPerPageCount == AllPlayers.Count)//最后一页满的
                lis = AllPlayers.GetRange(pageIndex * PlayersPerPageCount, PlayersPerPageCount);
        }
        if (null== lis||lis.Count <= 0)
            return;

        //lz-2016.11.10 Crash Bug 
        if (null == PlayersPageList||PlayersPageList.Count <= 0)
            return;

        foreach (CSUI_TeamListItem i in PlayersPageList)
        {
            if(null!=i)
                i.ClearInfo();
        }
        for (int i = 0; i < lis.Count; i++)
        {
            if (i < PlayersPageList.Count)
            {
                //lz-2016.11.28 错误 #7061 Crash bug
                PlayerNetwork playerNetwork = lis[i];
                if(null!= playerNetwork&& null!=playerNetwork.Battle)
                    PlayersPageList[i].SetInfo(playerNetwork.TeamId, playerNetwork.RoleName, playerNetwork.Battle._killCount, playerNetwork.Battle._deathCount, playerNetwork.Battle._point, playerNetwork);
            }
        }
        mPlayersPageCountText.text = (PlayersPageIndex + 1).ToString() + "/" + PlayersPageCount.ToString();
    }

	private void CreatTroops(int pageIndex, List<PlayerNetwork> _player)//Troops
	{
		ClearSelected();

		if (_player == null)
			return;

		int c = _player.Count % TroopsPerPageCount;
		int d = _player.Count / TroopsPerPageCount;

		if (c == 0)//说明每页都是满的
			TroopsPageCount = d;
		else//最后一页不是满的
			TroopsPageCount = d + 1;//求出总页数了

		List<PlayerNetwork> lis = new List<PlayerNetwork>();

		if (pageIndex < TroopsPageCount - 1)//没到最后一页
		{
			lis = _player.GetRange(pageIndex * TroopsPerPageCount, TroopsPerPageCount);
		}
		else if (pageIndex == TroopsPageCount - 1)//到了最后一页
		{
			if (TroopsPageCount * TroopsPerPageCount > _player.Count)//最后一页未满
				lis = _player.GetRange(pageIndex * TroopsPerPageCount, _player.Count % TroopsPerPageCount);
			else if (TroopsPageCount * TroopsPerPageCount == _player.Count)//最后一页满的
				lis = _player.GetRange(pageIndex * TroopsPerPageCount, TroopsPerPageCount);
		}

		foreach (CSUI_TeamListItem i in TroopsPageList)
		{
			i.ClearInfo();
			i.OnAgreementBtnEvent -= OnAgreeJoin;
		}

		for (int i = 0; i < lis.Count; i++)
		{
			TroopsPageList[i].SetInfo(lis[i].RoleName, lis[i].Battle._killCount, lis[i].Battle._deathCount, lis[i].Battle._point, lis[i]);
			TroopsPageList[i].OnAgreementBtnEvent += OnAgreeJoin;
		}
		mTroopsPageCountText.text = (TroopsPageIndex + 1).ToString() + "/" + TroopsPageCount.ToString();
	}

    //private void CreatPlayers()
    //{
    //    DestroyItem(mPlayersGrid.transform);
    //    CSUI_TeamListItem item;
    //    for (int i = 0; i < mAllPlayers.Count; i++)
    //    {
    //        item = CreatOneItem(mPlayersPrefab, mPlayersGrid.transform);

    //        item.SetInfo(mAllPlayers[i].TeamName, mAllPlayers[i].PlayerName, mAllPlayers[i].Kill, mAllPlayers[i].Death, mAllPlayers[i].Score, mAllPlayers[i].IsCaptain);
    //    }
    //    mPlayersGrid.repositionNow = true;
    //}

    //private CSUI_TeamListItem CreatOneItem(GameObject prefab, Transform parent)//创建一个item
    //{
    //    GameObject o = GameObject.Instantiate(prefab) as GameObject;
    //    o.transform.parent = parent.transform;
    //    o.transform.localRotation = Quaternion.identity;
    //    o.transform.localScale = Vector3.one;
    //    CSUI_TeamListItem item = o.GetComponent<CSUI_TeamListItem>();
    //    item.gameObject.GetComponentInChildren<UICheckbox>().radioButtonRoot = parent;
    //    return item;
    //}

    //private void DestroyItem(Transform tr)
    //{
    //    for (int i = 0; i < tr.childCount; i++)
    //    {
    //        //Destroy(tr.GetChild(i).gameObject);
    //        DestroyImmediate(tr.GetChild(i).gameObject);
    //    }
    //}


    //string m_TeamNameToJoin;//要加入的队伍名字
    private void SetTeamNameToJoin(MyPlayer mp)
    {
        //if (mp.TeamName != "")
            //m_TeamNameToJoin = mp.TeamName;
    }



    #region 提供接口
    public delegate void ReceiveDataDel();

    //public event ReceiveDataDel ReceiveIntegrationEvent;
    //public event ReceiveDataDel ReceivePlayersEvent;

    int _currentIndex = -1; 

    public void JoinTeamList(PlayerNetwork pnet)
    {
        //_MySelf = pnet;
        _currentIndex++;
        //TroopsPageList[_currentIndex].SetInfo(mp.PlayerName, mp.Kill, mp.Death, mp.Score, mp.IsCaptain, mp.IsRequest, mp);
    }

    #endregion


    void Awake()
    {
        m_Instance = this;
    }

    public UISlicedSprite m_CheckBtnBg;
    void Start()
    {
        InitGrid();

        //if (GameType == MyGameType.Survival)
        //    mTroopsBtn.SetActive(true);
        //else
        //    mTroopsBtn.SetActive(false);


        if (PeGameMgr.gameType == PeGameMgr.EGameType.Survive)//如果是生存模式
        {
            mTroopsBtn.SetActive(true);
            m_CheckBtnBg.transform.localScale = new Vector3(356f, 35f, 1f);

        }
        else
        {
            mTroopsBtn.SetActive(false);
            m_CheckBtnBg.transform.localScale = new Vector3(248f, 35f, 1f);
        }

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {

            //mTestDataPlayers();

        }

		if (null == PlayerNetwork.mainPlayer)
			return;

        if (PeGameMgr.gameType == PeGameMgr.EGameType.Survive)//生存模式
        {
            if (!mTroopsBtn.activeSelf)
                mTroopsBtn.SetActive(true);

            if (_creatTeam.gameObject.activeSelf)
                _creatTeam.gameObject.SetActive(false);

			if (PlayerNetwork.mainPlayer.isOriginTeam)
			{
				if (GroupNetwork.IsEmpty(PlayerNetwork.mainPlayer.TeamId))
				{
					if (!_joinTeam.gameObject.activeSelf)
						_joinTeam.gameObject.SetActive(true);
					if (!_invitation.gameObject.activeSelf)
						_invitation.gameObject.SetActive(true);
					if (mKickBtn.gameObject.activeSelf)
						mKickBtn.gameObject.SetActive(false);
					if (mReferredBtn.gameObject.activeSelf)
						mReferredBtn.gameObject.SetActive(false);
					if (mBreakBtn.gameObject.activeSelf)
						mBreakBtn.gameObject.SetActive(false);
					if (mQuitBtn.gameObject.activeSelf)
						mQuitBtn.gameObject.SetActive(false);
				}
				else
				{
					if (_joinTeam.gameObject.activeSelf)
						_joinTeam.gameObject.SetActive(false);
					if (!_invitation.gameObject.activeSelf)
						_invitation.gameObject.SetActive(true);
					if (!mKickBtn.gameObject.activeSelf)
						mKickBtn.gameObject.SetActive(true);
					if (!mReferredBtn.gameObject.activeSelf)
						mReferredBtn.gameObject.SetActive(true);
					if (!mBreakBtn.gameObject.activeSelf)
						mBreakBtn.gameObject.SetActive(true);
					if (mQuitBtn.gameObject.activeSelf)
						mQuitBtn.gameObject.SetActive(false);
				}
			}
			else
			{
				if (_joinTeam.gameObject.activeSelf)
					_joinTeam.gameObject.SetActive(false);
				if (_invitation.gameObject.activeSelf)
					_invitation.gameObject.SetActive(false);
				if (mKickBtn.gameObject.activeSelf)
					mKickBtn.gameObject.SetActive(false);
				if (mReferredBtn.gameObject.activeSelf)
					mReferredBtn.gameObject.SetActive(false);
				if (mBreakBtn.gameObject.activeSelf)
					mBreakBtn.gameObject.SetActive(false);
				if (!mQuitBtn.gameObject.activeSelf)
					mQuitBtn.gameObject.SetActive(true);
			}
		}
        else//其他模式
        {
            if (mTroopsBtn.activeSelf)
                mTroopsBtn.SetActive(false);

            if (_creatTeam.gameObject.activeSelf)
                _creatTeam.gameObject.SetActive(false);
            if (_joinTeam.gameObject.activeSelf)
                _joinTeam.gameObject.SetActive(false);
            if (_invitation.gameObject.activeSelf)
                _invitation.gameObject.SetActive(false);

			if (PlayerNetwork.mainPlayer.TeamId == -1)//没有队伍
			{
				if (mKickBtn.gameObject.activeSelf)
					mKickBtn.gameObject.SetActive(false);
				if (mReferredBtn.gameObject.activeSelf)
					mReferredBtn.gameObject.SetActive(false);
				if (mBreakBtn.gameObject.activeSelf)
					mBreakBtn.gameObject.SetActive(false);
				if (mQuitBtn.gameObject.activeSelf)
					mQuitBtn.gameObject.SetActive(false);

				if (_creatTeam.disable)
					_creatTeam.disable = false;

				if (_joinTeam.disable)
					_joinTeam.disable = false;

				if (!_invitation.disable)
					_invitation.disable = true;
			}
			else if (GroupNetwork.TeamExists(PlayerNetwork.mainPlayer.TeamId))//先判断是否存在
			{
				if (!PlayerNetwork.mainPlayer.isOriginTeam)//有队伍，不是队长
				{
					if (mKickBtn.gameObject.activeSelf)
						mKickBtn.gameObject.SetActive(false);
					if (mReferredBtn.gameObject.activeSelf)
						mReferredBtn.gameObject.SetActive(false);
					if (mBreakBtn.gameObject.activeSelf)
						mBreakBtn.gameObject.SetActive(false);
					if (!mQuitBtn.gameObject.activeSelf)
						mQuitBtn.gameObject.SetActive(true);

					if (!_creatTeam.disable)
						_creatTeam.disable = true;

					if (!_joinTeam.disable)
						_joinTeam.disable = true;

					if (!_invitation.disable)
						_invitation.disable = true;
				}
				else if (GroupNetwork.GetLeaderId(PlayerNetwork.mainPlayer.TeamId) == PlayerNetwork.mainPlayerId)//有队伍是队长
				{
					if (!mKickBtn.gameObject.activeSelf)
						mKickBtn.gameObject.SetActive(true);
					//if (!mKickBtn.disable)
					//    mKickBtn.disable = true;

					if (!mReferredBtn.gameObject.activeSelf)
						mReferredBtn.gameObject.SetActive(true);
					//if (!mReferredBtn.disable)
					//    mReferredBtn.disable = true;

					if (!mBreakBtn.gameObject.activeSelf)
						mBreakBtn.gameObject.SetActive(true);
					//if (!mBreakBtn.disable)
					//    mBreakBtn.disable = true;

					if (!mQuitBtn.gameObject.activeSelf)
						mQuitBtn.gameObject.SetActive(true);
					//if (!mQuitBtn.disable)
					//    mQuitBtn.disable = true;

					if (!_creatTeam.disable)
						_creatTeam.disable = true;

					if (!_joinTeam.disable)
						_joinTeam.disable = true;

					if (_invitation.disable)
						_invitation.disable = false;
				}
			}
		}
    }

    #region 按钮调用
    private void PageIntegrationOnActive(bool active)
    {
        if (active)
        {
            Debug.Log("点击了综合按钮");
            mIntegrationPage.SetActive(true);
            mPlayersPage.SetActive(false);
            mTroopsPage.SetActive(false);
            BtnType = MyType.Integration;
        }
    }
    private void PagePlayersOnActive(bool active)
    {
        if (active)
        {
            Debug.Log("点击了玩家按钮");
            mIntegrationPage.SetActive(false);
            mPlayersPage.SetActive(true);
            mTroopsPage.SetActive(false);
            BtnType = MyType.Players;
        }
    }
    private void PageTroopsOnActive(bool active)
    {
        if (active)
        {
            Debug.Log("点击了队伍按钮");
            mIntegrationPage.SetActive(false);
            mPlayersPage.SetActive(false);
            mTroopsPage.SetActive(true);
            BtnType = MyType.Troops;
        }
    }

    //integration
    private void OnRightBtn()
    {
        if (IntegrationPageIndex < IntegrationPageCount - 1)
        {
            IntegrationPageIndex += 1;
            CreatIntegration(IntegrationPageIndex);

        }

    }
    private void OnRightBtnEnd()
    {
        if (IntegrationPageIndex < IntegrationPageCount - 1)
        {
            IntegrationPageIndex = IntegrationPageCount - 1;
            CreatIntegration(IntegrationPageIndex);

        }

    }

    private void OnLeftBtn()
    {
        if (IntegrationPageIndex > 0)
        {
            IntegrationPageIndex -= 1;
            CreatIntegration(IntegrationPageIndex);

        }

    }
    private void OnLeftBtnEnd()
    {
        if (IntegrationPageIndex > 0)
        {
            IntegrationPageIndex = 0;
            CreatIntegration(IntegrationPageIndex);

        }
    }

    //players
    private void OnRightBtnPlayers()
    {
        if (PlayersPageIndex < PlayersPageCount - 1)
        {
            PlayersPageIndex += 1;
            CreatPlayers(PlayersPageIndex);

        }

    }
    private void OnRightBtnEndPlayers()
    {
        if (PlayersPageIndex < PlayersPageCount - 1)
        {
            PlayersPageIndex = PlayersPageCount - 1;
            CreatPlayers(PlayersPageIndex);

        }

    }

    private void OnLeftBtnPlayers()
    {
        if (PlayersPageIndex > 0)
        {
            PlayersPageIndex -= 1;
            CreatPlayers(PlayersPageIndex);

        }

    }
    private void OnLeftBtnEndPlayers()
    {
        if (PlayersPageIndex > 0)
        {
            PlayersPageIndex = 0;
            CreatPlayers(PlayersPageIndex);

        }
    }

    //troops
    private void OnRightBtnTroops()
    {
        if (TroopsPageIndex < TroopsPageCount - 1)
        {
            TroopsPageIndex += 1;
            CreatTroops(TroopsPageIndex, _MemberLis);

        }

    }
    private void OnRightBtnEndTroops()
    {
        if (TroopsPageIndex < TroopsPageCount - 1)
        {
            TroopsPageIndex = TroopsPageCount - 1;
            CreatTroops(TroopsPageIndex, _MemberLis);

        }

    }

    private void OnLeftBtnTroops()
    {
        if (TroopsPageIndex > 0)
        {
            TroopsPageIndex -= 1;
            CreatTroops(TroopsPageIndex, _MemberLis);

        }

    }
    private void OnLeftBtnEndTroops()
    {
        if (TroopsPageIndex > 0)
        {
            TroopsPageIndex = 0;
            CreatTroops(TroopsPageIndex, _MemberLis);

        }
    }

    //function Btns
    private void OnSetFreeJoinBtn()
    {
        if (!m_FreeJoin)
        {
            mYesSpr.SetActive(true);
            mNoSpr.SetActive(false);
            m_FreeJoin = true;
        }
        else
        {
            mYesSpr.SetActive(false);
            mNoSpr.SetActive(true);
            m_FreeJoin = false;
        }
    }
    private void OnCreatTeamBtn()//创建队伍
    {
        if (CreatTeamEvent != null)
            CreatTeamEvent();
    }

    private void OnJoinTeamBtn()//加入队伍
    {
        if (JoinTeamEvent != null && _currentChosedPlayer != null)
            JoinTeamEvent(_currentChosedPlayer.TeamId, m_FreeJoin);
    }

    private void OnInvitationBtn()//邀请加入
    {
        if (OnInvitationEvent != null && _currentChosedPlayer != null)
            OnInvitationEvent(_currentChosedPlayer.Id);
    }

    private void OnKickTeamBtn()//踢出玩家
    {
        if (KickTeamEvent != null)
        {
            KickTeamEvent(_currentChosedPlayer);
        }
    }

    private void OnDeliverToBtn()//转交队长
    {
        if (OnDeliverToEvent != null && null != _currentChosedPlayer)
            OnDeliverToEvent(_currentChosedPlayer.Id);
    }

    private void OnDissolveBtn()//解散队伍
    {
        if (OnDissolveEvent != null)
            OnDissolveEvent();
    }

    private void OnQuitTeamBtn()//退出队伍
    {
		//if (GroupNetwork.ExistTeamId(PlayerNetwork.MainPlayer.TeamId))
		//{
		//    if (PlayerNetwork.MainPlayerID == GroupNetwork.GetLeaderId(PlayerNetwork.MainPlayer.TeamId))//队长退出
		//    {
		//        if (OnLeaderQuitTeamEvent != null)
		//            OnLeaderQuitTeamEvent(PlayerNetwork.MainPlayer.TeamId);
		//    }
		//}
		//else//队员退出
		//{
		//    if (OnMemberQuitTeamEvent != null)
		//        OnMemberQuitTeamEvent(PlayerNetwork.MainPlayer.TeamId);
		//}

		if (OnMemberQuitTeamEvent != null)
			OnMemberQuitTeamEvent();

	}

    //***********************************************************************************************





    #endregion

    #endregion
    class TeamInfo
    {
        public int _group;
        public int _killCount;
        public int _deathCount;
        public float _point;
    }


    private List<PlayerNetwork> m_AllPlayers;
    public List<PlayerNetwork> AllPlayers
    {
        get { return m_AllPlayers; }
        set { m_AllPlayers = value; }
    }

    private void GetAllPlayers()//得到所有玩家
    {
        AllPlayers = PlayerNetwork.Get<PlayerNetwork>();
		AllPlayers.Sort((x, y) =>
		{
			if (-1 == x.TeamId)
				return 1;

			if (x.TeamId <= y.TeamId)
				return -1;

			return 0;
		});
        Debug.Log(AllPlayers.Count);
    }

    private void InfoSum()
    {
		TeamData[] teams = GroupNetwork.GetTeamInfos();

        mTotalTeamList.Clear();

        if (null == teams || teams.Length <= 0) return;
        
        //生成队的对象，并赋数据
        for (int i = 0; i < teams.Length; i++)
        {
            if (teams[i].Members.Count == 0)
                continue;

            TeamInfo tc = new TeamInfo();
            tc._group = teams[i].TeamId;
            foreach (PlayerNetwork pnet in teams[i].Members)
            {
                //lz-2016.11.28 错误 #6982 Crash bug
                if (null != pnet && null != pnet.Battle)
                {
                    tc._killCount += pnet.Battle._killCount;
                    tc._deathCount += pnet.Battle._deathCount;
                    tc._point += pnet.Battle._point;
                }
			}
            mTotalTeamList.Add(tc);
        }
    }

    private void ClearSelected()
    {
        ClearChosedPlayer();
        for (int i = 0; i < IntegrationPageList.Count; i++)
            IntegrationPageList[i].SetSelected(false);
        for (int i = 0; i < PlayersPageList.Count; i++)
            PlayersPageList[i].SetSelected(false);
        for (int i = 0; i < TroopsPageList.Count; i++)
            TroopsPageList[i].SetSelected(false);
    }

    private void UICheckItem(int _index, MyItemType _type)
    {
        ClearSelected();
        switch (_type)
        {
            case MyItemType.One:
                IntegrationPageList[_index].SetSelected(true);
                break;
            case MyItemType.Two:
                PlayersPageList[_index].SetSelected(true);
                break;
            case MyItemType.Three:
                TroopsPageList[_index].SetSelected(true);
                break;
        }
    }


    private PlayerNetwork _currentChosedPlayer = null;//当前被选中的玩家

    private void UICheckItemPlayer(PlayerNetwork pnet)//这个方法传过来一个PlayerNetwork，通过拿到他的阵营，判断加入哪只队伍
    {
        if (pnet != null)
            _currentChosedPlayer = pnet;
    }
    private void ClearChosedPlayer()
    {
        _currentChosedPlayer = null;
    }


    


	#region  Event
	private List<PlayerNetwork> _MemberLis = new List<PlayerNetwork>();

	public void RefreshTeamGrid(int _teamId)//刷新队伍列表
	{
		if (!isActiveAndEnabled || _teamId != PlayerNetwork.mainPlayer.TeamId)
			return;

		_MemberLis.Clear();

		TeamData td = GroupNetwork.GetTeamInfo(_teamId);
		if (null != td)
		{
			foreach (PlayerNetwork pnet in td.Members)
				_MemberLis.Add(pnet);
		}

		GroupNetwork.GetJoinRequest(_MemberLis);
		
		CreatTroops(TroopsPageIndex, _MemberLis);
	}

	public delegate void CreatTeamDel();//创建队伍
    public static event CreatTeamDel CreatTeamEvent;

	public void OnCreatTeam(int teamId)
	{
		if (PlayerNetwork.mainPlayer.TeamId == teamId)
			RefreshTeamGrid(teamId);

		GetAllPlayers();
		CreatPlayers(PlayersPageIndex);
	}

    public delegate void OnInvitationDel(int id);//邀请加入队伍
    public static event OnInvitationDel OnInvitationEvent;

    public void Invitation(PlayerNetwork _inviter)
    {
        if (_inviter == null)
            return;
        mInviter = _inviter;
        string _content =string.Format(PELocalization.GetString(8000504),  _inviter.RoleName);
        MessageBox_N.ShowYNBox(_content, OnYes);
    }

    PlayerNetwork mInviter = null;
    private void OnYes()
    {
        AcceptJoinTeamEvent(mInviter.Id, mInviter.TeamId);
        //RefreshTeamGrid(mInviter.TeamId);
    }

    public delegate void AcceptJoinTeamDel(int inviterId, int teamId);//接受邀请加入队伍
    public static event AcceptJoinTeamDel AcceptJoinTeamEvent; //这个事件只是把接受邀请的人加入到队伍列表里

	public void ApprovalJoin(int teamId)
	{
		if (-1 != teamId && teamId == PlayerNetwork.mainPlayer.TeamId)
			RefreshTeamGrid(teamId);
	}

    public delegate void JoinTeamDel(int _teamId, bool _freejoin);//申请加入队伍
    public static event JoinTeamDel JoinTeamEvent;

    public void JoinApply(int teamId)//传过来的是队长的mainPlayerID,以及申请的队的teamId
    {
        if (-1 != teamId && PlayerNetwork.mainPlayerId == GroupNetwork.GetLeaderId(teamId))
            RefreshTeamGrid(teamId);
    }

    public delegate void KickTeamDel(PlayerNetwork _kicked);//踢出队员
    public static event KickTeamDel KickTeamEvent;

    public void KickPlayer(int teamId)
    {
		if (-1 != teamId && PlayerNetwork.mainPlayer.TeamId == teamId)
		{
			RefreshTeamGrid(teamId);
		}
		else
		{
			if (-1 == PlayerNetwork.mainPlayer.TeamId)
				ClearTroops();
		}
    }

    private void ClearTroops()
    {
        foreach (CSUI_TeamListItem i in TroopsPageList)
        {
            i.ClearInfo();
            i.OnAgreementBtnEvent -= OnAgreeJoin;
        }
    }

    public delegate void OnAgreeJoinDel(bool _isAgree, PlayerNetwork _pnet);//同意和拒绝
    public static event OnAgreeJoinDel OnAgreeJoinEvent;
    private void OnAgreeJoin(bool _isAgree, PlayerNetwork _pnet)
    {
        OnAgreeJoinEvent(_isAgree, _pnet);
	}

    public delegate void OnDeliverToDel(int id);//转交队长
    public static event OnDeliverToDel OnDeliverToEvent;

	public void LeaderDeliver(int teamId)
	{
		if (-1 != teamId && PlayerNetwork.mainPlayer.TeamId == teamId)
			RefreshTeamGrid(teamId);
	}

	public delegate void OnDissolveToDel();//解散队伍
    public static event OnDissolveToDel OnDissolveEvent;

	public void DissolveTeam(int teamId)
	{
		if (-1 != teamId && teamId == PlayerNetwork.mainPlayer.TeamId)
			RefreshTeamGrid(teamId);
	}

	public delegate void OnQuitTeamDel();//退出队伍
    //public static event OnQuitTeamDel OnLeaderQuitTeamEvent;
    public static event OnQuitTeamDel OnMemberQuitTeamEvent;

	public void QuitTeam(int teamId)
	{
		if (-1 != teamId && PlayerNetwork.mainPlayer.TeamId == teamId)
		{
			RefreshTeamGrid(teamId);
		}
		else
		{
			if (-1 == PlayerNetwork.mainPlayer.TeamId)
				ClearTroops();
		}
	}
    #endregion
}
