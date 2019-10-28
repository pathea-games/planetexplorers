using UnityEngine;
using System.Collections;

public class VCEAddComponent : VCEModify
{
	public int m_Index;
	public VCComponentData m_Data;
	public VCEAddComponent (int index, VCComponentData data)
	{
		m_Index = index;
		m_Data = data.Copy();
	}
	public override void Undo ()
	{
		VCEditor.s_Scene.m_IsoData.m_Components[m_Index].DestroyEntity();
		VCEditor.s_Scene.m_IsoData.m_Components.RemoveAt(m_Index);
	}
	public override void Redo ()
	{
		VCComponentData data = m_Data.Copy();
		data.CreateEntity(true, null);
		VCEditor.s_Scene.m_IsoData.m_Components.Insert(m_Index, data);
		VCESelectComponent.s_LastCreate = data.m_Entity.GetComponent<VCEComponentTool>();
	}
}
