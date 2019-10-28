using UnityEngine;
using System.Collections;
using Pathea;

public class PEDigTool : PEAimAbleEquip 
{
	public int 		m_SkillID;
	public string 	m_AnimName = "Spade";
	public float 	m_AnimSpeed = 1f;
	public float[] 	m_AnimDownThreshold = new float[2]{40f, 20f};
	public float 	m_StaminaCost = 5f;
	public DigVoxelIndicator m_Indicator;
	public PEActionMask m_DigMask = PEActionMask.Dig;

	protected virtual void Update()
	{
		if(null != m_Indicator && null != m_Entity)
			m_Indicator.m_Radius = m_Entity.GetAttribute(Pathea.AttribType.ResRange);
	}
}
