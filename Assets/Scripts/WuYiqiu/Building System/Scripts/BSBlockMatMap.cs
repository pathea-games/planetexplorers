using Mono.Data.SqliteClient;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public class BSBlockMatMap 
{

	public static Dictionary<int, int> s_MatToItem = null;
	public static Dictionary<int, int> s_ItemToMat = null;



	public static List<int> GetAllProtoItems ()
	{
		List<int> proto_ids = new List<int>();
		foreach (int id in s_ItemToMat.Keys)
			proto_ids.Add(id);
		
		return proto_ids;
	}

	public static void Load ()
	{
		
		s_MatToItem = new Dictionary<int, int>();
		s_ItemToMat = new Dictionary<int, int>();
		
		
		try
		{
			SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("blocktype");
			while (reader.Read())
			{
				int block_id = Convert.ToInt32(reader.GetString(reader.GetOrdinal("matid")));
				int item_proto_id = Convert.ToInt32(reader.GetString(reader.GetOrdinal("itemid")));

				s_MatToItem[block_id] = item_proto_id;
				s_ItemToMat[item_proto_id] = block_id;
					
				
			}
			
		}
		catch (Exception)
		{
			Debug.LogError("Load buildtype database table erro!");
		}
	}
}
