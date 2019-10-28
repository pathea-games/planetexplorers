//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;

//public class CSBehaveMgr : MonoBehaviour 
//{
//    // Instance
//    private static CSBehaveMgr s_Instance = null;
//    public static CSBehaveMgr  Instance  { get { return s_Instance;}}

//    Dictionary<int, List<CSNPCBehave>>	m_BehaveQueues = null;


//    public static void AddQueue (int npc_id)
//    {
//        if (s_Instance == null)
//            return;

//        if (s_Instance.m_BehaveQueues.ContainsKey(npc_id))
//        {
//            Debug.Log("The NPC_ID [" + npc_id.ToString() + "] is already exist a behave queue");
//        }
//        else
//        {
//            s_Instance.m_BehaveQueues.Add(npc_id, new List<CSNPCBehave>());
//            Debug.Log("Add a new Behave Queue of NPC_ID [" + npc_id.ToString() + "] !");
//        }
//    }

//    public static void RemoveQueue (int npc_id)
//    {
//        if (s_Instance == null)
//            return;

//        if (s_Instance.m_BehaveQueues.ContainsKey(npc_id))
//        {
//            Debug.Log("Remove a Behave Queue of NPC_ID [" + npc_id.ToString() + "] !");
//            if (s_Instance.m_BehaveQueues[npc_id].Count != 0
//                && s_Instance.m_BehaveQueues[npc_id][0].IsRunning() )
//                s_Instance.m_BehaveQueues[npc_id][0].Break();

//            s_Instance.m_BehaveQueues[npc_id].Clear();
//            s_Instance.m_BehaveQueues.Remove(npc_id);
//        }
//    }

//    public static void AddBehave (int npc_id, CSNPCBehave behave)
//    {
//        if (s_Instance == null)
//            return;

//        if (s_Instance.m_BehaveQueues.ContainsKey(npc_id))
//        {
//            if (s_Instance.m_BehaveQueues[npc_id].Count != 0)
//            {
//                for (int i = 0; i < s_Instance.m_BehaveQueues[npc_id].Count; ++i)
//                {
//                    int priority = (int)s_Instance.m_BehaveQueues[npc_id][i].Pritority;
//                    if (priority <= (int)behave.Pritority)
//                    {
////						if (s_Instance.m_BehaveQueues[npc_id][i].State == CSNPCBehave.EState.Preparing)
////							s_Instance.m_BehaveQueues[npc_id].Insert(i, behave);
////						else
////						{
////							s_Instance.m_BehaveQueues[npc_id][i].Break();
////							s_Instance.m_BehaveQueues[npc_id][i] = behave;
////						}

//                        if (s_Instance.m_BehaveQueues[npc_id][i].IsRunning())
//                            s_Instance.m_BehaveQueues[npc_id][i].Pause();

//                        s_Instance.m_BehaveQueues[npc_id].Insert(i, behave);

//                        return;
//                    }
//                }

//                s_Instance.m_BehaveQueues[npc_id].Add(behave);
//            }
//            else
//                s_Instance.m_BehaveQueues[npc_id].Add(behave);
//        }

//    }

//    public static void ClearBehaves (int npc_id)
//    {
//        if (s_Instance == null)
//            return;

//        if (s_Instance.m_BehaveQueues.ContainsKey(npc_id))
//        {
//            if (s_Instance.m_BehaveQueues[npc_id].Count != 0
//                && s_Instance.m_BehaveQueues[npc_id][0].IsRunning() )
//                s_Instance.m_BehaveQueues[npc_id][0].Break();

//            s_Instance.m_BehaveQueues[npc_id].Clear();
//        }
//        else
//            Debug.LogWarning("You want to clear the npc behaves is not exist!");
//    }

//    public static CSNPCBehave GetCurBehave(int npc_id)
//    {
//        if (s_Instance == null)
//            return null;

//        if (s_Instance.m_BehaveQueues.ContainsKey(npc_id))
//        {
//            if (s_Instance.m_BehaveQueues[npc_id].Count != 0)
//                return s_Instance.m_BehaveQueues[npc_id][0];
//        }
//        else
//            Debug.LogWarning("You want to get the current npc behaves is not exist!");

//        return null;
//    }

//    public static List<CSNPCBehave> GeBehaves (int npc_id)
//    {
//        if (s_Instance == null)
//            return new List<CSNPCBehave>();

//        if (s_Instance.m_BehaveQueues.ContainsKey(npc_id))
//        {
//            return s_Instance.m_BehaveQueues[npc_id];
//        }
//        else
//            Debug.LogWarning("You want to get the npc behaves is not exist!");

//        return new List<CSNPCBehave>();
//    }

//    #region UNITY_FUNC
//    void Awake ()
//    {
//        if ( s_Instance != null )
//            Debug.LogError("Can not have a second instance of CSBehaveMgr !");
//        s_Instance = this;

//        m_BehaveQueues  = new Dictionary<int, List<CSNPCBehave>>();
//    }

//    void OnDestroy()
//    {

//    }

//    // Use this for initialization
//    void Start () 
//    {
	
//    }
	
//    // Update is called once per frame
//    void Update () 
//    {
//        //--to do: wait npc behave
//        //foreach (KeyValuePair<int, List<CSNPCBehave>> kvp in m_BehaveQueues)
//        //{
//        //    if (kvp.Value.Count != 0)
//        //    {
//        //        if (kvp.Value[0].State == CSNPCBehave.EState.Preparing)
//        //            kvp.Value[0].Start(kvp.Key);
//        //        else if (kvp.Value[0].State == CSNPCBehave.EState.Pause)
//        //            kvp.Value[0].Continue();
//        //        else if (kvp.Value[0].State == CSNPCBehave.EState.Finished)
//        //        {
//        //            kvp.Value[0].Over();
//        //            kvp.Value.RemoveAt(0);
//        //        }
//        //        else
//        //            kvp.Value[0].Update();
//        //    }
//        //}
//    }
//    #endregion	
//}
