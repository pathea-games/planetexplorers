using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CSRecord;
public partial class ColonyNetwork 
{
    void RPC_S2C_InitDataFarm(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSFarmData reocrdData = (CSFarmData)_ColonyObj._RecordData;
		reocrdData.m_CurDeleteTime = stream.Read <float>();
		reocrdData.m_CurRepairTime = stream.Read<float> ();
		reocrdData.m_DeleteTime = stream.Read<float> ();
		reocrdData.m_Durability = stream.Read<float> ();
		reocrdData.m_RepairTime = stream.Read<float> ();
		reocrdData.m_RepairValue = stream.Read<float> ();
		int [] seedkeys = stream.Read<int[]> ();
		int [] seedvalues = stream.Read<int[]> ();
		int [] toolskeys = stream.Read<int[]> ();
		int [] toolsvalues = stream.Read<int[]> ();
		for(int i = 0; i < seedkeys.Length; i++)
		{
			reocrdData.m_PlantSeeds[seedkeys[i]] = seedvalues[i];
		}
		for(int i = 0; i < toolskeys.Length; i++)
		{
			reocrdData.m_Tools[toolskeys[i]] = toolsvalues[i];
		}
		reocrdData.m_AutoPlanting = stream.Read<bool> ();
		reocrdData.m_SequentialPlanting = stream.Read<bool> ();
	}

	void RPC_S2C_FARM_SetPlantSeed(uLink.BitStream stream,uLink.NetworkMessageInfo info)
	{
		CSFarmData reocrdData = (CSFarmData)_ColonyObj._RecordData;
		int index = stream.Read <int>();
		int itemObjId = stream.Read <int>();
		bool success = stream.Read<bool> ();
		if (success)
			reocrdData.m_PlantSeeds [index] = itemObjId;

        if(CSUI_MainWndCtrl.Instance.FarmUI!=null){
            CSUI_MainWndCtrl.Instance.FarmUI.SetPlantSeedResult(success, itemObjId,index, m_Entity);
        }
	}

	void RPC_C2S_FARM_SetPlantTool(uLink.BitStream stream,uLink.NetworkMessageInfo info)
	{
		CSFarmData reocrdData = (CSFarmData)_ColonyObj._RecordData;
		int index = stream.Read <int>();
		int itemObjId = stream.Read <int>();
		bool success = stream.Read<bool> ();
		if (success)
			reocrdData.m_Tools [index] = itemObjId;

        if (CSUI_MainWndCtrl.Instance.FarmUI != null)
        {
            CSUI_MainWndCtrl.Instance.FarmUI.SetPlantToolResult(success, itemObjId, index, m_Entity);
        }
	}

	void RPC_S2C_FARM_SetSequentialActive(uLink.BitStream stream,uLink.NetworkMessageInfo info)
	{
		CSFarmData reocrdData = (CSFarmData)_ColonyObj._RecordData;
		bool bActive = stream.Read<bool> ();
		reocrdData.m_SequentialPlanting = bActive;
        if (CSUI_MainWndCtrl.Instance.FarmUI != null)
        {
            CSUI_MainWndCtrl.Instance.FarmUI.SetSequentialActiveResult(bActive, m_Entity);
        }
	}

	void RPC_S2C_FARM_SetAutoPlanting(uLink.BitStream stream,uLink.NetworkMessageInfo info)
	{
		CSFarmData reocrdData = (CSFarmData)_ColonyObj._RecordData;
		bool bActive = stream.Read<bool> ();
		reocrdData.m_AutoPlanting = bActive;
	}

    void RPC_S2C_FARM_FetchSeedItemResult(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        CSFarmData reocrdData = (CSFarmData)_ColonyObj._RecordData;
        int index = stream.Read<int>();
        bool success = stream.Read<bool>();
        if (success)
        {
            //1.data
            reocrdData.m_PlantSeeds[index] = 0;
        }
        //2.ui
        if (CSUI_MainWndCtrl.Instance.FarmUI != null)
        {
            CSUI_MainWndCtrl.Instance.FarmUI.FetchSeedResult(success,index, m_Entity);
        }
    }

