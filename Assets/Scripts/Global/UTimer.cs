using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.IO;

public class UTimer
{
	// Tick - Basic Time Counter
	protected long m_Tick = 0;
	public long Tick
	{
		get { return m_Tick; }
		set { m_Tick = value; }
	}

	public UTimer (long day2Hour = 24, long hour2Min = 60, long min2Sec = 60)
	{
		c_Day2Hour = day2Hour; 
		c_Hour2Min = hour2Min;
		c_Min2Sec = min2Sec;
	}

	// Extended time format
	public double Day
	{
		get { return (double)(m_Tick * c_Tick2Sec) / Day2Sec; }
		set { m_Tick = (long)(value * Day2Sec * c_Sec2Tick); }
	}
	public double Hour
	{
		get { return (double)(m_Tick * c_Tick2Sec) / Hour2Sec; }
		set { m_Tick = (long)(value * Hour2Sec * c_Sec2Tick); }
	}
	public double Minute
	{
		get { return (double)(m_Tick * c_Tick2Sec) / c_Min2Sec; }
		set { m_Tick = (long)(value * c_Min2Sec * c_Sec2Tick); }
	}
	public double Second
	{
		get { return (double)(m_Tick * c_Tick2Sec); }
		set { m_Tick = (long)(value * c_Sec2Tick); }
	}
	
	public double TimeInDay
	{
		get
		{
			double day = Day;
			return day - System.Math.Floor(day);
		}
	}
	public double HourInDay { get { return TimeInDay * c_Day2Hour; } }
	public double MinuteInDay { get { return TimeInDay * Day2Min; } }
	public double SecondInDay { get { return TimeInDay * Day2Sec; } }
	public double CycleInDay { get { return -System.Math.Cos(TimeInDay*System.Math.PI*2.0); } }
	
	// Time Elapse Speed
	protected float m_ElapseSpeed = 0;
	public float ElapseSpeed
	{
		get { return m_ElapseSpeed; }
		set { m_ElapseSpeed = value; }
	}
	public float ElapseSpeedBak = -1f;

	// Reset
	public void Reset ()
	{
		m_Tick = 0;
		m_ElapseSpeed = 0;
	}

	// Update Timer
	public void Update (float dt)
	{
		m_Tick += (long)(m_ElapseSpeed * dt * c_Sec2Tick);
	}
	
	// I/O
	public void Import (byte[] buffer)
	{
		if ( buffer != null && buffer.Length == sizeof(long) )
			m_Tick = System.BitConverter.ToInt64(buffer, 0);
		else
			m_Tick = 0;
	}
	public byte[] Export ()
	{
		return System.BitConverter.GetBytes(m_Tick);
	}
	
	public struct TimeStruct
	{
		public int Year;
		public float Season;
		public int Month;
		public int Date;
		public int Hour;
		public int Minute;
		public int Second;
		public int Millisecond;
	}
	
	public void SetTime (int year, int month, int date, int hour, int minute, int second, int millisecond)
	{
		TimeStruct ts = new TimeStruct ();
		ts.Year = year;
		ts.Month = month;
		ts.Date = date;
		ts.Hour = hour;
		ts.Minute = minute;
		ts.Second = second;
		ts.Millisecond = millisecond;
		m_Tick = TimeStructToTick(ts);
	}

	public void SetTime (int daycount, int second)
	{
		m_Tick = (long)(daycount * Day2Sec + second) * c_Sec2Tick;
	}

	public void AddTime (UTimer timer)
	{
		m_Tick += timer.m_Tick;
	}
	
	public void MinusTime (UTimer timer)
	{
		m_Tick -= timer.m_Tick;
	}
	
	// Calendar Functions
	public virtual TimeStruct DayToCalendar (int day)
	{
		TimeStruct ts = new TimeStruct ();
		ts.Date = day;
		return ts;
	}
	
	public virtual int CalendarToDay (TimeStruct ts)
	{
		return ts.Date;
	}
	
	public TimeStruct TickToTimeStruct (long tick)
	{
		PETimer timer = PETimerUtil.GetTmpTimer();
		timer.Tick = tick;
		double day = timer.Day;
		TimeStruct ts = DayToCalendar((int)(System.Math.Floor(day)));
		
		double inday = timer.TimeInDay;
		ts.Millisecond = (int)(inday * Day2Sec * 1000);
		ts.Second = ts.Millisecond / 1000;
		ts.Millisecond = ts.Millisecond % 1000;
		ts.Minute = ts.Second / ((int)c_Min2Sec);
		ts.Second = ts.Second % ((int)c_Min2Sec);
		ts.Hour = ts.Minute / ((int)c_Hour2Min);
		ts.Minute = ts.Minute % ((int)c_Hour2Min);
		ts.Hour = ts.Hour % ((int)c_Day2Hour);
		return ts;
	}
	
	public long TimeStructToTick (TimeStruct ts)
	{
		int day = CalendarToDay(ts);
		return (  (long)(day * Day2Sec)
		        + (long)(ts.Hour * Hour2Sec)
		        + (long)(ts.Minute * c_Min2Sec)
		        + (long)(ts.Second) ) * c_Sec2Tick 
			+ (long)(ts.Millisecond * c_Millisec2Tick);
	}
	
	// Output the formatted time string
	// Y - year
	// M - month
	// D - date
	// h - hour
	// m - minute
	// s - second
	// AP - am/pm
	public string FormatString ()
	{
		return FormatString("");
	}
	public string FormatString (string format)
	{
		if ( format == null || format.Trim().Length == 0 )
			format = "YY-MM-DD hh:mm:ss AP";
		TimeStruct ts = TickToTimeStruct(m_Tick);
		
		format = format.Replace("YYYY", ts.Year.ToString("0000"));
		format = format.Replace("YYY", ts.Year.ToString("000"));
		format = format.Replace("YY", ts.Year.ToString("00"));
		format = format.Replace("Y", ts.Year.ToString("0"));
		
		format = format.Replace("MM", ts.Month.ToString("00"));
		format = format.Replace("M", ts.Month.ToString("0"));
		
		format = format.Replace("DD", ts.Date.ToString("00"));
		format = format.Replace("D", ts.Date.ToString("0"));
		
		format = format.Replace("hh", ts.Hour.ToString("00"));
		format = format.Replace("h", ts.Hour.ToString("0"));
		
		format = format.Replace("mm", ts.Minute.ToString("00"));
		format = format.Replace("ss", ts.Second.ToString("00"));

		format = format.Replace("AP", ((ts.Hour < c_Day2Hour/2) ? "" : ""));
		return format;
	}

	// Constants and Time Calendar
	protected const long c_Sec2Tick = 100000;
	protected const long c_Millisec2Tick = c_Sec2Tick / 1000;
	protected const double c_Tick2Sec = 1.0 / (double)(c_Sec2Tick);
	protected long c_Min2Sec = 60;
	protected long c_Hour2Min = 60;
	protected long c_Day2Hour = 24;
	public long Min2Sec { get { return c_Min2Sec; } }
	public long Hour2Min { get { return c_Hour2Min; } }
	public long Day2Hour { get { return c_Day2Hour; } }
	public long Hour2Sec { get { return c_Hour2Min * c_Min2Sec; } }
	public long Day2Min { get { return c_Day2Hour * c_Hour2Min; } }
	public long Day2Sec { get { return c_Day2Hour * Hour2Sec; } }
}
