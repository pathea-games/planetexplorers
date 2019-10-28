using UnityEngine;
using System.Collections;
using Pathea;

public class AnimatorTranReattacher : StateMachineBehaviour 
{
	[System.Serializable]
	public class ReattachReq
	{
		public string objName;
		public string targetBoneName;
		public float reattachTime;
		[HideInInspector]
		public Transform objTran;
		[HideInInspector]
		public bool active;
	}

	[SerializeField]
	ReattachReq[] m_Reqs;

	BiologyViewCmpt m_View;

	public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if(null == m_View)
			m_View = animator.GetComponentInParent<BiologyViewCmpt>();

		for(int i = 0; i < m_Reqs.Length; ++i)
		{
			ReattachReq req = m_Reqs[i];
			req.active = true;
			if(null == req.objTran)
				req.objTran = PETools.PEUtil.GetChild(animator.transform, req.objName);
		}
	}

	public override void OnStateExit (Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if(null == m_View)
			return;
		for(int i = 0; i < m_Reqs.Length; ++i)
		{
			ReattachReq req = m_Reqs[i];
			if(req.active)
				Reattach(req);
		}
	}

	public override void OnStateUpdate (Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if(null == m_View)
			return;

		for(int i = 0; i < m_Reqs.Length; ++i)
		{
			ReattachReq req = m_Reqs[i];
			if(req.active && stateInfo.normalizedTime > req.reattachTime)
				Reattach(req);
		}
	}

	void Reattach(ReattachReq req)
	{
		req.active = false;
		if(null == req.objTran)
			return;		
		m_View.Reattach(req.objTran.gameObject, req.targetBoneName);
	}
}
