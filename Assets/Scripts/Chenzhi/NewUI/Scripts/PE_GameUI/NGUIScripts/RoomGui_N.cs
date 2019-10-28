using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CustomData;
using System.Linq;
using System;


public struct IsoDownLoadInfo
{
	public int mNeedCount;
	public int mLoadCount;
	public int mLoadSpeed;  //(kb/s)
}

public class RoomGui_N : UIStaticWnd
{
    public static RoomGui_N Instance { get { return m_Instance; } }
   
    public GameObject ForcePrefab;
    public GameObject ForceInfoGo;
    public GameObject ForceChatGo;
    public UITable ForceTable;
    public UILabel SartBtnText;
    public UIButton StartBtn;
    public UISlicedSprite StarBtnBg;

    private static RoomGui_N m_Instance;
    private RoomInfo_N m_RoomInfoComp;
    private RoomChat_N m_RoomChatComp;
    private List<ForceItem_N> m_ForceItemList;
    private bool m_IsRoomMaster = false;
    private Dictionary<int,Dictionary<int,int>> m_ForceDic; // ForceID<PlayerID,RoleID>
	//private List<RoomPlayerInfo> 		m_RoomPlayerList;
    private IsoDownLoadInfo m_IsoDownloadInfo;
	private bool	m_IsReady;
    private bool m_Repositioning;
    private bool m_InitAllForce;


    #region mono methods

    void Awake()
	{
		m_Instance = this;
        //this.m_RoomPlayerList = new List<RoomPlayerInfo>();
        this.m_IsoDownloadInfo = new IsoDownLoadInfo();
        this.m_ForceDic = new Dictionary<int, Dictionary<int, int>>();
        this.m_ForceItemList = new List<ForceItem_N>();
        this.m_RoomInfoComp = this.ForceInfoGo.GetComponent<RoomInfo_N>();
        this.m_RoomChatComp = this.ForceChatGo.GetComponent<RoomChat_N>();
        this.m_Repositioning = false;
        this.m_InitAllForce = false;
	}
    #endregion

    #region private methods
    private void RefreshIsoProcess()
	{
        this.m_RoomInfoComp.UpdateIsoSpeed(this.m_IsoDownloadInfo.mLoadSpeed.ToString() + " kb/s");
        this.m_RoomInfoComp.UpdateIsoCount(this.m_IsoDownloadInfo.mLoadCount.ToString() + "/" + m_IsoDownloadInfo.mNeedCount.ToString());
        if (this.m_IsoDownloadInfo.mNeedCount != 0)
        {
            this.m_RoomInfoComp.UpdateIsoProcess(Convert.ToSingle(m_IsoDownloadInfo.mLoadCount) / Convert.ToSingle(m_IsoDownloadInfo.mNeedCount));
            //lz-2016.06.28 如果iso下载完了，并且是准备状态就直接进入游戏
            if (this.m_IsoDownloadInfo.mLoadCount == this.m_IsoDownloadInfo.mNeedCount)
            {
                this.StartGame();
            }
        }
	}

    private void EnableBtnStar(bool value)
	{
		StartBtn.isEnabled = value;
		BoxCollider bc = StartBtn.gameObject.GetComponent<BoxCollider>();
		if(bc != null)
			bc.enabled = value;

		if(value == true)
			StarBtnBg.spriteName = "but_start_on";
		else
			StarBtnBg.spriteName = "but_start_off";
	}

    private void OnStart()
    {
        if (!m_IsReady)
        {
            m_IsReady = true;
            SartBtnText.text = PELocalization.GetString(8000540);
            this.StartGame();
        }
        else
        {
            m_IsReady = false;
            SartBtnText.text = PELocalization.GetString(8000378);
        }
    }

    private void StartGame()
    {
		if (this.m_IsReady && BaseNetwork.MainPlayer != null)
        {
            BaseNetwork.MainPlayer.RequestChangeStatus(ENetworkState.Ready);
        }
    }

    private void OnBack()
    {
        MessageBox_N.ShowYNBox(PELocalization.GetString(8000075), PeSceneCtrl.Instance.GotoLobbyScene);
    }

