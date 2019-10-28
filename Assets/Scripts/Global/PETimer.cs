using UnityEngine;
using System.Collections;
using System.IO;

public class PETimer : UTimer
{
	public PETimer () { c_Day2Hour = 26; }	

	// Calendar Functions
	public override TimeStruct DayToCalendar (int day)
	{
		TimeStruct ts = new TimeStruct ();
		ts.Date = day;
		return ts;
	}

	public override int CalendarToDay (TimeStruct ts)
	{
		return ts.Date;
	}
}
public static class PETimerUtil
{
	static PETimer _timer = new PETimer();
	public static PETimer GetTmpTimer()
	{
		_timer.Reset ();
		return _timer;
	}
}
