using Mono.Data.SqliteClient;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

namespace ItemAsset
{
    public enum EquipType
    {
        Null = 0,
        Spade = 1,
        Mine = 1 << 1,
        Axe = 1 << 2,
        Sword = 1 << 3,
        LongWeapon = 1 << 4,
        Bow = 1 << 5,
        HandGun = 1 << 6,
        Rifle = 1 << 7,
        Shield_Hand = 1 << 8,
        Shield_Energy = 1 << 9,
        Plastron = 1 << 10,
        Tasse = 1 << 11,
        Helm = 1 << 12,
        Glove = 1 << 13,
        Shoes = 1 << 14,
        Bag = 1 << 15,
        Bomb = 1 << 16,
        Build = 1 << 17,
        SkilledMainHand = 1 << 18,
        Clothes = 1 << 19,
        Trousers = 1 << 20,
    }

    public class MaterialItem
    {
        public int protoId;
        public int count;
    }

    public class ItemLabel
    {
		public enum Root 
		{
			all = 0,   
			weapon = 1,
			equipment = 2,
			tool = 3,
			turret = 4,
			consumables = 5,
			resoure = 6,
			part = 7,
			decoration = 8,
			ISO = 100,
		}

        class LabelItem
        {
            public int typeId;
            public Root rootParent;
            public int directParent;
            public string typeName;
        }

        static Dictionary<int, LabelItem> dicItems;

        public static void Clear()
        {
            dicItems.Clear();
        }

        public static void LoadData()
        {
            dicItems = new Dictionary<int, LabelItem>(22);
            SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("itemtype");

            while (reader.Read())
            {
                int id = Convert.ToByte(reader.GetString(0));
                string name = PELocalization.GetString(Convert.ToInt32(reader.GetString(1)));
                int fatherId = Convert.ToInt32(reader.GetString(2));
                LabelItem item = new LabelItem() { typeId = id, typeName = name, directParent = fatherId };
                dicItems.Add(id, item);                
            }

            CalculateRootParent();
        }

        public static int GetParent(int typeId)
        {
            LabelItem item = dicItems[typeId];

            if (null == item)
            {
                Debug.LogError("not exist typeId:"+typeId);
                return -2;
            }

            return item.directParent;
        }

        static int GetRootParentRecursive(int typeId)
        {
            int parent = GetParent(typeId);
            if (parent == -1)
            {
                return typeId;
            }
            else if (parent == -2)
            {
                return -2;
            }
            else
            {
                return GetRootParentRecursive(parent);
            }
        }

        static void CalculateRootParent()
        {
            foreach (LabelItem item in dicItems.Values)
            {
                int rootParent = GetRootParentRecursive(item.typeId);
                if (-2 != rootParent)
                {
                    item.rootParent = (Root)rootParent;
                }                
            }
        }

        public static Root GetRootParent(int typeId)
        {
            if (!dicItems.ContainsKey(typeId))
            {
                return Root.all;
            }
            return dicItems[typeId].rootParent;
        }

        public static string GetName(int typeId)
        {
            if (!dicItems.ContainsKey(typeId))
            {
                return null;
            }
            return dicItems[typeId].typeName;
        }
        static LabelItem[] GetDirectChildren(int typeId)
        {
            List<LabelItem> tmp = new List<LabelItem>(10);
            foreach (LabelItem item in dicItems.Values)
            {
                if (item.directParent == typeId)
                {
                    tmp.Add(item);
                }
            }
            return tmp.ToArray();
        }

        public static string[] GetDirectChildrenName(int typeId)
        {
            return Array.ConvertAll<LabelItem, string>(GetDirectChildren(typeId), delegate(LabelItem item) {
                return item.typeName;
            });
        }

        public static int GetItemTypeByName(string name)
        {
            foreach (LabelItem item in dicItems.Values)
            {
                if (item.typeName == name)
                {
                    return item.typeId;
                }
            }

            return -1;
        }
    }

    public class ItemProto
    {
        public class Bundle
        {
            class RandBundle
            {
                public int itemProtoId;
                public float probablity;
            }

