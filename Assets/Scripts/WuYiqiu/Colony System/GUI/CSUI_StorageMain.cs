using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using Pathea;
using System;

public class CSUI_StorageMain : MonoBehaviour
{
    //Grids Border

    //Grdis Main
    [SerializeField]
    private UIGrid m_MainRoot;
    [SerializeField]
    private Grid_N m_GridPrefab;

    [SerializeField]
    private UILabel m_PageLb;

    // Page Down & Up

    // Split
    [SerializeField]
    private GameObject m_SplitWnd;
    [SerializeField]
    private UILabel m_SplitNumLb;
    [SerializeField]

    //luwei
    private UIAtlas m_ButtonAtlas;
    public UIAtlas mNewUIAtlas;

    private List<Grid_N> m_Grids;

    public int m_GridsRow = 5;
    public int m_GridsCol = 8;

    private ItemPackage m_Package;
    [HideInInspector]
    public int m_Type = 0;

    private int m_PageIndex = 0;

    private int m_OpType = 0; //0 no. 1 SplitItem. 2 DeleteItem
    private Grid_N m_OpGird;
    private float m_SplitNumDur = 1;
    private float m_LastOpTime;
    private float m_SplitOpStartTime;

    public int PageIndex { get { return m_PageIndex; } }

    private SlotList m_CurPack;

    private bool m_IsWorking = true;

    //public bool isMission = false;

    public CSStorage m_storage;

    public ItemObject CurOperateItem { get { return m_OpGird ==null ? null : m_OpGird.ItemObj; } }

    // Event
    public enum EEventType
    {
        CantWork,
        PutItemInto,
        TakeAwayItem,
        ResortItem,
        SplitItem,
        DeleteItem
    }
    public delegate void OpStatusDel(EEventType type, object obj1, object obj2);
    public event OpStatusDel OpStatusEvent;

    public void SetPackage(ItemPackage package, int type = 0, CSStorage storage = null)
    {
        m_Package = package;
        m_PageIndex = 0;
        m_OpType = 0;

        SetType(type);
        if (GameConfig.IsMultiMode)
        {
            m_storage = storage;
        }
    }


    public void SetType(int type, int pageIndex = 0)
    {
        if (type <= (int)ItemPackage.ESlotType.None || type >= (int)ItemPackage.ESlotType.Max)
        {
            Debug.LogWarning("The type you give is out of range!");
            return;
        }

        m_Type = type;
        m_PageIndex = pageIndex;

        RestItems();
    }

    public void RestItems()
    {
        if (m_Grids == null) return;

        if (m_Package == null)
        {
            Debug.LogWarning("It must need a package to Reset the items!");
            return;
        }


        m_CurPack = m_Package.GetSlotList((ItemPackage.ESlotType)m_Type);

        int pageCount = m_GridsRow * m_GridsCol;

        if ((m_CurPack.Count - 1) / pageCount < m_PageIndex)
            m_PageIndex = (m_CurPack.Count - 1) / pageCount;

        int itemCount;

        if ((m_CurPack.Count - 1) / pageCount == m_PageIndex)
            itemCount = (m_CurPack.Count - m_PageIndex * pageCount);
        else
            itemCount = pageCount;


        for (int index = 0; index < itemCount; index++)
        {
            m_Grids[index].SetItem(m_CurPack[index + m_PageIndex * pageCount]);
            if (m_storage != null)
                m_Grids[index].SetItemPlace(ItemPlaceType.IPT_CSStorage, index + m_PageIndex * pageCount);
            else
                m_Grids[index].SetItemPlace(ItemPlaceType.IPT_NPCStorage, index + m_PageIndex * pageCount);

            switch (m_Type)
            {
                case (int)ItemPackage.ESlotType.Item:
                    m_Grids[index].SetGridMask(GridMask.GM_Item);
                    break;
                case (int)ItemPackage.ESlotType.Equipment:
                    m_Grids[index].SetGridMask(GridMask.GM_Equipment);
                    break;
                case (int)ItemPackage.ESlotType.Resource:
                    m_Grids[index].SetGridMask(GridMask.GM_Resource);
                    break;
                case (int)ItemPackage.ESlotType.Armor:
                    m_Grids[index].SetGridMask(GridMask.GM_Armor);
                    break;
            }
        }

        m_PageLb.text = (m_PageIndex + 1).ToString() + "/" + ((m_CurPack.Count - 1) / pageCount + 1);
    }



