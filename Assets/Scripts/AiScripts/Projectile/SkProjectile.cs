#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using SkillSystem;
using Pathea;
using Pathea.Effect;

namespace Pathea.Projectile
{
    public class ProjectileData
    {
        public int _id;
        public string _path;
        public string _bone;
        public int _sound;
        public int _effect;
        public Vector3 _axis;

        private static Dictionary<int, ProjectileData> m_data = new Dictionary<int, ProjectileData>();

        public static ProjectileData GetProjectileData(int pID)
        {
            return m_data.ContainsKey(pID) ? m_data[pID] : null;
        }

        public static void LoadData()
        {
            SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("projectile");
            while (reader.Read())
            {
                ProjectileData data = new ProjectileData();
                data._id = System.Convert.ToInt32(reader.GetString(reader.GetOrdinal("id")));
                data._path = reader.GetString(reader.GetOrdinal("path"));
                data._bone = reader.GetString(reader.GetOrdinal("bone"));
                data._sound = System.Convert.ToInt32(reader.GetString(reader.GetOrdinal("sound")));
                data._effect = System.Convert.ToInt32(reader.GetString(reader.GetOrdinal("effect")));
                data._axis = PETools.PEUtil.ToVector3(reader.GetString(reader.GetOrdinal("axis")), ',');
                m_data.Add(data._id, data);
            }
        }
    }

    public class SkProjectile : SkEntity, IDigTerrain
    {
        public static List<SkProjectile> s_Projectiles = new List<SkProjectile>();

		public static System.Action<SkEntity> onHitSkEntity;

        [SerializeField]
        float m_LifeTime;

        [SerializeField]
        float m_DelayTime;

        [SerializeField]
        int m_Atk;

        [SerializeField]
        int m_SkillID;

        [SerializeField]
        int m_SkillTerrainID;
#region eff
        [SerializeField]
        int m_SoundID;

        [SerializeField]
        int m_EffectID;

        [SerializeField]
        int m_EffectRot;
#endregion
        [SerializeField]
        float m_Interval;

        [SerializeField]
        float m_ResRange;

        [SerializeField]
        bool m_Trigger;

        [SerializeField]
        bool m_Explode;
        [SerializeField]
        float m_ExplodeRadius;

        #region bufferEff
        [SerializeField]
		Transform bufferEffect;

		[SerializeField]
		float bufferEffectTime;
#endregion

		[SerializeField]
		bool m_DeletWhenCasterDead;

		[SerializeField]
		float m_DeleteDelayTime = 0.5f;

		[SerializeField]
		bool m_DeletByHitLayer;

		[SerializeField]
		LayerMask m_DeletLayer;

		[SerializeField]
		[HideInInspector]
		bool m_Inited;

        bool m_Valid;

        float m_StartTime;

        Bounds m_Bounds;

        internal Transform m_Caster;
        internal Transform m_Emitter;
        internal Transform m_Target;
        internal Vector3   m_TargetPosition;

        internal PeEntity m_Entity;
		public event System.Action<SkEntity> onCastSkill;
		public SkEntity parentSkEntity{ get; set; }
		bool m_BoundsUpdated;
        public Bounds TriggerBounds 
		{
			get
			{
				if(!m_BoundsUpdated)
					CalculateTriggerBounds();
				return m_Bounds;
			}
		}

        //ProjectileData m_Data;
		[SerializeField]
		[HideInInspector]
        Trajectory m_Trajectory;

		List<SkEntity> m_Entities = new List<SkEntity>();
		List<SkEntity> m_DamageEntities = new List<SkEntity>();
        List<Collider> m_Colliders = new List<Collider>();
        //List<PeEntity> m_InjuredEntities = new List<PeEntity>();

		[SerializeField]
		[HideInInspector]
		Collider[] m_Triggers;

		Collider m_SelfCollider;

        public override Collider GetCollider(string name)
        {
            Transform child = PETools.PEUtil.GetChild(transform, name);
            return child != null ? child.GetComponent<Collider>() : null;
        }

        public GameObject Caster() 
        {
            if (m_Caster == null)
                return null;
            return m_Caster.gameObject;
        }

