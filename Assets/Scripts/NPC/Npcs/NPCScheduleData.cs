
using UnityEngine;
using System.Collections;

using Pathea.PeEntityExt;
using System.Collections.Generic;
using System;
using DbField = PETools.DbReader.DbFieldAttribute;

namespace Pathea
{
	public enum EScheduleType
	{
		None = 0,
		Sleep,
		Eat,
		Chat
	}

	public class NPCScheduleData 
	{ 
		public class Item
		{
			[DbField("ID")]
			public int id;
			[DbField("time")]
			public string time
			{
				set{LoadSlots(value);}
			}
			[DbField("team")]
			public int team;

			public EScheduleType ScheduleType { get { return (EScheduleType)id;}}
			public List<CheckSlot> Slots = new List<CheckSlot>();
			void LoadSlots(string str)
			{
				if (str != "")
				{
					string[] dataStr = PETools.PEUtil.ToArrayString(str, ',');
					foreach (string item in dataStr)
					{
						float[] data =  PETools.PEUtil.ToArraySingle(item, '_');
						if (data.Length == 2)
						{
							Pathea.CheckSlot slot = new CheckSlot(data[0],data[1]);
							Slots.Add(slot);
						}
					}
				}
			}

		}
		
		static List<Item> sList;
		public static void Load()
		{
			sList = new List<Item>(4);
			
			Mono.Data.SqliteClient.SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("NPCSchedule");
			
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
				if (item.id == id)
				{
					return true;
				}
				
				return false;
			});
		}
	}
}

