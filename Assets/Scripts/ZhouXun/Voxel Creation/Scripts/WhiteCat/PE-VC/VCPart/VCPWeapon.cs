using UnityEngine;
using SkillSystem;
using System.Collections.Generic;

namespace WhiteCat
{
	public interface IProjectileData
	{
		Transform emissionTransform { get; }
		Vector3 targetPosition { get; }
	}


	public enum WeaponType
	{
		Gun = 0,
		Cannon = 1,
		Missile = 2,
		AI = 3
	}


	public class VCPWeapon : VCPart, IProjectileData
	{
		[SerializeField] WeaponType _weaponType;
		[SerializeField] int _skillId;
		[SerializeField] float _attackPerBullet;        // 仅用于显示
        [SerializeField] int _bulletCapacity;				// 子弹容量. 用于 AI 炮塔
		[SerializeField] int _bulletProtoID;			 // 子弹类别. 用于 AI 炮塔
		[SerializeField] int _groupBulletsCount = 4;
		[SerializeField] float _groupIntervalTime = 1f;
		[SerializeField] float _intervalTime = 0.2f;
		[SerializeField] float _energyPerShot = 10;
		[SerializeField] Transform[] _muzzles;
		[SerializeField] int _groupIndex = 0;

		[Header("Rotation Params")]
		[SerializeField] bool _rotatable = false;
		[SerializeField] [Range(0, 90)] float _maxUpPitchAngle = 60;
		[SerializeField] [Range(0, 45)] float _maxDownPitchAngle = 15;
		[SerializeField] bool _clampHorizontalAngle = false;
		[SerializeField] [Range(0, 75)] float _maxHorizontalAngle = 30;
		[SerializeField] [Range(0, 360)] float _rotateSpeed = 120;
		[SerializeField] Transform _horizontalRotate;
		[SerializeField] Transform _verticalRotate;

		[Header("Effects")]
		[SerializeField] AudioClip _soundClip;
		[SerializeField] [Range(0, 1)] float _soundVolume = 1f;
		[SerializeField] ParticlePlayer _particlePlayer;
		[SerializeField] [Range(0,1)] float _cameraShakeRange = 0;

		int _currentMuzzleIndex = 0;
		float _remainTime = 0;
		int _bulletsCount = 0;

		float _targetAngleY;
		float _targetAngleX;
		Vector3 _angles;
		Vector3 _muzzleLocalCenter;

		BehaviourController _controller;
		SkCarrierCanonPara _weaponParam;


        Transform[] _realMuzzles;

        public Transform emissionTransform
        {
            get
            {
                var realMuzzle = _realMuzzles[_currentMuzzleIndex];
                var muzzle = _muzzles[_currentMuzzleIndex];

                // handle scale of weapon
                realMuzzle.forward = muzzle.TransformPoint(0, 0, 1f) - muzzle.position;

                return realMuzzle;
            }
        }


		public Vector3 targetPosition { get { return _controller.attackTargetPoint; } }


		public void Init(int weaponIndex)
		{
			_controller = GetComponentInParent<BehaviourController>();
			_weaponParam = new SkCarrierCanonPara(weaponIndex);
			enabled = true;

			_muzzleLocalCenter = Vector3.zero;
			for (int i = 0; i < _muzzles.Length; i++)
			{
				_muzzleLocalCenter += _muzzles[i].localPosition;
			}
			_muzzleLocalCenter /= _muzzles.Length;

            _realMuzzles = new Transform[_muzzles.Length];
            for (int i=0; i<_muzzles.Length; i++)
            {
                _realMuzzles[i] = new GameObject("RealMuzzle").transform;
                _realMuzzles[i].SetParent(_muzzles[i], false);
            }
        }


		public int groupIndex
		{
			set { _groupIndex = value; }
		}


        public float attackPerSecond
        {
            get
            {
                float time = (_groupBulletsCount - 1) * _intervalTime + _groupIntervalTime;
                return _attackPerBullet * _groupBulletsCount / time;
            }
        }


		// 子弹容量. 用于 AI 炮塔
		public int bulletCapacity
		{
			get { return _bulletCapacity; }
		}


        // 子弹类别. 用于 AI 炮塔
        public int bulletProtoID
        {
            get { return _bulletProtoID; }
        }


		#region Sound ---------------------------------------------------------------

		class DiscreteSounds
		{
			public AudioClip sound;
			public float volume;
			public float lastTime;

