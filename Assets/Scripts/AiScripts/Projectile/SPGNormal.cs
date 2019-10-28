using UnityEngine;
using System.Collections;
using SkillSystem;

namespace Pathea.Projectile
{
    public class SPGNormal : SkProjectileGroup
    {
        public IEnumerator SpawnProjectile()
		{
			Transform castTrans = GetCasterTrans();    
			if(null == castTrans)
			{
				Debug.LogWarning("Can't find caster");
				yield break;
			}

			bool immediately = projectileInterval < PETools.PEMath.Epsilon;

            for (int i = 0; i < projectileCount; i++)
            {
                if (m_Target != null)
					ProjectileBuilder.Instance.Register(projectileID, castTrans, transform, m_Target, i, immediately);
                else if(m_TargetPosition != Vector3.zero)
					ProjectileBuilder.Instance.Register(projectileID, castTrans, transform, m_TargetPosition, i, immediately);

				if (!immediately)
                    yield return new WaitForSeconds(projectileInterval);
            }
        }

		public override void SetData (ProjectileData data, Transform caster, Transform emitter, Transform target, Vector3 targetPosition, int index)
		{
			base.SetData (data, caster, emitter, target, targetPosition, index);
			StartCoroutine(SpawnProjectile());
		}
    }
}
