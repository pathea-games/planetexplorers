using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using CustomData;

public class UIDropItemWnd : UIBaseWnd
{
    [SerializeField]
    Grid_N mGridPrefab;
    [SerializeField]
    UIGrid mDropItemGrid;
    [SerializeField]
    UIScrollBar mDropItemScrollBar;
    [SerializeField]
    LayerMask mItemDroplayer;

    const int PAGE_CNT = 24;

    List<Grid_N> mDropItemGrids;
    List<DropItemInfo> mDropInfoList;
    UIItemPackageCtrl mItemPackage { get { return GameUI.Instance.mItemPackageCtrl; } }

    public class DropItemInfo
    {
        public int mPickTab;
        public int mPagkageIndex;
        public Grid_N mPackageGrid;
        public ItemObject mDropItemObj;

        public void SetInfo(int pickTab, int index, Grid_N grid, ItemObject dropItemObj)
        {
            mPickTab = pickTab;
            mPagkageIndex = index;
            mPackageGrid = grid;
            mDropItemObj = dropItemObj;
        }
    }


    #region mono methods
    void Update()
    {
        UpdatePos();
    }
    #endregion

    #region override methods
    protected override void InitWindow()
    {
        base.InitWindow();
        UpdatePos();
        mDropItemGrids = new List<Grid_N>();
        mDropInfoList = new List<DropItemInfo>();
        mItemPackage.e_OnResetItem += ResetPackageWnd;
        InitGrids();
    }

    public override void Show()
    {
        base.Show();
        ResetGridItems();
    }

    protected override void OnHide()
    {
        //lz-2016.09.23 关闭的时候调用取消丢弃，将物品返还到包裹
        CancelDropItems();
        base.OnHide();
    }

    #endregion


    #region public methods
    /// <summary>取消拖拽的Items</summary>
    public void CancelDropItems()
    {
        ResetGridItems();
        if (null != mItemPackage)
        {
            mItemPackage.ResetItem();
        }
    }

    public void AddToDropList(int packTab, int Index, Grid_N grid,Grid_N curDropItem=null)
    {
        if (curDropItem == null)
        {
            curDropItem = GetEmptyGrid();
            if (null == curDropItem)
            {
                PeTipMsg.Register(PELocalization.GetString(82209001), PeTipMsg.EMsgLevel.Warning);
                return;
            }
        }
        RemoveFromDropList(curDropItem);
        curDropItem.SetItem(grid.Item);
        DropItemInfo info = new DropItemInfo();
        info.SetInfo(packTab, Index, grid, grid.ItemObj);
        mDropInfoList.Add(info);
        ResetPackageWnd(packTab, Index);
    }

    public void RemoveFromDropList(Grid_N grid)
    {
        if (null == grid.ItemObj)
            return;

        int index = mDropInfoList.FindIndex(itr => itr.mDropItemObj == grid.ItemObj);
        if (index != -1)
        {
            mDropInfoList.RemoveAt(index);
            grid.SetItem(null);
            mItemPackage.ResetItem();
        }
    }

    public List<int> DropReqList { get { return mDropReq; } }

    #endregion


    #region private methods

    private Grid_N GetEmptyGrid()
    {
        return null==mDropItemGrids?null:mDropItemGrids.Find(it => it.Item == null);
    }

    private void OnMouseLeftClick(Grid_N grid)
    {
        if (null != grid && null != grid.ItemObj)
        {
            SelectItem_N.Instance.SetItemGrid(grid);
        }
    }

    private void OnDropItem(Grid_N grid)
    {
        if (null != mItemPackage && null != grid 
            && null!=SelectItem_N.Instance.Grid && SelectItem_N.Instance.Grid.ItemPlace== ItemPlaceType.IPT_Bag)
        {
            AddToDropList(mItemPackage.CurrentPickTab, mItemPackage.CurrentPageIndex, SelectItem_N.Instance.Grid,grid);
        }
    }
    private void ResetGridItems()
    {
        if (!base.mInit) return;
        foreach (Grid_N item in mDropItemGrids)
        {
            item.SetItem(null);
        }
        mDropInfoList.Clear();
        mDropItemScrollBar.scrollValue = 0;
    }

