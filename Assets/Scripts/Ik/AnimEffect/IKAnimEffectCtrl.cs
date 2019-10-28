using UnityEngine;
using System;
using RootMotion.FinalIK;

namespace PEIK
{
	public class IKAnimEffectCtrl : IKOffsetModifier
	{
		public IKMoveEffect		m_MoveEffect;
		public IKHitReaction	m_HitReaction;

#if UNITY_EDITOR
		void OnValidate()
		{
			if(null == transform.parent) return;
			if(Application.isPlaying) return;
			PEModelController mc = transform.parent.GetComponentInChildren<PEModelController>();
			if(null == mc) return;

			if(m_HitReaction==null) return;
			if(m_HitReaction.m_HitParts==null)return ;

			UnityEditor.Undo.RecordObject(this, "xxx");

			Pathea.PEDefenceTrigger defenceTrigger = transform.parent.GetComponentInChildren<Pathea.PEDefenceTrigger>();

			foreach(var hitpart in m_HitReaction.m_HitParts)
			{
				if(null != defenceTrigger && "" != hitpart.m_Name)
				{
					if(null == hitpart.m_PartTrans)
						hitpart.m_PartTrans = new System.Collections.Generic.List<Transform>();
					hitpart.m_PartTrans.Clear();
					for(int i = 0; i < defenceTrigger.defenceParts.Length; i++)
					{
						int count1 = hitpart.m_Name.Split('_').Length;
						int count2 = defenceTrigger.defenceParts[i].name.Split('_').Length;
						if(count1 == count2 && defenceTrigger.defenceParts[i].name.Contains(hitpart.m_Name)
						   && null != defenceTrigger.defenceParts[i].capsule.trans)
						{
							if(!hitpart.m_PartTrans.Contains(defenceTrigger.defenceParts[i].capsule.trans))
								hitpart.m_PartTrans.Add(defenceTrigger.defenceParts[i].capsule.trans);
						}
					}
				}
			}

			UnityEditor.EditorUtility.SetDirty(this);
		}
#endif

		public void StartMove(Vector3 velocity)
		{
			if(null != m_FBBIK)
				m_MoveEffect.StartMove(m_FBBIK.solver, velocity);
		}

		public void StopMove(Vector3 velocity)
		{
			if(null != m_FBBIK)
				m_MoveEffect.StopMove(m_FBBIK.solver, velocity);
		}

		public void StopMove(Vector3 velocity, bool rightFoot)
		{
			if(null != m_FBBIK)
				m_MoveEffect.StopMove(m_FBBIK.solver, velocity, rightFoot);
		}

		public void EndMoveEffect()
		{
			m_MoveEffect.EndEffect();
		}

		public bool moveEffectRunning{ get { return isActiveAndEnabled && m_Weight >0 && m_MoveEffect.isRunning; } }

		public void OnHit(Transform trans, Vector3 dir, float weight, float effectTime)
		{
			if(null != m_FBBIK)
				m_HitReaction.OnHit(m_FBBIK.solver, trans, dir, weight, effectTime);
		}

		#region implemented abstract members of IKOffsetModifier

		protected override void OnInit ()
		{
		}
		
		protected override void OnModifyOffset ()
		{
			if(null != m_FBBIK)
			{
				m_MoveEffect.OnModifyOffset(m_FBBIK.solver, m_Weight, deltaTime);
				m_HitReaction.OnModifyOffset(m_FBBIK.solver, m_Weight, deltaTime);
			}
		}

		#endregion

	}
}