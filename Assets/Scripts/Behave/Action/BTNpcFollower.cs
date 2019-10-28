using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using SkillSystem;
using PETools;
using Pathea.PeEntityExtNpcPackage;

namespace Behave.Runtime
{
    [BehaveAction(typeof(BTIsNpcFollower), "IsNpcFollower")]
    public class BTIsNpcFollower : BTNormal
    {
        BehaveResult Tick(Tree sender)
        {
            if (IsNpcFollower)
                return BehaveResult.Success;
            else
                return BehaveResult.Failure;
        }
    }
    [BehaveAction(typeof(BTNpcFollowerWork), "NpcFollowerWork")]
    public class BTNpcFollowerWork : BTNormal
    {
        static Vector3 s_Position = new Vector3(0.0f, -10000.0f, 0.0f);

        double m_StartTime = 0.0f;

        Vector3 m_Moveposition;

        bool m_Transparent;
        bool m_SetPos;
        float percent;
        //float WORKTIME = 300.0f;
        Vector3 GetMovePosition()
        {
            return PEUtil.GetRandomPosition(position, 1024.0f, 2048.0f);
        }

        Vector3 GetPosition()
        {
            return PEUtil.GetRandomPosition(GetMasterPosition(NpcMaster), 3.0f, 5.0f) + Vector3.up * 2.0f;
        }

        Vector3 GetPutItemPostion()
        {
            return PEUtil.GetRandomPosition(GetMasterPosition(NpcMaster), 3.0f, 5.0f);
        }

        Vector3 GetMasterPosition(ServantLeaderCmpt leader)
        {
            return leader.GetComponent<PeTrans>().position;
        }

        BehaveResult Init(Tree sender)
        {
            if (!IsNpcFollower || !IsNpcFollowerWork)
            {
                //SetModelFadeIn();
                return BehaveResult.Failure;
            }

            m_SetPos = false;
            m_Transparent = false;
            m_StartTime =  GameTime.Timer.Minute;
            m_Moveposition = GetMovePosition();
            percent = 0.0f;
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            SetNpcAiType(ENpcAiType.NpcFollowerWork);
            if (!IsNpcFollower)
                return BehaveResult.Failure;

            if (!IsNpcFollowerWork)
            {
                Fadein(3.0f);
                //  SetPosition(GetPosition());
                GetItemsSkill(GetPutItemPostion(), percent);
                return BehaveResult.Success;
            }

            double time = GameTime.Timer.Minute - m_StartTime;
            if (time < 3.0f)
                MoveToPosition(m_Moveposition, SpeedState.Run);
            else if (time < 4.0f)
            {
                if (!m_Transparent)
                {
                    Fadeout(3.0f);
                    m_Transparent = true;
                }
            }
            else
            {
                if (!m_SetPos)
                {
                    SetPosition(s_Position);
                    m_SetPos = true;
                }
            }

            if (time < 60.0f)//time < WORKTIME 
            {
                percent = System.Convert.ToSingle(time / 60.0f);
            }
            else
            {
                percent = 1.0f;
                CallBackFollower();
            }


            return BehaveResult.Running;
        }

        void Reset(Tree sender)
        {

        }
    }

    [BehaveAction(typeof(BTNpcFollower), "NpcFollower")]
    public class BTNpcFollower : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float firRadius = 0.0f;
            [BehaveAttribute]
            public float sndRadius = 0.0f;
            [BehaveAttribute]
            public float thdRadius = 0.0f;
            [BehaveAttribute]
            public string[] relax = new string[0];
            [BehaveAttribute]
            public string GatherTime;
            [BehaveAttribute]
            public string LootTime;
            [BehaveAttribute]
            public float normalPatrolRadiu;
            [BehaveAttribute]
            public float specialPatrolRadiu;

            Vector3 m_Anchor;

            Vector3 m_AnchorDir;

            public float PatrolRadiu = 2.0f;
            public Vector3 Anchor
            {
                get { return m_Anchor; }
            }

            public float SearchGatherTime = 2.0f;
            public float StartSearchGatherTime;
            public float SearchLootTime = 2.0f;
            public float StartLootTime;
            public float StandRadiu = 2.0f;

            //Vector3 m_LastAnchor;
            Vector3 m_LastPatrol;
            bool m_Init = false;
            bool m_Reached;
            bool m_Calculated;
            bool m_StandPatorl;

            bool m_CalculatedDir;

            SpeedState m_SpeedState;

            //static float minWalkRadius = 0.0f;
            static float maxWalkRadius = 6.0f;
            static float minRunRadius = 6.0f;
            static float maxRunRadius = 24.0f;
            static float minSprintRadius = 24.0f;
            //static float maxSprintRadius = 40.0f;

            GameObject mObj;

            public void Init()
            {
                if (!m_Init)
                {

                    float[] temp = PETools.PEUtil.ToArraySingle(GatherTime, ',');
                    if (temp.Length == 2)
                    {
                        SearchGatherTime = Random.Range(temp[0], temp[1]);

                        temp = PETools.PEUtil.ToArraySingle(LootTime, ',');
                        SearchLootTime = Random.Range(temp[0], temp[1]);

                    }
                    m_Init = true;
                }
            }

            public void CalculateAnchor(Vector3 pos, Vector3 center, Vector3 dir)
            {
                if (!m_Calculated)
                {
                    m_Reached = false;
                    m_Calculated = true;
                    //Vector3 v = Vector3.ProjectOnPlane(pos - center, Vector3.up);
                    bool _IsInSpSence = Pathea.PeGameMgr.IsAdventure && RandomDungenMgr.Instance != null && RandomDungenMgrData.dungeonBaseData != null;
                    float angle = _IsInSpSence ? 20.0f : 90.0f;
                    m_Anchor = PEUtil.GetRandomPosition(Vector3.zero, dir, firRadius, sndRadius, -angle, angle);
                    m_Anchor = new Vector3(m_Anchor.x, 0.0f, m_Anchor.z);
                    
                  
                }
            }