    // Set Item in current Page Grid which was empty
    public bool SetItemWithEmptyGrid(ItemObject item)
    {
        if (item == null)
            return false;

        int pageCount = m_GridsRow * m_GridsCol;
        int index = m_CurPack.VacancyIndex();
        if (index >= 0)
        {
            if ((index / pageCount) == m_PageIndex)
            {
                m_CurPack[index] = item;
                m_Grids[index % pageCount].SetItem(item);
                return true;
            }
            else
            {
                m_CurPack[index] = item;
                m_PageIndex = index / pageCount;

                RestItems();
                return true;
            }
        }


        return false;
    }


    public void SetWork(bool bWork)
    {
        if (bWork)
        {
            foreach (Grid_N grid in m_Grids)
                grid.mSkillCooldown.fillAmount = 0;
        }
        else
        {
            foreach (Grid_N grid in m_Grids)
                grid.mSkillCooldown.fillAmount = 1;
        }

        m_IsWorking = bWork;
    }

    /// <summary>
    /// lz-2016.10.26 用来检测某个Item是不是玩家正在操作的东西
    /// </summary>
    /// <returns></returns>
    public bool EqualUsingItem(ItemSample item, bool showUsingTip = true)
    {
        if (null == item || null == CurOperateItem)
            return false;
        if (CurOperateItem == item)
        {
            if (showUsingTip)
                PeTipMsg.Register(PELocalization.GetString(8000623), PeTipMsg.EMsgLevel.Error);
            return true;
        }
        else
            return false;
    }

    void ResetPage()
    {
        //GameUI.Instance.mItemPackageCtrl.ResetItem(m_Type, m_PageIndex);
    }

    #region NGUI_CALLBACK
    // page up
    void BtnLeftOnClick()
    {
        if (m_PageIndex > 0)
        {
            m_PageIndex -= 1;
            RestItems();
            ResetPage();
        }
    }

    void BtnLeftEndOnClick()
    {
        if (m_PageIndex > 0)
        {
            m_PageIndex = 0;
            RestItems();
            ResetPage();
        }
    }

    // page down
    void BtnRightOnClick()
    {
        int pageCount = m_GridsRow * m_GridsCol;
        int maxPage = ((m_CurPack.Count - 1) / pageCount + 1);

        if (m_PageIndex < maxPage)
        {
            m_PageIndex += 1;
            RestItems();
            ResetPage();
        }
    }

    void BtnRightEndOnClick()
    {
        int pageCount = m_GridsRow * m_GridsCol;
        int maxPage = ((m_CurPack.Count - 1) / pageCount + 1);

        if (m_PageIndex < maxPage)
        {
            m_PageIndex = maxPage;
            RestItems();
            ResetPage();
        }
    }

    void OnResort()
    {
        if (m_SplitWnd.activeInHierarchy)
            return;

        if (!GameConfig.IsMultiMode)
        {
            m_Package.Sort((ItemPackage.ESlotType)m_Type);
            RestItems();

            if (OpStatusEvent != null)
                OpStatusEvent(EEventType.ResortItem, null, null);
        }
        else
        {
            if (m_storage != null)
            {
                m_storage._ColonyObj._Network.STO_Sort((int)m_Type);//colony
            }
            else
            {
                PlayerNetwork.mainPlayer.RequestPersonalStorageSort((int)m_Type);//npcstorage
            }
        }

    }

    void OnSplitBtn()
    {
        if (m_SplitWnd.activeInHierarchy)
            return;

        if (m_OpType == 1)
        {
            m_OpType = 0;
            UICursor.Clear();
        }
        else
        {
            m_OpType = 1;
            UICursor.Set(mNewUIAtlas, "icocai");
        }

        m_OpGird = null;
    }

    void OnDeleteBtn()
    {
        if (m_SplitWnd.gameObject.activeInHierarchy)
            return;

        if (m_OpType == 2)
        {
            m_OpType = 0;
            UICursor.Clear();
        }
        else
        {
            m_OpType = 2;
            UICursor.Set(mNewUIAtlas, "icodelete");
        }

        m_OpGird = null;
    }

