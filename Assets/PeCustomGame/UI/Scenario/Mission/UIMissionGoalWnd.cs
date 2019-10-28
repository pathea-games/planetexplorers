using UnityEngine;
using System.Collections.Generic;
using System;

public class UIMissionGoalWnd : UIBaseWnd
{
    // List
    [SerializeField] GameObject mainStroyRoot;
    [SerializeField] UITable mainStoryTb;

    [SerializeField] GameObject sideQuestRoot;
    [SerializeField] UITable sideQuestTb;

    [SerializeField] UIMissionGoalNode missionNodePrefab;

    // About
    [SerializeField] GameObject aboutRoot;
    [SerializeField] UILabel aboutNpcNameLb;
    [SerializeField] UILabel aboutDescLb;
    [SerializeField] UISprite aboutNpcIcon;

    // Goal
    [SerializeField] GameObject goalRoot;
    [SerializeField] UITable goalsTb;
    [SerializeField] UIMissionGoalItem goalItemPrefab;

    // Reward
    [SerializeField] GameObject rewardRoot;
    [SerializeField] GameObject rewardNpcInfoRoot;
    [SerializeField] UILabel rewardNpcNameLb;
    [SerializeField] UISprite rewardNpcIcon;
    [SerializeField] UIGrid  rewardItemGrid;
    [SerializeField] UILabel rewardDesc;

    [SerializeField] Grid_N gridPrefab;

    List<UIMissionGoalNode> m_MainStoryNodes = new List<UIMissionGoalNode>(10);
    List<UIMissionGoalNode> m_SideQuestNodes = new List<UIMissionGoalNode>(10);

    List<UIMissionGoalItem> m_MissionGoalItems = new List<UIMissionGoalItem>(10);
    List<Grid_N> m_RewardGrids = new List<Grid_N>(10);

    public UIMissionGoalNode selectedNode { get; private set;}

    public bool IsShowAboutUI { get { return aboutRoot.activeSelf; } set { aboutRoot.SetActive(value); } }
    public bool IsShowGoalUI { get { return goalRoot.activeSelf; } set { goalRoot.SetActive(value); } }
    public bool IsShowRewardUI { get { return rewardRoot.activeSelf; } set { rewardRoot.SetActive(value); } }

    public List<UIMissionGoalNode> mainStoryNodes { get { return m_MainStoryNodes; } }
    public List<UIMissionGoalNode> sidQuestNodes { get { return m_SideQuestNodes; } }

    public int MissionType
    {
        get
        {
            if (mainStroyRoot.activeSelf)
                return 0;
            else if (sideQuestRoot.activeSelf)
                return 1;
            return -1;
        }
    }

    /// <summary>>Param (int) : 0 mainstory, 1 sidequest, others are invalid; /// </summary>
    public Action<int, UIMissionGoalNode> onSetNodeContent;
    /// <summary>Param (int) : 0 mainstory, 1 sidequest, others are invalid;  /// </summary>
    public Action<int, UIMissionGoalNode> onMissionNodeClick;
    /// <summary>Set the mission goal item content will be invoked /// </summary>
    public Action<UIMissionGoalItem> onSetGoalItemContent;
    /// <summary>Set the mission reward item content will be invoked /// </summary>
    public Action<Grid_N> onSetRewardItemContent;

    public Action<UIMissionGoalNode> onMissionDeleteClick;
    public Action<bool, int, UIMissionGoalNode> onTrackBoxSelected;

    #region MISSION_NODE
    public void UpdateMainStoryNodes (int count)
    {
        UIUtility.UpdateListItems<UIMissionGoalNode>(m_MainStoryNodes, missionNodePrefab,
           mainStoryTb.transform, count, OnSetMainStoryNodeContent, OnDestroyMissionNode);

        mainStoryTb.repositionNow = true;
    }

    public void UpdateSideQuestNodes (int count)
    {
        UIUtility.UpdateListItems<UIMissionGoalNode>(m_SideQuestNodes, missionNodePrefab,
           sideQuestTb.transform, count, OnSetMainStoryNodeContent, OnDestroyMissionNode);

        sideQuestTb.repositionNow = true;
    }

