using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CSRecord;
using ItemAsset;

using Pathea;
using Pathea.PeEntityExt;
using Pathea.Operate;

public partial class CSPersonnel : PersonnelBase 
{
    private CSCreator mCreator;
    public CSCreator m_Creator
    {
        get
        {
            return mCreator;
        }
        set
        {
            mCreator = value;
            if (m_NpcCmpt != null)
            {
                m_NpcCmpt.Creater = value;
            }
        }
    }
    public CSMgCreator mgCreator
    {
        get
        {
           return m_Creator as CSMgCreator;
        }
    }

	public PeEntity	m_Npc;
    private SkAliveEntity m_SkAlive;
    private EntityInfoCmpt m_NpcInfo;
    //private UseItemCmpt m_UseItem;
    private PeTrans m_Trans;
    private CommonCmpt m_NpcCommonInfo;
    private NpcCmpt m_NpcCmpt;


    //private RequestCmpt m_Request;

    public Request currentRequest;
    public Request lastRequest;
    #region interface
    public PeEntity NPC
    {
        get { return m_Npc; }
        set
        {
            m_Npc = value;
            if (m_Npc == null)
            {
                m_SkAlive = null;
                m_NpcInfo = null;
                //m_UseItem = null;
                m_NpcCommonInfo = null;
                //m_Request = null;
                m_NpcCmpt = null;
                m_Trans = null;
            }
            else
            {
                m_SkAlive = m_Npc.GetCmpt<SkAliveEntity>();
                m_NpcInfo = m_Npc.GetCmpt<EntityInfoCmpt>();
                //m_UseItem = m_Npc.GetCmpt<UseItemCmpt>(); 
                m_NpcCommonInfo = m_Npc.GetCmpt<CommonCmpt>();
                //m_Request = m_Npc.GetCmpt<RequestCmpt>();
                m_NpcCmpt = m_Npc.GetCmpt<NpcCmpt>();
                m_Trans = m_Npc.peTrans;
            }
        }
    }
	public SkAliveEntity SkAlive {get {return m_SkAlive;}}
	public List<NpcAbility> Npcabliys {get { return m_NpcCmpt.Npcskillcmpt.CurNpcAblitys;}}
    public float GetEnhanceSkill { get { return m_NpcCmpt.Npcskillcmpt.GetTalentPercent(AblityType.Reinforce); } }
    public float GetRecycleSkill { get { return m_NpcCmpt.Npcskillcmpt.GetTalentPercent(AblityType.Disassembly); } }
    public float GetRepairSkill { get { return m_NpcCmpt.Npcskillcmpt.GetTalentPercent(AblityType.Repair); } }
    public float GetCompoundSkill { get { return m_NpcCmpt.Npcskillcmpt.GetTalentPercent(AblityType.Arts); } }
    public float GetDiagnoseTimeSkill { get { return m_NpcCmpt.Npcskillcmpt.GetTalentPercent(AblityType.Diagnose); } }
    public float GetDiagnoseChanceSkill { get { return m_NpcCmpt.Npcskillcmpt.GetCorrectRate(AblityType.Diagnose); } }
    public float GetTreatTimeSkill { get { return m_NpcCmpt.Npcskillcmpt.GetTalentPercent(AblityType.Medical); } }
    public float GetTreatChanceSkill { get { return m_NpcCmpt.Npcskillcmpt.GetCorrectRate(AblityType.Medical); } }
    public float GetTentTimeSkill { get { return m_NpcCmpt.Npcskillcmpt.GetTalentPercent(AblityType.Nurse); } }
    public float GetProcessingTimeSkill { get { return m_NpcCmpt.Npcskillcmpt.GetTalentPercent(AblityType.Explore); } }
	public float GetFarmingSkill { get{return m_NpcCmpt.Npcskillcmpt.GetTalentPercent(AblityType.Farming);} }

	public Ablities GetNpcAllSkill { get { return m_NpcCmpt.AbilityIDs; } set { m_NpcCmpt.SetAbilityIDs(value); } }

