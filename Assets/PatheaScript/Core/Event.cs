using System.Collections.Generic;
using System.Xml;

namespace PatheaScript
{
    /*
     * EventProxy用来实现不同Trigger里面的相同Event，通过Event的Priority来控制Trigger的执行顺序。
     */
    public abstract class EventProxy
    {
        List<Event> mList;
        public string Name
        {
            get;
            set;
        }

        public void Add(Event e)
        {
            if (null == mList)
            {
                mList = new List<Event>(5);
            }

            mList.Add(e);            
        }

        public bool Remove(Event e)
        {
            return mList.Remove(e);
        }

        public int Count
        {
            get
            {
                if (null == mList)
                {
                    return 0;
                }

                return mList.Count;
            }
        }

        public virtual bool Subscribe() { return true;}
        public virtual void Tick() { }
        public virtual void Unsubscribe() { }

        protected void Emit(params object[] param)
        {
            if (null == mList || 0 == mList.Count)
            {
                return;
            }

            List<Event> emitList = mList.FindAll(delegate(Event e){
                return e.Filter(param);
            });
            

            emitList.Sort(delegate(Event x, Event y)
            {
                if (x.Priority > y.Priority)
                {
                    return 1;
                }
                else if (x.Priority == y.Priority)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            });

            emitList.ForEach(delegate(Event e) {
                e.Emit();
            });
        }
    }

    public class EventProxyMgr
    {
        PsScriptMgr mScriptMgr;

        List<EventProxy> mList;

        public EventProxyMgr(PsScriptMgr scriptMgr)
        {
            mScriptMgr = scriptMgr;
        }

        public void Tick()
        {
            if (null == mList)
            {
                return;
            }

            mList.ForEach(delegate(EventProxy e)
            {
                e.Tick();
            });
        }

        EventProxy GetProxy(string name)
        {
            return mList.Find(delegate(EventProxy ep)
            {
                if (string.Equals(ep.Name, name))
                {
                    return true;
                }

                return false;
            });
        }

        public bool SubEvent(Event e)
        {
            if (null == e)
            {
                return false;
            }

            if (null == mList)
            {
                mList = new List<EventProxy>(5);
            }

            EventProxy eventProxy = GetProxy(e.Name);

            if (null == eventProxy)
            {
                eventProxy = mScriptMgr.Factory.CreateEventProxy(e.Name);
                if (null == eventProxy)
                {
                    return false;
                }

                eventProxy.Name = e.Name;

                if (!eventProxy.Subscribe())
                {
                    return false;
                }
                else
                {
                    mList.Add(eventProxy);
                }
            }

            eventProxy.Add(e);

            return true;
        }

        public bool UnsubEvent(Event e)
        {
            if (null == e)
            {
                return false;
            }

            if (null == mList)
            {
                return false;
            }

            EventProxy ep = GetProxy(e.Name);

            if (null == ep)
            {
                return false;
            }
            
            if (false == ep.Remove(e))
            {
                return false;
            }

            if (ep.Count <= 0)
            {
                ep.Unsubscribe();
                mList.Remove(ep);
            }

            return true;
        }
    }

    public abstract class Event:TriggerChild
    {
        public string Name
        {
            get;
            set;
        }

        public int Priority
        {
            get;
            private set;
        }

        public abstract bool Filter(params object[] param);

        public interface IMsgHandler
        {
            void OnEventTriggered();
        }

        IMsgHandler mMsgHandler;

        public void Emit()
        {
            if (null == mMsgHandler)
            {
                return;
            }

            //Debug.Log(this);
            
            mMsgHandler.OnEventTriggered();
        }

        public void SetMsgHndler(IMsgHandler msgHandler)
        {
            mMsgHandler = msgHandler;
        }

        public override bool Parse()
        {
            Priority = Util.GetEventPriority(mInfo);

            return true;
        }

        public virtual bool Init()
        {
            mTrigger.Parent.Parent.EventProxyMgr.SubEvent(this);
            return true;
        }

