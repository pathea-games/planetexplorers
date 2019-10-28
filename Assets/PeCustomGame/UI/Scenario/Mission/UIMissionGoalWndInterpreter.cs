using UnityEngine;
using System.Collections.Generic;
using PeCustom;
using Pathea;

public class UIMissionGoalWndInterpreter : MonoBehaviour
{
    public UIMissionGoalWnd missionWnd;

    // Mission id
    List<int> m_MainStoryIds = new List<int>(5);
    List<int> m_SideQuestIdes = new List<int>(5);
    int _currentMissionId = -1;

    bool _initialized = false;
    public void Init ()
    {
        if(!_initialized)
        {
            missionWnd.onSetNodeContent += OnSetNodeContent;
            missionWnd.onMissionNodeClick += OnMissionNodeClick;
            missionWnd.onSetGoalItemContent += OnSetGoalItemContent;
            missionWnd.onSetRewardItemContent += OnSetRewardItemContent;
            missionWnd.onMissionDeleteClick += OnMissionDeleteClick;
            missionWnd.onTrackBoxSelected += OnTrackBoxSelected;

            PeCustomScene.Self.scenario.missionMgr.onRunMission += OnRunMission;
            PeCustomScene.Self.scenario.missionMgr.onCloseMission += OnCloseMission;
            PeCustomScene.Self.scenario.missionMgr.onSetMissionGoal += OnSetMissionGoal;
            PeCustomScene.Self.scenario.missionMgr.onMissionTrackChanged += OnMissionTrackChanged;
            PeCustomScene.Self.scenario.missionMgr.onResumeMission += OnMissionResume;

            RefreshNodeSelected();

            _initialized = true;
        }
    }

    public void Close ()
    {
        if (_initialized)
        {
            missionWnd.onSetNodeContent -= OnSetNodeContent;
            missionWnd.onMissionNodeClick -= OnMissionNodeClick;
            missionWnd.onSetGoalItemContent -= OnSetGoalItemContent;
            missionWnd.onSetRewardItemContent -= OnSetRewardItemContent;
            missionWnd.onMissionDeleteClick -= OnMissionDeleteClick;
            missionWnd.onTrackBoxSelected -= OnTrackBoxSelected;

            PeCustomScene.Self.scenario.missionMgr.onRunMission -= OnRunMission;
            PeCustomScene.Self.scenario.missionMgr.onCloseMission -= OnCloseMission;
            PeCustomScene.Self.scenario.missionMgr.onSetMissionGoal -= OnSetMissionGoal;
            PeCustomScene.Self.scenario.missionMgr.onMissionTrackChanged -= OnMissionTrackChanged;
            PeCustomScene.Self.scenario.missionMgr.onResumeMission -= OnMissionResume;

            _initialized = false;
        }
    }

    public void RefreshNodeSelected()
    {
        if (missionWnd.selectedNode == null)
        { 
            if (missionWnd.IsShowAboutUI)
                missionWnd.IsShowAboutUI = false;

            if (missionWnd.IsShowGoalUI)
                missionWnd.IsShowGoalUI = false;

            if (missionWnd.IsShowRewardUI)
                missionWnd.IsShowRewardUI = false;
        }
        else
        {
            if (!missionWnd.IsShowAboutUI)
                missionWnd.IsShowAboutUI = true;

            if (!missionWnd.IsShowGoalUI)
                missionWnd.IsShowGoalUI = true;

            if (!missionWnd.IsShowRewardUI)
                missionWnd.IsShowRewardUI = true;
        }
    }

    #region MISSION_CALL_BACK
    void OnRunMission (int id)
    {
        MissionProperty mp = PeCustomScene.Self.scenario.missionMgr.GetMissionProperty(id);
        if (mp != null)
        {
            if (mp.type == MissionProperty.EType.MainStory)
            {
                m_MainStoryIds.Add(id);
                missionWnd.AddMainStoryNode();
            }
            else if (mp.type == MissionProperty.EType.SideQuest)
            {
                m_SideQuestIdes.Add(id);
                missionWnd.AddSideQuestNode();
            }
        }
    }

    void OnCloseMission (int id, EMissionResult result)
    {
        int index = m_MainStoryIds.FindIndex(item0 => item0 == id);
        if (index != -1)
        {
            missionWnd.RemoveMainStoryNode(index);
            m_MainStoryIds.RemoveAt(index);
        }
        else
        {
            index = m_SideQuestIdes.FindIndex(item0 => item0 == id);
            if (index != -1)
            {
                missionWnd.RemoveSideQuestNode(index);
                m_SideQuestIdes.RemoveAt(index);
            }
        }

        RefreshNodeSelected();

        // Clear Goal
        if (missionWnd.selectedNode == null)
        {
            DetachGoalEvent();
            _currentMissionId = -1;
        }
    }

