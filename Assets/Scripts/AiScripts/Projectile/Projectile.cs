using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SkillAsset;
using ItemAsset;
using AiAsset;
using SoundAsset;

public class Projectile : SkillRunner
{
    public static float MindamagePercent = 0.0f;
    public static float DamageRadius = 0.3f;

    public SkillRunner emitRunner;
    public Transform emitTransform;
	public GameObject bufferEffect;
	public float bufferEffectLifetime = 2f;

    public float existTime;
    public int damageSkillID;
    public int hitEffectID;
    public int soundID;
    public int minDampRadius;
    public int maxDampRadius;

    public bool destruct;

    float energyShieldPercent = 1.0f;
    Vector3 spawnPosition = Vector3.zero;
    List<EnergyShieldCtrl> mShieldList = new List<EnergyShieldCtrl>();
	//byte projectileIndex = 1;

    bool mValid;
	//float random1;

    Trajectory mTrajectory;

    public virtual byte effectType { get { return 0; } }
	
    public override bool IsController
    {
        get
        {
            CommonInterface caster = emitRunner as CommonInterface;
            if (null != caster)
                return caster.IsController;

            return base.IsController;
        }
    }

    internal override uLink.NetworkView OwnerView
    {
        get
        {
            CommonInterface caster = emitRunner as CommonInterface;
            if (null != caster)
                return caster.OwnerView;

            return base.OwnerView;
        }
    }

    public void SetupTrajectory(SkillRunner caster, Transform emit, ISkillTarget target)
    {
        //mTrajectory = GetComponent<Trajectory>();

        //if (mTrajectory == null)
        //    return;

        //mTrajectory.emitter = caster != null ? caster.transform : null;

        //if (mTrajectory is TRRay)
        //{
        //    (mTrajectory as TRRay).Emit(emit.forward);
        //}
        //else if (mTrajectory is TRParabola)
        //{
        //    (mTrajectory as TRParabola).Emit(GetShootPosition(target));
        //}
        //else if (mTrajectory is TRRaycast)
        //{
        //    (mTrajectory as TRRaycast).Emit(emit);
        //}
        //else if (mTrajectory is TRTrack)
        //{
        //    (mTrajectory as TRTrack).Emit(target);
        //}
        //else if (mTrajectory is TRMultiPara)
        //{
        //    (mTrajectory as TRMultiPara).Emit(target);
        //}
        //else if (mTrajectory is TRStraight)
        //{
        //    (mTrajectory as TRStraight).Emit(GetShootPosition(target));
        //}
        //else if (mTrajectory is TRMultiImpulse)
        //{
        //    (mTrajectory as TRMultiImpulse).Emit(GetShootPosition(target), emit.forward, projectileIndex);
        //}
        //else if (mTrajectory is TRImpulse)
        //{
        //    (mTrajectory as TRImpulse).Emit(GetShootPosition(target), emit.forward);			
        //}
        //else if (mTrajectory is TRBind)
        //{
        //    (mTrajectory as TRBind).Emit(emit);			
        //}
        //else if (mTrajectory is TRSiloMissile)
        //{
        //    (mTrajectory as TRSiloMissile).Emit(target, emit.forward, projectileIndex, random1);			
        //}
        //else if (mTrajectory is TRGatling)
        //{
        //    (mTrajectory as TRGatling).Emit(emit.forward);
        //}
        //else if (mTrajectory is TRVLS)
        //{
        //    (mTrajectory as TRVLS).Emit(target);
        //}
        //else if (mTrajectory is TRTrackPhscs)
        //{
        //    (mTrajectory as TRTrackPhscs).Emit(target, emit.forward);
        //}
        //else if (mTrajectory is TRMotorFlame)
        //{
        //    (mTrajectory as TRMotorFlame).Emit();
        //}
        //else if (mTrajectory is TRMultiR04_l)
        //{
        //    (mTrajectory as TRMultiR04_l).Emit(projectileIndex);
        //}
        //else if (mTrajectory is TRFlare)
        //{
        //    (mTrajectory as TRFlare).Emit(GetShootPosition(target));
        //}
    }

