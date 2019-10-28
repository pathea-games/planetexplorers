using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ItemAsset;
using Pathea;
using UnityEngine;

public class ItemDropMousePickRandomItem:ItemDropMousePick
{
    public Vector3 genPos;
    public int[] GetAllItem()
    {
        int[] items =new int[_itemList.Count*2];
        for (int i = 0; i < _itemList.Count; i ++)
        {
            items[i*2] = _itemList[i].protoId;
            items[i*2+1] = _itemList[i].GetCount();
        }
        return items;
    }

    public override void Fetch(int index)
    {
		if (!CanFetch(index))
		{
			return;
		}
        ItemSample item = _itemList[index];

        if (!PeGameMgr.IsMulti)
        {
            playerPkg.Add(item.protoId, item.stackCount);
            _itemList.Remove(item);
            CheckDestroyObj();
        }
        else
        {
            int protoid = item.protoId;
            int count = item.stackCount;
            PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_RandomItemFetch, genPos, index, protoid, count);
        }
    }
    public override void FetchAll()
    {
		if (!CanFetchAll())
		{
			return;
		}
        if (!PeGameMgr.IsMulti)
        {
            foreach (ItemSample item in _itemList)
            {
                playerPkg.Add(item.protoId, item.stackCount);
            }
            _itemList.Clear();

            CheckDestroyObj();
        }
        else
        {
            PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_RandomItemFetchAll, genPos);
        }
    }

    #region multimode
    public void Remove(int index,int protoId,int count)
    {
        if(_itemList.Count>index&&_itemList[index].protoId==protoId&&_itemList[index].stackCount==count)
        {
            _itemList.RemoveAt(index);
            //_itemList[index].protoId = -1;
            //_itemList[index].stackCount = 0;
        }
        else
        {
            for (int i = 0; i < _itemList.Count(); i ++)
            {
                if (_itemList[i].protoId == protoId && _itemList[i].stackCount == count)
                {
                    _itemList.RemoveAt(i);
                    _itemList[i].protoId = -1;
                    _itemList[i].stackCount = 0;
                }
            }
        }

    }
    public void RemoveAll()
    {
        _itemList.Clear();
    }

    public void CheckDestroyObj()
    {
        if (_itemList.Count <= 0)
        {
            if (gameObject != null)
            {
                RandomItemObj rio = RandomItemMgr.Instance.GetRandomItemObj(genPos);
                if (rio != null){
					RandomItemMgr.Instance.RemoveRandomItemObj(rio);
					SceneMan.RemoveSceneObj(rio);
				}
                GameObject.Destroy(gameObject);
            }
        }
    }
    #endregion
}
