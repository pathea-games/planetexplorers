//------------------------------------------------------------------------------
// by Pugee
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

public enum DungeonType{
	Iron=0,
	Cave=1
}

public class DungeonBaseData{
	public int id;
	public int level;
	public bool IsTaskDungeon{
		get{return level>=DungeonConstants.TASK_LEVEL_START;}
	}
	public List<IdWeight> landMonsterId;
	public List<IdWeight> waterMonsterId;
	public float monsterAmount;
	public int monsterBuff;
	public List<IdWeight> bossId;
	public List<IdWeight> bossWaterId;
	public int bossMonsterBuff;
	public List<IdWeight> minBossId;
	public List<IdWeight> minBossWaterId;
	public int minBossMonsterBuff;
	public List<IdWeight> itemId;
	public float itemAmount;
	public List<IdWeight> rareItemId;
	public float rareItemChance;
	public List<IdWeight> rareItemTags;
	public List<ItemIdCount> specifiedItems;
	public string dungeonFlowPath ="Prefab/Item/Rdungeon/DungeonFlow_Main";
	public int type;
	public bool IsIron{
		get{return type==0;}
	}
	public DungeonType Type{
		get{return (DungeonType)type;}
	}

	public static List<string> isoTagList= new List<string> ();
	public static void InitIsoTagList(){
		isoTagList= new List<string> ();
		isoTagList.Add(IsoTags.Creation);
		isoTagList.Add(IsoTags.Equipment);
		isoTagList.Add(IsoTags.Sword);
		isoTagList.Add(IsoTags.Axe);
		isoTagList.Add(IsoTags.Bow);
		isoTagList.Add(IsoTags.Shield);
		isoTagList.Add(IsoTags.Gun);
		isoTagList.Add(IsoTags.Carrier);
		isoTagList.Add(IsoTags.Vehicle);
		isoTagList.Add(IsoTags.Ship);
		isoTagList.Add(IsoTags.Aircraft);
		isoTagList.Add(IsoTags.Armor);
		isoTagList.Add(IsoTags.Head);
		isoTagList.Add(IsoTags.Body);
		isoTagList.Add(IsoTags.ArmAndLeg);
		isoTagList.Add(IsoTags.HeadAndFoot);
		isoTagList.Add(IsoTags.Decoration);
		isoTagList.Add(IsoTags.Robot);
		isoTagList.Add(IsoTags.AITurret);
		isoTagList.Add(IsoTags.ObjectItem);
	}
}

public class RandomDungeonDataBase
{
	public const int LEVEL_MAX = 10;
	static Dictionary<int,DungeonBaseData> dungeonData = new Dictionary<int, DungeonBaseData>();
	static Dictionary<int,List<int>> levelId = new Dictionary<int,List<int> >();

