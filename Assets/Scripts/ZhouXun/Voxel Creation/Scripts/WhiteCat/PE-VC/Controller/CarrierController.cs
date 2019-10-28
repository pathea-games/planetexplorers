using UnityEngine;
using Pathea;
using WhiteCat.BitwiseOperationExtension;
using SkillSystem;
using System;

namespace WhiteCat
{
	public abstract class CarrierController : BehaviourController, IPeMsg
	{
        #region Fields ---------------------------------------------------------
        static int ImpactTargetSkillID = 30100562;
        //static int ImpactSelfSkillID   = 20110054;

        VCPCockpit _cockpit;                // 驾驶舱
		VCPSideSeat[] _sideSeats;           // 普通座位
		VCPJetExhaust[] _jetExhausts;       // 喷射器 (耗能)
		VCPCarrierLight[] _lights;          // 灯 (耗能)

		int _passengerCount = 0;            // 乘客总数
		uint _inputState = 0;               // 输入状态 (同步)

		PESkEntity _aimEntity;				// 瞄准的目标
		Vector3 _aimPoint;					// 瞄准的点

		float _timeToLock = -1f;            // 锁定目标需要的时间
		PESkEntity _targetToLock;           // 将要锁定的目标
		PESkEntity _lockedTarget;           // 当前锁定的目标
		ViewCmpt _targetViewToLock;         // 将要锁定目标的 view
		ViewCmpt _lockedTargetView;         // 当前锁定目标的 view

		float _inputX = 0;                  // X 轴输入 [-1, 1] (位同步)
		float _inputY = 0;                  // Y 轴输入 [-1, 1] (位同步)
		float _inputVertical = 0;           // 竖直方向输入(扩展位同步)
		bool _autoDrive = false;
		bool _isJetting = false;            // 是否喷射 (同步)
		bool _isLightOn = false;            // 是否开灯 (同步)
		bool _isAttackMode = false;         // 是否攻击模式 (同步)

		float _jetRestValue = 1f;           // 喷射器剩余值
		float _jetCDTime = 0;               // 喷射器 CD 值

		static CarrierController _playerDriving;	// 玩家当前驾驶的载具
		static GameObject _attackUICanvas;

		static Ray _ray;
		const float _rayMaxDistance = 300f;

		// Input Param --------------------

		public const float inputSensitivity = 4f;
		// Common Input
		public const int moveForwardBit = 0;
		public const int moveBackwardBit = 1;
		public const int moveLeftBit = 2;
		public const int moveRightBit = 3;
		public const int jetBit = 4;
		public const int lightBit = 5;
		public const int attackModeBit = 6;
		// Vehicle
		public const int brakeBit = 7;
		// Helicopter
		public const int moveUpBit = 8;
		public const int moveDownBit = 9;

        static PeInput.LogicFunction[] _weaponGroupKey = new PeInput.LogicFunction[]
		{
			PeInput.LogicFunction.VehicleWeaponGrp1,
			PeInput.LogicFunction.VehicleWeaponGrp2,
			PeInput.LogicFunction.VehicleWeaponGrp3,
			PeInput.LogicFunction.VehicleWeaponGrp4,
		};


		public static CarrierController playerDriving { get { return _playerDriving; } }
		public float inputX { get { return _inputX; } }
		public float inputY { get { return _inputY; } }
		public float inputVertical { get { return _inputVertical; } }
		public bool isJetting { get { return _isJetting; } }
		public bool isLightOn { get { return _isLightOn; } }
		public PESkEntity lockedTarget { get { return _lockedTarget; } }
		public PESkEntity targetToLock { get { return _targetToLock; } }
		public float timeToLock { get { return _timeToLock; } }
		public PESkEntity aimEntity { get { return _aimEntity; } }

		#endregion Fields

		#region Methods --------------------------------------------------------

		/// <summary> 普通座位 </summary>
		public VCPSideSeat[] sideSeats { get { return _sideSeats; } }