            class FixBundle
            {
                public int itemProtoId;
                public int count;
            }

            int mCountMax;
            int mCountMin;
            List<RandBundle> mRandList = new List<RandBundle>();
            List<FixBundle> mFixList = new List<FixBundle>();

            public static Bundle Load(string desc)
            {
				if (string.IsNullOrEmpty(desc) || InvalidStr == desc)
                {
                    return null;
                }

                string[] strings = desc.Split(';');
                string[] part1 = strings[0].Split(',');

                Bundle ret = new Bundle();

                if (part1.Length >= 4 && part1.Length % 2 == 0)
                {
                    ret.mCountMin = System.Convert.ToInt32(part1[0]);
                    ret.mCountMax = System.Convert.ToInt32(part1[1]);
                    for (int i = 2; i < part1.Length; )
                    {
                        RandBundle itemGot = new RandBundle();
                        itemGot.itemProtoId = System.Convert.ToInt32(part1[i++]);
                        itemGot.probablity = System.Convert.ToSingle(part1[i++]);
                        ret.mRandList.Add(itemGot);
                    }
                }

                if (desc.Contains(";"))
                {
                    string[] part2 = strings[1].Split(',');
                    if (part2.Length >= 2 && part2.Length % 2 == 0)
                    {
                        for (int i = 0; i < part2.Length; )
                        {
                            FixBundle itemGot = new FixBundle();
                            itemGot.itemProtoId = System.Convert.ToInt32(part2[i++]);
                            itemGot.count = System.Convert.ToInt32(part2[i++]);
                            ret.mFixList.Add(itemGot);
                        }
                    }
                }
                return ret;
            }

            public List<MaterialItem> GetItems()
            {
                List<MaterialItem> listTmp = new List<MaterialItem>(10);

                Dictionary<int, int> dicItem = new Dictionary<int, int>(10);

                int getNumber = UnityEngine.Random.Range(mCountMin, mCountMax);
                for (int i = 0; i <= getNumber; i++)
                {
                    float randValue = UnityEngine.Random.value;
                    for (int j = 0; j < mRandList.Count;j++ )
                    {
                        if (j ==0)
                        {
                            if (randValue <= mRandList[j].probablity)
                            {
                                if (dicItem.ContainsKey(mRandList[j].itemProtoId))
                                {
                                    dicItem[mRandList[j].itemProtoId] += 1;
                                }
                                else
                                {
                                    dicItem.Add(mRandList[j].itemProtoId, 1);
                                }
                                break;
                            }
                            continue;
                        }

                        if (j == mRandList.Count -1)
                        {
                            if (randValue <= mRandList[j].probablity)
                            {
                                if (dicItem.ContainsKey(mRandList[j].itemProtoId))
                                {
                                    dicItem[mRandList[j].itemProtoId] += 1;
                                }
                                else
                                {
                                    dicItem.Add(mRandList[j].itemProtoId, 1);
                                }
                                break;
                            }
                            continue;
                        }

                        if (randValue > mRandList[j -1].probablity && randValue <= mRandList[j].probablity)
                        {
                            if (dicItem.ContainsKey(mRandList[j].itemProtoId))
                            {
                                dicItem[mRandList[j].itemProtoId] += 1;
                            }
                            else
                            {
                                dicItem.Add(mRandList[j].itemProtoId, 1);
                            }
                            break;
                        }

                    }
                    //foreach (RandBundle j in mRandList)
                    //{
                    //    if (UnityEngine.Random.value < j.probablity)
                    //    {
                    //        if (dicItem.ContainsKey(j.itemProtoId))
                    //        {
                    //            dicItem[j.itemProtoId] += 1;
                    //        }
                    //        else
                    //        {
                    //            dicItem.Add(j.itemProtoId, 1);
                    //        }

                    //       // break;
                    //    }
                    //}
                }

                foreach (KeyValuePair<int, int> pair in dicItem)
                {
                    listTmp.Add(new MaterialItem() { protoId = pair.Key, count = pair.Value });
                }

                foreach (FixBundle i in mFixList)
                {
                    listTmp.Add(new MaterialItem() { protoId = i.itemProtoId, count = i.count });
                }

                return listTmp;
            }
        }