	public static void LoadData(){
//		int id=0;
//		for(int level=0;level<LEVEL_MAX;level++){
//			DungeonBaseData dbd = new DungeonBaseData ();
//			dbd.id=id++;
//			dbd.level=level;
//			dbd.landMonsterId = new List<IdWeight> ();
//			dbd.landMonsterId.Add(new IdWeight (104,50));
//			dbd.landMonsterId.Add(new IdWeight (106,50));
//			dbd.waterMonsterId = new List<IdWeight> ();
////			dbd.waterMonsterId.Add(new IdWeight (21,50));
//			dbd.waterMonsterId.Add(new IdWeight (16,100));
//			dbd.monsterAmount =0.1f+level*0.05f;
//			dbd.monsterBuff =1;
//			dbd.itemId = new List<IdWeight> ();
//			dbd.itemId.Add(new IdWeight (76,50));
//			dbd.itemId.Add(new IdWeight (80,50));
//			dbd.itemAmount = 0.1f+level*0.05f;
//			dbd.rareItemId=new List<IdWeight> ();
//			dbd.rareItemId.Add(new IdWeight (71,50));
//			dbd.rareItemId.Add(new IdWeight (74,50));
//			dungeonData.Add(dbd.id,dbd);
//			if(!levelId.ContainsKey(level))
//				levelId[level]= new List<int> ();
//			levelId[level].Add(dbd.id);
//		}

		SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("Rdungeon_monster_item");
		while (reader.Read())
		{
			DungeonBaseData dbd = new DungeonBaseData ();
			dbd.id = Convert.ToInt32(reader.GetString(reader.GetOrdinal("id")));
			dbd.level = Convert.ToInt32(reader.GetString(reader.GetOrdinal("level")));
			dbd.landMonsterId = RandomDunGenUtil.GetIdWeightList(reader.GetString(reader.GetOrdinal("landmonsterid")));
			dbd.waterMonsterId = RandomDunGenUtil.GetIdWeightList(reader.GetString(reader.GetOrdinal("watermonsterid")));
			dbd.monsterAmount = Convert.ToSingle(reader.GetString(reader.GetOrdinal("monsteramount")));
			dbd.monsterBuff = Convert.ToInt32(reader.GetString(reader.GetOrdinal("monsterbuff")));
			dbd.bossId = RandomDunGenUtil.GetIdWeightList(reader.GetString(reader.GetOrdinal("bossid")));
			dbd.bossWaterId = RandomDunGenUtil.GetIdWeightList(reader.GetString(reader.GetOrdinal("bosswaterid")));
			dbd.bossMonsterBuff = Convert.ToInt32(reader.GetString(reader.GetOrdinal("bossbuff")));
			dbd.minBossId = RandomDunGenUtil.GetIdWeightList(reader.GetString(reader.GetOrdinal("minbossid")));
			dbd.minBossWaterId = RandomDunGenUtil.GetIdWeightList(reader.GetString(reader.GetOrdinal("minbosswaterid")));
			dbd.minBossMonsterBuff = Convert.ToInt32(reader.GetString(reader.GetOrdinal("minbossbuff")));
			dbd.itemId = RandomDunGenUtil.GetIdWeightList(reader.GetString(reader.GetOrdinal("itemid")));
			dbd.itemAmount = Convert.ToSingle(reader.GetString(reader.GetOrdinal("itemamount")));
			dbd.rareItemId = RandomDunGenUtil.GetIdWeightList(reader.GetString(reader.GetOrdinal("rareitemid")));
			dbd.rareItemChance =  Convert.ToSingle(reader.GetString(reader.GetOrdinal("rareitemchance")));
			dbd.specifiedItems =  ItemIdCount.ParseStringToList(reader.GetString(reader.GetOrdinal("specifieditems")));
			dbd.dungeonFlowPath = reader.GetString(reader.GetOrdinal("dungeonflowpath"));
			dbd.type = Convert.ToInt32(reader.GetString(reader.GetOrdinal("type")));
			dbd.rareItemTags= RandomDunGenUtil.GetIdWeightList(reader.GetString(reader.GetOrdinal("rareItemtags")));



			dungeonData.Add(dbd.id,dbd);
			if(!levelId.ContainsKey(dbd.level))
				levelId[dbd.level]= new List<int> ();
			levelId[dbd.level].Add(dbd.id);
		}
		DungeonBaseData.InitIsoTagList();
	}

	public static DungeonBaseData GetDataFromLevel(int level){
		if(!levelId.ContainsKey(level))
			return null;
		List<int> idList = levelId[level];
		int pickid =idList[ new System.Random().Next(idList.Count)];
		if(!dungeonData.ContainsKey(pickid))
			return null;
		return dungeonData[pickid];
	}
	public static DungeonBaseData GetDataFromId(int id){
		if(!dungeonData.ContainsKey(id))
			return null;
		return dungeonData[id];
	}

	public static string GetRandomIsoTag(System.Random rand,List<IdWeight> weightPool){
		if(weightPool==null||weightPool.Count==0){
			Debug.LogError("RandomDungeonDataBase.GetRandomIsoTag:no weight pool");
			return DungeonBaseData.isoTagList[DungeonBaseData.isoTagList.Count-1];
		}
		int pickedId = RandomDunGenUtil.PickIdFromWeightList(rand,weightPool,1)[0];
		if(pickedId<DungeonBaseData.isoTagList.Count)
			return DungeonBaseData.isoTagList[pickedId];
		return DungeonBaseData.isoTagList[DungeonBaseData.isoTagList.Count-1];
	}
}

