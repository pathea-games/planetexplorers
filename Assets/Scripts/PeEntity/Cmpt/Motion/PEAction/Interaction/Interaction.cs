using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;
using Pathea;

namespace PEIK
{
	public abstract class Interaction
	{
		protected InteractionSystem m_CasterInteractionSystem;
		protected InteractionSystem m_TargetInteractionSystem;
		
		protected InteractionObject m_CasterInteractionObj;
		protected InteractionObject m_TargetInteractionObj;

		protected abstract string casterObjName{ get; }
		protected abstract string targetObjName{ get; }

		protected abstract FullBodyBipedEffector[] casterEffectors { get; }
		protected abstract FullBodyBipedEffector[] targetEffectors { get; }

		public void Init(Transform casterRoot, Transform targetRoot)
		{
			m_CasterInteractionSystem = casterRoot.GetComponentInChildren<InteractionSystem>();
			m_TargetInteractionSystem = targetRoot.GetComponentInChildren<InteractionSystem>();

			if(!string.IsNullOrEmpty(casterObjName))
			{
				InteractionObject[] objs = casterRoot.GetComponentsInChildren<InteractionObject>(true);
				for(int i = 0; i < objs.Length; i++)
				{
					if(objs[i].name == casterObjName)
					{
						m_CasterInteractionObj = objs[i];
						break;
					}
				}
			}
			
			if(!string.IsNullOrEmpty(targetObjName))
			{
				InteractionObject[] objs = targetRoot.GetComponentsInChildren<InteractionObject>(true);
				for(int i = 0; i < objs.Length; i++)
				{
					if(objs[i].name == targetObjName)
					{
						m_TargetInteractionObj = objs[i];
						break;
					}
				}
			}
		}
		public void StartInteraction()
		{
			DoStart();
		}

		public void StartInteraction(MonoBehaviour mono, float delayTime)
		{
			mono.Invoke("DoStart", delayTime);
		}
		
		void DoStart()
		{
			if(null != m_CasterInteractionSystem && null != m_TargetInteractionObj)
			{
				for(int i = 0; i < casterEffectors.Length; i++)
					m_CasterInteractionSystem.StartInteraction(casterEffectors[i], m_TargetInteractionObj, false);
			}

			if(null != m_TargetInteractionSystem && null != m_CasterInteractionObj)
			{
				for(int i = 0; i < targetEffectors.Length; i++)
					m_TargetInteractionSystem.StartInteraction(targetEffectors[i], m_CasterInteractionObj, false);
			}
		}
		
		public void EndInteraction(bool immediately = false)
		{
			if(null != m_CasterInteractionSystem)
			{
				for(int i = 0; i < casterEffectors.Length; i++)
				{
					if(immediately || !m_CasterInteractionSystem.IsPaused(casterEffectors[i]))
						m_CasterInteractionSystem.StopInteraction(casterEffectors[i]);
					else
						m_CasterInteractionSystem.ResumeInteraction(casterEffectors[i]);
				}
			}
			
			if(null != m_TargetInteractionSystem)
			{
				for(int i = 0; i < targetEffectors.Length; i++)
				{
					if(immediately)
						m_TargetInteractionSystem.StopInteraction(targetEffectors[i]);
					else
						m_TargetInteractionSystem.ResumeInteraction(targetEffectors[i]);
				}
			}
		}
	}
}
