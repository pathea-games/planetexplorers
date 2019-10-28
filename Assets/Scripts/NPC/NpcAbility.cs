using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SkillSystem;

using System;
namespace Pathea
{

    public class NpcAbility
    {
        public int id;
        public string icon;
        public int desc;
        public int skillId;
        public int buffId;
		public float Percent;
		public float learnTime;
		public List<int> ProtoIds =null;
        ItemAsset.ItemProto.Bundle itemBundle;
		public List<List<string>> Cond;
        public List<ItemAsset.MaterialItem> GetItem(float factor)
        {
            factor = Mathf.Clamp01(factor);

            if (itemBundle == null)
            {
                return null;
            }

            List<ItemAsset.MaterialItem> items = itemBundle.GetItems();
            if (null == items)
            {
                return null;
            }

            foreach(ItemAsset.MaterialItem item in items)
            {
                item.count = (int)(item.count * factor);
            }
            return items;
        }

		public bool Parse(string line)
		{
			if (string.IsNullOrEmpty(line))
			{
				return false;
			}		
			string[] args = line.Split(';');
			
			for (int i = 0; i < args.Length; i++)
			{
				if(Cond == null)
					Cond = new List<List<string>>();
				
				string[] args1 = args[i].Split(',');
				if(Cond.Count <= i)
					Cond.Add( new List<string>());
				Cond[i].AddRange(args1);			
			}		
			return true;
		}	

		public bool Parse_value(string line)
		{
			if (string.IsNullOrEmpty(line))
			{
				return false;
			}
			string[] args = line.Split(',');
			float f_value;
			for(int i=0;i<args.Length;i++)
			{
				f_value = Convert.ToSingle(args[i]);
				if(f_value <1)
				{
					Percent = f_value;
				}
				if(f_value >1)
				{
					if(ProtoIds == null)
						ProtoIds = new List<int>();

					ProtoIds.Add((int)f_value);
				}
			}
			return true;


		}

		public bool Isskill()
		{
			return skillId !=0;
		}
		public bool IsBuff()
		{
			return buffId != 0;
		}

		public bool IsTalent()
		{
			if((!Isskill()) &&(!IsBuff()) &&(!IsGetItem()))
				return true;
			else
				return false;
		}

		public bool IsGetItem()
		{
			return (GetItem(1.0f) != null);
		}

		public bool RefreshBuff(SkEntity target)
		{
			if(target == null)
				return false;
			
			//remove old buff
			SkEntity.UnmountBuff(target,buffId);
			//add buff
			SkEntity.MountBuff(target,buffId,new List<int>(),new List<float>()); 

			return true;
			
		}

		AblityType m_type;
		public AblityType Type
		{
			get{return m_type;}
		}
		public int type
		{set{m_type = (AblityType)value;}}

		int m_level;
		public SkillLevel Level
		{
			get{return (SkillLevel)m_level;}
		}
		public int level
		{
			set{m_level = value;}
			get{return m_level;}
		}
	
		float m_SkillRange;
		public float SkillRange
		{
			get{return m_SkillRange;}
		}

		float m_SkillPerCent =0;
		public float SkillPerCent
		{
			get{return m_SkillPerCent;}
		}

		float m_Correctrate = 0;
		public float Correctrate
		{
			get{return m_Correctrate;}
		}

		public bool CalculateCondtion()
		{
			foreach(List<string> iter in Cond)
			{
				if(iter != null)
				{
					if(iter.Count > 1)
					{
						if(iter[0] == "range")
						{
							m_SkillRange = Convert.ToInt32(iter[1]);
						}
						if(iter[0] == "hppct")
						{
							m_SkillPerCent = Convert.ToSingle(iter[1]);
						}
						if(iter[0] == "HIT")
						{
							m_Correctrate = Convert.ToSingle(iter[1]);
						}
						
					}
				}
			}		
			return true;
		}


        public class Mgr : MonoLikeSingleton<Mgr>
        {
			//public Dictionary<int,NpcAbility> Npcabilitys = null;//new Dictionary<int, NpcAbility>();
			public List<NpcAbility> mList = null;
            protected override void OnInit()
            {
                base.OnInit();
                Load();
            }

