using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSRecord;
using UnityEngine;

public partial class ColonyNetwork
{
    public CSMedicalTreat treatEntity
    {
        get { return m_Entity as CSMedicalTreat; }
    }
    void RPC_S2C_InitDataTreat(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        CSTreatData recordData = (CSTreatData)_ColonyObj._RecordData;
        recordData.m_CurDeleteTime = stream.Read<float>();
        recordData.m_CurRepairTime = stream.Read<float>();
        recordData.m_DeleteTime = stream.Read<float>();
        recordData.m_Durability = stream.Read<float>();
        recordData.m_RepairTime = stream.Read<float>();
        recordData.m_RepairValue = stream.Read<float>();

        recordData.m_ObjID = stream.Read<int>();
        recordData.npcIds = stream.Read<int[]>().ToList();
        recordData.m_CurTime = stream.Read<float>();
        recordData.m_Time = stream.Read<float>();
        recordData.isNpcReady = stream.Read<bool>();
        recordData.occupied = stream.Read<bool>();
    }


    void RPC_S2C_TRT_FindMachine(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        List<int> npcIds = stream.Read<int[]>().ToList();
		
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
        treatEntity.AddNpcResult(npcIds);
    }
    void RPC_S2C_TRT_SetTreat(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
		CSTreatment tInUse = stream.Read<CSTreatment>();
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
        treatEntity.SetTreat(tInUse);
    }
    
    void RPC_S2C_TRT_TryStart(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int npcId = stream.Read<int>();
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
		treatEntity.TryStartResult(npcId);
    }
	void RPC_S2C_TRT_StartTreatCounter(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
		treatEntity.StartCounterResult();
	}

    void RPC_S2C_TRT_SetItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int objId = stream.Read<int>();
		bool inorout = stream.Read<bool>();
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
        treatEntity.SetItemResult(objId, inorout);
    }
    void RPC_S2C_TRT_DeleteItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
		int instanceId = stream.Read<int>();
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
        treatEntity.DeleteMedicine(instanceId);
    }
    void RPC_S2C_TRT_RemoveDeadNpc(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int npcid = stream.Read<int>();
		
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
        treatEntity.RemoveDeadPatientResult(npcid);
    }
    
    void RPC_S2C_TRT_TreatFinish(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int npcId = stream.Read<int>();
        bool treatSuccess = stream.Read<bool>();
		
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
        treatEntity.TreatFinish(npcId, treatSuccess);

    }

	void RPC_S2C_TRT_ResetNpcToCheck(uLink.BitStream stream, uLink.NetworkMessageInfo info){
		int npcId = stream.Read<int>();
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
		treatEntity.ResetNpcToCheck(npcId);
	}
}