        public virtual void SetData(ProjectileData data, Transform caster, Transform emitter, Transform target, Vector3 targetPosition, int index = 0)
        {
            //m_Data = data;
            m_Caster = caster;
            m_Emitter = emitter;
            m_Target = target;
            m_TargetPosition = targetPosition;

            m_Valid = true;

            float atk = 0.0f;
            SkEntity skCaster = m_Caster.GetComponentInParent<SkEntity>();
            if (skCaster != null)
            {
                SetNet(skCaster._net, false);
                atk = skCaster.GetAttribute((int)AttribType.Atk);
            }

			parentSkEntity = skCaster;
            SkProjectile skPro = skCaster as SkProjectile;
            if (skPro != null)
            {
				parentSkEntity = skPro.GetSkEntityCaster();
				if (parentSkEntity != null)
                {
					m_Caster = parentSkEntity.transform;
                }
            }

            SetAttribute((int)AttribType.Atk, m_Atk + atk);

            if (m_Trajectory != null)
                m_Trajectory.SetData(m_Caster, m_Emitter, m_Target, m_TargetPosition, index);

            if (null != m_Emitter && !m_Explode)
                CheckInTrigger(m_Emitter.position);
        }

        public SkEntity GetSkEntityCaster()
        {
            if(m_Caster != null)
            {
                SkEntity entity = m_Caster.GetComponent<SkEntity>();
                if (entity is SkProjectile)
                    return (entity as SkProjectile).GetSkEntityCaster();
                else
                    return entity;
            }

            return null;
        }

		protected Transform GetCasterTrans()
		{
			SkEntity skEntity = GetSkEntityCaster();
			if(null != skEntity)
				return skEntity.transform;
			return null;
		}

        void CalculateTriggerBounds()
        {
            m_Bounds.center = Vector3.zero;
			m_Bounds.size = Vector3.zero;

            for (int i = 0; i < m_Triggers.Length; ++i)
            {
                Collider col = m_Triggers[i];
                if (col != null)
                {
                    if (m_Bounds.center != Vector3.zero)
                        m_Bounds.Encapsulate(col.bounds);
                    else
                    {
                        m_Bounds.center = col.bounds.center;
                        m_Bounds.size = col.bounds.size;
                    }
                }
            }
			m_BoundsUpdated = true;
        }


		 PECapsuleHitResult GetHitResult(Transform self, Collider other)
		{
			PECapsuleHitResult hitResult = null;
			PEDefenceTrigger defencetrigger = other.transform.GetComponent<PEDefenceTrigger>();
			
			if(null == m_SelfCollider)
				m_SelfCollider = GetComponentInChildren<Collider>();

			if(null != defencetrigger && (null == m_SelfCollider || !(m_SelfCollider is MeshCollider)))
			{
				float size = (TriggerBounds.size/2f).magnitude;
				if(size <= Mathf.Epsilon)
					size = 0.1f;
				if(!defencetrigger.active || !defencetrigger.GetClosest(self.position, (TriggerBounds.size/2f).magnitude, out hitResult))
					return null;
			}
			else
			{
				hitResult = new PECapsuleHitResult();
				hitResult.hitTrans = other.transform;
				hitResult.selfTrans = self;
				hitResult.hitPos = other.ClosestPointOnBounds(self.position);
				
				Vector3 dir = hitResult.hitPos - self.position;
				if(dir == Vector3.zero)
					dir = other.transform.position - self.position;
				hitResult.distance = dir.magnitude;
				hitResult.hitDir = dir.normalized;
				SkEntity skEntity = other.transform.GetComponentInParent<SkEntity>();
				if(null != skEntity)
				{
					if(skEntity is VFVoxelTerrain)
						hitResult.hitDefenceType = DefenceType.Carrier;
					else if(skEntity is WhiteCat.CreationSkEntity)
						hitResult.hitDefenceType = DefenceType.Carrier;
				}
				hitResult.damageScale = 1f; 
			}
			
			hitResult.selfAttackForm = AttackForm.Bullet;
			return hitResult;
		}
		
