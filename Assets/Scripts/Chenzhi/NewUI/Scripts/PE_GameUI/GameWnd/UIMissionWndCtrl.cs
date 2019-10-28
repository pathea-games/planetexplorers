using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;

public class UIMissionWndCtrl : UIBaseWnd
{
    public UIMissionTree mMainStroyTree;
    public UIMissionTree mSideQuestTree;
    public UICheckbox mCkMainStory;
    public UICheckbox mCkSideQuest;

    public Grid_N mPrefabGrid_N;
    public MissionTargetItem_N mPrefabTarget;
    public UIGrid mRewardsGrid;
    public UITable mTargetGrid;

    public UILabel mSPLabel;
    public UILabel mDesLabel;
    public UILabel mGiverName;
    public UISprite mGiverSpr;
    public UISprite mGiverBg;

    public UILabel mSubmitName;
    public UISprite mSubmitSpr;
    public UISprite mSubmitBg;

    public UIScrollBar mTargetScroll;
    public UIScrollBar mRewardsScroll;

    List<MissionTargetItem_N> mTargetList = new List<MissionTargetItem_N>();
    List<Grid_N> mRewardsList = new List<Grid_N>();

    

    bool fristShowMissionTrack = true;
    public override void Show()
    {
        base.Show();
        if (fristShowMissionTrack)
        {
            GameUI.Instance.mMissionTrackWnd.Show();
            fristShowMissionTrack = false;
        }
    }

    protected override void InitWindow()
    {
        base.InitWindow();
        UIMissionMgr.Instance.e_AddMission += AddMission;
        UIMissionMgr.Instance.e_DeleteMission += DeleteMission;
        UIMissionMgr.Instance.e_ReflahMissionWnd += ReflashMissionWnd;

        mMainStroyTree.e_ChangeSelectedNode += MissionNodeOnSelectChange;
        mSideQuestTree.e_ChangeSelectedNode += MissionNodeOnSelectChange;
        ReGetAllMission();
    }

    public void ReGetAllMission()
    {
        ClearMission();
        ClearMissionContent();
        foreach (var kv in UIMissionMgr.Instance.m_MissonMap)
            AddMission(kv.Value);
    }

    void AddMission(UIMissionMgr.MissionView misView)
    {
        UIMissionNode node = null;
        if (misView.mMissionType == MissionType.MissionType_Main ||
            misView.mMissionType == MissionType.MissionType_Time)
            node = mMainStroyTree.AddMissionNode(null, misView.mMissionTitle);
        else if (misView.mMissionType == MissionType.MissionType_Sub)
            node = mSideQuestTree.AddMissionNode(null, misView.mMissionTitle);

        node.mCheckBoxTag.isChecked = misView.mMissionTag;
        node.mData = misView;
        node.e_BtnDelete += MissionNodeOnDelete;
        node.e_CheckedTg += MissionNodeOnCheckTag;

        SelectMissionNode(node);
    }

    void DeleteMission(UIMissionMgr.MissionView misView)
    {
        UIMissionNode node = null;
        if (misView.mMissionType == MissionType.MissionType_Main || misView.mMissionType == MissionType.MissionType_Time)
        {
            node = mMainStroyTree.mNodes.Find(
                delegate(UIMissionNode nd)
                {
                    UIMissionMgr.MissionView mv = nd.mData as UIMissionMgr.MissionView;
                    return mv == misView;
                });
            mMainStroyTree.DeleteMissionNode(node);

            if (mMainStroyTree.mNodes.Count == 0)
                node = null;
            else
                node = mMainStroyTree.mNodes[0];
        }
        else if (misView.mMissionType == MissionType.MissionType_Sub)
        {
            node = mSideQuestTree.mNodes.Find(
                delegate(UIMissionNode nd)
                {
                    UIMissionMgr.MissionView mv = nd.mData as UIMissionMgr.MissionView;
                    return mv == misView;
                });
            mSideQuestTree.DeleteMissionNode(node);

            if (mSideQuestTree.mNodes.Count == 0)
                node = null;
            else
                node = mSideQuestTree.mNodes[0];
        }

        SelectMissionNode(node);
    }

    public void ClearMission()
    {
        mMainStroyTree.Clear();
        mSideQuestTree.Clear();
    }

