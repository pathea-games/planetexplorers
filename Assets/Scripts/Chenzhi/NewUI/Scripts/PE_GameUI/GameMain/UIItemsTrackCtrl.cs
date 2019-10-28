using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using System;

public class UIItemsTrackCtrl : UIBaseWnd 
{
    [SerializeField]
    private UIMissionTree itemsTree;
    [SerializeField]
    private UIDraggablePanel dragPanel;

    public enum TrackType
    {
        Script,
        ISO
    }
    public class ItemTrackData
    {
        public ItemTrackData(TrackType type)
        {
            this.type = type;
        }

        public TrackType type { get; private set; }
        public string itemName;
        public int itemCount;
        public int scriptID;

        /// <summary> key == id, value == need Count </summary>
        public Dictionary<int, int> costDic = new Dictionary<int, int>(2);
    }

    /// <summary>
    /// lz-2018.01.04 Data没有放在每个Node里面是为了避免拆箱
    /// </summary>
    Dictionary<UIMissionNode, ItemTrackData> _nodeDataDic = new Dictionary<UIMissionNode, ItemTrackData>(4);

    Coroutine _coroutine;

    public Action<int,bool> ScriptTrackChanged;

    #region mono methods

    void OnEnable()
    {
        _coroutine = StartCoroutine(UpdateItemsTrack());
    }

    private void OnDisable()
    {
        if (_coroutine != null) StopCoroutine(_coroutine);
    }

    #endregion

    #region public methods

    public bool ContainsScript(int scriptID)
    {
        foreach (var nodeData in _nodeDataDic.Values)
        {
            if (nodeData.scriptID == scriptID) return true;
        }
        return false;
    }

    public bool ContainsIso(string isoName)
    {
        foreach (var nodeData in _nodeDataDic.Values)
        {
            if (nodeData.type == TrackType.ISO && nodeData.itemName == isoName) return true;
        }
        return false;
    }

    public void AddIso(string isoName, Dictionary<int, int> m_Cost)
    {
        if (!ContainsIso(isoName))
        {
            ItemTrackData data = new ItemTrackData(TrackType.ISO);
            data.itemName = isoName;
            data.itemCount = 1;
            foreach (KeyValuePair<int, int> kv in m_Cost)
            {
                if(kv.Value>0) data.costDic.Add(kv.Key, kv.Value);
            }
            AddTrackData(data);
        }
    }

    public void RemoveIso(string isoName)
    {
        foreach (var item in _nodeDataDic)
        {
            if (item.Value.type == TrackType.ISO && item.Value.itemName == isoName)
            {
                DeleteNode(item.Key);
                return;
            }
        }
    }

    public void UpdateOrAddScript(Pathea.Replicator.Formula ms,int multiple)
    {
        if (ms == null) return;
        ItemTrackData data = null;
        foreach (var nodeData in _nodeDataDic.Values)
        {
            if (nodeData.type == TrackType.Script && nodeData.scriptID == ms.id)
            {
                data = nodeData;
                break;
            }
        }
        bool needAdd = false;
        if (data == null)
        {
            data = new ItemTrackData(TrackType.Script);
            data.scriptID = ms.id;
            data.itemName = ItemProto.GetName(ms.productItemId);
            needAdd = true;
        }
        else
        {
            data.costDic.Clear();
        }
        data.itemCount = multiple * ms.m_productItemCount;
        foreach (var item in ms.materials)
        {
            data.costDic.Add(item.itemId, item.itemCount * multiple);
        }

        if(needAdd) AddTrackData(data);
    }


    public void RemoveScript(int scriptID)
    {
        foreach (var item in _nodeDataDic)
        {
            if (item.Value.type == TrackType.Script && item.Value.scriptID == scriptID)
            {
                DeleteNode(item.Key);
                return;
            }
        }
    }

