using System.Collections.Generic;
using System.Xml;

namespace PatheaScript
{
    public abstract class ParseObj
    {
        protected Factory mFactory;
        protected XmlNode mInfo;

        public void SetInfo(Factory factory, XmlNode xmlNode)
        {
            mFactory = factory;
            mInfo = xmlNode;
        }

        public abstract bool Parse();
    }

    public abstract class TriggerChild : ParseObj
    {
        protected Trigger mTrigger;
        public void SetTrigger(Trigger trigger)
        {
            mTrigger = trigger;
        }
    }

    public class Trigger : ParseObj, Event.IMsgHandler, Storeable
    {
        class RepeatCount
        {
            int mCount;
            public RepeatCount(int count)
            {
                mCount = count;
            }

            public void Manus()
            {
                if (mCount <= 0)
                {
                    return;
                }

                mCount--;
            }

            public bool IsZero
            {
                get
                {
                    return mCount == 0;
                }
            }

            public override string ToString()
            {
                return mCount.ToString();
            }

            public int Value
            {
                get
                {
                    return mCount;
                }
            }
        }

        enum EStep
        {
            EventListenning,
            Comparing,
            ActionRunning,
            Finished,
            Max
        }

        public string Name { get; private set; }
        RepeatCount Repeat { get; set; }
        EStep mStep;

        EventGroup mEvent;
        ConditionOr mCondition;
        ActionConcurrent mAction;
        PsScript mScript;

        protected VariableMgr mVarMgr;

        public PsScript Parent
        {
            get
            {
                return mScript;
            }
        }

        public void RequireStop()
        {
            mEvent.SetMsgHndler(null);

            if (mStep == EStep.ActionRunning)
            {
                Repeat = new RepeatCount(1);
            }
            else
            {
                Repeat = new RepeatCount(0);
            }
        }

        public Variable GetVar(string varName, bool bFindInParent = true)
        {
            Variable var = mVarMgr.GetVar(varName);
            if (null != var)
            {
                return var;
            }

            if (false == bFindInParent)
            {
                return var;
            }

            return Parent.GetVar(varName);
        }

        public Variable AddVar(string varName, Variable.EScope eScope)
        {
            Variable var = null;

            if (Variable.EScope.Gloabel == eScope)
            {
                var = Parent.Parent.GetVar(varName);
                if (null == var)
                {
                    var = new Variable();
                    Parent.Parent.AddVar(varName, var);
                }
            }
            else if (Variable.EScope.Script == eScope)
            {
                var = Parent.GetVar(varName, false);
                if (null == var)
                {
                    var = new Variable();
                    Parent.AddVar(varName, var);
                }
            }
            else if (Variable.EScope.Trigger == eScope)
            {
                var = GetVar(varName, false);
                if (null == var)
                {
                    var = new Variable();
                    mVarMgr.AddVar(varName, var);
                }
            }            

            return var;
        }

        public Trigger(PsScript script)
        {
            mScript = script;
            mStep = EStep.Max;
            mVarMgr = new VariableMgr();
        }

