using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using ItemAsset;
using ItemAsset.PackageHelper;

namespace Pathea
{
    public class ReplicatorCmpt : PeCmpt, Replicator.IHandler
    {
        PlayerPackageCmpt mPackage;

        public Replicator replicator
        {
            get;
            private set;
        }

        public ReplicatorCmpt()
        {
            replicator = new Replicator(this);
        }

        public override void Start()
        {
			base.Start ();
			mPackage = Entity.GetCmpt<PlayerPackageCmpt>();
        }

        public override void Serialize(System.IO.BinaryWriter w)
        {
            replicator.Serialize(w);
        }

        public override void Deserialize(System.IO.BinaryReader r)
        {
            replicator.Deserialize(r);
        }

		public override void OnUpdate ()
		{
			if (RandomMapConfig.useSkillTree && null != Entity.skillTreeCmpt)
				replicator.needTimeScale = GameUI.Instance.mSkillWndCtrl._SkillMgr.CheckReduceTime(1f);
			replicator.UpdateReplicate();
		}

        int Replicator.IHandler.ItemCount(int itemId)
        {
            return mPackage.package.GetCount(itemId);            
        }

        bool Replicator.IHandler.DeleteItem(int itemId, int count)
        {
            return mPackage.Destory(itemId, count);
        }

        bool Replicator.IHandler.CreateItem(int itemId, int count)
        {
            return mPackage.Add(itemId, count);
        }

        bool Replicator.IHandler.HasEnoughPackage(int itemId, int count)
        {
            return mPackage.package.CanAdd(itemId, count);
        }
    }

    public class Replicator
    {
        public class Formula
        {
            public class Material
            {
                public int itemId;
                public int itemCount;
            }

            public int id;
            public int productItemId;
            public float timeNeed;
            public short m_productItemCount;	//number_each_product
            public List<Material> materials;
            public int workSpace;

            Formula() { }

            public class Mgr:Pathea.MonoLikeSingleton<Mgr>
            {
                protected override void OnInit()
                {
                    base.OnInit();
                    LoadData();
                }

                List<Formula> mList = new List<Formula>(50);
                void LoadData()
                {
                    Mono.Data.SqliteClient.SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("synthesis_skill");
                    while (reader.Read())
                    {
                        Formula skill = new Formula();

                        skill.id = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ID")));
                        skill.productItemId = Convert.ToInt32(reader.GetString(reader.GetOrdinal("object_ID")));
                        skill.m_productItemCount = Convert.ToInt16(reader.GetString(reader.GetOrdinal("num_each_product")));
                        skill.timeNeed = Convert.ToSingle(reader.GetString(reader.GetOrdinal("need_time")));
                        
                        int idxMat = reader.GetOrdinal("material1");
                        skill.materials = new List<Material>();
                        for (int i = 0; i < 6; i++)
                        {
                            int count = Convert.ToInt32(reader.GetString(idxMat + 1 + i * 2));
                            if (count <= 0)
                                break;
                            skill.materials.Add(new Material() { itemId = Convert.ToInt32(reader.GetString(idxMat + i * 2)), itemCount = count });
                        }

                        skill.workSpace = Convert.ToInt32(reader.GetString(reader.GetOrdinal("workspace")));

                        mList.Add(skill);
                    }
                }

                bool MatchId(Formula iter, int id)
                {
                    return iter.id == id;
                }

                public Formula FindByProductId(int productId)
                {
                    return mList.Find(
                        delegate(Formula hh)
                        {
                            return hh.productItemId == productId;
                        });
                }

                //lz-2016.08.08 通过productId找到所有这个合成这个product的配方
                public List<Formula> FindAllByProDuctID(int productId)
                {
                    return mList.FindAll(item => item.productItemId == productId);
                }

                public Formula Find(int id)
                {
                    return mList.Find(iterSkill1 => MatchId(iterSkill1, id));
                }

