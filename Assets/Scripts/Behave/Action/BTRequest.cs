using UnityEngine;
using System.Collections;
using Pathea;
using PETools;
using PEIK;

namespace Behave.Runtime
{
    [BehaveAction(typeof(BRHasRequest), "RHasRequest")]
    public class BRHasRequest : BTNormal
    {
        BehaveResult Tick(Tree sender)
        {
            if (hasAnyRequest)
                return BehaveResult.Success;
            else
                return BehaveResult.Failure;
        }
    }

    [BehaveAction(typeof(BRAnimation), "RAnimation")]
    public class BRAnimation : BTNormal
    {
        RQAnimation m_Animation;
        float m_StartTime;

        BehaveResult Init(Tree sender)
        {
            m_Animation = GetRequest(EReqType.Animation) as RQAnimation;
            if (m_Animation == null)
                return BehaveResult.Failure;

			if(!m_Animation.play)
			{
				SetBool(m_Animation.animName, false);
				RemoveRequest(m_Animation);
				return BehaveResult.Failure;
			}

            m_StartTime = Time.time;
			SetBool(m_Animation.animName, true);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
			SetNpcAiType(ENpcAiType.RAnimation);
			m_Animation = GetRequest(EReqType.Animation) as RQAnimation;

			if(m_Animation == null)
				return BehaveResult.Failure;

			if(m_Animation != null && !m_Animation.CanRun())		
				return BehaveResult.Failure;

            if (!entity.hasView)
            {
                SetBool(m_Animation.animName, false);
                RemoveRequest(m_Animation);
                return BehaveResult.Success;
            }

			if( entity.animCmpt == null || entity.animCmpt.animator == null)
				return BehaveResult.Failure;

	
			//play
			if (m_Animation != null && m_Animation.play)
            {
				if (m_Animation.animTime >PETools.PEMath.Epsilon && Time.time - m_StartTime > m_Animation.animTime)
                {
                    RemoveRequest(m_Animation);
                    return BehaveResult.Success;
                }

				if(m_Animation.animTime < PETools.PEMath.Epsilon && !entity.animCmpt.animator.GetBool(m_Animation.animName))
				{
					RemoveRequest(m_Animation);
					return BehaveResult.Success;
				}
            }
           
			//stop
			if (m_Animation != null && !m_Animation.play)
			{
				SetBool(m_Animation.animName, false);
				RemoveRequest(m_Animation);
				return BehaveResult.Success;
			}

            return BehaveResult.Running;
        }
    }

    [BehaveAction(typeof(BRIdle), "RIdle")]
    public class BRIdle : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public string idle;
			[BehaveAttribute]
			public int Type;
            [BehaveAttribute]
            public float minTime;
            [BehaveAttribute]
            public float maxTime;
            [BehaveAttribute]
            public float startTime;
            [BehaveAttribute]
            public float endTime;
            [BehaveAttribute]
            public string[] relax;

            float m_LastRelaxTime;
            float m_CurRelaxTime;

            public bool m_End = false;
            public float m_StartTime;
            public float m_EndTime;
            public RQIdle m_Idle;
			public RQIdle.RQidleType RqType {get {return (RQIdle.RQidleType)Type;}}

            public bool CanRelax()
            {
                return relax.Length > 0;
            }

            public bool CheckRelax()
            {
                if (Time.time - m_LastRelaxTime > m_CurRelaxTime)
                {
                    m_LastRelaxTime = Time.time;
                    m_CurRelaxTime = Random.Range(minTime, maxTime);
                    return true;
                }

                return false;
            }

            public string RandomRelax()
            {
                return relax[Random.Range(0, relax.Length)];
            }

