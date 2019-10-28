using UnityEngine;
using System;
using System.Collections.Generic;
using SkillSystem;

namespace Pathea
{
	public enum AttribType
    {//不要大于127！！！！！
        HpMax = 0,
		Hp,
		StaminaMax,
		Stamina,
		StaminaReducePercent,
		StaminaRecoverSpeed,	//5
		StaminaRecoverDelay,
		ComfortMax,
		Comfort,
		ComfortReducePercent,
		ComfortRecoverSpeed,	//10
		OxygenMax,
		Oxygen,
		OxygenReducePercent,
		OxygenRecoverSpeed,
		HungerMax,				//15
		Hunger,
		HungerReducePercent,
		HungerRecoverSpeed,
		Energy,
		EnergyMax,				//20
		ResDamage,
		ResRange,
		ResBouns,
		AtkRange,
		Atk,					//25
		CritPercent,
		Def,
		RigidMax,
		Rigid,
		RigidRecoverSpeed,		//30
		Shield,
		ShieldMax,
		ShieldMeleeProtect,
		ShieldRigidProtect,
		EnableShieldBlock,		//35
		RunSpeed,
		WalkSpeed,
		JumpHeight,
		WeightUpUnderWater,
		HitflyMax,				//40
		Hitfly,
		HitflyRecoverSpeed,
		Clean,
		CleanRecover,
		Food_Cereals,			//45
		Food_Meat,
		Food_Bean,
		Food_Fruit,
		Food_Energy,
		DebuffPercent01,		//50
		DebuffReduce01,
		DebuffPercent02,
		DebuffReduce02,
		DebuffPercent03,
		DebuffReduce03,			//55
		DebuffPercent04,
		DebuffReduce04,
		DebuffPercent05,
		DebuffReduce05,
		DebuffPercent06,		//60
		DebuffReduce06,
		DebuffPercent07,
		DebuffReduce07,
		DebuffPercent08,
		DebuffReduce08,			//65
		DebuffPercent09,
		DebuffReduce09,
		DebuffPercent10,
		DebuffReduce10,
		ThresholdWhacked,		//70
		ThresholdRepulsed,
		ThresholdWentfly,
		ThresholdKnocked,
		Exp,
		CutDamage,				//75
		CutBouns,
		SwordMultiple,
		ShieldMultiple,
		AxeMultiple,
		BowMultiple,			//80
		GunMultiple,
		ForceScale,
		AttNum,					
		ExpMax,
		SprintSpeed,			//85
		RotationSpeed,
		VisualRadius,
		VisualAngle,
		ListenRadius,
		RetreatSpeed,			//90
		DefaultPlayerID,
		CampID,
		DeleteTime,
		InjuredState,
		DamageID,				//95
		HPRecover,
        Max//不要大于127！！！！！						
    }

	public class AttribPair
	{
		public AttribType 	m_Current;
		public AttribType	m_Max;

		public AttribPair(AttribType current, AttribType max)
		{
			m_Current = current;
			m_Max = max;
		}
		
		public void CheckAttrib(PESkEntity entity, AttribType type, float value)
		{
			if(type == m_Current)
			{
				if(value < 0 || value > entity.GetAttribute(m_Max))
					entity.SetAttribute(m_Current, Mathf.Clamp(value, 0, entity.GetAttribute(m_Max)));
			}
			else if(type == m_Max)
			{
				float currentValue = entity.GetAttribute(m_Current);
				if(currentValue > value)
					entity.SetAttribute(m_Current, value);
			}
		}
	}

	public class AttribInspectoscope : PESingleton<AttribInspectoscope>
	{
		AttribPair[] m_AttribPairs;

		public AttribInspectoscope()
		{
			InitPair();
		}

		public void CheckAttrib(PESkEntity entity, AttribType type, float value)
		{
			if (m_AttribPairs[(int)type] != null) {
				m_AttribPairs [(int)type].CheckAttrib (entity, type, value);
			}
		}

