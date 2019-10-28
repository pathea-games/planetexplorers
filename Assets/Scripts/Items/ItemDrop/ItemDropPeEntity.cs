using UnityEngine;
using ItemAsset;
using ItemAsset.PackageHelper;
using System.Collections;
using System.Collections.Generic;

public class ItemDropPeEntity : ItemDrop
{
	bool _itemListsUpdated = false;
	Pathea.SkAliveEntity _skAlive = null;
	List<IDroppableItemList> _itemLists = new List<IDroppableItemList>();

	void Awake()
	{
		IniDropCmpts();
	}

    void Start()
    {
        MousePickablePeEntity mousePickable = GetComponent<MousePickablePeEntity>();
		if (mousePickable == null) {
			mousePickable = gameObject.AddComponent<MousePickablePeEntity>();
			mousePickable.CollectColliders();
		}

		CreateDroppableItemList();
        mousePickable.eventor.Subscribe((object sender, MousePickable.RMouseClickEvent e) =>
        {
            Pathea.PeTrans trans = e.mousePickable.GetComponent<Pathea.PeTrans>();
			OpenGui(trans.position);
        });
    }

	void CreateDroppableItemList()
    {
		if(_itemListsUpdated)	return;

		_itemListsUpdated = true;
		
		//_itemLists.Add(this); // this will be added at the end of this method(foreach)

		Pathea.PeEntity entity = GetComponent<Pathea.PeEntity>();
        if (null == entity)							return;
        _skAlive = entity.GetCmpt<Pathea.SkAliveEntity>();
        if (_skAlive == null || !_skAlive.isDead)	return;

		if(Pathea.PeGameMgr.IsMulti)
			return;
		Pathea.CommonCmpt common = entity.GetCmpt<Pathea.CommonCmpt>();
		if (common != null)
		{
			List<ItemSample> items = ItemDropData.GetDropItems(common.ItemDropId);
            if (common.entityProto.proto == Pathea.EEntityProto.Monster) 
            {
                if (items == null)
                    items = GetSpecialItem.MonsterItemAdd(common.entityProto.protoId);
                else
                    items.AddRange(GetSpecialItem.MonsterItemAdd(common.entityProto.protoId));   //特殊道具添加
            }
			if(items != null)
			{
				foreach(ItemSample item in items)
				{
					AddDroppableItem(item);
				}
			}
		}
		
		return;
    }

	void IniDropCmpts()
	{
		_itemLists.Clear();

		Component[] itemDropComps = GetComponents(typeof(IDroppableItemList));
		foreach (Component comp in itemDropComps)
		{
			_itemLists.Add(comp as IDroppableItemList);
		}
	}

	new bool CanFetch(int index)
	{
		if (index < 0 || index >= _itemLists.Count)
		{
			return false;
		}
		return playerPkg.package.CanAdd(_itemList[index]);
	}
	new bool CanFetchAll()
	{
		MaterialItem[] array = new MaterialItem[_itemList.Count];
		for(int i = 0; i < _itemList.Count; i++)
		{
			array[i] = new MaterialItem()
			{
				protoId = _itemList[i].protoId,
				count = _itemList[i].stackCount
			};
		}		
		return playerPkg.package.CanAdd(array);
	}
	bool ConvertIndex(int gIdx, out int n, out int lIdx)
	{
		n = 0;
		lIdx = gIdx;
		foreach(IDroppableItemList itemDrop in _itemLists)
		{
			if(lIdx < itemDrop.DroppableItemCount)
			{
				return true;
			}
			n++;
			lIdx -= itemDrop.DroppableItemCount;
		}
		return false;
	}

