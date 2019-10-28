using UnityEngine;
using Pathea;
using SkillSystem;
using WhiteCat.BitwiseOperationExtension;

namespace WhiteCat
{
	public abstract class BehaviourController : MonoBehaviour
	{
		CreationController _creationController;
		ItemAsset.ItemObject _srcItem;
		ItemAsset.LifeLimit _lifeCmpt;

		ItemAsset.Energy _energyCmpt;
		PeEntity _creationPeEntity;
		CreationSkEntity _creationSkEntity;
		AudioSource _audioSource;

		bool _hasOwner = false;				// 是否有归属者
		PeEntity _ownerEntity;				// 归属者
		PESkEntity _ownerSkEntity;          // 归属者

        bool _physicsEnabled = false;
        int _enablePhysicsRequest = 0;
        bool _networkEnabled = true;

		bool _isPlayerHost = false;			// 是否为主控端
		VCPWeapon[] _weapons;               // 武器 (耗能)
		float _deltaEnergy = 0;             // 能量变化量 (非主控端也可以消耗)
		int _weaponGroupToggle = -1;			// 武器组开关
		int _weaponControlToggle;           // 武器控制开关

		Rigidbody _rigidbody;
		float _standardDrag = 0.1f;
		float _underwaterDrag = 1f;
		float _standardAngularDrag = 0.1f;
		float _underwaterAngularDrag = 1f;
		float _underWaterFactor = 0;
        float _speedScale;

		Vector3 _tempVelocityForLOD;
		Vector3 _tempAngularVelocityForLOD;


		public CreationController creationController { get { return _creationController; } }
		public ItemAsset.ItemObject itemObject { get { return _srcItem; } }
		public new Rigidbody rigidbody { get { return _rigidbody; } }
		public bool isPlayerHost { get { return _isPlayerHost; } }
		public float underWaterFactor { get { return _underWaterFactor; } }
        public float speedScale { get { return _speedScale; } }


		// 初始化质量
		protected abstract float mass { get; }

		// 初始化质心位置
		protected abstract Vector3 centerOfMass { get; }

		// 初始化惯性张量系数
		protected abstract Vector3 inertiaTensorScale { get; }

		// 初始化阻力系数
		protected abstract void InitDrags(
			out float standardDrag, out float underwaterDrag,
			out float standardAngularDrag, out float underwaterAngularDrag);

		// 初始化网络
		protected abstract void InitNetwork();

		// 初始化其他内容
		protected abstract void InitOtherThings();

		// 网络同步事件
		protected abstract void OnNetworkSync();

		// 所属变化事件
		protected virtual void OnOwnerChange(PESkEntity oldOwner, PESkEntity newOwner) { }

		// 是否在攻击模式下
		public abstract bool isAttackMode { get; }

		// 攻击目标
		public abstract SkEntity attackTargetEntity { get; }

		// 攻击目标点
		public abstract Vector3 attackTargetPoint { get; }


		// HP 变化事件
		protected virtual void OnHpChange(float deltaHp, bool isDead)
		{
			if (isDead)
			{
				DragItemAgent item = DragItemAgent.GetById(GetComponent<DragItemLogicCreation>().id);
				SceneMan.RemoveSceneObj(item);
				ItemAsset.ItemMgr.Instance.DestroyItem(itemObject);
			}
		}