	#region npc interface
	public void UpgradeAttribute(Pathea.AttribType type, float plusValue){
		m_NpcCmpt.AttributeUpgrade(type,plusValue);
	}
	public bool CanUpgradeAttribute(){
		return m_NpcCmpt.CanAttributeUp();
	}
	public int UpgradeTimes{
		get{return m_NpcCmpt.mAttributeUpTimes;}
		set{m_NpcCmpt.mAttributeUpTimes= value;}
	}
    public int State { get { return (int)m_NpcCmpt.State; } }
    public Texture RandomNpcFace { get { return m_NpcInfo==null? null : m_NpcInfo.faceTex; } }
    public string MainNpcFace { get { return m_NpcInfo == null ? null : m_NpcInfo.faceIcon; } }
	public string GivenName { get { return m_NpcInfo == null ? "":m_NpcInfo.characterName.givenName; } }
	public string FullName { get { return m_NpcInfo == null ? "":m_NpcInfo.characterName.fullName; } }
	public string FamilyName { get { return m_NpcInfo == null ? "":m_NpcInfo.characterName.familyName; } }

	public PeSex Sex { get { return m_NpcCommonInfo == null ? PeSex.Male:m_NpcCommonInfo.sex; } }

	public bool IsRandomNpc { get { return m_Npc == null ? false:m_Npc.IsRandomNpc();} }

	public bool IsFollower { get { return m_Npc == null ? false:m_Npc.IsFollower(); } }

	public Transform transform { get { return m_Trans == null ? null:m_Trans.transform; } }

	public bool CanTrain{
		get{return m_NpcCmpt == null ? false : !m_NpcCmpt.NpcUnableWork;}
	}

	//--to do
	public bool CanChangeOccupation{
		get{ return NpcTypeDb.CanRun(m_NpcCmpt.NpcControlCmdId,ENpcControlType.ChangeRole) && !m_NpcCmpt.BaseNpcOutMission;}
	}
	public bool CanProcess{
		get{ return m_NpcCmpt==null?false:!m_NpcCmpt.NpcUnableProcess;}
	}

	public bool ShouldStopProcessing{
		get{ return m_NpcCmpt==null?true:m_NpcCmpt.NpcShouldStopProcessing;}
	}

	public ENpcUnableWorkType CannotWorkReason{
		get{return m_NpcCmpt==null?ENpcUnableWorkType.None:m_NpcCmpt.unableWorkReason;}
	}
	#endregion

	private CSDwellings m_Dwellings;
	public CSDwellings	Dwellings
	{
		get
		{
			return m_Dwellings;
		}
		set 
		{
			if (m_Dwellings != value)
			{
				if (value != null)
				{
					value.AddEventListener(OnDwellingsChangeState);
					m_Running = value.IsRunning;
				}

				if (m_Dwellings != null)
					m_Dwellings.RemoveEventListener(OnDwellingsChangeState);
			}
			m_Dwellings = value;
		}
	}

	private bool m_Running = false;
	public bool Running		{ get { return m_Running;} }

	// Data
	protected CSPersonnelData m_Data;
	public CSPersonnelData Data			{ get {return m_Data;} }

	public int Occupation  	{ get { return m_Occupation; } }
	public int m_Occupation
	{
		get { return Data.m_Occupation; }
		
		set
		{
			_updateOccupation(Data.m_Occupation, value);
            
		}
	}

    public bool TrySetOccupation(int occup)
    {
        if (Occupation == occup)
        {
            return true;
        }
		if(occup!=CSConst.potDweller&&!CanChangeOccupation)
			return false;

        if (PeGameMgr.IsMulti)
        {

            ((AiAdNpcNetwork)NetworkInterface.Get(ID)).RPCServer(EPacketType.PT_CL_CLN_SetOccupation, occup);
        }
        else
        {
            if (m_Occupation == CSConst.potProcessor&&occup != CSConst.potProcessor)
                TrySetProcessingIndex(-1);
            m_Occupation = occup;
        }
		return true;
    }

    private CSCommon m_WorkRoom;
    public CSCommon WorkRoom
    {
        get { return m_WorkRoom; }
        set
        {
            SetWorkRoom(value);
        }
    }
    public void TrySetWorkRoom(CSCommon w)
    {
        if (WorkRoom == w)
        {
            return;
        }
        if (PeGameMgr.IsMulti)
        {
            ((AiAdNpcNetwork)NetworkInterface.Get(ID)).RPCServer(EPacketType.PT_CL_CLN_SetWorkRoomID, w.ID);
        }
        else
        {
            WorkRoom = w;
        }
    }
   
    public PEMachine WorkMachine
    {
        get;
        set;
    }

    public PEDoctor HospitalMachine
    {
        get;
        set;
    }

	public PETrainner TrainerMachine{
		get;
		set;
	}

    public int m_WorkMode
    {
        get { return Data.m_WorkMode; }
        set
        {
            _updateWorkType(m_Data.m_WorkMode, value);
        }
    }