    public void AddMainStoryNode ()
    {
        UIMissionGoalNode node = UIUtility.CreateItem<UIMissionGoalNode>(missionNodePrefab, mainStoryTb.transform);
        m_MainStoryNodes.Add(node);
        OnSetMainStoryNodeContent(m_MainStoryNodes.Count - 1, node);
        mainStoryTb.repositionNow = true;
    }

    public void AddSideQuestNode ()
    {
        UIMissionGoalNode node = UIUtility.CreateItem<UIMissionGoalNode>(missionNodePrefab, sideQuestTb.transform);
        m_SideQuestNodes.Add(node);
        OnSetSideQuestNodeContent(m_SideQuestNodes.Count - 1, node);
        sideQuestTb.repositionNow = true;
    }

    public void RemoveMainStoryNode (int index)
    {
        if (index < 0 && index >= m_MainStoryNodes.Count)
            return;

        if (selectedNode == m_MainStoryNodes[index])
            selectedNode = null;

        OnDestroyMissionNode(m_MainStoryNodes[index]);
        Destroy(m_MainStoryNodes[index].gameObject);
        m_MainStoryNodes[index].transform.parent = null;
        m_MainStoryNodes.RemoveAt(index);
    }

    public void RemoveSideQuestNode (int index)
    {
        if (index < 0 && index >= m_SideQuestNodes.Count)
            return;

        if (selectedNode == m_SideQuestNodes[index])
            selectedNode = null;

        OnDestroyMissionNode(m_SideQuestNodes[index]);
        Destroy(m_SideQuestNodes[index].gameObject);
        m_SideQuestNodes[index].transform.parent = null;
        m_SideQuestNodes.RemoveAt(index);
    }

    void OnSetMainStoryNodeContent (int index, UIMissionGoalNode node)
    {
        node.index = index;
        node.onTitleClick += OnMissionTitileClick;
        node.onDeleteBtnClick += OnMissionDeleteBtnClick;
        node.onTrackBoxActive += OnMissionTrackBoxChecked;

        if (onSetNodeContent != null)
        {
            onSetNodeContent(0, node);
        }
    }

    void OnSetSideQuestNodeContent(int index, UIMissionGoalNode node)
    {
        node.index = index;
        node.onTitleClick += OnMissionTitileClick;
        node.onDeleteBtnClick += OnMissionDeleteBtnClick;
        node.onTrackBoxActive += OnMissionTrackBoxChecked;

        if (onSetNodeContent != null)
        {
            onSetNodeContent(1, node);
        }
    }

    void OnDestroyMissionNode(UIMissionGoalNode node)
    {
        node.onTitleClick -= OnMissionTitileClick;
        node.onDeleteBtnClick -= OnMissionDeleteBtnClick;
        node.onTrackBoxActive -= OnMissionTrackBoxChecked;
    }
    #endregion
    
    // 
    // Mission about
    // 
    public void SetMissionAbout (string _name, string _icon, string _desc)
    {
        aboutNpcNameLb.text = _name;
        aboutNpcIcon.spriteName = _icon;
        aboutDescLb.text = _desc;
    }

    // 
    // Mission Goal
    // 

    public UIMissionGoalItem GetGoalItem (int index)
    {
        if (index < 0 && index >= m_MissionGoalItems.Count)
            return null;

        return m_MissionGoalItems[index];
    }

    public void UpdateGoalItem (int count)
    {
        UIUtility.UpdateListItems<UIMissionGoalItem>(m_MissionGoalItems, goalItemPrefab, 
            goalsTb.transform, count, OnSetGoalItemContent, null);

        goalsTb.repositionNow = true;
    }

    void OnSetGoalItemContent (int index, UIMissionGoalItem item)
    {
        m_MissionGoalItems[index].index = index;
        if (onSetGoalItemContent != null)
            onSetGoalItemContent(m_MissionGoalItems[index]);
    }

