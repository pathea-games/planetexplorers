using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using CustomData;
using SkillAsset;
using ItemAsset;
public partial class PlayerNetwork
{

	public void CreateAccountItems(int itemType,int amount)
	{
		if(AccountItems.self.CheckCreateItems(itemType,amount))
		{
			RPCServer(EPacketType.PT_InGame_AccItems_CreateItem, itemType,amount);
		}
	}

	void RPC_AccItems_CreateItem(uLink.BitStream stream,uLink.NetworkMessageInfo info)
	{
		int itemType = stream.Read<Int32> ();
		int amount = stream.Read<Int32> ();
		AccountItems.self.DeleteItems (itemType, amount);
		UILobbyShopItemMgr._self.MallItemEvent (0, UIMallWnd.Instance.mCurrentTab);
		//updateUI
	}
}

