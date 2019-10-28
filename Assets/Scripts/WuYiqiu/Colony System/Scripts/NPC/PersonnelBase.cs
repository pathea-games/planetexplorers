using UnityEngine;
using System.Collections;
using Pathea;
using Pathea.Operate;

public class  PersonnelSpace
{
	public PersonnelBase m_Person;
    //private Vector3 m_Pos;
    public object m_Room;
    PEMachine m_workmachine;
    public PEMachine WorkMachine {
        set { m_workmachine = value;
            if(m_Person!=null)
                m_Person.UpdateWorkMachine(m_workmachine);
        }
        get{return m_workmachine;}

    }
    PEDoctor m_hospital;
    public PEDoctor HospitalMachine{
        set
        {
            m_hospital = value;
            if (m_Person != null)
                m_Person.UpdateHospitalMachine(m_hospital);
        }
        get { return m_hospital; }
    }
	PETrainner m_trainer;
	public PETrainner TrainerMachine{
		set
		{
			m_trainer = value;
			if (m_Person != null)
				m_Person.UpdateTrainerMachine(m_trainer);
		}
		get { return m_trainer; }
	}
    //public Vector3  Pos
    //{
    //    get { return m_Pos; }
    //    set 
    //    {
    //        m_Pos = value;
    //        FinalPos = value;
    //    }
    //}

    //private Vector3  m_FinalPos;  

    //public Vector3 FinalPos
    //{
    //    get { return m_FinalPos; }
    //    set 
    //    {
    //        if (!Vector3.Equals(m_FinalPos, value))
    //        {
    //            m_FinalPos = value;
    //            if (OnPosChanged != null)
    //                OnPosChanged(this);
    //        }
			
    //    }
    //}

    //public Quaternion 	 m_Rot;
    //public bool			 m_CanUse;

    //public delegate void EventDel (PersonnelSpace space);
    //public event EventDel OnPosChanged;

    public PersonnelSpace(object room)
    {
        //m_Pos = Vector3.zero;
        //m_Rot = Quaternion.identity;
        m_Room = room;
    }
}

public abstract class PersonnelBase  
{
	public int ID;

	// Pos 
	public virtual Vector3 m_Pos
	{
		get { return Vector3.zero; }
		set {}
	}

	// Rot
	public virtual Quaternion m_Rot
	{
		get { return Quaternion.identity;}
		set {}
	}
	private CounterScript m_CounterScript;

	public virtual string m_Name
	{
		get { return "Personnel"; }
	}
	
	
	
	public virtual bool ResetWorkSpace
	{
		get{
			return false;
		}

		set{}
	}

	// Gameobject
	public virtual GameObject m_Go
	{
		get{
			return null;
		}
	}

	// Speed
	public virtual float RunSpeed
	{
		get{
			return 0.0f;
		}
	}

	public virtual float WalkSpeed
	{
		get{
			return 0.0f;
		}
	}

	public virtual bool EqupsMeleeWeapon
	{
		get {
			return true;
		}
	}

	public virtual bool EqupsRangeWeapon
	{
		get {
			return false;
		}
	}

	// Animation
	public enum EAnimateType
	{
		WorkOnEnhanceMachine,
		WorkOnRepairMachine,
		WorkOnRecycleMachine,
		WorkOnFactory,
		WorkOnStorage,
		SitDown,
		WorkOnWatering,
		WorkOnWeedding
	}

	protected bool m_Active = true;

	// Delegate
	public delegate void AiNpcDelegate(AiNpcDelegate npc);

	// 
	public static Vector3[] s_BoundPos = new Vector3[]
	{
		new Vector3(0, 0, 0),
		new Vector3(0, 1, 0)
	};

	public PersonnelBase()
	{
//		m_BoundPos = new Vector3[2];
//		m_BoundPos[0] = new Vector3(0, 0, 0);
//		m_BoundPos[1] = new Vector3(0, 1, 0);
	}

	public delegate void ArriveDistination(PersonnelBase npc);

	#region BEHAVIOR_ABOUT

    //public void CmdToWork (Vector3 workPos,  CSWorkBParam.EWorkType workType)
    //{
    //    /*****************************************************
    //     * 
    //     * Now is not used this function 
    //     * 
    //     *****************************************************/


    //    // Clear behave 
    //    ClearAllBehave ();

    //    // Add Behave
    //    float sqrDis = (m_Pos - workPos).sqrMagnitude;
    //    if (sqrDis > 4.0f)
    //    {
    //        CSSimpleMoveBehave moveBehave = new CSSimpleMoveBehave();
    //        CSSimpleMoveBParam moveParam = new CSSimpleMoveBParam();
    //        moveParam.m_DesPos = workPos;
    //        moveBehave.Init(CSNPCBehave.EPriority.p_5, moveParam, this);
    //        CSBehaveMgr.AddBehave(ID, moveBehave);
    //    }

    //    CSSWorkBehave workBehave = new CSSWorkBehave();