		/// <summary>
		/// 仅在创建游戏实体时初始化, 拖拽效果不会调用
		/// </summary>
		public void InitController(int itemInstanceId)
		{
			_creationController = GetComponent<CreationController>();
			_srcItem = ItemAsset.ItemMgr.Instance.Get(itemInstanceId);
			_lifeCmpt = _srcItem.GetCmpt<ItemAsset.LifeLimit>();
			_energyCmpt = _srcItem.GetCmpt<ItemAsset.Energy>();

			// PeEntity

			int id = WorldInfoMgr.Instance.FetchNonRecordAutoId();
			_creationPeEntity = EntityMgr.Instance.InitEntity(id, gameObject);
			_creationPeEntity.carrier = this as CarrierController;

			// ViewCmpt
			var view = gameObject.AddComponent<CreationViewCmpt>();
			_creationPeEntity.viewCmpt = view;
			view.Init(creationController);

			// PeTrans

			PeTrans tr = gameObject.AddComponent<PeTrans>();
			_creationPeEntity.peTrans = tr;
			tr.SetModel(transform);
			tr.bound = _creationController.bounds;

			// SkEntity

			_creationSkEntity = gameObject.AddComponent<CreationSkEntity>();
			_creationSkEntity.Init(this);
			_creationPeEntity.skEntity = _creationSkEntity;
			_creationSkEntity.m_Attrs = new PESkEntity.Attr[5];
			for(int i=0; i < _creationSkEntity.m_Attrs.Length; i++)
			{
				_creationSkEntity.m_Attrs[i] = new PESkEntity.Attr();
			}

			_creationSkEntity.m_Attrs[0].m_Type = AttribType.Hp;
			_creationSkEntity.m_Attrs[0].m_Value = _lifeCmpt.floatValue.current;

			_creationSkEntity.m_Attrs[1].m_Type = AttribType.HpMax;
			_creationSkEntity.m_Attrs[1].m_Value = _lifeCmpt.valueMax;

			_creationSkEntity.m_Attrs[2].m_Type = AttribType.CampID;
			_creationSkEntity.m_Attrs[2].m_Value = 0;

			_creationSkEntity.m_Attrs[3].m_Type = AttribType.DamageID;
			_creationSkEntity.m_Attrs[3].m_Value = 0;

			_creationSkEntity.m_Attrs[4].m_Type = AttribType.DefaultPlayerID;
			_creationSkEntity.m_Attrs[4].m_Value = 99;

			_creationSkEntity.onHpChange += (skEntity, deltaHp) =>
			{
				_lifeCmpt.floatValue.current = _creationSkEntity.GetAttribute(AttribType.Hp);
				OnHpChange(deltaHp, _lifeCmpt.floatValue.current <= 0);
			};

			_creationSkEntity.InitEntity();

			// Physics

			_rigidbody = gameObject.AddComponent<Rigidbody>();
			_rigidbody.mass = mass;
			_rigidbody.centerOfMass = centerOfMass;
			// 椭球体的惯性张量 作为基数
			var size = creationController.bounds.size;
			_rigidbody.inertiaTensor = Vector3.Scale(
				inertiaTensorScale,
				_rigidbody.mass * 0.05f * new Vector3(
					size.y * size.y + size.z * size.z,
					size.x * size.x + size.z * size.z,
					size.x * size.x + size.y * size.y));
			_rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
			_rigidbody.interpolation = RigidbodyInterpolation.None;
			_rigidbody.useGravity = true;
			_rigidbody.isKinematic = true;
            _rigidbody.maxAngularVelocity = PEVCConfig.instance.maxRigidbodyAngularSpeed;
			InitDrags(out _standardDrag, out _underwaterDrag, out _standardAngularDrag, out _underwaterAngularDrag);

			// Network

			InitNetwork();

			// Weapons

			LoadParts(ref _weapons);
			for (int i = 0; i < _weapons.Length; i++)
			{
				_weapons[i].Init(i);
			}

			SetOwner(null);

			// InitOtherThings
			InitOtherThings();

			// Damage Controller
			gameObject.AddComponent<CreationDamageController>().Init(this);

			// LOD
			gameObject.AddComponent<DragCreationLodCmpt>();
			gameObject.AddComponent<DragItemLogicCreation>();
		}


		/// <summary> 获取炮弹数据块 </summary>
		public IProjectileData GetProjectileData(ISkPara param)
		{
			return _weapons[(param as SkCarrierCanonPara)._idxCanon];
		}


