
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using NaturalResAsset;
using SkillSystem;
using WhiteCat;

namespace SkillAsset
{

	public class CoroutineStoppable : IEnumerator
	{
		public bool stop = false;
		IEnumerator enumerator;
		//MonoBehaviour behaviour;
		//Coroutine coroutine;

		public CoroutineStoppable(MonoBehaviour behaviour, IEnumerator enumerator)
		{
			this.stop = false;
			//this.behaviour = behaviour;
			this.enumerator = enumerator;

            if (behaviour != null && behaviour.gameObject.activeSelf)
            {
                behaviour.StartCoroutine(this);
            }
		}

		// Interface implementations----TODO : need tst stop and WaitForSeconds
		public object Current { get { return enumerator.Current; } }
		public bool MoveNext() { return !stop && enumerator.MoveNext(); }
		public void Reset() { enumerator.Reset(); }
#if false	// Tst Func set
	public static IEnumerator TstFunc(){
			while(true){
				Debug.Log("InTstFunc at :"+Time.time);
				yield return new WaitForSeconds(0.5f);
			}
		}
	public static IEnumerator StopTstFunc(CoroutineStoppable coroutine, float waitSecond){
			yield return new WaitForSeconds(waitSecond);
			coroutine.stop = true;
		}
#endif
	}

	public class ESkillTarget
	{	// TODO : should be same as design doc
		public const int TAR_Enemy = 0x0001;
		public const int TAR_EnemyBuilding = 0x0002;
		public const int TAR_Self = 0x0004;
		public const int TAR_Partner = 0x0008;
		public const int TAR_PartnerBuilding = 0x0010;
		public const int TAR_aa = 0x0020;
		public const int TAR_Mud = 0x0040;
		public const int TAR_Mine = 0x0080;
		public const int TAR_Wood = 0x0100;
		public const int TAR_Herb = 0x0200;
		public const int TAR_Fish = 0x0400;
		public const int TAR_VitualMeat = 0x0800;
		public const int TAR_VitualGrass = 0x1000;
		public const int TAR_VitualWater = 0x2000;
		public static int Type2Mask(ESkillTargetType type)
		{
			if (type == ESkillTargetType.TYPE_SkillRunner)
				return TAR_Enemy | TAR_Self | TAR_Partner;

			if (type == ESkillTargetType.TYPE_Building)
				return TAR_EnemyBuilding | TAR_PartnerBuilding;

			if (type >= ESkillTargetType.TYPE_Mud && type <= ESkillTargetType.TYPE_VitualWater)
			{
                int sum = 1 << ((int)type - 1);
                return sum;
			}

			Debug.Log("Undefined Type2Mask behavior");
			return 0;
		}
	}
	public enum ESkillTargetType
	{   //Corresponding to resource table
		/*
		TYPE_Enemy = 1,
		TYPE_EnemyBuilding,
		TYPE_Self,
		TYPE_Partner,
		TYPE_PartnerBuilding,
		TYPE_aa,
		 * */
		TYPE_Mud = 7,
		TYPE_Mine,
		TYPE_Wood,
		TYPE_Herb,
		TYPE_Fish,
		TYPE_VitualMeat,
		TYPE_VitualGrass,
		TYPE_VitualWater,

		TYPE_SkillRunner = -1,
		TYPE_Building = -2,
	}
	
	public interface IHuman
	{
		List<ItemObject> Equipments{get;}
		bool CheckAmmoCost(EArmType type, int cost);
		void ApplyAmmoCost(EArmType type, int cost);
		EquipType GetEquipType();
		Equipment MainHandEquip();
		bool PutOnEquip(ItemObject item);
		bool TakeOffEquip(ItemObject item, bool directlyRemove = false);
		void ApplyDurabilityReduce(int Type);
		void GetOnCarrier(Transform tran);
		void GetOffCarrier();
	}
	
	public interface ISkillTarget
	{
		ESkillTargetType GetTargetType();
		Vector3 GetPosition();
	}
	
	public class DefaultPosTarget : ISkillTarget
	{
		public DefaultPosTarget(Vector3 position) { m_vPos = position; }
		public ESkillTargetType GetTargetType ()
		{
			return ESkillTargetType.TYPE_Building;
		}
		private Vector3 m_vPos;
		public Vector3 GetPosition() { return m_vPos; }
	}
	public interface INaturalResTarget : ISkillTarget
	{
		int GetDestroyed(SkillRunner caster, float durDec, float radius);
		List<ItemSample> ReturnItems(short resGotMultiplier, int num);
		bool IsDestroyed();
	}

	public class VFTerrainTarget : INaturalResTarget
	{
		public bool m_bDestroyed = false;
		public Vector3 m_vPos;
		public IntVector3 m_intPos;
		public VFVoxel m_voxel;
		
