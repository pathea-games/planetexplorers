//------------------------------------------------------------------------------
// by Pugee
//------------------------------------------------------------------------------
using System;
using UnityEngine;
using System.Collections.Generic;


public class DunItemGenerator:MonoBehaviour
{
    public List<string> boxIdList = new List<string>();

	Vector3 pos{
		get{return transform.position;}
	}
	
//	void Start(){
//        if (boxIdList.Count > 0)
//		{
////			Debug.LogError(gameObject.transform.parent.parent.name);
//            List<IdWeight> idWeightList = new List<IdWeight>();
//            for (int i = 0; i < boxIdList.Count; i++)
//            {
////				Debug.LogError(boxIdList[i]);
//                idWeightList.Add(RandomDunGenUtil.GetChanceWeightFromStr(boxIdList[i]));
//            }
//			int seed = (int)(System.DateTime.UtcNow.Ticks%Int32.MaxValue+(int)(gameObject.transform.position.x+gameObject.transform.position.y+gameObject.transform.position.z));
//			//Debug.LogError("seedint:"+seed);
//            System.Random rand = new System.Random(seed);
//            List<int> pickedIdList = RandomDunGenUtil.PickIdFromWeightList(rand,idWeightList, 1);
//
//			RandomItemMgr.Instance.TryGenItem(pos, pickedIdList[0],rand);
//		}
//	}
	public void GenItem(List<IdWeight> idWeightList,System.Random rand){
		if(idWeightList==null||idWeightList.Count==0)
			return;
		List<int> pickedIdList = RandomDunGenUtil.PickIdFromWeightList(rand,idWeightList, 1);
		RandomItemMgr.Instance.TryGenItem(pos, pickedIdList[0],rand);
	}
}

