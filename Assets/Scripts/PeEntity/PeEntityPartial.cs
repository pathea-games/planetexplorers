using UnityEngine;
using System.Collections.Generic;
using System.IO;
using SkillSystem;
using ItemAsset;
using WhiteCat;
using PETools;

namespace Pathea
{
    public partial class PeEntity
    {
		public void Reset()
		{
			ColletComponents();
#if UNITY_EDITOR			
			UnityEditor.EditorUtility.SetDirty(this);
#endif
		}

        void Awake()
        {
			if(!m_Inited)
				ColletComponents();
        }

		void ColletComponents()
		{
			m_Inited = true;
			mPeTrans = GetComponent<PeTrans>();
			mLodCmpt = GetComponent<LodCmpt>();
			mViewCmpt = GetComponent<ViewCmpt>();
			mCommonCmpt = GetComponent<CommonCmpt>();
			mEnityInfoCmpt = GetComponent<EntityInfoCmpt>();
			m_SkEntity = GetComponent<SkEntity>();
			mMotionMove = GetComponent<Motion_Move>();
			mMotionMgr = GetComponent<MotionMgrCmpt>();
			mNpcCmpt = GetComponent<NpcCmpt>();
			mAnim = GetComponent<AnimatorCmpt>();
			mMonsterCmpt = GetComponent<MonsterCmpt>();
			m_Carrier = GetComponent<CarrierController>();
			mPassengerCmpt = GetComponent<PassengerCmpt> ();
			m_Target = GetComponent<TargetCmpt>();
			mEquipmentCmpt = GetComponent<EquipmentCmpt>();
			mMotionEquipment = GetComponent<Motion_Equip>();
			mIKCmpt = GetComponent<IKCmpt>();
			mAlnormal = GetComponent<AbnormalConditionCmpt>();
			mPackage = GetComponent<PackageCmpt>();
			mUseItem=GetComponent<UseItemCmpt>();
			mOperateCmpt = GetComponent<OperateCmpt>();
			mMotionBeat = GetComponent<Motion_Beat>();
			mRequestCmpt = GetComponent<RequestCmpt>();
			mTower = GetComponent<TowerCmpt>();
			mBehaveCmpt = GetComponent<BehaveCmpt>();
			mRobotCmpt = GetComponent<RobotCmpt>();
			mReplicatorCmpt = GetComponent<ReplicatorCmpt>();
			mSkillTreeUnitMgr = GetComponent<SkillTreeUnitMgr>();
            m_MonstermountCtrl = GetComponent<MonstermountCtrl>();
            m_MountCmpt = GetComponent<MountCmpt>();

            if (m_SkEntity != null)
			{
				m_PeSkEntity = m_SkEntity as PESkEntity;
				m_SkAliveEntity = m_SkEntity as SkAliveEntity;
			}
		}

		[SerializeField]
		[HideInInspector]
		bool m_Inited;
		
		[SerializeField]
		[HideInInspector]
        PeTrans mPeTrans;
        public PeTrans peTrans { get { return mPeTrans; } set { mPeTrans = value; } }
		
		[SerializeField]
		[HideInInspector]
        SkEntity m_SkEntity;
        public SkEntity skEntity
        {
            get { return m_SkEntity; }
            set
            {
                m_SkEntity = value;

                if (m_SkEntity != null)
                {
                    m_PeSkEntity = m_SkEntity as PESkEntity;
                    m_SkAliveEntity = m_SkEntity as SkAliveEntity;
                }
            }
        }
		
		[SerializeField]
		[HideInInspector]
        NetCmpt mNetCmpt;
        public NetCmpt netCmpt { get { return mNetCmpt; } set { mNetCmpt = value; } }
		
		[SerializeField]
		[HideInInspector]
        MonsterCmpt mMonsterCmpt;
        public MonsterCmpt monster { get { return mMonsterCmpt; } }
		
		[SerializeField]
		[HideInInspector]
        ViewCmpt mViewCmpt;
        public ViewCmpt viewCmpt { get { return mViewCmpt; } set { mViewCmpt = value; } }
		public BiologyViewCmpt biologyViewCmpt { get { return mViewCmpt as BiologyViewCmpt; } }
		