    void OnMissionResume (int id)
    {
        MissionProperty mp = PeCustomScene.Self.scenario.missionMgr.GetMissionProperty(id);
        if (mp != null)
        {
            if (mp.type == MissionProperty.EType.MainStory)
            {
                if (!m_MainStoryIds.Contains(id))
                {
                    m_MainStoryIds.Add(id);
                    missionWnd.AddMainStoryNode();
                }
            }
            else if (mp.type == MissionProperty.EType.SideQuest)
            {
                if (!m_MainStoryIds.Contains(id))
                {
                    m_SideQuestIdes.Add(id);
                    missionWnd.AddSideQuestNode();
                }
            }
        }
    }

    void OnMissionTrackChanged (int mission_id, bool is_tracked)
    {
        int index = m_MainStoryIds.FindIndex(item0 => item0 == mission_id);
        if (index != -1)
        {
            UIMissionGoalNode node = missionWnd.mainStoryNodes[index];
            node.isTracked = is_tracked;
        }
        else
        {
            index = m_SideQuestIdes.FindIndex(item0 => item0 == mission_id);

            if (index != -1)
            {
                UIMissionGoalNode node = missionWnd.sidQuestNodes[index];
                node.isTracked = is_tracked;
            }
        }
    }

    void OnSetMissionGoal (int goal_id, int mission_id)
    {
        if (mission_id != _currentMissionId)
            return;

        DetachGoalEvent();
        _currentGoals = PeCustomScene.Self.scenario.missionMgr.GetGoals(mission_id);
        if (_currentGoals != null)
            _goals = _currentGoals.Values;
        AttachGoalEvent();

        missionWnd.UpdateGoalItem(_currentGoals.Count);
    }

    #endregion

    #region WND_EVENT

    void OnSetNodeContent (int type, UIMissionGoalNode node)
    {
        // Main Story
        MissionProperty mp = null;
        bool can_track = false;
        if (type == 0)
        {
            mp = PeCustomScene.Self.scenario.missionMgr.GetMissionProperty(m_MainStoryIds[node.index]);
            can_track = PeCustomScene.Self.scenario.missionMgr.MissionIsTracked(m_MainStoryIds[node.index]);
        }
        else if (type == 1)
        {
            mp = PeCustomScene.Self.scenario.missionMgr.GetMissionProperty(m_SideQuestIdes[node.index]);
            can_track = PeCustomScene.Self.scenario.missionMgr.MissionIsTracked(m_SideQuestIdes[node.index]);
        }

        if (mp != null)
        {
            node.SetContent(mp.name, mp.canAbort, true);
            node.IsSelected = false;
            node.isTracked = can_track;
        }

    }

    void OnMissionNodeClick (int type, UIMissionGoalNode node)
    {
        RefreshNodeSelected ();

        DetachGoalEvent();

        MissionProperty mp = null;
        if (type == 0)
        {
            mp = PeCustomScene.Self.scenario.missionMgr.GetMissionProperty(m_MainStoryIds[node.index]);
            _currentGoals = PeCustomScene.Self.scenario.missionMgr.GetGoals(m_MainStoryIds[node.index]);
            if (_currentGoals != null)
                _goals = _currentGoals.Values;

            _currentMissionId = m_MainStoryIds[node.index];
        }
        else if (type == 1)
        {
            mp = PeCustomScene.Self.scenario.missionMgr.GetMissionProperty(m_SideQuestIdes[node.index]);
            _currentGoals = PeCustomScene.Self.scenario.missionMgr.GetGoals(m_SideQuestIdes[node.index]);
            if (_currentGoals != null)
                _goals = _currentGoals.Values;

            _currentMissionId = m_SideQuestIdes[node.index];
        }
        else
        {
            _currentMissionId = -1;
            _currentGoals = null;
            _goals = null;

            _rewardItemIds = null;
            _rewardItemCount = null;
        }

        if (mp != null)
        {
            // Begin Npc
            if (CustomGameData.Mgr.Instance != null
                && CustomGameData.Mgr.Instance.curGameData.WorldIndex == mp.beginNpcWorldIndex)
            {
                SpawnPoint sp = PeCustomScene.Self.spawnData.GetSpawnPoint(mp.beginNpcId);
                if (sp as NPCSpawnPoint != null)
                {
                    NpcProtoDb.Item proto = NpcProtoDb.Get(sp.Prototype);
                    missionWnd.SetMissionAbout(sp.Name, proto.iconBig, mp.objective);
                }
                else if (sp as MonsterSpawnPoint != null)
                {
                    MonsterProtoDb.Item proto_mst = MonsterProtoDb.Get(sp.Prototype);
                    missionWnd.SetMissionAbout(sp.Name, proto_mst.icon, mp.objective);
                }
                else
                {
                    missionWnd.SetMissionAbout("None", "npc_big_Unknown", mp.objective);
                    //Debug.LogWarning("The Npc Id [" + mp.beginNpcId.ToString() + "] spawn point is not exist");
                }
            }
			else
			{
				missionWnd.SetMissionAbout("None", "npc_big_Unknown", mp.objective);
			}

            // End Npc
            if (CustomGameData.Mgr.Instance != null 
                && CustomGameData.Mgr.Instance.curGameData.WorldIndex == mp.endNpcWorldIndex)
            {
                SpawnPoint sp = PeCustomScene.Self.spawnData.GetSpawnPoint(mp.endNpcId);
                if (sp as NPCSpawnPoint != null)
                {
                    NpcProtoDb.Item proto = NpcProtoDb.Get(sp.Prototype);
                    missionWnd.SetRewardInfo(sp.Name, proto.iconBig);
                }
                else if (sp as MonsterSpawnPoint != null)
                {
                    MonsterProtoDb.Item proto_mst = MonsterProtoDb.Get(sp.Prototype);
                    missionWnd.SetRewardInfo(sp.Name, proto_mst.icon);
                }
                else
                {
                    missionWnd.SetRewardInfo("None", "npc_big_Unknown");
                    Debug.LogWarning("The Npc Id [" + mp.endNpcId.ToString() + "] spawn point is not exist");
                }
            }
            else
            {
                missionWnd.SetRewardInfo("None", "npc_big_Unknown");
            }

            // Reward
            if (mp.rewardDesc != null)
            {
                missionWnd.SetRewardDesc(mp.rewardDesc);
            }
            else
            {
                _rewardItemIds = mp.rewardItemIds;
                _rewardItemCount = mp.rewardItemCount;
                missionWnd.UpdateRewardItem(mp.rewardItemIds.Count);
            }
        }

        // goal
        if (_currentGoals != null)
        {
            AttachGoalEvent();
            missionWnd.UpdateGoalItem(_currentGoals.Count);
        }



    }

