//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//
////
////  Schedule For worker. when the personnel is a worker,
////		
////   	The most function in this partial class will be invoked.
////
//
//public partial class CSPersonnel : PersonnelBase 
//{
////	private CSCommon m_OldRoom = null;
//	/// <summary>
//	/// The main worker function, invoke in npc think
//	/// </summary>
//	public void WorkerTick()
//    {
//        //1.judge if request needed
//        //1)is doing work and work at the same room from currentRequest
//        //2)when 1) is not, is doing work and work at the same room from RequestCmpt
//
//
//        //2.if needed request
//        if (_doWork())
//        {
//            //3.change the parameters after request succeeded
//
//        }
//
//        
//
//
//
//
//
////--to do: new
////          if (m_State == CSConst.pstAtk )
////          {
////              //--to do: wait enemy
////              //if (m_Npc.enemy == null)
////              //    SetTargetMonster(null);
////              //else
////              //    return;
////          }
////        else if (m_State == CSConst.pstPrepare && m_RetainState != CSConst.pstWork)
////            return;
//		
////        // Calculate Time
////        int hour = (int) (GameTime.Timer.HourInDay);
//		
////        // Normak Schedule
////        if (m_WorkMode == CSConst.pwtNormalWork)
////        {
////            if (m_ScheduleMap.ContainsKey(hour))
////            {
////                if (m_ScheduleMap[hour] == EScheduleType.Rest)
////                {
////                    if (m_State != CSConst.pstRest)
////                        Rest();
////                }
////                else if (m_ScheduleMap[hour] == EScheduleType.Work)
////                {	
////                    if (Stamina < 20)
////                    {
////                        if (m_State != CSConst.pstRest)
////                            Rest();
////                    }
////                    else if (Stamina > 200)
////                    {
//////						if (WorkRoom != null && m_State != CSConst.pstWork)
//////							WorkNow();
//////						else if (WorkRoom == null && m_State != CSConst.pstIdle)
//////							Idle();
////                        if (!_doWork () && m_State != CSConst.pstIdle)
////                            Idle();
////                    }
////                    else
////                    {
////                        if (m_State != CSConst.pstRest )
////                        {
////                            if (!_doWork () && m_State != CSConst.pstIdle)
////                                Idle();
////                        }
////                    }
////                }
////            }
////            else
////            {
////                if (m_RetainState != CSConst.pstIdle)
////                    Idle(Random.Range(1, 10));
////            }
////        }
////        // Workaholic Schedule
////        else if (m_WorkMode == CSConst.pwtWorkaholic)
////        {
//			
////            if (Stamina < 20)
////            {
////                if (m_State != CSConst.pstRest)
////                    Rest();
////            }
////            else if (Stamina > 150)
////            {
//////				if (WorkRoom != null && m_State != CSConst.pstWork)
//////					WorkNow();
//////				else if (WorkRoom == null && m_State != CSConst.pstIdle)
//////					Idle();
//
////                if (!_doWork () && m_State != CSConst.pstIdle)
////                    Idle();
//
////            }
////            else
////            {
////                if (m_State != CSConst.pstRest)
////                {
////                    if (!_doWork () && m_State != CSConst.pstIdle)
////                        Idle();
////                }
////            }
////        }
////        // Work When Need
////        else if (m_WorkMode == CSConst.pwtWorkWhenNeed)
////        {
////            if (WorkRoom != null &&  WorkRoom.NeedWorkers())
////            {
////                if (Stamina < 20)
////                {
////                    if (m_State != CSConst.pstRest)
////                        Rest();
////                }
////                else if (Stamina > 200)
////                {
////                    if (!_doWork () && m_State != CSConst.pstIdle)
////                        Idle();
////                }
////                else
////                {
////                    if (m_State != CSConst.pstRest)
////                    {
////                        if (!_doWork () && m_State != CSConst.pstIdle)
////                            Idle();
////                    }
////                }
////            }
////            else
////            {
////                if (m_ScheduleMap[hour] == EScheduleType.Rest)
////                {
////                    if (m_State != CSConst.pstRest)
////                        Rest();
////                }
////                else if (m_ScheduleMap[hour] == EScheduleType.Work)
////                {
////                    if (Stamina > 300)
////                    {
////                        if (m_State != CSConst.pstIdle)
////                            Idle();
////                    }
////                    else if (Stamina < 100)
////                    {
////                        if (m_State != CSConst.pstRest && m_State != CSConst.pstWork)
////                            Rest();
////                    }
////                }
////            }
//			
////        }
//		
//	}
//
//	bool _doWork ()
//	{
//        //--to do: new
//        //if (WorkRoom != null)
//        //{
//        //    if ( (m_State != CSConst.pstWork && m_State != CSConst.pstPrepare) || m_OldRoom != WorkRoom)
//        //    {
//        //        WorkNow();
//        //        m_OldRoom = WorkRoom;
//        //        return true;
//        //    }
//
//        //    if (m_State == CSConst.pstWork || m_RetainState == CSConst.pstWork)
//        //        return true;
//
//        //}
//
//		//return false;
//
//        if (WorkRoom != null)
//        {
//            //--to do: addRequest
//        }
//        return false;
//	}
//}
