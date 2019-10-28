using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// Voxel Creation Editor Component Tools
public class VCEComponentTool : MonoBehaviour
{
	public bool m_IsBrush = false;
	public bool m_InEditor = false;
	public GameObject m_ToolGroup = null;
	public GLComponentBound m_SelBound = null;
	public Transform m_DrawPivot = null;
	public Transform m_MassCenter = null;
	public int m_Phase = 0;
	public List<GameObject> m_ModelPhases;

	public Vector3 WorldMassCenter { get { return ( m_MassCenter != null ) ? m_MassCenter.position : transform.position; } }
	
	[NonSerialized] public VCComponentData m_Data;
	
	void Start ()
	{
		if (m_InEditor)
		{
			SetLayer();
			SetPhase();
		}
	}
	
	void Update ()
	{
		SetPhase();
		if ( !m_InEditor )
		{
			GameObject.Destroy(m_ToolGroup);
			MonoBehaviour.Destroy(this);
		}
	}
	
	public void SetPivotPos(Vector3 vec)
	{
		Vector3 now_pos = m_DrawPivot.position;
		Vector3 this_pos = this.transform.position;
		Vector3 move = vec - now_pos;
		Vector3 this_new_pos = this_pos + move;
		this.transform.position = this_new_pos;
	}
	
	public void SetLayer()
	{
		Transform[] trans = GetComponentsInChildren<Transform>(true);
		foreach ( Transform t in trans )
		{
			t.gameObject.layer = m_InEditor ? VCConfig.s_EditorLayer : VCConfig.s_ProductLayer;
		}
	}
	
	public void SetPhase()
	{
		if ( m_ModelPhases.Count > 0 )
		{
			GameObject object_to_show = null;
			object_to_show = m_ModelPhases[m_Phase % m_ModelPhases.Count];
			foreach ( GameObject go in m_ModelPhases )
			{
				if ( go != null )
				{
					go.SetActive(go == object_to_show);
					if ( !m_InEditor && go != object_to_show )
						GameObject.Destroy(go);
				}
			}
		}		
	}
	public void SetPhase(int phase)
	{
		m_Phase = phase;
		SetPhase();
	}
}
