using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace PatheaScript
{
    public abstract class Action : TriggerChild, Storeable
    {
        enum EStep
        {
            Init,
            Running,
            Finished,
            Max
        }

        EStep mStep;
        public Action()
        {
            mStep = EStep.Init;
        }

        protected virtual bool OnInit()
        {
            return true;
        }

        protected virtual TickResult OnTick()
        {
            return TickResult.Running;
        }

        protected virtual void OnReset()
        {
        }

        public TickResult Tick()
        {
            if (mStep == EStep.Init)
            {
                if (!OnInit())
                {
                    return TickResult.Finished;
                }
                else
                {
                    mStep = EStep.Running;
                }
                //immediate action need execute in one frame
                //else
                //{
                //    return TickResult.Running;
                //}
            }

            if (mStep == EStep.Running)
            {
                if (TickResult.Finished == OnTick())
                {
                    OnReset();
                    mStep = EStep.Finished;
                }
                else
                {
                    return TickResult.Running;
                }
            }

            return TickResult.Finished;
        }

        public bool Init()
        {
            mStep = EStep.Init;
            return true;
        }

        public virtual void Store(BinaryWriter w)
        {
            w.Write((sbyte) mStep);
        }

        public virtual void Restore(BinaryReader r)
        {
            mStep = (EStep)r.ReadSByte();
        }
    }

    public abstract class ActionGroup : Action
    {
        protected List<Action> mList;
        public void Add(Action a)
        {           
            mList.Add(a);
        }

        public override bool Parse()
        {
            int childCount = mInfo.ChildNodes.Count;
            mList = new List<Action>(childCount);
            return true;
        }

        protected override bool OnInit()
        {
            if (false == base.OnInit())
            {
                return false;
            }

            if (mList.Count <= 0)
            {
                return false;
            }

            foreach (Action action in mList)
            {
                if (false == action.Init())
                {
                    return false;
                }
            }

            return true;
        }

        public override void Store(BinaryWriter w)
        {
            base.Store(w);

            foreach (Action a in mList)
            {
                a.Store(w);
            }
        }

        public override void Restore(BinaryReader r)
        {
            base.Restore(r);

            foreach (Action a in mList)
            {
                a.Restore(r);
            }
        }
    }

    public class ActionQueue : ActionGroup
    {
        int mId;
        int mCurRunningActionIndex;

        public ActionQueue()
        {
            //indicate not running
            mCurRunningActionIndex = -1;
        }

        public override bool Parse()
        {
            if (false == base.Parse())
            {
                return false;
            }

            if (mInfo.Name != "GROUP")
            {
                return false;
            }

            mId = Util.GetInt(mInfo, "index");

            foreach (XmlNode node in mInfo.ChildNodes)
            {
                Action action = mFactory.CreateAction(Util.GetStmtName(node));
                if (null != action)
                {
                    action.SetInfo(mFactory, node);
                    action.SetTrigger(mTrigger);

                    if (action.Parse())
                    {
                        Add(action);
                    }
                }
            }

            return true;
        }

        public int id
        {
            get
            {
                return mId;
            }
        }

        protected override bool OnInit()
        {
            if (false == base.OnInit())
            {
                return false;
            }

            mCurRunningActionIndex = 0;

            return true;
        }

        protected override TickResult OnTick()
        {
            if (TickResult.Finished == base.OnTick())
            {
                return TickResult.Finished;
            }

            //run all action in one tick
            while (true)
            {
                if (mList.Count == mCurRunningActionIndex)
                {
                    break;
                }

                if (TickResult.Finished == mList[mCurRunningActionIndex].Tick())
                {
                    mCurRunningActionIndex++;
                }
                else
                {
                    break;
                }
            }

            if (mList.Count == mCurRunningActionIndex)
            {
                return TickResult.Finished;
            }

            return TickResult.Running;
        }

        protected override void OnReset()
        {
            base.OnReset();
            mCurRunningActionIndex = -1;
        }

        public override void Store(BinaryWriter w)
        {
            base.Store(w);

            w.Write(mCurRunningActionIndex);
        }

        public override void Restore(BinaryReader r)
        {
            base.Restore(r);

            mCurRunningActionIndex = r.ReadInt32();
        }
    }    

    public class ActionConcurrent : ActionGroup
    {
        public override bool Parse()
        {
            if (false == base.Parse())
            {
                return false;
            }

            if (mInfo.Name != "ACTIONS")
            {
                return false;
            }

            foreach (XmlNode node in mInfo.ChildNodes)
            {
                Action action = new ActionQueue();

                action.SetInfo(mFactory, node);
                action.SetTrigger(mTrigger);

                if (action.Parse())
                {
                    Add(action);
                }
            }

            return true;
        }

        protected override TickResult OnTick()
        {
            if (TickResult.Finished == base.OnTick())
            {
                return TickResult.Finished;
            }

            TickResult result = TickResult.Finished;

            foreach(Action action in mList)
            {
                if (TickResult.Running == action.Tick())
                {
                    result = TickResult.Running;
                }
            }

            return result;
        }
    }

    public abstract class ActionImmediate : Action
    {
        public override bool Parse()
        {
            return true;
        }

        protected override TickResult OnTick()
        {
            if (TickResult.Finished == base.OnTick())
            {
                return TickResult.Finished;
            }

            Exec();
            return TickResult.Finished;
        }

        protected abstract bool Exec();
    }

    public class ActionLoadScript : ActionImmediate
    {
        protected override bool Exec()
        {
            VarRef scriptId = PatheaScript.Util.GetVarRefOrValue(mInfo, "mission", VarValue.EType.Int, mTrigger);

            if (((int)scriptId.Value) < 0)
            {
                Debug.LogError("error script id:"+scriptId);
                return false;
            }

            mTrigger.Parent.Parent.AddToLoadList((int)scriptId.Value);
         
            return true;
        }
    }

    public class ActionEndScript : ActionImmediate
    {
        protected override bool Exec()
        {
            VarRef scriptIdRef = PatheaScript.Util.GetVarRefOrValue(mInfo, "mission", VarValue.EType.Int, mTrigger);
            int scriptId = (int)scriptIdRef.Value;

            PatheaScript.PsScript.EResult result = PatheaScript.Util.GetScriptResult(mInfo);

            if (scriptId >= 0)
            {
                PsScript script = null;

                if (0 == scriptId)
                {
                    script = mTrigger.Parent;
                }
                else
                {
                    script = mTrigger.Parent.Parent.FindScriptById(scriptId);
                }

                if (null != script)
                {
                    script.RequireStop(result);
                    return true;
                }
            }
            else
            {
                List<PsScript> curScript = new List<PsScript>(mTrigger.Parent.Parent.CurScript);

                if (-2 == scriptId)
                {
                    curScript.Remove(mTrigger.Parent);
                }

                if (curScript.Count > 0)
                {
                    foreach (PsScript q in curScript)
                    {
                        q.RequireStop(result);
                    }
                }
            }

            return false;
        }
    }

    public class ActionSetVar : ActionImmediate
    {
        protected override bool Exec()
        {
            Functor functor = mFactory.GetFunctor(mInfo, "set");

            //Variable arg = new Variable((VarValue)Util.GetInt(mInfo, "value"));
            VarRef arg = Util.GetVarRefOrValue(mInfo, "value", VarValue.EType.Var, mTrigger);

            Variable.EScope eScope = Util.GetVarScope(mInfo);
            string varName = Util.GetString(mInfo, "name");

            Variable variable = mTrigger.AddVar(varName, eScope);

            functor.Set(variable, arg.Var);

            functor.Do();

            Debug.Log("execute result:" + functor);
            return true;
        }
    }

    public class ActionSetSwitch : ActionImmediate
    {
        protected override bool Exec()
        {
            Functor functor = mFactory.GetFunctor(mInfo, "set");

            bool arg = Util.GetBool(mInfo, "value");

            Variable.EScope eScope = Util.GetVarScope(mInfo);
            string varName = Util.GetString(mInfo, "name");

            Variable variable = mTrigger.AddVar(varName, eScope);

            functor.Set(variable, new Variable(arg));

            functor.Do();

            Debug.Log("execute result:" + functor);
            return true;
        }
    }
}