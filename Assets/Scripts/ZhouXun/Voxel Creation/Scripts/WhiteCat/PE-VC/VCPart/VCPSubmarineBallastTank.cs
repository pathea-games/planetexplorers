using UnityEngine;

namespace WhiteCat
{
	public class VCPSubmarineBallastTank : VCPart
	{
		[SerializeField] float _weightBase = 10000000f;
		[SerializeField] float _waterSpeed = 0.25f;
		[SerializeField] float _energyExpendSpeed = 10f;

		BoatController _controller;
		float _maxWeight;
        float _fillAmount = 0;


		public void Init(BoatController controller)
		{
			_controller = controller;
			Vector3 scale = transform.localScale;
			_maxWeight = - scale.x * scale.y * scale.z * _weightBase;
			enabled = true;
		}


		void FixedUpdate()
		{
			int increase = 0;

			if (_controller.hasDriver && _controller.isEnergyEnough(0.01f))
			{
				// 目标上升速度
				float targetYSpeed = 0;
				if (_controller.inputVertical > 0.01f)
				{
					targetYSpeed = _controller.inputVertical * PEVCConfig.instance.submarineMaxUpSpeed;
				}
				else if (_controller.inputVertical < -0.01f)
				{
					targetYSpeed = _controller.inputVertical * PEVCConfig.instance.submarineMaxDownSpeed;
				}

				// 判断应该加速还是减速
				float currentYSpeed = _controller.rigidbody.velocity.y;
				if (currentYSpeed < targetYSpeed - 0.3f) increase = 1;
				else if (currentYSpeed > targetYSpeed + 0.3f) increase = -1;
			}

			if (increase > 0) _fillAmount = Mathf.Clamp01(_fillAmount - _waterSpeed * Time.deltaTime);
			else if(increase < 0) _fillAmount = Mathf.Clamp01(_fillAmount + _waterSpeed * Time.deltaTime);

			if(_fillAmount > 0) _controller.rigidbody.AddForce(0, _fillAmount * _maxWeight, 0);

			// 消耗能量
			if (_controller.isPlayerHost && Mathf.Abs(_controller.inputVertical) > 0.01f)
			{
				_controller.ExpendEnergy(Time.deltaTime * _energyExpendSpeed);
			}
		}
    }
}