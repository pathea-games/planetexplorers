using UnityEngine;
using System.Collections.Generic;

namespace PeCustom
{
	public class TickMgr
	{
		public int tick = 0;

		public void Tick ()
		{
			if (OnTick != null)
				OnTick(tick);

			tick++;
		}

		public delegate void DIntNotify (int t);
		public event DIntNotify OnTick;
	}
}