		/// <summary> 是否有驾驶员 </summary>
		public bool hasDriver { get { return _cockpit.passenger != null && !_cockpit.passenger.Equals(null); } }

		/// <summary> 玩家是否是驾驶员 </summary>
		public bool isPlayerDriver { get { return _playerDriving == this; } }

		/// <summary> 乘客总数（包含驾驶员） </summary>
		public int passengerCount { get { return _passengerCount; } }

		/// <summary> 查找空座位下标, 返回 -1 表示驾驶舱, -2 表示已经满了 </summary>
		public int FindEmptySeatIndex()
		{
			if (_cockpit.passenger == null || _cockpit.passenger.Equals(null)) return -1;
			for (int i = 0; i < _sideSeats.Length; i++)
			{
				if (_sideSeats[i].passenger == null || _sideSeats[i].passenger.Equals(null)) return i;
			}
			return -2;
		}

		/// <summary> 乘客上载具 </summary>
		/// <param name="seatIndex"> 座位序号, -1 表示驾驶舱, 非负值表示普通座位序号 </param>
		public void GetOn(PeEntity entity, int seatIndex)
		{
			VCPBaseSeat seat = null;

			// 驾驶舱
			if (seatIndex < 0)
			{
				seat = _cockpit;

				ChangeOwner(entity);               // 设置归属者(攻击判定相关)
				ResetHost(entity.Id);     // 重置主控端

				// 玩家坐上驾驶舱
				if (isPlayerHost)
				{
					_playerDriving = this;

					UIDrivingCtrl.Instance.Show(
						() => maxHp, () => hp, () => maxEnergy, () => energy,
						() => { var v = rigidbody.velocity; return Mathf.Sqrt(v.x * v.x + v.z * v.z) * 3.6f; },
						() => _jetExhausts.Length > 0 ? _jetRestValue : 0);

					seat.getOffCallback += () =>
						{
							_playerDriving = null;
							UIDrivingCtrl.Instance.Hide();
							UIDrivingCtrl.Instance.SetWweaponGroupTogglesVisible(false, this);
							UISightingTelescope.Instance.Show(UISightingTelescope.SightingType.Null);
						};

					// 教程
					TutorialData.AddActiveTutorialID(TutorialData.GetOnVehicle);
				}

				// 驾驶员离开驾驶舱
				seat.getOffCallback += () =>
				{
					_inputState = OnDriverGetOff(_inputState);

                    _aimEntity = null;
					_lockedTarget = null;
					_timeToLock = -1f;
					_isAttackMode = false;
					DisableAllWeaponControl();

                    //终止撞击伤害技能
                    ActivateImpactDamage(false);
				};

                //单机模式玩家上车直接释放撞击伤害技能
                if (!GameConfig.IsMultiMode || IsController())
                    ActivateImpactDamage(true);
			}
			// 普通座位
			else
			{
				seat = _sideSeats[seatIndex];
			}

			_passengerCount += 1;

			seat.GetOn(entity.GetCmpt<PassengerCmpt>());

			// 当所有乘客都下车, 车的归属恢复默认
			seat.getOffCallback += () =>
			{
				_passengerCount -= 1;
				if (_passengerCount == 0)
				{
					ChangeOwner(null);
				}
			};
		}

		/// <summary> 遍历乘客 </summary>
		/// <param name="action"> PESkEntity: 乘客对象；bool：是否为驾驶员 </param>
		public void ForeachPassenger(System.Action<PESkEntity, bool> action)
		{
			PESkEntity passenger;
			
			PassengerCmpt passengerCmpt = _cockpit.passenger as PassengerCmpt;
			if (passengerCmpt != null)
			{
				passenger = passengerCmpt.GetComponent<PESkEntity>();
				if (passenger != null) action(passenger, true);
			}

			for (int i=0; i<_sideSeats.Length;i++)
			{
				passengerCmpt = _sideSeats[i].passenger as PassengerCmpt;
				if (passengerCmpt != null)
				{
					passenger = passengerCmpt.GetComponent<PESkEntity>();
					if (passenger != null) action(passenger, false);
				}
			}
		}