        public override bool Parse()
        {
            Name = Util.GetString(mInfo, "name");
            Repeat = new RepeatCount(Util.GetInt(mInfo, "repeat"));

            foreach (XmlNode node in mInfo.ChildNodes)
            {
                if (node.Name == "EVENTS")
                {
                    mEvent = new EventGroup();
                    mEvent.SetInfo(mFactory, node);
                    mEvent.SetTrigger(this);

                    if (false == mEvent.Parse())
                    {
                        return false;
                    }
                }
                else if (node.Name == "CONDITIONS")
                {
                    mCondition = new ConditionOr();
                    mCondition.SetInfo(mFactory, node);
                    mCondition.SetTrigger(this);

                    if (false == mCondition.Parse())
                    {
                        return false;
                    }
                }
                else if (node.Name == "ACTIONS")
                {
                    mAction = new ActionConcurrent();
                    mAction.SetInfo(mFactory, node);
                    mAction.SetTrigger(this);

                    if (false == mAction.Parse())
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void OnEventTriggered()
        {
            if (mStep != EStep.EventListenning)
            {
                return;
            }

            mStep = EStep.Comparing;

            Tick();
        }

        public TickResult Tick()
        {
            if (EStep.Finished == mStep)
            {
                return TickResult.Finished;
            }

            if (mStep == EStep.Comparing)
            {
                if (mCondition.Do())
                {
                    mAction.Init();

                    mStep = EStep.ActionRunning;
                }
                else
                {
                    mStep = EStep.EventListenning;
                }
            }
                
            if (mStep == EStep.ActionRunning)
            {
                if (TickResult.Finished == mAction.Tick())
                {
                    Repeat.Manus();

                    if (false == Repeat.IsZero)
                    {
                        mStep = EStep.EventListenning;
                    }
                    else
                    {
                        mStep = EStep.Finished;
                    }                    
                }
            }

            if (EStep.Finished == mStep || Repeat.IsZero)
            {
                return TickResult.Finished;
            }
            else
            {
                return TickResult.Running;
            }
        }

        public bool Init()
        {
            if (false == mEvent.Init())
            {
                return false;
            }

            mEvent.SetMsgHndler(this);

            mStep = EStep.EventListenning;
            return true;
        }

        public void Reset()
        {
            mEvent.SetMsgHndler(null);
            mEvent.Reset();
        }

        public override string ToString()
        {
            return string.Format("Trigger:Name={0},Repeat={1}", Name, Repeat);
        }

        public void Store(System.IO.BinaryWriter w)
        {
            w.Write(Repeat.Value);
            w.Write((sbyte)mStep);

            byte[] data = VariableMgr.Export(mVarMgr);
            w.Write(data.Length);
            w.Write(data);

            mAction.Store(w);
        }

        public void Restore(System.IO.BinaryReader r)
        {
            Repeat = new RepeatCount(r.ReadInt32());
            mStep = (EStep)r.ReadSByte();

            int length = r.ReadInt32();
            byte[] data = r.ReadBytes(length);
            mVarMgr = VariableMgr.Import(data);

            mAction.Restore(r);
        }
    }

    public class TriggerGroup : ParseObj, Storeable
    {
        List<Trigger> mList;
        
        PsScript mScript;
        public TriggerGroup(PsScript script)
        {
            mScript = script;
        }

        public void RequireStop()
        {
            foreach (Trigger trigger in mList)
            {
                trigger.RequireStop();
            }
        }

        public override bool Parse()
        {
            mList = new List<Trigger>(mInfo.ChildNodes.Count);

            foreach (XmlNode node in mInfo.ChildNodes)
            {
                Trigger trigger = new Trigger(mScript);
                trigger.SetInfo(mFactory, node);
                if (trigger.Parse())
                {
                    mList.Add(trigger);
                }
            }

            return true;
        }

        public bool Init()
        {
            if (null == mList)
            {
                return false;
            }

            foreach (Trigger t in mList)
            {
                if (!t.Init())
                {
                    return false;
                }
            }
            return true;
        }

        public TickResult Tick()
        {
            if (null == mList)
            {
                return TickResult.Finished;
            }

            TickResult result = TickResult.Finished;
            mList.ForEach(delegate(Trigger t)
            {
                if (t.Tick() == TickResult.Running)
                {
                    result = TickResult.Running;
                }
            });

            return result;
        }

        public void Reset()
        {
            if (null == mList || mList.Count == 0)
            {
                return;
            }

            mList.ForEach(delegate(Trigger t)
            {
                t.Reset();
            });
        }

        public void Store(System.IO.BinaryWriter w)
        {
            foreach (Trigger t in mList)
            {
                t.Store(w);
            }
        }

        public void Restore(System.IO.BinaryReader r)
        {
            foreach (Trigger t in mList)
            {
                t.Restore(r);
            }
        }
    }
}