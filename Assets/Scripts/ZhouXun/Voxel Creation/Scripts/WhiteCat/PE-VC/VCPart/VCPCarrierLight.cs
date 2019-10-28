using UnityEngine;

/// <summary>
/// 灯
/// </summary>
namespace WhiteCat
{
	public class VCPCarrierLight : VCPart
	{
		[SerializeField][Min(0)] float _energyExpendSpeed;
		[SerializeField] GameObject _effect;

		CarrierController _controller;


		protected override void Awake()
		{
			base.Awake();
			_effect.SetActive(false);
		}


		void Start()
		{
			_controller = GetComponentInParent<CarrierController>();
		}


		void Update()
		{
			float expend = _energyExpendSpeed * Time.deltaTime;

			if (_effect.activeSelf)
			{
				// 灯开着的时候, 驾驶员关闭了开关或没能量了都需要关灯
				if(!_controller.isLightOn || !_controller.isEnergyEnough(expend))
				{
					_effect.SetActive(false);
				}
				else
				{
					// 主控端消耗能量
					if (_controller.isPlayerHost)
					{
						_controller.ExpendEnergy(expend);
					}
				}
			}
			else
			{
				// 灯关着的时候, 驾驶员打开了开关并且有能量才可以打开
				if (_controller.isLightOn && _controller.isEnergyEnough(expend))
				{
					_effect.SetActive(true);
				}
			}
		}
	}
}