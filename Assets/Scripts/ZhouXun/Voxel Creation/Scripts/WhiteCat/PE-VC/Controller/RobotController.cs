using UnityEngine;
using Pathea;

namespace WhiteCat
{
	public class RobotController : AIBehaviourController
	{
		// 跟随本机玩家的机器人
		static RobotController _playerFollower;
		public static event System.Action<ItemAsset.ItemObject, GameObject> onPlayerGetRobot;
		public static event System.Action onPlayerLoseRobot;

		VCPRobotController _controller;
		TrailRenderer _trail;

		bool _active;                       // 是否活动（能量耗尽或玩家离线）

		float _updateTargetPositionCountDown;	// 更新目标位置的倒计时
		Vector3 _relativeTargetPosition;		// [相对玩家的] 移动目标位置
		Vector3 _targetPosition;                // 移动目标位置 (每帧更新)

		bool _collided;						// 发生碰撞


		/// <summary> 当前跟随玩家的机器人 </summary>
		public static RobotController playerFollower
		{
			get { return _playerFollower; }
		}


		/// <summary> 机器人是否活动状态，活动状态下无法被他人回收 </summary>
		public bool isActive
		{
			get { return _active; }
		}


		public override bool isAttackMode
		{
			get { return _active && base.isAttackMode; }
		}


		protected override void OnOwnerChange(PESkEntity oldOwner, PESkEntity newOwner)
		{
			base.OnOwnerChange(oldOwner, newOwner);

			if (oldOwner)
			{
				PeEntity entity = oldOwner.GetComponent<PeEntity>();

				if (PeCreature.Instance.mainPlayerId == entity.Id)
				{
					_playerFollower = null;
					if (onPlayerLoseRobot != null) onPlayerLoseRobot();
				}
			}
			if (newOwner)
			{
				PeEntity entity = newOwner.GetComponent<PeEntity>();

				if (PeCreature.Instance.mainPlayerId == entity.Id)
				{
					_playerFollower = this;
					if (onPlayerGetRobot != null) onPlayerGetRobot(itemObject, gameObject);
				}
			}
		}


        protected override AIMode defaultAIMode
		{
			get
			{
                return AIMode.Defence;
			}
		}


		// 初始化质量
		protected override float mass
		{
			get
			{
				return Mathf.Clamp(
					creationController.creationData.m_Attribute.m_Weight * PEVCConfig.instance.robotMassScale,
					PEVCConfig.instance.robotMinMass,
					PEVCConfig.instance.robotMaxMass);
			}
		}


		// 初始化质心位置
		protected override Vector3 centerOfMass
		{
			get { return Vector3.zero; }
		}


		// 初始化惯性张量系数
		protected override Vector3 inertiaTensorScale
		{
			get { return Vector3.one; }
		}


		// 初始化阻力系数
		protected override void InitDrags(
			out float standardDrag, out float underwaterDrag,
			out float standardAngularDrag, out float underwaterAngularDrag)
		{
			standardDrag = PEVCConfig.instance.robotStandardDrag;
			underwaterDrag = PEVCConfig.instance.robotUnderwaterDrag;
			standardAngularDrag = PEVCConfig.instance.robotStandardAngularDrag;
			underwaterAngularDrag = PEVCConfig.instance.robotUnderwaterAngularDrag;
		}


		protected override void InitOtherThings()
		{
			base.InitOtherThings();

			// 加个尾巴
			var trail = Instantiate(PEVCConfig.instance.robotTrail).transform;
			trail.SetParent(transform, false);
			_trail = trail.GetComponentInChildren<TrailRenderer>();

			// 物品操作
			gameObject.AddComponent<ItemScript>();
			gameObject.AddComponent<DragItemMousePickRobot>().Init(this);

			_controller = GetComponentInChildren<VCPRobotController>();
		}


		protected override float cureOwnerHpPerSecond
		{
			get
			{
				return _controller.cureOwnerHpPerSecond;
			}
		}


		protected override void FixedUpdate()
		{
			base.FixedUpdate();

			// 更新活动性
			_active = (energy > 0f && ownerSkEntity);
			rigidbody.useGravity = !_active;
			rigidbody.constraints = _active ? RigidbodyConstraints.None :
				(RigidbodyConstraints.FreezeAll & (~RigidbodyConstraints.FreezePositionY));

			if (!_active) ChangeOwner(null);
			else
			{
				if (isPlayerHost)
				{
					// 消耗能量
					ExpendEnergy(_controller.energyExpendSpeed * Time.deltaTime);

					// 更新攻击目标信息
					UpdateAttactTarget();

					// 更新移动目标
					_updateTargetPositionCountDown -= Time.deltaTime;
					if (_collided || _updateTargetPositionCountDown <= 0f)
					{
						// 卡主了？
						if (!creationController.visible &&
							(ownerEntity.position - transform.position).sqrMagnitude > 10000f)
						{
							FlashMove();
						}

						UpdateRelativeTargetPosition();

						_collided = false;

						_updateTargetPositionCountDown = UnityEngine.Random.Range(1f, 10f);
					}
					_targetPosition = ownerEntity.position + _relativeTargetPosition;

					// 更新行为
					UpdateBehaviour();

					// 修改拖尾参数
					_trail.time = Mathf.Clamp(rigidbody.velocity.sqrMagnitude, 0f, 2f);
				}
			}
		}


		void OnCollisionEnter()
		{
			_collided = true;
		}


