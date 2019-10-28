  using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

using Pathea.PeEntityExt;
using System.Collections.Generic;
using System;
using DbField = PETools.DbReader.DbFieldAttribute;

namespace Pathea
{
	public class AttPlusNPCData 
	{
//		public class AttDb
//		{
//			public class Data
//			{
//				public AttribType mType;
//				public int mvalve;
//
//				public Data(int type,int valve)
//				{
//					mType = (AttribType)type;
//					mvalve = valve;
//				}
//			}
//
//			public List<Data>  mlists = new List<Data>();
//			public static AttDb LoadFromText(string tmp)
//			{
//				if (!string.IsNullOrEmpty(tmp) && tmp != "0")
//				{
//					AttDb Attbase = new AttDb();
//					string[] tmplist = tmp.Split(';');
//					for (int i = 0; i < tmplist.Length; i++)
//					{
//						string[] s = tmplist[i].Split(',');
//						if (2 == s.Length)
//						{
//							Attbase.mlists.Add(new Data(Convert.ToInt32(s[0]), Convert.ToInt32(s[1])));
//						}
//						else
//						{
//							Debug.LogError("string[" + tmplist[i] + "] cant be splited by ',' to 2 parts.");
//						}
//					}
//					return Attbase;
//				}
//				return null;
//			}
//
//			public Data Get(AttribType type)
//			{
//				return mlists.Find((item) =>
//				                  {
//					if (item.mType == type)
//					{
//						return true;
//					}
//					
//					return false;
//				});
//			}
//
//			public List<int> GetType()
//			{
//				List<int> types = new List<int>();
//				foreach(Data data in mlists)
//				{
//					types.Add((int)data.mType);
//				}
//				return types;
//			}
//		}

		public class AttrPlus
		{
			public struct RandomInt
			{
				public int m_Min;
				public int m_Max;
				
				public int Random()
				{
					return UnityEngine.Random.Range(m_Min, m_Max);
				}
				
				public static RandomInt Load(string text)
				{
					RandomInt RandMax = new RandomInt();
					
					string[] tmplist;
					tmplist = text.Split('_');
					if (tmplist.Length != 2)
					{
						Debug.LogError("load RandomInt error:" + text);
					}
					else
					{
						RandMax.m_Min = Convert.ToInt32(tmplist[0]);
						RandMax.m_Max = Convert.ToInt32(tmplist[1]);
					}
					return RandMax;
				}
			}

			public struct Data
			{
				public int AttrID;
				public RandomInt PlusValue;
			}

			public  List<Data> mList = new List<Data>();
			public static AttrPlus LoadFromText(string tmp)
			{
				if (!string.IsNullOrEmpty(tmp))
				{
					AttrPlus plus = new AttrPlus();
					string[] tmpList;
					tmpList = tmp.Split(';');
					for(int i=0;i<tmpList.Length;i++)
					{
						string[] tmplist2;
						tmplist2 = tmpList[i].Split(',');
						if(tmplist2.Length %2 != 0)
						{
							Debug.LogError("load RandomInt error:" + tmp);
						}
						else
						{
							AttrPlus.Data data = new Data();
							data.AttrID = Convert.ToInt32(tmplist2[0]);
							data.PlusValue = RandomInt.Load(tmplist2[1]);
							plus.mList.Add(data);
						}
					}
					return plus;
				}
				return null;
			}

			public new List<AttribType> GetType()
			{
				List<AttribType> types = new List<AttribType>();
				foreach(var type in mList)
				{
					types.Add((AttribType)type.AttrID);
				}
				return types;
			}

			public Data GetPlusRandom(AttribType type )
			{
				return mList.Find((item) =>
				                  {
					if(item.AttrID == (int)type)
					{
						return true;
					}
					{
						return false;
					}
				}
				                  );
			}
		}

		public class Item
		{
			[DbField("NPC")]
			public int _id;
//			[DbField("RandNPC_Sort")]
//			public int RandNpcTypeid;
			[DbField("AttPlus")]
			string attPlus
			{
				set
				{
					AttPlus = AttrPlus.LoadFromText(value);
				}
			}
			public AttrPlus AttPlus;
			[DbField("PlusCount")]
			public int PlusCount;
		}

		static List<Item> sList;

		public static void Load()
		{
			sList = new List<Item>();
			
			Mono.Data.SqliteClient.SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("AttPlusNPC");
			while (reader.Read())
			{
				Item item = PETools.DbReader.ReadItem<Item>(reader);

				sList.Add(item);
			}
		}

		public static void Release()
		{
			sList = null;
		}

		public static Item Get(int id)
		{
			return sList.Find((item) =>
			                  {
				if (item._id == id)
				{
					return true;
				}
				
				return false;
			});
		}

		public static bool ComparePlusCout(int NpcId,int curCout)
		{
			AttPlusNPCData.Item npc = AttPlusNPCData.Get(NpcId);
			if(npc == null || npc.AttPlus == null)
				return false;

			return curCout < npc.PlusCount;
		}

		public static int GetPlusCount(int NpcId)
		{
			AttPlusNPCData.Item npc = AttPlusNPCData.Get(NpcId);
			if(npc == null || npc.AttPlus == null)
				return -1;

			return npc.PlusCount;
		}

