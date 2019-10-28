using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;

public class LogManager
{
	private static uLink.NetworkLogFlags customLogFlags;
	//private static uLink.NetworkLogLevel customLogLevel;

	public static void InitLogManager()
	{
		try{
			string dir = Path.Combine(Application.dataPath, "../log");
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			SetLevel(uLink.NetworkLogFlags.AuthoritativeServer, uLink.NetworkLogLevel.Debug);

			uLink.NetworkLog.errorWriter = delegate(uLink.NetworkLogFlags flags, object[] args)
			{
				string info = string.Format("{0} => {1}{2}", System.DateTime.Now, uLink.NetworkLogUtility.ObjectsToString(args), System.Environment.NewLine);
				string fileName = string.Format("{0}/{1:d4}{2:d2}{3:d2}_error.log", dir, System.DateTime.Today.Year, System.DateTime.Today.Month, System.DateTime.Today.Day);

				File.AppendAllText(fileName.ToString(), info.ToString());
			};

			uLink.NetworkLog.warningWriter = delegate(uLink.NetworkLogFlags flags, object[] args)
			{
				string info = string.Format("{0} => {1}{2}", System.DateTime.Now, uLink.NetworkLogUtility.ObjectsToString(args), System.Environment.NewLine);
				string fileName = string.Format("{0}/{1:d4}{2:d2}{3:d2}_warning.log", dir, System.DateTime.Today.Year, System.DateTime.Today.Month, System.DateTime.Today.Day);

				File.AppendAllText(fileName.ToString(), info.ToString());
			};

			uLink.NetworkLog.infoWriter = delegate(uLink.NetworkLogFlags flags, object[] args)
			{
				string info = string.Format("{0} => {1}{2}", System.DateTime.Now, uLink.NetworkLogUtility.ObjectsToString(args), System.Environment.NewLine);
				string fileName = string.Format("{0}/{1:d4}{2:d2}{3:d2}_info.log", dir, System.DateTime.Today.Year, System.DateTime.Today.Month, System.DateTime.Today.Day);

				File.AppendAllText(fileName.ToString(), info.ToString());
			};
		}catch(System.Exception e){
			Debug.Log("Failed to InitLogManager! "+e.ToString());
		}
	}

	public static void SetLevel(uLink.NetworkLogFlags logFlags, uLink.NetworkLogLevel logLevel)
	{
		customLogFlags = logFlags;
		//customLogLevel = logLevel;
		uLink.NetworkLog.SetLevel(logFlags, logLevel);
	}

	public static void Info(params object[] args)
	{
		Info(customLogFlags, args);
	}

	public static void Info(uLink.NetworkLogFlags flags, params object[] args)
	{
		uLink.NetworkLog.Info(flags, args);
	}

	public static void Warning(params object[] args)
	{
		Warning(customLogFlags, args);
	}

	public static void Warning(uLink.NetworkLogFlags flags, params object[] args)
	{
		uLink.NetworkLog.Warning(flags, args);
	}

	public static void Error(params object[] args)
	{
		Error(customLogFlags, args);
	}

	public static void Error(uLink.NetworkLogFlags flags, params object[] args)
	{
		uLink.NetworkLog.Error(flags, args);
	}
}
