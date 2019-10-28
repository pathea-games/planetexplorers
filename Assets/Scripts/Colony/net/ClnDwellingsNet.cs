using UnityEngine;
using System.Collections;
using CSRecord;
public partial class ColonyNetwork 
{

    void RPC_S2C_InitDataDwellings(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSDwellingsData reocrdData = (CSDwellingsData)_ColonyObj._RecordData;
		reocrdData.m_CurDeleteTime = stream.Read <float>();
		reocrdData.m_CurRepairTime = stream.Read<float> ();
		reocrdData.m_DeleteTime = stream.Read<float> ();
		reocrdData.m_Durability = stream.Read<float> ();
		reocrdData.m_RepairTime = stream.Read<float> ();
		reocrdData.m_RepairValue = stream.Read<float> ();
	}

    void RPC_S2C_DWL_SyncNpc(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int[] npcs = stream.Read<int[]>();

        foreach (int npcId in npcs)
        {
            if (npcId != 0)
            {
                CSDwellings dwl = m_Entity as CSDwellings;
                CSPersonnel csNpc;
                //-- to do:
                CSMgCreator creator = MultiColonyManager.GetCreator(TeamId,false);
                if (creator != null)
                {
                    csNpc = creator.GetNpc(npcId);
					if(csNpc!=null && csNpc.Dwellings!=null){
                    	csNpc.Dwellings.RemoveNpc(csNpc);
					}
					if(csNpc!=null && dwl!=null){
                    	dwl.AddNpcs(csNpc);
					}
                }
            }
		}
    }
}

