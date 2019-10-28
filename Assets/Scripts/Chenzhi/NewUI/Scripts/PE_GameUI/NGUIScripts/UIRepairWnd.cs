using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using ItemAsset.PackageHelper;
using Pathea;

public class UIRepairWnd : UIBaseWnd
{
    public Grid_N mRepairItem;
    public UILabel mCurrentD;
    public UILabel mAddD;
    public UILabel m_CostTimeLabel;
    public N_ImageButton ResetBtn;
    public N_ImageButton RepairBtn;
    public UIGrid mCostItemGrid;
    public CSUI_MaterialGrid mPerfab;
    public MapObjNetwork Net
    {
        set
        {
            _net = value;
        }
        get
        {
            return _net;
        }
    }

    private const int c_MultiplierOfCost = 1;
    private List<CSUI_MaterialGrid> mItemList = new List<CSUI_MaterialGrid>();
    private CSRepairObject mRepairMachine;
    private Vector3 mMachinePos = Vector3.zero;
    private MapObjNetwork _net = null;
    private PlayerPackageCmpt m_PlayerPackageCmpt;

    #region override methods
    protected override void InitWindow()
    {
        base.InitWindow();
        mRepairItem.SetItemPlace(ItemPlaceType.IPT_Repair, 0);
        mRepairItem.onLeftMouseClicked = OnLeftMouseClicked;
        mRepairItem.onRightMouseClicked = OnRightMouseClicked;
        mRepairItem.onDropItem = OnDropItem;
    }

    protected override void OnClose()
    {
        _net = null;
        mMachinePos = Vector3.zero;
        base.OnClose();
    }

    #endregion

    #region public methods
    public void OpenWnd(CSRepairObject repMachine)
    {
        Show();
        mRepairMachine = repMachine;
        mMachinePos = repMachine.GetComponentInParent<PeEntity>().transform.position;

        if (null != PeCreature.Instance && null != PeCreature.Instance.mainPlayer)
        {
            m_PlayerPackageCmpt = PeCreature.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
        }

        if (Pathea.PeGameMgr.IsMulti)
        {
            _net = MapObjNetwork.GetNet(mRepairMachine.m_Entity.ID);
            if (_net != null)
                Net.RequestItemList();
            else
                Debug.LogError("net id is error id = " + mRepairMachine.m_Entity.ID);
            return;
        }
        if (mRepairMachine != null && mRepairMachine.m_Repair != null)
        {
            if (this.mRepairMachine.m_Repair.IsRepairingM)
            {
                if (this.mRepairMachine.m_Repair.onRepairedTimeUp == null)
                {
                    this.mRepairMachine.m_Repair.onRepairedTimeUp = this.RepairComplate;
                }
                if (!mRepairMachine.m_Repair.IsRunning)
                {
                    this.mRepairMachine.m_Repair.CounterToRunning();
                }
            }
            UpdateItem(mRepairMachine.m_Repair.m_Item);
        }
        else
        {
            //lz-2016.08.18 修理机里的东西为空的时候需要更新UI状态
            this.UpdateBtnState();
        }

        TutorialData.AddActiveTutorialID(8);
    }

    public void SetItemByNet(MapObjNetwork net, int itemId)
    {
        if (_net != null && _net == net)
        {
            if (itemId != -1)
            {
                ItemObject obj = ItemMgr.Instance.Get(itemId);
				if(null != obj)
	                mRepairMachine.m_Repair.m_Item = obj.GetCmpt<Repair>();
            }
            else
            {
                mRepairMachine.m_Repair.m_Item = null;
                mRepairItem.SetItem(null);
            }
            UpdateItemForNet(_net);
        }
    }

    public void UpdateItemForNet(MapObjNetwork net)
    {
        if (_net != null && _net == net)
        {
            if (mRepairMachine == null || mRepairMachine.m_Repair == null)
                return;
            UpdateItem(mRepairMachine.m_Repair.m_Item);
            _net.RequestRepairTime();
            GameUI.Instance.mItemPackageCtrl.ResetItem();
        }
    }

    public void SetCounterByNet(MapObjNetwork net, float curTime, float finalTime)
    {
        if (_net != null && _net == net)
        {
            this.mRepairMachine.m_Repair.StopCounter();
            this.mRepairMachine.m_Repair.onRepairedTimeUp = this.RepairComplate;
            this.mRepairMachine.m_Repair.StartCounter(curTime, finalTime);
            this.UpdateBtnState();
        }
    }

