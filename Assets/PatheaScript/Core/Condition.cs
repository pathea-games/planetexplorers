using System.Collections.Generic;
using System.Xml;

namespace PatheaScript
{
    public abstract class Condition:TriggerChild
    {
        public virtual bool Do(){return true;}
    }

    public abstract class ConditionGroup : Condition
    {
        protected List<Condition> mList;

        public void Add(Condition c)
        {
            mList.Add(c);
        }

        public override bool Parse()
        {
            mList = new List<Condition>(mInfo.ChildNodes.Count);
            return true;
        }

        public override string ToString()
        {
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
            stringBuilder.AppendLine(this.GetType().ToString());

            foreach (Condition c in mList)
            {
                stringBuilder.AppendLine(c.ToString());
            }
            
            return stringBuilder.ToString();
        }
    }

    public class ConditionAnd : ConditionGroup
    {
        protected int mIndex;

        public override bool Do()
        {
            foreach (Condition c in mList)
            {
                if (!c.Do())
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Parse()
        {
            if (false == base.Parse())
            {
                return false;
            }

            mIndex = Util.GetInt(mInfo, "index");

            foreach (XmlNode node in mInfo.ChildNodes)
            {
                Condition c = mFactory.CreateCondition(Util.GetStmtName(node));

                if (null != c)
                {
                    c.SetInfo(mFactory, node);
                    c.SetTrigger(mTrigger);

                    if (c.Parse())
                    {                        
                        Add(c);
                    }
                }
            }

            return true;
        }
    }

    public class ConditionOr : ConditionGroup
    {
        public override bool Do()
        {
            foreach (Condition c in mList)
            {
                if (c.Do())
                {
                    return true;
                }
            }

            return false;
        }

        public override bool Parse()
        {
            if (false == base.Parse())
            {
                return false;
            }

            foreach (XmlNode node in mInfo.ChildNodes)
            {
                ConditionAnd c = new ConditionAnd();
                
                c.SetInfo(mFactory, node);
                c.SetTrigger(mTrigger);

                if (c.Parse())
                {    
                    Add(c);
                }
            }

            return true;
        }
    }

    public class ConditionAlways : Condition
    {
        public override bool Parse()
        {
            return true;
        }

        public override bool Do()
        {
            return true;
        }

        public override string ToString()
        {
            return string.Format("CAlways");
        }
    }

    public class ConditionCheckVar : Condition
    {
        VarRef mVarRef;
        Compare mCompare;
        VarRef mRefVar;

        public override bool Parse()
        {
            mCompare = mFactory.GetCompare(mInfo, "compare");

            mRefVar = Util.GetVarRefOrValue(mInfo, "ref", VarValue.EType.Var, mTrigger);

            mVarRef = Util.GetVarRef(mInfo, "name", mTrigger);

            return true;
        }

        public override bool Do()
        {
            return mCompare.Do(mVarRef.Value, mRefVar.Value);
        }

        public override string ToString()
        {
            return string.Format("Condition[CheckVar:{0} {1} {2}]", mVarRef, mCompare, mRefVar);
        }
    }

    public class ConditionSwitch : Condition
    {
        VarRef mVarRef;

        bool mRefVar;

        public override bool Parse()
        {
            mRefVar = Util.GetBool(mInfo, "value");

            mVarRef = Util.GetVarRef(mInfo, "switch", mTrigger);

            return true;
        }

        public override bool Do()
        {
            return mVarRef.Value == mRefVar;
        }

        public override string ToString()
        {
            return string.Format("Condition[Switch:{0} == {1}]", mVarRef, mRefVar);
        }
    }
}