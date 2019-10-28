//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using ItemAsset;
//using Pathea;
//
//public partial class CSPersonnel : PersonnelBase 
//{
////	private int m_HitCounter = 0;
//
//	#region STATE_GAIN_VAL
//	
//	public const float RestoreHPRate = 0.02f;	// ?/s game time
////	private float m_increaseHP = 0.0f;
//
//	public const float RestoreStaminaRate = 0.02f;  // ?/s game time
//
//	// Stamina consumption rate
//	public const float STAMINA_CONSUMPTION_RATE_FOR_WORK= 0.005f;  // ?/s game time
//	public const float STAMINA_CONSUMPTION_RATE_FOR_IDLE = 0.001f;  // ?/s game time
//	public const float STAMINA_CONSUMPTION_RATE_FOR_PATROL = 0.005f;
//
//	// 
//	public const float STAMINA_CONSUMPTION_WATERING = 0.5f;
//	public const float STAMINA_CONSUMPTION_WEEDING  = 0.5f;
//	public const float STAMINA_CONSUMPTION_HARVEST  = 0.8f;
//	public const float STAMINA_CONSUMPTION_PLANT    = 0.8f;
//
//	#endregion
//
////	float random_stamina_factor = 0.2f;
//	// Think every frame 
//    //protected void ThinkTick()
//    //{
//    //    // State gain
//    //    if (m_State == CSConst.pstRest)
//    //    {
//    //        m_increaseHP += (float)m_Creator.Deltatime * RestoreHPRate;
//    //        if (m_Skill.GetAttribute(AttribType.Hp) < m_Skill.GetAttribute(AttribType.HpMax))
//    //            m_Skill.SetAttribute(AttribType.Hp,m_Skill.GetAttribute(AttribType.Hp)+ (int)m_increaseHP); 
//			
//    //        m_increaseHP -= (int)m_increaseHP;
//
//    //        float increaseStamina = 0;
//    //        increaseStamina = (float)m_Creator.Deltatime * RestoreHPRate;
//    //        if (Stamina < MaxStamina)
//    //            Stamina += increaseStamina;
//    //        else
//    //            Stamina = MaxStamina;
//
//    //    }
//    //    else
//    //        m_increaseHP = 0;
//
//    //    // Auto eat food from Storage
//    //    if (m_State != CSConst.pstRest && Stamina < MaxStamina * random_stamina_factor)
//    //    {
//    //        if (Dwellings.Assembly != null)
//    //        {
//    //            foreach (CSCommon csc in Dwellings.Assembly.m_BelongObjectsMap[CSConst.ObjectType.Storage])
//    //            {
//    //                CSStorage storage = csc as CSStorage;
//    //                ItemObject item = storage.FindSpecifiedItem();
//    //                if (item != null)
//    //                {
//    //                    if (m_UseItem.Use(item))
//    //                    {
//    //                        CSStorageHistoryAttr cssha = new CSStorageHistoryAttr();
//    //                        cssha.m_Day = (int)GameTime.Timer.Day;
//    //                        cssha.m_TimeStr = GameTime.Timer.FormatString("hh : mm ") ;
//    //                        cssha.m_NpcName = Name;
//    //                        cssha.m_ItemStr = item.protoData.GetName();
//    //                        cssha.m_Type = CSStorageHistoryAttr.EType.NpcUseSth;
//    //                        storage.AddHistory(cssha);
//    //                        if (item.GetCount() == 0)
//    //                            storage.Remove(item);
//
//    //                        random_stamina_factor = Random.Range(0.2f, 0.6f);
//    //                    }
//
//    //                    break;
//    //                }
//    //            }
//    //        }
//    //        //--to do: wait ??
//    //        //ItemObject io = null;
//    //        //m_UseItem.Use(io);
//
//    //    }
//
//    //    if (m_State == CSConst.pstWork)
//    //    {
//    //        float expendedStamina = (float)m_Creator.Deltatime * STAMINA_CONSUMPTION_RATE_FOR_WORK;
//    //        if (Stamina > 0)
//    //            Stamina -= expendedStamina;
//    //        else
//    //            Stamina = 0;
//
//    //    }
//    //    else if (m_State == CSConst.pstIdle)
//    //    {
//    //        float expendedStamina = (float)m_Creator.Deltatime * STAMINA_CONSUMPTION_RATE_FOR_IDLE;
//    //        if (Stamina > 0)
//    //            Stamina -= expendedStamina;
//    //        else
//    //            Stamina = 0;
//    //    }
//    //    else if (m_State == CSConst.pstPatrol)
//    //    {
//    //        float expendedStamina = (float)m_Creator.Deltatime* STAMINA_CONSUMPTION_RATE_FOR_PATROL;
//    //        if (Stamina > 0)
//    //            Stamina -= expendedStamina;
//    //        else
//    //            Stamina = 0;
//    //    }
//    //    else if (m_State == CSConst.pstAtk)
//    //    {
//    //        if (m_Attaking)
//    //        {
//    //            //EquipedNpc npc =  m_EquipedNpc;
//
//    //            if (m_Npc != null)
//    //            {
//    //                if (EqupsMeleeWeapon)
//    //                {
//    //                    //--to do: wait
//    //                    //if (m_Npc.isAttacking)
//    //                    //    ObstacleDetect = false;
//    //                    //else
//    //                    ObstacleDetect = true;
//    //                }
//    //                else
//    //                    ObstacleDetect = false;
//
//    //            }
//    //        }
//    //        else
//    //            ObstacleDetect = false;
//
//    //    }
//    //}
//
//
//	// Think every 16 frame
//    //protected void Think()
//    //{
//    //    if (Dwellings.Assembly == null)
//    //        return;
//
//    //    if ( m_State == CSConst.pstDead)
//    //        return;
//
//    //    //--to do: wait
//    //    //// Npc is attacking or prepare attack
//    //    //if (m_Attaking)
//    //    //{
//    //    //    EnemyInst mi = null;
//    //    //    AiObject target_ai = null;
//    //    //    if (m_Npc.aiTarget.enemy != null && (m_TargetEnemy == null 
//    //    //         || m_Npc.aiTarget.enemy.gameObject != m_TargetEnemy.m_Enemy.gameObject))
//    //    //    {
//    //    //        target_ai = m_Npc.aiTarget.enemy.gameObject.GetComponent<AiObject>();
//				
//    //    //        if (!m_BrainMemory.m_ObstacleEnemys.Contains(target_ai))
//    //    //        {
//    //    //            mi = Dwellings.Assembly.Monsters.Find(item0 => item0.m_Enemy == target_ai);
//    //    //            if (mi != null)
//    //    //                SetTargetMonster(mi);
//    //    //        }
//    //    //        else
//    //    //        {
//    //    //            m_Npc.aiTarget.ClearHatred(m_Npc.aiTarget.enemy.gameObject);
//    //    //            if (m_TargetEnemy != null)
//    //    //                m_Npc.aiTarget.AddExtraHatred(m_TargetEnemy.m_Enemy.gameObject, 1);
//    //    //        }
//    //    //    }
//    //    //}
//
//    //    //test code
//    //    if (currentRequest == null)
//    //    {
//    //        Debug.Log("currentRequest==null");
//    //    }
//    //    else
//    //    {
//    //        Debug.Log(currentRequest.type==EReqType.MoveToPoint);
//    //        Debug.Log(currentRequest.state);
//    //        Debug.Log(currentRequest.mask);
//    //    }
//       
//    //    if (IsRandomNpc && IsFollower && m_Occupation != CSConst.potFollower)
//    //    {
//    //        ClearAllBehave();
//    //        m_Occupation = CSConst.potFollower;
//    //    }
//
//    //    // Occupation Schedule
//    //    if (m_Occupation == CSConst.potDweller)
//    //    {
//    //        DwellerTick();
//    //    }
//    //    else if (m_Occupation == CSConst.potWorker)
//    //    {
//    //        WorkerTick();
//    //    }
//    //    else if (m_Occupation == CSConst.potFarmer)
//    //    {
//    //        FarmerTick();
//    //    }
//    //    else if (m_Occupation == CSConst.potSoldier)
//    //    {
//    //        SoliderTick();
//    //    }
//    //    else if (m_Occupation == CSConst.potFollower)
//    //    {
//    //        FollowerTick();
//    //    }
//
//    //}
//
//	private void DwellerTick()
//	{
//        //1.check current Request
//
//        //2.remove and set current request null
//
//
//
//        
//        
//        //--to do: new
//        ////--to do: wait
//        ////if (m_State == CSConst.pstAtk )
//        ////{
//        ////    if (m_Npc.enemy == null)
//        ////        SetTargetMonster(null);
//        ////    else
//        ////        return;
//        ////}
//        ////else if (m_State == CSConst.pstPrepare)
//        ////    return;
//
//        //// Calculate Time
//        //int hour = (int) (GameTime.Timer.HourInDay);
//
//        //// Rest
//        //if (hour >= m_RestStartTime || (hour >= 0 
//        //    && hour <= m_RestDuration - (26 - m_RestStartTime ) ))
//        //{
//        //    if (m_State != CSConst.pstRest)
//        //        Rest();
//        //}
//        //// Idle
//        //else
//        //{
//        //    if (m_State != CSConst.pstIdle)
//        //    {
//        //        Idle(Random.Range(1, 10));
//        //    }
//        //}
//	}
//
//}