    void OnSetGoalItemContent (UIMissionGoalItem item)
    {
        MissionGoal mg = _goals[item.index];

        if (!mg.achieved)
            item.textColor = Color.white;
        else
            item.textColor = Color.green;

        if (mg as MissionGoal_Bool != null)
        {
            MissionGoal_Bool mgb = mg as MissionGoal_Bool;
            item.SetBoolContent(mgb.text, mgb.achieved);
        }
        else if (mg as MissionGoal_Item != null)
        {
            MissionGoal_Item mgi = mg as MissionGoal_Item;
            string text = mgi.text + " " + mgi.current.ToString() + "/" + mgi.target.ToString();

            item.value0 = mgi.current;
            item.value1 = mgi.target;

            if (mgi.item.isSpecificPrototype)
            {
                string[] sprites = ItemAsset.ItemProto.GetIconName(mgi.item.Id);
                if (sprites != null)
                    item.SetItemContent(text, sprites[0]);
                else
                    item.SetItemContent(text);
            }
            else
                item.SetItemContent(text);
        }
        else if (mg as MissionGoal_Kill != null)
        {
            MissionGoal_Kill mgk = mg as MissionGoal_Kill;

            string text = mgk.text + " " + mgk.current.ToString() + "/" + mgk.target.ToString();
            MissionGoal_Kill mgi = mg as MissionGoal_Kill;

            item.value0 = mgi.current;
            item.value1 = mgi.target;

            if (item != null)
            {
                if (mgi.monster.isSpecificEntity)
                {
                    SpawnPoint sp = PeCustomScene.Self.spawnData.GetSpawnPoint(mgi.id);
                    if (sp as MonsterSpawnPoint != null)
                    {
                        MonsterProtoDb.Item proto = MonsterProtoDb.Get(sp.ID);
                        item.SetItemContent(text, proto.icon);
                    }
                    else if (sp as NPCSpawnPoint != null)
                    {
                        NpcProtoDb.Item proto = NpcProtoDb.Get(sp.ID);
                        item.SetItemContent(text, proto.icon);
                    }
                    else
                    {
                        item.SetItemContent(text);
                    }
                }
                else if (mgi.monster.isSpecificPrototype)
                {
                    MonsterProtoDb.Item proto = MonsterProtoDb.Get(mgi.monster.Id);
                    if (proto != null)
                    {
                        item.SetItemContent(text, proto.icon);
                    }
                    else
                        item.SetItemContent(text);
                }
                else
                {
                    item.SetItemContent(text);
                }

            }
            else
            {
                item.SetItemContent(text);
            }
        }
    }

