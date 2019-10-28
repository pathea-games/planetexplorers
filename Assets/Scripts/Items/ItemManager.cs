#define PLANET_EXPLORERS

using SkillAsset;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PeIcon
{
    //string mIconString;
    //Texture mIconTexture;
    public PeIcon(string iconString)
    {
        //mIconString = iconString;
        //mIconTexture = null;
    }
    public PeIcon(Texture iconTex)
    {
        //mIconTexture = iconTex;
        //mIconString = null;
    }
}

public class PeFloatRangeNum
{
    float mMin;
    float mMax;
    float mCurrent;

    public PeFloatRangeNum(float cur, float min, float max)
    {
        mMin = min;
		mMax = max;
		mCurrent = Mathf.Clamp(cur, mMin, mMax);
    }

    public float percent
    {
        get
        {
            return (mCurrent - mMin) / (mMax - mMin);
        }
    }

    public float current
    {
        get
        {
            return mCurrent;
        }
        set
        {
            mCurrent = Mathf.Clamp(value, mMin, mMax);
        }
    }

    public float ExpendValue
    {
        get
        {
            return mMax - current;
        }
    }
    public bool Change(float delta)
    {
		if(float.IsNaN(current)){
			current = mMin;
		}
        current = current + delta;

        if (current < mMin)
        {
            current = mMin;
            return false;
        }
        else if( current > mMax)
        {
            current = mMax;
            return false;
        }
        
        return true;
    }
	public bool ChangePercent(float perc)
	{
		if(float.IsNaN(current)){
			current = mMin;
		}
		current = current + (mMax-mMin)*perc;
		
		if (current < mMin)
		{
			current = mMin;
			return false;
		}
		else if( current > mMax)
		{
			current = mMax;
			return false;
		}
		return true;
	}
	
	public void SetToMax()
	{
		current = mMax;
	}
	
	public void SetToMin()
    {
        current = mMin;
    }

    public bool IsCurrentMax()
    {
        return Mathf.Abs(current - mMax) < PETools.PEMath.Epsilon;
    }
    public bool IsCurrentMin()
    {
        return Mathf.Abs(current - 0f) < PETools.PEMath.Epsilon;
    }
}

namespace ItemAsset
{	
	public class ItemSample
	{
        int mProtoId;
        int mStackCount;
        ItemProto mProtoData;

        public int protoId
        {
            get
            {
                return mProtoId;
            }
            set
            {
                mProtoId = value;
            }
        }

        public ItemProto protoData
        {
            get
            {
                if (null == mProtoData)
                {
                    mProtoData = ItemProto.Mgr.Instance.Get(protoId);
                }

                return mProtoData;
            }
        }

        public string nameText
        {
            get
            {
                if (null != protoData)
                {
//                    if (protoId >= CreationData.ObjectStartID)
//                    {
//                        return protoData.name;
//                    }
					return protoData.name;
                }
                return null;
            }
        }

        public virtual string GetTooltip()
        {
            return nameText;
        }
        //public string englishNameText
        //{
        //    get
        //    {
        //        if (null != protoData)
        //        {	
        //            return protoData.englishName;
        //        }
        //        return null;
        //    }
        //}

        public int stackCount
        {
            get
            {
                return mStackCount;
            }
            set
            {
                mStackCount = value;
            }
        }

        public ItemSample() { }
		public ItemSample(int itemId, int stackCount = 1)
		{
            mProtoId = itemId;
            mStackCount = stackCount;
		}
		//luwei
		bool mClick =false ;

		public bool Click
		{
			get
			{
				return mClick ;
			}
			set
			{
				mClick = value ;
			}
		}

		float mClickedTime = 0;
		public float ClickedTime 
		{
			get
			{
				return mClickedTime ;
			}
			set
			{
				mClickedTime = value ;
			}
		}

		public int GetCount()
		{
            return stackCount;			
		}

        public virtual int GetStackMax()
        {
            return protoData.maxStackNum;
        }

