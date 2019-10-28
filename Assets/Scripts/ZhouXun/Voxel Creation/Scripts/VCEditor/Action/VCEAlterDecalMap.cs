using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VCEAlterDecalMap : VCEModify
{
	public int m_Index;
	public ulong m_OldDecal;
	public ulong m_NewDecal;
	
	public VCEAlterDecalMap ( int index, ulong old_guid, ulong new_guid )
	{
		m_Index = index;
		m_OldDecal = old_guid;
		m_NewDecal = new_guid;
		s_AllModify.Add(this);
	}
	
	~VCEAlterDecalMap()
	{
		s_AllModify.Remove(this);
	}
	
	public override void Undo ()
	{
		VCDecalAsset old_dcl = VCEditor.s_Scene.m_IsoData.m_DecalAssets[m_Index];
		VCDecalAsset new_dcl = VCEAssetMgr.GetDecal(m_OldDecal);
		ulong old_guid = (old_dcl == null) ? (0) : (old_dcl.m_Guid);
		ulong new_guid = (new_dcl == null) ? (0) : (new_dcl.m_Guid);
		if ( old_guid != new_guid )
		{
			VCEditor.s_Scene.m_IsoData.m_DecalAssets[m_Index] = new_dcl;
		}
	}
	public override void Redo ()
	{
		VCDecalAsset old_dcl = VCEditor.s_Scene.m_IsoData.m_DecalAssets[m_Index];
		VCDecalAsset new_dcl = VCEAssetMgr.GetDecal(m_NewDecal);
		ulong old_guid = (old_dcl == null) ? (0) : (old_dcl.m_Guid);
		ulong new_guid = (new_dcl == null) ? (0) : (new_dcl.m_Guid);
		if ( old_guid != new_guid )
		{
			VCEditor.s_Scene.m_IsoData.m_DecalAssets[m_Index] = new_dcl;
		}
	}
	
	//
	// Static
	//
	private static List<VCEAlterDecalMap> s_AllModify = new List<VCEAlterDecalMap> ();
}