		[SerializeField]
		[HideInInspector]
		LodCmpt mLodCmpt;
		public LodCmpt lodCmpt { get { return mLodCmpt; } }
		
		[SerializeField]
		[HideInInspector]
        Motion_Move mMotionMove;
        public Motion_Move motionMove { get { return mMotionMove; } }
		
		[SerializeField]
		[HideInInspector]
		MotionMgrCmpt mMotionMgr;
		public MotionMgrCmpt motionMgr { get { return mMotionMgr; } }
		
		[SerializeField]
		[HideInInspector]
        PESkEntity m_PeSkEntity;
		public PESkEntity peSkEntity { get { return m_PeSkEntity; } }
		
		[SerializeField]
		[HideInInspector]
		SkAliveEntity m_SkAliveEntity;
		public SkAliveEntity aliveEntity { get { return m_SkAliveEntity; } }
		
		[SerializeField]
		[HideInInspector]
        CarrierController m_Carrier;
        public CarrierController carrier { get { return m_Carrier; } set { m_Carrier = value; } }
		
		[SerializeField]
		[HideInInspector]
        TargetCmpt m_Target;
        public TargetCmpt target { get { return m_Target; } set { m_Target = value; } }
		
		[SerializeField]
		[HideInInspector]
        CommonCmpt mCommonCmpt;
        public CommonCmpt commonCmpt { get { return mCommonCmpt; } }
		
		[SerializeField]
		[HideInInspector]
        PassengerCmpt mPassengerCmpt;
        public PassengerCmpt passengerCmpt { get { return mPassengerCmpt; } }
		
		[SerializeField]
		[HideInInspector]
        EquipmentCmpt mEquipmentCmpt;
        public EquipmentCmpt equipmentCmpt { get { return mEquipmentCmpt; } }
		
		[SerializeField]
		[HideInInspector]
        Motion_Equip mMotionEquipment;
        public Motion_Equip motionEquipment { get { return mMotionEquipment; } }
		
		[SerializeField]
		[HideInInspector]
		EntityInfoCmpt mEnityInfoCmpt;
		public EntityInfoCmpt enityInfoCmpt {get { return mEnityInfoCmpt;} }
		
		[SerializeField]
		[HideInInspector]
		NpcCmpt mNpcCmpt;
		public NpcCmpt NpcCmpt { get { return mNpcCmpt;} }
		
		[SerializeField]
		[HideInInspector]
		AnimatorCmpt mAnim;
		public AnimatorCmpt animCmpt{ get { return mAnim; } }
		
		[SerializeField]
		[HideInInspector]
		IKCmpt mIKCmpt;
		public IKCmpt IKCmpt{ get { return mIKCmpt; } }
		
		[SerializeField]
		[HideInInspector]
        TowerCmpt mTower;
		public TowerCmpt Tower{ get { return mTower; } set { mTower = value; } }
		
		[SerializeField]
		[HideInInspector]
		AbnormalConditionCmpt mAlnormal;
		public  AbnormalConditionCmpt Alnormal { get {return mAlnormal;}}
		
		[SerializeField]
		[HideInInspector]
		PackageCmpt mPackage;
		public PackageCmpt packageCmpt{ get { return mPackage; } }
		
		[SerializeField]
		[HideInInspector]
		UseItemCmpt mUseItem;
		public UseItemCmpt UseItem { get { return mUseItem;}}
		
		[SerializeField]
		[HideInInspector]
        CSBuildingLogic m_Building;
        public CSBuildingLogic Building{ get { return m_Building; } }
		
		[SerializeField]
		[HideInInspector]
		OperateCmpt mOperateCmpt;
		public OperateCmpt operateCmpt{ get { return mOperateCmpt; } }
		
		[SerializeField]
		[HideInInspector]
		Motion_Beat mMotionBeat;
		public Motion_Beat motionBeat{ get { return mMotionBeat; } }
		