    //    CSWorkBParam  workParam = new CSWorkBParam();
    //    workParam.m_WorkSpace = GetWorkSpaces();

    //    workParam.m_WorkMode = workType; 
    //    workParam.m_DeplayTime = 0;
    //    workParam.onBehaveFinished = OnWorkToDest;
    //    workParam.onMeetBlock = OnWorkMeetBlock;
    //    workParam.m_DesPos = workPos;

    //    workBehave.Init(CSNPCBehave.EPriority.p_4, workParam, this);
    //    workBehave.m_bResetPos = m_Active;
    //    CSBehaveMgr.AddBehave(ID, workBehave);
    //}

    //public void CmdToWorkNew (PersonnelSpace ps, CSWorkBParam.EWorkType workType)
    //{
    //    // Clear behave 
    //    ClearAllBehave ();

    //    // Add Behave
    //    float sqrDis = (m_Pos - ps.Pos).sqrMagnitude;
    //    if (sqrDis > 4.0f)
    //    {
    //        //--to do: new
    //        //CSSimpleMoveBehave moveBehave = new CSSimpleMoveBehave();
    //        //CSSimpleMoveBParam moveParam = new CSSimpleMoveBParam();
    //        //moveParam.m_DesPos = ps.Pos;
    //        //moveBehave.Init(CSNPCBehave.EPriority.p_5, moveParam, this);
    //        //CSBehaveMgr.AddBehave(ID, moveBehave);
    //        ps.m_Person.MoveTo(ps.Pos, SpeedState.Walk);
    //    }

    //    CSWorkBehave workBehave = new CSWorkBehave();
		
    //    CSWorkBParam  workParam = new CSWorkBParam();
    //    workParam.m_WorkSpace = GetWorkSpaces();
		
    //    workParam.m_WorkMode = workType; 
    //    workParam.m_DeplayTime = 0;
    //    workParam.onBehaveFinished = OnWorkToDest;
    //    workParam.onMeetBlock = OnWorkMeetBlock;
    //    workParam.m_CurSpace = ps;
		
    //    workBehave.Init(CSNPCBehave.EPriority.p_4, workParam, this);
    //    CSBehaveMgr.AddBehave(ID, workBehave);
    //}
	
//    public void CmdToWorkWithoutObstacle(Vector3 workPos, Quaternion rot, CSWorkBParam.EWorkType workType, float delayTime)
//    {
//        // Clear behave 
//        ClearAllBehave ();

//        float sqrDis = (m_Pos - workPos).sqrMagnitude;
//        if (sqrDis > 4.0f)
//        {
//            CSSimpleMoveBehave moveBehave = new CSSimpleMoveBehave();
//            CSSimpleMoveBParam moveParam = new CSSimpleMoveBParam();
//            moveParam.m_DesPos = workPos;
//            moveParam.m_ForceFixPos = false;
//            moveBehave.Init(CSNPCBehave.EPriority.p_5, moveParam, this);
//            CSBehaveMgr.AddBehave(ID, moveBehave);
//        }

//        CSWorkBehaveNoObstacle workBehave = new CSWorkBehaveNoObstacle();
		
//        CSWorkBParam  workParam = new CSWorkBParam();
//        workParam.m_WorkSpace = GetWorkSpaces();
		
//        workParam.m_WorkMode = workType; 
//        workParam.m_DeplayTime = delayTime;
//        workParam.onBehaveFinished = OnWorkToDest;
//        workParam.m_DesPos = workPos;
//        workParam.m_Rot = rot;
		
//        workBehave.Init(CSNPCBehave.EPriority.p_4, workParam, this);
////		workBehave.m_bResetPos = m_Active;
//        CSBehaveMgr.AddBehave(ID, workBehave);
//    }

//    public void CmdToRest (Vector3 restPos)
//    {
//        // Clear behave 
//        ClearAllBehave ();

//        // Add Rest Behave
//        float sqrDis = (m_Pos - restPos).sqrMagnitude;
//        if (sqrDis > 1.0f)
//        {
//            //--to do: new
//            //CSSimpleMoveBehave moveBehave = new CSSimpleMoveBehave();
//            //CSSimpleMoveBParam moveParam = new CSSimpleMoveBParam();
//            //moveParam.m_DesPos = restPos;
//            //moveParam.m_DesRadius = 2.0f;
//            //moveBehave.Init(CSNPCBehave.EPriority.p_5, moveParam, this);
//            //CSBehaveMgr.AddBehave(ID, moveBehave);

//            MoveTo(restPos, SpeedState.Run);
//        }

//        CSSleepBehave  sleepBehave = new CSSleepBehave();

//        CSSleepBParam  sleepParam = new CSSleepBParam();
//        if (m_RestTrans != null)
//        { 
//            sleepParam.m_DesPos = m_RestTrans.position;
//            sleepParam.m_Rot = m_RestTrans.rotation;
//        }
//        else
//        {
//            sleepParam.m_DesPos = restPos;
//        }
//        sleepParam.m_DesRadius = 2.0f;
//        sleepParam.onBehaveFinished = OnRestToDest;
//        sleepParam.onMeetBlock	= OnRestMeetBlock;
//        sleepBehave.Init(CSNPCBehave.EPriority.p_4,  sleepParam, this);
////		sleepBehave.m_bRestPos = m_Active;
//        CSBehaveMgr.AddBehave(ID, sleepBehave);
//    }
	
