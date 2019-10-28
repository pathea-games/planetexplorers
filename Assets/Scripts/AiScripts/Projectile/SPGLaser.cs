using UnityEngine;
using System.Collections;

namespace Pathea.Projectile
{
    public class SPGLaser : SkProjectileGroup
    {
        public float distane;

        bool m_Spawned;

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
                else if (m_TargetPosition != Vector3.zero)
					ProjectileBuilder.Instance.Register(projectileID, castTrans, transform, m_TargetPosition, i, immediately);

				if (!immediately)
                    yield return new WaitForSeconds(projectileInterval);
            }

            GameObject.Destroy(gameObject);
        }

        new public void Update()
        {
            base.Update();

            if (m_Spawned) return;

            Vector3 pos = m_Target != null ? m_Target.position : m_TargetPosition;
            if(Vector3.Distance(transform.position, pos) < distane)
            {
                m_Spawned = true;
                StartCoroutine(SpawnProjectile());
            }
        }
    }
}
