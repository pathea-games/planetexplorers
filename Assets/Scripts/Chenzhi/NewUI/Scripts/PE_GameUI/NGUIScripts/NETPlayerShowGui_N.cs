using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NETPlayerShowGui_N : UIStaticWnd 
{
	public NetPlayerInfoItem_N 	mPerfab;
	public UIGrid						mGrid;
	
	List<NetPlayerInfoItem_N>	mPlayerList = new List<NetPlayerInfoItem_N>();
	
    //public void AddPlayer(Player netPlayer)
    //{
    //    NetPlayerInfoItem_N addPlayer = Instantiate(mPerfab) as NetPlayerInfoItem_N;
    //    addPlayer.transform.parent = mGrid.transform;
    //    addPlayer.transform.localPosition = Vector3.zero;
    //    addPlayer.transform.localScale = Vector3.one;
    //    addPlayer.SetPlayer(netPlayer,this);
    //    mPlayerList.Add(addPlayer);
    //    mGrid.Reposition();
    //}
	
	public void RemovePlayer(NetPlayerInfoItem_N item)
	{
		mPlayerList.Remove(item);
		item.transform.parent = null;
		Destroy(item.gameObject);
		mGrid.Reposition();
	}
}
