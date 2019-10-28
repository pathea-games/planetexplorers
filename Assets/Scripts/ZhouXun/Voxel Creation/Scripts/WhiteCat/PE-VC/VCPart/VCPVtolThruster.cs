using UnityEngine;

namespace WhiteCat
{

	public class VCPVtolThruster : VCPart
	{
		[SerializeField] float _forceFactor = 500000;
		[SerializeField] float _increaseSpeed = 0.3f;
		[SerializeField] float _decreaseSpeed = 0.3f;
		[SerializeField] GameObject _effect;
		[SerializeField] Light _light;
		[SerializeField] Transform _pivot;

		HelicopterController _controller;
		Direction _forceDirection;
		Vector3 _localAngularDirection;
		bool _rotateRight;
		bool _rotateLeft;
		//bool _isFront;

		//[SerializeField]//
		float _maxForce;


		float _maxForceRatio;
		float _currentForceRatio = 0;


		public void Init(HelicopterController controller)
		{
			_controller = controller;
			_forceDirection = GetDirection(transform.localRotation * Vector3.up);
			_maxForce = _forceFactor * (transform.localScale.x + transform.localScale.z) * 0.5f;

			// 相对质心的位置
			Vector3 relativePosition = _controller.transform.InverseTransformPoint(_pivot.position) - controller.rigidbody.centerOfMass;

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
				_pivot.position = _controller.transform.TransformPoint(_forceApplyPoint);
			}
			else if (_forceDirection == Direction.Forward || _forceDirection == Direction.Back)
			{
				_rotateRight = cos > 0.5f;
				_rotateLeft = cos < -0.5f;

				Vector3 newPivot, whatever;
				Utility.ClosestPoint(
					_pivot.position + new Vector3(0, 100, 0), _pivot.position + new Vector3(0, -100, 0),
					controller.rigidbody.worldCenterOfMass + transform.up * 100, controller.rigidbody.worldCenterOfMass - transform.up * 100,
					out newPivot, out whatever);

				_pivot.position = newPivot;

				//if (_rotateRight || _rotateLeft)
				//{
				//	Vector3 _forceApplyPoint = controller.rigidbody.centerOfMass;
				//	_forceApplyPoint.x += relativePosition.x;
				//	_forceApplyPoint.z += relativePosition.z;
				//	_pivot.position = _controller.transform.TransformPoint(_forceApplyPoint);
				//}
				//else _pivot.position = controller.rigidbody.worldCenterOfMass;
            }
			else
			{
				_rotateRight = cos > 0;
				_rotateLeft = cos < 0;
            }

			if (_forceDirection != Direction.Down) enabled = true;
		}


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


		float maxForceRatio
		{
			get
			{
				if (_forceDirection == Direction.Up)
				{
					float y = transform.up.y;
					if (y > 0.1f)
					{
						return Mathf.Clamp(_maxForceRatio / y, _maxForceRatio, 1f);
					}
				}
				return _maxForceRatio;
			}
		}


		public void InitMaxForceRatio(float upRatio)
		{
			if (Direction.Up == _forceDirection)
			{
				_maxForceRatio = upRatio;
			}
			else _maxForceRatio = 1f;
		}


		void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawSphere(_pivot.position, 0.2f);

