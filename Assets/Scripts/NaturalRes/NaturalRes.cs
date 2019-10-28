using UnityEngine;
using Mono.Data.SqliteClient;
using System;
using System.Collections;
using System.Collections.Generic;

namespace NaturalResAsset{

public class ResItemGot{
	internal int m_id;
	internal float m_probablity;
}
public class ResExtraGot{
	internal float extraPercent;
	internal List<ResItemGot> m_extraGot;
}
public class NaturalRes {
	internal int m_id;
	internal List<int> mLevel;
	internal int mIllustrationId;
	internal int m_type;
	internal float m_duration;
	internal List<ResItemGot> m_itemsGot;
	internal ResExtraGot m_extraGot;
	internal ResExtraGot m_extraSpGot;
	
	internal float mFixedNum;
	internal float mSelfGetNum;
		
	internal int mGroundEffectID;	
	internal int[] mGroundSoundIDs;	
		
	internal int mCastSkill;
	
	public static List<NaturalRes> s_tblNaturalRes;	
	public static void LoadData()
	{
		s_tblNaturalRes = new List<NaturalRes>();
		SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("resource");
		while(reader.Read()){
			NaturalRes naturalRes = new NaturalRes();
			naturalRes.m_id = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ID")));
			naturalRes.mLevel = ToTexIdList(reader.GetString(reader.GetOrdinal("level")));
			naturalRes.mIllustrationId = Convert.ToInt32(reader.GetString(reader.GetOrdinal("illustration")));
			naturalRes.m_type = Convert.ToInt32(reader.GetString(reader.GetOrdinal("type")));
			naturalRes.m_duration = Convert.ToSingle(reader.GetString(reader.GetOrdinal("durability")));
			naturalRes.m_itemsGot = ToItemsGot(reader.GetString(reader.GetOrdinal("production")));
			naturalRes.m_extraGot = ToExtraGot(reader.GetString(reader.GetOrdinal("production")));
			naturalRes.m_extraSpGot= ToExtraSpGot(reader.GetString(reader.GetOrdinal("production")));
			naturalRes.mFixedNum = Convert.ToSingle(reader.GetString(reader.GetOrdinal("fixed_num")));
			naturalRes.mSelfGetNum = Convert.ToSingle(reader.GetString(reader.GetOrdinal("self_num")));
			naturalRes.mGroundEffectID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ground_effect")));
				naturalRes.mGroundSoundIDs = PETools.Db.ReadIntArray(reader.GetString(reader.GetOrdinal("walk_sound")));
			naturalRes.mCastSkill = Convert.ToInt32(reader.GetString(reader.GetOrdinal("gather_skill")));
			s_tblNaturalRes.Add(naturalRes);
		}
	}
	public static bool MatchId(NaturalRes iter, int id){
		return iter.m_id == id;
	}	
	private static List<ResItemGot> ToItemsGot(string itemsGotDesc){
		string[] part = itemsGotDesc.Split(';');
 		string[] strings = part[0].Split(',');
		
		List<ResItemGot> itemsGot = new List<ResItemGot>();
		for( int i = 1; i < strings.Length; i+=2){
			ResItemGot itemGot = new ResItemGot();
			itemGot.m_id = Convert.ToInt32(strings[i-1]);
			itemGot.m_probablity = Convert.ToSingle(strings[i]);
			itemsGot.Add(itemGot);
		}
		return itemsGot;
	}
	private static ResExtraGot ToExtraGot(string itemsGotDesc){
		ResExtraGot extraGot = new ResExtraGot();
		extraGot.extraPercent = 0f;
		extraGot.m_extraGot = new List<ResItemGot>();
		string[] part = itemsGotDesc.Split(';');
		if(part.Length != 3)
			return extraGot;
		string[] strings = part[1].Split(',');
		if(strings.Length < 3 || strings.Length % 2 == 0)
			return extraGot;
		
		extraGot.extraPercent = Convert.ToSingle(strings[0]);
		for( int i = 2; i < strings.Length; i+=2){
			ResItemGot itemGot = new ResItemGot();
			itemGot.m_id = Convert.ToInt32(strings[i-1]);
			itemGot.m_probablity = Convert.ToSingle(strings[i]);
			extraGot.m_extraGot.Add(itemGot);
		}
		return extraGot;
	}
	private static ResExtraGot ToExtraSpGot(string itemsGotDesc)
	{
		ResExtraGot extraGot = new ResExtraGot();
		extraGot.extraPercent = 0f;
		extraGot.m_extraGot = new List<ResItemGot>();
		string[] part = itemsGotDesc.Split(';');
		if (part.Length != 3)
			return extraGot;
		string[] strings = part[2].Split(',');
		if (strings.Length < 3 || strings.Length % 2 == 0)
			return extraGot;
		
		extraGot.extraPercent = Convert.ToSingle(strings[0]);
		for (int i = 2; i < strings.Length; i += 2)
		{
			ResItemGot itemGot = new ResItemGot();
			itemGot.m_id = Convert.ToInt32(strings[i - 1]);
			itemGot.m_probablity = Convert.ToSingle(strings[i]);
			extraGot.m_extraGot.Add(itemGot);
		}
		return extraGot;
	}
	private static List<int> ToTexIdList(string texIdDesc){
 		string[] strings = texIdDesc.Split(',');
		
		List<int> texIdList = new List<int>();
		for( int i = 0; i < strings.Length; i++){
			texIdList.Add(Convert.ToInt32(strings[i]));
		}
		return texIdList;
	}
	public static NaturalRes GetTerrainResData(int vType)
	{
		for(int i = 0; i < s_tblNaturalRes.Count; ++i)
			if(s_tblNaturalRes[i].m_id == vType)
				return s_tblNaturalRes[i];
		return null;
	}
		/*
	public static NaturalRes GetBuildingResData(byte vType){
		return s_tblNaturalRes.Find(iterRes=>NaturalRes.MatchId(iterRes,50007));
	}
	*/
}
	
}
