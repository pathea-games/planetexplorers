using UnityEngine;
using System.Collections.Generic;
using System.IO;

using ItemAsset.SlotListHelper;

namespace ItemAsset
{
    public class ItemObject : ItemSample, IEnumerable<Cmpt>
    {
        public const int VERSION_0000 = 0;
        public const int CURRENT_VERSION = VERSION_0000;
        public int Version = 0;

        List<Cmpt> mListCmpt = new List<Cmpt>(10);

        int mInstanceId;
        public int instanceId
        {
            get
            {
                return mInstanceId;
            }
        }

        public void SetInstanceId(int id)
        {
            mInstanceId = id;
        }

        #region Constructor
        ItemObject(int protoId) : base(protoId) { }

        //public ItemObject(int instanceId, int prototypeId)
        //    : base(prototypeId)
        //{
        //    mInstanceId = instanceId;
        //}
        #endregion

        #region public function
        public void Init()
        {
            if (null == mListCmpt)
            {
                return;
            }

            foreach (Cmpt c in mListCmpt)
            {
                c.Init();
            }
        }

        public void Add(Cmpt cmpt)
        {
            if (null == cmpt)
            {
                return;
            }

            cmpt.itemObj = this;

            mListCmpt.Add(cmpt);
        }

        public bool Remove(Cmpt cmpt)
        {
            if (false == mListCmpt.Remove(cmpt))
            {
                return false;
            }

            return true;
        }

        public bool Remove(string cmptName)
        {
            Cmpt cmpt = GetCmpt(cmptName);

            return Remove(cmpt);
        }

        public bool Contains(string cmptName)
        {
            return null != GetCmpt(cmptName);
        }

        public string[] GetCmptNames()
        {
            List<string> listName = mListCmpt.ConvertAll<string>(delegate(Cmpt c)
            {
                return c.GetTypeName();
            });

            return listName.ToArray();
        }

        public int CmptCount
        {
            get
            {
                return mListCmpt.Count;
            }
        }

        public Cmpt GetCmpt(string cmptName)
        {
            Cmpt cmpt = mListCmpt.Find(delegate(Cmpt c)
            {
                if (c.GetTypeName() == cmptName)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            });

            return cmpt;
        }

        public T GetCmpt<T>() where T : Cmpt
        {
            Cmpt cmpt = mListCmpt.Find(delegate(Cmpt c)
            {

                if (c is T)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            });

            return cmpt as T;
        }

		public void Serialize(BinaryWriter w)
		{
			w.Write(protoId);
			w.Write(instanceId);
			PETools.Serialize.WriteData(Export, w);
		}
        #endregion

        #region ulink
        public static new object Deserialize(uLink.BitStream stream, params object[] codecOptions)
        {
            try
            {
                byte[] buff = stream.ReadBytes();
                return NetImport(buff);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e);
                return null;
            }
        }

        public static new void Serialize(uLink.BitStream stream, object value, params object[] codecOptions)
        {
            try
            {
                ItemObject itemObj = value as ItemObject;
                if (null != itemObj)
                {
                    byte[] buff = PETools.Serialize.Export(w => { itemObj.Export4Net(w); });
                    stream.WriteBytes(buff);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e);
            }
        }

        #endregion

        public static ItemObject Deserialize(byte[] buff)
        {
            if (null == buff || buff.Length <= 0)
            {
                Debug.LogError("buff is null");
                return null;
            }

            using (MemoryStream ms = new MemoryStream(buff, false))
            using (BinaryReader r = new BinaryReader(ms))
            {
                int prototypeId = r.ReadInt32();
                int instanceId = r.ReadInt32();
                byte[] itemBuff = PETools.Serialize.ReadBytes(r);

                ItemObject item = ItemMgr.Instance.Get(instanceId);
                if (null == item)
                {
                    item = Create(prototypeId);
                    if (null != item && null != itemBuff && itemBuff.Length > 0)
                    {
                        item.SetInstanceId(instanceId);
                        item.Import(itemBuff);
                        ItemMgr.Instance.Add(item);
                    }
                }
                else
                {
                    item.Import(itemBuff);
                }

                return item;
            }
        }

        public static byte[] Serialize(ItemObject item)
        {
            if (null == item)
            {
                Debug.LogError("item is null");
                return null;
            }
            using (MemoryStream ms = new MemoryStream(100))
            {
                using (BinaryWriter w = new BinaryWriter(ms))
                {
					item.Serialize(w);
                }
                return ms.ToArray();
            }
        }

        public static ItemObject NetImport(byte[] data)
        {
            if (null == data || data.Length == 0)
            {
                Debug.LogWarning("buff is null");
                return null;
            }

            ItemObject item = null;

            PETools.Serialize.Import(data, r =>
            {
                int protoId = BufferHelper.ReadInt32(r);
                int stackCount = BufferHelper.ReadInt32(r);
                int id = BufferHelper.ReadInt32(r);
                BufferHelper.ReadBoolean(r);
                int cmptCount = BufferHelper.ReadInt32(r);

                item = ItemMgr.Instance.Get(id);
                if (null == item)
                {
                    item = Create(protoId, id);
                    if (null == item)
                    {
                        Debug.LogWarningFormat("id:{0}, prototype id:{1} import error.", id, protoId);
                        return;
                    }

                    ItemMgr.Instance.Add(item);
                }

                item.SetStackCount(stackCount);

                for (int i = 0; i < cmptCount; i++)
                {
                    string cmptName = BufferHelper.ReadString(r);
                    Cmpt c = item.GetCmpt(cmptName);
                    if (null != c)
                    {
                        c.Import(r);
                    }
                    else
                    {
                        Debug.LogWarningFormat("item id:{0}, item id:{1} does not contain cmpt:{2}", id, protoId, cmptName);
                        break;
                    }
                }
            });

            return item;
        }
        #region import export
        public override void Export(BinaryWriter w)
        {
            PETools.Serialize.WriteData(base.Export, w);

			int nCmpt = mListCmpt.Count;
            w.Write(bind);
			w.Write(nCmpt);

			for (int i = 0; i < nCmpt; i++) {
				Cmpt cmpt = mListCmpt [i];
				w.Write (cmpt.GetTypeName ());
				PETools.Serialize.WriteBytes (cmpt.Export (), w);
			}
        }

