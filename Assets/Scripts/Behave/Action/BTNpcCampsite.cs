using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using Pathea.Operate;
using SkillSystem;
using PETools;

namespace Behave.Runtime
{
    [BehaveAction(typeof(BTIsNpcCampsite), "IsNpcCampsite")]
    public class BTIsNpcCampsite : BTNormal
    {
        BehaveResult Tick(Tree sender)
        {
            if (IsNpcCampsite)
                return BehaveResult.Success;
            else
                return BehaveResult.Failure;
        }
    }

    [BehaveAction(typeof(BTNpcCampsiteWander), "NpcCampsiteWander")]
    public class BTNpcCampsiteWander : BTNormal
    {

		class Data
		{
			[BehaveAttribute]
			public float WanderTime;
			[BehaveAttribute]
			public float WanderRadius;
			[BehaveAttribute]
			public float Probability;

		}

		float mStartTime =0.0f;
		float mWanderTime = 6.0f;
        Vector3 m_CurWanderPos;
		
        Vector3 GetWanderPos()
        {
			return PEUtil.GetRandomPositionOnGroundForWander(Campsite.Pos, 0.0f, Campsite.Radius);
        }

		bool CanWalkPos(out Vector3 walkpos)
		{
			Vector3 newpos = GetWanderPos();
			if(AiUtil.GetNearNodePosWalkable(newpos,out walkpos))
			{
				return true;
			}
			walkpos = Vector3.zero;
			return false;
		}

        BehaveResult Init(Tree sender)
        {
            if (!IsNpcCampsite)
                return BehaveResult.Failure;

            if (hasAnyRequest)
                return BehaveResult.Failure;

			if(!NpcTypeDb.CanRun(NpcCmdId,ENpcControlType.Stroll))
			{
				StopMove();
				return BehaveResult.Failure;
			}

			if (attackEnemy != null)
				return BehaveResult.Failure;

			if(!NpcCanWalkPos(Campsite.Pos,Campsite.Radius,out m_CurWanderPos))
				return BehaveResult.Failure;

			mStartTime = Time.time;
			mWanderTime = 6.0f;
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
			SetNpcAiType(ENpcAiType.NpcCampsiteWander);
            if (!IsNpcCampsite)
                return BehaveResult.Failure;

            if (hasAnyRequest)
                return BehaveResult.Failure;

			if(!NpcTypeDb.CanRun(NpcCmdId,ENpcControlType.Stroll))
			{
				StopMove();
				return BehaveResult.Failure;
			}

			if (attackEnemy != null)
				return BehaveResult.Failure;

//			if(CampiseChanceTalk() != Vector3.zero)
//				return BehaveResult.Failure;

            if (Stucking(5.0f))
            {
				//MoveToPosition(Vector3.zero,SpeedState.Sprint);
				if (NpcCanWalkPos(Campsite.Pos,Campsite.Radius,out m_CurWanderPos) && m_CurWanderPos != Vector3.zero)
					SetPosition(m_CurWanderPos);

                return BehaveResult.Failure;
            }

            if (PEUtil.SqrMagnitudeH(position, m_CurWanderPos) < 0.5f * 0.5f)
                return BehaveResult.Success;

			if(Time.time - mStartTime > mWanderTime)
				return BehaveResult.Success;
			
		
            MoveToPosition(m_CurWanderPos);
            return BehaveResult.Running;
        }
    }

	[BehaveAction(typeof(BTNpcCampsiteSleep), "NpcCampsiteSleep")]
	public class BTNpcCampsiteSleep : BTNormal
	{
	    class Data
		{	
			[BehaveAttribute]
			public int sleepId;
			[BehaveAttribute]
			public string sleepAnim;
			[BehaveAttribute]
			public string sleepTimeSlots;


			bool m_Init = false;
			public List<Pathea.CheckSlot> slots = new List<Pathea.CheckSlot>();

			public void Init(PeEntity npc)
			{

				if (!m_Init)
				{
					if(npc.NpcCmpt != null)
						npc.NpcCmpt.npcCheck.ClearSleepSlots();
					
					if (sleepTimeSlots != "")
					{
						string[] dataStr = PEUtil.ToArrayString(sleepTimeSlots, ',');
						foreach (string item in dataStr)
						{
							float[] data = PEUtil.ToArraySingle(item, '_');
							if (data.Length == 2)
							{
								Pathea.CheckSlot slot  = new CheckSlot(data[0],data[1]);
								slots.Add(slot);
								
								slots.Add(slot);
								if(npc.NpcCmpt != null)
									npc.NpcCmpt.npcCheck.AddSleepSlots(slot.minTime,slot.maxTime);
							}
						}
					}
					m_Init = true;
				}
			}

		public bool IsTimeSlot(float timeSlot)
		{
			return slots.Find(ret => ret.InSlot(timeSlot)) != null;
		}

		}
		Data m_Data;

		//int Id = 30200055;
		bool  mArrived = true;
		//bool Test = true;
		SleepPostion mSleepInfo = null;
		//Vector3 DirPos;
	    void NpcSleep(IOperator oper)
		{
			PEActionParamVQNS param = PEActionParamVQNS.param;
			param.vec = mSleepInfo._Pos;
			param.q = Quaternion.Euler(new Vector3(0,mSleepInfo._Rate,0));
			param.n = m_Data.sleepId;
			param.str =m_Data.sleepAnim;
			DoAction(PEActionType.Sleep, param);
		}

		Vector3 GetUpPos()
		{
			return PEUtil.GetRandomPositionOnGround(mSleepInfo._Pos, 0.0f, 3.0f);

		}

		void Getup(IOperator oper)
		{
			EndAction(PEActionType.Sleep);
		}

		bool GetEmptyPos(out SleepPostion _SleepInfo)
		{	
			_SleepInfo = new SleepPostion();
			_SleepInfo._Pos = Vector3.zero;
			_SleepInfo._Rate = 0.0f;
			_SleepInfo.Occpyied = false;
			_SleepInfo._Id = 0;

			for(int index =0;index < Campsite.LayDatas.Count;index++)
			{
				if(!Campsite.LayDatas[index].Occpyied)
				{
					Campsite.LayDatas[index].Occpyied = true;
					_SleepInfo = Campsite.LayDatas[index];
					_SleepInfo._Id = entity.Id;
					return true;
				}
			}
			return false;
		}

		void FreeThePos(SleepPostion _SleepInfo)
		{
			if(Campsite != null && Campsite.LayDatas != null)
			{
				for(int i=0;i<Campsite.LayDatas.Count;i++)
				{
					if(Campsite.LayDatas[i] == _SleepInfo)
					{
						Campsite.LayDatas[i].Occpyied = false;
					}
				}
			}

		}


