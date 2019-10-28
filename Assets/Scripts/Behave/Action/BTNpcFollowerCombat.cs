using UnityEngine;
using ItemAsset;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using SkillSystem;
using PETools;
using Pathea.PeEntityExtNpcPackage;
using Pathea.PeEntityExt;

namespace Behave.Runtime
{

	[BehaveAction(typeof(BTHasSelectEnemy), "HasSelectEnemy")]
	public class BTHasSelectEnemy : BTNormal
	{
		BehaveResult Tick(Tree sender)
		{
			if(!Enemy.IsNullOrInvalid(selectattackEnemy))
			{
				return BehaveResult.Success;
			}
			else
			{
				return BehaveResult.Failure;
			}
		}
		
	}

	[BehaveAction(typeof(BTHasAttackEnemy), "hasAttackEnemy")]
	public class BTHasAttackEnemy : BTNormal
	{
		BehaveResult Tick(Tree sender)
		{
			if(!Enemy.IsNullOrInvalid(attackEnemy))
			{
				return BehaveResult.Success;
			}
			else
			{
				return BehaveResult.Failure;
			}
		}

	}

	[BehaveAction(typeof(BTHasHatredEnemies), "hasHatredEnemies")]
	public class BTHasHatredEnemies : BTNormal
	{
		BehaveResult Tick(Tree sender)
		{
			if(Enemies != null && Enemies.Count > 0)
			{
				for(int i=0;i<Enemies.Count;i++)
				{
					if(Enemy.IsNullOrInvalid(Enemies[i]))
						Enemies.Remove(Enemies[i]);
				}

				if(Enemies.Count > 0)
				   return BehaveResult.Success;
				else
					return BehaveResult.Failure;
			}
			else
			{
				return BehaveResult.Failure;
			}
		}
		
	}


	[BehaveAction(typeof(BTEquip), "Equip")]
	public class BTEquip : BTNormal
	{
		class Data
		{
//			[BehaveAttribute]
//			public int EquipType;
			[BehaveAttribute]
			public int EqCombat;
			[BehaveAttribute]
			public int EqRange;
			[BehaveAttribute]
			public int EqMelee;
			[BehaveAttribute]
			public int EqSheild;
			[BehaveAttribute]
			public int EqTool;
			[BehaveAttribute]
			public int EqEnergySheild;
			[BehaveAttribute]
			public int EqEnergy;
		}
		Data m_Data;

		void TryEquipSheild()
		{
			if(entity.NpcCmpt.EqSelect.BetterAtkObj != null)
			{
				AttackMode m_Atkmode = entity.NpcCmpt.EqSelect.BetterAtkObj.protoData.weaponInfo.attackModes[0];
				if(m_Atkmode.type == AttackType.Ranged)
					return ;
			}
			
			ItemObject m_BetterDefObj = null;
			if(entity.NpcCmpt.EqSelect.SetSelectObjsDef(entity,EeqSelect.protect))
			{
				m_BetterDefObj = entity.NpcCmpt.EqSelect.GetBetterDefObj();
			}
			
			//Def
			if(entity.motionEquipment.sheild == null && m_BetterDefObj != null)
			{
				SelectItem.EquipByObj(entity,m_BetterDefObj);
			}
			else if(entity.motionEquipment.sheild != null && m_BetterDefObj != null
			        && entity.motionEquipment.sheild.m_ItemObj != m_BetterDefObj)
			{
				SelectItem.EquipByObj(entity,m_BetterDefObj);
			}
		}
		
		void TryEquipEnergy()
		{
			//energy
			if(entity.NpcCmpt.EqSelect.SetSelectObjsEnergy(entity,EeqSelect.energy))
			{
				SelectItem.EquipByObj(entity,entity.NpcCmpt.EqSelect.GetBetterEnergyObj());
			}
			
		}
		
		void TryEquipEnergySheild()
		{
			//energy Sheild
			if(entity.NpcCmpt.EqSelect.SetSelectObjsEnergySheild(entity,EeqSelect.energy_sheild))
			{
				SelectItem.EquipByObj(entity,entity.NpcCmpt.EqSelect.GetBetterEnergySheild());
			}
		}

