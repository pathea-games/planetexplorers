using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using SkillAsset;
using System.Text.RegularExpressions;
using System;
using Pathea;
using System.Linq;

public class UICompoundWndControl : UIBaseWnd
{
    public static Action OnShow;
    public UIPopupList mPopuplist;
    public UIScrollBar mListScrolBar;
    public Vector3 mQueryGridPos = new Vector3(30, -30, 0);
    public GameObject mListBox;
    public GameObject mListItemPrefab;
    public GameObject mListGrid;
    public GameObject mLeftQueryBtn;
    public GameObject mRightQueryBtn;
    public UIInput mQueryInput;
    public UICheckbox ckboxAll;
    public UIGraphControl mGraphCtrl;
    public UIGridItemCtrl mButtomGrid;
    public GameObject mGridItemPrefab;
    public GameObject mQureGridContent;
    public BoxCollider mBtnBackBc;
    public BoxCollider mBtnForwdBc;
    public UIInput mBottomCountLb;
    public UISlider mCompoundSlider;
    public N_ImageButton mBtnCompound;
    public UISprite mBtnCompoundSpr;
    public BoxCollider mBtnClearBc;
    public UISlicedSprite mBtnClearSp;
    public UILabel mLbGraphInfo;
    public Transform mScriptListParent;
    public UIScriptItem_N m_UIScriptItemPrefab;
    public int mScriptItemPaddingX = 30;
    public UIEfficientGrid m_LeftList;
    public UICheckbox ckItemTrack;

    //lz-2016.08.11 在打造中的时候要禁用这些按钮
    [SerializeField]
    private GameObject m_OpComponentParent;
    [SerializeField]
    private GameObject m_ProgressParent;
    [SerializeField]
    private UILabel m_ProgressLbl;
    [SerializeField]
    private UILabel m_AddToPackageLbl;


    public bool IsCompounding
    {
		get { return null != replicator ? null != replicator.runningReplicate : false; }
        set {
            this.UpdateComponentState();
            if (value &&isShow)
                GameUI.Instance.PlayCompoundAudioEffect();
            else
                GameUI.Instance.StopCompoundAudioEffect();
        }
    }

    private int CompoundFixedTimeCount = 0;
    private Queue<UIScriptItem_N> mScriptItemPool = new Queue<UIScriptItem_N>();
    private List<UIScriptItem_N> mCurScriptItemList = new List<UIScriptItem_N>();
    private List<int> mGraphResetList = new List<int>();
    private int mGraphResetListIndex = -1;
    private int mListSelectedIndex = -1;
    public UIEfficientGrid m_QueryList;
    private int mQueryListFristIndex = 0;
    private string m_AllStr = "";
    private ItemLabel.Root mRootType;//标识左边按钮的分类
    private int mItemType = 0;//标识上方下拉列表选项的分类
    private AudioController m_CompoundAudioCtrl;

	private Replicator mReplicator;
	private Replicator replicator
	{
		get
		{
			if(null == mReplicator)
			{
				if(null != MainPlayer.Instance.entity && null != MainPlayer.Instance.entity.replicatorCmpt)
				{
					mReplicator = MainPlayer.Instance.entity.replicatorCmpt.replicator;
					if(null != mReplicator)
						mReplicator.onReplicateEnd += OnEndRepulicate;
				}
			}
			return mReplicator;
		}
	}
    #region mono methods
    // Use this for initialization
    void Awake()
    {
        mQureGridContentPos_x = mQureGridContent.transform.localPosition.x;
        UIEventListener.Get(this.mBottomCountLb.gameObject).onSelect = this.OnCountInputSelected;
    }

    void Start()
    {
        InitWindow();
        m_AllStr = PELocalization.GetString(10055);  //lz-2016.07.08 “All” 的语言映射
        mRootType = ItemLabel.Root.all;
        AfterLeftMeunChecked();
    }

    // Update is called once per frame
    void Update()
    {
        ChangemListBoxPos_Z();
        if (IsMoveQureyGridContentPos)
            MoveQureyGridContentPos();
    }

    new void LateUpdate()
    {
		_updateLeftList = true;
		UpdatePoogressBar();
		UpdateCompundBtnState();
		UpdateClearBtnState();
    }

    #endregion 

    #region override methods
    public override void Show()
    {
        UpdateLeftList();
        //NewUpdateLeftList();
        
        //lz-2016.06.24 打开窗口的时候将合成进度置零
        mCompoundSlider.sliderValue = 0;

        replicator.eventor.Subscribe(UpdateLeftListEventHandler);

        base.Show();
        if (OnShow != null)
            OnShow.Invoke();

        if(IsCompounding)
		{
            GameUI.Instance.PlayCompoundAudioEffect();
			Replicator.RunningReplicate runningReplicate = replicator.runningReplicate;
			if(null != runningReplicate)
			{
				UpdateCurItemScriptList(runningReplicate.formula.productItemId);
				if(m_Formulas.ContainsKey(runningReplicate.formula.productItemId))				
				{
					List<Pathea.Replicator.KnownFormula> formulaList = m_Formulas[runningReplicate.formula.productItemId];
					for(int i = 0; i < formulaList.Count; ++i)
					{
						if(runningReplicate.formulaID == formulaList[i].id)
						{
							if(mCurScriptItemList.Count > i)
							{
								mCurScriptItemList[i].SelectItem(true);
								m_BackupScriptItem = mCurScriptItemList[i];
                                //UpdateCompoundCount(runningReplicate.formula.m_productItemCount * runningReplicate.count);
                                UpdateCompoundCount(runningReplicate.leftCount* runningReplicate.formula.m_productItemCount);
                                break;
							}
						}
					}
				}
				IsCompounding = true;
			}
		}
        GameUI.Instance.mItemsTrackWnd.ScriptTrackChanged += OnScriptTrackChanged;
    }

