using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CSUI_FactoryReplicator : MonoBehaviour
{
    // Total Delegate
    public delegate void DelegateType_0(GameObject go);
    public delegate void DelegateType_1(GameObject go, int index);
    public delegate void DelegateType_2(string selection);
    public delegate void DelegateType_3(object history);
    public delegate int DelegateType_4(int count);

    #region LEFT_MENU

    // Left part UI content
    [System.Serializable]
    public class LeftContent
    {
        public UIPopupList popList;

        //		public GameObject   listItemPrefab;
        //		public UIGrid	root;
        public UIEfficientGrid gridList;

        public UIScrollBox scrollBox;
        public UIPanel scrollBoxPanel;

        public List<UICheckbox> menuCBs;
    }

    public LeftContent m_MenuContent;

    // ---------------------------------
    // Custom event
    // ---------------------------------

    /// <summary>
    /// The left menu button list click event.
    /// </summary>
    public DelegateType_1 onMenuBtnClick;

    /// <summary>
    /// The menu popuplist selection event.
    /// </summary>
    public DelegateType_2 onMenueSelect;


    // ---------------------------------
    //  Call back
    // ---------------------------------

    // Left Menu Check Box Click 
    void OnMenuCBClick(GameObject go)
    {
        int index = m_MenuContent.menuCBs.FindIndex(item0 => item0.gameObject == go);

        if (index == -1)
        {
            Debug.LogError("Cant find the gameobject in menu check box.");
            return;
        }

        if (onMenuBtnClick != null)
            onMenuBtnClick(go, index);
    }

    // Menu poplist Selection click
    void OnMenuSelectionChange(string selection)
    {
        if (onMenueSelect != null)
            onMenueSelect(selection);
    }


    // ---------------------------------
    //  Func
    // ---------------------------------

    /// <summary>
    /// Instantiates the menu items of spectifies prefab .
    /// </summary>
    //	public GameObject InstantiateMenuListItem(string desc)
    //	{
    //		GameObject go = GameObject.Instantiate(m_MenuContent.listItemPrefab) as GameObject;
    //		go.name = desc;
    //		go.transform.parent = m_MenuContent.root.transform;
    //		go.transform.localPosition = Vector3.zero;
    //		go.transform.localRotation = Quaternion.identity;
    //		go.transform.localScale = Vector3.one;
    //
    //		return go;
    //	}

    /// <summary>
    /// Destroies the all menu list items.
    /// </summary>
    //	public void DestroyMenuListItems ()
    //	{
    //		UIDraggablePanel dp = m_MenuContent.scrollBox.m_DraggablePanel.gameObject.GetComponent<UIDraggablePanel>();
    //		dp.ResetPosition();
    //
    //		Transform trans = m_MenuContent.root.transform;
    //		for (int i = 0; i < trans.childCount; )
    //		{
    //			GameObject.Destroy(trans.GetChild(i).gameObject);
    //			trans.GetChild(i).parent = null;
    //		}
    //	}

    public int GetMenuListItemCount()
    {
        return m_MenuContent.gridList.Gos.Count;
    }

    public GameObject GetMenuListItemGo(int index)
    {
        return m_MenuContent.gridList.Gos[index];
    }

    public bool SetMenuCBChecked(int index, bool check)
    {
        if (index < -1 || index >= m_MenuContent.menuCBs.Count)
            return false;

        m_MenuContent.menuCBs[index].isChecked = true;
        return true;
    }

    #endregion

    #region GRAPH & QUERY_CONTENT

    /// Middle part UI Contents
    [System.Serializable]
    public class MiddleContent
    {
        public UIGraphControl graphCtrl;
        public UIScrollBox graphScrollBox;

        public UIGrid queryItemRoot;
        public GameObject queryItemPrefab;
        public UIScrollBox queryScrollBox;

        public UIButton queryLeftBtn;
        public UIButton queryRightBtn;

        public float duration = 1.0f;

        public UIInput queryInput;
        public UIButton querySearchBtn;
        public UIButton queryClearBtn;

    }
    public MiddleContent m_MiddleContent;

    // ---------------------------------
    // Custom event
    // ---------------------------------
    public DelegateType_3 onGraphUseHistory;
    public DelegateType_0 onQueryLeftBtnClick;
    public DelegateType_0 onQueryRightBtnClick;
    public DelegateType_0 onQuerySearchBtnClick;
    public DelegateType_0 onQueryClearBtnClick;

    // Graph
    const int c_GraphHistoryCont = 20;
    List<object> m_GraphHistory = new List<object>();
    int m_HistoryIndex = -1;

    // Query member
    bool m_QueryScrolling = false;
    float m_Factor;
    float m_prvVal;
    float m_TweenLen;
    UIDraggablePanel m_QueryDraggablePanel;
    //Transform m_QueryPanelTrans;

    // ---------------------------------
    // Func
    // ---------------------------------

    /// <summary>
    /// Clears the graph.
    /// </summary>
    public void ClearGraph()
    {
        m_MiddleContent.graphCtrl.ClearGraph();
    }

    /// <summary>
    /// Draws the graph.
    /// </summary>
    /// <param history="param"> history use </param>
    public void DrawGraph()
    {
        m_MiddleContent.graphCtrl.DrawGraph();
    }

    public void AddGraphHistory(object history)
    {
        if (m_GraphHistory.Count >= c_GraphHistoryCont)
            m_GraphHistory.RemoveAt(0);
        m_GraphHistory.Add(history);
        m_HistoryIndex = m_GraphHistory.Count - 2;
    }

    /// <summary>
    /// Instantiates the query grid item of spectifies prefab ..
    /// </summary>
    public GameObject InstantiateQueryItem(string desc)
    {
        GameObject go = GameObject.Instantiate(m_MiddleContent.queryItemPrefab) as GameObject;
        go.transform.parent = m_MiddleContent.queryItemRoot.transform;

        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;

        m_MiddleContent.queryItemRoot.repositionNow = true;
        return go;

    }

    /// <summary>
    /// Destroies all the query grid items.
    /// </summary>
    public void DestroyQueryItems()
    {
        m_QueryDraggablePanel.ResetPosition();

        Transform trans = m_MiddleContent.queryItemRoot.transform;

        for (int i = 0; i < trans.childCount; )
        {
            GameObject.Destroy(trans.GetChild(i).gameObject);
            trans.GetChild(i).parent = null;
        }

    }

    /// <summary>
    /// Gets the valid query string.
    /// </summary>
    public string GetQueryString()
    {
        UIInput input = m_MiddleContent.queryInput;

        input.text = input.text.Trim();
        string queryText = input.text;

        // replace some #
        queryText = queryText.Replace("*", "");
        queryText = queryText.Replace("$", "");
        queryText = queryText.Replace("(", "");
        queryText = queryText.Replace(")", "");
        queryText = queryText.Replace("@", "");
        queryText = queryText.Replace("^", "");
        queryText = queryText.Replace("[", "");
        queryText = queryText.Replace("]", "");
        //lz-2016.08.12 "\"增加字符过滤
        queryText = queryText.Replace("\\", "");
        //queryText = queryText.Replace(" ","");

        input.text = queryText;

        return queryText;
    }

    /// <summary>
    /// Determines whether Query input value is valid.
    /// </summary>
    public bool IsQueryInputValid()
    {
        string text = m_MiddleContent.queryInput.text;
        if (text.Length > 0)
            return true;
        return false;
    }

    // ---------------------------------
    // Callback
    // ---------------------------------

    void OnGraphBackBtnClick()
    {

        if (m_HistoryIndex >= m_GraphHistory.Count)
            m_HistoryIndex = m_GraphHistory.Count - 1;

        if (m_HistoryIndex < 0)
            return;

        if (onGraphUseHistory != null)
            onGraphUseHistory(m_GraphHistory[m_HistoryIndex]);

        m_HistoryIndex--;
    }

    void OnGraphForwadBtnClick()
    {

        if (m_HistoryIndex < 0)
            m_HistoryIndex = 1;

        if (m_HistoryIndex >= m_GraphHistory.Count)
            return;

        if (onGraphUseHistory != null)
            onGraphUseHistory(m_GraphHistory[m_HistoryIndex]);

        m_HistoryIndex++;
    }

    /// <summary>
    /// Raises the query left button click event.
    /// </summary>
    void OnQueryLeftBtnClick(GameObject go)
    {
        if (m_QueryScrolling)
            return;

        m_QueryScrolling = true;

        UIPanel mPanel = m_MiddleContent.queryScrollBox.m_DraggablePanel;

        if (mPanel.clipping == UIDrawCall.Clipping.SoftClip)
            m_TweenLen = m_MiddleContent.queryScrollBox.m_DraggablePanel.clipRange.z - mPanel.clipSoftness.x * 0.5f;
        else
            m_TweenLen = m_MiddleContent.queryScrollBox.m_DraggablePanel.clipRange.z;

        m_Factor = 0;
        m_prvVal = 0;

        // Custom event if have
        if (onQueryLeftBtnClick != null)
            return;
    }

    /// <summary>
    /// Raises the query right button click event.
    /// </summary>
    /// <param name="go">Go.</param>
    void OnQueryRightBtnClick(GameObject go)
    {
        if (m_QueryScrolling)
            return;

        m_QueryScrolling = true;

        UIPanel mPanel = m_MiddleContent.queryScrollBox.m_DraggablePanel;

        if (mPanel.clipping == UIDrawCall.Clipping.SoftClip)
            m_TweenLen = -(m_MiddleContent.queryScrollBox.m_DraggablePanel.clipRange.z - mPanel.clipSoftness.x * 0.5f);
        else
            m_TweenLen = -m_MiddleContent.queryScrollBox.m_DraggablePanel.clipRange.z;

        m_Factor = 0;
        m_prvVal = 0;

        // Custom event if have
        if (onQueryRightBtnClick != null)
            return;
    }

    /// <summary>
    /// Raises the query search button click event.
    /// </summary>
    void OnQuerySearchBtnClick(GameObject go)
    {
        if (onQuerySearchBtnClick != null)
            onQuerySearchBtnClick(go);
    }

    void OnQueryClearBtnClick(GameObject go)
    {
        if (onQueryClearBtnClick != null)
            onQueryClearBtnClick(go);
    }

    #endregion


    #region COMPOUND

    /// Right part UI Contents
    [System.Serializable]
    public class RightContent
    {
        public GameObject gridPrefab;
        public Transform gridRoot;

        public UIInput countInput;

        public UIButton addCountBtn;
        public UIButton subCountBtn;

        public UIButton maxCountBtn;
        public UIButton minCountBtn;

        public UIButton compoundBtn;
    }

    public RightContent m_RightContent;

    // ---------------------------------
    // Custom event
    // ---------------------------------
    public DelegateType_4 onCountIputChanged;
    public DelegateType_0 onCompoundBtnClick;

    // ---------------------------------
    // Func
    // ---------------------------------

    /// <summary>
    /// Instantiates the grid item for right content.
    /// </summary>
    public GameObject InstantiateGridItem(string desc)
    {
        GameObject go = GameObject.Instantiate(m_RightContent.gridPrefab) as GameObject;
        go.transform.parent = m_RightContent.gridRoot;

        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;

        m_MiddleContent.queryItemRoot.repositionNow = true;
        return go;
    }

    // ---------------------------------
    // Call Back
    // ---------------------------------

    /// <summary>
    /// Raises the count input changed event.
    /// </summary>
    //	void OnCountInputSubmit(string text)
    //	{
    //
    //	}

    void OnCountInputSelected(GameObject go,bool isSelect)
    {
        //lz-2016.08.12 当取消选中的时候更改值
        if (isSelect) return;
        int count = 1;
        if (CSUtils.IsNumber(m_RightContent.countInput.text))
        {
            count = int.Parse(m_RightContent.countInput.text);
        }
        UIGraphControl graph_ctrl = m_MiddleContent.graphCtrl;
        if (graph_ctrl.rootNode == null)
            return;

        //lz-2016.08.03 输入的数量限制在范围内
        count = Mathf.Clamp(count, graph_ctrl.rootNode.ms.m_productItemCount, graph_ctrl.GetMaxCount());
        this.UpdateInputCount(count,false);
    }

    private void UpdateInputCount(int count, bool immediateUpdateInputTxet = true)
    {
        if (onCountIputChanged != null)
            count = onCountIputChanged(count);
        if (immediateUpdateInputTxet)
            m_RightContent.countInput.text = count.ToString();
        else
            StartCoroutine(this.WiatUpdateInputIterator(count));
    }

    //lz-2016.08.12 延迟一帧更新是因为不能在UIInput的OnSelect事件里面设置UIInput.text,不然会出现CaratChar遗留在输入框的bug
    private IEnumerator WiatUpdateInputIterator(int count)
    {
        yield return null;
        m_RightContent.countInput.text = count.ToString();
    }

    /// <summary>
    /// Raises the add button click event.
    /// </summary>
    void OnAddBtnClick(GameObject go)
    {
        if (!CSUtils.IsNumber(m_RightContent.countInput.text))
            return;

        int count = int.Parse(m_RightContent.countInput.text);
        UIGraphControl graph_ctrl = m_MiddleContent.graphCtrl;
        if (graph_ctrl.rootNode == null)
            return;
        if (count >= graph_ctrl.GetMaxCount())
        {
            return;
        }
        if (count + graph_ctrl.rootNode.ms.m_productItemCount <= graph_ctrl.GetMaxCount())
        {
            count += graph_ctrl.rootNode.ms.m_productItemCount;
        }
        this.UpdateInputCount(count);
    }

    /// <summary>
    /// Raises the substract button click event.
    /// </summary>
    void OnSubstractBtnClick(GameObject go)
    {
        if (!CSUtils.IsNumber(m_RightContent.countInput.text))
            return;

        int count = int.Parse(m_RightContent.countInput.text);
        UIGraphControl graph_ctrl = m_MiddleContent.graphCtrl;
        if (graph_ctrl.rootNode == null)
            return;
        if (count > graph_ctrl.rootNode.ms.m_productItemCount)
        {
            count -= graph_ctrl.rootNode.ms.m_productItemCount;
        }
        this.UpdateInputCount(count);
    }


    /// <summary>
    /// Raises the minimum button click event.
    /// </summary>
    void OnMinBtnClick(GameObject go)
    {
        UIGraphControl graph_ctrl = m_MiddleContent.graphCtrl;
        int count = graph_ctrl.GetMinCount();
        this.UpdateInputCount(count);
    }

    /// <summary>
    /// Raises the max button click event.
    /// </summary>
    void OnMaxBtnClick(GameObject go)
    {
        UIGraphControl graph_ctrl = m_MiddleContent.graphCtrl;
        int count = graph_ctrl.GetMaxCount();
        this.UpdateInputCount(count);
    }

    void OnCompoundBtnClick(GameObject go)
    {
        if (onCompoundBtnClick != null)
            onCompoundBtnClick(go);
    }

    #endregion


    #region UNITY_INNER_FUNC

    void Awake()
    {
        UIEventListener el = null;

        // Menu check box 
        for (int i = 0; i < m_MenuContent.menuCBs.Count; i++)
        {
            el = UIEventListener.Get(m_MenuContent.menuCBs[i].gameObject);
            el.onClick = OnMenuCBClick;
        }

        // Query left button 
        el = UIEventListener.Get(m_MiddleContent.queryLeftBtn.gameObject);
        el.onClick = OnQueryLeftBtnClick;

        // Query right button 
        el = UIEventListener.Get(m_MiddleContent.queryRightBtn.gameObject);
        el.onClick = OnQueryRightBtnClick;

        // Query search button
        el = UIEventListener.Get(m_MiddleContent.querySearchBtn.gameObject);
        el.onClick = OnQuerySearchBtnClick;

        // Query clear button
        el = UIEventListener.Get(m_MiddleContent.queryClearBtn.gameObject);
        el.onClick = OnQueryClearBtnClick;

        //		// Count Input event
        //		m_RightContent.countInput.onSubmit += OnCountInputSubmit;

        // count add button
        el = UIEventListener.Get(m_RightContent.addCountBtn.gameObject);
        el.onClick = OnAddBtnClick;

        // count minimum button
        el = UIEventListener.Get(m_RightContent.minCountBtn.gameObject);
        el.onClick = OnMinBtnClick;

        // Count max button
        el = UIEventListener.Get(m_RightContent.maxCountBtn.gameObject);
        el.onClick = OnMaxBtnClick;

        // count substract button
        el = UIEventListener.Get(m_RightContent.subCountBtn.gameObject);
        el.onClick = OnSubstractBtnClick;

        el = UIEventListener.Get(m_RightContent.compoundBtn.gameObject);
        el.onClick = OnCompoundBtnClick;

        el = UIEventListener.Get(m_RightContent.countInput.gameObject);
        el.onSelect = OnCountInputSelected;

        m_QueryDraggablePanel = m_MiddleContent.queryScrollBox.m_DraggablePanel.gameObject.GetComponent<UIDraggablePanel>();
        //m_QueryPanelTrans = m_MiddleContent.queryScrollBox.m_DraggablePanel.transform;
    }


    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        #region QUERY_LIST_DRAG

        UIDraggablePanel dp = m_QueryDraggablePanel;


        UIPanel mPanel = m_MiddleContent.queryScrollBox.m_DraggablePanel;

        Bounds b = dp.bounds;
        Vector2 bmin = b.min;
        Vector2 bmax = b.max;

        if (mPanel.clipping == UIDrawCall.Clipping.SoftClip)
        {
            Vector2 soft = mPanel.clipSoftness;
            bmin -= soft;
            bmax += soft;
        }

        if (bmax.x > bmin.x)
        {
            Vector4 clip = mPanel.clipRange;
            float extents = clip.z * 0.5f;
            float min = clip.x - extents - b.min.x;
            float max = b.max.x - extents - clip.x;

            m_MiddleContent.queryLeftBtn.isEnabled = min > 0 ? true : false;
            m_MiddleContent.queryRightBtn.isEnabled = max > 0 ? true : false;
        }
        else
        {
            m_MiddleContent.queryLeftBtn.isEnabled = false;
            m_MiddleContent.queryRightBtn.isEnabled = false;
        }

        // Query Scrolling Tween
        if (m_QueryScrolling)
        {
            float amountPerDelta = Mathf.Abs((m_MiddleContent.duration > 0f) ? 1f / m_MiddleContent.duration : 1000f);

            m_Factor += amountPerDelta * Time.deltaTime;

            if (m_Factor > 1.0f)
            {
                m_Factor = 1.0f;
                m_QueryScrolling = false;
            }

            //			float val = Mathf.Sin(0.5f * Mathf.PI * m_Factor);
            //			float val = 1f - Mathf.Sin(0.5f * Mathf.PI * (1f - m_Factor));
            const float pi2 = Mathf.PI * 2f;
            float val = m_Factor - Mathf.Sin(m_Factor * pi2) / pi2;


            Vector3 relative = new Vector3(m_TweenLen * Mathf.Clamp01(val - m_prvVal), 0, 0);
            dp.transform.localPosition += relative;
            Vector4 cr = mPanel.clipRange;
            cr.x -= relative.x;
            cr.y -= relative.y;
            mPanel.clipRange = cr;

            m_prvVal = val;
        }
        else
        {
            // Set
            if (m_Factor != 0)
            {
                Vector3 local_pos = dp.transform.localPosition;
                local_pos.x = Mathf.RoundToInt(local_pos.x);
                local_pos.y = Mathf.RoundToInt(local_pos.y);
                local_pos.z = Mathf.RoundToInt(local_pos.z);
                dp.transform.localPosition = local_pos;

                Vector4 cr = mPanel.clipRange;
                cr.x = Mathf.RoundToInt(cr.x);
                cr.y = Mathf.RoundToInt(cr.y);
                mPanel.clipRange = cr;
            }

            m_Factor = 0;
            m_prvVal = 0;

            m_TweenLen = 0;
        }

        #endregion
    }

    #endregion
}
