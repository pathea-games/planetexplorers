using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSRecord;
using UnityEngine;

public partial class ColonyNetwork
{
    public CSMedicalCheck checkEntity
    {
        get { return m_Entity as CSMedicalCheck; }
    }
    void RPC_S2C_InitDataCheck(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        CSCheckData recordData = (CSCheckData)_ColonyObj._RecordData;
        recordData.m_CurDeleteTime = stream.Read<float>();
        recordData.m_CurRepairTime = stream.Read<float>();
        recordData.m_DeleteTime = stream.Read<float>();
        recordData.m_Durability = stream.Read<float>();
        recordData.m_RepairTime = stream.Read<float>();
        recordData.m_RepairValue = stream.Read<float>();

        recordData.npcIds = stream.Read<int[]>().ToList();
        recordData.m_CurTime = stream.Read<float>();
        recordData.m_Time = stream.Read<float>();
        recordData.isNpcReady = stream.Read<bool>();
        recordData.occupied = stream.Read<bool>();
    }


    void RPC_S2C_CHK_FindMachine(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        List<int> npcIds = stream.Read<int[]>().ToList();
		
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
        checkEntity.AddNpcResult(npcIds);
    }
    void RPC_S2C_CHK_SetDiagnose(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
        checkEntity.SetDiagnose();
    }
    
    void RPC_S2C_CHK_TryStart(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
		int npcId = stream.Read<int>();
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}

        checkEntity.TryStartResult(npcId);
    }

    void RPC_S2C_CHK_RemoveDeadNpc(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
		int npcid = stream.Read<int>();
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
        checkEntity.RemoveDeadPatientResult(npcid);
    }
    
    void RPC_S2C_CHK_CheckFinish(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int npcid = stream.Read<int>();
        List<CSTreatment> treatmentList = stream.Read<CSTreatment[]>().ToList();
		
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
        checkEntity.CheckFinish(npcid,treatmentList);
    }

}