    //public void CmdToIdle (Vector3 idlePos, float speed = 0.0f, float delay_time = 0.0f)
    //{
    //    // Clear behave 
    //    ClearAllBehave ();
    //    // Add Idle behave
    //    CSSimpleMoveBehave moveBehave = new CSSimpleMoveBehave();

    //    CSSimpleMoveBParam moveParam = new CSSimpleMoveBParam();
    //    moveParam.m_DesPos = idlePos;
    //    moveParam.m_DesRadius = 2.0f;
    //    moveParam.m_DelayTime = delay_time;
    //    moveParam.m_Speed = speed;
    //    moveParam.onBehaveFinished = OnIdleToDest;

    //    moveBehave.Init(CSNPCBehave.EPriority.p_5, moveParam, this);
    //    CSBehaveMgr.AddBehave(ID, moveBehave);


    //}

    //public void CmdToPatrol (Vector3 center, float radius, float speed = 0.0f)
    //{
    //    // Clear behave 
    //    ClearAllBehave ();
    //    CSPatrolBehave patrolBehave = new CSPatrolBehave();
		
    //    CSPatrolParam patrolParam = new CSPatrolParam();
    //    patrolParam.m_Center = center;
    //    patrolParam.m_Radius = radius;
    //    patrolParam.m_MinStaySec = 3;
    //    patrolParam.m_MaxStaySec = 20;

    //    patrolBehave.Init(CSNPCBehave.EPriority.p_5, patrolParam, this);
    //    CSBehaveMgr.AddBehave(ID, patrolBehave);
    //}

//	public void CmdToMotionDetect ()
//	{
//		// Clear behave 
//		ClearAllBehave ();
//		CSMotionDetectBehave mdBehave = new CSMotionDetectBehave();
//
//		CSBehaveParam param = new CSBehaveParam();
//
//		mdBehave.Init(CSNPCBehave.EPriority.p_5, param, this);
//		CSBehaveMgr.AddBehave(ID, mdBehave);
//	}
	

    //public void CmdToPosition (Vector3 destPos, CSBehaveParam.NpcParmDel arriveDest)
    //{
    //    // Clear behave 
    //    ClearAllBehave ();

    //    CSSimpleMoveBehave moveBehave = new CSSimpleMoveBehave();
		
    //    CSSimpleMoveBParam moveParam = new CSSimpleMoveBParam();
    //    moveParam.m_DesPos = destPos;
    //    moveParam.m_DesRadius = 2.0f;
    //    moveParam.m_DelayTime = 0.0f;
    //    moveParam.onBehaveFinished = arriveDest;

    //    moveBehave.Init(CSNPCBehave.EPriority.p_5, moveParam, this);
    //    CSBehaveMgr.AddBehave(ID, moveBehave);
    //}

    //public void ClearAllBehave ()
    //{
    //    CSBehaveMgr.ClearBehaves(ID);
    //}
	
    //public abstract void MoveTo (Vector3 destPos, SpeedState speed);

	public abstract void MoveToImmediately(Vector3 destPos);

	public abstract bool CanBehave();

	public abstract void Sleep(bool v);

	public abstract void Stay ();

	public abstract void PlayAnimation(EAnimateType type, bool v);

	public abstract PersonnelSpace[] GetWorkSpaces ();

//	public abstract bool IsActive();
		
	#endregion

	
	#region CALL_BACK

	protected virtual void OnWorkToDest (PersonnelBase npc)
	{
		Debug.Log("The NPC [" + npc.m_Name + "] is starting to Work.");
	}

	protected virtual void OnWorkMeetBlock (PersonnelBase npc)
	{

	}
	
	protected virtual void OnRestToDest (PersonnelBase npc)
	{
		Debug.Log("The NPC [" + npc.m_Name + "] is starting to Rest.");
	}

	protected virtual void OnRestMeetBlock (PersonnelBase npc)
	{

	}
	
	protected virtual void OnIdleToDest (PersonnelBase npc)
	{
		Debug.Log("The NPC [" + npc.m_Name + "] is starting to Idle.");
	}

    public abstract void UpdateWorkSpace(PersonnelSpace ps);
    public abstract void UpdateWorkMachine(PEMachine pm);
    public abstract void UpdateHospitalMachine(PEDoctor pd);
	public abstract void UpdateTrainerMachine(PETrainner pt);
	#endregion
	
	// Update is called once per frame
	public virtual void Update () 
	{
	
	}
}