            public void CalculateAvoidAnchor(Vector3 pos, Vector3 center, Vector3 dir)
            {
                if (!m_Calculated)
                {
                    m_Reached = false;
                    m_Calculated = true;
                    //Vector3 v = Vector3.ProjectOnPlane(pos - center, Vector3.up);
                    m_Anchor = PEUtil.GetRandomPosition(Vector3.zero, dir, firRadius, sndRadius, -90.0f, 90.0f);
                    m_Anchor = new Vector3(m_Anchor.x, 0.0f, m_Anchor.z);
                }
            }

            public bool IsOutside(Vector3 pos, Transform target)
            {
                float sqrDistanceH = PEUtil.SqrMagnitudeH(pos, target.position + m_Anchor);
                return sqrDistanceH > thdRadius * thdRadius;
            }

            public bool isReached(Vector3 pos, Transform target)
            {
                float sqrDistanceH = PEUtil.SqrMagnitudeH(pos, target.position + m_Anchor);
                if (sqrDistanceH < 1.0f * 1.0f)
                {
                    m_Reached = true;
                    m_Calculated = false;
                }

                return m_Reached;
            }

            public bool InRadius(Vector3 pos, Vector3 targetCentor, float r, float R, bool is3D)
            {
                float D = PETools.PEUtil.Magnitude(pos, targetCentor, is3D);
                return D > r && D <= R;
            }

            public void ResetCalculated()
            {
                m_Calculated = false;
            }

            public Vector3 GetFollowPosition(Transform target, Vector3 velocity)
            {
                if (CheckFirst())
                {
                    //m_LastAnchor = target.position;
                }

                //bool _IsInSpSence = Pathea.PeGameMgr.IsAdventure && RandomDungenMgr.Instance != null && RandomDungenMgrData.dungeonBaseData != null;
                //if (_IsInSpSence && m_dungenFollowPos != Vector3.zero)
                //    return m_dungenFollowPos;
                //return m_LastAnchor + m_Anchor /*+ velocity.normalized * m_Anchor.magnitude*/;
                return target.position + m_Anchor /*+ velocity.normalized * m_Anchor.magnitude*/;
            }


            //Vector3 m_dungenFollowPos;
            //void OnPathComplete(Pathfinding.Path  path)
            //{
            //    if (path != null)
            //    {
            //        path.Claim(path);
            //        if (path.vectorPath != null && path.vectorPath.Count > 0)
            //        {
            //            Vector3 pos = path.vectorPath[path.vectorPath.Count - 1];
            //            if (!PEUtil.CheckPositionUnderWater(pos - Vector3.up * 0.6f))
            //                m_dungenFollowPos = pos;
                        
            //        }
            //        path.Release(path);
            //    }
            //}

            //public void GetPos(Vector3 npcPos, Vector3 playerPos, Vector3 direction)
            //{
            //    if (AstarPath.active != null)
            //    {
            //        Pathfinding.RandomPath path = Pathfinding.RandomPath.Construct(npcPos, (int)Random.Range(firRadius, sndRadius) * 10, OnPathComplete);
            //        path.spread = 40000;
            //        path.aimStrength = 1f;
            //        path.aim = PETools.PEUtil.GetRandomPosition(playerPos, direction, firRadius, sndRadius, -75.0f, 75.0f);
            //        AstarPath.StartPath(path);               
            //    }
            //    m_dungenFollowPos = Vector3.zero;
            //}

            public Vector3 GetAnchorDir()
            {
                return m_AnchorDir;
            }
            public bool hasCalculatedDir { get { return m_CalculatedDir; } }
            public void ResetCalculatedDir()
            {
                m_CalculatedDir = false;
            }
            public bool GetCanMoveDirtion(PeEntity entity, float minAngle)
            {
                if (!m_CalculatedDir)
                {
                    for (int i = 1; i < (int)360.0f / minAngle; i++)
                    {
                        m_AnchorDir = Quaternion.AngleAxis(minAngle * i, Vector3.up) * entity.peTrans.forward;
                        if (!PETools.PEUtil.IsForwardBlock(entity, m_AnchorDir, 3.0f))
                        {
                            m_CalculatedDir = true;
                            return true;
                        }
                        m_AnchorDir = Vector3.zero;
                    }
                }
                m_AnchorDir = Vector3.zero;
                return false;
            }

            //public Vector3 GetPatrolPosition(Transform target)
            //{
            //    if (CheckSecond())
            //    {
            //        m_LastPatrol = PEUtil.GetRandomPosition(Vector3.zero, PatrolRadiu, sndRadius);
            //    }

            //    return target.position + m_Anchor + m_LastPatrol;
            //}

            /*
             * 0  - 16 walk
             * 16  - 32 run
             * 32 - 64 sprint
             */
            public SpeedState CalculateSpeedState(float sqrDistanceH)
            {
                SpeedState state = m_SpeedState;
                switch (state)
                {
                    case SpeedState.None:
                        m_SpeedState = SpeedState.Walk;
                        break;
                    case SpeedState.Walk:
                        if (sqrDistanceH > maxWalkRadius * maxWalkRadius)
                            m_SpeedState = SpeedState.Run;
                        break;
                    case SpeedState.Run:
                        if (sqrDistanceH < minRunRadius * minRunRadius)
                            m_SpeedState = SpeedState.Walk;

                        if (sqrDistanceH > maxRunRadius * maxRunRadius)
                            m_SpeedState = SpeedState.Sprint;
                        break;
                    case SpeedState.Sprint:
                        if (sqrDistanceH < minSprintRadius * minSprintRadius)
                            m_SpeedState = SpeedState.Run;
                        break;
                    default:
                        m_SpeedState = SpeedState.Walk;
                        break;
                }

                return m_SpeedState;
            }

