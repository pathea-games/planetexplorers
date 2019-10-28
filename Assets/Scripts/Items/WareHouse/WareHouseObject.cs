using UnityEngine;
using Mono.Data.SqliteClient;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using ItemAsset.PackageHelper;
using Pathea;
using ItemAsset;

public class WareHouseObject : MousePickableChildCollider, ISaveDataInScene
{
    const int PakCapacity = 30;

	public const float MaxOperateDistance = 10f;
    PeEntity _entity;

    public int _id;
    public ItemAsset.ItemPackage ItemPak
    {
        get
        {
            return _itemPak;
        }
    }
    ItemAsset.ItemPackage _itemPak;
    public MapObjNetwork _objNet = null;

    protected override void OnStart()
    {
        base.OnStart();
        WareHouseManager.AddWareHouseObjectList(this);
        if (_itemPak == null && PeGameMgr.IsSingleStory)
        {
            _itemPak = DescToItemPack(WareHouseManager.GetWareHouseData(_id).m_itemsDesc);
        }
        else if (_itemPak == null && PeGameMgr.playerType == PeGameMgr.EPlayerType.Tutorial)//这个位置在教程做完之后可能需要改
        {
            _itemPak = DescToItemPack(WareHouseManager.GetWareHouseData(_id).m_itemsDesc);
        }
        else if (/*_itemPak == null && */PeGameMgr.IsMulti)
        {
            GlobalBehaviour.RegisterEvent(RequestCreate);
        }

        if (null == _itemPak && PeGameMgr.IsSingle)
			_itemPak = new ItemPackage(PakCapacity);
		operateDistance = MaxOperateDistance;
        _entity = gameObject.GetComponentInParent<PeEntity>();
        if(_entity != null)
        {
            MapObjNetwork net = MapObjNetwork.GetNet(_entity.Id);
            if(net != null)
            {
                net.wareHouseObj = this;
                _id = net.AssetId;
                InitForNet(net);
            }
        }
    }
    bool RequestCreate()
    {
        while (PlayerNetwork.mainPlayer == null)
        {
            return false;
        }
        //if (PeGameMgr.IsStory)
        {
            
            if (MapObjNetwork.HadCreate(_id, (int)DoodadType.DoodadType_SceneBox))
            {
				MapObjNetwork obj = MapObjNetwork.GetNet(_id, (int)DoodadType.DoodadType_SceneBox);
				if (obj != null)
                    obj.RequestItemList();
            }
            else
                PlayerNetwork.mainPlayer.CreateSceneBox(_id);
        }
        return true;
    }
    public static bool MatchID(WareHouseObject iter, int id)
    {
        if (iter == null)
            return false;
        return (iter._id == id);
    }

    protected override void CheckOperate()
    {
		if (PeInput.Get(PeInput.LogicFunction.OpenItemMenu)
		    || PeInput.Get(PeInput.LogicFunction.InteractWithItem))
        {
			if (MissionManager.Instance != null && MissionManager.Instance.HasMission(MissionManager.m_SpecialMissionID45) && _id == 1)
            {
                if (PeGameMgr.IsMulti)
                    MissionManager.Instance.RequestCompleteMission(MissionManager.m_SpecialMissionID45);
                else
                {
                   MissionManager.Instance.CompleteMission(MissionManager.m_SpecialMissionID45);
                }
            }


            GameUI.Instance.mWarehouse.ResetItemPacket(_itemPak, transform, this);
            GameUI.Instance.mWarehouse.Show();
        }
    }

    public void ImportData(byte[] data)
    {
        if (PeGameMgr.IsMulti)
            return;
        _itemPak = new ItemAsset.ItemPackage(PakCapacity);
        if (data != null)
        {
            _itemPak.Import(data);
        }
    }
    public byte[] ExportData()
    {
        if (PeGameMgr.IsMulti)
            return null;
        if (_itemPak != null)
        {
			using (MemoryStream ms = new MemoryStream(1000))
			{
				using (BinaryWriter w = new BinaryWriter(ms))
				{
			        _itemPak.Export(w);
				}
				return ms.ToArray();
			}
        }
        return null;
    }

    public static ItemAsset.ItemPackage DescToItemPack(string desc)
    {
        ItemAsset.ItemPackage pak = new ItemAsset.ItemPackage(PakCapacity);
        if (desc != "0")
        {
            string[] items = desc.Split(';');
            for (int i = 0; i < items.Length; i++)
            {
                string[] itemlist = items[i].Split(',');
                if (itemlist.Length == 2)
                {
                    int id = Convert.ToInt32(itemlist[0]);
                    int cnt = Convert.ToInt32(itemlist[1]);
                    pak.Add(id, cnt);
                }
            }
        }
        return pak;
    }
    /*
    public static string ItemPackToByteArray(ItemAsset.ItemPackage pak)
    {
        for (int i = 0; i < pak.Length; i++)
        {
            string[] itemlist = items[i].Split(',');
            if (itemlist.Length == 2)
            {
                int id = Convert.ToInt32(itemlist[0]);
                int cnt = Convert.ToInt32(itemlist[1]);
                pak.Add(id, cnt);
            }
        }
        return;
    }
    */


    public void InitForNet(MapObjNetwork net)
    {
        if (_itemPak == null)
        {
            _itemPak = new ItemAsset.ItemPackage(PakCapacity);
        }
        _objNet = net;
    }

    public void ResetItemByIdList(List<int> itemIdList)
    {
        if (itemIdList.Count != PakCapacity * (int)ItemPackage.ESlotType.Max)
        {
            Debug.LogErrorFormat("WareHouseObject.ResetItemByIdList() Error: itemIdList.Count !={0},itemIdList.Count:{1}", PakCapacity * (int)ItemPackage.ESlotType.Max, itemIdList.Count);
        }
        ItemPak.Clear();
        if (itemIdList != null)
        {
            for(int i = 0; i < itemIdList.Count; i++)
            {
                ItemObject itemObj = itemIdList[i]==-1 ? null : ItemMgr.Instance.Get(itemIdList[i]);
                if (itemObj != null)
                {
                    //lz-2016.11.16 数据是整个类型的list，这里需要处理分类型
                    ItemPak.PutItem(itemObj, i% PakCapacity, (ItemPackage.ESlotType)(i/ PakCapacity));
                }
            }
        }		
		GameUI.Instance.mWarehouse.ResetItem();
    }

    public void RemoveItemById(int itemId)
    {
        ItemPak.RemoveItemById(itemId);
        GameUI.Instance.mWarehouse.ResetItem();
    }
}