		public bool IncreaseStackCount (int num)
		{
            if (mStackCount + num > GetStackMax())
            {
                return false;
            }

			mStackCount += num;
			
			return false;
		}

        public bool SetStackCount(int num)
        {
            if (num > GetStackMax() || num < 0)
            {
                return false;
            }

            mStackCount = num;
            return true;
        }

		public bool DecreaseStackCount(int num)
		{
            if (num > mStackCount)
            {
                return false;
            }
			mStackCount -= num;
			return true;
		}

        public Texture2D iconTex
        {
            get
            {
                if (null == protoData)
                {
                    return null;
                }

                return protoData.iconTex;
            }
        }

        public string iconString0
        {
            get
            {
                if (protoData == null)
                {
                    Debug.LogError("protoData == null" + protoId);
                    return "protoData == null" + protoId;
                }
                if (protoData.icon == null)
                {

                    Debug.LogError("icon==null" + protoId);
                    return "icon==null" + protoId;
                }
                return protoData.icon[0];
            }
        }

        public string iconString1
        {
            get
            {
                return protoData.icon[1];
            }
        }

        public string iconString2
        {
            get
            {
                string str = protoData.icon[2];
                if ("0" != str)
                {
                    return str;
                }
                else
                {
                    return "Null";
                }
            }
        }

        public int level
        {
            get
            {
                return protoData.level;
            }
        }

        #region Serialize
		public virtual void Export(BinaryWriter w)
        {
            w.Write(mProtoId);
            w.Write(mStackCount);
        }

		public virtual void Export4Net(BinaryWriter w)
		{
			BufferHelper.Serialize(w, mProtoId);
			BufferHelper.Serialize(w, mStackCount);
		}

        public virtual void Import(byte[] buff)
        {
            PETools.Serialize.Import(buff, delegate(BinaryReader r)
            {
                mProtoId = r.ReadInt32();
                mStackCount = r.ReadInt32();
            });
        }

		public virtual void Import(BinaryReader r)
		{
			mProtoId = BufferHelper.ReadInt32(r);
			mStackCount = BufferHelper.ReadInt32(r);
		}
        #endregion

        #region ulink
        public static object Deserialize(uLink.BitStream stream, params object[] codecOptions)
        {
            byte[] buff = stream.ReadBytes();
			ItemSample itemObj = new ItemSample();
			PETools.Serialize.Import(buff, r => { itemObj.Import(r); });
            return itemObj;
        }

        public static void Serialize(uLink.BitStream stream, object value, params object[] codecOptions)
        {
            ItemSample itemObj = value as ItemSample;
			byte[] buff = PETools.Serialize.Export(w => { itemObj.Export4Net(w); });
            stream.WriteBytes(buff);
        }
        #endregion

        #region Drag

        public virtual bool CanDrag()
        {
            return "0" != protoData.icon[1];
        }
        #endregion
    }

    public class ItemMgr : Pathea.MonoLikeSingleton<ItemMgr>
	{
        Pathea.IdGenerator mIdGenerator = new Pathea.IdGenerator(0, 0, CreationData.ObjectStartID);
        Dictionary<int, ItemObject> mItems = new Dictionary<int, ItemObject>(100);

        //lz-2016.10.11 删除Item事件
        public System.Action<int> DestoryItemEvent;

        public bool Add(ItemObject item)
        {
            if (item == null)
            {
                return false;
            }

            if (mItems.ContainsKey(item.instanceId))
            {
                return false;
            }

            mItems.Add(item.instanceId, item);
            return true;
        }

        //public bool Update(ItemObject item)
        //{
        //    if (item == null)
        //    {
        //        return false;
        //    }

        //    if (!mItems.ContainsKey(item.instanceId))
        //    {
        //        return false;
        //    }

        //    return true;
        //}

        public ItemObject Get(int id)
		{
            if (mItems.ContainsKey(id))
            {
                return mItems[id];
            }
            return null;
		}

