using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIMissionTrackCtrl : UIBaseWnd 
{
	public UIMissionTree mMissionTree;
    [SerializeField]
    private UIDraggablePanel m_DragPanel;
    [SerializeField]
    private Transform m_TutorialParent;
    [SerializeField]
    private GameObject m_TutorialPrefab;

    Coroutine _coroutine;

    #region mono methods
    void Awake()
	{
        UIMissionMgr.Instance.e_AddMission += OnAddMissionView;
		UIMissionMgr.Instance.e_DeleteMission += OnDelMissionView;
		UIMissionMgr.Instance.e_CheckTagMission += OnCheckTagMission;
        //lz-2018.01.04 主线任务排在前面
        mMissionTree.mContentTable.sorted = true;
        ReGetAllMission();
	}

    void OnEnable()
    {
        _coroutine = StartCoroutine(UpdateMissionTrack());
    }

    private void OnDisable()
    {
        if (_coroutine != null) StopCoroutine(_coroutine);
    }

    #endregion

    #region overrde methods

    public override void Show()
    {
        base.Show();
        ResetContentPos();
    }

    #endregion

    #region private methods

    void ResetContentPos()
    {
        m_DragPanel.enabled = false;
        mMissionTree.RepositionContent();
        StartCoroutine(DelayActiveDragIterator());
    }

    //lz-2016.09.28 ui刚打开的时候UIDraggablePanel计算bounds大小是0，需要等一下在激活，避免拖拽
    IEnumerator DelayActiveDragIterator()
    {
        yield return null;
        m_DragPanel.enabled = true;
    }
    void ReGetAllMission()
	{
		foreach( var kv in UIMissionMgr.Instance.m_MissonMap)
			OnAddMissionView( kv.Value);
	}

    bool ContainsMissionView(UIMissionMgr.MissionView view)
	{
		bool contains = false;
		foreach (UIMissionNode node in mMissionTree.mNodes)
		{
			if (node.mParent == null)
			{
				UIMissionMgr.MissionView dataView = node.mData as UIMissionMgr.MissionView;
				if (dataView.mMissionID == view.mMissionID)
					return true;
			}
		}
		return contains;
	}

    void OnAddMissionView(UIMissionMgr.MissionView view)
    {
        if (view.mMissionTag && !ContainsMissionView(view))
        {
            UIMissionNode rootNode = mMissionTree.AddMissionNode(null, "", false, false, false);
            rootNode.mData = view;
            rootNode.mLbTitle.maxLineCount = 1;

            UIMissionNode tragetNode = mMissionTree.AddMissionNode(rootNode, "", false, false, false);
            tragetNode.mData = view.mTargetList;
            tragetNode.mLbTitle.maxLineCount = 0;

            UpdateNodeText(rootNode);
            UpdateNodeText(tragetNode);

            Sort();

            rootNode.ChangeExpand();
        }
    }


    /// <summary>
    /// lz-2017.01.04 主线任务排在前面，支线在后面
    /// </summary>
    void Sort()
    {
        int minIndex = 0;
        int maxIndex = mMissionTree.mNodes.Count;
        for (int i = 0; i < mMissionTree.mNodes.Count; i++)
        {
            var node = mMissionTree.mNodes[i];
            if (node.mTablePartent == mMissionTree.mContentTable && node.mData is UIMissionMgr.MissionView)
            {
                var data = node.mData as UIMissionMgr.MissionView;
                node.gameObject.name = (data.mMissionType == MissionType.MissionType_Main ? minIndex++ : maxIndex++).ToString();
            }
        }
        mMissionTree.RepositionContent();
    }

    void OnCheckTagMission(UIMissionMgr.MissionView view)
    {
        if (view.mMissionTag)
        {
            OnAddMissionView(view);
        }
        else
        {
            OnDelMissionView(view);
        }
    }

    void OnDelMissionView(UIMissionMgr.MissionView view)
    {
        UIMissionNode node = FindMissionNode(view);
        if (node != null)
        {
            mMissionTree.DeleteMissionNode(node);
        }
    }

    UIMissionNode FindMissionNode(UIMissionMgr.MissionView view)
    {
        foreach (UIMissionNode node in mMissionTree.mNodes)
        {
            if (node.mParent == null)
            {
                UIMissionMgr.MissionView dataView = node.mData as UIMissionMgr.MissionView;
                if (dataView == null)
                {
                    Debug.LogError("missionView data eroor!");
                    continue;
                }
                if (dataView.mMissionID == view.mMissionID)
                    return node;
            }
        }
        return null;
    }

    IEnumerator UpdateMissionTrack()
    {
        while (true)
        {
            foreach (UIMissionNode node in mMissionTree.mNodes)
            {
                UpdateNodeText(node);
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    const string mainMissionColor = "[9b3df4]{0}[-]";
    const string sideMissionColor = "[37cbf4]{0}[-]";
    const string complateTitleFormat = "{0} [{1}]";

    const string complateMissionColor = "[B4B4B4]{0} {1}[-]\n";
    const string uncompletedMissionColor = "[FFFFFF]{0} {1}[-]\n";
    const string countFormat = "{0}/{1}";
    

    void UpdateNodeText(UIMissionNode node)
    {
        if (node.mData == null)
            return;
        if (node.mParent == null)
        {
            if (node.mData is UIMissionMgr.MissionView)
            {
                UIMissionMgr.MissionView view = node.mData as UIMissionMgr.MissionView;
                string colorFormat = view.mMissionType == MissionType.MissionType_Main ? mainMissionColor : sideMissionColor;
                string text = view.mComplete ? string.Format(complateTitleFormat, view.mMissionTitle, PELocalization.GetString(8000694)) : view.mMissionTitle;
                node.mLbTitle.text = string.Format(colorFormat, text);
            }
        }
        else
        {
            if (node.mData is List<UIMissionMgr.TargetShow>)
            {
                List<UIMissionMgr.TargetShow> showList = node.mData as List<UIMissionMgr.TargetShow>;
                string text = string.Empty;
                foreach (UIMissionMgr.TargetShow target in showList)
                {
                    string countStr = string.Empty;
                    if (target.mMaxCount > 0)
                    {
                        countStr = string.Format(countFormat, target.mCount, target.mMaxCount);
                    }
                    text += string.Format(target.mComplete ? complateMissionColor : uncompletedMissionColor, target.mContent, countStr);
                }
                node.mLbTitle.text = text;
            }
        }
    }

    #endregion

    #region public methods
    public void ClearUI()
	{
		if(mMissionTree != null)
			mMissionTree.Clear();
	}

    //lz-2016.09.12 进入射击模式的时候，鼠标会锁定，所以需要禁用Drag，不然鼠标状态不会被释放了
    public void EnableWndDrag(bool enable)
    {
        UIDragObject[] dragObjs=transform.GetComponentsInChildren<UIDragObject>();
        if (null != dragObjs && dragObjs.Length > 0)
        {
            for (int i = 0; i < dragObjs.Length; i++)
            {
                dragObjs[i].enabled = enable;
            }
        }
    }

    /// <summary>lz-2016.11.03 显示这个窗口的Tutorial</summary>
    public void ShowWndTutorial()
    {
        if (Pathea.PeGameMgr.IsTutorial)
        {
            GameObject go = Instantiate(m_TutorialPrefab);
            go.transform.parent = m_TutorialParent;
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.transform.localRotation = Quaternion.identity;
        }
    }
    #endregion

}
