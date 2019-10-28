using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

using Pathea.PeEntityExt;
using System.Collections.Generic;
using System;
using DbField = PETools.DbReader.DbFieldAttribute;

namespace Pathea
{
	public enum ENpcTalkType
	{
		None = 0,
		Conscribe_succeed,
		Dissolve,
		Business_trip,
		CallBack,
		Rest,//5
		Summon,
		Follower_comfort_common,
		Follower_comfort_medium,
		Follower_comfort_low,
		Follower_hunger_medium,//10
		Follower_hunger_low,
		Follower_health_medium,
		Follower_health_low,
		MainPlayer_comfort_common,
		MainPlayer_comfort_medium,//15
		MainPlayer_comfort_low,
		MainPlayer_hunger_medium,
		MainPlayer_hunger_low,
		MainPlayer_health_medium,
		MainPlayer_health_low,//20
		MainPlayer_sick,
		Day_to_day,
		NpcSick,
		NpcResurgence,
		NpcCombat,//25
		NpcAddBlood,
		BaseNpc_strike_hungger,
		BaseNpc_strike_sleep,
		Follower_Pkg_full,
		Follower_cut,//30
		Follower_Gather,
		Follower_Loot,
		Follower_LackDurability, //武器耐久不足使用
		Follower_LackAmmunition,//武器弹药不足
		Max
	}

	public class TalkInfo
	{
		public float mStartTime;
		public float mLoopTime;
		public ETalkLevel mLevel;
		public TalkInfo(float time,float loopTime,ETalkLevel level)
		{
			mStartTime = time;
			mLevel =level;
			mLoopTime = loopTime;
		}
	}

	public class NpcRandomTalkDb 
	{

		public class RandomCase
		{
			public List<int> listCases;
			public RandomCase()
			{
				listCases = new List<int>();
			}

			public int RandCase()
			{
				return listCases[UnityEngine.Random.Range(0, listCases.Count)];
			}

			public static RandomCase Load(string text)
			{
				if (!string.IsNullOrEmpty(text))
				{
					RandomCase _case = new RandomCase();
					string[] templist;
					templist = text.Split(',');
					for(int i=0;i<templist.Length;i++)
					{
						_case.listCases.Add(Convert.ToInt32(templist[i]));
					}
					return _case;
				}
				else
				{
					Debug.LogError("load RandomInt error:" + text);
					return null;
				}
			}
		}

		class RandomAttrChoce
		{
			List<AttribType> lists;
			public RandomAttrChoce()
			{
				lists = new List<AttribType>();
				lists.Add(AttribType.Hunger);
				lists.Add(AttribType.Hp);
				lists.Add(AttribType.Comfort);
			}

			public AttribType RandType()
			{
				return lists[(int)UnityEngine.Random.Range(0,lists.Count)];
			}
		}

		public class Item
		{
			[DbField("ID")]
			public int _id;
			public ENpcTalkType TalkType
			{get{return (ENpcTalkType)_id;}}
			
			[DbField("value")]
			public float _value;
			[DbField("probability")]
			public float _probability;
			[DbField("interval")]
			float interval
			{
				set{_interval = value *60.0f;}
			}
			public float _interval;
			[DbField("scenario")]
			string _scenario
			{
				set{Scenario = RandomCase.Load(value);}
			}
			public RandomCase Scenario;
			public AttribType Type;
			public AttribType TypeMax;
			public ETalkLevel Level;

		}

		static List<Item> sList;
		
		public static void Load()
		{
			sList = new List<Item>();
			
			Mono.Data.SqliteClient.SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("Npc_randomtalk");
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

//			for(int i=0;i<sList.Count;i++)
//			{
//				if(sList[i]._id == id)
//					return sList[i];
//			}
			return sList.Find((item) =>
			                  {
				if (item._id == id)
				{
					return true;
				}
				
				return false;
			});
		} 

		public static Item Get(ENpcTalkType type)
		{
			return Get((int)type);
		}

		public static int GetTalkCase(int id)
		{
			NpcRandomTalkDb.Item item = NpcRandomTalkDb.Get(id);
			if(item == null )
				return -1;

			if(UnityEngine.Random.value <= item._probability)
			{
				return item.Scenario.RandCase();
			}
			return -1;
		}

		public static int GetTalkCase(int id,out float time)
		{
			time = 0.0f;
			NpcRandomTalkDb.Item item = NpcRandomTalkDb.Get(id);
			if(item == null )
				return -1;
			
			if(UnityEngine.Random.value <= item._probability)
			{
				time = item._interval;
				return item.Scenario.RandCase();
			}
			return -1;
		}

