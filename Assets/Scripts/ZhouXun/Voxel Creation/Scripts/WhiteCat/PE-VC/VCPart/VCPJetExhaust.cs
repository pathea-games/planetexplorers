using UnityEngine;

namespace WhiteCat
{
	public class VCPJetExhaust : VCPart
	{
		[SerializeField][Min(0)] float _energyExpendSpeed;
		[SerializeField][Min(0)] float _pushForce;
		[SerializeField] GameObject _effect;

		[SerializeField] AudioSource _sound;
		[SerializeField] float _volume = 0.5f;


		CarrierController _controller;


		protected override void Awake()
		{
			base.Awake();
			_effect.SetActive(false);
		}


		public void Init(CarrierController controller, float maxForce, int count)
		{
			_controller = controller;
			_pushForce = Mathf.Min(maxForce, _pushForce);

            _volume *= Mathf.Pow(0.75f, count - 1);
        }


		void FixedUpdate()
		{
			float expend = _energyExpendSpeed * Time.deltaTime;

			if (_effect.activeSelf)
			{
				// 推进器开着的时候, 驾驶员关闭了开关 | 没能量 都需要关闭
				if (!_controller.isJetting || !_controller.isEnergyEnough(expend))
				{
					_effect.SetActive(false);
				}
				else
				{
					_controller.rigidbody.AddForce(transform.forward * _pushForce * _controller.speedScale);

					// 主控端消耗能量
					if (_controller.isPlayerHost)
					{
						_controller.ExpendEnergy(expend);
					}
				}

				_sound.volume = _volume * SystemSettingData.Instance.AbsEffectVolume;
			}
			else
			{
				// 推进器关着的时候, 驾驶员打开了开关 & 有能量 才可以打开
				if (_controller.isJetting && _controller.isEnergyEnough(expend))
				{
					_effect.SetActive(true);

					_sound.volume = _volume * SystemSettingData.Instance.AbsEffectVolume;
					_sound.time = 0f;
					_sound.Play();
				}

				_sound.volume -= _volume * Time.deltaTime * SystemSettingData.Instance.AbsEffectVolume;
			}

		}
	}
}