		bool InRadiu(Vector3 self,Vector3 target,float radiu)
		{
			float sqrDistanceH = PEUtil.SqrMagnitudeH(self, target);
			return sqrDistanceH < radiu * radiu;
		}

		BehaveResult Init(Tree sender)
		{
			
			if (!GetData<Data>(sender, ref m_Data))
				return BehaveResult.Failure;

			m_Data.Init(entity);

			if (!IsNpcCampsite)
				return BehaveResult.Failure;

			if(hasAnyRequest)
				return BehaveResult.Failure;

			if(PeCreature.Instance != null && PeCreature.Instance.mainPlayer != null && InRadiu(position,PeCreature.Instance.mainPlayer.position,5.0f))
				return BehaveResult.Failure;

			if(!NpcTypeDb.CanRun(NpcCmdId,ENpcControlType.Sleep))
				return BehaveResult.Failure;

			if(entity.NpcCmpt.NpcInAlert)
				return BehaveResult.Failure;

			if (!Enemy.IsNullOrInvalid(attackEnemy))
				 return BehaveResult.Failure;

			if(Campsite.LayDatas == null || Campsite.LayDatas.Count <=0)
				return BehaveResult.Failure;

			if (!m_Data.IsTimeSlot((float)GameTime.Timer.HourInDay))
				return BehaveResult.Failure;

			mArrived = false;

			if(mSleepInfo == null && !GetEmptyPos(out mSleepInfo))
				return BehaveResult.Failure;

			if(PeCreature.Instance != null && PeCreature.Instance.mainPlayer != null && InRadiu(mSleepInfo._Pos,PeCreature.Instance.mainPlayer.position,5.0f))
				return BehaveResult.Failure;
						
			if (m_Data.IsTimeSlot((float)GameTime.Timer.HourInDay))
				return BehaveResult.Success;

			FreeThePos(mSleepInfo);
			//DirPos = mSleepInfo._Doorpos;
			return BehaveResult.Failure;
		}

		BehaveResult Tick(Tree sender)
		{
			SetNpcAiType(ENpcAiType.NpcCampsiteSleep);
			if (!IsNpcCampsite)
				return BehaveResult.Failure;

			if(entity.NpcCmpt.NpcInAlert)
				return BehaveResult.Failure;

			if(!IsSelfSleep(entity.Id,out mSleepInfo))
			{
				Getup(Operator);				
				return BehaveResult.Failure;
			}

			if(mSleepInfo == null)
				return BehaveResult.Failure;

			if (GetBool("OperateMed"))
			{
				SetBool("OperateMed",false);
			}
			if (GetBool("OperateCom"))
			{
				SetBool("OperateCom",false);
			}
			if (GetBool("FixLifeboat"))
			{
				SetBool("FixLifeboat",false);
			}


			if(!NpcTypeDb.CanRun(NpcCmdId,ENpcControlType.Sleep))
			{
				Getup(Operator);
				FreeThePos(mSleepInfo);
				SetPosition(PEUtil.GetRandomPositionOnGround(Campsite.Pos, 0.0f, Campsite.Radius));
				return BehaveResult.Failure;
			}

			if(hasAnyRequest)
			{
				if(ContainsRequest(EReqType.Dialogue))
				{
					Getup(Operator);
					FreeThePos(mSleepInfo);
					return BehaveResult.Failure;
				}
				if(mArrived)
				{
					Getup(Operator);
					SetPosition(mSleepInfo._Doorpos);
				}
				FreeThePos(mSleepInfo);
				return BehaveResult.Failure;
			}

			if (attackEnemy != null)
			{
				Getup(Operator);
				FreeThePos(mSleepInfo);
				SetPosition(mSleepInfo._Doorpos);
				return BehaveResult.Failure;
			}

			if(!mArrived && IsReached(mSleepInfo._Pos,position,false))
			{
				StopMove();
				SetPosition(mSleepInfo._Pos);
				NpcSleep(Operator);
				mArrived = true;
			}

			if(!mArrived && !IsReached(mSleepInfo._Doorpos,position,false))
			{
				MoveToPosition(mSleepInfo._Doorpos,SpeedState.Run);
				if(Stucking())
					SetPosition(mSleepInfo._Pos);
			}

			if(!mArrived && IsReached(mSleepInfo._Doorpos,position,false))
			{
				SetPosition(mSleepInfo._Pos);
			}

	
			if (!m_Data.IsTimeSlot((float)GameTime.Timer.HourInDay))
			{
				Getup(Operator);
				SetPosition( mSleepInfo._Doorpos);
				FreeThePos(mSleepInfo);
				return BehaveResult.Success;
			}

			return BehaveResult.Running;

		}

		void Reset(Tree sender)
		{
			FreeThePos(mSleepInfo);
			mArrived =false;
		}


	}

	[BehaveAction(typeof(BTNpcCampsiteEat), "NpcCampsiteEat")]
	public class BTNpcCampsiteEat : BTNormal
	{
		class Data
		{
			public class AnimSlot
			{
				public bool bplay = false;
				public string playState = "";
				public string animName = "";
				public AnimSlot(string state,string anim)
				{
					if(state != "0")
					{
						playState = state;
						bplay = true;
					}
					else
						bplay = false;

					animName = anim;
				}
			}

			[BehaveAttribute]
			public string EatHour;
			[BehaveAttribute]
			public string AnimName;
			[BehaveAttribute]
			public float  PlayTime;

			public List<Pathea.CheckSlot> slots;
			List<AnimSlot> AnimSlots;
			public PEBuilding mBuild;

			public float mStartEatTime;
			public AnimSlot  CurSlot;
			bool mInit = false;
			public void Init()
			{
				if(!mInit)
				{
					slots = new List<Pathea.CheckSlot>();
					string[] dataStr = PEUtil.ToArrayString(EatHour,',');
					for(int i=0;i<dataStr.Length;i++)
					{
						float[] dataflot = PEUtil.ToArraySingle(dataStr[i],'_');
						if(dataflot.Length == 2)
						{
							slots.Add(new CheckSlot(dataflot[0],dataflot[1]));
						}
						
					}

					AnimSlots = new List<AnimSlot>();
					string[] dataStr2 = PEUtil.ToArrayString(AnimName,',');
					for(int i=0;i<dataStr2.Length;i++)
					{
						string[] dataflot = PEUtil.ToArrayString(dataStr2[i],'_');
						if(dataflot.Length == 2)
						{
							AnimSlots.Add(new AnimSlot(dataflot[0],dataflot[1]));
						}
					}

					mInit = true;
				}

			}