                public string GetName(int id)
                {
                    Formula data = Find(id);
                    if (data == null)
                    {
                        return "";
                    }
                    return ItemAsset.ItemProto.GetName(data.productItemId);
                }

                //public string GetIconName(int id)
                //{
                //    Formula data = Find(id);
                //    if (data == null)
                //    {
                //        return null;
                //    }

                //    return ItemAsset.ItemProto.GetIconName(data.productItemId);
                //}
            }
        }

        public interface IHandler
        {
            int ItemCount(int itemId);
            bool DeleteItem(int itemId, int count);
            bool CreateItem(int itemId, int count);
            bool HasEnoughPackage(int itemId, int count);
        }

        public class KnownFormula
        {
			public int id{ get{ return _id; } }
			public bool flag;
			int _id;
			Formula _formula = null;
			public KnownFormula(int formulaId, bool formulaFlag)
			{
				_id = formulaId;
				flag = formulaFlag;
				_formula = Formula.Mgr.Instance.Find(_id);
			}
            public Formula Get()
            {
				return _formula;
            }
        }

		public class RunningReplicate
		{
			public int formulaID;
			public int requestCount;
			public int leftCount;
			public int finishCount;
			public float runningTime;
			public float lastReciveTime;
			
			Replicator.Formula m_Formula;
			public Replicator.Formula formula{ get { if(null == m_Formula) m_Formula = Replicator.Formula.Mgr.Instance.Find(formulaID); return m_Formula; } }
		}

		public float needTimeScale = 1f;

        IHandler mHandler;
		List<KnownFormula> mForumlaList = new List<KnownFormula>(5);
		RunningReplicate mRunningReplicate;

		const float ReciveInterval = 5f;

		public Action onReplicateEnd;

		public RunningReplicate runningReplicate{ get { return mRunningReplicate; } }

        public Replicator(IHandler handler)
        {
            mHandler = handler;
        }

        public class EventArg : PeEvent.EventArg
        {
            public int formulaId;
        }

        PeEvent.Event<EventArg> mEventor = new PeEvent.Event<EventArg>();

        public PeEvent.Event<EventArg> eventor
        {
            get
            {
                return mEventor;
            }
        }

        public bool AddFormula(int formulaId)
        {
            KnownFormula kf = GetKnownFormula(formulaId);
            if (null != kf)
            {
                return false;
            }

            mForumlaList.Add(new KnownFormula(formulaId, true));

            eventor.Dispatch(new EventArg() { formulaId = formulaId });

            return true;
        }

        public bool HasEnoughPackage(int itemId, int count)
        {
            return mHandler.HasEnoughPackage(itemId, count);
        }

        public int GetItemCount(int itemId)
        {
            if (null == mHandler)
            {
                return 0;
            }
            return mHandler.ItemCount(itemId);
        }

        public int MaxProductCount(int formulaId)
        {
            Formula formula = Formula.Mgr.Instance.Find(formulaId);
			return MaxProductCount(formula);
		}
		
		int MaxProductCount(Formula formula)
		{
			if(null == mHandler || null == formula) return 0;
			
			int maxCount = 9999;
			foreach (Formula.Material m in formula.materials)
			{
				int count = mHandler.ItemCount(m.itemId)/m.itemCount;
				if (count < maxCount)
				{
					maxCount = count;
				}
			}
			
			return maxCount;
		}
		
		public int MinProductCount(int formulaId)
		{
			if (MaxProductCount(formulaId) <= 0)
			{
				return 0;
			}
			
			return 1;
        }

        bool DeleteMaterial(Formula formula, int count)
        {
            foreach (Formula.Material m in formula.materials)
            {
                if (!mHandler.DeleteItem(m.itemId, m.itemCount * count))
                {
                    return false;
                }
            }

            return true;
        }

