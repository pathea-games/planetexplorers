using CustomData;
using ItemAsset;
using ItemAsset.PackageHelper;
using System.Collections.Generic;
using UnityEngine;
using Pathea;
using System.Text.RegularExpressions;
using System.Linq;

public class CSUI_Factory : MonoBehaviour
{
    public CSFactory m_Factory;
    public CSEntity m_Entity;

    public void SetEntity(CSEntity enti)
    {
        if (enti == null)
        {
            Debug.LogWarning("Reference Entity is null.");
            return;
        }

        m_Factory = enti as CSFactory;
        if (m_Factory == null)
        {
            Debug.LogWarning("Reference Entity is not a Assembly Entity.");
            return;
        }
        m_Entity = enti;
        CSUI_MainWndCtrl.Instance.mSelectedEnntity = m_Entity;

    }

    private ItemLabel.Root m_RootType;
    private int m_ItemType;

    // Menu list item
    //private List<UICompoundWndListItem> m_MenuItems = new List<UICompoundWndListItem>();



    #region UI_CONTENT

    public CSUI_FactoryReplicator FactoryReplicator;
    private Replicator m_Replicator
    {
        get
        {
            return UIGraphControl.GetReplicator();
        }
    }

    public CSUI_CompoundItem m_CompoundItemPrefab;
    public UIGrid m_CompoundItemRoot;
    public Transform mScriptListParent;
    public UIScriptItem_N m_UIScriptItemPrefab;
    public int mScriptItemPaddingX = 30;
    public UICheckbox ckItemTrack;

    private Queue<UIScriptItem_N> mScriptItemPool = new Queue<UIScriptItem_N>();
    private List<UIScriptItem_N> mCurScriptItemList = new List<UIScriptItem_N>();
    private int mListSelectedIndex;
    private string m_AllStr = "";

    #endregion


    #region QUEUE_CONTENT

    List<CSUI_CompoundItem> m_CompoudItems = new List<CSUI_CompoundItem>();

    CSUI_CompoundItem _createCompoundItem()
    {
        CSUI_CompoundItem ci = Instantiate(m_CompoundItemPrefab) as CSUI_CompoundItem;
        ci.transform.parent = m_CompoundItemRoot.transform;

        ci.transform.localPosition = Vector3.zero;
        ci.transform.localRotation = Quaternion.identity;
        ci.transform.localScale = Vector3.one;
        ci.gameObject.GetComponent<UICheckbox>().radioButtonRoot = m_CompoundItemRoot.transform;
        ci.Count = 0;
        ci.SliderValue = 0;
        ci.RightBtnClickEvent = OnCompoudClick;
        ci.onDeleteBtnClick += OnDeleteBtnClick;

        m_CompoudItems.Add(ci);
        m_CompoundItemRoot.repositionNow = true;
        return ci;
    }

    // ---------------------------------
    //  Callback
    // ---------------------------------

