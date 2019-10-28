using System.Collections.Generic;
using System.Xml;
using Vector3 = UnityEngine.Vector3;

namespace PatheaScript
{
    public abstract class Factory
    {
        const string ScriptFileExt = "xml";

        class EventInfo
        {
            public string proxyClass;
            public string eventClass;

            public override string ToString()
            {
                return string.Format("event class:{0}, proxy class:{1}", eventClass, proxyClass);
            }
        }

        Dictionary<string, EventInfo> mDicEvent;
        Dictionary<string, string> mDicCondition;
        Dictionary<string, string> mDicAction;

        protected void RegisterEvent(string name, string proxyClass, string eventClass)
        {
            EventInfo info = new EventInfo();
            info.proxyClass = proxyClass;
            info.eventClass = eventClass;

            if (null == mDicEvent)
            {
                mDicEvent = new Dictionary<string, EventInfo>(5);
            }

            if (mDicEvent.ContainsKey(name))
            {
                Debug.LogWarning("event:" + name + " [" + mDicEvent[name] + "] will be replaced by [" + info + "]");
            }

            mDicEvent[name] = info;
        }

        protected void RegisterCondition(string name, string conditionClass)
        {
            if (null == mDicCondition)
            {
                mDicCondition = new Dictionary<string, string>(5);
            }

            if (mDicCondition.ContainsKey(name))
            {
                Debug.LogWarning("condition:" + name + " [" + mDicCondition[name] + "] will be replaced by [" + conditionClass + "]");
            }

            mDicCondition[name] = conditionClass;
        }

        protected void RegisterAction(string name, string actionClass)
        {
            if (null == mDicAction)
            {
                mDicAction = new Dictionary<string, string>(5);
            }

            if (mDicAction.ContainsKey(name))
            {
                Debug.LogWarning("condition:" + name + " [" + mDicAction[name] + "] will be replaced by [" + actionClass + "]");
            }

            mDicAction[name] = actionClass;
        }

        object CreateByName(string className)
        {
            System.Type t = System.Type.GetType(className);
            if (null == t)
            {
                return null;
            }

            return System.Activator.CreateInstance(t);
        }

        #region public
        public EventProxy CreateEventProxy(string name)
        {
            if (null != mDicEvent && mDicEvent.ContainsKey(name))
            {
                EventProxy e = CreateByName(mDicEvent[name].proxyClass) as EventProxy;

                if (null != e)
                {
                    return e;
                }
            }

            throw new System.Exception("no corresponding Event proxy to " + name);
        }

        //internal event
        public Event CreateEvent(string name)
        {
            if (null != mDicEvent && mDicEvent.ContainsKey(name))
            {
                Event e = CreateByName(mDicEvent[name].eventClass) as Event;

                if (null != e)
                {
                    return e;
                }
            }

            throw new System.Exception("no corresponding Event to " + name);
        }

        public Condition CreateCondition(string name)
        {
            if (null != mDicCondition && mDicCondition.ContainsKey(name))
            {
                Condition c = CreateByName(mDicCondition[name]) as Condition;

                if (null != c)
                {
                    return c;
                }
            }

            throw new System.Exception("no corresponding Condition to " + name);
        }

        public virtual Action CreateAction(string name)
        {
            if (null != mDicAction && mDicAction.ContainsKey(name))
            {
                Action a = CreateByName(mDicAction[name]) as Action;

                if (null != a)
                {
                    return a;
                }
            }

            throw new System.Exception("no corresponding Action to " + name);
        }

        //xmlNode is context to override
        public virtual Compare GetCompare(XmlNode xmlNode, string name)
        {
            int id = Util.GetInt(xmlNode, name);

            switch (id)
            {
                case 1:
                    return new Greater();
                case 2:
                    return new GreaterEqual();
                case 3:
                    return new Equal();
                case 4:
                    return new NotEqual();
                case 5:
                    return new LesserEqual();
                case 6:
                    return new Lesser();
                default:
                    return null;
            }
        }

        //xmlNode is context to override
        public virtual Functor GetFunctor(XmlNode xmlNode, string name)
        {
            int id = Util.GetInt(xmlNode, name);

            switch (id)
            {
                case 0:
                    return new FunctorSet();
                case 1:
                    return new FunctorPlus();
                case 2:
                    return new FunctorMinus();
                case 3:
                    return new FunctorDivide();
                case 4:
                    return new FunctorMultiply();
                case 5:
                    return new FunctorMod();
                case 6:
                    return new FunctorPower();
                case 7:
                    return new FunctorNot();
                default:
                    return null;
            }
        }

        public virtual bool Init()
        {
            RegisterEvent("MISSION END",    "PatheaScript.EventProxyScriptEnd",     "PatheaScript.EventScriptEnd");
            RegisterEvent("MISSION BEGIN",  "PatheaScript.EventProxyScriptBegin",   "PatheaScript.EventScriptBegin");

            RegisterCondition("ALWAYS",     "PatheaScript.ConditionAlways");
            RegisterCondition("CHECK VAR",  "PatheaScript.ConditionCheckVar");
            RegisterCondition("SWITCH",     "PatheaScript.ConditionSwitch");

            RegisterAction("RUN MISSION",   "PatheaScript.ActionLoadScript");
            RegisterAction("END MISSION",   "PatheaScript.ActionEndScript");
            RegisterAction("SET VAR",       "PatheaScript.ActionSetVar");
            RegisterAction("SET SWITCH",    "PatheaScript.ActionSetSwitch");
            return true;
        }

        public abstract string ScriptDirPath
        {
            get;
        }

        public abstract int EntryScriptId
        {
            get;
        }

        public string GetScriptPath(int id)
        {
            return ScriptDirPath + id + "." + ScriptFileExt;
        }

        #endregion
    }
}