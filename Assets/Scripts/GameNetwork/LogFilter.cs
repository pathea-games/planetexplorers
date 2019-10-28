public class LogFilter
{
	public enum EFilterLevel
	{
		Develop,
		Debug,
		Info,
		Warn,
		Error,
		Fatal
	}

#if UNITY_EDITOR
	public static EFilterLevel curLevel = EFilterLevel.Debug;
#else
	public static EFilterLevel curLevel = EFilterLevel.Warn;
#endif

	public static bool logDev { get { return curLevel <= EFilterLevel.Develop; } }
	public static bool logDebug { get { return curLevel <= EFilterLevel.Debug; } }
	public static bool logInfo { get { return curLevel <= EFilterLevel.Info; } }
	public static bool logWarn { get { return curLevel <= EFilterLevel.Warn; } }
	public static bool logError { get { return curLevel <= EFilterLevel.Error; } }
	public static bool logFatal { get { return curLevel <= EFilterLevel.Fatal; } }
}
