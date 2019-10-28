using UnityEngine;
using System.Collections;

namespace WhiteCat
{
    [System.Serializable]
    public class AudioState : State
    {
        public AudioSource source;
		[Range(0f, 1f)]public float maxVolume = 1f;

        public void Init()
        {
            source.enabled = true;
            source.playOnAwake = false;
            source.velocityUpdateMode = AudioVelocityUpdateMode.Fixed;
            source.Stop();

            onEnter = () =>
                {
                    source.timeSamples = 0;
                    source.Play();
                };

            onExit = () =>
                {
                    source.Stop();
                };
        }
    }


    /// <summary>
    /// 引擎音效控制
    /// 上车下车开关此控制器，打开时实时更新输入
    /// </summary>
    public class MultiSoundController : StatesBehaviour
    {
        [SerializeField] AudioState _turnon;
        [SerializeField] AudioState _standby;
        [SerializeField] AudioState _speedup;
        [SerializeField] AudioState _running;
        [SerializeField] AudioState _slowdown;


        float _input;
        bool _acc;
        bool _hasDriver;


        public void UpdateSound(float input, bool hasDriverAndEnergy)
        {
            if (Mathf.Sign(input) != Mathf.Sign(_input))
            {
                _acc = false;
                _input = input;
            }
            else
            {
                if (_acc)
                {
                    _acc = Mathf.Abs(input) - Mathf.Abs(_input) > -0.1f;
                    if (!_acc) _input = input;
                }
                else
                {
                    _acc = Mathf.Abs(input) - Mathf.Abs(_input) > 0.1f;
                    if (_acc) _input = input;
                }
            }

			if (_hasDriver != hasDriverAndEnergy)
            {
				_hasDriver = hasDriverAndEnergy;
				if (hasDriverAndEnergy)
                {
                    _turnon.source.volume = 0f;
                    state = _turnon;
                }
                else state = _standby;
            }
        }


        void Awake()
        {
            _turnon.Init();
            _standby.Init();
            _speedup.Init();
            _running.Init();
            _slowdown.Init();

            Pathea.PeGameMgr.PasueEvent += OnGamePause;

            _turnon.onUpdate = t =>
                {
					_turnon.source.volume = _turnon.source.time / _turnon.source.clip.length * SystemSettingData.Instance.AbsEffectVolume * _turnon.maxVolume;

					if (_turnon.source.clip.length - _turnon.source.time < Time.fixedDeltaTime || !_turnon.source.isPlaying)
                    {
                        state = _standby;
                    }
                };

            _standby.onUpdate = t =>
                {
					if (_hasDriver) _standby.source.volume += t * SystemSettingData.Instance.AbsEffectVolume * _standby.maxVolume;
					else _standby.source.volume -= t * SystemSettingData.Instance.AbsEffectVolume * _standby.maxVolume;

					_standby.source.volume = Mathf.Clamp(_standby.source.volume, 0f, SystemSettingData.Instance.AbsEffectVolume * _standby.maxVolume);

                    if (_hasDriver && _acc)
                    {
                        state = _speedup;
                    }
                };

            _speedup.onUpdate = t =>
                {
					_speedup.source.volume = SystemSettingData.Instance.AbsEffectVolume * _speedup.maxVolume;

                    if (_acc)
                    {
                        if (_speedup.source.clip.length - _speedup.source.time < Time.fixedDeltaTime || !_speedup.source.isPlaying)
                        {
                            state = _running;
                        }
                    }
                    else
                    {
						t = (1f - _speedup.source.time / _speedup.source.clip.length);
                        state = _slowdown;
						_slowdown.source.timeSamples = Mathf.Clamp((int)(t * _slowdown.source.clip.samples), 0, _slowdown.source.clip.samples-1);
                    }
                };

            _running.onUpdate = t =>
                {
					_running.source.volume = SystemSettingData.Instance.AbsEffectVolume * _running.maxVolume;

                    if (!_acc)
                    {
                        state = _slowdown;
                    }
                };

            _slowdown.onUpdate = t =>
                {
					_slowdown.source.volume = SystemSettingData.Instance.AbsEffectVolume * _slowdown.maxVolume;

					if (!_acc)
                    {
                        if (_slowdown.source.clip.length - _slowdown.source.time < Time.fixedDeltaTime || !_slowdown.source.isPlaying)
                        {
                            state = _standby;
                        }
                    }
                    else
                    {
						t = (1f - _slowdown.source.time / _slowdown.source.clip.length);
                        state = _speedup;
						_speedup.source.timeSamples = Mathf.Clamp((int)(t * _speedup.source.clip.samples), 0, _speedup.source.clip.samples-1);
                    }
                };

            enabled = true;
        }


        void OnGamePause(bool pause)
        {
            if (pause)
            {
                _turnon.source.Pause();
                _standby.source.Pause();
                _speedup.source.Pause();
                _running.source.Pause();
                _slowdown.source.Pause();
            }
            else
            {
                _turnon.source.UnPause();
                _standby.source.UnPause();
                _speedup.source.UnPause();
                _running.source.UnPause();
                _slowdown.source.UnPause();
            }
        }


        void OnDestroy()
        {
            Pathea.PeGameMgr.PasueEvent -= OnGamePause;
        }


        void FixedUpdate()
        {
            UpdateState(Time.deltaTime);
        }
    }
}