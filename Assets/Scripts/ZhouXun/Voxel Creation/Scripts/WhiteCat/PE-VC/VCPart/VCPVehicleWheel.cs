using UnityEngine;

namespace WhiteCat
{
	public class VCPVehicleWheel : VCPart
	{
		[SerializeField] float _mass = 100;
		[SerializeField] float _radius = 0.5f;
        [SerializeField] float _motorTorqueScale = 1f;
		[SerializeField] [Range(1f, 2f)] float _stiffness = 1.5f;

		[Space(8)]
		[SerializeField] Transform _wheelModel;
		[SerializeField] WheelCollider wheelCollider;

		[Header("Effects")]
		[SerializeField] ParticleSystem _dirtEffect;
		[SerializeField] float _rpmToStartSpeed;
		[SerializeField] float _rpmToEmissionRate;

		const float massToBrakeRatio = 100;

		[HideInInspector] public bool isMotorWheel;
		[HideInInspector] public bool isSteerWheel;

		VehicleController _controller;
		float _steerLeft;
		float _steerRight;

		static Vector3 _pos;
		static Quaternion _rot;


		public float rpm
		{
			get { return wheelCollider.rpm; }
		}


		public void InitLayer()
		{
			wheelCollider.gameObject.layer = VCConfig.s_WheelLayer;
		}


		public void Init(VehicleController controller, float steerLeft, float steerRight)
		{
			_controller = controller;
			_steerLeft = steerLeft;
			_steerRight = steerRight;

			wheelCollider.gameObject.layer = VCConfig.s_WheelLayer;
			wheelCollider.mass = _mass;
			wheelCollider.radius = _radius;
			wheelCollider.wheelDampingRate = 0.5f;

			Vector3 wheelRelativeBody = _controller.rigidbody.transform.InverseTransformPoint(wheelCollider.transform.position);
			float distance = _controller.rigidbody.centerOfMass.y - wheelRelativeBody.y + wheelCollider.radius;
			wheelCollider.forceAppPointDistance = distance + PEVCConfig.instance.wheelForceAppPointOffset;

			var friction = wheelCollider.sidewaysFriction;
			friction.stiffness = _stiffness * PEVCConfig.instance.sideStiffnessFactor + PEVCConfig.instance.sideStiffnessBase;
			wheelCollider.sidewaysFriction = friction;

			friction = wheelCollider.forwardFriction;
			friction.stiffness = _stiffness * PEVCConfig.instance.fwdStiffnessFactor + PEVCConfig.instance.fwdStiffnessBase;
			wheelCollider.forwardFriction = friction;

			_effectOffset = _dirtEffect.transform.localPosition;
		}


		public void OnFixedUpdate(float averageSprungMass)
		{
			// 更新弹簧和悬挂

			float sprungMass = wheelCollider.sprungMass;
			if (float.IsNaN(sprungMass)) sprungMass = averageSprungMass;
			else sprungMass = Mathf.Min(sprungMass, averageSprungMass * 5f);

			var jointSpring = wheelCollider.suspensionSpring;
			jointSpring.spring = sprungMass * PEVCConfig.instance.naturalFrequency * PEVCConfig.instance.naturalFrequency;
			jointSpring.damper = 2 * PEVCConfig.instance.dampingRatio * Mathf.Sqrt(jointSpring.spring * sprungMass);
			jointSpring.targetPosition = 0.5f;
			wheelCollider.suspensionSpring = jointSpring;

			wheelCollider.suspensionDistance = sprungMass * Mathf.Abs(Physics.gravity.y) / (jointSpring.targetPosition * jointSpring.spring);

            UpdateEffect();
        }


        // rpm 偶尔返回错误的数据, 需要延长处理
        float _directionChangeTime;
        bool _lastDirection;

        Vector3 _effectOffset;


        void UpdateEffect()
        {
            if (wheelCollider.isGrounded && Mathf.Abs(wheelCollider.rpm) > 5)
            {
                if (!_dirtEffect.isPlaying) _dirtEffect.Play();
                _dirtEffect.startSpeed = Mathf.Min(Mathf.Abs(wheelCollider.rpm * _rpmToStartSpeed), 5f);
                _dirtEffect.emissionRate = Mathf.Min(Mathf.Abs(wheelCollider.rpm * _rpmToEmissionRate), 100f);

                if (_lastDirection != wheelCollider.rpm > 0)
                {
                    _directionChangeTime += Time.deltaTime;
                    if (_directionChangeTime > 0.5f)
                    {
                        _directionChangeTime = 0f;
                        _lastDirection = !_lastDirection;
                    }
                }
                else _directionChangeTime = 0f;

                _dirtEffect.transform.localEulerAngles = new Vector3(-18f,
                    (_lastDirection ? -180f : 0f) + wheelCollider.steerAngle, 0f);
                _dirtEffect.transform.localPosition = _effectOffset + _wheelModel.localPosition;
            }
            else
            {
                if (_dirtEffect.isPlaying) _dirtEffect.Stop();
            }
        }


		public void OnUpdate(float motorTorque, float rotateFactor)
		{
			// 转弯
			if (isSteerWheel)
			{
				wheelCollider.steerAngle = _controller.inputX * rotateFactor
                    * (_controller.inputX > 0f ? _steerRight : _steerLeft);
			}

			// 驱动
			if (isMotorWheel)
			{
				wheelCollider.motorTorque = motorTorque * _motorTorqueScale;
			}

			// 刹车
			wheelCollider.brakeTorque = _controller.inputBrake * _mass * massToBrakeRatio;

			// 模型
			wheelCollider.GetWorldPose(out _pos, out _rot);
			_wheelModel.position = _pos;
			_wheelModel.rotation = _rot;
		}


#if UNITY_EDITOR

		public string debugInfo
		{
			get
			{
				float wheelSpeed = wheelCollider.rpm * Mathf.PI * _radius / 30f;
				return string.Format(" {0:00.0} m/s  {1:000}", wheelSpeed, wheelCollider.motorTorque);
			}
		}

#endif
	}
}