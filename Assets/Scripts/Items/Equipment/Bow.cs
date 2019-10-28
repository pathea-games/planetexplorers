using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SkillAsset;
using WhiteCat;

public class Bow : ShootEquipment 
{
 	public Transform	mBowLine;
	public Transform	mShootPos;
	public int			mArrowId;
	
	public Transform	mArrowBag;
	
	
	Animator			mAnimator;
	
	Animator			_Animator
	{
		get
		{
			if(null == mAnimator)
				mAnimator = GetComponent<Animator>();
			return mAnimator;
		}
	}
	
	public override void InitEquipment (SkillRunner runner, ItemAsset.ItemObject item)
	{
		base.InitEquipment (runner, item);
		Transform[] bones = runner.GetComponentsInChildren<Transform>();
		foreach(Transform tran in bones)
		{
			if(tran.name == "Bow_box")
			{
				mArrowBag.transform.parent = tran;
				mArrowBag.transform.localPosition = Vector3.zero;
				mArrowBag.transform.localScale = Vector3.one;
				mArrowBag.transform.localRotation = Quaternion.identity;
				break;
			}
		}
	}
	
	public override void RemoveEquipment ()
	{
		base.RemoveEquipment ();
		
		if(null != mArrowBag)
			Destroy(mArrowBag.gameObject);
	}
	
	public override bool CostSkill (ISkillTarget target, int sex = 2, bool buttonDown = false, bool buttonPressed = false)
	{
        if (!base.CostSkill(target, sex, buttonDown, buttonPressed))
        {
            return false;
        }

		if(buttonDown && mShootState == ShootState.Aim)
		{
			if(mHuman.CheckAmmoCost(EArmType.Ammo, mArrowId))
			{
				if(mSkillMaleId.Count == 0 || mSkillFemaleId.Count == 0)
					return false;
					
				EffSkillInstance skillInstance = null;
				switch(sex)
				{
				case 1:
					skillInstance = CostSkill(mSkillRunner, mSkillFemaleId[0], target);
					break;
				case 2:
					skillInstance = CostSkill(mSkillRunner, mSkillMaleId[0], target);
					break;
				}
				
				if(null != skillInstance)
				{
					mHuman.ApplyDurabilityReduce(0);
					mHuman.ApplyAmmoCost(EArmType.Ammo, mArrowId);
					_Animator.SetBool("Fire", true);
					mShootState = ShootState.Fire;
					return true;
				}
			}
		}
		return false;
	}
	
	public void SetAnimatorState(string name, bool state)
	{
		_Animator.SetBool(name, state);
	}
	
	protected override void Update()
	{
		switch(mShootState)
		{
		case ShootState.Null:
			_Animator.SetBool("Fire", false);
			_Animator.SetBool("Hold", false);
			break;
		case ShootState.PutOn:
			_Animator.SetBool("Fire", false);
			_Animator.SetBool("Hold", true);
			break;
		case ShootState.Aim:
			_Animator.SetBool("Fire", false);
			_Animator.SetBool("Hold", true);
			break;
		case ShootState.Fire:
			_Animator.SetBool("Fire", true);
			_Animator.SetBool("Hold", false);
			break;
		case ShootState.Reload:
			_Animator.SetBool("Fire", false);
			_Animator.SetBool("Hold", true);
			break;
		case ShootState.PutOff:
			_Animator.SetBool("Fire", false);
			_Animator.SetBool("Hold", false);
			break;
		}
	}
}