		/// <summary> 发动技能(武器) </summary>
		public void StartSkill(int skillId, ISkPara para)
		{
			_creationSkEntity.StartSkill(attackTargetEntity, skillId, para);
		}


		/// <summary> CreationPeEntity </summary>
		 public PeEntity creationPeEntity { get { return _creationPeEntity; } }


		/// <summary> CreationEntity </summary>
		public CreationSkEntity creationSkEntity { get { return _creationSkEntity; } }


		/// <summary> 归属者 Entity </summary>
		public PeEntity ownerEntity { get { return _ownerEntity; } }


		/// <summary> 归属者 Entity </summary>
		public PESkEntity ownerSkEntity { get { return _ownerSkEntity; } }


		/// <summary> 获取武器对象 </summary>
		public VCPWeapon GetWeapon(ISkPara para)
		{
			return _weapons[(para as SkCarrierCanonPara)._idxCanon];
		}

        public VCPWeapon GetWeapon(int index = 0)
        {
            return _weapons[index];
        }

		/// <summary> 武器组开关 </summary>
		public bool IsWeaponGroupEnabled(int index)
		{
			return _weaponGroupToggle.GetBit(index);
		}


		/// <summary> 武器控制开关 </summary>
		public bool IsWeaponControlEnabled(WeaponType type)
		{
			return _weaponControlToggle.GetBit((int)type);
		}


		/// <summary> 销毁 </summary>
		public void Destroy()
		{
			PeCreature.Instance.Destory(_creationPeEntity.Id); 
		}


		/// <summary> HP </summary>
		public float hp
		{
			get { return _lifeCmpt.floatValue.current; }
		}


		/// <summary> Max HP </summary>
		public float maxHp
		{
			get { return _creationController.creationData.m_Attribute.m_Durability; }
		}


		/// <summary> HP 百分比 </summary>
		public float hpPercentage
		{
			get { return _lifeCmpt.floatValue.percent; }
		}


		/// <summary> 是否死亡 </summary>
		public bool isDead
		{
			get { return _lifeCmpt.floatValue.current <= 0f; }
		}


		/// <summary> energy </summary>
		public float energy
		{
			get { return _energyCmpt.floatValue.current; }
		}


		/// <summary> Max Energy </summary>
		public float maxEnergy
		{
			get { return _creationController.creationData.m_Attribute.m_MaxFuel; }
		}


		/// <summary> 能量是否足够执行一个消耗(expend为正) </summary>
		public bool isEnergyEnough(float expend)
		{
			return _energyCmpt.floatValue.current + _deltaEnergy - expend >= 0;
		}


		/// <summary> 消耗能量(expend为正) </summary>
		public void ExpendEnergy(float expend)
		{
			_deltaEnergy -= expend;
		}


		/// <summary> 重置能量变化量 </summary>
		public void GetAndResetDeltaEnergy(ref float delta)
		{
			delta = _deltaEnergy;
			_deltaEnergy = 0;
		}


		/// <summary> 直接设置能量值 </summary>
		public void SetEnergy(float energy)
		{
			_energyCmpt.floatValue.current = energy;
		}

		
		protected void ChangeOwner(PeEntity owner)
		{
			if (_ownerEntity != owner)
			{
				SetOwner(owner);
			}
		}

        public bool IsController()
        {
            if (_creationPeEntity != null && _creationPeEntity.netCmpt != null)
                return _creationPeEntity.netCmpt.IsController;

            return false;
        }


