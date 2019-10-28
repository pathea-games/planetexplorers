using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using Pathea;
using Pathea.PeEntityExt;

public partial class CSPersonnel : PersonnelBase 
{ 
	protected int m_RetainState;

	public void ResetCmd ()
	{
        //if (m_State == CSConst.pstIdle)
        //    m_State = CSConst.pstIdle;
        //else if (m_State == CSConst.pstRest)
        //    Rest();
        //else if (m_State == CSConst.pstWork)
        //    WorkNow();
	}

	/// <summary>
	/// Sets the work room. 
	/// </summary>
	/// <param name="workRoom">Work room. : NULL means clear the work room,
	 /// note thant it will remeber by brain.</param>
	private void SetWorkRoom (CSCommon workRoom)
    {
        if (m_WorkRoom != workRoom)
        {
            if (m_WorkRoom != null)
            {
                m_WorkRoom.RemoveWorker(this);
            
                if (workRoom == null)
                {
                    WorkMachine = null;
                }
                else
                {
                    PersonnelSpace ps = workRoom.FindEmptySpace(this);
                    if (ps != null)
                    {
                        ps.m_Person = this;
                        WorkMachine = ps.WorkMachine;
						HospitalMachine = ps.HospitalMachine;
						TrainerMachine = ps.TrainerMachine;
                    }
                    workRoom.AddWorker(this);
                }
            }
            else
            {
                PersonnelSpace ps = workRoom.FindEmptySpace(this);
                if (ps != null)
                {
                    ps.m_Person = this;
                    WorkMachine = ps.WorkMachine;
                    HospitalMachine = ps.HospitalMachine;
					TrainerMachine = ps.TrainerMachine;
                }
                workRoom.AddWorker(this);
            }

            m_WorkRoom = workRoom;

            if (workRoom != null)
                Data.m_WorkRoomID = workRoom.ID;
            else
                Data.m_WorkRoomID = -1;

            if (m_NpcCmpt != null)
            {
                m_NpcCmpt.WorkEntity = m_WorkRoom;
                m_NpcCmpt.Work = WorkMachine;
                m_NpcCmpt.Cure = HospitalMachine;
				m_NpcCmpt.Trainner = TrainerMachine;
            }
        }
	}

    public override void UpdateWorkSpace(PersonnelSpace ps){
        WorkMachine = ps.WorkMachine;
        HospitalMachine = ps.HospitalMachine;
		TrainerMachine = ps.TrainerMachine;
        if (m_NpcCmpt != null)
        {
            m_NpcCmpt.Work = WorkMachine;
            m_NpcCmpt.Cure = HospitalMachine;
			m_NpcCmpt.Trainner = TrainerMachine;
        }
    }

    public override void UpdateWorkMachine(Pathea.Operate.PEMachine pm)
    {
        WorkMachine = pm;
        if (m_NpcCmpt != null)
        {
            m_NpcCmpt.Work = WorkMachine;
        }
    }

    public override void UpdateHospitalMachine(Pathea.Operate.PEDoctor pd)
    {
        HospitalMachine = pd;
        if (m_NpcCmpt != null)
        {
            m_NpcCmpt.Cure = HospitalMachine;
        }
    }

	public override void UpdateTrainerMachine(Pathea.Operate.PETrainner pt)
	{
		TrainerMachine = pt;
		if (m_NpcCmpt != null)
		{
			m_NpcCmpt.Trainner = TrainerMachine;
		}
	}

	/// <summary>
	/// Clears the work room. Dont remember it in brain
	/// </summary>
	public void ClearWorkRoom()
	{
        //m_BrainMemory.ClearWorkRoom();

        //lw:Bug report Win64 Steam Version:V1.0.5 from SteamId 76561198032245399
        if (WorkRoom == null) return;

        WorkRoom.RemoveWorker(this);
        WorkRoom = null;
	}

	public bool FollowMe (bool follow)
	{	
		if (follow)
		{
            if (m_Npc != null)
            {
                m_Npc.SetFollower(true);
                return true;
            }
		}
		else
		{
			if (m_Npc != null)
			{
                m_Npc.SetFollower(false);
                //Idle(2.0f);
				return true;
			}
		}


		return false;
	}

    public bool FollowMe(int hero_id)
    {
#if UNITY_EDITOR
        if (hero_id != 0 && hero_id != 1)
        {
            Debug.LogError("The giving hero id is wrong, check?");
            Debug.DebugBreak();
        }
#endif

        if (m_Npc == null)
        {
            Debug.LogWarning("This npc cannot be a follower");
            return false;
        }

        // Set NPC follow
        m_Npc.SetFollower(true, hero_id);

        m_RetainState = CSConst.pstFollow;

        return true;
    }
	