    public void TrySetWorkMode(int wm)
    {
        if (m_WorkMode == wm)
        {
            return;
        }
        if (PeGameMgr.IsMulti)
        {
            ((AiAdNpcNetwork)NetworkInterface.Get(ID)).RPCServer(EPacketType.PT_CL_CLN_SetWorkMode, wm);
        }
        else
        {
            m_WorkMode = wm;
        }
    }

    public float GetAttribute(AttribType type)
    {
       return m_SkAlive.GetAttribute(type);
    }

    public void SetAttribute(AttribType type,float value){
        m_SkAlive.SetAttribute(type,value);
    }

    #endregion

    public void CreateData (CSPersonnelData data)
	{
		bool isNew =  m_Creator.m_DataInst.AddData(data) ;
		m_Data = data;
		
		if (isNew&&(!PeGameMgr.IsMulti))
		{
			
		}
		else
		{
			Dwellings = m_Creator.GetCommonEntity(m_Data.m_DwellingsID) as CSDwellings;
			if(Dwellings!=null)
			{
				Dwellings.AddNpcs(this);
			}
			CSCommon workRoom = m_Creator.GetCommonEntity(m_Data.m_WorkRoomID);
            //if (workRoom != null && workRoom.AddWorker(this))
                //m_BrainMemory.Add(workRoom);
			if(workRoom!=null)
			{
				WorkRoom = workRoom;
			}
			//_createGuardTriggerGo ();
			//m_GuardTrigger.Pos = Data.m_GuardPos;


            if (processingIndex >= 0)
            {
                if (m_ProcessingIndexInitListenner != null)
                    m_ProcessingIndexInitListenner(this);
            }
            //if (m_Dwellings != null && m_Dwellings.Assembly != null)
            //{
            //    if ( !m_Dwellings.Assembly.InRange(m_Pos) )
            //        _state = CSConst.pstUnknown;
            //}
			
		}

        //--to do: wait
        //m_Skill.RegisterListener(OnNPCEventTrigger);
		
	}

	// Stamina
	private float m_Satmina = 500;
	private float m_SatminaDecimal = 0.0f;
	public float Stamina 	
	{
        get { return m_SkAlive == null ? m_Satmina : m_SkAlive.GetAttribute(AttribType.Comfort) + m_SatminaDecimal; }  
		set 
		{ 
			m_Satmina = value;

            m_SkAlive.SetAttribute(AttribType.Comfort, Mathf.FloorToInt(value));
			m_SatminaDecimal = Mathf.Max(0, CSUtils.SplitDecimals(value));

		} 
	}

    public float MaxStamina { get { return m_SkAlive == null ? m_Satmina : m_SkAlive.GetAttribute(AttribType.ComfortMax); } }
    public float HalfStamina { get { return m_SkAlive == null ? 250f : m_SkAlive.GetAttribute(AttribType.ComfortMax); } }


	#region STATE & OCCUPATION & WORK_TYPE UPDATE

    void _updateState(int old_state, int new_state)
    {
        //--to do: wait
        /*
        if (new_state == CSConst.pstIdle)
            m_Npc.AttackMode = EAttackMode.Defence;
        else if (new_state == CSConst.pstWork)
            m_Npc.AttackMode = EAttackMode.Defence;
        else if (new_state == CSConst.pstRest)
            m_Npc.AttackMode = EAttackMode.Defence;
        else if (new_state == CSConst.pstPrepare)
            m_Npc.AttackMode = EAttackMode.Defence;
        else if (new_state == CSConst.pstPatrol)
            m_Npc.AttackMode = EAttackMode.Attack;
        else if (new_state == CSConst.pstGuard)
            m_Npc.AttackMode = EAttackMode.Attack;

        if (old_state != new_state)
        {
            _state = new_state; 
            if (m_StateChangedListener != null)
                m_StateChangedListener(this, old_state);
        }
        else
            _state = new_state; 
         * */
    }