		List<VFVoxel> mRemoveList = new List<VFVoxel>();
		public VFTerrainTarget(Vector3 vPos, IntVector3 intPos, ref VFVoxel voxel)
		{
			m_vPos = vPos;
			m_intPos = intPos;
			m_voxel = voxel;
		}
		public Vector3 GetPosition() { return m_vPos; }
		public ESkillTargetType GetTargetType()
		{
			if (m_voxel.IsBuilding) return ESkillTargetType.TYPE_Building;
			if (m_voxel.Type == (byte)VFVoxel.EType.MUD) return ESkillTargetType.TYPE_Mud;
			/*TODO : cod for mine*/
			return ESkillTargetType.TYPE_Mud;
		}

		public int GetDestroyed(SkillRunner caster, float durDec, float radius)
		{
			return 0;
//			if (!GameConfig.IsMultiMode)
//			{
////				int count = DigTerrainManager.Instance.DigTerrain(m_intPos, durDec, radius, ref mRemoveList);
////				m_bDestroyed = count > 0;
////				return count;
//				return 0;
//			}
//			else
//			{
//				int count = DigTerrainManager.DigTerrainNetwork(caster, m_intPos, durDec, radius, ref m_voxel);
//                return count;
//			}
			
		}

		public List<ItemSample> ReturnItems(short resGotMultiplier, int num)
		{
			NaturalRes resData;
			num = mRemoveList.Count;
			List<ItemSample> itemGridList = new List<ItemSample>();
			for(int index = 0; index < num; index++)
			{
				if ((resData = NaturalRes.GetTerrainResData(mRemoveList[index].Type)) != null && resData.m_itemsGot.Count > 0)
				{
					List<float> randVars = new List<float>();
	
	                ItemSample[] itemGrids = new ItemSample[resData.m_itemsGot.Count];
	
	                for (int i = 0; i < resData.m_itemsGot.Count; i++)
	                    itemGrids[i] = new ItemSample(resData.m_itemsGot[i].m_id,0);
					
					float resGet =0;
					if(resData.mFixedNum > 0)
						resGet = resData.mFixedNum;
					else
						resGet = (resGotMultiplier + resData.mSelfGetNum);
		
	                for (int i = 0; i < resGet; i++)
	                {                    
	                    randVars.Add(UnityEngine.Random.Range(0, 100));
	                }
	
	                for (int i = 0; i < randVars.Count; i++)
	                {
	                    for (int j = 0; j < resData.m_itemsGot.Count; j++)
	                    {
	                        if (randVars[i] < resData.m_itemsGot[j].m_probablity)
	                        {
	                            itemGrids[j].IncreaseStackCount(1);
	                            break;
	                        }
	                    }
	                }
					//extra item get below, add by yinrui 
					List<ItemSample> itemGetExtra = new List<ItemSample>();
					if(resData.m_extraGot.extraPercent > 0 && Random.value < resGet * resData.m_extraGot.extraPercent)
					{
						for(int i = 0; i < resData.m_extraGot.m_extraGot.Count; i++)
							itemGetExtra.Add(new ItemSample(resData.m_extraGot.m_extraGot[i].m_id, 0));
						resGet *= resData.m_extraGot.extraPercent;
						int rand;
						for(int i = 0; i < resGet; i++)
						{
							rand = Random.Range(0, 100);
							for(int j = 0; j < resData.m_extraGot.m_extraGot.Count; j++)
							{
								if(rand < resData.m_extraGot.m_extraGot[j].m_probablity)
								{
									itemGetExtra[j].IncreaseStackCount(1);
									break;
								}
							}
						}
					}//part1 end

	                for (int i = 0; i < itemGrids.Length; i++)
	                {
	                    if (itemGrids[i].GetCount() > 0)
	                    {
							ItemSample findItem = itemGridList.Find(itr => itr.protoId == itemGrids[i].protoId);
							if(null != findItem)
								findItem.IncreaseStackCount(itemGrids[i].GetCount());
							else
		                        itemGridList.Add(itemGrids[i]);
	                    }
	                }

					//part2
					foreach (ItemSample data in itemGetExtra)
					{
						if(data.GetCount() > 0)
						{
							ItemSample findItem = itemGridList.Find(itr => itr.protoId == data.protoId);
							if(null != findItem)
								findItem.IncreaseStackCount(data.GetCount());
							else
		                        itemGridList.Add(data);
						}
					}//part2 end
				}
			}
			mRemoveList.Clear();
			return itemGridList;
		}
		public bool IsDestroyed(){	return m_bDestroyed;	}
	}

//	public class VFTerrainBuild : INaturalResTarget
//	{
//		public Vector3 m_vPos;
//		public List<IntVector3> m_intPos;
//		public VFVoxelChunkGo m_chunk;
//		public VFTerrainBuild(Vector3 vPos, List<IntVector3> intPos, VFVoxelChunkGo vChunk)
//		{
//			m_vPos = vPos;
//			m_intPos = intPos;
//			m_chunk = vChunk;
//		}
//		public Vector3 GetPosition() 			{ 	return m_vPos; }
//		public ESkillTargetType GetTargetType()	{	return ESkillTargetType.TYPE_Building;		}
//		public int GetDestroyed(SkillRunner caster, float durDec, short weaponBonus)
//		{
//			IntVector3 vPos = m_intPos[Random.Range(0, m_intPos.Count)];
//			Vector3 pos = new Vector3(vPos.x, vPos.y, vPos.z);
//
//			if (!GameConfig.IsMultiMode)
//			{
//				DigTerrainManager.self.DigBuilding(caster, m_intPos, durDec);
//			}
//			else
//			{
//				DigTerrainManager.self.DigBuildingNetwork(caster, m_intPos, durDec);
//			}
//
//			return 0;
//		}
//
//		public List<ItemSample> ReturnItems(short resGotMultiplier, int num)	{	return null;	}
//		public bool IsDestroyed()												{	return false;	}
//	}
	