			public bool GetEatPostions(int AssetId)
			{
				PeEntity[] entitys = EntityMgr.Instance.GetDoodadEntities(AssetId);
				if(entitys == null || entitys.Length <=0)
					return false;

				mBuild = entitys[0].Doodabuid;
				if(mBuild == null)
					return false;

				mBuild.SetFoodShowSlots(slots);
				return true;
			}

			public bool IsReached(Vector3 pos, Vector3 targetPos,float radiu = 1.0f)
			{
				float sqrDistanceH = PEUtil.SqrMagnitudeH(pos, targetPos);
				return sqrDistanceH < radiu * radiu;
			}

			public bool RangeAnim()
			{
				if(AnimSlots.Count == 0)
					return false;

				CurSlot = AnimSlots[Random.Range(0,AnimSlots.Count)];
				return true;
			}

			public bool InTimeSlot(float curTime)
			{
				return slots.Find(ret => ret.InSlot(curTime))!= null;
			}
		}
		Data m_Data;
		Vector3 eatpostion;
		//bool Hasreached = false;

		Transform mTrans;
		BehaveResult End()
		{
			if(m_Data.CurSlot.bplay)
			{
				if(GetBool(m_Data.CurSlot.playState))
				{
					SetBool(m_Data.CurSlot.playState,false);
					return BehaveResult.Running;
				}
				if(GetBool(m_Data.CurSlot.animName))
				{
					SetBool(m_Data.CurSlot.animName,false);
					return BehaveResult.Running;
				}
				
			}
			else
			{
				if(IsMotionRunning(PEActionType.Eat))
				{
					EndAction(PEActionType.Eat);
					return BehaveResult.Running;
				}
			}
			return BehaveResult.Failure;
		}

		BehaveResult Init(Tree sender)
		{
			if (!GetData<Data>(sender, ref m_Data))
				return BehaveResult.Failure;
			m_Data.Init();

			if (!IsNpcCampsite)
				return BehaveResult.Failure;

			//return BehaveResult.Failure;
			m_Data.RangeAnim();
			if(hasAttackEnemy)
				return BehaveResult.Failure;

			if(hasAnyRequest)
				return BehaveResult.Failure;

			if(entity.NpcCmpt.NpcInAlert)
				return BehaveResult.Failure;

			if(!NpcTypeDb.CanRun(NpcCmdId,ENpcControlType.Dining))
			{
				StopMove();
				return BehaveResult.Failure;
			}

			if(Campsite == null || Campsite.mEatInfo == null)
				return BehaveResult.Failure;

			if(!m_Data.InTimeSlot((float)GameTime.Timer.HourInDay))
				return BehaveResult.Failure;

			if(!m_Data.GetEatPostions(Campsite.mEatInfo.assesID))
				return BehaveResult.Failure;
			
			mTrans = m_Data.mBuild.Occupy(entity.Id);
			if(mTrans == null)
				return BehaveResult.Failure;

			eatpostion = Campsite.GetObjectPostion(Campsite.mEatInfo.assesID);
			if(eatpostion == Vector3.zero)
			   return BehaveResult.Failure;

			m_Data.mStartEatTime = Time.time;
			return BehaveResult.Running;
		}

		BehaveResult Tick(Tree sender)
		{
			SetNpcAiType(ENpcAiType.NpcCampsiteEat);
		
			if(!IsNpcCampsite || hasAttackEnemy || hasAnyRequest ||entity.NpcCmpt.NpcInAlert||
			   !NpcTypeDb.CanRun(NpcCmdId,ENpcControlType.Dining) || mTrans == null||
			   !m_Data.InTimeSlot((float)GameTime.Timer.HourInDay))
			{
				StopMove();
				return End();
			}

			if (GetBool("OperateMed"))
			{
				SetBool("OperateMed",false);
			}
			if (GetBool("OperateCom"))
			{
				SetBool("OperateCom",false);
			}
			if (GetBool("FixLifeboat"))
			{
				SetBool("FixLifeboat",false);
			}


			if(m_Data.IsReached(position,mTrans.position))
			{
				if(Time.time - m_Data.mStartEatTime >= m_Data.PlayTime)
				{
					if(m_Data.CurSlot.bplay)
					{
						if(!GetBool(m_Data.CurSlot.playState))
						{
							SetBool(m_Data.CurSlot.playState,true);
							return BehaveResult.Running;
						}
						if(!GetBool(m_Data.CurSlot.animName))
						{
							SetBool(m_Data.CurSlot.animName,true);
							return BehaveResult.Running;
						}
					}
					else
					{
						m_Data.mStartEatTime = Time.time;
						PEActionParamS param = PEActionParamS.param;
						param.str = m_Data.CurSlot.animName;
						DoAction(PEActionType.Eat, param);
					}
					FaceDirection(eatpostion - position);

				}
			}
			else
			{
				if(Stucking())
					SetPosition(mTrans.position);

			   MoveToPosition(mTrans.position,SpeedState.Run,false);
			}

			return BehaveResult.Running;
		}

		void Reset(Tree sender)
		{
			if(IsMotionRunning(PEActionType.Eat))
				EndAction(PEActionType.Eat);

			if(m_Data != null && m_Data.CurSlot != null)
			{
				SetBool(m_Data.CurSlot.playState,false);
				SetBool(m_Data.CurSlot.animName,false);
			}

			if(m_Data != null && m_Data.mBuild != null)
			{
				m_Data.mBuild.Release(entity.Id);
			}
		}

	}

	[BehaveAction(typeof(BTNpcCampsiteTalk), "NpcCampsiteTalk")]
	public class BTNpcCampsiteTalk : BTNormal
	{

		class Data
		{
			[BehaveAttribute]
			public string PlayState;
			[BehaveAttribute]
			public string AnimName;
			[BehaveAttribute]
			public float TalkRadius;
			[BehaveAttribute]
			public float TargetAngle;
			[BehaveAttribute]
			public float Probability;
			[BehaveAttribute]
			public float TalkTime;

			public float startTalkTime;
			bool mInit = false;
			public string[] Anims;
			public void Init()
			{
				if(!mInit)
				{
					Anims = PEUtil.ToArrayString(AnimName,',');
				}
			}

		}
		Data m_Data;

		float mStarTime;
		Vector3 mDirPostion;
		Vector3 mDir;
		PeEntity mTarget = null;
		//string mNowAction;
		//bool mHasAction;

		bool TargetCanTalk()
		{
			if(mTarget == null || mTarget.NpcCmpt == null)
				return false;

			if(!CantainTalkTarget(mTarget))
				return false;

			if(mTarget.NpcCmpt.hasAnyRequest)
			   return false;

			return true;
		}
	