        public class PropertyList : IEnumerable<PropertyList.PropertyValue>
        {
            public class PropertyValue
            {
                public Pathea.AttribType type;
                public float value;
            }

            PropertyValue[] mPropertys;

            public PropertyList(PropertyValue[] propertys)
            {
                mPropertys = propertys;
            }

            public PropertyList() { }

            public int GetCount()
            {
                return mPropertys.Length;
            }

            public float GetProperty(Pathea.AttribType property)
            {
                if (null == mPropertys)
                {
                    return 0f;
                }

                foreach (PropertyValue value in mPropertys)
                {
                    if (value.type == property)
                    {
                        return value.value;
                    }
                }

                return 0f;
            }

            public bool HasProperty(Pathea.AttribType property)
            {
                if (null == mPropertys)
                {
                    return false;
                }

                foreach (PropertyValue value in mPropertys)
                {
                    if (value.type == property)
                    {
                        return true;
                    }
                }

                return false;
            }

            public void AddProperty(Pathea.AttribType property, float v)
            {
                if (HasProperty(property))
                {
                    SetProperty(property, v);
                }

                PropertyValue[] newPropertys;

                if (mPropertys == null)
                {
                    newPropertys = new PropertyValue[1];
                }
                else
                {
                    newPropertys = new PropertyValue[mPropertys.Length + 1];
                    Array.Copy(mPropertys, newPropertys, mPropertys.Length);
                }

                mPropertys = newPropertys;

                mPropertys[mPropertys.Length-1] = new PropertyValue() { type = property, value = v };

            }

            public void SetProperty(Pathea.AttribType property, float value)
            {
                if (!HasProperty(property))
                {
                    AddProperty(property, value);
                }

                if (null == mPropertys)
                {
                    mPropertys = new PropertyValue[1];
                }

                foreach (PropertyValue v in mPropertys)
                {
                    if (v.type == property)
                    {
                        v.value = value;
                    }
                }
            }


            IEnumerator<PropertyList.PropertyValue> IEnumerable<PropertyList.PropertyValue>.GetEnumerator()
            {
                if (null == mPropertys)
                {
                    return null;
                }

                return mPropertys.AsEnumerable<PropertyValue>().GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                if (null == mPropertys)
                {
                    return null;
                }

                return mPropertys.GetEnumerator();
            }

            public static PropertyList LoadFromDb(Mono.Data.SqliteClient.SqliteDataReader reader)
            {
                float[] attributeArray = ReadFromDb(reader);

                int count = 0;
                for(int i = 0; i < attributeArray.Length; i++)
                {
                    if (Mathf.Abs(attributeArray[i] - 0f) > PETools.PEMath.Epsilon)
                    {
                        count++;
                    }
                }

                if (count == 0)
                {
                    return null;
                }

                PropertyList.PropertyValue[] propertys = new PropertyList.PropertyValue[count];

                count = 0;
                for (int i = 0; i < attributeArray.Length; i++)
                {
                    if (Mathf.Abs(attributeArray[i] - 0f) > PETools.PEMath.Epsilon)
                    {
                        propertys[count++] = new PropertyList.PropertyValue()
                        {
                            type = (Pathea.AttribType)i,
                            value = attributeArray[i]
                        };
                    }
                }

                return new PropertyList(propertys);
            }

            static float[] ReadFromDb(Mono.Data.SqliteClient.SqliteDataReader reader)
            {
                float[] attributeArray = new float[(int)Pathea.AttribType.Max];

                for (int i = (int)Pathea.AttribType.HpMax; i < (int)Pathea.AttribType.Max; i++)
                {
                    Pathea.AttribType a = (Pathea.AttribType)i;
                    string attributeName = a.ToString();
                    attributeArray[i] = System.Convert.ToSingle(reader.GetString(reader.GetOrdinal(attributeName)));
                }
                return attributeArray;
            }

            //public static PropertyList LoadFromText(string str)
            //{
            //    if (string.IsNullOrEmpty(str))
            //    {
            //        return null;
            //    }