		void InitPair()
		{
			m_AttribPairs = new AttribPair[(int)AttribType.Max]; 
			m_AttribPairs[(int)AttribType.Hp] 		= m_AttribPairs[(int)AttribType.HpMax] 		= new AttribPair(AttribType.Hp, AttribType.HpMax);
			m_AttribPairs[(int)AttribType.Stamina] 	= m_AttribPairs[(int)AttribType.StaminaMax] = new AttribPair(AttribType.Stamina, AttribType.StaminaMax);
			m_AttribPairs[(int)AttribType.Comfort] 	= m_AttribPairs[(int)AttribType.ComfortMax] = new AttribPair(AttribType.Comfort, AttribType.ComfortMax);
			m_AttribPairs[(int)AttribType.Oxygen] 	= m_AttribPairs[(int)AttribType.OxygenMax] 	= new AttribPair(AttribType.Oxygen, AttribType.OxygenMax);
			m_AttribPairs[(int)AttribType.Hunger] 	= m_AttribPairs[(int)AttribType.HungerMax] 	= new AttribPair(AttribType.Hunger, AttribType.HungerMax);
			m_AttribPairs[(int)AttribType.Energy] 	= m_AttribPairs[(int)AttribType.EnergyMax] 	= new AttribPair(AttribType.Energy, AttribType.EnergyMax);
			m_AttribPairs[(int)AttribType.Rigid] 	= m_AttribPairs[(int)AttribType.RigidMax] 	= new AttribPair(AttribType.Rigid, AttribType.RigidMax);
			m_AttribPairs[(int)AttribType.Shield] 	= m_AttribPairs[(int)AttribType.ShieldMax] 	= new AttribPair(AttribType.Shield, AttribType.ShieldMax);
			m_AttribPairs[(int)AttribType.Hitfly] 	= m_AttribPairs[(int)AttribType.HitflyMax] 	= new AttribPair(AttribType.Hitfly, AttribType.HitflyMax);
		}
	}
	
	public class PESkEntity : SkEntity
	{
		[Serializable]
		public class Attr
		{
			public AttribType 	m_Type;
			public float		m_Value;
		}
        public static event Action<SkEntity> entityDeadEvent;
        public static event Action<SkEntity, SkEntity, float> entityAttackEvent;

		public const int AttribsCnt = (int)AttribType.Max;
		public const int MaskBitsCnt = 32;
		
		public event Action<SkEntity, SkEntity> deathEvent;
		public event Action<SkEntity>			reviveEvent;
        public event Action<SkEntity, float> attackEvent;
        public event Action<SkEntity, float> onHpChange;
        public event Action<SkEntity, float> onHpReduce;
        public event Action<SkEntity, float> onHpRecover;
		public event Action<SkEntity> onSkillEvent;
        public event Action<SkEntity> onWeaponAttack;

        public event Action<PeEntity> OnEnemyEnter;
        public event Action<PeEntity> OnEnemyExit;
        public event Action<PeEntity> OnEnemyAchieve;
        public event Action<PeEntity> OnEnemyLost;
        public event Action<PeEntity> OnBeEnemyEnter;
        public event Action<PeEntity> OnBeEnemyExit;

		public event Action<float> onHpMaxChange;
		public event Action onStaminaReduce;
		public event Action onSheildReduce;

		public event Action<Vector3> onTranslate;
		
		event Action<AttribType, float, float> 	m_AttrChangeEvent;
		event Action<List<ItemToPack>> 			m_PakageChangeEvent;

        //List<PeEntity> m_Attackers = new List<PeEntity>();
		
		public bool isDead{ get;protected set; }

		//static int[] m_InitBuffList = new int[5]{30200049, 30200050, 30200051, 30200052, 30200053};

        public int[] m_InitBuffList;
		//		[HideInInspector]
		public Attr[] m_Attrs;

		public float GetAttribute(AttribType type,bool bSum =true)
		{
			return GetAttribute((int)type,bSum);
		}
		
		public void SetAttribute(AttribType type, float value, bool eventOff = true)
		{
			SetAttribute((int)type, value, eventOff);
		}
		
		public float HPPercent
		{
			get
			{
				return GetAttribute(AttribType.Hp,false) / GetAttribute(AttribType.HpMax,false);
			}
		}
		
