//using UnityEngine;
//using System.Collections;

//public class CSWorkBehaveNoObstacle : CSNPCBehave 
//{

//    private CSWorkBParam m_Param;

//    private float m_StartTime = 0.0f;
//    public override void Init (EPriority priority, CSBehaveParam param, PersonnelBase npc)
//    {
//        m_Priority = priority;
//        m_Param = param as CSWorkBParam;
//        m_Npc = npc;
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

//        m_State = EState.Running;
//    }
	

//    public override void Update ()
//    {
//        if (m_State == EState.Running)
//        {
//            m_StartTime = Time.time;
//            DoWorkAction(true);
//            m_State = EState.Holding;

//            if (m_Param.m_DeplayTime < 0.001f)
//            {
//                if (m_Param.onBehaveFinished != null)
//                    m_Param.onBehaveFinished(m_Npc);
//            }

////			if ( (m_Npc.m_Pos - m_Param.m_DesPos).sqrMagnitude > 2.0f)
////				m_Npc.m_Pos = m_Param.m_DesPos;

//            m_Npc.m_Rot = Quaternion.LookRotation(m_Param.m_DesPos - new Vector3(m_Npc.m_Pos.x, m_Param.m_DesPos.y, m_Npc.m_Pos.z));
//        }
//        else if (m_Param.m_DeplayTime > 0.0f )
//        {
//            float dt = Time.time - m_StartTime;
//            if (dt > m_Param.m_DeplayTime)
//            {
//                if (m_Param.onBehaveFinished != null)
//                    m_Param.onBehaveFinished(m_Npc);

//                m_Param.m_DeplayTime = 0;
//            }
//        }
//    }

//    public override void Break()
//    {
//        DoWorkAction(false);
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
//        case CSWorkBParam.EWorkType.Farm_Watering:
//            m_Npc.PlayAnimation(PersonnelBase.EAnimateType.WorkOnWatering, isDo);
//            break;
//        case CSWorkBParam.EWorkType.Farm_Weeding:
//            m_Npc.PlayAnimation(PersonnelBase.EAnimateType.WorkOnWatering, isDo);
//            break;
//        default:
//            break;
//        }
//    }
//}