			Gizmos.DrawRay(transform.position, transform.TransformDirection(_localAngularDirection));
		}


		void FixedUpdate()
		{
			int increaseOrDecrease = -1;
			float forceScale = 0f;

			if (_controller.isEnergyEnough(0.01f) && _controller.hasDriver)
			{
				switch (_forceDirection)
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
							if (currentYSpeed < targetYSpeed - 0.1f) increaseOrDecrease = 1;
							else if (currentYSpeed > targetYSpeed + 0.1f) increaseOrDecrease = -1;
							else increaseOrDecrease = 0;

							forceScale = 1f;
							break;
						}

					case Direction.Left:
					case Direction.Right:
						{
							if (Mathf.Abs(_controller.inputX) > 0.01f)
							{
                                if (_controller.inputX > 0 && _rotateRight)
                                {
                                    float rotateSpeedRatio = _controller.rigidbody.angularVelocity.y / 1.6f;
                                    increaseOrDecrease = rotateSpeedRatio >= 1f ? 0 : 1;
                                    forceScale = Mathf.Clamp01(1f - rotateSpeedRatio);
                                }
                                else if (_controller.inputX < 0 && _rotateLeft)
                                {
                                    float rotateSpeedRatio = -_controller.rigidbody.angularVelocity.y / 1.6f;
                                    increaseOrDecrease = rotateSpeedRatio >= 1f ? 0 : 1;
                                    forceScale = Mathf.Clamp01(1f - rotateSpeedRatio);
                                }
                            }
							else if(Mathf.Abs(_controller.rigidbody.angularVelocity.y) > 0.26f)
                            {
								if (_controller.rigidbody.angularVelocity.y > 0)
								{
									if (_rotateLeft)
									{
										increaseOrDecrease = 1;
										forceScale = 1f;
                                    }
								}
								else
								{
									if (_rotateRight)
									{
										increaseOrDecrease = 1;
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

									if (_controller.inputX > 0 && _rotateRight)
									{
										float rotateSpeedRatio = _controller.rigidbody.angularVelocity.y / 1.5f;
										increaseOrDecrease = rotateSpeedRatio >= 1f ? 0 : 1;
										forceScale = Mathf.Clamp01(1f - rotateSpeedRatio);
									}
									else if (_controller.inputX < 0 && _rotateLeft)
									{
										float rotateSpeedRatio = -_controller.rigidbody.angularVelocity.y / 1.5f;
										increaseOrDecrease = rotateSpeedRatio >= 1f ? 0 : 1;
										forceScale = Mathf.Clamp01(1f - rotateSpeedRatio);
									}

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
										increaseOrDecrease = 1;
										forceScale = 1f;
										break;
									}
									if (_rotateRight) break;
								}
								else
								{
									if (_rotateRight)
									{
										increaseOrDecrease = 1;
										forceScale = 1f;
										break;
									}
									if (_rotateLeft) break;
								}
							}

							if (_forceDirection == Direction.Forward)
							{
								increaseOrDecrease = _controller.inputY > 0.01f ? 1 : -1;
                                if (increaseOrDecrease == 1) forceScale = _controller.inputY;
							}
							else
							{
								increaseOrDecrease = _controller.inputY < -0.01f ? 1 : -1;
								if (increaseOrDecrease == 1) forceScale = -_controller.inputY;
							}
							break;
						}
				}
			}

			// 更新力比例
			if (increaseOrDecrease == 1)
			{
				_currentForceRatio = Mathf.Min(maxForceRatio, _currentForceRatio + _increaseSpeed * Time.deltaTime);
			}
			else if(increaseOrDecrease == -1)
			{
				_currentForceRatio = Mathf.Max(0, _currentForceRatio - _decreaseSpeed * Time.deltaTime);
			}
			
			if (forceScale > 0)
			{
				float force = _currentForceRatio * _maxForce * forceScale;
				
				if (_forceDirection == Direction.Up)
				{
					// 限制高度
					force *= _controller.liftForceFactor;

					// 向上推进器可以提供旋转扭矩
					_controller.rigidbody.AddTorque(_controller.inputX * force * PEVCConfig.instance.thrusterSteerHelp * transform.up);
				}

				// 应用力
                _controller.rigidbody.AddForceAtPosition(force * transform.up * _controller.speedScale, _pivot.position);

				// 消耗能量
				if (_controller.isPlayerHost)
				{
					_controller.ExpendEnergy(force * Time.deltaTime * PEVCConfig.instance.thrusterEnergySpeed);
				}
			}

			// 更新效果
			if (_effect.activeSelf)
			{
				if (increaseOrDecrease == -1 && _currentForceRatio < 0.05f)
				{
					_effect.SetActive(false);
				}
			}
			else
			{
				if (increaseOrDecrease == 1 && _currentForceRatio > 0.05f)
				{
					_effect.SetActive(true);
				}
			}
			_light.intensity = _currentForceRatio * 2f;
		}
	}

}