			//    if (str == InvalidStr)
            //    {
            //        return null;
            //    }

            //    string[] strList = str.Split(';');
            //    PropertyList.PropertyValue[] propertys = new PropertyList.PropertyValue[strList.Length];
            //    for (int i = 0; i < strList.Length; i++)
            //    {
            //        string[] substrList = strList[i].Split(',');

            //        propertys[i] = new PropertyList.PropertyValue()
            //        {
            //            type = (Pathea.AttribType)Convert.ToInt32(substrList[0]),
            //            value = Convert.ToSingle(substrList[1])
            //        };
            //    }
            //    return new PropertyList(propertys);
            //}
        }

		public class WeaponInfo
		{
			public AttackMode[] attackModes;
			public bool useEnergry;
			public int 	costItem;
			public int 	costPerShoot;
		}

        #region InfoBase
        public int id;
        
        //public string name;
        public string name;//for creation
//        public int nameStringId;

		public string dragName;
        
        //public string m_Explain;
        public string englishDescription;//for creation
        public int descriptionStringId;

		public int placeSoundID;

        public string shopIcon;

        public string[] icon;

        public Texture2D iconTex;//for creation

        public byte itemLabel;   // itemtype. for example: sword,ammo,ring,food,
        public ItemLabel.Root rootItemLabel
        {
            get
            {
                return ItemLabel.GetRootParent(itemLabel);
            }
        }

        public int level;
        public int sortLabel;
        public int tabIndex;
        public int itemClassId;
        public int currencyValue;
        public int currencyValue2;
        public int currency
        {
            get
            {
				if (Pathea.PeGameMgr.sceneMode==Pathea.PeGameMgr.ESceneMode.Story)//Pathea.Money.Digital
                {

                    return currencyValue;
                }
                else
                {
                    return currencyValue2;
                }
            }
        }
        public int maxStackNum;
        #endregion

        public byte setUp;

		const string InvalidStr = "0";
        public string resourcePath;
        public string resourcePath1;

        //public int m_soundID;
        //public int effectSoundId;	

        #region Equipment
        //public string m_AttachBone;
        public int equipReplacePos;
        public int equipPos;
        public Pathea.PeSex equipSex;
        public EquipType equipType;
        public float durabilityFactor;
        #endregion

        //public int m_TaskID;
        public int durabilityMax;
        public int engergyMax;
		public bool unchargeable=false;

        public int[] replicatorFormulaIds;

        public Bundle bundle;
        public string category;

        public int editorTypeId;

		public bool isFormula;

		public Color color = Color.white;

        public int towerEntityId;
        #region property
        public int buffId;
        public int skillId;
        public PropertyList propertyList; 
        #endregion

        #region repair
        //used to distinguish item class when repair, it can be remove.
        public int repairLevel;
        public List<MaterialItem> repairMaterialList; 
        #endregion

        public List<MaterialItem> strengthenMaterialList;

		public WeaponInfo weaponInfo;

		public ItemProto()
		{
			name = "";
			resourcePath = AssetsLoader.InvalidAssetPath;
			resourcePath1 = AssetsLoader.InvalidAssetPath;
			icon = null;
            englishDescription = "";
		}


        public class Mgr : Pathea.MonoLikeSingleton<Mgr>
        {
            List<ItemProto> mList;
			
			Dictionary<int, ItemEditorType> m_ItemEditorTypes = new Dictionary<int, ItemEditorType> ();

			public class ItemEditorType
			{
				public int	id;
				public Color color;
				public int parentID;
			}

			protected override void OnInit()
            {
                base.OnInit();
                LoadData();
            }

			public override void OnDestroy()
            {
                base.OnDestroy();
                Clear();
            }

            void Clear()
            {
                mList.Clear();
                ItemAsset.ItemLabel.Clear();
            }

            void LoadData()
            {
                ItemAsset.ItemLabel.LoadData();
                LoadProto();
            }