	//Add by lilj 2012/5/31
	public class GroundItemTarget : INaturalResTarget
	{
		public float m_Hp = 255.0f;
		public float mMaxHp = 255f;
		public Vector3	mPos;
		public GlobalTreeInfo	mGroundItem;
		public GroundItemTarget(Vector3 pos,GlobalTreeInfo item)
		{
			mPos = pos;
			mGroundItem = item;
			mMaxHp = m_Hp *= item._treeInfo.m_widthScale * item._treeInfo.m_widthScale;
		}
		
		public ESkillTargetType GetTargetType()
		{
			int typeFind =  NaturalResAsset.NaturalRes.GetTerrainResData(mGroundItem._treeInfo.m_protoTypeIdx + 1000).m_type;
			if(9 == typeFind)
				return ESkillTargetType.TYPE_Wood;
			else if(10 == typeFind)
				return ESkillTargetType.TYPE_Herb;
			return ESkillTargetType.TYPE_Herb;//default herb
		}
		public Vector3 GetPosition()	{		return mPos;		}
		public int GetDestroyed(SkillRunner caster, float durDec, float radius)
		{
            if (!GameConfig.IsMultiMode)
            {
                NaturalRes res = NaturalResAsset.NaturalRes.GetTerrainResData(mGroundItem._treeInfo.m_protoTypeIdx + 1000);
				if ( m_Hp > 0 )
				{
	                m_Hp -= durDec * res.m_duration;
					UITreeCut.Instance.SetSliderValue(mGroundItem._treeInfo, m_Hp / mMaxHp);
                    //PlayerFactory.mMainPlayer.UpdateTreeHp(mGroundItem._treeInfo, m_Hp);
                    //if (m_Hp <= 0)
                    //{
                    //    if (null != LSubTerrainMgr.Instance)
                    //    {
                    //        LSubTerrainMgr.DeleteTree(mGroundItem);
                    //        LSubTerrainMgr.RefreshAllLayerTerrains();
                    //    }
                    //    else if (null != RSubTerrainMgr.Instance)
                    //    {
                    //        RSubTerrainMgr.DeleteTree(mGroundItem._treeInfo);
                    //        RSubTerrainMgr.RefreshAllLayerTerrains();
                    //    }
                    //    TreeCutGui_N.Instance.HideWindow();
                    //    return 101;
                    //}
				}
            }
            else
            {
				//NaturalRes res = NaturalResAsset.NaturalRes.GetTerrainResData(mGroundItem._treeInfo.m_protoTypeIdx + 1000);
				//m_Hp -= durDec * res.m_duration;
				//TreeCutGui_N.Instance.SetSliderValue(m_Hp / mMaxHp);
				//PlayerFactory.mMainPlayer.UpdateTreeHp(mGroundItem._treeInfo, m_Hp);
				//if (m_Hp <= 0)
				//{
				//	if (null != RSubTerrainMgr.Instance)
				//	{
				//		TreeInfo treeInfo = mGroundItem._treeInfo;
				//		//RSubTerrainMgr.DeleteTreesAtPos(mGroundItem._treeInfo.m_pos);
				//		LogManager.Debug("RPC_C2S_GroundItemTarget: " + weaponBonus + ", " + treeInfo.m_protoTypeIdx + "," + treeInfo.m_widthScale + "," + treeInfo.m_heightScale);
				//		caster.RPC("RPC_C2S_GroundItemTarget", weaponBonus, treeInfo.m_protoTypeIdx, treeInfo.m_widthScale, treeInfo.m_heightScale,treeInfo.m_pos);
				//	}
				//	TreeCutGui_N.Instance.HideWnd();
				//	return 100;
				//}

//				NaturalRes res = NaturalResAsset.NaturalRes.GetTerrainResData(mGroundItem._treeInfo.m_protoTypeIdx + 1000);
//				if (null != res)
//				{
//					TreeInfo treeInfo = mGroundItem._treeInfo;
//					caster.RPCServer(EPacketType.PT_InGame_SkillGroundItem, res.mCastSkill, treeInfo.m_pos,
//						treeInfo.m_protoTypeIdx, treeInfo.m_heightScale, treeInfo.m_widthScale);
//				}

				return 0;
            }
			return 0;
		}
		public List<ItemSample> ReturnItems(short resGotMultiplier, int num)
		{
			List<ItemSample> reItem = new List<ItemSample>();
			NaturalRes res =  NaturalResAsset.NaturalRes.GetTerrainResData(mGroundItem._treeInfo.m_protoTypeIdx + 1000);
			
			ItemSample[] getItem = new ItemSample[res.m_itemsGot.Count];
			for(int i=0;i<res.m_itemsGot.Count;i++)
				getItem[i] = new ItemSample(res.m_itemsGot[i].m_id,0);
			
			float resGet =0;
			if(res.mFixedNum > 0)
				resGet = res.mFixedNum;
			else
				resGet = resGotMultiplier + res.mSelfGetNum * mGroundItem._treeInfo.m_widthScale * mGroundItem._treeInfo.m_widthScale * mGroundItem._treeInfo.m_heightScale;
			
			for(int numGet=0;numGet<(int)resGet;numGet++)
			{
				int getPro = UnityEngine.Random.Range(0, 100);
				for(int i=0;i<res.m_itemsGot.Count;i++)
				{
					if(getPro<res.m_itemsGot[i].m_probablity)
					{
						getItem[i].IncreaseStackCount(1);
						break;
					}
				}
			}
			//extra item get below, add by yinrui 
			List<ItemSample> itemGetExtra = new List<ItemSample>();
			if(res.m_extraGot.extraPercent > 0 && Random.value < resGet * res.m_extraGot.extraPercent)
			{
				for(int i = 0; i < res.m_extraGot.m_extraGot.Count; i++)
					itemGetExtra.Add(new ItemSample(res.m_extraGot.m_extraGot[i].m_id, 0));
				resGet *= res.m_extraGot.extraPercent;
				int rand;
				for(int i = 0; i < resGet; i++)
				{
					rand = Random.Range(0, 100);
					for(int j = 0; j < res.m_extraGot.m_extraGot.Count; j++)
					{
						if(rand < res.m_extraGot.m_extraGot[j].m_probablity)
						{
							itemGetExtra[j].IncreaseStackCount(1);
							break;
						}
					}
				}
			}//part1 end
			for(int i=0;i<res.m_itemsGot.Count;i++)
			{
				if(getItem[i].GetCount() > 0)
					reItem.Add(getItem[i]);
			}
			foreach(ItemSample data in itemGetExtra)
			{
				if(data.GetCount() > 0)
					reItem.Add(data);
			}
			//recalculation num with treeinfo's scale
			/*foreach(ItemSample finalGetItem in reItem)
			{
				int getnum = finalGetItem.GetCount();
				if(getnum == 1)
					continue;
				getnum = (int)(getnum * mGroundItem._treeInfo.m_widthScale * mGroundItem._treeInfo.m_heightScale);
				getnum = (getnum < 1)?1:getnum;
				finalGetItem.DecreaseStackCount(finalGetItem.GetCount());
				finalGetItem.CountUp(getnum);
			}*/
			return reItem;
		}
		public bool IsDestroyed()	{	return m_Hp<=0;	}
	}
	//Add end lilj 2012/5/31

