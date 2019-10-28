using UnityEngine;
using System.Collections;
using Pathea;

[RequireComponent(typeof(Animator))]
public class AnimatorCtrl : MonoBehaviour 
{
	Animator		m_Anim;
	AnimatorCmpt 	m_AnimCmpt;

	// Use this for initialization
	void Start () 
	{
		m_Anim = GetComponent<Animator>();
		PeEntity monoEntity = VCUtils.GetComponentOrOnParent<PeEntity>(gameObject);
		if(null != monoEntity)
			m_AnimCmpt = monoEntity.GetCmpt<AnimatorCmpt>();
	}
	
	public void AnimEvent(string para)
	{
		if(null != m_AnimCmpt)
			m_AnimCmpt.AnimEvent(para);
	}

	void OnAnimatorMove()
	{
		if(null != m_AnimCmpt)
		{
			m_AnimCmpt.m_LastRot = m_Anim.deltaRotation;
			m_AnimCmpt.m_LastMove = m_Anim.deltaPosition;
		}
	}
}
