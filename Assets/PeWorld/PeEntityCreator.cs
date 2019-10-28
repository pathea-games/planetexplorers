//#define USE_NPC_PREFAB
#if UNITY_EDITOR
#define USE_PREFAB_DATA
#endif
using UnityEngine;

using Pathea.PeEntityExtTrans;
using Pathea.PeEntityExt;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using DbField = PETools.DbReader.DbFieldAttribute;

public class DbAttr
{
	const int c_cntAttribType = (int)Pathea.AttribType.Max;
	public float[] attributeArray = new float[c_cntAttribType];
	static readonly int[] c_scalableAttrIndices = new int[]{
		//(int)Pathea.AttribType.WalkSpeed,
		//(int)Pathea.AttribType.RunSpeed,
		(int)Pathea.AttribType.HpMax,
		(int)Pathea.AttribType.Hp,
		(int)Pathea.AttribType.Atk,
		(int)Pathea.AttribType.Def,
	};

    public DbAttr Clone()
    {
        DbAttr attr = new DbAttr();

		Array.Copy(attributeArray, attr.attributeArray, c_cntAttribType);

        return attr;
    }

    public void ReadFromDb(Mono.Data.SqliteClient.SqliteDataReader reader)
    {
		//float[] attributeArray = new float[c_cntAttribType];

		for (int i = (int)Pathea.AttribType.HpMax; i < c_cntAttribType; i++)
        {
            Pathea.AttribType a = (Pathea.AttribType)i;
            string attributeName = a.ToString();

            try
            {
                attributeArray[i] = PETools.Db.GetFloat(reader, attributeName);
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex);
                attributeArray[i] = 0f;
            }
        }

    }

    public Pathea.PESkEntity.Attr[] ToAliveAttr()
    {
		Pathea.PESkEntity.Attr[] attrArray = new Pathea.PESkEntity.Attr[c_cntAttribType];

		for (int i = 0; i < c_cntAttribType; i++)
        {
            attrArray[i] = new Pathea.PESkEntity.Attr() { m_Type = (Pathea.AttribType)i, m_Value = attributeArray[i] };
        }
        return attrArray;
    }
	public Pathea.PESkEntity.Attr[] ToAliveAttrWithScale(float fScale)
	{
		Pathea.PESkEntity.Attr[] attrArray = new Pathea.PESkEntity.Attr[c_cntAttribType];
		
		for (int i = 0; i < c_cntAttribType; i++)
		{
			attrArray[i] = new Pathea.PESkEntity.Attr() { m_Type = (Pathea.AttribType)i, m_Value = attributeArray[i] };
		}
		for (int i = 0; i < c_scalableAttrIndices.Length; i++) {
			int idx = c_scalableAttrIndices[i];
			attrArray[idx].m_Value *= fScale;
		}
		return attrArray;
	}
}

namespace Pathea
{
    public enum EEntityProto
    {
        Player,
        RandomNpc,
        Npc,
        Monster,
        Tower,
        Doodad,
		NpcRobot,
        Max
    }

    public class EntityProto
    {
		public const int IdGrpMask   			= 0x40000000;
		public const int IdBcnLvlTypeMask   	= 0x20000000;
		public const int IdAirborneAllMask 		= 0x0c000000;
		public const int IdAirbornePujaMask 	= 0x08000000;
		public const int IdAirbornePajaMask 	= 0x04000000;
		
		public EEntityProto proto;
        public int protoId;
    }

    public class NpcProtoDb
    {
        public class Item
        {
            [DbField("id")]
            public int id;
			[DbField("sort")]
			public int sort;
            [DbField("default_name")]
            public string name;
            [DbField("default_showname")]
            public string showName;

            [DbField("npc_icon")]
            public string icon;
			[DbField("race")]
			public string race;
            [DbField("npc_bigicon")]
            public string iconBig;
            [DbField("path")]
            public string avatarModelPath;

            [DbField("Bonepath_Editor")]
            public string modelPrefabPath;
            [DbField("Bonepath")]
            public string modelBundlePath;
            [DbField("behavior_path")]
            public string behaveDataPath;
            [DbField("gender", true)]
            public PeSex sex;

			[DbField("NPC_Chat1")]
			public int[] chart1;
			[DbField("NPC_Chat2")]
			public int[] chart2;

            [DbField("VoiceType")]
            public int voiceType;

            [DbField("InitBuffList")]
            public int[] initBuff;

            public DbAttr dbAttr = new DbAttr();

			public int[] InFeildBuff = new int[]{30200053,30200046};
			public int[] RecruitBuff = new int[]{30200049,30200050};

			public string modelAssetPath = String.Empty;
			public UnityEngine.Object modelObj = null;
        }

		static List<Item> sList = new List<Item>(50);
        public static void Load()
        {
            sList.Clear();

            Mono.Data.SqliteClient.SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("PrototypeNPC");
            while (reader.Read())
            {
                Item item = PETools.DbReader.ReadItem<Item>(reader);
                //Item item = new Item();
                //item.id = PETools.Db.GetInt(reader, "id");
                //item.icon = PETools.Db.GetString(reader, "npc_icon");
                //item.iconBig = PETools.Db.GetString(reader, "npc_bigicon");
                //item.modelPath = PETools.Db.GetString(reader, "path");
                //item.behaveDataPath = PETools.Db.GetString(reader, "behavior_path");
                //item.sex = (PeSex)PETools.Db.GetInt(reader, "gender");
                item.dbAttr.ReadFromDb(reader);
                Behave.Runtime.BTResolver.RegisterToCache(item.behaveDataPath);

#if USE_NPC_PREFAB
				if (item.modelPrefabPath.StartsWith("Prefab")){
					item.modelAssetPath = item.modelPrefabPath;
				} else 
#endif
				{
#if USE_PREFAB_DATA
					if (!string.IsNullOrEmpty(item.modelPrefabPath) && item.modelPrefabPath != "0")
						item.modelAssetPath = item.modelPrefabPath;
#else
					if (!string.IsNullOrEmpty (item.modelBundlePath) && item.modelBundlePath != "0")
						item.modelAssetPath = item.modelBundlePath;
#endif	
				}
                sList.Add(item);
            }
        }
		
		public static void Release()
		{
			sList.Clear ();
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
		public static void CachePrefab()
		{
			int n = sList.Count;
			for (int i = 0; i < n; i++) {
				if (sList [i].modelAssetPath.StartsWith("Prefab")) {
					AssetsLoader.Instance.LoadPrefabImm(sList [i].modelAssetPath, true);
				}
			}
		}
    }

    public class DoodadProtoDb
    {
        public class Item
        {
            [DbField("id")]
            public int id;
            [DbField("name")]
            public string name;
			[DbField("LOOT")]
			public int dropItemId;
#if USE_PREFAB_DATA
			[DbField("prefab_path")]
			public string modelPath;
#else
            [DbField("asset_path")]
            public string modelPath;
#endif
			[DbField("radius")]
			public float[] rangeDesc;
			[DbField("ReputationValueID")]
			public int repValId;
			public RadiusBound range;

            public DbAttr dbAttr = new DbAttr();            
        }

		static List<Item> sList = new List<Item>(50);
        public static void Load()
        {
            sList.Clear();

            Mono.Data.SqliteClient.SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("PrototypeDoodad");
            while (reader.Read())
            {
                Item item = PETools.DbReader.ReadItem<Item>(reader);
                
                item.dbAttr.ReadFromDb(reader);

				if(item.rangeDesc == null || Mathf.RoundToInt(item.rangeDesc[0]) != 0){	// now only 0 is valid
					item.range = new RadiusBound(32, 0, 0, 0);
				} else {
					item.range = new RadiusBound(item.rangeDesc[4], item.rangeDesc[1], item.rangeDesc[2], item.rangeDesc[3]);
				}

                sList.Add(item);
            }
        }

