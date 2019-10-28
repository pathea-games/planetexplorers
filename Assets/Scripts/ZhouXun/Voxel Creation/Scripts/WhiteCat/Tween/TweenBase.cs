using UnityEngine;

namespace WhiteCat.Internal
{
	/// <summary>
	/// Tween 动画基类
	/// </summary>
	[ExecuteInEditMode]
	public abstract class TweenBase : BaseBehaviour
	{
		[SerializeField][Interpolator]TweenInterpolator _interpolator;
		public TweenInterpolator interpolator
		{
			get { return _interpolator; }
			set
			{
				if (_interpolator != value)
				{
					if (_interpolator && enabled)
					{
						_interpolator.onTween -= OnTween;
						_interpolator.onRecord -= OnRecord;
						_interpolator.onRestore -= OnRestore;
					}

					_interpolator = value;

					if (_interpolator && enabled)
					{
						_interpolator.onTween += OnTween;
						_interpolator.onRecord += OnRecord;
						_interpolator.onRestore += OnRestore;
					}
				}
			}
		}


		// 注意：这些方法在编辑器中也会调用。
		public abstract void OnTween(float factor);

		public abstract void OnRecord();

		public abstract void OnRestore();


		protected virtual void OnEnable()
		{
			if (!_interpolator) _interpolator = GetComponent<TweenInterpolator>();
			if (_interpolator)
			{
				_interpolator.onTween += OnTween;
				_interpolator.onRecord += OnRecord;
				_interpolator.onRestore += OnRestore;
			}
		}


		protected virtual void OnDisable()
		{
			if (_interpolator)
			{
				_interpolator.onTween -= OnTween;
				_interpolator.onRecord -= OnRecord;
				_interpolator.onRestore -= OnRestore;
			}
		}
	}
}
