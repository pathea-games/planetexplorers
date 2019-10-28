using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

using Pathea.PeEntityExt;
using System.Collections.Generic;
using System;
using System.Linq;
using DbField = PETools.DbReader.DbFieldAttribute;
using Pathea.PeEntityExtNpcPackage;

namespace Pathea
{
	public enum EEatType
	{
		Hp = AttribType.Hp,
		Hunger = AttribType.Hunger,
		Comfort = AttribType.Comfort,
		Max
	}

	public class NpcEatDb 
	{
		public class Item
		{
			[DbField("sort")]
			public int _sort;
			[DbField("attrID")]
			public int _typeID;
			[DbField("eatmin")]
			public float _eatmin;
			[DbField("eatmax")]
			public float _eatmax;
			[DbField("attrper")]
			public float _attrper;
			[DbField("PrototypeItem_ID")]
			public int _ProtoID;

			public ItemAttr _ItemAtrr;
			public void Init()
			{
				_ItemAtrr = new ItemAttr(_attrper,_ProtoID);
			}
		}

		public class ItemAttr
		{
			public  float _percent;
			public int _ProtoID;
			public ItemAttr(float percent,int id)
			{
				_percent = percent;
				_ProtoID = id;
			}


		}

		public class AttrPer
		{
			public  int mTypeId;
			public  float mCurPercent;
			public AttrPer(int typeid,float percent)
			{
				mTypeId = typeid;
				mCurPercent = percent;
			}
		}

		public class Items
		{
			public int mTypeId;
			public float mEatMin;
			public float mEatMax;

			float mMaxPer = 0.0f;
			float mMinPer =1.0f;
			List<Item> mLists;

			public void AddItem(Item item)
			{
				if(mLists == null)
					mLists = new List<Item>();

				mTypeId = item._typeID;
				mEatMin = item._eatmin;
				mEatMax = item._eatmax;
				if(item._attrper > mMaxPer) mMaxPer = item._attrper;
				if(item._attrper < mMinPer) mMinPer = item._attrper;

				mLists.Add(item);
				//mLists.Sort((a,b)=>b._ItemAtrr._percent.CompareTo(a._ItemAtrr._percent));
			}

			public List<Item> GetItems()
			{
				return mLists;
			}

			List<ItemAttr> GetItemAtts()
			{
				List<ItemAttr> ItemAtts = new List<ItemAttr>();
				for(int i=0;i<mLists.Count;i++)
				{
					ItemAtts.Add(mLists[i]._ItemAtrr);
				}
				return  ItemAtts;

			}

			public int[] GetProtoIdRange(float curpercent)
			{
				List<ItemAttr> ItemAtts = GetItemAtts();

				List<ItemAttr> newItemAtts = new List<ItemAttr>();
				List<int> ids = new List<int>();

				if(ItemAtts != null && ItemAtts.Count >1)
				{
//					if(curpercent < mMaxPer)
//						ItemAtts.Sort((a,b) => a._percent.CompareTo(b._percent));
//					else
//						ItemAtts.Sort((a,b) => b._percent.CompareTo(a._percent));

					for(int i=0;i<ItemAtts.Count;i++)
					{
						ItemAttr a= new ItemAttr(Math.Abs(ItemAtts[i]._percent - curpercent),ItemAtts[i]._ProtoID);
						newItemAtts.Add(a);
					}
					newItemAtts.Sort((a,b) => a._percent.CompareTo(b._percent));

					for(int i=0;i<newItemAtts.Count;i++)
					{
						ids.Add(newItemAtts[i]._ProtoID);
					}
				}

				return ids.ToArray();
			}

			public int[] GetProtoItemIds()
			{
				List<int> ids = new List<int>();
				for(int i=0;i<mLists.Count;i++)
				{
					ids.Add(mLists[i]._ProtoID);
				}
				return ids.ToArray();
			}

		}

		static Dictionary<int,Items> mData; 

		public static void LoadData()
		{
			mData = new Dictionary<int, Items>();
			
			Mono.Data.SqliteClient.SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("NpcEatList");
			while (reader.Read())
			{
				Item item = PETools.DbReader.ReadItem<Item>(reader);
				item.Init();

				if(!mData.ContainsKey(item._typeID))
					mData.Add(item._typeID,new Items());

				mData[item._typeID].AddItem(item);
			}
		}

		public static void Release()
		{
			mData = null;
		}


		public static Items GetIitems(int Type)
		{
			return mData[Type];
		}

//		public static int[] GetEatIDs(int TypeID)
//		{
//			return new int[]{393};
//		}

		public static int[] GetEatIDs(int TypeId,float curPercent)
		{
			Items items = GetIitems(TypeId);
			if(items == null)
				return null;

			return items.GetProtoIdRange(curPercent);
		}