    protected override void OnClose()
    {
        replicator.eventor.Unsubscribe(UpdateLeftListEventHandler);
        GameUI.Instance.mItemsTrackWnd.ScriptTrackChanged -= OnScriptTrackChanged;
        base.OnClose();
    }

    protected override void OnHide()
    {
        base.OnHide();
        GameUI.Instance.StopCompoundAudioEffect();
    }

    public override void OnCreate()
    {
        base.OnCreate();
        m_LeftList.itemGoPool.Init();
    }

    protected override void InitWindow()
    {
        base.InitWindow();
        SelfWndType = UIEnum.WndType.Compound;
    }

    #endregion

    #region privae methods

    void UpdateLeftListEventHandler(object sender, Replicator.EventArg e)
    {
        _updateLeftList = true;
        UpdateLeftList();
    }

    void UpdateCompundBtnState()
    {
        if (mGraphCtrl.rootNode == null)
        {
            EnableBtnCompound(false);
            return;
        }

        bool enable = false;
        if (mGraphCtrl.isCanCreate() == true)
        {
            //if (GameConfig.IsMultiMode)
            //    enable = true;
            //else
            //{
            //    if (mGraphCtrl.rootNode.ms.workSpace == 0)
            //        enable = true;
            //    else
            //        enable = false;
            //}

            if (mGraphCtrl.rootNode.ms.workSpace == 0)
                enable = true;
            else
                enable = false;
        }

        //lz-2016.09.12 在合成中的时候不能禁用合成按钮
        EnableBtnCompound(IsCompounding?true:enable);
    }

    void EnableBtnCompound(bool enable)
    {
        if (enable == true && !mBtnCompound.isEnabled)
        {
            mBtnCompound.isEnabled = true;
        }
        else if (enable == false && mBtnCompound.isEnabled)
        {
            mBtnCompound.isEnabled = false;
        }
    }

    bool isActiveClearBtn = true;
    void UpdateClearBtnState()
    {
        if (isActiveClearBtn == false &&mQueryInput.text.Length > 0)
        {
            mBtnClearBc.enabled = true;
            mBtnClearSp.color = new Color(1, 1, 1, 1);
            isActiveClearBtn = true;
        }
        else if (isActiveClearBtn == true &&mQueryInput.text.Length == 0)
        {
            mBtnClearBc.enabled = false;
            mBtnClearSp.color = new Color(0.6f, 0.6f, 0.6f, 1);
            isActiveClearBtn = false;
        }
    }

    //private int ProgressCount = 0;
    void UpdatePoogressBar()
    {
		if (!IsCompounding) return;
        if (mGraphCtrl.rootNode == null)
        {
			OnEndRepulicate();
            return;
        }
		Replicator.RunningReplicate runningReplicate = replicator.runningReplicate;
		if(null != runningReplicate)
		{
			mCompoundSlider.sliderValue = runningReplicate.runningTime / runningReplicate.formula.timeNeed;
            float allCount = runningReplicate.requestCount* runningReplicate.formula.m_productItemCount;
            float allFinishCount = runningReplicate.leftCount* runningReplicate.formula.m_productItemCount;
            float curFinishCount = runningReplicate.finishCount * runningReplicate.formula.m_productItemCount;
            m_ProgressLbl.text = string.Format("{0}/{1}", allCount- allFinishCount, allCount);
            m_AddToPackageLbl.text = PELocalization.GetString(8000690) + (allCount- allFinishCount- curFinishCount);
        }
        //        ProgressCount++;
        //        float value = Convert.ToSingle(ProgressCount) / Convert.ToSingle(CompoundFixedTimeCount);
        //        if (ProgressCount == CompoundFixedTimeCount)
        //            GetCompoundItem();
    }

    void GetCompoundItem()
    {
        Pathea.Replicator r = replicator;
        // add root item and delete need item

        if (!r.HasEnoughPackage(mGraphCtrl.rootNode.GetItemID(), mGraphCtrl.rootNode.getCount))
        {
            IsCompounding = false;
            //ProgressCount = 0;
            CompoundFixedTimeCount = 0;
            mCompoundSlider.sliderValue = 0;

            MessageBox_N.ShowOkBox(PELocalization.GetString(8000050));
            return;
        }


        if (mGraphCtrl.isCanCreate())
        {
            if (GameConfig.IsMultiMode == true)
            {
                int num = mGraphCtrl.rootNode.getCount / mGraphCtrl.rootNode.ms.m_productItemCount;
                PlayerNetwork.mainPlayer.RequestMergeSkill(mGraphCtrl.rootNode.ms.id, num);
            }
            else
            {
                r.Run(mGraphCtrl.rootNode.ms.id, mGraphCtrl.rootNode.getCount / mGraphCtrl.rootNode.ms.m_productItemCount);

            }

            GameUI.Instance.mItemPackageCtrl.ResetItem();
            for (int i = 0; i < m_CurScriptMatIDs.Count; i++)
                MissionManager.Instance.ProcessCollectMissionByID(m_CurScriptMatIDs[i]);
        }


        IsCompounding = false;
        //ProgressCount = 0;
        CompoundFixedTimeCount = 0;
        mCompoundSlider.sliderValue = 0;
    }

    float mListBoxPos_Z = 0;
    void ChangemListBoxPos_Z()
    {
        if (mPopuplist.isOpen)
            mListBoxPos_Z = 2;
        else
            mListBoxPos_Z = 0;
        if (mListBox.transform.localPosition.z != mListBoxPos_Z)
        {
            Vector3 pos = mListBox.transform.localPosition;
            pos.z = mListBoxPos_Z;
            mListBox.transform.localPosition = pos;
        }
    }

