using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using CustomData;
using SkillAsset;
using ItemAsset;
using Pathea;
 
/// <summary>
/// 物品相关的同步处理
/// by PuJi
/// </summary>
public partial class PlayerNetwork
{
	public void RequestShopData (int npcId)
	{
		RPCServer (EPacketType.PT_InGame_InitShop, npcId);
	}

	public void RequestBuy (int npcId, int objId, int num)
	{
		RPCServer (EPacketType.PT_InGame_Buy, npcId, objId, num);
	}

	public void RequestRepurchase (int npcId, int objId,int num)
	{
		RPCServer (EPacketType.PT_InGame_Repurchase, npcId, objId,num);
	}

	public void RequestSell (int npcId, int objId, int num)
	{
		RPCServer (EPacketType.PT_InGame_Sell, npcId, objId, num);
	}
	#region Action Callback APIs
	//初始化多人商店
	void RPC_S2C_InitNpcShop (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int npcid = stream.Read<int> ();
		int[] objsOnSell = stream.Read<int[]> ();

        //to do--init npc money

        //NpcRandom npc = NpcManager.Instance.GetNpc(npcid) as NpcRandom;
        //Debug.Log("npcName:" + npc.NpcName);

        //end--init npc money

        //Debug.Log("npcid:" + npcid);
        //Debug.Log("money:" + money);
        //Debug.Log("itemsOnSell count:" + objIdOnSell.Count());
        //for (int i = 0; i < objIdOnSell.Count(); i++)
        //{
        //    Debug.Log(objIdOnSell[i]);
        //}
        GameUI.Instance.mShopWnd.InitNpcShopWhenMultiMode(npcid, objsOnSell);
	}
	void RPC_S2C_ChangeCurrency (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int currency = stream.Read<int> ();
		if(currency == (int)CustomData.EMoneyType.Digital)
		{
			if(Money.Digital == true)
				return;
			foreach (var item in EntityMgr.Instance.All)
			{
				if(item == null || item.GetGameObject() == null)
					continue;
				NpcPackageCmpt npcpc = item.GetCmpt<NpcPackageCmpt>();
				if (npcpc == null)
					continue;
				npcpc.money.SetCur(npcpc.money.current * 4);
			}
			Money.Digital = true;
			GameUI.Instance.mShopWnd.mMeatSprite.gameObject.SetActive(false);
			GameUI.Instance.mShopWnd.mMoneySprite.gameObject.SetActive(true);
			GameUI.Instance.mItemPackageCtrl.nMoneyRoot.SetActive(true);
		}
		else if(currency == (int)CustomData.EMoneyType.Meat)
		{
			if(Money.Digital == false)
				return;
			Money.Digital = false;
			GameUI.Instance.mShopWnd.mMeatSprite.gameObject.SetActive(true);
			GameUI.Instance.mShopWnd.mMoneySprite.gameObject.SetActive(false);
			GameUI.Instance.mItemPackageCtrl.nMoneyRoot.SetActive(false);
		}
	}


	//void RPC_S2C_SetNpcMoney(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	//{
	//	int _npcid;
	//	int _money;
	//	stream.TryRead<int>(out _npcid);
	//	stream.TryRead<int>(out _money);

	//	NpcRandom npc = NpcManager.Instance.GetNpc(_npcid) as NpcRandom;
	//	npc.Money.Current = _money;
	//}

	//void RPC_S2C_SyncShopItemIndex(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	//{
       
	//	int[] _objIDs;

	//	stream.TryRead<int[]>(out _objIDs);

	//	//LogManager.Debug("size: "+_objIDs.Count());
	//	//for (int i=0;i<_objIDs.Count();i++)
	//	//{
	//	//    LogManager.Debug(_objIDs[i]);
	//	//}
	//	//to do: init shop.
	//	GameUI.Instance.mShopGui.InitShopWhenMutipleMode(_objIDs);

	//}

	/// <summary>
	/// 增加一个回购物品
	/// </summary>
	void RPC_S2C_SyncRepurchaseItemIDs (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int[] ids = stream.Read<int[]> ();

		GameUI.Instance.mShopWnd.RepurchaseList.Clear ();

		foreach (int id in ids)
			GameUI.Instance.mShopWnd.AddNewRepurchaseItem (id);

		GameUI.Instance.mShopWnd.ResetItem ();
	}
	#endregion
}