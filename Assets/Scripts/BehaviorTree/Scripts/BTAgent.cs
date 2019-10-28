//#define Profiler_Debug
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Behave.Runtime;
using Tree = Behave.Runtime.Tree;

namespace Behave.Runtime
{
    public class BehaveAgent : IAgent
    {
        static BehaveAgent _Agent;
        public static BehaveAgent Agent
        {
            get
            {
                if(_Agent == null)
                    _Agent = new BehaveAgent();

                return _Agent;
            }
        }
        public void Reset(Tree sender)
        {
        }

        public int SelectTopPriority(Tree sender, params int[] IDs)
        {
            return 0;
        }

        public BehaveResult Tick(Tree sender)
        {
            return BehaveResult.Success;
        }
    }

    public class BTAgent
    {
        string m_BtPath;
        string m_LibraryName;
        string m_TreeName;

        IAgent              m_Agent;
        Tree                m_Tree;
        BTCoroutine         m_Runner;
        bool                m_Pause;
        List<BTAction>      m_Actions;
		BehaveResult        m_LastTickResult;
#if Profiler_Debug
        string              m_ActiveAction;
#endif

        public string btPath { get { return m_BtPath; } }

        public BTAgent(string btPath, string libraryName, string treeName)
        {
            m_BtPath = btPath;
            m_LibraryName = libraryName;
            m_TreeName = treeName;

            m_Pause = false;
            m_Runner = null;
            m_Agent = null;

            m_Actions = new List<BTAction>();
            m_Tree = Reflecter.Instance.Instantiate(libraryName, treeName);
        }

        public BTAgent Clone()
        {
            BTAgent agent = new BTAgent(m_BtPath, m_LibraryName, m_TreeName);

            for (int i = 0; i < m_Actions.Count; i++)
            {
                agent.RegisterAction(m_Actions[i].Clone());
            }

            return agent;
        }

        void SetTreeForward(Tree tree, BTAction action)
        {
            try
            {
                int index = (int)System.Enum.Parse(tree.LibraryActions, action.Name);

                MethodInfo start = action.GetType().GetMethod("Start",
                                                                BindingFlags.NonPublic | BindingFlags.Instance);

                MethodInfo init = action.GetType().GetMethod("Init",
                                                                BindingFlags.NonPublic | BindingFlags.Instance,
                                                                null,
                                                                new Type[] { typeof(Tree) },
                                                                null);

                MethodInfo Tick = action.GetType().GetMethod("Tick",
                                                                BindingFlags.NonPublic | BindingFlags.Instance,
                                                                null,
                                                                new Type[] { typeof(Tree) },
                                                                null);

                MethodInfo Reset = action.GetType().GetMethod("Reset",
                                                                BindingFlags.NonPublic | BindingFlags.Instance,
                                                                null,
                                                                new Type[] { typeof(Tree) },
                                                                null);

                if (start != null)
                    start.Invoke(action, null);

                if (init != null)
                    tree.SetInitForward(index, Delegate.CreateDelegate(typeof(TickForward), action, init) as TickForward);

                if (Tick != null)
                    tree.SetTickForward(index, Delegate.CreateDelegate(typeof(TickForward), action, Tick) as TickForward);

                if (Reset != null)
                    tree.SetResetForward(index, Delegate.CreateDelegate(typeof(ResetForward), action, Reset) as ResetForward);
            }
            catch (Exception)
            {
                //Debug.LogWarning(e);
            }
        }

        bool CanRun()
        {
            if (m_Pause || m_Agent.Equals(null))
                return false;

            IBehave behave = m_Agent as IBehave;
            if (!behave.Equals(null) && !behave.BehaveActive)
                return false;

            return true;
        }

        IEnumerator Runner()
        {
            WaitForSeconds wait = new WaitForSeconds(1 / m_Tree.Frequency);

            while (m_Tree != null)
            {
                if (CanRun())
                {
#if Profiler_Debug
                    m_ActiveAction = System.Enum.ToObject(m_Tree.LibraryActions, m_Tree.LastTickedAction).ToString();
#endif

#if Profiler_Debug
                    Profiler.BeginSample(m_ActiveAction);
#endif
                    try
                    {
                        m_LastTickResult = m_Tree.Tick(m_Agent, null);
                    }
                    catch(Exception e)
                    {
                        Debug.LogWarning("Tree Tick Error : " + e);
                    }

#if Profiler_Debug
                    Profiler.EndSample();
#endif

					if (m_LastTickResult != BehaveResult.Running) m_Tree.Reset();
                }

                yield return wait;
            }
        }

        public List<BTAction> GetActions()
        {
            return m_Actions;
        }

        public BTAction GetAction(string actionName)
        {
            return m_Actions.Find(ret => ret.Name == actionName);
        }

        public bool Contains(string actionName)
        {
            return m_Actions.Find(ret => ret.Name == actionName) != null;
        }

        public void RegisterAction(BTAction action)
        {
            if (action == null)
                return;

            SetTreeForward(m_Tree, action);

            if(m_Agent != null && !m_Agent.Equals(null))
                action.SetAgent(m_Agent);

            m_Actions.Add(action);
        }

        public void SetAgent(IAgent agent)
        {
            if(m_Agent == null || m_Agent.Equals(null) || !m_Agent.Equals(agent))
            {
                m_Agent = agent;

                for (int i = 0; i < m_Actions.Count; i++)
                    m_Actions[i].SetAgent(m_Agent);

                if(m_Runner == null)
                    m_Runner = new BTCoroutine(BTLauncher.Instance, Runner());
            }
        }

        public void Tick()
        {
            if (m_Tree != null)
                m_Tree.Tick(BehaveAgent.Agent, null);
        }

        public void Start()
        {
            if(m_Runner != null)
                m_Runner.Start();
        }

        public void Pause(bool isPause)
        {
            if (m_Runner != null)
                m_Pause = isPause;
        }

        public bool IsStart()
        {
            if(m_Runner != null)
                return m_Runner.IsStart;

            return false;
        }

        public void Reset()
        {
            if (m_Runner != null)
            {
                m_Runner.Stop();
                m_Runner.Start();
            }
        }

        public void Stop()
        {
            if(m_Tree != null)
				m_Tree.Reset(m_Agent, m_Tree, null);

            if (m_Runner != null)
                m_Runner.Stop();
        }

        public void Destroy()
        {
            Stop();

            m_Tree = null;
            m_Runner = null;
            m_Agent = null;
        }
    }
}

