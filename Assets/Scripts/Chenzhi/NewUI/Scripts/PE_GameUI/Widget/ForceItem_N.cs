using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//log:lz-2016.05.06 create it
public class ForceItem_N:MonoBehaviour
{
    public UILabel ForceNameLabel;
    public UISprite ForceColorSprite;
    public UILabel ForceCountLabel;
    public UIGrid ForceGrid;
    public GameObject ForcePlayerItemPrefab;
    public Action RepositionEvent;
    public Action<int,int> ChangePlayerForceEvent;
    public Action<int> KickPlayerEvent;

    private ForceDesc m_ForceDesc;
    private List<ForcePlayerItem_N> m_CurForcePlayerItems;
    private Queue<ForcePlayerItem_N> m_ForcePlayerItemPool;
    private bool m_IsRoomMaster;
    private int m_FixRoleCount;

    #region mono methods

    void Awake()
    {
        this.m_CurForcePlayerItems = new List<ForcePlayerItem_N>();
        this.m_ForcePlayerItemPool = new Queue<ForcePlayerItem_N>();
        this.m_FixRoleCount = 0;
    }

    #endregion

    #region public methods

    public void SetForceInfo(ForceDesc forceInfo, List<PlayerDesc> roleInfos)
    {
        this.m_ForceDesc=forceInfo;
        this.InitRoleInfo(roleInfos);
        this.UpdateForceName(this.m_ForceDesc.Name);
        this.UpdateForceColor(this.m_ForceDesc.Color);
        this.UpdateForcePlayerCount();
    }

    //server backcall
    public void JoinForceByNet(RoomPlayerInfo playerInfo,bool isRoomMaster)
    {
        this.m_IsRoomMaster = isRoomMaster;
        this.AddForcePlayerItem(playerInfo);
    }

    //server backcall
    public void ExitForceByNet(int playerId)
    {
        this.RemoveForcePlayerItem(playerId);
    }

    public int GetForceID()
    {
        if (null == this.m_ForceDesc)
        {
            return -1;
        }
        return this.m_ForceDesc.ID;
    }

    public void ChangePlayerDelay(int playerID, int delay)
	{
        ForcePlayerItem_N forcePlayerItem = this.GetForcePlayerItemByPlayerID(playerID);
        if (forcePlayerItem != null)
        {
            forcePlayerItem.SetDelay(delay);
        }
	}

    public void ChangePlayerState(int playerID, int state)
    {
        ForcePlayerItem_N forcePlayerItem = this.GetForcePlayerItemByPlayerID(playerID);
        if (forcePlayerItem != null)
        {
            forcePlayerItem.SetState(state);
        }
    }

    public ForcePlayerItem_N GetForcePlayerItemByPlayerID(int playerID)
    {
        return this.m_CurForcePlayerItems.FirstOrDefault(a => a.GetPlayerID() == playerID);
    }

    public bool CheckFixRoleIsFull()
    {
        return !this.m_CurForcePlayerItems.Any(a => a.GetRoleID() != -1 && a.GetPlayerID() == -1);
    }

    #endregion

    #region private methods

    private void ChangeForceToNet(int roleID)
    {
        if (null != this.ChangePlayerForceEvent)
        {
            this.ChangePlayerForceEvent(this.GetForceID(), roleID);
        }
    }

    private void KickPlayerToNet(int playerID)
    {
        if (null != this.KickPlayerEvent)
        {
            this.KickPlayerEvent(playerID);
        }
    }

    private void InitRoleInfo(List<PlayerDesc> roleInfos)
    {
        if (roleInfos != null && roleInfos.Count > 0)
        {
            this.m_FixRoleCount = roleInfos.Count;
            for (int i = 0; i < roleInfos.Count; i++)
            {
                ForcePlayerItem_N forcePlayerItem_N = this.GetNewForcePlayerItem();
                forcePlayerItem_N.SetFixRole(roleInfos[i]);
            }
        }
        this.TryAddNullItem();
        this.RepostionForce();
    }

    private ForcePlayerItem_N GetNewForcePlayerItem()
    {
        ForcePlayerItem_N forcePlayerItem_N = null;
        if (this.m_ForcePlayerItemPool.Count() > 0)
        {
            forcePlayerItem_N = this.m_ForcePlayerItemPool.Dequeue();
            forcePlayerItem_N.gameObject.SetActive(true);
        }
        else
        {
            GameObject forcePlayerItem = GameObject.Instantiate(this.ForcePlayerItemPrefab);
            forcePlayerItem.transform.parent = this.ForceGrid.transform;
            forcePlayerItem.transform.localPosition = Vector3.zero;
            forcePlayerItem.transform.localScale = Vector3.one;
            forcePlayerItem_N = forcePlayerItem.GetComponent<ForcePlayerItem_N>();
        }
        forcePlayerItem_N.gameObject.name = this.GetNewItemName();
        forcePlayerItem_N.KickPlayerEvent = this.KickPlayerToNet;
        forcePlayerItem_N.ChangePlayerForceEvent = this.ChangeForceToNet;
        this.m_CurForcePlayerItems.Add(forcePlayerItem_N);
        return forcePlayerItem_N;
    }