    void RPC_S2C_FARM_FetchToolItemResult(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        CSFarmData reocrdData = (CSFarmData)_ColonyObj._RecordData;
        int index = stream.Read<int>();
        bool success = stream.Read<bool>();
        if (success)
        {
            //1.data
            reocrdData.m_Tools[index] = 0;
        }
        //2.ui
        if (CSUI_MainWndCtrl.Instance.FarmUI != null)
        {
            CSUI_MainWndCtrl.Instance.FarmUI.FetchToolResult(success, index, m_Entity);
        }
    }

	void RPC_S2C_FARM_DeleteSeed(uLink.BitStream stream,uLink.NetworkMessageInfo info)
	{
		CSFarmData recordData = (CSFarmData)_ColonyObj._RecordData;
		int itemObjId = stream.Read <int>();
        int key = -1;
        foreach (var item in recordData.m_PlantSeeds)
        {
            if (item.Value == itemObjId)
            {
                key = item.Key;
                break;
            }
        }

        if (key != -1 && CSUI_MainWndCtrl.Instance.FarmUI != null)
        {
            CSUI_MainWndCtrl.Instance.FarmUI.DeleteSeedResult(m_Entity, itemObjId, key);
        }


    }


	void RPC_S2C_FARM_DeletePlantTool(uLink.BitStream stream,uLink.NetworkMessageInfo info)
	{
		CSFarmData recordData = (CSFarmData)_ColonyObj._RecordData;
		int itemObjId = stream.Read <int>();
        int key = -1;
        foreach (var item in recordData.m_Tools)
        {
            if (item.Value == itemObjId)
            {
                key=item.Key;
                return;
            }
        }

        if (key != -1)
        {
            recordData.m_Tools.Remove(key);
        }

        if (CSUI_MainWndCtrl.Instance.FarmUI != null)
        {
            CSUI_MainWndCtrl.Instance.FarmUI.DeleteToolResult(m_Entity, itemObjId);
        }
	}

    void RPC_S2C_FARM_RestoreWater(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
//        CSFarmData reocrdData = (CSFarmData)_ColonyObj._RecordData;
		FarmPlantLogic plant = stream.Read<FarmPlantLogic>();
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
        (m_Entity as CSFarm).RestoreWateringPlant(plant);
    }
    void RPC_S2C_FARM_RestoreClean(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
//        CSFarmData reocrdData = (CSFarmData)_ColonyObj._RecordData;
		FarmPlantLogic plant = stream.Read<FarmPlantLogic>();
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
        (m_Entity as CSFarm).RestoreCleaningPlant(plant);
    }
    void RPC_S2C_FARM_RestoreGetBack(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
//        CSFarmData reocrdData = (CSFarmData)_ColonyObj._RecordData;
		FarmPlantLogic plant = stream.Read<FarmPlantLogic>();
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
        (m_Entity as CSFarm).RestoreRipePlant(plant);
    }


	public void SetPlantSeed(int index,int itemObjId)
	{
        RPCServer(EPacketType.PT_CL_FARM_SetPlantSeed, index, itemObjId);
	}

	public void SetPlantTool(int index,int itemObjId)
	{
        RPCServer(EPacketType.PT_CL_FARM_SetPlantTool, index, itemObjId);
	}

	public void SetSequentialActive(bool bActive)
	{
        RPCServer(EPacketType.PT_CL_FARM_SetSequentialActive,bActive);
	}

	public void SetAutoPlanting(bool bActive)
	{
        RPCServer(EPacketType.PT_CL_FARM_SetAutoPlanting,bActive);
	}

    public void FetchSeedItem(int index)
    {
        RPCServer(EPacketType.PT_CL_FARM_FetchSeedItem, index);
    }

    public void FetchToolItem(int index)
    {
        RPCServer(EPacketType.PT_CL_FARM_FetchToolItem, index);
    }
}