		void OnColliderEnter(Collider self, Collider other)
		{
			if (0 == ((1 << other.gameObject.layer) & GameConfig.ProjectileDamageLayer))
				return;
            if ((m_Caster == null || !other.transform.IsChildOf(m_Caster))
                && !other.transform.IsChildOf(transform) 
                && self.transform.IsChildOf(transform)
			    && 0 != ((1 << other.gameObject.layer) & GameConfig.ProjectileDamageLayer))
            {

				PECapsuleHitResult hitResult = GetHitResult(self.transform, other);
				if(null != hitResult)
					CollisionCheck(hitResult);
            }

			OnTriggerEnter(other);
        }

        protected virtual void CastSkill(SkEntity skEntity)
        {
			if (PETools.PEUtil.IsVoxelOrBlock45(skEntity))
            {
                if(m_SkillTerrainID > 0)
                    StartSkill(skEntity, m_SkillTerrainID);
            }
            else
            {
                if(m_SkillID > 0)
                    StartSkill(skEntity, m_SkillID);
            }

			if(null != onCastSkill)
				onCastSkill(skEntity);

			if(null != skEntity && null != onHitSkEntity)
				onHitSkEntity(skEntity);

        }

		protected virtual void CastSkill(SkEntity skEntity, PECapsuleHitResult hitResult)
		{
			if (PETools.PEUtil.IsVoxelOrBlock45(skEntity))
			{
				if(m_SkillTerrainID > 0)
				{
					SkInst skillinst = StartSkill(skEntity, m_SkillTerrainID, null, false);
					if(skillinst != null)
					{
						skillinst._colInfo = hitResult;
						skillinst.Start();
					}
				}
			}
			else
			{
				if(m_SkillID > 0)
				{
					SkInst skillinst = StartSkill(skEntity, m_SkillID, null, false);
					if(skillinst != null)
					{
						skillinst._colInfo = hitResult;
						skillinst.Start();
					}
				}
			}

			if(null != onCastSkill)
				onCastSkill(skEntity);
			
			if(null != skEntity && null != onHitSkEntity)
				onHitSkEntity(skEntity);
		}


		bool GetRaycastInfo(Vector3 position, Vector3 velcity, out RaycastHit hitInfo, out Transform hitTrans, out PECapsuleHitResult hitResult, out bool useHitReslut, int layer)
        {
			hitResult = null;
			useHitReslut = false;
            hitInfo = new RaycastHit();
			hitTrans = null;
            if (velcity.sqrMagnitude > 0.05f * 0.05f)
            {
                Ray rayStart = new Ray(position, velcity);
                RaycastHit[] hitInfos = Physics.RaycastAll(rayStart, velcity.magnitude, layer);
				hitInfos = PETools.PEUtil.SortHitInfo(hitInfos, false);
				RaycastHit hit;
				for(int i = 0; i < hitInfos.Length; ++i)
                {
					hit = hitInfos[i];
                    if (hit.transform == null)
                        continue;

                    if (hit.transform.IsChildOf(transform))
                        continue;

                    if (m_Caster != null && hit.transform.IsChildOf(m_Caster))
                        continue;

					if(hit.collider.gameObject.tag == "EnergyShield")
                    {
						EnergySheildHandler shield = hit.collider.GetComponent<EnergySheildHandler>();
                        if(shield != null)
                            shield.Impact(hit.point);
						continue;
                    }
					
					hitInfo = hit;
					hitTrans = hit.transform;
					
					PEDefenceTrigger defencetrigger = hit.transform.GetComponent<PEDefenceTrigger>();
					if(null != defencetrigger)
					{
						if(!defencetrigger.RayCast(rayStart, 100f, out hitResult))
							continue;
						hitInfo.point = hitResult.hitPos;
						hitTrans = hitResult.hitTrans;
						useHitReslut = true;
					}
					else if(hit.collider.isTrigger)
					{
						return false;
					}

                    return true;
                }
            }

            return false;
        }

