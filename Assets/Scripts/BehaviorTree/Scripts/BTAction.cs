using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Behave.Runtime
{
    public class BTAction
    {
        string m_Name;
        IAgent m_Agent;

        internal Dictionary<string, object> m_TreeDataList;

        public string Name { get { return m_Name; } }

        public BTAction()
        {
            m_Name = "Unknown";
            m_TreeDataList = new Dictionary<string, object>();
        }

        public BTAction Clone()
        {
            BTAction action = System.Activator.CreateInstance(GetType()) as BTAction;
            action.m_Name = m_Name;

            foreach (KeyValuePair<string, object> kvp in m_TreeDataList)
            {
                object obj = System.Activator.CreateInstance(kvp.Value.GetType());
                List<FieldInfo> fields = BTResolver.s_Fields[m_Name];
                for (int i = 0; i < fields.Count; i++)
                {
                    fields[i].SetValue(obj, fields[i].GetValue(kvp.Value));
                }

                action.m_TreeDataList.Add(kvp.Key, obj);
            }

            return action;
        }

        public void SetName(string argName)
        {
            m_Name = argName;
        }

        public void SetAgent(IAgent argAgent)
        {
            if (m_Agent == null || m_Agent.Equals(null) || !m_Agent.Equals(argAgent))
            {
                m_Agent = argAgent;

                InitAgent(argAgent);
            }
        }

        public void AddData(string name, object obj)
        {
            if (!m_TreeDataList.ContainsKey(name))
            {
                m_TreeDataList.Add(name, obj);
            }
            else
                m_TreeDataList[name] = obj;
        }

        public Dictionary<string, object> GetDatas()
        {
            return m_TreeDataList;
        }

        internal virtual void InitAgent(IAgent argAgent)
        {
 
        }
    }
}