	void _updateOccupation (int old_occupa, int new_occupa)
	{
		if (old_occupa != new_occupa)
		{
			ClearFarmWorks();
//            if (old_occupa == CSConst.potProcessor&&isProcessing)
//            {
//                Debug.Log("is Processing, can't change occupation");
//                if (m_OccupaChangedListener != null)
//                    m_OccupaChangedListener(this, old_occupa);
//                return;
//            }


			switch (new_occupa)
			{
			case CSConst.potDweller:
			{
                if (m_Npc != null && m_Npc.IsFollower())
                    m_Npc.SetFollower(false);

                WorkRoom = null;
				m_WorkMode = CSConst.pwtNoWork;
			} break;
			case CSConst.potWorker:
			{
                if (m_Npc != null && m_Npc.IsFollower())
                    m_Npc.SetFollower(false);

                WorkRoom = null;
				m_WorkMode = CSConst.pwtNormalWork;

			}break;
			case CSConst.potFarmer:
			{
				// Follower? Set to false
                if (m_Npc != null && m_Npc.IsFollower())
                    m_Npc.SetFollower(false);

				if (mgCreator != null && mgCreator.Assembly != null)
				{
                    WorkRoom = mgCreator.Assembly.Farm;
                }
                else
                {
                    WorkRoom = null;
                }
				
				m_WorkMode = CSConst.pwtFarmForMag;

			}break;
			case CSConst.potSoldier:
			{
				// Follower? Set to false
                if (m_Npc != null && m_Npc.IsFollower())
                    m_Npc.SetFollower(false);

                WorkRoom = null;
				m_WorkMode = CSConst.pwtPatrol;

			}break;
			case CSConst.potFollower:
			{
				WorkRoom = null;
				m_WorkMode = CSConst.pwtNoWork;
			}break;
            case CSConst.potProcessor:
                {
                    isProcessing = false;
                    processingIndex = -1;

                    if (mgCreator != null && mgCreator.Assembly != null)
                    {
                        WorkRoom = CSMain.s_MgCreator.Assembly.ProcessingFacility;
                    }
                    else
                    {
                        WorkRoom = null;
                    }

                    //--to do: if add linechangeListener
                    m_WorkMode = CSConst.pwtNoWork;
                }break;
            case CSConst.potDoctor:
                {
                    if (m_Npc != null && m_Npc.IsFollower())
                        m_Npc.SetFollower(false);

                    WorkRoom = null;
                    //--to do: if add linechangeListener
                    m_WorkMode = CSConst.pwtNoWork;
                } break;
            case CSConst.potTrainer:
                {
                    if (m_Npc != null && m_Npc.IsFollower())
                        m_Npc.SetFollower(false);

                    if (mgCreator != null && mgCreator.Assembly != null)
                    {
                        WorkRoom = CSMain.s_MgCreator.Assembly.TrainingCenter;
                    }
                    else
                    {
                        WorkRoom = null;
                    }
                    //--to do: if add linechangeListener
                    m_WorkMode = CSConst.pwtNoWork;

                } break;
            default:
                break;
			}
            
			m_Data.m_Occupation = new_occupa;
			if (m_OccupaChangedListener != null)
				m_OccupaChangedListener(this, old_occupa);

            //NpcCmpt
            UpdateNpcCmptOccupation();
		}
	}

	/// <summary>
	/// <PWT> updates the mode of the work. if the work mode changed
	/// </summary>
	void _updateWorkType(int old_type, int new_type)
	{
		if (old_type != new_type)
		{

			// Remove Soldier from all Entities if the old work mode is Patrol
			if (old_type == CSConst.pwtPatrol || old_type == CSConst.pwtGuard)
			{
//				CSMgCreator mgCreator = m_Creator as CSMgCreator;
//				if (mgCreator != null)
//				{
//					mgCreator.Assembly.RemoveSoldier(this);
//					Dictionary<int, CSCommon> commons = mgCreator.GetCommonEntities();
//
//					foreach (KeyValuePair<int, CSCommon> kvp in commons)
//						kvp.Value.RemoveSoldier(this);
//				}

			}

			// Add Soldier to all Entities and set the guard pos
			// if the new work mode is Guard.
			if (new_type == CSConst.pwtGuard )
			{
			}

			// Add Soldier to all Entities if the new work mode is Patrol
			if (new_type == CSConst.pwtPatrol)
			{

				// Add Soldier to all Entities if the new work mode is Patrol
//				if (Dwellings.Assembly != null)
//				{
//					Dwellings.Assembly.AddSoldier(this);
//
//					List<CSCommon>  commons = Dwellings.Assembly .GetBelongCommons();
//				
//					foreach (CSCommon common in commons)  
//						common.AddSoldier(this); 
//				}
			}


			ClearFarmWorks();
			m_Data.m_WorkMode = new_type; 
			if (m_WorkTypeChangedListener != null)
				m_WorkTypeChangedListener(this, old_type);

            GuardEntities = GetProtectedEntities();
            //NpcCmpt
            UpdateNpcCmptWorkMode();
		}
	}

	#endregion

	#region OVERRIDE_MEM
	// Rest
    private Pathea.Operate.PEBed bed;