    // Split Wnd Call Back
    void OnSplitAddBtn()
    {
        if (Time.time - m_LastOpTime > 0.1f)
        {
            m_SplitOpStartTime = Time.time;
            m_SplitNumDur += 1;
        }
        m_LastOpTime = Time.time;
        float dT = Time.time - m_SplitOpStartTime;
        if (dT < 0.5f)
            m_SplitNumDur += 0;
        else if (dT < 3f)
            m_SplitNumDur += 3 * Time.deltaTime;
        else if (dT < 5f)
            m_SplitNumDur += 6 * Time.deltaTime;
        else if (dT < 7f)
            m_SplitNumDur += 9 * Time.deltaTime;
        else
            m_SplitNumDur += 12 * Time.deltaTime;

        m_SplitNumDur = Mathf.Clamp(m_SplitNumDur, 1, m_OpGird.Item.GetCount() - 1);
        m_SplitNumLb.text = ((int)m_SplitNumDur).ToString();
    }

    void OnSplitSubstructBtn()
    {
        if (Time.time - m_LastOpTime > 0.1f)
        {
            m_SplitOpStartTime = Time.time;
            m_SplitNumDur -= 1;
        }
        m_LastOpTime = Time.time;
        float dT = Time.time - m_SplitOpStartTime;
        if (dT < 0.5f)
            m_SplitNumDur -= 0;
        else if (dT < 3f)
            m_SplitNumDur -= 3 * Time.deltaTime;
        else if (dT < 5f)
            m_SplitNumDur -= 6 * Time.deltaTime;
        else if (dT < 7f)
            m_SplitNumDur -= 9 * Time.deltaTime;
        else
            m_SplitNumDur -= 12 * Time.deltaTime;

        m_SplitNumDur = Mathf.Clamp(m_SplitNumDur, 1, m_OpGird.Item.GetCount() - 1);
        m_SplitNumLb.text = ((int)m_SplitNumDur).ToString();
    }

    void OnSplitOkBtn()
    {
        //lz-2016.12.23 Crash bug #7839
        if (m_OpGird == null|| null == m_OpGird.Item)
        {
            m_SplitWnd.SetActive(false);
            return;
        }

        m_SplitNumDur = Convert.ToInt32(m_SplitNumLb.text);
        if (m_SplitNumDur <= 0)
        {
            return;
        }
        m_SplitNumDur = Mathf.Clamp(m_SplitNumDur, 1, m_OpGird.Item.GetCount() - 1);
       

        if (!GameConfig.IsMultiMode)
        {
            ItemObject addItem = ItemMgr.Instance.CreateItem(m_OpGird.Item.protoId); // single
            addItem.IncreaseStackCount((int)m_SplitNumDur - 1);
            m_Package.AddItem(addItem);
            m_OpGird.ItemObj.DecreaseStackCount((int)m_SplitNumDur);
            RestItems();
            m_OpGird = null;

            if (OpStatusEvent != null)
                OpStatusEvent(EEventType.SplitItem, addItem.protoData.GetName(), m_SplitNumDur.ToString());
        }
        else
        {
            if (m_storage != null)
            {
                m_storage._ColonyObj._Network.STO_Split(m_OpGird.ItemObj.instanceId, (int)m_SplitNumDur);
            }
            else
            {
                PlayerNetwork.mainPlayer.RequestPersonalStorageSplit(m_OpGird.ItemObj.instanceId, (int)m_SplitNumDur);
            }
            m_OpGird = null;
        }

        m_SplitWnd.SetActive(false);
    }


    void OnSplitNoBtn()
    {
        m_OpGird = null;
        m_SplitWnd.SetActive(false);
    }

    #endregion

    #region GRID_N_CALLBACK

