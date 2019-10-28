using UnityEngine;
using Pathea;
using SkillSystem;
using System.Collections.Generic;

namespace WhiteCat
{
    public enum AIMode
    {
        Attack,
        Defence,
        Cure,
        Passive
    }


	public abstract class AIBehaviourController : BehaviourController
	{
		SkAliveEntity _attackOwnerEntity; ViewCmpt _attackOwnerView;    // 攻击玩家的（最高优先级）
		SkAliveEntity _attackSelfEntity; ViewCmpt _attackSelfView;      // 攻击自己的（次高优先级）
		SkAliveEntity _ownerAttackEntity; ViewCmpt _ownerAttackView;    // 玩家攻击的（最低优先级）

		CarrierController _ownerCarrier;    // 跟随者乘坐的载具

        AIMode _aiMode;                     // 战斗模式
		float _timeCountForFindEnemy;       // 主动攻击模式下，每隔一段时间查找一次目标
		float _timeCountForCure;			// 治疗模式下，每隔一段时间给主人回血

		bool _isAttackMode;                 // 是否攻击模式
		SkEntity _aimEntity;                // 瞄准的目标
		ViewCmpt _aimView;                  // 瞄准的目标view
		Vector3 _aimPoint;                  // 瞄准的点

		List<float> _cureValueList;
	


		// 默认战斗模式
        protected abstract AIMode defaultAIMode { get; }


		/// <summary> 多人模式下，创造AI后需要初始化创造者 </summary>
		public void SetCreater(int peEntityID)
		{   
			var entity = EntityMgr.Instance.Get(peEntityID);
			if (entity) 
			{
				if (energy > 0f) SetOwner(entity);
			}
			else SetOwner(null);
		}


		/// <summary> 战斗模式 </summary>
        public AIMode aiMode
		{
			get { return _aiMode; }
			set { _aiMode = value; }
		}


		public override bool isAttackMode
		{
			get { return _isAttackMode; }
		}


		// 跟随者乘坐的载具
		public CarrierController ownerCarrier
		{
			get { return _ownerCarrier; }
		}


		public override SkEntity attackTargetEntity
		{
			get { return _aimEntity; }
		}


		public override Vector3 attackTargetPoint
		{
			get { return _aimPoint; }
		}


		protected override void OnOwnerChange(PESkEntity oldOwner, PESkEntity newOwner)
		{
			if (oldOwner)
			{
				oldOwner.onHpReduce -= OnOwnerHpReduce;
				oldOwner.attackEvent -= OnOwnerAttack;
			}
			if (newOwner)
			{
				newOwner.onHpReduce += OnOwnerHpReduce;
				newOwner.attackEvent += OnOwnerAttack;
			}
		}


		void OnOwnerHpReduce(SkEntity enemy, float value)
		{
			enemy = PETools.PEUtil.GetCaster(enemy);
			if (enemy is SkAliveEntity && !enemy.Equals(null))
			{
				_attackOwnerEntity = enemy as SkAliveEntity;
				_attackOwnerView = enemy.GetComponent<ViewCmpt>();
			}
		}


		void OnOwnerAttack(SkEntity enemy, float value)
		{
			if (enemy is SkAliveEntity && !enemy.Equals(null))
			{
				_ownerAttackEntity = enemy as SkAliveEntity;
				_ownerAttackView = enemy.GetComponent<ViewCmpt>();
			}
		}


		protected override void InitOtherThings()
		{
			// 单人模式下，AI 属于玩家
			if (!PeGameMgr.IsMulti)
			{
				if (energy > 0f) SetOwner(MainPlayer.Instance.entity);
				ResetHost(MainPlayer.Instance.entity.Id);
			}

			// 监控敌人
			creationSkEntity.onHpReduce += (enemy, value) =>
			{
				if (enemy is SkAliveEntity && !enemy.Equals(null))
				{
					_attackSelfEntity = enemy as SkAliveEntity;
					_attackSelfView = enemy.GetComponent<ViewCmpt>();
				}
			};

			// 战斗模式
			_aiMode = defaultAIMode;
			_cureValueList = new List<float>();
			_cureValueList.Add(0f);
		}


		void CheckOwnerCarrier()
		{
				if (ownerEntity && _ownerCarrier != ownerEntity.passengerCmpt.carrier)
				{
					if (_ownerCarrier)
					{
						_ownerCarrier.creationSkEntity.onHpReduce -= OnOwnerHpReduce;
						_ownerCarrier.creationSkEntity.attackEvent -= OnOwnerAttack;
					}

					_ownerCarrier = ownerEntity.passengerCmpt.carrier;

					if (_ownerCarrier)
					{
						_ownerCarrier.creationSkEntity.onHpReduce += OnOwnerHpReduce;
						_ownerCarrier.creationSkEntity.attackEvent += OnOwnerAttack;
					}
				}
		}


