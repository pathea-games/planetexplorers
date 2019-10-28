using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CSRecord;

using Pathea;
using Pathea.PeEntityExt;
using Pathea.Operate;
public class CSDwellings : CSCommon
{
    public override bool IsDoingJob()
    {
        return IsRunning;
    }


    public override GameObject gameLogic
    {
        get
        {
            return base.gameLogic;
        }
        set
        {
            base.gameLogic = value;
            if (gameLogic != null)
            {
                m_Beds = gameLogic.GetComponentInParent<PEBed>();

                if (m_Beds != null)
                {
                    for (int i = 0; i < m_NPCS.Length; i++)
                    {
                        if (m_NPCS[i]!=null)
                            m_NPCS[i].Bed = m_Beds;
                    }
                }
            }
        }
    }
    //public override GameObject gameObject 
    //{
    //    get {
    //        return base.gameObject;
    //    }
    //    set {
    //        base.gameObject = value;
    //        if (m_Object != null)
    //        {
    //            m_Beds = m_Object.GetComponentInParent<PEBed>();
    //        }
    //    }
    //}
	
	private CSDwellingsData m_DData;
	public  CSDwellingsData Data 	
	{ 
		get { 
			if (m_DData == null)
				m_DData = m_Data as CSDwellingsData;
			return m_DData; 
		} 
	}
	
	//  information
	public  CSDwellingsInfo m_DInfo;
	public  CSDwellingsInfo Info
	{
		get
		{
			if (m_DInfo == null)
				m_DInfo = m_Info as CSDwellingsInfo;
			return m_DInfo;
		}
	}
	
	// Living NPC
	public CSPersonnel[]	m_NPCS;
    public PEBed m_Beds;

	public CSDwellings ()
	{
		m_Type = CSConst.etDwelling;
		m_NPCS = new CSPersonnel[4];

		m_Grade = CSConst.egtHigh;
	}

	public bool HasSpace()
	{
		for (int i = 0; i < m_NPCS.Length; i++)
		{
			if (m_NPCS[i] == null)
				return true;
		}
		return false;
	}

	public int GetEmptySpace()
	{
		int cnt = 0;
		for (int i = 0; i < m_NPCS.Length; i++)
		{
			if (m_NPCS[i] == null)
				cnt++;
		}
		return cnt;
	}
	
	public bool AddNpcs (CSPersonnel npc)
	{	
		if (npc == null)
			return false;

		for (int i = 0; i < m_NPCS.Length; i++)
		{
			if (m_NPCS[i] == null)
			{
				m_NPCS[i] = npc;
				
                //if (m_Object != null)
                //{
                //    CSDwellingsObject csdo = m_Object.GetComponent<CSDwellingsObject>();
                //    if (csdo == null)
                //        continue;
                //    npc.RestTrans = csdo.m_BedTrans[i];
                //    npc.RestStandTrans = csdo.m_BedEdgeTrans[i];
                //}
                npc.Bed = m_Beds;


				npc.Dwellings = this;
				return true;
			}
		}
		
		return false;
	}
	
	public void RemoveNpc (CSPersonnel npc)
	{
		
		for (int i = 0; i < m_NPCS.Length; i ++)
		{
			if (m_NPCS[i] == npc)
			{
				m_NPCS[i] = null;
				break;
			}
		}
	}

	public void RemoveNpc(int index)
	{
		if (index >= m_NPCS.Length )
			return;

		m_NPCS[index] = null;
	}
	

	#region CSENTITY_FUNC
	