            public static int GetIdFromPin(int pin)
            {
                SqliteDataReader reader = LocalDatabase.Instance.SelectWhereSingle("PrototypeItem", "id", "pin", " = ", "'" + pin + "'");
                if (reader.Read())
                {
                    return Convert.ToInt32(reader.GetString(reader.GetOrdinal("id")));
                }

                return -1;
            }

            void LoadProto()
            {
				SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("item_editor_type");

				while (reader.Read()) 
				{
					ItemEditorType editorType = new ItemEditorType();
					editorType.id = Convert.ToInt32(reader.GetString(reader.GetOrdinal("id")));

					string colorString = reader.GetString(reader.GetOrdinal("Colour"));
					string[] colStrs = colorString.Split(',');
					editorType.color = new Color(Convert.ToSingle(colStrs[0]) / 255f,
					                             Convert.ToSingle(colStrs[1]) / 255f,
					                             Convert.ToSingle(colStrs[2]) / 255f, 
					                             Convert.ToSingle(colStrs[3]) / 255f);
					editorType.parentID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("parentid")));
					m_ItemEditorTypes[editorType.id] = editorType;
				}

				reader = LocalDatabase.Instance.ReadFullTable("WeaponInfo");
				Dictionary<int, WeaponInfo> weaponInfos = new Dictionary<int, WeaponInfo>();

				while (reader.Read())
				{
					WeaponInfo weaponInfo = new WeaponInfo();
					string str = reader.GetString(reader.GetOrdinal("AttackMode"));
					weaponInfo.attackModes = ConvertToAttackModes(str);
					weaponInfo.useEnergry = Convert.ToInt32(reader.GetString(reader.GetOrdinal("UseEnergry"))) > 0;
					weaponInfo.costItem = Convert.ToInt32(reader.GetString(reader.GetOrdinal("CostItem")));
					weaponInfo.costPerShoot = Convert.ToInt32(reader.GetString(reader.GetOrdinal("CostPerShoot")));
					weaponInfos[Convert.ToInt32(reader.GetString(reader.GetOrdinal("ItemID")))] = weaponInfo;
				}
					
                reader = LocalDatabase.Instance.ReadFullTable("PrototypeItem");
                //skip first row
                reader.Read();

