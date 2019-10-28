using UnityEngine;

namespace WhiteCat
{

	public class VCPVtolRotor : VCPart
	{
		[SerializeField] float _forceFactor = 1f;
		[SerializeField] float _originalRadius = 0.38f;     // 5.8
        [SerializeField] bool _isBig;
        [SerializeField] float _maxDeflectionAngle = 15f;	// 10
		[SerializeField] Transform _rotatePivot;
		[SerializeField] Transform _deflectionPivot;
		[SerializeField] Transform _forcePivot;

		[Header("Effects")]
		[SerializeField] AudioSource _sound;
		[SerializeField] float _maxPitch = 1f;
		[SerializeField] float _basePitch = 0.5f;
		[SerializeField] float _maxVolume = 1f;
		[SerializeField] float _volumeSpeed = 2f;

		HelicopterController _controller;
		Direction _forceDirection;
		Vector3 _localAngularDirection;
        Vector3 _localForwardDirection;
		Vector3 _localTargetUp;
		bool _rotateRight;
		bool _rotateLeft;
		//bool _isFront;

		//[SerializeField]//
		float _maxForce;
		//[SerializeField]//
		float _accelerate;
		//[SerializeField]//
		float _decelerate;

		float _maxRotateSpeed;
		float _currentRotateSpeed = 0;
		float _currentYAngle = 0;


		public float maxLiftForce
		{
			get
			{
				if (Direction.Up == _forceDirection)
				{
					return _maxForce * (transform.localRotation * Vector3.up).y;
				}
				else return 0;
			}
		}


		float maxRotateSpeed
		{
			get
			{
				if (_forceDirection == Direction.Up)
				{
					float y = transform.up.y;
					if (y > 0.1f)
					{
						return Mathf.Clamp(_maxRotateSpeed / y, _maxRotateSpeed, PEVCConfig.instance.rotorMaxRotateSpeed);
					}
				}
				return _maxRotateSpeed;
			}
		}

        // 尺寸规格
        public int sizeType { get { return _isBig ? 0 : 1; } }

        // 方向类型
        public int directionType { get { return (int)_forceDirection; } }


		public void Init(HelicopterController controller)
		{
            _controller = controller;
			_forceDirection = GetDirection(transform.localRotation * Vector3.up);

			float radius = (transform.localScale.x + transform.localScale.z) * 0.5f * _originalRadius;
			_maxForce = _forceFactor * radius;

			// 计算加速度
			_accelerate = Mathf.Pow(1.15f, -radius) * PEVCConfig.instance.rotorAccelerateFactor;
			_decelerate = Mathf.Pow(1.15f, -radius) * PEVCConfig.instance.rotorDecelerateFactor;

			// 相对质心的位置
			Vector3 relativePosition = _controller.transform.InverseTransformPoint(_forcePivot.position) - controller.rigidbody.centerOfMass;

			// 前后位置
			//_isFront = relativePosition.z > 0;

			// 计算旋转偏向
			_localAngularDirection = Vector3.right;
			Vector3 relativeXZ = relativePosition;
			relativeXZ.y = 0;
			if (relativeXZ.sqrMagnitude > 0.25f)
			{
				_localAngularDirection = Vector3.Cross(Vector3.up, relativeXZ).normalized;
			}
			_localAngularDirection = Quaternion.Inverse(transform.localRotation) * _localAngularDirection;
			_localForwardDirection = Quaternion.Inverse(transform.localRotation) * Vector3.forward;
			float cos = Vector3.Dot(Vector3.up, _localAngularDirection);

			// 计算力应用位置和旋转方向
			if (_forceDirection == Direction.Up)
			{
				float adjustedZ = Mathf.Sign(relativePosition.z) * Mathf.Log(Mathf.Abs(relativePosition.z) + 1f);
				adjustedZ = adjustedZ * PEVCConfig.instance.rotorBalanceAdjust + relativePosition.z * (1f - PEVCConfig.instance.rotorBalanceAdjust);
				adjustedZ *= PEVCConfig.instance.rotorBalaceScale.Evaluate(Mathf.Abs(adjustedZ));

				Vector3 _forceApplyPoint = controller.rigidbody.centerOfMass;
				_forceApplyPoint.x += relativePosition.x;
				_forceApplyPoint.y += relativePosition.y;
				_forceApplyPoint.z += adjustedZ;
				_forcePivot.position = _controller.transform.TransformPoint(_forceApplyPoint);
			}
			else if (_forceDirection == Direction.Forward || _forceDirection == Direction.Back)
			{
				_rotateRight = cos > 0.5f;
				_rotateLeft = cos < -0.5f;

				Vector3 newPivot, whatever;
				Utility.ClosestPoint(
					_forcePivot.position + new Vector3(0, 100, 0), _forcePivot.position + new Vector3(0, -100, 0),
					controller.rigidbody.worldCenterOfMass + transform.up * 100, controller.rigidbody.worldCenterOfMass - transform.up * 100,
					out newPivot, out whatever);

				_forcePivot.position = newPivot;

				//if (_rotateRight || _rotateLeft)
				//{
				//	Vector3 _forceApplyPoint = controller.rigidbody.centerOfMass;
				//	_forceApplyPoint.x += relativePosition.x;
				//	_forceApplyPoint.z += relativePosition.z;
				//	_forcePivot.position = _controller.transform.TransformPoint(_forceApplyPoint);
				//}
				//else _forcePivot.position = controller.rigidbody.worldCenterOfMass;
			}
			else
			{
				_rotateRight = cos > 0;
				_rotateLeft = cos < 0;
			}

			if (_forceDirection != Direction.Down) enabled = true;
		}


