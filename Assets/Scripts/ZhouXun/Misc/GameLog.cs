//#define SAVE_GAME_LOG
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

public class GameLogNode
{
	public DateTime LogTime;
	public LogType Type;
	public string LogString;
	public string StackTrace;
	public int Count;
}

public class GameLog : MonoBehaviour
{
	enum EStr{
		Quit,
		SendReport,
		ErrorOccured,
		Sending,
		SendAndQuit,
		StepsToReproduce,
		OclError,
		NotEnoughSpace,
		CorruptSave,
		CorruptFile,
		NoAuthority,
		MissingNetFramework,

		FileNotFound = CorruptFile,
	};
	static readonly string[] c_strListEn = new string[]{
		"Quit",
		"Send  Report",
		"A fatal error has occured, the game can not continue! \r\nPressing 'Send Report' will send us helpful debugging information that may \r\nhelp us resolve this issue in the future.",
		"Sending ",
		"Send  and  Quit",
		"Steps to reproduce:",
		"Error occurred in OpenCL kernel, which might be caused by Display Driver Stopped Responding.\r\n\r\nPlease go to launcher options and select [Cpu] in the OpenCL Calculation options if this error occurs repeatedly",
		"There is not enough space on the disk!",
		"Save data corrupted or autosave failed!",
		"Some files are corrupt, please verify the integrity of the game cache.",
		"No authorization to access a file, please check file permissions and run the game as administrator!",
		"Missing .NET Framework 3.5! \r\n\r\nSteam would install .NET framework 3.5 before running the game, but the installation seems to have failed!\r\nYou can manually install it. See more details on http://steamcommunity.com/app/237870/discussions/0/343788552540901475/",
	};
	static readonly string[] c_strListCn = new string[]{
		"退 出 游 戏",
		"发 送 报 告",
		"出错啦!游戏发生错误需要关闭! \r\n按下 '发送报告' 会给我们发送错误信息报告,帮助我们解决这个问题.",
		"发 送 中 ",
		"发 送 并 退 出",
		"重现Bug的步骤:",
		"OpenCL内核运行出错, 可能是因为显示驱动停止响应.\r\n\r\n如果该问题反复发生, 请在游戏启动器选项里的[OpenCL Calculation]选择[CPU].",
		"磁盘空间不足!",
		"存档损坏或自动存档失败!",
		"文件损坏, 请验证游戏完整性!",
		"访问文件时没有权限, 请检查文件是否可读写, 并以管理员权限运行游戏!",
		"缺少.NET Framework 3.5! \r\n\r\nSteam会在游戏启动之前安装.NET framework 3.5, 但是安装可能被取消或失败了!\r\n你可以手动安装它. 安装过程请参考http://steamcommunity.com/app/237870/discussions/0/343788552540901475/",
	};
	static string[] _strListInUse = c_strListEn;
	static string _strRuntimeError = string.Empty;
	static string _strToThrowToMainThread = string.Empty;
	public static bool IsFatalError{ get { return !string.IsNullOrEmpty (_strRuntimeError); } }

	public GUISkin GSkin;
	public string Tips = "[Ctrl]+[Alt]+[L][O][G] to show log";
	
#if SAVE_GAME_LOG
	private bool bShowLog = false;
	private List<GameLogNode> Logs;
	private int HotKeyStep = 0;
#endif
	enum ErrGuiStep{
		Step_Idle,
		Step_Report,
		Step_QuitGame,
		Step_Dying,
	};
	ErrGuiStep _bugReportStep = ErrGuiStep.Step_Idle;
	DateTime _lastLogTime = new DateTime();
	string _reproduceDesc = "";
	
	// Awake
	void Awake ()
	{
		_bugReportStep = ErrGuiStep.Step_Idle;
		DontDestroyOnLoad(this);
	}
	
	// Use this for initialization
	void Start ()
	{
		//Application.RegisterLogCallback(HandleLog);
#if SAVE_GAME_LOG
		Logs = new List<GameLogNode>();
#endif
	}

