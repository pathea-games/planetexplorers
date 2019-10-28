using UnityEngine;
using System.Collections;
using SkillSystem;

namespace Pathea.Projectile
{
	public class SkProjectileEX : SkProjectile 
	{
		[SerializeField] int m_EmitProjectileID;

		[SerializeField] bool m_EmitOnEnd;

		protected override void Hit (Vector3 pos, Vector3 normal, Transform hitTrans)
		{
			EmitProjectile(hitTrans);
			base.Hit (pos, normal, hitTrans);
		}

		protected override void Hit (PECapsuleHitResult hitResult, SkEntity skEntity = null)
		{
			EmitProjectile(hitResult.hitTrans);
			base.Hit (hitResult, skEntity);
		}

		protected override void OnLifeTimeEnd ()
		{
			if(m_EmitOnEnd)
				EmitProjectile();
			base.OnLifeTimeEnd ();
		}

		void EmitProjectile(Transform hitTrans = null)
		{
			Transform castTrans = GetCasterTrans();    
			if(null == castTrans)
			{
				Debug.LogWarning("Can't find caster");
				return;
			}

			if (null != hitTrans)
				ProjectileBuilder.Instance.Register(m_EmitProjectileID, castTrans, transform, hitTrans);
			else if(m_TargetPosition != Vector3.zero)
				ProjectileBuilder.Instance.Register(m_EmitProjectileID, castTrans, transform, m_TargetPosition);
			else if(null != m_Target)
				ProjectileBuilder.Instance.Register(m_EmitProjectileID, castTrans, transform, m_Target);
		}

	}
}