	public void KickOut()
	{
		if (m_Creator == null)
		{
			Debug.LogWarning("The Creator is not exsit.");
			return;
		}

        //--to do: wait Dismiss to RemoveNpc
        if (!PeGameMgr.IsMulti)
        {
            m_Creator.RemoveNpc(m_Npc);
            m_Npc.Dismiss();
        }
        else
        {
            ((AiAdNpcNetwork)NetworkInterface.Get(ID)).RPCServer(EPacketType.PT_CL_CLN_RemoveNpc);
        }
	}



    //public override void MoveTo(Vector3 destPos, SpeedState speed)
    //{
    //    //--to do: wait
    //    //if (destPos == Vector3.zero)
    //    //    m_Npc.CmdMoveTo = Vector3.zero;
    //    //else
    //    //    m_Npc.SetCmdMoveTo(destPos, 1.0f, speed);
    //    Request req;
    //    if (m_Request != null)
    //    {
    //        req = m_Request.Register(EReqType.MoveToPoint, destPos, 5.0f, true, speed);
    //        if (req != null)
    //        {
    //            currentRequest = req;
    //            Debug.LogError(destPos);
    //        }
    //    }
    //}
	
	public override void MoveToImmediately(Vector3 destPos)
	{
        //--to do: wait
        //if (m_Npc.isActive)
        //    m_Npc.SetFadeOut(1.0f, destPos);  
        //else
        //    m_Npc.transform.position = destPos;
	}
	
	public override bool CanBehave()
	{
        //--to do: wait
        //if (m_Npc.CurAiType != AiNpcObject.EAiType.Fight && !m_Npc.dead)
        //    return true;
		
		return false;
	}
	
	public override void Sleep(bool v)
	{ 
        //--to do: wait
		//m_Npc.Sleep(v);
	}

	public override void Stay ()
	{
        //--to do: wait
		//m_Npc.CmdIdle = true;
	}
	
	public override void PlayAnimation(EAnimateType type, bool v)
	{
        //--to do: wait
        //switch (type)
        //{
        //case EAnimateType.WorkOnEnhanceMachine:
        //    m_Npc.WorkOnEnhancementMachine(v);
        //    break;
        //case EAnimateType.WorkOnRepairMachine:
        //    m_Npc.WorkOnRepairMachine(v);
        //    break;
        //case EAnimateType.WorkOnRecycleMachine:
        //    m_Npc.WorkOnEnhancementMachine(v);
        //    break;
        //case EAnimateType.WorkOnFactory:
        //    m_Npc.WorkOnEnhancementMachine(v);
        //    break;
        //case EAnimateType.WorkOnStorage:
        //    m_Npc.CmdIdle = true;
        //    break;
        //case EAnimateType.SitDown:
        //    m_Npc.CmdPlayAnimation("SitDown", v);
        //    break;
        //case EAnimateType.WorkOnWatering:
        //    m_Npc.Herb(v);
        //    break;
        //case EAnimateType.WorkOnWeedding:
        //    m_Npc.Herb(v);
        //    break;
        //default:
        //    break;
        //}
	}
	
	
	// -- Call back
	protected override void OnRestToDest (PersonnelBase npc)
	{
		base.OnRestToDest (npc);
		
        //m_State = CSConst.pstRest;
		
        //--to do: wait
		//m_Npc.AttackMode = EAttackMode.Passive;
	}
	
	protected override void OnRestMeetBlock (PersonnelBase npc)
	{
		base.OnRestMeetBlock (npc);
		
        //Idle (Random.Range(1, 7));
	}
	
	protected override void OnIdleToDest (PersonnelBase npc)
	{
		base.OnIdleToDest (npc);
		
        //m_State = CSConst.pstIdle;
	}
	
	protected override void OnWorkToDest (PersonnelBase npc)
	{
		base.OnWorkToDest (npc);
        //m_State = CSConst.pstWork;
        //--to do: wait
        //m_Npc.AttackMode = EAttackMode.Attack;

		if (WorkRoom.m_Type == CSConst.etFarm)
		{
            //_onWorkFarmToDest();
		}
	}
	
	protected override void OnWorkMeetBlock (PersonnelBase npc)
	{
		base.OnWorkMeetBlock (npc);
		
		Debug.Log("I cant work Now, I meet block. Shit!!! Shit!!!"); 
		
        //Idle (Random.Range(1, 7));
	}
}