	public class MergeSkillInstance
	{
		//public MergeSkill m_data;
		public float m_timeStartPrep;
		public CoroutineStoppable m_runner;
		public static bool MatchId(MergeSkillInstance iter, int id)
		{
            return false;
			//return iter.m_data.m_id == id;
		}
	}
	public class EffSkillInstance
	{
        public enum EffSection
        {
            None,
            Start,
            Running,
            Completed,
            Max
        }

        public EffSection m_section;
		public EffSkill m_data;
		public float m_timeStartPrep;
		public CoroutineStoppable m_runner;
		public CoroutineStoppable m_sharedRunner;
		
		//For SkillTargetChange and loop
		public ISkillTarget mNextTarget;
		public bool 		mSkillCostTimeAdd;
		
		public static bool MatchId(EffSkillInstance iter, int id)
		{
			return iter.m_data.m_id == id;
		}
		public static bool MatchType(EffSkillInstance iter, short type)
		{
			return iter.m_data.m_cdInfo.m_type == type;
		}
	}


	// Maintenence a list of running skills and their status and their buff for each player, each enemy
	// Player & AIMonster must inherit this.
	public abstract partial class SkillRunner : CommonInterface
	{
		SkEntity mSkEntity;

		protected SkEntity _SkEntity
		{
			get
			{
				if(null == mSkEntity)
					mSkEntity = GetComponent<SkEntity>();
				return mSkEntity;
			}
		}

		public virtual float GetAttribute(Pathea.AttribType type, bool isBase = false)
		{
			if(null != _SkEntity)
			{
				if(isBase)
					return _SkEntity.attribs.raws[(int)type];
				return _SkEntity.attribs.sums[(int)type];
			}
			return 0;
		}