		void CheckInTrigger(Vector3 emitter)
		{
			Collider[] colls = Physics.OverlapSphere(transform.position, 0.1f, GameConfig.ProjectileDamageLayer);

			for(int i = 0; i < colls.Length; ++i)
			{
				Collider hitCol = colls[i];
				if (hitCol.transform == null)
				continue;
				
				if (hitCol.transform.IsChildOf(transform))
				continue;
				
				if (m_Caster != null && hitCol.transform.IsChildOf(m_Caster))
				continue;
				
				if(hitCol.gameObject.tag == "EnergyShield")
				{
					EnergySheildHandler shield = hitCol.GetComponent<EnergySheildHandler>();
					if(shield != null)
						shield.Impact(emitter);
					continue;
				}
				
				PEDefenceTrigger defencetrigger = hitCol.transform.GetComponent<PEDefenceTrigger>();
				if(null != defencetrigger)
				{
					PECapsuleHitResult hitResult;
					if(!defencetrigger.GetClosest(emitter, (TriggerBounds.size/2f).magnitude, out hitResult))
						continue;
					Hit(hitResult.hitPos, -hitResult.hitDir, hitResult.hitTrans);
					if(!m_Trigger)
						m_Valid = false;
				}
				else if(!hitCol.isTrigger)
				{
					Hit(emitter, Vector3.Normalize(emitter - hitCol.transform.position), hitCol.transform);
					if(!m_Trigger)
						m_Valid = false;
				}
				return;
			}
		}
		
		void PlayEffectHit(Vector3 hitPos, Vector3 hitNormal, bool useNormal = true)
		{
			if (m_EffectID > 0)
			{
				Quaternion rot = Quaternion.identity;
				
				if (m_EffectRot == 1)
					rot = transform.rotation;
				else if(useNormal && m_EffectRot == 2)
					rot = Quaternion.LookRotation(hitNormal);
				
				EffectBuilder.Instance.Register(m_EffectID, null, hitPos, rot);
			}
			
			if (m_SoundID > 0)
				AudioManager.instance.Create(hitPos, m_SoundID);
        }

        void PlayEffect()
        {
			PlayEffectHit(transform.position, Vector3.zero, false);
        }

		protected virtual void Hit(Vector3 pos, Vector3 normal, Transform hitTrans)
		{
			PlayEffectHit(pos, normal);

			if(!m_Trigger)
			{
				Delete();
			}
			
			if (hitTrans != null)
			{
				SkEntity skEntity = hitTrans.GetComponentInParent<SkEntity>();
				if(null == skEntity)
					return;
				PeEntity entity = skEntity.GetComponent<PeEntity>();
				if (null != skEntity && (null == entity || entity.canInjured))
				{
					PECapsuleHitResult hitResult = new PECapsuleHitResult();
					hitResult.selfTrans = transform;
					hitResult.hitPos = pos;
					hitResult.hitDir = -normal;
					hitResult.hitTrans = hitTrans;
					hitResult.damageScale = 1f;
					CastSkill(skEntity, hitResult);
				}
			}

		}

		protected virtual void Hit(PECapsuleHitResult hitResult, SkEntity skEntity = null)
		{
			PlayEffectHit(hitResult.hitPos, -hitResult.hitDir);

			if(!m_Trigger)
			{
				Delete();
			}
			
			if (hitResult.hitTrans != null)
			{
				if(null == skEntity)
					skEntity = hitResult.hitTrans.GetComponentInParent<SkEntity>();
				if(null == skEntity)
					return;
				PeEntity entity = skEntity.GetComponent<PeEntity>();
				if (null == entity || entity.canInjured)
				{
					CastSkill(skEntity, hitResult);
				}
			}

		}

        void Delete()
		{
			StopAllCoroutines();
			
			Renderer[] renderers = GetComponentsInChildren<Renderer>();
			for(int i = 0; i < renderers.Length; ++i)
			{
				Renderer renderer = renderers[i];
				ParticleSystem particle = renderer.GetComponent<ParticleSystem>();
				if(null != particle)
					particle.enableEmission = false;
				if(renderer is LineRenderer || renderer is TrailRenderer || renderer is ParticleSystemRenderer)
					continue;
				renderer.enabled = false;
			}
            GameObject.Destroy(gameObject, m_DeleteDelayTime);

			m_Valid = false;

			if(null != m_Trajectory)
				m_Trajectory.isActive = false;
        }

		protected virtual void OnLifeTimeEnd()
		{
			if (m_Explode)
				Explode();
		}