    public Pathea.Operate.PEBed Bed
    {
        get { return bed; }
        set
        {
            bed = value;
            if (m_NpcCmpt != null)
                m_NpcCmpt.Sleep = bed;
        }
    }

    //public override bool ResetWorkSpace {
    //    get {
    //        return base.ResetWorkSpace;
    //    }
    //    set {

    //        if (value)
    //        {
    //            // Notice the behave to reset
    //            List<CSNPCBehave> behaves = CSBehaveMgr.GeBehaves(ID);
    //            foreach (CSNPCBehave behave in behaves)
    //            {
    //                if (behave != null && behave as CSSWorkBehave != null)
    //                {
    //                    CSSWorkBehave workBeahave = behave as CSSWorkBehave;
    //                    workBeahave.m_bResetPos = true;
    //                }
    //            }
    //        }
    //    }
    //}

	// Go
	public override GameObject m_Go {
		get {
			return m_Npc.GetGameObject();
		}
	}

	public override Vector3 m_Pos {
		get {
			if (m_Npc != null)
				return m_Npc.position;
			else
				return Vector3.zero;
		}
		set {
			if (m_Npc != null)
				m_Npc.MoveToPosition(value);
		}
	}

	public override Quaternion m_Rot {
		get {
			if (m_Npc != null)
				return m_Npc.rotation;
			else
				return Quaternion.identity;
		}
		set {
            //--to do: wait
            //if (m_Npc != null)
            //    m_Npc.rotation = value;
		}
	}

	public override float WalkSpeed {
		get {
            //--to do: wait
			//return m_Npc.walkSpeed;
            return 0;
		}
	}

	public override float RunSpeed {
		get {
            //--to do: wait
            //return m_Npc.runSpeed;
            return 0;
		}
	}

	public override string m_Name {
		get {
			return FullName;
		}
	}

	public override bool EqupsRangeWeapon {
		get 
		{
            return false;
            //--to do: wait
            //EquipedNpc npc = m_EquipedNpc;
            //if (npc == null)
            //    return base.EqupsRangeWeapon;
            //else
            //    return npc.IsRangeWeaponEquiped();
		}
	}

	public override bool EqupsMeleeWeapon {
		get {
			return !EqupsRangeWeapon;
		}
	}

	#endregion

	#region Event_About

	public delegate void StateChangedDel (CSPersonnel person, int prvState);
	static event StateChangedDel m_StateChangedListener;

	public static void RegisterStateChangedListener (StateChangedDel listener)
	{
		m_StateChangedListener += listener;
	}

	public static void UnRegisterStateChangedListener (StateChangedDel listener)
	{
		m_StateChangedListener -= listener;
	}

	static event StateChangedDel m_OccupaChangedListener;

	public static void RegisterOccupaChangedListener (StateChangedDel listener)
	{
		m_OccupaChangedListener += listener;
	}

	public static void UnregisterOccupaChangedListener (StateChangedDel listener)
	{
		m_OccupaChangedListener -= listener;
	}

	static event StateChangedDel m_WorkTypeChangedListener;

	public static void RegisterWorkTypeChangedListener(StateChangedDel listener)
	{
		m_WorkTypeChangedListener += listener;
	}

	public static void UnregisterWorkTypeChangedListener(StateChangedDel listener)
	{
		m_WorkTypeChangedListener -= listener;
	}

	#endregion


	public override PersonnelSpace[] GetWorkSpaces ()
	{
		if (WorkRoom == null)
			return null;

		return WorkRoom.WorkSpaces;  
	}

	#region SCHEDULE_ABOUT
	
//	private int m_RestStartTime = 22;
//	private int m_RestDuration = 10;

//	enum EScheduleType
//	{
//		Idle,
//		Rest,
//		Work,
//		Dine,
//		Relax
//	}
//	private Dictionary<int, EScheduleType> m_ScheduleMap;
	
	#endregion


	#region CALL_BACK

	private CSAssembly m_Assembly;