		public virtual void SetAttribute(Pathea.AttribType type, float value, bool isBase = true)
		{
			if(null != _SkEntity)
			{
				if(isBase)
					_SkEntity.attribs.raws[(int)type] = value;
				_SkEntity.attribs.sums[(int)type] = value;
			}
		}

		public PackBase BuffAttribs 
		{
			get { if(null != _SkEntity) return null; else return _SkEntity.attribs.pack; }
			set { if(null != _SkEntity){_SkEntity.attribs.pack = value;} }
		}

		// Caster
		public List<MergeSkillInstance> m_mergeSkillInsts = new List<MergeSkillInstance>();
		public List<EffSkillInstance> m_effSkillInsts = new List<EffSkillInstance>();
		public List<EffSkillInstance> m_effShareSkillInsts = new List<EffSkillInstance>();
		
		public LifeFormController mLifeFormController;
//		protected AudioSource mSoundSource;

		// These Buff is attached on skill targets
		public EffSkillBuffManager m_effSkillBuffManager = new EffSkillBuffManager();

		//
		public override ESkillTargetType GetTargetType() { return ESkillTargetType.TYPE_SkillRunner; }
		public override Vector3 GetPosition()
		{ 
			if(this == null) return Vector3.zero;
			return this.transform.position; 
		}

        /*
         *	ͬ��״̬�ĳ�Ա 
         */

        internal abstract byte GetBuilderId();
		internal abstract float GetAtkDist(ISkillTarget target);
//		internal abstract float GetDurAtk(ESkillTargetType resType);
//		internal abstract short GetResMultiplier();
		internal abstract ItemPackage GetItemPackage();
		internal abstract bool IsEnemy(ISkillTarget target);
		internal abstract ISkillTarget GetTargetInDist(float dist, int targetMask);
		internal abstract List<ISkillTarget> GetTargetlistInScope(EffScope scope, int targetMask, ISkillTarget target);

		//internal abstract Get
		// Apply changed of properties directly to 
		internal abstract void ApplyDistRepel(SkillRunner caster, float distRepel);
		internal abstract void ApplyHpChange(SkillRunner caster, float hpChange, float damagePercent, int type);
		internal abstract void ApplyComfortChange(float comfortChange);
		internal abstract void ApplySatiationChange(float satiationChange);
		internal abstract void ApplyThirstLvChange(float thirstLvChange);
//		internal abstract void ApplyBuffPermanent(EffSkillBuff buff);
		
		internal virtual void  ApplyPropertyChange(Dictionary<int, float> propertyChanges)
		{
			if(null != mLifeFormController)
				mLifeFormController.ApplyPropertyChange(propertyChanges);
		}
		
		internal virtual float ApplyEnergyShieldAttack(Projectile proj){return 1f;}
		internal virtual void ApplyDurChange(SkillRunner caster, float durChange, int type) { }
		internal virtual void ApplyBuffContinuous(SkillRunner caster, short buffSp) { }
		internal virtual Transform GetCastTransform(SkillAsset.EffItemCast cast) { return null; }
		internal virtual void ApplyLearnSkill(List<int> skillIdList) { }
		internal virtual void ApplyMetalScan(List<int> metalID){ }
		
		internal virtual float GetResRadius()
		{
			return 0.1f;
		}

		//effect and anim
		internal abstract void ApplyAnim(List<string> animName);
		
		internal virtual void ApplyAnim(string animName)
		{
			
		}
		internal virtual void ApplyEffect(List<int> effId, ISkillTarget target)
		{
            if (effId == null)
                return;

			// TODO :code
			for (int i = 0; i < effId.Count; i++)
				if (effId[i] != 0)
					EffectManager.Instance.Instantiate(effId[i], transform, null);
		}

        internal virtual void ApplySound(int soundID)
        {
			
		}

		public MergeSkillInstance RunMerge(int skillId, int productNum)
		{
            //if (null != m_mergeSkillInsts.Find(iterSkill0 => MergeSkillInstance.MatchId(iterSkill0, skillId)))
            //{
            //    // TODO : a warning message of a mergeskill is running
            //    return null;
            //}
            //MergeSkillInstance inst = new MergeSkillInstance();
            //inst.m_data = MergeSkill.s_tblMergeSkills.Find(iterSkill1 => MergeSkill.MatchId(iterSkill1, skillId));
            //if (null == inst.m_data)
            //{
            //    // TODO : a warning message of no such skill
            //    return null;
            //}
            //if (!inst.m_data.CheckCond())
            //{
            //    // TODO : a warning message of not measuring condition
            //    return null;
            //}
            //inst.m_timeStartPrep = Time.time;
            //inst.m_runner = new CoroutineStoppable(this, inst.m_data.Exec(this, productNum, inst));
            //return inst;
            return null;
		}

