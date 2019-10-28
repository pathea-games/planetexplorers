using UnityEngine;
using System.Collections;
using CSRecord;
using ItemAsset;
public partial class ColonyNetwork 
{
    void RPC_S2C_InitDataPPCoal(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSPPCoalData recordData = (CSPPCoalData)_ColonyObj._RecordData;
		recordData.m_CurDeleteTime = stream.Read <float>();
		recordData.m_CurRepairTime = stream.Read<float> ();
		recordData.m_DeleteTime = stream.Read<float> ();
		recordData.m_Durability = stream.Read<float> ();
		recordData.m_RepairTime = stream.Read<float> ();
		recordData.m_RepairValue = stream.Read<float> ();
		int [] keys = stream.Read<int[]> ();
		int [] values = stream.Read<int[]> ();
		
		for(int i = 0; i < keys.Length; i++)
		{
			recordData.m_ChargingItems[keys[i]] = values[i];
		}
		recordData.m_WorkedTime = stream.Read<float> ();
		recordData.m_CurWorkedTime = stream.Read<float> ();
	}

    void RPC_S2C_PPC_WorkedTime(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSPPCoalData reocrdData = (CSPPCoalData)_ColonyObj._RecordData;
		reocrdData.m_CurWorkedTime = stream.Read <float>();
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
        CSPPCoal m_CSPPCoal = m_Entity as CSPPCoal;
		m_CSPPCoal.StartWorkingCounter(reocrdData.m_CurWorkedTime);
        if (CSUI_MainWndCtrl.Instance.PPCoalUI != null)
        {
            CSUI_MainWndCtrl.Instance.PPCoalUI.AddFuelSuccess(m_CSPPCoal);
        }
    }
	public void PPC_AddFuel()
	{
        RPCServer(EPacketType.PT_CL_PPC_AddFuel);
	}
	public void PPC_QueryCurWorkedTime()
	{
        RPCServer(EPacketType.PT_CL_PPC_WorkedTime);
    }

    public void PPC_ShowElectric(bool active)
    {
        //RPCServer(EPacketType.PT_CL_PPC_ShowElectric, active);
    }

    void RPC_S2C_PPC_NoPower(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
        ((CSPPCoal)m_Entity).StopCounter();
        ((CSPPCoal)m_Entity).OnWorked();
	}


    void RPC_S2C_PPC_AddFuel(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		/*bool success = */stream.Read<bool> ();
	}

}
