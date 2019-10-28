using UnityEngine;
using System.Collections.Generic;
using PeCustom;

public class UIMissionTrackWndInterpreter : MonoBehaviour
{
    public UIMissionTrackWnd missionTrackWnd;

    List<int> m_TrackedIds = new List<int>(10);

    bool _initialized = false;
    public void Init ()
    {
        if (!_initialized)
        {
            missionTrackWnd.onSetViewNodeContent += OnViewNodeSetContent;
            missionTrackWnd.onDestroyViewNode += OnDestroyViewNode;

            PeCustomScene.Self.scenario.missionMgr.onRunMission += OnMissionRun;
            PeCustomScene.Self.scenario.missionMgr.onCloseMission += OnCloseMission;
            PeCustomScene.Self.scenario.missionMgr.onMissionTrackChanged += OnMissionTrackChanged;
            PeCustomScene.Self.scenario.missionMgr.onSetMissionGoal += OnSetMissionGoal;
            PeCustomScene.Self.scenario.missionMgr.onUnsetMissionGoal += OnUnsetMissionGoal; 
            PeCustomScene.Self.scenario.missionMgr.OnGoalAchieve += OnGoalAchieve;
            PeCustomScene.Self.scenario.missionMgr.onResumeMission += OnMissionResume;

            _initialized = true;
        }
    }

    public void Close ()
    {
        if (_initialized)
        {
            missionTrackWnd.onSetViewNodeContent -= OnViewNodeSetContent;
            missionTrackWnd.onDestroyViewNode -= OnDestroyViewNode;

            PeCustomScene.Self.scenario.missionMgr.onRunMission -= OnMissionRun;
            PeCustomScene.Self.scenario.missionMgr.onCloseMission -= OnCloseMission;
            PeCustomScene.Self.scenario.missionMgr.onMissionTrackChanged -= OnMissionTrackChanged;
            PeCustomScene.Self.scenario.missionMgr.onSetMissionGoal -= OnSetMissionGoal;
            PeCustomScene.Self.scenario.missionMgr.onUnsetMissionGoal -= OnUnsetMissionGoal;
            PeCustomScene.Self.scenario.missionMgr.OnGoalAchieve -= OnGoalAchieve;
            PeCustomScene.Self.scenario.missionMgr.onResumeMission -= OnMissionResume;

            _initialized = false;
        }
    }

    #region WND_EVENT

    
    void OnViewNodeSetContent (UIMissionGoalNode node)
    {
        MissionProperty mp = PeCustomScene.Self.scenario.missionMgr.GetMissionProperty(m_TrackedIds[node.index]);
        node.SetContent("[C8C800]" + mp.name + "[-]", false, false);

        node.onSetChildNodeContent += OnSetViewNodeChildContent;

        SortedList<int, MissionGoal> goals = PeCustomScene.Self.scenario.missionMgr.GetGoals(m_TrackedIds[node.index]);

        if (goals != null)
        {
            _goals = goals.Values;

            node.UpdateChildNode(_goals.Count, missionTrackWnd.childNodePrefab);
            node.PlayTween(true);
        }
        else
        {
            node.UpdateChildNode(0, missionTrackWnd.childNodePrefab);
        }
    }

    void OnDestroyViewNode (UIMissionGoalNode node)
    {
        node.onSetChildNodeContent -= OnSetViewNodeChildContent;
    }

    // Child node call back
    IList<MissionGoal> _goals = null;
    void OnSetViewNodeChildContent(int index,  GameObject child)
    {
        UIMissionGoalNode node = child.GetComponent<UIMissionGoalNode>();

        MissionGoal mg = _goals[index];

        string txt = "";
        if (mg is MissionGoal_Bool)
        {
            MissionGoal_Bool mb = mg as MissionGoal_Bool;
            txt = mb.text;
            node.value2 = mg.id;
        }
        else if (mg is MissionGoal_Item)
        {
            MissionGoal_Item mi = mg as MissionGoal_Item;
            txt = mi.text + " "+  mi.current.ToString() + "/" + mi.target.ToString();
            node.value0 = mi.current;
            node.value1 = mi.target;
            node.value2 = mg.id;
        }
        else if (mg is MissionGoal_Kill)
        {
            MissionGoal_Kill mi = mg as MissionGoal_Kill;
            txt = mi.text + " " + mi.current.ToString() + "/" + mi.target.ToString();
            node.value0 = mi.current;
            node.value1 = mi.target;
            node.value2 = mg.id;
        }

        node.SetContent(txt, false, false);

    }
    #endregion

    void OnMissionRun (int mission_id)
    {
        MissionProperty mp = PeCustomScene.Self.scenario.missionMgr.GetMissionProperty(mission_id);
        bool tracked = PeCustomScene.Self.scenario.missionMgr.MissionIsTracked(mission_id);
        

        if (mp != null && tracked)
        {
            if (mp.type != MissionProperty.EType.Hidden)
            {
                m_TrackedIds.Add(mission_id);
                missionTrackWnd.AddViewNode();
            }
        }
    }


