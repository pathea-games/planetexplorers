//------------------------------------------------------------------------------
// by Pugee
//------------------------------------------------------------------------------

using System;
using UnityEngine;
using System.Collections.Generic;

public class DunRareItemGenerator:MonoBehaviour
{
	public List<string> boxIdList = new List<string>();
	
	Vector3 pos{
		get{return transform.position;}
	}

	void Awake(){
	}

	void Start(){
	}

	public void GenItem(List<IdWeight> idWeightList,System.Random rand,List<ItemIdCount> specifiedItems=null){
		List<int> pickedIdList = RandomDunGenUtil.PickIdFromWeightList(rand,idWeightList, 1);
		RandomItemMgr.Instance.TryGenRareItem(pos, pickedIdList[0],rand,specifiedItems);
	}

	public void GenItem(int id,System.Random rand,List<ItemIdCount> specifiedItem){
		RandomItemMgr.Instance.TryGenRareItem(pos, id,rand,specifiedItem);
	}

	public void RandomId(out int id,List<IdWeight> idWeightList,System.Random rand){
		id=-1;
		List<int> pickedIdList = RandomDunGenUtil.PickIdFromWeightList(rand,idWeightList, 1);
		id = pickedIdList[0];
	}

	public static void GenAllItem(List<DunRareItemGenerator> drigList,List<IdWeight> idWeightList,float chance,List<IdWeight> rareItemTags,System.Random rand,List<ItemIdCount> specifiedItems=null){
		if(idWeightList==null||idWeightList.Count==0)
			return;
		if(Pathea.PeGameMgr.IsSingle)
		{
			//--to do foreach gen
			int isoCount = 0;
			foreach(DunRareItemGenerator drig in drigList){
					drig.GenItem(idWeightList,rand,specifiedItems);
					specifiedItems= null;
					if(rand.NextDouble()<chance)
						isoCount++;
			}
			string isoTag = RandomDungeonDataBase.GetRandomIsoTag(rand,rareItemTags);
			if(SteamWorkShop.Instance!=null)
				SteamWorkShop.Instance.GetRandIsos(RandomDungenMgrData.DungeonId, isoCount,isoTag);
		}else{
			List<Vector3> posList = new List<Vector3> ();
			List<int> idList = new List<int> ();
			int isoCount = 0;
			foreach(DunRareItemGenerator drig in drigList)
			{
				int id;
				Vector3 pos = drig.pos;
				drig.RandomId(out id,idWeightList,rand);
				posList.Add(pos);
				idList.Add(id);
				if(rand.NextDouble()<chance)
					isoCount ++;
			}
			List<ItemIdCount> sItems = new List<ItemIdCount> ();
			if(specifiedItems!=null&&specifiedItems.Count>0)
				sItems = specifiedItems;
			else
				sItems.Add(new ItemIdCount (-1,0));
			//send to server
			string isoTag = RandomDungeonDataBase.GetRandomIsoTag(rand,rareItemTags);
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_RandomItemRareAry, RandomDungenMgrData.entrancePos, isoCount,isoTag,posList.ToArray(),idList.ToArray(),sItems.ToArray());
		}
	}

}