		/// <summary> 设置归属者(攻击判定相关) </summary>
		protected void SetOwner(PeEntity owner)
		{
			_ownerEntity = owner;
			_hasOwner = owner;

			PESkEntity oldOwner = _ownerSkEntity;

			if (_hasOwner)
			{
				_ownerSkEntity = owner.GetComponent<PESkEntity>();
				_creationSkEntity.SetAttribute(AttribType.CampID, 5);
				_creationSkEntity.SetAttribute(AttribType.DamageID, 5);
				_creationSkEntity.SetAttribute(AttribType.DefaultPlayerID, owner.GetAttribute(AttribType.DefaultPlayerID));
			}
			else
			{
				_ownerSkEntity = null;
				_creationSkEntity.SetAttribute(AttribType.CampID, 0);
				_creationSkEntity.SetAttribute(AttribType.DamageID, 0);
				_creationSkEntity.SetAttribute(AttribType.DefaultPlayerID, 99);
			}

			OnOwnerChange(oldOwner, _ownerSkEntity);
		}


		/// <summary> 重置主控端 </summary>
		public void ResetHost(int controllerId)
		{
			if (_isPlayerHost != (controllerId == PeCreature.Instance.mainPlayer.Id))
			{
				_isPlayerHost = !_isPlayerHost;

				if (_isPlayerHost) _isFreezed = 0;
				else _isFreezed = 0xFF;
			}
		}


		/// <summary> AudioSource </summary>
		public AudioSource audioSource
		{
			get
			{
				if (!_audioSource)
				{
					_audioSource = _creationController.centerObject.gameObject.AddComponent<AudioSource>();
					_audioSource.spatialBlend = 1f;
				}
				return _audioSource;
			}
		}


		/// <summary> 设置武器组开关 </summary>
		protected void ReverseWeaponGroupEnabled(int index)
		{
			_weaponGroupToggle = _weaponGroupToggle.ReverseBit(index);
		}


		/// <summary> 设置武器控制开关 </summary>
		protected void SetWeaponControlEnabled(WeaponType type, bool enabled)
		{
			_weaponControlToggle = _weaponControlToggle.SetBit((int)type, enabled);
		}


		protected void DisableAllWeaponControl()
		{
			_weaponControlToggle = 0;
		}


		/// <summary> 物理更新 </summary>
		protected virtual void FixedUpdate()
		{
            CheckPhysicsEnabled();

            if (_hasOwner && _ownerEntity == null)
			{
				SetOwner(null);
			}

			// 单人模式下主动消耗能量
			if (!GameConfig.IsMultiMode)
			{
                if(_energyCmpt != null)
				    _energyCmpt.floatValue.current += _deltaEnergy;

				_deltaEnergy = 0;
			}

			// 限制极速

			float sqrMagnitude = rigidbody.velocity.sqrMagnitude;
			if (sqrMagnitude > PEVCConfig.instance.maxSqrRigidbodySpeed)
			{
				rigidbody.velocity = rigidbody.velocity * (PEVCConfig.instance.maxRigidbodySpeed / Mathf.Sqrt(sqrMagnitude));
			}

			// 计算阻力系数

			var extend = _creationController.bounds.extents;
			var center = _creationController.bounds.center;
			int count = 0;

			var point = center + extend;
			if (VFVoxelWater.self.IsInWater(transform.TransformPoint(point))) count++;
			point.x = center.x - extend.x;
			if (VFVoxelWater.self.IsInWater(transform.TransformPoint(point))) count++;
			point.y = center.y - extend.y;
			if (VFVoxelWater.self.IsInWater(transform.TransformPoint(point))) count++;
			point.x = center.x + extend.x;
			if (VFVoxelWater.self.IsInWater(transform.TransformPoint(point))) count++;

			point = center - extend;
			if (VFVoxelWater.self.IsInWater(transform.TransformPoint(point))) count++;
			point.x = center.x + extend.x;
			if (VFVoxelWater.self.IsInWater(transform.TransformPoint(point))) count++;
			point.y = center.y + extend.y;
			if (VFVoxelWater.self.IsInWater(transform.TransformPoint(point))) count++;
			point.x = center.x - extend.x;
			if (VFVoxelWater.self.IsInWater(transform.TransformPoint(point))) count++;

			_underWaterFactor = count / 8f;

			rigidbody.drag = underWaterFactor * (_underwaterDrag - _standardDrag) + _standardDrag;
			rigidbody.angularDrag = underWaterFactor * (_underwaterAngularDrag - _standardAngularDrag) + _standardAngularDrag;

            // 速度控制
            _speedScale = PEVCConfig.instance.speedScaleCurve.Evaluate(Vector3.ProjectOnPlane(rigidbody.velocity, Vector3.up).magnitude);

			if (_networkEnabled) UpdateNetwork();
		}