		public static List<Item> GetTalkItems(PeEntity entity)
		{
			if(entity == null)
				return null;
			
			List<Item> items = new List<Item>();
			Item curItem;

			curItem = GetTalkItemByType(entity,AttribType.Hp,AttribType.HpMax);
			if(curItem != null)
				items.Add(curItem);

			curItem = GetTalkItemByType(entity,AttribType.Hunger,AttribType.HungerMax);
			if(curItem != null)
				items.Add(curItem);

			curItem = GetTalkItemByType(entity,AttribType.Comfort,AttribType.ComfortMax);
			if(curItem != null)
				items.Add(curItem);

			return items;
		}

		public static Item GetTalkItemByType(PeEntity entity,AttribType _type,AttribType _typeMax)
		{
			Item curItem = null;
			float typevalue = entity.GetAttribute(_type);
			ETalkLevel typeLevel = SwichAtrrLevel(_type,typevalue,entity);
			if(typeLevel != ETalkLevel.Max)
			{
				
				curItem = GetTalkItem(_type,typeLevel);
				curItem.Type = _type;
				curItem.TypeMax = _typeMax;
				curItem.Level = typeLevel;
			}
			return curItem;
		}


		
		public static List<Item> GetPlyerTalkItems(PeEntity entity)
		{
			if(entity == null)
				return null;
			
			List<Item> items = new List<Item>();
			Item curItem;
			float Hp = entity.GetAttribute(AttribType.Hp);
			ETalkLevel HpLevel = SwichAtrrLevel(AttribType.Hp,Hp,entity);
			if(HpLevel != ETalkLevel.Max)
			{
				
				curItem = GetPlyerTalkItem(AttribType.Hp,HpLevel);
				curItem.Type = AttribType.Hp;
				curItem.Level = HpLevel;
				items.Add(curItem);
			}
			
			
			float Hunger = entity.GetAttribute(AttribType.Hunger);
			ETalkLevel Hungerlevel = SwichAtrrLevel(AttribType.Hunger,Hunger,entity);
			if(Hungerlevel != ETalkLevel.Max)
			{
				curItem = GetPlyerTalkItem(AttribType.Hunger,Hungerlevel);
				curItem.Type = AttribType.Hunger;
				curItem.Level = Hungerlevel;
				items.Add(curItem);
			}
			
			float Comfort = entity.GetAttribute(AttribType.Comfort);
			ETalkLevel Comfortevel = SwichAtrrLevel(AttribType.Comfort,Comfort,entity);
			if(Comfortevel != ETalkLevel.Max)
			{
				curItem = GetPlyerTalkItem(AttribType.Comfort,Comfortevel);
				curItem.Type = AttribType.Comfort;
				curItem.Level = Comfortevel;
				items.Add(curItem);
			}
			return items;
		}



		private static Dictionary<ETalkLevel,Item> SwichItems(AttribType type)
		{
			Dictionary<ETalkLevel,Item> Items = null;
			switch(type)
			{
			case AttribType.Hp:
				Items = GetHealthCase();
				break;
			case AttribType.Hunger:
				Items = GetHungerCase();
				break;
			case AttribType.Comfort:
				Items = GetComfortCase();
				break;
			default:
				break;
			}
			return Items;
		}

		private static Dictionary<ETalkLevel,Item> SwichMainPlyerItems(AttribType type)
		{
			Dictionary<ETalkLevel,Item> Items = null;
			switch(type)
			{
			case AttribType.Hp:
				Items = GetPlyerHealthCase();
				break;
			case AttribType.Hunger:
				Items = GetPlyerHungerCase();
				break;
			case AttribType.Comfort:
				Items = GetPlyerComfortCase();
				break;
			default:
				break;
			}
			return Items;
		}


		private static Item GetTalkItem(AttribType type,ETalkLevel level)
		{
			Dictionary<ETalkLevel,Item> Items = SwichItems(type);
			return Items[level];
		}

		private static Item GetPlyerTalkItem(AttribType type,ETalkLevel level)
		{
			Dictionary<ETalkLevel,Item> Items = SwichMainPlyerItems(type);
			return Items[level];
		}


		private static ETalkLevel SwichAtrrLevel(AttribType type,float attr,PeEntity entity)
		{
			switch(type)
			{
			case AttribType.Hp:
				  return SwichHealthLevel(attr,entity);
			case AttribType.Hunger:
				return SwichHungerLevel(attr,entity);
			case AttribType.Comfort:
				return SwichComfortLevel(attr,entity);
			default:
				return ETalkLevel.Max;
			}
			//return ETalkLevel.Max;
		}