		[SerializeField]
		[HideInInspector]
        RequestCmpt mRequestCmpt;
        public RequestCmpt requestCmpt { get { return mRequestCmpt; } }
		
		[SerializeField]
		[HideInInspector]
        BehaveCmpt mBehaveCmpt;
        public BehaveCmpt BehaveCmpt { get { return mBehaveCmpt; } set { mBehaveCmpt = value; } }
		
		[SerializeField]
		[HideInInspector]
		ReplicatorCmpt mReplicatorCmpt;
		public ReplicatorCmpt replicatorCmpt { get { return mReplicatorCmpt; } set { mReplicatorCmpt = value; } }
		
		[SerializeField]
		[HideInInspector]
		SkillTreeUnitMgr mSkillTreeUnitMgr;
		public SkillTreeUnitMgr skillTreeCmpt { get { return mSkillTreeUnitMgr; } set { mSkillTreeUnitMgr = value; } }

        [SerializeField]
        [HideInInspector]
        MonstermountCtrl m_MonstermountCtrl;
        public MonstermountCtrl monstermountCtrl { get { return m_MonstermountCtrl; } }

        [SerializeField]
        [HideInInspector]
        MountCmpt m_MountCmpt;
        public MountCmpt mountCmpt { get { return m_MountCmpt; } }


        RobotCmpt mRobotCmpt;
		public RobotCmpt robotCmpt { get{
				if(mRobotCmpt == null)
					mRobotCmpt = GetCmpt<RobotCmpt>();
				return mRobotCmpt;
			}}

		
		PEBuilding mDoodaBuid;
		public PEBuilding Doodabuid { get{ 
				if(mDoodaBuid == null) mDoodaBuid = GetComponentInChildren<PEBuilding>();
				return mDoodaBuid;}}

		public bool canInjured
		{
			get { return biologyViewCmpt ? biologyViewCmpt.canInjured : true; }
		}

