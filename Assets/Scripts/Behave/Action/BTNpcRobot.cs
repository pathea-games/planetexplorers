using UnityEngine;
using ItemAsset;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using SkillSystem;
using PETools;
using Pathea.PeEntityExtNpcPackage;

namespace Behave.Runtime
{
	[BehaveAction(typeof(BTFollowAround), "FollowAround")]
	public class BTFollowAround : BTNormal
	{
		class Data
		{
			[BehaveAttribute]
			public float cdTime = 10.0f;
			[BehaveAttribute]
			public float prob = 1.0f;
			[BehaveAttribute]
			public float minRange;
			[BehaveAttribute]
			public float maxRange;
			[BehaveAttribute]
			public float minHeight;
			[BehaveAttribute]
			public float maxHeight;
			[BehaveAttribute]
			public float minTime = 10.0f;
			[BehaveAttribute]
			public float maxTime = 10.0f;

			[BehaveAttribute]
			public int targetID;

			PeEntity mfollowEntity;
			public PeEntity followEntity 
			{
				get
				{
					if(mfollowEntity == null)
						mfollowEntity = EntityMgr.Instance.Get(targetID);
					return mfollowEntity;
				}
			}

		}
		
		Data m_Data;

		float m_Time;
		//float m_LastTime;
		float m_StartTime;
		Vector3 m_HoverPosition;

		Vector3 GetAroundPos()
		{
			if (field == MovementField.Sky)
				return PEUtil.GetRandomFollowPosInSky(m_Data.followEntity.position, transform.position - m_Data.followEntity.position, m_Data.minRange, m_Data.maxRange, m_Data.minHeight, m_Data.maxHeight, -90.0f, 90.0f);
			else if (field == MovementField.water)
				return PEUtil.GetRandomPositionInWater(m_Data.followEntity.position, transform.position - m_Data.followEntity.position, m_Data.minRange, m_Data.maxRange, m_Data.minHeight, m_Data.maxHeight, -90.0f, 90.0f);
			else
				return PEUtil.GetRandomPositionOnGround(m_Data.followEntity.position, transform.position - m_Data.followEntity.position, m_Data.minRange, m_Data.maxRange, -90.0f, 90.0f);

		}

		BehaveResult Init(Tree sender)
		{
			if (!GetData<Data>(sender, ref m_Data))
				return BehaveResult.Failure;
			
			if (m_Data.followEntity == null)
				return BehaveResult.Failure;

			if(!Enemy.IsNullOrInvalid(attackEnemy))
				return BehaveResult.Failure;

			PeEntityCreator.InitRobotInfo(entity,m_Data.followEntity);

			if (Random.value > m_Data.prob)
				return BehaveResult.Failure;
			
			m_HoverPosition = GetAroundPos();
			m_StartTime = Time.time;
			//m_LastTime = Time.time;
			m_Time = Random.Range(m_Data.minTime, m_Data.maxTime);
			return BehaveResult.Running;
		}
		
		BehaveResult Tick(Tree sender)
		{
			if (!GetData<Data>(sender, ref m_Data))
				return BehaveResult.Failure;
			
			if (m_Data.followEntity == null)
				return BehaveResult.Failure;

			if(!Enemy.IsNullOrInvalid(attackEnemy))
				return BehaveResult.Failure;
 

			if(!IsReached(position,m_Data.followEntity.position,false,40.0f))
			{
				m_HoverPosition = GetAroundPos();
				SetPosition(m_HoverPosition);
			}

			if (m_HoverPosition == Vector3.zero)
				return BehaveResult.Failure;
			else
			{
				float sqrDistance = PEUtil.SqrMagnitude(position, m_HoverPosition);
				if (sqrDistance < 1.0f * 1.0f || Stucking() || Time.time - m_StartTime > m_Time)
				{
					StopMove();
					return BehaveResult.Success;
				}
				else
				{
					MoveToPosition(m_HoverPosition, SpeedState.Run);
					return BehaveResult.Running;
				}
			}
		}
		
	}


	[BehaveAction(typeof(BTSkyRobotAttack), "SkyRobotAttack")]
	public class BTSkyRobotAttack : BTNormal
	{
		class Data
		{
			[BehaveAttribute]
			public float cdTime = 5.0f;
			[BehaveAttribute]
			public int skillID;

			[BehaveAttribute]
			public float minR;
			[BehaveAttribute]
			public  float maxR;
			[BehaveAttribute]
			public float minH;
			[BehaveAttribute]
			public float maxH;
			[BehaveAttribute]
			public int targetID;
			[BehaveAttribute]
			public float angle = 30.0f;


			public float mFindTime = 10.0f;
			PeEntity mfollowEntity;
			public PeEntity followEntity 
			{
				get
				{
					if(mfollowEntity == null)
						mfollowEntity = EntityMgr.Instance.Get(targetID);

					return mfollowEntity;
				}
			}

			public bool IsInFollowRadiu(Vector3 self,Vector3 targetPos,float radiu = 1.0f)
			{
				float sqrDistanceH = PEUtil.SqrMagnitudeH(self, targetPos);
				return sqrDistanceH < radiu * radiu;
			}

			public bool changrPos = false;
		}


		Vector3 GetAroundPos(Vector3 targetPos)
		{
			if (field == MovementField.Sky)
				return PEUtil.GetRandomFollowPosInSky(targetPos, transform.position - targetPos, m_Data.minR, m_Data.maxR, m_Data.minH, m_Data.maxH, -60.0f, 60.0f);

			return position + m_LocalPos;
		}

		Vector3 GetAttackPos()
		{
			if (field == MovementField.Sky)
				return PEUtil.GetRandomPositionInSky(m_Data.followEntity.position, m_Data.followEntity.position - attackEnemy.centerPos, m_Data.minR, m_Data.maxR, m_Data.minH, m_Data.maxH, -60.0f, 60.0f);

			return position;
		}