		void startTalk()
		{
			SetBool((Random.value > 0.5f) ? "Talk0":"Talk1",true);
			//mHasAction = true;
		}

		void endTalk()
		{
			//mHasAction = false;
			SetBool("Talk0",false);
			SetBool("Talk1",false);
		}


		void EndAnims()
		{
			for(int i=0;i<m_Data.Anims.Length;i++)
			{
				if(GetBool(m_Data.Anims[i]))
					SetBool(m_Data.Anims[i],false);
			}
			SetBool(m_Data.PlayState,false);
		}

		BehaveResult Init(Tree sender)
		{
			if (!GetData<Data>(sender, ref m_Data))
				return BehaveResult.Failure;
		 
			m_Data.Init();
			if (!IsNpcCampsite)
				return BehaveResult.Failure;
			
			if(hasAnyRequest)
				return BehaveResult.Failure;

			if(!Enemy.IsNullOrInvalid(attackEnemy))
				return BehaveResult.Failure;

			if(!NpcTypeDb.CanRun(NpcCmdId,ENpcControlType.Interaction))
			{
				StopMove();
				return BehaveResult.Failure;
			}

			if(Random.value > m_Data.Probability)
				return BehaveResult.Failure;

		
			if(!entity.NpcCmpt.NpcCanChat)
			{
				if(!Campsite.CalculatePostion(entity,m_Data.TalkRadius))
					return BehaveResult.Failure;
			}

			if(!entity.NpcCmpt.NpcCanChat)
				return BehaveResult.Failure;

			StopMove();
			SetBool(m_Data.PlayState,true);
			SetBool(m_Data.Anims[Random.Range(0,m_Data.Anims.Length)],true);
			m_Data.startTalkTime = Time.time;

			return BehaveResult.Running;
		}

		BehaveResult Tick(Tree sender)
		{

			if (!IsNpcCampsite || hasAnyRequest || !Enemy.IsNullOrInvalid(attackEnemy) || !entity.NpcCmpt.NpcCanChat)
			{
				EndAnims();
				return BehaveResult.Failure;
			}

			if(!NpcTypeDb.CanRun(NpcCmdId,ENpcControlType.Interaction))
			{
				EndAnims();
				return BehaveResult.Failure;
			}

			if(Time.time - m_Data.startTalkTime >m_Data.TalkTime)
			{
				EndAnims();
			}

			if((Time.time - m_Data.startTalkTime >m_Data.TalkTime) && (Time.time - m_Data.startTalkTime <m_Data.TalkTime + 3.0f))
			{
				MoveDirection(position - entity.NpcCmpt.ChatTarget.position);
				return BehaveResult.Running;
			}

			if(Time.time - m_Data.startTalkTime >m_Data.TalkTime + 3.0f)
			{
				entity.NpcCmpt.ChatTarget = null;
				return BehaveResult.Success;
			}
			FaceDirection(entity.NpcCmpt.ChatTarget.position - position);
			return BehaveResult.Running;
		}
	}

	[BehaveAction(typeof(BTMoveToPoints), "MoveToPoints")]
	public class BTMoveToPoints : BTNormal
	{
		class Data
		{
			[BehaveAttribute]
			public int speed;
			[BehaveAttribute]
			public string pathData;

			public Vector3[] Path;
			//public bool isLoop;
			bool mInit = false;
			public  void Init()
			{
				if(!mInit)
				{
					List<Vector3> Vectors = new List<Vector3>();
					string[] dataStr = PEUtil.ToArrayString(pathData, ';');
					for(int i=0;i<dataStr.Length;i++)
					{
						float[] dataInt = PEUtil.ToArraySingle(dataStr[i],',');
						Vectors.Add(new Vector3(dataInt[0],dataInt[1],dataInt[2]));
					}
					Path = Vectors.ToArray();
					mInit = true;
				}
			}
		
			public  bool IsReached(Vector3 pos, Vector3 targetPos,float radiu = 1.0f)
			{
				float sqrDistanceH = PEUtil.SqrMagnitudeH(pos, targetPos);
				return sqrDistanceH < radiu * radiu;
			}


			public  int GetClosetPointIndex(Vector3 position)
			{
				int tmpIndex = -1;
				float tmpSqrMagnitude = Mathf.Infinity;
				for (int i = 0; i <Path.Length; i++)
				{
					float sqrMagnitude = PETools.PEUtil.SqrMagnitudeH(Path[i], position);
					if (sqrMagnitude < tmpSqrMagnitude)
					{
						tmpSqrMagnitude = sqrMagnitude;
						tmpIndex = i;
					}
				}
				
				return tmpIndex;
			}

		}
		Data m_Data;
	

		int mIndex;
		BehaveResult Init(Tree sender)
		{
			if (!GetData<Data>(sender, ref m_Data))
				return BehaveResult.Failure;

			m_Data.Init();

			mIndex = m_Data.GetClosetPointIndex(position);
			if (mIndex < 0 || mIndex >= m_Data.Path.Length)
				return BehaveResult.Failure;

			return BehaveResult.Running;
		}

		BehaveResult Tick(Tree sender)
		{
			if(m_Data.IsReached(position, m_Data.Path[mIndex]))
			{
				mIndex++;
				if (mIndex == m_Data.Path.Length)
				{
					return BehaveResult.Success;
				}
			}
			MoveToPosition(m_Data.Path[mIndex],(SpeedState)m_Data.speed);
			return BehaveResult.Running;
		}
	}

	[BehaveAction(typeof(BTMoveToPoint), "MoveToPoint")]
	public class BTMoveToPoint : BTNormal
	{
		class Data
		{
			[BehaveAttribute]
			public int speed;
			[BehaveAttribute]
			public string point;
			
			public Vector3 vector;
			bool mInit = false;
			public  void Init()
			{
				if(!mInit)
				{
					float[] dataFloat = PEUtil.ToArraySingle(point,',');
					vector =new Vector3(dataFloat[0],dataFloat[1],dataFloat[2]);
					mInit = true;
				}
			}
			
			public  bool IsReached(Vector3 pos, Vector3 targetPos,float radiu = 1.0f)
			{
				float sqrDistanceH = PEUtil.SqrMagnitudeH(pos, targetPos);
				return sqrDistanceH < radiu * radiu;
			}
		}
		Data m_Data;

		BehaveResult Init(Tree sender)
		{
			if (!GetData<Data>(sender, ref m_Data))
				return BehaveResult.Failure;
			m_Data.Init();

			return BehaveResult.Running;
		}

		BehaveResult Tick(Tree sender)
		{
			if(m_Data.IsReached(position,m_Data.vector))
				return BehaveResult.Success;
			else
			{
				MoveToPosition(m_Data.vector,(SpeedState)m_Data.speed);
				return BehaveResult.Running;
			}

		}
	}