    public void OnLeftMouseClicked(Grid_N grid)
    {
        if (this.mRepairMachine.m_Repair != null && this.mRepairMachine.m_Repair.IsRepairingM)
            return;
        ActiveWnd();
        if (Pathea.PeGameMgr.IsMulti)
            return;
        if (null != mRepairItem.ItemObj)
            SelectItem_N.Instance.SetItem(mRepairItem.ItemObj, ItemPlaceType.IPT_Repair, 0);
    }

    public void OnRightMouseClicked(Grid_N grid)
    {
        if (this.mRepairMachine.m_Repair != null && this.mRepairMachine.m_Repair.IsRepairingM)
            return;
        ActiveWnd();
        if (Pathea.PeGameMgr.IsMulti)
        {
            if (grid.ItemObj == null)
                return;
            _net.GetItem(grid.ItemObj.instanceId);
        }
        else
        {
            TryAddCurRepairItemToPlayerPackage();
        }
    }

    public void SendToPlayer()
    {
        if (null != mRepairItem.ItemObj)
        {
            //if(PlayerFactory.mMainPlayer.GetItemPackage().GetEmptyGridCount(mRepairItem.ItemObj.mItemData.mTabIndex) > 0)
            //{
            //    PlayerFactory.mMainPlayer.AddItem(mRepairItem.ItemObj);
            //    RemoveItem();
            //}
        }
    }

    public void RemoveItem()
    {
        mRepairMachine.m_Repair.m_Item = null;
        mRepairItem.SetItem(null);
        UpdateItem(null);
    }

    public void OnDropItem(Grid_N grid)
    {
        if (this.mRepairMachine.m_Repair != null&& this.mRepairMachine.m_Repair.IsRepairingM)
        {
            return;
        }
        
        if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_HotKeyBar || SelectItem_N.Instance.Place == ItemPlaceType.IPT_Repair)
            return;

        //lz-2016.09.19 如果是ISO就返回，营地修理机不支持修ISO
        if (WhiteCat.CreationHelper.GetCreationItemClass(SelectItem_N.Instance.ItemObj) != WhiteCat.CreationItemClass.None)
        {
            //lz-2016.10.23 增加只能在基地修理的提示
            PeTipMsg.Register(PELocalization.GetString(8000843), PeTipMsg.EMsgLevel.Warning);
            return;
        }

        ActiveWnd();

        Repair dragRepairItem = SelectItem_N.Instance.ItemObj.GetCmpt<Repair>();
        if (dragRepairItem == null)
        {
            return;
        }

        if (dragRepairItem.protoData.repairMaterialList == null || dragRepairItem.protoData.repairMaterialList.Count == 0)
            return;

