using ScenarioRTL.IO;

namespace ScenarioRTL
{
	public abstract class EventListener : StatementObject
    {
        public abstract void Listen();
        public abstract void Close();

        protected void Post ()
        {
			if (OnPost != null)
			{
				OnPost(this);
			}
			if (trigger != null)
			{
	            if (!trigger.enabled)
	                return;

	            if (!trigger.isAlive)
	                return;

	            trigger.InitConditions();
				trigger.StartProcessCondition();
			}
        }

		public delegate void PostNotify (EventListener evtl);
		public event PostNotify OnPost;
    }
}
