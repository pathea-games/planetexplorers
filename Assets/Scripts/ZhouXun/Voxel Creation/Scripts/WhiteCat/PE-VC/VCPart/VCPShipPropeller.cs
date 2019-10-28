using UnityEngine;

namespace WhiteCat
{
	public class VCPShipPropeller : VCPart
	{
		[SerializeField] float _forceFactor = 200000;
		[SerializeField] float _maxRotateSpeed = 1801;
		[SerializeField] float _accelerate = 900;
		[SerializeField] Transform _rotatePivot;
		[SerializeField] Transform _forcePivot;

		BoatController _controller;
		Direction _forceDirection;
		Vector3 _forceApplyPoint;
		Vector3 _localAngularDirection;
		float _maxForce;
		bool _rotateRight;
		bool _rotateLeft;

		//bool _isFront;

		float _currentRotateSpeed = 0;
		float _currentZAngle = 0;


		public void Init(BoatController controller, bool isSubmarine)
		{
			_controller = controller;
			_forceDirection = GetDirection(transform.localRotation * Vector3.forward);
			_maxForce = (transform.localScale.x + transform.localScale.y) * 0.5f * _forceFactor;

			// 计算相对质心的位置
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
			float cos = Vector3.Dot(Vector3.forward, _localAngularDirection);

			// 计算力应用位置和旋转方向
			if (_forceDirection == Direction.Up || _forceDirection == Direction.Down)
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

				Vector3 prjectCenter = Vector3.ProjectOnPlane(controller.rigidbody.worldCenterOfMass, Vector3.Cross(Vector3.up, transform.forward));
				Vector3 newPivot, whatever;
				Utility.ClosestPoint(
					_forcePivot.position + new Vector3(0, 20, 0), _forcePivot.position + new Vector3(0, -20, 0),
					prjectCenter + transform.forward * 20, prjectCenter - transform.forward * 20,
					out newPivot, out whatever);

				_forcePivot.position = (newPivot + _forcePivot.position) * 0.5f;
			}
			else
			{
				_rotateRight = cos > 0;
				_rotateLeft = cos < 0;

				Vector3 _forceApplyPoint = controller.rigidbody.centerOfMass;
				_forceApplyPoint.x += relativePosition.x;
				_forceApplyPoint.z += relativePosition.z;
				_forcePivot.position = _controller.transform.TransformPoint(_forceApplyPoint);
			}

			enabled = isSubmarine ? true : (_forceDirection != Direction.Up && _forceDirection != Direction.Down);
		}


		void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawSphere(_forcePivot.position, 0.2f);

			Gizmos.DrawRay(transform.position, transform.TransformDirection(_localAngularDirection));
		}


		void FixedUpdate()
		{
			float targetRotateSpeed = 0;
            float forceScale = 0;

			if (_controller.isEnergyEnough(0.01f) && _controller.hasDriver)
			{
				switch (_forceDirection)
				{
					case Direction.Up:
					case Direction.Down:
						{
							targetRotateSpeed = _forceDirection == Direction.Up ? _maxRotateSpeed : -_maxRotateSpeed;
							targetRotateSpeed *= _controller.inputVertical;
							forceScale = 1f;
                            break;
						}

					case Direction.Left:
					case Direction.Right:
						{
							if (Mathf.Abs(_controller.inputX) > 0.01f)
							{
								if (_rotateRight)
								{
									targetRotateSpeed = _controller.inputX > 0 ? _maxRotateSpeed : -_maxRotateSpeed;
                                }
								if (_rotateLeft)
								{
									targetRotateSpeed = _controller.inputX > 0 ? -_maxRotateSpeed : _maxRotateSpeed;
								}
							}
							forceScale = 1f;
							break;
						}

					case Direction.Forward:
					case Direction.Back:
						{
							forceScale = Mathf.Clamp(Vector3.Dot(_controller.rigidbody.velocity,
								_currentRotateSpeed > 0 ? transform.forward : -transform.forward), 0f, 20f);
							forceScale = 1f - 0.0025f * forceScale * forceScale;

							if (Mathf.Abs(_controller.inputX) > 0.01f)
							{
								if (_rotateRight)
								{
									targetRotateSpeed = _controller.inputX > 0 ? _maxRotateSpeed : -_maxRotateSpeed;
									break;
								}
								if (_rotateLeft)
								{
									targetRotateSpeed = _controller.inputX > 0 ? -_maxRotateSpeed : _maxRotateSpeed;
									break;
								}
							}

							if (_forceDirection == Direction.Forward)
							{
								targetRotateSpeed = _controller.inputY * _maxRotateSpeed;
							}
							else
							{
								targetRotateSpeed = - _controller.inputY * _maxRotateSpeed;
							}
							break;
						}
				}
			}

			// 更新 Y 转速
			if (targetRotateSpeed > _currentRotateSpeed)
			{
				_currentRotateSpeed = Mathf.Min(targetRotateSpeed, _currentRotateSpeed + _accelerate * Time.deltaTime);
			}
			else
			{
				_currentRotateSpeed = Mathf.Max(targetRotateSpeed, _currentRotateSpeed - _accelerate * Time.deltaTime);
			}

			// 更新旋转角
			_currentZAngle = (_currentZAngle + _currentRotateSpeed * Time.deltaTime) % 360;
			_rotatePivot.localEulerAngles = new Vector3(0, 0, _currentZAngle);

			// 应用力
			if (forceScale > 0)
			{
				float force = _currentRotateSpeed / _maxRotateSpeed * _maxForce * forceScale;

				if (VFVoxelWater.self.IsInWater(transform.position))
				{
					// 应用力
					_controller.rigidbody.AddForceAtPosition(transform.forward * force * _controller.speedScale, _forcePivot.position);
				}

				// 消耗能量
				if (_controller.isPlayerHost)
				{
					_controller.ExpendEnergy(force * Time.deltaTime * PEVCConfig.instance.boatPropellerEnergySpeed);
				}
			}
		}
	}
}