	[BehaveAction(typeof(BTPlayAnimation), "PlayAnimation")]
	public class BTPlayAnimation : BTNormal
	{
		class Data
		{
			[BehaveAttribute]
			public string PlayState;
			[BehaveAttribute]
			public string AnimName;

		}
		Data m_Data;

		BehaveResult Init(Tree sender)
		{
			if (!GetData<Data>(sender, ref m_Data))
				return BehaveResult.Failure;

			if(!NpcTypeDb.CanRun(NpcCmdId,ENpcControlType.Work))
				return BehaveResult.Failure;

			if(hasAnyRequest || !Enemy.IsNullOrInvalid(attackEnemy))
				return BehaveResult.Failure;

			//Do action
			SetBool(m_Data.PlayState,true);
			return BehaveResult.Running;
		}
		
		BehaveResult Tick(Tree sender)
		{
			if(!NpcTypeDb.CanRun(NpcCmdId,ENpcControlType.Work))
				return BehaveResult.Failure;

			if(hasAnyRequest || !Enemy.IsNullOrInvalid(attackEnemy))
				return BehaveResult.Failure;

			SetBool(m_Data.AnimName,true);
			//DoAction(m_Data.actionType,m_Data.AnimName);
			return BehaveResult.Success;
			
		}
	}

	[BehaveAction(typeof(BTPlayLoopAnimation), "PlayLoopAnimation")]
	public class BTPlayLoopAnimation : BTNormal
	{
		
		class Data
		{
			[BehaveAttribute]
			public string PlayState;
			[BehaveAttribute]
			public string PlayAnim;
			[BehaveAttribute]
			public string PlayTimes;
			[BehaveAttribute]
			public float IntervalTime;
			[BehaveAttribute]
			public float MinTime;
			[BehaveAttribute]
			public float MaxTime;

			//public int AnimPlayTimes;
			public float mStartPlayTime;
			public float m_starIntervlTime;


			public bool InRadius(Vector3 self,Vector3 buidPos,float radiu = 1.0f)
			{
				float sqrDistanceH = PEUtil.SqrMagnitudeH(self, buidPos);
				return sqrDistanceH < radiu * radiu;
			}

		}
		Data m_Data;

		float actionTime ;
		BehaveResult Init(Tree sender)
		{
			if (!GetData<Data>(sender, ref m_Data))
				return BehaveResult.Failure;

			if(!NpcTypeDb.CanRun(NpcCmdId,ENpcControlType.Work))
				return BehaveResult.Failure;

			if(hasAnyRequest)
				return BehaveResult.Failure;

			if(!Enemy.IsNullOrInvalid(attackEnemy))
				return BehaveResult.Failure;


			m_Data.mStartPlayTime = Time.time;
			m_Data.m_starIntervlTime = Time.time;
			SetBool(m_Data.PlayState,true);
			SetBool(m_Data.PlayAnim,true);
			actionTime = UnityEngine.Random.Range(m_Data.MinTime,m_Data.MaxTime);
			//DoAction(m_Data.Anim);
			return BehaveResult.Running;
		}
		
		BehaveResult Tick(Tree sender)
		{
			if(!Enemy.IsNullOrInvalid(attackEnemy) || hasAnyRequest || entity.IsNpcInSleepTime || entity.IsNpcInDinnerTime || !NpcTypeDb.CanRun(NpcCmdId,ENpcControlType.Work))
			{
				StopMove();
				if(GetBool(m_Data.PlayAnim))
				{
					SetBool(m_Data.PlayAnim,false);
					return BehaveResult.Running;
				}

				if(GetBool(m_Data.PlayState))
				{
					SetBool(m_Data.PlayState,false);
					return BehaveResult.Running;
				}

				return BehaveResult.Failure;
			}

			if(NpcOccpyBuild != null)
			{
				Transform tran = NpcOccpyBuild.Occupy(entity.Id);
				if(tran != null)
					SetRotation(tran.rotation);
			}

			if(Time.time - m_Data.mStartPlayTime >= actionTime)
			{
				StopMove();
				if(GetBool(m_Data.PlayAnim))
				{
					SetBool(m_Data.PlayAnim,false);
					return BehaveResult.Running;
				}
				
				if(GetBool(m_Data.PlayState))
				{
					SetBool(m_Data.PlayState,false);
					return BehaveResult.Running;
				}

				return BehaveResult.Success;
			}

			if(Time.time -m_Data.m_starIntervlTime >= m_Data.IntervalTime)
			{
				if(NpcOccpyBuild != null && NpcOccpyBuild.Occupy(entity.Id) != null)
				{
					if(!m_Data.InRadius(position,NpcOccpyBuild.Occupy(entity.Id).transform.position))
					{
						StopMove();
						if(GetBool(m_Data.PlayAnim))
						{
							SetBool(m_Data.PlayAnim,false);
							return BehaveResult.Running;
						}
						
						if(GetBool(m_Data.PlayState))
						{
							SetBool(m_Data.PlayState,false);
							return BehaveResult.Running;
						}
						return BehaveResult.Failure;
					}
						
				}
				m_Data.m_starIntervlTime = Time.time;
				SetBool(m_Data.PlayAnim,true);


			}
			return BehaveResult.Running;
			
		}

		void Reset(Tree sender)
		{
			SetBool(m_Data.PlayAnim,false);
			SetBool(m_Data.PlayState,false);
			if(NpcOccpyBuild != null)
			{
				NpcOccpyBuild.Release(entity.Id);
				SetOccpyBuild(null);
			}
		}
	}

	[BehaveAction(typeof(BTFixedPatrol), "FixedPatrol")]
	public class BTFixedPatrol : BTNormal
	{
		class Data
		{

			[BehaveAttribute]
			public string PlayState;
			[BehaveAttribute]
			public string PlayAnim;
			[BehaveAttribute]
			public string  PlayTimes;
			[BehaveAttribute]
			public float IntervalTime;
			[BehaveAttribute]
			public float  MinTime;
			[BehaveAttribute]
			public float  MaxTime;
			[BehaveAttribute]
			public string TriggerTimes;
			[BehaveAttribute]
			public float Probability;