        public bool physicsEnabled
        {
            get
            {
                return _physicsEnabled;
            }
            set
            {
                if (_physicsEnabled != value)
                {
                    _physicsEnabled = value;

                    if (value)
                    {
                        if (!_networkEnabled) return;

                        _enablePhysicsRequest = 2;
                    }
                    else
                    {
                        _enablePhysicsRequest = 0;

                        _creationController.AddBuildFinishedListener(
                            () =>
                            {
                                _tempVelocityForLOD = rigidbody.velocity;
                                _tempAngularVelocityForLOD = rigidbody.angularVelocity;
                                rigidbody.isKinematic = true;
                            }
                        );
                    }
                }
            }
        }


        void CheckPhysicsEnabled()
        {
            var player = MainPlayer.Instance == null ? null : MainPlayer.Instance.entity;
            if (_enablePhysicsRequest != 0 && player != null && player.peTrans != null
                && (player.position - transform.position).sqrMagnitude < 10000f
                )
            {
                _enablePhysicsRequest--;
                if (_enablePhysicsRequest == 0)
                {
                    _creationController.AddBuildFinishedListener(
                        () =>
                        {
                            rigidbody.isKinematic = false;
                            rigidbody.velocity = _tempVelocityForLOD;
                            rigidbody.angularVelocity = _tempAngularVelocityForLOD;
                        }
                    );
                }
            }
        }


		public void LoadPart<T>(ref T member) where T : VCPart
		{
			member = _creationController.partRoot.GetComponentInChildren<T>();
		}


		public void LoadParts<T>(ref T[] lists) where T : VCPart
		{
			lists = _creationController.partRoot.GetComponentsInChildren<T>(true);
		}


		#region Network ------------------------

		const int _buffersCount = 17;
		static byte[][] _buffer = new byte[_buffersCount][];

		protected NetData<Vector3> _netPosition;
		protected NetData<Quaternion> _netRotation;
		protected NetData<Vector3> _netVelocity;
		protected NetData<Vector3> _netAngularVelocity;
		protected NetData<Vector3> _netAimPoint;
		protected NetData<ushort> _netInput;

		byte _isFreezed = 0xFF;


		static BehaviourController()
		{
			for (int i = 0; i < _buffersCount; i++)
			{
				_buffer[i] = new byte[(i + 1) * 4];
			}
		}


		static byte[] GetShortestBuffer(int byteCount)
		{
			int index = byteCount / 4;
			if (byteCount % 4 == 0) index -= 1;
			return _buffer[index];
		}


        public bool networkEnabled
        {
            get { return _networkEnabled; }
            set { _networkEnabled = value; }
        }


		void UpdateNetwork()
		{
			if (PeGameMgr.IsMulti && !_isPlayerHost)
			{
				// position

				byte mask = 1;

                if (_rigidbody.isKinematic)
                {
                    _rigidbody.position = Vector3.Lerp(
                        _rigidbody.position,
                        _netPosition.lastData,
                        PEVCConfig.instance.netDataApplyDamping * Time.deltaTime);
                }
                else if ((_isFreezed & mask) != 0)
                {
                    _rigidbody.position = _netPosition.lastData;
                    _rigidbody.velocity = Vector3.zero;
                    _rigidbody.drag = float.MaxValue;
                }

                // rotation

                mask <<= 1;

                if (_rigidbody.isKinematic)
                {
                    _rigidbody.rotation = Quaternion.Slerp(
                        _rigidbody.rotation,
                        _netRotation.lastData,
                        PEVCConfig.instance.netDataApplyDamping * Time.deltaTime);
                }
                else if ((_isFreezed & mask) != 0)
                {
                    _rigidbody.rotation = _netRotation.lastData;
                    rigidbody.angularVelocity = Vector3.zero;
                    rigidbody.angularDrag = float.MaxValue;
                }
			}
		}