    Grid_N GetNewGrid()
    {
        Grid_N newItem = Instantiate(mGridPrefab) as Grid_N;
        GameObject.Destroy(newItem.transform.GetComponent<UIPanel>());
        newItem.transform.parent = mDropItemGrid.transform;
        newItem.transform.localPosition = Vector3.zero;
        newItem.transform.localScale = Vector3.one;
        newItem.SetItem(null);
        newItem.SetItemPlace(ItemPlaceType.IPT_DropItem, mDropItemGrids.Count);
        newItem.onLeftMouseClicked = OnMouseLeftClick;
        newItem.onRightMouseClicked = RemoveFromDropList;
        newItem.onDropItem = OnDropItem;
        return newItem;
    }

    void ResetPackageWnd(int packTab, int pageIndex)
    {
        if (!isShow)
            return;
        foreach (DropItemInfo info in mDropInfoList)
        {
            if (info.mPickTab == packTab && info.mPagkageIndex == pageIndex)
                info.mPackageGrid.SetItem(null);
        }
    }

    void OnDropBtn()
    {
        if (null != GameUI.Instance.mMainPlayer && mDropInfoList.Count > 0)
            ApplyPackDropItem(mDropInfoList);
        base.OnHide();
    }

    void OnDropClose()
    {
        OnHide();
    }

    List<int> mDropReq = new List<int>();

    void ApplyPackDropItem(List<DropItemInfo> itemList)
    {
        //lz-2017.01.03 crash bug 错误 #8002
        if (null == itemList) return;
        if (null == GameUI.Instance || null == GameUI.Instance.mMainPlayer) return;

        Pathea.SkAliveEntity skEntity = GameUI.Instance.mMainPlayer.GetCmpt<Pathea.SkAliveEntity>();
        if (skEntity == null|| null==skEntity.Entity) return;

        mDropReq.Clear();
        foreach (DropItemInfo itemInfo in itemList)
        {
            if (null != itemInfo && null != itemInfo.mDropItemObj)
            {
                mDropReq.Add(itemInfo.mDropItemObj.instanceId);
            }
        }
        MapObj[] mapObjs = new MapObj[1];
        RaycastHit hitInfo;
        int count = 0;

        while (count++ < 100)
        {
            if (skEntity.Entity)
            {
                Vector3 offsetPos = new Vector3(Random.Range(-2f, 2f), 2f, Random.Range(-2f, 2f));
                if (Physics.Raycast(skEntity.Entity.position + offsetPos, Vector3.down, out hitInfo, 10f, mItemDroplayer))
                {
                    if (!hitInfo.collider.isTrigger && hitInfo.distance < 10f)
                    {
                        mapObjs[0] = new MapObj();
                        mapObjs[0].pos = hitInfo.point;
                        mapObjs[0].objID = skEntity.GetId();
                        break;
                    }
                }
            }
        }
        if (mapObjs[0] != null)
            PlayerNetwork.mainPlayer.CreateMapObj((int)DoodadType.DoodadType_Drop, mapObjs);
    }

    void InitGrids()
    {
        if (mDropItemGrid.transform.childCount > 0)
        {
            for (int i = 0; i < mDropItemGrid.transform.childCount; i++)
            {
                Destroy(mDropItemGrid.transform.GetChild(i).gameObject);
            }
        }
        for (int i = 0; i < PAGE_CNT; i++)
        {
            mDropItemGrids.Add(GetNewGrid());
        }
        mDropItemGrid.Reposition();
        mDropItemScrollBar.scrollValue = 0;
    }

    void UpdatePos()
    {
        Vector3 pos = mItemPackage.transform.localPosition;
        pos.x += 375;
        transform.localPosition = pos;
    }
    #endregion
}
