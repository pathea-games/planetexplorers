using UnityEngine;

namespace WhiteCat
{
	public class VCPCockpit : VCPBaseSeat
	{
		[SerializeField] float _maxSteerAngle = 20f;
		[SerializeField] Transform _steerRoot;
		[SerializeField] Transform _leftHandPoint;
		[SerializeField] Transform _rightHandPoint;
		public bool isMotorcycle = false;


		[SerializeField] MultiSoundController _sound;


		public override void GetOn(IVCPassenger passenger)
		{
			base.GetOn(passenger);
			passenger.SetHands(_leftHandPoint, _rightHandPoint);
		}


		public void UpdateCockpit(float inputX, float inputY, bool hasDriverAndEnergy)
		{
			if(_steerRoot)
			{
				var angles = _steerRoot.localEulerAngles;
				angles.y = inputX * _maxSteerAngle;
				_steerRoot.localEulerAngles = angles;
			}

			if (_sound) {
				_sound.UpdateSound(inputY, hasDriverAndEnergy);
			}
		}
	}
}