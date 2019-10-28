using UnityEngine;
using ItemAsset;
using ItemAsset.PackageHelper;

public class CSUI_Grid : MonoBehaviour
{
    // Grid Prefab
    [SerializeField]
    private Grid_N m_GridPrefab;

    [HideInInspector]
    public Grid_N m_Grid;

    // Animation Glow
    [SerializeField]
    private TweenPosition m_GlowTween;

    public UIButtonTween m_TargetBT;

    public UILabel m_ZeroNumLab;

    public bool bUseZeroLab = false;

    public int m_Index = -1;

    public bool m_Active = true;

    public bool m_UseDefaultExchangeDel = true;

    public void PlayGlow(bool forward)
    {
        m_GlowTween.gameObject.SetActive(true);
        m_GlowTween.Reset();
        m_GlowTween.Play(true);
    }

    void OnTweenFinished(UITweener tween)
    {
        m_GlowTween.gameObject.SetActive(false);
    }

    public enum ECheckItemType
    {
        OnDrop,
        LeftMouseClick,
        RightMounseClick
    }
    public delegate bool CheckItemDelegate(ItemObject item, ECheckItemType checkType);
    public CheckItemDelegate onCheckItem;
    public delegate void ItemChangedDelegate(ItemObject item, ItemObject oldItem, int index);
    public ItemChangedDelegate OnItemChanged;
    public ItemChangedDelegate ItemExchanged;

    //public event System.Action<Grid_N> OnDropItemEvent;
    //public event System.Action<Grid_N> OnLeftMouseClickedEvent;
    //public event System.Action<Grid_N> OnRightMouseClickedEvent;

    public delegate void OnMultiOperation(Grid_N grid,int index);
    public OnMultiOperation OnDropItemMulti;
    public OnMultiOperation OnLeftMouseClickedMulti;
    public OnMultiOperation OnRightMouseClickedMulti;
    #region GRID_CALL_BACK

    void OnDropItem(Grid_N grid)
    {
        //1.check
        if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_HotKeyBar && SelectItem_N.Instance.Place == ItemPlaceType.IPT_Null)
        {
            SelectItem_N.Instance.SetItem(null);
            return;
        }

        if (onCheckItem != null && !onCheckItem(SelectItem_N.Instance.ItemObj, ECheckItemType.OnDrop))
            return;

        //2.send 3.do
        if (GameConfig.IsMultiMode)
        {
            OnDropItemMulti(grid,m_Index);
            return;
        }

        ItemObject dragitem = SelectItem_N.Instance.ItemObj;

        if (dragitem == null)
            return;

        ItemObject dropItem = grid.ItemObj;
        ItemObject oldItem = grid.ItemObj;
        if (dropItem == null)
        {
            grid.SetItem(dragitem);
            if (OnItemChanged != null)
                OnItemChanged(dragitem, oldItem, m_Index);

            SelectItem_N.Instance.RemoveOriginItem();
            SelectItem_N.Instance.SetItem(null);
        }
        else
        {
            Pathea.PlayerPackageCmpt pkg = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.PlayerPackageCmpt>();
            ItemPackage.ESlotType dropType = ItemPackage.GetSlotType(dropItem.protoId);
            ItemPackage.ESlotType dragType = ItemPackage.GetSlotType(dragitem.protoId);

            //lz-2016.10.18 如果拖动的Item和放置的Item是同一类型，就直接交换ItemObj数据
            if (dropType == dragType&& null !=SelectItem_N.Instance.Grid)
            {
                if (SelectItem_N.Instance.Grid.onGridsExchangeItem != null)
                {
                    SelectItem_N.Instance.Grid.onGridsExchangeItem(SelectItem_N.Instance.Grid, dropItem);
                    grid.SetItem(dragitem);
                    SelectItem_N.Instance.SetItem(null);
                    if (OnItemChanged != null)
                        OnItemChanged(dragitem, oldItem, m_Index);
                }
            }
            //lz-2016.10.18 如果不是同一类型，或者没有Grid，就先添加，后移除
            else if (pkg.package.CanAdd(dropItem))
            {
                pkg.package.AddItem(dropItem);
                grid.SetItem(dragitem);
                SelectItem_N.Instance.RemoveOriginItem();
                SelectItem_N.Instance.SetItem(null);
            }
            if (OnItemChanged != null)
                OnItemChanged(dragitem, oldItem, m_Index);
        }
    }

    void OnGridsExchangeItem(Grid_N grid, ItemObject item)
    {
        grid.SetItem(item);
    }

    void OnLeftMouseClicked(Grid_N grid)
    {
        if (grid.Item == null)
            return;

        if (onCheckItem != null && !onCheckItem(null, ECheckItemType.LeftMouseClick))
            return;

        if (GameConfig.IsMultiMode)
        {
            OnLeftMouseClickedMulti(grid,m_Index);
            return;
        }

        SelectItem_N.Instance.SetItemGrid(grid);
    }

    void OnRightMouseClicked(Grid_N grid)
    {
        if (grid.Item == null)
            return;

        if (onCheckItem != null && !onCheckItem(null, ECheckItemType.RightMounseClick))
            return;

        if (GameConfig.IsMultiMode)
        {
            OnRightMouseClickedMulti(grid,m_Index);
            return;
        }

        ItemObject oldItem = grid.ItemObj;

        Pathea.PlayerPackageCmpt package = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.PlayerPackageCmpt>();

        if (package.package.CanAdd(grid.ItemObj))
        {
            package.Add(grid.ItemObj);
            GameUI.Instance.mItemPackageCtrl.ResetItem();
            grid.SetItem(null);

            if (OnItemChanged != null)
                OnItemChanged(null, oldItem, m_Index);
        }
        else
        {
            CSUI_MainWndCtrl.ShowStatusBar(UIMsgBoxInfo.mNotEnoughGrid.GetString(), Color.red);
        }
    }

    void OnRemoveOriginItem(Grid_N grid)
    {
        ItemObject oldItem = grid.ItemObj;
        grid.SetItem(null);

        if (OnItemChanged != null)
            OnItemChanged(null, oldItem, m_Index);
    }

    #endregion

    void Awake()
    {
        m_Grid = Instantiate(m_GridPrefab) as Grid_N;
        m_Grid.transform.parent = transform;
        m_Grid.transform.localPosition = Vector3.zero;
        m_Grid.transform.localRotation = Quaternion.identity;
    }

    // Use this for initialization
    void Start()
    {
        if (m_TargetBT == null)
            m_GlowTween.onFinished = OnTweenFinished;

        m_ZeroNumLab.enabled = false;

        if (m_Active)
        {
            m_Grid.SetItemPlace(ItemPlaceType.IPT_Null, 0);
            m_Grid.onDropItem = OnDropItem;
            m_Grid.onLeftMouseClicked = OnLeftMouseClicked;
            m_Grid.onRightMouseClicked = OnRightMouseClicked;
            m_Grid.onRemoveOriginItem = OnRemoveOriginItem;
        }

        if (m_UseDefaultExchangeDel)
            m_Grid.onGridsExchangeItem = OnGridsExchangeItem;

    }

    // Update is called once per frame
    void Update()
    {

        if (bUseZeroLab)
        {
            if (m_Grid.Item != null)
            {
                m_ZeroNumLab.enabled = (m_Grid.Item.stackCount == 0);
            }
            else
                m_ZeroNumLab.enabled = false;
        }
        else
            m_ZeroNumLab.enabled = false;
    }
}
