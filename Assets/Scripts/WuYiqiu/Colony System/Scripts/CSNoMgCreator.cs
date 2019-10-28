using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using CSRecord;

using Pathea;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtTrans;
using Pathea.PeEntityExtMotion_Move;
using Pathea.PeEntityExtFollow;

public class CSNoMgCreator : CSCreator 
{
	#region  ABOUT_ENTITY

	private Dictionary<int, CSCommon>	m_CommonEntities;

	// <CETC> Create Non-Managed Entity
	public override int CreateEntity (CSEntityAttr attr, out CSEntity outEnti)
	{
		outEnti = null;

		if (attr.m_Type == CSConst.etAssembly)
		{
			Debug.LogWarning("Non-Managed Creator cant create the Assembly Entity.");
			return CSConst.rrtUnkown;
		}

		if (m_CommonEntities.ContainsKey(attr.m_InstanceId))
		{
			outEnti = m_CommonEntities[attr.m_InstanceId];
			outEnti.gameObject = attr.m_Obj;
			outEnti.Position = attr.m_Pos;
			outEnti.ItemID = attr.m_protoId;
			outEnti.BaseData.m_Alive = true;
			return CSConst.rrtSucceed;
		}

		CSCommon csc = null;
		switch (attr.m_Type)
		{
		case CSConst.etStorage:
			csc = new CSStorage();
			CSStorage css = csc as CSStorage;
			css.m_Info = CSInfoMgr.m_StorageInfo;
			css.m_Creator = this;
			css.m_Power = attr.m_Power;
            css.m_Package.ExtendPackage(CSInfoMgr.m_StorageInfo.m_MaxItem, CSInfoMgr.m_StorageInfo.m_MaxEquip, CSInfoMgr.m_StorageInfo.m_MaxRecource, CSInfoMgr.m_StorageInfo.m_MaxArmor);
			break;
		case CSConst.etEnhance:
			csc = new CSEnhance();
			CSEnhance csen = csc as CSEnhance;
			csen.m_Creator = this;
			csen.m_Power   = attr.m_Power;
			csen.m_Info	= CSInfoMgr.m_EnhanceInfo;
			break;
		case CSConst.etRepair:
			csc = new CSRepair();
			CSRepair csr  = csc as CSRepair;
			csr.m_Creator = this;
			csr.m_Power   = attr.m_Power;
			csr.m_Info = CSInfoMgr.m_RepairInfo;
			break;
		case CSConst.etRecyle:
			csc = new CSRecycle();
			CSRecycle csrc = csc as CSRecycle;
			csrc.m_Creator = this;
			csrc.m_Power   = attr.m_Power;
			csrc.m_Info	= CSInfoMgr.m_RecycleInfo;
			break;
		case CSConst.etDwelling:
			csc = new CSDwellings();
			CSDwellings csd = csc as CSDwellings;
			csd.m_Creator = this;
			csd.m_Power   = attr.m_Power;;
			csd.m_Info = CSInfoMgr.m_DwellingsInfo;
			break;
		case CSConst.etppCoal:
			csc = new CSPPCoal();
			CSPPCoal cscppc = csc as CSPPCoal;
			cscppc.m_Creator = this;
			cscppc.m_Power = 10000;
			cscppc.m_RestPower = 10000;
			cscppc.m_Info = CSInfoMgr.m_ppCoal;
			break;
		case CSConst.etppSolar:
			csc = new CSPPSolar();
			CSPPSolar cspps = csc as CSPPSolar;
			cspps.m_Creator = this;
			cspps.m_Power = 10000;
			cspps.m_RestPower = 10000;
			cspps.m_Info = CSInfoMgr.m_ppCoal;
			break;
		case CSConst.etFactory:
			csc = new CSFactory();
			csc.m_Creator = this;
			csc.m_Info = CSInfoMgr.m_FactoryInfo;
			break;
		default:
			break;
		}
		 
		csc.ID = attr.m_InstanceId;
		csc.CreateData();
		csc.gameObject = attr.m_Obj;
		csc.Position = attr.m_Pos;
		csc.ItemID	 = attr.m_protoId;

		outEnti = csc;
		m_CommonEntities.Add(attr.m_InstanceId, csc);
		return CSConst.rrtSucceed;
	}

	public override CSEntity RemoveEntity (int id, bool bRemoveData = true)
	{
		CSEntity cse = null;

		if ( m_CommonEntities.ContainsKey(id) )
		{
			cse = m_CommonEntities[id];
			cse.BaseData.m_Alive = false;
			if (bRemoveData)
				m_CommonEntities[id].RemoveData();
			m_CommonEntities.Remove(id);

			ExecuteEvent(CSConst.cetRemoveEntity, cse);
		}
		else
			Debug.LogWarning("The Common Entity that you want to Remove is not contained!");

		return cse;
	}

	public override CSCommon GetCommonEntity (int ID)
	{
		if (m_CommonEntities.ContainsKey(ID))
			return m_CommonEntities[ID];
		
		return null;
	}

	public override int GetCommonEntityCnt ()
	{
		return m_CommonEntities.Count;
	}

	public override Dictionary<int, CSCommon> GetCommonEntities ()
	{
		return m_CommonEntities;
	}

	public override int CanCreate (int type, Vector3 pos)
	{
		if (type == CSConst.etAssembly)
		{
			return CSConst.rrtUnkown;
		}
	
		return CSConst.rrtSucceed;
	}
	#endregion

	#region ABOUT_NPC

	public override bool AddNpc (PeEntity npc, bool bSetPos = false)
	{
		return false;
	}

	public override void RemoveNpc (PeEntity npc)
	{

	}

	public override CSPersonnel[] GetNpcs ()
	{
		return null;
	}

    public override CSPersonnel GetNpc(int id)
    {
        return null;
    }
	#endregion

	void Awake ()
	{
		m_CommonEntities = new Dictionary<int, CSCommon>();
	}

	void Start () 
	{
		// Create colony object if has  
		Dictionary<int, CSDefaultData> objRecords = m_DataInst.GetObjectRecords();
		foreach (CSDefaultData defData in objRecords.Values)
		{
			CSObjectData objData = defData as CSObjectData;

			if (objData != null)
			{
				if (!objData.m_Alive)
					continue;
				CSEntityAttr attr = new CSEntityAttr();
				attr.m_InstanceId = objData.ID;
				attr.m_Type = objData.dType;
				attr.m_Pos  = objData.m_Position;
				attr.m_protoId = objData.ItemID;
				CSEntity cse = null;
				CreateEntity(attr, out cse);
			}
		}
	}

	// Update is called once per frame
	void Update () 
	{
		foreach (KeyValuePair<int, CSCommon> kvp in m_CommonEntities)
			kvp.Value.Update();
	}
}
