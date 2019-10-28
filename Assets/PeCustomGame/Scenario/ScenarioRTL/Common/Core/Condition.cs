using ScenarioRTL.IO;

namespace ScenarioRTL
{
	public abstract class Condition : StatementObject
    {
		protected int reqId = 0;
		private static int maxReqId = 0;
		protected void BeginReq ()
		{
			//if (reqId != 0)
				reqId = ++maxReqId;
			ConditionReq.AddReq(reqId);
		}

		protected void EndReq ()
		{
			ConditionReq.RemoveReq(reqId);
			reqId = 0;
		}

		protected virtual void SendReq () {}
		protected bool? ReqCheck ()
		{
			if (!isWaiting)
			{
				BeginReq();
				SendReq();
				return null;
			}
			else
			{
				bool? res = ConditionReq.GetResult(reqId);
				if (res != null)
					EndReq();
				return res;
			}
		}

		protected bool isWaiting { get { return reqId != 0; } }

        public abstract bool? Check ();
    }
}
