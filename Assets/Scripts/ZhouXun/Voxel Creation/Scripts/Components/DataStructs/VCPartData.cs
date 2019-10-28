using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public abstract class VCPartData : VCComponentData
{
	public override GameObject CreateEntity (bool for_editor, Transform parent)
	{
		if ( m_Entity != null )
			DestroyEntity();
		m_Entity = GameObject.Instantiate(VCConfig.s_Parts[m_ComponentId].m_ResObj) as GameObject;
		m_Entity.name = VCUtils.Capital(VCConfig.s_Parts[m_ComponentId].m_Name, true);
		if ( for_editor )
		{
			m_Entity.transform.parent = VCEditor.Instance.m_PartGroup.transform;
			VCEComponentTool tool = m_Entity.GetComponent<VCEComponentTool>();
			tool.m_IsBrush = false;
			tool.m_InEditor = true;
			tool.m_ToolGroup.SetActive(true);
			tool.m_SelBound.enabled = false;
			tool.m_SelBound.GetComponent<Collider>().enabled = false;
			tool.m_SelBound.m_BoundColor = GLComponentBound.s_Blue;
			tool.m_Data = this;
			Collider[] cs = m_Entity.GetComponentsInChildren<Collider>(true);
			foreach ( Collider c in cs )
			{
				if ( c.gameObject != tool.m_SelBound.gameObject )
					c.enabled = false;
			}
		}
		else
		{
			m_Entity.transform.parent = parent;
			Transform[] trs = m_Entity.GetComponentsInChildren<Transform>(true);
			foreach ( Transform t in trs )
				t.gameObject.layer = VCConfig.s_ProductLayer;
		}
		UpdateEntity(for_editor);
		if (!for_editor) UpdateComponent();
		return m_Entity;
	}
}