        #region Get func
		public Vector3              position            { get { return mPeTrans != null ? mPeTrans.position : Vector3.zero; } set { if(mPeTrans != null) mPeTrans.position = value; } }
		public Quaternion           rotation            { get { return mPeTrans != null ? mPeTrans.rotation : Quaternion.identity; } set { if(mPeTrans != null) mPeTrans.rotation = value; } }
        public Transform            tr                  { get { return mPeTrans != null ? mPeTrans.existent : null; } }
        public float                maxRadius           { get { return mPeTrans != null ? mPeTrans.radius : 0.0f; } }
        public float                maxHeight           { get { return mPeTrans != null ? mPeTrans.bound.size.y : 0.0f; } }
        public Bounds               bounds              { get { return mPeTrans != null ? mPeTrans.bound : new Bounds(); } }
		public Vector3				forward				{ get { return mPeTrans != null ? mPeTrans.forward : Vector3.forward; } }
        public Vector3              centerPos           { get { return mPeTrans != null ? mPeTrans.center : Vector3.zero; } }
        public Vector3              centerTop           { get { return mPeTrans != null ? mPeTrans.centerUp : Vector3.zero; } }
        public Vector3              spawnPos            { get { return mPeTrans != null ? mPeTrans.spawnPosition : Vector3.zero; } }
        public bool                 isRagdoll           { get { return biologyViewCmpt != null ? biologyViewCmpt.IsRagdoll : false; } }
        public bool                 IsFly               { get { return mMonsterCmpt != null ? mMonsterCmpt.IsFly : false; } }
        public bool                 IsGroup             { get { return mMonsterCmpt != null ? mMonsterCmpt.IsGroup : false; } }
        public bool                 IsLeader            { get { return mMonsterCmpt != null ? mMonsterCmpt.IsLeader : false; } }
        public bool                 IsMember            { get { return mMonsterCmpt != null ? mMonsterCmpt.IsMember : false; } }
        public bool                 IsDark              { get { return mMonsterCmpt != null ? mMonsterCmpt.IsDark : false; } }
        public bool                 IsDarkInDaytime     { get { return mMonsterCmpt != null ? mMonsterCmpt.IsDark && !GameConfig.IsNight 
                                                                        && !AiUtil.CheckPositionInCave(position, 128.0f, PEConfig.GroundedLayer) : false; } }
        public bool                 IsInjury            { get { return mMonsterCmpt != null ? mMonsterCmpt.IsInjury : false; } }
        public bool                 IsSeriousInjury     { get { return mMonsterCmpt != null ? mMonsterCmpt.IsSeriousInjury : false; }
                                                          set { if (mMonsterCmpt != null) mMonsterCmpt.IsSeriousInjury = value; } }
        public bool                 IsAttacking         { get { return mMonsterCmpt != null ? mMonsterCmpt.IsAttacking : false; } }
		public bool                 IsBoss              { get { return mCommonCmpt != null ? mCommonCmpt.IsBoss : false;}}
        public PeEntity             Leader              { get { return mMonsterCmpt != null ? mMonsterCmpt.Leader : null; } }
        public BehaveGroup          Group               { get { return mMonsterCmpt != null ? mMonsterCmpt.Group : null; } set { if ( mMonsterCmpt != null ) mMonsterCmpt.Group = value; } }
        public Vector3              GroupLocal          { get { return mMonsterCmpt != null ? mMonsterCmpt.GroupLocal : Vector3.zero; } set { if ( mMonsterCmpt != null ) mMonsterCmpt.GroupLocal = value; } }
        public EEntityProto         proto               { get { return mCommonCmpt != null ? mCommonCmpt.entityProto.proto : EEntityProto.Max; } }
        public ERace                Race                { get { return mCommonCmpt != null ? mCommonCmpt.Race : ERace.None; } }
        public int                  ProtoID             { get { return mCommonCmpt != null ? mCommonCmpt.entityProto.protoId : -1; } }
		public int 					ItemDropId			{ get { return mCommonCmpt != null ? mCommonCmpt.ItemDropId : 0;}}
        public Vector3              velocity            { get { return mMotionMove != null ? mMotionMove.velocity : Vector3.zero; } }
        public Vector3              movement            { get { return mMotionMove != null ? mMotionMove.movement : Vector3.zero; } }
        public float                gravity             { get { return mMotionMove != null ? mMotionMove.gravity : -1.0f; } set { if (mMotionMove != null) mMotionMove.gravity = value; } }
        public MovementField        Field               { get { return mMotionMove != null && (mMotionMove is Motion_Move_Motor) ? (mMotionMove as Motion_Move_Motor).Field : MovementField.None; } }
		public MovementState        MoveState           { get { return mMotionMove != null ? mMotionMove.state : MovementState.None; } }
        public CarrierController    vehicle             { get { return mPassengerCmpt != null ? mPassengerCmpt.carrier : null;} }
        public Enemy                attackEnemy         { get { return m_Target != null ? m_Target.GetAttackEnemy() : null; } }   
        public PeEntity             Chat                { get { return m_Target != null ? m_Target.Chat : null; }  set { if ( m_Target != null ) m_Target.Chat = value; } }   
        public PeEntity             Food                { get { return m_Target != null ? m_Target.Food : null; }  set { if ( m_Target != null ) m_Target.Food = value; }}   
        public PeEntity             Treat               { get { return m_Target != null ? m_Target.Treat : null; }  set { if ( m_Target != null ) m_Target.Treat = value; }}   
        public Rigidbody            Rigid               { get { return biologyViewCmpt != null ? biologyViewCmpt.GetModelRigidbody() : null; } }

		public bool                 IsNpcHunger         { get { return mNpcCmpt != null ? mNpcCmpt.IsHunger : false;}}
		public bool                 IsNpcLowHp          { get { return mNpcCmpt != null ? mNpcCmpt.IsLowHp : false;}}
		public bool                 IsNpcUncomfortable  { get { return mNpcCmpt != null ? mNpcCmpt.IsUncomfortable : false;}}
		public bool                 IsNpcInDinnerTime   { get { return mNpcCmpt != null ? mNpcCmpt.IsInDinnerTime : false;}}
		public bool                 IsNpcInSleepTime    { get { return mNpcCmpt != null ? mNpcCmpt.IsInSleepTime : false;}}
		public bool                 NpcHasAnyRequest    { get { return mNpcCmpt != null ? mNpcCmpt.hasAnyRequest : false;}}
		public bool                 IsMotorNpc          { get { return mNpcCmpt != null && motionMove is Motion_Move_Motor;}}