		public static int CheckAttrbCase(PESkEntity peskentity)
		{
			RandomAttrChoce attr = new RandomAttrChoce();
			AttribType type = attr.RandType();
			float curvalue = peskentity.GetAttribute(type);
			switch(type)
			{
			case AttribType.Hunger:
			{
				float maxHunger = peskentity.GetAttribute(AttribType.HungerMax);

				if(curvalue <= maxHunger * NpcRandomTalkDb.Get((int)ENpcTalkType.Follower_hunger_medium)._value
				   && curvalue > maxHunger * NpcRandomTalkDb.Get((int)ENpcTalkType.Follower_hunger_low)._value)
				{
					return NpcRandomTalkDb.Get((int)ENpcTalkType.Follower_hunger_medium).Scenario.RandCase();
				}else if(curvalue <= maxHunger * NpcRandomTalkDb.Get((int)ENpcTalkType.Follower_health_low)._value)
				{
					return NpcRandomTalkDb.Get((int)ENpcTalkType.Follower_hunger_low).Scenario.RandCase();
				}
				else
				{
					return -1;
				}

			}
			//	break;
			case AttribType.Hp:
			{
				float maxHp = peskentity.GetAttribute(AttribType.HpMax);
				if(curvalue <= maxHp * NpcRandomTalkDb.Get((int)ENpcTalkType.Follower_health_medium)._value
				   && curvalue > maxHp * NpcRandomTalkDb.Get((int)ENpcTalkType.Follower_health_low)._value)
				{
					return NpcRandomTalkDb.Get((int)ENpcTalkType.Follower_health_medium).Scenario.RandCase();
				}else if(curvalue <= maxHp * NpcRandomTalkDb.Get((int)ENpcTalkType.Follower_health_low)._value)
				{
					return NpcRandomTalkDb.Get((int)ENpcTalkType.Follower_health_low).Scenario.RandCase();
				}else
				{
					return -1;
				}
			}
			//	break;
			case AttribType.Comfort:
			{
				float maxcomfort = peskentity.GetAttribute(AttribType.ComfortMax);
				if(curvalue <= maxcomfort * NpcRandomTalkDb.Get((int)ENpcTalkType.Follower_comfort_common)._value
				   && curvalue > maxcomfort * NpcRandomTalkDb.Get((int)ENpcTalkType.Follower_comfort_medium)._value)
				{
					return  NpcRandomTalkDb.Get((int)ENpcTalkType.Follower_comfort_common).Scenario.RandCase();
				}else if(curvalue <= maxcomfort * NpcRandomTalkDb.Get((int)ENpcTalkType.Follower_comfort_medium)._value
				   && curvalue > maxcomfort * NpcRandomTalkDb.Get((int)ENpcTalkType.Follower_comfort_low)._value
				   )
				{
					return   NpcRandomTalkDb.Get((int)ENpcTalkType.Follower_comfort_medium).Scenario.RandCase();
				}else if(curvalue <= maxcomfort * NpcRandomTalkDb.Get((int)ENpcTalkType.Follower_comfort_low)._value)
				{
					return NpcRandomTalkDb.Get((int)ENpcTalkType.Follower_comfort_low).Scenario.RandCase();
				}
				else
				{
					return -1;
				}
			}
			//	break;
		
			default:
				break;
			}
			return -1;
		}

		private static ETalkLevel SwichHealthLevel(float attr,PeEntity entity)
		{
			Dictionary<ETalkLevel,Item> Items = GetHealthCase();
			if(attr <= Items[ETalkLevel.Medium]._value * entity.GetAttribute(AttribType.HpMax) && attr > Items[ETalkLevel.Low]._value * entity.GetAttribute(AttribType.HpMax))
			{
				return ETalkLevel.Medium;
			}else if(attr <= Items[ETalkLevel.Low] ._value * entity.GetAttribute(AttribType.HpMax))
			{
				return ETalkLevel.Low;
			}
			return ETalkLevel.Max;
		}

		private static ETalkLevel SwichHungerLevel(float attr,PeEntity entity)
		{
			Dictionary<ETalkLevel,Item> Items = GetHungerCase();
			if(attr <= Items[ETalkLevel.Medium]._value * entity.GetAttribute(AttribType.HungerMax) && attr > Items[ETalkLevel.Low]._value * entity.GetAttribute(AttribType.HungerMax))
			{
				return ETalkLevel.Medium;
			}else if(attr <= Items[ETalkLevel.Low] ._value * entity.GetAttribute(AttribType.HungerMax))
			{
				return ETalkLevel.Low;
			}
			return ETalkLevel.Max;
		}