			public List<Vector3> PathWalk;
			public  PEPathData PathData;
			//public RandIntDb PosIds;
			public RandIntDb RandTriggerTimes;
			public RandIntDb RandPlayTimes;
			public string[] RandAnims;
			public float AnimTime;
			public float AnimStartTime;
			public float AnimStartIntervalTime;
			public bool IsActive = false;
			public  int curPath;
			//public PEActionType mType = 0;
			bool mInit = false;
			public  void Init()
			{
				if(!mInit)
				{
					RandAnims = PEUtil.ToArrayString(PlayAnim,',');
					int[] dataint = PEUtil.ToArrayInt32(TriggerTimes,',');
					if(dataint != null)
						RandTriggerTimes = new RandIntDb(dataint[0],dataint[1]);

					dataint = PEUtil.ToArrayInt32(PlayTimes,',');
					if(dataint != null)
						RandPlayTimes = new RandIntDb(dataint[0],dataint[1]);

					if(PathWalk == null)
					   PathWalk = new List<Vector3>();

					mInit = true;
					curPath = 0;
				}
			}

			public void Realse()
			{
				if(PathWalk != null)
				{
					PathWalk.Clear();
					PathWalk = null;
				}
				mInit = false;
			}

			public bool  RandPathData(Camp camp,int curpath)
			{
				if(camp != null && camp.mPaths != null)
				{
					string str = camp.GetPath(curpath);
					if(str == null || str == "0")
						return false;

					if(PEPathSingleton.Instance == null)
						return false;

					PathData = CampPathDb.GetPathData(str); //PEPathSingleton.Instance.GetPathData(str);
					if(PathData.warpMode == EPathMode.Pingpong)
					{
						PathWalk.Clear();
						for(int i=0;i<PathData.path.Length;i++)
						{
							PathWalk.Add(PathData.path[i]);
						}
						
						for(int i=PathData.path.Length-2;i>0;i--)
						{
							PathWalk.Add(PathData.path[i]);
						}
					}else if(PathData.warpMode == EPathMode.Loop)
					{
						PathWalk.Clear();
						for(int i=0;i<PathData.path.Length;i++)
						{
							PathWalk.Add(PathData.path[i]);
						}
					}
					return true;
				}
				return false;
			}

			public  bool IsReached(Vector3 pos, Vector3 targetPos,float radiu = 1.0f)
			{
				float sqrDistanceH = PEUtil.SqrMagnitudeH(pos, targetPos);
				return sqrDistanceH < radiu * radiu;
			}

			public  int GetClosetPointIndex(PEPathData pathData,Vector3 position)
			{
				int tmpIndex = -1;
				float tmpSqrMagnitude = Mathf.Infinity;
				for (int i = 0; i <pathData.path.Length; i++)
				{
					float sqrMagnitude = PETools.PEUtil.SqrMagnitudeH(pathData.path[i], position);
					if (sqrMagnitude < tmpSqrMagnitude)
					{
						tmpSqrMagnitude = sqrMagnitude;
						tmpIndex = i;
					}
				}
				
				return tmpIndex;
			}

		}
		Data m_Data;
	
		int mIndex;
		int mTriggerTimes;
		int mPlayTimes;
		int mActiveAnim;
		int loopTimes;
		string curAnim;
	

		BehaveResult Init(Tree sender)
		{
			if(!IsNpcCampsite)
				return BehaveResult.Failure;

			if (!GetData<Data>(sender, ref m_Data))
				return BehaveResult.Failure;
			m_Data.Init();

			if(hasAnyRequest || !Enemy.IsNullOrInvalid(attackEnemy))
				return BehaveResult.Failure;

			if(!NpcTypeDb.CanRun(NpcCmdId,ENpcControlType.Stroll))
				return BehaveResult.Failure;

			if(entity.NpcCmpt.NpcInAlert)
				return BehaveResult.Failure;

			if(entity.IsNpcInSleepTime || entity.IsNpcInDinnerTime)
				return BehaveResult.Failure;

			if(UnityEngine.Random.value <= m_Data.Probability)
				return BehaveResult.Failure;

			if(!m_Data.RandPathData(Campsite,m_Data.curPath))
				return BehaveResult.Failure;

			if(m_Data.PathData.Equals(null) ||m_Data.PathData.path == null)
				return BehaveResult.Failure;


			mIndex = m_Data.GetClosetPointIndex(m_Data.PathData,position);
			m_Data.curPath++;
			if(Campsite.mPaths != null && m_Data.curPath >= Campsite.mPaths.Length)
				m_Data.curPath = 0;


			loopTimes = UnityEngine.Random.Range(1, 3);
			mTriggerTimes = m_Data.RandTriggerTimes.Random();
			m_Data.AnimTime = UnityEngine.Random.Range(m_Data.MinTime, m_Data.MaxTime);
			mActiveAnim =UnityEngine.Random.Range(0, m_Data.PathWalk.Count);
			SetBool(m_Data.PlayState,true);
			return BehaveResult.Running;
		}
		
		BehaveResult Tick(Tree sender)
		{
			if(!IsNpcCampsite)
				return BehaveResult.Failure;

			if(hasAnyRequest || !Enemy.IsNullOrInvalid(attackEnemy))
				return BehaveResult.Failure;

			if(!NpcTypeDb.CanRun(NpcCmdId,ENpcControlType.Stroll))
				return BehaveResult.Failure;

			if(entity.NpcCmpt.NpcInAlert)
				return BehaveResult.Failure;

			if(entity.IsNpcInSleepTime || entity.IsNpcInDinnerTime)
				return BehaveResult.Failure;

			if (GetBool("OperateMed"))
			{
				SetBool("OperateMed",false);
			}
			if (GetBool("OperateCom"))
			{
				SetBool("OperateCom",false);
			}
			if (GetBool("FixLifeboat"))
			{
				SetBool("FixLifeboat",false);
			}


			if(mIndex == mActiveAnim && !m_Data.IsActive)
			{
				mTriggerTimes--;
				if(mTriggerTimes >0)  
					mActiveAnim +=3;
				else
					mTriggerTimes = m_Data.RandTriggerTimes.Random();
				
				if(mActiveAnim > m_Data.PathWalk.Count)
					mActiveAnim = m_Data.PathWalk.Count - mActiveAnim;
				
				StopMove();
				m_Data.IsActive = true;
				m_Data.AnimStartTime = Time.time;
				mPlayTimes = m_Data.RandPlayTimes.Random();
				//Doaction
			}

			if(!m_Data.IsActive)
			{
				if(m_Data.IsReached(position, m_Data.PathWalk[mIndex]))
				{
					mIndex++;
					if (mIndex == m_Data.PathWalk.Count)
					{
						mIndex = 0;
						loopTimes--;
					}

					if(loopTimes<=0)
						return BehaveResult.Success;
				}

				if(Stucking(3.0f))
					SetPosition(m_Data.PathWalk[mIndex]);

				MoveToPosition(m_Data.PathWalk[mIndex]);

			}
			else
			{
				if(Time.time - m_Data.AnimStartTime >= m_Data.AnimTime)
				{
					m_Data.IsActive = false;
					SetBool(curAnim,false);
					mIndex++;
					if (mIndex == m_Data.PathWalk.Count)
					{
						mIndex = 0;
						loopTimes--;
					}
				}
				else
				{
					if(Time.time - m_Data.AnimStartIntervalTime >= m_Data.IntervalTime)
					{
						m_Data.AnimStartIntervalTime = Time.time;

						if(mPlayTimes > 0)
						{
							curAnim = m_Data.RandAnims[Random.Range(0, m_Data.RandAnims.Length)];
							SetBool(curAnim,true);
							mPlayTimes--;
						}
							
					}
					FaceDirection(position - Campsite.Pos);
				}
			}

			return BehaveResult.Running;
			
		}

