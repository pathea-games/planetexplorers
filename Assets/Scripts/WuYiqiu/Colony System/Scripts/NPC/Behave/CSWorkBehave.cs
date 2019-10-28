//using UnityEngine;
//using System.Collections;

//public class CSWorkBehave : CSNPCBehave 
//{

//    private CSWorkBParam m_Param; 
	

//    public override void Init (EPriority priority, CSBehaveParam param, PersonnelBase npc)
//    {
//        m_Priority = priority;
//        m_Param = param as CSWorkBParam;
//        m_Npc = npc;
//        if (m_Param.m_CurSpace != null)
//        {
//            m_Param.m_CurSpace.m_Person = m_Npc;
//            m_Param.m_CurSpace.OnPosChanged += OnPosChanged;
//        }
//        else
//            Debug.LogWarning("Init Work behave some warings happaned.");

//    }

//    public override void Start (int npc_id)
//    {
//        //if (NpcManager.Instance == null)
//        //{
//        //    Debug.LogError("There is no NpcManager in the scene.");
//        //    return;
//        //}

//        if (m_Param == null)
//        {
//            Debug.LogError("You must initialize this bebave before it start.");
//            return;
//        }
		
//        if (m_State != EState.Preparing)
//        {
//            Debug.LogError("This behave is already start!");
//            return;
//        }
		
//        if (m_Npc == null)
//        {
//            Debug.Log("Can't find the behave npc.");
//            return;
//        }

//        if (m_Param.m_CurSpace.m_Person != m_Npc)
//        {
//            Debug.LogWarning("The work space is not belong the behave NPC");
//            return;
//        }

//        m_State = EState.Running;

//        //m_Npc.m_Pos = m_Param.m_CurSpace.FinalPos;
//        //m_Npc.m_Rot = m_Param.m_CurSpace.m_Rot;

//    }

//    public override void Update ()
//    {
//        if (m_State == EState.Running)
//        {
//            DoWorkAction(true);
//            if (m_Param.onBehaveFinished != null)
//                m_Param.onBehaveFinished(m_Npc);
			
//            m_State = EState.Holding;
//        }

//        #if UNITY_EDITOR
//        if (m_Param.m_CurSpace.m_Person != m_Npc)
//        {
//            Debug.LogError("The Npc's work space is not his, check?");
//            Debug.DebugBreak();
//        }
//        #endif

//    }

//    public override void Break()
//    {
//        DoWorkAction(false);
//        //m_Param.m_CurSpace.OnPosChanged -= OnPosChanged;
//        //m_Param.m_CurSpace.m_Person = null;
//    }

//    public override void Pause ()
//    {
//        m_State = EState.Pause;
//        DoWorkAction(false);
//    }

//    public override void Continue ()
//    {
//        m_State = EState.Running;
//    }

//    public override void Over ()
//    {
//        DoWorkAction(false);
//        //m_Param.m_CurSpace.OnPosChanged -= OnPosChanged;
//        //m_Param.m_CurSpace.m_Person = null;
//    }

//    private void DoWorkAction(bool isDo)
//    {
//        switch (m_Param.m_WorkMode)
//        {
//        case CSWorkBParam.EWorkType.Enhance:
//            m_Npc.PlayAnimation(PersonnelBase.EAnimateType.WorkOnEnhanceMachine, isDo);
//            break;
//        case CSWorkBParam.EWorkType.Repair:
//            m_Npc.PlayAnimation(PersonnelBase.EAnimateType.WorkOnRepairMachine, isDo);
//            break;
//        case CSWorkBParam.EWorkType.Recycle:
//            m_Npc.PlayAnimation(PersonnelBase.EAnimateType.WorkOnRecycleMachine, isDo);
//            break;
//        case CSWorkBParam.EWorkType.Factory:
//            m_Npc.PlayAnimation(PersonnelBase.EAnimateType.WorkOnFactory, isDo);
//            break;
//        default:
//            break;
//        }
//    }


//    void OnPosChanged (PersonnelSpace space)
//    {
//        if ((space.Pos - space.FinalPos).sqrMagnitude < CSCommon.SqrMaxDistance)
//            m_Npc.m_Pos = space.FinalPos;
//        else
//        {
//            m_Npc.m_Pos = space.FinalPos;
//            if (m_Param.onMeetBlock != null)
//                m_Param.onMeetBlock(m_Npc);

//            m_State = EState.Finished;
//        }

//        m_Npc.m_Pos = space.FinalPos;
//    }
//}