    public void Start()
    {
        mValid = true;
        spawnPosition = transform.position;
        Invoke("Destruct", existTime);
    }

    public void Update()
    {

    }

    public virtual void DestroyProjectile()
    {
		if(null != bufferEffect)
			ShiftEffect(bufferEffect);

        Destroy(gameObject);
    }

    public void Init(byte index, SkillRunner emitRunner, ISkillTarget target, Transform emitTransform, float rand1)
    {
		//this.projectileIndex = index;
        this.emitRunner = emitRunner;
        this.emitTransform = emitTransform;
		//this.random1 = rand1;
        SetupTrajectory(emitRunner, emitTransform, target);
    }

    public void Init(byte index, SkillRunner emitRunner, ISkillTarget target, Transform emitTransform, int damageSkillID, float rand1)
    {
		//this.projectileIndex = index;
        this.emitRunner = emitRunner;
        this.emitTransform = emitTransform;
        this.damageSkillID = damageSkillID;
		//this.random1 = rand1;
        SetupTrajectory(emitRunner, emitTransform, target);
    }

    void Destruct()
    {
        if (destruct)
        {
            RunEff(damageSkillID, null);
            PlayHitEffect(transform.position, transform.rotation);
        }

        DestroyProjectile();
    }

    public bool IsIgnoreRaycastHit(RaycastHit hitInfo)
    {
        return IsIgnoreCollider(hitInfo.collider);
    }

    void CheckEnergyShield(Collider collider)
    {
        EnergyShieldCtrl energy = collider.GetComponent<EnergyShieldCtrl>();
        if (energy != null)
        {
            energy.HitShield(this);
        }
    }

	public static Vector3 GetPredictPosition(ISkillTarget target, Vector3 startPos, float speed)
	{
		SkillRunner runner = target as SkillRunner;

		if(runner != null && runner.GetComponent<Rigidbody>() != null)
		{
			Vector3 reverseVec = startPos - GetShootPosition(target);
			float sqrv22mv12 = Mathf.Sqrt(speed * speed - runner.GetComponent<Rigidbody>().velocity.sqrMagnitude);
			float cos2 = Mathf.Cos(Vector3.Angle(reverseVec, runner.GetComponent<Rigidbody>().velocity) / 180f * Mathf.PI);
			float temp1 = reverseVec.sqrMagnitude * runner.GetComponent<Rigidbody>().velocity.sqrMagnitude * cos2 * cos2;
			float predictTime;
			if(Vector3.Angle(reverseVec, runner.GetComponent<Rigidbody>().velocity) <= 90f)
				predictTime = (Mathf.Sqrt(reverseVec.sqrMagnitude + temp1 / sqrv22mv12 / sqrv22mv12) - Mathf.Sqrt(temp1 / sqrv22mv12 / sqrv22mv12)) / sqrv22mv12;
			else
				predictTime = (Mathf.Sqrt(reverseVec.sqrMagnitude + temp1 / sqrv22mv12 / sqrv22mv12) + Mathf.Sqrt(temp1 / sqrv22mv12 / sqrv22mv12)) / sqrv22mv12;
			Vector3 predictPos = GetShootPosition(target) + runner.GetComponent<Rigidbody>().velocity * predictTime;
			return predictPos;
		}
		else
			return GetShootPosition(target);
	}

    public static Vector3 GetShootPosition(ISkillTarget target)
    {
        SkillRunner runner = target as SkillRunner;

        if (runner != null && runner.GetComponent<Collider>() != null)
        {
            AiObject aiObj = runner as AiObject;
            if (aiObj != null)
            {
                if (aiObj.model != null)
                {
                    Rigidbody[] rigids = aiObj.model.GetComponentsInChildren<Rigidbody>();
                    if (rigids != null && rigids.Length > 0)
                    {
                        return rigids[Random.Range(0, rigids.Length)].worldCenterOfMass;
                    }
                }

                return aiObj.center;
            }
            else
                return AiUtil.GetColliderCenter(runner.GetComponent<Collider>());
        }
        else
        {
			//CreationSkillRunner creation = runner as CreationSkillRunner;
			//if (creation != null)
			//	return creation.transform.TransformPoint(creation.LocalBounds.center);
			//else
			//	return target.GetPosition();
        }
		return Vector3.zero;
	}