    private void QueryGridItems(int m_id)
    {
        mQureGridContentPos_x = 30;
        mQueryListFristIndex = 0;

        mQureGridContent.transform.localPosition = mQueryGridPos;


        m_QueryFormula.Clear();
        Pathea.Replicator r = replicator;
        //lz-2016.10.20 避免数据错误报错
        if (null != r)
        {
            foreach (Pathea.Replicator.KnownFormula kf in r.knowFormulas)
            {
                if (null != kf)
                {
                    Pathea.Replicator.Formula f = kf.Get();
                    if (null != f && null != f.materials && f.materials.Count > 0)
                    {
                        for (int j = 0; j < f.materials.Count; j++)
                        {
                            if (f.materials[j].itemId == m_id)
                            {
                                m_QueryFormula.Add(f);
                                break;
                            }
                        }
                    }
                }
            }
        }

        m_QueryList.UpdateList(m_QueryFormula.Count, SetQueryListContent, ClearQueryListContent);
        UpdateQueryGridBtnState();
    }

    private bool ReDrawGraph(int itemID,int scirptIndex=0)
    {
        if (mGraphCtrl == null)
            return false;
        this.AddScriptItemData(itemID);
        if (!this.m_Formulas.ContainsKey(itemID)||scirptIndex >= this.m_Formulas[itemID].Count || scirptIndex < 0)
            return true;
        if (mRootType == ItemLabel.Root.ISO)
        {
            // Iso
        }
        else
        {
            Pathea.Replicator.KnownFormula knownFornula=this.m_Formulas[itemID][scirptIndex];
            Pathea.Replicator.Formula ms =knownFornula.Get();
            ItemProto item = this.m_ItemDataList.Find(a => a.id ==itemID);

            if (ms == null || item == null)
            {
                return false;
            }

            // 临时处理
            bool enable = (ms.workSpace != 0) ? true : false;
            mLbGraphInfo.enabled = enable;
            mLbGraphInfo.text = enable ? PELocalization.GetString(8000151) : "";

            mGraphCtrl.ClearGraph();

            int level_v = 0;

            UIGraphNode root = mGraphCtrl.AddGraphItem(level_v, null, ms, item.icon, "Icon");
            root.mTipCtrl.SetToolTipInfo(ListItemType.mItem, itemID);
            root.mCtrl.ItemClick += this.GraphItemOnClick;

            this.m_CurScriptMatIDs.Clear();
            for (int j = 0; j < ms.materials.Count; j++)
            {
                if (ms.materials[j].itemId != 0)
                {
                    this.m_CurScriptMatIDs.Add(ms.materials[j].itemId);
                    ItemProto item2 = ItemProto.GetItemData(ms.materials[j].itemId);
                    UIGraphNode node = mGraphCtrl.AddGraphItem(level_v, root, null, item2.icon, "Icon");
                    node.mTipCtrl.SetToolTipInfo(ListItemType.mItem, ms.materials[j].itemId);
                    node.mCtrl.ItemClick += this.GraphItemOnClick;
                }
            }

            UpdateItemsTrackState(ms);
        }
        mGraphCtrl.DrawGraph();
        return true;
    }

    private void UpdateComponentState()
    {
        if(IsCompounding)
        {
            mBtnCompoundSpr.spriteName = "Craft3";
            this.m_OpComponentParent.SetActive(false);
            this.m_ProgressParent.SetActive(true);
        }
        else
        {
            mBtnCompoundSpr.spriteName = "Craft1";
            this.m_OpComponentParent.SetActive(true);
            this.m_ProgressParent.SetActive(false);
        }
    }

    #endregion

    #region QUERY_LIST

    List<Pathea.Replicator.Formula> m_QueryFormula = new List<Pathea.Replicator.Formula>();
    void SetQueryListContent(int index, GameObject go)
    {
        UIGridItemCtrl grid = go.GetComponent<UIGridItemCtrl>();
        grid.mIndex = index;
        grid.SetToolTipInfo(ListItemType.mItem, m_QueryFormula[index].productItemId);
        grid.mItemClick -= GridListItemOnClick;
        grid.mItemClick += GridListItemOnClick;

        ItemProto item = ItemProto.GetItemData(m_QueryFormula[index].productItemId);
        grid.SetCotent(item.icon);

    }
    void ClearQueryListContent(GameObject go)
    {
        UIGridItemCtrl grid = go.GetComponent<UIGridItemCtrl>();
        if (grid == null)
            return;

        grid.mItemClick -= ListItemOnClick;
    }


    #endregion

    #region LEFT_LIST

    //lz-2016.08.08 因为多种脚本可以合成同一种Item，左边的列表用Item显示，选了Item然后再选使用哪个脚本
    private Dictionary<int, List<Pathea.Replicator.KnownFormula>> m_Formulas = new Dictionary<int, List<Pathea.Replicator.KnownFormula>>();
    private List<ItemProto> m_ItemDataList = new List<ItemProto>();

    //int[] m_ProductItemList = null;
    //List<int> m_NeedShowItem = new List<int>(1);

    void SetLeftListContent(int index, GameObject go)
    {
        UICompoundWndListItem item = go.GetComponent<UICompoundWndListItem>();
        if (item == null)
            return;
        if (index < 0 || index >= m_ItemDataList.Count)
            return;

        ItemProto data = m_ItemDataList[index];
        if (!this.m_Formulas.ContainsKey(data.id))
            return;

        //lz-2016.08.08 有任何可以合成这个Item的新脚本就标记为new
        bool newFlag=this.m_Formulas[data.id].Any(a=>a.flag==true);

        item.SetItem(data.GetName(), data.id,newFlag, data.icon, data.GetName(), index, ListItemType.mItem);
        item.SetSelectmState(false);
        //lz-2017.01.16 错误 #8503 item颜色不正确
        item.SetTextColor(Color.white);

        item.mItemClick -= ListItemOnClick;
        item.mItemClick += ListItemOnClick;
    }

    void ClearLeftListContent(GameObject go)
    {
        UICompoundWndListItem item = go.GetComponent<UICompoundWndListItem>();
        if (item == null)
            return;

        item.mItemClick -= ListItemOnClick;
    }

