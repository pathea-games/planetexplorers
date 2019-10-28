using CSRecord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public partial class ColonyNetwork
{
    public CSTraining trainEntity
    {
        get { return m_Entity as CSTraining; }
    }
    void RPC_S2C_InitDataTrain(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        CSTrainData recordData = (CSTrainData)_ColonyObj._RecordData;

        recordData.m_CurDeleteTime = stream.Read<float>();
        recordData.m_CurRepairTime = stream.Read<float>();
        recordData.m_DeleteTime = stream.Read<float>();
        recordData.m_Durability = stream.Read<float>();
        recordData.m_RepairTime = stream.Read<float>();
        recordData.m_RepairValue = stream.Read<float>();

        byte[] dataArray = stream.Read<byte[]>();
        CSTraining.ParseData(dataArray, recordData);

    }
    void RPC_S2C_TRN_StartSkillTraining(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        List<int> skillIds = stream.Read<int[]>().ToList();
        int instructorId = stream.Read<int>();
        int traineeId = stream.Read<int>();
		
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
		trainEntity.OnStartSkillTrainingResult(skillIds,instructorId,traineeId);
    }
    void RPC_S2C_TRN_StartAttributeTraining(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int instructorId = stream.Read<int>();
        int traineeId = stream.Read<int>();
		
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
		trainEntity.OnTrainAttributeTrainingResult(instructorId,traineeId);
    }
    void RPC_S2C_TRN_SetInstructor(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
		int npcId = stream.Read<int>();
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
        CSPersonnel csp = trainEntity.m_MgCreator.GetNpc(npcId);
        trainEntity.AddInstructor(csp);
        trainEntity.SetCount();
    }
    void RPC_S2C_TRN_SetTrainee(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
		int npcId = stream.Read<int>();
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
        CSPersonnel csp = trainEntity.m_MgCreator.GetNpc(npcId);
        trainEntity.AddTrainee(csp);
        trainEntity.SetCount();
    }
    void RPC_S2C_TRN_SkillTrainFinish(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        List<int> learnedSkill = stream.Read<int[]>().ToList();
        int traineeId = stream.Read<int>();
		
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
		trainEntity.LearnSkillFinishResult(new Pathea.Ablities(learnedSkill),traineeId);
    }
    void RPC_S2C_TRN_AttributeTrainFinish(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int instructorId = stream.Read<int>();
        int traineeId = stream.Read<int>();
		int upgradeTimes = stream.Read<int>();
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
		trainEntity.AttributeFinish(instructorId,traineeId,upgradeTimes);
    }

	void RPC_S2C_TRN_StopTraining(uLink.BitStream stream, uLink.NetworkMessageInfo info){
		
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
		trainEntity.StopTrainingrResult();
	}

	void RPC_S2C_TRN_SyncCounter(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		float curTime = stream.Read<float>();
		float m_Time = stream.Read<float>();
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
		trainEntity.SetCounter(curTime, m_Time);
	}
}
