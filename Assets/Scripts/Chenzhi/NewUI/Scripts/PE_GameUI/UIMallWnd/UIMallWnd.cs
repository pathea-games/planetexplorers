using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface  MallItemData
{
	int GetID();  // 物品ID
	string GetSprName();
	string GetName(); // 名字
	int GetPrice();  // 价钱 		
	int GetDiscount(); // 折扣
	bool ShowDiscount(); // 是否显示折扣
	int GetCount();
	int GetItemType ();//物品类型
}

public enum Mall_Tab
{
	tab_Hot,
	tab_Tools,
	tab_Clothes,
	tab_Face,
	tab_Item,
	tab_Equip
}

public class UIMallWnd : UIBaseWnd 
{
	static UIMallWnd 							mInstance;
	public static UIMallWnd 					Instance{ get{ return mInstance; } }

	[SerializeField] GameObject 				mMailItemPrefab; 
	[SerializeField] UIGrid						mGrid;
	[SerializeField] UILabel					mLbPageText;
	[SerializeField] UILabel					mLbMyPrice;
						
	List<UIMallItem>							mMallItemList;								
	int pageIndex = 0;
	int maxPageIndex = 1;
	public const int GridCount = 12;
	int maxItemCount = 0;
	int myPrice;

	void Awake()
	{
		if (mInstance == null)
			mInstance = this;
		mMallItemList = new List<UIMallItem>();
	}

	void Start()
	{
		InitGrid();
		ClearPageInfo();
		
		//GameClientLobby.Self.GetShopDataAll ();
		e_Reflash += UILobbyShopItemMgr._self.MallItemEvent;
		e_OnBuyItemClick += UILobbyShopItemMgr._self.MallItemBuyEvent;
		e_ItemExportClick += UILobbyShopItemMgr._self.MallItemExportEvent;
		base.OnCreate ();
		SetMyBalance ((int)(AccountItems.self.balance));
	}


	void InitGrid()
	{
		for ( int i=0; i<GridCount ; i++ )
		{
			GameObject go = GameObject.Instantiate( mMailItemPrefab ) as GameObject;
			go.transform.parent = mGrid.transform;
			go.transform.localScale = Vector3.one;
			go.transform.localPosition = Vector3.zero;
			UIMallItem item = go.GetComponent<UIMallItem>();
			item.mCheckBox.radioButtonRoot = gameObject.transform;
			item.e_ItemBuy += MallItemBuy_OnClick;
			item.e_OnClick += MallItemSelect;
			item.e_ItemExport += MallItemExport_OnClick;
			mMallItemList.Add(item);
		}
		mGrid.repositionNow = true;
	}

	void BtnLeftOnClick()
	{
		if (pageIndex > 0)
		{
			pageIndex --;
			Reflash();
		}
	}
	
	void BtnRightOnClick()
	{
		if (pageIndex < maxPageIndex -1 )
		{
			pageIndex++;
			Reflash();
		}
	}


	void Reflash()
	{
		ClearPageInfo();
		int starIndex = pageIndex * 12; 
		if ( e_Reflash != null )
			e_Reflash(starIndex, mCurrentTab);
	}


	void Update()
	{
		maxPageIndex = (maxItemCount % GridCount == 0) ?   maxItemCount / GridCount : GridCount / GridCount + 1;
		if (maxPageIndex == 0)
			maxPageIndex = 1;
		mLbPageText.text = (pageIndex + 1).ToString() + "/" + maxPageIndex.ToString();
	}

	void ClearPageInfo()
	{
		for ( int i=0; i< GridCount; i++ )
			mMallItemList[i].ClearInfo();
	}

	void MallItemBuy_OnClick(int index,UIMallItem item)
	{
		if (e_OnBuyItemClick != null)
			e_OnBuyItemClick(mCurrentTab, item.mData);
	}

	void MallItemExport_OnClick(int index, UIMallItem item)
	{
		if (e_ItemExportClick != null)
			e_ItemExportClick(mCurrentTab, item.mData);
	}

	void MallItemSelect(int index,UIMallItem item)
	{
		if( e_ItemOnSelected != null)
			e_ItemOnSelected(mCurrentTab,item.mData);
	}

	void BtnHot_OnClick()
	{
		if (mCurrentTab != Mall_Tab.tab_Hot)
		{
			pageIndex = 0;
			mCurrentTab = Mall_Tab.tab_Hot;
			Reflash();
		}
	}
	
	
	void BtnTools_OnClick()
	{
		if (mCurrentTab != Mall_Tab.tab_Tools)
		{
			pageIndex = 0;
			mCurrentTab = Mall_Tab.tab_Tools;
			Reflash();
		}
	}
	
	void BtnClothes_OnClick()
	{
		if (mCurrentTab != Mall_Tab.tab_Clothes)
		{
			pageIndex = 0;
			mCurrentTab = Mall_Tab.tab_Clothes;
			Reflash();
		}
	}
	
	
	void BtnFace_OnClick()
	{
		if (mCurrentTab != Mall_Tab.tab_Face)
		{
			pageIndex = 0;
			mCurrentTab = Mall_Tab.tab_Face;
			Reflash();
		}
	}
	
	void BtnItem_OnClick()
	{
		if (mCurrentTab != Mall_Tab.tab_Item)
		{
			pageIndex = 0;
			mCurrentTab = Mall_Tab.tab_Item;
			Reflash();
		}
	}
	
	
	void BtnEquip_OnClick()
	{
		if (mCurrentTab != Mall_Tab.tab_Equip)
		{
			pageIndex = 0;
			mCurrentTab = Mall_Tab.tab_Equip;
			Reflash();
		}
	}

	#region viewData

	// 刷新窗口事件
	public delegate void MallItemEvent(int starIndex, Mall_Tab tabIndex);
	public event MallItemEvent e_Reflash = null;
	// 点击买东西的事件
	public delegate void MallItemBuyEvent(Mall_Tab tabIndex, MallItemData mallItem);
	public event MallItemBuyEvent e_OnBuyItemClick = null;
	// 选中事件
	public event MallItemBuyEvent e_ItemOnSelected = null;
	// 导出物品
	public event MallItemBuyEvent e_ItemExportClick = null;

	// 设置价格
	public void SetMyBalance(int price)
	{
		mLbMyPrice.text = price.ToString() + " [C8C800]PP[-]"; 
	}
	// 设置页面信息 12个信息
	public void SetPageInfo( int _maxItemCount ,  List<MallItemData> itemDataList )
	{
		for ( int i=0; i< GridCount; i++ )
		{
			if (i < itemDataList.Count)
				mMallItemList[i].SetInfo(itemDataList[i],i);
			else 
				mMallItemList[i].ClearInfo();
		}
		maxItemCount = _maxItemCount;
	}
	
	public Mall_Tab mCurrentTab = Mall_Tab.tab_Hot;
	#endregion
}



