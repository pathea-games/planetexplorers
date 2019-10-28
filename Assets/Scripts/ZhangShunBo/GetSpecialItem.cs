using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using ItemAsset;

public class GetSpecialItem {

    static Dictionary<int, SpecialItemData> targetPlant_ItemIDMaxNumChance = new Dictionary<int, SpecialItemData>();
    static Dictionary<int, SpecialItemData> targetMonster_ItemIDMaxNumChance = new Dictionary<int, SpecialItemData>();

    struct SpecialItemData 
    {
        public int itemID;
        public int targetItemID;
        public int maxNum;
        public int chance;
        public Vector3 pos;
        public int radius;
    }

	public static void ClearLootSpecialItemRecord()
	{
		targetPlant_ItemIDMaxNumChance.Clear();
		targetMonster_ItemIDMaxNumChance.Clear();
	}

    public static void AddLootSpecialItem(int targetID) 
    {
		TypeCollectData col = MissionManager.GetTypeCollectData(targetID);
        if (col == null)
            return;
        SpecialItemData data = new SpecialItemData();
        data.itemID = col.ItemID;
        data.targetItemID = col.m_TargetItemID;
        data.maxNum = col.m_MaxNum;
        data.chance = col.m_Chance;
        data.pos = col.m_TargetPos;
        data.radius = col.m_TargetRadius;
        if (col.m_Type == 2 || col.m_Type == 3)
        {
            if (!targetPlant_ItemIDMaxNumChance.ContainsKey(targetID))
                targetPlant_ItemIDMaxNumChance.Add(targetID, data);
        }
        if (col.m_Type == 1 || col.m_Type == 3)
        {
            if (!targetMonster_ItemIDMaxNumChance.ContainsKey(targetID))
                targetMonster_ItemIDMaxNumChance.Add(targetID, data);
        }
    }

    public static void RemoveLootSpecialItem(int targetID) 
    {
        if (targetPlant_ItemIDMaxNumChance.ContainsKey(targetID))
            targetPlant_ItemIDMaxNumChance.Remove(targetID);
        if (targetMonster_ItemIDMaxNumChance.ContainsKey(targetID))
            targetMonster_ItemIDMaxNumChance.Remove(targetID);
    }

	public static bool ExistSpecialItem(Pathea.PeEntity entity)
	{
		if (entity.proto == Pathea.EEntityProto.Monster) {
			foreach (KeyValuePair<int, SpecialItemData> item in targetMonster_ItemIDMaxNumChance)
			{
				if (item.Value.targetItemID == entity.ProtoID || item.Value.targetItemID == -999)
				{
					return true;
				}
			}
		}
		return false;
	}

    public static List<ItemSample> MonsterItemAdd(int monsterProtoID) 
    {
        List<ItemSample> tmp = new List<ItemSample>();
        System.Random r = new System.Random();
        foreach (KeyValuePair<int, SpecialItemData> item in targetMonster_ItemIDMaxNumChance)
        {
            if (item.Value.targetItemID == monsterProtoID || item.Value.targetItemID == -999)
            {
                if (item.Value.pos != Vector3.zero)
                {
                    if (Vector3.Distance(Pathea.PeCreature.Instance.mainPlayer.GetComponent<Pathea.PeTrans>().position, item.Value.pos) > item.Value.radius)
                        continue;
                }
                if (r.Next(100) < item.Value.chance)
                {
                    tmp.Add(new ItemSample(item.Value.itemID, r.Next(1, item.Value.maxNum + 1)));
                }
            }
        }
        return tmp;
    }

    public static void PlantItemAdd(ref List<int> plantList) 
    {
        System.Random r = new System.Random();
        foreach (KeyValuePair<int, SpecialItemData> item in targetPlant_ItemIDMaxNumChance)
        {
            if (item.Value.pos != Vector3.zero)
            {
                if (Vector3.Distance(Pathea.PeCreature.Instance.mainPlayer.GetComponent<Pathea.PeTrans>().position, item.Value.pos) > item.Value.radius)
                    continue;
            }
            if (r.Next(100) < item.Value.chance)
            {
                plantList.Add(item.Value.itemID);
                plantList.Add(r.Next(1 ,item.Value.maxNum + 1));
            }
        }
    }
}