                mList = new List<ItemProto>();
                while (reader.Read())
                {
                    ItemProto protoItem = new ItemProto();
                    protoItem.id = Convert.ToInt32(reader.GetString(reader.GetOrdinal("id")));
                    //f_TD.name = reader.GetString(reader.GetOrdinal("_name"));
                    protoItem.level = Convert.ToInt32(reader.GetString(reader.GetOrdinal("itemlv")));
//                    protoItem.name = reader.GetString(reader.GetOrdinal("_engName"));
//                    protoItem.nameStringId = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_nameID")));
					protoItem.name = PELocalization.GetString(Convert.ToInt32(reader.GetString(reader.GetOrdinal("_nameID"))));
					if("" == protoItem.name)
						protoItem.name = reader.GetString(reader.GetOrdinal("_engName"));
					protoItem.dragName = PELocalization.GetString(Convert.ToInt32(reader.GetString(reader.GetOrdinal("_dragnameID"))));
                    protoItem.itemLabel = Convert.ToByte(reader.GetString(reader.GetOrdinal("_typeId")));   // itemtype. for example: sword,ammo,ring,food,
                    protoItem.setUp = Convert.ToByte(reader.GetString(reader.GetOrdinal("_setUp")));

                    protoItem.resourcePath = reader.GetString(reader.GetOrdinal("_modelPath"));
					protoItem.resourcePath1 = reader.GetString(reader.GetOrdinal("_logicPath"));

                    protoItem.shopIcon = reader.GetString(reader.GetOrdinal("servericon"));

                    string iconString = reader.GetString(reader.GetOrdinal("_iconId"));
                    if (!string.IsNullOrEmpty(iconString))
                    {
                        protoItem.icon = iconString.Split(',');
                    }
                    //f_TD.effectSoundId = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_effectSound")));
					protoItem.placeSoundID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_placeSound")));
                    //f_TD.m_Explain = reader.GetString(reader.GetOrdinal("_explain"));
                    protoItem.englishDescription = "";//reader.GetString(reader.GetOrdinal("_engExplain"));
                    protoItem.descriptionStringId = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_engExplain")));
                    //f_TD.m_AttachBone = reader.GetString(reader.GetOrdinal("_bindNode"));
                    protoItem.equipReplacePos = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_replacePos")));
                    protoItem.durabilityFactor = Convert.ToSingle(reader.GetString(reader.GetOrdinal("durDec")));
                    protoItem.equipPos = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_position")));
                    protoItem.buffId = Convert.ToInt32(reader.GetString(reader.GetOrdinal("buffId")));
                    protoItem.skillId = Convert.ToInt32(reader.GetString(reader.GetOrdinal("skillId")));
                    protoItem.towerEntityId = Convert.ToInt32(reader.GetString(reader.GetOrdinal("turret_id")));
                    //f_TD.m_TaskID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_taskId")));
                    protoItem.durabilityMax = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_durability")));
                    
                    string tmp = reader.GetString(reader.GetOrdinal("_property"));
					if(!string.IsNullOrEmpty(tmp) && tmp != InvalidStr)
                    {
                        string[] tmpArray = tmp.Split(',');
                        if (tmpArray.Length > 0)
                        {
                            foreach (string s in tmpArray)
                            {
                                string[] sArray = s.Split(':');
                                if(sArray.Length == 2 && sArray[0] == "EnergyMax")
                                {
                                    protoItem.engergyMax = Convert.ToInt32(sArray[1]);
                                }
                                

                            }
                        }
                        
                    }

                    protoItem.currencyValue = Convert.ToInt32(reader.GetString(reader.GetOrdinal("currency_value")));
                    protoItem.currencyValue2 = Convert.ToInt32(reader.GetString(reader.GetOrdinal("currency_value2")));

                    //protoItem.disappearTime = Convert.ToSingle(reader.GetString(reader.GetOrdinal("disappear_time")));
                    protoItem.maxStackNum = Convert.ToInt32(reader.GetString(reader.GetOrdinal("stacking_num")));
                    protoItem.equipSex = Pathea.PeGender.Convert(Convert.ToInt32(reader.GetString(reader.GetOrdinal("sex"))));
                    protoItem.tabIndex = Convert.ToInt32(reader.GetString(reader.GetOrdinal("tab")));

                    //f_TD.m_OpType = (ItemOperationType)Convert.ToInt32(reader.GetString(reader.GetOrdinal("_oprationType")));
                    protoItem.itemClassId = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_function")));
                    protoItem.equipType = (EquipType)Convert.ToInt32(reader.GetString(reader.GetOrdinal("_WeaponType")));
                    protoItem.repairLevel = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_repair")));
                    //f_TD.bind = Convert.ToBoolean(Convert.ToInt32(reader.GetString(reader.GetOrdinal("bind"))));
                    protoItem.sortLabel = Convert.ToInt32(reader.GetString(reader.GetOrdinal("sort")));

                    string str = reader.GetString(reader.GetOrdinal("learn"));
					if (str != InvalidStr)
                    {
                        string[] strList = str.Split(',');
                        protoItem.replicatorFormulaIds = new int[strList.Length];
                        for (int i = 0; i < strList.Length; i++)
                        {
                            protoItem.replicatorFormulaIds[i] = Convert.ToInt32(strList[i]);
                        }
                    }

                    protoItem.propertyList = PropertyList.LoadFromDb(reader);

                    str = reader.GetString(reader.GetOrdinal("_repairMax"));
                    protoItem.repairMaterialList = ConvertToMaterialItems(str);

                    str = reader.GetString(reader.GetOrdinal("_strengthen"));
                    protoItem.strengthenMaterialList = ConvertToMaterialItems(str);

                    str = reader.GetString(reader.GetOrdinal("bundle"));
                    protoItem.bundle = Bundle.Load(str);
                    protoItem.category = reader.GetString(reader.GetOrdinal("category"));
					protoItem.isFormula = Convert.ToInt32(reader.GetString(reader.GetOrdinal("is_formula"))) != 0;
                    //f_TD.mPlayerStateActive = Convert.ToInt32(reader.GetOrdinal(23));
                    protoItem.editorTypeId = reader.GetInt32(reader.GetOrdinal("item_editor_type"));

					protoItem.color = m_ItemEditorTypes[protoItem.editorTypeId].color;
					
					//str = reader.GetString(reader.GetOrdinal("AttackMode"));
					if(weaponInfos.ContainsKey(protoItem.id))
						protoItem.weaponInfo = weaponInfos[protoItem.id];

                    mList.Add(protoItem);
                }
            }