            public void InitRelax()
            {
                m_LastRelaxTime = 0.0f;
                m_CurRelaxTime = Random.Range(minTime, maxTime);
            }


		

        }

		void InitIdletypeData()
		{
			if(m_Data.m_End)
				return ;

			switch(m_Data.RqType)
			{
				case RQIdle.RQidleType.BeCarry:
				{
					if (entity.biologyViewCmpt != null)
						entity.biologyViewCmpt.ActivateCollider(false);
					
					if (entity.motionMgr != null)
					entity.motionMgr.FreezePhyState(GetType(), true);
					
					if (entity.biologyViewCmpt != null)
						entity.biologyViewCmpt.ActivateInjured(false);
					
					if (entity.enityInfoCmpt != null)
                    {
                        entity.enityInfoCmpt.ShowName(false);
                        entity.enityInfoCmpt.ShowMissionMark(false);
                    }
						
				}
					break;
				case RQIdle.RQidleType.Idle:break;
				case RQIdle.RQidleType.InjuredRest:
				{
					if(entity.biologyViewCmpt != null)
						entity.biologyViewCmpt.ActivateInjured(false);
					
					if(entity.target != null)
						entity.target.SetEnityCanAttack(false);
					
					if(entity.motionMgr != null)
					entity.motionMgr.FreezePhyState(GetType(), true);
					
					if (entity.enityInfoCmpt != null){
                        entity.enityInfoCmpt.ShowName(false);
                        entity.enityInfoCmpt.ShowMissionMark(false);
                    }
						
					
					SetBool("BeCarry", false);
			}
				break;
				case RQIdle.RQidleType.InjuredSit:
				{
					entity.motionMgr.SetMaskState(PEActionMask.Cutscene, true);
				}
					break;
				case RQIdle.RQidleType.InjuredSitEX:break;
                case RQIdle.RQidleType.Lie:
                    {
                        if (entity.biologyViewCmpt != null)
                            entity.biologyViewCmpt.ActivateInjured(false);

                        if (entity.target != null)
                            entity.target.SetEnityCanAttack(false);
                    }
                    break;
				default:break;
			}
			return ;
		}

        Data m_Data;
        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            m_Data.m_Idle = GetRequest(EReqType.Idle) as RQIdle;

            if (m_Data.m_Idle == null)
                return BehaveResult.Failure;

            if (!m_Data.m_Idle.CanRun())
                return BehaveResult.Failure;

            if (!m_Data.m_Idle.state.Equals(m_Data.idle))
                return BehaveResult.Failure;

            StopMove();
            m_Data.m_End = false;
            m_Data.m_StartTime = Time.time;
            m_Data.m_EndTime = Time.time;
            m_Data.InitRelax();
            SetBool(m_Data.m_Idle.state, true);
			//InitIdletypeData();
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

			RQIdle idle0 = GetRequest(EReqType.Idle) as RQIdle;
			if(idle0 != null && !idle0.CanRun())
				return BehaveResult.Success;


			InitIdletypeData();
			if (!GameConfig.IsMultiMode && IsOnVCCarrier)
				GetOff();

			if (m_Data.m_Idle != null)
            {
                if (Time.time - m_Data.m_StartTime > m_Data.startTime)
                {
                    RQIdle idle = GetRequest(EReqType.Idle) as RQIdle;
					if (idle == null ||  !m_Data.m_Idle.Equals(idle))//!m_Data.m_Idle.CanRun() ||*/
                    {
                        if (!m_Data.m_End)
                        {
                            SetBool(m_Data.m_Idle.state, false);
                            m_Data.m_EndTime = Time.time;

                            m_Data.m_End = true;
							if(m_Data.RqType == RQIdle.RQidleType.InjuredRest)//m_Data.m_Idle.state == "InjuredRest")
							{
								if(entity.target != null)
									entity.target.SetEnityCanAttack(true);
							}

                            if (m_Data.RqType == RQIdle.RQidleType.InjuredSit)//m_Data.m_Idle.state == "InjuredSit")
                            {
                                entity.motionMgr.SetMaskState(PEActionMask.Cutscene, false );
                            }

                            if (m_Data.RqType == RQIdle.RQidleType.Lie)
                            {
                                if (entity.biologyViewCmpt != null)
                                    entity.biologyViewCmpt.ActivateInjured(true);

                                if (entity.target != null)
                                    entity.target.SetEnityCanAttack(true);
                            }
                        }
                        else
                        {
                            if (Time.time - m_Data.m_EndTime > m_Data.endTime)
                            {
								if(m_Data.RqType == RQIdle.RQidleType.InjuredRest)
								{
									if(entity.motionMgr != null)
										entity.motionMgr.FreezePhyState(GetType(), false);

									if (entity.enityInfoCmpt != null){
                                        entity.enityInfoCmpt.ShowName(true);
                                        entity.enityInfoCmpt.ShowMissionMark(true);
										entity.peTrans.SetModel(existent);
                                    }
										
								}

                                RemoveRequest(m_Data.m_Idle);
                                m_Data.m_Idle = null;
                                return BehaveResult.Success;
                            }
							StopMove();
                        }
                    }
                    else
                    {
                        if (m_Data.CanRelax() && m_Data.CheckRelax())
                        {
                            SetBool(m_Data.RandomRelax(), true);
                        }
                    }
                }
            }

            return BehaveResult.Running;
        }

        void Reset(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return;

			RQIdle idle0 = GetRequest(EReqType.Idle) as RQIdle;
			if(idle0 != null && !idle0.CanRun())
				return ;

			if(idle0 == null && m_Data.m_Idle != null)
			{
				if(GetBool(m_Data.m_Idle.state))
				{
					SetBool(m_Data.m_Idle.state,false);
				}

			}

            m_Data.m_Idle = null;
        }
    }

    [BehaveAction(typeof(BRMoveToPoint), "RMoveToPoint")]
    public class BRMoveToPoint : BTNormal
    {
        Vector3 m_Position;

        BehaveResult Init(Tree sender)
        {
            RQMoveToPoint request = GetRequest(EReqType.MoveToPoint) as RQMoveToPoint;
            if (request == null || !request.CanRun())
                return BehaveResult.Failure;

            SetNpcUpdateCampsite(false);
            MoveToPosition(request.position, request.speedState);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            RQMoveToPoint request = GetRequest(EReqType.MoveToPoint) as RQMoveToPoint;
            if (request == null || !request.CanRun())
            {
                if (request == null && StroyManager.Instance != null)
                    StroyManager.Instance.EntityReach(entity, false);
                return BehaveResult.Failure;
            }

			SetNpcAiType(ENpcAiType.RMoveToPoint);
			if(AskStop)
			{
				StopMove();
                if (null != StroyManager.Instance)
                    FaceDirection(StroyManager.Instance.GetPlayerPos() - position);
				return BehaveResult.Running;
			}

			if (PETools.PEUtil.MagnitudeH(position, request.position) < request.stopRadius)
            {
                RemoveRequest(request);
                //request.ReachPoint(entity);
                MoveToPosition(Vector3.zero);
                return BehaveResult.Success;
            }
            else
            {

	          if (Stucking(5.0f))
	          {
	              if (!request.isForce)
	                  request.Addmask(EReqMask.Stucking);
	              else
	              {

					SetPosition(request.position);					
	                RemoveRequest(request);
                    request.ReachPoint(entity);
	                return BehaveResult.Success;
	              }
	          }
            }

            MoveToPosition(request.position, request.speedState);
            return BehaveResult.Running;
        }

        void Reset(Tree sender)
        {
            SetNpcUpdateCampsite(true);
        }
    }

	[BehaveAction(typeof(BRRTalkMove), "RTalkMove")]
	public class BRRTalkMove : BTNormal
	{
		Vector3 m_Position;
		
		BehaveResult Init(Tree sender)
		{
			RQTalkMove request = GetRequest(EReqType.TalkMove) as RQTalkMove;
			if (request == null || !request.CanRun())
				return BehaveResult.Failure;

			MoveToPosition(request.position, request.speedState);
			return BehaveResult.Running;
		}
		
		BehaveResult Tick(Tree sender)
		{	
			RQTalkMove request = GetRequest(EReqType.TalkMove) as RQTalkMove;
			if (request == null || !request.CanRun())
				return BehaveResult.Failure;

			
			if (PETools.PEUtil.MagnitudeH(position, request.position) < request.stopRadius)
			{
				RemoveRequest(request);
				request.ReachPoint(entity);
				MoveToPosition(Vector3.zero);
				return BehaveResult.Success;
			}
			else
			{
				if (Stucking(5.0f))
				{
					if (!request.isForce)
						request.Addmask(EReqMask.Stucking);
					else
					{
						
						SetPosition(request.position);					
						RemoveRequest(request);
						request.ReachPoint(entity);
						return BehaveResult.Success;
					}
				}
			}
			
			MoveToPosition(request.position, request.speedState);
			return BehaveResult.Running;
		}
	}

    [BehaveAction(typeof(BRFollowPath), "RFollowPath")]
    public class BRFollowPath : BTNormal
    {
        int m_Index;

        int GetClosetPointIndex(RQFollowPath request, Vector3 position)
        {
            int tmpIndex = -1;
            float tmpSqrMagnitude = Mathf.Infinity;
            for (int i = 0; i < request.path.Length; i++)
            {
                float sqrMagnitude = PETools.PEUtil.SqrMagnitudeH(request.path[i], position);
                if (sqrMagnitude < tmpSqrMagnitude)
                {
                    tmpSqrMagnitude = sqrMagnitude;
                    tmpIndex = i;
                }
            }

            return tmpIndex;
        }

		int GetBetterIndex(RQFollowPath request,Vector3 _position)
		{
			int index0 = 0;
			float dis0 = PEUtil.Magnitude(request.path[index0], _position);
			for(int i=index0 +1;i<request.path.Length;i++)
			{
				float dis1 = PEUtil.Magnitude(request.path[i], _position);
				if(dis0 >= dis1)
				{
					index0 = i;
					dis0 = dis1;
				}


			}

			if(index0 >= request.path.Length - 1)
				return index0;

			int index1 = index0 + 1;
			float distance0 = PEUtil.Magnitude(request.path[index0], request.path[index1]);
			float distance1 = PEUtil.Magnitude(request.path[index0], _position);
			float distance2 = PEUtil.Magnitude(request.path[index1], _position);

			//距离过近会导致NPc不移动
            float  d=  PETools.PEUtil.SqrMagnitudeH(request.path[index0], _position);
            if (d < 0.0625f)// 0.25*0.25
				return index1;

			if(distance0 < distance1 || distance0 < distance2)
				return index0;
			else
				return index1;
		}



		//Hide
		float hideStarTime = 0.0f;
		float HIDE_TIME = 1.0f;
		FindHidePos mfind;
		Vector3 mdir;

        BehaveResult Init(Tree sender)
        {
            if (attackEnemy != null)
                return BehaveResult.Failure;

            RQFollowPath request = GetRequest(EReqType.FollowPath) as RQFollowPath;
            if (request == null || !request.CanRun())
                return BehaveResult.Failure;

            m_Index = GetClosetPointIndex(request, position);
            if (m_Index < 0 || m_Index >= request.path.Length)
                return BehaveResult.Failure;

			mfind = new Pathea.FindHidePos(8.0f,false);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
			SetNpcAiType(ENpcAiType.RFollowPath);
            RQFollowPath request = GetRequest(EReqType.FollowPath) as RQFollowPath;
            if (request == null || !request.CanRun())
                return BehaveResult.Failure;

            if (m_Index < 0 || m_Index >= request.path.Length)
                return BehaveResult.Failure;

            bool _recourse = NpcTypeDb.CanRun(NpcCmdId, ENpcControlType.Recourse);
            if (_recourse && entity.target != null) entity.target.SetEnityCanAttack(false);
  
            if (_recourse && entity.NpcCmpt.HasEnemyLocked())
			{
				if(Time.time -hideStarTime > HIDE_TIME)
				{
					mdir= mfind.GetHideDir(PeCreature.Instance.mainPlayer.position,position,Enemies);
					hideStarTime = Time.time;
				}
				
				Vector3 dirPos = position + mdir.normalized * 10.0f;
				if(mfind.bNeedHide)
				{
					MoveToPosition(dirPos,SpeedState.Run);
				}
				else
				{
					StopMove();
					FaceDirection(PeCreature.Instance.mainPlayer.position - position);
				}
				
				//吃到了合适的属性值或者没有合适的药品时停止
				ItemAsset.ItemObject mEatItem;
				if(NpcEatDb.IsContinueEat(entity,out mEatItem))
				{
					if(entity.UseItem.GetCdByItemProtoId(mEatItem.protoId) < PETools.PEMath.Epsilon)
					{
						UseItem(mEatItem);
					}
					
				}
				return BehaveResult.Running;
			}

			//任务需求站定等待
			if(AskStop)
			{
				StopMove();
                if (null != StroyManager.Instance)
                    FaceDirection(StroyManager.Instance.GetPlayerPos() - position);
				return BehaveResult.Running;
			}

			m_Index = GetBetterIndex(request,position);
			if (m_Index == request.path.Length -1)
			{
				if(IsReached(request.path[m_Index],position,true,2.0f))
				{
					if (request.isLoop)
						m_Index = 0;
					else
					{
						//m_Index = request.path.Length - 1;
						RemoveRequest(request);

                        bool _recourse0 = NpcTypeDb.CanRun(NpcCmdId, ENpcControlType.Recourse);
                        if (_recourse0 && entity.target != null) entity.target.SetEnityCanAttack(true);

						return BehaveResult.Success;
					}
				}

			}

			if(Stucking(5.0f))
				SetPosition(request.path[m_Index]);

            MoveToPosition(request.path[m_Index], request.speedState);
            return BehaveResult.Running;
        }
    }

	
	[BehaveAction(typeof(BRFollowTarget), "RFollowTarget")]
	public class BRFollowTarget : BTNormal
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
			
			public Vector3 Anchor
			{get{return m_Anchor;}}
			
			Vector3 m_Anchor;
			//Vector3 m_LastAnchor;
			Vector3 m_LastPatrol;
			bool m_Reached;
			bool m_Calculated;
			SpeedState m_SpeedState;
			
			//static float minWalkRadius = 0.0f;
			static float maxWalkRadius = 8.0f;
			static float minRunRadius = 8.0f;
			static float maxRunRadius = 16.0f;
			static float minSprintRadius = 16.0f;
			//static float maxSprintRadius = 32.0f;
			
            public float  startPralTime = 0.0f;
            public float  waitPralTime = 8.0f;

            public float startFindTime = 0.0f;
            public float waitFindTime = 3.0f;
            public float StandRadiu = 2.0f;

			public void CalculateAnchor(Vector3 pos, Vector3 center,Vector3 dir)
			{
				if(!m_Calculated)
				{
					m_Reached = false;
					m_Calculated = true;
					//Vector3 v = Vector3.ProjectOnPlane(pos - center, Vector3.up);
                    m_Anchor = PEUtil.GetRandomPosition(Vector3.zero, dir, firRadius, sndRadius, -90.0f, 90.0f);
					m_Anchor = new Vector3(m_Anchor.x, 0.0f, m_Anchor.z);
				}
			}

			public void ResetCalculated()
			{
				m_Calculated = false;
			}
			
			public bool IsOutside(Vector3 pos, Transform target)
			{
				float sqrDistanceH = PEUtil.Magnitude(pos, target.position + m_Anchor);
				return sqrDistanceH > thdRadius;
			}

            public bool InRadius(Vector3 pos, Vector3 targetCentor, float r, float R, bool is3D)
            {
                float D = PETools.PEUtil.Magnitude(pos, targetCentor, is3D);
                return D > r && D <= R;
            }

			
			public bool isReached(Vector3 pos, Transform target,bool is3D)
			{
				float sqrDistanceH = PEUtil.Magnitude(pos, target.position + m_Anchor);
				if (sqrDistanceH < 1.0f)
				{
					m_Reached = true;
					m_Calculated = false;
				}
				
				return m_Reached;
			}
			
			public Vector3 GetFollowPosition(Transform target, Vector3 velocity)
			{
				if(CheckFirst())
				{
					//m_LastAnchor = target.position;
				}
				
				//return m_LastAnchor + m_Anchor /*+ velocity.normalized * m_Anchor.magnitude*/;
				return target.position + m_Anchor /*+ velocity.normalized * m_Anchor.magnitude*/;
			}
			
			public Vector3 GetPatrolPosition(Transform target)
			{
				if (CheckSecond())
				{
					m_LastPatrol = PEUtil.GetRandomPosition(target.position + m_Anchor, firRadius, thdRadius);
				}
				
				return m_LastPatrol;
			}

            Vector3 m_AnchorDir;
            bool m_CalculatedDir = false;
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
			
			/*
             * 0  - 8 walk
             * 8  - 16 run
             * 16 - 32 sprint
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
		
		Vector3 GetFixedPosition(Vector3 pos1, Vector3 direction1,Vector3 pos2, Vector3 direction2,float height)
		{
			Vector3 pos = pos1;
			Vector3 newPosition = PEUtil.GetRandomPosition(pos1 + m_Data.Anchor, direction1, 2, m_Data.sndRadius-0.5f, -90.0f, 90.0f);
			if(PEUtil.CheckPositionNearCliff(newPosition))
			{
				pos = pos2;
				newPosition = PEUtil.GetRandomPosition(pos2 + m_Data.Anchor, direction2, 2, m_Data.sndRadius-0.5f, -90.0f, 90.0f);
			}
			
			RaycastHit hitInfo;
			Ray ray = new Ray(pos,Vector3.up);
			//Target in the hole
			if(Physics.Raycast(ray, out hitInfo, 128.0f, PEConfig.GroundedLayer))
			{
				ray = new Ray(newPosition, Vector3.up);
				if(Physics.Raycast(ray, out hitInfo, 128.0f, PEConfig.GroundedLayer))
				{
					//hole in water
					if(PEUtil.CheckPositionUnderWater(hitInfo.point - Vector3.up))
						return newPosition;
					else
					{
						ray = new Ray(newPosition, Vector3.down);
						if(Physics.Raycast(ray, out hitInfo, 128.0f, PEConfig.GroundedLayer))
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
				if(Physics.Raycast(rayStart, out hitInfo, 256.0f, PEConfig.GroundedLayer))
				{
					if(PEUtil.CheckPositionUnderWater(hitInfo.point))
						return newPosition;
					else
						return hitInfo.point + Vector3.up;
				}
			}
			return Vector3.zero;
		}
		
		BehaveResult Tick(Tree sender)
		{
			if (!GetData<Data>(sender, ref m_Data))
				return BehaveResult.Failure;
			
			RQFollowTarget request = GetRequest(EReqType.FollowTarget) as RQFollowTarget;
			if (request == null)
			{
				if (IsOnVCCarrier && !IsNpcFollower)
					GetOff();

				if (IsOnRail && !IsNpcFollower)
					GetOffRailRoute();


				if(!IsNpcFollower && (IsOnVCCarrier || IsOnRail))
					return BehaveResult.Running;
				else
				    return BehaveResult.Failure;
			}


            //获取跟随目标
			SetNpcAiType(ENpcAiType.RFollowTarget);
			PeEntity e = EntityMgr.Instance.Get(request.id);

            PeEntity dirTarget = request.dirTargetID != 0 ? EntityMgr.Instance.Get(request.dirTargetID) : null;

			if (e == null)
			{
				//lost target
				if (IsOnVCCarrier)
					GetOff();

				if (IsOnRail)
					GetOffRailRoute();

				return BehaveResult.Failure;
			}

            PeTrans trans = e.peTrans;
            Vector3 targetVelocity = Vector3.zero;

            if (entity.isRagdoll || entity.IsDeath())
                return BehaveResult.Running;

            //目标上载具或者轻轨时跟随
			PassengerCmpt passengerTarget = e.passengerCmpt;
			if (passengerTarget != null && passengerTarget.IsOnVCCarrier)
			{
				int index = passengerTarget.drivingController.FindEmptySeatIndex();
				if (index >= 0 && !IsOnVCCarrier)
				{
					GetOn(passengerTarget.drivingController, index);
					MoveToPosition(Vector3.zero);
				}
				else
				{
					//Motion_Move mover = e.GetComponent<Motion_Move>();
					if (e.motionMove != null) targetVelocity = e.motionMove.velocity;
				}
					
			}else if(passengerTarget != null && passengerTarget.IsOnRail)
			{
				Railway.Route route = Railway.Manager.Instance.GetRoute(passengerTarget.railRouteId);
				if(route != null && route.train != null && route.train.HasEmptySeat() && !IsOnRail)
				{
				   GetOn(passengerTarget.railRouteId,entity.Id,true);
				}
				else
				{
					if (e.motionMove != null) targetVelocity = e.motionMove.velocity;
				}
			}
			else
			{
				if (passengerTarget != null && !passengerTarget.IsOnVCCarrier && IsOnVCCarrier)
					GetOff();

				if (passengerTarget != null && !passengerTarget.IsOnRail && IsOnRail)
					GetOffRailRoute();
					
				if (e.motionMove != null) targetVelocity = e.motionMove.velocity;

			}

            bool _IsInSpSence = Pathea.PeGameMgr.IsStory && Pathea.PeGameMgr.IsSingle && Pathea.SingleGameStory.curType != Pathea.SingleGameStory.StoryScene.MainLand;
                //RandomDunGenUtil.IsInDungeon(entity);//(Pathea.PeGameMgr.IsAdventure && RandomDungenMgr.Instance != null && RandomDungenMgrData.dungeonBaseData != null);
            //|| (Pathea.PeGameMgr.IsStory && Pathea.PeGameMgr.IsSingle && Pathea.SingleGameStory.curType != Pathea.SingleGameStory.StoryScene.MainLand)
            //|| (Pathea.PeGameMgr.IsStory && Pathea.PeGameMgr.IsTutorial && Pathea.SingleGameStory.curType != Pathea.SingleGameStory.StoryScene.MainLand)
            //|| (Pathea.PeGameMgr.IsStory && Pathea.PeGameMgr.IsMulti && PlayerNetwork.mainPlayer != null && PlayerNetwork.mainPlayer._curSceneId != (int)Pathea.SingleGameStory.StoryScene.MainLand);

            Vector3 avoidDir0 = Vector3.zero;
            Vector3 avoidDir1 = Vector3.zero;
            Vector3 avoidDir2 = Vector3.zero;
            Vector3 avoidDir3 = Vector3.zero;


            bool _IsnearLeague = entity.NpcCmpt.HasNearleague;
            bool _IsBlockBrush = AiUtil.CheckBlockBrush(entity, out avoidDir0);
            bool _IsnearDig = AiUtil.CheckDig(entity, e, out avoidDir1);
            bool _IsDragging = AiUtil.CheckDraging(entity, out avoidDir2);
            bool _IsNearCreation = AiUtil.CheckCreation(entity, out avoidDir3);

            bool _needAvoid = _IsnearLeague || _IsnearDig || _IsBlockBrush || _IsDragging || _IsNearCreation;
            bool _Isstay = IsNpcFollowerSentry && !_needAvoid;
            bool _IsOnCreation = IsOnVCCarrier || IsOnRail;

            bool _InfirR = m_Data.InRadius(position, trans.position, 0.0f, m_Data.firRadius, true);
            bool _InsndR = m_Data.InRadius(position, trans.position, m_Data.firRadius, m_Data.sndRadius, true);
            //bool _InthdR = m_Data.InRadius(position, trans.position, m_Data.sndRadius, m_Data.thdRadius * 2.0f, true);

            Vector3 avoidplayer = position - trans.trans.position;
            Vector3 avoid0 = avoidDir0 != Vector3.zero ? position - avoidDir0 : Vector3.zero;
            Vector3 avoid1 = avoidDir1 != Vector3.zero ? position - avoidDir1 : Vector3.zero;
            Vector3 avoid2 = avoidDir2 != Vector3.zero ? position - avoidDir2 : Vector3.zero;
            Vector3 avoid3 = avoidDir3 != Vector3.zero ? position - avoidDir3 : Vector3.zero;
            Vector3 _AvoidDir = avoidplayer + avoid0 + avoid1 + avoid2 + avoid3;

            bool _IsForwardBlock = PETools.PEUtil.IsForwardBlock(entity, existent.forward, 2.0f) || !GetBool("OnGround");
            //NPC不在载具上时跟随
            if (!_IsOnCreation)
            {
              
                if (dirTarget != null && (_IsForwardBlock || Stucking()))
                {
                    bool playerReached = m_Data.InRadius(e.position, dirTarget.position, 0.0f, request.tgtRadiu, true);
                    if (playerReached)
                    {
                        Vector3 pos = _IsInSpSence ? dirTarget.position : PETools.PEUtil.GetRandomPositionOnGroundForWander(dirTarget.position, 1.0f, 3.0f);
                        if (pos != Vector3.zero) SetPosition(pos);
                    }
                        

                }

                if(request.targetPos != Vector3.zero && (_IsForwardBlock || Stucking()))
                {
                    bool playerReached = m_Data.InRadius(e.position, request.targetPos, 0.0f, request.tgtRadiu, true);
                    if (playerReached)
                    {
                        Vector3 pos = _IsInSpSence ? request.targetPos : PETools.PEUtil.GetRandomPositionOnGroundForWander(request.targetPos, 1.0f, 3.0f);
                        if (pos != Vector3.zero) SetPosition(pos);
                    }
                        
                }

                //超出跟随距离snd_thd
                if (!_InfirR && !_InsndR)
                {
                    if (GameConfig.IsMultiMode)
                    {
                        if (Stucking() || IsNpcFollowerSentry || _IsForwardBlock)
                        {
                            Vector3 fixedPos = GetFixedPosition(PETools.PEUtil.MainCamTransform.position, -PETools.PEUtil.MainCamTransform.forward, trans.position, -trans.forward, trans.bound.size.y);
                            if (_IsForwardBlock)
                            {
                                fixedPos = MainPlayer.Instance.entity.position;
                                Vector3 dir = new Vector3(-PEUtil.MainCamTransform.forward.x, 0.0f, -PEUtil.MainCamTransform.forward.z);
                                dir = Quaternion.AngleAxis(Random.Range(-90.0f, 90.0f), Vector3.up) * dir;
                                fixedPos = fixedPos + dir.normalized * Random.Range(4.0f, 6.0f);
                            }
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
                        if (Stucking() || _IsForwardBlock) //|| IsNpcFollowerStand
                        {
                            Vector3 fixedPos = GetFixedPosition(PETools.PEUtil.MainCamTransform.position, -PETools.PEUtil.MainCamTransform.forward, trans.position, -trans.forward, trans.bound.size.y);
                            float dy = Mathf.Abs(trans.position.y - fixedPos.y);
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
                if (_InfirR && !m_Data.InRadius(position, trans.trans.position + m_Data.Anchor, m_Data.firRadius, m_Data.sndRadius, true))
                {
                    m_Data.ResetCalculated();
                    m_Data.CalculateAnchor(position, PETools.PEUtil.MainCamTransform.position, -PETools.PEUtil.MainCamTransform.forward);
                    m_Data.startPralTime = Time.time;
                }

                if (Time.time - m_Data.startPralTime >= m_Data.waitPralTime)
                {
                    m_Data.startPralTime = Time.time;
                    m_Data.CalculateAnchor(position, PETools.PEUtil.MainCamTransform.position, -PETools.PEUtil.MainCamTransform.forward);
                    m_Data.ResetCalculatedDir();
                }

                bool _nearAnchor = IsReached(position, trans.trans.position + m_Data.Anchor, true, 2.0f);
                bool _InPralCdTime = Time.time - m_Data.startPralTime < m_Data.waitPralTime;
                bool _moveWait = _InPralCdTime && _nearAnchor && !_needAvoid;

                if (!_Isstay)
                {
                    if (!_needAvoid)
                    {
                        if (_moveWait)
                        {

                            if (_IsForwardBlock)
                            {
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
                            if (_IsForwardBlock || Stucking())
                            {
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
                                Vector3 followPos = m_Data.GetFollowPosition(trans.trans, targetVelocity);
                                SpeedState speed = m_Data.CalculateSpeedState(PEUtil.SqrMagnitudeH(position, trans.trans.position));
                                // if (speed == SpeedState.Walk && _needAvoid) speed = SpeedState.Run;
                                if (IsReached(position, trans.trans.position + m_Data.Anchor, false))
                                {
                                    //on the bridgew
                                    Vector3 pos0 = trans.trans.position + m_Data.Anchor;
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
                        m_Data.CalculateAnchor(position, trans.trans.position, _AvoidDir);
                        //Vector3 pos = _AvoidDir * 5.0f + position;
                        MoveDirection(_AvoidDir, SpeedState.Run);
                    }
                }
                else
                {
                    StopMove();
                }
            }

			//Blocking
			if(!request.CanRun())
			{
				RQAttack rqAt = GetRequest(EReqType.Attack) as RQAttack;
				if(rqAt != null)
				{
					//block by attack	
					if(!Enemy.IsNullOrInvalid(attackEnemy) && (IsOnVCCarrier || IsOnRail))
					{
						//do nothing go on followTarget
					}
					else
					{
						if(!Enemy.IsNullOrInvalid(attackEnemy))
							return BehaveResult.Failure;
					}
				}
				else
				{
					if (IsOnVCCarrier)
						GetOff();
					
					if (IsOnRail)
						GetOffRailRoute();

					return BehaveResult.Failure;
				}
				
			}

			return BehaveResult.Running;
		}
	}

    [BehaveAction(typeof(BRSalvation), "RSalvation")]
    public class BRSalvation : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public string animName = "";
            [BehaveAttribute]
            public float startCarryUpTime;
            [BehaveAttribute]
            public float carryUpTime;
            [BehaveAttribute]
            public float startCarryDownTime;
            [BehaveAttribute]
            public float carryDownTime;

            public bool m_BeCarry = false;
            public float m_StartCarryUpTime = 0.0f;
            public float m_StartCarryDownTime = 0.0f;
        }

        Data m_Data;

		//static float t = 0.1f;
        RQSalvation m_Salvation;
		Vector3 SalvatPos;
		Interaction_Carry carry;
        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (m_Salvation == null)
                m_Salvation = GetRequest(EReqType.Salvation) as RQSalvation;

            if (m_Salvation == null)
                return BehaveResult.Failure;

            m_Data.m_BeCarry = false;
            m_Data.m_StartCarryUpTime = 0.0f;
            m_Data.m_StartCarryDownTime = 0.0f;
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {

            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (!m_Salvation.CanRun())
                return BehaveResult.Failure;

			SetNpcAiType(ENpcAiType.RSalvation);
            PeEntity targetEntity = EntityMgr.Instance.Get(m_Salvation.id);
            if (targetEntity == null)
                return BehaveResult.Failure;

            PeTrans tr = targetEntity.peTrans;
            if (tr == null)
                return BehaveResult.Failure;

            NpcCmpt npc = targetEntity.NpcCmpt;
            if (npc == null)
                return BehaveResult.Failure;

            Motion_Move mover = targetEntity.motionMove;
            if(mover == null)
                return BehaveResult.Failure;


            if (GetBool("SquatTreat"))
            {
                SetBool("SquatTreat", false);
                return BehaveResult.Running;
            }
               

            if (m_Salvation.carry)
            {
                if (PEUtil.SqrMagnitudeH(position, tr.position) > 1.0f * 1.0f)
				{
					MoveToPosition(tr.position,SpeedState.Run,false);
					if(Stucking()) SetPosition(tr.position);

					return BehaveResult.Running;
				}
                else
                {
                    if(!m_Data.m_BeCarry)
					    MoveToPosition(tr.position + tr.trans.forward * 0.3f,SpeedState.Walk,false);

					if (m_Salvation.m_Direction == Vector3.zero)
                    {
                        m_Salvation.m_Direction = transform.position - tr.trans.position;
                        m_Salvation.m_Direction.y = 0.0f;
                    }

                    if (m_Salvation.m_Direction == Vector3.zero)
                    {
                        m_Salvation.m_Direction = tr.trans.forward;
                        m_Salvation.m_Direction.y = 0.0f;
                    }

                    targetEntity.motionMgr.SetMaskState(PEActionMask.Cutscene, false);
                    FaceDirection(m_Salvation.m_Direction);
                    mover.RotateTo(m_Salvation.m_Direction);


                    if (Vector3.Angle(tr.trans.forward, transform.forward) < 5.0f)
                    {
                        if (m_Data.m_StartCarryUpTime < PETools.PEMath.Epsilon)
                        {
                            m_Data.m_StartCarryUpTime = Time.time;
							StopMove();
                            SetBool("Carry", true);
							npc.MountID = entity.Id;
							npc.Battle = ENpcBattle.Passive;
							carry = new Interaction_Carry();
							carry.Init(entity.transform,tr.transform);
                        }

                        if (Time.time - m_Data.m_StartCarryUpTime > m_Data.startCarryUpTime && !m_Data.m_BeCarry)
                        {
                            //npc.Req_SetIdle("BeCarry");
                            m_Data.m_BeCarry = true;

							if(carry != null)
							  carry.StartInteraction();
						
                        }

						if (Time.time - m_Data.m_StartCarryUpTime > m_Data.carryUpTime)
						{
							RemoveRequest(m_Salvation);
							entity.target.SetEnityCanAttack(false);
                            return BehaveResult.Success;
                        }
                    }
                }
            }
            else
            {
                if (m_Data.m_StartCarryDownTime < PETools.PEMath.Epsilon)
                {
                    m_Data.m_StartCarryDownTime = Time.time;

                    npc.MountID = 0;
					npc.Battle = ENpcBattle.Defence;
                    SetBool("Carry", false);

					if(carry != null)
					   carry.EndInteraction();

                    //tr.position = transform.position - transform.forward * 0.5f;
                }

                if (Time.time - m_Data.m_StartCarryDownTime > m_Data.startCarryDownTime)
                {
                    //req.RemoveRequest(EReqType.Idle);
                }

                if (Time.time - m_Data.m_StartCarryDownTime > m_Data.carryDownTime)
                {
                    RemoveRequest(m_Salvation);
					entity.target.SetEnityCanAttack(true);
                    return BehaveResult.Success;
                }
            }

            return BehaveResult.Running;
        }

        void Reset(Tree sender)
        {
            m_Salvation = null;
        }
    }

    [BehaveAction(typeof(BRTranslate), "RTranslate")]
    public class BRTranslate : BTNormal
    {
		float translateTime = 0.0f;
        BehaveResult Tick(Tree sender)
        {
			SetNpcAiType(ENpcAiType.RTranslate);
            RQTranslate translate = GetRequest(EReqType.Translate) as RQTranslate;
            if (translate != null && translate.CanRun())
            {
//				if(translateTime == 0.0f)
//				{
//				  translateTime = Time.time;
//                 
//				}
//
//				if(translateTime != 0.0f && Time.time - translateTime <= 5.0f)
//					return BehaveResult.Running;

				if(translateTime == 0.0f)
				{
					translateTime = Time.time;
					SetPosition(translate.position,translate.adjust);
				}

				if(!hasModel)
				{
					if(Time.time - translateTime < 5.0f)
					   return BehaveResult.Running;
				}

                ClearNpcMount();
                StopMove();
                RemoveRequest(translate);
				translateTime = 0.0f;
            }

            return BehaveResult.Failure;
        }
    }

    [BehaveAction(typeof(BRRotate), "RRotate")]
    public class BRRotate : BTNormal
    {
        BehaveResult Tick(Tree sender)
        {
			SetNpcAiType(ENpcAiType.RRotate);
            RQRotate rotate = GetRequest(EReqType.Rotate) as RQRotate;
            if (rotate != null && rotate.CanRun())
            {
                SetRotation(rotate.rotation);
                //ClearNpcMount();
                StopMove();
                RemoveRequest(rotate);
            }

            return BehaveResult.Failure;
        }
    }

    [BehaveAction(typeof(BRDialogue), "RDialogue")]
    public class BRDialogue : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public string[] anims = new string[0];

            public float m_LastTime = 0.0f;
            public float m_CurrentTime = 0.0f;
			public string m_curAction = null;
            public float GetCurrentTime()
            {
                return Random.Range(3.0f, 10.0f);
            }
        }

        Data m_Data;

        RQDialogue m_Dialogue;
		string m_RqAction = "";



        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            m_Dialogue = GetRequest(EReqType.Dialogue) as RQDialogue;
            if (m_Dialogue == null)
                return BehaveResult.Failure;

            StopMove();
            m_Data.m_LastTime = Time.time;
            m_Data.m_CurrentTime = m_Data.GetCurrentTime();
			m_RqAction = m_Dialogue.RqAction;
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
			SetNpcAiType(ENpcAiType.RDialogue);
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            RQDialogue req = GetRequest(EReqType.Dialogue) as RQDialogue;
            if (req == null)
			{
				if(m_RqAction != "")
				   EndAction(PEActionType.Leisure);
				return BehaveResult.Success;
			}

            RQSalvation m_Salvation = GetRequest(EReqType.Salvation) as RQSalvation;
            if (req != null && m_Salvation != null)
            {
                RemoveRequest(EReqType.Dialogue);
                return BehaveResult.Success;
            }

			if(req.RqRatePos != Vector3.zero)
            {
				Vector3 dir = req.RqRatePos - position;
				if(PEUtil.InAimDistance(req.RqRatePos,position,NPCConstNum.IK_Aim_Distance_Min,NPCConstNum.IK_Aim_Distance_Max) &&
				   PETools.PEUtil.InAimAngle(req.RqRatePos,position,existent.forward,NPCConstNum.IK_Aim_AngleH))
                {
                    //SetIKLerpspeed(NPCConstNum.IK_Aim_Lerpspeed_0);
                    SetIKFadeInTime(NPCConstNum.IK_Aim_FadeInTime_0);
                    SetIKFadeOutTime(NPCConstNum.IK_Aim_FadeOutTime_0);
                    SetIKActive(true);
                    SetIKTargetPos(PETools.PEUtil.CalculateAimPos(req.RqRatePos, position)); 
                }	
				else
				{
                    if (!PEUtil.InAimDistance(req.RqRatePos, position, NPCConstNum.IK_Aim_Distance_Min, NPCConstNum.IK_Aim_Distance_Max))
                        SetIKActive(false);

					FaceDirection(dir);
				}
					
            }
				

			if(!Enemy.IsNullOrInvalid(attackEnemy))
			{
				entity.target.ClearEnemy();
			}

			m_RqAction = req.RqAction;
			if(req.CanDoAction())
			{
				//SetBool(m_RqAction,true);
				PEActionParamS param = PEActionParamS.param;
				param.str = m_RqAction;
				DoAction(PEActionType.Leisure, param);
			}

            if (m_Data.anims.Length > 0)
            {
                if (Time.time - m_Data.m_LastTime > m_Data.m_CurrentTime)
                {
                    m_Data.m_LastTime = Time.time;
                    m_Data.m_CurrentTime = m_Data.GetCurrentTime();

                    SetBool(m_Data.anims[Random.Range(0, m_Data.anims.Length)], true);
                }
            }



            return BehaveResult.Running;
        }

		void Reset(Tree sender)
		{
			if(IsMotionRunning(PEActionType.Leisure))
				EndAction(PEActionType.Leisure);

            //SetIKLerpspeed(NPCConstNum.IK_Aim_Lerpdefault);

            if (entity!= null && entity.IKCmpt != null && entity.IKCmpt.m_IKAimCtrl != null && entity.IKCmpt.iKAimCtrl.active)
            {
                SetIKActive(false);
                entity.StartCoroutine(endSth(NPCConstNum.IK_Aim_FadeOutTime_0));
            }
		}

        IEnumerator endSth(float time)
        {
            yield return new WaitForSeconds(time);
            SetIKFadeInTime(NPCConstNum.IK_Aim_FadeInTimedefault);
            SetIKFadeOutTime(NPCConstNum.IK_Aim_FadeOutTimedefault);
            yield break;
        }
    }


	[BehaveAction(typeof(BTSkillAround), "SkillAround")]
	public class BTSkillAround : BTNormal
	{
		PeEntity Target;
		int AlreadySkill;
		Vector3 TargetPos;

		RQUseSkill m_UseSkill;
		bool  ConditionDecide(Vector3 targetPos,int skillId)
		{
			float Dis = Vector3.Distance(CostPos,targetPos);
			float Range = GetSkillRange(skillId);
			if(Range == -1)
				return false;
			return Dis<Range && GetHpJudge(skillId);
		}

		BehaveResult Init(Tree sender)
		{
			m_UseSkill = GetRequest(EReqType.UseSkill) as RQUseSkill;
			if(m_UseSkill == null)
				return BehaveResult.Failure;

			Target = SkillTarget;
			if(Target == null)
				return BehaveResult.Failure;

			if(Target.IsDeath())
			{
				SkillOver();
				return BehaveResult.Failure;
			}


			TargetPos = AllyTargetPos;
//			if(TargetPos == null)	// false
//				return BehaveResult.Failure;
			
			AlreadySkill = GetAreadySkill();
			if(AlreadySkill == -1)
			{
				SkillOver();
				return BehaveResult.Failure;
			}
			
			if(!ConditionDecide(TargetPos,AlreadySkill) || IsSkillRunning(AlreadySkill))
			{
				SkillOver();
				return BehaveResult.Failure;
			}
			
			StartSkill(SkillTarget, AlreadySkill);
			return BehaveResult.Running;
		}
		
		BehaveResult Tick(Tree sender)
		{
			SetNpcAiType(ENpcAiType.SkillAround);
			if(!IsSkillCast || Target ==null)
				return BehaveResult.Failure;

			if(Target.IsDeath())
			{
				SkillOver();
				return BehaveResult.Failure;
			}
			
			if (IsSkillRunning(AlreadySkill))
				return BehaveResult.Running;
			else
			{
				if(SkillOver())
					return BehaveResult.Failure;
				else
					return BehaveResult.Running;
			}
		}

		void Reset(Tree sender)
		{
			SkillOver();
		}
	}
}