		public void CancelMerge()
		{
//            for (int i = m_mergeSkillInsts.Count - 1; i >= 0; i--)
//            {
//                m_mergeSkillInsts[i].m_runner.stop = true;

////                 if (GameConfig.IsMultiMode() && uLink.Network.isServer)
////                 {
////                     SendRemoveMergeSkillInsts(m_mergeSkillInsts[i].m_data.m_id);
////                 }

//                m_mergeSkillInsts.RemoveAt(i);
//            }
		}
		
		public bool CheckRunEffEnabl(int skillId, ISkillTarget target)
		{
			EffSkillInstance inst = new EffSkillInstance();
			inst.m_data = EffSkill.s_tblEffSkills.Find(iterSkill1=>EffSkill.MatchId(iterSkill1,skillId));
			if( null == inst.m_data ){
				// TODO : a warning message of no such skill
				return false;
			}
		
			if( null != m_effShareSkillInsts.Find(iterSkill0=>EffSkillInstance.MatchType(iterSkill0,inst.m_data.m_cdInfo.m_type)) ){
				// TODO : a warning message of another instance of this effskill is still running
				return false;
			}
			
			if( null != m_effSkillInsts.Find(iterSkill0=>EffSkillInstance.MatchId(iterSkill0,skillId)) ){
				// TODO : a warning message of another instance of this effskill is still running
				return false;
			}
			// TODO : cd info check 

			if(!inst.m_data.CheckTargetsValid(this,target)){
                // TODO : a warning message of not target in scope
                return false;
			}
			return true;
		}

		// TODO : Confirm how an enemy to attack building
		public EffSkillInstance RunEff(int skillId, ISkillTarget target)
		{
			EffSkillInstance inst = new EffSkillInstance();
			inst.m_data = EffSkill.s_tblEffSkills.Find(iterSkill1=>EffSkill.MatchId(iterSkill1,skillId));
			if( null == inst.m_data ){
				// TODO : a warning message of no such skill
				return null;
			}
		
			if( null != m_effShareSkillInsts.Find(iterSkill0=>EffSkillInstance.MatchType(iterSkill0,inst.m_data.m_cdInfo.m_type)) ){
				// TODO : a warning message of another instance of this effskill is still running
				return null;
			}
			
			if( null != m_effSkillInsts.Find(iterSkill0=>EffSkillInstance.MatchId(iterSkill0,skillId)) ){
				// TODO : a warning message of another instance of this effskill is still running
				return null;
			}
			// TODO : cd info check 

			if(!inst.m_data.CheckTargetsValid(this,target)){
                // TODO : a warning message of not target in scope
                return null;
			}

			if (!GameConfig.IsMultiMode || IsController)
			{
				if (GameConfig.IsMultiMode)
				{
					if (target is CommonInterface)
					{
						CommonInterface ta = target as CommonInterface;
						if (null != ta && null != ta.OwnerView)
						{
							RPCServer(EPacketType.PT_InGame_SkillCast, skillId, ta.OwnerView.viewID);
						}
						else
						{
							RPCServer(EPacketType.PT_InGame_SkillCast, skillId, uLink.NetworkViewID.unassigned);
						}
					}
					else if (target is DefaultPosTarget)
					{
						RPCServer(EPacketType.PT_InGame_SkillShoot, skillId, target.GetPosition());
					}
					else
					{
						RPCServer(EPacketType.PT_InGame_SkillCast, skillId, uLink.NetworkViewID.unassigned);
					}
				}

				inst.m_timeStartPrep = Time.time;
				inst.m_runner = new CoroutineStoppable(this, inst.m_data.Exec(this, target, inst));
				inst.m_sharedRunner = new CoroutineStoppable(this, inst.m_data.SharingCooling(this, inst));
			}
			return inst;
		}

		public EffSkillInstance RunEffOnProxy(int skillId, ISkillTarget target)
		{
			EffSkillInstance inst = new EffSkillInstance();
			inst.m_data = EffSkill.s_tblEffSkills.Find(iterSkill1 => EffSkill.MatchId(iterSkill1, skillId));
			if (null == inst.m_data)
			{
				// TODO : a warning message of no such skill
				return null;
			}

			if (null != m_effShareSkillInsts.Find(iterSkill0 => EffSkillInstance.MatchType(iterSkill0, inst.m_data.m_cdInfo.m_type)))
			{
				// TODO : a warning message of another instance of this effskill is still running
				return null;
			}

			if (null != m_effSkillInsts.Find(iterSkill0 => EffSkillInstance.MatchId(iterSkill0, skillId)))
			{
				// TODO : a warning message of another instance of this effskill is still running
				return null;
			}
			// TODO : cd info check 

			//if (!inst.m_data.CheckTargetsValid(this, target))
			//{
			//	// TODO : a warning message of not target in scope
			//	return null;
			//}

			inst.m_timeStartPrep = Time.time;
			inst.m_runner = new CoroutineStoppable(this, inst.m_data.ExecProxy(this, target, inst));
			inst.m_sharedRunner = new CoroutineStoppable(this, inst.m_data.SharingCooling(this, inst));
			return inst;
		}

