using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Behave.Runtime;

namespace Behave.Runtime
{
    public class BTLauncher : Singleton<BTLauncher>
    {
        const int s_MaxCount = 10000000;

        int m_Count;

        Dictionary<int, BTAgent> m_Agents = new Dictionary<int, BTAgent>();

        public int Instantiate(string btPath, IAgent agent, bool isLaunch = true)
        {
            if (!string.IsNullOrEmpty(btPath))
            {
                BTAgent btAgent = BTResolver.Instantiate(btPath, agent);

                if (btAgent != null)
                {
                    if (isLaunch) btAgent.Start();

                    int id = ++m_Count;

                    m_Agents.Add(id, btAgent);

                    return id;
                }
            }

            return -1;
        }

        public BTAgent GetAgent(int id)
        {
            if (m_Agents.ContainsKey(id))
                return m_Agents[id];

            return null;
        }

        public bool Excute(int id)
        {
            if (m_Agents.ContainsKey(id))
            {
                m_Agents[id].Start();
                return true;
            }

            return false;
        }

        public bool IsStart(int id)
        {
            if (m_Agents.ContainsKey(id))
            {
                return m_Agents[id].IsStart();
            }

            return false;
        }

        public bool Pause(int id, bool value)
        {
            if (m_Agents.ContainsKey(id))
            {
                m_Agents[id].Pause(value);
                return true;
            }

            return false;
        }

        public void PauseAll(bool value)
        {
            foreach (KeyValuePair<int, BTAgent> kvp in m_Agents)
            {
                kvp.Value.Pause(value);
            }
        }

        public bool Reset(int id)
        {
            if (m_Agents.ContainsKey(id))
            {
                m_Agents[id].Reset();
                return true;
            }

            return false;
        } 

        public bool Stop(int id)
        {
            if (m_Agents.ContainsKey(id))
            {
                m_Agents[id].Stop();
                return true;
            }

            return false;
        }

        public void Remove(int id)
        {
            if (m_Agents.ContainsKey(id))
            {
                m_Agents[id].Destroy();
                //BTResolver.Push(m_Agents[id]);
                m_Agents.Remove(id);
            }
        }

        public void Clear()
        {
            foreach (KeyValuePair<int, BTAgent> kvp in m_Agents)
            {
                kvp.Value.Destroy();
                //BTResolver.Push(kvp.Value);
            }

            m_Agents.Clear();
        }

        void OnLevelWasLoaded(int level)
        {
            m_Count = s_MaxCount;
            Clear();
        }
    }
}