        //多人模式
        if (Pathea.PeGameMgr.IsMulti)
        {
            int[] itemlist = new int[1];
            itemlist[0] = SelectItem_N.Instance.ItemObj.instanceId;
            _net.InsertItemList(itemlist);
            return;
        }
        else
        {
            //lz-2016.07.29 在修理机拖入物品的时候，如果里面有东西就先尝试取出里面的物品到背包，执行成功的时候再放进新的物品
            if (null == mRepairItem.ItemObj)
            {
                UpdateItem(dragRepairItem);
                SelectItem_N.Instance.RemoveOriginItem();
                SelectItem_N.Instance.SetItem(null);
            }
            else
            {
                //lz-2016.10.19 如果拖动的Item和放置的Item是同一类型，就直接交换ItemObj数据
                ItemPackage.ESlotType dropType = ItemPackage.GetSlotType(mRepairItem.ItemObj.protoId);
                ItemPackage.ESlotType dragType = ItemPackage.GetSlotType(dragRepairItem.itemObj.protoId);
                if (dropType == dragType && null != SelectItem_N.Instance.Grid)
                {
                    if (SelectItem_N.Instance.Grid.onGridsExchangeItem != null)
                    {
                        SelectItem_N.Instance.Grid.onGridsExchangeItem(SelectItem_N.Instance.Grid, mRepairItem.ItemObj);
                        UpdateItem(dragRepairItem);
                        SelectItem_N.Instance.SetItem(null);
                    }
                }
                //lz-2016.10.19 如果不是同一类型，或者没有Grid，就先添加，后移除
                else
                {
                    if (TryAddCurRepairItemToPlayerPackage())
                    {
                        UpdateItem(dragRepairItem);
                        SelectItem_N.Instance.RemoveOriginItem();
                        SelectItem_N.Instance.SetItem(null);
                    }
                }
            }
        }
    }

    public void DropItemByNet(MapObjNetwork net, int _instanceId)
    {
        if (_net != null && _net == net)
        {
            if (mRepairMachine == null)
                return;
            if (_instanceId != -1)
            {
				ItemObject itemObj = ItemMgr.Instance.Get(_instanceId);
				if(null != itemObj)
					UpdateItem(itemObj.GetCmpt<Repair>());
                SelectItem_N.Instance.SetItem(null);
                GameUI.Instance.mItemPackageCtrl.ResetItem();
            }
            else
            {
                mRepairMachine.m_Repair.m_Item = null;
                mRepairItem.SetItem(null);
                UpdateItem(null);
                GameUI.Instance.mItemPackageCtrl.ResetItem();
            }
        }
    }

    public void ResetItemByNet(MapObjNetwork net, int _instanceId)
    {
        if (this.mRepairMachine.m_Repair==null||!this.mRepairMachine.m_Repair.IsRepairingM)
            return;
        if (_net != null && _net == net)
        {
            if (mRepairMachine == null)
                return;
            if (_instanceId != -1)
            {
                Repair repairItem = ItemMgr.Instance.Get(_instanceId).GetCmpt<Repair>();
                if (repairItem == this.mRepairMachine.m_Repair.m_Item)
                {
                    this.mRepairMachine.m_Repair.StopCounter();
                    this.UpdateBtnState();
                }
            }
        }
    }

    #endregion 

    #region private methods

    bool TryAddCurRepairItemToPlayerPackage()
    {
        //log:lz-2016.05.04  唐小力提的bug：错误 #1909修理机鼠标右键取回物品
        if (null != mRepairItem.ItemObj)
        {
            if (m_PlayerPackageCmpt.package.CanAdd(mRepairItem.ItemObj))
            {
                m_PlayerPackageCmpt.package.AddItem(mRepairItem.ItemObj);
                this.RemoveItem();
                GameUI.Instance.mItemPackageCtrl.ResetItem();
                return true;
            }
            else
            {
                //lz-2016.10.19 背包已满提示
                MessageBox_N.ShowOkBox(PELocalization.GetString(9500312));
                return false;
            }
        }
        return false;
    }

    void UpdateItem(Repair obj)
    {
        if (null != obj)
        {
            mCurrentD.text = PELocalization.GetString(82220001)+": "+Mathf.CeilToInt(obj.GetValue().current*0.01f);
            mAddD.text = "(+" + (Mathf.CeilToInt(obj.GetValue().ExpendValue *0.01f)).ToString() + ")";
        }
        else
        {
            mCurrentD.text = PELocalization.GetString(82220001)+": 0";
            mAddD.text = "(+0)";
        }

        foreach (CSUI_MaterialGrid mi in mItemList)
        {
            mi.gameObject.SetActive(false);
            mi.transform.parent = null;
            Destroy(mi.gameObject);
        }
        mItemList.Clear();

        if (null == obj)
        {
            //lz-2016.08.18 然会的时候需要更新按钮的状态
            this.UpdateBtnState();
            return;
        }

        mRepairItem.SetItem(obj.itemObj);

        mRepairMachine.m_Repair.m_Item = obj;
        foreach (ItemAsset.MaterialItem item in obj.protoData.repairMaterialList)
        {
            int n = Mathf.CeilToInt(item.count * c_MultiplierOfCost * (1 - obj.GetValue().percent));
            AddCostItem(item.protoId, n < 0 ? 0 : n);
        }
        mCostItemGrid.Reposition();
        this.UpdateBtnState();
    }

    void AddCostItem(int id, int num)
    {
        CSUI_MaterialGrid mi = Instantiate(mPerfab) as CSUI_MaterialGrid;
        mi.transform.parent = mCostItemGrid.transform;
        mi.transform.localPosition = Vector3.zero;
        mi.transform.rotation = Quaternion.identity;
        mi.transform.localScale = Vector3.one;
        mi.ItemID = id;
        mi.NeedCnt = num;
        mi.ItemNum = m_PlayerPackageCmpt.package.GetCount(id);
        mItemList.Add(mi);
    }

    void UpdateBtnState()
    {
        if (null == mRepairMachine||null == mRepairMachine.m_Repair)
        {
            this.RepairBtn.isEnabled = false;
            this.ResetBtn.isEnabled = false;
        }
        else
        {
            if (null==mRepairMachine.m_Repair.m_Item||mRepairMachine.m_Repair.m_Item.GetValue().ExpendValue == 0f)
            {
                this.RepairBtn.isEnabled = false;
                this.ResetBtn.isEnabled = false;
            }
            else
            {
                if (mRepairMachine.m_Repair.IsRepairingM)
                {
                    this.RepairBtn.isEnabled = false;
                    this.ResetBtn.isEnabled = true;
                }
                else
                {
                    this.RepairBtn.isEnabled = true;
                    this.ResetBtn.isEnabled = false;
                }
            }
        }
    }

    void OnRepairBtn()
    {
        if (mRepairItem == null || mRepairItem.ItemObj == null)
            return;
        MessageBox_N.ShowYNBox(PELocalization.GetString(8000098), this.Repair);
    }

    void Repair()
    {
        Repair repairItem = mRepairItem.ItemObj.GetCmpt<Repair>();
        if (null != repairItem && !repairItem.GetValue().IsCurrentMax())
        {
            bool itemenough = true;
            string des = PELocalization.GetString(8000026)+ " [ffff00]";
            foreach (ItemAsset.MaterialItem item in repairItem.protoData.repairMaterialList)
            {
                int n = Mathf.CeilToInt(item.count * c_MultiplierOfCost * (1 - repairItem.GetValue().percent));
                int max = m_PlayerPackageCmpt.package.GetCount(item.protoId);
                if (max < n)
                {
                    des += " " + ItemProto.Mgr.Instance.Get(item.protoId).GetName() + "";
                    itemenough = false;
                    break;
                }
            }
            //if(PlayerFactory.mMainPlayer.GetItemNum(mi.ItemId) < mi.needNum)
            //{
            //    itemenough = false;
            //    ItemProto item = ItemProto.GetItemData(mi.ItemId);
            //    des += " " + item.GetName() + "";
            //    MessageBox_N.ShowOkBox(PELocalization.GetString(8000026) + item.GetName() + " " + PELocalization.GetString(8000027));
            //    return;
            //}
            if (!itemenough)
            {
                des += "[-]" + " " + PELocalization.GetString(8000027);
                MessageBox_N.ShowOkBox(des);
                return;
            }

            if (PeGameMgr.IsMulti)
            {
                if (null != mRepairItem.ItemObj)
                    _net.RequestRepair(mRepairItem.ItemObj.instanceId);
                return;
            }

            foreach (ItemAsset.MaterialItem item in repairItem.protoData.repairMaterialList)
            {
                int n = Mathf.CeilToInt(item.count * c_MultiplierOfCost * (1 - repairItem.GetValue().percent));
                m_PlayerPackageCmpt.package.Destroy(item.protoId, n);
            }
            GameUI.Instance.mItemPackageCtrl.ResetItem();
            this.mRepairMachine.m_Repair.StartCounter();
            this.mRepairMachine.m_Repair.onRepairedTimeUp = this.RepairComplate;
            UpdateItem(repairItem);
        }
    }

    void OnResetBtn()
    {
        MessageBox_N.ShowYNBox(PELocalization.GetString(8000100), Reset);
    }

    void Reset()
    {
        if (!GameConfig.IsMultiMode)
        {
            //lz-2016.11.03 错误 #5440 Crush Bug    
            if (null != mRepairMachine && null != mRepairMachine.m_Repair)
            {
                mRepairMachine.m_Repair.StopCounter();
                UpdateBtnState();
            }
        }
        else
        {
            if (null != _net && null != mRepairItem && null != mRepairItem.ItemObj)
            {
                _net.RequestStopRepair(mRepairItem.ItemObj.instanceId);
            }
        }
    }

    void RepairComplate(Repair item)
    {
        Repair repairItem = mRepairItem.ItemObj.GetCmpt<Repair>();
        if (repairItem == item)
        {
            repairItem.Do();
            GameUI.Instance.mItemPackageCtrl.ResetItem();
            UpdateItem(repairItem);
        }
    }

    void Update()
    {
        if (null != GameUI.Instance.mMainPlayer && mMachinePos != Vector3.zero)
        {
            if (Vector3.Distance(GameUI.Instance.mMainPlayer.position, mMachinePos) > 8f)
                OnClose();
        }
        if (mRepairItem == null || mRepairItem.ItemObj == null)
        {
            return;
        }

        if (this.mRepairMachine.m_Repair != null && this.mRepairMachine.m_Repair.IsRepairingM)
        {
            this.m_CostTimeLabel.text = CSUtils.GetRealTimeMS((int)mRepairMachine.m_Repair.CostsTime);
        }
        else
        {
            this.m_CostTimeLabel.text = "00:00";
        }
        //if(null != PlayerFactory.mMainPlayer && null != mRepairMachine)
        //{
        //    if(Vector3.Distance(PlayerFactory.mMainPlayer.transform.position,mRepairMachine.transform.position) > 10f)
        //        OnClose();
        //}
    }

    #endregion
}
