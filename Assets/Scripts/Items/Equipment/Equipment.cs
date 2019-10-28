using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using SkillAsset;
using SkillSystem;

public class Equipment : MonoBehaviour 
{
	[HideInInspector]
	public ItemObject 	mItemObj;
	[HideInInspector]
	public SkillRunner	mSkillRunner;
	
	protected IHuman	mHuman;

	protected bool		mMainPlayerEquipment;
	
	protected int		mSkillIndex;
	protected int		mCastSkillId;
	
	public List<int> 	mSkillMaleId;
	public List<int>	mSkillFemaleId;
	
	public EquipType	mEquipType{get{return mItemObj.protoData.equipType;}}	
	public virtual void InitEquipment(SkillRunner runner, ItemObject item)
	{
		mSkillRunner = runner;
		mHuman = mSkillRunner as IHuman;
//		InitNetworkLayer(runner.Netlayer);
		mItemObj = item;
		mSkillIndex = 0;
		//mMainPlayerEquipment = mSkillRunner == PlayerFactory.mMainPlayer;  
	}

	public virtual void InitEquipment(ItemObject item)
	{
		mItemObj = item;
	}
	
	public virtual void RemoveEquipment()
	{
		
	}
	// Use this for initialization
	public virtual bool CostSkill(ISkillTarget target = null, int sex = 2, bool buttonDown = false, bool buttonPressed = false)
	{
        LifeLimit si = mItemObj.GetCmpt<LifeLimit>();
        if (null == si)
        {
            return false;
        }
        if (si.lifePoint.current < PETools.PEMath.Epsilon)
        {
            return false;
        }

		if(mSkillMaleId.Count == 0 || mSkillFemaleId.Count == 0)
			return false;
		
        //if(mItemObj.GetProperty(ItemProperty.Durability) < PETools.PEMath.Epsilon)
        //    return false;
			
		mCastSkillId = 0;
		
		switch(sex)
		{
		case 1:
			mCastSkillId = mSkillFemaleId[0];
			mSkillIndex = 0;
			for(int i = 0; i < mSkillFemaleId.Count - 1; i++)
			{
				if(mSkillRunner.IsEffRunning(mSkillFemaleId[i]))
				{
					mCastSkillId = mSkillFemaleId[i + 1];
					mSkillIndex = i + 1;
				}
			}
			break;
		case 2:
			mCastSkillId = mSkillMaleId[0];
			mSkillIndex = 0;
			for(int i = 0; i < mSkillMaleId.Count - 1; i++)
			{
				if(mSkillRunner.IsEffRunning(mSkillMaleId[i]))
				{
					mCastSkillId = mSkillMaleId[i + 1];
					mSkillIndex = i + 1;
				}
			}
			break;
		}
		
		EffSkillInstance skillInstance = CostSkill(mSkillRunner, mCastSkillId, target);
		
		if(null != mSkillRunner && null != target && null != skillInstance && mSkillRunner != target)
		{
            Vector3 dir = target.GetPosition() - mSkillRunner.transform.position;
            mSkillRunner.transform.LookAt(mSkillRunner.transform.position + dir,Vector3.up);
		}
		
		return (null != skillInstance);
	}
	
	protected virtual EffSkillInstance CostSkill(SkillRunner coster, int id, ISkillTarget target)
	{
		return coster.RunEff(id, target);
	}

//    public bool IsChild(Transform child)
//    {
//        return child.IsChildOf(mSkillRunner.transform);
//    }

	protected virtual void CheckMainPlayerCtrl()
	{
		if(PeInput.Get (PeInput.LogicFunction.Item_Drag))
			CostSkill();
	}

	protected virtual void Update()
	{
		if(mMainPlayerEquipment)
			CheckMainPlayerCtrl();
	}

	public void TakeOffEquip() { }

	public bool CanTakeOff() { return true; }

	public bool OpEnable { get { return false; } }
	
	// 
//    internal override byte GetBuilderId()
//	{
//		return 1;
//	}
//	
//	internal override float GetAtkDist(ISkillTarget target)
//	{
//		if(mSkillRunner)
//			return mSkillRunner.GetAtkDist(target);
//		return 1f;
//	}
//	
//	internal override float GetAtk()
//	{
//		if(mSkillRunner)
//			return mSkillRunner.GetAtk();
//		return 1f;
//	}
//	
//	internal override float GetDef()
//	{
//		if(mSkillRunner)
//			return mSkillRunner.GetDef();
//		return 1f;
//	}
//	
//	internal override float GetBaseAtk()
//	{
//		if(mSkillRunner)
//			return mSkillRunner.GetBaseAtk();
//		return 1f;
//	}
//	internal override float GetDurAtk(ESkillTargetType resType)
//	{
//		if(mSkillRunner)
//			return mSkillRunner.GetDurAtk(resType);
//		return 1f;
//	}
//	
//	internal override short GetResMultiplier()
//	{
//		if(mSkillRunner)
//			return mSkillRunner.GetResMultiplier();
//		return 1;
//	}
//	
//	internal override ItemPackage GetItemPackage()
//	{
//		return null;
//	}
//	
//	internal override bool IsEnemy(ISkillTarget target)
//	{
//		if(mSkillRunner)
//			return mSkillRunner.IsEnemy(target);
//		return true;
//	}
//	
//	internal override ISkillTarget GetTargetInDist(float dist, int targetMask)
//	{
//		if(mSkillRunner)
//			return mSkillRunner.GetTargetInDist(dist, targetMask);
//		return null;
//	}
//	
//	internal override List<ISkillTarget> GetTargetlistInScope(EffScope scope, int targetMask, ISkillTarget target)
//	{
//		if(mSkillRunner)
//			return mSkillRunner.GetTargetlistInScope(scope, targetMask, target);
//        return null;
//	}
//
//	//internal abstract Get
//	// Apply changed of properties directly to 
//	internal override void ApplyDistRepel(SkillRunner caster, float distRepel)
//	{
//		if(mSkillRunner)
//			mSkillRunner.ApplyDistRepel(caster, distRepel);
//	}
//	
//	internal override void ApplyHpChange(SkillRunner caster, float hpChange, float damagePercent, int type)
//	{
//		if(mSkillRunner)
//			mSkillRunner.ApplyHpChange(caster, hpChange, damagePercent, type);
//	}
//	
//	internal override void ApplyComfortChange(float comfortChange)
//	{
//		if(mSkillRunner)
//			mSkillRunner.ApplyComfortChange(comfortChange);
//	}
//	
//	internal override void ApplySatiationChange(float satiationChange)
//	{
//		if(mSkillRunner)
//			mSkillRunner.ApplySatiationChange(satiationChange);
//	}
//	
//	internal override void ApplySound (int soundID)
//	{
//		if(mSkillRunner)
//			mSkillRunner.ApplySound (soundID);
//	}
//	
//	internal override void ApplyThirstLvChange(float thirstLvChange)
//	{
//		if(mSkillRunner)
//			mSkillRunner.ApplyThirstLvChange(thirstLvChange);
//	}
//	
//	internal override void ApplyBuffPermanent(EffSkillBuff buff)
//	{
//		if(mSkillRunner)
//			mSkillRunner.ApplyBuffPermanent(buff);
//	}
//
//	//effect and anim
//	internal override void ApplyAnim(List<string> animName)
//	{
//		if(mSkillRunner)
//			mSkillRunner.ApplyAnim(animName);
//	}
}
