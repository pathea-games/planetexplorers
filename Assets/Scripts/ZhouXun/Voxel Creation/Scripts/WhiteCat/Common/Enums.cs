
namespace WhiteCat
{
	/// <summary> 时间线 </summary>
	public enum TimeLine { Normal = 0, Unscaled = 1 }


	/// <summary> 循环模式 </summary>
	public enum WrapMode { Once = 0, Loop = 1, PingPong = 2, ClampForever = 3 }


	/// <summary> 更新模式 </summary>
	public enum UpdateMode { Update = 0, FixedUpdate = 1, LateUpdate = 2 }


	/// <summary> 旋转模式 </summary>
	public enum RotationMode { Ignore = 0, ConstantUp = 1, SlerpNodes = 2, MinimizeDelta = 3 }


	/// <summary> 插值方法 </summary>
	public enum TweenMethod : int
	{
		CustomCurve = -1,

		Linear,
		Square,
		Random,

		EaseIn,
		EaseOut,
		EaseInEaseOut,

		EaseInStrong,
		EaseOutStrong,
		EaseInEaseOutStrong,

		BackInEaseOut,
		EaseInBackOut,
		BackInBackOut,

		Triangle,
		Parabolic,
		Bell,
		Sine,
	}
}