		//public void RunEff(EffSkillInstance inst, ISkillTarget target)
		//{
		//	// Stand alone and client mine should check target, client proxy should not
		//	if (!GameConfig.IsMultiMode)
		//	{
		//		if (!inst.m_data.CheckTargetsValid(this, target))
		//		{
		//			// TODO : a warning message of not target in scope
		//			return;
		//		}
		//	}

		//	if (GameConfig.IsMultiMode)
		//	{
		//		ESkillTargetType type = target.GetTargetType();
		//		switch (type)
		//		{
		//		case ESkillTargetType.TYPE_SkillRunner:
		//			{
		//				SkillRunner ta = target as SkillRunner;
		//				if (null != ta)
		//					RPC("RPC_C2S_SkillCast", inst.m_data.m_id, ta.OwnerNetworkView.viewID);
		//			}
		//			break;
		//		}
		//	}

		//	inst.m_timeStartPrep = Time.time;
		//	inst.m_runner = new CoroutineStoppable(this, inst.m_data.Exec(this, target, inst));
		//	inst.m_sharedRunner = new CoroutineStoppable(this, inst.m_data.SharingCooling(this, inst));
		//}

		//public void RunEffOnProxy(EffSkillInstance inst, ISkillTarget target)
		//{
		//	// Stand alone and client mine should check target, client proxy should not
		//	if (!GameConfig.IsMultiMode)
		//	{
		//		if (!inst.m_data.CheckTargetsValid(this, target))
		//		{
		//			// TODO : a warning message of not target in scope
		//			return;
		//		}
		//	}

		//	inst.m_timeStartPrep = Time.time;
		//	inst.m_runner = new CoroutineStoppable(this, inst.m_data.Exec(this, target, inst));
		//	inst.m_sharedRunner = new CoroutineStoppable(this, inst.m_data.SharingCooling(this, inst));
		//}

		public ISkillTarget GetTargetByGameObject(RaycastHit hitinfo, EffSkillInstance inst)
		{
			ISkillTarget target = null;
			GameObject obj = hitinfo.collider.gameObject;
			if (obj != null)
			{
				int layer = obj.layer;
				switch (layer)
				{
					//VFVoxelTerrain
					case 12:
						{
							//VFVoxelChunkGo chunk;
							if ((obj.GetComponent<VFVoxelChunkGo>()) != null)
							{
								Vector3 fCurPos = hitinfo.point;
								Vector3 vPos = fCurPos / VoxelTerrainConstants._scale;

								vPos += -Vector3.up * 0.01f;

								IntVector3 fVoxelCenterPos = new IntVector3(Mathf.FloorToInt(vPos.x + 1), Mathf.FloorToInt(vPos.y + 1), Mathf.FloorToInt(vPos.z + 1));

								VFVoxel voxel = VFVoxelTerrain.self.Voxels.SafeRead(fVoxelCenterPos.x, fVoxelCenterPos.y, fVoxelCenterPos.z);
								target = new VFTerrainTarget(hitinfo.point, fVoxelCenterPos, ref voxel);
							}
						}
						break;

					//Player
					case 10:
						{
							target = obj.GetComponent<SkillRunner>();
						}
						break;
				}
			}

			if (!inst.m_data.CheckTargetsValid(this, target))
			{
				// TODO : a warning message of not target in scope
				return null;
			}

			return target;
		}
		
		
	VFVoxel GetRaycastHitVoxel(Vector3 normal,Vector3 point, out IntVector3 voxelPos)
	{
		Vector3 vPos = point;
		
		if(0.05f > Mathf.Abs(normal.normalized.x))
			vPos.x = Mathf.RoundToInt(vPos.x);
		else
			vPos.x = (normal.x > 0)?Mathf.Floor(vPos.x):Mathf.Ceil(vPos.x);
		if(0.05f > Mathf.Abs(normal.normalized.y))
			vPos.y = Mathf.RoundToInt(vPos.y);
		else
			vPos.y = (normal.y > 0)?Mathf.Floor(vPos.y):Mathf.Ceil(vPos.y);
		if(0.05f > Mathf.Abs(normal.normalized.z))
			vPos.z = Mathf.RoundToInt(vPos.z);
		else
			vPos.z = (normal.z > 0)?Mathf.Floor(vPos.z):Mathf.Ceil(vPos.z);
		
		voxelPos = new Vector3(Mathf.Round(vPos.x), Mathf.Round(vPos.y), Mathf.Round(vPos.z));
#if false		//VOXEL_OFFSET
		voxelPos.x += 1;
		voxelPos.y += 1;
		voxelPos.z += 1;
#endif
		VFVoxel voxel = VFVoxelTerrain.self.Voxels.SafeRead(voxelPos.x, voxelPos.y, voxelPos.z);
		float dis = 0;
		while(voxel.Volume==0)
		{
			dis += 0.1f;
			Vector3 hitPoint = point - normal*dis;
			voxelPos = new Vector3(Mathf.Round(hitPoint.x), Mathf.Round(hitPoint.y), Mathf.Round(hitPoint.z));
#if false			// VOXEL_OFFSET
			voxelPos.x += 1;
			voxelPos.y += 1;
			voxelPos.z += 1;
#endif
			voxel = VFVoxelTerrain.self.Voxels.SafeRead(voxelPos.x, voxelPos.y, voxelPos.z);
		}
		return voxel;
	}		
		