		private static ETalkLevel SwichComfortLevel(float attr,PeEntity entity)
		{
			Dictionary<ETalkLevel,Item> Items = GetComfortCase();
			if(attr <= Items[ETalkLevel.Common]._value * entity.GetAttribute(AttribType.ComfortMax) && attr > Items[ETalkLevel.Medium]._value * entity.GetAttribute(AttribType.ComfortMax))
			{
				return ETalkLevel.Common;
			}else if(attr <= Items[ETalkLevel.Medium]._value * entity.GetAttribute(AttribType.ComfortMax) && attr > Items[ETalkLevel.Low]._value * entity.GetAttribute(AttribType.ComfortMax))
			{
				return ETalkLevel.Medium;
			}else if(attr <= Items[ETalkLevel.Low] ._value * entity.GetAttribute(AttribType.ComfortMax))
			{
				return ETalkLevel.Low;
			}
			return ETalkLevel.Max;
		}


		private static Dictionary<ETalkLevel,Item> GetHungerCase()
		{
			Dictionary<ETalkLevel,Item> items = new Dictionary<ETalkLevel, Item>();
			items[ETalkLevel.Medium] = NpcRandomTalkDb.Get((int)ENpcTalkType.Follower_hunger_medium);
			items[ETalkLevel.Low] = NpcRandomTalkDb.Get((int)ENpcTalkType.Follower_hunger_low);
			return items;
		}

		private static Dictionary<ETalkLevel,Item> GetPlyerHungerCase()
		{
			Dictionary<ETalkLevel,Item> items = new Dictionary<ETalkLevel, Item>();
			items[ETalkLevel.Medium] = NpcRandomTalkDb.Get((int)ENpcTalkType.MainPlayer_hunger_medium);
			items[ETalkLevel.Low] = NpcRandomTalkDb.Get((int)ENpcTalkType.MainPlayer_hunger_low);
			return items;
		}

		private static Dictionary<ETalkLevel,Item>  GetHealthCase()
		{
			Dictionary<ETalkLevel,Item> items = new Dictionary<ETalkLevel, Item>();
			items[ETalkLevel.Medium] = NpcRandomTalkDb.Get((int)ENpcTalkType.Follower_health_medium);
			items[ETalkLevel.Low] = NpcRandomTalkDb.Get((int)ENpcTalkType.Follower_health_low);
			return items;
		}

		private static Dictionary<ETalkLevel,Item>  GetPlyerHealthCase()
		{
			Dictionary<ETalkLevel,Item> items = new Dictionary<ETalkLevel, Item>();
			items[ETalkLevel.Medium] = NpcRandomTalkDb.Get((int)ENpcTalkType.MainPlayer_health_medium);
			items[ETalkLevel.Low] = NpcRandomTalkDb.Get((int)ENpcTalkType.MainPlayer_health_low);
			return items;
		}

		private static Dictionary<ETalkLevel,Item>  GetComfortCase()
		{
			Dictionary<ETalkLevel,Item> items = new Dictionary<ETalkLevel, Item>();
			items[ETalkLevel.Common] = NpcRandomTalkDb.Get((int)ENpcTalkType.Follower_comfort_common);
			items[ETalkLevel.Medium] = NpcRandomTalkDb.Get((int)ENpcTalkType.Follower_comfort_medium);
			items[ETalkLevel.Low] = NpcRandomTalkDb.Get((int)ENpcTalkType.Follower_comfort_low);
			return items;
		}

		private static Dictionary<ETalkLevel,Item>  GetPlyerComfortCase()
		{
			Dictionary<ETalkLevel,Item> items = new Dictionary<ETalkLevel, Item>();
			items[ETalkLevel.Common] = NpcRandomTalkDb.Get((int)ENpcTalkType.MainPlayer_comfort_common);
			items[ETalkLevel.Medium] = NpcRandomTalkDb.Get((int)ENpcTalkType.MainPlayer_comfort_medium);
			items[ETalkLevel.Low] = NpcRandomTalkDb.Get((int)ENpcTalkType.MainPlayer_comfort_low);
			return items;
		}
	}