    void NewClearLeftListContent(GameObject go)
    {
        UICompoundWndListItem item = go.GetComponent<UICompoundWndListItem>();
        if (item == null)
            return;

        item.mItemClick -= ListItemOnClick;
    }

    #endregion

    public bool _updateLeftList = true;//使更新不过于太频繁

    //log:lz-2016.04.12: 这里加一个参数为了搜索和切换类型分开
    public void UpdateLeftList(bool useSearch=false)
    {
        if (!_updateLeftList)
            return;

        _updateLeftList = false;

        mListSelectedIndex = -1;

        string queryText = mQueryInput.text;

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
        //queryText = queryText.Replace(" ", "");

        mQueryInput.text = queryText;

        m_Formulas.Clear();
        m_ItemDataList.Clear();
        Pathea.Replicator r = replicator;

        Dictionary<ItemProto, List<Pathea.Replicator.KnownFormula>> itemDic = new Dictionary<ItemProto, List<Pathea.Replicator.KnownFormula>>();
        foreach (Pathea.Replicator.KnownFormula kf in r.knowFormulas)
        {
            if (kf == null)
            {
                continue;
            }

            Pathea.Replicator.Formula f = kf.Get();
            if (null == f)
            {
                continue;
            }

            ItemProto item = ItemProto.GetItemData(f.productItemId);
            if (item == null) continue;
            bool AddItem = false;
            if (mRootType == ItemLabel.Root.ISO)
            {
                // Iso 
            }
            else if (mRootType == ItemLabel.Root.all || (mRootType == item.rootItemLabel && (mItemType == 0 || mItemType == item.itemLabel)))
            {
                if (useSearch)
                {
                    if (QueryItem(queryText, item.GetName()))
                    {
                        AddItem = true;
                    }
                }
                else
                    AddItem = true;
            }

            if(AddItem)
            {
                //lz-2016.08.09 这里要用Any()不能用Container()
                if (!itemDic.Keys.Any(a=>a.id==item.id))
                    itemDic.Add(item, new List<Replicator.KnownFormula>());

                ItemProto itemKey = itemDic.Keys.First(a => a.id == item.id);
                if (!itemDic[itemKey].Any(a => a.id == kf.id))
                    itemDic[itemKey].Add(kf);
            }
        }

        //lz-2016.08.08根据Item的Sort字段来排序
        itemDic = itemDic.OrderBy(a => a.Key.sortLabel).ToDictionary(k => k.Key, v => v.Value);

        m_Formulas = itemDic.ToDictionary(k => k.Key.id, v => v.Value);
        m_ItemDataList = itemDic.Keys.ToList();
        m_LeftList.UpdateList(m_ItemDataList.Count, SetLeftListContent, ClearLeftListContent);
        mListScrolBar.scrollValue = 0;
    }

    private bool LockChange = false;
    void OnSelectionChange1(string selectedItemName)
    {
        if (LockChange)
            return;

        if (selectedItemName == m_AllStr)
            mItemType = 0;
        else
            mItemType = ItemLabel.GetItemTypeByName(selectedItemName);

        UpdateLeftList();
    }

    private void SetPopuplistItem(bool useScerch)
    {

        LockChange = true;
        mPopuplist.items.Clear();
        mPopuplist.items.Add(m_AllStr);


        if (mRootType == ItemLabel.Root.all)
        {
        }
        else
        {
            mPopuplist.items.AddRange(ItemLabel.GetDirectChildrenName((int)mRootType));
        }

        if (mPopuplist.items.Count > 0)
            mPopuplist.selection = mPopuplist.items[0];
        mItemType = 0;
        UpdateLeftList(useScerch);
        LockChange = false;
    }

    private void AfterLeftMeunChecked(bool useScerch=false)
    {
        SetPopuplistItem(useScerch);
    }

    // QueryGridList
    private bool IsMoveLeft = true;
    private bool IsMoveQureyGridContentPos = false;
    private float mQureGridContentPos_x = 0;

    private void MoveQureyGridContentPos()
    {
        float Pos_x = mQureGridContent.transform.localPosition.x;

        if (IsMoveLeft)
        {
            if (mQureGridContentPos_x < Pos_x)
            {

                mQureGridContent.transform.localPosition = Vector3.Lerp(mQureGridContent.transform.localPosition, new Vector3(mQureGridContentPos_x, -30, 0), 0.3f);
                if (Pos_x - mQureGridContentPos_x < 3)
                    mQureGridContent.transform.localPosition = new Vector3(mQureGridContentPos_x, -30, 0);

            }
            else
                IsMoveQureyGridContentPos = false;
        }
        else
        {

            if (mQureGridContentPos_x > Pos_x)
            {
                mQureGridContent.transform.localPosition = Vector3.Lerp(mQureGridContent.transform.localPosition, new Vector3(mQureGridContentPos_x, -30, 0), 0.3f);
                if (mQureGridContentPos_x - Pos_x < 3)
                    mQureGridContent.transform.localPosition = new Vector3(mQureGridContentPos_x, -30, 0);
            }
            else
                IsMoveQureyGridContentPos = false;
        }

        m_QueryList.repositionVisibleNow = true;
    }