		BehaveResult Tick(Tree sender)
		{
			if(!GetData<Data>(sender, ref m_Data))
				return BehaveResult.Failure;

			if(Enemy.IsNullOrInvalid(selectattackEnemy))
				return BehaveResult.Failure;

			if(m_Data.EqCombat >0)
			{
				entity.NpcCmpt.EqSelect.ClearSelect();
				entity.NpcCmpt.EqSelect.ClearAtkSelects();
				if(m_Data.EqRange >0)
				{
					entity.NpcCmpt.EqSelect.SetSelectObjsAtk(entity,AttackType.Ranged);
				}

				if(m_Data.EqMelee >0)
				{
					entity.NpcCmpt.EqSelect.SetSelectObjsAtk(entity,AttackType.Melee);
				}
				entity.NpcCmpt.EqSelect.GetBetterObj(entity,selectattackEnemy,EeqSelect.combat);

				ItemObject m_BetterAtkObj = entity.NpcCmpt.EqSelect.BetterAtkObj;
				if(m_BetterAtkObj != null && entity.motionEquipment.ActiveableEquipment == null)
				{
					SelectItem.EquipByObj(entity,m_BetterAtkObj);

				}
				else if(m_BetterAtkObj != null && entity.motionEquipment.ActiveableEquipment != null
				        && entity.motionEquipment.ActiveableEquipment.m_ItemObj != m_BetterAtkObj)
				{
					SelectItem.TakeOffEquip(entity);
					SelectItem.EquipByObj(entity,m_BetterAtkObj);
				}

			}

			if(m_Data.EqSheild > 0)
				TryEquipSheild();

			if(m_Data.EqEnergySheild > 0)
				TryEquipEnergySheild();

			if(m_Data.EqEnergy > 0)
				TryEquipEnergy();

			return BehaveResult.Success;


		}
		
	}

	[BehaveAction(typeof(BTDefend), "Defend")]
	public class BTDefend : BTNormal
	{
		class Data
		{
		}
		Data m_Data;
		BehaveResult Tick(Tree sender)
		{
//			if(!GetData<Data>(sender, ref m_Data))
//				return BehaveResult.Failure;
			return BehaveResult.Failure;
		}

	}

	[BehaveAction(typeof(BTAssist0), "Assist0")]
	public class BTAssist0 : BTNormal
	{
		class Data
		{
		}
		Data m_Data;
		BehaveResult Tick(Tree sender)
		{
			//			if(!GetData<Data>(sender, ref m_Data))
			//				return BehaveResult.Failure;
			return BehaveResult.Failure;
		}
		
	}


	[BehaveAction(typeof(BTChoiceEnemy), "ChoiceEnemy")]
	public class BTChoiceEnemy : BTNormal
	{
		class Data
		{
		}
		Data m_Data;
		BehaveResult Tick(Tree sender)
		{
			if(entity.NpcCmpt == null)
				return BehaveResult.Failure;

			if(entity.NpcCmpt.BattleMgr.CanChoiceEnemy(Enemies))
			{
				SetCambat(true);
			   return BehaveResult.Success;
			}
			else
			{
				SetCambat(false);
                ClearEnemy();
				return BehaveResult.Failure;
			}
		}
		
	}

	[BehaveAction(typeof(BTCheckProfession), "CheckProfession")]
	public class BTCheckProfession : BTNormal
	{
		class Data
		{
			[BehaveAttribute]
			public int Profession;
		}
		Data m_Data;
		BehaveResult Tick(Tree sender)
		{
			if(!GetData<Data>(sender, ref m_Data))
				return BehaveResult.Failure;

			if((ENpcBattProfession)m_Data.Profession == entity.NpcCmpt.Profession)
			{
				return BehaveResult.Success;
			}
			else
			{
				return BehaveResult.Failure;
			}
		}
		
	}


	//检测怪物的伤害值决定仆从是否能去攻击
	[BehaveAction(typeof(BTEnemyDamageValue), "EnemyDamageValue")]
	public class BTEnemyDamageValue : BTNormal
	{
		class Data
		{
			[BehaveAttribute]
			public float DamageValuePer = 0.0f;
		}
		Data m_Data;

		BehaveResult Tick(Tree sender)
		{
			if (!GetData<Data>(sender, ref m_Data))
				return BehaveResult.Failure;

			if(Enemy.IsNullOrInvalid(selectattackEnemy))
				return BehaveResult.Failure;

//
//			float CurHp = GetAttribute(AttribType.Hp);
//			if(attackEnemy.entityTarget.Atk >  CurHp * m_Data.DamageValuePer)
//			{
//				Debug.Log("EnemyDamageValue    Success" );
//				return BehaveResult.Success;
//			}


			return BehaveResult.Failure;
		}

	}

	//检测玩家或者自身的条件是否能攻击
	[BehaveAction(typeof(BTOwnBlood), "OwnBlood")]
	public class BTOwnBlood : BTNormal
	{
		class Data
		{
			[BehaveAttribute]
			public float minHpPercent = 0.0f;
		}

		Data m_Data;