        public virtual void Reset()
        {
            mTrigger.Parent.Parent.EventProxyMgr.UnsubEvent(this);
            SetMsgHndler(null);
        }
    }

    public class EventGroup : TriggerChild
    {
        List<Event> mList;

        void Add(Event e)
        {
            mList.Add(e);
        }

        public override bool Parse()
        {
            mList = new List<Event>(mInfo.ChildNodes.Count);

            foreach (XmlNode childNode in mInfo.ChildNodes)
            {
                string eventName = Util.GetStmtName(childNode);
                Event e = mFactory.CreateEvent(eventName);

                if (null != e)
                {
                    e.SetInfo(mFactory, childNode);
                    e.SetTrigger(mTrigger);
                    e.Name = eventName;

                    if (e.Parse())
                    {
                        Add(e);
                    }
                }
            }

            return true;
        }

        public void SetMsgHndler(Event.IMsgHandler msgHandler)
        {            
            foreach (Event e in mList)
            {
                e.SetMsgHndler(msgHandler);
            }
        }

        public bool Init()
        {
            foreach(Event e in mList)
            {
                if(false == e.Init())
                {
                    return false;
                }
            }

            return true;
        }

        public void Reset()
        {
            mList.ForEach(delegate(Event e)
            {
                e.Reset();
            });
        }
    }

    public class EventProxyScriptEnd : EventProxy
    {
        void ScriptEnd(PsScript script)
        {
            if (null == script)
            {
                return;
            }

            Emit(script);
        }

        public override bool Subscribe()
        {
            if (false == base.Subscribe())
            {
                return false;
            }

            InternalEvent.Instance.eventScriptEnd += ScriptEnd;
            return true;
        }

        public override void Unsubscribe()
        {
            InternalEvent.Instance.eventScriptEnd -= ScriptEnd;

            base.Unsubscribe();
        }
    }

    public class EventScriptEnd : Event
    {
        VarRef mScriptIdRef;
        PsScript.EResult mResult = PsScript.EResult.Max;

        public override bool Parse()
        {
            if (false == base.Parse())
            {
                return false;
            }

            mScriptIdRef = Util.GetVarRefOrValue(mInfo, "mission", VarValue.EType.Int, mTrigger);

            mResult = Util.GetScriptResult(mInfo);

            return true;
        }

        public override bool Filter(params object[] param)
        {
            if(1 != param.Length)
            {
                return false;
            }

            PsScript script = param[0] as PsScript;
            if (null == script)
            {
                return false;
            }

            int scriptId = (int)mScriptIdRef.Value;

            //self
            if (0 == scriptId)
            {
                scriptId = mTrigger.Parent.Id;
            }

            if (script.Id != scriptId)
            {
                return false;
            }

            if (script.Result != mResult)
            {
                return false;
            }

            return true;
        }

    }

    public class EventProxyScriptBegin : EventProxy
    {
        void ScriptBegin(PsScript script)
        {
            if (null == script)
            {
                return;
            }

            Emit(script);
        }

        public override bool Subscribe()
        {
            if (false == base.Subscribe())
            {
                return false;
            }

            InternalEvent.Instance.eventScriptBegin += ScriptBegin;
            return true;
        }

        public override void Unsubscribe()
        {
            InternalEvent.Instance.eventScriptBegin -= ScriptBegin;

            base.Unsubscribe();
        }
    }

    public class EventScriptBegin : Event
    {
        VarRef mScriptIdRef;        

        public override bool Parse()
        {
            if (false == base.Parse())
            {
                return false;
            }

            mScriptIdRef = Util.GetVarRefOrValue(mInfo, "mission", VarValue.EType.Int, mTrigger);

            return true;
        }

        public override bool Filter(params object[] param)
        {
            if (1 != param.Length)
            {
                return false;
            }

            PsScript script = param[0] as PsScript;
            if (null == script)
            {
                return false;
            }

            int scriptId = (int)mScriptIdRef.Value;
            //self
            if (0 == scriptId)
            {
                scriptId = mTrigger.Parent.Id;
            }

            if (script.Id != scriptId)
            {
                return false;
            }

            return true;
        }
    }
}