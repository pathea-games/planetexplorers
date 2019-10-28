using Mono.Data.SqliteClient;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;



public class BSVoxelMatMap 
{

	public class MapData
	{
		public int matID;
		public int itemProtoID;
		public int costNum;
		public string icon;
	}

	private static List<MapData> s_Datas = null;
	private static int[] s_MatToItemMap = null;
	private static Dictionary<int, List<int>> s_ItemToMatMap = null;

	public static List<int> GetAllProtoItems ()
	{
		List<int> proto_ids = new List<int>();
		foreach (int id in s_ItemToMatMap.Keys)
			proto_ids.Add(id);

		return proto_ids;
	}
	

	public static void Load ()
	{

		s_Datas = new List<MapData>();

		s_MatToItemMap = new int[256];
		for (int i = 0; i < s_MatToItemMap.Length; i++)
		{
			s_MatToItemMap[i] = -1;
		}

		s_ItemToMatMap = new Dictionary<int, List<int>>();
	

		try
		{
			SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("buildblock");
			while (reader.Read())
			{
				int voxel_mat_id = Convert.ToInt32(reader.GetString(reader.GetOrdinal("matID")));
				int item_proto_id = Convert.ToInt32(reader.GetString(reader.GetOrdinal("itemID")));
				int cost_num = Convert.ToInt32(reader.GetString(reader.GetOrdinal("costNum")));
				string icon_name = reader.GetString(reader.GetOrdinal("icon"));

				MapData data = new MapData();
				data.matID = voxel_mat_id;
				data.itemProtoID = item_proto_id;
				data.costNum = cost_num;
				data.icon = icon_name;
				s_Datas.Add(data);

				int index = s_Datas.Count - 1;

				s_MatToItemMap[voxel_mat_id] = s_Datas.Count - 1;

				if (!s_ItemToMatMap.ContainsKey(item_proto_id))
					s_ItemToMatMap.Add(item_proto_id, new List<int>());
			
				s_ItemToMatMap[item_proto_id].Add(index);

			}
		
		}
		catch (Exception)
		{
			Debug.LogError("Load buildblock database table erro!");
		}
	}

	public static MapData GetMapData (int voxel_type)
	{
		if (s_MatToItemMap[voxel_type] != -1)
			return s_Datas[s_MatToItemMap[voxel_type]];

		return null;
	}

	public static int GetItemID (int voxel_type)
	{
		if (s_MatToItemMap[voxel_type] != -1)
			return s_Datas[ s_MatToItemMap[voxel_type]].itemProtoID;
		return -1;
	}

	public static List<int> GetMaterialIDs (int item_id)
	{
		List<int> mat_ids = new List<int>();
		if ( s_ItemToMatMap.ContainsKey(item_id))
		{

			List<int> map_indexs = s_ItemToMatMap[item_id];
			foreach (int index in map_indexs)
			{
				mat_ids.Add(s_Datas[index].matID);
			}

		}

		return mat_ids;
	}

}
