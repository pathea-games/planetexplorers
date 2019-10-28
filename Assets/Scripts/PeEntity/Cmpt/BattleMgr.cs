using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using Pathea.Operate;
using PETools;
using SkillSystem;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtTrans;
using Mono.Data.SqliteClient;

namespace Pathea
{	
	public class BattleMgr 
	{
		static float  MaxDis = 128.0f;
		PeEntity mSelf;
		Enemy mChoicedEnemy;
        public Enemy choicedEnemy { get {
            if (!Enemy.IsNullOrInvalid(mChoicedEnemy))
                mChoicedEnemy.Update();

            return mChoicedEnemy; 
        } }
		public BattleMgr(PeEntity entity)
		{
			mSelf = entity;
		}

		private float CalculateWeight(float _hp,float _dis)
		{
			float  enemyHp =_hp;
			float dis;
			if(_dis >MaxDis)
				dis = MaxDis;
			else
				dis = _dis;
			
			float  enemyDispcent = 1.0f - dis/MaxDis;
			return 0.3f * enemyHp +  0.7f * enemyDispcent;
		}

		public  Enemy CompareEnemy(Enemy one,Enemy other)
		{
			float  oneCnt = CalculateWeight(one.entity.HPPercent,PETools.PEUtil.MagnitudeH(mSelf.position,one.position));
			float  otherinfo = CalculateWeight(other.entity.HPPercent,PETools.PEUtil.MagnitudeH(mSelf.position,other.position));
			if(oneCnt <= otherinfo)
				return one;
			else
				return other;

		}

		public  Enemy ChoiceEnemy(List<Enemy> enemies)
		{
			if(enemies == null || enemies.Count <= 0)
				return null;

			//bool hasRangeEquip = SelectItem.HasCanEquip(mSelf,EeqSelect.combat,AttackType.Ranged);
			Enemy tempE = enemies[0];
			for(int i=0;i < enemies.Count;i++)
			{
				if(!Enemy.IsNullOrInvalid(enemies[i]) && enemies[i].canAttacked)
				{
					bool enemyMacth =  SelectItem.MatchEnemyAttack(mSelf,enemies[i].entityTarget);//hasRangeEquip ? true : SelectItem.MatchEnemyEquip(mSelf,enemies[i].entityTarget);
					if(enemyMacth  && enemies[i].Distance <= MaxDis && EnmeyTargetIsAlliance(enemies[i]))
						tempE = CompareEnemy(tempE,enemies[i]);
				}

			}

			return !Enemy.IsNullOrInvalid(tempE) && !tempE.entityTarget.isRagdoll && tempE.Distance <= MaxDis  ? tempE : null;
		}

		public void SetSelectEnemy(Enemy one)
		{
			mChoicedEnemy = one;
		}

		public bool ChoiceTheEnmey(PeEntity self,PeEntity target)
		{
			//
			List<Enemy> enemies = self.target.GetEnemies();
			for(int i=0;i<enemies.Count;i++)
			{
				if(enemies[i].entityTarget == target)
				{
					mChoicedEnemy = enemies[i];
					return true;
				}
			}

           self.target.AddSharedHatred(target, 5.0f);// new Enemy(self, target, 5.0f); ;
			return false;
		}

		bool EnmeyTargetIsAlliance(Enemy enemy)
		{
			if(enemy.entityTarget == null || enemy.entityTarget.target == null)
				return true;

			Enemy TargetEnemy = enemy.entityTarget.target.GetAttackEnemy();
			if(TargetEnemy == null)
				return true;

			int playerID = (int)TargetEnemy.entityTarget.GetAttribute(AttribType.DefaultPlayerID);
			if (GameConfig.IsMultiClient)
			{
				if (ForceSetting.Instance.GetForceType(playerID) == EPlayerType.Human)
					return true;
				
			}
			else
			{
				if (ForceSetting.Instance.GetForceID(playerID) == 1)
					return true;
			}
			return false;
		}

		public  bool CanChoiceEnemy(List<Enemy> enemies)
		{
			mChoicedEnemy = ChoiceEnemy(enemies);
			return mChoicedEnemy != null;
		}


	}

}
