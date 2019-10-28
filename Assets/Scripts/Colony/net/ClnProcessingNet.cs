using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSRecord;
using UnityEngine;

public partial class ColonyNetwork
{
    public CSProcessing csp
    {
        get { return m_Entity as CSProcessing; }
    }
    void RPC_S2C_InitDataProcessing(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        CSProcessingData recordData = (CSProcessingData)_ColonyObj._RecordData;
        recordData.m_CurDeleteTime = stream.Read<float>();
        recordData.m_CurRepairTime = stream.Read<float>();
        recordData.m_DeleteTime = stream.Read<float>();
        recordData.m_Durability = stream.Read<float>();
        recordData.m_RepairTime = stream.Read<float>();
        recordData.m_RepairValue = stream.Read<float>();
        //--to do init tasktable
        byte[] dataArray = stream.Read<byte[]>();
        CSProcessing.ParseData(dataArray, recordData);


    }

    void RPC_S2C_PRC_AddItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int taskIndex = stream.Read<int>();
		ItemIdCount[] protoIdCount = stream.Read<ItemIdCount[]>();
		if(csp==null){
			Debug.LogError("processing not exist");
			return;
		}
		if(taskIndex<0 ||taskIndex>=csp.mTaskTable.Length)
		{
			Debug.LogError("index illegel");
			return;
		}
        if (csp.mTaskTable[taskIndex] == null)
        {
            csp.mTaskTable[taskIndex] = new ProcessingTask();
        }
        csp.mTaskTable[taskIndex].itemList = protoIdCount.ToList();
        csp.AddItemResult(taskIndex);
    }
    //--to do: removeItem
    void RPC_S2C_PRC_RemoveItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int taskIndex = stream.Read<int>();
		ItemIdCount[] protoIdCount = stream.Read<ItemIdCount[]>();
		if(csp==null){
			Debug.LogError("processing not exist");
			return;
		}
		if(taskIndex<0 ||taskIndex>=csp.mTaskTable.Length)
		{
			Debug.LogError("index illegel");
			return;
		}
        if (csp.mTaskTable[taskIndex] == null)
        {
            csp.mTaskTable[taskIndex] = new ProcessingTask();
        }
        csp.mTaskTable[taskIndex].itemList = protoIdCount.ToList();
        csp.RemoveItemResult(taskIndex);
    }
    //--to do: addNpc
    void RPC_S2C_PRC_AddNpc(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {

    }
    //--to do: removeNpc
    void RPC_S2C_PRC_RemoveNpc(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {

    }

	void RPC_S2C_PRC_SetRound(uLink.BitStream stream, uLink.NetworkMessageInfo info){
		int taskIndex = stream.Read<int>();
		int count = stream.Read<int>();
		if (csp.mTaskTable[taskIndex] == null)
		{
			csp.mTaskTable[taskIndex] = new ProcessingTask();
		}
		csp.mTaskTable[taskIndex].SetRunCount( count);
		csp.SetRoundResult(taskIndex);
	}

    //--to do: setAuto
    void RPC_S2C_PRC_SetAuto(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
		bool isAuto = stream.Read<bool>();
		if(csp==null){
			Debug.LogError("processing not exist");
			return;
		}
        csp.IsAuto = isAuto;

    }
    //--to do: startTask
    void RPC_S2C_PRC_StartTask(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
		int index = stream.Read<int>();
		if(csp==null){
			Debug.LogError("processing not exist");
			return;
		}
		if(index<0 ||index>=csp.mTaskTable.Length)
		{
			Debug.LogError("index illegel");
			return;
		}
		if(csp.mTaskTable[index]==null)
			return;
        csp.StartProcessing(index);
        csp.StartResult(index);
    }
    //--to do: stopTask
    void RPC_S2C_PRC_StopTask(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
		int index = stream.Read<int>();
		if(csp==null){
			Debug.LogError("processing not exist");
			return;
		}
		if(index<0||index>=csp.mTaskTable.Length)
		{
			Debug.LogError("index illegel");
			return;
		}

        csp.Stop(index);
        csp.StopResult(index);

    }

 
    void RPC_S2C_PRC_GenResult(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        Vector3 pos = stream.Read<Vector3>();
        Quaternion rot = stream.Read<Quaternion>();
        int[] items = stream.Read<int[]>();
		if(PlayerNetwork.mainPlayer==null)
			return;
		if(TeamId== BaseNetwork.MainPlayer.TeamId)
			CSUtils.ShowTips(ProcessingConst.INFORM_FINISH_TO_RANDOMITEM);
		RandomItemMgr.Instance.GenProcessingItem(pos,rot,items);
//        RandomItemObj rio = new RandomItemObj(pos,items,rot);
//        RandomItemMgr.Instance.AddItemToManager(rio);
    }

	void RPC_S2C_PRC_FinishToStorage(uLink.BitStream stream, uLink.NetworkMessageInfo info){
		if(PlayerNetwork.mainPlayer==null)
			return;
		if(TeamId == BaseNetwork.MainPlayer.TeamId){
			CSUtils.ShowTips(ProcessingConst.INFORM_FINISH_TO_STORAGE);
		}
	}
	void RPC_S2C_PRC_SyncAllCounter(uLink.BitStream stream, uLink.NetworkMessageInfo info){
		if(csp==null){
			Debug.LogError("processing not exist");
			return;
		}
		List<float> curCounter = stream.Read<float[]>().ToList();
		List<float> finalCounter = stream.Read<float[]>().ToList();
		List<int> runCount = stream.Read<int[]>().ToList();
		for(int i=0;i<ProcessingConst.TASK_NUM;i++){
			if(curCounter[i]<0){
				csp.SyncStop(i);
				csp.StopResult(i);
			}else{
				csp.SetCounter(i,curCounter[i],finalCounter[i],runCount[i]);
			}
		}
	}
    #region send
    public void InitResultPos(Vector3[] resultTrans)
    {
        RPCServer(EPacketType.PT_CL_PRC_InitResultPos, resultTrans);
    }
    #endregion
}