		void Reset(Tree sender)
		{
			if(m_Data != null)
			{
				SetBool(m_Data.PlayState,false);
				for(int i=0;i<m_Data.RandAnims.Length;i++)
				{
					if(GetBool(m_Data.RandAnims[i]))
						SetBool(m_Data.RandAnims[i],false);
				}
				m_Data.Realse();
			}

		}
	}

	[BehaveAction(typeof(BTNpcWander), "NpcWander")]
	public class BTNpcWander : BTNormal
	{
		class Data
		{
//			[BehaveAttribute]
//			public float WanderTime;
			[BehaveAttribute]
			public float WanderRadius;
			[BehaveAttribute]
			public float Probability;


			public float m_StartWanderTime;
			public bool GetWanderPos(Vector3 center,float radiu,out Vector3 walkpos)
			{
				Vector3 newpos = PEUtil.GetRandomPositionOnGroundForWander(center, 0.0f,radiu);
				if(AiUtil.GetNearNodePosWalkable(newpos,out walkpos))
				{
					return true;
				}
				walkpos = Vector3.zero;
				return false;
			}

			public  bool IsReached(Vector3 pos, Vector3 targetPos,float radiu = 1.0f)
			{
				float sqrDistanceH = PEUtil.SqrMagnitudeH(pos, targetPos);
				return sqrDistanceH < radiu * radiu;
			}
		}

		Data m_Data;

		Vector3 mWanderPos;

		//Vector3 avaidPos;
		float avaidStatTime;
		//float avaidTime = 2.0f;

		BehaveResult Init(Tree sender)
		{
			if(!IsNpcCampsite)
				return BehaveResult.Failure;
			
			if (!GetData<Data>(sender, ref m_Data))
				return BehaveResult.Failure;

			if(hasAnyRequest || !Enemy.IsNullOrInvalid(attackEnemy))
				return BehaveResult.Failure;

			if(entity.NpcCmpt.NpcInAlert)
				return BehaveResult.Failure;

			if(entity.IsNpcInDinnerTime || entity.IsNpcInSleepTime)
				return BehaveResult.Failure;

			if(UnityEngine.Random.value < m_Data.Probability)
				return BehaveResult.Failure;

			if(m_Data.GetWanderPos(position,m_Data.WanderRadius,out mWanderPos))
				return BehaveResult.Failure;

			m_Data.m_StartWanderTime = Time.time;
			//avaidPos = Vector3.zero;
			return BehaveResult.Running;
		}

		BehaveResult Tick(Tree sender)
		{
			if(!IsNpcCampsite)
				return BehaveResult.Failure;

			if(hasAnyRequest || !Enemy.IsNullOrInvalid(attackEnemy))
				return BehaveResult.Failure;

			if(entity.NpcCmpt.NpcInAlert)
				return BehaveResult.Failure;

			if(entity.IsNpcInDinnerTime || entity.IsNpcInSleepTime)
				return BehaveResult.Failure;

//			if(Time.time - m_Data.m_StartWanderTime >= m_Data.WanderTime)
//				return BehaveResult.Success;

			if (GetBool("OperateMed"))
			{
				SetBool("OperateMed",false);
			}
			if (GetBool("OperateCom"))
			{
				SetBool("OperateCom",false);
			}
			if (GetBool("FixLifeboat"))
			{
				SetBool("FixLifeboat",false);
			}

			if(m_Data.IsReached(position,mWanderPos))
			{
//				if(m_Data.GetWanderPos(position,m_Data.WanderRadius,out mWanderPos))
//					return BehaveResult.Failure;
				return BehaveResult.Success;
			}
			else
			{
			   MoveToPosition(mWanderPos);
			}
			return BehaveResult.Running;
		}

		void Reset(Tree sender)
		{
			StopMove();
		}
	}

	[BehaveAction(typeof(BTCheckPostion), "CheckPostion")]
	public class BTCheckPostion : BTNormal
	{

		class Data
		{
			[BehaveAttribute]
			public int Type;
			[BehaveAttribute]
			public float Probability;
			[BehaveAttribute]
			public int Speed;
			[BehaveAttribute]
			public string AttackBreak;


			public SpeedState speedState;
			int[] PosIds;
			public PEBuilding mBuild;
			public bool CanAttackBreak;
			public void InitPos(Camp camp)
			{
				PosIds = camp.GetPosByType((EPosType)Type);
				CanAttackBreak = System.Convert.ToInt32(AttackBreak) >0;
				speedState = (SpeedState)Speed;
			}

			public bool GetBuidPos()
			{
				if(PosIds != null && PosIds.Length ==1 && PosIds[0] != 0)
				{
					PeEntity[] bEntity = EntityMgr.Instance.GetDoodadEntities(PosIds[0]);
					if(bEntity == null || bEntity.Length <1)
						return false;

					if(mBuild == null)
						mBuild = bEntity[0].Doodabuid;

					if(mBuild == null)
						return false;

					return true;
				}
				return false;
			}

			public bool IsReached(Vector3 pos, Vector3 targetPos,float radiu = 1.0f)
			{
				float sqrDistanceH = PEUtil.SqrMagnitudeH(pos, targetPos);
				return sqrDistanceH < radiu * radiu;
			}


		}
		Data m_Data;

		Transform mTrans;
		Vector3 mDoorPos;
		Vector3 dirPos;

