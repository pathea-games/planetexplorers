using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace PEIK
{
	public class IKHumanMgr : MonoBehaviour 
	{
		public AimIK		m_AimIK;
		public FullBodyBipedIK m_FBBIK;

		public IKHumanMove		m_HumanMove;
		public IKHumanInertia	m_HumanInertia;


		// Use this for initialization
		void Start () 
		{
			if(null != m_AimIK)
				m_AimIK.Disable();
			if(null != m_FBBIK)
				m_FBBIK.Disable();

		}
		
		// Update is called once per frame
		void LateUpdate () 
		{
			if(null != m_FBBIK)
				m_FBBIK.solver.Update();
			if(null != m_AimIK)
				m_AimIK.solver.Update();
		}
	}
}