        public bool                 IsMainPlayer        { get { return this == MainPlayer.Instance.entity; } }

        public bool                 IsSnake             { get { return mAnim != null && !mAnim.Equals(null) ? mAnim.GetBool("Snake") : false; } }

        public bool                 IsMount             { get; private set; }  //lz-2017.02.17 是坐骑

        public bool                 HasMount            { get { return null == m_MountCmpt ? false : null != m_MountCmpt.Mount; } }//玩家有坐骑
        public BHPatrolMode PatrolMode {
            get { return mBehaveCmpt != null ? mBehaveCmpt.PatrolMode : BHPatrolMode.None;}
            set { if (mBehaveCmpt != null) mBehaveCmpt.PatrolMode = value; }
        }

		[System.Obsolete("Use hasView instead.")]
        public bool HasModel
        {
			get { return mViewCmpt ? mViewCmpt.hasView : false; }
		}

		public bool hasView
		{
			get { return mViewCmpt ? mViewCmpt.hasView : false; }
		}

		public float HPPercent
        {
            get { return Mathf.Clamp01(GetAttribute(AttribType.Hp) / GetAttribute(AttribType.HpMax)); }
            set { SetAttribute(AttribType.Hp, GetAttribute(AttribType.HpMax) * Mathf.Clamp01(value), false); }
        }

		public float Atk
		{
			get { return GetAttribute(AttribType.Atk); }
		}


        public PeEntity Afraid
        {
            get { return m_Target != null ? m_Target.Afraid : null;}
            set { if(m_Target != null) m_Target.Afraid = value;}
        }

        public PeEntity Doubt
        {
            get { return m_Target != null ? m_Target.Doubt : null;}
            set { if(m_Target != null) m_Target.Doubt = value;}
        }

        public Transform centerBone
        {
            get { return mViewCmpt.centerTransform; }
        }

        public MovementField GetLimiter()
        {
            if (mMotionMove is Motion_Move_Human)
                return MovementField.Land;
            else if (mMotionMove is Motion_Move_Motor)
                return (mMotionMove as Motion_Move_Motor).Field;
            else
                return MovementField.None;
        }

        public float GetAttribute(AttribType type, bool bSum = true)
        {
            return m_SkEntity != null ? m_SkEntity.GetAttribute((int)type, bSum) : 0.0f;
        }

		public void SetAttribute(AttribType type, float attrValue, bool offEvent = true)
		{
			if(null != m_SkEntity)
                m_SkEntity.SetAttribute((int)type, attrValue, offEvent);
		}

        public bool IsDeath()
        {
            return m_PeSkEntity != null ? m_PeSkEntity.isDead : false;
        }

        public bool Stucking(float time = 5.0f)
        {
			return (mMotionMove != null) ? mMotionMove.Stucking (time) : false;
        }

        public bool IntersectRayExtend(Ray ray)
        {
            if(mPeTrans != null)
            {
                Vector3 pos = mPeTrans.trans.InverseTransformPoint(ray.origin);
                Vector3 dir = mPeTrans.trans.InverseTransformDirection(ray.direction);

                Ray newRay = new Ray(pos, dir);
                return mPeTrans.boundExtend.IntersectRay(newRay);
            }

            return false;
        }

        public bool ContainsPointExtend(Vector3 point)
        {
            if (mPeTrans != null)
            {
                Vector3 pos = mPeTrans.trans.InverseTransformPoint(point);
                return mPeTrans.boundExtend.Contains(pos);
            }

            return false;
        }

        public bool IntersectsExtend(Bounds bounds)
        {
            if (mPeTrans != null)
            {
                return mPeTrans.boundExtend.Intersects(bounds);
            }

            return false;
        }