    void OnDropItem(Grid_N grid)
    {
        //lz-2016.11.16 当前包裹数据为空的时候直接返回
        if (CSUI_MainWndCtrl.Instance == null || grid == null|| null==m_CurPack)
            return;

        if (!CSUI_MainWndCtrl.IsWorking())
            return;


        if (!m_IsWorking)
        {
            if (OpStatusEvent != null)
                OpStatusEvent(EEventType.CantWork, CSUtils.GetEntityName(CSConst.etStorage), null);
            return;
        }

        if (null==SelectItem_N.Instance .ItemObj|| SelectItem_N.Instance.Place == ItemPlaceType.IPT_HotKeyBar)
        {
            SelectItem_N.Instance.SetItem(null);
            return;
        }

        if (grid.ItemObj == null)
        {
            switch (SelectItem_N.Instance.Place)
            {
                default:

                    if (GameConfig.IsMultiMode)
                    {
                        if (SelectItem_N.Instance.GridMask != GridMask.GM_Mission)
                        {
                            if (m_storage == null)
                            {
                                if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_Bag)
                                    PlayerNetwork.mainPlayer.RequestPersonalStorageStore(SelectItem_N.Instance.ItemObj.instanceId, grid.ItemIndex);
                                else if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_NPCStorage)
                                    PlayerNetwork.mainPlayer.RequestPersonalStorageExchange(SelectItem_N.Instance.ItemObj.instanceId, SelectItem_N.Instance.Index, grid.ItemIndex);
                            }
                            else
                            {
                                if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_Bag)
                                    m_storage._ColonyObj._Network.STO_Store(grid.ItemIndex, SelectItem_N.Instance.ItemObj);
                                else if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_CSStorage)
                                    m_storage._ColonyObj._Network.STO_Exchange(SelectItem_N.Instance.ItemObj.instanceId, SelectItem_N.Instance.Index, grid.ItemIndex);

                                return;
                            }
                            if (OpStatusEvent != null)
                                OpStatusEvent(EEventType.PutItemInto, SelectItem_N.Instance.ItemObj.protoData.GetName(), CSUtils.GetEntityName(CSConst.etStorage));
                        }
                    }
                    else
                    {
                        if (SelectItem_N.Instance.GridMask != GridMask.GM_Mission)
                        {
                            SelectItem_N.Instance.RemoveOriginItem();
                            grid.SetItem(SelectItem_N.Instance.ItemObj);
                            m_CurPack[grid.ItemIndex] = SelectItem_N.Instance.ItemObj;
                            if (OpStatusEvent != null)
                                OpStatusEvent(EEventType.PutItemInto, SelectItem_N.Instance.ItemObj.protoData.GetName(), CSUtils.GetEntityName(CSConst.etStorage));
                        }
                    }
                    

                    SelectItem_N.Instance.SetItem(null);
                    break;
            }

        }
        else
        {
            if (GameConfig.IsMultiMode)
            {
                if (m_storage == null)
                {
                    if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_NPCStorage)
                        PlayerNetwork.mainPlayer.RequestPersonalStorageExchange(SelectItem_N.Instance.ItemObj.instanceId, SelectItem_N.Instance.Index, grid.ItemIndex);
                }
                else
                {
                    if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_CSStorage)
                        m_storage._ColonyObj._Network.STO_Exchange(SelectItem_N.Instance.ItemObj.instanceId, SelectItem_N.Instance.Index, grid.ItemIndex);

                    return;
                }
            }
            else
            {
                Pathea.PlayerPackageCmpt pkg = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.PlayerPackageCmpt>();

                ItemObject dragItem = SelectItem_N.Instance.ItemObj;
                ItemObject dropItem = grid.ItemObj;

                ItemPackage.ESlotType dropType = ItemPackage.GetSlotType(dropItem.protoId);
                ItemPackage.ESlotType dragType = ItemPackage.GetSlotType(dragItem.protoId);

                //lz-2016.10.18 如果拖动的Item和放置的Item是同一类型，就直接交换ItemObj数据
                if (dropType == dragType && null != SelectItem_N.Instance.Grid)
                {
                    if (SelectItem_N.Instance.Grid.onGridsExchangeItem != null)
                    {
                        SelectItem_N.Instance.Grid.onGridsExchangeItem(SelectItem_N.Instance.Grid, dropItem);
                        grid.SetItem(dragItem);
                        m_CurPack[grid.ItemIndex] = grid.ItemObj;
                        SelectItem_N.Instance.SetItem(null);
                    }
                }
                //lz-2016.10.18 如果不是同一类型，或者没有Grid，就先添加，后移除
                else if (pkg.package.CanAdd(dropItem))
                {
                    pkg.package.AddItem(dropItem);
                    grid.SetItem(dragItem);
                    SelectItem_N.Instance.RemoveOriginItem();
                    SelectItem_N.Instance.SetItem(null);
                }
            }
        }
    }

    void OnGridsExchangeItems(Grid_N grid, ItemObject item)
    {
        grid.SetItem(item);
        m_CurPack[grid.ItemIndex] = grid.ItemObj;
    }

    void RemoveOriginItem(Grid_N grid)
    {
        if (grid == null || grid.ItemIndex < -1 || m_CurPack.Count <= grid.ItemIndex)
        {
            Debug.LogWarning("The giving grid is error.");
            return;
        }

        if (OpStatusEvent != null)
            OpStatusEvent(EEventType.TakeAwayItem, grid.Item.protoData.GetName(), CSUtils.GetEntityName(CSConst.etStorage));

        m_CurPack[grid.ItemIndex] = null;
        grid.SetItem(null);
    }

    void OnLeftMouseClicked(Grid_N grid)
    {
        if (!CSUI_MainWndCtrl.IsWorking())
            return;

        if (!m_IsWorking)
        {
            if (OpStatusEvent != null)
                OpStatusEvent(EEventType.CantWork, CSUtils.GetEntityName(CSConst.etStorage), null);
            return;
        }

        if (grid.Item == null) return;

        //lz-2016.10.26 不允许操作正在操作的东西
        if (EqualUsingItem(grid.Item, false)) return;

        switch (m_OpType)
        {
            case 0:

                SelectItem_N.Instance.SetItemGrid(grid);
                break;
            case 1:
                {
                    if (grid.Item.GetCount() > 1)
                    {
                        int mark = -1;
                        if (!GameConfig.IsMultiMode)
                            mark = m_Package.GetVacancySlotIndex(0);
                        else
                            mark = PeCreature.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>().package.GetVacancySlotIndex(0);

                        if (-1 == mark)
                        {
                            MessageBox_N.ShowOkBox(PELocalization.GetString(8000102));
                        }
                        else if (m_OpGird == null)
                        {
                            m_SplitWnd.SetActive(true);
                            m_OpGird = grid;
                            m_SplitNumDur = 1;
                            m_SplitNumLb.text = "1";
                        }
                    }
                } break;
            case 2:
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        m_OpGird = grid;

                        //wan
                        //				mOpBagID = grid.ItemIndex;
                        if (m_OpGird.Item.protoId / 10000000 == 9)
                            MessageBox_N.ShowOkBox(PELocalization.GetString(8000054));
                        else
                            MessageBox_N.ShowYNBox(PELocalization.GetString(8000055), OnDeleteItem);
                    }
                }
                break;
            default:
                break;
        }
    }

    void OnRightMouseClicked(Grid_N grid)
    {
        if (!CSUI_MainWndCtrl.IsWorking())
            return;

        if (!m_IsWorking)
        {
            if (OpStatusEvent != null)
                OpStatusEvent(EEventType.CantWork, CSUtils.GetEntityName(CSConst.etStorage), null);
            return;
        }
        if (!GameUI.Instance.mItemPackageCtrl.IsOpen())
            GameUI.Instance.mItemPackageCtrl.Show();

        if (grid.ItemObj == null) return;

        //lz-2016.10.26 不允许操作正在操作的东西
        if (EqualUsingItem(grid.Item, false)) return;

        if (GameConfig.IsMultiMode)
        {
            if (m_storage == null)
            {
                PlayerNetwork.mainPlayer.RequestPersonalStorageFetch(grid.ItemObj.instanceId, -1);
            }
            else
            {
                m_storage._ColonyObj._Network.STO_Fetch(grid.ItemObj.instanceId, -1);
            }
        }
        else
        {
            if (PeCreature.Instance.mainPlayer.GetCmpt<PackageCmpt>().Add(grid.ItemObj))
            {
                GameUI.Instance.mItemPackageCtrl.ResetItem();
                RemoveOriginItem(grid);
            }
            else
            {
                //lz-2016.09.14 提示背包已满
                PeTipMsg.Register(PELocalization.GetString(9500312), PeTipMsg.EMsgLevel.Warning);
            }
        }
    }

    void OnPlayerPackageRightClicked(Grid_N grid)
    {
        if (!CSUI_MainWndCtrl.IsWorking(false))
            return;

        if (!m_IsWorking) return;

        if (GameConfig.IsMultiMode)
        {
            if (m_storage == null)
            {
                PlayerNetwork.mainPlayer.RequestPersonalStorageStore(grid.ItemObj.instanceId, -1);
            }
            else
            {
                m_storage._ColonyObj._Network.STO_Store(-1, grid.ItemObj);
            }
        }
        else
        {
            if (SetItemWithEmptyGrid(grid.ItemObj))
            {
                //			CSUI_Main.ShowStatusBar("You put the " + grid.Item.mItemData.m_Englishname + " into the storage.");
                if (OpStatusEvent != null)
                    OpStatusEvent(EEventType.PutItemInto, grid.Item.protoData.GetName(), CSUtils.GetEntityName(CSConst.etStorage));
                //			CSUI_Main.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mPutIntoMachine.GetString(), grid.Item.mItemData.GetName(), CSUtils.GetEntityName(CSConst.etStorage)));
                PeCreature.Instance.mainPlayer.GetCmpt<PackageCmpt>().Remove(grid.ItemObj);
                //GetItemPackage().RemoveItem(grid.ItemObj);
                grid.SetItem(null);
            }
        }
    }

    void OnDeleteItem()
    {
        if (GameConfig.IsMultiMode)
        {
            if (m_storage == null)
            {
                PlayerNetwork.mainPlayer.RequestPersonalStorageDelete(m_OpGird.ItemObj.instanceId);
            }
            else
            {
                m_storage._ColonyObj._Network.Delete(m_OpGird.ItemObj.instanceId);
                return;
            }
            m_OpGird = null;
        }
        else
        {
            if (OpStatusEvent != null)
                OpStatusEvent(EEventType.DeleteItem, m_OpGird.Item.protoData.GetName(), CSUtils.GetEntityName(CSConst.etStorage));
            m_CurPack[m_OpGird.ItemIndex] = null;
            m_OpGird.SetItem(null);
        }
    }

    #endregion

    #region UNITY_INNER_FUNC

    void OnDisable()
    {
        if (GameUI.Instance != null)
            GameUI.Instance.mItemPackageCtrl.onRightMouseCliked -= OnPlayerPackageRightClicked;
    }

    void OnEnable()
    {
        if (GameUI.Instance != null)
            GameUI.Instance.mItemPackageCtrl.onRightMouseCliked += OnPlayerPackageRightClicked;
    }

    // Use this for initialization
    void Start()
    {
        m_Grids = new List<Grid_N>();

        m_SplitWnd.SetActive(false);

        // Creater Border & Grids
        for (int i = 0; i < m_GridsRow; i++)
        {
            for (int j = 0; j < m_GridsCol; j++)
            {

                //Grids
                Grid_N grid = Instantiate(m_GridPrefab) as Grid_N;
                grid.transform.parent = m_MainRoot.transform;
                grid.transform.localPosition = Vector3.zero;
                grid.transform.localRotation = Quaternion.identity;
                grid.transform.localScale = Vector3.one;

                grid.onDropItem = OnDropItem;
                grid.onRemoveOriginItem = RemoveOriginItem;
                grid.onLeftMouseClicked = OnLeftMouseClicked;
                grid.onRightMouseClicked = OnRightMouseClicked;
                grid.onGridsExchangeItem = OnGridsExchangeItems;

                m_Grids.Add(grid);
            }
        }

        m_MainRoot.repositionNow = true;

        RestItems();
    }

    // Update is called once per frame
    void Update()
    {

        // Page Button
        //if (m_PageIndex == 2)
        //    m_PageUpBtn.isEnabled = false;
        //else
        //    m_PageUpBtn.isEnabled = true;

        //if (m_PageIndex == 0)
        //    m_PageDownBtn.isEnabled = false;
        //else
        //    m_PageDownBtn.isEnabled = true;

        if (Input.GetKeyDown(KeyCode.PageUp))
            BtnLeftOnClick();

        if (Input.GetKeyDown(KeyCode.PageDown))
            BtnRightOnClick();

        //lz-2016.10.26 鼠标右键点击取消操作
		if ((PeInput.Get(PeInput.LogicFunction.OpenItemMenu)|| Input.GetMouseButtonDown(1))&& !m_SplitWnd.activeSelf && m_OpGird == null)
        {
            m_OpType = 0;
            UICursor.Clear();
        }

        switch (m_OpType)
        {
            case 1:
                UICursor.Set(mNewUIAtlas, "icocai");
                break;
            case 2:
                UICursor.Set(mNewUIAtlas, "icodelete");
                break;
        }
    }
    #endregion


    #region MultiMode
    public void SetPackageItemWithIndex(ItemPackage package, ItemObject item, int tabIndex, int index)
    {
        package.PutItem(item, index, (ItemPackage.ESlotType)tabIndex);

        //List<ItemObject> objList = package.GetItemList(tabIndex);
        //objList[index] = item;
    }

    public void SetItemWithIndex(ItemObject item, int index)
    {
        int pageCount = m_GridsRow * m_GridsCol;
        if ((index / pageCount) == m_PageIndex)
        {
            //m_CurPack[index] = item;
            m_Grids[index % pageCount].SetItem(item);
        }
        else
        {
            //m_CurPack[index] = item;
            //m_PageIndex = index / pageCount;

            RestItems();
        }
    }
    public void CSStoreResultStore(bool success, int index, int objId, CSStorage storage)
    {
        if (success)
        {
            if (storage == m_storage)
			{
				ItemObject itemObj = ItemMgr.Instance.Get(objId);
                if (OpStatusEvent != null)
                    OpStatusEvent(EEventType.PutItemInto, itemObj.protoData.GetName(), CSUtils.GetEntityName(CSConst.etStorage));
                if ((int)m_Type == itemObj.protoData.tabIndex)
                {
                    SetItemWithIndex(itemObj, index);
                }
            }
        }
    }

    public void CSStoreResultFetch(bool success, int objId, CSStorage storage)
    {
        if (success)
        {
            if (storage == m_storage)
			{
				ItemObject itemObj = ItemMgr.Instance.Get(objId);
                if (OpStatusEvent != null)
                    OpStatusEvent(EEventType.TakeAwayItem, itemObj.protoData.GetName(), CSUtils.GetEntityName(CSConst.etStorage));
                if ((int)m_Type == itemObj.protoData.tabIndex)
                {
                    RestItems();
                }
            }

        }
    }

    public void CSStoreResultExchange(bool success, int objId, int destIndex, int destId, int originIndex, CSStorage storage)
    {
        if (success)
        {
            if (storage == m_storage)
			{
				ItemObject originObj = ItemMgr.Instance.Get(objId);
				int type = originObj.protoData.tabIndex;
				ItemObject destObj;
				if (destId == -1)
				{
					destObj = null;
				}
				else
				{
					destObj = ItemMgr.Instance.Get(destId);
				}
                if ((int)m_Type == type)
                {
                    SetItemWithIndex(originObj, destIndex);
                    SetItemWithIndex(destObj, originIndex);
                }
            }
        }
    }

    public void CSStoreSortSuccess(int tabIndex, int[] ids, CSStorage storage)
    {
        if (storage == m_storage)
        {
            if (tabIndex == (int)m_Type)
            {
                int pageCount = m_GridsRow * m_GridsCol;
                for (int i = 0; i < ids.Length; i++)
                {
                    if (ids[i] == -1)
                    {
                        //m_CurPack[i] = null;
                        if ((i / pageCount) == m_PageIndex)
                        {
                            m_Grids[i % pageCount].SetItem(null);
                        }
                    }
                    else
                    {
                        ItemObject itemObj = ItemMgr.Instance.Get(ids[i]);
                        //m_CurPack[i] = itemObj;
                        if ((i / pageCount) == m_PageIndex)
                        {
                            m_Grids[i % pageCount].SetItem(itemObj);
                        }
                    }
                }
            }
        }
    }

    public void CSStoreResultSplit(bool suc, int objId, int destIndex, CSStorage storage)
    {
        ItemObject itemObj = ItemMgr.Instance.Get(objId);
        int type = itemObj.protoData.tabIndex;
        if (suc)
        {
            if (storage == m_storage)
            {
                if (type == (int)m_Type)
                {
                    SetItemWithIndex(itemObj, destIndex);
                }
            }
        }
    }

    public void CSStoreResultDelete(bool suc, int index, int objId, CSStorage storage)
    {
        ItemObject itemObj = ItemMgr.Instance.Get(objId);
        int type = itemObj.protoData.tabIndex;
        if (suc)
        {
            if (storage == m_storage)
            {
                if (OpStatusEvent != null)
                    OpStatusEvent(EEventType.DeleteItem, itemObj.protoData.GetName(), CSUtils.GetEntityName(CSConst.etStorage));

                if (type == (int)m_Type)
                {
                    if (index != -1)
                    {
                        SetItemWithIndex(null, index);
                    }
                }
            }
        }
    }
	public void CSStorageResultSyncItemList(CSStorage storage){
		if(storage==m_storage)
			RestItems();

	}
    #endregion
}
