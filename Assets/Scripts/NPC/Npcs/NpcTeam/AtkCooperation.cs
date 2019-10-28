using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Pathea
{
	public class AtkCooperation : Cooperation
	{
		List<PeEntity> mAtkTarget;
		int            mAtkNum;
		
		public AtkCooperation(int memberNum,int atkNumber):base(memberNum)
		{
			mAtkNum = atkNumber;
			mAtkTarget = new List<PeEntity>(mAtkNum);
			
		}
		
		public override bool AddMember (PeEntity entity)
		{
			if(base.AddMember (entity))
			{
				if(entity.NpcCmpt != null)
				   entity.NpcCmpt.SetLineType(ELineType.TeamAtk);

				return true;
			}
			return false;
		}
		
		public override void DissolveCooper ()
		{
			for(int i=0;i<mCooperMembers.Count;i++)
			{
				mCooperMembers[i].NpcCmpt.BattleMgr.SetSelectEnemy(null);
				mCooperMembers[i].target.SetEnityCanAttack(true);// used false
			}
			mCooperMembers.Clear();
			mAtkTarget.Clear();
		}
		
		public override bool CanAddMember (params object[] objs)
		{
			if(objs != null && objs.Length >0)
			{
				PeEntity target = (PeEntity)objs[0];
				return base.CanAddMember() && mAtkTarget.Contains(target);
			}
			return base.CanAddMember();
		}
		
		
		
		#region	Atk Fun
		public List<PeEntity> GetAtkCooperMembers()
		{
			return mCooperMembers;
		}

		public List<PeEntity> GetAktCooperTarget()
		{
			return mAtkTarget;
		}
		//设置攻击目标
		public void SetAtkTarget(PeEntity target)
		{
			for(int i=0 ;i < mCooperMembers.Count;i++)
			{
				 mCooperMembers[i].NpcCmpt.BattleMgr.ChoiceTheEnmey(mCooperMembers[i],target);
			}
		}
		
		//添加攻击目标
		public void AddAktTarget(PeEntity target)
		{
			if(HasBeTarget(target))
				return;

            mAtkTarget.Clear();
			SetAtkTarget(target);
			mAtkTarget.Add(target);
			return;

		}
		
		public bool RomoveAtkTarget(PeEntity target)
		{
			return mAtkTarget.Remove(target);
		}
		
		
		public void  OnAtkTargetDeath(SkillSystem.SkEntity skSelf, SkillSystem.SkEntity skCaster)
		{
			PeEntity self = skSelf.GetComponent<PeEntity>();
			RemoveEnemy(self);
			RomoveAtkTarget(self);
			ClearCooper();
			
		}
		
		public void OnAtkTargetDestroy(SkillSystem.SkEntity entity)
		{
			PeEntity peEntity = entity.GetComponent<PeEntity>();
			
			RemoveEnemy(peEntity);
			RomoveAtkTarget(peEntity);
		}
		
		public void OnAtkTargetLost(PeEntity entity)
		{
			RemoveEnemy(entity);
			RomoveAtkTarget(entity);
		}
		
		public bool HasBeTarget(PeEntity target)
		{
			return mAtkTarget.Contains(target);
		}
		
		public bool CanBeTarget(PeEntity target)
		{
			return  mAtkTarget.Count < mAtkNum;
		}
		
		void RemoveEnemy(PeEntity enmey)
		{
			if(mCooperMembers != null)
			{
				for(int i=0 ;i < mCooperMembers.Count;i++)
				{
					if(mCooperMembers[i] != null && mCooperMembers[i].NpcCmpt != null && 
					   !Enemy.IsNullOrInvalid(mCooperMembers[i].NpcCmpt.BattleMgr.choicedEnemy) 
					   && mCooperMembers[i].NpcCmpt.BattleMgr.choicedEnemy.entityTarget == enmey)
					{
						mCooperMembers[i].NpcCmpt.BattleMgr.SetSelectEnemy(null);
                        mCooperMembers[i].target.SetEnityCanAttack(true); //// used false
					}
				}
			}
		}
		
		Enemy GetEnemy(PeEntity self,PeEntity target)
		{
			List<Enemy> enemies = self.target.GetEnemies();
			for(int i=0;i<enemies.Count;i++)
			{
				if(enemies[i].entityTarget == target)
					return enemies[i];
			}
			return null;
		}
		#endregion
	}
}