    private void AddForcePlayerItem(RoomPlayerInfo playerInfo)
    {
        ForcePlayerItem_N forcePlayerItem_N=null;
        if (this.m_CurForcePlayerItems != null)
            forcePlayerItem_N = this.m_CurForcePlayerItems.FirstOrDefault(a => a.GetRoleID() == playerInfo.mRoleID && a.GetPlayerID() == -1);
        if (null == forcePlayerItem_N)
        {
            forcePlayerItem_N = this.GetNewForcePlayerItem();
        }
        forcePlayerItem_N.SetPlayer(playerInfo, this.m_IsRoomMaster);
        this.TryAddNullItem();
        this.UpdateForcePlayerCount();
        this.RepostionForce();
    }

    private string GetNewItemName()
    {
        return "ForcePlayerItem" + this.m_CurForcePlayerItems.Count().ToString("D4");
    }

    private void TryAddNullItem()
    {
        if (null==this.GetNullJoinableItem()&&this.GetCurJoinableCount()<this.m_ForceDesc.JoinablePlayerCount)
        {
            ForcePlayerItem_N forcePlayerItem_N_null = this.GetNewForcePlayerItem();
            forcePlayerItem_N_null.SetFixRole(null);
            forcePlayerItem_N_null.UpdateCanJoinCount(this.GetCurJoinableCount(), this.m_ForceDesc.JoinablePlayerCount);
        }
    }

    private int GetCurJoinableCount()
    {
        return this.m_CurForcePlayerItems.Count(a => a.GetRoleID() == -1 && a.GetPlayerID() != -1);
    }

    private int GetCurPlayerCount()
    {
        return this.m_CurForcePlayerItems.Where(a => a.GetPlayerID() != -1).Count();
    }

    private int GetCanJoinPlayerCount()
    {
        return this.m_FixRoleCount + this.m_ForceDesc.JoinablePlayerCount;
    }

    private void RemoveForcePlayerItem(int playerID)
    {
        ForcePlayerItem_N forcePlayerItem_N = null;
        if (this.m_CurForcePlayerItems != null)
            forcePlayerItem_N = this.m_CurForcePlayerItems.FirstOrDefault(a => a.GetPlayerID() == playerID);
        if (forcePlayerItem_N != null)
        {
            if (forcePlayerItem_N.GetRoleID() == -1)
            {
                forcePlayerItem_N.ResetAll();
                forcePlayerItem_N.gameObject.SetActive(false);
                this.m_CurForcePlayerItems.Remove(forcePlayerItem_N);
                this.m_ForcePlayerItemPool.Enqueue(forcePlayerItem_N);
            }
            else
            {
                forcePlayerItem_N.ResetPlayer();
            }
            this.RenameAllItem();
            ForcePlayerItem_N nullItem=this.GetNullJoinableItem();
            if (null == nullItem)
                TryAddNullItem();
            else
                nullItem.UpdateCanJoinCount(this.GetCurJoinableCount(), this.m_ForceDesc.JoinablePlayerCount);
            this.UpdateForcePlayerCount();
            this.RepostionForce();
        }
    }

    private void RenameAllItem()
    {
        for (int i = 0; i < this.m_CurForcePlayerItems.Count; i++)
        {
            this.m_CurForcePlayerItems[i].gameObject.name = "ForcePlayerItem" + i.ToString("D4");
        }
    }

    //获取空的的自由角色的Item
    private ForcePlayerItem_N GetNullJoinableItem()
    {
        return this.m_CurForcePlayerItems.FirstOrDefault(a => a.GetPlayerID() == -1&&a.GetRoleID()==-1);
    }

    private void RepostionForce()
    {
        this.ForceGrid.Reposition();
        if (null != this.RepositionEvent)
        {
            this.RepositionEvent();
        }
    }

    private void UpdateForceName(string name)
    {
        if(null!=name)
            this.ForceNameLabel.text=name;
    }

    private void UpdateForceColor(Color32 color)
    {
        this.ForceColorSprite.color = new Color(color.r/255f,color.g/255f,color.b/255f,color.a/255f);
    }

    private void UpdateForcePlayerCount()
    {
        if(null!=this.m_CurForcePlayerItems&&null!=this.m_ForceDesc)
            this.ForceCountLabel.text = string.Format("{0}/{1}", this.GetCurPlayerCount(), this.GetCanJoinPlayerCount());
    }
    #endregion ForceCountLabel
}