		public static int[] GetEatIDs(EEatType type)
		{
			return GetEatIDs((int)type);
		}

		public static int[] GetEatIDs(int TypeId)
		{
			Items items = GetIitems(TypeId);
			if(items == null)
				return null;

			return items.GetProtoItemIds();
		}

		public static bool CanEat(PeEntity entity)
		{
			if(entity.UseItem == null)
				return false;
		
			List<AttrPer> attrpers;
			if(IsWantEat(entity,out attrpers))
			{
				for(int i=0;i<attrpers.Count;i++)
				{
					int[] eatids = GetEatIDs(attrpers[i].mTypeId,attrpers[i].mCurPercent);
					ItemAsset.ItemObject item = GetCanEatItemFromPackage(entity,eatids);
					if(item != null)
						return true;
				}
			}
			return false;
		}

		public static bool CanEatByAttr(PeEntity entity,AttribType type,AttribType typeMax,bool bContinue)
		{
			if(entity.UseItem == null)
				return false;

			AttrPer Db = WantByType(entity,type,typeMax,bContinue);
			if(Db == null)
				return false;

			int[] eatids = GetEatIDs(Db.mTypeId,Db.mCurPercent);
			ItemAsset.ItemObject item = GetCanEatItemFromPackage(entity,eatids);
			if(item != null)
				return true;

			return false;
		}


		public static bool CanEat(PeEntity entity,out ItemAsset.ItemObject item)
		{
			item = null;

			if(entity.UseItem == null)
				return false;
			List<AttrPer> attrpers;
			if(IsWantEat(entity,out attrpers))
			{
				for(int i=0;i<attrpers.Count;i++)
				{
					int[] eatids = GetEatIDs(attrpers[i].mTypeId,attrpers[i].mCurPercent);
					item = GetCanEatItemFromPackage(entity,eatids);
					if(item != null)
						return true;
				}
			}
			return false;
		}

		public static bool CanEatFromStorage(PeEntity entity,CSStorage storage)
		{
			ItemAsset.ItemObject item = null;
			List<AttrPer> attrpers;
			if(IsWantEat(entity,out attrpers))
			{
				for(int i=0;i<attrpers.Count;i++)
				{
					int[] eatids = GetEatIDs(attrpers[i].mTypeId,attrpers[i].mCurPercent);
					item = GetCanEatItemFromCSStorage(entity,storage,eatids);
					if(item != null)
						return true;
				}
			}
			return false;
			
		}

		public static List<int> GetWantEatIds(PeEntity entity)
		{
			List<AttrPer> attrpers;
			List<int> resIds = new List<int>();
			if(IsWantEat(entity,out attrpers))
			{
				for(int i=0;i<attrpers.Count;i++)
				{
					int[] eatids = GetEatIDs(attrpers[i].mTypeId,attrpers[i].mCurPercent);
					for(int n=0;n<eatids.Length;n++)
					{
						resIds.Add(eatids[n]);
					}
				}
			}
			return resIds;
		}

		public static bool CanEatFromStorage(PeEntity entity,CSStorage storage,out ItemAsset.ItemObject item)
		{
			item = null;
			List<AttrPer> attrpers;
			if(IsWantEat(entity,out attrpers))
			{
				for(int i=0;i<attrpers.Count;i++)
				{
					int[] eatids = GetEatIDs(attrpers[i].mTypeId,attrpers[i].mCurPercent);
					item = GetCanEatItemFromCSStorage(entity,storage,eatids);
					if(item != null)
						return true;
				}
			}
			return false;

		}

		public static bool CanEatSthFromStorages(PeEntity entity,List<CSCommon> storages)
		{
			if(storages == null)
				return false;

			ItemAsset.ItemObject item;
			for(int i=0;i<storages.Count;i++)
			{
				CSStorage storage = storages[i] as CSStorage;
				if(storage != null && IsContinueEatFromStorage(entity,storage,out item))
				{
					return true;
				}
			}
			return false;
		}