			public DiscreteSounds(AudioClip sound, float volume)
			{
				this.sound = sound;
				this.volume = volume;
				lastTime = 0;
			}


			public void Play(AudioSource source)
			{
				if(Time.timeSinceLevelLoad - lastTime > PEVCConfig.instance.minWeaponSoundInterval)
				{
					lastTime = Time.timeSinceLevelLoad;
					source.volume = SystemSettingData.Instance.EffectVolume;
					source.PlayOneShot(sound, volume);
				}
			}
		}


		static List<DiscreteSounds> sounds = new List<DiscreteSounds>(4);
		int soundIndex = -1;


		void PlaySound()
		{
			if(soundIndex < 0)
			{
				soundIndex = sounds.FindIndex(sound => sound.sound == _soundClip);
				if (soundIndex < 0)
				{
					sounds.Add(new DiscreteSounds(_soundClip, _soundVolume));
					soundIndex = sounds.Count - 1;
				}
			}
			sounds[soundIndex].Play(_controller.audioSource);
		}

		#endregion Sound


		public void PlayEffects()
		{
			_particlePlayer.enabled = true;
			PlaySound();
		}


		void UpdateRotation()
		{
			// 射击模式下且开启了组 转向目标，否则转向正前方
			if (_controller.isAttackMode && (_weaponType == WeaponType.AI || _controller.IsWeaponGroupEnabled(_groupIndex)))
			{
				// 计算水平旋转
				_angles = _horizontalRotate.parent.InverseTransformVector(_controller.attackTargetPoint - _horizontalRotate.position);
				_targetAngleY = Mathf.Atan2(_angles.x, _angles.z) * Mathf.Rad2Deg;
				if (_clampHorizontalAngle) _targetAngleY = Mathf.Clamp(_targetAngleY, -_maxHorizontalAngle, _maxHorizontalAngle);

				// 计算垂直旋转
				_angles = _verticalRotate.parent.InverseTransformVector(
					_controller.attackTargetPoint - _muzzles[0].parent.TransformPoint(_muzzleLocalCenter));
				_targetAngleX = Mathf.Clamp(Mathf.Asin(-_angles.y / _angles.magnitude) * Mathf.Rad2Deg, -_maxUpPitchAngle, _maxDownPitchAngle);
			}
			else
			{
				_targetAngleY = 0;
				_targetAngleX = 0;
			}

			// 插值水平旋转
			_targetAngleY = Mathf.MoveTowardsAngle(
				_horizontalRotate.localEulerAngles.y,
				_targetAngleY,
				_rotateSpeed * Time.deltaTime
				);

			// 插值垂直旋转
			_targetAngleX = Mathf.MoveTowardsAngle(
				_verticalRotate.localEulerAngles.x,
				_targetAngleX,
				_rotateSpeed * Time.deltaTime
				);

			// 应用旋转
			if (_horizontalRotate == _verticalRotate)
			{
				_angles.Set(_targetAngleX, _targetAngleY, 0);
				_horizontalRotate.localEulerAngles = _angles;
			}
			else
			{
				_angles.Set(0, _targetAngleY, 0);
				_horizontalRotate.localEulerAngles = _angles;
				_angles.Set(_targetAngleX, 0, 0);
				_verticalRotate.localEulerAngles = _angles;
			}
		}


		void FixedUpdate()
		{
			// 旋转
			if(_rotatable) UpdateRotation();

			// 刷新CD
			_remainTime -= Time.deltaTime;

			// 开火
			if (_controller.isPlayerHost
				&& _controller.isAttackMode
				&& _remainTime <= 0f
				&& (_weaponType == WeaponType.AI || _controller.IsWeaponGroupEnabled(_groupIndex))
				&& _controller.IsWeaponControlEnabled(_weaponType)
				&& _controller.isEnergyEnough(_energyPerShot))
			{
				_controller.StartSkill(_skillId, _weaponParam);
				_controller.ExpendEnergy(_energyPerShot);
			
				_currentMuzzleIndex = (_currentMuzzleIndex + 1) % _muzzles.Length;
				_bulletsCount++;

				if (_bulletsCount == _groupBulletsCount)
				{
					_bulletsCount = 0;
					_remainTime = _groupIntervalTime;
				}
				else _remainTime = _intervalTime;

				if (_cameraShakeRange > 0) PeCamera.PlayAttackShake();
			}
			else
			{
				if (_remainTime <= -_groupIntervalTime)
				{
					_bulletsCount = 0;
					_remainTime = 0f;
				}
			}
		}

    }
}