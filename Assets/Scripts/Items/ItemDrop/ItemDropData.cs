using UnityEngine;
using Mono.Data.SqliteClient;
using System;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;

public class ItemDropData
{
	struct MeatData
	{
		public int lower;
		public int upper;
	}
	struct DropData
	{
		public int id;
		public float pro;
	}
	int _cnt;
	List<DropData> _dropList = new List<DropData>();
	MeatData _meatData = new MeatData();

	public static List<ItemSample> GetDropItems(int id)
	{
		ItemDropData itemDropData = null;
		if(!s_ItemDropDataTbl.TryGetValue(id, out itemDropData))
		{
			return null;
		}

		List<ItemSample> ret = new List<ItemSample>();
		System.Random r = new System.Random();
		// Meat
		int n = r.Next(itemDropData._meatData.lower, itemDropData._meatData.upper);
		if(n > 0)
		{
			ret.Add(new ItemSample(229,n));
		}
		// Other
		for(int i = 0; i < itemDropData._cnt; i++)
		{
			foreach(DropData dat in itemDropData._dropList)
			{
				float perc = (float)r.NextDouble();
				if(perc < dat.pro)
				{
					if(dat.id > 0)
					{
						ItemSample item = ret.Find(it=>it.protoId == dat.id);
						if(item == null)
						{
							ret.Add(new ItemSample(dat.id));
						}
						else
						{
							item.stackCount += 1;
						}
					}
					break;
				}
			}
		}
		return ret;
	}
	public static Dictionary<int, ItemDropData> s_ItemDropDataTbl = null;
	public static void LoadData()
	{
		if (s_ItemDropDataTbl != null)			return;

		s_ItemDropDataTbl = new Dictionary<int, ItemDropData>();
		SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("loot");		
		while (reader.Read())
		{
			string strId = reader.GetString(reader.GetOrdinal("id"));
			int id = Convert.ToInt32(strId);

			bool validData = false;
			// Meat
			ItemDropData itemDropData = new ItemDropData();
			string strMeat = reader.GetString(reader.GetOrdinal("meat"));
			string[] strLowerUpper = strMeat.Split(';');
			if(strLowerUpper.Length == 2)
			{
				itemDropData._meatData.lower = Convert.ToInt32(strLowerUpper[0]);
				itemDropData._meatData.upper = Convert.ToInt32(strLowerUpper[1]);
				validData = true;
			}
			// Other
			string strTmp = reader.GetString(reader.GetOrdinal("loot"));
			string[] strlist0 = strTmp.Split(';');
			if (strlist0.Length == 2)
			{
				int count = Convert.ToInt32(strlist0[0]);
				string[] stritemlist = strlist0[1].Split(',');
				if (count > 0 && stritemlist.Length > 0)
				{
					List<DropData> dropLst = new List<DropData>();
					for (int i = 0; i < stritemlist.Length; i++)
					{
						string[] strlist1 = stritemlist[i].Split('_');
						if (strlist1.Length == 2)
						{
							DropData dropData = new DropData();
							dropData.id = Convert.ToInt32(strlist1[0]);
							dropData.pro = Convert.ToSingle(strlist1[1]);
							dropLst.Add(dropData);
						}
					}
					itemDropData._cnt = count;
					itemDropData._dropList = dropLst;
				}
				validData = true;
			}
			if(validData)	s_ItemDropDataTbl[id] = itemDropData;
		}
	}
}