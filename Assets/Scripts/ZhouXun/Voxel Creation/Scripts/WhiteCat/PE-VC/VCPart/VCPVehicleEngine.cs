using UnityEngine;
using System.Collections.Generic;



namespace WhiteCat
{
	public class VCPVehicleEngine : VCPart
	{
		[SerializeField] float _motorTorque = 100000;
		[SerializeField] float _maxRPM = 600;
		[SerializeField] [Min(0)] float _energyExpendSpeed;
		[SerializeField] float _powerIncreaseSpeed = 0.4f;
		[SerializeField] float _powerDecreaseSpeed = 0.4f;


		float _sign = 1f;
		float _power = 0f;


		public float UpdateEngine(VehicleController controller, float rpm)
		{
			float targetPower = controller.inputY;
			float targetSign = Mathf.Sign(targetPower);
			targetPower = Mathf.Abs(targetPower);

			if (_sign == targetSign)
			{
				_power = Mathf.MoveTowards(
					_power, targetPower,
					(targetPower > _power ? _powerIncreaseSpeed : _powerDecreaseSpeed) * Time.deltaTime);
			}
			else
			{
				_power = Mathf.MoveTowards(
					_power, 0f,
					_powerDecreaseSpeed * Time.deltaTime);

				if (_power < 0.01f) _sign = targetSign;
			}

			float expend = _power * Time.deltaTime * _energyExpendSpeed;

			float torque = 0;

			if (controller.isEnergyEnough(expend))
			{
				torque = _power * _sign * _motorTorque
					* PEVCConfig.instance.motorForce.Evaluate(Mathf.Clamp01(Mathf.Abs(rpm) / _maxRPM));
			}

			if (controller.isPlayerDriver)
			{
				controller.ExpendEnergy(expend);
			}

			return torque;
		}


		//float _pitchAudioTime = 0;
		//bool _lastDirection = false;


		//float _runningAudioVolume = 0f;
		//float _standbyAudioVolume = 0f;
	}
}