        IEnumerator DeleteEnumerator(float delayTime)
        {
            yield return new WaitForSeconds(delayTime);
			OnLifeTimeEnd();
			Delete();
        }

        IEnumerator TriggerDamage()
        {
            while (m_Trigger && !m_Explode)
            {
				
				for(int i = 0; i < m_Entities.Count; ++i)
				{
					if(null != m_Entities[i])
					{
						PeEntity entity = m_Entities[i].GetComponent<PeEntity>();
						if(null == entity || entity.canInjured)
							CastSkill(m_Entities[i]);
					}
				}

                if(m_Colliders.Count > 0)
                {
                    PlayEffect();
                }

                yield return new WaitForSeconds(m_Interval);
            }
        }


        void Explode()
        {
            if (!m_Valid) return;

			for(int i = 0; i < m_Entities.Count; ++i)
			{
				if(null != m_Entities[i])
				{
					PeEntity entity = m_Entities[i].GetComponent<PeEntity>();
					if(null == entity || entity.canInjured)
						CastSkill(m_Entities[i]);
				}
			}

            if (m_Colliders.Count > 0)
            {
                PlayEffect();
            }

            m_Valid = false;
        }

#if UNITY_EDITOR
		public void ResetProjectile()
		{
			m_Inited = true;
			m_Entity = GetComponent<PeEntity>();
			if(null == m_Entity)
				m_Entity = gameObject.AddComponent<PeEntity>();
			m_Entity.Reset();
			m_Triggers = PETools.PEUtil.GetCmpts<Collider>(transform);
			m_Trajectory = GetComponent<Trajectory>();
		}
#endif

        public void Awake()
        {
			Init (null, null, (int)AttribType.Max);

            m_StartTime = Time.time;

			if(!m_Inited)
			{
	            m_Entity = GetComponent<PeEntity>();
	            if(m_Entity == null)
					m_Entity = gameObject.AddComponent<PeEntity>();
				m_Triggers = GetComponentsInChildren<Collider>();
				m_Trajectory = GetComponent<Trajectory>();
			}


            if (m_Trigger && m_Interval > PETools.PEMath.Epsilon)
            {
                StartCoroutine(TriggerDamage());
            }

            SetAttribute((int)AttribType.ResRange, m_ResRange);

            PETrigger.AttachTriggerEvent(gameObject, OnColliderEnter, null, null);

            if(bufferEffect != null)
            {
                PEFollow.Follow(bufferEffect, transform);
            }

            StartCoroutine(DeleteEnumerator(m_LifeTime));

            s_Projectiles.Add(this);
        }

        public void Update()
        {
            if (!m_Valid || Time.time - m_StartTime <= m_DelayTime)
                return;

			m_BoundsUpdated = false;

			CheckCasterAlive();

            if (m_Trajectory != null)
            {
                Vector3 moveVector = m_Trajectory.Track(Time.deltaTime);
                Quaternion rotation = m_Trajectory.Rotate(Time.deltaTime);

                RaycastHit hitInfo;
				Transform hitTrans;
				PECapsuleHitResult hitResult;
				bool useHitResult;
				if (GetRaycastInfo(transform.position, moveVector, out hitInfo, out hitTrans, out hitResult, out useHitResult, GameConfig.ProjectileDamageLayer))
                {
					if(!m_Explode)
                    {
						if(!m_Trajectory.rayCast)
						{
	                        moveVector = hitInfo.point - transform.position;
							if(useHitResult && hitResult.hitTrans != null)
								Hit(hitResult);
							else
								Hit(hitInfo.point, hitInfo.normal, hitTrans);
						}

						if(!m_Trigger)
                        	m_Valid = false;
                    }
					else if (m_ExplodeRadius < PETools.PEMath.Epsilon)
                    {
                        Explode();
						Delete();
                    }
                }

                transform.position += moveVector;
                transform.rotation = rotation;
                m_Trajectory.moveVector = moveVector;
            }
            else
            {
                if (m_Explode && m_ExplodeRadius > PETools.PEMath.Epsilon)
                {
                    Collider[] colliders = Physics.OverlapSphere(transform.position, m_ExplodeRadius);
                    for(int i = 0; i < colliders.Length; i++)
                    {
                        CommonCmpt com = colliders[i].GetComponentInParent<CommonCmpt>();
                        if (com != null && com.isPlayerOrNpc)
                        {
                            Explode();
							Delete();
							break;
                        }
                    }
                }
            }
        }

