using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using System;

public class WareHouseManager : MonoBehaviour
{
    public static Dictionary<int, WareHouse> m_WareHouseMap = new Dictionary<int, WareHouse>();
    public static List<WareHouseObject> m_WareHouseObjectList = new List<WareHouseObject>();
	
	Dictionary<AssetReq, int> reqlist = new Dictionary<AssetReq, int> ();

    void Start()
    {
        RemoveAll();

        string path = "Prefab/Item/Scene/emergency_kit";

        foreach (KeyValuePair<int, WareHouse> ite in m_WareHouseMap)
        {
            UnityEngine.Object obj = Resources.Load(path);
            if (obj != null)
            {
                GameObject gameObject = Instantiate(obj, ite.Value.m_Pos, ite.Value.m_Rotation) as GameObject;
                gameObject.name = "emergency_kit";
                gameObject.transform.parent = transform;

                WareHouseObject script = gameObject.GetComponent<WareHouseObject>();
                script._id = ite.Value.m_ID;
            }
			
//			AssetBundleReq req = AssetBundlesMan.Instance.AddReq(path, ite.Value.m_Pos, ite.Value.m_Rotation);
//			req.ReqFinishWithReqHandler += OnSpawned;
//			reqlist[req] = ite.Value.m_ID;
        }
    }
	
	void CreateWareHouse(GameObject go, int id)
	{
        go.name = "emergency_kit";
        go.transform.parent = transform;
		go.transform.localScale = Vector3.one;

        WareHouseObject script = go.GetComponent<WareHouseObject>();
        script._id = id;
	}
	
	public void OnSpawned(GameObject go, AssetReq req)
	{
		if(reqlist.ContainsKey(req))
		{
			CreateWareHouse(go, reqlist[req]);
			reqlist.Remove(req);
		}
		else
			Destroy(go);
	}

    void RemoveAll()
    {
        for (int i = 0; i < m_WareHouseObjectList.Count; i++)
        {
            if (m_WareHouseObjectList[i] == null)
                continue;

            Destroy(m_WareHouseObjectList[i].gameObject);
        }

        m_WareHouseObjectList.Clear();
    }

    public static void AddWareHouseObjectList(WareHouseObject obj)
    {
        if (!m_WareHouseObjectList.Contains(obj))
             m_WareHouseObjectList.Add(obj);
    }

    public static void RemoveWareHouseObjectList(WareHouseObject obj)
    {
        if (m_WareHouseObjectList.Contains(obj))
            m_WareHouseObjectList.Remove(obj);
    }

    public static WareHouseObject GetWareHouseObject(int id)
    {
        return m_WareHouseObjectList.Find(ite => WareHouseObject.MatchID(ite, id));
    }

    public static void LoadData()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("box");
        reader.Read();
        while (reader.Read())
        {
            WareHouse ware = new WareHouse();
            ware.m_ID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("boxid")));

            string strTemp = reader.GetString(reader.GetOrdinal("position"));
            if (strTemp != "0")
            {
                string[] pos = strTemp.Split(',');
                if (pos.Length < 3)
                    Debug.LogError("Mission's LookPosition is Error");
                float x = Convert.ToSingle(pos[0]);
                float y = Convert.ToSingle(pos[1]);
                float z = Convert.ToSingle(pos[2]);
                ware.m_Pos = new Vector3(x, y, z);
            }

            strTemp = reader.GetString(reader.GetOrdinal("rotation"));
            if (strTemp != "0")
            {
                string[] pos = strTemp.Split(',');
                if (pos.Length < 4)
                    Debug.LogError("Mission's LookPosition is Error");

                float x = Convert.ToSingle(pos[0]);
                float y = Convert.ToSingle(pos[1]);
                float z = Convert.ToSingle(pos[2]);
                float w = Convert.ToSingle(pos[3]);
                ware.m_Rotation = new Quaternion(x, y, z, w);
            }

			ware.m_itemsDesc = reader.GetString(reader.GetOrdinal("itemlist"));
            

            m_WareHouseMap.Add(ware.m_ID, ware);
        }
    }

    public static WareHouse GetWareHouseData(int id)
    {
        if (m_WareHouseMap.ContainsKey(id))
        {
            if (m_WareHouseMap[id] == null)
                return null;

            return m_WareHouseMap[id];
        }

        return null;
       // return m_WareHouseMap.ContainsKey(id) ? m_WareHouseMap[id] : null;
    }
}