    public static Vector3 GetTargetCenter(ISkillTarget target)
    {
        SkillRunner runner = target as SkillRunner;

        if (runner != null && runner.GetComponent<Collider>() != null)
        {
            AiObject aiObj = runner as AiObject;
            if (aiObj != null)
                return aiObj.center;
            else
                return AiUtil.GetColliderCenter(runner.GetComponent<Collider>());
        }
        else
        {
            //CreationSkillRunner creation = runner as CreationSkillRunner;
            //if (creation != null)
                //return creation.transform.TransformPoint(creation.LocalBounds.center);
           // else
               // return target.GetPosition();
        }
		return Vector3.zero;
    }

    public void CheckMovementCollision()
    {
        //if (mTrajectory == null || mTrajectory.velocity == Vector3.zero)
        //    return;

        //RaycastHit[] hitInfos = Physics.SphereCastAll(mTrajectory.lastPos, DamageRadius, mTrajectory.velocity, mTrajectory.velocity.magnitude);

        //List<RaycastHit> hits = new List<RaycastHit>(hitInfos);

        //hitInfos = hits.FindAll(ret => !IsIgnoreCollider(ret.collider)).ToArray();

        //RaycastHit hitInfo;

        //if (AiUtil.GetCloestRaycastHit(out hitInfo, hitInfos))
        //{
        //    CollisionHitInfo(hitInfo);
        //}
    }

    public void CollisionHitInfo(RaycastHit hitInfo)
    {
        if(mValid)
        {
            CastDamageSkill(hitInfo.collider);
            PlayEffect(hitInfo);
            PlaySound(hitInfo.point);
            DestroyProjectile();
            
            mValid = false;
        }
    }

    public void TriggerCollider(Collider col)
    {
        if (mValid)
        {
            CastDamageSkill(col);
            PlayHitEffect(col.ClosestPointOnBounds(transform.position), Quaternion.identity);
            PlaySound(col.ClosestPointOnBounds(transform.position));
            DestroyProjectile();

            mValid = false;
        }
    }

    public void TriggerColliderInterval(Collider col)
    {
        CastDamageSkill(col);
        PlayHitEffect(col.ClosestPointOnBounds(transform.position), Quaternion.identity);
    }

    void PlaySound(Vector3 pos)
    {
        if (soundID <= 0)
            return;

        AudioManager.instance.Create(pos, soundID);
    }

    public bool IsIgnoreCollider(Collider collider)
    {
        if (collider.transform.IsChildOf(transform))
            return true;

        if (collider.transform.tag == "WorldCollider")
            return true;

        if (collider.isTrigger)
        {
            CheckEnergyShield(collider);
            return true;
        }

        //AiDataObject aiDataObject = collider.GetComponent<AiDataObject>();
        //if (aiDataObject != null && !(aiDataObject is AiSceneStaticObject))
        //    return true;

        if (emitRunner != null)
        {
            Transform root = emitRunner.transform;
            CreationSkillRunner creation = VCUtils.GetComponentOrOnParent<CreationSkillRunner>(root.gameObject);
            if (creation != null)
            {
                root = creation.transform;
            }

            if (collider.transform.IsChildOf(root))
            {
                return true;
            }

//            if (emitRunner is ShootEquipment)
//            {
//                if ((emitRunner as ShootEquipment).IsChild(collider.transform))
//                {
//                    return true;
//                }
//            }
        }

        return false;
    }