	public override void DestroySelf ()
	{
		base.DestroySelf();

		// Find other Dwellings
		Dictionary<int, CSCommon> commons = m_Creator.GetCommonEntities();
		List<CSDwellings> dwellings = new List<CSDwellings>();
		foreach (CSCommon csc in commons.Values)
		{
			if (csc != this && csc as CSDwellings != null)
				dwellings.Add(csc as CSDwellings);
		}

        // Set for other
        if (!PeGameMgr.IsMulti)
        {
            for (int i = 0; i < m_NPCS.Length; i++)
            {
                if (m_NPCS[i] != null)
                {
                    foreach (CSDwellings csd in dwellings)
                    {
                        if (csd.AddNpcs(m_NPCS[i]))
                        {
                            m_NPCS[i].ResetCmd();
                            m_NPCS[i] = null;
                        }
                    }
                }
            }

		    // Remove npc from CSMain
		    foreach (CSPersonnel npc in m_NPCS)
		    {
			    if (npc != null)
			    {
                    //--to do: wait Dismiss to RemoveNpc
                    m_Creator.RemoveNpc(npc.NPC);
                    npc.NPC.Dismiss();
			    }
		    }

        }
	}

	public override void ChangeState ()
	{
		base.ChangeState ();

        if(!PeGameMgr.IsMulti)
        {
            if (!m_IsRunning)
            {
                // Find other Dwellings to live
                CSMgCreator mgCreator = m_Creator as CSMgCreator;
                if (mgCreator != null)
                {
                    Dictionary<int, CSCommon> commons = mgCreator.GetCommonEntities();
                    int index = 0;
                    foreach (KeyValuePair<int, CSCommon> kvp in commons)
                    {
                        if (kvp.Value.m_Type != CSConst.etDwelling)
                            continue;

                        if (!kvp.Value.IsRunning)
                            continue;

                        CSDwellings dwellings = kvp.Value as CSDwellings;
                        for (; index < this.m_NPCS.Length; index++)
                        {
                            if (m_NPCS[index] != null)
                            {
                                if (dwellings.AddNpcs(m_NPCS[index]))
                                    RemoveNpc(index);
                                else
                                    break;
                            }
                        }

                        if (index == this.m_NPCS.Length)
                            break;
                    }
                }
            }
            else
            {
                CSMgCreator mgCreator = m_Creator as CSMgCreator;
                if (mgCreator != null)
                {
                    if (HasSpace())
                    {
                        Dictionary<int, CSCommon> commons = mgCreator.GetCommonEntities();
                        foreach (KeyValuePair<int, CSCommon> kvp in commons)
                        {
                            if (kvp.Value.m_Type != CSConst.etDwelling)
                                continue;

                            if (kvp.Value.IsRunning)
                                continue;

                            CSDwellings dwellings = kvp.Value as CSDwellings;

                            bool flag = false;
                            for (int i = 0; i < dwellings.m_NPCS.Length; i++)
                            {
                                if (dwellings.m_NPCS[i] == null)
                                    continue;

                                if (AddNpcs(dwellings.m_NPCS[i]))
                                    dwellings.RemoveNpc(i);
                                else
                                {
                                    flag = true;
                                    break;
                                }
                            }

                            if (flag)
                                break;
                        }

                    }
                }
            }
        }

		ExcuteEvent(CSConst.eetDwellings_ChangeState, m_IsRunning);
	}

	public override void CreateData ()
	{
        CSDefaultData ddata = null; 
        bool isNew;
        if (GameConfig.IsMultiMode)
        {
            isNew = MultiColonyManager.Instance.AssignData(ID, CSConst.dtDwelling, ref ddata, _ColonyObj);
        }
        else
        {
            isNew = m_Creator.m_DataInst.AssignData(ID, CSConst.dtDwelling, ref ddata);
        }
		m_Data = ddata as CSDwellingsData;
		
		if (isNew)
		{
			Data.m_Name = CSUtils.GetEntityName(m_Type);
			Data.m_Durability = Info.m_Durability;
		}
		else
		{
			StartRepairCounter(Data.m_CurRepairTime, Data.m_RepairTime, Data.m_RepairValue);
			StartDeleteCounter(Data.m_CurDeleteTime, Data.m_DeleteTime);
			
		}
	}
	
	public override void RemoveData ()
	{
		m_Creator.m_DataInst.RemoveObjectData(ID);
		
		if (m_CSRepair != null)
			GameObject.Destroy(m_CSRepair);
		if (m_CSDelete != null)
			GameObject.Destroy(m_CSDelete);
	}
	
	public override void Update ()
	{
		base.Update();
	}
	
	#endregion
}
