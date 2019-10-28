using UnityEngine;
using System;

namespace WhiteCat
{
	public class VCPShipRudder : VCPart
	{
		[SerializeField]
		Transform _pivot;

		[SerializeField]
		float _steerAngle = 40f;

		[SerializeField]
		float _steerFactor;

		BoatController _controller;
		Vector3 _angles;


		public void Init(BoatController controller)
		{
			_controller = controller;

			// 计算相对质心的位置
			Vector3 relativePosition = _controller.transform.InverseTransformPoint(_pivot.position) - controller.rigidbody.centerOfMass;

			if (relativePosition.z < 0) _steerAngle = -_steerAngle;

			_steerFactor = (transform.localScale.y + transform.localScale.z) * _steerFactor * (float)(Math.Log(_controller.rigidbody.mass / 6666 + 1f) * 6666);

			enabled = true;
		}


		void FixedUpdate()
		{
			_angles.y = _controller.inputX * _steerAngle;
			transform.localEulerAngles = _angles;

			// 应用力
			if (VFVoxelWater.self.IsInWater(_pivot.position))
			{
				float  speed = Vector3.Dot(_controller.rigidbody.velocity, _controller.transform.forward);
				float torque = Mathf.Clamp(speed, -4f, 4f) * 0.25f;
				torque = Mathf.Sign(torque) * torque * torque * _steerFactor * _controller.inputX;
                _controller.rigidbody.AddRelativeTorque(0, torque *_controller.speedScale, 0);
			}
		}

	}
}