        public static bool CanEatSthFromStorages(PeEntity entity, List<CSCommon> storages, out ItemAsset.ItemObject item)
        {
            item = null;
            if (storages == null)
                return false;

            for (int i = 0; i < storages.Count; i++)
            {
                CSStorage storage = storages[i] as CSStorage;
                if (storage != null && IsContinueEatFromStorage(entity, storage, out item))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool CanEatSthFromStorages(PeEntity entity, List<CSCommon> storages, out ItemAsset.ItemObject item ,out CSStorage storage)
        {
            item = null;
            storage = null;
            if (storages == null)
                return false;

            for (int i = 0; i < storages.Count; i++)
            {
                storage = storages[i] as CSStorage;
                if (storage != null && IsContinueEatFromStorage(entity, storage, out item))
                {
                    return true;
                }
            }
            return false;
        }

		public static bool IsContinueEatFromStorage(PeEntity entity,CSStorage storage,out ItemAsset.ItemObject item)
		{
			item = null;

			List<AttrPer> attrpers;
			if(IsWantContinueEat(entity,out attrpers))
			{
				for(int i=0;i<attrpers.Count;i++)
				{
					int[] eatids = GetEatIDs(attrpers[i].mTypeId,attrpers[i].mCurPercent);
					item = GetCanEatItemFromCSStorage(entity,storage,eatids);
					if(item != null)
						return true;
				}
			}
			return false;
		}

		public static bool IsContinueEat(PeEntity entity,out ItemAsset.ItemObject item)
		{
			item = null;
			if(entity.UseItem == null)
				return false;
		
			List<AttrPer> attrpers;
			if(IsWantContinueEat(entity,out attrpers))
			{
				for(int i=0;i<attrpers.Count;i++)
				{
					int[] eatids = GetEatIDs(attrpers[i].mTypeId,attrpers[i].mCurPercent);
					item = GetCanEatItemFromPackage(entity,eatids);
					if(item != null)
						return true;
				}
			}
			return false;
		}

		static ItemAsset.ItemObject GetCanEatItemFromPackage(PeEntity entity,int[] eatIds)
		{
			if(entity.UseItem == null)
				return null;

			ItemAsset.ItemObject item = null;
			if(eatIds != null && eatIds.Length >0)
			{
				for(int i=0;i<eatIds.Length;i++)
				{
					item = entity.GetBagItemObj(eatIds[i]);
					if(item != null)
						break;
				}
			}
			return item;
		}

		static ItemAsset.ItemObject GetCanEatItemFromCSStorage(PeEntity entity,CSStorage storage,int[] eatIds)
		{
			ItemAsset.ItemObject item = null;
			for(int i=0;i<eatIds.Length;i++)
			{
				item =  storage.m_Package.FindItemByProtoId(eatIds[i]);
				if(item != null)
					break;
			}
			return item;
		}

        public static List<AttrPer> IsNeedEatAttr(PeEntity entity)
        {
            List<AttrPer> _AttrPers = null;
            IsWantEat(entity, out _AttrPers);
            return _AttrPers;
        }

		public static bool IsNeedEatsth(PeEntity entity)
		{
			List<AttrPer> _AttrPers = null;
			return IsWantEat(entity,out _AttrPers);
		}

        public static bool IsNeedContineEat(PeEntity entity)
        {
            List<AttrPer> _AttrPers = null;
            return IsWantContinueEat(entity, out _AttrPers);
        }

		static bool IsWantEat(PeEntity entity,out List<AttrPer> AttrPers)
		{
			AttrPers = new List<AttrPer>();

			if(entity == null)
				return false;

			AttrPer attrper = WantByType(entity,AttribType.Hp,AttribType.HpMax);
			if(attrper != null)
				AttrPers.Add(attrper);

			attrper = WantByType(entity,AttribType.Comfort,AttribType.ComfortMax);
			if(attrper != null)
				AttrPers.Add(attrper);

			attrper = WantByType(entity,AttribType.Hunger,AttribType.HungerMax);
			if(attrper != null)
				AttrPers.Add(attrper);

			return AttrPers.Count >0;
		}

		static bool  IsWantContinueEat(PeEntity entity,out List<AttrPer> AttrPers)
		{
			AttrPers = new List<AttrPer>();
			if(entity == null)
				return false;

			AttrPer attrper = WantByType(entity,AttribType.Hp,AttribType.HpMax,true);
			if(attrper != null)
				AttrPers.Add(attrper);
			
			attrper = WantByType(entity,AttribType.Comfort,AttribType.ComfortMax,true);
			if(attrper != null)
				AttrPers.Add(attrper);
			
			attrper = WantByType(entity,AttribType.Hunger,AttribType.HungerMax,true);
			if(attrper != null)
				AttrPers.Add(attrper);


			return AttrPers.Count >0;
		}

		public static AttrPer WantByType(PeEntity entity,AttribType _type,AttribType _typeMax,bool _bContinue = false)
	    {
			AttrPer attrper = null;
			float  curPercent = 1.0f;
			int TypeID = (int)_type;
			Items items = NpcEatDb.GetIitems(TypeID);
			if(items == null)
				return null;

			//float  eatLimt = _bContinue ? items.mEatMax : items.mEatMin;
			float Hp = entity.GetAttribute(_type);
			float HpMax = entity.GetAttribute(_typeMax);
			curPercent = Hp/HpMax;
			if(curPercent < items.mEatMax)
				attrper = new AttrPer(TypeID,curPercent);

			return attrper;
		}

		public static AttrPer WantByType(PeEntity entity,AttribType _type,AttribType _typeMax, float _eatMin,float _eatMax, bool _bContinue = false)
		{
			AttrPer attrper = null;
			float  curPercent = 1.0f;
			int TypeID = (int)_type;

			//float  eatLimt = _bContinue ? _eatMax : _eatMin;
			float p = entity.GetAttribute(_type);
			float pMax = entity.GetAttribute(_typeMax);
			curPercent = p/pMax;
			if(curPercent < _eatMax)
				attrper = new AttrPer(TypeID,curPercent);
			
			return attrper;
		}

	}

    public class NpcVoiceDb
    {
        public class Item
        {
            [DbField("ID")]
            public int _Id;
            [DbField("ScenarioID")]
            public int _SecnarioID;
            [DbField("V1")]
            public int _V1
            { set { VoiceType = new int[50]; VoiceType[0] = value; } }
            [DbField("V2")]
             public int _V2
            { set { VoiceType[1] = value; } }
            [DbField("V3")]
             public int _V3
            { set { VoiceType[2] = value; } }
            [DbField("V4")]
            public int _V4
            { set { VoiceType[3] = value; } }
            [DbField("V5")]
            public int _V5
            { set { VoiceType[4] = value; } }
            [DbField("V6")]
            public int _V6
            { set { VoiceType[5] = value; } }
            [DbField("V7")]
            public int _V7
            { set { VoiceType[6] = value; } }
            [DbField("V8")]
            public int _V8
            { set { VoiceType[7] = value; } }
            [DbField("V9")]
            public int _V9
            { set { VoiceType[8] = value; } }
            [DbField("V10")]
            public int _V10
            { set { VoiceType[9] = value; } }
            [DbField("V11")]
            public int _V11
            { set { VoiceType[10] = value; } }
            [DbField("V12")]
            public int _V12
            { set { VoiceType[11] = value; } }
            [DbField("V13")]
            public int _V13
            { set { VoiceType[12] = value; } }
            [DbField("V14")]
            public int _V14
            { set { VoiceType[13] = value; } }
            [DbField("V15")]
            public int _V15
            { set { VoiceType[14] = value; } }
            [DbField("V16")]
            public int _V16
            { set { VoiceType[15] = value; } }
            [DbField("V17")]
            public int _V17
            { set { VoiceType[16] = value; } }
            [DbField("V18")]
            public int _V18
            { set { VoiceType[17] = value; } }
            [DbField("V19")]
            public int _V19
            { set { VoiceType[18] = value; } }
            [DbField("V20")]
            public int _V20
            { set { VoiceType[19] = value; } }
            [DbField("V21")]
            public int _V21
            { set { VoiceType[20] = value; } }
            [DbField("V22")]
            public int _V22
            { set { VoiceType[21] = value; } }
            [DbField("V23")]
            public int _V23
            { set { VoiceType[22] = value; } }
            [DbField("V24")]
            public int _V24
            { set { VoiceType[23] = value; } }
            [DbField("V25")]
            public int _V25
            { set { VoiceType[24] = value; } }
            [DbField("V26")]
            public int _V26
            { set { VoiceType[25] = value; } }
            [DbField("V27")]
            public int _V27
            { set { VoiceType[26] = value; } }
            [DbField("V28")]
            public int _V28
            { set { VoiceType[27] = value; } }
            [DbField("V29")]
            public int _V29
            { set { VoiceType[28] = value; } }
            [DbField("V30")]
            public int _V30
            { set { VoiceType[29] = value; } }
            [DbField("V31")]
            public int _V31
            { set { VoiceType[30] = value; } }
            [DbField("V32")]
            public int _V32
            { set { VoiceType[31] = value; } }
            [DbField("V33")]
            public int _V33
            { set { VoiceType[32] = value; } }
            [DbField("V34")]
            public int _V34
            { set { VoiceType[33] = value; } }
            [DbField("V35")]
            public int _V35
            { set { VoiceType[34] = value; } }
            [DbField("V36")]
            public int _V36
            { set { VoiceType[35] = value; } }
            [DbField("V37")]
            public int _V37
            { set { VoiceType[36] = value; } }
            [DbField("V38")]
            public int _V38
            { set { VoiceType[37] = value; } }
            [DbField("V39")]
            public int _V39
            { set { VoiceType[38] = value; } }

            public int[] VoiceType;
        }

        static Dictionary<int,Item> mData;
        public static void LoadData()
        {
            mData =new Dictionary<int,Item>();

            Mono.Data.SqliteClient.SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("NPCVoice");
            while (reader.Read())
            {
                Item item = PETools.DbReader.ReadItem<Item>(reader);
                mData.Add(item._SecnarioID, item);
            }
        }

        public static void Release()
        {
            mData = null;
        }

        public static int GetVoiceId(int secnarioID, int voiceType)
        {
            return mData[secnarioID] != null && voiceType >0? mData[secnarioID].VoiceType[voiceType - 1] : -1;
        }
    }
}
