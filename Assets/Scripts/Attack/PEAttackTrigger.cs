using UnityEngine;
using System;
using System.Collections.Generic;
using SkillSystem;

namespace Pathea
{
	public enum AttackForm
	{
		SharpMetal = 0,
		SharpNonMetal,
		Blunt_Metal,
		Blunt_NonMetal,
		Bullet,
		Poison,
		Fire,
		Explosion,
		Energy_ColdWeapon,
		Energy_Laser,
		Energy_Particle
	}

	public class PEAttackTrigger : MonoBehaviour
	{
		public static System.Action<SkEntity> onHitSkEntity;

		[System.Serializable]
		public class PEAttackPart 
		{
			public AttackForm attackForm;
			public PECapsuleTrigger capsule;

			public void Init()
			{
				capsule.ResetInfo();
			}

			public void Update(Vector3 centerPos)
			{
				capsule.Update(centerPos);
			}
		}
		
		public LayerMask attackLayer;
		public PEAttackPart[] attackParts;

		public event Action<PEDefenceTrigger, PECapsuleHitResult> onHitTrigger;

		List<PEDefenceTrigger> m_HitTriggers = new List<PEDefenceTrigger>();
		List<PEDefenceTrigger> m_EffectedTriggers = new List<PEDefenceTrigger>();
		List<SkEntity> m_HitEntitys = new List<SkEntity>();
		bool m_Active = true;

		SkEntity m_SkEntity;
		PeEntity m_PEEntity;
		
		public bool active
		{
			get { return m_Active; }
			set
			{
				if(m_Active == value) return;
				m_Active = value;
				Collider col = GetComponent<Collider>();
				if(null != col)	col.enabled = m_Active;
				UpdatePartsInfo();

#if UNITY_EDITOR
				if(showDebugInfo && m_Active)
					Debug.LogError("AttackTrigger: active");
					
				for(int i = 0; i < attackParts.Length; i++)
					attackParts[i].capsule.TestHit = m_Active;
#endif
			} 
		}

		public void ClearHitInfo()
		{
			m_HitTriggers.Clear();
			m_EffectedTriggers.Clear();
			m_HitEntitys.Clear();
		}

		public void ResetHitInfo()
		{
			for(int i = 0; i < m_EffectedTriggers.Count; i++)
				m_HitTriggers.Add(m_EffectedTriggers[i]);
			m_EffectedTriggers.Clear();
			m_HitEntitys.Clear();
		}
		
		void Reset()
		{
			attackLayer = LayerMask.GetMask("Damage", "GIEProductLayer");
		}

		void Start()
		{
			for(int i = 0; i < attackParts.Length; i++)
				attackParts[i].Init();
			m_SkEntity = GetComponentInParent<SkEntity>();
			if(null != m_SkEntity)
				m_PEEntity = m_SkEntity.GetComponent<PeEntity>();
			active = false;
		}

		// Update is called once per frame
		void LateUpdate ()
		{
			try
			{
				UpdatePartsInfo();

				UpdateHitState();
			}
			catch (Exception e)
			{
				if(null != m_PEEntity.animCmpt && null != m_PEEntity.animCmpt.animator)
				{
					Animator anim = m_PEEntity.animCmpt.animator;
					string playingAnim = "PlayingAnim:";

					List<AnimatorClipInfo> animClips = new List<AnimatorClipInfo>();
					for(int i = 0; i < anim.layerCount; ++i)
						animClips.AddRange(anim.GetCurrentAnimatorClipInfo(i));
					for(int i = 0; i < animClips.Count; ++i)
						playingAnim = playingAnim + animClips[i].clip.name + "\n";
					Debug.LogError(playingAnim + e.ToString());
				}
			}
		}

		void OnTriggerEnter(Collider other)
		{
			if(0 != (attackLayer.value & (1<<other.gameObject.layer)))
			{
				SkEntity hitEntity = other.gameObject.GetComponentInParent<SkEntity>();

				if(null == hitEntity || hitEntity == m_SkEntity)
					return;

				PEDefenceTrigger hitTrigger = other.GetComponent<PEDefenceTrigger>();
				if(null != hitTrigger)
				{
					if(!m_HitTriggers.Contains(hitTrigger)
					   && !m_EffectedTriggers.Contains(hitTrigger))
					{
						if(CheckHit(hitTrigger))
							m_EffectedTriggers.Add(hitTrigger);
						else
							m_HitTriggers.Add(hitTrigger);
					}
				}
				else
				{
					OnHitCollider(other);
				}
			}
		}
		
		void OnTriggerExit(Collider other)
		{
			if(0 != (attackLayer.value & (1<<other.gameObject.layer)))
			{
				PEDefenceTrigger hitTrigger = other.GetComponent<PEDefenceTrigger>();
				if(null != hitTrigger)
					m_HitTriggers.Remove(hitTrigger);
			}
		}

		void UpdatePartsInfo()
		{
			if(!m_Active) return;
			Vector3 center = null != m_PEEntity ? m_PEEntity.centerPos : Vector3.zero;
			for(int i = 0; i < attackParts.Length; i++)
				attackParts[i].Update(center);
		}