        public virtual void OnDestroy()
        {
			if(bufferEffect != null)
				ShiftEffect();

            PETrigger.DetachTriggerEvent(gameObject, OnColliderEnter, null, null);

            s_Projectiles.Remove(this);
        }

        public void OnTriggerEnter(Collider other)
        {
			if(m_DeletByHitLayer && 0 != (m_DeletLayer.value & (1 << other.gameObject.layer)))
				Delete();

            if (!m_Valid)
				return;
			
			if (0 == ((1 << other.gameObject.layer) & GameConfig.ProjectileDamageLayer))
				return;

            if (other.gameObject.GetComponentInParent<SkProjectile>() != null)
                return;

            if (other.transform.IsChildOf(transform))
                return;

            if (m_Caster != null && other.transform.IsChildOf(m_Caster))
                return;

            if (!m_Colliders.Contains(other))
                m_Colliders.Add(other);

            SkEntity entity = other.GetComponentInParent<SkEntity>();
            if (entity != null)
            {
                if (!m_Explode && !m_DamageEntities.Contains(entity))
				{
					PECapsuleHitResult hitResult = GetHitResult(transform, other);
					if(null != hitResult)
					{
						if(other.gameObject.tag == "EnergyShield")
						{							
							EnergySheildHandler shield = other.GetComponent<EnergySheildHandler>();
							if(shield != null)
								shield.Impact(hitResult.hitPos);
							return;
						}

						if(!m_DamageEntities.Contains(entity))
						{
							Hit(hitResult);
//							CastSkill(entity, hitResult);
	                        m_DamageEntities.Add(entity);
						}
                    }
				}

                if (!m_Entities.Contains(entity))
                    m_Entities.Add(entity);
            }
        }
        public void OnTriggerExit(Collider other)
        {
            if (other.transform.IsChildOf(transform))
                return;

            if (m_Caster != null && other.transform.IsChildOf(m_Caster))
                return;

            if (m_Colliders.Contains(other))
                m_Colliders.Remove(other);

            SkEntity entity = other.GetComponentInParent<SkEntity>();
            if (entity != null && m_Entities.Contains(entity))
            {
                m_Entities.Remove(entity);
            }
        }

		public IntVector4 digPosType
        {
            get { return new IntVector4(transform.position, 0); }
        }

		public override void ApplyEmission(int emitId, SkRuntimeInfo info)
        {
			base.ApplyEmission(emitId, info);

			ProjectileBuilder.Instance.Register(emitId, transform, info, 0, false);
        }

		void ShiftEffect()
		{
			ParticleSystem[] ParticleSystems = bufferEffect.GetComponentsInChildren<ParticleSystem>();
			for(int i = 0; i < ParticleSystems.Length; ++i)
				ParticleSystems[i].enableEmission = false;
			bufferEffect.gameObject.AddComponent<DestroyTimer>().m_LifeTime = bufferEffectTime;
		}

		void CheckCasterAlive()
		{
			if(m_DeletWhenCasterDead)
			{
				PESkEntity entity = GetSkEntityCaster() as PESkEntity;
				if(null != entity && entity.isDead)
					Delete();
			}
		}
    }
}

#if UNITY_EDITOR
public partial class PeCustomMenu : EditorWindow
{	
	[MenuItem("Assets/ResetProjectile")]
	static void ResetProjectile()
	{
		GameObject[] selectedObjArray = Selection.gameObjects;
		
		for (int i = 0; i < selectedObjArray.Length; ++i) 
		{			
			Pathea.Projectile.SkProjectile projectile = selectedObjArray[i].GetComponent<Pathea.Projectile.SkProjectile>();
			
			if(null != projectile)
			{
				projectile.ResetProjectile();
				UnityEditor.EditorUtility.SetDirty(selectedObjArray[i]);
			}
		}
	}
}
#endif
