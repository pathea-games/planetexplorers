using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

using Pathea.PeEntityExt;
using System.Collections.Generic;
using System;
using DbField = PETools.DbReader.DbFieldAttribute;

namespace Pathea
{
	public class NpcTypeDb  
	{

		public class Item
		{
			[DbField("TypeID")]
			public int _id;
			[DbField("TypeName")]
			public string _name;
			[DbField("Leisure")]
			int _Leisure
			{set{mControlInfo[(int)ENpcControlType.Leisure] = value;}}
			[DbField("Interaction")]
			int _Interaction
			{set{mControlInfo[(int)ENpcControlType.Interaction] = value;}}
			[DbField("Stroll")]
			int _Stroll
			{set{mControlInfo[(int)ENpcControlType.Stroll] = value;}}
			[DbField("Patrol")]
			int _Patrol
			{set{mControlInfo[(int)ENpcControlType.Patrol] = value;}}
			[DbField("Guard")]
			int _Guard
			{set{mControlInfo[(int)ENpcControlType.Guard] = value;}}
			[DbField("Dining")]
			int _Dining
			{set{mControlInfo[(int)ENpcControlType.Dining] = value;}}
			[DbField("Sleep")]
			int _Sleep
			{set{mControlInfo[(int)ENpcControlType.Sleep] = value;}}
			[DbField("MoveTo")]
			int _MoveTo
			{set{mControlInfo[(int)ENpcControlType.MoveTo] = value;}}
			[DbField("Work")]
			int _Work
			{set{mControlInfo[(int)ENpcControlType.Work] = value;}}
			[DbField("Cure")]
			int _Cure
			{set{mControlInfo[(int)ENpcControlType.Cure] = value;}}
			[DbField("ChangeRole")]
			int _ChangeRole
			{set{mControlInfo[(int)ENpcControlType.ChangeRole] = value;}}
			[DbField("AddHatred")]
			int _AddHatred
			{set{mControlInfo[(int)ENpcControlType.AddHatred] = value;}}
			[DbField("ReceiveHatred")]
			int _ReceiveHatred
			{set{mControlInfo[(int)ENpcControlType.ReceiveHatred] = value;}}
			[DbField("InjuredHatred")]
			int _InjuredHatred
			{set{mControlInfo[(int)ENpcControlType.InjuredHatred] = value;}}
			[DbField("SelfDefense")]
			int _SelfDefense
			{set{mControlInfo[(int)ENpcControlType.SelfDefense] = value;}}
			[DbField("Pursuit")]
			int _Pursuit
			{set{mControlInfo[(int)ENpcControlType.Pursuit] = value;}}
			[DbField("Assist")]
			int _Assist
			{set{mControlInfo[(int)ENpcControlType.Assist] = value;}}
			[DbField("Recourse")]
			int _Recourse
			{set{mControlInfo[(int)ENpcControlType.Recourse] = value;}}
			[DbField("Attack")]
			int Attack
			{set{mControlInfo[(int)ENpcControlType.Attack] = value;}}
			[DbField("Dodge")]
			int _Dodge
			{set{mControlInfo[(int)ENpcControlType.Dodge] = value;}}
			[DbField("Block")]
			int _Block
			{set{mControlInfo[(int)ENpcControlType.Block] = value;}}
			[DbField("CanTalk")]
			int _CanTalk
			{set{mControlInfo[(int)ENpcControlType.CanTalk] = value;}}
			[DbField("CanHanded")]
			int _CanHanded
			{set{mControlInfo[(int)ENpcControlType.CanHanded] = value;}}

			int[] mControlInfo = new int[(int)ENpcControlType.Max + 1];
		
			public bool CanRun(ENpcControlType type)
			{
				return mControlInfo[(int)type] !=0;
			}
		}

		static List<Item> sList;
		public static void Load()
		{
			sList = new List<Item>();
			
			Mono.Data.SqliteClient.SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("NPCType");
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
            int n = sList.Count;

            for (int i = 0; i < n; i++)
            {
                if (sList[i]._id == id)
                    return sList[i];
            }

            return null;
		}

		public static bool CanRun(int typeId,ENpcControlType type)
		{
			Item item = Get(typeId);
			if(item == null)
				return false;

			return item.CanRun(type);
		}
	}


}
