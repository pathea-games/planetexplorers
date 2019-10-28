//------------------------------------------------------------------------------
// by Pugee
//------------------------------------------------------------------------------
using System;
using UnityEngine;
using System.Collections.Generic;
using Pathea;

public class MonsterGenerator:MonoBehaviour
{
    public List<string> RandomMonsterStrList = new List<string>();

	Vector3 pos{
		get{return transform.position;}
	}

	void Start(){
	}

	public void GenerateMonster(List<IdWeight> landMonsterId,List<IdWeight> waterMonsterId,int buff,System.Random rand,bool isTaskDungeon){
//		if (RandomMonsterStrList.Count > 0)
//		{
//			List<IdWeight> idWeightList = new List<IdWeight>();
//			for (int i = 0; i < RandomMonsterStrList.Count; i++) {
//				idWeightList.Add(RandomDunGenUtil.GetChanceWeightFromStr(RandomMonsterStrList[i]));
//			}
//			int seed = (int)(System.DateTime.UtcNow.Ticks%Int32.MaxValue);
//			System.Random rand = new System.Random(seed);
//			List<int> pickedIdList = RandomDunGenUtil.PickIdFromWeightList(rand,idWeightList, 1);
//			
//			PeEntity monster = MonsterEntityCreator.CreateMonster(pickedIdList[0], pos);
//			PESkEntity mp = monster.GetComponent<PESkEntity>();
//			mp.deathEvent+=RandomDungenMgr.Instance.OpenLockedDoor;
//			RandomDungenMgrData.AddMonster(monster);
//		}

		//--to do:check water
		List<IdWeight> monsterList = new List<IdWeight> ();
		if (VFVoxelWater.self != null)
		{
			if(VFVoxelWater.self.IsInWater(pos)){
				if(VFVoxelWater.self.IsInWater(pos+new Vector3(0,1,0))){
					if(VFVoxelWater.self.IsInWater(pos+new Vector3(0,4,0)))
						monsterList = waterMonsterId;
					else
						return;
				}
				else
					monsterList = landMonsterId;
			}else{
				monsterList = landMonsterId;
			}
		}
		if(monsterList==null||monsterList.Count==0)
			return;
		List<int> pickedIdList = RandomDunGenUtil.PickIdFromWeightList(rand,monsterList, 1);

		MonsterEntityCreator.CreateDungeonMonster(pickedIdList[0], pos,RandomDungenMgrData.DungeonId,buff);

	}

}