		// 在主控端 FixedUpdate 中调用
		protected void UpdateAttactTarget()
		{
			CheckOwnerCarrier();

			SkEntity newAimTarget = null;
			ViewCmpt newAimView = null;

			if (_aiMode != AIMode.Passive && _aiMode != AIMode.Cure)
			{
				// 优先选择攻击玩家的敌人
				if (_attackOwnerEntity
					&& _attackOwnerView
					&& !_attackOwnerEntity.isDead
					&& _attackOwnerView.hasView
					&& _attackOwnerEntity.Entity != ownerEntity)
				{
					newAimTarget = _attackOwnerEntity;
					newAimView = _attackOwnerView;
				}
				else
				{
					_attackOwnerEntity = null;
					_attackOwnerView = null;

					// 其次选择攻击自己的敌人
					if (_attackSelfEntity
						&& _attackSelfView
						&& !_attackSelfEntity.isDead
						&& _attackSelfView.hasView
						&& _attackSelfEntity.Entity != ownerEntity)
					{
						newAimTarget = _attackSelfEntity;
						newAimView = _attackSelfView;
					}
					else
					{
						_attackSelfEntity = null;
						_attackSelfView = null;

						// 最后选择玩家攻击的敌人
						if (_ownerAttackEntity
						&& _ownerAttackView
						&& !_ownerAttackEntity.isDead
						&& _ownerAttackView.hasView
						&& _ownerAttackEntity.Entity != ownerEntity)
						{
							newAimTarget = _ownerAttackEntity;
							newAimView = _ownerAttackView;
						}
						else
						{
							_ownerAttackEntity = null;
							_ownerAttackView = null;

							// 攻击模式下主动寻找目标
							if (_aiMode == AIMode.Attack)
							{
								// 如果当前目标有效则继续攻击
								if (_aimEntity
									&& _aimView
									&& _aimView.hasView
									&& _aimEntity is SkAliveEntity
									&& !(_aimEntity as SkAliveEntity).isDead
									&& (_aimView.Entity.position - transform.position).sqrMagnitude < PEVCConfig.instance.sqrRobotAttackRange)
								{
									newAimTarget = _aimEntity;
									newAimView = _aimView;
								}
								else
								{
									_timeCountForFindEnemy += Time.deltaTime;

									// 当前目标无效则每隔 0.77 秒搜索一次目标
									if (_timeCountForFindEnemy > 0.77f)
									{
										_timeCountForFindEnemy = 0f;

										foreach (var pair in EntityMgr.Instance.mDicEntity)
										{
											if (pair.Value.hasView
												&& !pair.Value.IsDeath()
												&& (pair.Value.position - transform.position).sqrMagnitude < PEVCConfig.instance.sqrRobotAttackRange
												&& PETools.PEUtil.CanAttack(creationPeEntity, pair.Value))
											{
												newAimTarget = pair.Value.skEntity;
												newAimView = pair.Value.viewCmpt;
												break;
											}
										}
									}
								}
							}
						}
					}
				}
			}
			else if (_aiMode == AIMode.Cure)
			{
				OnCureModeUpdate();
			}

			// 更新攻击目标
			_isAttackMode = newAimTarget;
			if (_isAttackMode) _aimPoint = newAimView.centerPosition;
			_aimEntity = newAimTarget;
			_aimView = newAimView;

            // 开火
            SetWeaponControlEnabled(WeaponType.AI,
                isAttackMode && (attackTargetPoint - transform.position).sqrMagnitude < PEVCConfig.instance.sqrRobotAttackRange);
		}


		static List<int> _cureIdxList;
		static AIBehaviourController()
		{
			_cureIdxList = new List<int>();
			_cureIdxList.Add(0);
		}


		void OnCureModeUpdate()
		{
			_timeCountForCure += Time.deltaTime;
			if (_timeCountForCure >= 1f)
			{
				_timeCountForCure = 0f;

				if (ownerSkEntity!=null && !ownerSkEntity.isDead && ownerSkEntity.HPPercent < 1f)
				{
					ExpendEnergy(PEVCConfig.instance.robotCureExpendEnergyPerSecond);
					_cureValueList[0] = cureOwnerHpPerSecond;

					SkEntity.MountBuff(
						ownerSkEntity,
						30200169,
						_cureIdxList,
						_cureValueList);
				}
			}
		}


		protected virtual float cureOwnerHpPerSecond
		{
			get
			{
				return 0f;
			}
		}


		protected override void InitNetwork()
		{
			_netPosition = new NetData<Vector3>
			(
				last => (transform.position - last).sqrMagnitude >= PEVCConfig.instance.minSyncSqrDistance,
				() => transform.position,
				value => { if (!rigidbody.isKinematic) rigidbody.position = value; }
			);

			_netRotation = new NetData<Quaternion>
			(
				last => Quaternion.Angle(transform.rotation, last) >= PEVCConfig.instance.minSyncAngle,
				() => transform.rotation,
				value => { if (!rigidbody.isKinematic) rigidbody.rotation = value; }
			);

			_netVelocity = new NetData<Vector3>
			(
				last => (rigidbody.velocity - last).sqrMagnitude >= PEVCConfig.instance.minSyncSqrSpeed,
				() => rigidbody.velocity,
				value => rigidbody.velocity = value
			);

			_netAngularVelocity = new NetData<Vector3>
			(
				last => (rigidbody.angularVelocity - last).sqrMagnitude >= PEVCConfig.instance.minSyncSqrAngularSpeed,
				() => rigidbody.angularVelocity,
				value => rigidbody.angularVelocity = value
			);

			_netAimPoint = new NetData<Vector3>
			(
				last => (_aimPoint - last).sqrMagnitude >= PEVCConfig.instance.minSyncSqrAimPoint,
				() => _aimPoint,
				value => _aimPoint = value
			);

			_netInput = new NetData<ushort>
			(
				last => _isAttackMode != (last == 1),
				() => (ushort)(_isAttackMode ? 1 : 0),
				value => _isAttackMode = (value == 1)
			);
		}


		protected override void OnNetworkSync()
		{
		}
	}
}