using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SkillAsset;
using ItemAsset;
using AiAsset;

public partial class AiObject : SkillRunner
{

    public virtual float minDamageRange { get { return 0.0f; } }
    public virtual float maxDamageRange { get { return 0.0f; } }

    protected bool m_isAttackIdle = false;
    protected bool m_isAttacking = false;
    protected bool m_isHurted = true;

    protected float m_lastDamageTime = 0.0f;

    public bool isAttackIdle { get { return m_isAttackIdle; } set { m_isAttackIdle = value; } }
    public bool isAttacking { get { return m_isAttacking; } set { m_isAttacking = value; } }
    public bool isHurted { get { return m_isHurted; } set { m_isHurted = value; } }

    protected float m_deathStartTime = Mathf.Infinity;
    public float deathStartTime { get { return m_deathStartTime; } }

    //public AiEnemy enemy { get { return m_aiTarget != null ? m_aiTarget.enemy : null; } }
    public float lastDamageTime { get { return m_lastDamageTime; } }

    protected virtual float CorpseTime { get { return 0f; } }
    
    //bool IsSightBlocked(AiEnemy aiEnemy)
    //{
    //    Vector3 eyePosition = transform.position + transform.up * height * 0.75f + transform.forward * radius;

    //    Ray rayStart = new Ray(eyePosition, aiEnemy.center - eyePosition);
    //    float distance = Vector3.Distance(aiEnemy.center, eyePosition);

    //    return Physics.Raycast(rayStart, distance, 0);
    //}

    protected virtual void InitAttackData()
    {
    }
	
    //public virtual void OnEnemyAchieve(AiEnemy aiEnemy)
    //{
    //    aiEnemy.ResetHate(true);
    //}
	
    //public virtual void OnEnemyLost(AiEnemy aiEnemy)
    //{
    //    StopMove();
    //    aiEnemy.ResetHate(false);
    //}

    public virtual void ApplyDamage(float damage)
    {

    }

    public virtual void ApplyDamage(Transform hurter, float damage)
    {

    }

    protected virtual void OnDeath()
    {
        HandleTheDeathEvent();
    }

    protected virtual void OnDamage(float damage)
    {

    }

    protected virtual void OnDamage(Transform hurter, float damage)
    {

    }

    public virtual void OnKilled(GameObject dead)
    {

    }

    protected virtual void OnBeKilled(GameObject killer)
    {

    }
	
    //public virtual bool IsEnemyValid(AiEnemy aiEnemy)
    //{
    //    if (aiEnemy == null || !aiEnemy.valid)
    //        return false;
		
    //    if (aiEnemy.Threat < PETools.PEMath.Epsilon)
    //        return false;
		
    //    //if (IsSightBlocked(aiEnemy))
    //    //    return false;
		
    //    return true;
    //}

    #region SkillRunner
    internal override byte GetBuilderId()
    {
        return 0;
    }
    internal override float GetAtkDist(ISkillTarget target)
    {
        return 0f;
    }
//    internal override float GetDurAtk(ESkillTargetType resType)
//    {
//        return 0f;
//    }
//    internal override short GetResMultiplier()
//    {
//        return 0;
//    }
    internal override ItemPackage GetItemPackage()
    {
        return null;
    }
    internal override bool IsEnemy(ISkillTarget target)
    {
        return true;
    }
    internal override ISkillTarget GetTargetInDist(float dist, int targetMask)
    {
        return null;
    }
    internal override List<ISkillTarget> GetTargetlistInScope(EffScope scope, int targetMask, ISkillTarget target)
    {
        //if (target == null) return null;

        List<ISkillTarget> targetList = new List<ISkillTarget>();
        Vector3 scopeCenter = transform.position;
        if (scope.m_centerType == -1)
            scopeCenter = transform.position;

        if (scope.m_centerType == -2)
            scopeCenter = center;

        Collider[] colliders = Physics.OverlapSphere(scopeCenter, scope.m_radius + radius, 0);
        // Edit by zhouxun
        // we need check the parent gameobject also.

        foreach (Collider c in colliders)
        {
            if (c.isTrigger || c.gameObject == this.gameObject)
                continue;

			if (GameConfig.IsMultiMode)
			{
				SkillRunner obj = VCUtils.GetComponentOrOnParent<SkillRunner>(c.gameObject);
				if (null == obj)
					continue;

				int targetHarm = AiUtil.GetHarm(obj.gameObject);
				if (AiHarmData.GetHarmValue(harm, targetHarm) == 0)
					continue;
			}
			else
			{
	            int targetHarm = AiUtil.GetHarm(c.gameObject);
	            if (AiHarmData.GetHarmValue(harm, targetHarm) == 0)
	                continue;
			}

            float cosAngle = AiMath.Dot(transform, c.transform);
            float angle = Mathf.Acos(cosAngle) * Mathf.Rad2Deg;

            if (angle > scope.m_degEnd || angle < scope.m_degStart)
                continue;

            SkillRunner runner = VCUtils.GetComponentOrOnParent<SkillRunner>(c.gameObject);
            if (runner != null && (runner != this) && !targetList.Contains(runner))
            {
                targetList.Add(runner);
            }
        }
        return targetList;
    }

    //internal abstract Get
    // Apply changed of properties directly to 
    internal override void ApplyDistRepel(SkillRunner caster, float distRepel) { }
    internal override void ApplyHpChange(SkillRunner caster, float hpChange, float damagePercent, int type) 
    {
        if (OnBuff(Buff_Sp.INVENSIBLE)) 
            return;

        if (!GameConfig.IsMultiMode)
        {
            float _defence = (float)defence / (float)(defence + 0);

            if (caster != null)
            {
                float attack = caster.GetAttribute(Pathea.AttribType.Atk) * damagePercent + hpChange;
                float damage = attack * (1 - _defence) * Random.Range(0.9f, 1.1f);

                int damageType = type == 0 ? AiDamageTypeData.GetDamageType(caster) : type;
                damage *= AiDamageTypeData.GetDamageScale(damageType, defenceType);

                //Debug.Log(caster + " attack " + this.gameObject + (int)damage + " hp");
                if (caster.gameObject.Equals(this.gameObject))
                    ApplyDamage(Mathf.CeilToInt(damage));
                else
                    ApplyDamage(caster.transform, Mathf.CeilToInt(damage));
            }
            else
            {
                float attack = hpChange;
                float damage = attack * (1 - _defence) * Random.Range(0.9f, 1.1f);

                ApplyDamage(Mathf.CeilToInt(damage));
            }
        }
    }
    internal override void ApplyComfortChange(float comfortChange) { }
    internal override void ApplySatiationChange(float satiationChange) { }
    internal override void ApplyThirstLvChange(float thirstLvChange) { }
//    internal override void ApplyBuffPermanent(EffSkillBuff buff) { }

    //effect and anim
    internal override void ApplyAnim(List<string> animName) { }
    #endregion
}