    void OnCloseMission (int mission_id, EMissionResult r)
    {
        int index = m_TrackedIds.FindIndex(item0 => item0 == mission_id);
        if (index != -1)
        {
           missionTrackWnd.RemoveViewNode(index);
            m_TrackedIds.RemoveAt(index);
        }
    }
    void OnMissionResume(int mission_id)
    {
        MissionProperty mp = PeCustomScene.Self.scenario.missionMgr.GetMissionProperty(mission_id);
        bool tracked = PeCustomScene.Self.scenario.missionMgr.MissionIsTracked(mission_id);


        if (mp != null && tracked)
        {
            if (mp.type != MissionProperty.EType.Hidden && !m_TrackedIds.Contains(mission_id))
            {
                m_TrackedIds.Add(mission_id);
                missionTrackWnd.AddViewNode();
            }
        }
    }

    void OnMissionTrackChanged (int mission_id, bool is_tracked)
    {
        int index = m_TrackedIds.FindIndex(item0 => item0 == mission_id);
        if (is_tracked)
        {
            if (index == -1)
            {
                m_TrackedIds.Add(mission_id);
                missionTrackWnd.AddViewNode();
            }
            else
            {
                Debug.LogWarning("The mission [" + mission_id.ToString() +"] is already tracked." );
            }
        }
        else
        {
            if (index != -1)
            {
                missionTrackWnd.RemoveViewNode(index);
                m_TrackedIds.RemoveAt(index);
            }
        }
    }

    void OnGoalAchieve(int goal_id, int mission_id)
    {
        int index = m_TrackedIds.FindIndex(item0 => item0 == mission_id);
        if (index != -1)
        {
            SortedList<int, MissionGoal> goals = PeCustomScene.Self.scenario.missionMgr.GetGoals(m_TrackedIds[index]);
            if (goals != null)
            {
                int goal_index = goals.IndexOfKey(goal_id);
                UIMissionGoalNode child_node = missionTrackWnd.GetNode(index).childNode[goal_index].GetComponent<UIMissionGoalNode>();

                if (child_node != null)
                {
                    child_node.titleColor = new Color(0.65f, 0.65f, 0.65f);
                }
            }
        }
    }

    void OnSetMissionGoal (int goal_id, int mission_id)
    {
        int index = m_TrackedIds.FindIndex(item0 => item0 == mission_id);
        if (index != -1)
        {
            UIMissionGoalNode node = missionTrackWnd.GetNode(index);

            SortedList<int, MissionGoal> goals = PeCustomScene.Self.scenario.missionMgr.GetGoals(m_TrackedIds[node.index]);
            if (goals != null)
            {
                _goals = goals.Values;
                node.UpdateChildNode(goals.Count, missionTrackWnd.childNodePrefab);
                node.PlayTween(true);
            }
            else
            {
                node.UpdateChildNode(0, missionTrackWnd.childNodePrefab);
            }

            missionTrackWnd.repositionNow = true;
        }
    }

    void OnUnsetMissionGoal (int goal_id, int mission_id)
    {
        int index = m_TrackedIds.FindIndex(item0 => item0 == mission_id);
        if (index != -1)
        {
            SortedList<int, MissionGoal> goals = PeCustomScene.Self.scenario.missionMgr.GetGoals(m_TrackedIds[index]);
            if (goals != null)
            {
                int goal_index = goals.IndexOfKey(goal_id);
                if (goal_index != -1)
                {
                    UIMissionGoalNode node = missionTrackWnd.GetNode(index);
                    node.RemoveChildeNode(goal_index);
                }
            }
        }
    }

    

    #region UNITY_INNER_FUNC

    void Update ()
    {
        if (!_initialized)
            return;

        if (missionTrackWnd.isShow)
        {
            for (int i = 0; i < missionTrackWnd.viewNodes.Count; i++)
            {
                UIMissionGoalNode vNode = missionTrackWnd.viewNodes[i];
                int mission_id = m_TrackedIds[i];
                for (int j = 0; j < vNode.childNode.Count; j++)
                {
                    UIMissionGoalNode child_node = vNode.childNode[j].GetComponent<UIMissionGoalNode>();
                    MissionGoal mg = PeCustomScene.Self.scenario.missionMgr.GetGoal(child_node.value2, mission_id);
                    if (mg != null)
                    {
                        if (mg is MissionGoal_Item)
                        {
                            MissionGoal_Item mgi = mg as MissionGoal_Item;
                            int current = mgi.current;
                            int target = mgi.target;

                            if (current != child_node.value0 || target != child_node.value1)
                            {
                                if (mgi.achieved)
                                    child_node.titleColor = new Color(0.65f, 0.65f, 0.65f);
                                else
                                    child_node.titleColor = Color.white;
                                string text = mgi.text + " " + mgi.current.ToString() + "/" + mgi.target.ToString();

                                child_node.SetContent(text, false, false);

                                child_node.value0 = current;
                                child_node.value1 = target;
                            }
                        }
                        else if (mg is MissionGoal_Kill)
                        {
                            MissionGoal_Kill mgi = mg as MissionGoal_Kill;
                            int current = mgi.current;
                            int target = mgi.target;
                            if (current != child_node.value0 || target != child_node.value1)
                            {
                                if (mgi.achieved)
                                    child_node.titleColor = new Color(0.75f, 0.75f, 0.75f);
                                else
                                    child_node.titleColor = Color.white;
                                string text = mgi.text + " " + mgi.current.ToString() + "/" + mgi.target.ToString();

                                child_node.SetContent(text, false, false);

                                child_node.value0 = current;
                                child_node.value1 = target;
                            }
                        }
                    }
                }
            }
        }
    }

    #endregion
}
