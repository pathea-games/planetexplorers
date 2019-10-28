using UnityEngine;
using System.Collections;
using CSRecord;
using ItemAsset;
using Pathea;
public partial class ColonyNetwork 
{

    void RPC_S2C_InitDataRepair(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSRepairData reocrdData = (CSRepairData)_ColonyObj._RecordData;
		reocrdData.m_CurDeleteTime = stream.Read <float>();
		reocrdData.m_CurRepairTime = stream.Read<float> ();
		reocrdData.m_DeleteTime = stream.Read<float> ();
		reocrdData.m_Durability = stream.Read<float> ();
		reocrdData.m_RepairTime = stream.Read<float> ();
		reocrdData.m_RepairValue = stream.Read<float> ();
		reocrdData.m_ObjID = stream.Read<int>();
		reocrdData.m_CurTime = stream.Read<float> ();
		reocrdData.m_Time = stream.Read<float>();
	}

    void RPC_S2C_RPA_SetItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int objId = stream.Read<int>();
        bool success = stream.Read<bool>();
        if (success) { 
            //1.data
            CSRepairData reocrdData = (CSRepairData)_ColonyObj._RecordData;
            reocrdData.m_ObjID = objId;
		}
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
        //2.ui
		if (CSUI_MainWndCtrl.Instance.EngineeringUI != null && CSUI_MainWndCtrl.Instance.EngineeringUI.m_SubEngneering != null)
        {
			CSUI_MainWndCtrl.Instance.EngineeringUI.m_SubEngneering.SetResult(success,objId, m_Entity);
        }
	}

    void RPC_S2C_RPA_Start(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		string roleName = stream.Read<string> ();
        bool success = stream.Read<bool>();
        if (success)
        {
            //1.data
            CSRepairData recordData = (CSRepairData)_ColonyObj._RecordData;
            recordData.m_CurTime = 0;
			recordData.m_Time = ((CSRepair)m_Entity).CountFinalTime();
			//recordData.m_Time = 20;
			if(m_Entity==null){
				Debug.LogError("entity not ready");
				return;
			}
            ((CSRepair)m_Entity).StartCounter(recordData.m_CurTime, recordData.m_Time);

            //2.ui
			if (CSUI_MainWndCtrl.Instance.EngineeringUI != null)
            {
				CSUI_MainWndCtrl.Instance.EngineeringUI.StartWorkerResult(CSConst.etRepair, m_Entity, roleName);
            }
        }
	}

    void RPC_S2C_RPA_Stop(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		bool suc = stream.Read<bool> ();
        if (suc)
        {
            ((CSRepair)m_Entity).StopCounter();
        }
	}


    void RPC_S2C_RPA_End(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        CSRepairData recordData = (CSRepairData)_ColonyObj._RecordData;
        recordData.m_CurTime = -1;
		recordData.m_Time = -1;
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
        ((CSRepair)m_Entity).StopCounter();
        ((CSRepair)m_Entity).OnRepairItemEnd();
	}


    void RPC_S2C_RPA_FetchItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
        bool success = stream.Read<bool>();
        if (success)
        {
            //1.data
            CSRepairData reocrdData = (CSRepairData)_ColonyObj._RecordData;
            reocrdData.m_ObjID = 0;
		}
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
        //2.ui
		if (CSUI_MainWndCtrl.Instance.EngineeringUI != null && CSUI_MainWndCtrl.Instance.EngineeringUI.m_SubEngneering != null)
        {
			CSUI_MainWndCtrl.Instance.EngineeringUI.m_SubEngneering.FetchResult(success, m_Entity);
        }
	}

    void RPC_S2C_RPA_SyncTime(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		/*float time = */stream.Read<float>();
	}
	public void RPA_Stop()
	{
        RPCServer(EPacketType.PT_CL_RPA_Stop);
	}
	public void RPA_Start()
	{
        RPCServer(EPacketType.PT_CL_RPA_Start);
	}
	public void RPA_SetItem( int objId)
	{
		ItemObject item = ItemMgr.Instance.Get (objId);
		if(item == null)
			return;
        if (!(PeCreature.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>().package.HasItemObj(item)
             || PeCreature.Instance.mainPlayer.GetCmpt<EquipmentCmpt>()._ItemList.Contains(item))// equiped item
		   )
            return;
        RPCServer(EPacketType.PT_CL_RPA_SetItem,objId);
	}
	public void RPA_FetchItem()
	{
        RPCServer(EPacketType.PT_CL_RPA_FetchItem);
	}
}