			public override void OnDestroy()
            {
                base.OnDestroy();
				mList = null;
            }

            void Load()
            {
                mList = new List<NpcAbility>();

                Mono.Data.SqliteClient.SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("npcSkill");

                while (reader.Read())
                {
                    NpcAbility ability = new NpcAbility();
                    ability.id = System.Convert.ToInt32(reader.GetString(reader.GetOrdinal("id")));
                    ability.icon = reader.GetString(reader.GetOrdinal("icon"));
                    ability.desc = System.Convert.ToInt32(reader.GetString(reader.GetOrdinal("desc")));
                    ability.skillId = System.Convert.ToInt32(reader.GetString(reader.GetOrdinal("skill")));
                    ability.buffId = System.Convert.ToInt32(reader.GetString(reader.GetOrdinal("buff")));
                    ability.itemBundle = ItemAsset.ItemProto.Bundle.Load(reader.GetString(reader.GetOrdinal("getItem")));
					ability.level = System.Convert.ToInt32(reader.GetString(reader.GetOrdinal("level")));
					ability.type = System.Convert.ToInt32(reader.GetString(reader.GetOrdinal("type")));
					ability.learnTime = System.Convert.ToInt32(reader.GetString(reader.GetOrdinal("time")));
					string cond = reader.GetString(reader.GetOrdinal("cond"));
					ability.Parse(cond);

					string value = reader.GetString(reader.GetOrdinal("value"));
					ability.Parse_value(value);

					if(mList == null)
						mList = new List<NpcAbility>();
					mList.Add(ability);
                }
            }

        }
    }

	public enum SkillLevel
	{
		Primary,
		Intermediate,
		Highgrade,
		Expertlevel,
		none,
	}

	public enum AblityType
	{
		Sword = 1,  //剑术
		Arrow = 2,  //弓箭
		Gun = 3,    //枪
		Shield = 4, //盾牌
		Cutting = 5, //伐木
		HerbsPicking = 6, //采药
		Oremining = 7,   //采矿
		Hunting = 8,     //狩猎
		EmergencyTrain = 9, //急救
		Attack = 10,        //攻击力
		Defense = 11,       //防御
		Repair = 12,        //修理
		Reinforce = 13,     //强化
		Disassembly = 14,    //拆卸
		Arts = 15,          //艺术
		Herb_Cook =16,        //烹饪
		Technology =17,     //科技
		Farming = 18,       //耕作
		Harvest = 19,       //收割
		LifeUp = 20,        //生命力强化
		AtrackUp = 21,      //攻击强化
		DefenseUp = 22,     //防御强化
		Bind_up = 23,      //包扎
		Meat_Cook = 24,    //肉类烹饪
		Explore = 25,      //探索
		Diagnose = 26,      //诊断
		Medical = 27,       //医疗
		Nurse = 28,        //看护
		Max
	}

	public class AblityInfo
	{
		public bool IsSkill;
		public bool IsBuff;
		public bool IsGetItem;
		public bool IsTalent;
		public float _Percent;
		public float _Skill_R;
		public float _Skill_Per;
		public float _Correctrate;
		public string _icon;
		public int _abityid;
		public int SkillId;
		public int BuffId;
		public int DecsId;
		public int _level;
		public AblityType _type;
		public List<int> _ProtoIds;
		public List<ItemAsset.MaterialItem> _Items;

		public AblityInfo()
		{
			_ProtoIds = new List<int>();
			_Items = new List<ItemAsset.MaterialItem>();
		}
	}

	public class Ablities : List<int>
	{
        const int VERSION_0000 = 0;
        const int CURRENT_VERSION = VERSION_0000;
        private bool _bDirty = false;

		public new bool Remove(int item)
		{
			_bDirty = true;
		    return  base.Remove(item);
		}

		public new void AddRange (IEnumerable<int> collection)
		{
			base.AddRange (collection);
			_bDirty = true;
		}
		public new void RemoveAt (int idx)
		{
			base.RemoveAt (idx);
			_bDirty = true;
		}

		public new void RemoveAll(Predicate<int> match)
		{
			base.RemoveAll(match);
			_bDirty = true;
		}

		public new void Add(int idx)
		{
			base.Add(idx);
			_bDirty = true;
		}


