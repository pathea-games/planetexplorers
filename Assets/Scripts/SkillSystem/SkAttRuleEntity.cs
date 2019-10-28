//using UnityEngine;
//using System;
//using SkillSystem;
//
//namespace SkillSystem
//{
//	public enum AttribType
//	{
//		HpMax = 0,
//		Hp,
//		StaminaMax,
//		Stamina,
//		StaminaReducePercent,
//		StaminaRecoverSpeed,	//5
//		StaminaRecoverDelay,
//		ComfortMax,
//		Comfort,
//		ComfortReducePercent,
//		ComfortRecoverSpeed,	//10
//		OxygenMax,
//		Oxygen,
//		OxygenReducePercent,
//		OxygenRecoverSpeed,
//		HungerMax,				//15
//		Hunger,
//		HungerReducePercent,
//		HungerRecoverSpeed,
//		Energy,
//		EnergyMax,				//20
//		ResDamage,
//		ResRange,
//		ResBouns,
//		AtkRange,
//		Atk,					//25
//		CritPercent,
//		Def,
//		RigidMax,
//		Rigid,
//		RigidRecoverSpeed,		//30
//		Shield,
//		ShieldMax,
//		ShieldMeleeProtect,
//		ShieldRigidProtect,
//		EnableShieldBlock,		//35
//		RunSpeed,
//		WalkSpeed,
//		JumpHeight,
//		WeightUpUnderWater,
//		HitflyMax,				//40
//		Hitfly,
//		HitflyRecoverSpeed,
//		Clean,
//		CleanRecover,
//		Food_Cereals,			//45
//		Food_Meat,
//		Food_Bean,
//		Food_Fruit,
//		Food_Energy,
//		DebuffPercent01,		//50
//		DebuffReduce01,
//		DebuffPercent02,
//		DebuffReduce02,
//		DebuffPercent03,
//		DebuffReduce03,			//55
//		DebuffPercent04,
//		DebuffReduce04,
//		DebuffPercent05,
//		DebuffReduce05,
//		DebuffPercent06,		//60
//		DebuffReduce06,
//		DebuffPercent07,
//		DebuffReduce07,
//		DebuffPercent08,
//		DebuffReduce08,			//65
//		DebuffPercent09,
//		DebuffReduce09,
//		DebuffPercent10,
//		DebuffReduce10,
//		ThresholdWhacked,		//70
//		ThresholdRepulsed,
//		ThresholdWentfly,
//		ThresholdKnocked,
//		Exp,
//		CutDamage,				//75
//		CutBouns,
//		SwordMultiple,
//		ShieldMultiple,
//		AxeMultiple,
//		BowMultiple,			//80
//		GunMultiple,
//		ForceScale,
//		AttNum,					//83
//		Max = AttNum
//	}
//
//	public class SkAttRuleEntity : SkEntity 
//	{
//		public const int AttribsCnt = (int)AttribType.Max;
//		public const int MaskBitsCnt = 32;
//
////		//AttRule
////		protected AttRuleCtrl mAttRuleCtrl;
////		public static readonly int[] _CommonRule = {};
////		public static readonly int[] _CommonAliveRule = {};
////		public static readonly int[] _CommonMachineRule = {};
//
//		public event Action<AttribType> m_AttrChangeEvent;
//		public event Action<int> m_PakageChangeEvent;
//
//
//		public virtual void InitEntity(SkEntity parent = null, bool[] useParentMasks = null)
//		{
//			if(parent != null && useParentMasks != null)
//			{
//				Init(OnNumAttribs, OnPutInPak, parent, useParentMasks);
//			}
//			else if(parent != null || useParentMasks != null)
//			{
//				Debug.LogError("Invalid Arguments to init skill entity");
//			}
//			else
//			{
//				Init(OnNumAttribs, OnPutInPak, AttribsCnt, MaskBitsCnt);
//			}
////			mAttRuleCtrl = new AttRuleCtrl(this);
////			foreach(int ID in _CommonRule)
////				mAttRuleCtrl.AddRule(ID);
////			if(isAlive)
////			{
////				foreach(int ID in _CommonAliveRule)
////					mAttRuleCtrl.AddRule(ID);
////			}
////			else
////			{
////				foreach(int ID in _CommonMachineRule)
////					mAttRuleCtrl.AddRule(ID);
////			}
//		}
//
////		protected virtual void Update()
////		{
////			if(null != mAttRuleCtrl)
////				mAttRuleCtrl.Update();
////		}
//
//		public void AddAttrListener(Action<AttribType> func)
//		{
//			m_AttrChangeEvent += func;
//		}
//
//		public void RemoveAttrListener(Action<AttribType> func)
//		{
//			m_AttrChangeEvent -= func;
//		}
//
//		public virtual void OnNumAttribs(int attType, float oldVal, float newVal)
//		{
//			if(null != m_AttrChangeEvent)
//				m_AttrChangeEvent((AttribType)attType);
//		}
//
//		public void AddPakageListener(Action<int> func)
//		{
//			m_PakageChangeEvent += func;
//		}
//		
//		public void RemovePakageListener(Action<int> func)
//		{
//			m_PakageChangeEvent -= func;
//		}
//
//		public virtual void OnPutInPak(int pakID)
//		{
//			if(null != m_PakageChangeEvent)
//				m_PakageChangeEvent(pakID);
//		}
////		public void AddAttRule(int ID)
////		{
////			mAttRuleCtrl.AddRule(ID);
////		}
////		public void RemoveRule(int ID)
////		{
////			mAttRuleCtrl.RemoveRule(ID);
////		}
//
//	}
//}
