using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VCEAlterMaterialMap : VCEModify
{
	public int m_Index;
	public ulong m_OldMat;
	public ulong m_NewMat;
	
	public VCEAlterMaterialMap ( int index, ulong old_guid, ulong new_guid )
	{
		m_Index = index;
		m_OldMat = old_guid;
		m_NewMat = new_guid;
		s_AllModify.Add(this);
	}
	
	~VCEAlterMaterialMap()
	{
		s_AllModify.Remove(this);
	}
	
	public override void Undo ()
	{
		VCMaterial old_mat = VCEditor.s_Scene.m_IsoData.m_Materials[m_Index];
		VCMaterial new_mat = VCEAssetMgr.GetMaterial(m_OldMat);
		ulong old_guid = (old_mat == null) ? (0) : (old_mat.m_Guid);
		ulong new_guid = (new_mat == null) ? (0) : (new_mat.m_Guid);
		if ( old_guid != new_guid )
		{
			VCEditor.s_Scene.m_IsoData.m_Materials[m_Index] = new_mat;
			VCEditor.s_Scene.GenerateIsoMat();
		}
	}
	public override void Redo ()
	{
		VCMaterial old_mat = VCEditor.s_Scene.m_IsoData.m_Materials[m_Index];
		VCMaterial new_mat = VCEAssetMgr.GetMaterial(m_NewMat);
		ulong old_guid = (old_mat == null) ? (0) : (old_mat.m_Guid);
		ulong new_guid = (new_mat == null) ? (0) : (new_mat.m_Guid);
		if ( old_guid != new_guid )
		{
			VCEditor.s_Scene.m_IsoData.m_Materials[m_Index] = new_mat;
			VCEditor.s_Scene.GenerateIsoMat();
		}
	}
	
	//
	// Static
	//
	private static List<VCEAlterMaterialMap> s_AllModify = new List<VCEAlterMaterialMap> ();
	public static void MatChange (ulong old_guid, ulong new_guid)
	{
		foreach (VCEAlterMaterialMap modify in s_AllModify)
		{
			if ( modify.m_OldMat == old_guid )
				modify.m_OldMat = new_guid;
			if ( modify.m_NewMat == old_guid )
				modify.m_NewMat = new_guid;
		}
	}
}