		BehaveResult Init(Tree sender )
		{
			if(!IsNpcCampsite)
				return BehaveResult.Failure;

			if(!NpcTypeDb.CanRun(NpcCmdId,ENpcControlType.Work))
				return BehaveResult.Failure;

			if (!GetData<Data>(sender, ref m_Data))
				return BehaveResult.Failure;
			m_Data.InitPos(Campsite);

			if(hasAnyRequest)
				return BehaveResult.Failure;
			
			if(m_Data.CanAttackBreak && !Enemy.IsNullOrInvalid(attackEnemy))
				return BehaveResult.Failure;

			if(m_Data.CanAttackBreak && entity.NpcCmpt.NpcInAlert)
				return BehaveResult.Failure;

			if(UnityEngine.Random.value > m_Data.Probability)
				return BehaveResult.Failure;

			if(!m_Data.GetBuidPos())
				return BehaveResult.Failure;

			mTrans = m_Data.mBuild.Occupy(entity.Id);
			if(mTrans == null)
				return BehaveResult.Failure;


			dirPos = mTrans.position;
			if(m_Data.mBuild.mDoorPos != null && m_Data.mBuild.mDoorPos.Length >0)
				mDoorPos = m_Data.mBuild.mDoorPos[Random.Range(0,m_Data.mBuild.mDoorPos.Length)].position;

			if(mDoorPos != Vector3.zero)
				dirPos = mDoorPos;

			SetOccpyBuild(m_Data.mBuild);
			return BehaveResult.Running;
		}

		BehaveResult Tick(Tree sender)
		{
			if(!NpcTypeDb.CanRun(NpcCmdId,ENpcControlType.Work))
				return BehaveResult.Failure;

			mTrans = m_Data.mBuild.Occupy(entity.Id);
			if(mTrans == null)
				return BehaveResult.Failure;

			if(hasAnyRequest)
				return BehaveResult.Failure;
			
			if(m_Data.CanAttackBreak && !Enemy.IsNullOrInvalid(attackEnemy))
				return BehaveResult.Failure;

			if(m_Data.CanAttackBreak && entity.NpcCmpt.NpcInAlert)
				return BehaveResult.Failure;

			if(m_Data.IsReached(position,mTrans.position))
			{
				StopMove();
				SetPosition(mTrans.position);
				return BehaveResult.Success;
			}
			else
			{
				if(Stucking())
					SetPosition(mTrans.position);

				if(m_Data.IsReached(position,dirPos) && dirPos != mTrans.position)
					SetPosition(mTrans.position);

				MoveToPosition(dirPos,m_Data.speedState,false);
			}

			return BehaveResult.Running;
		}

		void Reset(Tree sender)
		{
//			if(m_Data != null && m_Data.mBuild != null)
//			{
//				m_Data.mBuild.Release(entity.Id);
//				m_Data.mBuild = null;
//			}
			dirPos = Vector3.zero;
			mDoorPos = Vector3.zero;
		}

	}

	[BehaveAction(typeof(BTNpcBattleFear), "NpcBattleFear")]
	public class BTNpcBattleFear : BTNormal
	{

		class Data
		{
			[BehaveAttribute]
			public string PlayState;	//状态机名字
			[BehaveAttribute]
			public string PlayAnim;	//动作组的名字(idle1,idle2)
			[BehaveAttribute]
			public float IntervalTime;//	动作组的动作间隔时间：5秒

			public string[] mAnims;
			public float mStartIntervalTime;
			public bool mRest;

			public void Init()
			{
				mAnims = PEUtil.ToArrayString(PlayAnim,',');
			}
		}

		Data m_Data;

		BehaveResult Init(Tree sender )
		{
			if(hasAnyRequest)
				return BehaveResult.Failure;

			if(entity != null && entity.NpcCmpt != null && !entity.NpcCmpt.NpcInAlert)
			{
				if(Enemy.IsNullOrInvalid(attackEnemy))
					return BehaveResult.Failure;
			}


			if (!GetData<Data>(sender, ref m_Data))
				return BehaveResult.Failure;
			m_Data.Init();

			SetBool(m_Data.PlayState,true);
			SetBool(m_Data.PlayAnim,true);
			m_Data.mStartIntervalTime = Time.time;
			m_Data.mRest = true;
			return BehaveResult.Running;
		}

		BehaveResult Tick(Tree sender)
		{
			if(!entity.NpcCmpt.NpcInAlert && Enemy.IsNullOrInvalid(attackEnemy))// )
			{
				StopMove();
				for(int i=0;i<m_Data.mAnims.Length;i++)
				{
					if(GetBool(m_Data.mAnims[i]))
					{
						SetBool(m_Data.mAnims[i],false);
						return BehaveResult.Running;
					}
				}

				if(GetBool(m_Data.PlayState))
				{
					SetBool(m_Data.PlayAnim,false);
					return BehaveResult.Running;
				}


				return BehaveResult.Success;
			}

			if(hasAnyRequest)
			{
				for(int i=0;i<m_Data.mAnims.Length;i++)
				{
					if(GetBool(m_Data.mAnims[i]))
					{
						SetBool(m_Data.mAnims[i],false);
						return BehaveResult.Running;
					}
				}
				
				if(GetBool(m_Data.PlayState))
				{
					SetBool(m_Data.PlayAnim,false);
					return BehaveResult.Running;
				}

				return BehaveResult.Success;
			}

			if(NpcOccpyBuild != null)
			{
				Transform tran = NpcOccpyBuild.Occupy(entity.Id);
				if(tran != null)
					SetRotation(tran.rotation);
			}

			if(Time.time - m_Data.mStartIntervalTime >= m_Data.IntervalTime)
			{
			   SetBool(m_Data.mAnims[Random.Range(0,m_Data.mAnims.Length)],true);
			}

			return BehaveResult.Running;
		}

		void Reset(Tree sender)
		{
			if(m_Data != null && m_Data.mRest)
			{
				SetBool(m_Data.PlayState,false);
				for(int i=0;i<m_Data.mAnims.Length;i++)
				{
					if(GetBool(m_Data.mAnims[i]))
						SetBool(m_Data.PlayAnim,false);
				}

				if(NpcOccpyBuild != null)
				{
					NpcOccpyBuild.Release(entity.Id);
					SetOccpyBuild(null);
				}
				m_Data.mRest = false;
			}
		}
	}


	[BehaveAction(typeof(BTCheckPlayerPos), "CheckPlayerPos")]
	public class BTCheckPlayerPos : BTNormal
	{
		class Data
		{
			[BehaveAttribute]
			public float Radius;
		}
		Data m_Data;

		bool InRadiu(Vector3 self,Vector3 target,float radiu)
		{
			float sqrDistanceH = PEUtil.SqrMagnitudeH(self, target);
			return sqrDistanceH < radiu * radiu;
		}

		BehaveResult Tick(Tree sender)
		{
			if (!GetData<Data>(sender, ref m_Data))
				return BehaveResult.Failure;

			if(PeCreature.Instance != null && PeCreature.Instance.mainPlayer != null && InRadiu(position,PeCreature.Instance.mainPlayer.position,m_Data.Radius))
			{
				return BehaveResult.Failure;
			}
			else
			{
				return BehaveResult.Success;
			}
		}
	}


}