        /// <summary> 遍历乘客 </summary>
		/// <param name="action"> PESkEntity: 乘客对象；bool：是否为驾驶员 </param>
		public void ForeachPassenger(System.Action<PassengerCmpt, bool> action)
		{
			PassengerCmpt passengerCmpt = _cockpit.passenger as PassengerCmpt;
			if (passengerCmpt != null)
			{
				action(passengerCmpt, true);
			}

			for (int i=0; i<_sideSeats.Length;i++)
			{
				passengerCmpt = _sideSeats[i].passenger as PassengerCmpt;
				if (passengerCmpt != null)
				{
					action(passengerCmpt, false);
				}
			}
		}


        /// <summary>
        /// 
        /// </summary>
        public PeEntity driver
        {
			get { return _cockpit.passenger != null && !_cockpit.passenger.Equals(null) ?
                    (_cockpit.passenger as PassengerCmpt).Entity : null;}
        }

		#endregion Methods

		#region implementation -------------------------------------------------

		// 初始化网络
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
				last => (ushort)(_inputState) != last,
				() => (ushort)(_inputState),
				value => _inputState = value
			);
		}

		// 初始化其他内容
		protected override void InitOtherThings()
		{
			gameObject.AddComponent<ImpactAtkTriggrer>();

			// 创建 UI
			if (!_attackUICanvas)
			{
				_attackUICanvas = Instantiate(PEVCConfig.instance.canvasObject);
			}

			// 物品操作
			gameObject.AddComponent<ItemScript_Carrier>();
			gameObject.AddComponent<DragItemMousePickCarrier>();

			// 初始化部件引用
			LoadPart(ref _cockpit);
			LoadParts(ref _sideSeats);
			LoadParts(ref _jetExhausts);
			LoadParts(ref _lights);

			// 激活灯光部件
			foreach (var light in _lights)
			{
				light.enabled = true;
			}

			// 激活喷射器组件
			float maxForce = PEVCConfig.instance.maxJetAccelerate * rigidbody.mass;
			foreach (var jet in _jetExhausts)
			{
				jet.Init(this, maxForce / _jetExhausts.Length, _jetExhausts.Length);
				jet.enabled = true;
			}

			// 删除椅子上的人模型
			_cockpit.DestroyHumanModel();
			foreach(var seat in _sideSeats)
			{
				seat.DestroyHumanModel();
			}

			// 添加地图显示
			PeMap.CarrierMark mark = new PeMap.CarrierMark();
			mark.carrierController = this;
			PeMap.LabelMgr.Instance.Add(mark);
			 
		}

		// HP 变化事件
		protected override void OnHpChange(float deltaHp, bool isDead)
		{
			base.OnHpChange(deltaHp, isDead);

			if (isPlayerHost)
			{
				if (isDead)
				{
					// 渣渣们去死吧!
					ForeachPassenger((passenger, isDriver) => passenger.Kill(false));
					RemoveMapMark();
				}
				else if (deltaHp < 0)
				{
					// 乘客受伤
					ForeachPassenger((passenger, isDriver) =>
					{
						float damage = passenger.GetAttribute(AttribType.HpMax)
						* (deltaHp / maxHp) * PEVCConfig.instance.randomPassengerDamage;
						passenger.SetAttribute(AttribType.Hp, passenger.GetAttribute(AttribType.Hp) + damage, false);
					});
				}
			}
		}

		// 网络同步事件
		protected override void OnNetworkSync()
		{
			_cockpit.SyncPassenger();
			for (int i = 0; i < _sideSeats.Length; i++)
			{
				_sideSeats[i].SyncPassenger();
			}
		}

		// 攻击目标
		public override SkEntity attackTargetEntity
		{
			get { return _lockedTarget; }
		}

		// 攻击目标点
		public override Vector3 attackTargetPoint
		{
			get { return _aimPoint; }
		}

		// 是否在攻击模式
		public override bool isAttackMode
		{
			get { return _isAttackMode; }
		}

		#endregion implementation

		#region Functions ------------------------------------------------------

		protected virtual uint OnDriverGetOff(uint inputState)
		{
			inputState = inputState.SetBit(moveForwardBit, false);
			inputState = inputState.SetBit(moveBackwardBit, false);
			inputState = inputState.SetBit(moveLeftBit, false);
			inputState = inputState.SetBit(moveRightBit, false);
			inputState = inputState.SetBit(jetBit, false);
			inputState = inputState.SetBit(attackModeBit, false);

			return inputState;
		}


		// 编码网络需要的输入数据
		protected virtual uint EncodeInput(uint inputState)
		{
			inputState = inputState.SetBit(moveForwardBit, PeInput.Get(PeInput.LogicFunction.MoveForward));
			inputState = inputState.SetBit(moveBackwardBit, PeInput.Get(PeInput.LogicFunction.MoveBackward));
			inputState = inputState.SetBit(moveLeftBit, PeInput.Get(PeInput.LogicFunction.MoveLeft));
			inputState = inputState.SetBit(moveRightBit, PeInput.Get(PeInput.LogicFunction.MoveRight));

			// 添加自动驾驶
			if (PeInput.Get(PeInput.LogicFunction.AutoRunOnOff)) _autoDrive = !_autoDrive;
			if (inputState.GetBit(moveBackwardBit)) _autoDrive = false;
			if (_autoDrive) inputState = inputState.SetBit1(moveForwardBit);

			if (PeInput.Get(PeInput.LogicFunction.SwitchLight)) inputState = inputState.ReverseBit(lightBit);
			if (PeInput.Get(PeInput.LogicFunction.Vehicle_AttackModeOnOff)) inputState = inputState.ReverseBit(attackModeBit);

			if (_isJetting)
			{
				_jetCDTime = PEVCConfig.instance.jetDecToIncInterval;

				if (PeInput.Get(PeInput.LogicFunction.Vehicle_Sprint) && energy > 0)
				{
					_jetRestValue = Mathf.Clamp01(_jetRestValue - PEVCConfig.instance.jetDecreaseSpeed * Time.deltaTime);
					_isJetting = (_jetRestValue > 0);
				}
				else
				{
					_isJetting = false;
				}
			}
			else
			{
				if (PeInput.Get(PeInput.LogicFunction.Vehicle_Sprint))
				{
					_isJetting = (_jetRestValue > 0);
				}
				else
				{
					_jetCDTime -= Time.deltaTime;
					if (_jetCDTime < 0) _jetCDTime = 0;
					if (_jetCDTime == 0)
					{
						_jetRestValue = Mathf.Clamp01(_jetRestValue + PEVCConfig.instance.jetIncreaseSpeed * Time.deltaTime);
					}
					_isJetting = false;
				}
			}

			inputState = inputState.SetBit(jetBit, _isJetting);

			inputState = inputState.SetBit(moveUpBit, PeInput.Get(PeInput.LogicFunction.Vehicle_LiftUp));
			inputState = inputState.SetBit(moveDownBit, PeInput.Get(PeInput.LogicFunction.Vehicle_LiftDown));

			return inputState;
		}


		// 解码输入
		protected virtual void DecodeInput(uint inputState)
		{
			float inputTarget = 0;
			inputTarget += inputState.GetBit(moveRightBit) ? 1f : 0f;
			inputTarget -= inputState.GetBit(moveLeftBit) ? 1f : 0f;
			_inputX = Mathf.MoveTowards(_inputX, inputTarget, inputSensitivity * Time.deltaTime);

			inputTarget = 0;
			inputTarget += inputState.GetBit(moveForwardBit) ? 1f : 0f;
			inputTarget -= inputState.GetBit(moveBackwardBit) ? 1f : 0f;
			_inputY = Mathf.MoveTowards(_inputY, inputTarget, inputSensitivity * Time.deltaTime);

			_isJetting = inputState.GetBit(jetBit);
			_isLightOn = inputState.GetBit(lightBit);

			if (_isAttackMode != inputState.GetBit(attackModeBit))
			{
				_isAttackMode = !_isAttackMode;
				if (_isAttackMode) EnterAttactMode();
				else ExitAttackMode();
			}

			inputTarget = 0;
			inputTarget += inputState.GetBit(moveUpBit) ? 1f : 0f;
			inputTarget -= inputState.GetBit(moveDownBit) ? 1f : 0f;
			_inputVertical = Mathf.MoveTowards(_inputVertical, inputTarget, inputSensitivity * Time.deltaTime);
		}


		// 处理非传输输入
		void HandleNormalInput()
		{
			if (_isAttackMode)
			{
				// 获取瞄准信息

				var hit = Physics.RaycastAll(_ray = PeCamera.mouseRay, _rayMaxDistance, PEVCConfig.instance.attackRayLayerMask);
				int selectedIndex = -1;

				if (hit.Length > 0)
				{
					float distance = float.MaxValue;

					for (int i = 0; i < hit.Length; i++)
					{
						if (!hit[i].collider.isTrigger && hit[i].distance < distance && !hit[i].transform.IsChildOf(transform))
						{
							selectedIndex = i;
                            distance = hit[i].distance;
                        }
					}
				}

				if (selectedIndex >= 0)
				{
					_aimPoint = hit[selectedIndex].point;
					_aimEntity = hit[selectedIndex].collider.GetComponentInParent<PESkEntity>();
					if (_aimEntity != null)
					{
						// 排除友方和死亡目标
						if (ForceSetting.Instance.AllyPlayer(
							(int)_aimEntity.GetAttribute(AttribType.DefaultPlayerID),
							(int)creationSkEntity.GetAttribute(AttribType.DefaultPlayerID)
							) || _aimEntity.isDead)
						{
							_aimEntity = null;
						}
					}
				}
				else
				{
					_aimPoint = _ray.direction * _rayMaxDistance + _ray.origin;
					_aimEntity = null;
				}

				UpdateLockTarget();

				// 导弹发射
				SetWeaponControlEnabled(WeaponType.Missile, PeInput.Get(PeInput.LogicFunction.MissleLaunch));

				// 炮发射
				if (PeInput.Get(PeInput.LogicFunction.Vehicle_BegFixedShooting))
				{
					SetWeaponControlEnabled(WeaponType.Cannon, true);
				}
				if (PeInput.Get(PeInput.LogicFunction.Vehicle_EndFixedShooting))
				{
					SetWeaponControlEnabled(WeaponType.Cannon, false);
				}

				// 枪发射
				if (PeInput.Get(PeInput.LogicFunction.Vehicle_BegUnfixedShooting))
				{
					SetWeaponControlEnabled(WeaponType.Gun, true);
				}
				if (PeInput.Get(PeInput.LogicFunction.Vehicle_EndUnfixedShooting))
				{
					SetWeaponControlEnabled(WeaponType.Gun, false);
				}

				// 武器组开关
				for (int i = 0; i < _weaponGroupKey.Length; i++)
				{
					if (PeInput.Get(_weaponGroupKey[i]))
					{
						ReverseWeaponGroupEnabled(i);
						UIDrivingCtrl.Instance.SetWweaponGroupToggles(i, IsWeaponGroupEnabled(i));
					}
				}
			}
			else
			{
				_timeToLock = -1f;
				_targetToLock = null;
				_lockedTarget = null;
				DisableAllWeaponControl();
			}
		}


        // 更新锁定目标
		void UpdateLockTarget()
		{
			// 锁定目标
			if (PeInput.Get(PeInput.LogicFunction.MissleTarget))
			{
				_lockedTarget = null;
				_lockedTargetView = null;
				if (_aimEntity)
				{
					_targetToLock = _aimEntity;
					_targetViewToLock = _aimEntity.GetComponent<ViewCmpt>();
					_timeToLock = PEVCConfig.instance.lockTargetDuration;
				}
				else
				{
					_targetToLock = null;
					_targetViewToLock = null;
					_timeToLock = -1;
				}
			}

			if (_timeToLock > 0f)
			{
				if (_targetToLock == _aimEntity && _targetToLock && (!_targetViewToLock || _targetViewToLock.hasView))
				{
					_timeToLock -= Time.deltaTime;
					if (_timeToLock <= 0f)
					{
						_lockedTarget = _targetToLock;
						_lockedTargetView = _targetViewToLock;
					}
				}
				else
				{
					_targetToLock = null;
					_targetViewToLock = null;
					_timeToLock = -1;
				}
			}
			else
			{
				if ((_lockedTarget && _lockedTarget.isDead) || (_lockedTargetView && !_lockedTargetView.hasView))
				{
					_lockedTarget = null;
					_lockedTargetView = null;
				}
			}
		}


		void EnterAttactMode()
		{
			if (isPlayerDriver)
			{
				UISightingTelescope.Instance.Show(UISightingTelescope.SightingType.Default);
				//GameUI.Instance.mUIMainMidCtrl.quickBarForbiden = true;
				UIDrivingCtrl.Instance.SetWweaponGroupTogglesVisible(true, this);
			}
		}


		void ExitAttackMode()
		{
			if (isPlayerDriver)
			{
				UISightingTelescope.Instance.Show(UISightingTelescope.SightingType.Null);
				//GameUI.Instance.mUIMainMidCtrl.quickBarForbiden = false;
				UIDrivingCtrl.Instance.SetWweaponGroupTogglesVisible(false, this);
			}
		}

        //开启/关闭撞击伤害技能
        void ActivateImpactDamage(bool isActive)
        {
            if (!isActive)
            {
                creationPeEntity.StopSkill(ImpactTargetSkillID);
                //creationPeEntity.StopSkill(ImpactSelfSkillID);
            }
            else
            {
                creationPeEntity.StopSkill(ImpactTargetSkillID);
                //creationPeEntity.StopSkill(ImpactSelfSkillID);

                creationPeEntity.StartSkill(null, ImpactTargetSkillID);
                //creationPeEntity.StartSkill(null, ImpactSelfSkillID);
            }
        }


		// 更新
		protected virtual void Update()
		{
			// 驾驶员需要监测输入
			if (isPlayerDriver)
			{
				// 编码需要传输的输入
				_inputState = EncodeInput(_inputState);

				// 应用解码的输入
				DecodeInput(_inputState);

				// 处理其他输入
				HandleNormalInput();
			}
			else DecodeInput(_inputState);

			// 同步方向盘旋转
			if (_cockpit) _cockpit.UpdateCockpit(_inputX, _inputY, hasDriver && isEnergyEnough(0.01f));
		}

        #endregion

        #region IPeMsg

		void Start()
		{
			PeEntity entity = GetComponent<PeEntity>();
			if(null != entity)
				entity.AddMsgListener(this);
		}

		void OnDestroy()
		{
			PeEntity entity = GetComponent<PeEntity>();
			if(null != entity)
				entity.RemoveMsgListener(this);
			RemoveMapMark();
		}

        public void OnMsg(EMsg msg, params object[] args)
        {
            switch (msg)
            {
                case EMsg.Net_Controller:
                    ActivateImpactDamage(true);
                    break;
                case EMsg.Net_Proxy:
                    ActivateImpactDamage(false);
                    break;
            }
        }
        #endregion

		void RemoveMapMark()
		{
			// 移除地图显示
			PeMap.CarrierMark mark = new PeMap.CarrierMark();
			mark.carrierController = this;
			PeMap.LabelMgr.Instance.Remove(mark);
		}
#if UNITY_EDITOR

        void OnDrawGizmos()
		{
			Gizmos.color = Color.magenta;
			Gizmos.DrawSphere(rigidbody.worldCenterOfMass, 0.1f);
		}
#endif
    }
}