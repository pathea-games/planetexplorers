using UnityEngine;
using Mono.Data.SqliteClient;
using System;

public class RelationInfo 
{
	public string relationLevelEN;
	public string relationLevelCN;
	public bool warState;
	public bool specialMission;
	public bool normalMission;
	public bool canUseShop;
	public float shopPriceScale;
	public bool canUseBuilding;

	static RelationInfo[] g_RelationInfos;

	public static RelationInfo GetData(ReputationSystem.ReputationLevel level)
	{
		return g_RelationInfos[(int)level];
	}

	public static void LoadData()
	{
		g_RelationInfos = new RelationInfo[(int)ReputationSystem.ReputationLevel.MAX];

		SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("RelationInfo");
		//CancelFirstRow
		reader.Read();

		int count = 0;

		while (reader.Read())
		{
			RelationInfo data = new RelationInfo();
			data.relationLevelEN = reader.GetString(reader.GetOrdinal("RelationLevel"));
			data.relationLevelCN = reader.GetString(reader.GetOrdinal("RelationLevel_CN"));
			data.warState = Convert.ToInt32(reader.GetString(reader.GetOrdinal("WarState"))) > 0;
			data.specialMission = Convert.ToInt32(reader.GetString(reader.GetOrdinal("SpecialMission"))) > 0;
			data.normalMission = Convert.ToInt32(reader.GetString(reader.GetOrdinal("NormalMission"))) > 0;
			data.canUseBuilding = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Building"))) > 0;

			string shopStr = reader.GetString(reader.GetOrdinal("Shop"));
			string[] subStrs = shopStr.Split(',');
			data.canUseShop = Convert.ToInt32(subStrs[0]) > 0;
			data.shopPriceScale = Convert.ToSingle(subStrs[1]);
			g_RelationInfos[(int)ReputationSystem.ReputationLevel.MAX - ++count] = data;
		}
	}
}
