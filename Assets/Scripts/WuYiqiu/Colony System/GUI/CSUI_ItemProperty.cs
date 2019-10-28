////unknown
//using UnityEngine;
//using System.Collections;
//using ItemAsset;

//public class CSUI_ItemProperty : MonoBehaviour 
//{
//    // Icon
//    [SerializeField]
//    protected Grid_N    m_IconGridPrefab;
//    [SerializeField]
//    protected Transform m_IcomRoot;
//    [SerializeField]
//    protected UILabel  m_ItemNameLb;
	
//    protected Grid_N	m_IconGrid;
	
//    public ItemObject	Item	{ 
//        get { 
//            if (m_IconGrid == null)
//                return null;
//            return m_IconGrid.ItemObj; 
//        } 
//        set {
//            if (m_IconGrid != null)
//                m_IconGrid.SetItem(value);
//        } 
//    } 
	
//    public bool m_Enable = true;
	
//    // Delgate
//    public delegate void ItemDelegate (ItemObject item);
//    public ItemDelegate onItemChanged ;
	
//    public virtual bool SetItem (ItemObject item)
//    {	
//        if (m_IconGrid == null)
//            InitIconGrid();
		
//        m_IconGrid.SetItem(item);
		
//        if (onItemChanged != null)
//            onItemChanged(item);
		
//        return true;
//    }
	
//    protected virtual void UpdateItemInfo ()
//    {
//        ItemObject item = m_IconGrid.ItemObj;
		
//        if (item == null)
//            m_ItemNameLb.text = "";
//        else
//            m_ItemNameLb.text = item.prototypeData.m_Name;
//    }
	
//    protected void InitIconGrid ()
//    {
//        m_IconGrid = Instantiate(m_IconGridPrefab) as Grid_N;
//        m_IconGrid.transform.parent 		= m_IcomRoot;
//        m_IconGrid.transform.localPosition 	= Vector3.zero;
//        m_IconGrid.transform.localRotation	= Quaternion.identity;
//        m_IconGrid.transform.localScale 	= Vector3.one;
		
//        m_IconGrid.onDropItem = OnDropItem;
//        m_IconGrid.onLeftMouseClicked = OnLeftMouseClicked;
//        m_IconGrid.onRemoveOriginItem = OnRemoveOriginItem;
//        m_IconGrid.onRightMouseClicked = OnRightMouseClicked;
//    }
	
//    #region GRID_N_CALLBACK
	
//    void OnDropItem (Grid_N grid)
//    {
//        if (!m_Enable || SelectItem_N.Instance.Place == ItemPlaceType.IPT_HotKeyBar)
//            return;
		
//        if (grid.Item == null)
//        {
//            switch(SelectItem_N.Instance.Place)
//            {
//            case ItemPlaceType.IPT_HotKeyBar:
//                SelectItem_N.Instance.SetItem(null);
//                break;
//            default:
//                if (SetItem(SelectItem_N.Instance.ItemObj))
//                {
//                    SelectItem_N.Instance.RemoveOriginItem();
//                    SelectItem_N.Instance.SetItem(null);
					
//                    if (onItemChanged != null)
//                        onItemChanged(Item);
//                }
//                break;
//            }
//        }
//        else
//        {
//            ItemObject io = grid.ItemObj;
//            Pathea.PackageCmpt pkg = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.PackageCmpt>();

//            if(pkg.pkgAccessor.CanAdd(grid.Item)
//                && SetItem(SelectItem_N.Instance.ItemObj))
//            {
//                PlayerFactory.mMainPlayer.AddItem(io);
//                SelectItem_N.Instance.RemoveOriginItem();
//                SelectItem_N.Instance.SetItem(null);
				
//                if (onItemChanged != null)
//                    onItemChanged(Item);
//            }
//        }
//    }
	
//    void OnLeftMouseClicked(Grid_N grid)
//    {
//        if (grid.Item == null || !m_Enable)	return;
		
//        SelectItem_N.Instance.SetItemGrid(grid);
//    }
	
//    void OnRightMouseClicked(Grid_N grid)
//    {
//        GameUI.Instance.mUIItemPackageCtrl.Show();
		
//        if (!m_Enable)	return;
		
//        if (grid.ItemObj != null)
//        {
//            if (ItemPackage.InvalidIndex != PlayerFactory.mMainPlayer.GetItemPackage().AddItem(grid.ItemObj))
//            {
//                GameUI.Instance.mUIItemPackageCtrl.ResetItem();
//                grid.SetItem(null);
				
//                if (onItemChanged != null)
//                    onItemChanged(Item);
//            }
//        }
//    }
	
//    void OnRemoveOriginItem (Grid_N grid)
//    {
//        grid.SetItem(null);	
		
//        if (onItemChanged != null)
//            onItemChanged(Item);
//    }
	
//    #endregion
//    // Use this for initialization
//    protected void Start () 
//    {
		
//    }
	
//    // Update is called once per frame
//    protected void Update ()
//    {
//        if (m_IconGrid == null)
//            return;
		
//        UpdateItemInfo();
//    }
//}
