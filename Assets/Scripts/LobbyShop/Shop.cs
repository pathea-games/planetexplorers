using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using uLobby;
using uLink;
using CustomData;
using ItemAsset;
using Pathea;


public class LobbyShopMgr
{
	static List<LobbyShopData> shopData = new List<LobbyShopData>();
	public static List<LobbyShopData> ShopData
	{
		get
		{
			return shopData;
		}
	}
	public static void Add(LobbyShopData data)
	{
		if (!shopData.Contains (data))
		{
			shopData.Add (data);
			UILobbyShopItem item = new UILobbyShopItem(data);
			UILobbyShopItemMgr._self.Add(item);
		}
	}
	public static void Remove(LobbyShopData data)
	{
		shopData.Remove (data);
	}
	public static void Add (LobbyShopData[] data)
	{
		for(int i = 0 ; i < data.Length; i ++)
		{
			if(!shopData.Contains(data[i]))
			{
				Add(data[i]);
			}
		}
	}
	public static void AddAll(LobbyShopData[] data)
	{
		for(int i = 0 ; i < data.Length; i ++)
		{
			LobbyShopData item = shopData.Find(iter => iter.id == data[i].id);
			if(item == null)
			{
				Add(data[i]);
			}
		}
		UILobbyShopItemMgr._self.MallItemEvent (0, Mall_Tab.tab_Hot);
	}
	public static void AddRange(LobbyShopData[] data,int startIndex,int tabIndex)
	{
		for(int i = 0 ; i < data.Length; i ++)
		{
			if(!shopData.Contains(data[i]))
			{
				Add(data[i]);
			}
		}
		UILobbyShopItemMgr._self.MallItemEvent (startIndex, (Mall_Tab)tabIndex);
	}

	public static int GetPrice(int id)
	{
		LobbyShopData data = shopData.Find (iter => { return iter.id == id;});
		if(data != null)
		{
			return data.price*data.rebate/100;
        }
        return -1;
    }

	public static int GetTab(int id)
	{
		LobbyShopData data = shopData.Find (iter => { return iter.id == id;});
		if(data != null)
		{
			return data.tab;
		}
		return -1;
	}

	public static int GetForbid(int itemType)
	{
		LobbyShopData data = shopData.Find (iter => { return iter.itemtype == itemType;});
		if(data != null)
			return data.forbid;
		return -1;
	}
}
public class UILobbyShopItem : MallItemData
{
	LobbyShopData _data;
	ItemProto _itemData;
	public UILobbyShopItem(LobbyShopData data)
	{
		_data = data;
		if (_data != null)
			_itemData = ItemProto.Mgr.Instance.Get (_data.itemtype);
		if (_itemData == null)
			Debug.LogError ("lobby shop _itemData is null itemtype = "+ _data.itemtype);
	}
	public int GetID()
	{
		return _data.id;  // 物品ID
	}
	public string GetSprName()
	{
		if(_itemData != null)
			return _itemData.shopIcon;
		return "";
	}
	public string GetName() // 名字
	{
		if(_itemData != null)
			return _itemData.GetName();
		return "";
	}
	public int GetPrice()  // 价钱 		
	{
		return _data.price;
	}
	public int GetDiscount() // 折扣
	{
		return _data.rebate;
	}
	public bool ShowDiscount() // 是否显示折扣
	{
		if (_data.rebate == 100)
			return false;
		return true;
	}
	public int GetTab()
	{
		return _data.tab;
	}
	public int GetItemType()
	{
		return _data.itemtype;
	}
	public int GetCount()
	{
		return 1;
	}
}

public class UIMyLobbyShopItem : MallItemData
{
	ItemProto _itemData;
	int _count;
	public int Count
	{
		get
		{
			return _count;
		}
		set
		{
			_count = value;
		}
	}
	public UIMyLobbyShopItem(ItemProto data,int count)
	{
		_itemData = data;
		_count = count;
		if (_itemData == null)
			Debug.LogError ("UIMyLobbyShopItem shop _itemData is null itemtype = "+ data.id);
	}
	public int GetID()
	{
		return -1;  // 流水D
	}
	public string GetSprName()
	{
		if(_itemData != null)
			return _itemData.shopIcon;
		return "";
	}
	public string GetName() // 名字
	{
		if(_itemData != null)
			return _itemData.GetName();
		return "";
	}
	public int GetPrice()  // 价钱 		
	{
		return -1;
	}
	public int GetDiscount() // 折扣
	{
		return -1;
	}
	public bool ShowDiscount() // 是否显示折扣
	{
		return false;
	}
	public int GetTab()
	{
		if (_itemData.equipType > EquipType.Null && _itemData.equipType <= EquipType.Trousers)
			return (int)Mall_Tab.tab_Equip;
		else
			return (int)Mall_Tab.tab_Item;
	}
	public int GetItemType()
	{
		return _itemData.id;
	}
	public int GetCount()
	{
		return _count;
	}
}