		public virtual void InitEntity(SkEntity parent = null, bool[] useParentMasks = null)
		{
			if(parent != null && useParentMasks != null)
			{
				Init(OnNumAttribs, OnPutInPak, parent, useParentMasks);
			}
			else if(parent != null || useParentMasks != null)
			{
				Debug.LogError("Invalid Arguments to init skill entity");
			}
			else
			{
				Init(OnNumAttribs, OnPutInPak, AttribsCnt);
			}
			
			if (null != m_Attrs)
			{
				foreach (Attr attr in m_Attrs) 
				{
					SetAttribute ((int)(attr.m_Type), attr.m_Value);
				}
			}
			
			if(null != m_InitBuffList)
				foreach(int buffID in m_InitBuffList)
					MountBuff(this, buffID, new System.Collections.Generic.List<int>(), new System.Collections.Generic.List<float>());
		}

		protected virtual void CheckInitAttr()
		{
			if(GetAttribute(AttribType.Hp) <= 0)
				OnDeath(null);
			else
				SetAutoBuffActive(true);

		}

		protected void SetAutoBuffActive(bool active)
		{
			if(null == m_InitBuffList)
				return;
			if(active)
			{
				foreach(int buffID in m_InitBuffList)
					MountBuff(this, buffID, new System.Collections.Generic.List<int>(), new System.Collections.Generic.List<float>());
			}
			else
			{
				foreach(int buffID in m_InitBuffList)
					CancelBuffById(buffID);
			}

		}
		
		public void AddAttrListener(Action<AttribType, float, float> func)
		{
			m_AttrChangeEvent += func;
		}
		
		public void RemoveAttrListener(Action<AttribType, float, float> func)
		{
			m_AttrChangeEvent -= func;
		}
		
		void OnNumAttribs(int attType, float oldVal, float newVal)
		{
			CheckAttrEvent((AttribType)attType, oldVal, newVal);
			if(null != m_AttrChangeEvent)
				m_AttrChangeEvent((AttribType)attType, oldVal, newVal);
		}

        public void DispatchEnemyEnterEvent(PeEntity enemy)
        {
            if (OnEnemyEnter != null)
                OnEnemyEnter(enemy);
        }

        public void DispatchEnemyExitEvent(PeEntity enemy)
        {
            if (OnEnemyExit != null)
                OnEnemyExit(enemy);
        }

        public void DispatchEnemyAchieveEvent(PeEntity enemy)
        {
            if (OnEnemyAchieve != null)
                OnEnemyAchieve(enemy);
        }

        public void DispatchEnemyLostEvent(PeEntity enemy)
        {
            if (OnEnemyLost != null)
                OnEnemyLost(enemy);
        }

        public void DispatchBeEnemyEnterEvent(PeEntity attacker)
        {
            if (OnBeEnemyEnter != null)
                OnBeEnemyEnter(attacker);
        }

        public void DispatchBeEnemyExitEvent(PeEntity attacker)
        {
            if (OnBeEnemyExit != null)
                OnBeEnemyExit(attacker);
        }

        public void DispatchHPChangeEvent(SkEntity caster, float hpChange)
        {
            if (null != onHpChange)
            {
                onHpChange(caster, hpChange);
                PeEventGlobal.Instance.HPChangeEvent.Invoke(this, caster, hpChange);
            }

            if (hpChange < -PETools.PEMath.Epsilon)
            {
                if(onHpReduce != null)
                    onHpReduce(caster, Mathf.Abs(hpChange));
                if (entityAttackEvent != null)
                    entityAttackEvent(this, caster, Mathf.Abs(hpChange));

                PESkEntity _caster = PETools.PEUtil.GetCaster(caster) as PESkEntity;
                if(_caster != null && _caster.attackEvent != null)
                    _caster.attackEvent(this, Mathf.Abs(hpChange));

                PeEventGlobal.Instance.HPReduceEvent.Invoke(this, caster, Mathf.Abs(hpChange));
            }

            if (hpChange > PETools.PEMath.Epsilon)
            {
                if(onHpRecover != null)
                    onHpRecover(caster, Mathf.Abs(hpChange));

                PeEventGlobal.Instance.HPRecoverEvent.Invoke(this, caster, Mathf.Abs(hpChange));
            }
        }

		public void DispatchTargetSkill(SkEntity caster)
		{
			if(onSkillEvent != null)
				onSkillEvent(caster);

		}