    void SelectMissionNode(UIMissionNode node)
    {
        if (node == null)
            ClearMissionContent();
        else
            ReFlashMissionContent(node);
    }

    void ReflashMissionWnd()
    {
        if (mMainStroyTree == null || mSideQuestTree == null)
            return;
        UIMissionNode node = mMainStroyTree.gameObject.activeSelf ? mMainStroyTree.mSelectedNode : mSideQuestTree.mSelectedNode;
        SelectMissionNode(node);
    }

    public void ClearMissionContent()
    {
        for (int i = 0; i < mTargetList.Count; i++)
        {
            mTargetList[i].transform.parent = null;
            Destroy(mTargetList[i].gameObject);
        }
        mTargetList.Clear();

        for (int i = 0; i < mRewardsList.Count; i++)
        {
            mRewardsList[i].transform.parent = null;
            Destroy(mRewardsList[i].gameObject);
        }
        mRewardsList.Clear();

        mDesLabel.text = "";
        mSPLabel.text = "";
        mGiverName.text = "";
        mSubmitName.text = "";
        mGiverSpr.spriteName = "Null";
        mSubmitSpr.spriteName = "Null";

        mGiverSpr.transform.parent.gameObject.SetActive(false);
        mSubmitSpr.transform.parent.gameObject.SetActive(false);
    }

    //lz-2016.08.17 这个接口提供外部通过UIMissionMgr.MissionView来选中任务节点
    public void SelectMissionNodeByData(object data)
    {
        if (null == data) return;
        if (data is UIMissionMgr.MissionView)
        {
             UIMissionMgr.MissionView misView=(UIMissionMgr.MissionView)data;
             if (misView.mMissionType == MissionType.MissionType_Main || misView.mMissionType == MissionType.MissionType_Time)
             {
                 mCkMainStory.isChecked=true;
                 mCkSideQuest.isChecked=false;
                 BtnMainStroy();
                 UIMissionNode findNode=mMainStroyTree.mNodes.Find(a=>(a.mParent==null&&a.mData!=null&&a.mData is  UIMissionMgr.MissionView&&((UIMissionMgr.MissionView)a.mData).mMissionID==misView.mMissionID));
                 if (null != findNode)
                 {
                     if(null != mMainStroyTree.mSelectedNode)
                         mMainStroyTree.mSelectedNode.Selected=false;
                     mMainStroyTree.mSelectedNode = findNode;
                     findNode.Selected = true;
                     ReFlashMissionContent(findNode);
                 }
             }
             else if (misView.mMissionType == MissionType.MissionType_Sub)
             {
                 mCkMainStory.isChecked=false;
                 mCkSideQuest.isChecked=true;
                 BtnSideQuest();
                 UIMissionNode findNode=mSideQuestTree.mNodes.Find(a=>(a.mParent==null&&a.mData!=null&&a.mData is  UIMissionMgr.MissionView&&((UIMissionMgr.MissionView)a.mData).mMissionID==misView.mMissionID));
                 if (null != findNode)
                 {
                     if (null != mSideQuestTree.mSelectedNode)
                         mSideQuestTree.mSelectedNode.Selected = false;
                     mSideQuestTree.mSelectedNode = findNode;
                     findNode.Selected = true;
                     ReFlashMissionContent(findNode);
                 }
             }
        }
    }


    void ReFlashMissionContent(UIMissionNode node)
    {

        if (mTargetScroll != null)
            mTargetScroll.scrollValue = 0;

        ClearMissionContent();

        UIMissionMgr.MissionView view = node.mData as UIMissionMgr.MissionView;
        if (view == null)
            return;

        mDesLabel.text = view.mMissionDesc;
        int missionId = view.mMissionID;
        MissionCommonData mcd = MissionManager.GetMissionCommonData(missionId);
        if (mcd != null)
        {
            if (mcd.addSpValue > 0)
                mSPLabel.text = "SP" + " " + "+" + " " + "[ffff00]" + mcd.addSpValue + "[-]";
        }

        SetGiver(view.mMissionStartNpc.mNpcIcoStr, view.mMissionStartNpc.mName);
        SetSubmit(view.mMissionReplyNpc.mNpcIcoStr, view.mMissionReplyNpc.mName);

        for (int i = 0; i < view.mTargetList.Count; i++)
            AddTarget(view.mTargetList[i]);
        mTargetGrid.Reposition();
        //if (mTargetScroll != null)
        //    mTargetScroll.scrollValue = 0;

        for (int i = 0; i < view.mRewardsList.Count; i++)
            AddRewards(view.mRewardsList[i]);
        mRewardsGrid.Reposition();
        if (mRewardsScroll != null)
            mRewardsScroll.scrollValue = 0;
    }