		void UpdateHitState()
		{
			if(active)
			{
				for(int i = m_HitTriggers.Count - 1; i >= 0; i--)
				{
					if(null == m_HitTriggers[i])
					{
						m_HitTriggers.RemoveAt(i);
					}
					else if(CheckHit(m_HitTriggers[i]))
					{
						m_EffectedTriggers.Add(m_HitTriggers[i]);
						m_HitTriggers.RemoveAt(i);
					}
				}
			}
		}

		bool CheckHit(PEDefenceTrigger defenceTrigger)
		{
			defenceTrigger.UpdateInfo();
			PECapsuleHitResult mostCross = null;
			for(int i = 0; i < attackParts.Length; i++)
			{
				if(attackParts[i].capsule.enable)
				{
					for(int j = 0; j < defenceTrigger.defenceParts.Length; j++)
					{
						if(defenceTrigger.defenceParts[j].capsule.enable)
						{
							PECapsuleHitResult result;
							if(attackParts[i].capsule.CheckCollision(defenceTrigger.defenceParts[j].capsule, out result))
							{
								result.selfAttackForm = attackParts[i].attackForm;
								result.hitDefenceType = defenceTrigger.defenceParts[j].defenceType;
								result.damageScale = defenceTrigger.defenceParts[j].damageScale;
								if(null == mostCross || mostCross.distance < result.distance)
									mostCross = result;
							}
						}
					}
				}
			}

			if(null != mostCross)
			{
				OnHit(defenceTrigger, mostCross);
				return true;
			}

			return false;
		}

		void OnHitCollider(Collider other)
		{
			if(null == m_SkEntity)
				return;
			
			if(null != m_PEEntity && m_PEEntity.IsDeath())
				return;

			SkEntity hitEntity = other.gameObject.GetComponentInParent<SkEntity>();

			bool isTerrain = PETools.PEUtil.IsVoxelOrBlock45(hitEntity);

			if(null == hitEntity || m_HitEntitys.Contains(hitEntity) || (!isTerrain && !PETools.PEUtil.CanDamage(m_SkEntity, hitEntity)))
				return;
			m_HitEntitys.Add(hitEntity);

			PECapsuleHitResult result = null;
			bool getpos = false;
			float minDis = 100f;
			
			PECapsuleHitResult findResult;
			
			for(int i = 0; i < attackParts.Length; i++)
			{
				if(!attackParts[i].capsule.enable)
					continue;
				if(attackParts[i].capsule.GetClosestPos(other.transform.position, out findResult))
				{
					result = findResult;
					result.selfAttackForm = attackParts[i].attackForm;
					result.hitTrans = other.transform;
					result.damageScale = 1f;
					return;
				}
				else if(findResult.distance < minDis)
				{
					minDis = findResult.distance;
					result = findResult;
					result.hitTrans = other.transform;
					result.selfAttackForm = attackParts[i].attackForm;
					result.damageScale = 1f;
					getpos = true;
				}
			}

			if(getpos)
			{
				m_SkEntity.CollisionCheck(result);
				
				if(null != onHitSkEntity)
					onHitSkEntity(hitEntity);
			}
		}

		void OnHit(PEDefenceTrigger defenceTrigger, PECapsuleHitResult result)
		{
			if(null != m_PEEntity && m_PEEntity.IsDeath())
				return;

			if(null != defenceTrigger)
			{
				SkEntity hitEntity = defenceTrigger.GetComponentInParent<SkEntity>();				
				bool isTerrain = PETools.PEUtil.IsVoxelOrBlock45(hitEntity);
				if(isTerrain || PETools.PEUtil.CanDamage(m_SkEntity, hitEntity))
				{
					if(null != onHitSkEntity)
						onHitSkEntity(hitEntity);
				}
				else
					return;
			}
			if(null != onHitTrigger)
				onHitTrigger(defenceTrigger, result);
			if(null != m_SkEntity)
				m_SkEntity.CollisionCheck(result);
			if(null != m_PEEntity)
				m_PEEntity.SendMsg(EMsg.Battle_AttackHit, result);
#if UNITY_EDITOR
			if(showDebugInfo)
			{
				DrawArrow arrow = new DrawArrow();
				arrow.pos = result.hitPos;
				arrow.rot = Quaternion.LookRotation(result.hitDir);
				hitResults.Add(arrow);
			}
#endif
		}

#if UNITY_EDITOR
		[System.Serializable]
		public class DrawArrow
		{
			public Vector3 pos;
			public Quaternion rot;
		}

		public List<DrawArrow> hitResults;

		public float arrowSize = 1f;

		public bool showDebugInfo = false;

		void OnValidate()
		{
			for(int i = 0; i < attackParts.Length; i++)
				attackParts[i].capsule.UpdateSettingValue();
		}

		void OnDrawGizmosSelected()
		{
			if(null == attackParts)
				return;

			if(showDebugInfo && null != hitResults)
			{
				UnityEditor.Handles.color = Color.red;
				foreach(DrawArrow arrow in hitResults)
				{
					UnityEditor.Handles.SphereCap(0, arrow.pos, arrow.rot, arrowSize * 0.1f);
					UnityEditor.Handles.ArrowCap(0, arrow.pos, arrow.rot, arrowSize);
				}
			}
			
			for(int i = 0; i < attackParts.Length; i++)
				attackParts[i].capsule.DrawGizmos();
		}
#endif
	}
}