        public void InitSoundScale(int count)
        {
            _maxVolume *= Mathf.Pow(0.75f, count - 1);
            _volumeSpeed *= Mathf.Pow(0.75f, count - 1);
        }


		public void InitMaxRotateSpeed(float upValue)
		{
			if (Direction.Up == _forceDirection)
			{
				_maxRotateSpeed = upValue;
			}
			else _maxRotateSpeed = PEVCConfig.instance.rotorMaxRotateSpeed;
		}


		void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawSphere(_forcePivot.position, 0.2f);

			Gizmos.DrawRay(transform.position, transform.TransformDirection(_localAngularDirection));
		}


		void FixedUpdate()
		{
			_localTargetUp = Vector3.up;
			int accelerateOrDecelerate = -1;
			float forceScale = 0;

			if (_controller.isEnergyEnough(0.01f) && _controller.hasDriver)
			{
				switch(_forceDirection)
				{
					case Direction.Up:
						{
							// 目标上升速度
							float targetYSpeed = 0;
							if (_controller.inputVertical > 0.01f)
							{
								targetYSpeed = _controller.inputVertical * PEVCConfig.instance.helicopterMaxUpSpeed;
							}
							else if (_controller.inputVertical < -0.01f)
							{
								targetYSpeed = _controller.inputVertical * PEVCConfig.instance.helicopterMaxDownSpeed;
							}

							// 判断应该加速还是减速
							float currentYSpeed = _controller.rigidbody.velocity.y;
							if (currentYSpeed < targetYSpeed - 0.1f) accelerateOrDecelerate = 1;
							else if (currentYSpeed > targetYSpeed + 0.1f) accelerateOrDecelerate = -1;
							else accelerateOrDecelerate = 0;

							// 计算偏转方向
							Vector3 direction = _controller.inputX * _localAngularDirection + _controller.inputY * _localForwardDirection;
							if (direction != Vector3.zero)
							{
								_localTargetUp = Vector3.RotateTowards(Vector3.up, direction, _maxDeflectionAngle * Mathf.Deg2Rad, 0);
							}

							forceScale = 1f;
							break;
						}

					case Direction.Left:
					case Direction.Right:
						{
							if (Mathf.Abs(_controller.inputX) > 0.01f)
							{
								if (_controller.inputX > 0 ? _rotateRight : _rotateLeft)
								{
									_localTargetUp = Vector3.RotateTowards(Vector3.up, _controller.inputX * _localAngularDirection, _maxDeflectionAngle * Mathf.Deg2Rad, 0);
									accelerateOrDecelerate = 1;
									forceScale = 1f;
								}
							}
							else if (Mathf.Abs(_controller.rigidbody.angularVelocity.y) > 0.26f)
							{
								// 在没有旋转输入的情况下, 反向组件提供阻力保持稳定

								if (_controller.rigidbody.angularVelocity.y > 0)
								{
									if (_rotateLeft)
									{
										accelerateOrDecelerate = 1;
										forceScale = 1f;
									}
								}
								else
								{
									if (_rotateRight)
									{
										accelerateOrDecelerate = 1;
										forceScale = 1f;
									}
								}
							}
							break;
						}

					case Direction.Forward:
					case Direction.Back:
						{
							if (Mathf.Abs(_controller.inputX) > 0.01f)
							{
								if (_controller.inputX > 0 ? _rotateRight : _rotateLeft)
								{
									// 首选作为旋转动力, 其次作为前进/后退动力
									_localTargetUp = Vector3.RotateTowards(Vector3.up, _controller.inputX * _localAngularDirection, _maxDeflectionAngle * Mathf.Deg2Rad, 0);
									accelerateOrDecelerate = 1;
									forceScale = 1f;
									break;
								}
								if (_controller.inputX > 0 ? _rotateLeft : _rotateRight)
								{
									// 在不阻止旋转的前提下才作为前进/后退动力
									break;
								}
							}
							else if (Mathf.Abs(_controller.rigidbody.angularVelocity.y) > 0.26f)
							{
								// 在没有旋转输入的情况下, 反向组件提供阻力保持稳定, 正向组件停止工作

								if (_controller.rigidbody.angularVelocity.y > 0)
								{
									if (_rotateLeft)
									{
										accelerateOrDecelerate = 1;
										forceScale = 1f;
										break;
									}
									if (_rotateRight) break;
								}
								else
								{
									if (_rotateRight)
									{
										accelerateOrDecelerate = 1;
										forceScale = 1f;
										break;
									}
									if (_rotateLeft) break;
								}
							}

							if (_forceDirection == Direction.Forward)
							{
								accelerateOrDecelerate = _controller.inputY > 0.01f ? 1 : -1;
								if (accelerateOrDecelerate == 1) forceScale = _controller.inputY;
							}
							else
							{
								accelerateOrDecelerate = _controller.inputY < -0.01f ? 1 : -1;
								if (accelerateOrDecelerate == 1) forceScale = -_controller.inputY;
							}
							break;
						}
				}
			}

			// 更新偏转角
			Quaternion targetRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(Vector3.forward, _localTargetUp), _localTargetUp);
			_deflectionPivot.localRotation = Quaternion.RotateTowards(
				_deflectionPivot.localRotation, targetRotation,
				PEVCConfig.instance.rotorDeflectSpeed * Time.deltaTime);

