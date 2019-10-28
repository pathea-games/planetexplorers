//#define DEBUG_TIMER
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameTime : MonoBehaviour
{
	public static PETimer Timer = null;
	public static UTimer PlayTime = null;

	public static float NormalTimeSpeed = 12F;
	public const double StoryModeStartDay = 94.35;
	public const double BuildModeStartDay = 94.50;
	
	public static bool AbnormalityTimePass = false;
	public delegate void CancelTimePassFunc();
	public static event CancelTimePassFunc OnCancelTimePass;

    static List<GameObject> locks = new List<GameObject>(); 

	public static float DeltaTime { get { return (Timer != null) ? (Time.deltaTime * Timer.ElapseSpeed) : (0); } }
    public static void ClearTimerPass() {if(AbnormalityTimePass && OnCancelTimePass != null) OnCancelTimePass();}
    public static void Lock(GameObject obj) { if (!locks.Contains(obj)) locks.Add(obj); }
    public static void UnLock(GameObject obj) { if (locks.Contains(obj)) locks.Remove(obj); }

#if DEBUG_TIMER
	[SerializeField] private string m_TimeString;
	[SerializeField] private string m_Tick;
	[SerializeField] private double m_Day;
	[SerializeField] private double m_Hour;
	[SerializeField] private double m_Minute;
	[SerializeField] private double m_Second;
	[SerializeField] private double m_TimeInDay;
	[SerializeField] private double m_HourInDay;
	[SerializeField] private double m_MinuteInDay;
	[SerializeField] private double m_SecondInDay;
	[SerializeField] private double m_CycleInDay;
	[SerializeField] private float m_ElapseSpeed;
#endif

	// Use this for initialization
	void Start ()
	{
		Timer = new PETimer ();
		Timer.ElapseSpeed = 0;

		PlayTime = new UTimer ();
		PlayTime.ElapseSpeed = 1;
	}

	// Update is called once per frame
	void Update ()
	{
		if ( Timer != null )
		{
			if ( _passingTime )
			{
				if ( Timer.Tick >= _targetTick )
				{
					Timer.Tick = _targetTick;
					_passingTime = false;
					_targetTick = 0;
					Timer.ElapseSpeed = NormalTimeSpeed;
				}
			}

			Timer.Update(Time.deltaTime);
			PlayTime.Update(Time.deltaTime);

			if ( Application.isEditor )
			{
				if ( Input.GetKeyDown(KeyCode.Keypad1) )
				{
					Timer.ElapseSpeed = 10000;
				}
				if ( Input.GetKeyUp(KeyCode.Keypad1) )
				{
					Timer.ElapseSpeed = NormalTimeSpeed;
				}
				if ( Input.GetKey(KeyCode.Keypad1) )
				{
					Timer.ElapseSpeed *= 1.01f;
					if ( Timer.ElapseSpeed > 1000000 )
						Timer.ElapseSpeed = 1000000;
				}
			}

#if DEBUG_TIMER
			if ( Application.isEditor )
			{
				m_ElapseSpeed = Timer.ElapseSpeed;
				m_Tick = Timer.Tick.ToString("#,##0");
				m_Day = Timer.Day;
				m_Hour = Timer.Hour;
				m_Minute = Timer.Minute;
				m_Second = Timer.Second;
				m_TimeInDay = Timer.TimeInDay;
				m_HourInDay = Timer.HourInDay;
				m_MinuteInDay = Timer.MinuteInDay;
				m_SecondInDay = Timer.SecondInDay;
				m_CycleInDay = Timer.CycleInDay;
				m_TimeString = Timer.FormatString("D hh:mm:ss AP");
			}
#endif
		}
	}

	private static bool _passingTime = false;
	private static long _targetTick;

	public static void PassTime (double gameTime, double trueTime = 1)
	{
		if ( trueTime < 0.1f )
			trueTime = 0.1f;
		PETimer tar = new PETimer ();
		tar.Tick = Timer.Tick;
		PETimer add = new PETimer ();
		add.Second = gameTime;
		tar.AddTime(add);
		_targetTick = tar.Tick;

		Timer.ElapseSpeed = (float)(gameTime / trueTime);
		_passingTime = true;
	}

	#region Action Callback APIs
	public static void RPC_S2C_SyncInitGameTimer(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		GameTime.Timer.Tick = stream.Read<long>();
		GameTime.Timer.ElapseSpeed = stream.Read<float>();
		// [Edit by zx]
//		NVWeatherSys.Instance.Sun.m_PeriodTime = stream.Read<float>();
//		NVWeatherSys.Instance.Sun.m_EquaAngle = stream.Read<float>();
//		NVWeatherSys.Instance.LocalLatitude = stream.Read<float>();
		ServerAdministrator.updataFlag = true;
	}

	public static void RPC_S2C_SyncGameTimer(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		GameTime.Timer.Tick = stream.Read<long>();
	}

    public static void RPC_S2C_ElapseSpeedShift(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        GameTime.Timer.ElapseSpeed = stream.Read<float>();
    }
	#endregion
}