		BehaveResult Tick(Tree sender)
		{
			//SetCambat(false);
			if (!GetData<Data>(sender, ref m_Data))
				return BehaveResult.Failure;

			if(PeCreature.Instance.mainPlayer == null)
				return BehaveResult.Success;

			float Hppercent;
            //if(!PeGameMgr.IsMulti)
            //{
            //    Hppercent = PeCreature.Instance.mainPlayer.HPPercent;
            //    if(Hppercent < m_Data.minHpPercent)
            //    {
            //        //entity.target.ClearEnemy();
            //        Debug.Log("OwnBlood    Success" );
            //        return BehaveResult.Success;
            //    }
            //}
           
			Hppercent = entity.HPPercent;
			if(Hppercent < m_Data.minHpPercent)
			{
				//entity.target.ClearEnemy();
				Debug.Log("OwnBlood    Success" );
				return BehaveResult.Success;
			}
			
			return BehaveResult.Failure;
		}

	}

	[BehaveAction(typeof(BTIsEnemyValid), "IsEnemyValid")]
	public class BTIsEnemyValid : BTNormal
	{
		class Data
		{
			
		}
		
		BehaveResult Tick(Tree sender)
		{
			if(Enemy.IsNullOrInvalid(selectattackEnemy) || selectattackEnemy == null)
				return BehaveResult.Success;

			if(selectattackEnemy.entityTarget.target != null)
			{
				Enemy TargetEnemy = selectattackEnemy.entityTarget.target.GetAttackEnemy();
				if(TargetEnemy == null)
				{
					//entity.target.ClearEnemy();
					Debug.Log("IsEnemyValid    Success" );
					return BehaveResult.Success;
				}
				
				
				int playerID = (int)TargetEnemy.entityTarget.GetAttribute(AttribType.DefaultPlayerID);
				if (GameConfig.IsMultiClient)
				{
					if (ForceSetting.Instance.GetForceType(playerID) == EPlayerType.Human)
						return BehaveResult.Failure;
					
				}
				else
				{
					if (ForceSetting.Instance.GetForceID(playerID) == 1)
						return BehaveResult.Failure;
				}
				Debug.Log("IsEnemyValid    Success" );
			}

			return BehaveResult.Success;
		}
		
	}
	

	//选择武器:根据怪物的类型，距离，伤害选择合适的武器
	[BehaveAction(typeof(BTChoiceWeapons), "ChoiceWeapons")]
	public class BTChoiceWeapons : BTNormal
	{
		class Data
		{
			[BehaveAttribute]
			public float maxEnemyDis;
			[BehaveAttribute]
			public float minFollowerHpPer;
			[BehaveAttribute]
			public float compareHpPer;

		}

		Data m_Data;

		void EquipSheild()
		{
			if(entity.NpcCmpt.EqSelect.BetterAtkObj != null)
			{
				AttackMode m_Atkmode = entity.NpcCmpt.EqSelect.BetterAtkObj.protoData.weaponInfo.attackModes[0];
				if(m_Atkmode.type == AttackType.Ranged)
					return ;
			}
				
			ItemObject m_BetterDefObj = null;
			if(entity.NpcCmpt.EqSelect.SetSelectObjsDef(entity,EeqSelect.protect))
			{
				m_BetterDefObj = entity.NpcCmpt.EqSelect.GetBetterDefObj();
			}

            if (m_BetterDefObj == null)
                return;

            ItemAsset.Equip equip = m_BetterDefObj.GetCmpt<ItemAsset.Equip>();
            if (null == equip)
                return;

            bool HasEquipUsed = false;
            for (int i = 0; i < entity.equipmentCmpt._ItemList.Count; ++i)
            {
                ItemObject item = entity.equipmentCmpt._ItemList[i];
                ItemAsset.Equip existEquip = item.GetCmpt<ItemAsset.Equip>();
                if (null != existEquip)
                {
                    if (System.Convert.ToBoolean(equip.equipPos & existEquip.equipPos))
                    {
                        HasEquipUsed = true;
                        break;
                    }
                }
            }

            if (HasEquipUsed && entity.motionEquipment.sheild == null)
                return;
			//Def
            if ((!HasEquipUsed && m_BetterDefObj != null))// entity.motionEquipment.sheild == null
			{
				SelectItem.EquipByObj(entity,m_BetterDefObj);
			}
            else if ((HasEquipUsed && m_BetterDefObj != null && entity.motionEquipment.sheild.m_ItemObj != m_BetterDefObj))
			{
				SelectItem.EquipByObj(entity,m_BetterDefObj);
			}
		}

