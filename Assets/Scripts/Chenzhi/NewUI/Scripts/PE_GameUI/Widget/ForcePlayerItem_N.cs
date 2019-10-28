using UnityEngine;
using System.Collections;
using CustomData;
using System;

//log:lz-2016.05.06 create it
public class ForcePlayerItem_N : MonoBehaviour
{
    public UILabel RoleNameLabel;
	public UILabel 	PlayerNameLabel;
	public UILabel 	DelayLabel;
	public UILabel	StateLabel;
    public UILabel CanJoinCountLabel;
	public UIButton ExitBtn;
    public UISprite RoomMasterMarkSprite;
    public UISprite SelecteSprite;
    public Action<int> ChangePlayerForceEvent;    
    public Action<int> KickPlayerEvent;

	private RoomPlayerInfo m_PlayerInfo;
    private PlayerDesc m_RoleInfo;
    private bool m_IsRoomMaster;

    #region mono methods

    void OnEnable()
    {
        UIEventListener.Get(this.ExitBtn.gameObject).onClick += this.OnKickPlayerClick;
        UIEventListener.Get(this.gameObject).onClick += this.OnJoinClick;
    }

    void OnDisable()
    {
        UIEventListener.Get(this.ExitBtn.gameObject).onClick -= this.OnKickPlayerClick;
        UIEventListener.Get(this.gameObject).onClick -= this.OnJoinClick;
    }
    #endregion

    #region public methods

    public int GetPlayerID()
    {
        if (null == this.m_PlayerInfo)
            return -1;
        return this.m_PlayerInfo.mId;
    }

    public int GetRoleID()
    {
        if (null == this.m_RoleInfo)
            return -1;
        return this.m_RoleInfo.ID;
    }

    public void SetDelay(int delay)
	{
        this.UpdateDelay(delay);
	}
	
	public void SetState(int state)
	{
        this.UpdateState(state);
	}

    public void SetPlayer(RoomPlayerInfo playerinfo, bool isRoomMaster)    //curIsMasterUI can operation all player
	{
        this.ResetPlayer();
		this.m_PlayerInfo = playerinfo;
        this.m_IsRoomMaster = isRoomMaster;
        if (null != this.m_PlayerInfo)
        {
            this.UpdatePlayerName(this.m_PlayerInfo.mPlayerInfo.mName);
            this.UpdateDelay(this.m_PlayerInfo.mDelay);
            this.UpdateState(this.m_PlayerInfo.mState);
            this.UpdateRoomMasterMark(this.m_PlayerInfo.mRoomMaster);
            this.UpdateExitBtnState(isRoomMaster && this.m_PlayerInfo.mId != BaseNetwork.MainPlayer.Id);
            this.UpdateSelect(playerinfo.mId == BaseNetwork.MainPlayer.Id);
        }
    }

    public void SetFixRole(PlayerDesc roleInfo)
    {
        this.ResetAll();
        this.m_RoleInfo = roleInfo;
        this.UpdateRoleName(this.m_RoleInfo==null?"":this.m_RoleInfo.Name);
    }

    public void ResetAll()
    {
        this.RoleNameLabel.text = "";
        this.m_RoleInfo = null;
        this.ResetPlayer();
    }

    public void ResetPlayer()
    {
        this.PlayerNameLabel.text = "";
        this.DelayLabel.text = "";
        this.StateLabel.text = "";
        this.UpdateExitBtnState(false);
        this.RoomMasterMarkSprite.enabled = false;
        this.m_PlayerInfo = null;
        this.CanJoinCountLabel.text = "";
        this.CanJoinCountLabel.gameObject.SetActive(false);
        this.UpdateSelect(false);
    }

    public void UpdateCanJoinCount(int currentCount, int totalCount)
    {
        if (!this.CanJoinCountLabel.gameObject.activeInHierarchy)
        {
            this.CanJoinCountLabel.gameObject.SetActive(true);
        }
        this.CanJoinCountLabel.text = String.Format(PELocalization.GetString(8000184),(totalCount-currentCount).ToString());
    }

    #endregion

    #region private methods

    private void UpdateRoleName(string roleName)
    {
        if (null != roleName)
            this.RoleNameLabel.text = roleName;
    }

    private void UpdatePlayerName(string playerName)
    {
        if (null != playerName)
            this.PlayerNameLabel.text = playerName;
    }

    private void UpdateDelay(int delay)
    {
        Color delayCor=Color.white;
        if (delay < 151)
            delayCor = new Color(10f / 255f, 205f / 255f, 42f / 255f);
        else if (delay < 251)
            delayCor = new Color(1f, 219 / 255f, 108f / 255f);
        else
            delayCor = Color.red;
        this.DelayLabel.color = delayCor;
        this.DelayLabel.text = delay.ToString();
    }

    private void UpdateState(int state)
    {
        if (state == (int)ENetworkState.Gameing)
            this.StateLabel.text = "Gaming";
        else if (state == (int)ENetworkState.Loading)
            this.StateLabel.text = "Loading";
        else if (state == (int)ENetworkState.Null)
            this.StateLabel.text = "Prepare";
        else
            this.StateLabel.text = "Ready";
    }

    private void UpdateExitBtnState(bool isEnable)
    {
            this.ExitBtn.enabled = isEnable;
            this.ExitBtn.gameObject.SetActive(isEnable);
    }

    private void UpdateRoomMasterMark(bool isRoomMaster)
    {
        this.RoomMasterMarkSprite.enabled = isRoomMaster;
    }

    private void UpdateSelect(bool isSelecte)
    {
        this.SelecteSprite.gameObject.SetActive(isSelecte);
    }

    private  void OnKickPlayerClick(GameObject go)
	{
		if(Input.GetMouseButtonUp(0))
		{
            if (this.KickPlayerEvent != null && this.m_PlayerInfo!=null)
            {
                if (this.m_IsRoomMaster && this.m_PlayerInfo.mId != BaseNetwork.MainPlayer.Id)
                {
                    this.KickPlayerEvent(this.m_PlayerInfo.mId);
                }
            }
		}
	}

    private void OnJoinClick(GameObject go)
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (this.ChangePlayerForceEvent != null && this.m_PlayerInfo == null)
            {
                this.ChangePlayerForceEvent(this.GetRoleID());
            }
        }
    }
    #endregion
}