		/// <summary>
		/// 主控端发送数据
		/// </summary>
		/// <returns> 如果不需要发送，返回 null </returns>
		public byte[] C2S_GetData()
		{
			byte needSync = 0;
			byte freezeCmd = 0;
			int byteCount = 1;

			// position
			byte mask = 1;
			SyncAction action = _netPosition.GetSyncAction();
			if (action != SyncAction.none)
			{
				needSync |= mask;
				if (action == SyncAction.freeze) freezeCmd |= mask;
				byteCount += Kit.sizeOfVector3;
			}

			// rotation
			mask <<= 1;
			action = _netRotation.GetSyncAction();
			if (action != SyncAction.none)
			{
				needSync |= mask;
				if (action == SyncAction.freeze) freezeCmd |= mask;
				byteCount += Kit.sizeOfQuaternion;
			}

			// velocity
			mask <<= 1;
			action = _netVelocity.GetSyncAction();
			if (action != SyncAction.none)
			{
				needSync |= mask;
				if (action == SyncAction.freeze) freezeCmd |= mask;
				byteCount += Kit.sizeOfVector3;
			}

			// angularVelocity
			mask <<= 1;
			action = _netAngularVelocity.GetSyncAction();
			if (action != SyncAction.none)
			{
				needSync |= mask;
				if (action == SyncAction.freeze) freezeCmd |= mask;
				byteCount += Kit.sizeOfVector3;
			}

			// aimPoint
			mask <<= 1;
			action = _netAimPoint.GetSyncAction();
			if (action != SyncAction.none)
			{
				needSync |= mask;
				if (action == SyncAction.freeze) freezeCmd |= mask;
				byteCount += Kit.sizeOfVector3;
			}

			// input
			mask <<= 1;
			action = _netInput.GetSyncAction();
			if (action != SyncAction.none)
			{
				needSync |= mask;
				if (action == SyncAction.freeze) freezeCmd |= mask;
				byteCount += Kit.sizeOfUshort;
			}

			// freezeCmd
			mask <<= 1;
			if (freezeCmd != 0)
			{
				needSync |= mask;
				byteCount += 1;
			}

			byte[] buffer = null;

			if (needSync != 0)
			{
				buffer = GetShortestBuffer(byteCount);
				int offset = 0;

				UnionValue union = new UnionValue(needSync);
				union.WriteByteTo(buffer, ref offset);

				// position
				mask = 1;
				if ((needSync & mask) != 0)
					Kit.WriteToBuffer(buffer, ref offset, _netPosition.GetData());

				// rotation
				mask <<= 1;
				if ((needSync & mask) != 0)
					Kit.WriteToBuffer(buffer, ref offset, _netRotation.GetData());

				// velocity
				mask <<= 1;
				if ((needSync & mask) != 0)
					Kit.WriteToBuffer(buffer, ref offset, _netVelocity.GetData());

				// angularVelocity
				mask <<= 1;
				if ((needSync & mask) != 0)
					Kit.WriteToBuffer(buffer, ref offset, _netAngularVelocity.GetData());

				// aimPoint
				mask <<= 1;
				if ((needSync & mask) != 0)
					Kit.WriteToBuffer(buffer, ref offset, _netAimPoint.GetData());

				// input
				mask <<= 1;
				if ((needSync & mask) != 0)
				{
					union.ushortValue = _netInput.GetData();
					union.WriteUShortTo(buffer, ref offset);
				}

				// freezeCmd
				mask <<= 1;
				if ((needSync & mask) != 0)
				{
					union.byteValue = freezeCmd;
					union.WriteByteTo(buffer, ref offset);
				}
			}

			return buffer;
		}