        public bool Run(int formulaId, int count)
        {
            Formula formula = Formula.Mgr.Instance.Find(formulaId);
            if (null == formula)
            {
                Debug.LogError("no replicate formula with id:"+formulaId);
                return false;
            }

            if (null == mHandler)
            {
                Debug.LogError("no handler to handle item");
                return false;
            }

            if (MaxProductCount(formulaId) < count)
            {
                Debug.LogError("not engugh material");
                return false;
            }

            if (!DeleteMaterial(formula, count))
            {
                Debug.LogError("delete material failed");
                return false;
            }

            if (!mHandler.CreateItem(formula.productItemId, formula.m_productItemCount * count))
            {
                Debug.LogError("create item :" + formula.productItemId + ", count:" + formula.m_productItemCount * count + "failed:");
                return false;
            }

            return true;
        }

        public int[] GetProductItemIds()
        {
            List<int> list = new List<int>(10);

            foreach (KnownFormula kf in knowFormulas)
            {
                Formula f = kf.Get();

                if (!list.Contains(f.productItemId))
                {
                    list.Add(f.productItemId);
                }
            }

            return list.ToArray();
        }

        public KnownFormula[] GetKnowFormulasByProductItemId(int productItemId)
        {
            List<KnownFormula> list = new List<KnownFormula>(1);
            foreach (KnownFormula kf in knowFormulas)
            {
                Formula f = kf.Get();
                if (f != null && f.productItemId == productItemId)                
                {
                    list.Add(kf);
                }
            }
            return list.ToArray();
        }

        public int[] GetProductItems()
        {
            List<int> list = new List<int>(1);

            foreach (KnownFormula kf in knowFormulas)
            {
                Formula f = kf.Get();

                if(!list.Contains(f.productItemId))
                {
                    list.Add(f.productItemId);
                }
            }

            return list.ToArray();
        }

        public IEnumerable<KnownFormula> knowFormulas
        {
            get
            {
                return mForumlaList;
            }
        }

        public int knowFormulaCount
        {
            get
            {
                return mForumlaList.Count;
            }
        }

        public KnownFormula GetKnownFormula(int id)
        {
            return mForumlaList.Find(delegate(KnownFormula kf)
            {
                if (kf.id == id)
                {
                    return true;
                }
                return false;
            });
        }

        public void SetKnownFormulaFlag(int id)
        {
            KnownFormula kf = GetKnownFormula(id);
            if (null == kf)
            {
                return;
            }

            kf.flag = false;
        }

        //public int knownFormulaCount
        //{
        //    get
        //    {
        //        return mForumlaList.Count;
        //    }
        //}
		
		public bool StartReplicate(int formulaID, int count)
		{
			if(null != mRunningReplicate)
				return false;

			mRunningReplicate = new RunningReplicate();
			mRunningReplicate.formulaID = formulaID;
			mRunningReplicate.requestCount = count;
			mRunningReplicate.leftCount = count;
			mRunningReplicate.lastReciveTime = Time.time;
			return true;
		}
		
		public bool CancelReplicate(int formulaID)
		{
            //lz-2016.09.09 取消合成的时候保证要返还的数量大于0
            if (mRunningReplicate.finishCount > 0)
            {
                if (!HasEnoughPackage(mRunningReplicate.formula.productItemId, mRunningReplicate.formula.m_productItemCount * (mRunningReplicate.finishCount + 1)))
                    return false;
                if (GameConfig.IsMultiMode && MaxProductCount(mRunningReplicate.formula) >= mRunningReplicate.finishCount)
                    PlayerNetwork.mainPlayer.RequestMergeSkill(mRunningReplicate.formulaID, mRunningReplicate.finishCount);
                else
                    mHandler.CreateItem(mRunningReplicate.formula.productItemId, mRunningReplicate.formula.m_productItemCount * mRunningReplicate.finishCount);
            }
            mRunningReplicate.finishCount = 0;
            mRunningReplicate.lastReciveTime = Time.time;
            mRunningReplicate = null;
            if (null != onReplicateEnd)
                onReplicateEnd();
            return true;
        }
		