		public override void Export4Net(BinaryWriter w)
        {
			base.Export4Net(w);

			int nCmpt = mListCmpt.Count;
            BufferHelper.Serialize(w, mInstanceId);
            BufferHelper.Serialize(w, bind);
			BufferHelper.Serialize(w, nCmpt);

			for (int i = 0; i < nCmpt; i++) {
				Cmpt cmpt = mListCmpt [i];
				BufferHelper.Serialize (w, cmpt.GetTypeName ());
				cmpt.Export (w);
			}
        }

        public override void Import(byte[] buffer)
        {
            PETools.Serialize.Import(buffer, (r) =>
            {
                byte[] baseBuff = PETools.Serialize.ReadBytes(r);
                base.Import(baseBuff);

                bind = r.ReadBoolean();

                int count = r.ReadInt32();

                for (int i = 0; i < count; i++)
                {
                    string name = r.ReadString();

                    byte[] cmptBuff = PETools.Serialize.ReadBytes(r);

                    if (null == cmptBuff || cmptBuff.Length <= 0)
                    {
                        continue;
                    }

                    Cmpt c = GetCmpt(name);
                    if (null != c)
                    {
                        c.Import(cmptBuff);
                    }
                }
            }
            );

            Init();
        }

        public override void Import(BinaryReader r)
        {
            base.Import(r);

            mInstanceId = BufferHelper.ReadInt32(r);
            bind = BufferHelper.ReadBoolean(r);
            int count = BufferHelper.ReadInt32(r);

            for (int i = 0; i < count; i++)
            {
                string name = BufferHelper.ReadString(r);
                Cmpt c = GetCmpt(name);
                if (null != c)
                {
                    c.Import(r);
                }
                else
                {
                    Debug.LogErrorFormat("item id:{0}, item id:{1} does not contain cmpt:{2}", instanceId, protoId, name);
                    break;
                }
            }
        }

        #endregion

        #region IEnumerable<PeCmpt>
        IEnumerator<Cmpt> IEnumerable<Cmpt>.GetEnumerator()
        {
            return mListCmpt.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return mListCmpt.GetEnumerator();
        }
        #endregion

        #region Price

        PeFloatRangeNum mFloatFactor = null;
        public void SetPriceFactor(PeFloatRangeNum f)
        {
            mFloatFactor = f;
        }

        public float priceFactor
        {
            get
            {
                if (mFloatFactor == null)
                {
                    return 1f;
                }

                return mFloatFactor.percent;
            }
        }

        static int GetPrice(int basePrice, float factor)
        {
            return (int)(basePrice * (0.8f * factor + 0.2f));
        }

        public int GetSellPrice()
        {
            return GetPrice(protoData.currency, priceFactor);
        }

        public int GetBuyPrice()
        {
            // ShopData data = ShopRespository.GetShopData(protoId);
            ShopData data = ShopRespository.GetShopDataByItemId(protoId);
            if (data == null)
            {
                return 0;
            }

            return GetPrice(data.m_Price, priceFactor);
        }

        public int GetBuyPrice(ShopData data)
        {
            //ShopData data = ShopRespository.GetShopData(protoId);
            if (data == null)
            {
                return 0;
            }

            return GetPrice(data.m_Price, priceFactor);
        }

        #endregion

        #region tooltip
        string GetOriginText()
        {
            return PELocalization.GetString(protoData.descriptionStringId);
        }

        string CmptProcess(string text)
        {
            foreach (ItemAsset.Cmpt c in mListCmpt)
            {
                text = c.ProcessTooltip(text);
            }

            return text;
        }

        string SellPriceProcess(string text)
        {
            return text.Replace("[SP]", GetSellPrice().ToString());
        }

        string BuyPriceProcess(string text)
        {
            return text.Replace("[BP]", GetBuyPrice().ToString());
        }

        string GetCreationTooltip()
        {
            if (protoId >= CreationData.ObjectStartID)
            {
                CreationData data = CreationMgr.GetCreation(protoId);
                if (null != data)
                {
                    return data.AttrDescString(this);
                }
            }
            return null;
        }

        public override string GetTooltip()
        {
            string creationTooltip = GetCreationTooltip();
            if (null != creationTooltip)
            {
                return creationTooltip;
            }

            string text = GetOriginText();
            text = CmptProcess(text);
            text = SellPriceProcess(text);
            text = BuyPriceProcess(text);
            return text;
        }

        #endregion

        #region Bind
        public bool bind
        {
            get;
            set;
        }
        #endregion

        #region Factory

        static Dictionary<int, string[]> dicItemType;

        static void LoadItemType()
        {
            Mono.Data.SqliteClient.SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("item_class");

            while (reader.Read())
            {
                int id = System.Convert.ToInt32(reader.GetString(reader.GetOrdinal("id")));
                string cmpts = reader.GetString(reader.GetOrdinal("classname"));
                dicItemType[id] = cmpts.Split(',');
            }
        }