		/// <summary>
		/// 非主控端接收数据
		/// </summary>
		public void S2C_SetData(byte[] data)
		{
#if UNITY_EDITOR
			//_syncCount ++;
			//_syncData = data;
#endif

			int offset = 0;
			UnionValue union = new UnionValue();

			// needSync
			union.ReadByteFrom(data, ref offset);
			byte needSync = union.byteValue;

			// position
			byte mask = 1;
			if ((needSync & mask) != 0)
			{
				_netPosition.SetData(Kit.ReadVector3FromBuffer(data, ref offset));
				_isFreezed = (byte)(_isFreezed & (~mask));
			}

			// rotation
			mask <<= 1;
			if ((needSync & mask) != 0)
			{
				_netRotation.SetData(Kit.ReadQuaternionFromBuffer(data, ref offset));
				_isFreezed = (byte)(_isFreezed & (~mask));
			}

			// velocity
			mask <<= 1;
			if ((needSync & mask) != 0)
			{
				_netVelocity.SetData(Kit.ReadVector3FromBuffer(data, ref offset));
				_isFreezed = (byte)(_isFreezed & (~mask));
			}

			// angularVelocity
			mask <<= 1;
			if ((needSync & mask) != 0)
			{
				_netAngularVelocity.SetData(Kit.ReadVector3FromBuffer(data, ref offset));
				_isFreezed = (byte)(_isFreezed & (~mask));
			}

			// aimPoint
			mask <<= 1;
			if ((needSync & mask) != 0)
			{
				_netAimPoint.SetData(Kit.ReadVector3FromBuffer(data, ref offset));
				_isFreezed = (byte)(_isFreezed & (~mask));
			}

			// input
			mask <<= 1;
			if ((needSync & mask) != 0)
			{
				union.ReadUShortFrom(data, ref offset);
				_netInput.SetData(union.ushortValue);
				_isFreezed = (byte)(_isFreezed & (~mask));
			}

			// freezeCmd
			mask <<= 1;
			if ((needSync & mask) != 0)
			{
				union.ReadByteFrom(data, ref offset);
				byte freezeCmd = union.byteValue;

				mask = 1;
				for (int i = 0; i < 8; i++)
				{
					if ((freezeCmd & mask) != 0) _isFreezed |= mask;
					mask <<= 1;
				}
			}

			OnNetworkSync();
		}


		#endregion -----------------------------

#if UNITY_EDITOR

		//float _syncTime = 0;
		//int _syncCount = 0;
		//int _syncSpeed = 0;
		//byte[] _syncData;

		//bool _showSyncData;

		protected virtual void OnGUI()
		{
			//if (!isPlayerHost && false)
			//{
			//	if (Time.unscaledTime - _syncTime >= 1f)
			//	{
			//		_syncSpeed = _syncCount;
			//		_syncCount = 0;
			//		_syncTime = Time.unscaledTime;
			//	}

			//	Rect r = new Rect(Screen.width / 2 - 100, 10, 200, 21);
			//	UnityEditor.EditorGUI.DrawRect(r, new Color(0, 0, 0, 0.5f));
			//	GUI.Label(r, _syncSpeed.ToString() + " /s");

			//	r.y = r.yMax + 1;
			//	_showSyncData = GUI.Toggle(r, _showSyncData, "Show Sync Data", UnityEditor.EditorStyles.miniButton);

			//	if (_showSyncData && _syncData != null && _syncData.Length != 0)
			//	{
			//		r.y = r.yMax + 1;
			//		r.height = _syncData.Length * 21 + 42;
			//		UnityEditor.EditorGUI.DrawRect(r, new Color(0, 0, 0, 0.5f));
			//		GUI.Label(r, WhiteCat.ArrayExtension.ArrayExtension.GetContentString(_syncData));
			//	}
			//}
		}

#endif
	}
}