	 // Dwellings change state call back
	void OnDwellingsChangeState(int event_type, CSEntity entiy, object arg)
	{
		if (event_type == CSConst.eetDwellings_ChangeState)
		{
			bool state = (bool)arg ;
			if (state) // Add
			{
				CSAssembly assem = Dwellings.Assembly;
//				if (m_WorkMode == CSConst.pwtPatrol)
//				{
//					Dwellings.Assembly.AddSoldier(this);
//					List<CSCommon>  commons = Dwellings.Assembly.GetBelongCommons();
//
//					foreach (CSCommon common in commons)  
//						common.AddSoldier(this); 
//				}
//				else if (m_WorkMode == CSConst.pwtGuard)
//				{
//					if ( CSUtils.SphereContainsAndIntersectBound(m_GuardTrigger.Pos, m_GuardTrigger.Radius, assem.Bound))
//						assem.AddSoldier(this);
//
//					List<CSCommon>  commons = assem.GetBelongCommons();
//					foreach (CSCommon common in commons)  
//					{
//						if (CSUtils.SphereContainsAndIntersectBound(m_GuardTrigger.Pos, m_GuardTrigger.Radius, common.Bound))
//							common.AddSoldier(this);
//					}
//				}

				assem.AddEventListener(OnAssemblyEventHandler);
				m_Assembly = assem;
			}
			else // Remove
			{
//				if (m_WorkMode == CSConst.pwtPatrol || m_WorkMode == CSConst.pwtGuard)
//				{
//					CSMgCreator mgCreator = m_Creator as CSMgCreator;
//					if (mgCreator != null)
//					{
//						mgCreator.Assembly.RemoveSoldier(this);
//						Dictionary<int, CSCommon> commons = mgCreator.GetCommonEntities();
//						
//						foreach (KeyValuePair<int, CSCommon> kvp in commons)
//							kvp.Value.RemoveSoldier(this);
//					}
//				}

				if (m_Assembly != null)
				{
					m_Assembly.RemoveEventListener(OnAssemblyEventHandler);
					m_Assembly = null;
				}
			}

			m_Running = state;
		}


	}

	//
	void OnAssemblyEventHandler(int event_type, CSEntity entity, object arg)
	{
		if (event_type == CSConst.eetAssembly_AddBuilding)
		{
			CSCommon common = arg as CSCommon;
			if (common == null)
			{
				Debug.LogError("The argument is error");
				return;
			}

//			if (m_WorkMode == CSConst.pwtPatrol)
//			{
//				common.AddSoldier(this);
//			}
//			else if (m_WorkMode == CSConst.pwtGuard)
//			{
//				if ( CSUtils.SphereContainsAndIntersectBound(m_GuardTrigger.Pos, m_GuardTrigger.Radius, entity.Bound))
//					common.AddSoldier(this);
//			}

		}
	}

	#endregion


	#region MAIN_FUNC

	public CSPersonnel ()
	{
        //--to do: new
        //// Init Schedule map
        //m_ScheduleMap = new Dictionary<int, EScheduleType>();

        //for (int i = 0; i < 7; i++)
        //    m_ScheduleMap.Add(i, EScheduleType.Rest);

        //for (int i = 7; i < 22; i++)
        //    m_ScheduleMap.Add(i, EScheduleType.Work);

        //for (int i = 22; i < 26; i++)
        //    m_ScheduleMap.Add(i, EScheduleType.Rest);

        //InitMisc();
	}
	
	public void CreateData ()
	{
		CSDefaultData ddata = null;
		bool isNew =  m_Creator.m_DataInst.AssignData(ID, CSConst.dtPersonnel, ref ddata);
		m_Data = ddata as CSPersonnelData;

		if (isNew)
		{

		}
		else
		{
            
			Dwellings = m_Creator.GetCommonEntity(m_Data.m_DwellingsID) as CSDwellings;
			Dwellings.AddNpcs(this);
			CSCommon workRoom = m_Creator.GetCommonEntity(m_Data.m_WorkRoomID);
            //if (workRoom != null && workRoom.AddWorker(this))
            WorkRoom = workRoom;

//			_createGuardTriggerGo ();
//			m_GuardTrigger.Pos = Data.m_GuardPos;

            //init processor
            if(processingIndex>=0)
            {
                if (m_ProcessingIndexChangeListenner != null)
                    m_ProcessingIndexChangeListenner(this, -1, processingIndex);
            }

//			if (m_Dwellings.m_Assembly != null)
//			{
////				_updateWorkType(-1, Data.m_WorkMode);
//
//				// Add Soldier to all Entities and set the guard pos
//				// if the new work mode is Guard.
//				if (m_WorkMode == CSConst.pwtGuard )
//				{
//					if (Data.m_GuardPos == Vector3.zero)
//						SetGuardAttr(m_Pos);
//					else
//						SetGuardAttr(Data.m_GuardPos);
//				}
//				
//				// Add Soldier to all Entities if the new work mode is Patrol
//				if (m_WorkMode == CSConst.pwtPatrol)
//				{
//					m_Dwellings.m_Assembly.AddSoldier(this);
//
////					Dictionary<int, CSCommon> commons = mgCreator.GetCommonEntities();
//					List<CSCommon>  commons = m_Dwellings.m_Assembly .GetBelongCommons();
//						
////					foreach (KeyValuePair<int, CSCommon> kvp in commons) 
////						kvp.Value.AddSoldier(this); 
//					foreach (CSCommon common in commons)  
//						common.AddSoldier(this); 
//				}
//			}



            //if (m_Dwellings != null && m_Dwellings.Assembly != null)
            //{
            //    if ( !m_Dwellings.Assembly.InRange(m_Pos) )
            //        _state = CSConst.pstUnknown;
            //}
			
		}

        //--to do: wait
		//m_Npc.RegisterListener(OnNPCEventTrigger);

	}
	