    public void CastDamageSkill(Collider other)
    {
		if(emitRunner == null || other == null)
			return;

        VFVoxelChunkGo chunk = other.GetComponent<VFVoxelChunkGo>();
		B45ChunkGo	buildChunk = other.GetComponent<B45ChunkGo>();
        if (chunk != null || null != buildChunk)
        {
            EffSkill skillData = EffSkill.s_tblEffSkills.Find(iterSkill1 => EffSkill.MatchId(iterSkill1, damageSkillID));
            if (skillData.m_scopeOfSkill != null)
                RunEff(damageSkillID, null);
            else
                RunEff(damageSkillID, new DefaultPosTarget(transform.position));
        }
        else
        {
            if (other != null)
            {
				int emitHarm = AiUtil.GetHarm(emitRunner.gameObject);
				if (GameConfig.IsMultiMode)
				{
					SkillRunner obj = VCUtils.GetComponentOrOnParent<SkillRunner>(other.gameObject);
					if (null == obj)
						return;
					
					int targetHarm = AiUtil.GetHarm(obj.gameObject);

					if (AiHarmData.GetHarmValue(emitHarm, targetHarm) == 0)
						return;
				}
				else
				{
					int targetHarm = AiUtil.GetHarm(other.gameObject);

	                if (AiHarmData.GetHarmValue(emitHarm, targetHarm) == 0)
	                    return;
				}
            }

            EffSkill data = EffSkill.s_tblEffSkills.Find(iterSkill1 => EffSkill.MatchId(iterSkill1, damageSkillID));

            if (data != null)
            {
                if (data.m_scopeOfSkill != null)
                {
                    RunEff(damageSkillID, null);
                }
                else
                {
                    SkillRunner runner = VCUtils.GetComponentOrOnParent<SkillRunner>(other.gameObject);
                    if (runner != null)
                        RunEff(damageSkillID, runner);
                }
            }
        }
    }

    public void PlayEffect(RaycastHit hitInfo)
    {
        Vector3 effectPos = transform.position;
        Quaternion effectRot = transform.rotation;

        if ((effectType << 4) >> 7 == 1)
            effectPos = hitInfo.collider.transform.position;
        else if ((effectType << 4) >> 6 == 1)
            effectPos = transform.position;
        else
            effectPos = hitInfo.point;
        //effPos = hitInfo.collider.ClosestPointOnBounds(transform.position);

        if (effectType >> 7 == 1)
            effectRot = Quaternion.FromToRotation(Vector3.forward, hitInfo.point - transform.position);
        //effQua = Quaternion.FromToRotation(Vector3.forward, hitInfo.collider.ClosestPointOnBounds(transform.position) - transform.position);
        else if (effectType >> 6 == 1)
            effectRot = transform.rotation;
        else if (effectType >> 5 == 1)
            effectRot = Quaternion.AngleAxis(Random.Range(0, 360), new Vector3(Random.value, Random.value, Random.value));
        else
            effectRot = Quaternion.identity;

        EffectManager.Instance.Instantiate(hitEffectID, effectPos, effectRot, null);
    }

    public void PlayHitEffect(Vector3 position, Quaternion rot)
    {
        EffectManager.Instance.Instantiate(hitEffectID, position, rot, null);
    }

	public void ShiftEffect(GameObject eff)
	{
		eff.transform.parent = transform.parent;
		if(null != eff.GetComponent<ParticleSystem>())
			eff.GetComponent<ParticleSystem>().enableEmission = false;
		eff.AddComponent<DestroyTimer>();
		eff.GetComponent<DestroyTimer>().m_LifeTime = bufferEffectLifetime;
	}

    public bool ShieldHasBeenHitted(EnergyShieldCtrl esc)
    {
        return mShieldList.Contains(esc);
    }

    public void ApplyDamageReduce(float reducePercent, EnergyShieldCtrl esc)
    {
        energyShieldPercent *= reducePercent;
        mShieldList.Add(esc);
    }

    public float GetFinalAttack()
    {
        EffSkill data = EffSkill.s_tblEffSkills.Find(iterSkill1 => EffSkill.MatchId(iterSkill1, damageSkillID));

        float attackDamage = GetAttribute(Pathea.AttribType.Atk) * data.m_guidInfo.m_hpChangePercent + data.m_guidInfo.m_hpChangeOnce;
        return attackDamage;
    }

	public override float GetAttribute (Pathea.AttribType type, bool isBase = false)
	{
		if(type == Pathea.AttribType.Atk)
			return GetBaseAtk();
		return base.GetAttribute (type, isBase);
	}

