using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Mono.Data.SqliteClient;

public class ShopData
{
    public int m_ID;
    public int m_ItemID;


	public int m_Price1;
    public int m_Price2;
	public int m_Price
	{
		get
		{
			if (!Pathea.Money.Digital)//Pathea.Money.Digital
			{
				return m_Price1;
			}
			else
			{
				return m_Price2;
			}
		}
	}
    public int m_ExtDemand;
    public int m_LimitNum;
    public int m_RefreshTime;
    public int m_LimitType;
    public List<int> m_LimitMisIDList = new List<int>();
}

public class ShopRespository
{
    public static Dictionary<int, ShopData> m_ShopMap = new Dictionary<int, ShopData>();
    public static Dictionary<int, int> m_ShopRefresh = new Dictionary<int, int>();

    public static ShopData GetShopData(int id)
    {
        return m_ShopMap.ContainsKey(id) ? m_ShopMap[id] : null;
    }
	
	public static ShopData GetShopDataByItemId(int itemId)
	{
		foreach(ShopData sD in m_ShopMap.Values)
			if(sD.m_ItemID == itemId)
				return sD;
		return null;
	}

    public static void LoadData()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("store");


        reader.Read();

		while (reader.Read())
        {
            ShopData data = new ShopData();
           data.m_ID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("store_id")));
           data.m_ItemID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("item_id")));

			data.m_Price1 = Convert.ToInt32(reader.GetString(reader.GetOrdinal("sale_price")));
            data.m_Price2 = Convert.ToInt32(reader.GetString(reader.GetOrdinal("sale_price2")));
            data.m_ExtDemand = Convert.ToInt32(reader.GetString(reader.GetOrdinal("extra_demand")));
            data.m_LimitNum = Convert.ToInt32(reader.GetString(reader.GetOrdinal("sale_limit")));
            data.m_RefreshTime = Convert.ToInt32(reader.GetString(reader.GetOrdinal("refresh_time")));

            if (data.m_LimitNum > 0)
                m_ShopRefresh.Add(data.m_ID, data.m_RefreshTime);

            string strTmp = reader.GetString(reader.GetOrdinal("PreQuestLimit"));
            string[] tmpList = strTmp.Split(':');
            if(tmpList.Length == 2)
            {
                data.m_LimitType = Convert.ToInt32(tmpList[0]);
                string[] tmpList2 = tmpList[1].Split(',');
                for (int i = 0; i < tmpList2.Length; i++)
                {
                    int id = Convert.ToInt32(tmpList2[i]);
                    if (id == 0)
                        continue;

                    data.m_LimitMisIDList.Add(id);
                }
            }

            m_ShopMap.Add(data.m_ID, data);
			//m_ShopMap.Add(data.m_ItemID, data);
        }
    }
	
	public static int GetPriceBuyItemId(int itemId)
	{
		foreach(int key in m_ShopMap.Keys)
		{
			if(m_ShopMap[key].m_ItemID == itemId)
				return m_ShopMap[key].m_Price;
		}
		
		return 0;
	}

	public static int GetLimitNum(int id){
		if(!m_ShopMap.ContainsKey(id))
			return -1;
		return m_ShopMap[id].m_LimitNum;
	}

	public static List<int> GetAllIdOfSameItem(int id){
		if(!m_ShopMap.ContainsKey(id))
			return null;
		int itemId = m_ShopMap[id].m_ItemID;
		List<int> result = new List<int> ();
		foreach(KeyValuePair<int,ShopData> kvp in m_ShopMap){
			if(kvp.Value.m_ItemID==itemId)
				result.Add(kvp.Key);
		}
		return result;
	}
}