public class UILobbyShopItemMgr
{
	public static UILobbyShopItemMgr _self = new UILobbyShopItemMgr();
	Dictionary<int,List< UILobbyShopItem>> _items = new Dictionary<int, List<UILobbyShopItem>> ();		
	public const int GAMEMODE_VS = 0x0001;
	public const int GAMEMODE_COOPERATION = 0x0002;
	public const int GAMEMODE_SURVIVE = 0x0004;

	public void Add(UILobbyShopItem item)
	{
		if (!_items.ContainsKey (item.GetTab ()))
			_items [item.GetTab ()] = new List<UILobbyShopItem> ();
		_items[item.GetTab ()].Add (item);
	}
	public void Remove(UILobbyShopItem item)
	{
		if (!_items.ContainsKey (item.GetTab ()))
			return;
		_items[item.GetTab ()].Remove (item);
	}
	#region ui register event
	public void MallItemEvent(int starIndex, Mall_Tab tabIndex)
	{
		int tab = (int)tabIndex;
		List< MallItemData> items = new List<MallItemData> ();
		int maxCount = 0;
		if(tabIndex<Mall_Tab.tab_Item)
		{
			if (!_items.ContainsKey (tab))
				return;
			if (_items [tab].Count < starIndex)
				return;
			for(int i = starIndex; i < starIndex + UIMallWnd.GridCount; i++)
			{
				if(i >= _items [tab].Count)
					break;
				items.Add((MallItemData)(_items [tab][i]));
			}
			maxCount = _items[tab].Count;
		}
		else
		{
			//from player package
			List< MallItemData> myItems = GetMyShopItemsByTab((int)tabIndex);
			maxCount = myItems.Count;
			if (maxCount < starIndex)
				return;
			for(int i = starIndex; i < starIndex + UIMallWnd.GridCount; i++)
			{
				if(i >= myItems.Count)
					break;
				items.Add((MallItemData)(myItems[i]));
			}
		}
		UIMallWnd.Instance.SetPageInfo (maxCount,items);
	}
	public void MallItemBuyEvent(Mall_Tab tabIndex, MallItemData mallItem)
	{
		int tab = (int)tabIndex;
		if (!_items.ContainsKey (tab))
			return;
		BuyItems (mallItem.GetID (), 1);
	}
	public void MallItemExportEvent(Mall_Tab tabIndex, MallItemData mallItem)
	{
		if(mallItem == null )
		{
			Debug.LogError("mallItem is null");
			return;
		}
//		int id = mallItem.GetID ();
		if(AccountItems.self.CheckCreateItems(mallItem.GetItemType(),1))
		{
			CreateAccountItems(mallItem.GetItemType(),1);
		}
	}
	#endregion
	#region interface
	public bool BuyItems(int id,int amount)
	{
		int price = LobbyShopMgr.GetPrice (id);
		if( price <= 0 ||  price * amount > (int)(AccountItems.self.balance))
			return false;
		LobbyInterface.LobbyRPC(ELobbyMsgType.BuyItems, id,amount);
		return true;
	}
	
	public bool CreateAccountItems(int itemType,int amount)
	{
		int forbid = LobbyShopMgr.GetForbid ( itemType );
		switch(PeGameMgr.gameType)
		{
		case PeGameMgr.EGameType.Cooperation:
		{
			if( (forbid & GAMEMODE_COOPERATION) == 1 )
			{
				return false;
			}
		}
			break;
		case PeGameMgr.EGameType.VS:
		{
			if( (forbid & GAMEMODE_VS) == 1 )
			{
				return false;
			}
		}
			break;
		case PeGameMgr.EGameType.Survive:
		{
			if( (forbid & GAMEMODE_SURVIVE) == 1 )
			{
				return false;
			}
		}
			break;
		}
		PlayerNetwork.mainPlayer.CreateAccountItems (itemType, amount);
		return true;
	}

	public Dictionary <int,int> MyShopItems()
	{
		return AccountItems.self.MyShopItems;
	}
	public List<MallItemData> GetMyShopItemsByTab(int tab)
	{
		List< MallItemData> items = new List<MallItemData> ();
		foreach(var iter in MyShopItems())
		{
			ItemProto itemData = ItemProto.Mgr.Instance.Get (iter.Key);
			if(itemData != null)
			{
				UIMyLobbyShopItem item = new UIMyLobbyShopItem(itemData,iter.Value);
				if(item.GetTab() == tab)
					items.Add((MallItemData)item);
			}
		}
		return items;
	}
	#endregion
}