        public void DispatchWeaponAttack(SkEntity caster)
        {
            if (onWeaponAttack != null)
                onWeaponAttack(caster);
        }

		public void DispatchOnTranslate(Vector3 pos)
		{
			if(onTranslate != null)
				onTranslate(pos);
		}

		void DispatchHPMaxChangeEvent(float newVal)
		{
			if(null != onHpMaxChange)
				onHpMaxChange(newVal);
		}

        void DispatchDeathEvent(SkEntity caster)
        {
            if (null != deathEvent)
                deathEvent(this, caster);
            if (null != entityDeadEvent)
                entityDeadEvent(this);

			if(GetComponent<ItemDropPeEntity>() == null)
			{
				PeEntity entity = GetComponent<PeEntity>();
				if(entity != null && (entity.ItemDropId > 0 || GetSpecialItem.ExistSpecialItem(entity)))
				{
//					gameObject.AddComponent<ItemDropPeEntity>();
//					LootItemDropPeEntity.AddPeEntity(entity);
					LootItemMgr.Instance.RequestCreateLootItem(entity);
				}
			}
            PeEventGlobal.Instance.DeathEvent.Invoke(this, caster);

        }

		protected void DispatchReviveEvent()
		{
			if(null != reviveEvent)
				reviveEvent(this);

			if(GetComponent<ItemDropPeEntity>() != null)
			{
				GameObject.Destroy(GetComponent<ItemDropPeEntity>());
			}
		}

		void DispatchStaminaEvent(float staminaChange)
		{
			if(staminaChange < -PETools.PEMath.Epsilon && null != onStaminaReduce)
			{
				onStaminaReduce();
			}
		}

		void DispatchSheildEvent(float sheildChange)
		{
			if(sheildChange < -PETools.PEMath.Epsilon && null != onSheildReduce)
			{
				onSheildReduce();
			}
		}



		void OnDeath(SkEntity caster)
		{
			DispatchDeathEvent(caster);
			SetAutoBuffActive(false);
			isDead = true;
		}
		
		void CheckAttrEvent(AttribType attType, float oldVal, float newVal)
		{
			switch(attType)
			{
			case AttribType.Hp:
                    SkEntity caster;
                if (PeGameMgr.IsSingle)
				    caster = GetCasterToModAttrib((int)attType);
                else
                    caster = GetNetCasterToModAttrib((int)attType);

                float hpChange = newVal - oldVal;
				if(Mathf.Abs(hpChange) > PETools.PEMath.Epsilon)
				{
                    if(!PeGameMgr.IsMulti)
					    DispatchHPChangeEvent(caster, hpChange);

					if(GetAttribute(AttribType.Hp) < GetAttribute(AttribType.HpMax))
						HPChangeEventDataMan.Instance.OnHpChange(this, caster, hpChange);
				}

				if(newVal <= 0 && oldVal >0 && null != deathEvent)
				{
					OnDeath(caster);
				}

				break;
			case AttribType.HpMax:
				DispatchHPMaxChangeEvent(newVal);
				break;
			case AttribType.Stamina:
				if(oldVal != newVal)
				{
					DispatchStaminaEvent(newVal - oldVal);
					if(oldVal > newVal)
						_lastestTimeOfConsumingStamina = Time.time;
				}
				break;
			case AttribType.Shield:
				if(oldVal != newVal)
					DispatchSheildEvent(newVal - oldVal);
				break;
			}

			if(oldVal != newVal)
				AttribInspectoscope.Instance.CheckAttrib(this, attType, newVal);
		}
		
		public void AddPakageListener(Action<List<ItemToPack>> func)
		{
			m_PakageChangeEvent += func;
		}
		
		public void RemovePakageListener(Action<List<ItemToPack>> func)
		{
			m_PakageChangeEvent -= func;
		}
		
		protected virtual void OnPutInPak(List<ItemToPack> itemsToPack)
		{
			if(null != m_PakageChangeEvent)
				m_PakageChangeEvent(itemsToPack);
		}
	}
}



// public override void ApplySpEff (SkInst inst) 应用力
//public override void ApplyEmission(int emitId, SkInst inst)
//public override void ApplyEff(int effId, SkInst inst)
//public override void ApplyCamEff(int camEffId, SkInst inst)
