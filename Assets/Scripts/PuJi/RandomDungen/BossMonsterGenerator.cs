//------------------------------------------------------------------------------
// 2016-6-14 20:43:50
// by pugee
//------------------------------------------------------------------------------
using System;
using UnityEngine;
using System.Collections.Generic;
using Pathea;


public class BossMonsterGenerator:MonoBehaviour
{
	Vector3 pos{
		get{return transform.position;}
	}

	public void GenBoss(List<IdWeight> landMonsterId,List<IdWeight> waterMonsterId,int buff, System.Random rand,bool isTaskDungeon){
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
		
//		if(PeGameMgr.IsSingle&&monster!=null){
//			PESkEntity mp = monster.GetComponent<PESkEntity>();
//			mp.deathEvent+=RandomDungenMgr.Instance.OpenLockedDoor;
//			RandomDungenMgrData.AddMonster(monster);
//			//--to do add monsterBuff
//			if(buff!=0)
//				SkillSystem.SkEntity.MountBuff(monster.skEntity, buff, new List<int>(), new List<float>());
//		}

	}
	public static void GenAllBoss(List<BossMonsterGenerator> allBoss,List<IdWeight> landList,List<IdWeight> waterList,int buff, System.Random rand,bool isTaskDungeon){
		foreach(BossMonsterGenerator bmg in allBoss){
			bmg.GenBoss(landList,waterList,buff,rand,isTaskDungeon);
		}
	}
}