        public bool DestroyItem(int id)
        {
            ItemObject item = Get(id);
            if (null == item)
            {
                return false;
            }
            ExecDestoryItemEvent(id);
            return mItems.Remove(id);
        }

		public bool DestroyItem(ItemObject item)
		{
			if (null == item)
			{
				return false;
			}
            ExecDestoryItemEvent(item.instanceId);
            return mItems.Remove(item.instanceId);
		}

		public static bool IsCreationItem(int protoId)
        {
            if (protoId < CreationData.ObjectStartID)
            {
                return false;
            }

            return true;
        }

        public ItemObject CreateItem(int prototypeId)
        {
            int instanceId = 0;

            if (!IsCreationItem(prototypeId))
            {
                instanceId = mIdGenerator.Fetch();
            }
            else
            {
                instanceId = prototypeId;
            }

            ItemObject item = ItemObject.Create(prototypeId, instanceId);
            Add(item);

            return item;
        }
		
		public void Export(BinaryWriter w)
		{
            PETools.Serialize.WriteData(mIdGenerator.Export, w);

            w.Write((int)mItems.Count);
            foreach (ItemObject item in mItems.Values)
            {
				if(item != null){
					PETools.Serialize.WriteData(item.Serialize, w);
				} else {
					PETools.Serialize.WriteData(null, w);
				}
            }

            w.Write(KillNPC.ashBox_inScene);
		}
		
		public void Import(byte[] buffer)
		{
            PETools.Serialize.Import(buffer, (r)=>
            {
                mIdGenerator.Import(PETools.Serialize.ReadBytes(r));

                int count = r.ReadInt32();

                for (int i = 0; i < count; i++)
                {
                    ItemObject item = ItemObject.Deserialize(PETools.Serialize.ReadBytes(r));

                    Add(item);
                }
                //场景中骨灰盒npcID对应关系的读取
                if (Pathea.ArchiveMgr.Instance.GetCurArvhiveVersion() > Pathea.Archive.Header.Version_2)
                {
                    KillNPC.ashBox_inScene = r.ReadInt32();
                }
            });
            
		}

        void Clear()
        {
            mIdGenerator.Cur = 0;
            mItems.Clear();
        }
        /// <summary>执行删除Item事件</summary>
        void ExecDestoryItemEvent(int instanceId)
        {
            if (null != DestoryItemEvent)
            {
                DestoryItemEvent(instanceId);
            }
        }

		public override void OnDestroy()
        {
            base.OnDestroy();
            Clear();
        }
	}

	public class ItemSkillMar 
	{

		Dictionary <int ,ItemSample> mItems =new Dictionary<int, ItemSample>(100);
		
		float mFillcount = 0;

		public float Fillcount
		{
			get
			{
				return mFillcount ;
			}
			set
			{
				mFillcount = value ;
			}
		}
	
		public float GetFillcount(int id)
		{
			ItemSample item =Get(id);
			if(item == null )
				return 0;
			else 
				return mFillcount;
		}

		public ItemSample Get(int id)
		{
			if (mItems.ContainsKey(id))
			{
				return mItems[id];
			}
			return null;
		}

		public void AddSkillCD(ItemSample  Item)
		{
			if(null == Item )
				return ;
			if(Item.protoData.skillId == 0)
				return ;

			mItems[Item.protoId] = Item;

		}

		public bool DelateSkillCD(int protoId)
		{
			ItemSample item = Get(protoId);
			if (null == item)
			{
				return false;
			}
			
			return mItems.Remove(protoId);
		}

/*		bool mIsinskillcd = false ;
		public bool IsInskillCd
		{
			get
			{
				return mIsinskillcd ;
			}
			set
			{
				mIsinskillcd = value ;
			}
		}
*/
		public bool IsInSkillCD(int protoId)
		{
			ItemSample item = Get(protoId);

			if(item == null )
				return false ;
			else 
			   return true ;
		}





































	}

}