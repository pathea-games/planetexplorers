using UnityEngine;
using System.Collections.Generic;
using ScenarioRTL;
using System.IO;

namespace PeCustom
{
	public class Stopwatch
	{
		public Stopwatch ()
		{
			name = "";
			timer = new UTimer ();
			timer.Reset();
		}

		public string name;
		public UTimer timer;
	}

	public class StopwatchMgr
	{
		public Dictionary<int, Stopwatch> stopwatches = new Dictionary<int, Stopwatch>();

		public void SetStopwatch (int id, string name, EFunc func_time, double amount_time, EFunc func_speed, float amount_speed)
		{
			if (!stopwatches.ContainsKey(id))
				stopwatches[id] = new Stopwatch();
			Stopwatch sw = stopwatches[id];
			sw.name = name;
			sw.timer.Second = Utility.Function(sw.timer.Second, amount_time, func_time);
			sw.timer.ElapseSpeed = Utility.Function(sw.timer.ElapseSpeed, amount_speed, func_speed);
		}

		public void UnsetStopwatch (int id)
		{
			stopwatches.Remove(id);
		}

		public bool CompareStopwatch (int id, ECompare comp, double amount)
		{
			double sec = 0;
			if (stopwatches.ContainsKey(id))
				sec = stopwatches[id].timer.Second;
			return Utility.Compare(sec, amount, comp);
		}

		public void Update (float deltaTime)
		{
			List<int> del_list = null;
			foreach (var kvp in stopwatches)
			{
				kvp.Value.timer.Update(deltaTime);
				if (kvp.Value.timer.Second <= 0)
				{
					kvp.Value.timer.Second = 0;
					if (OnTimeout != null)
						OnTimeout(kvp.Key);
					if (del_list == null)
						del_list = new List<int> ();
					del_list.Add(kvp.Key);
				}
			}

			if (del_list != null)
			{
				for (int i = 0; i < del_list.Count; ++i)
					stopwatches.Remove(del_list[i]);
			}
		}

		#region IO
		public void Import (BinaryReader r)
		{
			r.ReadInt32();
			int count = r.ReadInt32();
			for (int i = 0; i < count; ++i)
			{
				int key = r.ReadInt32();
				string name = r.ReadString();
				double sec = r.ReadDouble();
				float es = r.ReadSingle();
				SetStopwatch(key, name, EFunc.SetTo, sec, EFunc.SetTo, es);
			}
		}

		public void Export (BinaryWriter w)
		{
			w.Write(0);
			w.Write(stopwatches.Count);
			foreach (var kvp in stopwatches)
			{
				w.Write(kvp.Key);
				w.Write(kvp.Value.name);
				w.Write(kvp.Value.timer.Second);
				w.Write(kvp.Value.timer.ElapseSpeed);
			}
		}
		#endregion

		public delegate void DIntNotify (int id);
		public event DIntNotify OnTimeout;
	}
}