    private void InitAllForce()
    {
        List<ForceDesc> roomForceList = ForceSetting.Instance.HumanForce;
        List<PlayerDesc> roomRoleList = ForceSetting.Instance.HumanPlayer;
        for (int i = 0; i < roomForceList.Count(); i++)
        {
            GameObject forceItemGo = GameObject.Instantiate(this.ForcePrefab);
            forceItemGo.transform.parent = this.ForceTable.transform;
            forceItemGo.transform.localPosition = Vector3.zero;
            forceItemGo.transform.localScale = Vector3.one;
            forceItemGo.gameObject.name = "ForceItem"+i.ToString("D4");
            ForceItem_N forceItem = forceItemGo.GetComponent<ForceItem_N>();
            forceItem.RepositionEvent =()=> this.RepostionForceTable();
            forceItem.ChangePlayerForceEvent = this.ChangePlayerTeamToNet;
            forceItem.KickPlayerEvent = this.KickPlayerToNet;
            List<PlayerDesc> roleListForForce = roomRoleList.Where(a => a.Force == roomForceList[i].ID).ToList();
            forceItem.SetForceInfo(roomForceList[i], roleListForForce);
            this.m_ForceItemList.Add(forceItem);
            this.m_ForceDic.Add(roomForceList[i].ID,new Dictionary<int,int>());
        }
        this.RepostionForceTable();
    }

    private void RepostionForceTable()
    {
        if (this.m_Repositioning)
        {
            this.StopCoroutine("RepositionForceIterator");
            this.m_Repositioning = false;
        }
        this.StartCoroutine("RepositionForceIterator");
    }

    private IEnumerator RepositionForceIterator()
    {
        if (!this.m_Repositioning)
        {
            this.m_Repositioning = true;
            yield return null;
            this.ForceTable.Reposition();
            this.m_Repositioning = false;
        }
    }

    private void TryRemovePlayer(int playerID)
    {
            ForceItem_N forceItem=this.FindForceItemByPlayerID(playerID);
            if (forceItem!=null)
            {
                this.m_ForceDic[forceItem.GetForceID()].Remove(playerID);
                forceItem.ExitForceByNet(playerID);
            }
    }

    private ForceItem_N FindForceItemByPlayerID(int playerID)
    {
        if (this.m_ForceDic != null && this.m_ForceDic.Count() > 0)
        {
            int levelForceID = -1;
            foreach (KeyValuePair<int, Dictionary<int,int>> kv in this.m_ForceDic)
            {
                if (kv.Value.ContainsKey(playerID))
                {
                    levelForceID = kv.Key;
                    break;
                }
            }

            return this.m_ForceItemList.FirstOrDefault(item => item.GetForceID() == levelForceID);
        }
        return null;
    }

    //private bool IsPlayerLiveInForce(int playerID, int forceID)
    //{
    //    if (this.m_ForceDic.ContainsKey(forceID))
    //    {
    //        if (this.m_ForceDic[forceID].Contains(playerID))
    //        {
    //            return true;
    //        }
    //    }
    //    return false;
    //}

    #endregion

    #region public methods
    public override void Show ()
	{
		base.Show ();
		this.m_IsReady= false;
        this.m_RoomInfoComp.UpdateInfo();
        this.m_RoomChatComp.SendMsgEvent = this.SyncRoomMsgToNet;
	}

    #region Network Response

    public static void UpdateDownLoadInfo(int leftLoad, int speed)
	{
		if (null != Instance)
		{
			int curIndex = Instance.m_IsoDownloadInfo.mNeedCount - leftLoad;
			curIndex = Mathf.Clamp(curIndex, 0, Instance.m_IsoDownloadInfo.mNeedCount);
			Instance.m_IsoDownloadInfo.mLoadCount = curIndex;
			Instance.m_IsoDownloadInfo.mLoadSpeed = speed;

			if (Instance.isShow)
				Instance.RefreshIsoProcess();
		}
	}

	public static void SetDownLoadInfo(int totalLoad)
	{
		if (null != Instance)
		{
			Instance.m_IsoDownloadInfo.mNeedCount = Mathf.Max(0, totalLoad);
			if (Instance.isShow)
				Instance.RefreshIsoProcess();
		}
	}

    public static void SetMapInfo(string info)
    {
        if (null != Instance)
        {
            Instance.m_RoomInfoComp.UpdateMapInfo(info);
        }
    }

    public static void InitRoomForceByNet()
    {
        if (null == Instance)
            return;
        if (Instance.m_InitAllForce == false)
        {
            Instance.InitAllForce();
            Instance.m_InitAllForce = true;
        }
    }

