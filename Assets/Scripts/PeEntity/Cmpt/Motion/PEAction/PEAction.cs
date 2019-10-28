using UnityEngine;
using Mono.Data.SqliteClient;
using System;
using System.Collections.Generic;

namespace Pathea
{
	public enum PEActionType
	{
		None = 0,
		Move,
		Sprint,
		Rotate,
		Jump,
		Whacked,
		Repulsed,
		Wentfly,
		Knocked,
		Death,
		GetUp, //10
		Revive,
		Step,
		EquipmentHold,
		SwordAttack,
		HoldShield,
		EquipmentPutOff,
		Fall,
		GunHold,
		GunFire,
		GunReload, //20
		GunPutOff,
		BowHold,
		BowPutOff,
		BowShoot,
		BowReload,
		Sleep,
		Eat,
		AimEquipHold,
		AimEquipPutOff,
		Dig,
		Fell,
		Gather,
		PickUpItem,
		Drive,
		JetPack,
		Parachute,
		Glider,
		Climb,
		Build,
		HoldFlashLight,
		Stuned,
		Pump,
		Sit,
		Operation,
		Draw,
		Throw,
		TwoHandSwordHold,
		TwoHandSwordPutOff,
		TwoHandSwordAttack,
		Halt,
		GunMelee,
		Lie,
		GetOnTrain,
		Cutscene,
		Cure,
		Hand,
		Handed,
		Leisure,
		AlienDeath,
		RopeGunShoot,
		Abnormal,
        Ride, //lz-2016.12.21 Æï
		Max
	}
	
	public enum PEActionMask
	{
		Ground = 0,
		InAir,
		InWater,
		Rotate,
		Sprint,
		Jump,
		Repulsed,
		Wentfly,
		Knocked,
		Death,
		GetUp,
		Revive,
		Step,
		EquipmentHold,
		SwordAttack,
		HoldShield,
		Halt,
		Fall,
		GunHold,
		GunReload,
		GunShoot,
		BowHold,
		BowShoot,
		BowReload,
		Sleep,
		Eat,
		AimEquipHold,
		Dig,
		Fell,
		Gather,
		PickUpItem,
		OnVehicle,
		JetPack,
		Parachute,
		Glider,
		Climb,
		Build,
		HoldFlashingLight,
		Stuned,
		Pump,
		Sit,
		Operation,
		Draw,
		Throw,
		Lie,
		GetOnTrain,
		Cutscene,
		Whacked,
		Cure,
		HeavyEquipment,
		DrillingDig,
		Hand,
		Talk,
		RopeGunShoot,
		Abnormal,
        Ride, //lz-2016.12.21 Æï
        Max
	}
	
	public class ActionRelationData
	{
		static ActionRelationData[] m_Relations;

		public class DependData
		{
			public PEActionMask maskType;
			public bool maskValue;
		}
		
		public PEActionType m_ActionType;
		public List<DependData> m_DependMask; //
		public List<PEActionType> m_PauseAction;
		public List<PEActionType> m_EndAction;
		public List<PEActionType> m_EndImmediately;
		
		public static ActionRelationData GetData(PEActionType type)
		{
			if(null == m_Relations)
			{
				Debug.LogError("ActionRelationData not init");
				return null;
			}
			return m_Relations[(int)type];
		}
		
		public static void LoadActionRelation()
		{
			m_Relations = new ActionRelationData[(int)PEActionType.Max];
			SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("ActionRelation");
			int fieldCount = reader.FieldCount;
			while (reader.Read())
			{
				ActionRelationData data = new ActionRelationData();
				data.m_ActionType = (PEActionType)Convert.ToInt32(reader.GetString(reader.GetOrdinal("ActionType")));
				
				data.m_DependMask = new List<DependData>();
				
				int startIndex = 5;
				
				for(int i = startIndex; i < fieldCount; i++)
				{
					int mask = Convert.ToInt32(reader.GetString(i));
					if(mask > 0)
					{
						DependData dependData = new DependData();
						dependData.maskType = (PEActionMask)(i - startIndex);
						dependData.maskValue = (mask == 1);
						data.m_DependMask.Add(dependData);
					}
				}
				
				data.m_PauseAction = new List<PEActionType>();
				string[] pauseStr = reader.GetString(reader.GetOrdinal("PauseAction")).Split(',');
				foreach(string str in pauseStr)
					if("0" != str)
						data.m_PauseAction.Add((PEActionType)Convert.ToInt32(str));
				
				data.m_EndAction = new List<PEActionType>();
				string[] endStr = reader.GetString(reader.GetOrdinal("EndAction")).Split(',');
				foreach(string str in endStr)
					if("0" != str)
						data.m_EndAction.Add((PEActionType)Convert.ToInt32(str));
				
				data.m_EndImmediately = new List<PEActionType>();
				string[] endImmediatelyStr = reader.GetString(reader.GetOrdinal("EndImmediately")).Split(',');
				foreach(string str in endImmediatelyStr)
					if("0" != str)
						data.m_EndImmediately.Add((PEActionType)Convert.ToInt32(str));
				if((int)data.m_ActionType < m_Relations.Length)
					m_Relations[(int)data.m_ActionType] = data;
			}
		}
	}

	public abstract class PEAction
	{
		public abstract PEActionType ActionType{ get; }

		[HideInInspector] public List<PEActionType> m_PauseActions = new List<PEActionType>();
		public bool pauseAction{ get{ return m_PauseActions.Count > 0; } }
		public MotionMgrCmpt motionMgr { get; set;}

		protected PeEntity		entity{ get { return motionMgr.Entity; } }
		protected Motion_Move	move{ get { return entity.motionMove; } }
		protected PeTrans      	trans { get { return entity.peTrans; } }
		protected SkAliveEntity skillCmpt { get { return entity.aliveEntity; } }
		protected EquipmentCmpt	equipCmpt { get { return entity.equipmentCmpt; } }
		protected PackageCmpt	packageCmpt { get { return entity.packageCmpt; } }
		protected BiologyViewCmpt viewCmpt { get { return entity.biologyViewCmpt; } }
		protected IKCmpt		ikCmpt{ get { return entity.IKCmpt; } }

		AnimatorCmpt	m_Anim;
		protected AnimatorCmpt 	anim
		{
			get 
			{
				if(null == m_Anim)
				{
					m_Anim = entity.animCmpt;
					if(null != m_Anim)
						m_Anim.AnimEvtString += OnAnimEvent;
				}
				return m_Anim; 
			}
		}

		public virtual void PreDoAction() { }
		
		public virtual void DoAction(PEActionParam para) { }
		
		public virtual void ResetAction(PEActionParam para) { }
		
		public virtual void PauseAction() { }
		
		public virtual void ContinueAction() { }
		
		public virtual void OnModelBuild() { }

		public virtual void OnModelDestroy() { motionMgr.EndImmediately(ActionType); }

		public virtual void EndAction() { motionMgr.EndImmediately(ActionType); }
		
		public virtual void EndImmediately() { }

		public virtual bool CanDoAction(PEActionParam para) { return true; }
		
		public virtual bool Update() { return true; }

		protected virtual void OnAnimEvent(string eventParam) { }
	}
}
