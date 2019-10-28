using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Pathea
{
	public class SleepCooperation : Cooperation
	{

		double mStartHour;
		//public double startHour { get {return mStartHour;}}
		public SleepCooperation(int memNum,double _startHour) : base(memNum)
		{
			mStartHour = _startHour;
		}

		public override void DissolveCooper ()
		{
			throw new NotImplementedException ();
		}

		public bool IsTimeout()
		{
			return GameTime.Timer.Hour - mStartHour > CSNpcTeam.Sleep_ALl_Time;
		}
	}

}
