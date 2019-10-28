using ItemAsset;
using ItemAsset.PackageHelper;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathea;

public interface IDroppableItemList
{
	int DroppableItemCount{ get; }
	ItemSample GetDroppableItemAt(int idx);
	void AddDroppableItem(ItemSample item);
	void RemoveDroppableItem(ItemSample item);
	void RemoveDroppableItemAll();
}

public interface IItemDrop
{
    ItemSample Get(int index);
    int GetCount();
    void Fetch(int index);
    void FetchAll();
}

public class ItemDrop : MonoBehaviour, IItemDrop, IDroppableItemList
{
    public System.Action fetchItem;
	MapObjNetwork _net;
	public void SetNet(MapObjNetwork net)
	{
		_net = net;
	}
    const float PickDistance = 8f;
    protected List<ItemSample> _itemList = new List<ItemSample>(1);

    protected PlayerPackageCmpt mPlayerPkg = null;
    protected PlayerPackageCmpt playerPkg
    {
        get
        {
            if (null == mPlayerPkg)
            {
                mPlayerPkg = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.PlayerPackageCmpt>();
            }

            return mPlayerPkg;
        }
    }

    public int GetCountByProtoId(int protoId) 
    {
        int n = 0;
        foreach (var item in _itemList)
        {
            if (item.protoId == protoId)
                n += item.stackCount;
        }
        return n;
    }
	
	public void AddItem(int itemId, int count)
	{
		ItemSample itemGrid = new ItemSample(itemId, count);
		AddItem(itemGrid);
	}


	public void AddItem(ItemSample item)
	{
		_itemList.Add(item);
	}

	protected bool CanFetch(int index)
	{
		IItemDrop itemDrop = this as IItemDrop;
		if (index < 0 || index >= itemDrop.GetCount())
		{
			return false;
		}
		return playerPkg.package.CanAdd(itemDrop.Get(index));
	}

	protected bool CanFetchAll()
	{
		int n = GetCount();
		MaterialItem[] array = new MaterialItem[n];
		for(int i = 0; i < n; i++)
		{
			ItemSample item = Get(i);
			array[i] = new MaterialItem()
			{
				protoId = item.protoId,
				count = item.stackCount
			};
		}		
		return playerPkg.package.CanAdd(array);
	}
#region IItemDrop
    public virtual ItemSample Get(int index)
    {
        return _itemList[index];
    }
	public virtual int GetCount()
    {
        return _itemList.Count;
    }
	public virtual void Fetch(int index)
    {
		if (!CanFetch(index))
		{
			return;
		}

		if(_net != null)
		{
			_net.GetItem( ((ItemObject)_itemList[index]).instanceId );
		}
		else
		{
			ItemSample item = _itemList[index];
			
			playerPkg.Add(item.protoId, item.stackCount);
            _itemList.Remove(item);
            if (fetchItem != null)
                fetchItem.Invoke();
		}
    }
	public virtual void FetchAll()
    {
		if (!CanFetchAll())
		{
			return;
		}
		if(_net != null)
		{
			_net.GetAllItem();
		}
		else
		{
			foreach (ItemSample item in _itemList)
			{
				playerPkg.Add(item.protoId, item.stackCount);
			}
			_itemList.Clear();
            if (fetchItem != null)
                fetchItem.Invoke();
		}
    }	
#endregion
#region IDroppableItemList
	public int DroppableItemCount{ get{ return _itemList.Count; } }
	public ItemSample GetDroppableItemAt(int idx)
	{
		return _itemList[idx];
	}
	public void AddDroppableItem(ItemSample item)
	{
		_itemList.Add(item);
	}
	public void RemoveDroppableItem(ItemSample item)
	{
		_itemList.Remove(item);
	}
	public void RemoveDroppableItemAll()
	{
		_itemList.Clear();
	}
#endregion

    bool CheckDistance(Vector3 pos)
    {
        Pathea.PeTrans playerTrans = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.PeTrans>();
        if (playerTrans == null)
        {
            return true;
        }

        return Vector3.Distance(playerTrans.position, pos) < PickDistance;
    }
	public bool CheckDistance()
	{
		MousePickable mousePickable = GetComponent<MousePickable>();
		if (mousePickable != null)
		{
			Pathea.PeTrans trans = mousePickable.GetComponent<Pathea.PeTrans>();
			if (trans != null)
			{
				return CheckDistance(trans.position);
			}
		}
		return false;
	}

    protected void OpenGui(Vector3 pos)
    {
        if (!CheckDistance(pos))
        {
            return;
        }

        if (GameUI.Instance != null)
        {
            GameUI.Instance.mItemGet.Show();
            GameUI.Instance.mItemGet.SetItemDrop(this);
        }
    }
}