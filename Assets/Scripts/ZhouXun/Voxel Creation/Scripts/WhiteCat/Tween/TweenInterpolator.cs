using System;
using UnityEngine;
using UnityEngine.Events;
using WhiteCat.Internal;

namespace WhiteCat
{
	/// <summary> 插值器 </summary>
	[AddComponentMenu("White Cat/Tween/Interpolator")]
	public class TweenInterpolator : BaseBehaviour
	{
		#region Field

		// 插值方法
		public TweenMethod method = TweenMethod.Linear;

		// 自定义曲线
		[SerializeField] AnimationCurve _customCurve = AnimationCurve.Linear(0, 0, 1, 1);

		// 延时
		[SerializeField][GetSet("delay")] float _delay = 0;

		// 总持续时间
		[SerializeField][GetSet("duration")] float _duration = 1.0f;

		// 动画速度
		[SerializeField][GetSet("speed")] float _speed = 1.0f;

		/// <summary> 循环模式 </summary>
		public WrapMode wrapMode = WrapMode.Once;

		/// <summary> 时间线 </summary>
		public TimeLine timeLine = TimeLine.Normal;

		/// <summary> 播放到开头的事件 </summary>
		public UnityEvent onArriveAtBeginning = new UnityEvent();

		/// <summary> 播放到结尾的事件 </summary>
		public UnityEvent onArriveAtEnding = new UnityEvent();


		/// <summary> 根据插值比例更新动画状态 </summary>
		public event Action<float> onTween;

		/// <summary> 记录当前的动画状态 </summary>
		public event Action onRecord;

		/// <summary> 使用记录来恢复动画状态 </summary>
		public event Action onRestore;

		#endregion




		#region Method

		/// <summary> 记录当前所有关联动画的状态 </summary>
		public void Record()
		{
			if (onRecord != null) onRecord();
		}


		/// <summary> 使用记录来恢复所有关联动画状态 </summary>
		public void Restore()
		{
			if (onRestore != null) onRestore();
		}


		/// <summary> 延时 </summary>
		public float delay
		{
			get { return _delay; }
			set { _delay = value < 0 ? 0 : value; }
		}


		/// <summary> 持续时间 </summary>
		public float duration
		{
			get { return _duration; }
			set { _duration = value > 0.01f ? value : 0.01f; }
		}


		/// <summary> 动画速度 </summary>
		public float speed
		{
			get { return _speed; }
			set { _speed = Mathf.Clamp(value, -100, 100); }
		}


		/// <summary> 将速度取反 </summary>
		public void ReverseSpeed()
		{
			_speed = -_speed;
		}


		/// <summary> 是否正在播放（在编辑器下也可以使用） </summary>
		public bool isPlaying
		{
			get
			{
#if UNITY_EDITOR
				if (!Application.isPlaying) return _isPlayingInEditor;
#endif
				return enabled;
			}
			set
			{
#if UNITY_EDITOR
				if (!Application.isPlaying)
				{
					if (_isPlayingInEditor != value)
					{
						if (value) BeginUpdateInEditor();
						else EndUpdateInEditor();
					}
				}
				else
#endif
				enabled = value;
			}
		}


		/// <summary> 重置内部时间并播放 </summary>
		public void Replay()
		{
			_time = 0;
			_normalizedTime = 0;
			isPlaying = true;
		}


		/// <summary> 单位化的当前时间 </summary>
		public float normalizedTime
		{
			get { return _normalizedTime; }
			set
			{
				_normalizedTime = Mathf.Clamp01(value);
				if (onTween != null) onTween(Interpolate(_normalizedTime));
			}
		}
		float _normalizedTime = 0;


		/// <summary> 插值方法 </summary>
		public float Interpolate(float t)
		{
			if (method == TweenMethod.CustomCurve) return _customCurve.Evaluate(t);
			else return _interpolates[(int)method](t);
		}


		static Func<float, float>[] _interpolates = new Func<float, float>[]
		{
			Interpolation.Linear,
			Interpolation.Square,
			Interpolation.Random,

			Interpolation.EaseIn,
			Interpolation.EaseOut,
			Interpolation.EaseInEaseOut,

			Interpolation.EaseInStrong,
			Interpolation.EaseOutStrong,
			Interpolation.EaseInEaseOutStrong,

			Interpolation.BackInEaseOut,
			Interpolation.EaseInBackOut,
			Interpolation.BackInBackOut,

			Interpolation.Triangle,
			Interpolation.Parabolic,
			Interpolation.Bell,
			Interpolation.Sine,
		};


		/// <summary> 添加插值器到物体上 </summary>
		public static TweenInterpolator Create(
			GameObject gameObject,
			bool isPlaying = true,
			float delay = 0.0f,
			float duration = 1.0f,
			float speed = 1.0f,
			TweenMethod method = TweenMethod.Linear,
			WrapMode wrapMode = WrapMode.Once,
			TimeLine timeLine = TimeLine.Normal,
			Action<float> onUpdate = null,
			UnityAction onArriveAtEnding = null,
			UnityAction onArriveAtBeginning = null)
		{
			TweenInterpolator interpolator = gameObject.AddComponent<TweenInterpolator>();
			interpolator.isPlaying = isPlaying;
			interpolator.delay = delay;
			interpolator.duration = duration;
			interpolator.speed = speed;
			interpolator.method = method;
			interpolator.wrapMode = wrapMode;
			interpolator.timeLine = timeLine;
			if (onUpdate != null) interpolator.onTween += onUpdate;
			if (onArriveAtEnding != null) interpolator.onArriveAtEnding.AddListener(onArriveAtEnding);
			if (onArriveAtBeginning != null) interpolator.onArriveAtBeginning.AddListener(onArriveAtBeginning);
			return interpolator;
		}