        public PeEntity GetReputation(ReputationSystem.ReputationLevel minType, ReputationSystem.ReputationLevel maxType)
        {
            if (m_Target != null)
            {
                return m_Target.GetReputation(minType, maxType);
            }

            return null;
        }

        public List<IWeapon> GetWeaponlist()
        {
            if (mMotionEquipment != null)
                return mMotionEquipment.GetWeaponList();

            return null;
        }

		Dictionary<int, Transform> m_TransDic = new Dictionary<int, Transform>();
		public Transform GetChild(string boneName)
		{
			if(string.IsNullOrEmpty(boneName) || "0" == boneName)
				return null;

			Transform child;
			int nameHash = boneName.GetHashCode();
			if(m_TransDic.ContainsKey(nameHash))
			{
				child = m_TransDic[nameHash];
				if(null != child)
					return child;
			}

			if(null != biologyViewCmpt && null != biologyViewCmpt.modelTrans)
				child = PEUtil.GetChild(biologyViewCmpt.modelTrans, boneName);
			else
				child = PEUtil.GetChild(transform, boneName);
			if(null != child)
				m_TransDic[nameHash] = child;
			return child;
		}

        #endregion

        #region Set Func
        public void OnDamageMember(PeEntity caster, float value)
        {
            if(m_Target != null && caster != null)
            {
                m_Target.OnDamageMember(caster, value);
            }
        }

        public void OnTargetDiscover(PeEntity target)
        {
            if(m_Target != null && target != null)
            {
                m_Target.OnTargetDiscover(target);
            }
        }

        public void MoveToPosition(Vector3 targetPosition)
        {
            if (mMotionMove == null)
                return;

            mMotionMove.MoveTo(targetPosition);
        }

        public void StartSkill(SkEntity target, int id, ISkPara para = null, bool bStartImm = true)
        {
            if(m_SkEntity != null)
            {
				if(m_PeSkEntity != null)
					m_PeSkEntity.DispatchTargetSkill(m_SkEntity);

                m_SkEntity.StartSkill(target, id, para, bStartImm);
            }
        }


        // infrom target
        public void WeaponAtttck(IWeapon weapon,SkEntity caster)
        {
            if (m_SkEntity != null)
            {
                if (m_PeSkEntity != null)
                    m_PeSkEntity.DispatchWeaponAttack(m_SkEntity);
            }
        }

        public void StopSkill(int id)
        {
            if(m_SkEntity != null)
            {
                m_SkEntity.CancelSkillById(id);
            }
        }

		public void SetNpcAlert(bool value)
		{
			if(mNpcCmpt != null)
			{
				mNpcCmpt.NpcInAlert = value;
			}
		}

        public bool IsSkillRunning(int id, bool cdInclude = true)
        {
            if (m_SkEntity != null)
                return m_SkEntity.IsSkillRunning(id, cdInclude);

            return false;
        }

        public bool IsSkillRunable(int id)
        {
            if (m_SkEntity != null)
                return m_SkEntity.IsSkillRunnable(id);

            return false;
        }

		public void  DispatchTargetSkill(SkEntity caster)
		{
			if(m_PeSkEntity != null)
				m_PeSkEntity.DispatchTargetSkill(caster);
		}

        public void DispatchWeaponAttack(SkEntity caster)
        {
            if (m_PeSkEntity != null)
                m_PeSkEntity.DispatchWeaponAttack(caster);
        }

		public void DispatchOnTranslate(Vector3 pos)
		{
			if(m_PeSkEntity != null)
				m_PeSkEntity.DispatchOnTranslate(pos);
		}

        public void SetMount(bool isMount)
        {
            IsMount = isMount;
        }
	
        #endregion

		#region ProtoDb

		MonsterProtoDb.Item mMonsterProtoDb;
		public  MonsterProtoDb.Item monsterProtoDb
		{
			get
			{
				if(mMonsterCmpt != null && mMonsterProtoDb == null)
					mMonsterProtoDb = MonsterProtoDb.Get(this.entityProto.protoId);

				return mMonsterProtoDb;
			}
		}
		#endregion
    }
}