    //lz-2016.09.12 进入射击模式的时候，鼠标会锁定，所以需要禁用Drag，不然鼠标状态不会被释放了
    public void EnableWndDrag(bool enable)
    {
        UIDragObject[] dragObjs = transform.GetComponentsInChildren<UIDragObject>();
        if (null != dragObjs && dragObjs.Length > 0)
        {
            for (int i = 0; i < dragObjs.Length; i++)
            {
                dragObjs[i].enabled = enable;
            }
        }
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

    bool AddTrackData(ItemTrackData data)
    {
        UIMissionNode rootNode = itemsTree.AddMissionNode(null, "", false, true, false);
        rootNode.mLbTitle.maxLineCount = 1;
        rootNode.e_BtnDelete += DeleteNode;

        UIMissionNode tragetNode = itemsTree.AddMissionNode(rootNode, "", false, false, false);
        tragetNode.mLbTitle.maxLineCount = 0;

        _nodeDataDic.Add(rootNode, data);

        UpdateNodeText(rootNode);

        rootNode.ChangeExpand();

        if (!GameUI.Instance.mItemsTrackWnd.isShow) GameUI.Instance.mItemsTrackWnd.Show();

        if (data != null && data.type == TrackType.Script)
        {
            if (ScriptTrackChanged != null) ScriptTrackChanged(data.scriptID, true);
        }

        return true;
    }


    void DeleteNode(object obj)
    {
        UIMissionNode node = obj as UIMissionNode;
        if (node)
        {
            ItemTrackData data = null;
            if (_nodeDataDic.ContainsKey(node))
            {
                data = _nodeDataDic[node];
                _nodeDataDic.Remove(node);
            }
            itemsTree.DeleteMissionNode(node);
            if (data != null && data.type == TrackType.Script)
            {
                if (ScriptTrackChanged != null) ScriptTrackChanged(data.scriptID,false);
            }
        }
    }

    void ResetContentPos()
    {
        dragPanel.enabled = false;
        itemsTree.RepositionContent();
        StartCoroutine(DelayActiveDragIterator());
    }

    //lz-2016.09.28 ui刚打开的时候UIDraggablePanel计算bounds大小是0，需要等一下在激活，避免拖拽
    IEnumerator DelayActiveDragIterator()
    {
        yield return null;
        dragPanel.enabled = true;
    }


    IEnumerator UpdateItemsTrack()
    {
        while (true)
        {
            foreach (UIMissionNode node in _nodeDataDic.Keys)
            {
                UpdateNodeText(node);
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    const string complateTitleFormat = "{0} X{1} [{2}]";
    const string complateTitleColor = "[C8C800]{0}[-]";

    const string complateCountColor = "[B4B4B4]{0} {1}[-]\n";
    const string uncompletedCountColor = "[FFFFFF]{0} {1}[-]\n";
    const string countFormat = "{0}/{1}";

    PlayerPackage _playerPackage = null;
    PlayerPackage playerPackage
    {
        get
        {
            if (null == _playerPackage)
            {
                Pathea.PlayerPackageCmpt p = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.PlayerPackageCmpt>();
                _playerPackage = p.package;
            }

            return _playerPackage;
        }
    }

    void UpdateNodeText(UIMissionNode node)
    {
        if (node == null) return;
        if (!_nodeDataDic.ContainsKey(node)) return;
        ItemTrackData data = _nodeDataDic[node];
        if (data == null) return;

        UIMissionNode childNode = null;
        if (node.mChilds.Count > 0) childNode = node.mChilds[0];

        bool complate = true;
        if (playerPackage != null)
        {
            string text = string.Empty;
            foreach (var kv in data.costDic)
            {
                string costName = ItemProto.GetName(kv.Key);
                int curCount = playerPackage.GetCount(kv.Key);
                string countStr = string.Empty;
                countStr = string.Format(countFormat, curCount, kv.Value);
                bool costComplate = curCount >= kv.Value;
                text += string.Format(costComplate ? complateCountColor : uncompletedCountColor, costName, countStr);
                if (!costComplate) complate = false;
            }
            if(childNode) childNode.mLbTitle.text = text;
        }

        string titleText = data.itemName;
        if (complate)
        {
            titleText = string.Format(complateTitleColor, string.Format(complateTitleFormat, data.itemName, data.itemCount, PELocalization.GetString(8000694)));
        }
        node.mLbTitle.text = titleText;
    }

    #endregion

}