		void EquipEnergy()
		{
			//energy
			if(entity.NpcCmpt.EqSelect.SetSelectObjsEnergy(entity,EeqSelect.energy))
			{
				SelectItem.EquipByObj(entity,entity.NpcCmpt.EqSelect.GetBetterEnergyObj());
			}

		}

		void EquipEnergySheild()
		{
			//energy Sheild
			if(entity.NpcCmpt.EqSelect.SetSelectObjsEnergySheild(entity,EeqSelect.energy_sheild))
			{
				SelectItem.EquipByObj(entity,entity.NpcCmpt.EqSelect.GetBetterEnergySheild());
			}
		}

		BehaveResult Tick(Tree sender)
		{
			if (!GetData<Data>(sender, ref m_Data))
				return BehaveResult.Failure;

			if(Enemy.IsNullOrInvalid(selectattackEnemy))
				return BehaveResult.Failure;

			if(selectattackEnemy.entityTarget == null)//||  selectattackEnemy.entityTarget.target == null)
				return BehaveResult.Failure;

		
			entity.NpcCmpt.EqSelect.ClearSelect();
			entity.NpcCmpt.EqSelect.ClearAtkSelects();
			if(entity.NpcCmpt.EqSelect.SetSelectObjsAtk(entity,EeqSelect.combat))
			{
				entity.NpcCmpt.EqSelect.GetBetterAtkObj(entity,selectattackEnemy);
			}

			EquipEnergy();
			EquipSheild();
			EquipEnergySheild();
			return BehaveResult.Success;
		}
		
	}

	[BehaveAction(typeof(BTIsUnSuitable), "IsUnSuitable")]
	public class BTIsUnSuitable : BTNormal
	{
		class Data
		{

		}

		BehaveResult Tick(Tree sender)
		{
			if(Enemy.IsNullOrInvalid(selectattackEnemy))
				return BehaveResult.Failure;

			if(selectattackEnemy.entityTarget.Field == MovementField.Sky)
			{
				if(entity.NpcCmpt.EqSelect.BetterAtkObj == null)
				{
					//Enemies.Remove(selectattackEnemy);
					return BehaveResult.Success;
				}
				
				AttackMode mode = entity.NpcCmpt.EqSelect.BetterAtkObj.protoData.weaponInfo.attackModes[0];
				if(mode.type != AttackType.Ranged)
				{
					//Enemies.Remove(selectattackEnemy);
					return BehaveResult.Success;
				}
			}
			return BehaveResult.Failure;
		}
	}

	[BehaveAction(typeof(BTAttcakingAway), "AttcakingAway")]
	public class BTAttcakingAway : BTNormal
	{
		class Data
		{
			[BehaveAttribute]
			public float awayTime;
			[BehaveAttribute]
			public float awayRadiu;

			public float StartTime;
		}
		Data m_Data;

		bool InRadiu(Vector3 self,Vector3 target,float radiu)
		{
			float sqrDistanceH = PEUtil.SqrMagnitudeH(self, target);
			return sqrDistanceH < radiu * radiu;
		}
		
		void DoStep()
		{
			Vector3 dir = position - selectattackEnemy.position;
			PEActionParamV paramV = PEActionParamV.param;
			paramV.vec = dir;
			DoAction(PEActionType.Step,paramV);
		}
		
		void DoSheid()
		{
			DoAction(PEActionType.HoldShield);
			Vector3 dir = selectattackEnemy.position - position ;
			FaceDirection(dir);
		}
		
		bool CanStep()
		{
			Vector3 dir = position - selectattackEnemy.position;
			PEActionParamV param = PEActionParamV.param;
			param.vec = dir;
			return InRadiu(position, selectattackEnemy.position,3.0f) && CanDoAction(PEActionType.Step,param); 
		}
		
		void RunAway()
		{
			Vector3 dir = position - selectattackEnemy.position;
			Vector3 pos = position + dir * 3.0f;
			MoveToPosition(pos,SpeedState.Sprint);
		}

		BehaveResult Init(Tree sender)
		{
			if (!GetData<Data>(sender, ref m_Data))
				return BehaveResult.Failure;

			m_Data.StartTime = Time.time;
			return BehaveResult.Running;
		}

