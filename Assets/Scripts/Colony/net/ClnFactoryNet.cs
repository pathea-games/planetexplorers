using UnityEngine;
using System.Collections;
using CSRecord;
using CustomData;
using ItemAsset;
public partial class ColonyNetwork 
{
    void RPC_S2C_InitDataFactory(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSFactoryData recordData = (CSFactoryData)_ColonyObj._RecordData;
		recordData.m_CurDeleteTime = stream.Read <float>();
		recordData.m_CurRepairTime = stream.Read<float> ();
		recordData.m_DeleteTime = stream.Read<float> ();
		recordData.m_Durability = stream.Read<float> ();
		recordData.m_RepairTime = stream.Read<float> ();
		recordData.m_RepairValue = stream.Read<float> ();
		CompoudItem [] data =  stream.Read<CompoudItem[]>();
		if (data == null)
			return;
		for (int i = 0; i < data.Length; i++)
		{
			recordData.m_CompoudItems.Add(data[i]);
		}
	}
    void RPC_S2C_FCT_IsReady(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
        /*int itemId = */stream.Read<int>();
		int index = stream.Read<int>();
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
        //CSFactoryData recordData = (CSFactoryData)_ColonyObj._RecordData;
        //if (recordData.m_CompoudItems.Count > index && recordData.m_CompoudItems[index]!=null)
        //{
        //    recordData.m_CompoudItems[index].curTime = recordData.m_CompoudItems[index].time;
        //}
        ((CSFactory)m_Entity).MultiModeIsReady(index);
	}

    void RPC_S2C_FCT_AddCompoudList(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CompoudItem data_ci = stream.Read <CompoudItem>();
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
        ((CSFactory)m_Entity).SetCompoudItem(data_ci.itemID,data_ci.itemCnt,data_ci.time);
	}

    void RPC_S2C_FCT_RemoveCompoudList(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int index = stream.Read <int>();
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
		((CSFactory)m_Entity).MultiModeTakeAwayCompoudItem(index);
	}

    void RPC_S2C_FCT_SyncItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int index = stream.Read <int>();
		CompoudItem data_ci = stream.Read <CompoudItem>();

		CSFactoryData recordData = (CSFactoryData)_ColonyObj._RecordData;
		if(recordData.m_CompoudItems.Count > index)
			recordData.m_CompoudItems [index] = data_ci;
	}

    void RPC_S2C_FCT_Fetch(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int itemId = stream.Read <int>();
		bool succ = stream.Read <bool>();
        if (succ)
        {
            //1.success UI
            CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mTakeAwayCompoundItem.GetString(), ItemProto.GetItemData(itemId).GetName()));
        }
	}

    void RPC_S2C_FCT_Compoud(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		bool succ = stream.Read <bool>();

        if (succ)
        {
            int itemId = stream.Read<int>();

            //2.success UI
            if (CSUI_MainWndCtrl.Instance.FactoryUI != null)
            {
                CSUI_MainWndCtrl.Instance.FactoryUI.OnCompoundBtnClickSuccess(itemId, (CSFactory)m_Entity);
            }
        }
	}

	void RPC_S2C_FCT_SyncAllItems(uLink.BitStream stream, uLink.NetworkMessageInfo info){
		CompoudItem[] allItems = stream.Read <CompoudItem[]>();
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
		((CSFactory)m_Entity).SetAllItems(allItems);
	}

	void RPC_S2C_FCT_GenFactoryCancel(uLink.BitStream stream, uLink.NetworkMessageInfo info){
		Vector3 pos = stream.Read<Vector3>();
		Quaternion rot = stream.Read<Quaternion>();
		int[] items = stream.Read<int[]>();

		RandomItemMgr.Instance.GenFactoryCancel(pos,rot,items);
	}

	public void FCT_Compoud(int skillId,int count)
	{
		RPCServer(EPacketType.PT_CL_FCT_Compoud, skillId, count);
	}

	public void FCT_Fetch( int index)
	{
		RPCServer(EPacketType.PT_CL_FCT_Fetch,index);
	}

}
