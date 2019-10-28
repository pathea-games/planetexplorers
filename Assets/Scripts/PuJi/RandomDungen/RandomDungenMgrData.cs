//------------------------------------------------------------------------------
// by Pugee
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UnityEngine;
using Pathea;

public enum DungeonWaterType{
	None,
	Shallow,
	Deep
}

public class RandomDungenMgrData
{
	public static int DungeonId=0;
	public static Vector3 entrancePos;
	public static Vector3 enterPos;
	public static Vector3 genDunPos;
	public static Vector3 revivePos;
	public static DungeonWaterType waterType;
	public static DungeonBaseData dungeonBaseData;
	public static bool IsTaskDungeon{
		get{return dungeonBaseData.IsTaskDungeon;}
	}

	static List<int> followerNpcId = new List<int> ();

	public static bool InDungeon{
		get{
			if(PeGameMgr.yirdName ==AdventureScene.Dungen.ToString())
				return true; 
			if(PeCreature.Instance.mainPlayer==null)
				return false;
			if(PeGameMgr.IsAdventure&&PeCreature.Instance.mainPlayer.position.y<-100)
				return true;
			return false;
		}
	}

	public static List<MonsterGenerator> allMonsters = new List<MonsterGenerator> ();
	public static List<MinBossGenerator> allMinBoss = new List<MinBossGenerator> ();
	public static List<BossMonsterGenerator> allBoss = new List<BossMonsterGenerator> ();
	public static List<DunItemGenerator> allItems = new List<DunItemGenerator> ();
	public static List<DunRareItemGenerator> allRareItems = new List<DunRareItemGenerator> ();

	public static List<PeEntity> monsterEntity = new List<PeEntity> ();
	public static List<int> pickedKeys = new List<int>();

	public static List<RandomItemObj> rareRandomItem = new List<RandomItemObj>();

	public static Dictionary<IntVector2,int> initTaskEntrance = new Dictionary<IntVector2, int>();

	public static void AddInitTaskEntrance(IntVector2 pos,int level){
		if(!initTaskEntrance.ContainsKey(pos))
			initTaskEntrance.Add(pos,level);
	}

	public static void AddRareItem(RandomItemObj rio){
		rareRandomItem.Add(rio);
	}

	public static void RareItemReady(int instanceId,int dungeonId){
		if(DungeonId!=dungeonId)
			return;
		if(rareRandomItem.Count==0)
			return;
		rareRandomItem[0].AddRareInstance(instanceId);
		rareRandomItem.RemoveAt(0);
		Debug.LogError("Rare Item Ready!rareRandomItem.Count:"+rareRandomItem.Count);
	}

	public static void SetPosByEnterPos(Vector3 entrancsP){
		entrancePos = entrancsP;
		enterPos = PeCreature.Instance.mainPlayer.position;
		revivePos = new Vector3 (enterPos.x,-512,enterPos.z);
		genDunPos = revivePos;
	}
	public static void SetPosByGenPlayerPos(Vector3 genPlayerPos){
		revivePos = genPlayerPos;
		genDunPos = revivePos;
	}

    public static void Clear() {
		entrancePos = Vector3.zero;
		enterPos = Vector3.zero;
		revivePos = Vector3.zero;
		genDunPos = Vector3.zero;
        followerNpcId = new List<int>();
		allMonsters = new List<MonsterGenerator> ();
		allMinBoss = new List<MinBossGenerator> ();
		allBoss = new List<BossMonsterGenerator> ();
		allItems = new List<DunItemGenerator> ();
		allRareItems= new List<DunRareItemGenerator> ();

		monsterEntity = new List<PeEntity> ();
		pickedKeys = new List<int> ();
		rareRandomItem = new List<RandomItemObj> ();
    }


    public static void AddFollower(int id) {
        if (!followerNpcId.Contains(id))
            followerNpcId.Add(id);
    }

    public static void AddServants() {
        NpcCmpt[] servants = PeCreature.Instance.mainPlayer.GetComponent<ServantLeaderCmpt>().GetServants();
        foreach (NpcCmpt nc in servants)
        {
            if (nc != null)
                AddFollower(nc.Entity.Id);
        }
		foreach (NpcCmpt follower in MissionManager.Instance.m_PlayerMission.followers)
		{
			if(follower!=null)
				AddFollower(follower.Entity.Id);
		}
    }

    public static List<int> GetAllFollowers() {
        return followerNpcId;
    }

	public static void AddMonsterMgr(MonsterGenerator mgm){
		if(!allMonsters.Contains(mgm))
			allMonsters.Add(mgm);
	}

	public static void AddDunItemMgr(DunItemGenerator digm){
		if(!allItems.Contains(digm))
			allItems.Add(digm);
	}

	public static void AddDunRareItem(DunRareItemGenerator drig){
		if(!allRareItems.Contains(drig))
			allRareItems.Add(drig);
	}

	public static void AddMonster(PeEntity monster){
		monsterEntity.Add(monster);
	}
}
