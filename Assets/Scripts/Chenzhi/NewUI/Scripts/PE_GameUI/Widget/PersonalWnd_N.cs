using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PersonalWnd_N : MonoBehaviour 
{
	public UILabel mGroupName;
	public PlayerScoreItem_N mPerfab;
	public Transform mListRoot;
	
	List<PlayerScoreItem_N> mPlayerItemList = new List<PlayerScoreItem_N>();
	
	public void Init()
	{
		for(int i = 0; i < 8; i++)
		{
			PlayerScoreItem_N addItem = Instantiate(mPerfab) as PlayerScoreItem_N;
			addItem.transform.parent = mListRoot;
			addItem.transform.localScale = Vector3.one;
			addItem.transform.localPosition = new Vector3(0, 88 - i * 30f ,0);
			mPlayerItemList.Add(addItem);
		}
	}
	
	internal void SetInfo(List<PlayerBattleInfo> playerList)
	{
		for(int i = 0; i < playerList.Count; i++)
		{
			if(i < mPlayerItemList.Count)
			{
				mPlayerItemList[i].SetInfo(playerList[i].RoleName, playerList[i].Info._killCount,
				                           playerList[i].Info._deathCount, (int)playerList[i].Info._point);
			}
		}
	}
}
