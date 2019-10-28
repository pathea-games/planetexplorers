//------------------------------------------------------------------------------
// by Pugee
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UnityEngine;
using Pathea;

public class MousePickableRandomItem:MousePickableChildCollider
{
	List<ItemIdCount> allItems= new List<ItemIdCount> ();
	List<int> allItemObjs = new List<int> ();
	List<ItemIdCount> undefineItem = new List<ItemIdCount> ();
	bool isOpen{
		get{
			return undefineItem==null||undefineItem.Count==0;
		}
	}

	public Vector3 genPos;
	bool clicked = false;
	
	public void RefreshItem(int[] items,List<ItemIdCount> items2,List<int> itemObjList){
		allItems = new List<ItemIdCount> ();
		for(int i=0;i<items.Length;i+=2){
			allItems.Add(new ItemIdCount (items[i],items[i+1]));
		}
		undefineItem = items2;
		allItemObjs= itemObjList;
	}


	protected override void OnStart ()
	{
		base.OnStart ();
		priority = MousePicker.EPriority.Level3;
		operateDistance = 7f;
	}

	protected override void CheckOperate()
	{
		//base.CheckOperate();
		if(PeInput.Get (PeInput.LogicFunction.PickBody))
		{
            if (!clicked)
            {
//				if(!isOpen){
//					//--to do: the rate of progress
//					return;
//				}
                if (PeGameMgr.IsSingle)
                    OnClickRandomItem();
                else
                    PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_RandomItemClicked,genPos,transform.position);
                clicked = true;
            }
		}
	}

	void OnClickRandomItem(){
		for (int i = 0; i < allItems.Count; i++)
			LootItemMgr.Instance.AddLootItem(transform.position, allItems[i].protoId, allItems[i].count);
		for (int i = 0; i < undefineItem.Count; i++)
			LootItemMgr.Instance.AddLootItem(transform.position, undefineItem[i].protoId, undefineItem[i].count);
		for(int i=0;i<allItemObjs.Count;i++)
			LootItemMgr.Instance.AddLootItem(transform.position,allItemObjs[i]);
		DestroySelf();
	}

	void DestroySelf(){
		if (gameObject != null)
		{
			RandomItemObj rio = RandomItemMgr.Instance.GetRandomItemObj(genPos);
			if (rio != null){
                if (rio.type == RandomObjType.MonsterFeces)
                    Pathea.Effect.EffectBuilder.Instance.Register(EffectId.PICK_FECES,null,transform.position,Quaternion.identity);
				RandomItemMgr.Instance.RemoveRandomItemObj(rio);
				SceneMan.RemoveSceneObj(rio);
			}
			GameObject.Destroy(gameObject);
		}
	}
}