        static ItemObject Create(int protoId)
        {
            if (CreationData.ObjectStartID <= protoId)
            {
                SteamWorkShop.GetCreateionHead(protoId);
            }
            ItemProto prototypeData = ItemProto.Mgr.Instance.Get(protoId);
            if (null == prototypeData)
            {
                Debug.LogError("cant find prorotype data by prototype id:" + protoId);
                return null;
            }

            int typeId = prototypeData.itemClassId;

            if (null == dicItemType)
            {
                dicItemType = new Dictionary<int, string[]>(50);
                LoadItemType();
            }

            ItemObject item = new ItemObject(protoId);

            if (!dicItemType.ContainsKey(typeId))
            {
                //Debug.LogWarning("cant find type id:" + typeId + ", item protoId:" + protoId+", it will have no cmpt");
                return item;
            }

            string[] cmptList = dicItemType[typeId];

            foreach (string cmptName in cmptList)
            {
                try
                {
                    if (string.IsNullOrEmpty(cmptName) || cmptName == "0")
                    {
                        continue;
                    }

                    System.Type cmptType = System.Type.GetType("ItemAsset." + cmptName);

                    Cmpt c = System.Activator.CreateInstance(cmptType) as Cmpt;

                    item.Add(c);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("create failed, item:" + protoId + " cmpt:" + cmptName);
                    Debug.LogWarning(ex);
                }
            }

            item.Init();
            return item;
        }

        public static ItemObject Create(int prototypeId, int instanceId)
        {
            ItemObject item = Create(prototypeId);
            if (null == item)
            {
                return null;
            }

            item.mInstanceId = instanceId;

            return item;
        }