        public static void Release()
        {
			sList.Clear ();
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

    public class TowerProtoDb
    {
        public struct TowerEffectData
        {
            public float hpPercent;
            public int effectID;
            public int audioID;
        }

        public class BulletData
        {
            [DbField("needBlock")]
            public bool needBlock;
            [DbField("bulletType")]
            public int bulletType;
            [DbField("bulletID")]
            public int bulletId;
            [DbField("bulletCost")]
            public int bulletCost;
            [DbField("maxBullet")]
            public int bulletMax;
            [DbField("energyCost")]
            public int energyCost;
            [DbField("maxEnergy")]
            public int energyMax;
            [DbField("skillID")]
            public int skillId;
        }

        public class Item
        {
            [DbField("id")]
            public int id;
            [DbField("icon")]
            public string icon;
            [DbField("ENG_name")]
            public string name;
            [DbField("assetbundle_path")]
            public string modelPath;
            [DbField("behave_path")]
            public string behaveDataPath;
            [DbField("identity")]
            public EIdentity eId;
            [DbField("race")]
            public ERace eRace;
            [DbField("HPLossEffect")]
            public string effect;

            public DbAttr dbAttr = new DbAttr();
            public BulletData bulletData;
            public List<TowerEffectData> effects;

            public void InitEffect()
            {
                effects = new List<TowerEffectData>();
                string[] tmpEffects = PETools.PEUtil.ToArrayString(effect, ',');
                for (int i = 0; i < tmpEffects.Length; i++)
                {
                    string[] effData = PETools.PEUtil.ToArrayString(tmpEffects[i], '_');
                    TowerEffectData data;
                    data.hpPercent = System.Convert.ToSingle(effData[0]);
                    data.effectID = System.Convert.ToInt32(effData[1]);
                    data.audioID = System.Convert.ToInt32(effData[2]);
                    effects.Add(data);
                }
            }
        }

		static List<Item> sList = new List<Item>(50);
        public static void Load()
        {
            sList.Clear();

            Mono.Data.SqliteClient.SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("PrototypeTurret");
            while (reader.Read())
            {
                //Item item = new Item();
                //item.id = PETools.Db.GetInt(reader, "id");
                //item.icon = PETools.Db.GetString(reader, "icon");
                //item.name = PETools.Db.GetString(reader, "ENG_name");
				//item.modelPath = PETools.Db.GetString(reader, "assetbundle_path");
                //item.behaveDataPath = PETools.Db.GetString(reader, "behave_path");
                //item.dbAttr.ReadFromDb(reader);
                //item.eId = (EIdentity)Enum.Parse(typeof(EIdentity), PETools.Db.GetString(reader, "identity"));
                //item.eRace = (ERace)Enum.Parse(typeof(ERace), PETools.Db.GetString(reader, "race"));

                //BulletData bulletData = new BulletData();
                //bulletData.needBlock = PETools.Db.GetBool(reader, "needBlock");
                //bulletData.bulletType = PETools.Db.GetInt(reader, "bulletType");
                //bulletData.bulletId = PETools.Db.GetInt(reader, "bulletID");
                //bulletData.bulletCost = PETools.Db.GetInt(reader, "bulletCost");
                //bulletData.bulletMax = PETools.Db.GetInt(reader, "maxBullet");
                //bulletData.energyCost = PETools.Db.GetInt(reader, "energyCost");
                //bulletData.energyMax = PETools.Db.GetInt(reader, "maxEnergy");
                //bulletData.skillId = PETools.Db.GetInt(reader, "skillID");

                //item.bulletData = bulletData;
                Item item = PETools.DbReader.ReadItem<Item>(reader);
                item.bulletData = PETools.DbReader.ReadItem<BulletData>(reader);
                item.dbAttr.ReadFromDb(reader);
                item.InitEffect();
                Behave.Runtime.BTResolver.RegisterToCache(item.behaveDataPath);

                sList.Add(item);
            }
        }

        public static void Release()
        {
            sList.Clear();
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

    public class MonsterRandomDb
    {
        public class Item
        {
            [DbField("color_type")]
            public int player;
            [DbField("color_setting")]
            public Color color;
            [DbField("npc_equipment")]
            public int[] equipments;
            [DbField("npc_weapon")]
            public int[] weapons;

            public Dictionary<string, Material[]> materialDic = new Dictionary<string, Material[]>();

            public void RegisterMaterials(string modelName, Material[] materials)
            {
                if (!materialDic.ContainsKey(modelName))
                {
                    Material[] tmpMaterials = new Material[materials.Length];
                    for (int i = 0; i < tmpMaterials.Length; i++)
                    {
                        tmpMaterials[i] = Material.Instantiate(materials[i]);
                        tmpMaterials[i].SetColor("_SkinColor", color); 
                        tmpMaterials[i].SetFloat("_SkinCoef", 1.0f); 
                    }
                    materialDic.Add(modelName, tmpMaterials);
                }
            }
        }

        static List<Item> sList;
        public static void Load()
        {
            sList = new List<Item>(50);

            Mono.Data.SqliteClient.SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("AdventureCampRelated");

            while (reader.Read())
            {
                Item item = PETools.DbReader.ReadItem<Item>(reader);
                sList.Add(item);
            }
        }

        public static void RegisterMaterials(int player, string modelName, Material[] materials)
        {
            Item item = sList.Find(ret => ret.player == player);
            if (item != null)
            {
                item.RegisterMaterials(modelName, materials);
            }
        }

        public static bool ContainsMaterials(int player, string modelName)
        {
            Item item = sList.Find(ret => ret.player == player);
            return item != null && item.materialDic.ContainsKey(modelName);
        }

        public static Material[] GetMaterials(int player, string modelName)
        {
            Item item = sList.Find(ret => ret.player == player);
            if (item != null && item.materialDic.ContainsKey(modelName))
                return item.materialDic[modelName];
            else
                return null;
        }

        public static int[] GetEquipments(int player)
        {
            Item item = sList.Find(ret => ret.player == player);
            if (item != null)
                return item.equipments;
            else
                return null;
        }

        public static int GetWeapon(int player)
        {
            Item item = sList.Find(ret => ret.player == player);
            if (item != null)
                return item.weapons[UnityEngine.Random.Range(0, item.weapons.Length)];
            else
                return -1;
        }
    }

    public class MonsterProtoDb
    {
        public class Item
        {
            [DbField("id")]
            public int id;
            [DbField("monster_icon")]
            public string icon;
            [DbField("TranslationID")]
            public string nameID;

            //lz-2016.08.18 将名字进行语言处理，
            public string name { 
                get 
                {
                    int id = 0;
                    if(int.TryParse(nameID,out id))
                    {
                        return PELocalization.GetString(id);
                    }
                    return string.Empty;
                }}

			[DbField("Scale")]
			public float[] fScaleMinMax;
			[DbField("SpawnHeight")]
			public float hOffset;
#if USE_PREFAB_DATA
			[DbField("prefab_path")]
			public string modelPath;
#else
            [DbField("assetbundle_path")]
            public string modelPath;
#endif
            [DbField("model_name")]
            public string modelName;
            [DbField("Area")]
			public int[] monsterAreaId;
			[DbField("Canbepush")]
			public bool canBePush;
            [DbField("behave_path")]
            public string behaveDataPath;
            [DbField("EquipID")]
            public int[] initEquip;
            [DbField("identity")]
            public EIdentity eId;
            [DbField("race")]
            public ERace eRace;
            [DbField("isBoss")]
            public bool isBoss;
			[DbField("ReputationValueID")]
			public int repValId;
            [DbField("environment")]
            public Pathea.MovementField movementField;
            [DbField("InjuredLv")]
            public int injuredLevel;
            [DbField("InjuredState")]
            public float injuredState;
            [DbField("EscapeProb")]
            public float escapeProb;
            [DbField("loot")]
            public int dropItemId;
            [DbField("DeathSoundID")]
            public int deathAudioID;
            [DbField("YawpSoundMinDistance")]
            public int idleSoundDis;

            [DbField("InitBuffList")]
            public int[] initBuff;

			[DbField("deathbuff")]
			public string deathBuff;

			[DbField("BeHitSound")]
			public int[] beHitSound;

            [DbField("YawpSoundID")]
            public int[] idleSounds;

            [DbField("AttackType")]
			public int attackType;

            [DbField("Npc_id")]
            public int npcProtoID;

			[DbField("Canberepelled")]
			public int RepulsedType;

            [DbField("AttackNum")]
			string atkDbStr
			{
				set
				{
					AtkDb = AtkNumDb.Load(value);
				}
			}

			public AtkNumDb AtkDb;

            public DbAttr dbAttr = new DbAttr();

			public class AtkNumDb
			{
				public int   mNumber;     //安排攻击的数量
                public int   mChaseNumber;//可追击的最大数量
				public bool  mNeeedEqup;  //0:任何武器都可以 1：仅能使用远程
                public int   mMaxMeleeNum = 4;

				public static AtkNumDb Load(string str)
				{
					AtkNumDb atkDb = new AtkNumDb();
					
					string[] tmplist;
					tmplist = str.Split(',');
					if (tmplist.Length != 3)
					{
						Debug.LogError("load AtkNum error:" + str);
					}
					else
					{
						atkDb.mNumber = Convert.ToInt32(tmplist[0]);
                        atkDb.mChaseNumber = Convert.ToInt32(tmplist[1]);
						atkDb.mNeeedEqup =Convert.ToBoolean(Convert.ToInt32(tmplist[2]));
					}
					return atkDb;
				}
			}
        }

		static List<Item> sList = new List<Item>(50);
        public static void Load()
        {
            sList.Clear();

            Mono.Data.SqliteClient.SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("PrototypeMonster");
            while (reader.Read())
            {
                //Item item = new Item();
                //item.id = PETools.Db.GetInt(reader, "id");
                //item.icon = PETools.Db.GetString(reader, "monster_icon");
                //item.name = PETools.Db.GetString(reader, "ENG_name");
				//item.modelPath = PETools.Db.GetString(reader, "prefab_path");
                //item.behaveDataPath = PETools.Db.GetString(reader, "behave_path");
                //item.initEquip = PETools.Db.GetIntArray(reader, "EquipID");

                //item.eId = (EIdentity)Enum.Parse(typeof(EIdentity), PETools.Db.GetString(reader, "identity"));
                //item.eRace = (ERace)Enum.Parse(typeof(ERace), PETools.Db.GetString(reader, "race"));
                //item.isBoss = PETools.Db.GetBool(reader, "isBoss");

                Item item = PETools.DbReader.ReadItem<Item>(reader);
                item.dbAttr.ReadFromDb(reader);
                Behave.Runtime.BTResolver.RegisterToCache(item.behaveDataPath);
                MonsterXmlData.InitializeData(item.id,item.behaveDataPath);
                sList.Add(item);
            }
        }
        public static void Release()
        {
			sList.Clear ();
        }
        public static Item Get(int id)
        {
			for(int i = 0; i < sList.Count; ++i)
				if(sList[i].id == id)
					return sList[i];
			return null;
        }
    }

	public class MonsterGroupProtoDb
	{
		public class Item
		{
			[DbField("id")]
			public int id;
			[DbField("ENG_name")]
			public string name;
			
			[DbField("SpawnHeight")]
			public float hOffset;
			[DbField("prefab_path")]
			public string prefabPath;
			[DbField("behave_path")]
			public string behaveDataPath;
			[DbField("SMonsterID")]
			public int protoID;
			[DbField("Attack_Num")]
			public int[] atkMinMax;
            [DbField("Spawn_Num")]
			public int[] cntMinMax;
			[DbField("RadiusDesc")]
			public float[] radiusDesc;

			[DbField("SubID")]
			public int[] subProtoID;
			[DbField("SubPos")]
			public Vector3[] subPos;
			[DbField("SubScl")]
			public Vector3[] subScl;
			[DbField("SubRot")]
			public Vector3[] subRot;
		}
		
		static List<Item> sList;
		static Dictionary<int, string> sProtoIDMapBehave;

		public static void Load()
		{
			sList = new List<Item>(50);			
			Mono.Data.SqliteClient.SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("PrototypeMonsterGroup");
			
			while (reader.Read())
			{
				Item item = PETools.DbReader.ReadItem<Item>(reader);
                Behave.Runtime.BTResolver.RegisterToCache(item.behaveDataPath);
				sList.Add(item);
			}

			sProtoIDMapBehave = new Dictionary<int, string>(50);
			foreach (Item it in sList)
			{
				sProtoIDMapBehave[it.id] = it.behaveDataPath;
			}
		}
		public static void Release()
		{
			sList = null;
		}
		public static Item Get(int id)
		{
			return sList.Find ((item) => item.id == id);
		}

		public static string GetBehavePath (int protoId)
		{
			if (sProtoIDMapBehave.ContainsKey(protoId))
				return sProtoIDMapBehave[protoId];

			return "";
		}
	}

    public static class PlayerProtoDb
    {
        public class Item
        {
            [DbField("InitBuffList")]
            public int[] initBuff;

            public DbAttr dbAttr;

			public int[] InFeildBuff = new int[]{30200053,30200046};
			public int[] RecruitBuff = new int[]{30200049,30200050};
            //public int[] initEquip = new int[]{113, 149, 210, 95, 131, 192};
        }

		static List<Item> items;
       

        public static Item Get()
        {
			return items.Count >0 ? items[0] : null;
        }

		public static Item GetRandomNpc()
		{
			return items.Count >1 ? items[1] : null;
		}

        public static void Load()
        {
            Mono.Data.SqliteClient.SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("initprop");
			items = new List<Item>();

            while (reader.Read())
            {
				Item item = new Item();
                item = PETools.DbReader.ReadItem<Item>(reader);
                item.dbAttr = new DbAttr();
                item.dbAttr.ReadFromDb(reader);
                //item.initEquip = Util.ReadIntArray(reader.GetString(reader.GetOrdinal("EquipID")));
				items.Add(item);
            }
        }

        public static void Release()
        {
			items = null;
        }
    }

    public class RandomNpcDb
    {

		public class ItemcoutDb
		{
			public int protoId;
			public int count;
			
			public ItemcoutDb(int _portoId,int _cnt)
			{
				protoId = _portoId;
				count = _cnt;
			}
			
			public static List<ItemcoutDb> LoadItemcout(string item)
			{
				List<ItemcoutDb> datas = null;
				if(!item.Equals("0"))
				{
					string[] strItemData = item.Split(',', ';');
					if (strItemData.Length > 1)
					{
						for (int i = 0; i < (strItemData.Length / 2); i++)
						{
							if(datas == null)
								datas = new List<ItemcoutDb>();
							
							datas.Add(new ItemcoutDb(Convert.ToInt32(strItemData[i * 2]), Convert.ToInt32(strItemData[i * 2 + 1])));
							//SetNpcPackageItem(npc, Convert.ToInt32(strItemData[i * 2]), Convert.ToInt32(strItemData[i * 2 + 1]));
						}
					}
				}
				
				return datas;
			}
			
		}


        public class NpcMoney
        {
            public RandomInt initValue;
            public RandomInt incValue;
            public int max;

            public static NpcMoney LoadFromText(string text)
            {
                string[] groupStrArray = text.Split(';');
                if (groupStrArray.Length != 3)
                {
                    return null;
                }

                string[] strArray = groupStrArray[0].Split(',');
                if (strArray.Length != 2)
                {
                    return null;
                }

                NpcMoney info = new NpcMoney();

                if (!int.TryParse(strArray[0], out info.initValue.m_Min))
                {
                    return null;
                }

                if (!int.TryParse(strArray[1], out info.initValue.m_Max))
                {
                    return null;
                }

                strArray = groupStrArray[1].Split(',');
                if (strArray.Length != 2)
                {
                    return null;
                }

                if (!int.TryParse(strArray[0], out info.incValue.m_Min))
                {
                    return null;
                }
                if (!int.TryParse(strArray[1], out info.incValue.m_Max))
                {
                    return null;
                }

                if (!int.TryParse(groupStrArray[2], out info.max))
                {
                    return null;
                }

                return info;
            }
        }

        public class RandomAbility
        {
            class Data
            {
                public int id;
                public int weight;
                public string label;//used for skill gennerating. two skill can't have the same label.
            }

            class Count
            {
                public int count;
                public int weight;
            }

            List<Count> countList = new List<Count>(2);
            List<Data> dataList = new List<Data>(5);

            public void AddData(int skillid, int weight, string label)
            {
                if (weight <= 0)
                {
                    Debug.LogError("skill weight error. fixed to 1");
                    weight = 1;
                }

                Data skillData = new Data();
                skillData.id = skillid;
                skillData.weight = weight;
                skillData.label = label;
                dataList.Add(skillData);
            }

            public void AddCount(int count, int weight)
            {
                if (count < 0)
                {
                    Debug.LogError("count error. fixed to 1");
                    count = 0;
                }

                if (weight <= 0)
                {
                    Debug.LogError("count weight error. fixed to 1");
                    weight = 1;
                }

                Count skillCount = new Count();
                skillCount.count = count;
                skillCount.weight = weight;

                countList.Add(skillCount);
            }

            int GetCount()
            {
                int totalWeight = 0;
                foreach (Count skillCount in countList)
                {
                    totalWeight += skillCount.weight;
                }

                int r = UnityEngine.Random.Range(0, totalWeight);

                int t = 0;
                foreach (Count skillCount in countList)
                {
                    if (r < t + skillCount.weight)
                    {
                        return skillCount.count;
                    }

                    t += skillCount.weight;
                }

                return 0;
            }

			public Ablities Get()
            {
                List<int> outSkillList = new List<int>(2);

                int skillOutCount = GetCount();
                if (skillOutCount <= 0 || skillOutCount > dataList.Count)
                {
					return NpcAblitycmpt.CompareSkillType(outSkillList);
                }

                List<Data> lastList = dataList;
                Data lastSelected = null;

                for (int i = 0; i < skillOutCount; i++)
                {
                    List<Data> skillDataListTmp = new List<Data>(lastList);

                    if (null != lastSelected)
                    {
                        skillDataListTmp.RemoveAll(delegate(Data skillData)
                        {

                            if (skillData.label == lastSelected.label)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        });
                    }

                    Data skill = GetData(skillDataListTmp);
                    if (skill != null)
                    {
                        outSkillList.Add(skill.id);

                        lastList = skillDataListTmp;
                        lastSelected = skill;
                    }
                    else
                    {
                        break;
                    }
                }

                //        Debug.Log("*********Random skill**********");
                //        foreach(int skill in outSkillList)
                //        {
                //            Debug.Log(""+skill);
                //        }
				Ablities  outAblityList = NpcAblitycmpt.CompareSkillType(outSkillList);
				return outAblityList;
            }

            Data GetData(List<Data> list)
            {
                if (list.Count <= 0)
                {
                    Debug.LogError("Get Random Skill error. no skill.");
                    return null;
                }

                int totalWeight = 0;
                foreach (Data data in list)
                {
                    totalWeight += data.weight;
                }

                if (totalWeight <= 0)
                {
                    Debug.LogError("Get Random Skill error. all skill weight is 0.");
                    return null;
                }

                int r = UnityEngine.Random.Range(0, totalWeight);

                int t = 0;
                foreach (Data data in list)
                {
                    if (r < t + data.weight)
                    {
                        return data;
                    }

                    t += data.weight;
                }

                Debug.LogError("Get Random Skill error.");
                return null;
            }

            public void LoadSkill(string tmp)
            {
                //tmp = PETools.Db.GetString(reader, "skill");
                if (!string.IsNullOrEmpty(tmp) && tmp != "0")
                {
                    string[] tmplist = tmp.Split(';');
                    for (int i = 0; i < tmplist.Length; i++)
                    {
                        if (tmplist[i] != "0")
                        {
                            string[] s = tmplist[i].Split(',');
                            if (3 == s.Length)
                            {
                                AddData(Convert.ToInt32(s[0]), Convert.ToInt32(s[1]), s[2]);
                            }
                            else
                            {
                                Debug.LogError("string[" + tmplist[i] + "] cant be splited by ',' to 3 parts.");
                            }
                        }
                    }
                }
            }

            public void LoadSkillNum(string tmp)
            {
                if (!string.IsNullOrEmpty(tmp) && tmp != "0")
                {
                    //tmp = PETools.Db.GetString(reader, "skillnum");
                    string[] tmplist = tmp.Split(';');
                    for (int i = 0; i < tmplist.Length; i++)
                    {
                        if (tmplist[i] != "0")
                        {
                            string[] s = tmplist[i].Split(',');
                            if (2 == s.Length)
                            {
                                AddCount(Convert.ToInt32(s[0]), Convert.ToInt32(s[1]));
                            }
                            else
                            {
                                Debug.LogError("string[" + tmplist[i] + "] cant be splited by ',' to 2 parts.");
                            }
                        }
                    }
                }
            }
        }

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
                RandomInt hpMax = new RandomInt();

                string[] tmplist;
                tmplist = text.Split(',');
                if (tmplist.Length != 2)
                {
                    Debug.LogError("load RandomInt error:" + text);
                }
                else
                {
                    hpMax.m_Min = Convert.ToInt32(tmplist[0]);
                    hpMax.m_Max = Convert.ToInt32(tmplist[1]);
                }
                return hpMax;
            }
        }

        public class VoiceMatch
        {
            public List<int> womanVoice;
            public List<int> manVoice;
            public VoiceMatch()
            {
                womanVoice = new List<int>();
                manVoice = new List<int>();
            }

            public int GetRandomVoice(PeSex sex)
            {
                switch (sex)
                {
                    case PeSex.Female: return womanVoice[UnityEngine.Random.Range(0, womanVoice.Count)];
                    case PeSex.Male: return   manVoice[UnityEngine.Random.Range(0, manVoice.Count)];
                    default:
                        return -1;
                }
            }

            public static VoiceMatch LoadData(string tmp)
            {
                if (!string.IsNullOrEmpty(tmp) && tmp != "0")
                {
                    string[] tmplist = tmp.Split(';');
                    if (tmplist.Length != 2)
                        return null;

                    VoiceMatch v = new VoiceMatch();
                    string[] s0 = tmplist[0].Split(',');
                     for (int i = 0; i < s0.Length; i++)
                     {
                         v.womanVoice.Add(Convert.ToInt32(s0[i]));
                     }

                    string[] s1 = tmplist[1].Split(',');
                    for (int i = 0; i < s1.Length; i++)
                    {
                        v.manVoice.Add(Convert.ToInt32(s1[i]));
                    }
                    return v;
                }

                return null;
            }
        }
        public class Item
        {
            [DbField("ID")]
            public int id;

            [DbField("HpMax")]
            string hpMaxStr
            {
                set
                {
                    hpMax = RandomInt.Load(value);
                }
            }
            public RandomInt hpMax;

            [DbField("Atk")]
            string atkStr
            {
                set
                {
                    atk = RandomInt.Load(value);
                }
            }
            public RandomInt atk;

            [DbField("Def")]
            string defStr
            {
                set
                {
                    def = RandomInt.Load(value);
                }
            }
            public RandomInt def;

            [DbField("money")]
            string moneyStr
            {
                set
                {
                    npcMoney = NpcMoney.LoadFromText(value);
                }
            }
            public NpcMoney npcMoney;

            public VoiceMatch voiveMatch;
            [DbField("VoiceType")]
            string voiceStr
            {
                set
                {
                    voiveMatch = VoiceMatch.LoadData(value);
                }
            }
          
            [DbField("skill")]
            string abilityStr
            {
                set
                {
                    if (randomAbility == null)
                    {
                        randomAbility = new RandomAbility();
                    }

                    randomAbility.LoadSkill(value);
                }
            }

            [DbField("skillnum")]
            string abilityNumStr
            {
                set
                {
                    if (randomAbility == null)
                    {
                        randomAbility = new RandomAbility();
                    }

                    randomAbility.LoadSkillNum(value);
                }
            }
            public RandomAbility randomAbility;

            [DbField("ResDamage")]
            public int resDamage;
            [DbField("AtkRange")]
            public float atkRange;

            [DbField("revive")]
            public int reviveTime;
            [DbField("equipment")]
            public int[] initEquipment;
            [DbField("item")]
            string initItem
			{
				set
				{
					initItems = ItemcoutDb.LoadItemcout(value);
				}
			}

			public List<ItemcoutDb> initItems;
            [DbField("AiPath")]
            public string behaveDataPath;

			public bool TryGetAttrRandom(AttribType type, out RandomInt randomInt)
			{
				switch(type)
				{
				case AttribType.HpMax : randomInt = hpMax;return true;
				case AttribType.Atk:    randomInt = atk;return true;
				case AttribType.Def:    randomInt = def;return true;
				default:
					randomInt = new RandomInt();return false;
				}
			}
        }

        static List<Item> sList;

        public static void Load()
        {
            sList = new List<Item>(50);

            Mono.Data.SqliteClient.SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("RandNPC");

            while (reader.Read())
            {
                Item item = PETools.DbReader.ReadItem<Item>(reader);
                Behave.Runtime.BTResolver.RegisterToCache(item.behaveDataPath);

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

	public class NpcRobotDb
	{
        public int             mID = 9088;
        public int             mFollowID = 9011;
		public string          mName = "AavRobot";
		public float           mMoveSpeed;
		public MovementField   movementField = MovementField.Sky;
		public string          robotModelPath = "Prefab/Human/AvaAzniv_robot.prefab";
		public string          behaveDataPath = "AiPrefab/EntityData/Npc/Npc_Robot";
		public Vector3         startPos = new Vector3(0,0,0);

		public NpcRobotDb(){}
		public static NpcRobotDb Instance;

		public static void Load()
		{
			Instance = new NpcRobotDb();

			Behave.Runtime.BTResolver.RegisterToCache(Instance.behaveDataPath);
		}
	}

    public class PeEntityCreator : PeSingleton<PeEntityCreator>//, IPesingleton
    {
        public const string PlayerPrefabPath = "EntityPlayer";
        public const string NpcPrefabPath = "EntityNpc";
        public const string NpcPrefabNativePath = "EntityNpcNative";
        public const string TowerPrefabPath = "EntityTower";
        public const string DoodadPrefabPath = "EntityDoodad";
        public const string MonsterPrefabPath = "EntityMonster";
        public const string MonsterNpcPrefabPath = "EntityMonster_Npc";
        public const string GroupPrefabPath = "EntityGroup";

        const int EasyBuffID = 30200168;
        public const int HumanMonsterMask = 10000;

        //void IPesingleton.Init()
        //{
        //    NpcProtoDb.Load();
        //    MonsterProtoDb.Load();
        //    RandomNpcDb.Load();
        //    PlayerProtoDb.Load();
        //    TowerProtoDb.Load();
        //}

        //~ PeEntityCreator()
        //{
        //    NpcProtoDb.Release();
        //    MonsterProtoDb.Release();
        //    RandomNpcDb.Release();
        //    PlayerProtoDb.Release();
        //    TowerProtoDb.Release();
        //}

        #region client interface


		public PeEntity CreateDoodad(int id, int protoId, Vector3 pos, Quaternion rot, Vector3 scl)
		{
            DoodadProtoDb.Item protoItem = DoodadProtoDb.Get(protoId);
            if (null == protoItem)
            {
                Debug.LogError("cant find doodad proto by id:" + protoId);
                return null; ;
            }

			PeEntity entity = EntityMgr.Instance.Create(id, DoodadPrefabPath, pos, rot, scl);
			if (entity == null)
            {
                return null;
            }

            entity.ExtSetName(new CharacterName(protoItem.name));
            //entity.ExtSetFaceIcon(protoItem.icon);
            //entity.ExtSetFaceIconBig(protoItem.icon);
            entity.SetViewModelPath(protoItem.modelPath);

            InitAttrs(entity, protoItem.dbAttr, null);

            InitProto(entity, EEntityProto.Doodad, protoId);

			CommonCmpt common = entity.GetCmpt<CommonCmpt>();
			if (common != null)
			{
				common.ItemDropId = protoItem.dropItemId;
			}
            return entity;
        }

		public PeEntity CreateTower(int id, int protoId, Vector3 pos, Quaternion rot, Vector3 scl)
		{
			TowerProtoDb.Item protoItem = TowerProtoDb.Get(protoId);
            if (null == protoItem)
            {
                //Debug.LogError("cant find tower proto by id:" + protoId);
                return null; ;
            }

			PeEntity entity = EntityMgr.Instance.Create(id, TowerPrefabPath, pos, rot, scl);
			if (entity == null)
            {
                return null;
            }

            entity.ExtSetName(new CharacterName(protoItem.name));
            //entity.ExtSetFaceIcon(protoItem.icon);
            //entity.ExtSetFaceIconBig(protoItem.icon);
            entity.SetViewModelPath(protoItem.modelPath);

            InitBehaveData(entity, protoItem.behaveDataPath);

            InitAttrs(entity, protoItem.dbAttr, null);

            InitProto(entity, EEntityProto.Tower, protoId);

            InitIdentity(entity, protoItem.eId, protoItem.eRace, false);

            InitTowerBulletData(entity, protoItem.bulletData);

			InitBattle(entity);
            return entity;
        }

		public PeEntity CreateMonster(int id, int protoId, Vector3 pos, Quaternion rot, Vector3 scl, float exScale = -1.0f, int colorType = -1,int buffId=0)	// default use protoDb's scale
		{
			MonsterProtoDb.Item protoItem = MonsterProtoDb.Get(protoId);
            if (null == protoItem){
                //Debug.LogError("cant find monster proto by id:" + protoId);
                return null;
            }

			float fScale = exScale > 0.0f ? exScale : UnityEngine.Random.Range (protoItem.fScaleMinMax [0], protoItem.fScaleMinMax [1]);

			PeEntity entity = null;
            if (protoId < HumanMonsterMask)
                entity = EntityMgr.Instance.Create(id, MonsterPrefabPath, pos, rot, scl * fScale);
            else
                entity = EntityMgr.Instance.Create(id, MonsterNpcPrefabPath, pos, rot, scl * fScale);

            if (entity == null){
                return null;
            }

            entity.ExtSetName(new CharacterName(protoItem.name));

            entity.ExtSetFaceIcon(protoItem.icon);
            //entity.ExtSetFaceIconBig(protoItem.icon);
            entity.SetViewModelPath(protoItem.modelPath);

			InitBehaveData(entity, protoItem.behaveDataPath);

            initMonsterEscape(entity, protoItem.injuredState, protoItem.escapeProb);

            InitProto(entity, EEntityProto.Monster, protoId);

            InitIdentity(entity, protoItem.eId, protoItem.eRace, protoItem.isBoss);

            if (protoId < HumanMonsterMask)
            {
                InitAttrsWithScale(entity, protoItem.dbAttr, protoItem.initBuff, fScale);
                InitEquipment(entity, protoItem.initEquip);

                if(entity.Race == ERace.Puja || entity.Race == ERace.Paja)
                {
                    if (colorType >= 0 && colorType <= 7)
                        entity.biologyViewCmpt.SetColorID(colorType);
                }
            }
            else
            {
                ApplyCustomCharactorData(entity, CreateCustomData());
                InitMonsterNpc(entity, protoItem.npcProtoID);

                InitAttrsWithScale(entity, protoItem.dbAttr, protoItem.initBuff, fScale, protoItem.npcProtoID);

                if(entity.Race == ERace.Mankind)
                {
                    if (colorType >= 0 && colorType <= 7)
                    {
                        InitEquipment(entity, MonsterRandomDb.GetEquipments(colorType));
                        InitWeapon(entity, MonsterRandomDb.GetWeapon(colorType));
                    }
                    else
                    {
                        RandomNpcDb.Item item = RandomNpcDb.Get(protoItem.npcProtoID);
                        if (item != null)
                            InitEquipment(entity, item.initEquipment);
                    }
                }
            }

            Motion_Move_Motor motor = entity.GetCmpt<Motion_Move_Motor>();
            if (motor != null)
            {
                motor.Field = protoItem.movementField;
            }

            CommonCmpt common = entity.GetCmpt<CommonCmpt>();
            if (common != null)
            {
                common.ItemDropId = protoItem.dropItemId;
            }

            MonsterCmpt monster = entity.GetCmpt<MonsterCmpt>();
            if (monster != null)
            {
                monster.InjuredLevel = protoItem.injuredLevel;
            }

            //easy mode
            if(PeGameMgr.gameLevel == PeGameMgr.EGameLevel.Easy)
            {
                if(entity.skEntity != null)
                    SkillSystem.SkEntity.MountBuff(entity.skEntity, EasyBuffID, new List<int>(), new List<float>());
            }

			if(buffId>0&&entity.skEntity != null)
				SkillSystem.SkEntity.MountBuff(entity.skEntity, buffId, new List<int>(), new List<float>());

            MonsterEntityCreator.AttachMonsterDeathEvent(entity);
            return entity;
        }

		public PeEntity CreateMonsterNet(int id, int protoId, Vector3 pos, Quaternion rot, Vector3 scl, float exScale = -1.0f,int buffId=0) // default use protoDb's scale
		{
			MonsterProtoDb.Item protoItem = MonsterProtoDb.Get(protoId);
			if (null == protoItem)
				return null;

			float fScale = exScale > 0.0f ? exScale : UnityEngine.Random.Range(protoItem.fScaleMinMax[0], protoItem.fScaleMinMax[1]);

			PeEntity entity = null;
			if (protoId < HumanMonsterMask)
				entity = EntityMgr.Instance.Create(id, MonsterPrefabPath, pos, rot, scl * fScale);
			else
				entity = EntityMgr.Instance.Create(id, MonsterNpcPrefabPath, pos, rot, scl * fScale);

			if (entity == null)
				return null;

			entity.ExtSetName(new CharacterName(protoItem.name));

			entity.ExtSetFaceIcon(protoItem.icon);
			entity.SetViewModelPath(protoItem.modelPath);

			InitBehaveData(entity, protoItem.behaveDataPath);

			InitAttrsWithScale(entity, protoItem.dbAttr, protoItem.initBuff, fScale);

			InitProto(entity, EEntityProto.Monster, protoId);

			InitIdentity(entity, protoItem.eId, protoItem.eRace, protoItem.isBoss);

			InitEquipment(entity, protoItem.initEquip);

			initMonsterEscape(entity, protoItem.injuredState, protoItem.escapeProb);

			Motion_Move_Motor motor = entity.GetCmpt<Motion_Move_Motor>();
			if (motor != null)
			{
				motor.Field = protoItem.movementField;
			}

			CommonCmpt common = entity.GetCmpt<CommonCmpt>();
			if (common != null)
			{
				common.ItemDropId = protoItem.dropItemId;
			}

			MonsterCmpt monster = entity.GetCmpt<MonsterCmpt>();
			if (monster != null)
			{
				monster.InjuredLevel = protoItem.injuredLevel;
			}

			//easy mode
			if (PeGameMgr.gameLevel == PeGameMgr.EGameLevel.Easy)
			{
				if (entity.skEntity != null)
					SkillSystem.SkEntity.MountBuff(entity.skEntity, EasyBuffID, new List<int>(), new List<float>());
			}

			if(buffId>0&&entity.skEntity != null)
				SkillSystem.SkEntity.MountBuff(entity.skEntity, buffId, new List<int>(), new List<float>());

			MonsterEntityCreator.AttachMonsterDeathEvent(entity);
			return entity;
		}

		public PeEntity CreateNpcRobot(int id, int protoId, Vector3 pos, Quaternion rot, Vector3 scl, float exScale = -1.0f)	// default use protoDb's scale
		{

            if (id != NpcRobotDb.Instance.mFollowID)
				return null;


			PeEntity follow = EntityMgr.Instance.Get(id);
			if(follow == null || follow.transform == null)
			{
				return null;
			}

			NpcProtoDb.Item protoItem = NpcProtoDb.Get(protoId);
			if (null == protoItem)
			{
				Debug.LogError("cant find doodad proto by id:" + protoId);
				return null; 
			}

//			MonsterProtoDb.Item protoItem = MonsterProtoDb.Get(protoId);
//			if (null == protoItem){
//				//Debug.LogError("cant find monster proto by id:" + protoId);
//				return null;
//			}

            PeEntity entity = EntityMgr.Instance.Create(NpcRobotDb.Instance.mID, MonsterPrefabPath, pos, rot, scl);
			if (entity == null){
				return null;
			}

			entity.ExtSetName(new CharacterName(NpcRobotDb.Instance.mName));
			entity.SetViewModelPath(NpcRobotDb.Instance.robotModelPath);
			InitBehaveData(entity, NpcRobotDb.Instance.behaveDataPath);
			InitProto(entity, EEntityProto.NpcRobot, protoId);
			InitAttrs(entity, protoItem.dbAttr, protoItem.InFeildBuff);					
			InitRobotInfo(entity,follow);

			entity.peTrans.position = NpcRobotDb.Instance.startPos;
		
			return entity;
		}

		public PeEntity CreatePlayer(int id, Vector3 pos, Quaternion rot, Vector3 scl, CustomCharactor.CustomData data = null)
        {
			PeEntity entity = EntityMgr.Instance.Create(id, PlayerPrefabPath, pos, rot, scl);

			ApplyCustomCharactorData(entity, data);

            //lz-2016.08.04  这里增加一个判断，保证必须在Tutorial模式下
            if (SingleGameStory.curType == SingleGameStory.StoryScene.TrainingShip &&Pathea.PeGameMgr.IsTutorial)
                entity.enityInfoCmpt.characterName = new CharacterName("Tutorial");

            PlayerProtoDb.Item item = PlayerProtoDb.Get();
            InitAttrs(entity, item.dbAttr, item.initBuff);

            InitProto(entity, EEntityProto.Player, -1);

            InitIdentity(entity, EIdentity.Player, ERace.Mankind, false);

            Pathea.CommonCmpt commonCmpt = entity.GetCmpt<Pathea.CommonCmpt>();
            if (null != commonCmpt)
            {
                commonCmpt.OwnerID = 1;
            }


            return entity;
        }
		public PeEntity CreateNpc(int id, int protoId, Vector3 pos, Quaternion rot, Vector3 scl)
		{
			NpcProtoDb.Item protoItem = NpcProtoDb.Get(protoId);
			if (null == protoItem)
			{
				Debug.LogError("cant find npc proto by id:" + protoId);
				return null; ;
			}

			string npcPath = NpcPrefabPath;
			if (protoItem.race == "Puja" || protoItem.race == "Paja")
				npcPath = NpcPrefabNativePath;

			PeEntity entity = EntityMgr.Instance.Create(id, npcPath, pos, rot, scl);
			if (entity == null)
			{
				return null;
			}
			
			entity.ExtSetName(new CharacterName().InitStoryNpcName(protoItem.name, protoItem.showName));
			entity.ExtSetFaceIcon(protoItem.icon);
			entity.ExtSetFaceIconBig(protoItem.iconBig);
            entity.ExtSetVoiceType(protoItem.voiceType);
			if (protoItem.modelObj != null) {
				entity.SetViewModelPath(protoItem.modelPrefabPath);
			} else {
				if (!string.IsNullOrEmpty (protoItem.modelAssetPath)) {
					entity.SetViewModelPath (protoItem.modelAssetPath);
				} else {
					entity.SetAvatarNpcModelPath (protoItem.avatarModelPath);
				}
			}
			
			entity.ExtSetSex(protoItem.sex);
            entity.ExtSetVoiceType(protoItem.voiceType);
			InitBehaveData(entity, protoItem.behaveDataPath);			
			InitAttrs(entity, protoItem.dbAttr, protoItem.InFeildBuff);			
			InitProto(entity, EEntityProto.Npc, protoId);			
			InitIdentity(entity, EIdentity.Npc, ERace.Mankind, false);	
			InitBattle(entity);
		
            Pathea.CommonCmpt commonCmpt = entity.GetCmpt<Pathea.CommonCmpt>();
            if (null != commonCmpt)
            {
                commonCmpt.OwnerID = 1;
            }

			return entity;
		}
	
		public PeEntity CreateNpcForNet(int id, int protoId, Vector3 pos, Quaternion rot, Vector3 scl)
		{
			NpcProtoDb.Item protoItem = NpcProtoDb.Get(protoId);
			if (null == protoItem)
			{
				Debug.LogError("cant find npc proto by id:" + protoId);
				return null; ;
			}
			if (EntityMgr.Instance == null)
				return null;

			string npcPath = NpcPrefabPath;
			if (protoItem.race == "Puja" || protoItem.race == "Paja")
				npcPath = NpcPrefabNativePath;

			PeEntity entity = EntityMgr.Instance.Create(id, npcPath, pos, rot, scl);
			if (entity == null)
			{
				return null;
			}
			
			entity.ExtSetName(new CharacterName().InitStoryNpcName(protoItem.name, protoItem.showName));
			entity.ExtSetFaceIcon(protoItem.icon);
			entity.ExtSetFaceIconBig(protoItem.iconBig);
			if (protoItem.modelObj != null) {
				entity.SetViewModelPath (protoItem.modelPrefabPath);
			} else {
				if (!string.IsNullOrEmpty (protoItem.modelAssetPath)) {
					entity.SetViewModelPath (protoItem.modelAssetPath);
				} else {
					entity.SetAvatarNpcModelPath (protoItem.avatarModelPath);
				}
			}			
			
			entity.ExtSetSex(protoItem.sex);
            entity.ExtSetVoiceType(protoItem.voiceType);
			
			InitBehaveData(entity, protoItem.behaveDataPath);
			
			InitAttrs(entity, protoItem.dbAttr, protoItem.InFeildBuff);
			
			InitProto(entity, EEntityProto.Npc, protoId);
			
			InitIdentity(entity, EIdentity.Npc, ERace.Mankind, false);

			InitBattle(entity);

            Pathea.CommonCmpt commonCmpt = entity.GetCmpt<Pathea.CommonCmpt>();
            if (null != commonCmpt)
            {
                commonCmpt.OwnerID = 1;
            }
			
			return entity;
		}

		public PeEntity CreateRandomNpcForNet(int templateId, int id, Vector3 pos, Quaternion rot, Vector3 scl)
        {
			PeEntity entity = EntityMgr.Instance.Create(id, NpcPrefabPath, pos, rot, scl);
			
			if (null == entity)
            {
                return null;
            }

            RandomNpcDb.Item item = RandomNpcDb.Get(templateId);
            if (item == null)
            {
                Debug.LogError("no npc random data found with templateId:" + templateId);
                return entity;
            }

			InitRandomNpcAttrForNet(entity, item);

            //InitNpcAbility(entity, item.randomAbility);

            InitBehaveData(entity, item.behaveDataPath);

            InitProto(entity, EEntityProto.RandomNpc, templateId);
            InitIdentity(entity, EIdentity.Npc, ERace.Mankind, false);

            Pathea.CommonCmpt commonCmpt = entity.GetCmpt<Pathea.CommonCmpt>();
            if (null != commonCmpt)
            {
                commonCmpt.OwnerID = 1;
            }

            return entity;
        }

		public PeEntity CreateRandomNpc(int templateId, int id, Vector3 pos, Quaternion rot, Vector3 scl)
		{
			PeEntity entity = EntityMgr.Instance.Create(id, NpcPrefabPath, pos, rot, scl);
			if (null == entity)
                return null;

            ApplyCustomCharactorData(entity, CreateCustomData());
            InitRandomNpc(entity, templateId);
            InitProto(entity, EEntityProto.RandomNpc, templateId);
            InitIdentity(entity, EIdentity.Npc, ERace.Mankind, false);
			InitBattle(entity);

            Pathea.CommonCmpt commonCmpt = entity.GetCmpt<Pathea.CommonCmpt>();
            if (null != commonCmpt)
            {
                commonCmpt.OwnerID = 1;
            }

            return entity;
        }

        public PeEntity CreateMountsMonster(MountMonsterData data)
        {

            int noid = Pathea.WorldInfoMgr.Instance.FetchNonRecordAutoId(); // not save id

            //monster base data
            PeEntity entity = CreateMonster(noid, data._protoId, data._curPostion, data._rotation,data._scale,1.0f);

            if (entity == null)
                return null;

            //mounts force data
            entity.SetAttribute(AttribType.CampID, data._mountsForce._campID);
            entity.SetAttribute(AttribType.DamageID, data._mountsForce._damageID);
            entity.SetAttribute(AttribType.DefaultPlayerID, data._mountsForce._defaultPlyerID);

            //mounts skill data
            entity.monstermountCtrl.m_MountsForceDb = data._mountsForce.Copy();
            entity.monstermountCtrl.ResetMountsSkill(data._mountsSkill);
            entity.monstermountCtrl.SetctrlType(data._eCtrltype);
            //mounts hp
            float _hpMax = entity.GetAttribute(AttribType.HpMax);
            entity.SetAttribute(AttribType.Hp, data._hp * _hpMax);



            return entity;
        }

        #endregion

        public static void initMonsterEscape(PeEntity entity, float escapeBase, float escapeProb)
        {
            Pathea.TargetCmpt c = entity.GetCmpt<Pathea.TargetCmpt>();
            if (c == null)
            {
                return;
            }

            c.EscapeBase = escapeBase;
            c.EscapeProp = escapeProb;
        }

        public static void InitTowerBulletData(PeEntity entity, TowerProtoDb.BulletData bulletData)
        {
            Pathea.TowerCmpt c = entity.GetCmpt<Pathea.TowerCmpt>();
            if (c == null)
            {
                return;
            }

            c.NeedVoxel = bulletData.needBlock;
            c.CostType = (ECostType)bulletData.bulletType;
            c.ConsumeItem = bulletData.bulletId;
            c.ConsumeCost = bulletData.bulletCost;
            c.ConsumeCountMax = bulletData.bulletMax;
            c.ConsumeEnergyCost = bulletData.energyCost;
            c.ConsumeEnergyMax = bulletData.energyMax;
            c.SkillID = bulletData.skillId;
            //c.ConsumeCount = c.ConsumeCountMax;
            //c.ConsumeEnergy = c.ConsumeEnergyMax;
        }

        public static void InitBehaveData(PeEntity entity, string behaveDataPath)
        {
            Pathea.BehaveCmpt behaveCmpt = entity.GetCmpt<Pathea.BehaveCmpt>();
            if (behaveCmpt != null)
            {
                behaveCmpt.SetAssetPath(behaveDataPath);
            }
        }

        //public static void InitEntityInfo(PeEntity entity)
        //{
        //    EntityInfoCmpt info = entity.GetCmpt<EntityInfoCmpt>();
        //    if (null != info)
        //    {
        //        //info.Name = characterName;
        //        //info.FaceIcon = npcIcon;
        //        info.mapIcon = PeMap.MapIcon.FlagIcon;
        //    }
        //}

        public static void InitProto(PeEntity entity, EEntityProto prototype, int prototypeId)
        {
            entity.entityProto = new EntityProto()
            {
                proto = prototype,
                protoId = prototypeId
            };
        }

        public static void InitIdentity(PeEntity entity, EIdentity eId, ERace eRace, bool bBoss)
        {
            Pathea.CommonCmpt commonCmpt = entity.GetCmpt<Pathea.CommonCmpt>();
            if (null == commonCmpt)
            {
                Debug.LogError("cant find common cmpt");
                return;
            }

            commonCmpt.Identity = eId;
            commonCmpt.Race = eRace;
            commonCmpt.IsBoss = bBoss;
        }

        public static void InitAttrs(PeEntity entity, DbAttr dbAttr, int[] initBuff)
        {
            Pathea.SkAliveEntity skAlive = entity.GetCmpt<Pathea.SkAliveEntity>();
            if (null != skAlive)
            {
                skAlive.m_InitBuffList = initBuff;
                skAlive.m_Attrs = dbAttr.ToAliveAttr();
                skAlive.InitSkEntity();
            }
        }

		public static void InitAttrsWithScale(PeEntity entity, DbAttr dbAttr, int[] initBuff, float fScale)
		{
			Pathea.SkAliveEntity skAlive = entity.GetCmpt<Pathea.SkAliveEntity>();
			if (null != skAlive)
			{
				skAlive.m_InitBuffList = initBuff;
				skAlive.m_Attrs = dbAttr.ToAliveAttrWithScale(fScale);
				skAlive.InitSkEntity();
			}
		}

        public static void InitAttrsWithScale(PeEntity entity, DbAttr dbAttr, int[] initBuff, float fScale, int npcRandID)
        {
            Pathea.SkAliveEntity skAlive = entity.GetCmpt<Pathea.SkAliveEntity>();
            if (null != skAlive)
            {
                skAlive.m_InitBuffList = initBuff;
                skAlive.m_Attrs = dbAttr.ToAliveAttrWithScale(fScale);

                RandomNpcDb.Item item = RandomNpcDb.Get(npcRandID);
                if (item != null)
                {
                    int n = skAlive.m_Attrs.Length;
                    Array.Resize(ref skAlive.m_Attrs, n + 4);

                    skAlive.m_Attrs[n + 0] = new PESkEntity.Attr() { m_Type = AttribType.HpMax, m_Value = item.hpMax.Random() };
                    skAlive.m_Attrs[n + 1] = new PESkEntity.Attr() { m_Type = AttribType.Atk, m_Value = item.atk.Random() };
                    skAlive.m_Attrs[n + 2] = new PESkEntity.Attr() { m_Type = AttribType.Def, m_Value = item.def.Random() };
                    skAlive.m_Attrs[n + 3] = new PESkEntity.Attr() { m_Type = AttribType.Hp, m_Value = skAlive.m_Attrs[n + 0].m_Value };
                }

                skAlive.InitSkEntity();
            }
        }

        public static void InitAttrs(PeEntity entity, PESkEntity.Attr[] dbAttr, int[] initBuff)
		{
			Pathea.SkAliveEntity skAlive = entity.GetCmpt<Pathea.SkAliveEntity>();
			if (null != skAlive)
			{
				skAlive.m_InitBuffList = initBuff;
				skAlive.m_Attrs = dbAttr;
				skAlive.InitSkEntity();
			}
		}

		public static void InitBattle(PeEntity entity)
		{
			if(entity == null)
				return ;

			NpcCmpt npccmpt = entity.GetCmpt<Pathea.NpcCmpt>();
			if(npccmpt == null)
				return ;
			npccmpt.Battle = ENpcBattle.Defence;
		}

        public static void ApplyCustomCharactorData(PeEntity entity, CustomCharactor.CustomData data)
        {
            if (null == data)
            {
                data = CustomCharactor.CustomData.DefaultFemale();
                //data = CustomCharactor.CustomData.DefaultMale();
            }

            if (!string.IsNullOrEmpty(data.charactorName))
            {
                entity.ExtSetName(new CharacterName(data.charactorName));
            }

            entity.ExtSetSex(PeGender.Convert(data.sex));
            entity.SetAvatarData(data.appearData, data.nudeAvatarData);
        }

        public static CustomCharactor.CustomData CreateCustomData()
        {
            CustomCharactor.CustomData customData = null;
            string playerHeadPath = null;
            if (CustomCharactor.CustomDataMgr.Instance.Current != null)
            {
                playerHeadPath = CustomCharactor.CustomDataMgr.Instance.Current.nudeAvatarData[CustomCharactor.AvatarData.ESlot.Head];
            }

            PeSex sex = PeGender.Random();
            if (sex == PeSex.Female)
            {
                customData = CustomCharactor.CustomData.RandomFemale(playerHeadPath);
            }
            else
            {
                customData = CustomCharactor.CustomData.RandomMale(playerHeadPath);
            }

            customData.appearData.Random();
            customData.charactorName = null;
            return customData;
        }

        public static void InitRandomNpcAttr(PeEntity entity, RandomNpcDb.Item item)
        {

            //使用RandomNpc模板的属性
			PlayerProtoDb.Item RandomNpcItem = PlayerProtoDb.GetRandomNpc();
			DbAttr attr = RandomNpcItem.dbAttr.Clone();

            attr.attributeArray[(int)Pathea.AttribType.HpMax] = item.hpMax.Random();
            attr.attributeArray[(int)Pathea.AttribType.Atk] = item.atk.Random();
            attr.attributeArray[(int)Pathea.AttribType.ResDamage] = item.resDamage;
            attr.attributeArray[(int)Pathea.AttribType.AtkRange] = item.atkRange;
            attr.attributeArray[(int)Pathea.AttribType.Def] = item.def.Random();
			attr.attributeArray[(int)Pathea.AttribType.Hp] =  attr.attributeArray[(int)Pathea.AttribType.HpMax];
			attr.attributeArray[(int)Pathea.AttribType.Hunger] =  attr.attributeArray[(int)Pathea.AttribType.HungerMax];

			InitAttrs(entity, attr, RandomNpcItem.InFeildBuff);
        }

		public static void InitRandomNpcAttrForNet(PeEntity entity, RandomNpcDb.Item item)
		{
			//使用RandomNpc模板的属性
			PlayerProtoDb.Item RandomNpcItem = PlayerProtoDb.GetRandomNpc();
			DbAttr attr = RandomNpcItem.dbAttr.Clone();
			InitAttrs(entity, attr, RandomNpcItem.InFeildBuff);
		}

        public static void InitMonsterSkinRandom(PeEntity entity, int playerId)
        {
            switch (entity.Race)
            {
                case ERace.Mankind:
                    InitEquipment(entity, MonsterRandomDb.GetEquipments(playerId));
                    InitWeapon(entity, MonsterRandomDb.GetWeapon(playerId));
                    break;
                case ERace.Puja:
                    entity.biologyViewCmpt.SetColorID(playerId);
                    break;
                case ERace.Paja:
                    entity.biologyViewCmpt.SetColorID(playerId);
                    break;
            }
        }

		public static void InitMonsterSkinRandomNet(PeEntity entity, int playerId)
		{
			switch (entity.Race)
			{
				case ERace.Puja:
					entity.biologyViewCmpt.SetColorID(playerId);
					break;
				case ERace.Paja:
					entity.biologyViewCmpt.SetColorID(playerId);
					break;
			}
		}

		public static void InitMonsterNpc(PeEntity entity, int npcProtoID)
        {
            if (null == entity) return;

            PeSex sex = entity.ExtGetSex();
            int race = UnityEngine.Random.Range(1, 5);
            entity.ExtSetName(WorldInfoMgr.Instance.FetchName(sex, race));
        }

        public static void InitRandomNpc(PeEntity entity, int templateId)
        {
            if (null == entity)
            {
                return;
            }

            PeSex sex = entity.ExtGetSex();
            int race = UnityEngine.Random.Range(1, 5);
            entity.ExtSetName(WorldInfoMgr.Instance.FetchName(sex, race));
 
            RandomNpcDb.Item item = RandomNpcDb.Get(templateId);
            if (item == null)
            {
                Debug.LogError("no npc random data found");
                return;
            }

            NpcCmpt npcCmpt = entity.GetCmpt<NpcCmpt>();
            if (npcCmpt != null)
            {
                npcCmpt.ReviveTime = item.reviveTime;
            }

            InitRandomNpcAttr(entity, item);
            InitEquipment(entity, item.initEquipment);
            InitPackage(entity, item.initItems);
            InitNpcMoney(entity, item.npcMoney);
            //mAutoReviveTime = npcRandomData.mReviveTime;
            InitNpcAbility(entity, item.randomAbility);
            InitBehaveData(entity, item.behaveDataPath);

            entity.ExtSetVoiceType(item.voiveMatch.GetRandomVoice(sex));
        }

        public static void InitNpcAbility(PeEntity entity, RandomNpcDb.RandomAbility ability)
        {
            if (null == ability)
            {
                return;
            }

            NpcCmpt npc = entity.GetCmpt<NpcCmpt>();
            if (npc == null)
            {
                return;
            }

			npc.SetAbilityIDs(ability.Get());
        }

        public static void InitPackage(PeEntity entity, IEnumerable<int> itemProtoIds)
        {
            Pathea.PackageCmpt pkgCmpt = entity.packageCmpt;
            if (null == pkgCmpt)
            {
                return;
            }

            if (null != itemProtoIds)
            {
                foreach (int itemProtoId in itemProtoIds)
                {
                    ItemAsset.ItemObject itemObj = ItemAsset.ItemMgr.Instance.CreateItem(itemProtoId);
                    if (itemObj != null)
                    {
                        pkgCmpt.Add(itemObj);
                    }
                }
            }
        }

		public static void InitPackage(PeEntity entity,List<RandomNpcDb.ItemcoutDb> items)
		{
			if(entity.packageCmpt == null)
				return ;

			if(items == null)
				return ;

			for(int i=0;i<items.Count;i++)
			{
				entity.packageCmpt.Add(items[i].protoId,items[i].count); 
			}
		}

        public static void InitNpcMoney(PeEntity entity, RandomNpcDb.NpcMoney npcMoney)
        {
            Pathea.NpcPackageCmpt npcPkgCmpt = entity.GetCmpt<Pathea.NpcPackageCmpt>();
            if (npcPkgCmpt != null)
            {
                npcPkgCmpt.money.current = npcMoney.initValue.Random();
                npcPkgCmpt.InitAutoIncreaseMoney(npcMoney.max, npcMoney.incValue.Random());
            }
        }

        public static void InitWeapon(PeEntity entity, int weaponId)
        {
            if (null == entity.equipmentCmpt)
                Debug.LogError("no equipment cmpt");
            else
            {
                ItemAsset.ItemObject itemObj = ItemAsset.ItemMgr.Instance.CreateItem(weaponId);
                if (itemObj != null)
                    entity.equipmentCmpt.AddInitEquipment(itemObj);
            }
        }

        public static void InitEquipment(PeEntity entity, IEnumerable<int> equipmentItemProtoIds)
        {
            if (equipmentItemProtoIds == null)
            {
                return;
            }

            Pathea.EquipmentCmpt equipmentCmpt = entity.GetCmpt<Pathea.EquipmentCmpt>();
            if (null == equipmentCmpt)
            {
                Debug.LogError("no equipment cmpt");
                return;
            }

            PeSex entitySex = entity.ExtGetSex();

            foreach (int equipmentItemProtoId in equipmentItemProtoIds)
            {
                ItemAsset.ItemProto itemProto = ItemAsset.ItemProto.Mgr.Instance.Get(equipmentItemProtoId);
                if (itemProto == null)
                {
                    continue;
                }

                if (!PeGender.IsMatch(itemProto.equipSex, entitySex))
                {
                    continue;
                }

                ItemAsset.ItemObject itemObj = ItemAsset.ItemMgr.Instance.CreateItem(equipmentItemProtoId);
                if (itemObj != null)
                {
                    equipmentCmpt.AddInitEquipment(itemObj);
                }
            }
        }

        public static void InitRobot()
        {
            PeEntity robot = EntityMgr.Instance.Get(NpcRobotDb.Instance.mID);
            if (robot == null)
                return;

            PeEntity follow = EntityMgr.Instance.Get(NpcRobotDb.Instance.mFollowID);
            if (follow == null || follow.transform == null)
                return ;

            InitRobotInfo(robot, follow);

        }
		public static void InitRobotInfo(PeEntity robot,PeEntity ownerEntity)
		{
			if(robot == null || ownerEntity == null)
				return ;

			if(robot.robotCmpt == null)
			  robot.gameObject.AddComponent<RobotCmpt>();

			Motion_Move_Motor motor = robot.motionMove as Motion_Move_Motor;
			if(motor != null)
			{
				motor.Field = MovementField.Sky;
			}

			if(ownerEntity.NpcCmpt != null)
				ownerEntity.NpcCmpt.AddFollowRobot(robot);

            robot.SetAttribute(AttribType.DamageID, 27);
            robot.SetAttribute(AttribType.CampID, 0);
			return;
		}

		//Npc招募与流放Buff更换
		public static void RecruitMainNpc(PeEntity entity)
		{
			NpcProtoDb.Item protoItem = NpcProtoDb.Get(entity.entityProto.protoId);
			if (null == protoItem)
			{
				Debug.LogError("cant find npc proto by id:" + entity.entityProto.protoId);
				return;
			}

			Pathea.SkAliveEntity skAlive = entity.GetCmpt<Pathea.SkAliveEntity>();
			if(null == skAlive)
				return ;

			for(int i=0;i<protoItem.RecruitBuff.Length;i++)
			{
				SkillSystem.SkEntity.MountBuff(skAlive, protoItem.RecruitBuff[i], new List<int>(), new List<float>());
			}

			NpcCmpt npccmpt = entity.NpcCmpt;
			if(npccmpt != null)
			{
				npccmpt.UpdateCampsite = false;
			}

		}

		public static void ExileMainNpc(PeEntity entity)
		{

			if(entity.IsDeath())
			{	
				if(entity.NpcCmpt != null  && entity.NpcCmpt.FixedPointPos != Vector3.zero) //&& entity.NpcCmpt.Creater == null
				{
					float Distance = PETools.PEUtil.Magnitude(entity.position,entity.NpcCmpt.FixedPointPos);
					if(Mathf.Abs(Distance) >= 256.0f )
					{
                        PETools.PEUtil.RagdollTranlate(entity, entity.NpcCmpt.FixedPointPos);
                        //entity.lodCmpt.DestroyView();
                        //entity.ExtSetPos(entity.NpcCmpt.FixedPointPos);	
                        //SceneMan.SetDirty(entity.lodCmpt);
					}
				}
				return ;
			}

			NpcProtoDb.Item protoItem = NpcProtoDb.Get(entity.entityProto.protoId);
			if (null == protoItem)
			{
				Debug.LogError("cant find npc proto by id:" + entity.entityProto.protoId);
				return;
			}

			Pathea.SkAliveEntity skAlive = entity.GetCmpt<Pathea.SkAliveEntity>();
			if(null == skAlive)
				return ;

			for(int i=0;i<protoItem.RecruitBuff.Length;i++)
			{
				skAlive.CancelBuffById(protoItem.RecruitBuff[i]);
			}

			NpcCmpt npccmpt = entity.NpcCmpt;
			if(npccmpt != null)
			{
				npccmpt.UpdateCampsite = true;
			}

		}

		public static void RecruitRandomNpc(PeEntity entity)
		{
			Pathea.SkAliveEntity skAlive = entity.GetCmpt<Pathea.SkAliveEntity>();
			if(null == skAlive)
				return ;

			//使用RandomNpc模板的属性
			PlayerProtoDb.Item RandomNpcItem = PlayerProtoDb.GetRandomNpc();
			RandomNpcItem.dbAttr.Clone();

			for(int i=0;i<RandomNpcItem.RecruitBuff.Length;i++)
			{
				SkillSystem.SkEntity.MountBuff(skAlive, RandomNpcItem.RecruitBuff[i], new List<int>(), new List<float>());
			}

			NpcCmpt npccmpt = entity.NpcCmpt;
			if(npccmpt != null)
			{
				npccmpt.UpdateCampsite = false;
			}
		}

		public static void ExileRandomNpc(PeEntity entity)
		{
			if(entity.IsDeath())
			{

				if(entity.NpcCmpt != null  && entity.NpcCmpt.FixedPointPos != Vector3.zero) //&& entity.NpcCmpt.Creater == null
				{
					float Distance = PETools.PEUtil.Magnitude(entity.position,entity.NpcCmpt.FixedPointPos);
					if(Mathf.Abs(Distance) >= 256.0f )
					{
						entity.lodCmpt.DestroyView();
						entity.ExtSetPos(entity.NpcCmpt.FixedPointPos);	
						SceneMan.SetDirty(entity.lodCmpt);
					}
				}
				return ;
			}

			Pathea.SkAliveEntity skAlive = entity.aliveEntity;
			if(null == skAlive)
				return ;

			//使用RandomNpc模板的属性
			PlayerProtoDb.Item RandomNpcItem = PlayerProtoDb.GetRandomNpc();
			RandomNpcItem.dbAttr.Clone();

			for(int i=0;i<RandomNpcItem.RecruitBuff.Length;i++)
			{
				skAlive.CancelBuffById(RandomNpcItem.RecruitBuff[i]);
			}

			NpcCmpt npccmpt = entity.NpcCmpt;
			if(npccmpt != null && npccmpt.FixedPointPos != Vector3.zero)
			{
				if(npccmpt.Creater != null || npccmpt.IsServant)
				{}
				else
				   npccmpt.Req_MoveToPosition(npccmpt.FixedPointPos,1.0f,true,SpeedState.Run);
				//npccmpt.UpdateCampsite = true;
			}
			if(npccmpt != null)
				npccmpt.UpdateCampsite = true;

		}
    }
}