            static List<MaterialItem> ConvertToMaterialItems(string text)
            {
                if (string.IsNullOrEmpty(text))
                {
                    return null;
                }

				if (text == InvalidStr)
                {
                    return null;
                }

                string[] strList = text.Split(';');

                List<MaterialItem> list = new List<MaterialItem>(strList.Length);

                for (int i = 0; i < strList.Length; i++)
                {
                    string[] substrList = strList[i].Split(',');
                    int id = Convert.ToInt32(substrList[0]);
                    int cnt = Convert.ToInt32(substrList[1]);

                    list.Add(new MaterialItem()
                    {
                        protoId = id,
                        count = cnt
                    });
                }
                return list;
            }

			static AttackMode[] ConvertToAttackModes(string text)
			{
				if (string.IsNullOrEmpty(text))
				{
					return null;
				}
				
				if (text == InvalidStr)
				{
					return null;
				}

				string[] strList = text.Split(';');
				AttackMode[] retList = new AttackMode[strList.Length];

				for(int i = 0; i < strList.Length; ++i)
				{
					string[] substrList = strList[i].Split(',');
					AttackMode attackMode = new AttackMode();
					attackMode.type = (AttackType)Convert.ToInt32(substrList[0]);
					attackMode.minRange = Convert.ToSingle(substrList[1]);
					attackMode.maxRange = Convert.ToSingle(substrList[2]);
					attackMode.minSwitchRange = Convert.ToSingle(substrList[3]);
					attackMode.maxSwitchRange = Convert.ToSingle(substrList[4]);
					attackMode.minAngle = Convert.ToSingle(substrList[5]);
					attackMode.maxAngle = Convert.ToSingle(substrList[6]);
					attackMode.frequency = Convert.ToSingle(substrList[7]);
					attackMode.damage = Convert.ToSingle(substrList[8]);
					attackMode.ignoreTerrain = Convert.ToInt32(substrList[9]) > 0;
					retList[i] = attackMode;
				}
				return retList;
			}

            public ItemProto Get(int protoId)
            {
                return mList.Find((item) =>
                {
                    if (item.id == protoId)
                    {
                        return true;
                    }

                    return false;
                });
            }

			public ItemEditorType GetEditorType(int editorID)
			{
				if(m_ItemEditorTypes.ContainsKey(editorID))
					return m_ItemEditorTypes[editorID];
				return null;
			}

            public ItemProto GetByEditorType(int editorType) 
            {
                return mList.Find(item =>
                {
                    if (item.editorTypeId == editorType)
                        return true;
                    return true;
                });
            }

            public void Add(ItemProto data)
            {
                mList.Add(data);
            }

            public bool Remove(ItemProto data)
            {
                return mList.Remove(data);
            }

            public bool Remove(int protoId)
            {
                return mList.RemoveAll((item) =>
                {
                    if (item.id == protoId)
                    {
                        return true;
                    }

                    return false;
                }) > 0;
            }

            public void Foreach(Action<ItemProto> action)
            {
                mList.ForEach(action);
            }