    void OnEnable()
	{
        //Application.RegisterLogCallback(HandleLog);
        Application.logMessageReceived += HandleLog;
    }
    void OnDisable()
	{
        Application.logMessageReceived -= HandleLog;
        //Application.RegisterLogCallback(null);
    }
    
#if SAVE_GAME_LOG
	// Update is called once per frame
	void Update ()
	{
		Tips = "[Ctrl]+[Alt]+[L][O][G] to show log";
		if ( Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt) )
		{
			if ( HotKeyStep == 0 && Input.GetKeyDown(KeyCode.L) )
			{
				HotKeyStep = 1;
			}
			else if ( HotKeyStep == 1 && Input.anyKeyDown )
			{
				if ( Input.GetKeyDown(KeyCode.O) )
					HotKeyStep = 2;
				else
					HotKeyStep = 0;
			}
			else if ( HotKeyStep == 2 && Input.anyKeyDown )
			{
				if ( Input.GetKeyDown(KeyCode.G) )
					bShowLog = !bShowLog;
				HotKeyStep = 0;
			}
		}
		else
		{
			HotKeyStep = 0;
		}
	}
	
	Vector2 scrollViewPos;
	string Detail = "";
#endif
	void OnGUI ()
	{
		GUI.skin = GSkin;
		GUI.depth = -1000;
		switch (_bugReportStep) {
		default:
		case ErrGuiStep.Step_Idle:
			if ( _strRuntimeError.Length > 0 )
			{
				Time.timeScale = 0;
				
				GUI.Label(new Rect(0,0,Screen.width,Screen.height), "", "BlackMask");
				GUI.Label(new Rect(40,40,24,24), "" ,"ErrorTip");
				GUI.Label(new Rect(70,46,Screen.width-140, Screen.height - 70), _strRuntimeError, "WhiteVerdanaText");
				Rect rect = new Rect( (Screen.width - 566)/2, (Screen.height-156)/2, 566, 156 );
				GUI.BeginGroup(rect,"","MsgBoxWindow");
				// Title / Caption
				GUI.Label( new Rect(1,7,rect.width,26), "System Error", "MsgCaptionTextSD" );
				GUI.Label( new Rect(0,6,rect.width,26), "System Error", "MsgCaptionText" );
				
				// Content
				GUI.Label( new Rect(25,46,48,48), "", "ErrorSignal" );
				GUI.Label( new Rect(88,59,450,90), _strListInUse[(int)EStr.ErrorOccured], "WhiteVerdanaText" );
				
				int button_x_begin = ((int)rect.width - 108) / 2;
				
				if ( GUI.Button(new Rect(button_x_begin-100, 112, 108, 24), _strListInUse[(int)EStr.Quit] ,"ButtonStyle") )
				{
					_bugReportStep = ErrGuiStep.Step_QuitGame;
				}
				if ( GUI.Button(new Rect(button_x_begin+100, 112, 108, 24), _strListInUse[(int)EStr.SendReport] ,"ButtonStyle") )
				{ 
					_bugReportStep = ErrGuiStep.Step_Report;
				}
				GUI.EndGroup();
			} else if(!string.IsNullOrEmpty(_strToThrowToMainThread)){
				Debug.LogError(_strToThrowToMainThread);
				_strToThrowToMainThread = string.Empty;
			}
			break;
		case ErrGuiStep.Step_Report:
			if(true)
			{
				Time.timeScale = 0;
				
				GUI.Label(new Rect(0,0,Screen.width,Screen.height), "", "BlackMask");
				GUI.Label(new Rect(40,40,24,24), "" ,"ErrorTip");
				GUI.Label(new Rect(70,46,Screen.width-140, Screen.height - 70), _strRuntimeError, "WhiteVerdanaText");

				Rect rect = new Rect( (Screen.width - 566)/2, (Screen.height-156)/2, 566, 156 );
				GUI.BeginGroup(rect,"","MsgBoxWindow");
				// Title / Caption
				GUI.Label( new Rect(1,7,rect.width,26), "System Error", "MsgCaptionTextSD" );
				GUI.Label( new Rect(0,6,rect.width,26), "System Error", "MsgCaptionText" );

				GUI.Label(new Rect(12,36,240,20), _strListInUse[(int)EStr.StepsToReproduce] ,"WhiteVerdanaText");
				_reproduceDesc = GUI.TextArea(new Rect(20,56, 526, 60), _reproduceDesc);
				int button_x_begin = ((int)rect.width - 108) / 2;
				if(BugReporter.IsSending){
					string strSending = _strListInUse[(int)EStr.Sending];
					int len = ((int)Time.realtimeSinceStartup)%(strSending.Length);
					GUI.Button(new Rect(button_x_begin, 116, 108, 24), strSending.Substring(0, len),"ButtonStyle");
				} 
				else if ( GUI.Button(new Rect(button_x_begin, 116, 108, 24), _strListInUse[(int)EStr.SendAndQuit],"ButtonStyle") )
				{ 
					BugReporter.SendEmailAsync(_strRuntimeError + "\nReproduce Steps:\n"+_reproduceDesc, 5, delegate {_bugReportStep = ErrGuiStep.Step_QuitGame;});
				}
				GUI.EndGroup();
			}
			break;
		case ErrGuiStep.Step_QuitGame:
			_bugReportStep = ErrGuiStep.Step_Dying;
			Debug.Log(_lastLogTime.ToString("G")+"[Quit Game Unexpectedly]");
			Application.Quit();
#if UNITY_EDITOR
			Time.timeScale = 1;
			//Application.RegisterLogCallback(null);
#endif
			break;
		case ErrGuiStep.Step_Dying:
			// Donothing but dying
			break;
		}

#if SAVE_GAME_LOG
		if ( bShowLog )
		{
			GUI.Label(new Rect( 0, Screen.height - 380, Screen.width, 380 ), "", "BottomBG3ZX");
			GUI.Label(new Rect( -20, Screen.height - 375, Screen.width, 20 ), "Game Logs:", "LogTip");
			scrollViewPos = GUI.BeginScrollView(new Rect( 0, Screen.height - 350, Screen.width - 8, 246 ), scrollViewPos, new Rect( 0, 0, Screen.width - 20, Logs.Count*25) );
			for ( int i = 0; i < Logs.Count; i++ )
			{
				if ( Logs[i].Type == LogType.Log )
				{
					GUI.Label(new Rect(12, i*25, Screen.width -100, 25), Logs[i].LogTime.ToShortTimeString() + "\t\t" + Logs[i].LogString, "LogTip");
					GUI.Label(new Rect(Screen.width-80, i*25, 50, 25), Logs[i].Count.ToString(), "LogTip");
				}
				else if ( Logs[i].Type == LogType.Warning )
				{
					GUI.Label(new Rect(12, i*25, Screen.width -100, 25), Logs[i].LogTime.ToShortTimeString() + "\t\t" + Logs[i].LogString, "WarningTip");
					GUI.Label(new Rect(Screen.width-80, i*25, 50, 25), Logs[i].Count.ToString(), "WarningTip");
				}
				else
				{
					GUI.Label(new Rect(12, i*25, Screen.width -100, 25), Logs[i].LogTime.ToShortTimeString() + "\t\t" + Logs[i].LogString, "ErrorTip");
					GUI.Label(new Rect(Screen.width-80, i*25, 50, 25), Logs[i].Count.ToString(), "ErrorTip");
				}
				if ( GUI.Button(new Rect(Screen.width-155, i*25+2, 73, 22), "Detail", "ButtonStyle") )
				{
					Detail = Logs[i].StackTrace;
				}
			}
			GUI.EndScrollView();
			GUI.Label(new Rect( 0, Screen.height - 350, Screen.width, 250 ), "", "NumberInput");
			GUI.TextField(new Rect( 4, Screen.height - 99, Screen.width-8, 98 ), Detail, "WhiteVerdanaText");
			GUI.Label(new Rect( 0, Screen.height - 100, Screen.width, 100 ), "", "NumberInput");
		}
#endif
	}
	
    void HandleLog (string logString, string stackTrace, LogType type)
	{
#if SAVE_GAME_LOG
		if ( Logs == null )
			Logs = new List<GameLogNode> ();
		bool found = false;
        for ( int i = 0; i < Logs.Count; i++ )
		{
			if ( Logs[i].Type == type && Logs[i].LogString == logString && Logs[i].StackTrace == stackTrace )
			{
				Logs[i].Count = Logs[i].Count + 1;
				found = true;
				break;
			}
		}
		if ( !found )
		{
			GameLogNode log = new GameLogNode ();
			log.LogTime = DateTime.Now;
			log.LogString = logString;
			log.StackTrace = stackTrace;
			log.Type = type;
			log.Count = 1;
			Logs.Add(log);
			
			// If there are many logs, delete old and useless log.
			if ( Logs.Count > 512 )
			{
		        for ( int i = 0; i < Logs.Count; i++ )
				{
					if ( Logs[i].Type == LogType.Log )
					{
						Logs.RemoveAt(i);
						break;
					}			
				}
			}
		}
#endif
		
		//
		// Catch Null pointer error. 
		// 
		// !!!!! DO NOT Delete !!!!!
		//
		if (type == LogType.Assert || type == LogType.Error || type == LogType.Exception)
		{
			if(string.IsNullOrEmpty(_strRuntimeError))
			{
				int ex_pos = logString.IndexOf("Exception");				
				int line_end = logString.IndexOf("\n");
				if ( line_end < 0 )
					line_end = logString.Length;				
				
				if ( ex_pos >= 0 && ex_pos < line_end )
				{
					_strListInUse = SystemSettingData.Instance.IsChinese ? c_strListCn : c_strListEn;
					if(logString.Contains("OclKernelError")){
						_strRuntimeError = _strListInUse[(int)EStr.OclError];
					} else if(logString.Contains("IOException: Win32 IO returned 112.") || logString.Contains("IOException: Disk full.")){
						_strRuntimeError = _strListInUse[(int)EStr.NotEnoughSpace] + "\r\n\r\n" + logString + "\r\n" + stackTrace;
					} else if(logString.Contains("SaveDataCorrupt")){
						_strRuntimeError = _strListInUse[(int)EStr.CorruptSave] + "\r\n\r\n" + logString + "\r\n" + stackTrace;
					} else if(logString.Contains("FilesCorrupt") || logString.Contains("IO.EndOfStreamException")){
						_strRuntimeError = _strListInUse[(int)EStr.CorruptFile] + "\r\n\r\n" + logString + "\r\n" + stackTrace;
					} else if(logString.Contains("IO.FileNotFoundException")){
						_strRuntimeError = _strListInUse[(int)EStr.FileNotFound] + "\r\n\r\n" + logString + "\r\n" + stackTrace;
					} else if(logString.Contains("UnauthorizedAccessException")){
						_strRuntimeError = _strListInUse[(int)EStr.NoAuthority] + "\r\n\r\n" + logString + "\r\n" + stackTrace;
					} else if(logString.Contains("DllNotFoundException")){
						_strRuntimeError = _strListInUse[(int)EStr.MissingNetFramework];
					} else {
						_strRuntimeError = logString + "\r\n\r\n" + stackTrace;
					}
					_lastLogTime = DateTime.Now;
					// Force disp cursor
					try{ 
						Cursor.lockState = Screen.fullScreen? CursorLockMode.Confined: CursorLockMode.None;
						Cursor.visible = true;
						PeCamera.SetVar("ForceShowCursor", true);
					}catch{}
				}
			}
		}
    }

	public enum EIOFileType{
		SaveData,
		Settings,
		InstallFiles,
		Other,
	}
	public static void HandleIOException(Exception ex, EIOFileType type = EIOFileType.SaveData)
	{
		string strEx = ex.ToString ();
		if (strEx.Contains ("IOException: Win32 IO returned 112.")) {
			Debug.LogError (strEx);
		} else if(strEx.Contains ("UnauthorizedAccessException")){
			Debug.LogError (strEx);
		} else {
			switch(type){
			case EIOFileType.SaveData:
				if(Pathea.ArchiveMgr.Instance != null && Pathea.ArchiveMgr.Instance.autoSave){
					// Ignore autosave
					Debug.LogWarning ("AutoSaveDataCorrupt:" + strEx);
				} else {
					Debug.LogError ("SaveDataCorrupt:" + strEx);
				}
				break;
			case EIOFileType.InstallFiles:
				Debug.LogError ("FilesCorrupt:" + strEx);
				break;
			default:
				Debug.LogWarning (ex);
				break;
			}
		}
	}
	public static void HandleExceptionInThread(Exception ex)
	{
		string strEx = ex.ToString ();
		if (strEx.Contains ("IOException: Win32 IO returned 112.")) {
			_strToThrowToMainThread = strEx;
		}
	}
}