		//yang 处理土地
		public ISkillTarget GetTerrainTarget(Vector3 normal, Vector3 point)
		{
			IntVector3 fVoxelCenterPos;
			VFVoxel voxel = GetRaycastHitVoxel(normal,point, out fVoxelCenterPos);
			ISkillTarget target = new VFTerrainTarget(point, fVoxelCenterPos, ref voxel);
			return target;
		}
		
		public EffSkillInstance SkipEff(int skillId)
		{
			//enter skill's cooldown without taking effect
			EffSkillInstance inst = new EffSkillInstance();
			inst.m_data = EffSkill.s_tblEffSkills.Find(iterSkill1 => EffSkill.MatchId(iterSkill1, skillId));
			if (null == inst.m_data)
			{
				// TODO : a warning message of no such skill
				return null;
			}

			inst.m_runner = new CoroutineStoppable(this, inst.m_data.SkipExec(this, inst));
			inst.m_sharedRunner = new CoroutineStoppable(this, inst.m_data.SharingCooling(this, inst));
			return inst;
		}

        public void DeadClear()
        {
            m_effSkillInsts.Clear();
            m_effShareSkillInsts.Clear();
            m_mergeSkillInsts.Clear();
        }

		public void StopEff()
		{
			for (int i = m_effSkillInsts.Count - 1; i >= 0; i--)
			{
				if (m_effSkillInsts[i].m_data.m_interruptable)
				{
					m_effSkillInsts[i].m_runner.stop = true;
					m_effSkillInsts[i].m_sharedRunner.stop = true;
					m_effSkillInsts.RemoveAt(i);
				}
			}
		}
		public void StopEff(int skillId)
		{
			for (int i = m_effSkillInsts.Count - 1; i >= 0; i--)
			{
				if (m_effSkillInsts[i].m_data.m_id == skillId && m_effSkillInsts[i].m_data.m_interruptable)
				{
					m_effSkillInsts[i].m_runner.stop = true;
					m_effSkillInsts[i].m_sharedRunner.stop = true;
					m_effSkillInsts.RemoveAt(i);
					break;
				}
			}
		}
		public void CancelSkillBuff(int skillId)
		{
			m_effSkillBuffManager.Remove(skillId);
		}

		public bool IsEffRunning()
		{
			return m_effSkillInsts.Count > 0;
		}

		public bool IsEffRunning(int skillId)
		{
			return m_effSkillInsts.Find(iterSkill0 => EffSkillInstance.MatchId(iterSkill0, skillId)) != null;
		}

        public bool IsSkillRunning(int skillId)
        {
            EffSkillInstance inst = m_effSkillInsts.Find(iterSkill0 => EffSkillInstance.MatchId(iterSkill0, skillId));
            return inst != null 
                && inst.m_section > EffSkillInstance.EffSection.None 
                && inst.m_section < EffSkillInstance.EffSection.Completed;
        }
		
		public EffSkillInstance GetRunningEff(int skillId)
		{
			return m_effSkillInsts.Find(iterSkill0 => EffSkillInstance.MatchId(iterSkill0, skillId));
		}

		public bool IsSkillCooling(int skillId)
		{
			EffSkill skill = EffSkill.s_tblEffSkills.Find(iterSkill1 => EffSkill.MatchId(iterSkill1, skillId));
			if (skill == null) return false;

			if (null != m_effSkillInsts.Find(iterSkill0 => EffSkillInstance.MatchId(iterSkill0, skillId)))
			{
				// TODO : a warning message of another instance of this effskill is still running
				return true;
			}

			return false;
		}

		public bool IsSharedCooling(int skillId)
		{
			EffSkill skill = EffSkill.s_tblEffSkills.Find(iterSkill1 => EffSkill.MatchId(iterSkill1, skillId));
			if (skill == null) return false;

			if (null != m_effShareSkillInsts.Find(iterSkill0 => EffSkillInstance.MatchType(iterSkill0, skill.m_cdInfo.m_type)))
			{
				// TODO : a warning message of another instance of this effskill is still running
				return true;
			}

			return false;
		}

        public bool IsValidSkill(int skillId)
        {
            return !IsSkillCooling(skillId) && !IsSharedCooling(skillId);
        }

        public bool IsSkillCoolingByType(short type)
        {
            return m_effShareSkillInsts.Find(iterSkill0 => EffSkillInstance.MatchType(iterSkill0, type)) != null;
        }

		public bool IsAppendBuff(int buffId)
		{
			return m_effSkillBuffManager.IsAppendBuff(buffId);
		}
	}
}