        #endregion
    }

    public abstract class Cmpt
    {
        public ItemObject itemObj;

        public ItemProto protoData
        {
            get
            {
                return itemObj.protoData;
            }
        }

        #region Serialize
        public virtual byte[] Export()
        {
            return null;
        }

        public virtual void Export(BinaryWriter w)
        {
        }

        public virtual void Import(byte[] buff)
        {
        }

        public virtual void Import(BinaryReader r)
        {
        }
        #endregion

        public string GetTypeName()
        {
            return GetType().ToString();
        }

        public virtual string ProcessTooltip(string text)
        {
            return text;
        }

        public virtual void Init() { }
    }

    public class Bundle : Cmpt
    {
        public IEnumerable<ItemAsset.ItemObject> Extract()
        {
            ItemAsset.ItemProto.Bundle bundle = protoData.bundle;
            if (null == bundle)
            {
                return null;
            }

            List<MaterialItem> itemList = bundle.GetItems();
            if (null == itemList || itemList.Count == 0)
            {
                return null;
            }

            SlotList slotList = new SlotList(itemList.Count);

            foreach (MaterialItem s in itemList)
            {
                if (!slotList.CanAdd(s.protoId, s.count))
                {
                    slotList = SlotList.ResetCapacity(slotList, 2 * slotList.Count);
                }

                slotList.Add(s.protoId, s.count, true);
            }
            return slotList.ToList();
        }
    }

    public class ReplicatorFormula : Cmpt
    {
        public int[] formulaId
        {
            get
            {
                return itemObj.protoData.replicatorFormulaIds;
            }
        }

        public override string ProcessTooltip(string text)
        {
            Pathea.PeEntity entity = Pathea.PeCreature.Instance.mainPlayer;
            if (null != entity)
            {
                Pathea.ReplicatorCmpt r = entity.GetCmpt<Pathea.ReplicatorCmpt>();
                if (null == r.replicator.GetKnownFormula(formulaId[0]))//没学过
                {
                    return base.ProcessTooltip(text);
                }
                else//学过
                {
                    string _text = text + PELocalization.GetString(4000001);
                    return _text;
                }
            }
            return base.ProcessTooltip(text);
        }

        static string GetKnownScriptTooltip()
        {
            return PELocalization.GetString(4000001);

            //    EffSkill skill = EffSkill.s_tblEffSkills.Find(iterSkill1=>EffSkill.MatchId(iterSkill1,mItemGrid.prototypeData.m_SkillID));
            //    if(skill != null && skill.m_skillIdsGot != null)
            //    {
            //        Pathea.PeEntity entity = Pathea.PeCreature.Instance.mainPlayer;

            //        if (null != entity)
            //        {
            //            bool learnAllSkill = true;
            //            foreach (int id in skill.m_skillIdsGot)
            //            {
            //                Pathea.ReplicatorCmpt r = entity.GetCmpt<Pathea.ReplicatorCmpt>();

            //                if (null == r.replicator.GetKnownFormula(id))
            //                //if(!PlayerFactory.mMainPlayer.m_skillBook.m_mergeSkillIDs.Contains(id))
            //                {
            //                    learnAllSkill = false;
            //                    break;
            //                }
            //            }
            //            if (learnAllSkill)
            //            {
            //                tipStr += PELocalization.GetString(4000001);
            //            }
            //        }
            //    }
        }
    }

    public class Property : Cmpt
    {
        float mFactor = 1f;

        public int buffId
        {
            get
            {
                return protoData.buffId;
            }
        }

        public int skillId
        {
            get
            {
                return protoData.skillId;
            }
        }

        public void SetFactor(float factor)
        {
            mFactor = factor;
        }

        #region Property

        public float GetProperty(Pathea.AttribType property)
        {
			if(property == Pathea.AttribType.ResRange)
				GetRawProperty(property);
            return GetRawProperty(property) * mFactor;
        }

        public float GetRawProperty(Pathea.AttribType property)
        {
            // protoData.propertyList = null ? 
            return protoData.propertyList.GetProperty(property);
        }

        #endregion

        #region Buff skill

        public SkillSystem.SkInst StartSkSkill(SkillSystem.SkEntity skEntity)
        {
            List<int> indexList = new List<int>(10);
            List<float> valueList = new List<float>(10);
            foreach (ItemProto.PropertyList.PropertyValue p in protoData.propertyList)
            {
                indexList.Add((int)p.type);
                valueList.Add(p.value * mFactor);
            }

            return skEntity.StartSkill(skEntity, skillId, new SkillSystem.SkUseItemPara(indexList, valueList));
        }

        public SkillSystem.SkBuffInst CreateSkBuff(SkillSystem.SkEntity skEntity)
        {
            List<int> indexList = new List<int>(10);
            List<float> valueList = new List<float>(10);
            foreach (ItemProto.PropertyList.PropertyValue p in protoData.propertyList)
            {
                indexList.Add((int)p.type);
                valueList.Add(p.value * mFactor);
            }

            return SkillSystem.SkEntity.MountBuff(skEntity, buffId, indexList, valueList);
        }

        public bool DestroySkBuff(SkillSystem.SkEntity skEntity)
        {
            SkillSystem.SkEntity.UnmountBuff(skEntity, buffId);
            return true;
        }
        #endregion

        public override string ProcessTooltip(string text)
        {
            foreach (ItemProto.PropertyList.PropertyValue v in protoData.propertyList)
            {
                string replaceName = "$" + (int)v.type + "$";
                float valueF = GetProperty(v.type);//v.value;

                if (v.type == Pathea.AttribType.ShieldMeleeProtect)
                {
                    valueF *= 100f;
                }

                text = text.Replace(replaceName, ((int)valueF).ToString());
            }

            return text;
        }
    }

    public class Strengthen : Cmpt
    {
        Property mProperty;
        Durability mDurability;
        LifeLimit mLifeLimit;

        int mStrengthenTime;
        public int strengthenTime
        {
            get
            {
                return mStrengthenTime;
            }
        }

        public override void Init()
        {
            base.Init();
            mProperty = itemObj.GetCmpt<Property>();

            mDurability = itemObj.GetCmpt<Durability>();

            mLifeLimit = itemObj.GetCmpt<LifeLimit>();

            ApplyStrengthenTime();
        }

        void ApplyStrengthenTime()
        {
            if (null != mProperty)
            {
                mProperty.SetFactor(GetStrengthFactor(mStrengthenTime));
            }

            if (null != mDurability)
            {
                mDurability.SetMax(mDurability.GetRawMax() * GetStrengthFactor(mStrengthenTime));
            }

            if (null != mLifeLimit)
            {
                mLifeLimit.SetMax(mLifeLimit.GetRawMax() * GetStrengthFactor(mStrengthenTime));
            }
        }

        public void LevelUp()
        {
            mStrengthenTime++;

            ApplyStrengthenTime();
        }

        public float GetStrengthFactor()
        {
            return GetStrengthFactor(mStrengthenTime);
        }

        public static float GetStrengthFactor(int time)
        {
            return 1.2f - 0.2f / (1 << time);
        }

        public float GetNextLevelProperty(Pathea.AttribType pro)
        {
            return GetPropertyByStrengthTime(pro, strengthenTime + 1);
        }

        public float GetCurLevelProperty(Pathea.AttribType pro)
        {
            return GetPropertyByStrengthTime(pro, strengthenTime);
        }

        float GetPropertyByStrengthTime(Pathea.AttribType pro, int time)
        {
            float value = 0f;
            if (null != mProperty)
            {
                value = mProperty.GetRawProperty(pro);
            }

            float factor = Strengthen.GetStrengthFactor(time);

            return value * factor;
        }

        #region tooltip

        public float GetNextMaxDurability()
        {
            if (mDurability == null)
            {
                return -1f;
            }

            return Strengthen.GetStrengthFactor(mStrengthenTime + 1) * mDurability.GetRawMax();
        }

        public float GetNextMaxLife()
        {
            if (mLifeLimit == null)
            {
                return -1f;
            }

            return Strengthen.GetStrengthFactor(mStrengthenTime + 1) * mLifeLimit.GetRawMax();
        }

        public float GetCurMaxDurability()
        {
            if (mDurability == null)
            {
                return -1f;
            }

            return mDurability.valueMax;
        }

        public float GetCurMaxLife()
        {
            if (mLifeLimit == null)
            {
                return -1f;
            }

            return mLifeLimit.valueMax;
        }

        #endregion
        #region Serialize
        public override byte[] Export()
        {
            return PETools.Serialize.Export(delegate(BinaryWriter w)
            {
                w.Write((int)mStrengthenTime);
            }, 20);
        }

        public override void Export(BinaryWriter w)
        {
            base.Export(w);
            BufferHelper.Serialize(w, mStrengthenTime);
        }

        public override void Import(byte[] buff)
        {
            PETools.Serialize.Import(buff, delegate(BinaryReader r)
            {
                mStrengthenTime = r.ReadInt32();
				ApplyStrengthenTime();
            });
        }

        public override void Import(BinaryReader r)
        {
            base.Import(r);
			mStrengthenTime = BufferHelper.ReadInt32(r);
			ApplyStrengthenTime();
        }
        #endregion

        public MaterialItem[] GetMaterialItems()
        {
            if (protoData.strengthenMaterialList != null)
                return protoData.strengthenMaterialList.ToArray();
            else
                return null;

            //List<MaterialItem> items = new List<MaterialItem>(protoData.mStrengthenRequireList.Count);

            //foreach (KeyValuePair<int, int> kvp in protoData.mStrengthenRequireList)
            //{
            //    items.Add(new MaterialItem() { protoId = kvp.Key, count = kvp.Value});
            //}

            //return items.ToArray();
        }
    }

    public abstract class OneFloat : Cmpt
    {
        protected PeFloatRangeNum mValue;

        float mMaxValue = -1f;
        bool mInit = false;

        //public override void Init()
        //{
        //    base.Init();

        //    if (false == mInit)
        //    {
        //        mInit = true;
        //        maxValue = GetRawMax();
        //    }
        //}

        public void SetMax(float v)
        {
            mInit = true;
            mMaxValue = v;

            CreateValue();
        }

        public abstract float GetRawMax();

        public float valueMax
        {
            get
            {
                if (false == mInit)
                {
                    mInit = true;
                    mMaxValue = GetRawMax();
                }

                return mMaxValue;
            }
        }

        public PeFloatRangeNum floatValue
        {
            get
            {
                if (mValue == null)
                {
                    CreateValue();
                }
                return mValue;
            }
        }

        protected void CreateValue()
        {
            float initLife = valueMax;
            if (null != mValue)
            {
                initLife = mValue.current;
            }

            mValue = new PeFloatRangeNum(initLife, 0f, valueMax);
        }

        #region Serialize
        public override byte[] Export()
        {
            return PETools.Serialize.Export(delegate(BinaryWriter w)
            {
                w.Write((float)floatValue.current);
            }, 10);
        }

        public override void Export(BinaryWriter w)
        {
            base.Export(w);
            BufferHelper.Serialize(w, floatValue.current);
        }

        public override void Import(byte[] buff)
        {
            PETools.Serialize.Import(buff, delegate(BinaryReader r)
            {
                mValue = new PeFloatRangeNum(r.ReadSingle(), 0f, valueMax);
            });
        }

        public override void Import(BinaryReader r)
        {
            base.Import(r);
            float cur = BufferHelper.ReadSingle(r);
            mValue = new PeFloatRangeNum(cur, 0f, valueMax);
        }
        #endregion
    }

    public class Energy : OneFloat
    {
        public override float GetRawMax()
        {
            return itemObj.protoData.engergyMax;
        }

        public PeFloatRangeNum energy
        {
            get
            {
                return floatValue;
            }
        }

        public override string ProcessTooltip(string text)
        {
            string t = base.ProcessTooltip(text);
            t = t.Replace("$powerMax$", ((int)valueMax).ToString());
            t = t.Replace("$100000006$", ((int)energy.current).ToString());
            return t;
        }
    }


    public class EnergySmall : Energy
    {

    }

    public class LifeLimit : OneFloat
    {
        public override float GetRawMax()
        {
            return itemObj.protoData.propertyList.GetProperty(Pathea.AttribType.HpMax);
        }

        //public string GetInfo()
        //{
        //    return lifePoint.current.ToString() + "   ( [00BBFF] + " + Mathf.RoundToInt(lifePoint.ExpendValue) + "[ffffff] )";
        //}

        //public string GetRepairInfo()
        //{
        //    return Mathf.FloorToInt(lifePoint.current).ToString() + " ([00BBFF] + " + Mathf.FloorToInt(lifePoint.ExpendValue).ToString() + "[ffffff])";
        //}

        public PeFloatRangeNum lifePoint
        {
            get
            {
                return floatValue;
            }
        }

        public bool ExpendLife(float deltaPoint)
        {
            return lifePoint.Change(-deltaPoint);
        }

        public virtual void Revive()
        {
            lifePoint.SetToMax();
        }

        public List<MaterialItem> GetRepairRequirements()
        {
            if (itemObj.protoId < CreationData.ObjectStartID)
            {
                return null;
            }

            return protoData.repairMaterialList;

            //List<MaterialItem> tmpList = new List<MaterialItem>();

            //foreach (KeyValuePair<int, int> kvp in protoData.mRepairRequireList)//repairrequireList.
            //{
            //    if (kvp.Key > 30200000 && kvp.Key < 30300000)
            //    {
            //        tmpList.Add(new MaterialItem(){protoId = kvp.Key, count = kvp.Value});
            //    }
            //}

            //return tmpList;
        }

        public override string ProcessTooltip(string text)
        {
            string t = base.ProcessTooltip(text);

            t = t.Replace("$" + (int)Pathea.AttribType.Hp + "$", ((int)lifePoint.current).ToString());
            t = t.Replace("$" + (int)Pathea.AttribType.HpMax + "$", ((int)valueMax).ToString());
            return t;
        }
    }

    public class Durability : OneFloat
    {
        public override float GetRawMax()
        {
            return protoData.durabilityMax;
        }

        public PeFloatRangeNum value
        {
            get
            {
                return floatValue;
            }
        }

        public bool Expend(float deltaTime)
        {
            return floatValue.Change(-deltaTime);
        }

        public List<MaterialItem> GetRepairRequirements()
        {
            //if (itemObj.protoId >= CreationData.ObjectStartID)
            //{
            //    return null;
            //}

            return protoData.repairMaterialList;
        }

        public override string ProcessTooltip(string text)
        {
            string t = base.ProcessTooltip(text);
            t = t.Replace("$100000000$", (Mathf.CeilToInt(value.current / 100f)).ToString());
            t = t.Replace("$durabilityMax$", (Mathf.CeilToInt(valueMax / 100f)).ToString());
            return t;
        }

        //#region Serialize
        //public override byte[] Export()
        //{
        //    return PETools.Serialize.Export(delegate(BinaryWriter w)
        //    {
        //        w.Write((float)floatValue.current);
        //        w.Write((float)valueMax);
        //    }, 10);
        //}

        //public override void Import(byte[] buff)
        //{
        //    PETools.Serialize.Import(buff, delegate(BinaryReader r)
        //    {
        //        float valueCurrent = r.ReadSingle();
        //        float valueMax = r.ReadSingle();
        //        SetMax(valueMax);
        //        value.current = valueCurrent;
        //    });
        //}
        //#endregion
    }

    public class Repair : Cmpt
    {
        LifeLimit mLifeLimit;
        Durability mDurability;

        public override void Init()
        {
            base.Init();

            mLifeLimit = itemObj.GetCmpt<LifeLimit>();
            mDurability = itemObj.GetCmpt<Durability>();

            itemObj.SetPriceFactor(GetValue());
        }

		static void AddRequirements(ref List<MaterialItem> retRequirements, List<MaterialItem> baseRequirements, float factor)
        {
			if (factor - 1f > PETools.PEMath.Epsilon || factor < PETools.PEMath.Epsilon)
            {
                return;
            }

            List<MaterialItem> resList = baseRequirements;
            if (null == resList)
            {
                return;
            }

			foreach (MaterialItem item in resList)
            {
				retRequirements.Add(new MaterialItem()
                {
                    protoId = item.protoId,
                    count = Mathf.CeilToInt(item.count * factor)
                });
            }

			retRequirements.RemoveAll((item) =>
            {
                if (item.count <= 0)
                {
                    return true;
                }
                return false;
            });

            return;
        }

		public List<MaterialItem> GetRequirements()
        {
			List<MaterialItem> requiremens = new List<MaterialItem>(10);
            if (null != mLifeLimit)
            {
				AddRequirements(ref requiremens,
                            mLifeLimit.GetRepairRequirements()
                            , 1f - mLifeLimit.lifePoint.percent
                    );
            }

            if (null != mDurability)
            {
				AddRequirements(ref requiremens,
                            mDurability.GetRepairRequirements()
                            , 1f - mDurability.value.percent
                    );
            }
			return requiremens;
        }

        public PeFloatRangeNum GetValue()
        {
            if (null != mDurability)
            {
                return mDurability.value;
            }

            if (null != mLifeLimit)
            {
                return mLifeLimit.lifePoint;
            }

            return null;
        }

        public void Do()
        {
            if (null != mLifeLimit)
            {
                mLifeLimit.lifePoint.SetToMax();
            }

            if (null != mDurability)
            {
                mDurability.value.SetToMax();
            }
        }
    }

    public class Consume : Cmpt
    {
        public SkillSystem.SkInst StartSkSkill(SkillSystem.SkEntity skEntity)
        {
            ItemAsset.Property property = itemObj.GetCmpt<ItemAsset.Property>();
            if (null == property)
            {
                return null;
            }

            return property.StartSkSkill(skEntity);
        }
    }

    public class Instantiate : Cmpt
    {
        public string viewResPath
        {
            get
            {
                return protoData.resourcePath;
            }
        }

        public string logicResPath
        {
            get
            {
                return protoData.resourcePath1;
            }
        }

        public virtual GameObject CreateViewGameObj(System.Action<Transform> initTransform)
        {
            return CreateGameObj(viewResPath, initTransform);
        }

        public virtual GameObject CreateLogicGameObj(System.Action<Transform> initTransform)
        {
            return CreateGameObj(logicResPath, initTransform);
        }

        public virtual GameObject CreateDraggingGameObj(System.Action<Transform> initTransform)
        {
            return CreateViewGameObj(initTransform);
        }

        protected static GameObject CreateGameObj(string path, System.Action<Transform> initTransform)
        {
            if (!string.IsNullOrEmpty(path))
            {
                Object prefab = AssetsLoader.Instance.LoadPrefabImm(path);
                if (null != prefab)
                {
                    var go = Object.Instantiate(prefab) as GameObject;
                    go.name = prefab.name;
                    if (initTransform != null) initTransform(go.transform);
                    return go;
                }
            }

            return null;
        }
    }

    public class InstantiateCreation : Instantiate
    {
        public override GameObject CreateViewGameObj(System.Action<Transform> initTransform)
        {
            return CreationMgr.InstantiateCreation(itemObj.protoId, itemObj.instanceId, false, initTransform);
            //return base.CreateViewGameObj();
        }

        public override GameObject CreateLogicGameObj(System.Action<Transform> initTransform)
        {
            GameObject go = CreationMgr.InstantiateCreation(itemObj.protoId, itemObj.instanceId, true, initTransform);

            return go;
            //return base.CreateLogicGameObj();
        }

        public override GameObject CreateDraggingGameObj(System.Action<Transform> initTransform)
        {
            return CreationMgr.InstantiateCreation(itemObj.protoId, itemObj.instanceId, false, initTransform);
        }
    }

    //public class InstantiateTower : Instantiate
    //{
    //}

    public class Train : Cmpt
    {

    }

    public class Block : Cmpt
    {

    }

    public class Drag : Cmpt
    {
        Instantiate mInstantiateGameObj;

        public override void Init()
        {
            base.Init();
            mInstantiateGameObj = itemObj.GetCmpt<Instantiate>();
            if (mInstantiateGameObj == null)
            {
                Debug.LogError("item:" + itemObj.protoId + ", drag need InstantiateGameObj");
            }
        }

        public virtual GameObject CreateLogicGameObject(System.Action<Transform> initTransform)
        {
            if (mInstantiateGameObj == null)
            {
                return null;
            }

            return mInstantiateGameObj.CreateLogicGameObj(initTransform);
        }

        public virtual GameObject CreateViewGameObject(System.Action<Transform> initTransform)
        {
            if (mInstantiateGameObj == null)
            {
                return null;
            }

            return mInstantiateGameObj.CreateViewGameObj(initTransform);
        }

        public virtual GameObject CreateDraggingGameObject(System.Action<Transform> initTransform)
        {
            return mInstantiateGameObj.CreateDraggingGameObj(initTransform);
        }
    }

    public class Equip : Cmpt
    {
        Instantiate mInstantiateGameObj;

        public override void Init()
        {
            base.Init();

            mInstantiateGameObj = itemObj.GetCmpt<Instantiate>();
            if (mInstantiateGameObj == null)
            {
                Debug.LogError("item:" + itemObj.protoId + ", Equip need InstantiateGameObj");
            }
        }

        public EquipType equipType		{	get{	return protoData.equipType;	}        }
        public Pathea.PeSex sex			{	get{	return protoData.equipSex;	}        }
        public int equipPos				{	get{	return protoData.equipPos;	}        }
        public int replacePos			{	get{    return protoData.equipReplacePos; }  }
        protected float durabilityFactor{	get{	return protoData.durabilityFactor;}  }

        protected bool DurabilityExhaust()
        {
            Durability d = itemObj.GetCmpt<Durability>();

            if (d == null)
            {
                return false;
            }

            return d.value.IsCurrentMin();
        }

        public bool AddBuff(SkillSystem.SkEntity skEntity)
        {
            if (DurabilityExhaust())
            {
                return false;
            }

            ItemAsset.Property property = itemObj.GetCmpt<ItemAsset.Property>();
            if (null == property)
            {
                //Debug.LogError("item proto:" + itemObj.protoId + "' equip cmpt need Property cmpt");
                return false;
            }

            return null != property.CreateSkBuff(skEntity);
        }

        public bool RemoveBuff(SkillSystem.SkEntity skEntity)
        {
            ItemAsset.Property property = itemObj.GetCmpt<ItemAsset.Property>();
            if (null == property)
            {
                return false;
            }

            return property.DestroySkBuff(skEntity);
        }

        public bool AddMotionBuff(SkillSystem.SkEntity skEntity)
        {
            return AddBuff(skEntity);
        }

        public bool RemoveMotionBuff(SkillSystem.SkEntity skEntity)
        {
            return RemoveBuff(skEntity);
        }

        public GameObject CreateGameObj()
        {
            if (mInstantiateGameObj == null)
            {
                return null;
            }
//			if (!string.IsNullOrEmpty (mInstantiateGameObj.viewResPath)) { // for debug 
//				Debug.Log ("Equip.Load:" + mInstantiateGameObj.viewResPath);
//			}
            return mInstantiateGameObj.CreateViewGameObj(null);
        }

        public GameObject CreateLogicObj()
        {
            if (mInstantiateGameObj == null)
            {
                return null;
            }

            return mInstantiateGameObj.CreateLogicGameObj(null);
        }

        public virtual bool ExpendAttackDurability(SkillSystem.SkEntity skEntity)
        {
            return false;
        }

        public virtual bool ExpendDefenceDurability(SkillSystem.SkEntity skEntity, float hpChange)
        {
            return false;
        }

        protected bool ChangeDurability(SkillSystem.SkEntity skEntity, float v)
        {
            Durability d = itemObj.GetCmpt<Durability>();

            if (d == null)
            {
                return false;
            }

            d.value.Change(v);

            if (d.value.IsCurrentMin())
            {
                RemoveBuff(skEntity);
            }

            return true;
        }

        public override string ProcessTooltip(string text)
        {
            string t = base.ProcessTooltip(text);

            t = t.Replace("$durabilityDec$", ((int)durabilityFactor).ToString());

            return t;
        }
    }

    public class EquipAttack : Equip
    {
        public override bool ExpendAttackDurability(SkillSystem.SkEntity skEntity)
        {
            return ChangeDurability(skEntity, -durabilityFactor);
        }
    }

    public class EquipDefence : Equip
    {
        public override bool ExpendDefenceDurability(SkillSystem.SkEntity skEntity, float hpChange)
        {
            float d = hpChange * durabilityFactor;

            d = Mathf.Clamp(d, 0f, Mathf.Infinity);

            return ChangeDurability(skEntity, -d);
        }
    }

    public class JetPkg : Cmpt
    {
        public float energy;

        #region Serialize
        public override byte[] Export()
        {
            return PETools.Serialize.Export(delegate(BinaryWriter w)
            {
                w.Write(energy);
            }, 800);
        }

        public override void Export(BinaryWriter w)
        {
            base.Export(w);
            BufferHelper.Serialize(w, energy);
        }

        public override void Import(byte[] buff)
        {
            PETools.Serialize.Import(buff, delegate(BinaryReader r)
            {
                energy = r.ReadSingle();
            });
        }

        public override void Import(BinaryReader r)
        {
            base.Import(r);
            BufferHelper.ReadSingle(r);
        }
        #endregion
    }

    public class Arrow : Cmpt
    {
        public int index;

        #region Serialize
        public override byte[] Export()
        {
            return PETools.Serialize.Export(delegate(BinaryWriter w)
            {
                w.Write((int)index);
            }, 800);
        }

        public override void Export(BinaryWriter w)
        {
            base.Export(w);
            BufferHelper.Serialize(w, index);
        }

        public override void Import(byte[] buff)
        {
            PETools.Serialize.Import(buff, delegate(BinaryReader r)
            {
                index = r.ReadInt32();
            });
        }

        public override void Import(BinaryReader r)
        {
            base.Import(r);
            index = BufferHelper.ReadInt32(r);
        }
        #endregion
    }

    public class GunAmmo : Cmpt
    {
        public int index = 0;
        public int count = -1;

        #region Serialize
        public override byte[] Export()
        {
            return PETools.Serialize.Export(delegate(BinaryWriter w)
            {
                w.Write((int)index);
                w.Write((int)count);
            }, 10);
        }

        public override void Export(BinaryWriter w)
        {
            base.Export(w);

            BufferHelper.Serialize(w, index);
            BufferHelper.Serialize(w, count);
        }

        public override void Import(byte[] buff)
        {
            PETools.Serialize.Import(buff, delegate(BinaryReader r)
            {
                index = r.ReadInt32();
                count = r.ReadInt32();
            });
        }

        public override void Import(BinaryReader r)
        {
            base.Import(r);

            index = BufferHelper.ReadInt32(r);
            count = BufferHelper.ReadInt32(r);
        }
        #endregion
    }

    public class Tower : Cmpt
    {
        int mCurCostValue = -1;
        public int curCostValue
        {
            get
            {
                if (mCurCostValue == -1)
                {
                    if (costType == Pathea.ECostType.Item)
                    {
                        mCurCostValue = bulletData.bulletMax;
                    }
                    else if (costType == Pathea.ECostType.Energy)
                    {
                        mCurCostValue = bulletData.energyMax;
                    }
                }

                return mCurCostValue;
            }

            set
            {
                mCurCostValue = value;
            }
        }

        public int id
        {
            get
            {
                return protoData.towerEntityId;
            }
        }

        public Pathea.ECostType costType
        {
            get
            {
                return (Pathea.ECostType)bulletData.bulletType;
            }
        }

        Pathea.TowerProtoDb.BulletData mBulletData;
        public Pathea.TowerProtoDb.BulletData bulletData
        {
            get
            {
                if (mBulletData == null)
                {
                    Pathea.TowerProtoDb.Item item = Pathea.TowerProtoDb.Get(id);
                    if (item != null)
                    {
                        mBulletData = item.bulletData;
                    }
                }

                return mBulletData;
            }
        }

        public override byte[] Export()
        {
            return PETools.Serialize.Export(delegate(BinaryWriter w)
            {
                w.Write((int)mCurCostValue);
            }, 10);
        }

        public override void Export(BinaryWriter w)
        {
            base.Export(w);
            BufferHelper.Serialize(w, mCurCostValue);
        }

        public override void Import(byte[] buff)
        {
            PETools.Serialize.Import(buff, delegate(BinaryReader r)
            {
                mCurCostValue = r.ReadInt32();
            });
        }

        public override void Import(BinaryReader r)
        {
            base.Import(r);
            mCurCostValue = BufferHelper.ReadInt32(r);
        }

        public override string ProcessTooltip(string text)
        {
            string t = base.ProcessTooltip(text);

            t = t.Replace("$powerMax$", bulletData.energyMax.ToString());
            t = t.Replace("$ammoMax$", bulletData.bulletMax.ToString());

            t = t.Replace("$100000006$", curCostValue.ToString());
            t = t.Replace("$100000007$", curCostValue.ToString());

            return t;
        }
    }

    public abstract class Recycle : Cmpt
    {
        LifeLimit mLifeLimit;
        Durability mDurability;

        public override void Init()
        {
            base.Init();

            mLifeLimit = itemObj.GetCmpt<LifeLimit>();
            mDurability = itemObj.GetCmpt<Durability>();
        }

        public PeFloatRangeNum GetCurrent()
        {
            if (null != mDurability)
            {
                return mDurability.value;
            }

            if (null != mLifeLimit)
            {
                return mLifeLimit.lifePoint;
            }

            return null;
        }

        float GetFactor()
        {
            PeFloatRangeNum v = GetCurrent();
            if (null != v)
            {
                return v.percent;
            }

            return 1f;
        }

        public abstract List<MaterialItem> GetResItemList();

        public virtual MaterialItem[] GetRecycleItems()
        {
            List<MaterialItem> resList = GetResItemList();

            if (null == resList)
            {
                return null;
            }

            List<MaterialItem> tmpList = new List<MaterialItem>(10);

            foreach (MaterialItem item in resList)
            {
                tmpList.Add(new MaterialItem()
                {
                    protoId = item.protoId,
                    count = Mathf.CeilToInt(item.count * GetFactor())
                });
            }

            tmpList.RemoveAll((item) =>
            {
                if (item.count <= 0)
                {
                    return true;
                }
                return false;
            });

            return tmpList.ToArray();
        }
    }

    public class RecycleReplicate : Recycle
    {
        public override List<MaterialItem> GetResItemList()
        {
            if (itemObj.protoId >= CreationData.ObjectStartID)
            {
                return null;
            }

            Pathea.Replicator.Formula ms = Pathea.Replicator.Formula.Mgr.Instance.FindByProductId(itemObj.protoId);

            if (null == ms)
            {
                return null;
            }

            List<MaterialItem> tmpList = new List<MaterialItem>(10);

            foreach (Pathea.Replicator.Formula.Material msmi in ms.materials)
            {
                tmpList.Add(new MaterialItem() { protoId = msmi.itemId, count = msmi.itemCount });
            }

            return tmpList;
        }
    }

    public class RecycleCreation : Recycle
    {
        public override List<MaterialItem> GetResItemList()
        {
            if (itemObj.protoId < CreationData.ObjectStartID)
            {
                return null;
            }

            return protoData.repairMaterialList;

            //List<MaterialItem> tmpList = new List<MaterialItem>(10);

            //foreach (KeyValuePair<int, int> kvp in protoData.mRepairRequireList)
            //{
            //    tmpList.Add(new MaterialItem() { protoId = kvp.Key, count = kvp.Value });
            //}

            //return tmpList;
        }
    }

    public class Seed : Cmpt
    {
        public int type
        {
            get
            {
                return 0;
            }
        }
    }

    public class MetalScan : Cmpt
    {
        public int[] metalIds
        {
            get
            {
                return itemObj.protoData.replicatorFormulaIds;
            }
        }

        public override string ProcessTooltip(string text)
        {
            Pathea.PeEntity entity = Pathea.PeCreature.Instance.mainPlayer;
            if (null != entity)
            {
                //Pathea.ReplicatorCmpt r = entity.GetCmpt<Pathea.ReplicatorCmpt>();
                if (!MetalScanData.HasMetal(metalIds[0]))//没学过
                {
                    return base.ProcessTooltip(text);
                }
                else//学过
                {
                    //string _text = text.Split(new[] { "\n[00FF00]" }, System.StringSplitOptions.RemoveEmptyEntries)[0] + PELocalization.GetString(4000001);
                    int _index = text.LastIndexOf("\\n");
                    string _text = text.Substring(0, _index);
                    _text += PELocalization.GetString(4000001);
                    return _text;
                }
            }
            return base.ProcessTooltip(text);
        }

    }
}