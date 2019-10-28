using UnityEngine;
using System;
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
	
	public enum  ELineType
	{
		IDLE,
		TeamAtk,
		TeamWork,
		TeamEat,
		TeamSleep,
		TeamChat,
        Max
	}

	public enum ENpcBattProfession
	{
		T = 1,
		ADC,
		AD
	}
	
	public enum ENpcBattle
	{
		Attack,
		Defence,
		Passive,
		Evasion,
		Stay,
	}
	
	public enum ENpcJob
	{
		None,
		Resident,         //居民:1
		Farmer,           //农民：2
		Worker,           //工人 ： 3
		Soldier,          //士兵 ：4
		Follower,         //随从：5
		Processor,        //采集者：6
		Doctor,           //医生 ： 7
		Trainer,           //训练人 ：8
		Max
	}
	
	public enum ENpcType
	{
		None,
		Follower,
		Field,
		Campsite,
		Base
	}
	
	public enum ENpcManor
	{
		None,
		Field,
		Campsite,
		Base
	}
	
	public enum ENpcTitle
	{
		None = 0,
		Manage = 1,
		Harvest = 2,
		Plant = 4
	}
	
	public enum ENpcSoldier
	{
		None,
		Patrol,
		Guard
	}
	
	public enum ENpcState
	{
		UnKnown,
		Prepare,
		Idle,
		Rest,
		Work,
		Follow,
		Dead,
		Attack,
		Patrol,
		Plant,
		Watering,
		Weeding,
		Gain
	}
	
	public enum ENpcMedicalState
	{
		None,
		//Waiting,
		WaitForDiagnos,
		SearchDiagnos,
		Diagnosing,
		WaitForTreat,
		SearchTreat,
		Treating,
		SearchHospital,
		WaitForTent,
		In_Hospital,
		Cure
	}
	
	public enum ENpcControlType
	{
		Leisure = 1,
		Interaction,
		Stroll,
		Patrol,
		Guard,    //5
		Dining,
		Sleep,
		MoveTo,
		Cure,
		Work,  //10
		ChangeRole,
		AddHatred,
		ReceiveHatred,
		InjuredHatred,
		SelfDefense,  //15
		Pursuit,
		Assist,
		Recourse,
		Attack,
		Dodge,     //20
		Block,
		CanTalk,
		CanHanded,
		Max
	}
	
	public enum ENpcSpeakType
	{
		Topleft,
		TopHead,
		Both,
		Max
	}
	
	public enum ETalkLevel
	{
		Common,
		Medium,
		Low,
		Max
	}
	
	public enum ENpcAiType
	{
		None,
		RDialogue,
		RTranslate,
		RRotate,
		Ridle,
		Ridle_inJured,
		Ridle_Rest,
		Ridle_Becarry,
		RFollowTarget,
		RFollowPath,
		RMoveToPoint,
		RAnimation,
		RSalvation,
		SkillAround,
		
		FollowUp,
		AttackWeaPon,
		
		IsNpcFollower,
		NpcFollowerWork,
		NpcFollower,
		
		NpcBaseWander,
		NpcBaseIdle,
		NpcMedicalDiagnose,
		NpcMedicalTreat,
		NpcInHospital,
		NpcBaseSleep,
		NpcBaseTakeFood,
		
		NpcBaseJobResident,
		NpcBaseJobFarmer,
		NpcBaseJobFarmer_Plant,
		NpcBaseJobFarmer_PlantWater,
		NpcBaseJobFarmer_Plantclean,
		NpcBaseJobFarmer_Harvest,
		
		NpcBaseWorker,
		NpcBaseSoldier,
		NpcBaseSoldier_Patrol,
		NpcBaseSoldier_Guard,
		
		NpcBaseJobProcessor,
		NpcBaseJobProcessor_Collect,
		NpcBaseJobDoctor,
		NpcBaseJobTrain,
		NpcBaseJobTrain_Instructor,
		NpcBaseJobTrain_Trainee,
		
		NpcCampsiteSleep,
		NpcCampsiteEat,
		NpcCampsiteTalk,
		NpcCampsiteWander,
		
		FieldNpcIdle_Patrol,
		FieldNpcIdle_Idle,
		FieldNpcIdle_FixePoint,
		Idle,
		Succeed,
		Max
	}
	
	public enum ENpcMotionStyle
	{
		Normal = 1,
		InjuredSitEX,
		TerriblyIll
	}
	
	public enum ENpcUnableWorkType
	{
		None = -1,
		HasRequest,
		IsNeedMedicine,
		IsHunger,
		IsHpLow,
		IsUncomfortable,
		IsSleeepTime,
		IsDinnerTime,
		MAX
	}
	
	public class NpcCmpt : PeCmpt,IPeMsg
	{
		const int Version_0000 = 0;
		const int Version_0001 = 1;
		const int Version_0002 = 2;
		const int Version_0003 = 3;
		const int Version_0004 = 4;
		const int Version_0005 = 5;
		const int Version_0006 = 6;
		const int Version_0007 = 7;
		const int Version_0008 = 8;
		const int Version_0009 = 9;
        const int Version_0010 = 10;
        const int Version_0011 = 11;
        const int Version_0012 = 12;
		const int Version_Current = Version_0012;
		NetworkInterface _net;
		public NetworkInterface Net
		{
			get
			{
				if (Entity != null && PeGameMgr.IsMulti && _net == null)
				{
					_net = NetworkInterface.Get(Entity.Id);
				}
				return _net;
			}
		}

        public event Action<int> OnAddEnemyLock;
        public event Action<int> OnRemoveEnemyLock;
        public event Action OnClearEnemyLocked;

        public void DispatchAddEnemyLock(int entityId)
        {
            if (OnAddEnemyLock != null)
                OnAddEnemyLock(entityId);
        }
        public void DispatchRemoveEnemyLock(int entityId)
        {
            if (OnRemoveEnemyLock != null)
                OnRemoveEnemyLock(entityId);
        }
        public void DispatchClearEnemyLock()
        {
            if (OnClearEnemyLocked != null)
                OnClearEnemyLocked();
        }

		private Transform m_Mount;
		private RequestCmpt m_Request;
		private BehaveCmpt m_Behave;
		private PeEntity  m_RobotEntity;
		public SkAliveEntity Alive { get { return Entity.aliveEntity; } }
		
		private NpcPackageCmpt mNpcPackage;
		public NpcPackageCmpt NpcPackage { get { return mNpcPackage; } }
		
		PeEntity mChatTarget = null;
		public PeEntity ChatTarget { get { return mChatTarget; } set { mChatTarget = value; } }
		
		ENpcType m_Type = ENpcType.Field;
		public ENpcType Type { get { return m_Type; } set { m_Type = value; } }
		
		ENpcJob m_Job = ENpcJob.None;
		public ENpcJob Job
		{
			get
			{
				if (!NpcTypeDb.CanRun(NpcControlCmdId, ENpcControlType.Work))
					return ENpcJob.Resident;

                if (lineType != ELineType.IDLE || !m_caCanIdle || (m_Job != ENpcJob.Resident && m_Job != ENpcJob.Processor && !NpcThinkDb.CanDoing(Entity, EThinkingType.Work))) //|| PeNpcGroup.Instance.InDanger
				{
					return ENpcJob.Resident;
				}
				return m_Job;
			}
			set
			{
				PeNpcGroup.Instance.OnCsJobChange(Entity,m_Job,value);

				m_Job = value;
				if (m_Job == ENpcJob.Resident || m_Job == ENpcJob.None)
					ThinkRemove(EThinkingType.Work);
				else
					ThinkAdd(EThinkingType.Work);//ThinkAgent.AddThink(EThinkingType.Work);
				
				m_CanWander = ((m_Job == ENpcJob.Resident || m_Job == ENpcJob.None) ? true : false);

				
			}
		}
		
		ETrainingType e_TrainningType = ETrainingType.Skill;
		public ETrainingType TrainningType { get { return e_TrainningType; } set { e_TrainningType = value; } }
		
		ETrainerType e_TrainerType = ETrainerType.none;
		public ETrainerType TrainerType { get { return e_TrainerType; } set { e_TrainerType = value; } }
		
		ENpcTalkType e_NpcTalkType;
		public ENpcTalkType NpcTalkTYpe
		{ get { return e_NpcTalkType; } set { e_NpcTalkType = value; } }
		
		ENpcBattProfession m_Profession = ENpcBattProfession.AD;
		public ENpcBattProfession Profession
		{
			set {m_Profession = value;}
			get {return m_Profession;}
		}
		
		BattleMgr m_BattleMgr;
		public BattleMgr BattleMgr
		{
			get
			{
				if(m_BattleMgr == null)
					m_BattleMgr = new BattleMgr(Entity);
				
				return m_BattleMgr;
			}
		}
		ENpcBattle m_Battle = ENpcBattle.Defence;
		public ENpcBattle Battle
		{
			get { return m_Battle; }
			set
			{
				if (m_Battle != value)
				{
					m_Battle = value;
                    UpdateBattle();
				}
			}
		}
		
		ENpcTitle m_Title = ENpcTitle.None;
		
		ENpcSoldier m_Soldier = ENpcSoldier.None;
		public ENpcSoldier Soldier { get { return m_Soldier; } set { m_Soldier = value; } }
		
		ENpcState m_State;
		public ENpcState State { get { return m_State; } set { m_State = value; } }
		
		ENpcAiType m_AiType = ENpcAiType.None;
		public ENpcAiType AiType { get { return m_AiType; } set { m_AiType = value; } }
		
		ENpcMotionStyle m_MotionStyle = ENpcMotionStyle.Normal;
		public ENpcMotionStyle MotionStyle
		{
			get { return m_MotionStyle; }
			set
			{
				m_MotionStyle = value;
				UpdateNpcMotionStyle();
			}
		}

		ELineType m_LineType = ELineType.IDLE;
		public ELineType lineType {get { return  m_LineType;}}
		public void SetLineType(ELineType type)
		{
			if(!type.Equals(null) && !m_LineType.Equals(null))
			{
				if(m_LineType != type)
				{
					PeNpcGroup.Instance.OnCsLineChange(Entity,m_LineType,type);
				}
				
				m_LineType = type;
			}

		}

		object[] mTeamData;
		public  object[] TeamData { get {return mTeamData;}}
		public void setTeamData(params object[] objs)
		{
			mTeamData = objs;
		}

//        SpeedState m_MoveSpeed = SpeedState.Walk;
 //       public SpeedState moveSpeed { get { return m_MoveSpeed; } }
//        public void SetMoveSpeed(SpeedState speed)
//        {
//            m_MoveSpeed = speed;
//        }

		//atk run (can not wrok)
		bool m_caCanIdle = true;
		public bool csCanIdle {get {return m_caCanIdle;}}
		public void SetCanIdle(bool _canIdle)
		{
			m_caCanIdle = _canIdle;
		}

		AttackType mNpcAtkType;
		public void SetAtkType(AttackType type)
		{
			if(mNpcAtkType != type)
			{
				PeNpcGroup.Instance.OnCsAttackTypeChange(Entity,mNpcAtkType,type);
			}
			mNpcAtkType = type;
		}

		List<PeEntity> beEnemiesLocked;
		public bool AddEnemyLocked(PeEntity enemy)
		{
			if(m_MountID != 0)
			{
				PeEntity entity = EntityMgr.Instance.Get(m_MountID);
				if (entity != null)
				{
					return transferAddEnemyLocked(entity,enemy);
				}
				return false;
			}
			
			if(beEnemiesLocked == null)
                beEnemiesLocked = new List<PeEntity>();
            
            if (!beEnemiesLocked.Contains(enemy))
            {
                beEnemiesLocked.Add(enemy);
                DispatchAddEnemyLock(enemy.Id);
            }
			return true;
		}
		
		public bool RemoveEnemyLocked(PeEntity enemy)
		{
			if(m_MountID != 0)
			{
				PeEntity entity = EntityMgr.Instance.Get(m_MountID);
				if (entity != null)
				{
					return transferRemoveEnemyLocked(entity,enemy);
				}
				return false;
			}
			
			if(beEnemiesLocked == null)
				return false;

            if (beEnemiesLocked.Contains(enemy))
                DispatchRemoveEnemyLock(enemy.Id);

			return beEnemiesLocked.Remove(enemy);
		}
		
		
		public void ClearLockedEnemies()
		{
			if(beEnemiesLocked == null || beEnemiesLocked.Count <=0)
				return ;

            DispatchClearEnemyLock();
            beEnemiesLocked.Clear();
		}
		
		public bool HasEnemyLocked()
		{
			if(beEnemiesLocked == null)
				return false;

            for (int i = 0; i < beEnemiesLocked.Count;i++)
            {
                if (beEnemiesLocked[i] != null && beEnemiesLocked[i].hasView)
                    return true;
            }
            ClearLockedEnemies();
            return false;
		}
		
		public bool transferAddEnemyLocked(PeEntity other,PeEntity enemy)
		{
			if(other == null || other.NpcCmpt == null)
				return false;
			
			return other.NpcCmpt.AddEnemyLocked(enemy);
		}
		
		public bool transferRemoveEnemyLocked(PeEntity other,PeEntity enemy)
		{
			if(other == null || other.NpcCmpt == null)
				return false;
			
			return other.NpcCmpt.RemoveEnemyLocked(enemy);
		}

		#region robot
		public void AddFollowRobot(PeEntity robot)
		{
			m_RobotEntity = robot;
		}
		#endregion

		#region BaseNpcData
		//基地NPC数据
		CSCreator m_Creater;
		public CSCreator Creater
		{
			get
			{
				if (m_BaseNpcOutMission || IsFollower)
					return null;
				
				return m_Creater;
			}
			set
			{
				if (m_Creater != null && value == null)
				{
					SendTalkMsg((int)ENpcTalkType.Dissolve, 0, ENpcSpeakType.Both);
				}
				
				m_Creater = value;
				UpdateType();
				
				if (m_Creater != null && m_Behave != null)
				{
					m_Behave.Excute();
				}
			}
		}
		
		IOperation m_Work;
		public IOperation Work { get { return m_Work; } set { m_Work = value; } }
		
		IOperation m_Cure;
		public IOperation Cure { get { return m_Cure; } set { m_Cure = value; } }
		
		IOperation m_Sleep;
		public IOperation Sleep { get { return m_Sleep; } set { m_Sleep = value; } }
		
		IOperation m_trainner;
		public IOperation Trainner { get { return m_trainner; } set { m_trainner = value; } }
		
		CSEntity m_WorkEntity;
		public CSEntity WorkEntity { get { return m_WorkEntity; } set { m_WorkEntity = value; } }
		
		
		float m_GuardRadius;
		public float GuardRadius
		{
			get { return m_GuardRadius; }
			set { m_GuardRadius = value; }
		}
		
		Vector3 m_GuardPosition;
		public Vector3 GuardPosition
		{
			get { return m_GuardPosition; }
			set { m_GuardPosition = value; }
		}
		
		List<CSEntity> m_baseEntities;
		public List<CSEntity> BaseEntities
		{
			get { return m_baseEntities; }
			set { m_baseEntities = value; }
		}
		
		public void AddTitle(ENpcTitle title)
		{
			m_Title = ENpcTitle.None;
			m_Title |= title;
		}
		
		public void RemoveTitle(ENpcTitle title)
		{
			m_Title &= ~title;
		}
		
		public bool ContainsTitle(ENpcTitle title)
		{
			return (m_Title & title) != 0;
		}
		#endregion
		
		#region Npcability
		Ablities m_AbilityIdes = new Ablities(5);
		
		public  Ablities  AbilityIDs  {get { return m_AbilityIdes; }}
		
		public void  SetAbilityIDs(Ablities abl)
		{
			m_AbilityIdes = abl;
			m_AbilityIdes.SetDirty(true);
		}
		
		public void AddAbility(int Id)
		{
			if (m_AbilityIdes.Contains(Id))
				return;
			
			m_AbilityIdes.Add(Id);
		}
		public bool RemoveAbliy(int Id)
		{
			if (m_AbilityIdes.Remove(Id))
			{
				return true;
			}
			return false;
		}
		
		
		NpcAblitycmpt m_NpcSkillcmpt;
		public NpcAblitycmpt Npcskillcmpt
		{
			get
			{
				if (m_NpcSkillcmpt == null)
					m_NpcSkillcmpt = new NpcAblitycmpt(Entity.aliveEntity);
				
				return m_NpcSkillcmpt;
			}
			set { m_NpcSkillcmpt = value; }
		}
		
		public bool HasAllys()
		{
			UpdateAllys();
			return (m_Allys.Count > 1);
		}
		
		public bool Containself()
		{
			if (m_Allys == null)
				return false;
			
			return m_Allys.Contains(this);
		}
		
		List<NpcCmpt> m_Allys;
		public List<NpcCmpt> Allys
		{
			get
			{
				UpdateAllys();
				return m_Allys;
			}
		}
		
		private NpcCmpt m_NpcSkillTarget;
		public NpcCmpt NpcSkillTarget
		{
			get { return m_NpcSkillTarget; }
			set { m_NpcSkillTarget = value; }
		}
		
		public List<int> GetSkllIds()
		{
			if (Npcskillcmpt == null)
				return null;
			
			return Npcskillcmpt.GetSkillIDs();
		}
		
		public int GetReadySkill()
		{
			List<int> Skills = GetSkllIds();
			if (Skills == null)
				return -1;
			
			foreach (int skillId in Skills)
			{
				if (!Entity.aliveEntity.IsSkillRunning(skillId))
					return skillId;
			}
			return -1;
		}
		
		public float GetNpcSkillRange(int skillId)
		{
			if (m_NpcSkillcmpt == null)
				return 0.0f;
			
			return m_NpcSkillcmpt.GetCmptSkillRange(skillId);
		}
		
		public bool TryGetItemSkill(Vector3 pos, float percent = 1.0f)
		{
			if (m_NpcSkillcmpt != null)
			{
				return m_NpcSkillcmpt.TryGetItemskill(pos, percent) != null;
			}
			return false;
		}
		
		public float GetHpPerChange()
		{
			List<int> Skills = GetSkllIds();
			if (Skills == null)
				return 0.0f;
			foreach (int skillId in Skills)
			{
				return GetNpcChange_Hp(skillId);
			}
			return 0.0f;
		}
		public float GetNpcChange_Hp(int SkillId)
		{
			if (m_NpcSkillcmpt == null)
				return 0.0f;
			
			return m_NpcSkillcmpt.GetChangeHpPer(SkillId);
		}
		#endregion
		
		#region AttributeUpgrade
		public int mAttributeUpTimes = 0;
		public void AttributeUpgrade(AttribType type, float value)
		{
			if (!CanAttributeUp())
				return;
			
			AttPlusBuffDb.Item item = AttPlusBuffDb.Get(type);
			if (item == null)
				return;
			
			List<int> idxList = new List<int>();
			idxList.Add(0);
			
			List<float> valList = new List<float>();
			valList.Add(value);
			
			mAttributeUpTimes++;
			
			SkEntity.MountBuff(Entity.aliveEntity, item._buffId, idxList, valList);
			return;
		}
		public int curAttributeUpTimes
		{
			get { return mAttributeUpTimes; }
		}
		public bool CanAttributeUp()
		{
			return AttPlusNPCData.ComparePlusCout(Entity.entityProto.protoId, mAttributeUpTimes);
		}
		#endregion
		
		#region Medical
		ENpcMedicalState m_MedicalState;
		public ENpcMedicalState MedicalState { get { return m_MedicalState; } set { m_MedicalState = value; } }
		
		//Medicine
		List<PEAbnormalType> m_illAbnormals;
		public List<PEAbnormalType> illAbnormals { get { return m_illAbnormals; } set { m_illAbnormals = value; } }
		
		bool m_IsNeedMedicine = false;
		public bool IsNeedMedicine
		{
			get { return m_IsNeedMedicine; }
			set { m_IsNeedMedicine = value; }
		}
		
		public void AddSick(PEAbnormalType type)
		{
			if (Entity != null && Entity.Alnormal != null)
			{
				Entity.Alnormal.StartAbnormalCondition(type);
			}
		}
		
		public void CureSick(PEAbnormalType type)
		{
			if (Entity.Alnormal != null)
			{
				Entity.Alnormal.EndAbnormalCondition(type);
				
				if (m_illAbnormals != null && m_illAbnormals.Contains(type))
					m_illAbnormals.Remove(type);
				
				int buffid = AbnormalTypeTreatData.GetCureSkillId((int)type);
				if (buffid > 0)
					SkEntity.MountBuff(Entity.aliveEntity, buffid, new List<int>(), new List<float>());
				
				m_IsNeedMedicine = NeedToMedical();
				
			}
		}
		
		bool NeedToMedical()
		{
			//Profiler.BeginSample("NeedMed");
			m_illAbnormals.Clear();
			int n = AbnormalTypeTreatData.treatmentDatas.Count;
			for (int i = 0; i < n; i++)
			{
				int abnomolId = AbnormalTypeTreatData.treatmentDatas[i].abnormalId;
				if (Entity.Alnormal.CheckAbnormalCondition((PEAbnormalType)abnomolId))
				{
					if (AbnormalTypeTreatData.CanBeTreatInColony(abnomolId) && !m_illAbnormals.Contains((PEAbnormalType)abnomolId))
						m_illAbnormals.Add((PEAbnormalType)abnomolId);
				}
			}
			//Profiler.EndSample ();
			return m_illAbnormals.Count > 0;
		}
		#endregion
		
		#region passenger OnVCCarrier
		PassengerCmpt m_Passenger;
		public PassengerCmpt Passenger
		{
			get
			{
				return Entity != null ? Entity.passengerCmpt : null;
			}
		}
		
		public bool IsOnVCCarrier
		{
			get { return Passenger != null ? Passenger.IsOnVCCarrier : false; }
		}
		
		public bool IsOnRail { get { return Passenger != null ? Passenger.IsOnRail : false; } }
		#endregion
		
		
		#region Follower
		
		EquipSelect m_EqSelect;
		public  EquipSelect EqSelect
		{
			get{
				if(m_EqSelect == null)
					m_EqSelect = new EquipSelect();
				
				return m_EqSelect;
			}
		}

		bool m_FollowerWork;
        //lz-2016.09.12 仆从的工作状态改变的时候调用事件
        public Action FollowerWorkStateChangeEvent;
		public bool FollowerWork
		{
			get { return m_FollowerWork; }
			set
			{
				if (!value)
					RelashModel();

                if (m_FollowerWork != value)
                {
                    AddTalkInfo(ENpcTalkType.Business_trip, ENpcSpeakType.TopHead);
                    m_FollowerWork = value;
                    if (null != FollowerWorkStateChangeEvent)
                    {
                        FollowerWorkStateChangeEvent();
                    }
                }
			}
		}
		
		bool m_FollowerSentry = false;
		public bool FollowerSentry { get { return m_FollowerSentry; } set { m_FollowerSentry = value; } }
		
		bool m_FollowerCut = false;
		//TreeInfo m_PlayerOpTree;
		public bool FollowerCut { get { return m_FollowerCut; } set { m_FollowerCut = value; } }

		bool m_servantCallback = false;
		public bool servantCallback { get {return m_servantCallback;} set {m_servantCallback = value;}}

		public void ServantCallBack()
		{
			if (Master == null)
				return;
			
			if (Entity.target != null)
				Entity.target.ClearEnemy();
			
			
			if (Entity.biologyViewCmpt != null)
				Entity.biologyViewCmpt.Fadein();

			Vector3 pos = PeCreature.Instance.mainPlayer.ExtGetPos();
			pos.z += 2;
			pos.y += 1;
			Req_Translate(pos);

			m_servantCallback = true;
		}

		void RelashModel()
		{
			//mEntity.ExtGetPos();
			Vector3 pos = PeCreature.Instance.mainPlayer.ExtGetPos();
			pos.z += 2;
			pos.y += 1;
			
			Entity.ExtSetPos(pos);
		}
		
		float m_FollowerReviceTime = 1200.0f;
		public float FollowerReviceTime
		{ get { return m_FollowerReviceTime; } }
		
		float m_FollowerCurReviveTime;
		public float FollowerCurReviveTime
		{ get { return m_FollowerCurReviveTime; } set { m_FollowerCurReviveTime = value; } }
		
		public PeEntity Follwerentity { get { return Entity; } }
		public Vector3 FollowerHidePostion { get { return Entity.target != null ? Entity.target.HidePistion : Vector3.zero; } }
		
		ServantLeaderCmpt m_Master;
		public ServantLeaderCmpt Master {get { return m_Master; }}

        public void SetServantLeader(ServantLeaderCmpt leader)
        {
            if (m_Master != leader)
            {
                if (m_Master != null && leader == null)
                {
                    SendTalkMsg(ENpcTalkType.Dissolve, ENpcSpeakType.TopHead);
                    RemoveServentSign();
                    if (Entity.aliveEntity != null && Entity.aliveEntity.isDead)
                        PeLogicGlobal.Instance.ReviveEntity(Entity.aliveEntity, (float)ReviveTime);
                }

                m_Master = leader;
                UpdateType();
                UpdateBattle();
                if (m_Master != null && m_Behave != null)  m_Behave.Excute();
            }
        }
		
		Vector3 mRecruitPos;
		public Vector3 RecruitPos
		{ get { return mRecruitPos; } set { mRecruitPos = value; } }
		
		int m_GatherprotoTypeIdx = -99;
		public int GatherprotoTypeIdx { get { return m_GatherprotoTypeIdx; } set { m_GatherprotoTypeIdx = value; } }
		
		public void ServentProcet()
		{
			if (m_Master != null)
			{
				Vector3 pos = m_Master.Entity.position;
				Req_Translate(pos);
			}
		}
		
		public float FollowDistance
		{
			get
			{
				if (!IsFollower)
					return 0.0f;
				
				if (Master != null)
				{
					PeTrans tr = Master.GetComponent<PeTrans>();
					if (tr != null)
						return PEUtil.SqrMagnitude(Entity.peTrans.position, tr.position);
				}
				
				if (m_Request != null)
				{
					RQFollowTarget ft = m_Request.GetRequest(EReqType.FollowTarget) as RQFollowTarget;
					if (ft != null && ft.id != 0)
					{
						PeEntity e = EntityMgr.Instance.Get(ft.id);
						if (e != null)
							return PEUtil.SqrMagnitude(Entity.peTrans.position, e.position);
					}
				}
				
				return 0.0f;
			}
		}
		
		void AddServentDieSign()
		{
			if (Entity == null)
				return;
			
			if (IsServant)
			{
				ServantDeadLabel label=null;
				if (Entity.entityProto.proto == EEntityProto.RandomNpc)
				{
					label = new ServantDeadLabel(PeMap.MapIcon.ServantDeadPlace, NpcPostion, Entity.enityInfoCmpt.characterName.fullName, Entity.Id);
				}
				else
				{
					label = new ServantDeadLabel(PeMap.MapIcon.ServantDeadPlace, NpcPostion, Entity.enityInfoCmpt.characterName.fullName, Entity.entityProto.protoId);
				}
				PeMap.LabelMgr.Instance.Add(label);
				
			}
			
		}
		
		void RemoveServentSign()
		{
			PeMap.ILabel label=null;
			if (Entity.entityProto.proto == EEntityProto.RandomNpc)
			{
				label = PeMap.LabelMgr.Instance.Find(item =>(item is ServantDeadLabel)&&((ServantDeadLabel)item).servantId==Entity.Id);
			}
			else
			{
				label = PeMap.LabelMgr.Instance.Find(item => (item is ServantDeadLabel) && ((ServantDeadLabel)item).servantId == Entity.entityProto.protoId);
			}
			PeMap.LabelMgr.Instance.Remove(label);
		}
		
		#endregion
		
		
		#region bool

        bool _IsCsBacking = false;
        public bool CsBacking { get { return _IsCsBacking; } }
        public void SetCsBacking(bool value)
        {
            _IsCsBacking = value;
        }

		bool _NeedSeekHelp = false;
		public bool NeedSeekHelp {get {return _NeedSeekHelp;} set{_NeedSeekHelp =value;}}
		
		bool _MisstionAskStop = false;
		public bool MisstionAskStop { get { return _MisstionAskStop; } set { _MisstionAskStop = value; } }
		
		public bool IsServant { get { return m_Master != null; } }
		
		public bool IsFollower { get { return IsServant || (m_Request != null && m_Request.GetRequest(EReqType.FollowTarget) != null); } }

		public bool IsFlollowTarget { get { return  m_Request.GetRequest(EReqType.FollowTarget) != null;} }
		
		bool m_isStoreNpc = false;
		public bool IsStoreNpc { get { return m_isStoreNpc; } set { m_isStoreNpc = value; } }
		
		bool m_InAllys;
		public bool InAllys { get { return m_InAllys; } set { m_InAllys = value; } }
		
		public bool CanRecive //lz-2016.10.13 错误 #4024 空对象bug
        { get { return (null == this) ? false : (Entity.viewCmpt != null ? Entity.viewCmpt.hasView : false); } }
		
		public bool hasAnyRequest
		{ get { return m_Request != null ? m_Request.HasAnyRequest() : false; } }
		
		bool m_IsHunger;
		public bool IsHunger { get { return m_IsHunger; } }
		
		float LOW_PERCENT = 0.15f;
		bool m_IsUncomfortable;
		public bool IsUncomfortable { get { return m_IsUncomfortable; } }
		
		bool m_IsLowHp;
		public bool IsLowHp { get { return m_IsLowHp; } }
		
		bool m_IsInSleepTime = false;
		public bool IsInSleepTime { get { return m_IsInSleepTime; } }
		
		bool m_IsInDinnerTime = false;
		public bool IsInDinnerTime { get { return m_IsInDinnerTime; } }

        public bool NpcUnableProcess { get { return m_BaseNpcOutMission || !NpcTypeDb.CanRun(NpcControlCmdId,ENpcControlType.Work) || hasAnyRequest || IsNeedMedicine || lineType == ELineType.TeamEat || lineType == ELineType.TeamSleep; } }

		public bool NpcUnableWork {get { return m_BaseNpcOutMission || !NpcTypeDb.CanRun(NpcControlCmdId,ENpcControlType.Work) || hasAnyRequest || IsNeedMedicine || lineType == ELineType.TeamEat || lineType == ELineType.TeamSleep; } }

		public bool NpcShouldStopProcessing{
			get{return m_BaseNpcOutMission ||  hasAnyRequest || !NpcTypeDb.CanRun(NpcControlCmdId,ENpcControlType.Work);}
		}

		public bool IsIdle()
		{
			return !hasAnyRequest && m_Type == ENpcType.Field;
		}
		
		public ENpcUnableWorkType unableWorkReason
		{
			get
			{
				if (hasAnyRequest || m_BaseNpcOutMission)
					return ENpcUnableWorkType.HasRequest;
				else if (IsNeedMedicine)
					return ENpcUnableWorkType.IsNeedMedicine;
				else if (IsHunger || lineType == ELineType.TeamEat)
					return ENpcUnableWorkType.IsHunger;
				else if (IsUncomfortable || lineType == ELineType.TeamEat)
					return ENpcUnableWorkType.IsUncomfortable;
				else if (IsLowHp || lineType == ELineType.TeamEat)
					return ENpcUnableWorkType.IsHpLow;
				else if (IsInSleepTime || lineType == ELineType.TeamSleep)
					return ENpcUnableWorkType.IsSleeepTime;
				else if (IsInDinnerTime || lineType == ELineType.TeamEat)
					return ENpcUnableWorkType.IsDinnerTime;
				
				return ENpcUnableWorkType.None;
			}
		}
		
		bool m_Processing;
		public bool Processing
		{ get { return m_Processing; }	 
			set { 
				if(m_Processing && !value)
				{
					CallBackProcess();
				}
				m_Processing = value; 
				CanWander = !m_Processing; 
			} 
		}
		
		public bool CallBackProcess()
		{
			return  this.Equals(null) ? false : NpcMgr.CallBackColonyNpcImmediately(Entity);
		}
		
		public bool NpcCanChat { get { return mChatTarget != null; } }
		
		bool m_IsTrainning = false;
        public bool IsTrainning { get { return m_IsTrainning; } set { setIsTrainning(value); CanWander = !m_IsTrainning; } }
        void setIsTrainning(bool value)
        {
            m_IsTrainning = value;
        }
		
		bool m_CanWander = false;
		public bool CanWander { get { return m_CanWander; } set { m_CanWander = value; } }
		
		bool m_CanTalk = true;
		public bool CanTalk
		{
			get
			{
                bool _IsInSpSence = Entity != null ? RandomDunGenUtil.IsInDungeon(Entity) : false;
                if (IsServant || Entity.IsDeath() || _IsInSpSence)
                    return false;
				
				return m_CanTalk;
			}
			set
			{
				m_CanTalk = value;
			}
		}
		
		bool m_CanHanded = false;
		public bool CanHanded { get { return m_CanHanded; } set { m_CanHanded = value; } }
		
		public bool HasConsume { get { return IsServant || Creater != null; } }
		
		bool m_NpcInAlert = false;
		public bool NpcInAlert { get { return m_NpcInAlert; } set { m_NpcInAlert = value; } }
		
		bool m_BaseNpcOutMission = false;
		public bool BaseNpcOutMission { get { return m_BaseNpcOutMission; } 
            set 
            {
                if (!m_BaseNpcOutMission && value)
                {
					MissionReady();
				}
				
				if (m_BaseNpcOutMission && !value)
                {
					MissionFinish();
				}
				
				m_BaseNpcOutMission = value;
            } 
        }

		void MissionReady()
		{
			NpcMgr.NpcMissionReady(Entity);
		}

		void MissionFinish()
		{
			NpcMgr.NpcMissionFinish(Entity);
		}

		public bool NpcNeedRest(float Percent = 0.1f)
		{
			if (Entity.aliveEntity == null)
				return false;
			
			float hunger = Entity.aliveEntity.GetAttribute(AttribType.Hunger);
			float maxHunger = Entity.aliveEntity.GetAttribute(AttribType.HungerMax);
			float comfort = Entity.aliveEntity.GetAttribute(AttribType.Comfort);
			float maxComfort = Entity.aliveEntity.GetAttribute(AttribType.ComfortMax);
			
			return hunger <= maxHunger * Percent || comfort <= maxComfort * Percent;
		}
		
		bool m_HasNearleague = false;
		public bool HasNearleague { get { return m_HasNearleague; } }
		
		bool m_bRunAway = false;
		public bool bRunAway { get { return m_bRunAway; } set { m_bRunAway = value; } }
		#endregion
		
		#region Npc Talk
        public int voiceType { get; set; }
		NpaTalkAgent mTalkAgent;
		public NpaTalkAgent TalkAngent
		{
			get
			{
				if (mTalkAgent == null)
					mTalkAgent = new NpaTalkAgent(Entity);
				
				return mTalkAgent;
			}
		}
		
		NpcThinkAgent mThinkAgent;
		public NpcThinkAgent ThinkAgent
		{
			get
			{
				if (mThinkAgent == null)
					mThinkAgent = new NpcThinkAgent();
				
				return mThinkAgent;
			}
		}

		NpcCheckTime mNpcCheckTime;
		public NpcCheckTime npcCheck
		{
			get
			{
				if(mNpcCheckTime == null)
					mNpcCheckTime = new NpcCheckTime();

				return mNpcCheckTime;
			}
		}
		
		void ThinkAdd(EThinkingType type)
		{
			if (ThinkAgent != null && NpcThinkDb.CanDoing(Entity, type))
				ThinkAgent.AddThink(type);
		}
		
		void ThinkRemove(EThinkingType type)
		{
			if (ThinkAgent != null)
			{
				ThinkAgent.RemoveThink(type);
			}
			
		}
		
		public void AddTalkInfo(ENpcTalkType talkType, ENpcSpeakType spType, bool canLoop = false)
		{
			TalkAngent.AddAgentInfo(new NpaTalkAgent.AgentInfo(talkType, spType, canLoop));
		}
		
		public bool RmoveTalkInfo(ENpcTalkType type)
		{
			return TalkAngent.RemoveAgentInfo(type);
		}
		
		public bool SendTalkMsg(ENpcTalkType talkType, ENpcSpeakType spType, float loopTime = 0.0f)
		{
			return SendTalkMsg((int)talkType, loopTime, spType);
		}
		
		public bool SendTalkMsg(int caseid, float time = 0.0f, ENpcSpeakType speaker = ENpcSpeakType.TopHead)
		{
			int scenarioid = NpcRandomTalkDb.GetTalkCase(caseid);
			
			if (scenarioid < 0)
				return false;
			//send to ui 
			if (Entity == null || Entity.IsDead() || Entity.enityInfoCmpt == null || Entity.aliveEntity == null || Entity.aliveEntity.isDead)
				return false;
			Entity.enityInfoCmpt.NpcSayOneWord(scenarioid, time, speaker);

            //play audio
            NpcRandomTalkAudio.PlaySound(Entity,caseid, scenarioid);
			return true;
		}
		#endregion
		
		#region CampNpc
		Camp m_Camp;
		public Camp Campsite
		{
			get { return m_Camp; }
			set
			{
				if (m_Camp != value)
				{
					m_Camp = value;
					
					UpdateType();
				}
			}
		}
		
		private bool m_UpdateCampsite;
		public bool UpdateCampsite
		{
			set { m_UpdateCampsite = value; }
		}
		
		#endregion
		
		RandomItemObj m_RandomItemobj;
		public RandomItemObj mRandomItemObj
		{
			get
			{
				return m_RandomItemobj;
			}
			set
			{
				if ((m_RandomItemobj != null) && (value == null))
				{
					m_RandomItemobj.TryGenObject();
				}
				m_RandomItemobj = value;
			}
		}
		
		int m_ReviveTime = 10;
		public int ReviveTime
		{
			get { return m_ReviveTime; }
			set { m_ReviveTime = value; }
		}
		
		int m_MountID;
		public int MountID
		{
			get { return m_MountID; }
			set
			{
				if (m_MountID != value)
				{
					if (value == 0)
					{
						m_Mount = null;

//						if (Entity.enityInfoCmpt != null)
//							Entity.enityInfoCmpt.ShowName(true);
					}
					else
					{
						PeEntity entity = EntityMgr.Instance.Get(value);
						if (entity != null)
						{
							PeTrans trans = entity.GetComponent<PeTrans>();
							Transform tr = PEUtil.GetChild(trans.existent, "CarryUp");
							if (tr != null)
							{
								if (Entity.biologyViewCmpt != null)
									Entity.biologyViewCmpt.ActivateCollider(false);
								
								if (Entity.motionMgr != null)
									Entity.motionMgr.FreezePhyState(GetType(), true);
								
								if (Entity.enityInfoCmpt != null){
                                    Entity.enityInfoCmpt.ShowName(false);
                                    Entity.enityInfoCmpt.ShowMissionMark(false);
                                }
								
								if (Entity.biologyViewCmpt != null)
									Entity.biologyViewCmpt.ActivateInjured(false);
								
								Req_SetIdle("BeCarry");
								m_Mount = tr;
								
							}
						}
					}
					
					m_MountID = value;
					Req_Mount(value);
				}
			}
		}
		
		public Vector3 NpcPostion { get { return Entity.peTrans.trans.position; } }
		
		Vector3 mFixedPointPos;
		public Vector3 FixedPointPos { get { return mFixedPointPos; } set { SetFixPos(value); } }
		public void SetFixPos(Vector3 pos)
		{
            bool hide = pos.x < 0 && pos.z < 0;//任务隐藏npc时 y z 为-100
            if (PeGameMgr.IsSingleStory && !hide && !WorldCollider.IsPointInWorld(pos))
                return;

            //if (pos.y < -5000f && CSMain.s_MgCreator.Assembly != null)//npc采集时被修改fixed值时，如果是基地npc,默认为基地核心位置
            //    pos = CSMain.s_MgCreator.Assembly.Position;

			mFixedPointPos = pos;
		}
		
		float mStandRotate;
		public float StandRotate { get { return mStandRotate; } set { mStandRotate = value; } }
		
		int mNpcControlCmdId = 1;
		public int NpcControlCmdId { get { return mNpcControlCmdId; } set {
				UpdateNpcControlInfo(value);
				mNpcControlCmdId = value; 
				
			} 
		}
		
		List<Collider> m_IgnorePlantColliders;
		
		PeTrans m_mianplayerTran;
		public Vector3 PlayerPostion
		{
			get
			{
				if (m_mianplayerTran == null)
					m_mianplayerTran = PeCreature.Instance.mainPlayer.peTrans;
				return m_mianplayerTran.position;
			}
		}
		
		PEBuilding m_OccopyBuild;
		public PEBuilding OccopyBuild { get { return m_OccopyBuild; } set { m_OccopyBuild = value; } }
		
		public float NpcHppercent { get { return Entity != null ? Entity.HPPercent : 0.0f; } }
		
		bool NpcHasSleep = false;
		int NpcsleepBuffId = 0;
		float sleepWaitTime = 3.0f;
		float sleepStartTime;
		
		#region Update Fun
		void UpdateAllys()
		{
			NpcCmpt[] followers = ServantLeaderCmpt.Instance.GetServants();
			List<NpcCmpt> mForcedFollowers = ServantLeaderCmpt.Instance.mForcedFollowers;
			List<NpcCmpt> Servants = new List<NpcCmpt>();
			if (MissionManager.Instance != null)
				Servants = MissionManager.Instance.m_PlayerMission.followers;
			
			NpcCmpt Pelayer = null;
			if (PeCreature.Instance != null && PeCreature.Instance.mainPlayer != null)
				Pelayer = PeCreature.Instance.mainPlayer.GetCmpt<NpcCmpt>();
			
			if (m_Allys == null)
				m_Allys = new List<NpcCmpt>();
			
			m_Allys.Clear();
			if (Pelayer != null)
			{
				m_Allys.Add(Pelayer);
			}
			
			for (int i = 0; i < followers.Length; i++)
			{
				if (followers[i] != null) //&&(followers[i].Follwerentity.Id != Follwerentity.Id))
					m_Allys.Add(followers[i]);
			}
			
			foreach (NpcCmpt forceNpc in mForcedFollowers)
			{
				if (!m_Allys.Contains(forceNpc))
				{
					m_Allys.Add(forceNpc);
				}
			}
			
			if (Servants != null)
			{
				foreach (NpcCmpt Npc in Servants)
				{
					if ((!m_Allys.Contains(Npc)) && (Npc != this))
					{
						m_Allys.Add(Npc);
					}
				}
			}
			
		}
		
		void UpdateType()
		{
			if (m_Master != null)
				m_Type = ENpcType.Follower;
			else if (Creater != null)
				m_Type = ENpcType.Base;
			else if (m_Camp != null)
				m_Type = ENpcType.Campsite;
			else
				m_Type = ENpcType.Field;
		}
		
		void UpdateMount()
		{

			if (m_MountID != 0 && m_Mount == null)
			{
				RQIdle idle = Req_GetRequest(EReqType.Idle) as RQIdle;
				if (idle != null && idle.state == "BeCarry")
				{
					PeEntity entity = EntityMgr.Instance.Get(m_MountID);
					if (entity != null)
					{
						PeTrans trans = entity.GetComponent<PeTrans>();
						Transform tr = PEUtil.GetChild(trans.existent, "CarryUp");
						if (tr != null)
						{
							m_Mount = tr;
							
							if (Entity.biologyViewCmpt != null)
								Entity.biologyViewCmpt.ActivateCollider(false);
							
							if (Entity.motionMgr != null)
								Entity.motionMgr.FreezePhyState(typeof(StroyManager), true);
							
							if (Entity.enityInfoCmpt != null){
                                Entity.enityInfoCmpt.ShowName(false);
                                Entity.enityInfoCmpt.ShowMissionMark(false);
                            }
							
						}
					}
				}
			}
            //if (PeGameMgr.IsMulti && MissionManager.Instance.HadCompleteMission(18) && !MissionManager.Instance.HadCompleteMission(27))
            //{
            //    if (Entity.Id == 9008)
            //        MountID = 0;
            //}
            if (m_Mount != null && !Entity.isRagdoll)
			{
				Entity.peTrans.position = m_Mount.position;
				Entity.peTrans.rotation = m_Mount.rotation;
			}
			
		}
		
		void UpdateNpcControlInfo(int cmdid)
		{
			NpcTypeDb.Item item = NpcTypeDb.Get(cmdid);
			if (item == null)
				return;
			
			if (Entity.target != null)
			{
				Entity.target.ClearEnemy();
				Entity.target.Scan = item.CanRun(ENpcControlType.AddHatred);
				Entity.target.IsAddHatred = item.CanRun(ENpcControlType.InjuredHatred);
				Entity.target.CanTransferHatred = item.CanRun(ENpcControlType.ReceiveHatred);
			}
			
			m_CanTalk = item.CanRun(ENpcControlType.CanTalk);
			m_CanHanded = item.CanRun(ENpcControlType.CanHanded);
			
			return;
		}
		
		void UpdateAbility()
		{
			if(m_AbilityIdes != null && m_AbilityIdes.GetDrity())
			{
				Npcskillcmpt.SetAblitiyIDs(m_AbilityIdes);
				m_AbilityIdes.SetDirty(false);
			}
		}
		
		void UpdateBattle()
		{
            switch (m_Battle)
            {
                case ENpcBattle.Attack:
                    if (Entity.target != null)
                    {
                        Entity.target.Scan = true;
                        Entity.target.IsAddHatred = true;
                        Entity.target.CanActiveAttck = true;
                        Entity.target.CanTransferHatred = true;
                        m_FollowerSentry = false;
                    }
                    break;
                case ENpcBattle.Defence:
                    if (Entity.target != null)
                    {
                        Entity.target.ClearEnemy();
                        Entity.target.Scan = false;
                        Entity.target.IsAddHatred = true;
                        Entity.target.CanActiveAttck = true;
                        Entity.target.CanTransferHatred = true;
                        m_FollowerSentry = false;
                    }
                    break;
                case ENpcBattle.Passive:
                    if (Entity.target != null)
                    {
                        Entity.target.ClearEnemy();
                        Entity.target.Scan = false;
                        Entity.target.IsAddHatred = false;
                        Entity.target.CanTransferHatred = false;
                        Entity.target.CanActiveAttck = false;
                        m_FollowerSentry = false;
                    }
                    break;
                case ENpcBattle.Evasion:
                    if (Entity.target != null)
                    {
                        Entity.target.ClearEnemy();
                        Entity.target.Scan = true;
                        Entity.target.IsAddHatred = false;
                        Entity.target.CanTransferHatred = false;
                        Entity.target.CanActiveAttck = false;
                        m_FollowerSentry = false;
                    }
                    break;
                case ENpcBattle.Stay:
                    if (Entity.target != null)
                    {
                        Entity.target.ClearEnemy();
                        Entity.target.Scan = false;
                        Entity.target.IsAddHatred = true;
                        Entity.target.CanActiveAttck = true;
                        Entity.target.CanTransferHatred = true;
                        m_FollowerSentry = true;
                    }
                    break;
                default:
                    break;
            }
		}
		
		void UpdateNpcTalk()
		{
			if (Entity == null)
				return;
			
			if (Entity.aliveEntity == null || Entity.aliveEntity.isDead)
				return;
			
			if (Entity.commonCmpt == null || Entity.commonCmpt.IsPlayer)
				return;
			
			if (IsServant)
			{
				if (IsNeedMedicine)
					AddTalkInfo(ENpcTalkType.NpcSick, ENpcSpeakType.TopHead, true);
				
				if (Entity.target != null && Entity.target.HasEnemy())
					AddTalkInfo(ENpcTalkType.NpcCombat, ENpcSpeakType.TopHead, true);
				
				if(Entity.motionEquipment!= null && Entity.motionEquipment.ActiveableEquipment != null && !CheckNpcEquipment_Durability())
					AddTalkInfo(ENpcTalkType.Follower_LackDurability, ENpcSpeakType.TopHead, true);
				
				if(!CheckNpcEquipment_Ammunition())
					AddTalkInfo(ENpcTalkType.Follower_LackAmmunition, ENpcSpeakType.TopHead, true);
				
				TalkAngent.RunAttrAgent(Entity);
			}
			
			if (Creater != null)
			{
				TalkAngent.RunAgent();
			}
		}
		
		void UpdateNpcThink()
		{
			if(updateCnt %10 == 0)
			{
				if (Entity == null)
					return;
				
				if (Entity.aliveEntity == null || Entity.aliveEntity.isDead)
					return;
				
				if (Entity.commonCmpt == null || Entity.commonCmpt.IsPlayer)
					return;
				//base npc
				if (Creater != null && Creater.Assembly != null)
				{
					//生病
					if (IsNeedMedicine)
					{
						AddTalkInfo(ENpcTalkType.NpcSick, ENpcSpeakType.Both, true);
						ThinkAdd(EThinkingType.Cure);
						
					}
					else
						ThinkAgent.RemoveThink(EThinkingType.Cure);
					
					
					//舒适度低
					if (m_IsUncomfortable)//|| NpcInRestTime()
						ThinkAdd(EThinkingType.Sleep);
					
					
					//睡觉时间段
					if (lineType == ELineType.TeamSleep)//npcCheck.IsSleepTimeSlot((float)GameTime.Timer.HourInDay))
						ThinkAdd(EThinkingType.Sleep);
					else
						ThinkAgent.RemoveThink(EThinkingType.Sleep);
					
					//饥饿
					//在规定时间段内且厂库有合适的吃的时才考虑Dining
					if (CSNpcTeam.checkTime != null && CSNpcTeam.checkTime.IsEatTimeSlot((float)GameTime.Timer.HourInDay) && 
					    NpcEatDb.CanEatSthFromStorages(Entity, Creater.Assembly.Storages))					
						ThinkAdd(EThinkingType.Dining);					
					else
						ThinkAgent.RemoveThink(EThinkingType.Dining);

//					if(lineType == ELineType.TeamEat)
//						ThinkAdd(EThinkingType.Dining);
//					else
//						ThinkAgent.RemoveThink(EThinkingType.Dining);
				
				}

				if(Creater == null)
				{
					if (m_IsUncomfortable || m_IsHunger || m_IsLowHp)
						ThinkAdd(EThinkingType.Dining);
					else
						ThinkAgent.RemoveThink(EThinkingType.Dining);
				}

				if (Entity.target != null && !Enemy.IsNullOrInvalid(Entity.attackEnemy))
					ThinkAdd(EThinkingType.Combat);
				else
					ThinkAgent.RemoveThink(EThinkingType.Combat);
				
				if (hasAnyRequest)
					ThinkAdd(EThinkingType.Mission);	
				else		
					ThinkAgent.RemoveThink(EThinkingType.Mission);
			}
		}

        #region lz-2016.12.06 播放chenzhen的音乐用

        float m_DistanceWithPlayer = float.MaxValue;
        const float m_ChenZhenPlayMusicRadio = 3f;
        const int m_ChenZhenID = 9007;
        const int m_ChenZhenMusicID = 4513;
        const float m_PlayIntervalTime = 18000f;
        const int m_ChenZhenMusicMissionID = 10047;
        float m_WaitPlayStartTime = -18000f;
        AudioController m_ChenZhenMusicAduioCtrl;

        void UpdateDisWithPlayer()
        {
            if (Entity.hasView)
            {
                if (null != PeCreature.Instance && null != PeCreature.Instance.mainPlayer)
                {
                    m_DistanceWithPlayer = Vector3.Distance(Entity.position, PeCreature.Instance.mainPlayer.position);
                }
                if (Entity.Id == m_ChenZhenID)
                {
                    //lz-2016.12.06 chenzhen距离玩家某个距离内 
                    if (m_DistanceWithPlayer <= m_ChenZhenPlayMusicRadio)
                    {
                        //lz-2017.02.24 
                        if (MissionManager.Instance && !MissionManager.Instance.HadCompleteMission(m_ChenZhenMusicMissionID))
                        {
                            //lz-2016.12.15 到了间隔时间
                            if (Time.realtimeSinceStartup - m_WaitPlayStartTime >= m_PlayIntervalTime)
                            {
                                //lz-2016.12.15 没有在播放中
                                if (null == m_ChenZhenMusicAduioCtrl && null != Entity.peTrans)
                                {
                                    m_ChenZhenMusicAduioCtrl = AudioManager.instance.Create(Entity.position, m_ChenZhenMusicID, Entity.peTrans.realTrans, true, true);
                                    m_ChenZhenMusicAduioCtrl.DestroyEvent += ChenZhenMusicDeleteEvent;
                                }
                            }
                        }
                    }
                }
            }
        }

        void ChenZhenMusicDeleteEvent(AudioController audio)
        {
            if (audio == m_ChenZhenMusicAduioCtrl)
            {
                m_ChenZhenMusicAduioCtrl.DestroyEvent -= ChenZhenMusicDeleteEvent;
                m_ChenZhenMusicAduioCtrl = null;
                //lz-2016.12.15 播放完重置间隔时间
                m_WaitPlayStartTime = Time.realtimeSinceStartup;
            }
        }

        #endregion

        void UpdateNpcMotionStyle()
		{
			switch (m_MotionStyle)
			{
			case ENpcMotionStyle.InjuredSitEX:
			{
				Entity.motionMove.baseMoveStyle = MoveStyle.Abnormal;
				//Req_SetIdle("InjuredSitEX");
			}
				break;
			default:
			{
				Entity.motionMove.baseMoveStyle = MoveStyle.Normal;
				// Req_Remove(EReqType.Idle);
			}
				break;
				
			}
		}
		
		void UpdateNpcAttr()
		{
			if (Entity == null)
				return;
			
			if (Entity.IsDeath())
				return;
			
			float percet = Entity.GetAttribute(AttribType.Hp) / Entity.GetAttribute(AttribType.HpMax);
			m_IsLowHp = (percet <= LOW_PERCENT);
			
			percet = Entity.GetAttribute(AttribType.Comfort) / Entity.GetAttribute(AttribType.ComfortMax);
			m_IsUncomfortable = (percet <= LOW_PERCENT);
			
			percet = Entity.GetAttribute(AttribType.Hunger) / Entity.GetAttribute(AttribType.HungerMax);
			m_IsHunger = (percet <= LOW_PERCENT);

		}
		
		void UpdateTime()
		{
			if (Entity == null)
				return;
			
			if (Campsite != null || Creater != null)
			{

				m_IsInDinnerTime = npcCheck.IsEatTimeSlot((float)GameTime.Timer.HourInDay);
				m_IsInSleepTime  = npcCheck.IsSleepTimeSlot((float)GameTime.Timer.HourInDay);
			}

			if (NpcPackage != null && NpcPackage.IsFull())
				AddTalkInfo(ENpcTalkType.Follower_Pkg_full, ENpcSpeakType.TopHead, true);
			
			if (NpcPackage != null && !NpcPackage.IsFull())
				RmoveTalkInfo(ENpcTalkType.Follower_Pkg_full);
			
			if (Time.time - sleepStartTime >= sleepWaitTime)
				NpcSleep();
			
			
		}
		
		void CheckNearLeague()
		{
			int playerID = (int)Entity.GetAttribute(AttribType.DefaultPlayerID);
			m_HasNearleague = EntityMgr.Instance.NearEntityModel(Entity.peTrans.position, 0.3f, playerID, false, Entity);
		}
		
		bool CheckNpcEquipment_Ammunition()
		{
			if(Entity.motionEquipment == null)
				return false;
			
			return  Entity.motionEquipment.CheckEquipmentAmmunition();
		}
		
		bool CheckNpcEquipment_Durability()
		{
			if(Entity.motionEquipment == null)
				return false;
			
			return  Entity.motionEquipment.CheckEquipmentDurability();
		}
		
		#endregion
		
		bool battachEvent = false;
		void AttachEvent()
		{
			if (null != Pathea.PeCreature.Instance.mainPlayer)
			{
				Pathea.Action_Fell actionFell = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.MotionMgrCmpt>().GetAction<Pathea.Action_Fell>();
				if (null != actionFell)
				{
					battachEvent = true;
					actionFell.startFell += OnStartFell;
					actionFell.endFell += OnEndFell;
				}
			}
		}
		
		#region IEnumerator
		IEnumerator IgnorePlant(float radius)
		{
			m_IgnorePlantColliders = new List<Collider>();
			
			while (true)
			{
				if (m_Job == ENpcJob.Farmer)
				{
					Collider[] colliders = Physics.OverlapSphere(Entity.peTrans.position, radius, 1 << Pathea.Layer.Default);
					foreach (Collider collider in colliders)
					{
						if (!m_IgnorePlantColliders.Contains(collider))
						{
							Entity.biologyViewCmpt.IgnoreCollision(collider);
							m_IgnorePlantColliders.Add(collider);
						}
					}
				}
				else
				{
					if (m_IgnorePlantColliders.Count > 0)
					{
						foreach (Collider collider in m_IgnorePlantColliders)
						{
							Entity.biologyViewCmpt.IgnoreCollision(collider, false);
						}
						
						m_IgnorePlantColliders.Clear();
					}
				}
				
				yield return new WaitForSeconds(1.0f);
			}
		}
		
		IEnumerator CalculateCamp()
		{
			while (true)
			{
				if (Creater == null)
				{
					if (!m_UpdateCampsite)
						Campsite = null;
					else
					{
						if (Campsite == null)
						{
							Campsite = Camp.GetCamp(Entity.peTrans.position);
							if (Campsite != null)
								Campsite.AddNpcIntoCamp(Entity.Id);
						}
						else
						{
							Camp _tmpCampsite = Camp.GetCamp(Entity.peTrans.position);
							if (_tmpCampsite == null)
								Campsite.RemoveFromCamp(Entity.Id);
							
							if (_tmpCampsite != null && !_tmpCampsite.Equals(Campsite))
							{
								_tmpCampsite.AddNpcIntoCamp(Entity.Id);
								Campsite.RemoveFromCamp(Entity.Id);
							}
							Campsite = _tmpCampsite;
							
						}
					}
				}
				
				if (Creater != null && Campsite != null)
				{
					Campsite = null;
				}
				yield return new WaitForSeconds(5.0f);
			}
		}
		
		IEnumerator CheckMedicine()
		{
			yield return new WaitForSeconds(0.5f);
			while (true)
			{
				if (Entity != null && Entity.Alnormal != null)
				{
					if (m_illAbnormals == null)
						m_illAbnormals = new List<PEAbnormalType>();
					
					m_IsNeedMedicine = NeedToMedical();
					if (!m_IsNeedMedicine)
						MedicalState = ENpcMedicalState.Cure;
				}
				yield return new WaitForSeconds(0.5f);
			}
		}
		
		IEnumerator ThinkingSth()
		{
			while (true)
			{
				UpdateNpcAttr();
				UpdateTime();
				yield return new WaitForSeconds(5.0f);
			}
		}
		
		IEnumerator NpcTalk(float time)
		{
			while (true)
			{
				UpdateNpcTalk();
				if(Entity.hasView){
					CheckNearLeague();
				}
				float wTime = UnityEngine.Random.Range(1.0f, time);
				yield return new WaitForSeconds(wTime);
			}
		}

		#endregion
		
		#region Story
		
		public bool Req_UseSkill()
		{
			if (Net != null && !Net.hasOwnerAuth)
			{
				Net.RPCServer(EPacketType.PT_NPC_RequestAiOp, (int)EReqType.UseSkill);
				return true;
			}
			else
			{
				if (m_Request != null && m_Request.Register(EReqType.UseSkill, null) != null)
					return true;
				else
					return false;
			}
		}
		
		public bool Req_Mount(int mountId)
		{
			if (Net != null && Net.hasOwnerAuth)
			{
				Net.RPCServer(EPacketType.PT_NPC_Mount, mountId);
				return true;
			}
			return false;
		}
		
		public bool Req_PauseAll()
		{
			if (m_Request != null && m_Request.Register(EReqType.PauseAll) != null)
				return true;
			else
				return false;
		}
		
		public bool Req_Dialogue(params object[] objs)
		{
			if (m_Request != null && m_Request.Register(EReqType.Dialogue, objs) != null)
				return true;
			else
				return false;
		}
		
		public bool Req_Translate(Vector3 position,bool adjust = true,bool lostController = true)
		{
			if (Net != null && !Net.hasAuth)
			{
				Net.RPCServer(EPacketType.PT_NPC_RequestAiOp, (int)EReqType.Translate, position, adjust, lostController);
				return true;
			}
			else
			{
				if (Entity.peTrans == null || Entity.peTrans.position != position)
				{
                    if( (Net!= null && Net.hasOwnerAuth) || Net == null)
					    if (m_Request != null && m_Request.Register(EReqType.Translate, position,adjust) != null)
						    return true;
				}
			}
			
			return false;
		}
		
		
		public bool Req_Rotation(Quaternion rotation)
		{
			// 			if(Net != null && !Net.IsController)
			// 			{
			// 				Net.RPCServer(EPacketType.PT_NPC_RequestAiOp,(int)EReqType.Rotate,rotation);
			// 				return true;
			// 			}
			// 			else
			// 			{
			if (Entity.peTrans == null || Quaternion.Angle(Entity.peTrans.rotation, rotation) > 1.0f)
			{
				if (m_Request != null && m_Request.Register(EReqType.Rotate, rotation) != null)
					return true;
			}
			//			}
			
			return false;
		}
		
		
		public bool Req_SetIdle(string name)
		{
			if (m_Request != null && m_Request.Register(EReqType.Idle, name) != null)
				return true;
			else
				return false;
		}
		
        //public bool Req_SetLie(string name)
        //{
        //    if (m_Request != null && m_Request.RequestLie(EReqType.Idle, name) != null)
        //        return true;
        //    return false;
        //}

		public bool Req_PlayAnimation(string name, float time,bool play = true)
		{
			if (Net != null && !Net.hasOwnerAuth)
			{
				Net.RPCServer(EPacketType.PT_NPC_RequestAiOp, (int)EReqType.Animation, name, time,play);
                return true;
            }
			else
			{
				if (m_Request != null && m_Request.Register(EReqType.Animation, name, time,play) != null)
					return true;
				else
					return false;
			}
		}
		
		public bool Req_MoveToPosition(Vector3 position, float stopRadius, bool isForce, SpeedState state)
		{
            if (Net != null && !Net.hasOwnerAuth && Entity != null && Entity.viewCmpt != null && Entity.viewCmpt.hasView)
			{
				Net.RPCServer(EPacketType.PT_NPC_RequestAiOp, (int)EReqType.MoveToPoint, position, stopRadius, isForce, (int)state);
				return true;
			}
			else if (Net != null && !Net.hasAuth)
			{
				Net.RPCServer(EPacketType.PT_NPC_RequestAiOp, (int)EReqType.MoveToPoint, position, stopRadius, isForce, (int)state);
				return true;
			}
			else
			{
                RQMoveToPoint req = Req_GetRequest(EReqType.MoveToPoint) as RQMoveToPoint;
                if (req != null && req.position == position && Net != null)
                    return false;
                if (m_Request != null && m_Request.Register(EReqType.MoveToPoint, position, stopRadius, isForce, state) != null)
					return true;
				else
					return false;
			}
		}
		
		public bool Req_TalkMoveToPosition(Vector3 position, float stopRadius, bool isForce, SpeedState state)
		{
            if (Net != null && !Net.hasOwnerAuth && Entity != null && Entity.viewCmpt != null && Entity.viewCmpt.hasView)
            {
                //Net.RPCServer(EPacketType.PT_NPC_RequestAiOp, (int)EReqType.TalkMove, position, stopRadius, isForce, (int)state);
                return true;
            }
            else if (Net != null && !Net.hasAuth)
            {
                //Net.RPCServer(EPacketType.PT_NPC_RequestAiOp, (int)EReqType.TalkMove, position, stopRadius, isForce, (int)state);
                return true;
            }
            else
            {
				if (m_Request != null && m_Request.Register(EReqType.TalkMove, position, stopRadius, isForce, state) != null)
					return true;
				else
					return false;
			}
		}
		
		
		
		public bool Req_FollowPath(Vector3[] path, bool isLoop,SpeedState state = SpeedState.Run,bool fromnet = false)
		{
            if (Net != null/* && !Net.hasOwnerAuth && Entity != null && Entity.viewCmpt != null && Entity.viewCmpt.hasView*/)
            {
                if(!fromnet)
                    Net.RPCServer(EPacketType.PT_NPC_RequestAiOp, (int)EReqType.FollowPath, path, isLoop);
                RQFollowPath req = Req_GetRequest(EReqType.FollowPath) as RQFollowPath;
                if (req != null && path.Length > 1)
                {
                    Vector3[] vpos = new Vector3[2];
                    vpos[0] = path[0];
                    vpos[1] = path[path.Length - 1];
                    if (!req.Equal(vpos) && (m_Request != null && m_Request.Register(EReqType.FollowPath, path, isLoop, state) != null))
                        return true;
                }
                else if (m_Request != null && m_Request.Register(EReqType.FollowPath, path, isLoop, state) != null)
                    return true;
                else
                    return false;
                return true;
            }
            else
            {
				if (m_Request != null && m_Request.Register(EReqType.FollowPath, path, isLoop,state) != null)
					return true;
				else
					return false;
			}
		}
		
		
		public bool Req_FollowTarget(int targetId,Vector3 targetPos ,int dirTargetID ,float tRadius , bool bNet = false,bool send = true)
		{
			if (Net != null)
			{
				if (!bNet && send)
				{
                    Net.RPCServer(EPacketType.PT_NPC_RequestAiOp, (int)EReqType.FollowTarget, targetId, targetPos, dirTargetID, tRadius);
				}
				if (Req_Contains(EReqType.FollowTarget))
					return false;
				PeEntity entity = EntityMgr.Instance.Get(targetId);
                if (entity != null && m_Request != null && m_Request.Register(EReqType.FollowTarget, targetId, targetPos, dirTargetID, tRadius) != null)
					return true;
				else
					return false;
			}
			else
			{
				PeEntity entity = EntityMgr.Instance.Get(targetId);
                if (entity != null && m_Request != null && m_Request.Register(EReqType.FollowTarget, targetId, targetPos, dirTargetID, tRadius) != null)
					return true;
				else
					return false;
			}
		}
		
		
		public bool Req_Salvation(int id, bool carryUp)
		{
            GameUI.Instance.mShopWnd.Hide();
            NetworkInterface other = NetworkInterface.Get(id);
			if (Net != null && Net.hasOwnerAuth != other.hasOwnerAuth && Net.hasOwnerAuth)
			{
				Net.RPCServer(EPacketType.PT_NPC_RequestAiOp, (int)EReqType.Salvation, id, carryUp);
				return true;
			}
			else if (Net != null && Net.hasOwnerAuth && other.hasOwnerAuth)
			{
				if (m_Request != null && m_Request.Register(EReqType.Salvation, id, carryUp) != null)
					return true;
				else
					return false;
			}
			else if (Net == null)
			{
				if (m_Request != null && m_Request.Register(EReqType.Salvation, id, carryUp) != null)
					return true;
				else
					return false;
			}
			return true;
		}
		
		public void Req_Remove(EReqType type)
		{
			if (m_Request != null)
			{
				m_Request.RemoveRequest(type);
			}
		}
		
		public void Req_Remove(Request request)
		{
			if (m_Request != null)
			{
				m_Request.RemoveRequest(request);
			}
		}
		
		public bool Req_Contains(EReqType type)
		{
			if (m_Request != null)
			{
				return m_Request.Contains(type);
			}
			
			return false;
		}
		
		public int GetFollowTargetId()
		{
			if (m_Request != null)
			{
				return m_Request.GetFollowID();
			}
			return -1;
		}
		
		public Request Req_GetRequest(EReqType type)
		{
			if (m_Request != null)
			{
				return m_Request.GetRequest(type);
			}
			return null;
		}
		#endregion
		
		#region Event

		public event Action<PeEntity> OnServentDie;
		public event Action<PeEntity> OnServentRevive;
		public void AddServantDeathEvent(PeEntity servant)
		{
			if(OnServentDie != null)
				OnServentDie(servant);
		}


		public void AddServantReviveEvent(PeEntity servant)
		{
			if(OnServentRevive != null)
				OnServentRevive(servant);
		}


		void OnDeath(SkEntity self, SkEntity caster)
		{
			State = ENpcState.Dead;
			CSMain.KickOutFromHospital(Entity);
			AddServentDieSign();
			
			if (IsServant)
				PeLogicGlobal.Instance.ServantReviveAtuo(Entity, 5.0f);
            if (PeGameMgr.IsMultiStory)
            {
                NpcMissionData data = NpcMissionDataRepository.GetMissionData(Entity.Id);
                if (null == data)
                    return;
                if (data.m_Rnpc_ID != -1)
                {
                    if (Entity.entityProto.proto == EEntityProto.Npc)
                    {
                        if(ReviveTime < 0)
                            PlayerNetwork.mainPlayer.CreateSceneItem("ash_ball", Entity.position, "1339,1", Entity.Id);
                    }
                    else if(Entity.entityProto.proto == EEntityProto.RandomNpc)
                    {
                        Pathea.RandomNpcDb.Item item = Pathea.RandomNpcDb.Get(data.m_Rnpc_ID);
                        if (item != null)
                        {
                            if (item.reviveTime < 0)
                                PlayerNetwork.mainPlayer.CreateSceneItem("ash_ball", Entity.position, "1339,1", Entity.Id);
                        }
                    }                        
                }
            }

        }
		
		void OnRevive(SkEntity entity)
		{
            //lz-2016.07.16 npc复活后把状态置空
            State = ENpcState.UnKnown;
			RemoveServentSign();

			if (IsServant)
				AddTalkInfo(ENpcTalkType.NpcResurgence, ENpcSpeakType.TopHead, false);
		}

		void OnAttack(SkEntity skEntity, float damage)
		{
			PeEntity tarEntity = skEntity.GetComponent<PeEntity>();
			if (tarEntity != null && tarEntity != Entity)
			{
                NpcHatreTargets.Instance.TryAddInTarget(Entity,tarEntity,damage);
			}
		}

		void OnDamage (SkEntity entity, float damage)
		{
			if (null == Alive || null == entity)
				return;
			
			PeEntity peEntity = entity.GetComponent<PeEntity>();
			if(peEntity == Entity)
				return ;

            NpcHatreTargets.Instance.TryAddInTarget(Entity,peEntity,damage,true);
		}

        void OnBeEnemyEnter(PeEntity attacker)
        {
            OnSkillTarget(attacker.skEntity);
            AddEnemyLocked(attacker);
        }

        void OnBeEnemyExit(PeEntity enemyEntity)
        {
            RemoveEnemyLocked(enemyEntity);
        }

		void OnEnemyAchieve(PeEntity enemyEntity)
		{
			if (Entity.aliveEntity != null && Entity.motionMgr != null)
			{
				Entity.motionMgr.EndAction(PEActionType.Sleep);
				NpcsleepBuffId = 0;
				NpcHasSleep = false;
			}
		}

        void OnEnemyLost(PeEntity enemyEntity)
        {
           
        }

		void OnSkillTarget(SkEntity caster)
		{
			if (null == Alive || null == caster)
				return;
			
			int playerID = (int)Alive.GetAttribute ((int)AttribType.DefaultPlayerID);

			//SkEntity Ca_entity = PETools.PEUtil.GetCaster(caster);
			PeEntity peEntity = caster.GetComponent<PeEntity>();
			if(peEntity == Entity)
				return ;
			
			float tansDis = peEntity.IsBoss ? 128f : 64f;
			bool canTrans = false;
			if (GameConfig.IsMultiClient) {
				if (ForceSetting.Instance.GetForceType (playerID) == EPlayerType.Human)
					canTrans = true;
			} else {
				if (ForceSetting.Instance.GetForceID (playerID) == 1)
					canTrans = true;
			}
			
			if (canTrans)
			{
				List<PeEntity> entities = EntityMgr.Instance.GetEntities (Entity.peTrans.position, tansDis, playerID, false, Entity);
				for(int i = 0; i < entities.Count; i++)
				{
					if (entities[i] == null)
						continue;
					
					if (!entities[i].Equals (Entity) && entities[i].target != null) 
					{
						entities[i].target.OnTargetSkill(peEntity.skEntity);
					}
				}
			}
		}

        void OnWeaponAttack(SkEntity caster)
        {
            OnSkillTarget(caster);
        }

       

		public void OnLeaderSleep(int buffid)
		{
			NpcsleepBuffId = buffid;
			sleepStartTime = Time.time;
		}
		
		void NpcSleep()
		{
			if (Entity.aliveEntity != null && Entity.motionMgr != null && !NpcHasSleep && NpcsleepBuffId != 0)
			{
				PEActionParamVQNS param = PEActionParamVQNS.param;
				param.vec = Entity.peTrans.position;
				param.q = Entity.peTrans.rotation;
				param.n = NpcsleepBuffId;
				param.str = "Sleep";
				Entity.motionMgr.DoAction(PEActionType.Sleep, param);
				NpcHasSleep = true;
			}
		}


		public void OnLeaderEndSleep(int buffid)
		{
			if (this != null && !this.Equals(null) && gameObject && Entity.aliveEntity != null && Entity.motionMgr != null)
			{
				Entity.motionMgr.EndAction(PEActionType.Sleep);
				NpcsleepBuffId = 0;
				NpcHasSleep = false;
			}
		}
		
		void OnStartFell(TreeInfo treeInfo)
		{
			if (IsServant)
			{
				//m_PlayerOpTree = treeInfo;
				m_FollowerCut = true;
				// AddTalkInfo(ENpcTalkType.Follower_cut,ENpcSpeakType.TopHead,true);
			}
		}
		
		void OnEndFell()
		{
			//m_FollowerCut = false;
			//m_PlayerOpTree = null;
		}
		
        /**************************************
         * 移除NPC睡觉BUFF（跟随玩家睡觉时添加）
         * 
         * **********       *****************/
		public void RemoveSleepBuff()
		{
			if (Entity.aliveEntity != null && Entity.motionMgr != null)
			{
				Entity.motionMgr.EndAction(PEActionType.Sleep);
			}
		}

        //npc行为关闭时（无request 也非基地npc 或者仆从），将npc传送至FixedPoint
		public void OnBehaveStop(int behaveId)
        {
            if (this == null || this.Equals(null))
                return;

            if (mFixedPointPos == Vector3.zero || Entity == null)
                return;

            //float d = PETools.PEUtil.Magnitude(Entity.position, mFixedPointPos);
            if (!PeGameMgr.IsMulti && Entity.peTrans != null )//&& d > NPCConstNum.Fix_Distance_Max)
                Entity.peTrans.position = mFixedPointPos;

        }

        public void OnFastTravel(Vector3 pos)
        {
            if (this == null || this.Equals(null) || PeGameMgr.IsMulti)
                return;

            if (m_Request != null && m_Request.HasAnyRequest())
                return;

            if (this != null && (this.Type == ENpcType.Follower || this.Type == ENpcType.Base))
                return;

            if(mFixedPointPos == Vector3.zero || Entity == null)
                return ;

           // float d = PETools.PEUtil.Magnitude(Entity.position, mFixedPointPos);
            if (!PeGameMgr.IsMulti  && Entity.peTrans!= null)// && d > NPCConstNum.Fix_Distance_Max)
                Entity.peTrans.position = mFixedPointPos;
        }

		#endregion

		public void OnMsg(EMsg msg, params object[] args)
		{
			switch (msg)
			{
			case EMsg.Trans_Pos_set:
				Vector3 pos = (Vector3)args[0];
                if (Entity.Id == NpcRobotDb.Instance.mFollowID)
                {
                   
                    if(m_RobotEntity == null)
                        PeEntityCreator.InitRobot();

                    if (m_RobotEntity != null)
                        m_RobotEntity.robotCmpt.Translate(pos);
                }
                break;
            case EMsg.View_Model_Build:
				  if (Entity.Id == NpcRobotDb.Instance.mFollowID)
                    {
                   
                        if(m_RobotEntity == null)
                            PeEntityCreator.InitRobot();

                        if (m_RobotEntity != null)
                            m_RobotEntity.robotCmpt.Translate(Entity.position);
                    }
                break;
			default:
				break;
			}
		}
		
		#region Unity Fun
		public override void Awake()
		{
			base.Awake();
			
		}
		
		public override void Start()
		{
			base.Start();
			
			m_UpdateCampsite = true;
			m_Request = GetComponent<RequestCmpt>();
			m_Behave = GetComponent<BehaveCmpt>();
			mNpcPackage = GetComponent<NpcPackageCmpt>();
			if (Entity != null && Entity.aliveEntity != null)
			{
				Entity.aliveEntity.deathEvent += OnDeath;
				Entity.aliveEntity.reviveEvent += OnRevive;
				Entity.aliveEntity.onHpReduce += OnDamage;
				Entity.aliveEntity.attackEvent += OnAttack;
				Entity.aliveEntity.onSkillEvent += OnSkillTarget;

                Entity.aliveEntity.onWeaponAttack += OnWeaponAttack;
                Entity.aliveEntity.OnBeEnemyEnter += OnBeEnemyEnter;
                Entity.aliveEntity.OnBeEnemyExit += OnBeEnemyExit;

				Entity.aliveEntity.OnEnemyAchieve += OnEnemyAchieve;
                Entity.aliveEntity.OnEnemyLost += OnEnemyLost;
			}
            if (m_Behave != null)
                m_Behave.OnBehaveStop += OnBehaveStop;
            Pathea.FastTravelMgr.Instance.OnFastTravel += OnFastTravel;

			AttachEvent();
			
			StartCoroutine(IgnorePlant(10.0f));
			StartCoroutine(CalculateCamp());
			StartCoroutine(CheckMedicine());
			StartCoroutine(ThinkingSth());
			StartCoroutine(NpcTalk(5.0f));
		}
		
		private ulong updateCnt = 0;
		public override void OnUpdate()
		{
			base.OnUpdate();
			updateCnt ++;
			UpdateMount();
			//UpdateBattle();
			if (!battachEvent) AttachEvent();
			UpdateAbility();
			UpdateNpcThink();
            UpdateDisWithPlayer();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            if (Entity.aliveEntity != null)
            {
                Entity.aliveEntity.deathEvent -= OnDeath;
            }

            if (m_RobotEntity != null)
            {
                m_RobotEntity.robotCmpt.OnDestroy();
            }

            if (Pathea.FastTravelMgr.Instance != null)
                Pathea.FastTravelMgr.Instance.OnFastTravel -= OnFastTravel;
        }
		
		public override void Serialize(System.IO.BinaryWriter w)
		{
			base.Serialize(w);
			
			if (PeEntity.CURRENT_VERSION >= PeEntity.VERSION_0001)
			{
				w.Write(Version_Current);
				
				if (Master == null)
					w.Write(0);
				else
					w.Write(Master.Entity.Id);
				
				if (Version_Current >= Version_0001)
				{
					w.Write(m_ReviveTime);
				}
				
				if (Version_Current >= Version_0002)
				{
					
					w.Write((int)MedicalState);

                    //----------------write ablity--------------------------
                    if (m_AbilityIdes != null)
                    {
                        w.Write(m_AbilityIdes.Count);
                        for (int i = 0; i < m_AbilityIdes.Count; i++)
                        {
                            w.Write(m_AbilityIdes[i]);
                        }
                    }
                    else
                    {
                        w.Write(0);
                    }
                    //if (Npcskillcmpt != null && Npcskillcmpt.AblityId != null)
                    //    PETools.Serialize.WriteBytes(Npcskillcmpt.AblityId.Export(), w);
                }
				
				if (Version_Current >= Version_0003)
				{
					w.Write(mAttributeUpTimes);
				}
				
				if (Version_Current >= Version_0004)
				{
					w.Write(FixedPointPos.x);
					w.Write(FixedPointPos.y);
					w.Write(FixedPointPos.z);
				}
				
				//------------保存NPCTYpe数据
				if (Version_Current >= Version_0005)
				{
					w.Write(mNpcControlCmdId);
				}
				//-----------保存仆从复活时间与NPCmotionstyle
				if (Version_Current >= Version_0006)
				{
					w.Write(m_FollowerCurReviveTime);
					w.Write((int)m_MotionStyle);
				}
				//-------------保存m_MountID--------
				if (Version_Current >= Version_0007)
				{
					w.Write(m_MountID);
				}
				
				if (Version_Current >= Version_0008)
				{
					w.Write(m_NpcInAlert);
				}
				
				if (Version_Current >= Version_0009)
				{
					w.Write(m_BaseNpcOutMission);
				}
				if (Version_Current >= Version_0010)
				{
					w.Write(m_FollowerSentry);
				}

                //--------------Battle-----------------------
                if (Version_Current >= Version_0011)
                {
                    w.Write((int)m_Battle);
                }

                //-----------------viocetype-----------------
                if (Version_Current >= Version_0012)
                {
                    w.Write(voiceType);
                }
			}
		}
		
		public override void Deserialize(System.IO.BinaryReader r)
		{
			base.Deserialize(r);
			
			if (PeEntity.CURRENT_VERSION >= PeEntity.VERSION_0001)
			{
				int version = r.ReadInt32();
				
				int masterID = r.ReadInt32();
				PeEntity entity = EntityMgr.Instance.Get(masterID);
				if (entity != null)
				{
					ServantLeaderCmpt leader = entity.GetComponent<ServantLeaderCmpt>();
					if (leader != null)
					{
						leader.AddServant(this);
					}
				}
				
				if (version >= Version_0001)
				{
					m_ReviveTime = r.ReadInt32();
				}
				
				if (version >= Version_0002)
				{
					//MedicalState 
					MedicalState = (Pathea.ENpcMedicalState)r.ReadInt32();

                    //----------read ablity---------------------------
                    //byte[] buffablities = PETools.Serialize.ReadBytes(r);
                    //m_AbilityIdes.Import(buffablities);
                    int length = r.ReadInt32();
                    if (length > 0)
                    {
                        for (int i = 0; i < length; i++)
                        {
                            m_AbilityIdes.Add(r.ReadInt32());
                        }
                    }
                }

                if (version >= Version_0003)
				{
					mAttributeUpTimes = r.ReadInt32();
				}
				
				if (version >= Version_0004)
				{
					FixedPointPos = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
				}
				
				if (version >= Version_0005)
				{
					NpcControlCmdId = r.ReadInt32();
				}
				
				if (version >= Version_0006)
				{
					FollowerCurReviveTime = r.ReadSingle();
					MotionStyle = (ENpcMotionStyle)r.ReadInt32();
				}
				if (version >= Version_0007)
				{
					MountID = r.ReadInt32();
				}
				
				if (version >= Version_0008)
				{
					m_NpcInAlert = r.ReadBoolean();
				}
				
				if (version >= Version_0009)
				{
                    m_BaseNpcOutMission = r.ReadBoolean();
					if(m_BaseNpcOutMission)
						this.Invoke("MissionReady",5.0f);
				}
				if (version >= Version_0010)
				{
					m_FollowerSentry = r.ReadBoolean();
				}

                //--------------Battle-----------------------
                if (version >= Version_0011)
                {
                    m_Battle = (ENpcBattle)r.ReadInt32();
                }

                //-----------------viocetype-----------------
                if (version >= Version_0012)
                {
                    voiceType = r.ReadInt32();
                }
			}
		}
		#endregion
	}
	
}