    private void UpdateQueryGridBtnState()
    {
        //lz-2016.10.13  修改【错误 #4012】空对象bug
        if (null == m_QueryList|| null==m_QueryList.Gos)
            return;

        BoxCollider bc1 = mLeftQueryBtn.GetComponent<BoxCollider>();
        BoxCollider bc2 = mRightQueryBtn.GetComponent<BoxCollider>();

        UISlicedSprite spr1 = mLeftQueryBtn.GetComponentInChildren<UISlicedSprite>();
        UISlicedSprite spr2 = mRightQueryBtn.GetComponentInChildren<UISlicedSprite>();

        if (null == bc1 || null == bc2 || null == spr1 || null == spr2)
            return;

        int query_count = m_QueryList.Gos.Count;

        if (query_count <= 10)
        {
            bc1.enabled = false;
            spr1.color = new Color(0.6f, 0.6f, 0.6f, 1);

            bc2.enabled = false;
            spr2.color = new Color(0.6f, 0.6f, 0.6f, 1);
        }
        else
        {
            if (mQueryListFristIndex == 0)
            {
                bc1.enabled = false;
                spr1.color = new Color(0.6f, 0.6f, 0.6f, 1);

                bc2.enabled = true;
                spr2.color = new Color(1, 1, 1, 1);
            }
            else if ((query_count - mQueryListFristIndex) > 0 && (query_count - mQueryListFristIndex) <= 10)
            {
                bc1.enabled = true;
                spr1.color = new Color(1, 1, 1, 1);

                bc2.enabled = false;
                spr2.color = new Color(0.6f, 0.6f, 0.6f, 1);
            }
            else
            {
                bc1.enabled = true;
                spr1.color = new Color(1, 1, 1, 1);

                bc2.enabled = true;
                spr2.color = new Color(1, 1, 1, 1);
            }
        }

    }

    private void SetListBtnActive(bool isActive)
    {
        mLeftQueryBtn.SetActive(isActive);
        mRightQueryBtn.SetActive(isActive);
    }

    private bool QueryItem(string text, string ItemName)
    {
        if (text.Trim().Length == 0)
            return true;
        string mtext = mToLower(text);
        string mItemName = mToLower(ItemName);
        //lz-2016.11.10 正则玩家输入的会报错，改为互相匹配
        return mItemName.Contains(mtext)|| mtext.Contains(mItemName);
    }

    private string mToLower(string strs)
    {
        string str = strs;
        char[] ch = str.ToCharArray();
        System.Text.RegularExpressions.Regex R = new System.Text.RegularExpressions.Regex("[A-Z]");
        str = "";
        foreach (char s in ch)
        {
            if (R.IsMatch(s.ToString()))
            {
                str += s.ToString().ToLower();
            }
            else
            {
                str += s.ToString();
            }
        }
        return str;
    }

    private void SetBottomInfo()
    {
        UIGraphNode rootNode = mGraphCtrl.rootNode;
        if (rootNode.mCtrl.mContentSprites[0].gameObject.activeSelf)
        {
            int m_id = rootNode.GetItemID();
            ItemProto item = ItemProto.GetItemData(m_id);
            mButtomGrid.SetCotent(item.icon);
            mButtomGrid.SetToolTipInfo(ListItemType.mItem, m_id);
        }
        else
        {
            mButtomGrid.SetCotent(rootNode.mCtrl.mContentTexture.mainTexture);
            mButtomGrid.SetToolTipInfo(ListItemType.mItem, rootNode.GetItemID());
        }

        mBottomCountLb.text = rootNode.ms.m_productItemCount.ToString();
    }

    private void AddGraphResetList(int m_id)
    {

        if (mGraphResetList.Count > 10)
            mGraphResetList.RemoveAt(0);

        mGraphResetListIndex = mGraphResetList.Count;
        mGraphResetList.Add(m_id);

        if (mGraphResetList.Count > 1)
        {
            mBtnBackBc.enabled = true;
            mBtnForwdBc.enabled = false;
        }
    }


    #region Item Track

    void OnScriptTrackChanged(int scriptID,bool add)
    {
        if (mGraphCtrl.rootNode != null && mGraphCtrl.rootNode.ms != null && mGraphCtrl.rootNode.ms.id == scriptID)
        {
            ckItemTrack.isChecked = add;
        }
    }

    void UpdateItemsTrackState(Replicator.Formula ms)
    {
        bool containe = GameUI.Instance.mItemsTrackWnd.ContainsScript(ms.id);
        ckItemTrack.isChecked = containe;
    }

    void OnItemTrackCk(bool isChecked)
    {
        if (mGraphCtrl.rootNode == null || mGraphCtrl.rootNode.ms == null) return;
        if (isChecked)
        {
            int k = mGraphCtrl.rootNode.getCount / mGraphCtrl.rootNode.ms.m_productItemCount;
            GameUI.Instance.mItemsTrackWnd.UpdateOrAddScript(mGraphCtrl.rootNode.ms, k);
        }
        else
        {
            GameUI.Instance.mItemsTrackWnd.RemoveScript(mGraphCtrl.rootNode.ms.id);
        }
    }

    #endregion

    #region on click funcs

    private void Ck0AllOnClick()
    {
        //bool ok = Input.GetMouseButtonUp(0);

        if (Input.GetMouseButtonUp(0))
        {
            if (mRootType != ItemLabel.Root.all)
            {
                mRootType = ItemLabel.Root.all;
                AfterLeftMeunChecked();
            }
        }
    }

