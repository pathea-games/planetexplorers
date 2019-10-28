using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSRecord;
using UnityEngine;

public partial class ColonyNetwork
{
    public CSMedicalTent tentEntity
    {
        get { return m_Entity as CSMedicalTent; }
    }

    void RPC_S2C_InitDataTent(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
         CSTentData recordData = (CSTentData)_ColonyObj._RecordData;

         recordData.m_CurDeleteTime = stream.Read<float>();
         recordData.m_CurRepairTime = stream.Read<float>();
         recordData.m_DeleteTime = stream.Read<float>();
         recordData.m_Durability = stream.Read<float>();
         recordData.m_RepairTime = stream.Read<float>();
         recordData.m_RepairValue = stream.Read<float>();

         byte[] dataArray = stream.Read<byte[]>();
         CSMedicalTent.ParseData(dataArray, recordData);

    }
    void RPC_S2C_TET_FindMachine(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        List<int> npcIds = stream.Read<int[]>().ToList();
        int npcId = stream.Read<int>();
		int index = stream.Read<int>();
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
        tentEntity.AddNpcResult(npcIds, npcId, index);
    }
    void RPC_S2C_TET_TryStart(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
		int npcId = stream.Read<int>();
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
        tentEntity.TryStartResult(npcId);
    }
    void RPC_S2C_TET_SetTent(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
		int npcId = stream.Read<int>();
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
        tentEntity.SetTent(npcId);
    }
    void RPC_S2C_TET_RemoveDeadNpc(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
		int npcId = stream.Read<int>();
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
        tentEntity.RemoveDeadPatientResult(npcId);
    }
    
    void RPC_S2C_TET_TentFinish(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
		int npcId = stream.Read<int>();
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
        tentEntity.TentFinish(npcId);
    }
}