	public void RemoveData ()
	{
		m_Creator.m_DataInst.RemovePersonnelData(m_Data.ID);
		//--to do: wait
        //m_Npc.UngegisterListener(OnNPCEventTrigger);
        //m_BrainMemory.Clear();

//        if (m_GuardTrigger != null)
//            GameObject.Destroy(m_GuardTrigger.gameObject);
	}
	
	public override void Update()
	{
		base.Update();

		if (m_Npc == null)
			return;

		// 
		if (Dwellings != null)
			m_Data.m_DwellingsID = Dwellings.ID;
		else
			m_Data.m_DwellingsID = -1;

        //--to do: new
        //if (m_BrainMemory.WorkRoom != null)
        //    m_Data.m_WorkRoomID = m_BrainMemory.WorkRoom.ID;
        //else
        //    m_Data.m_WorkRoomID = -1;

        //// Think for every tick
        //ThinkTick();

        //// Think for every 16 frame
        //if (Time.frameCount % 16 == 0)
        //    Think();

        //// Misc Update
        //MiscUpdate();
	}
	
	#endregion

	#region Other_Important_Func


	// To protect his building. Dont use this if you dont konw what it is
	public void ProtectedEntityDamaged(CSEntity entiy, GameObject caster, float damage)
	{
        //--to do: wait
        //m_Npc.aiTarget.AddExtraHatred(caster, Mathf.CeilToInt( damage * 5 * (CSConst.egtTotal - entiy.Grade)));
	}


	public List<CSEntity>  GetProtectedEntities()
	{
		if (m_Creator == null)
			return new List<CSEntity>();

		CSMgCreator mgCreator = m_Creator as CSMgCreator;
		if (mgCreator == null)
		{
			Debug.LogWarning("The Creator is not a Managed creator, it cant produce the protected entities ");
			return new List<CSEntity>();
		}

		List<CSEntity> enties = new List<CSEntity>();

		if ( mgCreator.Assembly != null)
        {
            //lz-2016.07.25 把基地核心和依赖这个核心的所以设备添加到巡逻列表
            enties.Add(mgCreator.Assembly);
            foreach (KeyValuePair<CSConst.ObjectType, List<CSCommon>> kv in mgCreator.Assembly.m_BelongObjectsMap)
            {
                for (int i = 0; i < kv.Value.Count; i++)
                {
                    enties.Add(kv.Value[i]);
                }
            }
        }

		return enties;
	}
	#endregion

    #region Behave interface
    //public void MoveToPos(Vector3 pos,SpeedState speed)
    //{
    //    Request req;
    //    if (m_Request != null)
    //    {
    //        req = m_Request.Register(EReqType.MoveToPoint, pos, 5.0f, true, speed);
    //        if (req != null)
    //        {
    //            currentRequest = req;
    //            Debug.LogError(pos);
    //        }
    //    }
    //}

