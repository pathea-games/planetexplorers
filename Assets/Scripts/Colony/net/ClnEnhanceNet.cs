using UnityEngine;
using System.Collections;
using CSRecord;
using ItemAsset;
using Pathea;
public partial class ColonyNetwork 
{
    void RPC_S2C_InitDataEnhance(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSEnhanceData reocrdData = (CSEnhanceData)_ColonyObj._RecordData;
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

    void RPC_S2C_EHN_SetItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
        int objId = stream.Read<int>();
		bool success = stream.Read<bool>();
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
        if (success)
        {
            //1.data
            CSEnhanceData reocrdData = (CSEnhanceData)_ColonyObj._RecordData;
            reocrdData.m_ObjID = objId;
        }
        //2.ui
        if (CSUI_MainWndCtrl.Instance.EngineeringUI != null && CSUI_MainWndCtrl.Instance.EngineeringUI.m_SubEngneering != null)
        {
            CSUI_MainWndCtrl.Instance.EngineeringUI.m_SubEngneering.SetResult(success, objId, m_Entity);
        }
	}

    void RPC_S2C_EHN_Fetch(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		bool success = stream.Read<bool>();
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
        if (success)
        {
            //1.data
            CSEnhanceData reocrdData = (CSEnhanceData)_ColonyObj._RecordData;
            reocrdData.m_ObjID = 0;
        }
        //2.ui
        if (CSUI_MainWndCtrl.Instance.EngineeringUI != null && CSUI_MainWndCtrl.Instance.EngineeringUI.m_SubEngneering != null)
        {
            CSUI_MainWndCtrl.Instance.EngineeringUI.m_SubEngneering.FetchResult(success, m_Entity);
        }
	}


    void RPC_S2C_EHN_Start(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		string roleName = stream.Read<string> ();
		bool success = stream.Read<bool>();
		if (success)
		{
			//1.data
			CSEnhanceData recordData = (CSEnhanceData)_ColonyObj._RecordData;
			recordData.m_CurTime = 0;
			recordData.m_Time = ((CSEnhance)m_Entity).CountFinalTime();
			//recordData.m_Time = 20;
			if(m_Entity==null){
				Debug.LogError("entity not ready");
				return;
			}
			((CSEnhance)m_Entity).StartCounter(recordData.m_CurTime, recordData.m_Time);
			
			//2.ui
            if (CSUI_MainWndCtrl.Instance.EngineeringUI != null)
			{
                CSUI_MainWndCtrl.Instance.EngineeringUI.StartWorkerResult(CSConst.etEnhance, m_Entity, roleName);
			}
		}
	}

    void RPC_S2C_EHN_Stop(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		bool suc = stream.Read<bool>();
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
        if (suc)
        {
            ((CSEnhance)m_Entity).StopCounter();
        }

	}

    void RPC_S2C_EHN_End(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
        CSEnhanceData recordData = (CSEnhanceData)_ColonyObj._RecordData;
        recordData.m_CurTime = -1;
		recordData.m_Time = -1;
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
        ((CSEnhance)m_Entity).StopCounter();
        ((CSEnhance)m_Entity).OnEnhanced();
	}

    void RPC_S2C_EHN_SyncTime(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		/*float time = */stream.Read<float>();
	}
	public void EHN_SetItem(ItemObject item)
	{
		if(item == null)
            return;
        if (!(PeCreature.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>().package.HasItemObj(item)
             || PeCreature.Instance.mainPlayer.GetCmpt<EquipmentCmpt>()._ItemList.Contains(item))// equiped item
		   )
			return;
        RPCServer(EPacketType.PT_CL_EHN_SetItem, item.instanceId);
	}
	
	public void EHN_Start()
	{
		RPCServer (EPacketType.PT_CL_EHN_Start);
	}
	
	public void EHN_Stop()
	{
		RPCServer (EPacketType.PT_CL_EHN_Stop);
	}
	
	public void EHN_Fetch()
	{
		RPCServer (EPacketType.PT_CL_EHN_Fetch);	
	}

}