    void OnMissionGoalAchieved(int goal_id, int mission_id)
    {
        int index = _currentGoals.IndexOfKey(goal_id);
        UIMissionGoalItem item = missionWnd.GetGoalItem(index);
        if (item != null)
        {
            MissionGoal mg = _currentGoals[goal_id];
            if (!mg.achieved)
                item.textColor = Color.white;
            else
                item.textColor = Color.green;
        }
    }

    void OnSetRewardItemContent (Grid_N grid)
    {
        int item_id = _rewardItemIds[grid.ItemIndex];
        int item_count = _rewardItemCount[grid.ItemIndex];

        //ItemAsset.ItemProto item_proto = ItemAsset.ItemProto.GetItemData(item_id);
        ItemAsset.ItemSample sample = new ItemAsset.ItemSample(item_id, item_count);
        grid.SetItem(sample);
    }

    int _deleteMissionId;
    void OnMissionDeleteClick (UIMissionGoalNode node)
    {
        //MessageBox_N.ShowYNBox("Sure to")

        _deleteMissionId = -1;
        // Main Stroy
        if (missionWnd.MissionType == 0)
        {
            _deleteMissionId = m_MainStoryIds[node.index];
        }
        else if (missionWnd.MissionType == 1)
        {
            _deleteMissionId = m_SideQuestIdes[node.index];
        }

        if (_deleteMissionId != -1)
        {
            MissionProperty mp = PeCustomScene.Self.scenario.missionMgr.GetMissionProperty(_deleteMissionId);
            if (mp != null)
            {
                if (mp.canAbort)
                    MessageBox_N.ShowYNBox(PELocalization.GetString(8000066), DeleteMissionOk);
                else
                {
                    //lz-2016.10.31 The mission can't be aborted.
                    new PeTipMsg(PELocalization.GetString(8000850), PeTipMsg.EMsgLevel.Warning);
                }
            }
            else
            {
                Debug.LogError("Get the Deleted Mission property is error");
            }
        }

       
    }

    void DeleteMissionOk()
    {
        PeCustomScene.Self.scenario.missionMgr.CloseMission(_deleteMissionId, EMissionResult.Aborted);
    }

    void OnTrackBoxSelected (bool active, int type, UIMissionGoalNode node)
    {
        int mission_id = -1;
        if (type == 0)
        {
            mission_id = m_MainStoryIds[node.index];
        }
        else if (type == 1)
        {
            mission_id = m_SideQuestIdes[node.index];
        }

        if (mission_id != -1)
        {
            PeCustomScene.Self.scenario.missionMgr.SetMissionIsTracked(mission_id, active);
            Debug.Log("Do mission [" + mission_id.ToString() + "] track");
        }
    }
    #endregion

    void UpdateGoal ()
    {

    }

    #region UNITY_FUNC

    void Update ()
    {
        if (!_initialized)
            return;

        if (missionWnd.isShow)
        {


            if (_currentGoals != null)
            {
                for (int i = 0; i < _goals.Count; i++)
                {
                    MissionGoal mg = _goals[i];


                    if (mg as MissionGoal_Item != null)
                    {
                        MissionGoal_Item mgi = mg as MissionGoal_Item;
                        UIMissionGoalItem item = missionWnd.GetGoalItem(i);

                        int current = mgi.current;
                        int target = mgi.target;
                        if (item != null && (current != item.value0 || target != item.value1))
                        {
                            string text = mgi.text + " " + mgi.current.ToString() + "/" + mgi.target.ToString();

                            item.itemText = text;

                            item.value0 = current;
                            item.value1 = target;
                        }

                    }
                    else if (mg as MissionGoal_Kill != null)
                    {
                        MissionGoal_Kill mgi = mg as MissionGoal_Kill;
                        UIMissionGoalItem item = missionWnd.GetGoalItem(i);

                        int current = mgi.current;
                        int target = mgi.target;
                        if (item != null && (current != item.value0 || target != item.value1))
                        {
                            string text = mgi.text + " " + mgi.current.ToString() + "/" + mgi.target.ToString();

                            item.itemText = text;

                            item.value0 = current;
                            item.value1 = target;

                        }
                    }
                }
            }
           
        }
    }

    #endregion

    SortedList<int, MissionGoal> _currentGoals;
    IList<MissionGoal> _goals;

    void AttachGoalEvent ()
    {
        if (_currentGoals == null)
            return;

        for (int i = 0; i < _currentGoals.Count; i++)
        {
            _currentGoals.Values[i].onAchieve += OnMissionGoalAchieved;
        }
    }

    void DetachGoalEvent()
    {
        if (_currentGoals == null)
            return;

        for (int i = 0; i < _currentGoals.Count; i++)
        {
            _currentGoals.Values[i].onAchieve -= OnMissionGoalAchieved;
        }
    }


    List<int> _rewardItemIds;
    List<int> _rewardItemCount;
}
