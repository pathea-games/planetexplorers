using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;

public class StoreData
{
    public int npcid;
    //private List<int> _itemList = new List<int>();
    public string iconname;

    public List<int> itemListstory = new List<int>();
    public List<int> itemListadvensingle = new List<int>();
    public List<int> itemListadvencoop = new List<int>();
    public List<int> itemListadvenvs = new List<int>();
    public List<int> itemListadvenfree = new List<int>();

    public List<int> itemList
    {
        get 
        {
            if (Pathea.PeGameMgr.IsStory)
                return itemListstory;
            else if (Pathea.PeGameMgr.IsSingleAdventure)
                return itemListadvensingle;
            else if (Pathea.PeGameMgr.IsCooperation)
                return itemListadvencoop;
            else if (Pathea.PeGameMgr.IsVS)
                return itemListadvenvs;
            else
                return itemListadvenfree;
        }
    }
}

public class StoreRepository
{
    private static Dictionary<int, StoreData> dicShop = new Dictionary<int, StoreData>();

    public static StoreData GetStoreData(int id)
    {
        if (dicShop.ContainsKey(id))
        {
            return dicShop[id];
        }

        return null;
    }

    public static StoreData GetNpcStoreData(int npcid)
    {
        foreach (KeyValuePair<int, StoreData> iter in dicShop)
        {
            StoreData data = iter.Value;
            if (data.npcid == npcid)
                return data;
        }

        return null;
    }

    public static string GetStoreNpcIcon(int npcid)
    {
        StoreData data = GetNpcStoreData(npcid);
        //lz-2016.06.07 加入item数量过滤，避免出现NPC商店图标，但是打开NPC界面没有商店的情况(参见UINpcWnd.cs 304行)
        if (data == null || data.itemList.Count <= 0)
            return "0";

        return data.iconname;
    }

    public static void LoadData()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("NPCstore");
        while (reader.Read())
        {
            StoreData shopData = new StoreData();
            int id = Convert.ToInt32(reader.GetString(reader.GetOrdinal("store_id")));
            shopData.npcid = Convert.ToInt32(reader.GetString(reader.GetOrdinal("storenpc_id")));

            string strTemp = reader.GetString(reader.GetOrdinal("story"));
            string[] shoplist = strTemp.Split(',');
            for (int i = 0; i < shoplist.Length; i++)
            {
                if (shoplist[i] == "0")
                    continue;

                shopData.itemListstory.Add(Convert.ToInt32(shoplist[i]));
            }

            strTemp = reader.GetString(reader.GetOrdinal("adven_single"));
            shoplist = strTemp.Split(',');
            for (int i = 0; i < shoplist.Length; i++)
            {
                if (shoplist[i] == "0")
                    continue;

                shopData.itemListadvensingle.Add(Convert.ToInt32(shoplist[i]));
            }

            strTemp = reader.GetString(reader.GetOrdinal("adven_coop"));
            shoplist = strTemp.Split(',');
            for (int i = 0; i < shoplist.Length; i++)
            {
                if (shoplist[i] == "0")
                    continue;

                shopData.itemListadvencoop.Add(Convert.ToInt32(shoplist[i]));
            }

            strTemp = reader.GetString(reader.GetOrdinal("adven_vs"));
            shoplist = strTemp.Split(',');
            for (int i = 0; i < shoplist.Length; i++)
            {
                if (shoplist[i] == "0")
                    continue;

                shopData.itemListadvenvs.Add(Convert.ToInt32(shoplist[i]));
            }

            strTemp = reader.GetString(reader.GetOrdinal("adven_freemode"));
            shoplist = strTemp.Split(',');
            for (int i = 0; i < shoplist.Length; i++)
            {
                if (shoplist[i] == "0")
                    continue;

                shopData.itemListadvenfree.Add(Convert.ToInt32(shoplist[i]));
            }

            shopData.iconname = reader.GetString(reader.GetOrdinal("icon"));

            dicShop.Add(id, shopData);
        }
    }
}