			// 更新转速
			if (accelerateOrDecelerate == 1)
			{
				_currentRotateSpeed = Mathf.Min(maxRotateSpeed, _currentRotateSpeed + _accelerate * Time.deltaTime);
			}
			else if(accelerateOrDecelerate == -1)
			{
				_currentRotateSpeed = Mathf.Max(0, _currentRotateSpeed - _decelerate * Time.deltaTime);
			}

			// 更新旋转角
			_currentYAngle = (_currentYAngle + _currentRotateSpeed * Time.deltaTime) % 360;
			_rotatePivot.localEulerAngles = new Vector3(0, _currentYAngle, 0);

			// 应用力
			if (forceScale > 0)
			{
				float force = _currentRotateSpeed / PEVCConfig.instance.rotorMaxRotateSpeed * _maxForce * forceScale;

				if (_forceDirection == Direction.Up)
				{
					// 限制高度
					force *= _controller.liftForceFactor;

					// 向上螺旋桨可以提供旋转扭矩
					_controller.rigidbody.AddTorque(_controller.inputX * force * PEVCConfig.instance.rotorSteerHelp * transform.up);

                    // 应用力

                    _controller.rigidbody.AddForceAtPosition(transform.up * force * _controller.speedScale, _forcePivot.position);

                    _controller.rigidbody.AddForceAtPosition(
                        Vector3.ProjectOnPlane(_rotatePivot.up * force * _controller.speedScale, transform.up),
                        (_forcePivot.position + _controller.rigidbody.worldCenterOfMass) * 0.5f);
                }
                else
                {
                    // 应用力
                    _controller.rigidbody.AddForceAtPosition(_rotatePivot.up * force * _controller.speedScale, _forcePivot.position);
                }

				// 消耗能量
				if (_controller.isPlayerHost)
				{
					_controller.ExpendEnergy(force * Time.deltaTime * PEVCConfig.instance.rotorEnergySpeed);
				}
			}

			UpdateSound();
		}


		void UpdateSound()
		{
			var ratio = Mathf.Clamp01(Mathf.Abs(_currentRotateSpeed / PEVCConfig.instance.rotorMaxRotateSpeed));

			if (ratio > 0.01f)
			{
				_sound.pitch = ratio * (_maxPitch - _basePitch) + _basePitch;
				_sound.volume = Mathf.Clamp(ratio * _volumeSpeed, 0f, _maxVolume) * SystemSettingData.Instance.AbsEffectVolume;

				if (!_sound.isPlaying) _sound.Play();
			}
			else
			{
				if (_sound.isPlaying) _sound.Stop();
			}
        }
	}

}
