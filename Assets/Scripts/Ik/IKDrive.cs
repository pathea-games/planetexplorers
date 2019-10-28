using UnityEngine;
using System.Collections;

public class IKDrive : MonoBehaviour 
{
	[HideInInspector]
	public Transform m_LHand;
	[HideInInspector]
	public Transform m_RHand;
	[HideInInspector]
	public float HandFoward = 0.07f;
	[HideInInspector]
	public float HandUp = 0.03f;
	
	Animator m_Anim;

	public bool active { get; set; }
	
	// Use this for initialization
	void Awake () {
		m_Anim = GetComponent<Animator>();
		active = false;
	}
	
	void OnAnimatorIK()
	{
		if(null != m_Anim)
		{
			m_Anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, active ? 1f : 0f);
			m_Anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, active ? 1f : 0f);
			m_Anim.SetIKPositionWeight(AvatarIKGoal.RightHand, active ? 1f : 0f);
			m_Anim.SetIKRotationWeight(AvatarIKGoal.RightHand, active ? 1f : 0f);
			if(active)
			{
				if(null != m_LHand)
				{
					m_Anim.SetIKPosition(AvatarIKGoal.LeftHand, m_LHand.position - m_LHand.transform.forward * HandFoward + m_LHand.transform.up * HandUp);
					m_Anim.SetIKRotation(AvatarIKGoal.LeftHand, m_LHand.transform.rotation);
				}
				
				if(null != m_RHand)
				{
					m_Anim.SetIKPosition(AvatarIKGoal.RightHand, m_RHand.position - m_RHand.transform.forward * HandFoward + m_RHand.transform.up * HandUp);
					m_Anim.SetIKRotation(AvatarIKGoal.RightHand, m_RHand.transform.rotation);
				}
			}
		}
	}
}