		BehaveResult Tick(Tree sender)
		{
			if(Time.time - m_Data.StartTime > m_Data.awayTime)
				return BehaveResult.Success;

			if(Enemy.IsNullOrInvalid(selectattackEnemy))
				return BehaveResult.Failure;

			bool canShield = CanDoAction(PEActionType.HoldShield);
			bool canStep = CanStep();
			if(!canShield && !canStep)
			{
				RunAway();
			}
			
			if(!canShield && canStep)
			{
				DoStep();
				
			}
			
			if(canShield && !canStep)
			{
				DoSheid();
			}
			
			
			if(canShield && canStep)
			{
				if(Random.value > 0.5f)
				{
					DoStep();
				}
				else
				{
					DoSheid();
				}
				
			}

//			if(entity.target != null && entity.target.beSkillTarget)
//			{
//				Vector3 dr = position - selectattackEnemy.position;
//				dr.y = 0;
//				Vector3 v3 = position + dr*m_Data.awayRadiu;
//				MoveToPosition(v3,SpeedState.Run);
//			}

			return BehaveResult.Running;
			
		}
	}


	[BehaveAction(typeof(BTSelectEquip_Range), "IsSelectEquipRange")]
	public class BTSelectEquip_Range : BTNormal
	{
		class Data
		{
			
		}
		
		BehaveResult Tick(Tree sender)
		{
			if(entity.NpcCmpt.EqSelect.BetterAtkObj == null)
				return BehaveResult.Failure;

			AttackMode mode = entity.NpcCmpt.EqSelect.BetterAtkObj.protoData.weaponInfo.attackModes[0];
			if(mode.type == AttackType.Ranged)
			{
				return BehaveResult.Success;
			}
			else
			{
				return BehaveResult.Failure;
			}

		}
		
	}

	[BehaveAction(typeof(BTSelectEquip_Melee), "IsSelectEquipclose")]
	public class BTSelectEquip_Melee : BTNormal
	{
		class Data
		{
			
		}
		
		BehaveResult Tick(Tree sender)
		{
			if(entity.NpcCmpt.EqSelect.BetterAtkObj == null)
				return BehaveResult.Failure;
			
			AttackMode mode = entity.NpcCmpt.EqSelect.BetterAtkObj.protoData.weaponInfo.attackModes[0];
			if(mode.type == AttackType.Melee)
			{
				return BehaveResult.Success;
			}
			else
			{
				return BehaveResult.Failure;
			}
			
		}
		
	}

	[BehaveAction(typeof(BTIsOnlyGloves), "IsOnlyGloves")]
	public class BTIsOnlyGloves : BTNormal
	{
		class Data
		{
			
		}
		
		BehaveResult Tick(Tree sender)
		{
			if(entity.NpcCmpt.EqSelect.BetterAtkObj == null)
			{
				if(entity.motionEquipment.axe != null && entity.motionEquipment.axe is PEChainSaw)
				{
					SelectItem.TakeOffEquip(entity);
				}else if(entity.motionEquipment.digTool != null && entity.motionEquipment.digTool is PECrusher)
				{
					SelectItem.TakeOffEquip(entity);
				}

				return BehaveResult.Success;
			}
		     else
				return BehaveResult.Failure;
		}
		
	}

	[BehaveAction(typeof(BTWeaponry), "Weaponry")]
	public class BTWeaponry : BTNormal
	{
		class Data
		{

		}

		Data m_Data;
		//float startTime = 0.0f;
		//float waitTime = 3.0f;

		ItemObject m_BetterAtkObj;
		//AttackMode m_Atkmode;

		ItemObject m_BetterDefObj;
		BehaveResult Tick(Tree sender)
		{
			if(entity.NpcCmpt.EqSelect.BetterAtkObj == null)
				return BehaveResult.Success;

			m_BetterAtkObj = entity.NpcCmpt.EqSelect.BetterAtkObj;
			//m_Atkmode = entity.NpcCmpt.EqSelect.BetterAtkObj.protoData.weaponInfo.attackModes[0];	

			//Atk
			if(m_BetterAtkObj != null && entity.motionEquipment.ActiveableEquipment == null)
			{
				SelectItem.EquipByObj(entity,m_BetterAtkObj);
				return BehaveResult.Running;
			}
			else if(m_BetterAtkObj != null && entity.motionEquipment.ActiveableEquipment != null
			        && entity.motionEquipment.ActiveableEquipment.m_ItemObj != m_BetterAtkObj)
			{
				SelectItem.EquipByObj(entity,m_BetterAtkObj);
				return BehaveResult.Running;
			}
			else
			{
				return BehaveResult.Success;
			}
		}
		
	}

	[BehaveAction(typeof(BTRunawayFromEnemy), "RunawayFromEnemy")]
	public class BTRunawayFromEnemy : BTNormal
	{
		class Data
		{
			[BehaveAttribute]
			public float RunRadius;
			[BehaveAttribute]
			public float minHpPercent;
		}
		Data m_Data;
		FindHidePos mFind;
		float startRunTime = 0.0f;
		float CHECK_TIME = 10.0f;

