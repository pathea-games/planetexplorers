using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea;

namespace Pathea.Projectile
{
	public class SPGDifferentTarget : SkProjectileGroup
	{

		[SerializeField] int[] projectileIDList;

		public IEnumerator SpawnProjectile()
		{
			if (projectileIDList.Length == 0) 
			{
				GameObject.Destroy(gameObject);
				yield break;
			}
			Transform castTrans = GetCasterTrans();    
			if(null == castTrans)
			{
				Debug.LogWarning("Can't find caster");
				GameObject.Destroy(gameObject);
				yield break;
			}
			
			bool immediately = projectileInterval < PETools.PEMath.Epsilon;

			TargetCmpt targetCmpt = castTrans.GetComponentInParent<TargetCmpt>();
			if (null != targetCmpt)
			{
				List<Enemy> enemies = targetCmpt.GetEnemies ();

				if(null != enemies)
				{
					for (int i = 0; i < projectileCount; i++)
					{
						Transform targetTrans = null;
						if(enemies.Count > 0)
						{
							int targetIndex = i%enemies.Count;
							if(null != enemies[targetIndex] || null != enemies[targetIndex].entityTarget || null != enemies[targetIndex].entityTarget.tr)
								targetTrans = enemies[targetIndex].entityTarget.tr;
						}
						int castProjectileID = projectileIDList[i%projectileIDList.Length];
						ProjectileBuilder.Instance.Register (castProjectileID, castTrans, targetTrans, i, immediately);	
						if (!immediately)
							yield return new WaitForSeconds (projectileInterval);
					}
				}
			}
		}
		
		public override void SetData (ProjectileData data, Transform caster, Transform emitter, Transform target, Vector3 targetPosition, int index)
		{
			base.SetData (data, caster, emitter, target, targetPosition, index);
			StartCoroutine(SpawnProjectile());
		}
	}
}