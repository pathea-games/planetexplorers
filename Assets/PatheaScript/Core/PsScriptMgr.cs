using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace PatheaScript
{
    public interface Storeable
    {
        void Store(BinaryWriter w);
        void Restore(BinaryReader r);
    }

    public enum TickResult
    {
        Running,
        Finished,
        Max
    }

    public class PsScriptMgr
    {
        List<PsScript> mScriptList = new List<PsScript>(10);
        List<int> mScriptRemoveList = new List<int>(2);
        List<int> mScriptLoadList = new List<int>(2);
        
        VariableMgr mVarMgr;
        
        public Factory Factory
        {
            get;
            private set;
        }

        public EventProxyMgr EventProxyMgr
        {
            get;
            private set;
        }

        public Variable GetVar(string varName)
        {
            return mVarMgr.GetVar(varName);            
        }
        public bool AddVar(string varName, Variable var)
        {
            return mVarMgr.AddVar(varName, var);
        }

        PsScriptMgr(Factory f)
        {
            Factory = f;
            EventProxyMgr = new EventProxyMgr(this);
        }

        void LoadEntry()
        {
            AddToLoadList(Factory.EntryScriptId);
        }

        public PsScript FindScriptById(int id)
        {
            return mScriptList.Find(delegate(PsScript q) {
                if (q.Id == id)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            });
        }

        void RemoveScript()
        {
            if (mScriptRemoveList.Count <= 0)
            {
                return;
            }

            foreach (int scriptId in mScriptRemoveList)
            {
                PsScript q = FindScriptById(scriptId);
                if (null == q)
                {
                    Debug.Log("no script with id:"+ scriptId +" found to terminat");
                    continue;
                }

                Debug.Log(q + " terminated");

                q.Reset();
                mScriptList.Remove(q);
            }

            mScriptRemoveList.Clear();
        }

        void LoadScript()
        {
            if (mScriptLoadList.Count <= 0)
            {
                return;
            }

            int[] loadArray = mScriptLoadList.ToArray();

            foreach (int id in loadArray)
            {
                PsScript q = FindScriptById(id);

                if (null != q)
                {
                    Debug.LogError("Duplicated Script:" + q);
                    continue;
                }

                PsScript script = PsScript.Load(this, id);
                if (null == script)
                {
                    continue;
                }

                if (!script.Init())
                {
                    continue;
                }

                mScriptList.Add(script);
                mScriptLoadList.Remove(id);

                Debug.Log("begin script:" + script.Id);
                //脚本开始事件发生后，可能会loadscript。导致向mScriptLoadList添加新元素而发生异常。
                //所以改为遍历数组。
                InternalEvent.Instance.EmitScriptBegin(script);
            }
        }

        void TickScript()
        {
            foreach (PsScript q in mScriptList)
            {
                if (TickResult.Finished == q.Tick())
                {
                    AddToRemoveList(q.Id);
                }
            }
        }

        public void Tick()
        {
            EventProxyMgr.Tick();

            LoadScript();
            TickScript();
            RemoveScript();
        }

        //can't modify mCurScriptDict in action directly.
        public void AddToRemoveList(int q)
        {
            mScriptRemoveList.Add(q);
        }
        //can't modify mCurScriptDict in action directly.
        public void AddToLoadList(int id)
        {
            mScriptLoadList.Add(id);
        }

        public List<PsScript> CurScript
        {
            get
            {
                return mScriptList;
            }
        }

        public static PsScriptMgr Create(Factory factory)
        {
            factory.Init();

            PsScriptMgr scriptMgr = new PsScriptMgr(factory);
            scriptMgr.mVarMgr = new VariableMgr();

            scriptMgr.LoadEntry();

            return scriptMgr;
        }
        
        public static PsScriptMgr Deserialize(Factory factory, byte[] data)
        {
            if (null == data)
            {
                return null;
            }

            factory.Init();

            PsScriptMgr scriptMgr = new PsScriptMgr(factory);

            MemoryStream sm = new MemoryStream(data, false);

            using (BinaryReader r = new BinaryReader(sm))
            {
                int varDatalength = r.ReadInt32();
                byte[] varData = r.ReadBytes(varDatalength);

                scriptMgr.mVarMgr = VariableMgr.Import(varData);

                int count = r.ReadInt32();

                for (int i = 0; i < count; i++)
                {
                    int id = r.ReadInt32();

                    PsScript script = PsScript.Load(scriptMgr, id);
                    if (null == script)
                    {
                        continue;
                    }

                    if (!script.Init())
                    {
                        continue;
                    }

                    scriptMgr.mScriptList.Add(script);

                    script.Restore(r);
                }

                DeserializeList(scriptMgr.mScriptLoadList, r);
                DeserializeList(scriptMgr.mScriptRemoveList, r);
            }

            return scriptMgr;
        }

        public static byte[] Serialize(PsScriptMgr mgr)
        {
            MemoryStream sm = new MemoryStream(1024 * 10);

            using (BinaryWriter w = new BinaryWriter(sm))
            {
                byte[] data = VariableMgr.Export(mgr.mVarMgr);
                w.Write(data.Length);
                w.Write(data);

                w.Write(mgr.mScriptList.Count);

                foreach (PsScript q in mgr.mScriptList)
                {
                    w.Write(q.Id);

                    q.Store(w);
                }

                SerializeList(mgr.mScriptLoadList, w);
                SerializeList(mgr.mScriptRemoveList, w);
            }

            return sm.ToArray();
        }

        static void SerializeList(List<int> list, BinaryWriter w)
        {
            w.Write(list.Count);

            foreach (int id in list)
            {
                w.Write(id);
            }
        }

        static void DeserializeList(List<int> list, BinaryReader r)
        {
            int count = r.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                list.Add(r.ReadInt32());
            }
        }
    }
}