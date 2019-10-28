using UnityEngine;
using System;
using System.Collections.Generic;
using RootMotion.FinalIK;

namespace PEIK
{
	[Serializable]
	public class IKHitReaction 
	{
		public float weightScale = 1f;

		public bool isRunning
		{
			get
			{
				foreach(HitPart part in m_HitParts)
					if(part.m_Effect.isRunning)
						return true;
				return false;
			}
		}

		[Serializable]
		public class HitPart
		{
			public string m_Name;
			public List<Transform> m_PartTrans;
			public IKAnimEffect m_Effect;
			public void Hit(IKSolverFullBodyBiped solver, Vector3 dir, float weight, float effectTime, float fadeTime = 0.1f)
			{
				m_Effect.m_StepTime = effectTime;
				m_Effect.m_FadeTime = fadeTime;
				m_Effect.DoEffect(solver, dir, weight);
			}

			public void OnModifyOffset(IKSolverFullBodyBiped solver, float weight, float deltaTime)
			{
				m_Effect.OnModifyOffset(solver, weight, deltaTime);
			}
		}

		public HitPart[] m_HitParts;


		public void OnHit(IKSolverFullBodyBiped solver, Transform trans, Vector3 dir, float weight, float effectTime)
		{
			foreach(HitPart part in m_HitParts)
				if(part.m_PartTrans.Contains(trans))
					part.Hit(solver, dir, weight * weightScale, effectTime);
		}

		public void OnModifyOffset(IKSolverFullBodyBiped solver, float weight, float deltaTime)
		{
			foreach(HitPart part in m_HitParts)
				part.OnModifyOffset(solver, weight * weightScale, deltaTime);
		}
	}
}
