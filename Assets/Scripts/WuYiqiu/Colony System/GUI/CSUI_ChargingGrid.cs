using UnityEngine;
using System.Collections;
using ItemAsset;
using ItemAsset.PackageHelper;
using Pathea;
public class CSUI_ChargingGrid : MonoBehaviour
{
    public bool m_bCanChargeLargedItem = true;

    [SerializeField]
    private Grid_N m_GridPrefab;
    [SerializeField]
    private UISlider m_Silder;
    [SerializeField]
    private UILabel m_Label;
    [SerializeField]
    private UISlicedSprite m_EmptySprite;

    private Grid_N m_Grid;

    public ItemObject Item { get { return m_Grid.ItemObj; } }
    public Energy energyItem;
    public int m_Index;
    public bool m_bUseMsgBox = true;

    // CallBack
    public delegate void OnItemChangedDel(int index, ItemObject item);
    public OnItemChangedDel onItemChanded;
    public delegate bool OnItemCheck(ItemObject item);
    public OnItemCheck onItemCheck;

    public delegate void OnMultiOperation(int index, Grid_N grid);
    public OnMultiOperation OnDropItemMulti;
    public OnMultiOperation OnLeftMouseClickedMulti;
    public OnMultiOperation OnRightMouseClickedMulti;

    public bool IsChargeable(ItemObject itemObj)
    {
        if (null == itemObj)
        {
            return false;
        }

        ItemAsset.Energy energy = itemObj.GetCmpt<ItemAsset.Energy>();

		if (null == energy||itemObj.protoData.unchargeable)
        {
            if (m_bUseMsgBox)
                //lz-2016.10.24 把对话框的提示改成左上角提示
                PeTipMsg.Register(PELocalization.GetString(8000094),PeTipMsg.EMsgLevel.Error);
            else
                CSUI_MainWndCtrl.ShowStatusBar(PELocalization.GetString(8000094), Color.red);
            return false;
        }

        if ((!(energy is ItemAsset.EnergySmall)) && !m_bCanChargeLargedItem)
        {
            if (m_bUseMsgBox)
                //lz-2016.10.24 把对话框的提示改成左上角提示
                PeTipMsg.Register(PELocalization.GetString(8000095), PeTipMsg.EMsgLevel.Warning);
            else
                CSUI_MainWndCtrl.ShowStatusBar(PELocalization.GetString(8000095), Color.red);
            return false;
        }

        return true;
    }

    /// <summary>
    /// multiMode used only
    /// </summary>
    public void SetItemUI(ItemObject itemObj)
    {
        m_Grid.SetItem(itemObj);
        if (itemObj != null)
            energyItem = itemObj.GetCmpt<Energy>();
        else
            energyItem = null;
    }

    public bool SetItem(ItemObject itemObj)
    {
        if (itemObj == null)
        {
            if (onItemChanded != null)
                onItemChanded(m_Index, itemObj);
            m_Grid.SetItem(itemObj);
            energyItem = null;
            return true;
        }

        if (!IsChargeable(itemObj))
        {
            return false;
        }
        //if (itemObj.instanceId < CreationData.s_ObjectStartID)
        //{
        //    if (!(itemObj is LifeLimitItem))
        //    {
        //        if (m_bUseMsgBox)
        //            MessageBox_N.ShowOkBox(PELocalization.GetString(8000094));
        //        else
        //            CSUI_MainWndCtrl.ShowStatusBar(PELocalization.GetString(8000094), Color.red);
        //        return false;
        //    }
        //}
        //else if (m_bCanChargeLargedItem)
        //{
        //    if ((itemObj.prototypeData.m_OpType & ItemOperationType.EquipmentItem) != 0)
        //    {
        //        if (m_bUseMsgBox)
        //            MessageBox_N.ShowOkBox(PELocalization.GetString(8000094));
        //        else
        //            CSUI_MainWndCtrl.ShowStatusBar(PELocalization.GetString(8000094), Color.red);
        //        return false;
        //    }

        //}
        //else
        //{
        //    if (m_bUseMsgBox)
        //        MessageBox_N.ShowOkBox(PELocalization.GetString(8000095));
        //    else
        //        CSUI_MainWndCtrl.ShowStatusBar(PELocalization.GetString(8000095), Color.red);
        //    return false;
        //}

        if (onItemChanded != null)
            onItemChanded(m_Index, itemObj);
        m_Grid.SetItem(itemObj);
        energyItem = itemObj.GetCmpt<Energy>();
        return true;
    }

    #region GRID_FUNC