	public class NpaTalkAgent
	{
		NpcCmpt m_npcCmpt;
		//EntityInfoCmpt m_entityInfo;
		AgentInfo[] _msgs;
		List<AgentInfo> _romveCases;
		public class AgentInfo
		{
			public ENpcTalkType _type;
			public ENpcSpeakType _spType;
			public ETalkLevel _level;
			public bool _canLoop;
			public float _startTime;
			public float _loopTime;
			public bool _hasSend;
			public AgentInfo(ENpcTalkType type,ENpcSpeakType spType,bool canLoop = false )
			{
				_type = type;
				_spType =spType;
				_startTime = Time.time;
				_canLoop = canLoop;
				_loopTime = NpcRandomTalkDb.Get(type)._interval;
			}
		}

		public NpaTalkAgent(PeEntity entity)
		{
			_msgs = new AgentInfo[(int)ENpcTalkType.Max];
			_romveCases = new List<AgentInfo>();
			m_npcCmpt = entity.GetComponent<NpcCmpt>();
			//m_entityInfo = entity.GetComponent<EntityInfoCmpt>();
		}

		public void AddAgentInfo(AgentInfo info)
		{
			if(null == _msgs[(int)info._type])
				_msgs[(int)info._type] = info;
		}

		public void AddAgentInfo(ENpcTalkType type,ENpcSpeakType spType,bool canLoop = false)
		{
			if(_msgs == null)
				_msgs = new AgentInfo[(int)ENpcTalkType.Max];

			AddAgentInfo (new AgentInfo(type,spType,canLoop));
		}

		public bool RemoveAgentInfo(ENpcTalkType type)
		{
			if(_msgs == null || null == _msgs[(int)type])
				return false;

			_msgs [(int)type] = null;
			return  true;

		}

		bool CampeareTime(AgentInfo keyInfo,float NowTime)
		{
			return NowTime - keyInfo._startTime >= keyInfo._loopTime;
		}

		public void RunAttrAgent(PeEntity peEntity)
		{
			//npc self
			List<NpcRandomTalkDb.Item> Items = NpcRandomTalkDb.GetTalkItems(peEntity);
			for (int i = 0; i < Items.Count; i++)
			{
				if (Items[i] == null || Items[i].Type == AttribType.Max || Items[i].Level == ETalkLevel.Max)
		             continue;

				if(!NpcEatDb.CanEatByAttr(peEntity,Items[i].Type,Items[i].TypeMax,false))
                   AddAgentInfo(new AgentInfo(Items[i].TalkType,ENpcSpeakType.TopHead,true));
			}


			//ask for Player
//			PeEntity palyer = null;
//			//int playerID = (int)peEntity.GetAttribute (AttribType.DefaultPlayerID);
//			if (GameConfig.IsMultiClient)
//			{
//				//get main palyer peEntity
//			}
//			else
//			{
//				if (PeCreature.Instance != null)
//				{
//					palyer = PeCreature.Instance.mainPlayer;
//				}
//			}

//			if(palyer != null)
//			{
//				Items = NpcRandomTalkDb.GetPlyerTalkItems(palyer);
//				for (int i = 0; i < Items.Count; i++)
//				{
//					if (Items[i] == null || Items[i].Type == AttribType.Max || Items[i].Level == ETalkLevel.Max)
//						continue;
//					
//					AddAgentInfo(new AgentInfo(Items[i].TalkType,ENpcSpeakType.TopHead,true));
//				}
//			}
//
//			NpcRandomTalkDb.Item item = NpcRandomTalkDb.Get(ENpcTalkType.Day_to_day);
//			AddAgentInfo(new AgentInfo(item.TalkType,ENpcSpeakType.TopHead,true));

			RunAgent();
		}

		public  void RunAgent()
		{
			for (int i = 0; i < (int)ENpcTalkType.Max; i++) {
				if(null != _msgs[i] && !_msgs[i]._hasSend){
					m_npcCmpt.SendTalkMsg((int)_msgs[i]._type,_msgs[i]._loopTime,_msgs[i]._spType);
					_msgs[i]._hasSend = true;
					_romveCases.Add(_msgs[i]);
				}
			}

			int lenth = _romveCases.Count;
			if(lenth > 0)
			{
				for(int i=0;i<lenth;i++)
				{
					if(!_romveCases[i]._canLoop)
					{
						RemoveAgentInfo(_romveCases[i]._type);
						_romveCases.Remove(_romveCases[i]);
						return ;
					}

					if(_romveCases[i]._canLoop && CampeareTime(_romveCases[i],Time.time))
					{
						RemoveAgentInfo(_romveCases[i]._type);
						_romveCases.Remove(_romveCases[i]);
						return ;
					}
				}
			}

		}

	}

}
