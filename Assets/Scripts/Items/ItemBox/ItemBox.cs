using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;

public class ItemBox : MousePickableChildCollider
{
	[HideInInspector]
	public int mID;
	
	[HideInInspector]
	public MapObjNetwork mNetWork;
	
	public Vector3 mPos
	{
		get { return transform.position; }
		set { transform.position = value; }
	}
	
	List<int> mItemList;
	
	public List<int> ItemList{ get { return mItemList; } }
	
	void Awake()
	{
		mItemList = new List<int>();
	}
	
	public void OnRequestItemList(List<int> itemlist ,bool bShow = true)
	{
        if (null != GameUI.Instance.mItemBox && bShow)
			GameUI.Instance.mItemBox.Show();
		ResetItem(itemlist);
		if(null != GameUI.Instance.mItemBox)
			GameUI.Instance.mItemBox.SetItemList(this, mItemList);
	}
	
	public void ResetItem(List<int> itemlist)
	{
		mItemList.Clear();
		AddItems(itemlist);
	}
	
	public void InsertItem(List<int> itemList)
	{
		if(null != mNetWork)
			mNetWork.InsertItemList(itemList.ToArray());
	}
	
	public void AddItems(List<int> itemlist)
	{
		foreach(int objID in itemlist)
		{
			if(!mItemList.Contains(objID))
				mItemList.Add(objID);
		}
		ResetUI();
	}

	public void AddItem(int objID)
	{
		if(!mItemList.Contains(objID))
		{
			mItemList.Add(objID);
			ResetUI();
		}
	}

	public void AddItem(ItemObject item)
	{
		AddItem(item.instanceId);
	}
	
	public void RemoveItem(int objID)
	{
		if(mItemList.Remove(objID))
			ResetUI();
	}
	
	void ResetUI()
	{
		if(null != GameUI.Instance.mItemBox && GameUI.Instance.mItemBox.OpBox == this)
			GameUI.Instance.mItemBox.SetItemList(this, mItemList);
	}

    protected override void CheckOperate()
	{
		base.CheckOperate ();
		if(PeInput.Get(PeInput.LogicFunction.OpenItemMenu))
		{
            if(null != GameUI.Instance.mMainPlayer && !GameUI.Instance.bMainPlayerIsDead)
            {
                if(GameConfig.IsMultiMode)
                {
                    if(null != mNetWork)
                        mNetWork.RequestItemList();
                    GameUI.Instance.mItemBox.Show();
                    mItemList.Clear();
					GameUI.Instance.mItemBox.SetItemList(this, mItemList);
                }
                else
                {
					GameUI.Instance.mItemBox.Show();
					GameUI.Instance.mItemBox.SetItemList(this, mItemList);
                    ResetUI();
                }
            }
		}
	}
	
	public void SendItemToPlayer(ItemObject itemObj)
	{
        //if(null != PlayerFactory.mMainPlayer)
        //{
        //    if(GameConfig.IsMultiMode)
        //    {
        //        if(null != mNetWork)
        //            mNetWork.GetItem(itemObj.instanceId);
        //    }
        //    else
        //    {
        //        if(PlayerFactory.mMainPlayer.AddItem(itemObj))
        //        {
        //            mItemList.Remove(itemObj.instanceId);
        //            ResetUI();
        //            CheckDestroy();
        //        }
        //    }
        //}
	}
	
	public void SendAllItemToPlayer()
	{
		if(GameConfig.IsMultiMode)
		{
			if(null != mNetWork)
				mNetWork.GetAllItem();
		}
		else
		{
			foreach(int itemObjID in mItemList)
			{
				ItemObject itemObj = ItemMgr.Instance.Get(itemObjID);
                //if(PlayerFactory.mMainPlayer.AddItem(itemObj))
                //    mItemList.Remove(itemObj.instanceId);
                //else
                //{
                //    ResetUI();
                //    break;
                //}
                if (null != Pathea.MainPlayerCmpt.gMainPlayer)
                {
                    Pathea.PlayerPackageCmpt pPC = Pathea.MainPlayerCmpt.gMainPlayer.GetComponent<Pathea.PlayerPackageCmpt>();
                    pPC.Add(itemObj);
                }
			}
            mItemList.Clear();
			CheckDestroy();
		}
		
	}
	
	void CheckDestroy()
	{
		if(mItemList.Count == 0)
			ItemBoxMgr.Instance.Remove(this);
	}
}