    public void UpdateNpcCmpt(){
        if (m_NpcCmpt != null)
        {
            m_NpcCmpt.Creater = mCreator;

            switch (Data.m_Occupation)
            {
                case CSConst.potDweller:
                    m_NpcCmpt.Job = ENpcJob.Resident;
                    break;
                case CSConst.potWorker:
                    m_NpcCmpt.Job = ENpcJob.Worker;
                    break;
                case CSConst.potFarmer:
                    m_NpcCmpt.Job = ENpcJob.Farmer;
                    break;
                case CSConst.potSoldier:
                    m_NpcCmpt.Job = ENpcJob.Soldier;
                    break;
                case CSConst.potFollower:
                    m_NpcCmpt.Job = ENpcJob.Follower;
				    break;
			    case CSConst.potProcessor:
				    m_NpcCmpt.Job = ENpcJob.Processor;
                    break;
			    case CSConst.potDoctor:
				   m_NpcCmpt.Job = ENpcJob.Doctor;
                   break;
                case CSConst.potTrainer:
                   m_NpcCmpt.Job = ENpcJob.Trainer;
                   break;
                default:
                    m_NpcCmpt.Job = ENpcJob.None;
                    break;
            }

            switch(Data.m_WorkMode){
                case CSConst.pwtFarmForMag:
                    m_NpcCmpt.AddTitle(ENpcTitle.Manage);
                    break;
                case CSConst.pwtFarmForPlant:
                    m_NpcCmpt.AddTitle(ENpcTitle.Plant);
                    break;
                case CSConst.pwtFarmForHarvest:
                    m_NpcCmpt.AddTitle(ENpcTitle.Harvest);
                    break;
                default:
                    m_NpcCmpt.AddTitle(ENpcTitle.None);
                    break;
            }


            //--to do: update dwell
            m_NpcCmpt.Sleep = Bed;
            //--to do: work room
            m_NpcCmpt.BaseEntities = GuardEntities;
            m_NpcCmpt.WorkEntity = WorkRoom;
            m_NpcCmpt.Work = WorkMachine;
			m_NpcCmpt.Cure = HospitalMachine;
			m_NpcCmpt.Trainner = TrainerMachine;
        }
    }

    public void UpdateNpcCmptWorkMode()
    {
        switch (Data.m_WorkMode)
        {
            case CSConst.pwtFarmForMag:
                m_NpcCmpt.AddTitle(ENpcTitle.Manage);
                break;
            case CSConst.pwtFarmForPlant:
                m_NpcCmpt.AddTitle(ENpcTitle.Plant);
                break;
            case CSConst.pwtFarmForHarvest:
                m_NpcCmpt.AddTitle(ENpcTitle.Harvest);
                break;
            case CSConst.pwtPatrol:
                m_NpcCmpt.Soldier = ENpcSoldier.Patrol;
                break;
            case CSConst.pwtGuard:
                m_NpcCmpt.Soldier = ENpcSoldier.Guard;
                break;
            default:
                m_NpcCmpt.AddTitle(ENpcTitle.None);
                break;
        }

        m_NpcCmpt.BaseEntities = GuardEntities;
        m_NpcCmpt.WorkEntity = WorkRoom;
        m_NpcCmpt.Work = WorkMachine;
		m_NpcCmpt.Cure = HospitalMachine;
		m_NpcCmpt.Trainner = TrainerMachine;
    }

    public void UpdateNpcCmptOccupation()
    {
        switch (Data.m_Occupation)
        {
            case CSConst.potDweller:
                m_NpcCmpt.Job = ENpcJob.Resident;
                break;
            case CSConst.potWorker:
                m_NpcCmpt.Job = ENpcJob.Worker;
                break;
            case CSConst.potFarmer:
                m_NpcCmpt.Job = ENpcJob.Farmer;
                break;
            case CSConst.potSoldier:
                m_NpcCmpt.Job = ENpcJob.Soldier;
                break;
            case CSConst.potFollower:
                m_NpcCmpt.Job = ENpcJob.Follower;
                break;
            case CSConst.potProcessor:
                m_NpcCmpt.Job = ENpcJob.Processor;
                break;
		    case CSConst.potDoctor:
			    m_NpcCmpt.Job = ENpcJob.Doctor;
                break;
            case CSConst.potTrainer:
                m_NpcCmpt.Job = ENpcJob.Trainer;
                break;
            default:
                m_NpcCmpt.Job = ENpcJob.None;
                break;
        }
        m_NpcCmpt.BaseEntities = GuardEntities;
        m_NpcCmpt.WorkEntity = WorkRoom;
        m_NpcCmpt.Work = WorkMachine;
        m_NpcCmpt.Cure = HospitalMachine;
		m_NpcCmpt.Trainner = TrainerMachine;
    }

//    public void UpdateNpcCmptGuard()
//    {
//        m_NpcCmpt.BaseEntities = GuardEntities;
//        m_NpcCmpt.GuardPosition = Data.m_GuardPos;
//        m_NpcCmpt.GuardRadius = m_GuardTrigger.Radius;
//    }

    public void UpdateNpcCmptGuardEntities()
    {
        m_NpcCmpt.BaseEntities = GuardEntities;
    }

	
	public void UpdateNpcCmptTraining()
	{
		if (m_NpcCmpt != null)
		{
			m_NpcCmpt.IsTrainning = IsTraining;
			m_NpcCmpt.TrainerType = trainerType;
			m_NpcCmpt.TrainningType = trainingType;
		}
	}

    #endregion
}