		Data m_Data;



		void PitchRotation()
		{
			Quaternion r2 = Quaternion.identity;
			Transform _pitch =existent;
			
			Vector3 direction = attackEnemy.position - position;
			
			Vector3 _right = _pitch.TransformDirection(Vector3.right);
			Vector3 _forward = _pitch.TransformDirection(Vector3.forward);
			Vector3 _direction = Vector3.ProjectOnPlane(direction, _right);
			
			r2 = Quaternion.FromToRotation(_forward, _direction);

			_pitch.rotation = Quaternion.Inverse(r2) * _pitch.rotation;
			return;
		}


		float realRadius
		{
			get { return attackEnemy != null ? (attackEnemy.radius + radius) : radius; }
		}
		
		bool IsInRange(Enemy e)
		{
			float h = position.y - e.position.y - e.height;
			float d = PEUtil.MagnitudeH(e.closetPoint, e.farthestPoint) - realRadius;
			
			if (h < m_Data.minH || h > m_Data.maxH)
				return false;
			
			if (d < m_Data.minR || d > m_Data.maxR)
				return false;
			
			return true;
		}
		
		bool IsArrived(Enemy e)
		{
			return Vector3.Distance(position, attackEnemy.position + m_LocalPos) < 1.0f;
		}
		
		bool IsAttacked(Enemy e)
		{
			return Vector3.Distance(position, attackEnemy.position + m_AttcakPos) < 1.0f;
		}

		static float _angle = 45.0f;
		bool IsInAngle(Enemy e)
		{
			Vector3 v1 = transform.forward;
			Vector3 v2 = attackEnemy.centerPos - transform.position;
			float angle = Vector3.Angle(v1, v2);
			return angle <_angle;
		}

		Vector3 GetLocalPosition(Enemy e)
		{
			Vector3 v = Vector3.ProjectOnPlane(position - attackEnemy.position, Vector3.up).normalized;
			
			Vector3 pos = v * m_Data.maxR * 1.5f;
			
			float h = e.height + (m_Data.minH + m_Data.maxH) * 0.5f;
			return pos + Vector3.up * h;
		}
		
		Vector3 GetAttackPosition(Enemy e)
		{
			Vector3 v = Vector3.ProjectOnPlane(position - attackEnemy.position, Vector3.up).normalized;
			Vector3 pos = v * (m_Data.minR + m_Data.maxR) * 0.5f;
			float h = e.height + (m_Data.minH + m_Data.maxH) * 0.5f;
			return pos + Vector3.up * h;
		}


		void coatSkill()
		{
			existent.LookAt(attackEnemy.centerPos);
			StartSkill(attackEnemy.entityTarget,m_Data.skillID);
		}

		//float m_StartTime;
		//bool m_Arrived;
		Vector3 m_LocalPos;
		Vector3 m_AttcakPos;
		//bool m_findAttack;

		//float startFindPosTime;

		static float x1 = 5.0f;
		static float x2 = 50.0f;
		static float x3 = 2.0f;
		BehaveResult Init(Tree sender)
		{
			if (!GetData<Data>(sender, ref m_Data))
				return BehaveResult.Failure;

			if(Enemy.IsNullOrInvalid(attackEnemy))
				return BehaveResult.Failure;


			if(entity.robotCmpt == null)
				return BehaveResult.Failure;

			if(m_Data.followEntity == null)
				return BehaveResult.Failure;

			PeEntityCreator.InitRobotInfo(entity,m_Data.followEntity);
			//m_StartTime = Time.time;
			m_Data.changrPos = true;
			//m_findAttack = true;
			//startFindPosTime = Time.time;
			m_AttcakPos = GetAroundPos(attackEnemy.centerPos);

			//m_Arrived = false;
			return BehaveResult.Running;
		}

		BehaveResult Tick(Tree sender)
		{
			if (!GetData<Data>(sender, ref m_Data))
				return BehaveResult.Failure;
			
			if(Enemy.IsNullOrInvalid(attackEnemy))
			{
				existent.rotation = Quaternion.identity;
				return BehaveResult.Success;
			}

			if(!IsReached(position,attackEnemy.position,true,64.0f) || !IsReached(position,m_Data.followEntity.position,true,64.0f))
			{
				Enemies.Remove(attackEnemy);

                Vector3 pos = GetAroundPos(m_Data.followEntity.position);
                SetPosition(pos);
				return BehaveResult.Failure;
			}

			m_LocalPos = GetLocalPosition(attackEnemy);
			if(IsReached(m_AttcakPos,attackEnemy.centerPos,false,x1))	
			{
				m_AttcakPos = GetAroundPos(attackEnemy.centerPos);
			}

			if(!IsReached(position,m_AttcakPos,false,x3))
			{
				MoveToPosition(m_AttcakPos,SpeedState.Run);


			}
			else
			{
				if(IsReached(position,attackEnemy.centerPos,false,x1))
				{
					m_AttcakPos = GetAroundPos(attackEnemy.centerPos);
				}
				else
				{
					if(!IsInAngle(attackEnemy))
					{
						Vector3 dir = attackEnemy.centerPos - position;
						Vector3 _newForward = Vector3.Slerp(transform.forward, dir,x2 * Time.deltaTime);
						FaceDirection(_newForward);
					}
					else
					{
						StopMove();
						FaceDirection(attackEnemy.centerPos - position);
						coatSkill();
					}
				}
			}
			return BehaveResult.Running;

		}


		void Reset(Tree sender)
		{
			existent.rotation = Quaternion.identity;
		}
		
	}


}