		public void UpdateReplicate()
		{
			if(null == mRunningReplicate) return;

			mRunningReplicate.runningTime += Time.deltaTime;

			if(mRunningReplicate.runningTime >= mRunningReplicate.formula.timeNeed * needTimeScale)
			{
				if(!HasEnoughPackage(mRunningReplicate.formula.productItemId, mRunningReplicate.formula.m_productItemCount * (mRunningReplicate.finishCount + 1)))
				{
					mRunningReplicate.runningTime = 0;
					MessageBox_N.ShowOkBox(PELocalization.GetString(8000050));
					return;
				}

				int maxCount = MaxProductCount(mRunningReplicate.formula);
				if(GameConfig.IsMultiMode ? maxCount > mRunningReplicate.finishCount : maxCount > 0)
				{
					mRunningReplicate.runningTime -= mRunningReplicate.formula.timeNeed;
					--mRunningReplicate.leftCount;
					++mRunningReplicate.finishCount;
					if(!GameConfig.IsMultiMode)
						DeleteMaterial(mRunningReplicate.formula, 1);
//					ItemProto itemProto = ItemProto.GetItemData(mRunningReplicate.formula.productItemId);
					if(mRunningReplicate.leftCount == 0 || Time.time - mRunningReplicate.lastReciveTime > ReciveInterval)
					{
						if(GameConfig.IsMultiMode)
							PlayerNetwork.mainPlayer.RequestMergeSkill(mRunningReplicate.formulaID, mRunningReplicate.finishCount);
						else
							mHandler.CreateItem(mRunningReplicate.formula.productItemId, mRunningReplicate.formula.m_productItemCount * mRunningReplicate.finishCount);
						mRunningReplicate.finishCount = 0;
						mRunningReplicate.lastReciveTime = Time.time;
					}
					if(0 == mRunningReplicate.leftCount)
					{
						mRunningReplicate = null;
						if(null != onReplicateEnd)
							onReplicateEnd();
					}
				}
				else
				{
					MessageBox_N.ShowOkBox(PELocalization.GetString(8000689));
					if(!GameConfig.IsMultiMode)
						mHandler.CreateItem(mRunningReplicate.formula.productItemId, mRunningReplicate.formula.m_productItemCount * mRunningReplicate.finishCount);
					mRunningReplicate = null;
					if(null != onReplicateEnd)
						onReplicateEnd();
				}
			}
		}

        public void Serialize(System.IO.BinaryWriter w)
        {
			w.Write(-1); //As version mask

            w.Write((int)mForumlaList.Count);

            foreach (KnownFormula f in mForumlaList)
            {
                w.Write((int)f.id);
                w.Write((bool)f.flag);
            }

			if(null != mRunningReplicate)
			{
				w.Write(true);
				w.Write(mRunningReplicate.formulaID);
				w.Write(mRunningReplicate.requestCount);
				w.Write(mRunningReplicate.leftCount);
				w.Write(mRunningReplicate.finishCount);
				w.Write(mRunningReplicate.runningTime);
			}
			else
				w.Write(false);
        }

        public void Deserialize(System.IO.BinaryReader r)
        {
			int versionMask = r.ReadInt32();
			int count = versionMask;
			if(versionMask <= -1)
            	count = r.ReadInt32();
            for(int i = 0; i < count; i++)
            {
				int id = r.ReadInt32();
				bool flag = r.ReadBoolean();
                mForumlaList.Add(new KnownFormula(id, flag));
            }
			if(versionMask <= -1)
			{
				if(r.ReadBoolean())
				{
					mRunningReplicate = new RunningReplicate();
					mRunningReplicate.formulaID = r.ReadInt32();
					mRunningReplicate.requestCount = r.ReadInt32();
					mRunningReplicate.leftCount = r.ReadInt32();
					mRunningReplicate.finishCount = r.ReadInt32();
					mRunningReplicate.runningTime = r.ReadSingle();
				}
			}
        }
    }
}