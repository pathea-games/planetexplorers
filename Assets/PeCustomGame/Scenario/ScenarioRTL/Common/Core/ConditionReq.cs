using System.Collections.Generic;

namespace ScenarioRTL
{
	public static class ConditionReq
	{
		static Dictionary<int, bool?> reqs = new Dictionary<int, bool?>();

		public static void AddReq (int id)
		{
			reqs[id] = null;
		}

		public static void RemoveReq (int id)
		{
			reqs.Remove(id);
		}

		public static void AlterReq (int id, bool result)
		{
			reqs[id] = result;
		}

		public static void ClearReq ()
		{
			reqs.Clear();
		}

		public static bool? GetResult (int id)
		{
			if (reqs.ContainsKey(id))
				return reqs[id];
			return null;
		}
	}
}