		float startHideTime = 0.0f;
		float CHECK_Hide_TIME = 1.0f;

		BehaveResult Init(Tree sender)
		{
			if (!GetData<Data>(sender, ref m_Data))
				return BehaveResult.Failure;

            mFind = new FindHidePos(m_Data.RunRadius, false, m_Data.RunRadius);
			startRunTime  = Time.time;
			//make sure frist run time
			//startHideTime = Time.time;
			SetCambat(false);
			return BehaveResult.Running;
		}

		BehaveResult Tick(Tree sender)
		{

			if(Time.time - startHideTime > CHECK_Hide_TIME)
			{
				Vector3 dir = mFind.GetHideDir(PeCreature.Instance.mainPlayer.peTrans.position,position,Enemies);
				if(mFind.bNeedHide)
				{
                    Vector3 dirPos = position + dir.normalized * m_Data.RunRadius;
					MoveToPosition(dirPos,SpeedState.Run);
				}
				else
				{
                    StopMove();
                    FaceDirection(PeCreature.Instance.mainPlayer.peTrans.position - position);
                    //if(entity.target.beSkillTarget)
                    //{
                    //    MoveDirection(transform.right,SpeedState.Run);
                    //}
                    //else
                    //{
                    //    StopMove();
                    //    FaceDirection(PeCreature.Instance.mainPlayer.peTrans.position - position);
                    //}

				}

				startHideTime = Time.time;
			}

            if (Time.time - startRunTime > CHECK_TIME || entity.HPPercent > m_Data.minHpPercent 
                || ItemAsset.SelectItem.HasCanEquip(entity,EeqSelect.combat,AttackType.Ranged))
			{
				SetCambat(true);
				return BehaveResult.Success;
			}

			//吃到了合适的属性值或者没有合适的药品时停止
			ItemAsset.ItemObject mEatItem;
			if(NpcEatDb.IsContinueEat(entity,out mEatItem))
			{
				if(entity.UseItem.GetCdByItemProtoId(mEatItem.protoId) < PETools.PEMath.Epsilon)
				{
					UseItem(mEatItem);
				}
				
			}

			return BehaveResult.Running;
		}

		void Reset(Tree sender)
		{
			startHideTime = 0;
		}
	}

    [BehaveAction(typeof(BTRunawayFromSkyEnemy), "RunawayFromSkyEnemy")]
    public class BTRunawayFromSkyEnemy : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float RunRadius;
            [BehaveAttribute]
            public float minHpPercent;

