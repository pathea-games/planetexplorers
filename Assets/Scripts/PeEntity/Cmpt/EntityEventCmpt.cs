using System;

namespace Pathea
{
	public class EntityEventCmpt : PeCmpt, IPeMsg
	{
        PeEvent.Event<EventArg> mEventor = new PeEvent.Event<EventArg>();

        public class EventArg:PeEvent.EventArg
        {
            public EMsg msg;
            public object[] args;

            public override string ToString()
            {
                return "Msg:"+msg;
            }
        }

        public PeEvent.Event<EventArg> eventor
        {
            get
            {
                return mEventor;
            }
        }

        void IPeMsg.OnMsg(EMsg msg, params object[] args)
		{
            EventArg e = new EventArg();
            e.msg = msg;
            e.args = args;

            eventor.Dispatch(e, this.Entity);
		}
    }
}
