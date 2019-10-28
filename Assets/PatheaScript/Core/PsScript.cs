using System.Collections.Generic;
using System.Xml;

namespace PatheaScript
{
    public class PsScript:Storeable
    {
        public enum EResult
        {
            Accomplished,
            Failed,
            Abort,
            Max
        }

        public int Id { get; private set; }
        public string Name { get; private set; }

        TriggerGroup mTriggerGroup;
        VariableMgr mVarMgr;
        PsScriptMgr mMgr;
        public EResult Result
        {
            get;
            private set;
        }

        PsScript(PsScriptMgr mgr)
        {
            mMgr = mgr;
            mVarMgr = new VariableMgr();

            Result = EResult.Max;
        }

        public PsScriptMgr Parent
        {
            get
            {
                return mMgr;
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

        public bool AddVar(string varName, Variable var)
        {
            return mVarMgr.AddVar(varName, var);
        }

        public static PsScript Load(PsScriptMgr mgr, int id)
        {
            string path = mgr.Factory.GetScriptPath(id);

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                XmlNode scriptNode = doc.SelectSingleNode("//MISSION");

                if (scriptNode == null)
                {
                    return null;
                }

                PsScript script = new PsScript(mgr);

                script.Id = id;

                script.Name = Util.GetString(scriptNode, "name");

                TriggerGroup triggerGroup = new TriggerGroup(script);
                triggerGroup.SetInfo(mgr.Factory, scriptNode);
                if (false == triggerGroup.Parse())
                {
                    return null;
                }
                else
                {
                    script.mTriggerGroup = triggerGroup;
                }

                return script;
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
                return null;
            }
        }

        public override string ToString()
        {
            return string.Format("Script[id={0}, name={1}]", Id, Name);
        }

        public void RequireStop(EResult eResult)
        {
            Result = eResult;
            mTriggerGroup.RequireStop();

            Debug.Log(this+" require stop.");
            InternalEvent.Instance.EmitScriptEnd(this);
        }

        public bool Init()
        {
            if (false == mTriggerGroup.Init())
            {
                return false;
            }
            
            return true;
        }

        public TickResult Tick()
        {
            return mTriggerGroup.Tick();
        }

        public void Reset()
        {
            mTriggerGroup.Reset();
        }

        public void Store(System.IO.BinaryWriter w)
        {
            w.Write((sbyte)Result);

            byte[] data = VariableMgr.Export(mVarMgr); ;
            w.Write(data.Length);
            w.Write(data);

            mTriggerGroup.Store(w);
        }

        public void Restore(System.IO.BinaryReader r)
        {
            Result = (EResult) r.ReadSByte();

            int length = r.ReadInt32();
            byte[] data = r.ReadBytes(length);

            mVarMgr = VariableMgr.Import(data);

            mTriggerGroup.Restore(r);
        }
    }
}