			public float minRadius = 32.0f;
        }
        Data m_Data;

        Vector3 runPos;
        //float startRunTime = 0.0f;
        //float CHECK_TIME = 10.0f;

        //float startHideTime = 0.0f;
        //float CHECK_Hide_TIME = 1.0f;

        void OnPathComplete(Pathfinding.Path path)
        {
            if (path != null && path.vectorPath.Count > 0)
            {
                Vector3 pos = path.vectorPath[path.vectorPath.Count - 1];
                runPos = pos;
            }
        }

        Vector3 GetPatrolPosition(Vector3 center, Vector3 direction, float minRadius, float maxRadius)
        {
			if (AstarPath.active != null)//PEUtil.IsInAstarGrid(position))
            {
                Pathfinding.RandomPath path = Pathfinding.RandomPath.Construct(position, (int)Random.Range(minRadius, maxRadius) * 100, OnPathComplete);
                path.spread = 40000;
                path.aimStrength = 1f;
                path.aim = PEUtil.GetRandomPosition(position, direction, minRadius, maxRadius, -75.0f, 75.0f);
                AstarPath.StartPath(path);

                return Vector3.zero;
            }
            return Vector3.zero;
        }

        Vector3 GetRunDir(PeEntity npc,float radius = 32.0f)
        {
            Vector3 dir = Vector3.zero;
            for (int i = 0; i < Enemies.Count;i++)
            {
                float d= PETools.PEUtil.Magnitude(npc.position,Enemies[i].position);
                if (d > radius)
                    continue;

                dir += (npc.position - Enemies[i].position).normalized * (1.0f - d / radius); 
            }

            return dir;
        }

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (Enemy.IsNullOrInvalid(selectattackEnemy))
                return BehaveResult.Failure;

            //startRunTime = Time.time;
            //startHideTime = Time.time;
            SetCambat(false);

            Vector3 v = GetRunDir(entity);
            if (v == Vector3.zero)
                return BehaveResult.Failure;
            //Vector3 v = Vector3.ProjectOnPlane(position - selectattackEnemy.position, Vector3.up).normalized;

            GetPatrolPosition(position,v,32.0f,64.0f);

            if (selectattackEnemy.entityTarget.Field == MovementField.Sky)
                m_Data.minRadius = 32.0f;
            else if (selectattackEnemy.entityTarget.IsBoss)
                m_Data.minRadius = 128.0f;
            else
				m_Data.minRadius = 16.0f;
            //runPos = PEUtil.GetRandomPosition(position, v, 5.0f, 10.0f, -45f, 45f);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {            
            bool matchEnemy = false;
			if(entity.NpcCmpt.BattleMgr.CanChoiceEnemy(Enemies) && !Enemy.IsNullOrInvalid(selectattackEnemy))
			{
                matchEnemy = SelectItem.MatchEnemyAttack(entity, selectattackEnemy.entityTarget);
            }

			if (Enemy.IsNullOrInvalid(selectattackEnemy))
			{
				SetCambat(true);
				return BehaveResult.Success;
			}

            if (!IsReached(position, selectattackEnemy.position, false, m_Data.minRadius) || matchEnemy)
            {
                SetCambat(true);
                return BehaveResult.Success;
            }


            if (IsReached(runPos, position, false) || Stucking())
            {
                Vector3 v = GetRunDir(entity);
				GetPatrolPosition(position,v,32.0f,64.0f);
            }
           
            MoveToPosition(runPos, SpeedState.Run);
            return BehaveResult.Running;
        }

    }

	[BehaveAction(typeof(BTAttackEnemyIsAttacking),"AttackEnemyIsAttacking")]
	public class BTAttackEnemyIsAttacking : BTNormal
	{

        bool IsInEnemyFoward(Enemy enemy,PeEntity self)
        {
            Vector3 selfPos = self.position;
            Vector3 targetPos = enemy.position;

            Vector3 forward = enemy.entityTarget.peTrans.forward;
            Vector3 vec = (selfPos -targetPos).normalized ;

            float ang = Mathf.Abs(PETools.PEUtil.Angle(forward, vec));

            return ang <= 90.0f;
        }
		BehaveResult Tick(Tree sender)
		{
			if(Enemy.IsNullOrInvalid(selectattackEnemy))
				return BehaveResult.Failure;

			if(selectattackEnemy.entityTarget == null ||  selectattackEnemy.entityTarget.target == null)
				return BehaveResult.Failure;

			if(selectattackEnemy.entityTarget.target != null)
			{
				Enemy TargetEnemy = selectattackEnemy.entityTarget.target.GetAttackEnemy();
				if(TargetEnemy == null)
					return BehaveResult.Failure;
				
				
				if (selectattackEnemy.entityTarget.IsAttacking && TargetEnemy.entityTarget == entity && IsInEnemyFoward(selectattackEnemy,entity))//) //&& Random.value >= 0.3f
				{
					return BehaveResult.Success;
				}
			}


			return BehaveResult.Failure;
		}
	}


	[BehaveAction(typeof(BTChoiceAction),"ChoiceAction")]
	public class BTChoiceAction : BTNormal
	{

		bool IsInEnemyFoward(Enemy enemy,PeEntity self)
		{
			Vector3 selfPos = self.position;
			Vector3 targetPos = enemy.position;
			
			Vector3 forward = enemy.entityTarget.peTrans.forward;
			Vector3 vec = (selfPos -targetPos).normalized ;
			
			float ang = Mathf.Abs(PETools.PEUtil.Angle(forward, vec));
			
			return ang <= 90.0f;
		}

		bool InRadiu(Vector3 self,Vector3 target,float radiu)
		{
			float sqrDistanceH = PEUtil.SqrMagnitudeH(self, target);
			return sqrDistanceH < radiu * radiu;
		}

		void DoStep()
		{
			Vector3 dir = position - selectattackEnemy.position;
			PEActionParamV paramV = PEActionParamV.param;
			paramV.vec = dir;
			DoAction(PEActionType.Step,paramV);
		}

		void DoSheid()
		{
			DoAction(PEActionType.HoldShield);
			Vector3 dir = selectattackEnemy.position - position ;
			FaceDirection(dir);
		}

		bool EndSheid()
		{
			if(IsMotionRunning(PEActionType.HoldShield))
			{
				EndAction(PEActionType.HoldShield);
			}
			return !IsMotionRunning(PEActionType.HoldShield);
		}

		bool CanStep()
		{
			Vector3 dir = position - selectattackEnemy.position;
			PEActionParamV param = PEActionParamV.param;
			param.vec = dir;
			return InRadiu(position, selectattackEnemy.position,3.0f) && IsInEnemyFoward(selectattackEnemy,entity) && CanDoAction(PEActionType.Step,param); 
		}

        static float X = 3.0f;
		bool RunAway()
		{
            if (InRadiu(entity.position, selectattackEnemy.position, X))
			{
				Vector3 dir = position - selectattackEnemy.position;
				dir.y = 0;
                Vector3 pos = position + dir * X;
				MoveToPosition(pos,SpeedState.Sprint);
                return true;
			}
            return false;

		}

		float mStartActionTime;
		float mActiomTime = 1.0f;

		BehaveResult Init(Tree sender)
		{
			if(Enemy.IsNullOrInvalid(selectattackEnemy))
				return BehaveResult.Failure;

			mStartActionTime = Time.time;
			return BehaveResult.Running;
		}

		BehaveResult Tick(Tree sender)
		{
			if(Enemy.IsNullOrInvalid(selectattackEnemy))
			{
				if(EndSheid())
					return BehaveResult.Success;
				else
					return BehaveResult.Running;
			}

			bool canShield = CanDoAction(PEActionType.HoldShield);
			bool canStep = CanStep();
            if (!canShield && !canStep)
            {
                if (RunAway())
                    return BehaveResult.Running;
                else
                    return BehaveResult.Success;
            }

			if(!canShield && canStep)
			{
				DoStep();

			}

			if(canShield && !canStep)
			{
				DoSheid();
			}


			if(canShield && canStep)
			{
				if(Random.value > 0.5f)
				{
					DoStep();
				}
				else
				{
					DoSheid();
				}

			}

			if(Time.time - mStartActionTime > mActiomTime)
			{
				if(EndSheid())
					return BehaveResult.Success;
				else
					return BehaveResult.Running;
			}
			else
				return BehaveResult.Running;

		}
	}


	[BehaveAction(typeof(BTRadiusStep),"RadiusStep")]
	public class BTRadiusStep : BTNormal
	{
		class Data
		{
			[BehaveAttribute]
			public float minRadius;
		}
		Data m_Data;

		void DoStep()
		{
			Vector3 dir = position - selectattackEnemy.position;
			PEActionParamV paramV = PEActionParamV.param;
			paramV.vec = dir;
			DoAction(PEActionType.Step,paramV);
		}

		bool InRadiu(Vector3 self,Vector3 target,float radiu)
		{
			float sqrDistanceH = PEUtil.SqrMagnitudeH(self, target);
			return sqrDistanceH < radiu * radiu;
		}

		BehaveResult Tick(Tree sender)
		{
			if(!GetData<Data>(sender,ref m_Data))
				return BehaveResult.Failure;

			if(Enemy.IsNullOrInvalid(selectattackEnemy))
				return BehaveResult.Failure;

			if(selectattackEnemy.entityTarget.Field != MovementField.Sky && InRadiu(position ,selectattackEnemy.position,m_Data.minRadius))
			{
				DoStep();
			}
			return BehaveResult.Success;
		}
	}


	[BehaveAction(typeof(BTCheckEquips),"CheckEquips")]
	public class BTCheckEquips : BTNormal
	{
		class Data
		{
			[BehaveAttribute]
			public int EqType;
			[BehaveAttribute]
			public int AttackType;
		}
		Data m_Data;

		BehaveResult Tick(Tree sender)
		{
			if (!GetData<Data>(sender, ref m_Data))
				return BehaveResult.Failure;

			if(SelectItem.HasCanEquip(entity,(EeqSelect)m_Data.EqType ,(AttackType)m_Data.AttackType))
				return BehaveResult.Success;
			else
				return BehaveResult.Failure;
		}

	}

    [BehaveAction(typeof(BTCheckEnemy),"CheckEnemy")]
	public class BTCheckEnemy : BTNormal
	{
		class Data
		{
			[BehaveAttribute]
			public int Type;
		}
		Data m_Data;

        bool CheckField(Enemy enemy)
        { 
        switch (m_Data.Type)
        {
            case 1: return enemy.entityTarget == null || enemy.entityTarget.Field == MovementField.Sky || enemy.entityTarget.IsBoss;
            default: break;
        }
        return false;
        }
		BehaveResult Tick(Tree sender)
		{
			if (!GetData<Data>(sender, ref m_Data))
				return BehaveResult.Failure;

			if(Enemy.IsNullOrInvalid(selectattackEnemy))
				return BehaveResult.Failure;

			if(entity.IsMotorNpc)
				return BehaveResult.Failure;

            if (CheckField(selectattackEnemy))
                return BehaveResult.Success;
            else
                return BehaveResult.Failure;

		}

	}


    
}