		void FlashMove()
		{
			var camera = PETools.PEUtil.MainCamTransform;

			Vector3 center;
			if (ownerCarrier) center = ownerCarrier.creationController.boundsCenterInWorld;
			else center = ownerEntity.position + Vector3.up * 2f;

			Vector3 direction = Vector3.zero;
			float startDistance;
			RaycastHit hit;

			Vector3 movePoint;

			for (int i = 0; i < 16; i++)
			{
				if (i == 0)
				{
					direction.x = direction.z = (camera.position - center).magnitude;
					direction = (camera.TransformPoint(direction) - center).normalized;
				}
				else if (i == 1)
				{
					direction.x = -(direction.z = (camera.position - center).magnitude);
					direction = (camera.TransformPoint(direction) - center).normalized;
				}
				else
				{
					direction = UnityEngine.Random.onUnitSphere;
					direction.y = Mathf.Abs(direction.y);
				}

				if (ownerCarrier) startDistance = ownerCarrier.creationController.BoundsRadius;
				else startDistance = 0.5f;

				if (Physics.SphereCast(
					center + startDistance * direction,
					creationController.robotRadius,
					direction,
					out hit, 64f, PEVCConfig.instance.getOffLayerMask))
				{
					movePoint = hit.point - direction;
				}
				else
				{
					movePoint = center + 63f * direction;
				}

				if (Vector3.Dot((movePoint - camera.position).normalized, camera.forward) < 0.7f)
				{
					transform.position = movePoint;
					return;
				}
			}
		}


		void UpdateRelativeTargetPosition()
		{
			float radiansY;

			if (ownerCarrier)
			{
				// 在载具

				if (isAttackMode)
				{
					// 战斗状态
					_relativeTargetPosition = attackTargetPoint - ownerCarrier.creationController.boundsCenterInWorld;
					radiansY = Mathf.Atan2(_relativeTargetPosition.x, _relativeTargetPosition.z);
					radiansY += UnityEngine.Random.Range(-0.25f, 0.25f) * Mathf.PI;
				}
				else
				{
					// 非战斗状态
					radiansY = UnityEngine.Random.Range(-1f, 1f) * Mathf.PI;
				}

				float distance = PEVCConfig.instance.randomRobotDistance + ownerCarrier.creationController.BoundsRadius;

				_relativeTargetPosition.x = Mathf.Sin(radiansY) * distance;
				_relativeTargetPosition.z = Mathf.Cos(radiansY) * distance;
				_relativeTargetPosition.y = PEVCConfig.instance.randomRobotHeight + ownerCarrier.creationController.BoundsRadius;

				_relativeTargetPosition = _relativeTargetPosition + ownerCarrier.creationController.boundsCenterInWorld - ownerEntity.position;
			}
			else
			{
				// 不在载具

				if (isAttackMode)
				{
					// 战斗状态
					_relativeTargetPosition = attackTargetPoint - ownerEntity.position;
					radiansY = Mathf.Atan2(_relativeTargetPosition.x, _relativeTargetPosition.z);
					radiansY += UnityEngine.Random.Range(-0.25f, 0.25f) * Mathf.PI;
				}
				else
				{
					// 非战斗状态
					radiansY = UnityEngine.Random.Range(-1f, 1f) * Mathf.PI;
				}

				float distance = PEVCConfig.instance.randomRobotDistance;

				_relativeTargetPosition.x = Mathf.Sin(radiansY) * distance;
				_relativeTargetPosition.z = Mathf.Cos(radiansY) * distance;
				_relativeTargetPosition.y = PEVCConfig.instance.randomRobotHeight;
			}
		}


		void UpdateBehaviour()
		{
			// 目标速度
			Vector3 targetVelocity = (_targetPosition - transform.position) * PEVCConfig.instance.robotSpeedScale;
			if (targetVelocity.sqrMagnitude > PEVCConfig.instance.maxSqrRigidbodySpeed)
			{
				targetVelocity *= PEVCConfig.instance.maxRigidbodySpeed / targetVelocity.magnitude;
			}

			// 更新速度
			targetVelocity = Vector3.RotateTowards(
				rigidbody.velocity,
				targetVelocity,
				Time.deltaTime * PEVCConfig.instance.robotVelocityRotateSpeed,
				Time.deltaTime * PEVCConfig.instance.robotVelocityChangeSpeed);

			float swingTime01 = (Time.timeSinceLevelLoad % PEVCConfig.instance.robotSwingPeriod) / PEVCConfig.instance.robotSwingPeriod;

			rigidbody.velocity = targetVelocity + Mathf.Sin(swingTime01 * 2f * Mathf.PI) * PEVCConfig.instance.robotSwingRange * Vector3.up;

			// 目标旋转
			Quaternion targetRotation;

			if (isAttackMode)
			{
				targetRotation = Quaternion.LookRotation(attackTargetPoint - transform.position);
			}
			else
			{
				if (targetVelocity.x * targetVelocity.x + targetVelocity.z * targetVelocity.z > 0.25f)
				{
					targetRotation = Quaternion.LookRotation(targetVelocity);
				}
				else targetRotation = transform.rotation;
			}

			// 更新旋转
			transform.rotation = Quaternion.RotateTowards(
				transform.rotation,
				targetRotation,
				Time.deltaTime * PEVCConfig.instance.robotRotateSpeed);
		}


		protected override void OnHpChange(float deltaHp, bool isDead)
		{
			base.OnHpChange(deltaHp, isDead);

			if (isDead)
			{
				if (_playerFollower == this)
				{
					_playerFollower = null;
					if (onPlayerLoseRobot != null) onPlayerLoseRobot();
				}
			}
		}


#if UNITY_EDITOR

		void OnDrawGizmos()
		{
			if (_active && isPlayerHost)
			{
				Gizmos.color = Color.cyan;
				Gizmos.DrawSphere(_targetPosition, 0.1f);
			}
		}

#endif
	}
}