using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pathea.Projectile
{
    public class SPGRange : SkProjectileGroup
    {
        public float minRange;
        public float maxRange;
        public float minHeight;
        public float maxHeight;
        public float spaceRadius;

        List<Vector3> m_PointList;

        bool IsGiveUp(Vector3 pos)
        {
            for (int i = 0; i < m_PointList.Count; i++)
            {
                if (PETools.PEUtil.SqrMagnitudeH(pos, m_PointList[i]) < spaceRadius*spaceRadius)
                    return true;
            }

            return false;
        }

        Vector3 GetPosition()
        {
            if (m_PointList == null)
                m_PointList = new List<Vector3>();

            for (int i = 0; i < 10; i++)
            {
                Vector3 newPos = PETools.PEUtil.GetRandomPositionOnGround(transform.position, minRange, maxRange, minHeight, maxHeight, false);
                if (newPos != Vector3.zero)
                {
                    if (IsGiveUp(newPos))
                        continue;
                    else
                        return newPos;
                }
            }

            return Vector3.zero;
        }

        Quaternion GetRotation()
        {
            return Quaternion.AngleAxis(Random.Range(0.0f, 360.0f), Vector3.up);
        }

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
                Vector3 pos = GetPosition();
                Quaternion rot = GetRotation();

                if (pos == Vector3.zero) continue;

                if (m_Target != null)
					ProjectileBuilder.Instance.Register(projectileID, castTrans, pos, rot, m_Target, i, immediately);
                else if (m_TargetPosition != Vector3.zero)
					ProjectileBuilder.Instance.Register(projectileID, castTrans, pos, rot, m_TargetPosition, i, immediately);

				if (!immediately)
                    yield return new WaitForSeconds(projectileInterval);
            }

            GameObject.Destroy(gameObject);
        }

		public override void SetData (ProjectileData data, Transform caster, Transform emitter, Transform target, Vector3 targetPosition, int index)
		{
			base.SetData (data, caster, emitter, target, targetPosition, index);
			StartCoroutine(SpawnProjectile());
		}
    }
}