    internal float GetBaseAtk()
    {
        if (emitRunner == null) return 0.0f;

		float attackValue = emitRunner.GetAttribute(Pathea.AttribType.Atk);

        float spaceDamagePercent = 1.0f;
        if (minDampRadius > PETools.PEMath.Epsilon && maxDampRadius > PETools.PEMath.Epsilon && maxDampRadius > minDampRadius)
        {
            float moveDistance = Mathf.Max(0.0f, Vector3.Distance(spawnPosition, transform.position) - minDampRadius);

            spaceDamagePercent = Mathf.Lerp(1.0f, MindamagePercent, moveDistance / (maxDampRadius - minDampRadius));
        }

        return attackValue * energyShieldPercent * spaceDamagePercent;
    }

    internal override List<ISkillTarget> GetTargetlistInScope(EffScope scope, int targetMask, ISkillTarget target)
    {
        List<ISkillTarget> targetList = new List<ISkillTarget>();

        if (emitRunner == null)
            return targetList;

        Collider[] cs = Physics.OverlapSphere(transform.position, scope.m_radius);
        List<Transform> transforms = new List<Transform>();

        foreach (Collider item in cs)
        {
            if (item.isTrigger)
                continue;

            if (!transforms.Contains(item.transform) &&
				!item.gameObject.Equals(this.gameObject) &&
				!item.gameObject.Equals(emitRunner.gameObject))
            {
                transforms.Add(item.transform);
            }

            Transform root = item.transform;
            CreationSkillRunner creation = VCUtils.GetComponentOrOnParent<CreationSkillRunner>(root.gameObject);
            if (creation != null)
            {
                if (!transforms.Contains(creation.transform))
                {
                    transforms.Add(creation.transform);
                }
            }
        }

        foreach (Transform tr in transforms)
        {
            if (tr == null || emitTransform == null)
                continue;

            float cosAngle = AiMath.Dot(emitTransform, tr);
            float angle = Mathf.Acos(cosAngle) * Mathf.Rad2Deg;

			int emitHarm = AiUtil.GetHarm(emitRunner.gameObject);

			if (GameConfig.IsMultiMode)
			{
				SkillRunner obj = tr.GetComponent<SkillRunner>();
				if (null == obj)
					continue;

				int targetHarm = AiUtil.GetHarm(obj.gameObject);

				if (AiHarmData.GetHarmValue(emitHarm, targetHarm) == 0)
					continue;
			}
			else
			{
	            int targetHarm = AiUtil.GetHarm(tr.gameObject);

	            if (AiHarmData.GetHarmValue(emitHarm, targetHarm) == 0)
	                continue;
			}

            Ray rayStart = new Ray(transform.position, tr.position - transform.position);
            float distance = Vector3.Distance(transform.position, tr.position);

            if (Physics.Raycast(rayStart, distance,
                AiUtil.groundedLayer | AiUtil.obstructLayer))
                continue;

            if (angle > scope.m_degEnd || angle < scope.m_degStart)
                continue;

            SkillRunner runner = tr.GetComponent<SkillRunner>();

            if (runner == null)
            {
                runner = VCUtils.GetComponentOrOnParent<SkillRunner>(tr.gameObject);
            }

            if (runner != null && !targetList.Contains(runner))
            {
                targetList.Add(runner);
            }
        }

        return targetList;
    }

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

    internal override void ApplyEffect(List<int> effId, ISkillTarget target)
    {
        base.ApplyEffect(effId, target);
    }

    //internal abstract Get
    // Apply changed of properties directly to 
    internal override void ApplyDistRepel(SkillRunner caster, float distRepel) { }
    internal override void ApplyHpChange(SkillRunner caster, float hpChange, float damagePercent, int type) { }
    internal override void ApplyComfortChange(float comfortChange) { }
    internal override void ApplySatiationChange(float satiationChange) { }
    internal override void ApplyThirstLvChange(float thirstLvChange) { }
//    internal override void ApplyBuffPermanent(EffSkillBuff buff) { }

    //effect and anim
    internal override void ApplyAnim(List<string> animName) { }
    #endregion
}