		public bool GetDrity()
		{
			return _bDirty;
		}

		public void SetDirty(bool bDirty)
		{
			_bDirty = bDirty;
		}

		public void ClearDrity()
		{
			_bDirty = false;
		}

		public Ablities(int capacity) : base(capacity)
		{
		}

		public Ablities() : base()
		{
			
		}

		public Ablities(IEnumerable<int> collection) :base(collection)
		{

		}

        public byte[] Export()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter w = new BinaryWriter(ms))
                {
                    w.Write(CURRENT_VERSION);
                    w.Write(Count);

                    for (int i = 0; i < Count; i++)
                    {
                        w.Write(this[i]); 
                    }
                }

                return ms.ToArray();
            }
        }

        public void Import(byte[] buffer)
        {
            using (MemoryStream ms = new MemoryStream(buffer, false))
            {
                using (BinaryReader r = new BinaryReader(ms))
                {
                    int version = r.ReadInt32();
                    if (version > CURRENT_VERSION)
                    {
                        Debug.LogError("error version:" + version);
                    }
                    int length = r.ReadInt32();
                    for (int i = 0; i < length; i++)
                    {
                        int ablityid = r.ReadInt32();
                        Add(ablityid);
                    }
                }
            }
        }

    }

	public class NpcAblitycmpt 
	{
	    Dictionary <int,AblityInfo> AblityInfos = new Dictionary<int, AblityInfo>();
		public NpcAblitycmpt(SkEntity target)
		{
			_SkEnity = target;
			m_AblityId = new Ablities(5);;
		}

        static List<NpcAbility> mNpcSkills { get { return NpcAbility.Mgr.Instance.mList; } }
		SkEntity _SkEnity;
	  
		Ablities m_AblityId;
		public Ablities AblityId { get  {return m_AblityId;} }

		public void SetAblitiyIDs(Ablities abl)
		{
			m_AblityId.Clear();
			m_AblityId.AddRange(abl);
			ReflashBuffbyIds(m_AblityId,_SkEnity);
			UpdateCurablity();
			GetAblityInfos();
		}
	

		List<NpcAbility> m_curNpcAblitys;
		public List<NpcAbility> CurNpcAblitys
		{
			get
			{
				UpdateCurablity();
				return m_curNpcAblitys;
			}
		}

		#region Ablity_fun
		private void UpdateCurablity()
		{
			if(m_AblityId != null)
			{
				if(m_curNpcAblitys == null)
					m_curNpcAblitys = new List<NpcAbility>();
				m_curNpcAblitys.Clear();
				m_curNpcAblitys = FindAblitysById(m_AblityId);
			}
			return ;
		}
		private float GetValueById(int Id)
		{
			NpcAbility ablliy =FindNpcAblityById(Id);
			if(ablliy == null)
				return 0;
			
			return ablliy.Percent;
		}

		private List<int> GetProtoIDs(int Id)
		{
			NpcAbility ablliy =FindNpcAblityById(Id);
			if(ablliy == null)
				return null;

			return ablliy.ProtoIds;
		}

		public NpcAbility GetAblity(AblityType type,SkillLevel level)
		{
			foreach(NpcAbility ablity in mNpcSkills)
			{
				if((ablity.Type == type) &&(ablity.Level == level))
				{
					return ablity;
				}
			}
			return null;
		}

		void ClearAbliy()
		{
			if(m_AblityId != null)
			{
				for(int i=0;i<m_AblityId.Count;i++)
				{
					m_AblityId[i]=0;
				}
			}

		}
		private void  GetAblityInfos()
		{
			AblityInfos.Clear();
			foreach(NpcAbility Ablity in m_curNpcAblitys)
			{
				if(Ablity == null)
					return ;
				if(Ablity.IsTalent())
				{
					AblityInfo info = new AblityInfo();
					Ablity.CalculateCondtion();
					info._Percent = GetValueById(Ablity.id);
					info._ProtoIds = GetProtoIDs(Ablity.id);
					info.IsTalent = true;
					info._type = Ablity.Type;	
					info._Correctrate = Ablity.Correctrate;
					info.DecsId = Ablity.desc;
					info._icon = Ablity.icon;
					info._level = Ablity.level;
					AblityInfos[Ablity.id] = info;
				}
				else if(Ablity.Isskill())
				{
					AblityInfo info = new AblityInfo();
					info.IsSkill = true;
					Ablity.CalculateCondtion();
					info.SkillId = Ablity.skillId;
					info._Skill_R = Ablity.SkillRange;
					info._Percent = Ablity.SkillPerCent;
					info._type = Ablity.Type;
					info.DecsId = Ablity.desc;
					info._icon = Ablity.icon;
					info._level = Ablity.level;
					AblityInfos[Ablity.id] = info;
				}
				else if(Ablity.IsBuff())
				{					
					AblityInfo info = new AblityInfo();
					info.IsBuff = true;
					info.BuffId = Ablity.buffId;
					info._type = Ablity.Type;
					info.DecsId = Ablity.desc;
					info._icon = Ablity.icon;
					info._level = Ablity.level;
					AblityInfos[Ablity.id] = info;
				}
				if(Ablity.IsGetItem())
				{					
					AblityInfo info = new AblityInfo();
					info.IsGetItem = true;
					info._Items = Ablity.GetItem(1.0f);
					info._type = Ablity.Type;
					info.DecsId = Ablity.desc;
					info._level = Ablity.level;
					AblityInfos[Ablity.id] = info;
				}

			}
			return ;
		}
		#endregion

		#region Interface


		#region CurAblity
		public bool Cur_ContainsId( int id)
		{
			for(int i=0;i<m_AblityId.Count;i++)
			{
				if(m_AblityId[i] == id)
					return true;
			}
			return false;
		}

		public bool Cur_ContainsType(AblityType type)
		{
			if(AblityInfos == null)
				return false;

			foreach(int key in AblityInfos.Keys)
			{
				if(AblityInfos[key]._type == type)
					return true;
			}
			return false;
		}

		public bool HasCollectSkill()
		{
			for(int i=5;i<9;i++)
			{
				if(Cur_ContainsType((AblityType)i))
					return true;
			}
			return false;
		}

		public AblityInfo Cur_GetAblityInfoById(int Id)
		{
			return AblityInfos.ContainsKey(Id) ? AblityInfos[Id] : null;
		}

		public AblityInfo Cur_GetAblityInfoByType(AblityType type)
		{
			if(AblityInfos == null)
				return null;

			foreach(int key in AblityInfos.Keys)
			{
				if(AblityInfos[key]._type == type)
					return AblityInfos[key];
			}
			return null;
		}

		public List<AblityInfo> Cur_GetAblityByType(AblityType type)
		{
			List<AblityInfo> infos = new List<AblityInfo>();
			foreach(int key in AblityInfos.Keys)
			{
				if(AblityInfos[key]._type == type)
					infos.Add(AblityInfos[key]);
			}
			return infos;
		}

		public bool ReflashBuffById(int Id,SkEntity target)
		{
			NpcAbility ablity= CanAddBuff(Id);
			if(ablity == null)
				return false;
			
			if(_SkEnity == null)
				return false;
			
			return ablity.RefreshBuff(target);
		}
		
		public  void ReflashBuffbyIds(Ablities ids,SkEntity target)
		{
			if((target == null) ||(ids ==null))
				return ;
			
			for(int i=0;i<ids.Count;i++)
			{
				ReflashBuffById(ids[i],target);
			}
			return ;
		}

		//Talent fun
		public float GetTalentPercent(AblityType type)
		{
			if(AblityInfos.Count == 0)
				return 0.0f;
			
			foreach(int key in AblityInfos.Keys)
			{
				if(AblityInfos[key]._type == type) 
					return AblityInfos[key]._Percent;
			}
			return 0.0f;
		}
		
		public float GetCorrectRate(AblityType type)
		{
			if(AblityInfos.Count == 0)
				return 0.0f;
			foreach(int key in AblityInfos.Keys)
			{
				if(AblityInfos[key]._type == type) 
					return AblityInfos[key]._Correctrate;
			}
			return 0.0f;
			
		}

		//itemskill fun
		public RandomItemObj TryGetItemskill(Vector3 pos,float percent = 1.0f)
		{
			System.Random rand = new System.Random();
			List<List<ItemAsset.MaterialItem>> ItemsMasters = new List<List<ItemAsset.MaterialItem>>();
			//List<ItemAsset.MaterialItem> ItemsMaster;
			if(m_curNpcAblitys == null)
				return null;
			
			foreach(NpcAbility Ablity in m_curNpcAblitys)
			{
				if(Ablity.IsGetItem())
				{
					//ItemsMaster = Ablity.GetItem(percent);
					ItemsMasters.Add(Ablity.GetItem(percent));
					//break;
				}
			}
			if(ItemsMasters == null)
				return null;
			
			List<int> _Items = new List<int>();

			for(int i=0;i<ItemsMasters.Count;i++)
			{
				if(ItemsMasters[i]!= null && ItemsMasters[i].Count >0)
				{
					foreach(ItemAsset.MaterialItem masrerItem in ItemsMasters[i])
					{
						if(masrerItem.count >0)
						{
							_Items.Add(masrerItem.protoId);
							_Items.Add(masrerItem.count);
						}
					}
				}

			}

			if(_Items == null || _Items.Count <=0)
				return null;
			
			return new RandomItemObj(pos + new Vector3((float)rand.NextDouble()*0.15f, 0, (float)rand.NextDouble()*0.15f), _Items.ToArray());
			
		}

		public List<int> GetProtoIds(AblityType type)
		{
			if(AblityInfos.Count == 0)
				return null;
			foreach(int key in AblityInfos.Keys)
			{
				if(AblityInfos[key]._type == type) 
					return AblityInfos[key]._ProtoIds;
			}
			return null;
		}

		//coast skill fun
		public List<int> GetSkillIDs()
		{
			List<int> SkillIds = new List<int>();
			foreach(int key in AblityInfos.Keys)
			{
				if(AblityInfos[key].IsSkill)
					SkillIds.Add(AblityInfos[key].SkillId);
			}
			return SkillIds;
		}


		public float GetCmptSkillRange(int SkillId)
		{
			foreach(int key in AblityInfos.Keys)
			{
				if(AblityInfos[key].IsSkill && AblityInfos[key].SkillId == SkillId)
					return AblityInfos[key]._Skill_R;
			}
			return 0.0f;
		}

		public AblityType GetSkillType(int skillid)
		{
			foreach(int key in AblityInfos.Keys)
			{
				if(AblityInfos[key].IsSkill && AblityInfos[key].SkillId == skillid)
					return AblityInfos[key]._type;
			}
			return AblityType.Max;
		}
		
		public float GetChangeHpPer(int SkillId)
		{
			foreach(int key in AblityInfos.Keys)
			{
				if(AblityInfos[key].IsSkill && AblityInfos[key].SkillId == SkillId)
					return AblityInfos[key]._Percent;
			}
			return 0.0f;
		}

		//abliyLearn fun

		public int GetCanLearnId(int learnId)
		{
			NpcAbility ability = CanLearnAblity(learnId);
			return ability != null ? ability.id : 0;
		}

		private NpcAbility CanLearnAblity(int learnId)
		{

			NpcAbility learn_ablity =  FindNpcAblityById(learnId);
			if(learn_ablity == null || learn_ablity.Type == AblityType.Max)
				return null;
		    
			if(Cur_ContainsType(learn_ablity.Type))
			{
				AblityInfo info = Cur_GetAblityInfoByType(learn_ablity.Type);
				if(info._level >= 4)
					return null;

				if(learn_ablity.level == info._level)
					return null;

				if(learn_ablity.level > info._level)
					return FindNpcAblity(learn_ablity.Type,info._level+1);

			}
			return FindNpcAblity(learn_ablity.Type,1);
		}

		#endregion


		#region static ablitycmpt

		private static NpcAbility CanAddBuff(int Id)
		{
			foreach(NpcAbility ablity in mNpcSkills)
			{
				if(ablity.id == Id)
				{
					if(ablity.IsBuff())
						return ablity;
					else
						return null;
				}
			}
			return null;
		}

		public  static NpcAbility FindNpcAblityById(int Id)
		{
			foreach(NpcAbility ablity in mNpcSkills)
			{
				if(ablity.id == Id)
					return ablity;
			}
			
			return null;
		}

		public  static NpcAbility FindNpcAblity(AblityType type,int level)
		{
			foreach(NpcAbility ablity in mNpcSkills)
			{
				if(ablity.Type == type && ablity.level == level)
					return ablity;
			}
			return null;
		}

		public static List<NpcAbility> FindAblitysById(Ablities Ids)
		{
			if(Ids == null)
				return null;
			
			List<NpcAbility> ablitys = new List<NpcAbility>();
			for(int i=0; i<Ids.Count;i++)
			{
				ablitys.Add(FindNpcAblityById(Ids[i]));
			}
			return ablitys;
		}


		public static NpcAbility FindNpcAblityBySkillId(int SkillId)
		{
			foreach(NpcAbility ablity in mNpcSkills)
			{
				if(ablity.skillId == SkillId)
					return ablity;
			}
			return null;
		}

		public static List<NpcAbility> GetAbilityByType(AblityType type)
		{
			List<NpcAbility> Ablitys = new List<NpcAbility>();
			foreach(NpcAbility ablity in mNpcSkills)
			{
				if(ablity.Type == type)
					Ablitys.Add(ablity);
			}
			return Ablitys;
		}

		private static AblityType GetAblityType(int Id)
		{
			foreach(NpcAbility ablity in mNpcSkills)
			{
				if(ablity.id == Id)
					return ablity.Type;
			}
		
			return AblityType.Max;
		}

		private static List<int> GetTheSameTypeAbliy(int id)
		{
			List<int> Ids= new List<int>();
			foreach(NpcAbility ablity in mNpcSkills)
			{
				if(ablity.id == id)
					Ids.Add(id);
			}
			return Ids;
		}

		public static List<int> GetCoverAbilityId(int cur_ablityId)
		{
			List<int> coverIds = new List<int>();
			
			List<NpcAbility> abilitys = CoverAblity(cur_ablityId);
			if(abilitys == null || abilitys.Count <=0)
				return coverIds;
			
			for(int i=0;i<abilitys.Count;i++)
			{
				coverIds.Add(abilitys[i].id);
			}
			return coverIds;
		}

		public static float GetLearnTime(int abilityid)
		{
			NpcAbility ablity = FindNpcAblityById(abilityid);
			if(ablity == null)
				return 0.0f;

			return ablity.learnTime;
		}

		private static List<NpcAbility> CoverAblity(int abliyId)
		{
			List<NpcAbility> covers = new List<NpcAbility>();
			
			NpcAbility ablity = FindNpcAblityById(abliyId);
			if(ablity == null)
				return covers;
			
			List<NpcAbility> all = GetAbilityByType(ablity.Type);
			if(all == null)
				return covers;
			
			foreach(NpcAbility info in all)
			{
				if(ablity.level > info.level)
					covers.Add(info);
			}
			return covers;
		}

		private static SkillLevel GetSkilllevel(int Id)
		{
			NpcAbility ablity = FindNpcAblityById(Id);
			if(ablity == null)
				return SkillLevel.none;
			
			return ablity.Level;
		}

        public static Ablities CompareSkillType(List<int> _ablityIds)
        {
			Ablities ablityIds = new Ablities(_ablityIds.Count);
			ablityIds.AddRange(_ablityIds);

            if (ablityIds.Count <= 1)
                return ablityIds;

            if (ablityIds.Count == 2)
            {
                int Id0 = ablityIds[0];
                int Id1 = ablityIds[1];
                NpcAbility ablity1 = FindNpcAblityById(Id0);
				if(ablity1 == null)
				{
					Debug.LogError("Random Ability error. no Ability!!!" +"   " + Id0);
					return ablityIds;
				}
                NpcAbility ablity2 = FindNpcAblityById(Id1);
				if(ablity1 == null)
				{
					Debug.LogError("Random Ability error. no Ability!!!" +"   " + Id1);
					return ablityIds;
				}

                if (ablity1.Type == ablity2.Type)
                {
                    if (ablity1.level - ablity2.level > 0)
                    {
                        ablityIds.Remove(Id1);
                    }
                    else
                    {
                        ablityIds.Remove(Id0);
                    }
                }
            }

            return ablityIds;
        }
		#endregion

		#endregion
	}
}