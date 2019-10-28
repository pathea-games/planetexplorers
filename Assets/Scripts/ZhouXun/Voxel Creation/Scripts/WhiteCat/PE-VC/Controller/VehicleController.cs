using System.Collections.Generic;
using UnityEngine;
using WhiteCat.BitwiseOperationExtension;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WhiteCat
{
	public class VehicleController : CarrierController
	{
		VCPVehicleEngine _engine;		// 引擎
		VCPVehicleWheel[] _wheels;      // 轮胎
		//VCPVehicleWheel[] _steerWheels;
		VCPVehicleWheel[] _motorWheels;

		float _inputBrake = 1f;
		public float inputBrake { get { return _inputBrake; } }
		float _wheelRangeWidth;

		// 初始化质量
		protected override float mass
		{
			get
			{
				return Mathf.Clamp(
					creationController.creationData.m_Attribute.m_Weight * PEVCConfig.instance.vehicleMassScale,
					PEVCConfig.instance.vehicleMinMass,
					PEVCConfig.instance.vehicleMaxMass);
			}
		}

		// 初始化质心位置
		protected override Vector3 centerOfMass
		{
			get
			{
				return new Vector3(0, creationController.creationData.m_Attribute.m_CenterOfMass.y * 0.7f, creationController.bounds.size.z * 0f);
			}
		}

		// 初始化惯性张量系数
		protected override Vector3 inertiaTensorScale
		{
			get
			{
				return PEVCConfig.instance.vehicleInertiaTensorScale;
			}
		}

		// 初始化阻力系数
		protected override void InitDrags(
			out float standardDrag, out float underwaterDrag,
			out float standardAngularDrag, out float underwaterAngularDrag)
		{
			standardDrag = PEVCConfig.instance.vehicleStandardDrag;
			underwaterDrag = PEVCConfig.instance.vehicleUnderwaterDrag;
			standardAngularDrag = PEVCConfig.instance.vehicleStandardAngularDrag;
			underwaterAngularDrag = PEVCConfig.instance.vehicleUnderwaterAngularDrag;
		}


		protected override void InitOtherThings()
		{
			base.InitOtherThings();

			// 获取引用

			LoadPart(ref _engine);
			LoadParts(ref _wheels);

			List<VCPVehicleWheel> steerWheelsList = new List<VCPVehicleWheel>(_wheels.Length);
			List<VCPVehicleWheel> motorWheelsList = new List<VCPVehicleWheel>(_wheels.Length);

			foreach (var wheel in _wheels)
			{
				if (wheel.isSteerWheel) steerWheelsList.Add(wheel);
				if (wheel.isMotorWheel) motorWheelsList.Add(wheel);
			}

			//_steerWheels = steerWheelsList.ToArray();
			_motorWheels = motorWheelsList.ToArray();

			// 轮胎边界框

			Vector3 lp = _wheels[0].transform.localPosition;

			float maxZ = lp.z;
			float minZ = lp.z;
			float maxX = lp.x;
			float minX = lp.x;

			foreach (var wheel in _wheels)
			{
				lp = wheel.transform.localPosition;

				if (lp.z > maxZ) maxZ = lp.z;
				if (lp.z < minZ) minZ = lp.z;
				if (lp.x > maxX) maxX = lp.x;
				if (lp.x < minX) minX = lp.x;
			}

			_wheelRangeWidth = maxX - minX;

			// 初始化轮胎转角
			float angle = PEVCConfig.instance.maxWheelSteerAngle;

			foreach (var wheel in _wheels)
			{
				Vector3 loc = wheel.transform.localPosition;

				float steerZ = (maxZ - minZ < 0.1f) ? 1f :
					((loc.z - (maxZ + minZ) * 0.5f) / ((maxZ - minZ) * 0.5f));

				float radius = Interpolation.EaseOut(Mathf.Clamp01(_wheelRangeWidth * 0.125f)) 
					* PEVCConfig.instance.vehicleSteerRadiusExtend + PEVCConfig.instance.vehicleSteerRadiusBase;

				float steerXL = angle + Mathf.Asin((maxX - loc.x) * Mathf.Sin(angle) / radius);
				float steerXR = angle + Mathf.Asin((loc.x - minX) * Mathf.Sin(angle) / radius);

				wheel.Init(this, steerXL * steerZ, steerXR * steerZ);
			}
		}


		protected override void FixedUpdate()
		{
			base.FixedUpdate();

			if (rigidbody.isKinematic) return;

			// 稳定车辆
			if (_wheelRangeWidth < 0.5f)
			{
				Vector3 forward = transform.forward;
				Vector3 product = Vector3.Cross(Vector3.up, forward);

				if (product.sqrMagnitude > 0.01f && Vector3.Dot(transform.up, Vector3.up) > 0.25f)
				{
					product = Vector3.Cross(forward, product);
					product = transform.InverseTransformDirection(product);
					float angle = -Mathf.Atan2(product.x, product.y) * Mathf.Rad2Deg;
					angle -= inputX * PEVCConfig.instance.motorcycleBiasAngle * Mathf.Clamp01(Mathf.Abs(Vector3.Dot(rigidbody.velocity, forward)) * 0.1f);

					product = rigidbody.angularVelocity;
					rigidbody.angularVelocity = product + (1f - _wheelRangeWidth - _wheelRangeWidth) * (
						angle * PEVCConfig.instance.motorcycleBalanceHelp * forward - Vector3.Project(product, forward)
						);
				}
			}

			// 轮胎
			for (int i = 0; i < _wheels.Length; i++)
			{
				_wheels[i].OnFixedUpdate(rigidbody.mass / _wheels.Length);
			}
		}


		protected override void Update()
		{
			base.Update();

			// 引擎
			float rpm = 0f;
			for (int i = 0; i < _motorWheels.Length; i++)
			{
				rpm += _motorWheels[i].rpm;
			}
			float motorTorque = _engine.UpdateEngine(this, rpm / _motorWheels.Length);
			motorTorque /= _motorWheels.Length;

            var rotateFactor = PEVCConfig.instance.speedToRotateFactor.Evaluate(
                Vector3.Project(rigidbody.velocity, transform.forward).magnitude);

			for (int i = 0; i < _wheels.Length; i++)
			{
				_wheels[i].OnUpdate(motorTorque, rotateFactor);
			}
		}


		protected override uint EncodeInput(uint inputState)
		{
			inputState = base.EncodeInput(inputState);

			bool brake = PeInput.Get(PeInput.LogicFunction.Vehicle_Brake);

			// 默认输入为 0, 因此 0 代表刹车
			inputState = inputState.SetBit(brakeBit, !brake);
			return inputState;
		}


		protected override void DecodeInput(uint inputState)
		{
			base.DecodeInput(inputState);

			// 运动方向与朝向在一定范围内时用户按反方向需要转换为刹车

			float dot = Vector3.Dot(rigidbody.velocity, transform.forward);
			float inputTarget = 0f;

			if ((inputY < 0 && dot > 1) || (inputY > 0 && dot < -1))
			{
				inputTarget = 1;
			}
			else
			{
				bool brake = !inputState.GetBit(brakeBit);

				// 速度极小且静止时打开手刹
				if(!brake && !isJetting && Mathf.Abs(inputY) < 0.01f)
				{
					if(rigidbody.velocity.sqrMagnitude < 0.05f)
					{
						brake = true;
					}
				}

				inputTarget = brake ? 1f : 0f;
			}

			_inputBrake = Mathf.MoveTowards(_inputBrake, inputTarget, inputSensitivity * Time.deltaTime);
		}


		protected override uint OnDriverGetOff(uint inputState)
		{
			inputState = base.OnDriverGetOff(inputState);
			inputState = inputState.SetBit0(brakeBit);
			return inputState;
		}


#if UNITY_EDITOR

		bool showInfo = false;

		protected override void OnGUI()
		{
			base.OnGUI();

			if (playerDriving == this)
			{
				Color back = new Color(0, 0, 0, 0.5f);

				Rect rect = new Rect(50, 50, 200, 21);
				showInfo = GUI.Toggle(rect, showInfo, "Show Info", EditorStyles.miniButton);

				if (showInfo)
				{
					rect.y += 22f;
					EditorGUI.DrawRect(rect, back);
					GUI.Label(rect, string.Format(" {0:00.0} m/s", Vector3.Dot(rigidbody.velocity, transform.forward)));

					foreach (var wheel in _wheels)
					{
						rect.y += 22f;
						EditorGUI.DrawRect(rect, back);
						GUI.Label(rect, wheel.debugInfo);
					}
				}
			}
		}

#endif
	}
}