		#endregion




		#region Private

		float _time = 0;				// 计时
		float _deltaNormalizedTime;		// 间隔的单位化时间
		float _normalizedTimeTarget;	// 单位化时间目标


		void OverBeginning()
		{
			switch (wrapMode)
			{
				default:
				case WrapMode.Once:
					normalizedTime = 0;
					isPlaying = false;
					if (onArriveAtBeginning != null && Application.isPlaying) onArriveAtBeginning.Invoke();
					return;

				case WrapMode.Loop:
					do
					{
						normalizedTime = 0;
						if (onArriveAtBeginning != null && Application.isPlaying) onArriveAtBeginning.Invoke();
						_normalizedTimeTarget += 1;
					} while (_normalizedTimeTarget <= 0);
					normalizedTime = _normalizedTimeTarget;
					return;

				case WrapMode.PingPong:
					do
					{
						if (speed > 0)
						{
							normalizedTime = 1;
							if (onArriveAtEnding != null && Application.isPlaying) onArriveAtEnding.Invoke();
						}
						else
						{
							normalizedTime = 0;
							if (onArriveAtBeginning != null && Application.isPlaying) onArriveAtBeginning.Invoke();
						}
						speed = -speed;
						_normalizedTimeTarget += 1;
					} while (_normalizedTimeTarget <= 0);
					normalizedTime = speed < 0 ? _normalizedTimeTarget : (1 - _normalizedTimeTarget);
					return;

				case WrapMode.ClampForever:
					_normalizedTimeTarget = _normalizedTime;
					normalizedTime = 0;
					if (onArriveAtBeginning != null && Application.isPlaying && _normalizedTimeTarget > 0) onArriveAtBeginning.Invoke();
					return;
			}
		}


		void OverEnding()
		{
			switch (wrapMode)
			{
				default:
				case WrapMode.Once:
					normalizedTime = 1;
					isPlaying = false;
					if (onArriveAtEnding != null && Application.isPlaying) onArriveAtEnding.Invoke();
					return;

				case WrapMode.Loop:
					do
					{
						normalizedTime = 1;
						if (onArriveAtEnding != null && Application.isPlaying) onArriveAtEnding.Invoke();
						_normalizedTimeTarget -= 1;
					} while (_normalizedTimeTarget >= 1);
					normalizedTime = _normalizedTimeTarget;
					return;

				case WrapMode.PingPong:
					do
					{
						if (speed > 0)
						{
							normalizedTime = 1;
							if (onArriveAtEnding != null && Application.isPlaying) onArriveAtEnding.Invoke();
						}
						else
						{
							normalizedTime = 0;
							if (onArriveAtBeginning != null && Application.isPlaying) onArriveAtBeginning.Invoke();
						}
						speed = -speed;
						_normalizedTimeTarget -= 1;
					} while (_normalizedTimeTarget >= 1);
					normalizedTime = speed > 0 ? _normalizedTimeTarget : (1 - _normalizedTimeTarget);
					return;

				case WrapMode.ClampForever:
					_normalizedTimeTarget = _normalizedTime;
					normalizedTime = 1;
					if (onArriveAtEnding != null && Application.isPlaying && _normalizedTimeTarget < 1) onArriveAtEnding.Invoke();
					return;
			}
		}


		void UpdateTween(float deltaTime)
		{
			_time += deltaTime;
			if (_time < _delay) return;

			_deltaNormalizedTime = deltaTime * speed / _duration;
			
			if (_deltaNormalizedTime == 0)
			{
				normalizedTime = _normalizedTime;
				return;
			}

			_normalizedTimeTarget = _normalizedTime + _deltaNormalizedTime;

			if (_normalizedTimeTarget >= 1.0f)
			{
				OverEnding();
				return;
			}

			if (_normalizedTimeTarget <= 0)
			{
				OverBeginning();
				return;
			}

			normalizedTime = _normalizedTimeTarget;
		}


		void Update()
		{
#if UNITY_EDITOR
			if (!_isDragging)
#endif
			UpdateTween(timeLine == TimeLine.Normal ? Time.deltaTime : Time.unscaledDeltaTime);
		}

		#endregion




#if UNITY_EDITOR

		bool _isPlayingInEditor;
		bool _isDragging;
		int _originalSpeedSign;
		double _lastTime;


		void Reset()
		{
			TweenBase[] components = GetComponents<TweenBase>();
			foreach (TweenBase c in components)
			{
				if (c.interpolator == null)
				{
					c.interpolator = this;
				}
			}
		}


		void BeginUpdateInEditor()
		{
			Record();

			_originalSpeedSign = speed > 0 ? 1 : (speed < 0 ? -1 : 0);

			_isPlayingInEditor = true;
			_isDragging = false;
			_lastTime = UnityEditor.EditorApplication.timeSinceStartup;

			UnityEditor.EditorApplication.update += UpdateInEditor;
			UnityEditor.EditorApplication.playmodeStateChanged += EndUpdateInEditor;
		}


		void EndUpdateInEditor()
		{
			Restore();
			_normalizedTime = 0;

			if (_originalSpeedSign != 0)
			{
				if (speed > 0 && _originalSpeedSign < 0) speed = -speed;
				if (speed < 0 && _originalSpeedSign > 0) speed = -speed;
			}

			_isPlayingInEditor = false;

			UnityEditor.EditorApplication.update -= UpdateInEditor;
			UnityEditor.EditorApplication.playmodeStateChanged -= EndUpdateInEditor;
		}


		void UpdateInEditor()
		{
			if (!_isDragging)
			{
				UpdateTween((float)(UnityEditor.EditorApplication.timeSinceStartup - _lastTime));
			}

			_lastTime = UnityEditor.EditorApplication.timeSinceStartup;
		}

#endif
	}
}