            float m_FirTime = 0.0f;
            float m_CurFirTime = 0.0f;

            float m_SndTime = 0.0f;
            float m_CurSndTime = 0.0f;

            public float startPralTime = 0.0f;
            public float lastSetPosTime = 0.0f;
            public float waitPralTime;

            //无寻路数据时
            public float sWalkTime0 = 2.0f;
            public float sWalkTime1 = 2.0f;
            public float sWalkTime2 = 10.0f;


            public float LastWalkTime = 0.0f;
            public float LastStopTime = 0.0f;

            public bool CheckFirst()
            {
                if (Time.time - m_FirTime > m_CurFirTime)
                {
                    m_FirTime = Time.time;
                    m_CurFirTime = Random.Range(1.0f, 3.0f);
                    return true;
                }

                return false;
            }

            public bool CheckSecond()
            {
                if (Time.time - m_SndTime > m_CurSndTime)
                {
                    m_SndTime = Time.time;
                    m_CurSndTime = Random.Range(10.0f, 15.0f);
                    return true;
                }

                return false;
            }
        }

        Data m_Data;


        Vector3 GetFixedPosition(Vector3 pos1, Vector3 direction1, Vector3 pos2, Vector3 direction2, float height)
        {
            Vector3 pos = pos1;
            Vector3 newPosition = PEUtil.GetRandomPosition(pos1 + m_Data.Anchor, direction1, m_Data.firRadius, m_Data.sndRadius - 0.5f, -90.0f, 90.0f);
            if (PEUtil.CheckPositionNearCliff(newPosition))
            {
                pos = pos2;
                newPosition = PEUtil.GetRandomPosition(pos2 + m_Data.Anchor, direction2, m_Data.firRadius, m_Data.sndRadius - 0.5f, -90.0f, 90.0f);
            }

            RaycastHit hitInfo;
            Ray ray = new Ray(pos, Vector3.up);
            //Target in the hole
            if (Physics.Raycast(ray, out hitInfo, 128.0f, PEConfig.GroundedLayer))
            {
                ray = new Ray(newPosition, Vector3.up);
                if (Physics.Raycast(ray, out hitInfo, 128.0f, PEConfig.GroundedLayer))
                {
                    //hole in water
                    if (PEUtil.CheckPositionUnderWater(hitInfo.point - Vector3.up))
                        return newPosition;
                    else
                    {
                        ray = new Ray(newPosition, Vector3.down);
                        if (Physics.Raycast(ray, out hitInfo, 128.0f, PEConfig.GroundedLayer))
                            return hitInfo.point + Vector3.up;
                    }
                }
                else
                    return Vector3.zero;
            }
            else
            {
                //Target not in the hole
                Ray rayStart = new Ray(newPosition + 128.0f * Vector3.up, -Vector3.up);
                if (Physics.Raycast(rayStart, out hitInfo, 256.0f, PEConfig.GroundedLayer))
                {
                    if (PEUtil.CheckPositionUnderWater(hitInfo.point))
                        return newPosition;
                    else
                        return hitInfo.point + Vector3.up;
                }
            }
            return Vector3.zero;
        }

        bool CanCather(float radius = 2.0f, float angle = 360.0f)
        {
            List<GlobalTreeInfo> grassInfoList = new List<GlobalTreeInfo>();
            if (null != LSubTerrainMgr.Instance)
                grassInfoList = LSubTerrainMgr.Picking(position, transform.forward, false, radius, angle);
            else if (null != RSubTerrainMgr.Instance)
                grassInfoList = RSubTerrainMgr.Picking(position, transform.forward, false, radius, angle);

            return grassInfoList.Count > 0;
        }

        void EatSth()
        {
            ItemAsset.ItemObject mEatItem;
            if (NpcEatDb.IsContinueEat(entity, out mEatItem))
            {
                if (entity.UseItem.GetCdByItemProtoId(mEatItem.protoId) < PETools.PEMath.Epsilon)
                {
                    UseItem(mEatItem);
                }

            }
        }

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (!IsNpc || !IsNpcFollower || IsNpcFollowerCut)
                return BehaveResult.Failure;

            SetNpcState(ENpcState.Follow);
            entity.NpcCmpt.servantCallback = false;
            m_Data.StartSearchGatherTime = Time.time;
            m_Data.StartLootTime = Time.time;
            m_Data.waitPralTime = Random.Range(5.0f, 10.0f);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (entity.IsDeath())
                return BehaveResult.Failure;

            if (!IsNpc || !IsNpcFollower || IsNpcFollowerWork || IsNpcFollowerCut)
            {
                if (IsOnVCCarrier)
                    GetOff();

                if (IsOnRail)
                    GetOffRailRoute();

                SetNpcState(ENpcState.UnKnown);
                return BehaveResult.Failure;
            }

            SetNpcAiType(ENpcAiType.NpcFollower);
            PeTrans tr = NpcMaster.peEntity.peTrans;
            EatSth();

            if (IsSkillCast)
            {
                SetNpcState(ENpcState.UnKnown);
                return BehaveResult.Failure;
            }


            if (IsMotionRunning(PEActionType.HoldShield))
            {
                EndAction(PEActionType.HoldShield);
            }


            if (!IsOnVCCarrier && !IsOnRail && Time.time - m_Data.StartSearchGatherTime > m_Data.SearchGatherTime)
            {
                if (CanCather())//|| LootItemDropPeEntity.HasLootEntity()
                {
                    return BehaveResult.Failure;
                }
                else
                {
                    m_Data.StartSearchGatherTime = Time.time;
                }
            }

            if (entity.isRagdoll || entity.IsDeath())
                return BehaveResult.Running;

            Vector3 targetVelocity = Vector3.zero;
            PassengerCmpt passengerTarget = NpcMaster.Entity.passengerCmpt;
            if (passengerTarget != null && passengerTarget.IsOnVCCarrier)
            {
                //玩家在载具上NPC上车选座：跟随任务NPC优先
                int index = passengerTarget.drivingController.FindEmptySeatIndex();
                if (index >= 0 && !IsOnVCCarrier && (index - NpcMaster.RQFollowersIndex >= 0))
                {
                    GetOn(passengerTarget.drivingController, index);
                    MoveToPosition(Vector3.zero);
                }
                else
                {
                    //等到玩家entity移动速度
                    if (NpcMaster != null && NpcMaster.Entity.motionMove != null)
                        targetVelocity = NpcMaster.Entity.motionMove.velocity;
                }
            }
            else if (passengerTarget != null && passengerTarget.IsOnRail)
            {
                Railway.Route route = Railway.Manager.Instance.GetRoute(passengerTarget.railRouteId);
                if (route != null && route.train != null && route.train.HasEmptySeat() && !IsOnRail)
                    GetOn(passengerTarget.railRouteId, entity.Id, true);
                else
                {
                    if (NpcMaster != null && NpcMaster.Entity.motionMove != null)
                        targetVelocity = NpcMaster.Entity.motionMove.velocity;
                }
            }
            else
            {
                //玩家不在载具上时，NPC下载具
                if (passengerTarget != null && !passengerTarget.IsOnVCCarrier && IsOnVCCarrier)
                    GetOff();

                //下轻轨
                if (passengerTarget != null && !passengerTarget.IsOnRail && IsOnRail)
                    GetOffRailRoute();

                if (NpcMaster != null && NpcMaster.Entity.motionMove != null)
                    targetVelocity = NpcMaster.Entity.motionMove.velocity;
            }



            Vector3 avoidDir0 = Vector3.zero;
            Vector3 avoidDir1 = Vector3.zero;
            Vector3 avoidDir2 = Vector3.zero;
            Vector3 avoidDir3 = Vector3.zero;

            bool _IsnearLeague = entity.NpcCmpt.HasNearleague;
            bool _IsBlockBrush = AiUtil.CheckBlockBrush(entity, out avoidDir0);
            bool _IsnearDig = AiUtil.CheckDig(entity, NpcMaster.Entity, out avoidDir1);
            bool _IsDragging = AiUtil.CheckDraging(entity, out avoidDir2);
            bool _IsNearCreation = AiUtil.CheckCreation(entity, out avoidDir3);

            bool _needAvoid = _IsnearLeague || _IsnearDig || _IsBlockBrush || _IsDragging || _IsNearCreation;
            //bool _needAvoid0 = _IsnearDig || _IsBlockBrush || _IsDragging || _IsNearCreation;
            bool _Isstay = IsNpcFollowerSentry && !_needAvoid;
            bool _IsOnCreation = IsOnVCCarrier || IsOnRail;

            bool _InfirR = m_Data.InRadius(position, tr.position, 0.0f, m_Data.firRadius, true);
            bool _InsndR = m_Data.InRadius(position, tr.position, m_Data.firRadius, m_Data.sndRadius, true);
            //bool _InthdR = m_Data.InRadius(position, tr.position, m_Data.sndRadius, m_Data.thdRadius * 2.0f, true);

            Vector3 avoidplayer = position - tr.trans.position;
            Vector3 avoid0 = avoidDir0 != Vector3.zero ? position - avoidDir0 : Vector3.zero;
            Vector3 avoid1 = avoidDir1 != Vector3.zero ? position - avoidDir1 : Vector3.zero;
            Vector3 avoid2 = avoidDir2 != Vector3.zero ? position - avoidDir2 : Vector3.zero;
            Vector3 avoid3 = avoidDir3 != Vector3.zero ? position - avoidDir3 : Vector3.zero;
            Vector3 _AvoidDir = avoidplayer + avoid0 + avoid1 + avoid2 + avoid3 + existent.forward;

            bool _IsInSpSence = RandomDunGenUtil.IsInDungeon(entity);//(Pathea.PeGameMgr.IsAdventure && RandomDungenMgr.Instance != null && RandomDungenMgrData.dungeonBaseData != null);
          //|| (Pathea.PeGameMgr.IsStory && Pathea.PeGameMgr.IsSingle && Pathea.SingleGameStory.curType != Pathea.SingleGameStory.StoryScene.MainLand)
          //|| (Pathea.PeGameMgr.IsStory && Pathea.PeGameMgr.IsTutorial && Pathea.SingleGameStory.curType != Pathea.SingleGameStory.StoryScene.MainLand)
          //|| (Pathea.PeGameMgr.IsStory && Pathea.PeGameMgr.IsMulti && PlayerNetwork.mainPlayer != null && PlayerNetwork.mainPlayer._curSceneId != (int)Pathea.SingleGameStory.StoryScene.MainLand);

            //在建筑内跟随：检测头上是否有碰撞，保持和玩家的Y值差，不用找点方式寻路
            bool _underBlock = PETools.PEUtil.IsUnderBlock(entity);
            bool _IsForwardBlock = _IsInSpSence ? false : _underBlock ? PETools.PEUtil.IsForwardBlock(entity, entity.peTrans.forward, 2.0f) : false;
            //NPC不在载具上时跟随
            if (!_IsOnCreation)
            {
                //超出跟随距离snd_thd
                if (!_InfirR && !_InsndR)
                {
                    if (GameConfig.IsMultiMode)
                    {
                        if (Stucking(1.0f) || IsNpcFollowerSentry)
                        {
                            Vector3 fixedPos = GetFixedPosition(PETools.PEUtil.MainCamTransform.position, -PETools.PEUtil.MainCamTransform.forward, tr.position, -tr.forward, tr.bound.size.y);
                            if (PEUtil.CheckErrorPos(fixedPos))
                            {
                                SetPosition(fixedPos);
                                MoveToPosition(Vector3.zero);
                            }
                        }
                        else
                            m_Data.CalculateAnchor(position, PETools.PEUtil.MainCamTransform.position, -PETools.PEUtil.MainCamTransform.forward);
                    }
                    else
                    {
                        if (Stucking(1.0f) || _IsForwardBlock) // || IsNpcFollowerStand 单人放哨时 不跟随传送原地待命 || !_canPointMove
                        {
                            Vector3 fixedPos = GetFixedPosition(PETools.PEUtil.MainCamTransform.position, -PETools.PEUtil.MainCamTransform.forward, tr.position, -tr.forward, tr.bound.size.y);
                            float dy = Mathf.Abs(tr.position.y - fixedPos.y);
                            if (PEUtil.CheckErrorPos(fixedPos) && dy <= 3.0f)
                            {
                                SetPosition(fixedPos);
                                MoveToPosition(Vector3.zero);
                            }
                        }
                        else
                            m_Data.CalculateAnchor(position, PETools.PEUtil.MainCamTransform.position, -PETools.PEUtil.MainCamTransform.forward);

                        m_Data.startPralTime = Time.time;
                    }
                }

                //在内圈
                if (_InfirR && !m_Data.InRadius(position, tr.trans.position + m_Data.Anchor, m_Data.firRadius, m_Data.sndRadius, true))
                {
                    m_Data.ResetCalculated();
                    m_Data.CalculateAnchor(position, PETools.PEUtil.MainCamTransform.position, -PETools.PEUtil.MainCamTransform.forward);
                    m_Data.startPralTime = Time.time;
                }

                bool needCdTime = _IsInSpSence ?  !_InsndR : true;
                if (needCdTime && Time.time - m_Data.startPralTime >= m_Data.waitPralTime)
                {
                    m_Data.startPralTime = Time.time;
                    m_Data.CalculateAnchor(position, PETools.PEUtil.MainCamTransform.position, -PETools.PEUtil.MainCamTransform.forward);
                    m_Data.ResetCalculatedDir();
                }
                bool _nearAnchor = IsReached(position, tr.trans.position + m_Data.Anchor, true, 2.0f);
                bool _InPralCdTime = Time.time - m_Data.startPralTime < m_Data.waitPralTime;
                bool _moveWait = _InPralCdTime && _nearAnchor && !_needAvoid; //

                if (!_Isstay)
                {
                    if (!_needAvoid)
                    {
                        if (_moveWait)
                        {
                            if (_IsForwardBlock || Stucking(1.0f))
                            {
                                if (_IsInSpSence)
                                    m_Data.ResetCalculatedDir();

                                m_Data.GetCanMoveDirtion(entity, 30.0f);
                                if (m_Data.GetAnchorDir() != Vector3.zero)
                                {
                                    m_Data.ResetCalculated();
                                    m_Data.CalculateAnchor(position, PETools.PEUtil.MainCamTransform.position, -PETools.PEUtil.MainCamTransform.forward);
                                    FaceDirection(m_Data.GetAnchorDir());
                                }
                            }
                            StopMove();
                            m_Data.ResetCalculated();
                        }
                        else
                        {
                            if (_IsForwardBlock || Stucking(1.0f))
                            {
                                if(_IsInSpSence)
                                    m_Data.ResetCalculatedDir();

                                m_Data.GetCanMoveDirtion(entity, 30.0f);
                                if (m_Data.GetAnchorDir() != Vector3.zero)
                                {
                                    m_Data.ResetCalculated();
                                    m_Data.CalculateAnchor(position, PETools.PEUtil.MainCamTransform.position, -PETools.PEUtil.MainCamTransform.forward);
                                    FaceDirection(m_Data.GetAnchorDir());
                                }
                                StopMove();
                            }
                            else
                            {
                                Vector3 followPos = m_Data.GetFollowPosition(tr.trans, targetVelocity);
                                Vector3 speedPos = _underBlock ? tr.trans.position : followPos;
                                SpeedState speed = m_Data.CalculateSpeedState(PEUtil.SqrMagnitudeH(position, speedPos));
                                // if (speed == SpeedState.Walk && _needAvoid) speed = SpeedState.Run;
                                if (IsReached(position, tr.trans.position + m_Data.Anchor, false))
                                {
                                    //on the bridgew
                                    Vector3 pos0 = tr.trans.position + m_Data.Anchor;
                                    float Y = pos0.y - position.y;
                                    float dy = Mathf.Abs(Y);
                                    if (dy >= 1.0f)
                                    {
                                        if (Y > 0)
                                        {
                                            pos0 = PETools.PEUtil.CorrectionPostionToStand(pos0);
                                        }
                                        else
                                        {
                                            pos0 = PETools.PEUtil.CorrectionPostionToStand(pos0, 6.0f, 8.0f);
                                        }

                                        SetPosition(pos0);
                                        m_Data.ResetCalculated();
                                        m_Data.CalculateAnchor(position, PETools.PEUtil.MainCamTransform.position, -PETools.PEUtil.MainCamTransform.forward);
                                    }
                                }
                                MoveToPosition(followPos, speed);
                            }
                        }
                    }
                    else
                    {
                        m_Data.ResetCalculated();
                        m_Data.CalculateAnchor(position, tr.trans.position, _AvoidDir);
                        //Vector3 pos = _AvoidDir * 5.0f + position;
                        MoveDirection(_AvoidDir, SpeedState.Run);
                    }

                }
                else
                {
                    StopMove();
                }
            }

            //有怪物时，NPC不在载具上即退出此行为-切换为攻击
            if (attackEnemy != null && !IsOnVCCarrier && !IsOnRail)
                return BehaveResult.Failure;

            return BehaveResult.Running;
        }
    }

    [BehaveAction(typeof(BTNpcFollowerEat), "NpcFollowerEat")]
    public class BTNpcFollowerEat : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public string eatAnim;
        }

        Data m_Data;
        ItemAsset.ItemObject mEatItem;

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (!NpcEatDb.CanEat(entity))
                return BehaveResult.Failure;

            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!Enemy.IsNullOrInvalid(attackEnemy))
                return BehaveResult.Failure;

            //吃到了合适的属性值或者没有合适的药品时停止
            if (!NpcEatDb.IsContinueEat(entity, out mEatItem))
                return BehaveResult.Failure;

            if (GetCdByProtoId(mEatItem.protoId) >= PETools.PEMath.Epsilon)
                return BehaveResult.Failure;

            if (mEatItem != null)
            {

                UseItem(mEatItem);

                if (CanDoAction(PEActionType.Eat) && !IsActionRunning(PEActionType.Eat))
                {
                    PEActionParamS param = PEActionParamS.param;
                    param.str = m_Data.eatAnim;
                    DoAction(PEActionType.Eat, param);
                }
            }

            return BehaveResult.Running;
        }

        void Reset(Tree sender)
        {
            EndAction(PEActionType.Eat);
        }
    }

    [BehaveAction(typeof(BTNpcFollowerCut), "NpcFollowerCut")]
    public class BTNpcFollowerCut : BTNormal
    {
        class Data
        {
            //			[BehaveAttribute]
            //			public float OnceCutTime;


            public float mStartCutTime;
            public bool HasActive;
            public Action_Fell mActionFell;
        }
        Data m_Data = new Data();

        GlobalTreeInfo treeInfo;

        Vector3 cutPos;
        //Vector3 mayCenter;
        Vector3 GetStandPos(GlobalTreeInfo _GlobaltreeInfo, PeEntity player, PeEntity Npc)
        {
            if (_GlobaltreeInfo._treeInfo == null)
                return Vector3.zero;

            if (_GlobaltreeInfo.HasCreatPickPos)
            {
                Vector3 cutPos;
                if (_GlobaltreeInfo.AddCutter(Npc.Id, out cutPos))
                {
                    return cutPos;
                }
                return Vector3.zero;
            }
            else
            {
                GameObject obj = null;
                if (null != RSubTerrainMgr.Instance)
                {
                    if (RSubTerrainMgr.HasCollider(_GlobaltreeInfo._treeInfo.m_protoTypeIdx))
                    {
                        obj = RSubTerrainMgr.Instance.GlobalPrototypeColliders[_GlobaltreeInfo._treeInfo.m_protoTypeIdx];
                    }
                }
                else if (null != LSubTerrainMgr.Instance)
                {
                    if (LSubTerrainMgr.HasCollider(_GlobaltreeInfo._treeInfo.m_protoTypeIdx))
                    {
                        obj = LSubTerrainMgr.Instance.GlobalPrototypeColliders[_GlobaltreeInfo._treeInfo.m_protoTypeIdx];
                    }
                }

                if (obj == null)
                    return Vector3.zero;


                CapsuleCollider collider = obj.GetComponent<CapsuleCollider>();
                if (collider == null)
                    return Vector3.zero;

                Vector3 loacl = new Vector3(_GlobaltreeInfo._treeInfo.m_widthScale, _GlobaltreeInfo._treeInfo.m_heightScale, _GlobaltreeInfo._treeInfo.m_widthScale);
                Vector3 c = new Vector3(collider.center.x * loacl.x, collider.center.y * loacl.y, collider.center.z * loacl.z);
                _GlobaltreeInfo.TreeCapCenterPos = new Vector3(_GlobaltreeInfo.WorldPos.x + c.x, _GlobaltreeInfo.WorldPos.y, _GlobaltreeInfo.WorldPos.z + c.z);
                Vector3 direction = (player.peTrans.position - _GlobaltreeInfo.TreeCapCenterPos).normalized;
                if (_GlobaltreeInfo.CreatCutPos(_GlobaltreeInfo.TreeCapCenterPos, direction, collider.radius))
                {
                    Vector3 cutPos;
                    if (_GlobaltreeInfo.AddCutter(Npc.Id, out cutPos))
                    {
                        return cutPos;
                    }
                    return Vector3.zero;
                }
                return Vector3.zero;

            }
        }

        float endCutWaitStartTime;
        float END_TIME = 3.0f;
        BehaveResult Init(Tree sender)
        {
            //			if (!GetData<Data>(sender, ref m_Data))
            //				return BehaveResult.Failure;

            if (!IsNpc || !IsNpcFollower)
                return BehaveResult.Failure;

            if (!Enemy.IsNullOrInvalid(attackEnemy))
                return BehaveResult.Failure;

            if (!IsNpcFollowerCut)
                return BehaveResult.Failure;


            if (entity.NpcCmpt.EqSelect.SetSelectObjsTool(entity, ItemAsset.EeqSelect.tool))
            {
                ItemAsset.SelectItem.EquipByObj(entity, entity.NpcCmpt.EqSelect.GetBetterToolObj());
            }

            //energy
            if (entity.NpcCmpt.EqSelect.SetSelectObjsEnergy(entity, ItemAsset.EeqSelect.energy))
            {
                ItemAsset.SelectItem.EquipByObj(entity, entity.NpcCmpt.EqSelect.GetBetterEnergyObj());
            }

            if (entity.motionEquipment == null || entity.motionEquipment.axe == null)
            {
                EndFollowerCut();
                return BehaveResult.Failure;
            }

            if (entity.motionEquipment.axe is PEChainSaw)
            {
                if (GetAttribute(AttribType.Energy) < PETools.PEMath.Epsilon)
                {
                    EndFollowerCut();
                    return BehaveResult.Failure;
                }
            }

            treeInfo = NpcMaster.peEntity.aliveEntity.treeInfo;
            if (treeInfo == null)
            {
                EndFollowerCut();
                return BehaveResult.Failure;
            }

            m_Data.mActionFell = SetGlobalTreeInfo(treeInfo);
            if (m_Data.mActionFell == null || !m_Data.mActionFell.CanDoAction(null))
            {
                EndFollowerCut();
                return BehaveResult.Failure;
            }

            cutPos = GetStandPos(treeInfo, NpcMaster.peEntity, entity);
            if (cutPos == Vector3.zero)
            {
                EndFollowerCut();
                return BehaveResult.Failure;
            }

            m_Data.mStartCutTime = Time.time;
            m_Data.HasActive = false;
            entity.NpcCmpt.AddTalkInfo(ENpcTalkType.Follower_cut, ENpcSpeakType.TopHead, true);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (SkEntitySubTerrain.Instance.GetTreeHP(m_Data.mActionFell.treeInfo.WorldPos) <= 0 || !Enemy.IsNullOrInvalid(attackEnemy)
               || entity.motionEquipment.axe == null || entity.NpcCmpt.servantCallback
               || (entity.motionEquipment.axe != null && entity.motionEquipment.axe is PEChainSaw && entity.GetAttribute(AttribType.Energy) < PETools.PEMath.Epsilon)
               )
            {
                if (endCutWaitStartTime == 0)
                    endCutWaitStartTime = Time.time;

                if (Time.time - endCutWaitStartTime >= END_TIME)
                {
                    endCutWaitStartTime = 0;
                    if (entity.motionEquipment.axe != null && entity.motionEquipment.axe is PEChainSaw)
                    {
                        ItemAsset.SelectItem.TakeOffEquip(entity);
                    }
                    return BehaveResult.Failure;
                }
                //UseTool(false);
                EndAction(PEActionType.Fell);
                EndFollowerCut();
                ActiveWeapon(entity.motionEquipment.axe, false);
                return BehaveResult.Running;
            }



            if (m_Data.HasActive)
            {
                FaceDirection(treeInfo.TreeCapCenterPos - position);
                DoAction(PEActionType.Fell);

            }
            Debug.DrawRay(treeInfo.TreeCapCenterPos, Vector3.up * 10.0f, Color.red);
            Debug.DrawRay(cutPos, Vector3.up * 10.0f, Color.blue);
            Debug.DrawRay(treeInfo.WorldPos, Vector3.up * 10.0f, Color.yellow);

            if (IsReached(position, cutPos, false, 1.0f) && !m_Data.HasActive)
            {
                StopMove();
                ActiveWeapon(entity.motionEquipment.axe, true);
                FaceDirection(treeInfo.TreeCapCenterPos - position);
                DoAction(PEActionType.Fell);
                m_Data.HasActive = true;
            }
            else
            {
                if (Stucking())
                {
                    StopMove();
                    FaceDirection(treeInfo.TreeCapCenterPos - position);
                    ActiveWeapon(entity.motionEquipment.axe, true);
                    m_Data.HasActive = true;
                }

                if (!m_Data.HasActive)
                    MoveToPosition(cutPos, SpeedState.Run, false);
            }
            return BehaveResult.Running;
        }

        void Reset(Tree sender)
        {
            //EndFollowerCut();
        }
    }


    [BehaveAction(typeof(BTNpcFollowerGather), "NpcFollowerGather")]
    public class BTNpcFollowerGather : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float GatherRadius;
            [BehaveAttribute]
            public float Probability;


            public float mStartTime;
            public Action_Gather Gather;
        }
        Data m_Data;

        GlobalTreeInfo glassInfo;
        List<GlobalTreeInfo> mGlobalTreeInfos;
        bool reached = false;

        void GetGlobalTreeInfoForGather()
        {
            if (null != LSubTerrainMgr.Instance)
                mGlobalTreeInfos = LSubTerrainMgr.Picking(position, transform.forward, false, m_Data.GatherRadius, 360f);
            else if (null != RSubTerrainMgr.Instance)
                mGlobalTreeInfos = RSubTerrainMgr.Picking(position, transform.forward, false, m_Data.GatherRadius, 360f);
        }

        bool DetectionTreeIndex(GlobalTreeInfo _treeInfo, PeEntity _entity)
        {
            for (int i = 0; i < NpcMaster.mFollowers.Length; i++)
            {
                if (NpcMaster.mFollowers[i] != null && NpcMaster.mFollowers[i].GatherprotoTypeIdx == _treeInfo._treeInfo.m_protoTypeIdx)
                {
                    return false;
                }
            }
            _entity.NpcCmpt.GatherprotoTypeIdx = _treeInfo._treeInfo.m_protoTypeIdx;
            return true;
        }

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (!IsNpc || !IsNpcFollower || !Enemy.IsNullOrInvalid(attackEnemy))
                return BehaveResult.Failure;

            if (Random.value > m_Data.Probability)
                return BehaveResult.Failure;

            GetGlobalTreeInfoForGather();
            if (mGlobalTreeInfos == null || mGlobalTreeInfos.Count <= 0)
                return BehaveResult.Failure;

            glassInfo = mGlobalTreeInfos[Random.Range(0, mGlobalTreeInfos.Count)];
            if (glassInfo == null)
                return BehaveResult.Failure;

            if (!DetectionTreeIndex(glassInfo, entity))
                return BehaveResult.Failure;

            m_Data.Gather = SetGlobalGatherInfo(glassInfo);
            if (m_Data.Gather == null && !m_Data.Gather.CanDoAction(null))
                return BehaveResult.Failure;

            reached = false;
            entity.NpcCmpt.AddTalkInfo(ENpcTalkType.Follower_Gather, ENpcSpeakType.TopHead, true);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (SkEntitySubTerrain.Instance.GetTreeHP(glassInfo.WorldPos) <= PETools.PEMath.Epsilon || !Enemy.IsNullOrInvalid(attackEnemy))
            {
                EndAction(PEActionType.Gather);
                return BehaveResult.Failure;
            }

            if (reached)
                DoAction(PEActionType.Gather);

            if (IsReached(position, glassInfo.WorldPos, false, 1.0f) && !reached)
            {
                StopMove();
                reached = true;
                m_Data.mStartTime = Time.time;
            }
            else
            {
                if (Stucking())
                {
                    if (entity.viewCmpt == null || !entity.viewCmpt.hasView)
                        return BehaveResult.Failure;
                }
                else
                {
                    reached = true;
                }


                MoveToPosition(glassInfo.WorldPos, SpeedState.Run);
            }
            return BehaveResult.Running;
        }

        void Reset(Tree sender)
        {
            if (entity.NpcCmpt != null)
            {
                entity.NpcCmpt.GatherprotoTypeIdx = -99;
                entity.NpcCmpt.RmoveTalkInfo(ENpcTalkType.Follower_Gather);
                glassInfo = null;
            }
        }
    }


    [BehaveAction(typeof(BTNpcFollowerLoot), "NpcFollowerLoot")]
    public class BTNpcFollowerLoot : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float LootRadius;

            public float lootTime = 1.0f;
            public float startLootTime;

        }
        Data m_Data = null;

        PeEntity mLootEntity;
        ItemDropPeEntity mItemDrop;
        bool bReached = false;
        BehaveResult Init(Tree sender)
        {
            return BehaveResult.Failure;

            //if (!GetData<Data>(sender, ref m_Data))
            //    return BehaveResult.Failure;

            //if (!Enemy.IsNullOrInvalid(attackEnemy))
            //    return BehaveResult.Failure;

            //PEActionParamS param = PEActionParamS.param;
            //param.str = "Gather";
            //if (!CanDoAction(PEActionType.Leisure, param))
            //    return BehaveResult.Failure;

            //mLootEntity = LootItemDropPeEntity.GetLootEntity(position, m_Data.LootRadius);
            //if (mLootEntity == null)
            //    return BehaveResult.Failure;

            //mItemDrop = mLootEntity.GetComponent<ItemDropPeEntity>();
            //if (mItemDrop == null || !mItemDrop.NpcCanFetchAll(entity.NpcCmpt.NpcPackage))
            //    return BehaveResult.Failure;

            //bReached = false;
            //entity.NpcCmpt.AddTalkInfo(ENpcTalkType.Follower_Loot, ENpcSpeakType.TopHead, false);
            //LootItemDropPeEntity.RemovePeEntity(mLootEntity);
            //return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (bReached && Time.time - m_Data.startLootTime > m_Data.lootTime)
            {
                mLootEntity = null;
                bReached = false;
                EndAction(PEActionType.Leisure);
            }

            if (!bReached && mLootEntity == null)
            {
                if (!Enemy.IsNullOrInvalid(attackEnemy))
                    return BehaveResult.Failure;

                List<PeEntity> entities = LootItemDropPeEntity.GetEntities(position, m_Data.LootRadius);
                if (entities.Count > 0)
                {
                    mLootEntity = LootItemDropPeEntity.GetLootEntity(position, m_Data.LootRadius);
                    mItemDrop = mLootEntity.GetComponent<ItemDropPeEntity>();
                    if (mItemDrop == null || !mItemDrop.NpcCanFetchAll(entity.NpcCmpt.NpcPackage))
                        return BehaveResult.Failure;

                    LootItemDropPeEntity.RemovePeEntity(mLootEntity);
                }
                else
                {
                    return BehaveResult.Failure;
                }
            }

            if (!bReached)
            {
                if (IsReached(position, mLootEntity.peTrans.position, false, 1.0f) || Stucking())
                {
                    StopMove();
                    FaceDirection(mLootEntity.peTrans.position - position);

                    PEActionParamS param = PEActionParamS.param;
                    param.str = "Gather";
                    DoAction(PEActionType.Leisure, param);
                    mItemDrop.NpcFetchAll(entity.NpcCmpt.NpcPackage);
                    m_Data.startLootTime = Time.time;
                    bReached = true;
                }
                else
                {
                    MoveToPosition(mLootEntity.peTrans.position, SpeedState.Run);
                }
            }

            return BehaveResult.Running;
        }

        void Reset(Tree sender)
        {
        }
    }


}