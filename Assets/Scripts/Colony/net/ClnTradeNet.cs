using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSRecord;
using UnityEngine;


public partial class ColonyNetwork
{
    public CSTrade tradeEntity
    {
        get { return m_Entity as CSTrade; }
    }
    void RPC_S2C_InitDataTrade(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        CSTradeData recordData = (CSTradeData)_ColonyObj._RecordData;
        recordData.m_CurDeleteTime = stream.Read<float>();
        recordData.m_CurRepairTime = stream.Read<float>();
        recordData.m_DeleteTime = stream.Read<float>();
        recordData.m_Durability = stream.Read<float>();
        recordData.m_RepairTime = stream.Read<float>();
		recordData.m_RepairValue = stream.Read<float>();
    }


//    void RPC_S2C_TRD_AddTown(uLink.BitStream stream, uLink.NetworkMessageInfo info)
//	{
//		if(PlayerNetwork.mainPlayer==null)
//			return;
//        if (TeamId != BaseNetwork.MainPlayer.TeamId)
//        {
//            Debug.LogError("TradePost not my team:my="+ BaseNetwork.MainPlayer.TeamId+" this="+TeamId);
//            return;
//        }
//            
//        Vector3 pos = stream.Read<Vector3>();
//        string name = stream.Read<string>();
//        int campId = stream.Read<int>();
//        int tradeId = stream.Read<int>();
//        float currentTime = stream.Read<float>();
//        TradeObj[] needItems = stream.Read<TradeObj[]>();
//        TradeObj[] rewardItems = stream.Read<TradeObj[]>();
//		
//		if(m_Entity==null){
//			Debug.LogError("entity not ready");
//			return;
//		}
//        DetectedTown dt = new DetectedTown(pos, name, campId);
//        if (tradeEntity.ttiiDic.ContainsKey(dt.PosCenter))
//        {
//            return;
//        }
//        CSTradeInfoData ctid = CSTradeInfoData.GetData(tradeId);
//        if (ctid == null)
//        {
//            Debug.LogError("tradeId Error:" + tradeId);
//            return;
//		}
//        TownTradeItemInfo ttii = new TownTradeItemInfo(dt, tradeId, currentTime, needItems, rewardItems);
//        tradeEntity.AddTownResult(ttii);
//
//    }
//
//	void RPC_S2C_TRD_RemoveTown(uLink.BitStream stream, uLink.NetworkMessageInfo info){
//		if(PlayerNetwork.mainPlayer==null)
//			return;
//		if (TeamId != BaseNetwork.MainPlayer.TeamId)
//		{
//			Debug.LogError("TradePost not my team:my="+ BaseNetwork.MainPlayer.TeamId+" this="+TeamId);
//			return;
//		}
//
//		IntVector2 keyPos = stream.Read<IntVector2>();
//		if (!tradeEntity.ttiiDic.ContainsKey(keyPos))
//		{
//			return;
//		}
//		tradeEntity.RemoveTownResult(keyPos);
//	}
//
//
//    void RPC_S2C_TRD_TryTrade(uLink.BitStream stream, uLink.NetworkMessageInfo info)
//	{
//		if(PlayerNetwork.mainPlayer==null)
//			return;
//        if (TeamId != BaseNetwork.MainPlayer.TeamId)
//        {
//            Debug.LogError("TradePost not my team:my=" + BaseNetwork.MainPlayer.TeamId + " this=" + TeamId);
//            return;
//        }
//        int pId = stream.Read<int>();
//        IntVector2 pos = stream.Read<IntVector2>();
//        TradeObj[] needItems = stream.Read<TradeObj[]>();
//		
//		if(m_Entity==null){
//			Debug.LogError("entity not ready");
//			return;
//		}
//        if (!tradeEntity.ttiiDic.ContainsKey(pos))
//        {
//            return;
//        }
//        TownTradeItemInfo ttii = tradeEntity.ttiiDic[pos];
//        ttii.SetNeedItems(needItems);
//
//        if (PlayerNetwork.mainPlayer.Id == pId)
//        {
//            tradeEntity.TradeSucessResult(ttii);
//        }
//        else
//        {
//            tradeEntity.UpdateTradeItemView(ttii);
//        }
//    }
//
//
//    void RPC_S2C_TRD_RefreshItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
//    {
//        IntVector2 pos = stream.Read<IntVector2>();
//        TradeObj[] needItems = stream.Read<TradeObj[]>();
//        TradeObj[] rewardItems = stream.Read<TradeObj[]>();
//		
//		if(m_Entity==null){
//			Debug.LogError("entity not ready");
//			return;
//		}
//        if (!tradeEntity.ttiiDic.ContainsKey(pos))
//        {
//            return;
//        }
//        TownTradeItemInfo ttii = tradeEntity.ttiiDic[pos];
//        ttii.BeRefreshed(needItems, rewardItems);
//        tradeEntity.RefreshItem(ttii);
//    }


	//new
	void RPC_S2C_BuyItem(uLink.BitStream stream, uLink.NetworkMessageInfo info){
		int instanceId = stream.Read<int>();
		if(tradeEntity!=null)
			tradeEntity.UpdateBuyResultMulti(instanceId);
	}
	void RPC_S2C_SellItem(uLink.BitStream stream, uLink.NetworkMessageInfo info){
		int instanceId = stream.Read<int>();
		if(tradeEntity!=null)
			tradeEntity.UpdateSellResultMulti(instanceId);
	}
	void RPC_S2C_RepurchaseItem(uLink.BitStream stream, uLink.NetworkMessageInfo info){
		int instanceId = stream.Read<int>();
		if(tradeEntity!=null)
			tradeEntity.UpdateRepurchaseResultMulti(instanceId);
	}
	void RPC_S2C_UpdateBuyItem(uLink.BitStream stream, uLink.NetworkMessageInfo info){
		List<int> instanceIdIdList = stream.Read<int[]>().ToList();
		if(tradeEntity!=null)
			tradeEntity.UpdateBuyItemMulti(instanceIdIdList);
	}
	void RPC_S2C_UpdateRepurchaseItem(uLink.BitStream stream, uLink.NetworkMessageInfo info){
		List<int> instanceIdIdList = stream.Read<int[]>().ToList();
		if(tradeEntity!=null)
			tradeEntity.UpdateRepurchaseMulti(instanceIdIdList);
	}
	void RPC_S2C_UpdateMoney(uLink.BitStream stream, uLink.NetworkMessageInfo info){
		int money = stream.Read<int>();
		if(tradeEntity!=null)
			tradeEntity.UpdateMoneyMulti(money);
	}

		
			
}
