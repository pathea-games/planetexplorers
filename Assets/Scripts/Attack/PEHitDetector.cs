using UnityEngine;
using System.Collections;
using SkillSystem;
using Pathea;
using Pathea.Projectile;

public class PEHitDetector : MonoBehaviour 
{
	protected SkEntity m_SkEntity;
	// Use this for initialization
	void Start () 
	{
		m_SkEntity = GetComponentInParent<SkEntity>();
		SkProjectile.onHitSkEntity += OnHitEntity;
		PEAttackTrigger.onHitSkEntity += OnHitEntity;
	}

	void OnDestroy()
	{
		SkProjectile.onHitSkEntity -= OnHitEntity;
		PEAttackTrigger.onHitSkEntity -= OnHitEntity;
	}

	void OnHitEntity(SkEntity entity)
	{
		if(null != entity && entity == m_SkEntity)
			OnHit();
	}

	protected virtual void OnHit()
	{

	}
}