    void AddTarget(UIMissionMgr.TargetShow target)
    {
        MissionTargetItem_N AddItem = Instantiate(mPrefabTarget) as MissionTargetItem_N;
        AddItem.transform.parent = mTargetGrid.transform;
        AddItem.transform.localPosition = new Vector3(0, 0, -1);
        AddItem.transform.localRotation = Quaternion.identity;
        AddItem.transform.localScale = Vector3.one;
        AddItem.SetTarget(target);
        mTargetList.Add(AddItem);
    }

    void AddRewards(ItemSample itemGrid)
    {
        Grid_N AddItem = Instantiate(mPrefabGrid_N) as Grid_N;
        AddItem.transform.parent = mRewardsGrid.transform;
        AddItem.transform.localPosition = new Vector3(0, 0, -1);
        AddItem.transform.localRotation = Quaternion.identity;
        AddItem.transform.localScale = Vector3.one;
        AddItem.SetItem(itemGrid);
        mRewardsList.Add(AddItem);
    }

    void SetGiver(string IconName, string name)
    {
        bool enable = true;
        if (name == null || name.Length == 0)
            enable = false;

        mGiverSpr.enabled = enable;
        mGiverName.enabled = enable;
        mGiverBg.enabled = enable;

        mGiverSpr.transform.parent.gameObject.SetActive(true);
        mGiverSpr.spriteName = IconName;
        mGiverSpr.MakePixelPerfect();
        mGiverName.text = name;
    }

    void SetSubmit(string IconName, string name)
    {
        bool enable = true;
        if (name == null || name.Length == 0)
            enable = false;

        mSubmitSpr.enabled = enable;
        mSubmitName.enabled = enable;
        mSubmitBg.enabled = enable;

        mSubmitSpr.transform.parent.gameObject.SetActive(true);
        mSubmitSpr.spriteName = IconName;
        mSubmitSpr.MakePixelPerfect();
        mSubmitName.text = name;
    }

    #region Click funcs
    void BtnMainStroy()
    {
        mSideQuestTree.gameObject.SetActive(false);
        mMainStroyTree.gameObject.SetActive(true);

        SelectMissionNode(mMainStroyTree.mSelectedNode);
    }
    void BtnSideQuest()
    {
        mMainStroyTree.gameObject.SetActive(false);
        mSideQuestTree.gameObject.SetActive(true);

        SelectMissionNode(mSideQuestTree.mSelectedNode);
    }

    UIMissionMgr.MissionView delView = null;
    void MissionNodeOnDelete(object sender)
    {
        UIMissionNode node = sender as UIMissionNode;
        if (node == null)
            return;
        delView = node.mData as UIMissionMgr.MissionView;
        if (delView == null)
            return;

        MissionCommonData data = MissionManager.GetMissionCommonData(delView.mMissionID);
        if (data == null)
            return;
        //else
        //    MissionManager.Instance.m_PlayerMission.FailureMission(delView.mMissionID);

        if (!data.m_bGiveUp)
        {
            new PeTipMsg(PELocalization.GetString(8000174), PeTipMsg.EMsgLevel.Warning);
            return;
        }

        MessageBox_N.ShowYNBox(PELocalization.GetString(8000066), DeleteMissionOk);
    }

    void DeleteMissionOk()
    {
        if(UIMissionMgr.Instance)
            UIMissionMgr.Instance.DeleteMission(delView);
        delView = null;
    }

    void MissionNodeOnSelectChange(object sender)
    {
        UIMissionNode node = sender as UIMissionNode;
        if (node == null)
            return;

        SelectMissionNode(node);
    }

    void MissionNodeOnCheckTag(object sender, bool isChecked)
    {
        UIMissionNode node = sender as UIMissionNode;
        if (node == null)
            return;
        UIMissionMgr.MissionView view = node.mData as UIMissionMgr.MissionView;
        if (view == null)
            return;

        view.mMissionTag = isChecked;
        UIMissionMgr.Instance.CheckMissionTag(view);
    }
    #endregion
}