    void OnCompoudClick(CSUI_CompoundItem ci)
    {
        if (!GameConfig.IsMultiMode)
        {
            int index = m_CompoudItems.FindIndex(item0 => item0 == ci);
            if (index != -1)
            {
                if (!CSUI_MainWndCtrl.IsWorking())
                    return;

                if (m_Factory.CompoudItemsCount <= index)
                    return;

                Pathea.PlayerPackageCmpt pkg = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.PlayerPackageCmpt>();
				if(pkg==null){
					Debug.LogError("CSUI_Factory.OnCompoundClick: pkg is null!");
					return;
				}

                CompoudItem ci_data = null;
				ItemProto item_data;
				if(!m_Factory.GetTakeAwayCompoundItem(index, out ci_data)){
					if(ci_data!=null){
						item_data = ItemProto.GetItemData(ci_data.itemID);
						CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mIsCompounding.GetString(), item_data.GetName()), Color.red);
					}
					return;
				}
					
				item_data = ItemProto.GetItemData(ci_data.itemID);

				int pacakgeEmptyCount  = pkg.package.GetSlotList(ci_data.itemID).vacancyCount;
                if (!pkg.package.CanAdd(m_Factory.Data.m_CompoudItems[index].itemID, ci.Count))
                {
                    // Status bar
                    //lz-2016.07.16 背包空间不足这条提示是基地复制器和玩家复制器通用的，在基地用的时候，基地的提示只能显示一行，不能换行
                    string tip = PELocalization.GetString(8000050).Replace("\\n", " ");
                    CSUI_MainWndCtrl.ShowStatusBar(tip, Color.red);
					if(item_data.maxStackNum>1||pacakgeEmptyCount==0){
						return;
					}
                }

				int originalCount = ci_data.itemCnt;
				int addCount = originalCount;
				if(item_data.maxStackNum>1){
					pkg.Add(ci_data.itemID,ci_data.itemCnt);
				}else{
					if(originalCount>pacakgeEmptyCount){
						addCount = pacakgeEmptyCount;
					}
					pkg.Add(ci_data.itemID,addCount);
				}

				ci_data.itemCnt=originalCount-addCount;
				if(ci_data.itemCnt==0){
					m_Factory.TakeAwayCompoudItem(index);
                	CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mTakeAwayCompoundItem.GetString(), item_data.GetName()));
				}
                    
            }
        }
        else
        {
            int index = m_CompoudItems.FindIndex(item0 => item0 == ci);
            if (index != -1)
            {
                if (m_Factory.CompoudItemsCount <= index
                    || m_Factory.Data.m_CompoudItems[index] == null
                    || m_Factory.Data.m_CompoudItems[index].curTime < m_Factory.Data.m_CompoudItems[index].time)
                    return;
                m_Factory._ColonyObj._Network.FCT_Fetch(index);
            }
        }


    }

    void OnDeleteBtnClick(CSUI_CompoundItem ci)
    {
        if (ci == null)
            return;
        if (m_Factory == null)
            return;
        int index = m_CompoudItems.FindIndex(item0 => item0 == ci);
        if (index != -1)
        {
            m_Factory.OnCancelCompound(index);
        }
    }

    #endregion



    #region MENU_ABOUT_FUNC

    // ---------------------------------
    //  Func
    // ---------------------------------

    void SetPopuplistItem()
    {
        UIPopupList popupList = FactoryReplicator.m_MenuContent.popList;
        popupList.items.Clear();
        popupList.items.Add(this.m_AllStr);

        if (m_RootType == ItemLabel.Root.all)
        {
        }
        else
        {
            popupList.items.AddRange(ItemLabel.GetDirectChildrenName((int)m_RootType));

            //foreach (KeyValuePair<int, int> kv in ItemType.s_tblItemFather)
            //{
            //    if (kv.Value != -1 && (int)m_RootType == kv.Value)
            //    {
            //        popupList.items.Add(ItemType.s_tblItemType[kv.Key]);
            //    }
            //}
        }

        if (popupList.items.Count > 0)
            popupList.selection = popupList.items[0];
        UpdateLeftList(true);
    }

    void UpdateLeftListEventHandler(object sender, Replicator.EventArg e)
    {
        UpdateLeftList();
    }

    void UpdateLeftList(bool useSearch=false)
    {
        if (null == m_Replicator)
        {
            return;
        }
                
        string queryText = FactoryReplicator.GetQueryString();

        m_Formulas.Clear();
        m_ItemDataList.Clear();

        Dictionary<ItemProto, List<Pathea.Replicator.KnownFormula>> itemDic = new Dictionary<ItemProto, List<Pathea.Replicator.KnownFormula>>();
        foreach (Pathea.Replicator.KnownFormula kf in m_Replicator.knowFormulas)
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
            if (m_RootType == ItemLabel.Root.ISO)
            {
                // Iso 
            }
            else if (m_RootType == ItemLabel.Root.all || (m_RootType == item.rootItemLabel && (m_ItemType == 0 || m_ItemType == item.itemLabel)))
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

            if (AddItem)
            {
                //lz-2016.08.09 这里要用Any()不能用Container()
                if (!itemDic.Keys.Any(a => a.id == item.id))
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
        FactoryReplicator.m_MenuContent.gridList.UpdateList(m_ItemDataList.Count, SetMenuListItemContent, ClearMenuListItemContent);

        FactoryReplicator.m_MenuContent.scrollBox.m_VertScrollBar.scrollValue = 0;
    }

    private bool QueryItem(string text, string ItemName)
    {
        if (text.Trim().Length == 0)
            return true;
        string mtext = mToLower(text);
        string mItemName = mToLower(ItemName);

        //lz-2016.11.10 正则玩家输入的会报错，改为互相匹配
        return mItemName.Contains(mtext) || mtext.Contains(mItemName);
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

    //lz-2016.08.08 因为多种脚本可以合成同一种Item，左边的列表用Item显示，选了Item然后再选使用哪个脚本
    private Dictionary<int, List<Pathea.Replicator.KnownFormula>> m_Formulas = new Dictionary<int, List<Pathea.Replicator.KnownFormula>>();
    private List<ItemProto> m_ItemDataList = new List<ItemProto>();

    void SetMenuListItemContent(int index, GameObject go)
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
        bool newFlag = this.m_Formulas[data.id].Any(a => a.flag == true);

		item.SetItem(data.name, data.id, newFlag, data.icon, data.GetName(), index, ListItemType.mItem);
        item.SetSelectmState(false);

        item.mItemClick -= OnMenuListItemClick;
        item.mItemClick += OnMenuListItemClick;
    }

    void ClearMenuListItemContent(GameObject go)
    {
        UICompoundWndListItem cwl = go.GetComponent<UICompoundWndListItem>();
        if (cwl == null)
            return;

        cwl.mItemClick -= OnMenuListItemClick;

    }

    // ---------------------------------
    //  Call back
    // ---------------------------------

    // Replicator --- Menu
    void OnLeftMenuClick(GameObject go, int index)
    {
        if (go == null)
        {
            Debug.LogError("Gamobject is miss for Replicator menu");
            return;
        }

        bool ok = Input.GetMouseButtonUp(0);
        if (ok)
        {
            switch (index)
            {
                case 0:
                    {
                        if (m_RootType != ItemLabel.Root.all)
                        {
                            m_RootType = ItemLabel.Root.all;
                            SetPopuplistItem();
                        }
                    } break;
                case 1:
                    {
                        if (m_RootType != ItemLabel.Root.weapon)
                        {
                            m_RootType = ItemLabel.Root.weapon;
                            SetPopuplistItem();
                        }
                    } break;
                case 2:
                    {
                        if (m_RootType != ItemLabel.Root.equipment)
                        {
                            m_RootType = ItemLabel.Root.equipment;
                            SetPopuplistItem();
                        }
                    } break;
                case 3:
                    {
                        if (m_RootType != ItemLabel.Root.tool)
                        {
                            m_RootType = ItemLabel.Root.tool;
                            SetPopuplistItem();
                        }
                    } break;
                case 4:
                    {
                        if (m_RootType != ItemLabel.Root.turret)
                        {
                            m_RootType = ItemLabel.Root.turret;
                            SetPopuplistItem();
                        }
                    } break;
                case 5:
                    {
                        if (m_RootType != ItemLabel.Root.consumables)
                        {
                            m_RootType = ItemLabel.Root.consumables;
                            SetPopuplistItem();
                        }
                    } break;
                case 6:
                    {
                        if (m_RootType != ItemLabel.Root.resoure)
                        {
                            m_RootType = ItemLabel.Root.resoure;
                            SetPopuplistItem();
                        }
                    } break;
                case 7:
                    {
                        if (m_RootType != ItemLabel.Root.part)
                        {
                            m_RootType = ItemLabel.Root.part;
                            SetPopuplistItem();
                        }
                    } break;
                case 8:
                    {
                        if (m_RootType != ItemLabel.Root.decoration)
                        {
                            m_RootType = ItemLabel.Root.decoration;
                            SetPopuplistItem();
                        }
                    } break;
                case 9:
                    {
                        if (m_RootType != ItemLabel.Root.ISO)
                        {
                            m_RootType = ItemLabel.Root.ISO;
                            SetPopuplistItem();
                        }
                    } break;
            }
        }
    }

    void OnMenuSelectionChange(string selection)
    {
        if (selection == this.m_AllStr)
            m_ItemType = 0;
        else
            m_ItemType = ItemLabel.GetItemTypeByName(selection);
        this.UpdateLeftList();
    }

    void OnMenuListItemClick(int index)
    {
        if (index < 0 || index >= m_ItemDataList.Count)
            return;
        List<GameObject> items = FactoryReplicator.m_MenuContent.gridList.Gos;

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

    #endregion

    #region MIDDLE_CONTENT

    // ---------------------------------
    //  Func
    // ---------------------------------

    void _updateQueryGridItems(int m_id)
    {
        if (FactoryReplicator == null)
            return;

        Pathea.Replicator r = UIGraphControl.GetReplicator();
        if (null == r)
        {
            return;
        }
        // Clear first
        FactoryReplicator.DestroyQueryItems();

        foreach (Pathea.Replicator.KnownFormula kf in r.knowFormulas)
        {
            Pathea.Replicator.Formula ms = kf.Get();
            if (null == ms)
            {
                continue;
            }
            for (int j = 0; j < ms.materials.Count; j++)
            {
                if (ms.materials[j].itemId == m_id)
                {
                    _createQueryItems(ms.productItemId);
                }
            }
        }
    }

    void _createQueryItems(int ID)
    {
        GameObject go = FactoryReplicator.InstantiateQueryItem("");
        UIGridItemCtrl grid = go.GetComponent<UIGridItemCtrl>();
        grid.SetToolTipInfo(ListItemType.mItem, ID);

        UIEventListener el = go.GetComponent<UIEventListener>();
        el.onClick = OnQueryGridItemClick;

        ItemProto item = ItemProto.GetItemData(ID);
        grid.SetCotent(item.icon);
    }

    // ---------------------------------
    //  Call back
    // ---------------------------------

    // Query Grid Item call back
    void OnQueryGridItemClick(GameObject go)
    {
        UIGridItemCtrl grid = go.GetComponent<UIGridItemCtrl>();
        if (grid == null)
            return;

        int m_id = grid.mItemId;
        bool ok = ReDrawGraph(m_id);
        if (ok)
        {
            _updateQueryGridItems(m_id);

            _setCompundInfo(m_id);
            // Add to history
            FactoryReplicator.AddGraphHistory(m_id);
        }

        FactoryReplicator.m_MiddleContent.graphScrollBox.Reposition();
    }

    // Query search button event
    void OnQuerySearchBtnClick(GameObject go)
    {
        if (FactoryReplicator.IsQueryInputValid())
        {
            m_RootType = ItemLabel.Root.all;
            FactoryReplicator.SetMenuCBChecked(0, true);
            SetPopuplistItem();
        }
        else
        {
            //lz-2016.07.01 如果搜索内容为空，直接显示全部
            UpdateLeftList();
        }
    }

    void NewOnQuerySearchBtnClick()
    {
        if (FactoryReplicator.IsQueryInputValid())
        {
            m_RootType = ItemLabel.Root.all;
            FactoryReplicator.SetMenuCBChecked(0, true);
            SetPopuplistItem();
        }
        else
        {
            //lz-2016.07.01 如果搜索内容为空，直接显示全部
            UpdateLeftList();
        }
    }

    // Query clear button event
    void OnQueryClearBtnClick(GameObject go) 
    {
        if (FactoryReplicator.IsQueryInputValid())
        {
            //			mQueryInput.text = string.Empty;
            FactoryReplicator.m_MiddleContent.queryInput.text = string.Empty;
            UpdateLeftList();
        }
    }


    #endregion

    #region RIGHT_CONTENT

    // ---------------------------------
    //  Member
    // ---------------------------------
    private UIGridItemCtrl m_RightGridItem;


    // ---------------------------------
    //  Func
    // ---------------------------------
    void _initCompundGridItems()
    {
        if (m_RightGridItem != null)
            return;

        GameObject go = FactoryReplicator.InstantiateGridItem("");
        m_RightGridItem = go.GetComponent<UIGridItemCtrl>();

    }

    void _setCompundInfo(int id)
    {
        UIGraphNode rootNode = FactoryReplicator.m_MiddleContent.graphCtrl.rootNode;
        if (rootNode.mCtrl.mContentSprites[0].gameObject.activeSelf)
        {
            int m_id = rootNode.GetItemID();
            ItemProto item = ItemProto.GetItemData(m_id);
            m_RightGridItem.SetCotent(item.icon);
            m_RightGridItem.SetToolTipInfo(ListItemType.mItem, m_id);
        }
        else
        {
            m_RightGridItem.SetCotent(rootNode.mCtrl.mContentTexture.mainTexture);
            m_RightGridItem.SetToolTipInfo(ListItemType.mItem, rootNode.GetItemID());
        }

        FactoryReplicator.m_RightContent.countInput.text = rootNode.ms.m_productItemCount.ToString();
    }

    // ---------------------------------
    //  Call back
    // ---------------------------------

    int OnCountIputChanged(int count)
    {
        UIGraphControl graph_ctrl = FactoryReplicator.m_MiddleContent.graphCtrl;
        int n = count;
        if (graph_ctrl.rootNode != null)
        {
            if (n > 9999)
                n = 9999;

            int m = n % graph_ctrl.rootNode.ms.m_productItemCount;
            if (m != 0)
            {
                n = n - m + graph_ctrl.rootNode.ms.m_productItemCount;
            }

            if (n > 9999)
                n = n - graph_ctrl.rootNode.ms.m_productItemCount;

            graph_ctrl.SetGraphCount(n);
        }
        return n;
    }

    void OnCompoundBtnClick(GameObject go)
    {
        if (!GameConfig.IsMultiMode)
        {
            UIGraphControl graph_ctrl = FactoryReplicator.m_MiddleContent.graphCtrl;

            if (!graph_ctrl.isCanCreate())
                return;

            int item_id = graph_ctrl.rootNode.GetItemID();
            int item_num = graph_ctrl.rootNode.getCount;

            //lz-2016.09.26 时间使用份数计算的，所有要除掉productItemCount
            float time = graph_ctrl.rootNode.ms.timeNeed * item_num/ graph_ctrl.rootNode.ms.m_productItemCount;

            if (RandomMapConfig.useSkillTree)
            {
                if (GameUI.Instance.mSkillWndCtrl._SkillMgr != null)
                {
                    time = GameUI.Instance.mSkillWndCtrl._SkillMgr.CheckReduceTime((float)time);
                }
            }

            if (m_Factory.SetCompoudItem(item_id, item_num, time))
            {
                for (int i = 0; i < graph_ctrl.mGraphItemList.Count; i++)
                {
                    if (graph_ctrl.mGraphItemList[i].mPartent == graph_ctrl.rootNode)
                        PeCreature.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>().package.Destroy(graph_ctrl.mGraphItemList[i].GetItemID(), graph_ctrl.mGraphItemList[i].needCount);
                }

                // Status bar
                CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mJoinCompoudingQueue.GetString(), ItemProto.GetItemData(item_id).GetName()));

                //....
                if (GameUI.Instance.mItemPackageCtrl != null)
                    GameUI.Instance.mItemPackageCtrl.ResetItem();
            }
        }
        else
        {
            UIGraphControl graph_ctrl = FactoryReplicator.m_MiddleContent.graphCtrl;

            if (!graph_ctrl.isCanCreate())
                return;
            int skill_id = graph_ctrl.rootNode.ms.id;
            //lz-2016.09.26 多人的接口需要的是productCount
            int productCount = graph_ctrl.rootNode.getCount/ graph_ctrl.rootNode.ms.m_productItemCount;
            //to do--skillid,num
            if (m_Factory.Data.m_CompoudItems.Count >= CSFactory.c_CompoudItemCount)
                return;
            //lz-2016.09.26 多人的接口需要的是productCount
            m_Factory._ColonyObj._Network.FCT_Compoud(skill_id, productCount);
        }


    }

    #endregion


    #region GRAPH_FUNC

    // ---------------------------------
    //  Func
    // ---------------------------------

    private bool ReDrawGraph(int itemID, int scirptIndex = 0)
    {
        if (FactoryReplicator == null)
            return false;
        this.AddScriptItemData(itemID);
        if (!this.m_Formulas.ContainsKey(itemID) || scirptIndex >= this.m_Formulas[itemID].Count || scirptIndex < 0)
            return true;

        UIGraphControl graph_ctrl = FactoryReplicator.m_MiddleContent.graphCtrl;

        if (m_RootType == ItemLabel.Root.ISO)
        {
            // Iso
        }
        else
        {
            if (itemID != -1)
            {
                //Pathea.Replicator.Formula ms = Pathea.Replicator.Formula.Mgr.Instance.FindByProductId(m_id);

                Pathea.Replicator.KnownFormula knownFornula = this.m_Formulas[itemID][scirptIndex];
                Pathea.Replicator.Formula ms = knownFornula.Get();
                ItemProto item = this.m_ItemDataList.Find(a => a.id == itemID);

                if (ms == null||item==null)
                {
                    return false;
                }
                
                FactoryReplicator.ClearGraph();

                int level_v = 0;

                UIGraphNode root = graph_ctrl.AddGraphItem(level_v, null, ms, item.icon, "Icon");
                root.mTipCtrl.SetToolTipInfo(ListItemType.mItem, itemID);
                root.mCtrl.ItemClick += this.OnGraphItemClick;


                for (int j = 0; j < ms.materials.Count; j++)
                {
                    if (ms.materials[j].itemId != 0)
                    {

                        ItemProto item2 = ItemProto.GetItemData(ms.materials[j].itemId);
                        string[] strico2 = item2.icon;

                        UIGraphNode node = graph_ctrl.AddGraphItem(level_v, root, null, strico2, "Icon");
                        node.mTipCtrl.SetToolTipInfo(ListItemType.mItem, ms.materials[j].itemId);
                        node.mCtrl.ItemClick += this.OnGraphItemClick;
                    }
                }
                UpdateItemsTrackState(ms);
            }
        }
        //		graph_ctrl.DrawGraph();
        FactoryReplicator.DrawGraph();
        return true;
    }

    // ---------------------------------
    //  Call back
    // ---------------------------------

    // Replicator --- Graph
    void OnGraphItemClick(int index)
    {
        if (index == -1)
            return;

        UIGraphControl graph_ctrl = FactoryReplicator.m_MiddleContent.graphCtrl;

        int m_id = graph_ctrl.mGraphItemList[index].GetItemID();

        if (ReDrawGraph(m_id))
        {
            _setCompundInfo(m_id);
            // Add id to graphistory
            FactoryReplicator.AddGraphHistory(m_id);
        }
        else
        {
            if (graph_ctrl.mSelectedIndex != -1)
                graph_ctrl.mGraphItemList[graph_ctrl.mSelectedIndex].mCtrl.SetSelected(false);
            graph_ctrl.mGraphItemList[index].mCtrl.SetSelected(true);
            graph_ctrl.mSelectedIndex = index;
        }

        _updateQueryGridItems(m_id);

    }

    void OnGraphUseHistory(object history)
    {
        int id = (int)history;

        ReDrawGraph(id);

        _setCompundInfo(id);
    }

    #endregion


    #region GLOBAL_FUNC

    public void Init()
    {
        FactoryReplicator.m_MenuContent.gridList.itemGoPool.Init();
        m_AllStr = PELocalization.GetString(10055);  //lz-2016.08.09 “All” 的语言映射
    }

    #endregion


    #region UNITY_FUNC

    void Awake()
    {

    }

    // Use this for initialization
    void Start()
    {

        _initCompundGridItems();

        FactoryReplicator.onMenuBtnClick = OnLeftMenuClick;
        FactoryReplicator.onMenueSelect = OnMenuSelectionChange;
        FactoryReplicator.onQuerySearchBtnClick = OnQuerySearchBtnClick;
        FactoryReplicator.onQueryClearBtnClick = OnQueryClearBtnClick;
        FactoryReplicator.onGraphUseHistory = OnGraphUseHistory;
        FactoryReplicator.onCountIputChanged = OnCountIputChanged;
        FactoryReplicator.onCompoundBtnClick = OnCompoundBtnClick;

        for (int i = 0; i < CSFactory.c_CompoudItemCount; i++)
        {
            _createCompoundItem();
        }
    }

    // Update is called once per frame
    void Update()
    {
        //lz-2016.10.14 m_Factory空对象
        if (FactoryReplicator == null|| null==m_Factory)
            return;

        UIGraphControl graph_ctrl = FactoryReplicator.m_MiddleContent.graphCtrl;
        if (graph_ctrl.isCanCreate())
        {
            FactoryReplicator.m_RightContent.compoundBtn.isEnabled = true;
        }
        else
            FactoryReplicator.m_RightContent.compoundBtn.isEnabled = false;

        // Queue
        for (int i = 0; i < m_CompoudItems.Count; i++)
        {
            if (i < m_Factory.CompoudItemsCount)
            {
                ItemProto data = ItemProto.GetItemData(m_Factory.Data.m_CompoudItems[i].itemID);
                string[] iconStr = data.icon;

                if (iconStr.Length != 0)
                    m_CompoudItems[i].IcomName = iconStr[0];
                else
                    m_CompoudItems[i].IcomName = "";

                m_CompoudItems[i].Count = m_Factory.Data.m_CompoudItems[i].itemCnt;

                m_CompoudItems[i].SliderValue = m_Factory.Data.m_CompoudItems[i].curTime / m_Factory.Data.m_CompoudItems[i].time;
                m_CompoudItems[i].ShowSlider = true;
            }
            else
            {
                m_CompoudItems[i].IcomName = "Null";
                m_CompoudItems[i].Count = 0;
                m_CompoudItems[i].SliderValue = 0;
                m_CompoudItems[i].ShowSlider = false;
            }
        }

    }

    
    void OnEnable()
    {
        if (null != m_Replicator)
        {
            //lz-2016.10.14 打开的时候注册脚本改变事件
            m_Replicator.eventor.Subscribe(UpdateLeftListEventHandler);
            //lz-2016.10.14 打开的时候刷新脚本列表
            UpdateLeftList();

            if (GameUI.Instance.mItemsTrackWnd != null) GameUI.Instance.mItemsTrackWnd.ScriptTrackChanged += OnScriptTrackChanged;
        }
    }

    void OnDisable()
    {
        if (null != m_Replicator)
        {
            //lz-2016.10.14 关闭的时候移除脚本改变事件
            m_Replicator.eventor.Unsubscribe(UpdateLeftListEventHandler);

            if (GameUI.Instance.mItemsTrackWnd != null) GameUI.Instance.mItemsTrackWnd.ScriptTrackChanged -= OnScriptTrackChanged;
        }
    }

    #endregion


    #region multimode
    public void OnCompoundBtnClickSuccess(int item_id, CSFactory entity)
    {
        if (m_Factory == entity)
        {
            CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mJoinCompoudingQueue.GetString(), ItemProto.GetItemData(item_id).GetName()));
        }
    }
    #endregion

    //lz-2016.08.09 因为一个Item可以由多个脚本合成，左边的列表用来显示Item，右边可以选择使用哪个脚本
    #region ScriptItem methods

    private UIScriptItem_N m_BackupScriptItem;
    private int m_CurItemID;

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
                Pathea.Replicator.KnownFormula knownFormula = UIGraphControl.GetReplicator().GetKnownFormula(formulaList[i].id);
                if (null != knownFormula)
                    knownFormulaList.Add(knownFormula);
            }
            ItemProto item = ItemProto.GetItemData(itemID);
            this.m_ItemDataList.Add(item);
            this.m_Formulas.Add(itemID, knownFormulaList);
        }
        if (this.m_Formulas.ContainsKey(itemID) && itemID != this.m_CurItemID)
        {
            this.UpdateCurItemScriptList(itemID);
            this.SelectFirstScritItem(false);
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

    //lz-2016.08.09 点击ScriptItem事件
    private void ScriptItemEvent(int itemID, int scriptIndex)
    {
        if (!this.m_Formulas.ContainsKey(itemID) || scriptIndex >= this.m_Formulas[itemID].Count || scriptIndex < 0)
            return;

        //更改new的状态
        Pathea.Replicator.KnownFormula knownFornula = this.m_Formulas[itemID][scriptIndex];
        Pathea.Replicator r = UIGraphControl.GetReplicator();
        if (null != r)
        {
            r.SetKnownFormulaFlag(knownFornula.id);
        }

        bool ok = ReDrawGraph(itemID, scriptIndex);
        if (ok)
        {
            _updateQueryGridItems(itemID);
            _setCompundInfo(itemID);
            // Add id to grap history
            FactoryReplicator.AddGraphHistory(itemID);
        }

        FactoryReplicator.m_MiddleContent.graphScrollBox.Reposition();
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

    #region Item Track

    UIGraphControl mGraphCtrl
    {
        get
        {
            if (FactoryReplicator == null || FactoryReplicator.m_MiddleContent == null || FactoryReplicator.m_MiddleContent.graphCtrl == null) return null;
            return FactoryReplicator.m_MiddleContent.graphCtrl;
        }
    }

    void OnScriptTrackChanged(int scriptID, bool add)
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
}
