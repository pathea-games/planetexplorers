using UnityEngine;
using System.Collections;
using CSRecord;
using ItemAsset;
using Pathea;
public partial class ColonyNetwork 
{
    void RPC_S2C_InitDataRecycle(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSRecycleData reocrdData = (CSRecycleData)_ColonyObj._RecordData;
		reocrdData.m_CurDeleteTime = stream.Read <float>();
		reocrdData.m_CurRepairTime = stream.Read<float> ();
		reocrdData.m_DeleteTime = stream.Read<float> ();
		reocrdData.m_Durability = stream.Read<float> ();
		reocrdData.m_RepairTime = stream.Read<float> ();
		reocrdData.m_RepairValue = stream.Read<float> ();
		reocrdData.m_ObjID = stream.Read<int>();
		reocrdData.m_CurTime = stream.Read<float> ();
		reocrdData.m_Time = stream.Read<float>();
		int [] keys = stream.Read<int[]> ();
		int [] values = stream.Read<int[]> ();
		
		for(int i = 0; i < keys.Length; i++)
		{
			((ColonyRecycle)_ColonyObj).m_RecycleItems[keys[i]] = values[i];
		}
	}

    void RPC_S2C_RCY_SyncRecycleItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		/*int itemId = */stream.Read <int>();
		/*int amount = */stream.Read<int> ();

        //to do
	}

    void RPC_S2C_RCY_SetItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
        int objId = stream.Read<int>();
        bool success = stream.Read<bool>();
        if (success)
        {
            //1.data
            CSRecycleData reocrdData = (CSRecycleData)_ColonyObj._RecordData;
            reocrdData.m_ObjID = objId;
		}
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
        //2.ui
        if (CSUI_MainWndCtrl.Instance.EngineeringUI != null && CSUI_MainWndCtrl.Instance.EngineeringUI.m_SubEngneering != null)
        {
            CSUI_MainWndCtrl.Instance.EngineeringUI.m_SubEngneering.SetResult(success, objId, m_Entity);
        }
	}

    void RPC_S2C_RCY_Start(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
        bool success = stream.Read<bool>();
        if (success)
        {
            //1.data
            CSRecycleData recordData = (CSRecycleData)_ColonyObj._RecordData;
            recordData.m_CurTime = 0;
			recordData.m_Time = ((CSRecycle)m_Entity).CountFinalTime();
			if(m_Entity==null){
				Debug.LogError("entity not ready");
				return;
			}
            ((CSRecycle)m_Entity).StartCounter(recordData.m_CurTime, recordData.m_Time);

            //2.ui
            if (CSUI_MainWndCtrl.Instance.EngineeringUI != null)
            {
                CSUI_MainWndCtrl.Instance.EngineeringUI.StartWorkerResult(CSConst.etRecyle, m_Entity, "");
            }
        }
    }

    void RPC_S2C_RCY_Stop(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		bool suc = stream.Read<bool>();
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
        if (suc)
        {
            ((CSRecycle)m_Entity).StopCounter();
        }
    }

    void RPC_S2C_RCY_FetchMaterial(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		/*int itemId = */stream.Read<int> ();
		/*bool suc = */stream.Read<bool> ();

        //to do
	}

    //lz-2016.12.28 回收完成，物品放进了玩家背包
    void RPC_S2C_RCY_End(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        if (TeamId == BaseNetwork.MainPlayer.TeamId)
        {
            CSUtils.ShowTips(RecycleConst.INFORM_FINISH_TO_PACKAGE);
            ResetRecycle();
        }
    }

    //lz-2016.12.28 回收完成，物品放进了基地仓库
    void RPC_S2C_RCY_MatsToStorage(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        if (PlayerNetwork.mainPlayer == null)
            return;
        if (TeamId == BaseNetwork.MainPlayer.TeamId)
        {
            CSUtils.ShowTips(RecycleConst.INFORM_FINISH_TO_STORAGE);
            ResetRecycle();
        }
    }

    //lz-2016.12.28 回收完成，物品放进了随机物品球
    void RPC_S2C_RCY_MatsToResult(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        Vector3 pos = stream.Read<Vector3>();
        Quaternion rot = stream.Read<Quaternion>();
        int[] items = stream.Read<int[]>();
        if (PlayerNetwork.mainPlayer == null)
            return;
        if (TeamId == BaseNetwork.MainPlayer.TeamId)
        {
            CSUtils.ShowTips(RecycleConst.INFORM_FINISH_TO_RANDOMITEM);
            RandomItemMgr.Instance.GenProcessingItem(pos, rot, items);
            ResetRecycle();
        }
    }

    void ResetRecycle()
    {
        if (null == _ColonyObj)
            return;

        CSRecycleData recordData = (CSRecycleData)_ColonyObj._RecordData;
        if (null == recordData)
            return;

        recordData.m_CurTime = -1;
        recordData.m_Time = -1;

        if (m_Entity == null)
        {
            Debug.LogError("entity not ready");
            return;
        }

        ((CSRecycle)m_Entity).StopCounter();
        ((CSRecycle)m_Entity).m_Item = null;
        ((CSRecycle)m_Entity).onRecylced();
    }

    void RPC_S2C_RCY_FetchItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
        bool success = stream.Read<bool>();
        if (success)
        {
            //1.data
            CSRecycleData reocrdData = (CSRecycleData)_ColonyObj._RecordData;
            reocrdData.m_ObjID = 0;
        }
        //2.ui
        if (CSUI_MainWndCtrl.Instance.EngineeringUI != null && CSUI_MainWndCtrl.Instance.EngineeringUI.m_SubEngneering != null)
        {
            CSUI_MainWndCtrl.Instance.EngineeringUI.m_SubEngneering.FetchResult(success, m_Entity);
        }
	}

    void RPC_S2C_RCY_SyncTime(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		/*float time = */stream.Read<float>();
	}

    public void RCY_SetItem(ItemObject item)
	{
		if(item == null)
            return;
        if (!(PeCreature.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>().package.HasItemObj(item)
             || PeCreature.Instance.mainPlayer.GetCmpt<EquipmentCmpt>()._ItemList.Contains(item))// equiped item
		   )
			return;
        RPCServer(EPacketType.PT_CL_RCY_SetItem, item.instanceId);
	}

	public void RCY_Start()
	{
        RPCServer(EPacketType.PT_CL_RCY_Start);
    }

	public void RCY_Stop()
	{
        RPCServer(EPacketType.PT_CL_RCY_Stop);
    }

    public void RCY_FetchMaterial(int itemId)
	{
        RPCServer(EPacketType.PT_CL_RCY_FetchMaterial,itemId);
	}

    public void RCY_FetchItem()
    {
        RPCServer(EPacketType.PT_CL_RCY_FetchItem);
    }
}

