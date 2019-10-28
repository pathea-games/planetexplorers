using UnityEngine;
using System.Collections;
using Pathea.Projectile;

public enum PEAbnormalAttackType
{
	None = 0,
	Dazzling = 1,
	BlurredVision = 1<<2,
	Flashlight = 1<<3,
	Tinnitus = 1<<4,
	Deafness = 1<<5
}

[System.Serializable]
public class PEAbnormalAttack
{
	public PEAbnormalAttackType type;
	
	public float radius;
	
	public float strength;

	public float duration;
}

public class PEAbnormalAttackTrigger : MonoBehaviour
{
	public bool costByProjectileHit = false;
	public bool deletSelf = true;

	public PEAbnormalAttack[] abnormalAttacks;

	void Start()
	{
		if(costByProjectileHit)
		{
			SkProjectile projectile = GetComponent<SkProjectile>();
			if(null != projectile)
				projectile.onCastSkill += (obj) => CheckTrigger();
			else
				costByProjectileHit = false;
		}

		if(!costByProjectileHit)
			CheckTrigger();
		if(deletSelf)
			GameObject.Destroy(gameObject);
	}

	void CheckTrigger()
	{
		if(null != abnormalAttacks && null != Pathea.PeCreature.Instance.mainPlayer)
		{
			Pathea.AbnormalConditionCmpt abnormalCmpt = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.AbnormalConditionCmpt>();
			if(null != abnormalCmpt)
			{
				foreach(PEAbnormalAttack attackAttr in abnormalAttacks)
					if(Vector3.SqrMagnitude(abnormalCmpt.Entity.position - transform.position) < attackAttr.radius * attackAttr.radius)
						abnormalCmpt.ApplyAbnormalAttack(attackAttr, transform.position);
			}
		}
	}
}