    //
    // Mission Reward
    //
    public void SetRewardInfo (string _npcName, string _npcIcon)
    {
        if (!rewardNpcInfoRoot.activeSelf)
            rewardNpcInfoRoot.SetActive(true);

        rewardNpcNameLb.text = _npcName;
        rewardNpcIcon.spriteName = _npcIcon;
    }

    public void HideRewardInfo()
    {
        if (rewardNpcInfoRoot.activeSelf)
            rewardNpcInfoRoot.SetActive(false);
    }

    public void SetRewardDesc (string _desc)
    {
        if (!rewardDesc.gameObject.activeSelf)
            rewardDesc.gameObject.SetActive (true);
        rewardDesc.text = _desc;

        if (rewardItemGrid.gameObject.activeSelf)
            rewardItemGrid.gameObject.SetActive(false);
    }

    public void UpdateRewardItem (int count)
    {
        if (!rewardItemGrid.gameObject.activeSelf)
            rewardItemGrid.gameObject.SetActive(true);

        UIUtility.UpdateListItems<Grid_N>(m_RewardGrids, gridPrefab, rewardItemGrid.transform, count,
        OnSetRewardItemContent, null);
        rewardItemGrid.repositionNow = true;

        if (rewardDesc.gameObject.activeSelf)
            rewardDesc.gameObject.SetActive(false);
    }
    
    void OnSetRewardItemContent (int index, Grid_N grid)
    {
        grid.ItemIndex = index;
        if (onSetRewardItemContent != null)
            onSetRewardItemContent(grid);

    }

    #region UI_EVENT
    void OnMissionTitileClick (UIMissionGoalNode node)
    {
        int type = -1;
        Transform trans = node.transform;
        if (trans.parent == mainStoryTb.transform)
        {
            type = 0;
            node.IsSelected = true;
            selectedNode = node;
            for (int i = 0; i < m_MainStoryNodes.Count; i++)
            {
                if (m_MainStoryNodes[i] != node)
                    m_MainStoryNodes[i].IsSelected = false;
            }
        }
        else if (trans.parent = sideQuestRoot.transform)
        {
            type = 1;
            node.IsSelected = true;
            selectedNode = node;
            for (int i = 0; i < m_SideQuestNodes.Count; i++)
            {
                if (m_SideQuestNodes[i] != node)
                    m_SideQuestNodes[i].IsSelected = false;
            }
        }


        if (onMissionNodeClick != null)
            onMissionNodeClick(type, node);
    }

    void OnMissionDeleteBtnClick (UIMissionGoalNode node)
    {
        if (onMissionDeleteClick != null)
        {
            onMissionDeleteClick(node);
        }
    }

    void OnMissionTrackBoxChecked (bool active, UIMissionGoalNode node)
    {
        int type = 0;
        if (m_MainStoryNodes.Count > node.index && m_MainStoryNodes[node.index] == node)
            type = 0;
        else if (m_SideQuestNodes.Count > node.index && m_SideQuestNodes[node.index] == node)
            type = 1;
        else
            type = -1;

        if (type != -1)
        {
            if (onTrackBoxSelected != null)
                onTrackBoxSelected(active, type, node);
        }
    }

    void OnMainStoryMenuClick ()
    {
        if (!mainStroyRoot.activeSelf)
            mainStroyRoot.SetActive(true);

        if (sideQuestRoot.activeSelf)
            sideQuestRoot.SetActive(false);
    }

    void OnSideQuestMenuClick()
    {
        if (!sideQuestRoot.activeSelf)
            sideQuestRoot.SetActive(true);

        if (mainStroyRoot.activeSelf)
            mainStroyRoot.SetActive(false);
    }
    #endregion

    #region UIBASEWND
    public override void Show()
    {

        base.Show();
    }

    protected override void OnHide()
    {
        base.OnHide();
    }

    protected override void OnClose()
    {
        base.OnClose();
    }
    #endregion
}