	#region ItemDrop override
	public override ItemSample Get(int index)
	{
		int n, lIdx;
		if(!ConvertIndex(index, out n, out lIdx))
		{
			return null;
		}
		return _itemLists[n].GetDroppableItemAt(lIdx);
	}
	public override int GetCount()
	{
		int cnt = 0;
		foreach(IDroppableItemList itemDrop in _itemLists)
		{
			cnt += itemDrop.DroppableItemCount;
		}
		return cnt;
	}
	public override void Fetch(int index)
	{
		int n, lIdx;
		if(!ConvertIndex(index, out n, out lIdx))
		{
			return;
		}

		if (GameConfig.IsMultiClient)
		{
			ItemSample dropItem = _itemLists[n].GetDroppableItemAt(lIdx);

			if (null != PlayerNetwork.mainPlayer && null != _skAlive && null != _skAlive.Entity)
				PlayerNetwork.mainPlayer.RequestDeadObjItem(_skAlive.Entity.Id, lIdx, dropItem.protoId);

			return;
		}

        Pathea.PeEntity npc = gameObject.GetComponent<Pathea.PeEntity>();
        if (npc != null)
        {
            if (npc.entityProto.proto == Pathea.EEntityProto.RandomNpc || npc.entityProto.proto == Pathea.EEntityProto.Npc)
            {
                OwnerData.deadNPC = new OwnerData();
                OwnerData.deadNPC.npcID = gameObject.GetComponent<Pathea.PeEntity>().Id;
                OwnerData.deadNPC.npcName = gameObject.GetComponent<Pathea.PeEntity>().name;
            }
        }

		ItemSample item = _itemLists[n].GetDroppableItemAt(lIdx);
		if(playerPkg.package.CanAdd(item))
		{
			playerPkg.Add(item.protoId, item.stackCount);
			_itemLists[n].RemoveDroppableItem(item); 
		}

		if(_skAlive != null && GetCount() == 0)
		{
			PeEventGlobal.Instance.PickupEvent.Invoke(_skAlive);
		}
	}
	public override void FetchAll()
	{
		if (GameConfig.IsMultiClient)
		{
			if (null != PlayerNetwork.mainPlayer && null != _skAlive && null != _skAlive.Entity)
				PlayerNetwork.mainPlayer.RequestDeadObjAllItems(_skAlive.Entity.Id);

			return;
		}

		List<MaterialItem> items = new List<MaterialItem>();
		foreach(IDroppableItemList dropItems in _itemLists)
		{
			int n = dropItems.DroppableItemCount;
			for(int i = 0; i < n; i++)
			{
				ItemSample item = dropItems.GetDroppableItemAt(i);
				items.Add(new MaterialItem(){
					protoId = item.protoId,
					count = item.stackCount
				});
			}
		}
		if (!playerPkg.package.CanAdd(items.ToArray()))
		{
			return;
		}

        Pathea.PeEntity npc = gameObject.GetComponent<Pathea.PeEntity>();
        if (npc != null)
        {
            if (npc.entityProto.proto == Pathea.EEntityProto.RandomNpc || npc.entityProto.proto == Pathea.EEntityProto.Npc)
            {
                OwnerData.deadNPC = new OwnerData();
                OwnerData.deadNPC.npcID = gameObject.GetComponent<Pathea.PeEntity>().Id;
                OwnerData.deadNPC.npcName = gameObject.GetComponent<Pathea.PeEntity>().name;
            }
        }

        foreach (MaterialItem item in items)
        {
            playerPkg.Add(item.protoId, item.count);
        }

		foreach(IDroppableItemList itemDrop in _itemLists)
		{
			itemDrop.RemoveDroppableItemAll();
		}

		if(_skAlive != null && GetCount() == 0)
		{
			PeEventGlobal.Instance.PickupEvent.Invoke(_skAlive);
		}
	}


	public bool NpcCanFetchAll(Pathea.NpcPackageCmpt npcPackage)
	{
		List<MaterialItem> items = new List<MaterialItem>();
		foreach(IDroppableItemList dropItems in _itemLists)
		{
			int n = dropItems.DroppableItemCount;
			for(int i = 0; i < n; i++)
			{
				ItemSample item = dropItems.GetDroppableItemAt(i);
				items.Add(new MaterialItem(){
					protoId = item.protoId,
					count = item.stackCount
				});
			}
		}
		
		return npcPackage.CanAddItemList(items);
	}

	public void NpcFetchAll(Pathea.NpcPackageCmpt npcPackage)
	{
		List<MaterialItem> items = new List<MaterialItem>();
		foreach(IDroppableItemList dropItems in _itemLists)
		{
			int n = dropItems.DroppableItemCount;
			for(int i = 0; i < n; i++)
			{
				ItemSample item = dropItems.GetDroppableItemAt(i);
				items.Add(new MaterialItem(){
					protoId = item.protoId,
					count = item.stackCount
				});
			}
		}
	
		if (!npcPackage.CanAddItemList(items))
		{
			return;
		}

		foreach (MaterialItem item in items)
		{
			npcPackage.Add(item.protoId, item.count);
		}
		
		foreach(IDroppableItemList itemDrop in _itemLists)
		{
			itemDrop.RemoveDroppableItemAll();
		}
		
		if(_skAlive != null && GetCount() == 0)
		{
			PeEventGlobal.Instance.PickupEvent.Invoke(_skAlive);
		}
	}

	#endregion
}