    private void Ck1WeaponOnClick()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (mRootType != ItemLabel.Root.weapon)
            {
                mRootType = ItemLabel.Root.weapon;
                AfterLeftMeunChecked();
            }
        }
    }

    private void Ck2EquipmentOnClick()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (mRootType != ItemLabel.Root.equipment)
            {
                mRootType = ItemLabel.Root.equipment;
                AfterLeftMeunChecked();
            }
        }
    }

    private void Ck3ToolOnClick()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (mRootType != ItemLabel.Root.tool)
            {
                mRootType = ItemLabel.Root.tool;
                AfterLeftMeunChecked();
            }
        }
    }

    private void Ck4TurretOnClick()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (mRootType != ItemLabel.Root.turret)
            {
                mRootType = ItemLabel.Root.turret;
                AfterLeftMeunChecked();
            }
        }

    }

    private void Ck5ConsumablesOnClick()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (mRootType != ItemLabel.Root.consumables)
            {
                mRootType = ItemLabel.Root.consumables;
                AfterLeftMeunChecked();
            }
        }

    }

    private void Ck6ResoureOnClick()
    {
        if (Input.GetMouseButtonUp(0))
        {

            if (mRootType != ItemLabel.Root.resoure)
            {
                mRootType = ItemLabel.Root.resoure;
                AfterLeftMeunChecked();
            }
        }
    }

    private void Ck7PartOnClick()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (mRootType != ItemLabel.Root.part)
            {
                mRootType = ItemLabel.Root.part;
                AfterLeftMeunChecked();
            }
        }
    }

    private void Ck8DecorationOnClick()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (mRootType != ItemLabel.Root.decoration)
            {
                mRootType = ItemLabel.Root.decoration;
                AfterLeftMeunChecked();
            }
        }
    }

    private void Ck9IsoOnClick()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (mRootType != ItemLabel.Root.ISO)
            {
                mRootType = ItemLabel.Root.ISO;
                AfterLeftMeunChecked();
            }
        }
    }

    private void ListItemOnClick(int index)
    {
        if (IsCompounding)
            return;
        if (index < 0 || index >= m_ItemDataList.Count)
            return;
        List<GameObject> items = m_LeftList.Gos;

        if (mListSelectedIndex != -1 && mListSelectedIndex < items.Count)
        {
            UICompoundWndListItem item = items[mListSelectedIndex].GetComponent<UICompoundWndListItem>();
            item.SetSelectmState(false);
        }
        if (index < items.Count)
        {
            UICompoundWndListItem item = items[index].GetComponent<UICompoundWndListItem>();
            item.SetSelectmState(true);
        }
        mListSelectedIndex = index;
        this.UpdateCurItemScriptList(this.m_ItemDataList[index].id);
        this.SelectFirstScritItem();
    }

    private void NewListItemOnClick(int index)
    {
        //ItemProto data = ItemProto.GetItemData(m_NeedShowItem[index]);
        //Pathea.Replicator.KnownFormula[] kfs = replicator.GetKnowFormulasByProductItemId(m_NeedShowItem[index]);


    }

    private void GraphItemOnClick(int index)
    {
        if (index == -1)
            return;

        if (IsCompounding)
            return;

        int m_id = mGraphCtrl.mGraphItemList[index].GetItemID();
        if (ReDrawGraph(m_id,0))
        {
            AddGraphResetList(m_id);
            SetBottomInfo();
        }

        else
        {
            if (mGraphCtrl.mSelectedIndex != -1)
                mGraphCtrl.mGraphItemList[mGraphCtrl.mSelectedIndex].mCtrl.SetSelected(false);
            mGraphCtrl.mGraphItemList[index].mCtrl.SetSelected(true);
            mGraphCtrl.mSelectedIndex = index;
        }

        QueryGridItems(m_id);
    }

    private void OnQueryBtnOnClick()
    {
        string queryText = mQueryInput.text;
        if (queryText.Length > 0)
        {
            //ckboxAll.isChecked = true;
            //log:lz-2016.04.12 搜索的时候在当前选择类型中搜索
            //mRootType = ItemLabel.Root.all;
            AfterLeftMeunChecked(true);
        }
        else
        {
            mQueryInput.text = string.Empty;
            UpdateLeftList();
        }
    }

    private void ClearBtnOnClick()
    {
        if (mQueryInput.text.Length > 0)
        {
            mQueryInput.text = string.Empty;
            UpdateLeftList();
            //NewUpdateLeftList();
        }
    }

    private void BtnLeftOnClick()
    {
        //		if(mQueryList.Count <= 10)
        //			return;
        if (m_QueryList.Gos.Count <= 10)
            return;

        if (IsMoveQureyGridContentPos == false)
        {
            mQureGridContentPos_x = mQureGridContentPos_x + 520;
            IsMoveLeft = false;
            mQueryListFristIndex -= 10;

            IsMoveQureyGridContentPos = true;
            UpdateQueryGridBtnState();
        }

    }

    private void BtnRightOnClick()
    {
        //		if(mQueryList.Count <= 1)
        //			return;
        if (m_QueryList.Gos.Count <= 1)
            return;

        if (IsMoveQureyGridContentPos == false)
        {
            mQureGridContentPos_x = mQureGridContentPos_x - 520;
            IsMoveLeft = true;
            mQueryListFristIndex += 10;

            IsMoveQureyGridContentPos = true;
            UpdateQueryGridBtnState();
        }
    }

    private void GridListItemOnClick(int index)
    {
        if (index == -1)
            return;

        if (IsCompounding)
            return;

        int m_id = m_QueryList.Gos[index].GetComponent<UIGridItemCtrl>().mItemId;

        if (ReDrawGraph(m_id,0))
        {
            AddGraphResetList(m_id);
            SetBottomInfo();
            QueryGridItems(m_id);
        }


    }

    private void GraphBtnBackOnClick()
    {
        if (IsCompounding)
            return;

        if (mGraphResetListIndex > 0)
        {
            mGraphResetListIndex--;
            ReDrawGraph(mGraphResetList[mGraphResetListIndex],0);

            if (mGraphResetListIndex == 0)
                mBtnBackBc.enabled = false;
            mBtnForwdBc.enabled = true;
        }

    }

    private void GraphBtnForwdOnClick()
    {
        if (IsCompounding)
            return;

        if (mGraphResetListIndex < 0)
            return;

        if (mGraphResetListIndex < mGraphResetList.Count - 1)
        {
            mGraphResetListIndex++;
            ReDrawGraph(mGraphResetList[mGraphResetListIndex],0);
            if (mGraphResetListIndex == mGraphResetList.Count - 1)
                mBtnForwdBc.enabled = false;
            mBtnBackBc.enabled = true;
        }
    }

    private void BtnAddOnClick()
    {
        if (IsCompounding)
            return;
        if (mGraphCtrl.rootNode == null)
            return;
        if (!IsNumber(mBottomCountLb.text))
            return;

        int n = Convert.ToInt32(mBottomCountLb.text);

        //lz-2016.08.10 限制最大数量
        if (n >= mGraphCtrl.GetMaxCount())
            return;

        if (n + mGraphCtrl.rootNode.ms.m_productItemCount <= mGraphCtrl.GetMaxCount())
        {
            n += mGraphCtrl.rootNode.ms.m_productItemCount;
        }

        this.UpdateCompoundCount(n);
    }

    private void OnSubstractBtnClick()
    {
        if (IsCompounding) 
            return;
        if (mGraphCtrl.rootNode == null)
            return;
        if (!IsNumber(mBottomCountLb.text))
            return;

        int n = Convert.ToInt32(mBottomCountLb.text);
        if (n > mGraphCtrl.rootNode.ms.m_productItemCount)
        {
            n -= mGraphCtrl.rootNode.ms.m_productItemCount;
        }
        this.UpdateCompoundCount(n);
    }
	
	void OnEndRepulicate()
	{
		IsCompounding = false;		
		//ProgressCount = 0;
		CompoundFixedTimeCount = 0;
		mCompoundSlider.sliderValue = 0;
        m_ProgressLbl.text = "";
        m_AddToPackageLbl.text = "";
    }

    private void OnCountInputSelected(GameObject go, bool isSelect)
    {
        //lz-2016.08.12 当取消选中的时候更改值
        if (isSelect) return;
        if (IsCompounding)
            return;
        if (mGraphCtrl.rootNode == null)
            return;
        if (!IsNumber(mBottomCountLb.text))
            return;
        string inputText = mBottomCountLb.text;

        if (inputText.Trim().Length == 0)
        {
            mBottomCountLb.text = "";
            return;
        }
        int count = Convert.ToInt32(inputText);
        //lz-2016.08.10 输入的数量限制在范围内
        count = Mathf.Clamp(count, mGraphCtrl.rootNode.ms.m_productItemCount, mGraphCtrl.GetMaxCount());
        this.UpdateCompoundCount(count,false);
    }

    private void BtnMaxOnClick()
    {
        if (IsCompounding)
            return;
        if (mGraphCtrl.rootNode == null)
            return;
        this.UpdateCompoundCount(mGraphCtrl.GetMaxCount());
    }

    private void BtnMinOnClick()
    {
        if (IsCompounding)
            return;
        if (mGraphCtrl.rootNode == null)
            return;
        this.UpdateCompoundCount(mGraphCtrl.GetMinCount());
    }

    //lz-2016.08.10 更新合成数量
    private void UpdateCompoundCount(int count, bool immediateUpdateInputTxet=true)
    {
        if (null == mGraphCtrl.rootNode)
            return;

        if (count > 9999)
            count = 9999;
        int m = count % mGraphCtrl.rootNode.ms.m_productItemCount;
        if (m != 0)
        {
            count = count - m + mGraphCtrl.rootNode.ms.m_productItemCount;
        }

        if (count > 9999)
            count = count - mGraphCtrl.rootNode.ms.m_productItemCount;
        mGraphCtrl.SetGraphCount(count);
        if (immediateUpdateInputTxet)
            mBottomCountLb.text = count.ToString();
        else
            StartCoroutine(this.WiatUpdateInputIterator(count));

    }

    //lz-2016.08.12 延迟一帧更新是因为不能在UIInput的OnSelect事件里面设置UIInput.text,不然会出现CaratChar遗留在输入框的bug
    private IEnumerator WiatUpdateInputIterator(int count)
    {
        yield return null;
        mBottomCountLb.text = count.ToString();
    }

    private void BtnCompoundOnClick()
    {
        if (mGraphCtrl.rootNode == null)
            return;

        Pathea.Replicator r = replicator;
        if (null == r)
        {
            return;
        }

        if (IsCompounding)
        {
            //lz-2016.09.09 如果在制造中就取消制造
            IsCompounding = !replicator.CancelReplicate(mGraphCtrl.rootNode.ms.id);
            return;
        }

        if (!r.HasEnoughPackage(mGraphCtrl.rootNode.GetItemID(), mGraphCtrl.rootNode.getCount))
        {
            MessageBox_N.ShowOkBox(PELocalization.GetString(8000050));
            return;
        }

        if (mGraphCtrl.isCanCreate())
        {
            CompoundFixedTimeCount = Convert.ToInt32(mGraphCtrl.rootNode.ms.timeNeed / 0.02);
            if (RandomMapConfig.useSkillTree)
            {
                if (GameUI.Instance.mSkillWndCtrl._SkillMgr != null)
                {
                    CompoundFixedTimeCount = (int)GameUI.Instance.mSkillWndCtrl._SkillMgr.CheckReduceTime((float)CompoundFixedTimeCount);
                }
            }
			replicator.StartReplicate(mGraphCtrl.rootNode.ms.id, mGraphCtrl.rootNode.getCount / mGraphCtrl.rootNode.ms.m_productItemCount);
            IsCompounding = true;
        }
    }

    #endregion

    public bool IsNumber(String strNumber)
    {
        Regex objNotNumberPattern = new Regex("[^0-9.-]");
        Regex objTwoDotPattern = new Regex("[0-9]*[.][0-9]*[.][0-9]*");
        Regex objTwoMinusPattern = new Regex("[0-9]*[-][0-9]*[-][0-9]*");
        String strValidRealPattern = "^([-]|[.]|[-.]|[0-9])[0-9]*[.]*[0-9]+$";
        String strValidIntegerPattern = "^([-]|[0-9])[0-9]*$";
        Regex objNumberPattern = new Regex("(" + strValidRealPattern + ")|(" + strValidIntegerPattern + ")");
        return !objNotNumberPattern.IsMatch(strNumber) &&
            !objTwoDotPattern.IsMatch(strNumber) &&
                !objTwoMinusPattern.IsMatch(strNumber) &&
                objNumberPattern.IsMatch(strNumber);
    }

    #region ScriptItem methods

    private UIScriptItem_N m_BackupScriptItem;
    private int m_CurItemID;
    private List<int> m_CurScriptMatIDs=new List<int> ();

    //lz-2016.08.08 更新可以合成当前选择的Item的脚本列表
    private void UpdateCurItemScriptList(int itemID)
    {
        this.m_BackupScriptItem = null;
        this.RecoveryScriptItem();
        if (!this.m_Formulas.ContainsKey(itemID))
            return;
        this.m_CurItemID = itemID;
        int scriptCount = this.m_Formulas[itemID].Count;

        for (int i = 0; i < scriptCount; i++)
        {
            UIScriptItem_N item = GetNewScriptItem();
            item.UpdateInfo(itemID, i);
            item.SelectEvent = (scriptItem) =>
            {
                if (item != this.m_BackupScriptItem)
                {
                    this.ScriptItemEvent(scriptItem.ItemID, scriptItem.ScriptIndex);
                    if (null != this.m_BackupScriptItem)
                        this.m_BackupScriptItem.CanSelectItem();
                    this.m_BackupScriptItem = item;
                }
            };
            item.transform.localPosition = new Vector3(i* this.mScriptItemPaddingX, 0, 0);
            this.mCurScriptItemList.Add(item);
			if(scriptCount==1)
				item.gameObject.SetActive(false);
        }
    }

    //lz-2016.08.08 回收当前显示的ScriptItem
    private void RecoveryScriptItem()
    {
        if (this.mCurScriptItemList.Count > 0)
        {
            for (int i = 0; i < this.mCurScriptItemList.Count; i++)
            {
                this.mCurScriptItemList[i].Reset();
                this.mCurScriptItemList[i].gameObject.SetActive(false);
                this.mScriptItemPool.Enqueue(this.mCurScriptItemList[i]);
            }
            this.mCurScriptItemList.Clear();
        }
    }

    //lz-2016.08.08 点击ScriptItem事件
    private void ScriptItemEvent(int itemID, int scriptIndex)
    {
        if (!this.m_Formulas.ContainsKey(itemID) || scriptIndex >= this.m_Formulas[itemID].Count || scriptIndex < 0)
            return;

        //更改new的状态
        Pathea.Replicator.KnownFormula knownFornula = this.m_Formulas[itemID][scriptIndex];
        Pathea.Replicator r = replicator;
        if (null != r)
        {
            r.SetKnownFormulaFlag(knownFornula.id);
        }


        //颜色根据选的脚本改变
        List<GameObject> ListItems = m_LeftList.Gos;
        if (mListSelectedIndex >= 0 && mListSelectedIndex < ListItems.Count)
        {

            Pathea.Replicator.Formula formula = knownFornula.Get();
            bool isInColony = (formula.workSpace != 0) ? true : false;
            Color textColor = isInColony ? Color.red : Color.white;
            UICompoundWndListItem listItem = ListItems[mListSelectedIndex].GetComponent<UICompoundWndListItem>();
            listItem.SetTextColor(textColor);
        }

        //绘制当前选择的脚本和Item
        bool ok = ReDrawGraph(itemID, scriptIndex);
        if (ok)
        {
            AddGraphResetList(itemID);
            SetBottomInfo();
            QueryGridItems(itemID);
        }

    }

    //lz-2016.08.08 获取一个新的ScirptItem
    private UIScriptItem_N GetNewScriptItem()
    {
        UIScriptItem_N item = null;
        if (this.mScriptItemPool.Count > 0)
        {
            item = this.mScriptItemPool.Dequeue();
            item.gameObject.SetActive(true);
        }
        else
        {
            GameObject Go = GameObject.Instantiate(this.m_UIScriptItemPrefab.gameObject);
            Go.transform.parent = this.mScriptListParent;
            Go.transform.localPosition = Vector3.zero;
            Go.transform.localScale = Vector3.one;
            Go.transform.localRotation = Quaternion.identity;
            item = Go.GetComponent<UIScriptItem_N>();
        }
        return item;
    }

    //lz-2016.08.08 通过ItemID找到所有可以合成这个Item的脚本
    private void AddScriptItemData(int itemID)
    {
        if (!this.m_Formulas.ContainsKey(itemID))
        {
            List<Pathea.Replicator.Formula> formulaList = Pathea.Replicator.Formula.Mgr.Instance.FindAllByProDuctID(itemID);
            if (null == formulaList || formulaList.Count <= 0) return;
            List<Pathea.Replicator.KnownFormula> knownFormulaList = new List<Replicator.KnownFormula>();
            for (int i = 0; i < formulaList.Count; i++)
            {
                Pathea.Replicator.KnownFormula knownFormula = replicator.GetKnownFormula(formulaList[i].id);
				if (null == knownFormula && 193 != formulaList[i].id && 520 != formulaList[i].id)
				{
					replicator.AddFormula(formulaList[i].id);
					knownFormula = replicator.GetKnownFormula(formulaList[i].id);
				}
				if(null != knownFormula)
	                knownFormulaList.Add(knownFormula);
            }
            ItemProto item = ItemProto.GetItemData(itemID);
            this.m_ItemDataList.Add(item);
            this.m_Formulas[itemID] = knownFormulaList;
        }
        if (this.m_Formulas.ContainsKey(itemID) && itemID != this.m_CurItemID)
        {
            this.UpdateCurItemScriptList(itemID);
            this.SelectFirstScritItem(false);
        }
    }

    //lz-2016.08.09 选中第一个ScriptItem
    private void SelectFirstScritItem(bool execEvent=true)
    {
        if (this.mCurScriptItemList.Count > 0)
        {
            this.mCurScriptItemList[0].SelectItem(execEvent);
            this.m_BackupScriptItem = this.mCurScriptItemList[0];
        }
    }
    #endregion

}