    void OnDropItem(Grid_N grid)
    {
        //lz-2016.10.25 因为多人暂时只支持从背包中拖到充电中，所以统一多人和单人，避免出现显示问题
        if (SelectItem_N.Instance.Place != ItemPlaceType.IPT_Bag)
            return;

        if (onItemCheck != null && !onItemCheck(grid.ItemObj))
            return;

        if (grid.Item == null)
        {
            switch (SelectItem_N.Instance.Place)
            {
                case ItemPlaceType.IPT_HotKeyBar:
                    SelectItem_N.Instance.SetItem(null);
                    break;
                default:
                    if (SetItem(SelectItem_N.Instance.ItemObj))
					{
						if (GameConfig.IsMultiMode)
						{
							if (OnDropItemMulti != null)
								OnDropItemMulti(m_Index, grid);
						}
                        SelectItem_N.Instance.RemoveOriginItem();
                        SelectItem_N.Instance.SetItem(null);

                        if (!m_bUseMsgBox)
                            CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mStartToCharge.GetString(), grid.Item.protoData.GetName()));
                        //						CSUI_Main.ShowStatusBar("Start to charge the " + grid.Item.mItemData.m_Englishname + ".");
                    }
                    else
                    {
                        if (!m_bUseMsgBox)
                            CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mNotToBeCharged.GetString(), SelectItem_N.Instance.ItemObj.protoData.GetName()), Color.red);
                        //						CSUI_Main.ShowStatusBar("The " + SelectItem_N.Instance.ItemObj.mItemData.m_Englishname + " is not need to be charged.", Color.red);
                    }
                    break;
            }

        }
        else
        {
            ItemObject io = grid.ItemObj;
            Pathea.PlayerPackageCmpt pkg = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.PlayerPackageCmpt>();
            if (pkg.package.CanAdd(io) && SetItem(SelectItem_N.Instance.ItemObj))
            {
                pkg.package.AddItem(io);
                SelectItem_N.Instance.RemoveOriginItem();
                SelectItem_N.Instance.SetItem(null);

                if (!m_bUseMsgBox)
                    if (grid.Item != null)
                        CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mStartToCharge.GetString(), grid.Item.protoData.GetName()));
                //					CSUI_Main.ShowStatusBar("Start to charge the " + grid.Item.mItemData.m_Englishname + ".");
            }
            else
            {
                if (!m_bUseMsgBox)
                    //					CSUI_Main.ShowStatusBar("The " + SelectItem_N.Instance.ItemObj.mItemData.m_Englishname + " is not need to be charged.", Color.red);
                    CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mNotToBeCharged.GetString(), SelectItem_N.Instance.ItemObj.protoData.GetName()), Color.red);
            }
        }
    }

    void OnLeftMouseClicked(Grid_N grid)
    {
        if (GameConfig.IsMultiMode)
        {
            if (OnLeftMouseClickedMulti != null)
                OnLeftMouseClickedMulti(m_Index, grid);
            return;
        }

        if (onItemCheck != null && !onItemCheck(grid.ItemObj))
            return;

        if (grid.Item == null)
            return;
        SelectItem_N.Instance.SetItemGrid(grid);
    }

    void OnRightMouseClicked(Grid_N grid)
    {
        if (GameConfig.IsMultiMode)
        {
            if (OnRightMouseClickedMulti != null)
                OnRightMouseClickedMulti(m_Index, grid);
            return;
        }

        if (onItemCheck != null && !onItemCheck(grid.ItemObj))
            return;

        GameUI.Instance.mItemPackageCtrl.Show();

        if (grid.ItemObj != null)
        {
            if (ItemPackage.InvalidIndex != PeCreature.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>().package.AddItem(grid.ItemObj))
            {
                GameUI.Instance.mItemPackageCtrl.ResetItem();
                SetItem(null);
            }
        }

    }

    void OnRemoveOriginItem(Grid_N grid)
    {
        SetItem(null);
    }

    #endregion

    void Awake()
    {
        m_Grid = Instantiate(m_GridPrefab) as Grid_N;
        m_Grid.transform.parent = transform;
        m_Grid.transform.localPosition = Vector3.zero;
        m_Grid.transform.localRotation = Quaternion.identity;
        m_Grid.transform.localScale = Vector3.one;

        m_Grid.onDropItem = OnDropItem;
        m_Grid.onLeftMouseClicked = OnLeftMouseClicked;
        m_Grid.onRightMouseClicked = OnRightMouseClicked;
        m_Grid.onRemoveOriginItem = OnRemoveOriginItem;
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (energyItem != null)
        {
            m_Silder.enabled = true;
            m_Label.enabled = true;
            m_EmptySprite.enabled = false;


            float percent = energyItem.energy.percent;
            m_Silder.sliderValue = percent;
            m_Label.text = Mathf.FloorToInt(percent * 100).ToString() + "%";

            //if (Item.instanceId < CreationData.s_ObjectStartID)
            //{


            //    float percent = Item.GetProperty(ItemProperty.BatteryPower) / Item.GetProperty(ItemProperty.BatteryPowerMax);
            //    m_Silder.sliderValue = percent;
            //    m_Label.text = Mathf.FloorToInt(percent * 100).ToString() + "%";
            //}
            //else
            //{
            //    CreationData cdata = CreationMgr.GetCreation(Item.instanceId);
            //    float percent = cdata.m_Fuel / cdata.m_Attribute.m_MaxFuel;
            //    m_Silder.sliderValue = percent;
            //    m_Label.text = Mathf.FloorToInt(percent * 100).ToString() + "%";
            //}
        }
        else
        {
            m_Silder.enabled = false;
            m_Label.enabled = false;
            m_EmptySprite.enabled = true;
            m_Silder.sliderValue = 0;
        }
    }
}