		public static bool GetRandom(int npcId,AttribType type,out AttPlusNPCData.AttrPlus.RandomInt Rand)
		{

			Rand = new AttrPlus.RandomInt();
			AttPlusNPCData.Item npc = AttPlusNPCData.Get(npcId);
			if(npc == null || npc.AttPlus == null)
				return false;

			AttPlusNPCData.AttrPlus.Data data = npc.AttPlus.GetPlusRandom(type);
			Rand = data.PlusValue;
			return true;

		}

		public static AttribType GetRandMaxAttribute(int npcId,PESkEntity peSkentity)
		{
			AttPlusNPCData.Item npc = AttPlusNPCData.Get(npcId);
			if(npc == null || npc.AttPlus == null)
			{
				Debug.Log("Don't have NPcdata" + npcId);
				return AttribType.Max;
			}
		
			return GetRandMaxAttr(npcId,peSkentity,npc.AttPlus.GetType().ToArray());

		}

		public static AttribType  GetProtoMaxAttribute(int npcId,PESkEntity peSkentity)
		{
			AttPlusNPCData.Item npc = AttPlusNPCData.Get(npcId);
			if(npc == null || npc.AttPlus == null)
				return AttribType.Max;

			return GetProtoMaxAttr(npcId,peSkentity,npc.AttPlus.GetType().ToArray());
		}

		private static AttribType GetRandMaxAttr(int npcId,PESkEntity entity,AttribType[] ChangeAbleAttr)
		{
			float maxP = 0;
			AttribType FindAttr = ChangeAbleAttr[0];
			RandomNpcDb.Item rand = RandomNpcDb.Get(npcId);
			RandomNpcDb.RandomInt randomint = new RandomNpcDb.RandomInt();
			for(int i = 0; i < ChangeAbleAttr.Length; i++)
			{
				if(!rand.TryGetAttrRandom(ChangeAbleAttr[i],out randomint) || randomint.m_Max == 0)
					continue;

				float currentAttrP = (entity.GetAttribute(ChangeAbleAttr[i],false) - randomint.m_Min)/ (randomint.m_Max - randomint.m_Min);
				if(currentAttrP > maxP)
				{
					FindAttr = ChangeAbleAttr[i];
					maxP = currentAttrP;
				}
			}
			return FindAttr;
		}

		private static AttribType GetProtoMaxAttr(int npcId,PESkEntity entity,AttribType[] ChangeAbleAttr)
		{
//			float maxP = 0;
			//主线NPC固定
			AttribType FindAttr = ChangeAbleAttr[0];

//			NpcProtoDb.Item Proto = NpcProtoDb.Get(npcId);
//			Pathea.PESkEntity.Attr[] attr = Proto.dbAttr.ToAliveAttr();
//			if(attr == null)
//				return AttribType.Max;
//
//			for(int i=0;i<attr.Length;i++)
//			{
//
//			}
//			
//			for(int i=0;i<ChangeAbleAttr.Length;i++)
//			{
//				float currentAttrP = entity.GetAttribute(ChangeAbleAttr[i])/Proto.dbAttr.attributeArray[(int)ChangeAbleAttr[i]];
//				if(currentAttrP > maxP)
//				{
//					FindAttr = ChangeAbleAttr[i];
//					maxP = currentAttrP;
//				}
//			}
			return FindAttr;
		}

//		public static Item GetRandType(int id)
//		{
//			return sList.Find((item) =>
//			                  {
//				if (item.RandNpcTypeid == id)
//				{
//					return true;
//				}
//				
//				return false;
//			});
//		}

	}

	public class AttPlusBuffDb
	{
		public class Item
		{
			[DbField("AttPlusID")]
			public int _type;
			[DbField("skBuff_id")]
			public int _buffId;
		}

		static List<Item> sList;

		public static void Load()
		{
			sList = new List<Item>();
			
			Mono.Data.SqliteClient.SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("AttPlusBuff");
			while (reader.Read())
			{
				Item item = PETools.DbReader.ReadItem<Item>(reader);
				
				sList.Add(item);
			}
		}

		public static void Release()
		{
			sList = null;
		}
		
		public static Item Get(int buffid)
		{
			return sList.Find((item) =>
			                  {
				if (item._buffId == buffid)
				{
					return true;
				}
				
				return false;
			});
		}

		public static Item Get(AttribType type)
		{
			return sList.Find((item) =>
			                  {
				if (item._type == (int)type)
				{
					return true;
				}
				
				return false;
			});
		}
	}


	public class NPCAimData
	{
		public static  Vector3 GetOffset(float distance)
		{
			return Vector3.zero;
//			Vector3 w = new Vector3(1f * (1.2f - 0.4f * UnityEngine.Random.value), 1f * (1.2f - 0.4f * UnityEngine.Random.value), 1f * (1.2f - 0.4f * UnityEngine.Random.value));
//			float maxDis = 500f;
//			float maxOffset = 2f;
//			float offsetMax = Mathf.Clamp(distance, 0, maxDis) / maxDis * maxOffset;
//			if(distance <= minper) offsetMax = 0.0f;
//			float initPhase = Mathf.PI * time / w;
//			return new Vector3(offsetMax * Mathf.Sin(Time.time / w.x), offsetMax * Mathf.Sin( Time.time / w.y), offsetMax * Mathf.Sin( Time.time / w.z));
		}
	}
}
