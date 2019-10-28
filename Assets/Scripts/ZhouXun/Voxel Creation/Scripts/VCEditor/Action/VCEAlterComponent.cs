using UnityEngine;
using System.Collections;

public class VCEAlterComponent : VCEModify
{
	public int m_Index;
	public VCComponentData m_OldData;
	public VCComponentData m_NewData;
	public VCEAlterComponent (int index, VCComponentData old_data, VCComponentData new_data)
	{
		m_Index = index;
		m_OldData = old_data.Copy();
		m_NewData = new_data.Copy();
	}
	public override void Undo ()
	{
		VCComponentData data = VCEditor.s_Scene.m_IsoData.m_Components[m_Index];
		data.Import(m_OldData.Export());
		data.UpdateEntity(true);
	}
	public override void Redo ()
	{
		VCComponentData data = VCEditor.s_Scene.m_IsoData.m_Components[m_Index];
		data.Import(m_NewData.Export());
		data.UpdateEntity(true);
	}
}