            public void ClearCreation()
            {
                for (int i = mList.Count - 1; i >= 0; i--)
                {
                    if (mList[i].id >= CreationData.ObjectStartID)
                    {
                        mList.RemoveAt(i);
                    }
                }
            }
        }

		public static byte[] GetBuffer(ItemProto data)
		{
			if (null == data)
				return null;

			using (MemoryStream ms = new MemoryStream())
			using (BinaryWriter bw = new BinaryWriter(ms))
			{
				bw.Write(data.id);
				bw.Write(data.itemLabel);
				bw.Write(data.setUp);
                //bw.Write(data.m_AttachBone);
				bw.Write(data.equipPos);
				bw.Write(data.buffId);
				bw.Write(data.durabilityMax);
				bw.Write(data.currencyValue);
                bw.Write(data.currencyValue2);
				bw.Write(data.maxStackNum);
				bw.Write((int)data.equipSex);
				bw.Write(data.tabIndex);
				//bw.Write(data.actionSkill);
				//bw.Write(data.playerStateActive);
                //bw.Write((int)data.m_OpType);
				bw.Write((int)data.equipType);
				bw.Write(data.itemClassId);
				bw.Write(data.sortLabel);
				bw.Write(data.engergyMax);

                if (data.propertyList == null)
                {
                    bw.Write((int)0);
                }
                else
                {
                    bw.Write(data.propertyList.GetCount());
                    foreach (PropertyList.PropertyValue v in data.propertyList)
                    {
                        bw.Write((int)v.type);
                        bw.Write(v.value);
                    }
                }

                //bw.Write(data.mEquipRandomProperty.Count);
                //foreach (KeyValuePair<ItemProperty, float> kv in data.mEquipRandomProperty)
                //{
                //    bw.Write((int)kv.Key);
                //    bw.Write(kv.Value);
                //}

                if (data.repairMaterialList == null)
                {
                    bw.Write((int)0);
                }
                else
                {
                    bw.Write(data.repairMaterialList.Count);
                    foreach (MaterialItem item in data.repairMaterialList)
                    {
                        bw.Write(item.protoId);
                        bw.Write(item.count);
                    }
                }

                if (data.strengthenMaterialList == null)
                {
                    bw.Write((int)0);
                }
                else
                {
                    bw.Write(data.strengthenMaterialList.Count);
                    foreach (MaterialItem item in data.strengthenMaterialList)
                    {
                        bw.Write(item.protoId);
                        bw.Write(item.count);
                    }
                }
                bw.Write(data.durabilityFactor);
				bw.Flush();
				return ms.ToArray();
			}
		}

        public static string GetName(int id)
        {
            ItemProto data = ItemProto.Mgr.Instance.Get(id);
            return (data != null) ? data.name : "";
        }
		
		public string GetName()
		{
//            string name = "";
//            if (id < CreationData.ObjectStartID)
//                name = PELocalization.GetString(nameStringId);
//            else
//                name = name;
            return name;
		}
		
		public static string[] GetIconName(int id)
		{
            ItemProto data = ItemProto.Mgr.Instance.Get(id);
            return (data != null) ? data.icon : null;
		}

        public static ItemProto GetItemData(int id)
        {
            return ItemProto.Mgr.Instance.Get(id);
        }

        public static ItemProto GetItemDataByEditorType(int editorType) 
        {
            return ItemProto.Mgr.Instance.GetByEditorType(editorType);
        }

        public static int GetPrice(int protoId)
        {
            return ItemProto.Mgr.Instance.Get(protoId).currencyValue;
        }

		public static List<MaterialItem> GetRepairMaterialList(int protoId){
			if(ItemProto.Mgr.Instance.Get(protoId)==null)
				return null;
			return ItemProto.Mgr.Instance.Get(protoId).repairMaterialList;
		}

        public static int GetStackMax(int protoId)
        {
            return ItemProto.Mgr.Instance.Get(protoId).maxStackNum;
        }

        public static byte GetSetUp(int id)
        {
            ItemProto data = ItemProto.Mgr.Instance.Get(id);
            return (data != null) ? data.setUp : (byte)0;
        }
		
        //public static int GetBuildItemID(byte type)
        //{
        //    ItemData data = s_tblItemData.Find(ite0 => {return ite0.m_SetUp == type;});
        //    return (data != null) ? data.m_ID : 0;
        //}
		
        //public static ItemData GetBuildItemData(byte type)
        //{
        //    return s_tblItemData.Find(ite0 => {return ite0.m_SetUp == type;});
        //}

        public bool IsBlock()
        {
            return itemClassId == 13;
        }
    }

}