    public static void InitRoomPlayerByNet(RoomPlayerInfo playerInfo)
    {
        if (null == Instance)
            return;
        InitRoomForceByNet();
        Instance.m_IsRoomMaster = GameClientNetwork.MasterId == BaseNetwork.MainPlayer.Id;
        if (playerInfo.mId != -1/* && playerInfo.mFocreID != -1*/)
        {
            if (Instance.m_ForceDic.ContainsKey(playerInfo.mFocreID))
            {
				var playerDic = Instance.m_ForceDic[playerInfo.mFocreID];
				playerDic[playerInfo.mId] = playerInfo.mRoleID;
                ChangeRoomPlayerByNet(playerInfo);
            }
            else
            {
                Debug.Log("Find Not contains ForceID: " + playerInfo.mFocreID + "  PlayerID: " + playerInfo.mId + "  RoleID: " + playerInfo.mRoleID);
            }
        }
    }

    //log:lz-2016.05.09  1.玩家存在于某个的阵营就是改变阵营，先从原来的阵营移除，然后加入新的阵营，  2.否者 就直接加入
    public static void ChangeRoomPlayerByNet(RoomPlayerInfo playerInfo)
	{
        if (null == Instance)
            return;
        Instance.TryRemovePlayer(playerInfo.mId);
        if (Instance.m_ForceDic != null && Instance.m_ForceDic.Count() > 0)
        {
            ForceItem_N newForceItem = Instance.m_ForceItemList.FirstOrDefault(item => item.GetForceID() == playerInfo.mFocreID);
            if (null != newForceItem)
            {
                newForceItem.JoinForceByNet(playerInfo, Instance.m_IsRoomMaster);
                Instance.m_ForceDic[playerInfo.mFocreID].Add(playerInfo.mId, playerInfo.mRoleID);
                Instance.ActiveStartBtn();
            }
        }
     }

    public static void RemoveRoomPlayerByNet(int playerId)
	{
        if (null == Instance)
            return;
        Instance.TryRemovePlayer(playerId);
	}

    public static void ChangePlayerDelayByNet(int playerID, int delay)
	{
        if (null == Instance)
            return;
        ForceItem_N forceItem = Instance.FindForceItemByPlayerID(playerID);
        if (forceItem != null)
        {
            forceItem.ChangePlayerDelay(playerID, delay);
        }
	}

    public static void ChangePlayerStateByNet(int playerID, int state)
	{
        if (null == Instance)
            return;
        ForceItem_N forceItem = Instance.FindForceItemByPlayerID(playerID);
        if (forceItem != null)
        {
            forceItem.ChangePlayerState(playerID, state);
        }
	}

    public static void GetNewMsgByNet(string playerName,string msg)
    {
        if (null == Instance)
            return;

        string col = "";
        if (BaseNetwork.MainPlayer.RoleName == playerName)
            col = "99C68B";
        else
            col = "EDB1A6";
        Instance.m_RoomChatComp.AddMsg(playerName, msg, col);
    }

    public static void KickPlayerByNet(int playerInstanceId)
    {
        if (null == Instance)
            return;
        Instance.TryRemovePlayer(playerInstanceId);
        if (BaseNetwork.MainPlayer.Id == playerInstanceId)
        {
            MessageBox_N.ShowOkBox(PELocalization.GetString(8000183), PeSceneCtrl.Instance.GotoLobbyScene);
        }
    }
    #endregion

    #region Network Request

	public void KickPlayerToNet(int playerInstanceId)
	{
        //log-2016.11.21 Crush bug
        if (null!=BaseNetwork.MainPlayer)
            BaseNetwork.MainPlayer.KickPlayer(playerInstanceId);
    }

    public void SyncRoomMsgToNet(string msg)
    {
        //log-2016.11.21 Crush bug
        if (null != BaseNetwork.MainPlayer)
            BaseNetwork.MainPlayer.SendMsg(msg);
    }

    public void ChangePlayerTeamToNet(int forceId,int roleId)
	{
        if (this.m_ForceDic.ContainsKey(forceId))
        {
            int playerID=BaseNetwork.MainPlayer.Id;
            if (this.m_ForceDic[forceId].ContainsKey(playerID))
            {
                if (this.m_ForceDic[forceId][playerID] != roleId)
                {
                    //在同一个阵营换角色
                    BaseNetwork.MainPlayer.RequestChangeTeam(forceId, roleId);
                }
                return;
            }
            else
            {
                //换新阵营
                BaseNetwork.MainPlayer.RequestChangeTeam(forceId, roleId);
            }
        }
	}
    #endregion

    public void ActiveStartBtn()
	{
        if (Pathea.PeGameMgr.IsCustom)
        {
            this.m_ForceItemList.ForEach(a=>
            {
                if (!a.CheckFixRoleIsFull())
                {
                    this.EnableBtnStar(false);
                    return;
                }
            });
            this.EnableBtnStar(true);
        }
        else
        {
            this.EnableBtnStar(true);
        }
	}

    #endregion
}
